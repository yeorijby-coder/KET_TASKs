// 작성자   : WCS Scheduler
// 통합판   : WCS_IO_SCH_Original 의 1층(1F) 담당 스케줄러 스레드
// 작성일   : 2026-06-23 (2026-06-27 기존 JOB_MST 스키마 통합)
// 수정일   : 2026-07-09 KET(한국단자) 1층(1F) 자동운전 로직 추가
//            - 레거시 ECS(EcsSv) 의 1층 입고/출고 흐름 포팅 (PrepareNewJobs 영역 주석 참조)
//            - HOST 신규작업('99') 접수 → S/C·홈스탠드 결정 → CV↔SC  체이닝
//            - 입고: 99→11→15→19(핸드오프)→21→25→29 / 출고: 99→21→25→29(핸드오프)→11→15→19
//              (2026-07-10 대기 상태 10/20 제거 - '99' 또는 직전  완료 상태가 대기 역할)
//            - 토폴로지는 SC_HS_DEF(홈스탠드) / DEST_POS_DEF(공용 출고대 그룹·RET_CNT) 데이터 주도
//
//  ※ 이번 현장 구성 (단순 반송):
//     - MES/CEID 통신 없음, CELL(로케이션) 관리 없음, 바코드/공파레트 없음
//     - JOB(작업)은 외부/상위 시스템이 JOB_MST 에 직접 INSERT 하며,
//       START_POS / DEST_POS 가 이미 채워져 들어온다. (스케줄러는 작업을 생성하지 않는다)
//     - 따라서 본 스케줄러는 "JOB_MST 의 구동대기 작업 + 유휴 설비 → 설비 명령 지시
//       + 완료 감지 + 상태 변경" 만 담당한다.
//
//  ※ DB : PostgreSQL (방언은 DbLang 사용). 파라미터는 :PARAM, 날짜는 DbLang.SYSDATE(NOW()).
//  ※ 사용 테이블 : JOB_MST(작업), CV_DATA(컨베이어), SC_DATA(스태커크레인)
//     (KET 현장은 RTV/RGV 미설치 - 관련 처리 2026-07-10 전체 제거)
//     - 설비 상태는 _RD(PLC→DB readback) 컬럼, 명령은 _OD(DB→PLC) 컬럼 + OD_RQ_YN 플래그로 핸드셰이크.
//     - OD_RQ_YN='N' = 유휴(명령 수신 가능), 명령 지시 시 _OD 채우고 OD_RQ_YN='Y'.
//       (PLC↔DB 게이트웨이가 명령 반영 후 OD_RQ_YN 을 'N'으로 되돌린다)
//
//  ※ 기존 설비 스레드 cThread_CV / cThread_SC / cThread_R 의 SQL 패턴을 인용하여 작성.
//     (해당 스레드는 현장 구성에서 빌드 제외 - DB 구조 참고용으로만 보존)

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data;
using NpgsqlTypes;
using Samoh_Lib;

namespace TSK_COMM_IOSCH
{
    /// <summary>
    /// KET 물류창고 자동 반송 Scheduler Thread (JOB_MST 스키마)
    /// - C/V 구동 : 입고대/출고대 팔레트 + 구동대기 작업(JOB_STATUS='99') → CV 명령 지시
    /// - S/C 반송 : 구동대기 작업(JOB_STATUS='20') + 유휴 S/C → 이송 명령 지시
    /// </summary>
    public class cThread_SCH_1F : IOSchDB
    {
        #region 변수 선언
        // ※ m_nId / m_Thread / m_Main / callPsMsgView 는 기반 클래스(IOSchDB)의 필드를 그대로 사용한다.
        //    (2026-07-11 수정 : 파생 클래스에 동일 이름 필드를 중복 선언하면 SYS_MAIN 이 세팅한
        //     델리게이트가 파생 필드에만 들어가고, IOSchDB 쪽 함수(MakeMsg_Error_NoLog 등)는
        //     여전히 null 인 기반 필드를 참조하여 NullReferenceException 발생 → 중복 선언 제거)
        private bool     m_Open;
        public  bool     IsOpen       { get { return m_Open; } set { m_Open = value; } }

        /*
         * 수동 중지 지시.
         *
         *   화면의 스레드 상태 표시를 눌러 "스레드 중지" 를 고르면 참이 된다.
         *   - 스레드 본체(Thread_Doing)는 이 플래그를 보고 루프를 빠져나와 종료한다.
         *   - 메인폼의 Thread_Tick 은 이 플래그가 선 스레드를 재기동하지 않는다.
         *   UI 스레드가 쓰고 스케줄러 스레드가 읽으므로 volatile 로 가시성을 보장한다.
         */
        public volatile bool m_bManualStop = false;
        public  string   m_strLogName = "";

        // 설비별 직전 지시 작업 추적 (동일 작업 중복 지시 방지)
        private Dictionary<string, string> m_dicPrevCV  = new Dictionary<string, string>();
        private Dictionary<string, string> m_dicPrevSC  = new Dictionary<string, string>();
        #endregion

        #region 상수 정의
        // 창고 구분 : cDefApp.eWHTYP.SKI_WH01 = 10
        private static readonly string SCH_WH_TYP = ((int)cDefApp.eWHTYP.KET_WH01).ToString();

        // 명령 지시 주체 표기
        private const string OD_USER = "IOTASK";

        // 공용 리턴 메세지 
        public string strRTN_MSG = "";
        public string m_strRtnMsg = "";


        #endregion

        #region 생성자
        public cThread_SCH_1F(int Id)
        {
            m_nId = Id;
        }
        #endregion

        // ※ 메시지 출력 헬퍼(MakeMsg / MakeMsg_Error / MakeMsg_Imp / MakeMsg_Error_NoLog)는
        //    기반 클래스(IOSchDB)의 공용 구현을 사용한다. (중복 선언 제거 - CS0108 숨김 경고 해소)

        // ─────────────────────────────────────────────────────────────────
        // 메인 Thread 루프 (200ms 폴링)
        // ─────────────────────────────────────────────────────────────────
        #region Thread_Doing
        public void Thread_Doing(object value)
        {
            // DB 연결이 성공할 때까지 재시도 (연결 전에 처리 메서드가 호출되지 않도록)
            while (!IsDBOpen)
            {
                // @.DB 연결을 기다리는 중에도 중지 지시에 반응한다.
                if (m_bManualStop) { MakeMsg("[SCH] 중지 지시로 스레드를 종료합니다."); m_Thread = null; return; }

                try
                {
                    if (DBOpen())
                    {
                        MakeMsg("[SCH] DB Open 완료 - Scheduler 시작 (JOB_MST 스키마)");
                        break;
                    }
                    else
                    {
                        MakeMsg_Error("[SCH] DB Open 실패 - 5초 후 재시도");
                    }
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[SCH] DB Open 오류 - 5초 후 재시도: " + ex.Message);
                }
                Thread.Sleep(5000);
            }

            while (!m_bManualStop)
            {
                Thread.Sleep(200);

                // _pBdb null 안전 확인
                if (_pBdb == null)
                {
                    try { IsDBOpen = false; DBOpen(); } catch { }
                    continue;
                }

                try
                {
                    /*
                    // C/V 도착보고
                    if (!ARRIVE_CV(SCH_WH_TYP, m_nId.ToString(), ref strRTN_MSG))
                    {
                        if (strRTN_MSG != "")
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                    }
                    else
                    {
                        if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }

                        //MakeMsg(strRTN_MSG + "출고대에서 완료보고 하였습니다.");
                    }
                    Thread.Sleep(10);

                    // 출고 H/S에서 최종목적지 결정 C/V목적지쓰기
                    if (!CHECK_CV_RETHS(SCH_WH_TYP, m_nId.ToString(), ref strRTN_MSG))
                    {
                        if (strRTN_MSG != "")
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                    }
                    else
                    {
                        if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                    }
                    Thread.Sleep(10);

                    // 입고대에서 출발 하기
                    if (!NEW_JOB_ORDER(SCH_WH_TYP, m_nId.ToString(), ref strRTN_MSG))
                    {
                        if (strRTN_MSG != "")
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                    }
                    else
                    {
                        if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                    }
                    Thread.Sleep(10);
                    //*/

                    RunSchFunc(JobAccept);              // 신규 작업 접수 99 -> 10 / 20

                    RunSchFunc(StartInvokeCheck2);      // 1F 구라인 입고대 출발
                    RunSchFunc(StartInvokeCheck5);      // 1F 신라인 입고대 출발
                    RunSchFunc(RetInvokeCheck2);        // 1F 구라인 출고HS → 픽킹대 지시
                    RunSchFunc(RetInvokeCheck5);        // 1F 신라인 출고HS → 픽킹대 지시
                    RunSchFunc(ArrivedCheck2);          // 1F 구라인 픽킹대 도착보고
                    RunSchFunc(ArrivedCheck5);          // 1F 신라인 픽킹대 도착보고

                    RunSchFunc(StoHsCheck2);            // 1F 구라인 입고 H/S -> 크레인 입고 지시
                    RunSchFunc(StoHsCheck5);            // 1F 신라인 입고 H/S -> 크레인 입고 지시

                    RunSchFunc(RetCmdCheck);            // 출고 지시 20 -> 21
                    RunSchFunc(ScCompleteCheck);        // 크레인 작업 완료 -> 29

                    RunSchFunc(CopyTrackData2);         // 이음새 352→631 작업 데이터 복사
                    RunSchFunc(DeleteTrackData2);       // 이음새 352 작업 데이터 삭제
                    RunSchFunc(CopyTrackData5);         // 이음새 654→355 작업 데이터 복사
                    RunSchFunc(DeleteTrackData5);       // 이음새 654 작업 데이터 삭제

                    RunSchFunc(NewStartRoutinePlc2);    // 1F 구라인 출고 대기대에서 출발
                    RunSchFunc(NewStartRoutinePlc5);    // 1F 신라인 출고 대기대에서 출발
                    RunSchFunc(ReStartRoutine);         // 1F 위치 결정대에서 출고대#1/#2 분배
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[SCH] 처리 오류: " + ex.Message);
                    // DB 연결 끊김 가능성 → 재연결 시도
                    try
                    {
                        if (_pConObj == null ||
                            _pConObj.State != System.Data.ConnectionState.Open)
                        {
                            IsDBOpen = false;
                            DBOpen();
                        }
                    }
                    catch { }
                }
            }
        
            // @.중지 지시로 루프를 빠져나온 경우의 정리.
            //   m_Thread 를 비워야 메인폼이 "중지됨" 으로 인식하고,
            //   시작 지시가 오면 새 스레드를 만들 수 있다.
            MakeMsg("[SCH] 중지 지시로 스레드를 종료했습니다.");
            IsOpen = false;
            m_Thread = null;
}
        #endregion

