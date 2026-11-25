// 작성자   : WCS Scheduler
// 통합판   : WCS_IO_SCH_Original 의 3층(3F) 담당 스케줄러 스레드
// 작성일   : 2026-06-23 (2026-06-27 기존 JOB_MST 스키마 통합)
// 수정일   : 2026-07-09 KET(한국단자) 3층(3F) 자동운전 로직 추가
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
    public class cThread_SCH_3F : IOSchDB
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

        #endregion

        #region 상수 정의




        #endregion

        #region 생성자
        public cThread_SCH_3F(int Id)
        {
            m_nId = Id;
        }
        #endregion

        // ※ 메시지 출력 헬퍼(MakeMsg / MakeMsg_Error / MakeMsg_Imp / MakeMsg_Error_NoLog)는
        //    기반 클래스(IOSchDB)의 공용 구현을 사용한다. (중복 선언 제거 - CS0108 숨김 경고 해소)

        // ─────────────────────────────────────────────────────────────────
        // 메인 Thread 루프 (200ms 폴링)
        // ─────────────────────────────────────────────────────────────────
        #region 지시 전용 스레드
        /*
         * @.WMS 지시 -> 설비 구동까지의 지연을 줄이려고 "지시를 내는 함수"만 따로 0.05초 주기로 돈다. (2026-11-08)
         *   전에는 메인 바퀴(함수 20여 개 + 함수마다 10ms + 바퀴 200ms)를 한 번 돌아야 접수(99)->지시(20->21)가 이어졌다.
         *   여기서 도는 것 : JobAccept, StartInvokeCheck3, StartInvokeCheck6, StoHsCheck3, StoHsCheck6, RetCmdCheck, RackToRackCheck
         *   나머지(도착 검사, 완료, 출고 H/S, 청소 등)는 메인 바퀴에 그대로 둔다. 같은 함수가 두 곳에서 돌지 않게 메인에서는 뺐다.
         *   자기 DB 연결을 쓴다.
         */
        protected override IOSchDB CreateWorker() { return new cThread_SCH_3F(m_nId); }

        protected override SchFunc[] CmdFuncs()
        {
            return new SchFunc[] { JobAccept, StartInvokeCheck3, StartInvokeCheck6, StoHsCheck3, StoHsCheck6, RetCmdCheck, RackToRackCheck };
        }
        #endregion

        #region 도착 검사 전용 스레드
        /*
         * @.도착 검사(상태 11 -> 19)를 메인 바퀴에서 빼서 0.05초 주기로 도는 전용 스레드로 옮겼다. (2026-11-09)
         *   메인 바퀴가 부하가 걸리면 1초 남짓 걸려, 도착 신호를 보고도 한 바퀴를 더 기다렸다. (1F 는 전부터 이렇게 돈다)
         *   여기서 도는 것 : ArrivedCheck3, ArrivedCheck6, SizeChkWatch, MovingTrackCheckPlc3, MovingTrackCheckPlc6
         *   자기 DB 연결을 쓴다. 메인에서는 이 함수들을 뺐다.
         */
        protected override SchFunc[] ArvFuncs()
        {
            return new SchFunc[] { ArrivedCheck3, ArrivedCheck6, SizeChkWatch,
                                   MovingTrackCheckPlc3,   // 3F 구라인 픽킹 레인 진입 허가 (게이트에서 기다리는 화물이 허가를 기다리는 시간을 줄이려고 빠른 스레드에서 돈다)
                                   MovingTrackCheckPlc6 }; // 3F 신라인 픽킹 레인 진입 허가
        }
        #endregion

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

            StartCmdWorker();   // 지시 전용 스레드
            StartArvWorker();   // 도착 검사 전용 스레드

            while (!m_bManualStop)
            {
                Thread.Sleep(200);

                // _pBdb null 안전 확인
                //  연결이 끊긴 것을 못 보면 "전송 연결에 데이터를 쓸 수 없습니다" 만 계속 나온다.
                //  null 만 보지 말고 실제 연결 상태를 본다.
                if (!IsDbAlive())
                {
                    if (!DBReopen()) Thread.Sleep(3000);
                    continue;
                }

                try
                {

                    // ── 3층(3F) 처리 : ECS *3/*6 함수 포팅 (구라인 PLC 03 / 신라인 PLC 06)

                    RunSchFunc(RetInvokeCheck3);        // 3F 구라인 출고HS → 픽킹대 지시
                    RunSchFunc(RetInvokeCheck6);        // 3F 신라인 출고HS → 픽킹대 지시
                    RunSchFunc(DeleteDataStnCheck3);    // 삭제 작업대(207 매거진) 작업 없는 작업번호 청소 (원본 ArrivedCheck3)
                    RunSchFunc(CopyTrackData3);         // 이음새 352→631 작업 데이터 복사
                    RunSchFunc(DeleteTrackData3);       // 이음새 352 작업 데이터 삭제
                    RunSchFunc(CopyTrackData6);         // 이음새 654→355 작업 데이터 복사
                    RunSchFunc(DeleteTrackData6);       // 이음새 654 작업 데이터 삭제

                    // ── 크레인(SC) : 레거시 CSc::StoreRoutine(RANK_3) / RetrieveRoutine(RANK_4)
                    RunSchFunc(ScCompleteCheck3);       // 크레인 작업 완료 -> 29 (3층 지시만, 2026-11-11)

                    // @.팔렛트 매거진(트랙 301 = 스테이션 207) 공파레트 입고 요청.
                    //   질의가 없는 컬럼(CD.STN_NO)을 짚고 있어 돌 수 없었고 호출도 막아
                    //   두었다. 컬럼을 HOST_STN_NO 로 바로잡고 다시 태운다.
                    //   IF_REQ_MST 에 그 스테이션의 요청 슬롯이 있어야 걸린다.
                    RunSchFunc(CHECK_PM_STO_REQUEST);   // 팔렛트 매거진 입고 요청

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
            StopCmdWorker();
            StopArvWorker();
            MakeMsg("[SCH] 중지 지시로 스레드를 종료했습니다.");
            IsOpen = false;
            m_Thread = null;
}
        #endregion

        #region 기존 함수들
        // 새작업 있는지 체크해서 Invoke 한다.
        public bool NEW_JOB_INVOKE_FOR_CV(string strWH_TYP,
                                          string strPLC_NO,
                                      ref string pRTN_MSG)
        {
            try
            {
                string strLUGG_NO = "";
                string strTRACK_NO = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";

                int nSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[NEW_JOB_ORDER]";

                strSql = "";
                strSql += cDefApp.CRLF + " SELECT *                                 ";
                strSql += cDefApp.CRLF + "   FROM JOB_MST                           ";
                strSql += cDefApp.CRLF + "  WHERE WH_TYP	    = :WH_TYP           ";
                strSql += cDefApp.CRLF + "    AND JOB_TYP IN (" + JT_IN_STO + "," + JT_IN_MOVE + ")  ";   // @.입고 / 이동 - 반자동(11 / 10) 포함
                strSql += cDefApp.CRLF + "    AND JOB_STATUS 	= '99'              ";
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
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " UPDATE JOB_MST                                ";
                    strSql += CRLF + "    SET JOB_STATUS    = '10'                   ";
                    strSql += CRLF + "      , UPD_DT        = " + DbLang.SYSDATE + " ";
                    strSql += CRLF + "      , UPD_USER_ID   = 'IO_TASK_3F'           ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP                ";
                    strSql += CRLF + "    AND LUGG_NO       = :LUGG_NO               ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        m_strRtnMsg = pRTN_MSG;
                        throw new Exception(m_strRtnMsg);
                    }
                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "변경할 작업 정보가 존재하지 않습니다. [LUGG NO : " + strLUGG_NO + "]";
                        _pBdb.Rollback();
                        m_strRtnMsg = pRTN_MSG;
                        throw new Exception(m_strRtnMsg);
                    }
                }

                pRTN_MSG = "[신규 작업]을 [CV 구동대기]로 변경하였습니다. [작업번호:" + strLUGG_NO + "]";

                _pBdb.Commit();

                InsertLog(SCH_WH_TYP, strRTN_MSG, "", "", strLUGG_NO, "10");
                return true;

            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                _pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // PM 입고 요청 하기 
        //   대상 : 트랙번호 301(ST 207)
        // 
        // ─────────────────────────────────────────────────────────────────
        public bool DeleteDataStnCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_DELETE_DATA_STN(strWH_TYP, CV_PLC_3F_OLD, "[DeleteDataStnCheck3]", ref pRTN_MSG); }

        public bool CHECK_PM_STO_REQUEST(string strWH_TYP,
                                         string strPLC_NO,
                                     ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string strMC_DECIDE = "301";    // 이거 밖에 없음!

                string strFunction = pRTN_MSG = "[CHECK_PM_STO_REQUEST]";

                strSql = "";
                // @.CV_DATA 에는 STN_NO 가 없다. 상위 스테이션 번호는 HOST_STN_NO 다.
                //   (트랙 301 = 스테이션 207) 예전 질의는 없는 컬럼을 짚어 아예 돌지 않았다.
                strSql += CRLF + " SELECT  CD.MC_NO, CD.PLC_NO, CD.HOST_STN_NO            ";
                strSql += CRLF + "   FROM  CV_DATA CD                                     ";
                strSql += CRLF + "  INNER  JOIN IF_REQ_MST IRM                            ";
                strSql += CRLF + "     ON  IRM.WH_TYP            = CD.WH_TYP              ";
                strSql += CRLF + "    AND  IRM.STN_NO            = CD.HOST_STN_NO         ";
                strSql += CRLF + "    AND  IRM.MSG_TYP           = 'N'                    ";
                strSql += CRLF + "    AND  IRM.IF_STATUS        <> 'N'                    ";
                strSql += CRLF + "  WHERE  CD.WH_TYP             = '10'                   ";   // Pallet 창고
                strSql += CRLF + "    AND  CD.MC_NO              = :MC_NO                 ";   // Pallet Magazine 입고대
                strSql += CRLF + "    AND  CD.SENSOR0_DATA_RD    = '1'                    ";   // 재하
                strSql += CRLF + "    AND  CD.STO_READY_RD       = '1'                    ";   // 출발 준비
                strSql += CRLF + "    AND  CD.AUTO_MODE_RD       = '1'                    ";
                strSql += CRLF + "    AND  CD.OD_RQ_YN           = 'N'                    ";
                strSql += CRLF + "    AND  CD.OD_RQ_FLAG         = 'N'                    ";
                strSql += CRLF + "    AND  COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                    ";
                strSql += CRLF + "    AND  COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
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

                string strDestMc = "";
                string strDestNm = "";

                // @.조회한 칸만 읽는다. 예전에는 JOB_TYP / PRODUCT_SIZE 처럼 SELECT 에 없는
                //   칸을 읽어, 질의가 돌더라도 여기서 예외가 났다. 요청에 필요한 것은
                //   스테이션 번호 하나뿐이다.
                string strSTN_NO = dtDecide.Rows[0]["HOST_STN_NO"].ToString().Trim();
                if (strSTN_NO == "")
                {
                    pRTN_MSG = strFunction + "TRACK " + strMC_DECIDE + " 에 상위 스테이션 번호(HOST_STN_NO)가 없습니다.";
                    return false;
                }

                _pBdb.BeginTrans();
                //// 상위로 입고 요청을 하기 위한 테이블에 요청 
                if (UPDATE_IF_REQ_MST(strWH_TYP,
                                        "N",
                                        "1",
                                        strSTN_NO,
                                    ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strFunction + "TRACK " + strMC_DECIDE + "번(Pallet Magazine)에서 HOST로 입고 요청 하였습니다.[STN_NO:" + strSTN_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", "0", ST_CV_RUN, strMC_DECIDE);

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }




        #endregion

        // ─────────────────────────────────────────────────────────────────
        // 3층(3F) 함수들 - 레거시 ECS(EcsSv Cv.cpp) 의 *3/*6 함수 포팅 (2026-07-12)
        //   3층 CV PLC : 03(구라인, 트랙 3xx) / 06(신라인, 트랙 6xx)
        //   ECS 활성 함수 : RetInvokeCheck3/6(출고HS→픽킹), MovingTrackCheckPlc3/6(레인 진입 제한)
        //   ECS 비활성 클론 : StartInvokeCheck3/6, ArrivedCheck3/6, Copy/DeleteTrackData3/6
        //     (ECS 에서는 공통 함수가 전 PLC 를 처리 - 여기서는 PLC 한정 버전으로 동작 일치 구현)
        //   레거시 트랙(3xxx/6xxx) → To-Be MC(3xx/6xx) 변환은 SC_HS_DEF 등록값으로 검증됨.
        // ─────────────────────────────────────────────────────────────────
        #region 3층(3F) 함수들
        private const string CV_PLC_3F_OLD = "03";   // 3F 구라인 CV PLC
        private const string CV_PLC_3F_NEW = "06";   // 3F 신라인 CV PLC

        // 구↔신 라인 이음새 트랙 (레거시 CopyTrackData3/6 : 3052→6031, 6054→3055)
        private const string SEAM_FROM_3 = "352";
        private const string SEAM_TO_3   = "631";
        private const string SEAM_FROM_6 = "654";
        private const string SEAM_TO_6   = "355";



        // ── ECS *3/*6 대응 공개 함수 (기존 함수들과 동일 시그니처) ──
        /*
         * 신규 작업 접수(99 → 10 / 20)는 층이 아니라 작업 단위의 일이라
         * 본체를 공통(IOSchDB.JOB_ACCEPT)으로 올렸다. 세 스레드가 다 이 래퍼를
         * 갖고, 한 주기에는 잠금을 먼저 잡은 하나만 실제로 접수한다.
         * (전에는 1F 에만 있어 USE_1F=N 이면 3층·BOX 가 통째로 멈췄다)
         */
        public bool JobAccept(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return JOB_ACCEPT(strWH_TYP, "[JobAccept]", ref pRTN_MSG); }

        public bool StartInvokeCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_STO_START_PLC(strWH_TYP, CV_PLC_3F_OLD, "[StartInvokeCheck3]", ref pRTN_MSG); }
        public bool StartInvokeCheck6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_STO_START_PLC(strWH_TYP, CV_PLC_3F_NEW, "[StartInvokeCheck6]", ref pRTN_MSG); }

        public bool RetInvokeCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_RETHS_PLC(strWH_TYP, CV_PLC_3F_OLD, EN_RET_FLOOR.F3, "[RetInvokeCheck3]", ref pRTN_MSG); }
        public bool RetInvokeCheck6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_RETHS_PLC(strWH_TYP, CV_PLC_3F_NEW, EN_RET_FLOOR.F3, "[RetInvokeCheck6]", ref pRTN_MSG); }

        public bool ArrivedCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_ARRIVE_PLC(strWH_TYP, CV_PLC_3F_OLD, "[ArrivedCheck3]", ref pRTN_MSG); }
        public bool ArrivedCheck6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_ARRIVE_PLC(strWH_TYP, CV_PLC_3F_NEW, "[ArrivedCheck6]", ref pRTN_MSG); }

        public bool MovingTrackCheckPlc3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return MOVING_TRACK_CHECK_PLC(strWH_TYP, "3", "[MovingTrackCheckPlc3]", ref pRTN_MSG); }
        public bool MovingTrackCheckPlc6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return MOVING_TRACK_CHECK_PLC(strWH_TYP, "6", "[MovingTrackCheckPlc6]", ref pRTN_MSG); }

        public bool CopyTrackData3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return COPY_TRACK_DATA(strWH_TYP, SEAM_FROM_3, SEAM_TO_3, "[CopyTrackData3]", ref pRTN_MSG); }
        public bool CopyTrackData6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return COPY_TRACK_DATA(strWH_TYP, SEAM_FROM_6, SEAM_TO_6, "[CopyTrackData6]", ref pRTN_MSG); }

        public bool DeleteTrackData3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return DELETE_TRACK_DATA(strWH_TYP, SEAM_FROM_3, SEAM_TO_3, "[DeleteTrackData3]", ref pRTN_MSG); }
        public bool DeleteTrackData6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return DELETE_TRACK_DATA(strWH_TYP, SEAM_FROM_6, SEAM_TO_6, "[DeleteTrackData6]", ref pRTN_MSG); }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 1 : 3층 입고대 출발 (ECS StartInvokeCheck3/6 - NEW_JOB_ORDER 의 PLC 한정판)
        //   해당 PLC 입고대(START_POS)에 재하 + 구동대기('99') 작업 → CV 지시 + 상태 '15'
        // ─────────────────────────────────────────────────────────────────


        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 2 : 3층 출고HS → 픽킹 스테이션 지시 (ECS RetInvokeCheck3/6)
        //   S/C 반출 완료('29') + 출고 HS(SC_HS_DEF HS_NO='04') 재하 → CV 지시 + 상태 '15'
        //   ECS 와 동일하게 목적지 재매핑 없음 (JOB_MST.DEST_POS 그대로 지시 - 1층과 다름)
        // ─────────────────────────────────────────────────────────────────






        #endregion
        // ─────────────────────────────────────────────────────────────────
        // DB Helper
        // ─────────────────────────────────────────────────────────────────
        #region DB Helper






        // ─────────────────────────────────────────────────────────────────
        // 3층(3F) 크레인 지시 - 레거시 ECS CSc::StoreRoutine(RANK_3) / RetrieveRoutine(RANK_4)
        //
        //   크레인 한 대가 1층과 3층을 같이 본다. 층은 이름이 아니라 H/S 자리 번호(RANK)로
        //   구분한다. 레거시 CSc::GetScSelfStoHS / GetScSelfRetHS (Sc.cpp) 는 랭크를 그대로
        //   돌려주고, 그 값이 Melsec D176(입고 출발 H/S) / D180(출고 도착 H/S) 에 실린다.
        //
        //     RANK_1 = 1층 입고 H/S     RANK_2 = 1층 출고 H/S
        //     RANK_3 = 3층 입고 H/S     RANK_4 = 3층 출고 H/S
        //
        //   여기는 3층이므로 RANK_3 / RANK_4 다. To-Be 는 PLC 를 직접 쓰지 않고 SC_DATA 의
        //   _OD 컬럼에 실어 SC_TASK 가 대신 쓰므로, START_HSPOS_FK1_OD 에 3,
        //   DEST_HSPOS_FK1_OD 에 4 를 넣는다. (1층 판은 1 과 2 를 넣는다)
        //
        //   출고 작업이 어느 층으로 나갈지는 도착 작업대 하나로 정해진다.
        //   레거시 CLib::GetRank (Lib.cpp:2343) 를 그대로 옮긴 것이 STN_3F_PLT 다.
        //   호기간 이동(레거시 JOB_PATTERN_AISLE)만 예외로 목적지를 보지 않고 3층으로 뺀다.
        //   (CJobItem::GetRank / CJob::FetchScRetJobByScNumberNPriority 의
        //    2011.02.06 "RTV->C/V 교체건 - S/C 호기 이동시 3층으로 출고" 수정을 따른다)
        // ─────────────────────────────────────────────────────────────────
        #region 3층(3F) 크레인 지시

        // @.3층 출고 H/S 번호, 자리 번호(HSPOS_*_3F), 출고 작업대(STN_3F_PLT), 202 쏠림 제한 작업대는
        //   1층과 함께 쓰는 출고 지시(ScRetrieveRoutine)가 IOSchDB 로 올라가면서 IOSchDB 에 있다.

        // ── ECS *3/*6 대응 크레인 공개 함수 ──
        public bool StoHsCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScStoreRoutine(strWH_TYP, CV_PLC_3F_OLD, HSPOS_STO_3F, HSPOS_RET_3F, "[StoHsCheck3]", ref pRTN_MSG); }
        public bool StoHsCheck6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScStoreRoutine(strWH_TYP, CV_PLC_3F_NEW, HSPOS_STO_3F, HSPOS_RET_3F, "[StoHsCheck6]", ref pRTN_MSG); }
        public bool RetCmdCheck(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScRetrieveRoutine(strWH_TYP, HS_NO_RET_3F, EN_RET_FLOOR.F3, "[RetCmdCheck]", ref pRTN_MSG); }

        #endregion
        #endregion
    }
}