        #region 기존 함수들
        public bool NEW_JOB_ORDER(string strWH_TYP,
                                 string strPLC_NO,
                             ref string pRTN_MSG)
        {
            try
            {
                string strLUGG_NO = "";
                string strTRACK_NO = "";
                string strJOB_START_POS = "";
                string strJOB_DEST_POS = "";
                string strJOB_DEST_LOC = "";
                string strPRODUCT_SIZE = "";
                string strDestPos = "";
                string strSENSOR1 = "";         // 1단감지
                string strSENSOR2 = "";         // 2단감지
                int nJobType = 1;
                int nSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[NEW_JOB_ORDER]";

                strSql = "";
                strSql += cDefApp.CRLF + " SELECT CD.*, JM.*                            ";
                strSql += cDefApp.CRLF + "   FROM CV_DATA CD                            ";
                strSql += cDefApp.CRLF + "  INNER JOIN JOB_MST JM                       ";
                strSql += cDefApp.CRLF + "     ON CD.HOST_STN_NO = JM.START_POS         ";
                strSql += cDefApp.CRLF + "    AND JM.JOB_STATUS = '" + ST_CV_WAIT + "'      ";   // 10 = CV 구동대기. 신규('99')는 JOB_ACCEPT 가 10/20 으로 나눈다
                strSql += cDefApp.CRLF + "  WHERE CD.LUGG_NO_RD    IN ('','0','0000')   ";
                strSql += cDefApp.CRLF + "    AND CD.STO_READY_RD 	= '1'               ";
                strSql += cDefApp.CRLF + "    AND CD.SENSOR0_DATA_RD = '1'              ";
                strSql += cDefApp.CRLF + "    AND CD.AUTO_MODE_RD 	= '1'               ";
                strSql += cDefApp.CRLF + "    AND CD.ERROR_CODE		IN ('0','0000')     ";
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_YN		= 'N'               ";
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_FLAG		= 'N'               ";
                strSql += cDefApp.CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')               ";
                strSql += cDefApp.CRLF + "    AND CD.WH_TYP		    = :WH_TYP           ";
                strSql += cDefApp.CRLF + "    AND 0 = (SELECT COUNT(*)                  ";
                strSql += cDefApp.CRLF + "               FROM JOB_MST                   ";
                strSql += cDefApp.CRLF + "              WHERE LUGG_NO = CD.LUGG_NO_RD)  ";
                strSql += cDefApp.CRLF + "  LIMIT 1                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();
                for (int i = 0; i < nSelCnt; i++)
                {
                    DataTable dtDestPos = new DataTable();

                    strTRACK_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    strPRODUCT_SIZE = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strJOB_DEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strJOB_DEST_LOC = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strJOB_START_POS = "" + _pBdb.mDtMain.Rows[i]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_POS"].ToString();

                    strSENSOR1 = "" + _pBdb.mDtMain.Rows[i]["SENSOR1_DATA_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SENSOR1_DATA_RD"].ToString();
                    strSENSOR2 = "" + _pBdb.mDtMain.Rows[i]["SENSOR2_DATA_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SENSOR2_DATA_RD"].ToString();

                    // 명령 대상 PLC 는 조회 행(CV_DATA)에서 읽어 사용 (스레드 ID 아님)
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();

                    // 설비타스크에 작업지시
                    if (UPDATE_CV_DATA(nJobType.ToString(), strPRODUCT_SIZE, "0", strJOB_DEST_POS, "0", strLUGG_NO, strWH_TYP, strPLC_NO, strTRACK_NO, "", ref pRTN_MSG) == false)
                    {
                        m_strRtnMsg = pRTN_MSG;
                        _pBdb.Rollback();
                        throw new Exception(m_strRtnMsg);
                    }

                    // 작업상태 변경 
                    if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, nJobType.ToString(), ref pRTN_MSG) == false)
                    {
                        m_strRtnMsg = pRTN_MSG;
                        _pBdb.Rollback();
                        throw new Exception(m_strRtnMsg);
                    }

                    pRTN_MSG = strFunction + "TRACK " + strTRACK_NO + "번[입고대]에서 CV_TASK를 통해서 작업 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                    _pBdb.Commit();
                    InsertLog(SCH_WH_TYP, strRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strJOB_START_POS, strDestPos);
                    return true;
                }
                pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                _pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }
        public bool ARRIVE_CV(string strWH_TYP,
                              string strPLC_NO,
                          ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[ARRIVE_CV]";

                // BCR 도착.
                strSql = "";
                strSql += cDefApp.CRLF + " SELECT JM.*, CD.*                                ";
                strSql += cDefApp.CRLF + "   FROM CV_DATA CD                                ";
                strSql += cDefApp.CRLF + "  INNER JOIN JOB_MST JM                           ";
                strSql += cDefApp.CRLF + "     ON CD.WH_TYP             = JM.WH_TYP 	    ";
                strSql += cDefApp.CRLF + "    AND CD.HOST_STN_NO       = JM.DEST_POS 	    ";
                strSql += cDefApp.CRLF + "    AND CD.LUGG_NO_RD         = JM.LUGG_NO        ";
                // ※ PLC_NO 필터 제거 (2026-07-11) : 스케줄러는 전체 PLC 를 관장한다.
                strSql += cDefApp.CRLF + "  WHERE CD.WH_TYP		        = :pWH_TYP          ";
                strSql += cDefApp.CRLF + "    AND CD.RET_READY_RD 	    = '1'               ";   // 출고대 READY ON
                strSql += cDefApp.CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'               ";   // 자동모드
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_YN		    = 'N'               ";
                strSql += cDefApp.CRLF + "    AND JM.JOB_STATUS 	    = '11'             ";   // 11 = CV 구동중
                strSql += cDefApp.CRLF + "    AND JM.DEST_POS Is not null                   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";
                string strMC_NO = "";
                string strCOMMING_DEST_TR = "";
                _pBdb.BeginTrans();

                for (int i = 0; i < nSelCnt; i++)
                {
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strMC_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    // ※ COMMING_DEST_TR(도착 후 후속 반출 목적지)은 세미피니시 스키마 전용 컬럼 -
                    //    KET 현장 CV_DATA 에는 없으므로 존재할 때만 읽는다 (없으면 '0' = 후속 지시 없음)
                    strCOMMING_DEST_TR = _pBdb.mDtMain.Columns.Contains("COMMING_DEST_TR") == false ? "0"
                        : ("" + _pBdb.mDtMain.Rows[i]["COMMING_DEST_TR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["COMMING_DEST_TR"].ToString());

                    //// 상위 TASK에 도착 보고 - 추후확인이 필요하다...
                    if (UPDATE_IF_LUGG_STA(strWH_TYP,
                                            strLUGG_NO,
                                            "90",           //  strJOB_STATUS,      <= 90: 정상 완료(IF_LUGG_STA의 WRK_STA 값임)
                                        ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // 도착보고가 성공하면 - 작업 삭제 
                    if (DELETE_JOB_DATA(strLUGG_NO,
                                        strWH_TYP,
                                    ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    //// 후속 반출 목적지가 정의된 경우에만 도착 트랙에 이동 지시
                    //// (KET 현장은 출고대 도착이 최종 - 도착보고/작업삭제만 수행)
                    //if (strCOMMING_DEST_TR != "0" && strCOMMING_DEST_TR != "")
                    //{
                    //    if (UPDATE_CV_DATA(strJOB_TYP
                    //                     , strTRAY_TYP
                    //                     , strTRAY_LEV
                    //                     , strCOMMING_DEST_TR
                    //                     , strIS_TURN
                    //                     , strLUGG_NO
                    //                     , strWH_TYP
                    //                     , strPLC_NO
                    //                     , strMC_NO
                    //                     , ""
                    //                     , ref pRTN_MSG) == false)
                    //    {
                    //        _pBdb.Rollback();
                    //        return false;
                    //    }
                    //}
                }

                pRTN_MSG = strFunction + "TRACK " + strMC_NO + "번[출고대]에서 HOST_TASK를 통해서 완료보고 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                _pBdb.Commit();

                InsertLog(SCH_WH_TYP, strRTN_MSG, "", "", strLUGG_NO, "19");

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        public bool CHECK_CV_RETHS(string strWH_TYP,
                                   string strPLC_NO,
                               ref string pRTN_MSG)
        {
            try
            {
                int nMainSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[CHECK_CV_RETHS]";

                strSql = "";
                strSql += CRLF + " SELECT  JM.*, CD.*, SHD.*                          ";
                strSql += CRLF + "   FROM  JOB_MST JM                                 ";
                strSql += CRLF + "  INNER  JOIN CV_DATA CD                            ";
                strSql += CRLF + "     ON  JM.WH_TYP             = CD.WH_TYP          ";
                strSql += CRLF + "    AND  JM.HS_TRACK_NO        = CD.MC_NO           ";
                strSql += CRLF + "   LEFT  OUTER       JOIN        SC_HS_DEF SHD      ";
                strSql += CRLF + "     ON  JM.WH_TYP             = SHD.WH_TYP         ";
                strSql += CRLF + "    AND  JM.HS_TRACK_NO        = SHD.HS_MC_NO       ";
                // ※ LIKE 방향 수정 (2026-07-11) : 서비스 가능 출고대 목록(DEST_DEF_DAT='103, 104, 105')
                //    안에 작업 목적지(DEST_POS)가 포함되는지 판정
                strSql += CRLF + "    AND  SHD.DEST_DEF_DAT   like '%' " + DbLang.II + " JM.DEST_POS " + DbLang.II + " '%'";
                // ※ PLC_NO 필터 제거 (2026-07-11) : 스케줄러는 전체 PLC 를 관장한다.
                //    (기존에는 스레드 ID(SCH_GR01=50)가 :PLC_NO 로 전달되어 무동작이었음)
                strSql += CRLF + "  WHERE  CD.WH_TYP             = :WH_TYP            ";
                strSql += CRLF + "    AND  COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                ";    // 트랙 일시정지가 아니어야 함! - 안보는게 나을듯!
                strSql += CRLF + "    AND  CD.SENSOR0_DATA_RD    = '1'                ";
                strSql += CRLF + "    AND  JM.JOB_STATUS 	     = '29'               ";    // 도착 보고 완료
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nMainSelCnt = _pBdb.ExcuteQry(strSql);
                if (nMainSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nMainSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strSC_NO = "";
                string strSTART_LOCATION = "";
                string strWAIT_TRACK = "";
                string strLOT_NO = "";
                DataTable dtDestPos = new DataTable();

                for (int i = 0; i < nMainSelCnt; i++)
                {
                    _pBdb.BeginTrans();

                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    strSC_NO = "" + _pBdb.mDtMain.Rows[i]["SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SC_NO"].ToString();
                    strSTART_LOCATION = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString();
                    strWAIT_TRACK = "" + _pBdb.mDtMain.Rows[i]["WAIT_TRACK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WAIT_TRACK"].ToString();

                    //if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Move ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Ret ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.PRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.OtherRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.FireRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RackRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.ManualRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.ManualPickingRet) //이정민 추가
                    //{
                    if (Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeAutoRet ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeAutoPR ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeAutoW2W ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeAutoMove ||    // KET 현장 구조상 이렇게 되지는 않음
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeSemiRet ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeSemiPR ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeSemiW2W ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeSemiMove ) 
                    {
                        // 대기 트랙의 정보가 있다면...
                        //   ※ '0' 체크 추가 (2026-07-12) : WAIT_TRACK 미정의(NULL)시 기본값 '0' 이
                        //     목적지를 덮어쓰는 버그 수정 - 3층 출고 HS(hs04)는 대기 트랙이 없음
                        if (strWAIT_TRACK != "" && strWAIT_TRACK != null && strWAIT_TRACK != "0")
                            strDEST_POS = strWAIT_TRACK;

                        // C/V에 목적지정보쓰기.
                        if (UPDATE_CV_DATA(strJOB_TYP
                                         , strTRAY_TYP
                                         , strTRAY_LEV
                                         , strDEST_POS          // strWAIT_TRACK        
                                         , strIS_TURN
                                         , strLUGG_NO
                                         , strWH_TYP
                                         , strPLC_NO
                                         , strSTART_POS
                                         , strLOT_NO    //파쇄기 라인 출고 품목 표시를 위한 수정 (조한성. 0302)
                                         , ref pRTN_MSG) == false)
                        {
                            //return false;
                            _pBdb.Rollback();
                            continue;
                        }
                        // 작업시작(구동중)
                        if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }
                        pRTN_MSG = strFunction + "TRACK " + strSTART_POS + "번[출고 H/S]에 CV_TASK를 통해 DATA 기록 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                        _pBdb.Commit();

                        InsertLog(SCH_WH_TYP, strRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strSTART_POS, strDEST_POS);
                        continue;

                    }
                    _pBdb.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        /// <summary>
        /// 출고 계열 작업 여부 (CHECK_CV_RETHS 의 작업구분 목록과 동일)
        /// </summary>
        private bool IsRetJobType(string strJOB_TYP)
        {
            int nTyp;
            if (int.TryParse(strJOB_TYP, out nTyp) == false) return false;
            return (nTyp == (int)EN_JOB_TYPE.enJobTypeAutoRet ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeAutoPR ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeAutoW2W ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeAutoMove ||    // KET 현장 구조상 이렇게 되지는 않음
                    nTyp == (int)EN_JOB_TYPE.enJobTypeSemiRet ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeSemiPR ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeSemiW2W ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeSemiMove);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        // 1층(1F) 함수들 - 레거시 ECS(EcsSv Cv.cpp) 의 *2/*5 함수 포팅 (2026-07-20)
        //   1층 CV PLC : 02(구라인, 트랙 2xx) / 05(신라인, 트랙 5xx)
        //   ECS 활성 함수 : RetInvokeCheck2/5(출고HS→픽킹), MovingTrackCheckPlc2/5(레인 진입 제한)
        //   ECS 비활성 클론 : StartInvokeCheck2/5, ArrivedCheck2/5, Copy/DeleteTrackData2/5
        //     (ECS 에서는 공통 함수가 전 PLC 를 처리 - 여기서는 PLC 한정 버전으로 동작 일치 구현)
        //   레거시 트랙(2xxx/5xxx) → To-Be MC(2xx/5xx) 변환은 SC_HS_DEF 등록값으로 검증됨.
        // ─────────────────────────────────────────────────────────────────
        #region 1층(1F) 함수들
        private const string CV_PLC_1F_OLD = "02";   // 1F 구라인 CV PLC
        private const string CV_PLC_1F_NEW = "05";   // 1F 신라인 CV PLC

        // 구↔신 라인 이음새 트랙 (레거시 CopyTrackData2/5 : 2064→5031, 5056→2050)
        private const string SEAM_FROM_2 = "264";
        private const string SEAM_TO_2 = "531";
        private const string SEAM_FROM_5 = "556";
        private const string SEAM_TO_5 = "250";

        // 이음새 복사 상태 (레거시 m_bCopied 대응 : key = FROM_TO, value = 복사한 작업번호)
        private readonly Dictionary<string, string> m_dicSeamCopied = new Dictionary<string, string>();

        // Thread_Doing 공용 호출 헬퍼 (기존 함수들과 동일한 호출/메시지 패턴)
        private delegate bool SchFunc(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG);
        private void RunSchFunc(SchFunc fn)
        {
            if (!fn(SCH_WH_TYP, m_nId.ToString(), ref strRTN_MSG))
            {
                if (strRTN_MSG != "")
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
            }
            else
            {
                if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
            }
            Thread.Sleep(10);
        }

        // ── ECS *2/*5 대응 공개 함수 (기존 함수들과 동일 시그니처) ──
        public bool StartInvokeCheck2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_STO_START_PLC(strWH_TYP, CV_PLC_1F_OLD, "[StartInvokeCheck2]", ref pRTN_MSG); }
        public bool StartInvokeCheck5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_STO_START_PLC(strWH_TYP, CV_PLC_1F_NEW, "[StartInvokeCheck5]", ref pRTN_MSG); }

        public bool RetInvokeCheck2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_RETHS_PLC(strWH_TYP, CV_PLC_1F_OLD, "[RetInvokeCheck2]", ref pRTN_MSG); }
        public bool RetInvokeCheck5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_RETHS_PLC(strWH_TYP, CV_PLC_1F_NEW, "[RetInvokeCheck5]", ref pRTN_MSG); }

        public bool ArrivedCheck2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_ARRIVE_PLC(strWH_TYP, CV_PLC_1F_OLD, "[ArrivedCheck2]", ref pRTN_MSG); }
        public bool ArrivedCheck5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_ARRIVE_PLC(strWH_TYP, CV_PLC_1F_NEW, "[ArrivedCheck5]", ref pRTN_MSG); }

        public bool StoHsCheck2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return SC_STO_CMD_PLC(strWH_TYP, CV_PLC_1F_OLD, "[StoHsCheck2]", ref pRTN_MSG); }

        public bool StoHsCheck5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return SC_STO_CMD_PLC(strWH_TYP, CV_PLC_1F_NEW, "[StoHsCheck5]", ref pRTN_MSG); }

        /*
         * 크레인 진행/완료는 층에 속한 일이 아니라 작업에 속한 일이다.
         * SC_HS_DEF 를 보면 모든 호기가 1층 H/S(HS_NO 02)와 3층 H/S(03/04)를 다 가진다.
         * 세 스레드(1F/3F/BOX)가 같은 작업을 중복해서 잡지 않도록 여기서만 돌린다.
         */
        public bool JobAccept(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return JOB_ACCEPT(strWH_TYP, "[JobAccept]", ref pRTN_MSG); }


        /*
         * 출고 지시도 크레인 완료와 마찬가지로 층이 아니라 작업에 속한 일이다.
         * 다만 내려놓을 출고 H/S 는 층마다 다르므로(SC_HS_DEF.HS_NO) 층 값을 넘긴다.
         * 1층은 '02' 다. 세 스레드가 겹치지 않도록 여기서만 돌린다.
         */
        public bool RetCmdCheck(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return SC_RET_CMD_PLC(strWH_TYP, HS_NO_RETRIEVE, "[RetCmdCheck]", ref pRTN_MSG); }

        public bool ScCompleteCheck(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return SC_COMP_CHK(strWH_TYP, "[ScCompleteCheck]", ref pRTN_MSG); }

        public bool NewStartRoutinePlc2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CHECK_CV_RET_START(strWH_TYP, CV_PLC_1F_OLD, "[NewStartRoutinePlc2]", ref pRTN_MSG); }
        public bool NewStartRoutinePlc5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CHECK_CV_RET_START(strWH_TYP, CV_PLC_1F_NEW, "[NewStartRoutinePlc5]", ref pRTN_MSG); }

        public bool ReStartRoutine(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CHECK_CV_RET_RESTART(strWH_TYP, CV_PLC_1F_OLD, "[ReStartRoutine]", ref pRTN_MSG); }

        //public bool MovingTrackCheckPlc2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        //{ return MOVING_TRACK_CHECK_PLC(strWH_TYP, "3", "[MovingTrackCheckPlc2]", ref pRTN_MSG); }
        //public bool MovingTrackCheckPlc5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        //{ return MOVING_TRACK_CHECK_PLC(strWH_TYP, "6", "[MovingTrackCheckPlc5]", ref pRTN_MSG); }

        public bool CopyTrackData2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return COPY_TRACK_DATA(strWH_TYP, SEAM_FROM_2, SEAM_TO_2, "[CopyTrackData2]", ref pRTN_MSG); }
        public bool CopyTrackData5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return COPY_TRACK_DATA(strWH_TYP, SEAM_FROM_5, SEAM_TO_5, "[CopyTrackData5]", ref pRTN_MSG); }

        public bool DeleteTrackData2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return DELETE_TRACK_DATA(strWH_TYP, SEAM_FROM_2, SEAM_TO_2, "[DeleteTrackData2]", ref pRTN_MSG); }
        public bool DeleteTrackData5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return DELETE_TRACK_DATA(strWH_TYP, SEAM_FROM_5, SEAM_TO_5, "[DeleteTrackData5]", ref pRTN_MSG); }

        // ─────────────────────────────────────────────────────────────────
        // 출고 대기대 → 1층 출고 루프 출발  (ECS CCv::StartRoutine 포팅)
        //   대상 : 출고 대기 트랙(SC_HS_DEF.WAIT_TRACK = CV_DATA.MC_NO)에 재하된
        //          작업(JOB_MST.LUGG_NO = CV_DATA.LUGG_NO_RD, 상태 '11')
        //   유량 제한 (ECS m_nRetCnt + m_nRetCntNew 대응) :
        //          EQP_MST의 CV 2호기/5호기 CV_RET_CNT 합계(PLC가 보고하는 1층 출고 루프상
        //          화물 갯수) 가 DEL_HIS_SETTING(TABLE_NAME='1f_ret_rimit')의 CYCLE 값보다
        //          크면 출발하지 않는다 (return false)
        //   출발 규칙 :
        //          - 출발 크레인이 1호기(SC_HS_DEF.SC_NO='901')면 목적지 = 출고대#2(스테이션 104)
        //          - 그 외는 대기 화물 중 가장 빨리 도착한 것부터 목적지 = 출고위치 결정대(스테이션 171)
        //            (결정대는 1자리 - 빈 상태 + 진입중 화물 없음일 때 사이클당 1건만 출발)
        //   스테이션(104/171) → 실트랙(MC) 변환은 DEST_POS_DEF(TRACK_NO→MC_NO) 데이터 사용.
        // ─────────────────────────────────────────────────────────────────
        private bool m_bRetLimitHold = false;   // 유량 제한 보류 상태 (메시지 1회 출력용)

        public bool CHECK_CV_RET_START(string strWH_TYP,
                                       string strPLC_NO,
                                       string strTitle,
                                   ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[CHECK_CV_RET_START]";

                // ── 1) 1층 출고 유량 제한 확인
                //      현재 PLC 상 화물 갯수(CV 2호기 + 5호기의 CV_RET_CNT 합) > 제한(cycle) → 출발 보류
                strSql = "";
                strSql += CRLF + " SELECT COALESCE(SUM(CAST(NULLIF(EM.CV_RET_CNT,'') AS INT)), 0) AS RET_SUM   ";
                strSql += CRLF + "      , (SELECT CAST(NULLIF(DHS.CYCLE,'') AS INT)                            ";
                strSql += CRLF + "           FROM DEL_HIS_SETTING DHS                                          ";
                strSql += CRLF + "          WHERE DHS.WH_TYP     = :WH_TYP2                                    ";
                strSql += CRLF + "            AND DHS.TABLE_NAME = '1f_ret_rimit') AS RET_LIMIT                ";
                strSql += CRLF + "   FROM EQP_MST EM                                                           ";
                strSql += CRLF + "  WHERE EM.WH_TYP  = :WH_TYP                                                 ";
                strSql += CRLF + "    AND EM.EQP_TYP = 'CV'                                                    ";
                strSql += CRLF + "    AND EM.PLC_NO IN ('02','05')                                             ";   // CV 2호기(구라인)/5호기(신라인)
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                int nRetSum = 0;
                int nRetLimit = 0;
                if (nSelCnt > 0)
                {
                    nRetSum = _pBdb.mDtMain.Rows[0]["RET_SUM"] == DBNull.Value ? 0 : Convert.ToInt32(_pBdb.mDtMain.Rows[0]["RET_SUM"]);
                    nRetLimit = _pBdb.mDtMain.Rows[0]["RET_LIMIT"] == DBNull.Value ? 0 : Convert.ToInt32(_pBdb.mDtMain.Rows[0]["RET_LIMIT"]);
                }

                if (nRetSum > nRetLimit)
                {
                    // 제한 초과 - 출발하지 않음 (보류 진입 시 1회만 메시지)
                    if (m_bRetLimitHold == false)
                    {
                        m_bRetLimitHold = true;
                        pRTN_MSG = strFunction + "1층 출고 유량 제한으로 출발 보류. [현재:" + nRetSum + " / 제한:" + nRetLimit + "]";
                    }
                    else
                        pRTN_MSG = "";
                    return false;
                }
                if (m_bRetLimitHold == true)
                {
                    m_bRetLimitHold = false;
                    MakeMsg(strFunction + "1층 출고 유량 정상 - 출발 재개. [현재:" + nRetSum + " / 제한:" + nRetLimit + "]");
                }

                // ── 2) 스테이션 → 실트랙(MC) 변환 (DEST_POS_DEF)
                string strMC_RET2 = "";     // 출고대#2 (스테이션 104)
                string strMC_DECIDE = "";   // 출고위치 결정대 (스테이션 171)
                if (GET_DEST_POS_MC(strWH_TYP, "104", ref strMC_RET2, ref pRTN_MSG) == false) return false;
                if (GET_DEST_POS_MC(strWH_TYP, "171", ref strMC_DECIDE, ref pRTN_MSG) == false) return false;

                // ── 3) 출고 대기 트랙(WAIT_TRACK)에 재하된 출발 대상 작업 조회 (도착 오래된 순)
                strSql = "";
                strSql += CRLF + " SELECT  JM.LUGG_NO, JM.JOB_TYP, JM.DEST_POS, JM.TURN                ";
                strSql += CRLF + "       , JM.PRODUCT_SIZE, JM.TRAY_LEV                                ";
                strSql += CRLF + "       , CD.MC_NO, CD.PLC_NO                                         ";
                strSql += CRLF + "       , SHD.SC_NO  AS  HS_SC_NO                                     ";
                strSql += CRLF + "   FROM  SC_HS_DEF SHD                                               ";
                strSql += CRLF + "  INNER  JOIN CV_DATA CD                                             ";
                strSql += CRLF + "     ON  CD.WH_TYP             = SHD.WH_TYP                          ";
                strSql += CRLF + "    AND  CD.MC_NO              = SHD.WAIT_TRACK                      ";   // 대기 트랙 매치
                strSql += CRLF + "  INNER  JOIN JOB_MST JM                                             ";
                strSql += CRLF + "     ON  JM.WH_TYP             = CD.WH_TYP                           ";
                strSql += CRLF + "    AND  JM.LUGG_NO            = CD.LUGG_NO_RD                       ";   // 재하 화물의 작업 매치
                strSql += CRLF + "  WHERE  SHD.WH_TYP            = :WH_TYP                             ";
                strSql += CRLF + "    AND  SHD.HS_NO             = '" + HS_NO_RETRIEVE + "'            ";   // 출고 HS 정의의 대기 트랙
                strSql += CRLF + "    AND  COALESCE(SHD.WAIT_TRACK,'') <> ''                           ";
                strSql += CRLF + "    AND  SHD.HS_USE_YN         = 'Y'                                 ";
                strSql += CRLF + "    AND  CD.SENSOR0_DATA_RD    = '1'                                 ";   // 재하
                strSql += CRLF + "    AND  CD.STO_READY_RD       = '1'                                 ";   // 출발 준비
                strSql += CRLF + "    AND  CD.AUTO_MODE_RD       = '1'                                 ";
                strSql += CRLF + "    AND  CD.OD_RQ_YN           = 'N'                                 ";
                strSql += CRLF + "    AND  CD.OD_RQ_FLAG         = 'N'                                 ";
                strSql += CRLF + "    AND  COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                                 ";
                strSql += CRLF + "    AND  CD.ERROR_CODE        IN ('0','00','000','0000')             ";
                strSql += CRLF + "    AND  JM.JOB_STATUS         = '" + ST_CV_RUN + "'                 ";   // CV 구동중 (출고HS→대기대 도착)
                strSql += CRLF + "  ORDER  BY JM.UPD_DT ASC                                            ";   // 가장 빨리 도착한 화물부터
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }
                DataTable dtWait = _pBdb.mDtMain.Copy();

                // ── 4) 출고위치 결정대(171) 가용 확인 : 빈 상태 + 진입중(지시된) 화물 없음
                bool bDecideReady = false;
                strSql = "";
                strSql += CRLF + " SELECT COUNT(*) AS READY_CNT                                        ";
                strSql += CRLF + "   FROM CV_DATA CD                                                   ";
                strSql += CRLF + "  WHERE CD.WH_TYP           = :WH_TYP                                ";
                strSql += CRLF + "    AND CD.MC_NO            = :MC_NO                                 ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                                    ";   // 빈 상태
                strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                                    ";
                strSql += CRLF + "    AND CD.ERROR_CODE      IN ('0','00','000','0000')                ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM CV_DATA CD2                        ";   // 결정대로 진입중인 화물 없음
                strSql += CRLF + "              WHERE CD2.WH_TYP      = :WH_TYP2                       ";
                strSql += CRLF + "                AND CD2.DEST_POS_OD = :MC_NO2                        ";
                strSql += CRLF + "                AND CD2.OD_RQ_YN    = 'Y')                           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_DECIDE;
                _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO2", DbLang.VARCHAR).Value = strMC_DECIDE;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt > 0)
                    bDecideReady = Convert.ToInt32(_pBdb.mDtMain.Rows[0]["READY_CNT"]) > 0;

                // ── 5) 출발 지시
                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strWAIT_MC = "";
                string strCV_PLC = "";
                string strHS_SC_NO = "";
                string strDestMc = "";

                for (int i = 0; i < dtWait.Rows.Count; i++)
                {
                    strJOB_TYP = dtWait.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : dtWait.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + dtWait.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : dtWait.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + dtWait.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : dtWait.Rows[i]["TRAY_LEV"].ToString();
                    strIS_TURN = "" + dtWait.Rows[i]["TURN"].ToString() == "" ? "0" : dtWait.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + dtWait.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : dtWait.Rows[i]["LUGG_NO"].ToString();
                    strWAIT_MC = "" + dtWait.Rows[i]["MC_NO"].ToString() == "" ? "0" : dtWait.Rows[i]["MC_NO"].ToString();
                    strCV_PLC = "" + dtWait.Rows[i]["PLC_NO"].ToString() == "" ? "0" : dtWait.Rows[i]["PLC_NO"].ToString();
                    strHS_SC_NO = "" + dtWait.Rows[i]["HS_SC_NO"].ToString() == "" ? "0" : dtWait.Rows[i]["HS_SC_NO"].ToString();

                    // 출고 계열 작업만 대상
                    if (IsRetJobType(strJOB_TYP) == false)
                        continue;

                    if (strHS_SC_NO == "901")
                    {
                        // 출발 크레인이 1호기 → 출고대#2(스테이션 104) 직행
                        //   (최종 목적지 확정이므로 JOB_MST.DEST_POS 도 함께 변경 - ARRIVE_CV 도착 매칭용)
                        strDestMc = strMC_RET2;

                        _pBdb.BeginTrans();
                        if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strDestMc, strIS_TURN,
                                           strLUGG_NO, strWH_TYP, strCV_PLC, strWAIT_MC, "", ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }
                        if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDestMc) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }
                        pRTN_MSG = strFunction + "TRACK " + strWAIT_MC + "번[출고 대기대]에서 출고대#2(" + strDestMc + ")로 출발 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                        _pBdb.Commit();
                        InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strWAIT_MC, strDestMc);
                        return true;
                    }

                    // 1호기 외 → 출고위치 결정대(스테이션 171)로 선착순 1건 출발
                    if (bDecideReady == false)
                        continue;   // 결정대 사용중 - 다음 사이클 재시도 (1호기 직행은 계속 가능)

                    strDestMc = strMC_DECIDE;

                    _pBdb.BeginTrans();
                    if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strDestMc, strIS_TURN,
                                       strLUGG_NO, strWH_TYP, strCV_PLC, strWAIT_MC, "", ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    //   (결정대는 경유지이므로 JOB_MST.DEST_POS 는 변경하지 않음 - 최종 목적지는
                    //    CHECK_CV_RET_RESTART 가 출고대#1/#2 중에서 결정한다)
                    if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    pRTN_MSG = strFunction + "TRACK " + strWAIT_MC + "번[출고 대기대]에서 출고위치 결정대(" + strDestMc + ")로 출발 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                    _pBdb.Commit();
                    InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strWAIT_MC, strDestMc);
                    return true;    // 결정대는 1자리 - 사이클당 1건만 출발
                }

                pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 출고위치 결정대(171) → 출고대#1/#2 분배  (ECS CCv::ReStartRoutine2 포팅)
        //   결정대(스테이션 171 = 실트랙 232)에 도착한 화물의 최종 출고대를 결정 :
        //     출고대#1(스테이션 103) 이 빈 상태면 → #1, 아니면 출고대#2(스테이션 104) 가
        //     빈 상태면 → #2, 둘 다 사용중이면 대기 (다음 사이클 재시도)
        //   결정 후 JOB_MST.DEST_POS 와 CV_DATA(_OD) 의 목적지를 함께 변경한다.
        // ─────────────────────────────────────────────────────────────────
        public bool CHECK_CV_RET_RESTART(string strWH_TYP,
                                         string strPLC_NO,
                                         string strTitle,
                                     ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[CHECK_CV_RET_RESTART]";

                // ── 1) 스테이션 → 실트랙(MC) 변환 (DEST_POS_DEF)
                string strMC_DECIDE = "";   // 출고위치 결정대 (스테이션 171 = 232)
                string strMC_RET1 = "";     // 출고대#1 (스테이션 103)
                string strMC_RET2 = "";     // 출고대#2 (스테이션 104)
                if (GET_DEST_POS_MC(strWH_TYP, "171", ref strMC_DECIDE, ref pRTN_MSG) == false) return false;
                if (GET_DEST_POS_MC(strWH_TYP, "103", ref strMC_RET1, ref pRTN_MSG) == false) return false;
                if (GET_DEST_POS_MC(strWH_TYP, "104", ref strMC_RET2, ref pRTN_MSG) == false) return false;

                // ── 2) 결정대에 도착(재하)한 출고 작업 조회
                strSql = "";
                strSql += CRLF + " SELECT  JM.LUGG_NO, JM.JOB_TYP, JM.TURN                             ";
                strSql += CRLF + "       , JM.PRODUCT_SIZE, JM.TRAY_LEV                                ";
                strSql += CRLF + "       , CD.MC_NO, CD.PLC_NO                                         ";
                strSql += CRLF + "   FROM  CV_DATA CD                                                  ";
                strSql += CRLF + "  INNER  JOIN JOB_MST JM                                             ";
                strSql += CRLF + "     ON  JM.WH_TYP             = CD.WH_TYP                           ";
                strSql += CRLF + "    AND  JM.LUGG_NO            = CD.LUGG_NO_RD                       ";   // 재하 화물의 작업 매치
                strSql += CRLF + "  WHERE  CD.WH_TYP             = :WH_TYP                             ";
                strSql += CRLF + "    AND  CD.MC_NO              = :MC_NO                              ";   // 결정대(171 = 232)
                strSql += CRLF + "    AND  CD.SENSOR0_DATA_RD    = '1'                                 ";   // 재하
                strSql += CRLF + "    AND  CD.STO_READY_RD       = '1'                                 ";   // 출발 준비
                strSql += CRLF + "    AND  CD.AUTO_MODE_RD       = '1'                                 ";
                strSql += CRLF + "    AND  CD.OD_RQ_YN           = 'N'                                 ";
                strSql += CRLF + "    AND  CD.OD_RQ_FLAG         = 'N'                                 ";
                strSql += CRLF + "    AND  COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                                 ";
                strSql += CRLF + "    AND  CD.ERROR_CODE        IN ('0','0000')                        ";
                strSql += CRLF + "    AND  JM.JOB_STATUS         = '" + ST_CV_RUN + "'                 ";   // CV 구동중
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_DECIDE;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }
                DataTable dtDecide = _pBdb.mDtMain.Copy();

                // ── 3) 출고대#1/#2 가용 확인 : 빈 상태 + 진입중(지시된) 화물 없음
                //      (ECS ReStartRoutine2 : 출고대#1 가용 → #1 우선, 아니면 #2)
                bool bRet1Ready = CHECK_RET_LANE_READY(strWH_TYP, strMC_RET1);
                bool bRet2Ready = CHECK_RET_LANE_READY(strWH_TYP, strMC_RET2);

                string strDestMc = "";
                string strDestNm = "";
                string strDestStn = "";     // 작업대 번호 (JOB_MST.DEST_POS 는 트랙이 아니라 작업대다)
                if (bRet1Ready == true) { strDestMc = strMC_RET1; strDestStn = "103"; strDestNm = "출고대#1"; }
                else if (bRet2Ready == true) { strDestMc = strMC_RET2; strDestStn = "104"; strDestNm = "출고대#2"; }
                else
                {
                    // 둘 다 사용중 - 다음 사이클 재시도
                    pRTN_MSG = "";
                    return true;
                }

                // ── 4) 목적지 지시 : JOB_MST + CV_DATA 목적지 함께 변경 (결정대는 1자리 - 1건 처리)
                string strJOB_TYP = dtDecide.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : dtDecide.Rows[0]["JOB_TYP"].ToString();
                string strTRAY_TYP = "" + dtDecide.Rows[0]["PRODUCT_SIZE"].ToString() == "" ? "0" : dtDecide.Rows[0]["PRODUCT_SIZE"].ToString();
                string strTRAY_LEV = "" + dtDecide.Rows[0]["TRAY_LEV"].ToString() == "" ? "0" : dtDecide.Rows[0]["TRAY_LEV"].ToString();
                string strIS_TURN = "" + dtDecide.Rows[0]["TURN"].ToString() == "" ? "0" : dtDecide.Rows[0]["TURN"].ToString();
                string strLUGG_NO = "" + dtDecide.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : dtDecide.Rows[0]["LUGG_NO"].ToString();
                string strCV_PLC = "" + dtDecide.Rows[0]["PLC_NO"].ToString() == "" ? "0" : dtDecide.Rows[0]["PLC_NO"].ToString();

                // 출고 계열 작업만 대상
                if (IsRetJobType(strJOB_TYP) == false)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();

                // C/V에 목적지정보쓰기 (결정대 → 출고대)
                if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strDestMc, strIS_TURN,
                                   strLUGG_NO, strWH_TYP, strCV_PLC, strMC_DECIDE, "", ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }

                // JOB_MST 목적지 변경 (ARRIVE_CV 가 출고대 도착을 매칭하도록)
                //   도착보고는 CD.HOST_STN_NO = JM.DEST_POS 로 맞추므로 트랙번호가 아니라
                //   작업대 번호를 넣어야 한다. CV 에는 위에서 트랙번호를 이미 썼다.
                if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDestStn) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strFunction + "TRACK " + strMC_DECIDE + "번[출고위치 결정대]에서 " + strDestNm + "(" + strDestMc + ")로 목적지 결정/지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strMC_DECIDE, strDestMc);

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }
        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 1 : 1층 입고대 출발 (ECS StartInvokeCheck2/5 - NEW_JOB_ORDER 의 PLC 한정판)
        //   해당 PLC 입고대(START_POS)에 재하 + 구동대기('99') 작업 → CV 지시 + 상태 '15'
        // ─────────────────────────────────────────────────────────────────
        /*
         * GfCvDestPos :: CV 에 실을 목적지 번호
         *
         *   크레인이 목적지일 때 JOB_MST.DEST_POS 는 WCS 표기인 9NN(901~911)이다.
         *   CV 레지스터의 목적지 자리는 한 바이트라 904 를 넣으면 136(904 & 0xFF)으로
         *   잘린다. 설비가 쓰는 번호는 호기 번호 1~11 이므로 그것으로 바꿔 넘긴다.
         *   (상위는 1~11 로 주고, HOST 태스크가 9NN 으로 저장한다.
         *    WCS_TASK_HOST/CSrvWork.cs 의 Convert S/C No)
         */
        private string GfCvDestPos(string strDestPos)
        {
            int nDest = 0;
            Int32.TryParse((strDestPos == null) ? "" : strDestPos.Trim(), out nDest);

            if (nDest > 900 && nDest < 1000)
                return (nDest - 900).ToString("000");

            return strDestPos;
        }

        private bool CV_STO_START_PLC(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*                            ";
                strSql += CRLF + "   FROM CV_DATA CD                            ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                       ";
                strSql += CRLF + "     ON CD.HOST_STN_NO = JM.START_POS         ";
                strSql += CRLF + "    AND JM.JOB_STATUS = '" + ST_CV_WAIT + "'      ";   // 10 = CV 구동대기. 신규('99')는 JOB_ACCEPT 가 10/20 으로 나눈다
                strSql += CRLF + "  WHERE CD.PLC_NO         = :CV_PLC           ";   // 3층 해당 PLC 한정 (ECS m_nNum 게이트)
                strSql += CRLF + "    AND (   (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_STO) + " <> 0 AND CD.STO_READY_RD = '1'   ";
                strSql += CRLF + "             AND CD.LUGG_NO_RD IN ('','0','0000'))                                                        ";
                strSql += CRLF + "         OR (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_ARV) + " <> 0 AND CD.RET_READY_RD = '1') )  ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD = '1'              ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	= '1'               ";
                strSql += CRLF + "    AND CD.ERROR_CODE		IN ('0','0000')     ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		= 'N'               ";
                strSql += CRLF + "    AND CD.OD_RQ_FLAG		= 'N'               ";
                strSql += CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')               ";
                strSql += CRLF + "    AND CD.WH_TYP		    = :WH_TYP           ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*)                  ";
                strSql += CRLF + "               FROM JOB_MST                   ";
                strSql += CRLF + "              WHERE LUGG_NO = CD.LUGG_NO_RD)  ";
                strSql += CRLF + "  LIMIT 1                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                string strJOB_TYP = "" + _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "1" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                string strPRODUCT_SIZE = "" + _pBdb.mDtMain.Rows[0]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PRODUCT_SIZE"].ToString();
                string strTRAY_LEV = "" + _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString();
                string strJOB_DEST_POS = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                string strJOB_START_POS = "" + _pBdb.mDtMain.Rows[0]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_POS"].ToString();

                // @.CV 에 지시할 때는 물리 트랙번호(CV_DATA.MC_NO)를 써야 한다.
                //   START_POS 는 HOST 가 쓰는 스테이션 번호(101)이고, 설비를 움직이는
                //   번호는 MC_NO(217)다. 조회를 HOST_STN_NO 로 맞춰 뒀으므로 여기서
                //   같은 행의 MC_NO 를 꺼내 쓴다. (예전에는 101 로 UPDATE 해서 0건이
                //   갱신되고 "설비 미준비" 로 조용히 재시도만 반복했다)
                string strCV_MC_NO = _pBdb.mDtMain.Rows[0]["MC_NO"].ToString();
                if (strCV_MC_NO == "") strCV_MC_NO = strJOB_START_POS;
                string strIS_TURN = "" + _pBdb.mDtMain.Rows[0]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TURN"].ToString();

                _pBdb.BeginTrans();

                if (UPDATE_CV_DATA(strJOB_TYP, strPRODUCT_SIZE, strTRAY_LEV, GfCvDestPos(strJOB_DEST_POS), strIS_TURN,
                                   strLUGG_NO, strWH_TYP, strCV_PLC, strCV_MC_NO, "", ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }

                if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + "TRACK " + strJOB_START_POS + "번[1층 입고대]에서 CV_TASK를 통해서 작업 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strJOB_START_POS, strJOB_DEST_POS);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 2 : 1층 출고HS → 출고대기대 지시 (ECS RetInvokeCheck2/5)
        //   S/C 반출 완료('29') + 출고 HS(SC_HS_DEF HS_NO='04') 재하 → CV 지시 + 상태 '15'
        //   ECS 와 동일하게 목적지 재매핑 없음 (JOB_MST.DEST_POS 그대로 지시 - 1층과 다름)
        // ─────────────────────────────────────────────────────────────────
        private bool CV_RETHS_PLC(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT  JM.*, CD.*                                  ";
                strSql += CRLF + "   FROM  JOB_MST JM                                  ";
                strSql += CRLF + "  INNER  JOIN CV_DATA CD                             ";
                strSql += CRLF + "     ON  JM.WH_TYP             = CD.WH_TYP           ";
                strSql += CRLF + "    AND  JM.HS_TRACK_NO        = CD.MC_NO            ";
                strSql += CRLF + "  INNER  JOIN SC_HS_DEF SHD                          ";
                strSql += CRLF + "     ON  JM.WH_TYP             = SHD.WH_TYP          ";
                strSql += CRLF + "    AND  JM.HS_TRACK_NO        = SHD.HS_MC_NO        ";
                strSql += CRLF + "    AND  SHD.HS_NO             = '" + HS_NO_RETRIEVE + "'   ";   // 1층 출고 HS (203/207/215/221/258/248)
                strSql += CRLF + "    AND  SHD.HS_USE_YN         = 'Y'                 ";
                strSql += CRLF + "  WHERE  CD.WH_TYP             = :WH_TYP             ";
                strSql += CRLF + "    AND  CD.PLC_NO             = :CV_PLC             ";   // 3층 해당 PLC 한정 (ECS m_nNum 게이트)
                strSql += CRLF + "    AND  COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                 ";
                strSql += CRLF + "    AND  CD.SENSOR0_DATA_RD    = '1'                 ";   // 재하 (ECS IsOnSensorIO(0))
                strSql += CRLF + "    AND  CD.OD_RQ_YN           = 'N'                 ";
                strSql += CRLF + "    AND  JM.JOB_STATUS 	     = '29'                ";   // S/C 반출 완료 (CV 요구)
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                DataTable dtHs = _pBdb.mDtMain.Copy();

                for (int i = 0; i < dtHs.Rows.Count; i++)
                {
                    string strJOB_TYP = dtHs.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : dtHs.Rows[i]["JOB_TYP"].ToString();
                    string strTRAY_TYP = "" + dtHs.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : dtHs.Rows[i]["PRODUCT_SIZE"].ToString();
                    string strTRAY_LEV = "" + dtHs.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : dtHs.Rows[i]["TRAY_LEV"].ToString();
                    string strDEST_POS = "" + dtHs.Rows[i]["DEST_POS"].ToString() == "" ? "0" : dtHs.Rows[i]["DEST_POS"].ToString();
                    string strIS_TURN = "" + dtHs.Rows[i]["TURN"].ToString() == "" ? "0" : dtHs.Rows[i]["TURN"].ToString();
                    string strLUGG_NO = "" + dtHs.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : dtHs.Rows[i]["LUGG_NO"].ToString();
                    string strHS_MC = "" + dtHs.Rows[i]["MC_NO"].ToString() == "" ? "0" : dtHs.Rows[i]["MC_NO"].ToString();

                    // 출고 계열 작업만 대상 (CHECK_CV_RETHS 와 동일 기준)
                    if (IsRetJobType(strJOB_TYP) == false)
                        continue;

                    _pBdb.BeginTrans();

                    // C/V에 목적지정보쓰기 (ECS: JobItem.m_nDestPos 그대로 - 재매핑 없음)
                    if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strDEST_POS, strIS_TURN,
                                       strLUGG_NO, strWH_TYP, strCV_PLC, strHS_MC, "", ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    pRTN_MSG = strTitle + "TRACK " + strHS_MC + "번[1층 출고 H/S]에서 픽킹대(" + strDEST_POS + ")로 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                    _pBdb.Commit();
                    InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strHS_MC, strDEST_POS);
                    return true;
                }

                pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 3 : 1층 도착보고 (ECS ArrivedCheck2/5 - ARRIVE_CV 의 PLC 한정판)
        //   목적지 트랙 도착(RET_READY + 작업번호 일치) → 상위 보고 + 작업 삭제(HIS 이관)
        // ─────────────────────────────────────────────────────────────────
        private bool CV_ARRIVE_PLC(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT JM.*, CD.*                                ";
                strSql += CRLF + "   FROM CV_DATA CD                                ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                           ";
                strSql += CRLF + "     ON CD.WH_TYP             = JM.WH_TYP 	    ";
                strSql += CRLF + "    AND CD.HOST_STN_NO       = JM.DEST_POS 	    ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD         = JM.LUGG_NO        ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP           ";
                strSql += CRLF + "    AND CD.PLC_NO             = :CV_PLC           ";   // 3층 해당 PLC 한정 (ECS m_nNum 게이트)
                strSql += CRLF + "    AND (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_RET | cDefApp.STN_KIND_ARV) + " <> 0)  ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '1'               ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'               ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'               ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '11'              ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null                   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO = "";
                string strMC_NO = "";
                _pBdb.BeginTrans();

                for (int i = 0; i < nSelCnt; i++)
                {
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strMC_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    string strJOB_TYP = "" + _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "1" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();

                    // 상위 TASK에 도착 보고 (LFC 인터페이스 테이블)
                    if (UPDATE_IF_LUGG_STA(strWH_TYP, strLUGG_NO, "90", ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    /*
                     * 여기서 작업을 지우면 안 된다.
                     *
                     *   HOST 태스크는 JOB_MST 의 상태로 보고 대상을 고른다.
                     *     GetJobCompleteReport(19)  출고작업 완료 보고(CV 완료)
                     *     GetJobCompleteReport(29)  입고작업 완료 보고(SC 완료)
                     *   행을 지워 버리면 보고할 것이 없어져 완료보고(F)가 나가지 않는다.
                     *   상위는 그 작업이 끝난 줄 모르니 다음 작업도 만들지 않아,
                     *   이동 -> 입고 -> 출고 순환이 첫 단계에서 멈췄다.
                     *
                     *   원본 참고 구현(CLS/cThread_CV.cs)도 같은 자리에 이렇게 적어 두었다.
                     *     "목적지 이동완료 (도착보고시 기존작업삭제 후 MES에서 새작업을
                     *      생성하기에 JOB_STATUS = '19' 로 처리한다."
                     *   거기서도 DELETE_JOB_DATA 는 주석 처리돼 있다.
                     *   실제 삭제는 HOST 태스크가 완료보고를 보낸 뒤에 한다.
                     */
                    if (UPDATE_JOB_DATA(ST_CV_DONE, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                }

                pRTN_MSG = strTitle + "TRACK " + strMC_NO + "번[1층 출고대]에서 HOST_TASK를 통해서 완료보고 요청하였습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_DONE);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }



        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 0 : 신규 작업 접수 (99 → 10 / 20)
        //   상위가 넣은 신규('99')를 어느 설비 구간에서 시작할지 정한다.
        //     이동(6)/입고(1) : CV 에서 시작한다  → 10(CV 구동대기)
        //     그 밖(출고 등)  : 크레인에서 시작한다 → 20(SC 구동요구)
        //   층이 아니라 작업 단위의 일이라 1F 스레드에서 한 번만 돌린다.
        // ─────────────────────────────────────────────────────────────────
        private bool JOB_ACCEPT(string strWH_TYP, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT LUGG_NO, JOB_TYP        ";
                strSql += CRLF + "   FROM JOB_MST                 ";
                strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP    ";
                strSql += CRLF + "    AND JOB_STATUS = '" + ST_NEW + "' ";
                strSql += CRLF + "  ORDER BY LUGG_NO              ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strMsg = "";
                _pBdb.BeginTrans();

                for (int i = 0; i < nSelCnt; i++)
                {
                    string strLUGG_NO = _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    string strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();

                    // CV 에서 시작하는 작업인가
                    bool bCvFirst = (strJOB_TYP == JT_STO) || (strJOB_TYP == JT_MOVE);
                    string strNext = bCvFirst ? ST_CV_WAIT : ST_SC_WAIT;

                    if (UPDATE_JOB_DATA(strNext, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    strMsg += (strMsg == "" ? "" : ", ") + strLUGG_NO + "→" + strNext;
                    InsertLog(SCH_WH_TYP, strTitle + "작업 " + strLUGG_NO + " 접수 (상태 " + strNext + ")",
                              "", "", strLUGG_NO, strNext, "", "", false);
                }

                pRTN_MSG = strTitle + "신규 작업을 접수했습니다. [" + strMsg + "]";
                _pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 5 : 입고 H/S 도착 -> 크레인 입고 지시 (레거시 ECS CSc::Store)
        //   화물이 크레인 입고 H/S 에 올라오면 그 크레인에 입고를 지시한다.
        //     지시 대상 크레인 : JOB_MST.DEST_POS (901~911)
        //     넣을 랙 위치     : JOB_MST.DEST_LOCATION (상위가 준 값)
        //   입고 H/S 인지는 CV_DATA.STOHS_READY_RD 로 안다. 그 값은 CV 태스크가
        //   DeviceMap 의 ScStoHS 영역에서 읽어 채운다.
        // ─────────────────────────────────────────────────────────────────
        private bool SC_STO_CMD_PLC(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT JM.LUGG_NO, JM.JOB_TYP, JM.DEST_POS, JM.DEST_LOCATION, CD.MC_NO  ";
                strSql += CRLF + "   FROM JOB_MST JM                                        ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD                                   ";
                strSql += CRLF + "     ON CD.WH_TYP           = JM.WH_TYP                   ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD       = JM.LUGG_NO                  ";
                strSql += CRLF + "  INNER JOIN SC_DATA SD                                   ";
                strSql += CRLF + "     ON SD.WH_TYP           = JM.WH_TYP                   ";
                strSql += CRLF + "    AND SD.SC_NO            = JM.DEST_POS                 ";
                strSql += CRLF + "  WHERE JM.WH_TYP           = :WH_TYP                     ";
                strSql += CRLF + "    AND JM.JOB_TYP          = '" + JT_STO + "'            ";
                strSql += CRLF + "    AND JM.JOB_STATUS       = '" + ST_CV_RUN + "'         ";   // CV 구동중
                strSql += CRLF + "    AND CD.PLC_NO           = :CV_PLC                     ";
                strSql += CRLF + "    AND CD.STOHS_READY_RD   = '1'                         ";   // 입고 H/S 준비
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '1'                         ";   // 재하
                strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                         ";
                strSql += CRLF + "    AND CD.ERROR_CODE       IN ('0','0000')               ";
                strSql += CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')          ";
                strSql += CRLF + "    AND SD.OD_RQ_YN         = 'N'                         ";   // 크레인이 지시를 받을 수 있는 상태
                strSql += CRLF + "    AND SD.ERR_CODE_RD      = '0000'                      ";
                strSql += CRLF + "    AND SD.AUTO_MODE_RD     = '1'                         ";
                strSql += CRLF + "  LIMIT 1                                                 ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;

                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO  = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                string strSC_NO    = _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                string strDEST_LOC = _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();
                string strMC_NO    = _pBdb.mDtMain.Rows[0]["MC_NO"].ToString();

                _pBdb.BeginTrans();

                if (UpdateScData(strSC_NO, JT_STO, strLUGG_NO, "", strDEST_LOC, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 크레인 미준비 - 다음 사이클 재시도
                }

                if (UPDATE_JOB_DATA(ST_SC_RUN, strLUGG_NO, strWH_TYP, JT_STO, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + "TRACK " + strMC_NO + "번[입고 H/S]에서 SC_TASK를 통해서 "
                         + strSC_NO + "호기에 입고 지시하였습니다. [작업번호:" + strLUGG_NO + "][도착LOC:" + strDEST_LOC + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_SC_RUN, strMC_NO, strSC_NO);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }


        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 7 : 출고 지시 (20 → 21) - 레거시 ECS CSc::Retrieve
        //   상위가 넣은 출고 작업을 크레인에 넘긴다.
        //     꺼낼 곳   : JM.START_LOCATION (랙 위치)
        //     내려놓을 곳 : 그 호기의 출고 H/S (SC_HS_DEF, 층마다 HS_NO 가 다르다)
        //     크레인    : JM.START_POS (출고 작업은 출발지가 호기다)
        //   출고 H/S 가 비어 있어야 크레인이 내려놓을 수 있다.
        //   이어받는 곳은 CV_RETHS_PLC 다. 그쪽이 JM.HS_TRACK_NO 로 트랙을 찾으므로
        //   여기서 그 값을 채워 준다.
        // ─────────────────────────────────────────────────────────────────
        private bool SC_RET_CMD_PLC(string strWH_TYP, string strHS_NO, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                // ── 1) 지시할 출고 작업을 고른다 (호기는 아직 모른다)
                strSql = "";
                strSql += CRLF + " SELECT LUGG_NO, JOB_TYP, START_POS, START_LOCATION, DEST_POS ";
                strSql += CRLF + "   FROM JOB_MST                                            ";
                strSql += CRLF + "  WHERE WH_TYP           = :WH_TYP                         ";
                strSql += CRLF + "    AND JOB_STATUS       = '" + ST_SC_WAIT + "'            ";   // 20 = SC 구동요구
                strSql += CRLF + "  ORDER BY JOB_PRIORITY DESC, LUGG_NO                      ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                DataTable dtJob = _pBdb.mDtMain.Copy();

                string strLUGG_NO = "", strJOB_TYP = "", strSC_NO = "", strSTART_LOC = "";
                string strDEST_POS = "", strHS_MC_NO = "";
                bool bFound = false;

                for (int i = 0; i < dtJob.Rows.Count; i++)
                {
                    strLUGG_NO   = dtJob.Rows[i]["LUGG_NO"].ToString();
                    strJOB_TYP   = dtJob.Rows[i]["JOB_TYP"].ToString();
                    strSTART_LOC = dtJob.Rows[i]["START_LOCATION"].ToString();
                    strDEST_POS  = dtJob.Rows[i]["DEST_POS"].ToString();

                    if (IsRetJobType(strJOB_TYP) == false)
                        continue;

                    // ── 2) 호기를 정한다. 상위가 호기를 줬으면 그대로, 아니면 랙 뱅크에서 구한다.
                    strSC_NO = dtJob.Rows[i]["START_POS"].ToString();
                    if (IsScNo(strSC_NO) == false)
                    {
                        if (GetScNoByLocation(strSTART_LOC, ref strSC_NO) == false)
                            continue;
                    }

                    // ── 3) 그 호기와 이 층의 출고 H/S 가 지시를 받을 수 있는 상태인지 본다
                    strSql = "";
                    strSql += CRLF + " SELECT SHD.HS_MC_NO                                      ";
                    strSql += CRLF + "   FROM SC_DATA SD                                        ";
                    strSql += CRLF + "  INNER JOIN SC_HS_DEF SHD                                ";
                    strSql += CRLF + "     ON SHD.WH_TYP          = SD.WH_TYP                   ";
                    strSql += CRLF + "    AND SHD.SC_NO           = SD.SC_NO                    ";
                    strSql += CRLF + "    AND SHD.HS_NO           = :HS_NO                      ";   // 이 층의 출고 H/S
                    strSql += CRLF + "    AND SHD.HS_USE_YN       = 'Y'                         ";
                    strSql += CRLF + "  INNER JOIN CV_DATA CD                                   ";
                    strSql += CRLF + "     ON CD.WH_TYP           = SD.WH_TYP                   ";
                    strSql += CRLF + "    AND CD.MC_NO            = SHD.HS_MC_NO                ";
                    strSql += CRLF + "  WHERE SD.WH_TYP           = :WH_TYP                     ";
                    strSql += CRLF + "    AND SD.SC_NO            = :SC_NO                      ";
                    strSql += CRLF + "    AND SD.OD_RQ_YN         = 'N'                         ";   // 크레인이 지시를 받을 수 있다
                    strSql += CRLF + "    AND SD.ERR_CODE_RD      = '0000'                      ";
                    strSql += CRLF + "    AND SD.AUTO_MODE_RD     = '1'                         ";
                    strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                         ";   // 출고 H/S 가 비어 있다
                    strSql += CRLF + "    AND CD.LUGG_NO_RD       IN ('','0','0000')            ";
                    strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                         ";
                    strSql += CRLF + "    AND CD.ERROR_CODE       IN ('0','0000')               ";
                    strSql += CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')          ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("HS_NO",  DbLang.VARCHAR).Value = strHS_NO;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strSC_NO;

                    nSelCnt = _pBdb.ExcuteQry(strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }
                    if (nSelCnt == 0)
                        continue;   // 이 층 소관이 아니거나 아직 받을 수 없다

                    strHS_MC_NO = _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString();
                    bFound = true;
                    break;
                }

                if (bFound == false)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();

                if (UpdateScData(strSC_NO, JT_RET, strLUGG_NO, strSTART_LOC, "", ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 크레인 미준비 - 다음 사이클 재시도
                }

                // 이어받을 CV_RETHS_PLC 가 HS_TRACK_NO 로 출고 H/S 트랙을 찾는다
                if (UPDATE_JOB_DATA(ST_SC_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG,
                                    "0", "0", strHS_MC_NO) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + strSC_NO + "호기에 출고를 지시하였습니다. [작업번호:" + strLUGG_NO
                         + "][출발LOC:" + strSTART_LOC + "][출고H/S:" + strHS_MC_NO + "][도착지:" + strDEST_POS + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_SC_RUN, strHS_MC_NO, strSC_NO);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 6 : 크레인 작업 완료 -> 작업 29(SC 구동완료 보고)
        //   D110(COMPLETE_RD)이 0 이 아니면 완료다. (1=포크1, 2=포크2, 3=전체)
        //   어느 크레인인지는 ITN_LUGG_FK1 로 안다. 작업대 번호로 찾으면 안 된다 -
        //   출고 작업은 출발지가 '000' 이고 도착지는 출고대라 크레인이 아니다.
        //   입고는 29 가 최종이라 HOST_TASK 가 완료보고(F)를 보내고 작업을 지운다.
        //   출고는 29 이후 CV 구간(11→15→19)이 남는다.
        //   완료를 확인했으면 포크 데이터를 지워 크레인을 다음 작업에 쓸 수 있게 한다.
        // ─────────────────────────────────────────────────────────────────
        private bool SC_COMP_CHK(string strWH_TYP, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT JM.LUGG_NO, JM.JOB_TYP, SD.SC_NO, SD.COMPLETE_RD ";
                strSql += CRLF + "   FROM JOB_MST JM                                       ";
                strSql += CRLF + "  INNER JOIN SC_DATA SD                                  ";
                strSql += CRLF + "     ON SD.WH_TYP           = JM.WH_TYP                  ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK1     = JM.LUGG_NO                 ";   // 이 작업을 들고 있는 크레인
                strSql += CRLF + "  WHERE JM.WH_TYP           = :WH_TYP                    ";
                strSql += CRLF + "    AND JM.JOB_STATUS       = '" + ST_SC_RUN + "'        ";
                strSql += CRLF + "    AND COALESCE(SD.COMPLETE_RD,'0') NOT IN ('','0')     ";   // 작업완료표시
                strSql += CRLF + "    AND SD.READ_UPD_DT      > SD.WRITE_UPD_DT            ";   // 지시를 쓴 뒤에 읽은 값이어야 한다
                strSql += CRLF + "    AND SD.ERR_CODE_RD      = '0000'                     ";
                strSql += CRLF + "  ORDER BY JM.LUGG_NO                                    ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                string strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                string strSC_NO   = _pBdb.mDtMain.Rows[0]["SC_NO"].ToString();

                _pBdb.BeginTrans();

                if (UPDATE_JOB_DATA(ST_SC_DONE, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                if (ClearScFork1(strSC_NO, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + strSC_NO + "호기가 작업을 완료했습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_SC_DONE, "", strSC_NO);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        /*
         * ClearScFork1 :: 완료한 크레인의 포크#1 데이터를 지운다.
         *
         *   SC 태스크가 CMD_RQ_YN='Y' 를 보고 CMD_RQ_ID 별로 PLC 에 명령을 쓴다.
         *   DELFK1 은 D199=16(포크#1 데이터 삭제)이고, 기록 뒤 SC 태스크가
         *   ITN_LUGG_FK1 을 '0' 으로 되돌린다. 이 현장은 SINGLE 포크라 FK1 만 쓴다.
         */
        private bool ClearScFork1(string strScNo, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE SC_DATA                        ";
                strSql += CRLF + "    SET CMD_RQ_YN  = 'Y'               ";
                strSql += CRLF + "      , CMD_RQ_ID  = 'DELFK1'          ";
                strSql += CRLF + "      , OD_USER_ID = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP           ";
                strSql += CRLF + "    AND SC_NO      = :SC_NO            ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strScNo;

                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "SC_DATA 포크 삭제 지시 오류:" + _pBdb.ErrMsg; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }
        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 4 : 픽킹 레인 진입 제한 (ECS MovingTrackCheckPlc3/6)
        //   레거시 : 레인 점유수 < 제한(6)이면 진입허가 비트를 PLC 워드 558 에 기록.
        //   To-Be  : DEAD_LOCK_ZONE_DEF(게이트=CUR_POS, 레인=BUFFERS, 제한=COUNT) 기반으로
        //            기존 cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE 판정 → 게이트 트랙의
        //            TR_PAUSE_OD 를 '0'(진입허가)/'1'(대기) 로 제어한다.
        //   ★현장확인 : CV 통신 Task 가 TR_PAUSE_OD 를 PLC 진입허가(레거시 워드 558)로
        //               반영하는지 확인 필요. (변화가 있을 때만 기록하여 부하 최소화)
        // 이거는 좀더 확인이 필요함! => 일단 갯수 카운트를 하고 있는지가 확인이 필요함!
        // ─────────────────────────────────────────────────────────────────
        //private bool MOVING_TRACK_CHECK_PLC(string strWH_TYP, string strPlcDigit, string strTitle, ref string pRTN_MSG)
        //{
        //    try
        //    {
        //        int nSelCnt = 0;
        //        string strSql = "";

        //        pRTN_MSG = strTitle;

        //        // 게이트(대기) 트랙에 화물이 재하된 레인 정의 조회
        //        strSql = "";
        //        strSql += CRLF + " SELECT DISTINCT DLZ.CUR_POS, DLZ.CUR_DEST_POS,                 ";
        //        strSql += CRLF + "        CD.LUGG_NO_RD, CD.TR_PAUSE_RD                           ";
        //        strSql += CRLF + "   FROM DEAD_LOCK_ZONE_DEF DLZ                                  ";
        //        strSql += CRLF + "  INNER JOIN CV_DATA CD                                         ";
        //        strSql += CRLF + "     ON CD.WH_TYP = DLZ.WH_TYP AND CD.MC_NO = DLZ.CUR_POS       ";
        //        strSql += CRLF + "  WHERE DLZ.WH_TYP   = :WH_TYP                                  ";
        //        strSql += CRLF + "    AND DLZ.USE_YN   = 'Y'                                      ";
        //        strSql += CRLF + "    AND DLZ.CUR_POS LIKE :PFX                                   ";   // '3%' / '6%' (해당 층 라인)
        //        strSql += CRLF + "    AND CD.SENSOR0_DATA_RD = '1'                                ";   // 게이트에 화물 재하
        //        strSql += CRLF + "    AND CD.AUTO_MODE_RD    = '1'                                ";
        //        strSql += CRLF + "    AND CD.ERROR_CODE     IN ('0','0000')                       ";
        //        strSql += CRLF + "    AND COALESCE(CD.DEST_POS_RD,'') = DLZ.CUR_DEST_POS          ";   // 화물 목적지 = 레인 방향
        //        _pBdb.mComMain.CommandType = CommandType.Text;
        //        _pBdb.mComMain.Parameters.Clear();
        //        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
        //        _pBdb.mComMain.Parameters.Add("PFX", DbLang.VARCHAR).Value = strPlcDigit + "%";
        //        nSelCnt = _pBdb.ExcuteQry(strSql);
        //        if (nSelCnt < 0)
        //        {
        //            pRTN_MSG += _pBdb.ErrMsg;
        //            return false;
        //        }
        //        if (nSelCnt == 0)
        //        {
        //            pRTN_MSG = "";
        //            return true;
        //        }

        //        DataTable dtGate = _pBdb.mDtMain.Copy();
        //        pRTN_MSG = "";

        //        for (int i = 0; i < dtGate.Rows.Count; i++)
        //        {
        //            string strGATE = "" + dtGate.Rows[i]["CUR_POS"].ToString();
        //            string strDEST = "" + dtGate.Rows[i]["CUR_DEST_POS"].ToString();
        //            string strLUGG = "" + dtGate.Rows[i]["LUGG_NO_RD"].ToString();

        //            // 레인 점유수 판정 (기존 공용 함수 - 초과 시 false)
        //            string strChkMsg = "";
        //            DataTable dtDeadLock = new DataTable();
        //            bool bEnterOk = cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strGATE, strDEST, ref strChkMsg, ref dtDeadLock);

        //            // 진입허가('0') / 대기('1') - 값이 바뀔 때만 기록
        //            string strPause = bEnterOk ? "0" : "1";
        //            int nChg = UPDATE_CV_TR_PAUSE(strWH_TYP, strGATE, strPause, ref pRTN_MSG);
        //            if (nChg < 0) return false;
        //            if (nChg > 0)
        //            {
        //                pRTN_MSG = strTitle + "TRACK " + strGATE + "번[픽킹 레인 게이트] " +
        //                           (bEnterOk ? "진입 허가" : "레인 만석 - 진입 대기") +
        //                           " (목적지:" + strDEST + ", 작업번호:" + strLUGG + ")";
        //                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG, "", strGATE, strDEST, false);
        //                return true;
        //            }
        //        }

        //        pRTN_MSG = "";
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        pRTN_MSG = strTitle + ex.ToString();
        //        return false;
        //    }
        //}

        /// <summary>
        /// 게이트 트랙의 TR_PAUSE_OD 기록 (값이 다를 때만) - 반환 : 변경 행수(0=변화없음, -1=오류)
        /// ★현장확인 : CV Task 가 TR_PAUSE_OD 를 PLC 에 반영해야 한다 (레거시 진입허가 워드 558 대응)
        /// </summary>
        private int UPDATE_CV_TR_PAUSE(string strWH_TYP, string strMC_NO, string strPause, ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE CV_DATA                                     ";
                strSql += CRLF + "    SET TR_PAUSE_OD  = :TR_PAUSE                    ";
                strSql += CRLF + "      , OD_USER_ID   = 'IOTASK'                     ";
                strSql += CRLF + "      , OD_UPD_DT    = " + DbLang.SYSDATE + "        ";
                strSql += CRLF + "  WHERE WH_TYP       = :WH_TYP                      ";
                strSql += CRLF + "    AND MC_NO        = :MC_NO                       ";
                strSql += CRLF + "    AND TR_PAUSE_OD IS DISTINCT FROM :TR_PAUSE2     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("TR_PAUSE", DbLang.VARCHAR).Value = strPause;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_NO;
                _pBdb.mComMain.Parameters.Add("TR_PAUSE2", DbLang.VARCHAR).Value = strPause;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { pRTN_MSG += "TR_PAUSE_OD 기록 오류:" + _pBdb.ErrMsg; return -1; }
                return n;
            }
            catch (Exception ex) { pRTN_MSG += ex.Message; return -1; }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 5/6 : 구↔신 라인 이음새 데이터 복사/삭제 (ECS Copy/DeleteTrackData2/5)
        //   레거시 : FROM 트랙의 작업데이터(화물번호/작업구분/목적지)를 TO 트랙에 복사한 뒤,
        //            반영이 확인되면 FROM 트랙 데이터를 삭제 (화물번호가 두 PLC 에 동시에
        //            존재하지 않도록 하는 핸드오프)
        // ─────────────────────────────────────────────────────────────────
        private bool COPY_TRACK_DATA(string strWH_TYP, string strFROM_MC, string strTO_MC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string strKey = strFROM_MC + "_" + strTO_MC;

                pRTN_MSG = strTitle;

                // FROM 재하 작업 + TO 빈 트랙(물리 도착) 확인
                strSql = "";
                strSql += CRLF + " SELECT CDF.LUGG_NO_RD, CDT.PLC_NO AS TO_PLC,                    ";
                strSql += CRLF + "        JM.JOB_TYP, JM.PRODUCT_SIZE, JM.TRAY_LEV, JM.TURN,       ";
                strSql += CRLF + "        JM.DEST_POS                                              ";
                strSql += CRLF + "   FROM CV_DATA CDF                                              ";
                strSql += CRLF + "  INNER JOIN CV_DATA CDT                                         ";
                strSql += CRLF + "     ON CDT.WH_TYP = CDF.WH_TYP AND CDT.MC_NO = :TO_MC           ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                                          ";
                strSql += CRLF + "     ON JM.WH_TYP = CDF.WH_TYP AND JM.LUGG_NO = CDF.LUGG_NO_RD   ";
                strSql += CRLF + "  WHERE CDF.WH_TYP = :WH_TYP                                     ";
                strSql += CRLF + "    AND CDF.MC_NO  = :FROM_MC                                    ";
                strSql += CRLF + "    AND CDF.LUGG_NO_RD NOT IN ('','0','0000')                    ";
                strSql += CRLF + "    AND CDF.SENSOR0_DATA_RD  = '0'                               ";   // FROM 센서 이탈 (이음새 통과 중)
                strSql += CRLF + "    AND CDT.LUGG_NO_RD IN ('','0','0000')                        ";   // TO 데이터 비어있음
                strSql += CRLF + "    AND CDT.STO_READY_RD     = '1'                               ";
                strSql += CRLF + "    AND CDT.SENSOR0_DATA_RD  = '1'                               ";   // TO 에 물리 도착
                strSql += CRLF + "    AND CDT.OD_RQ_YN         = 'N'                               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("TO_MC", DbLang.VARCHAR).Value = strTO_MC;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("FROM_MC", DbLang.VARCHAR).Value = strFROM_MC;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO_RD"].ToString();
                string strTO_PLC = "" + _pBdb.mDtMain.Rows[0]["TO_PLC"].ToString();
                string strJOB_TYP = "" + _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                string strTRAY_TYP = "" + _pBdb.mDtMain.Rows[0]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PRODUCT_SIZE"].ToString();
                string strTRAY_LEV = "" + _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString();
                string strIS_TURN = "" + _pBdb.mDtMain.Rows[0]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TURN"].ToString();
                string strDEST_POS = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();

                // 동일 화물 중복 복사 방지
                if (m_dicSeamCopied.ContainsKey(strKey) && m_dicSeamCopied[strKey] == strLUGG_NO)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();
                if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strDEST_POS, strIS_TURN,
                                   strLUGG_NO, strWH_TYP, strTO_PLC, strTO_MC, "", ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }
                _pBdb.Commit();

                m_dicSeamCopied[strKey] = strLUGG_NO;
                pRTN_MSG = strTitle + "이음새 TRACK " + strFROM_MC + "→" + strTO_MC + " 작업 데이터 복사 지시. [작업번호:" + strLUGG_NO + "]";
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strFROM_MC, strTO_MC);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        private bool DELETE_TRACK_DATA(string strWH_TYP, string strFROM_MC, string strTO_MC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string strKey = strFROM_MC + "_" + strTO_MC;

                pRTN_MSG = "";

                // 복사 이력이 없으면 통과
                if (m_dicSeamCopied.ContainsKey(strKey) == false)
                    return true;

                string strLUGG_NO = m_dicSeamCopied[strKey];

                // TO 트랙에 복사가 반영(PLC readback)되었는지 확인
                strSql = "";
                strSql += CRLF + " SELECT CDF.PLC_NO AS FROM_PLC                                   ";
                strSql += CRLF + "   FROM CV_DATA CDT                                              ";
                strSql += CRLF + "  INNER JOIN CV_DATA CDF                                         ";
                strSql += CRLF + "     ON CDF.WH_TYP = CDT.WH_TYP AND CDF.MC_NO = :FROM_MC         ";
                strSql += CRLF + "  WHERE CDT.WH_TYP     = :WH_TYP                                 ";
                strSql += CRLF + "    AND CDT.MC_NO      = :TO_MC                                  ";
                strSql += CRLF + "    AND CDT.LUGG_NO_RD = :LUGG_NO                                ";   // 반영 확인
                strSql += CRLF + "    AND CDF.LUGG_NO_RD = :LUGG_NO2                               ";   // FROM 에 아직 잔존
                strSql += CRLF + "    AND CDF.OD_RQ_YN   = 'N'                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("FROM_MC", DbLang.VARCHAR).Value = strFROM_MC;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("TO_MC", DbLang.VARCHAR).Value = strTO_MC;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("LUGG_NO2", DbLang.VARCHAR).Value = strLUGG_NO;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG = strTitle + _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    // 반영 전이거나 FROM 이 이미 비워짐 - FROM 이 비워졌으면 이력 정리
                    return true;
                }

                string strFROM_PLC = "" + _pBdb.mDtMain.Rows[0]["FROM_PLC"].ToString();

                // FROM 트랙 데이터 삭제 지시 (작업데이터 0 클리어 - 레거시 WriteTrackInfo(...,0,0,0))
                _pBdb.BeginTrans();
                if (UPDATE_CV_DATA("0", "0", "0", "0", "0", "0",
                                   strWH_TYP, strFROM_PLC, strFROM_MC, "", ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }
                _pBdb.Commit();

                m_dicSeamCopied.Remove(strKey);
                pRTN_MSG = strTitle + "이음새 TRACK " + strFROM_MC + " 작업 데이터 삭제 지시 (복사 완료 확인). [작업번호:" + strLUGG_NO + "]";
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strFROM_MC, strTO_MC);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        /// <summary>
        /// 출고대 가용 확인 : 빈 상태(무재하/AUTO/무에러) + 해당 출고대로 진입중인 화물 없음
        /// </summary>
        private bool CHECK_RET_LANE_READY(string strWH_TYP, string strMC_NO)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT COUNT(*) AS READY_CNT                                        ";
                strSql += CRLF + "   FROM CV_DATA CD                                                   ";
                strSql += CRLF + "  WHERE CD.WH_TYP           = :WH_TYP                                ";
                strSql += CRLF + "    AND CD.MC_NO            = :MC_NO                                 ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                                    ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                                    ";
                strSql += CRLF + "    AND CD.ERROR_CODE      IN ('0','0000')                           ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM CV_DATA CD2                        ";
                strSql += CRLF + "              WHERE CD2.WH_TYP      = :WH_TYP2                       ";
                strSql += CRLF + "                AND CD2.DEST_POS_OD = :MC_NO2                        ";
                strSql += CRLF + "                AND CD2.OD_RQ_YN    = 'Y')                           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_NO;
                _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO2", DbLang.VARCHAR).Value = strMC_NO;
                int nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt <= 0) return false;
                return Convert.ToInt32(_pBdb.mDtMain.Rows[0]["READY_CNT"]) > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 스테이션 번호 → 실트랙(MC_NO) 변환 (DEST_POS_DEF.TRACK_NO → MC_NO)
        /// </summary>
        private bool GET_DEST_POS_MC(string strWH_TYP, string strSTATION, ref string strMC, ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT DPD.MC_NO                          ";
                strSql += CRLF + "   FROM DEST_POS_DEF DPD                   ";
                strSql += CRLF + "  WHERE DPD.WH_TYP   = :WH_TYP             ";
                strSql += CRLF + "    AND DPD.TRACK_NO = :TRACK_NO           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strSTATION;
                int nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt <= 0)
                {
                    pRTN_MSG += "DEST_POS_DEF 에 스테이션(" + strSTATION + ") 정의가 없습니다.";
                    return false;
                }
                strMC = _pBdb.mDtMain.Rows[0]["MC_NO"].ToString();
                if (strMC == "")
                {
                    pRTN_MSG += "DEST_POS_DEF 스테이션(" + strSTATION + ")의 MC_NO 가 비어 있습니다.";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.Message;
                return false;
            }
        }

        #endregion


        // ─────────────────────────────────────────────────────────────────
        // DB Helper
        // ─────────────────────────────────────────────────────────────────
        #region DB Helper

        /// <summary>
        /// JOB_MST.JOB_STATUS 상태 변경 (기존 cThread_*.UPDATE_JOB_DATA 의 핵심부)
        /// </summary>
        private bool UpdateJobStatus(string strStatus, string strLuggNo, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE JOB_MST                       ";
                strSql += CRLF + "    SET JOB_STATUS  = :JOB_STATUS     ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_USER_ID = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP         ";
                strSql += CRLF + "    AND LUGG_NO     = :LUGG_NO        ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = strStatus;
                _pBdb.mComMain.Parameters.Add("WH_TYP",     DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO",    DbLang.VARCHAR).Value = strLuggNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "JOB_MST 상태변경 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "변경할 JOB_MST 작업이 없음(LUGG_NO:" + strLuggNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /// <summary>
        /// 신규 작업 상태 변경 : 상태 + 출발/도착 + S/C/HS 일괄 기록 (PrepareNewJobs 전용)
        ///   JOB_STATUS='99' 인 행만 갱신하여 중복 접수를 방지한다.
        /// </summary>
        private bool UpdateJobInvoke(string strStatus, string strLuggNo,
                                     string strStartPos, string strDestPos,
                                     string strScNo, string strHsTrack, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE JOB_MST                        ";
                strSql += CRLF + "    SET JOB_STATUS   = :JOB_STATUS     ";
                strSql += CRLF + "      , START_POS    = :START_POS      ";
                strSql += CRLF + "      , DEST_POS     = :DEST_POS       ";
                strSql += CRLF + "      , SC_NO        = :SC_NO          ";
                strSql += CRLF + "      , HS_TRACK_NO  = :HS_TRACK_NO    ";
                strSql += CRLF + "      , JOB_START_DT = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_DT       = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_USER_ID  = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP       = :WH_TYP         ";
                strSql += CRLF + "    AND LUGG_NO      = :LUGG_NO        ";
                strSql += CRLF + "    AND JOB_STATUS   = '" + ST_CV_WAIT + "' ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_STATUS",  DbLang.VARCHAR).Value = strStatus;
                _pBdb.mComMain.Parameters.Add("START_POS",   DbLang.VARCHAR).Value = strStartPos;
                _pBdb.mComMain.Parameters.Add("DEST_POS",    DbLang.VARCHAR).Value = strDestPos;
                _pBdb.mComMain.Parameters.Add("SC_NO",       DbLang.VARCHAR).Value = strScNo;
                _pBdb.mComMain.Parameters.Add("HS_TRACK_NO", DbLang.VARCHAR).Value = strHsTrack;
                _pBdb.mComMain.Parameters.Add("WH_TYP",      DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO",     DbLang.VARCHAR).Value = strLuggNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "JOB_MST 상태 변경 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "접수할 신규 작업이 없음(LUGG_NO:" + strLuggNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /// <summary>
        ///  핸드오프 : 상태 변경 + 출발지 치환 (CompleteCV→SC  / CompleteSC→CV )
        /// </summary>
        private bool UpdateJobLeg(string strStatus, string strNewStartPos, string strLuggNo, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE JOB_MST                       ";
                strSql += CRLF + "    SET JOB_STATUS  = :JOB_STATUS     ";
                strSql += CRLF + "      , START_POS   = :START_POS      ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_USER_ID = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP         ";
                strSql += CRLF + "    AND LUGG_NO     = :LUGG_NO        ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = strStatus;
                _pBdb.mComMain.Parameters.Add("START_POS",  DbLang.VARCHAR).Value = strNewStartPos;
                _pBdb.mComMain.Parameters.Add("WH_TYP",     DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO",    DbLang.VARCHAR).Value = strLuggNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "JOB_MST 변경 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "변경할 JOB_MST 작업이 없음(LUGG_NO:" + strLuggNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /// <summary>
        /// 목적지 갱신 : 공용 출고대가 물리 레인으로 분배된 경우 작업에 반영
        ///   (레거시 ECS 도 job 의 m_nDestPos 를 분배된 물리 레인으로 재기록)
        /// </summary>
        private bool UpdateJobDest(string strDestPos, string strLuggNo, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE JOB_MST                       ";
                strSql += CRLF + "    SET DEST_POS    = :DEST_POS       ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_USER_ID = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP         ";
                strSql += CRLF + "    AND LUGG_NO     = :LUGG_NO        ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDestPos;
                _pBdb.mComMain.Parameters.Add("WH_TYP",   DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO",  DbLang.VARCHAR).Value = strLuggNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "JOB_MST 목적지갱신 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "갱신할 JOB_MST 작업이 없음(LUGG_NO:" + strLuggNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /// <summary>
        /// CV_DATA 명령 지시 (기존 cThread_CV.UPDATE_CV_DATA)
        ///   _OD 컬럼 기록 + OD_RQ_YN='Y'. 유휴(OD_RQ_YN='N') + 무에러 행만 대상.
        /// </summary>
        private bool UpdateCvData(
                    string strJobTyp
            ,       string strDestPos
            ,       string strLuggNo
            ,       string strPlcNo
            ,       string strTrackNo
            , ref   string strRtn
            )
        {
            try
            {
                // 실제 CV_DATA 명령(_OD) 컬럼만 사용: JOB_TYP_OD, DEST_POS_OD, LUGG_NO_OD, OD_RQ_YN
                //   방향(입고0/출고1, D0310)은 통신 Task가 JOB_TYP 기반으로 PLC에 기록.
                string strSql = "";
                strSql += CRLF + " UPDATE CV_DATA                                 ";
                strSql += CRLF + "    SET JOB_TYP_OD  = :JOB_TYP_OD               ";
                strSql += CRLF + "      , DEST_POS_OD = :DEST_POS_OD              ";
                strSql += CRLF + "      , LUGG_NO_OD  = :LUGG_NO_OD               ";
                strSql += CRLF + "      , OD_RQ_YN    = 'Y'                       ";
                strSql += CRLF + "      , OD_USER_ID  = '" + OD_USER + "'         ";
                strSql += CRLF + "      , OD_UPD_DT   = " + DbLang.SYSDATE + "     ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP                   ";
                strSql += CRLF + "    AND PLC_NO      = :PLC_NO                   ";
                strSql += CRLF + "    AND MC_NO       = :TRACK_NO                 ";
                strSql += CRLF + "    AND OD_RQ_YN    = 'N'                       ";
                strSql += CRLF + "    AND (ERROR_CODE = '0' OR ERROR_CODE = '0000' OR ERROR_CODE IS NULL)";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_TYP_OD",  DbLang.VARCHAR).Value = strJobTyp;
                _pBdb.mComMain.Parameters.Add("DEST_POS_OD", DbLang.VARCHAR).Value = strDestPos;
                _pBdb.mComMain.Parameters.Add("LUGG_NO_OD",  DbLang.VARCHAR).Value = strLuggNo;
                _pBdb.mComMain.Parameters.Add("WH_TYP",      DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO",      DbLang.VARCHAR).Value = strPlcNo;
                _pBdb.mComMain.Parameters.Add("TRACK_NO",    DbLang.VARCHAR).Value = strTrackNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "CV_DATA 명령 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "지시할 CV_DATA 가 없음(TRACK_NO:" + strTrackNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        //*
        /// <summary>
        /// SC_DATA 이송 명령 지시 (레거시 ECS CSc::Store / CSc::Retrieve 의 명령 데이터 포팅)
        ///   레거시 Melsec 명령 D171~D192 : 명령(1=입고/2=출고), 화물번호, BANK/BAY/LEVEL, HS(=랭크)
        ///   - 입고(Store)    : 출발 = 입고 HS(랭크1), 도착 = 랙 위치(DEST_LOCATION 분해)
        ///   - 출고(Retrieve) : 출발 = 랙 위치(START_LOCATION 분해), 도착 = 출고 HS(랭크2)
        ///   위치 문자열 = BANK(2) + BAY(3) + LEVEL(2)  (레거시 LOCATION_LEN=7)
        ///   FK1 단일포크 사용(SC_DATA.SC_TYP='SINGLE'), WRITE_FLAG_OD='1' = 명령 기록 완료 플래그.
        /// </summary>
        private bool UpdateScData(
                    string strScNo
            ,       string strJobTyp
            ,       string strLuggNo
            ,       string strStartLoc
            ,       string strDestLoc
            , ref   string strRtn
            )
        {
            try
            {
                bool bStore = (strJobTyp == JT_STO);

                // 랙 위치 분해 (입고는 도착, 출고는 출발이 랙)
                string strBank = "0", strBay = "0", strLev = "0";
                string strLoc = bStore ? strDestLoc : strStartLoc;
                if (!ParseLocation(strLoc, ref strBank, ref strBay, ref strLev))
                {
                    strRtn += "랙 위치 형식 오류(LOC:" + strLoc + ")";
                    return false;
                }

                string strSql = "";
                strSql += CRLF + " UPDATE SC_DATA                                        ";
                strSql += CRLF + "    SET JOB_TYP_OD          = :JOB_TYP_OD              ";
                strSql += CRLF + "      , LUGG_NO_FK1_OD      = :LUGG_NO_FK1_OD          ";
                strSql += CRLF + "      , START_BANK_FK1_OD   = :START_BANK              ";
                strSql += CRLF + "      , START_BAY_FK1_OD    = :START_BAY               ";
                strSql += CRLF + "      , START_LEVEL_FK1_OD  = :START_LEVEL             ";
                strSql += CRLF + "      , START_HSPOS_FK1_OD  = :START_HSPOS             ";
                strSql += CRLF + "      , DEST_BANK_FK1_OD    = :DEST_BANK               ";
                strSql += CRLF + "      , DEST_BAY_FK1_OD     = :DEST_BAY                ";
                strSql += CRLF + "      , DEST_LEVEL_FK1_OD   = :DEST_LEVEL              ";
                strSql += CRLF + "      , DEST_HSPOS_FK1_OD   = :DEST_HSPOS              ";
                strSql += CRLF + "      , USE_FK_OD           = '1'                      ";   // FK1 사용 (SINGLE 포크)
                strSql += CRLF + "      , WRITE_FLAG_OD       = '1'                      ";   // 명령 기록 완료 (레거시 D191=1)
                strSql += CRLF + "      , OD_RQ_YN            = 'Y'                      ";
                strSql += CRLF + "      , OD_USER_ID          = '" + OD_USER + "'        ";
                strSql += CRLF + "      , OD_UPD_DT           = " + DbLang.SYSDATE + "   ";
                strSql += CRLF + "  WHERE WH_TYP              = :WH_TYP                  ";
                strSql += CRLF + "    AND SC_NO               = :SC_NO                   ";
                strSql += CRLF + "    AND OD_RQ_YN            = 'N'                      ";
                strSql += CRLF + "    AND ERR_CODE_RD         = '0000'                   ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_TYP_OD",     DbLang.VARCHAR).Value = bStore ? SC_CMD_STORE : SC_CMD_RETRIEVE;
                _pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_OD", DbLang.VARCHAR).Value = strLuggNo;
                _pBdb.mComMain.Parameters.Add("START_BANK",     DbLang.VARCHAR).Value = bStore ? "0" : strBank;
                _pBdb.mComMain.Parameters.Add("START_BAY",      DbLang.VARCHAR).Value = bStore ? "0" : strBay;
                _pBdb.mComMain.Parameters.Add("START_LEVEL",    DbLang.VARCHAR).Value = bStore ? "0" : strLev;
                _pBdb.mComMain.Parameters.Add("START_HSPOS",    DbLang.VARCHAR).Value = bStore ? "1" : "0";   // 입고 = 입고HS(랭크1) 출발
                _pBdb.mComMain.Parameters.Add("DEST_BANK",      DbLang.VARCHAR).Value = bStore ? strBank : "0";
                _pBdb.mComMain.Parameters.Add("DEST_BAY",       DbLang.VARCHAR).Value = bStore ? strBay  : "0";
                _pBdb.mComMain.Parameters.Add("DEST_LEVEL",     DbLang.VARCHAR).Value = bStore ? strLev  : "0";
                _pBdb.mComMain.Parameters.Add("DEST_HSPOS",     DbLang.VARCHAR).Value = bStore ? "0" : "2";   // 출고 = 출고HS(랭크2) 도착
                _pBdb.mComMain.Parameters.Add("WH_TYP",         DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO",          DbLang.VARCHAR).Value = strScNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "SC_DATA 명령 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "지시할 SC_DATA 가 없음(SC_NO:" + strScNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }
        //*/


        /*
         * IsScNo :: 크레인 번호(901~911) 형태인가.
         *   상위가 출발지에 호기를 주는 현장도 있고 '000' 을 주는 현장도 있다.
         */
        private bool IsScNo(string strPos)
        {
            int nPos;
            if (int.TryParse(strPos, out nPos) == false) return false;
            return ((nPos > 900) && (nPos < 1000));
        }

        /*
         * GetScNoByLocation :: 랙 위치에서 담당 호기를 구한다.
         *
         *   호기 = (뱅크 + 1) / 2      뱅크 1,2 -> 1호기 / 3,4 -> 2호기 / ...
         *   WCS_TASK_HOST 의 modDefApp.GetStackerNum 과 같은 식이다.
         *   확인 : 입고 1002 의 랙 07-001-01 -> (7+1)/2 = 4 -> 904 호기
         */
        private bool GetScNoByLocation(string strLoc, ref string strScNo)
        {
            string strBank = "", strBay = "", strLev = "";
            if (ParseLocation(strLoc, ref strBank, ref strBay, ref strLev) == false)
                return false;

            int nBank;
            if (int.TryParse(strBank, out nBank) == false) return false;
            if (nBank < 1) return false;

            strScNo = (900 + ((nBank + 1) / 2)).ToString();
            return true;
        }
        /*
         * ParseLocation :: 랙 위치 문자열을 뱅크/베이/단으로 나눈다.
         *
         *   상위가 주는 형식이 현장마다 다르다. "07-001-01" 처럼 구분자를 넣기도 하고
         *   "0700101" 처럼 붙여 쓰기도 한다. 숫자만 뽑아 2/3/2 로 자른다.
         *   (레거시 LOCATION_LEN=7 : BANK 2 + BAY 3 + LEVEL 2)
         */
        private bool ParseLocation(string strLoc, ref string strBank, ref string strBay, ref string strLev)
        {
            if (strLoc == null)
                return false;

            string strDigit = "";
            foreach (char ch in strLoc)
            {
                if ((ch >= '0') && (ch <= '9'))
                    strDigit += ch;
            }

            if (strDigit.Length != 7)
                return false;

            strBank = strDigit.Substring(0, 2);
            strBay  = strDigit.Substring(2, 3);
            strLev  = strDigit.Substring(5, 2);
            return true;
        }
        /// <summary>DataRow 값 추출 (null/공백 안전, Trim)</summary>
        private string GetVal(DataRow row, string col)
        {
            if (row[col] == null || row[col] == DBNull.Value) return "";
            return row[col].ToString().Trim();
        }
        #endregion
    }
}
