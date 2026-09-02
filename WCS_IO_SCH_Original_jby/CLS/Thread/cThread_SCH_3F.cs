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
                //  연결이 끊긴 것을 못 보면 "전송 연결에 데이터를 쓸 수 없습니다" 만 계속 나온다.
                //  null 만 보지 말고 실제 연결 상태를 본다.
                if (!IsDbAlive())
                {
                    if (!DBReopen()) Thread.Sleep(3000);
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

                    // ── 3층(3F) 처리 : ECS *3/*6 함수 포팅 (구라인 PLC 03 / 신라인 PLC 06)
                    RunSchFunc(StartInvokeCheck3);      // 3F 구라인 입고대 출발
                    RunSchFunc(StartInvokeCheck6);      // 3F 신라인 입고대 출발
                    RunSchFunc(RetInvokeCheck3);        // 3F 구라인 출고HS → 픽킹대 지시
                    RunSchFunc(RetInvokeCheck6);        // 3F 신라인 출고HS → 픽킹대 지시
                    RunSchFunc(ArrivedCheck3);          // 3F 구라인 픽킹대 도착보고
                    RunSchFunc(ArrivedCheck6);          // 3F 신라인 픽킹대 도착보고
                    RunSchFunc(CopyTrackData3);         // 이음새 352→631 작업 데이터 복사
                    RunSchFunc(DeleteTrackData3);       // 이음새 352 작업 데이터 삭제
                    RunSchFunc(CopyTrackData6);         // 이음새 654→355 작업 데이터 복사
                    RunSchFunc(DeleteTrackData6);       // 이음새 654 작업 데이터 삭제
                    RunSchFunc(MovingTrackCheckPlc3);   // 3F 구라인 픽킹 레인 진입 제한
                    RunSchFunc(MovingTrackCheckPlc6);   // 3F 신라인 픽킹 레인 진입 제한

                    // ── 크레인(SC) : 레거시 CSc::StoreRoutine(RANK_3) / RetrieveRoutine(RANK_4)
                    RunSchFunc(StoHsCheck3);            // 3F 구라인 입고 H/S -> 크레인 입고 지시
                    RunSchFunc(StoHsCheck6);            // 3F 신라인 입고 H/S -> 크레인 입고 지시
                    RunSchFunc(RetCmdCheck);            // 3F 출고 지시 20 -> 21
                    RunSchFunc(ScCompleteCheck);        // 크레인 작업 완료 -> 29

                    ////// 중복인듯 하지만 NEW_JOB_INVOKE_FOR_CV 이놈은 예외로 처리해줘야할지도...
                    //RunSchFunc(NEW_JOB_INVOKE_FOR_CV);  // CV 구동대기 작업이 있는지 확인 후 신규 작업 지시
                    //RunSchFunc(NEW_JOB_ORDER);          // 입고대에서 출발 하기
                    //RunSchFunc(ARRIVE_CV);              // 도착완료
                    //RunSchFunc(CHECK_CV_RETHS);         // 출고HS 목적지 쓰기
                    //RunSchFunc(CHECK_PM_STO_REQUEST);   // 팔렛트 매거진 입고 요청

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
                strSql += cDefApp.CRLF + "    AND (JOB_TYP = '1' OR JOB_TYP = '6')  ";  // 반자동 작업이 없으면
//                strSql += cDefApp.CRLF + "    AND JOB_TYP  IN ('1','6','10','11')   ";  // 반자동 작업이 있으면
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
                strSql += CRLF + " SELECT  CD.MC_NO, CD.PLC_NO, CD.STN_N                  ";
                strSql += CRLF + "   FROM  CV_DATA CD                                     ";
                strSql += CRLF + "  INNER  JOIN IF_REQ_MST IRM                            ";
                strSql += CRLF + "     ON  IRM.WH_TYP            = CD.WH_TYP              ";
                strSql += CRLF + "    AND  IRM.STN_NO            = CD.STN_NO              ";
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
                strSql += CRLF + "    AND  CD.ERROR_CODE        IN ('0','00','000','0000')";
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

                string strJOB_TYP = dtDecide.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : dtDecide.Rows[0]["JOB_TYP"].ToString();
                string strTRAY_TYP = "" + dtDecide.Rows[0]["PRODUCT_SIZE"].ToString() == "" ? "0" : dtDecide.Rows[0]["PRODUCT_SIZE"].ToString();
                string strTRAY_LEV = "" + dtDecide.Rows[0]["TRAY_LEV"].ToString() == "" ? "0" : dtDecide.Rows[0]["TRAY_LEV"].ToString();
                string strIS_TURN = "" + dtDecide.Rows[0]["TURN"].ToString() == "" ? "0" : dtDecide.Rows[0]["TURN"].ToString();
                string strLUGG_NO = "" + dtDecide.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : dtDecide.Rows[0]["LUGG_NO"].ToString();
                string strCV_PLC = "" + dtDecide.Rows[0]["PLC_NO"].ToString() == "" ? "0" : dtDecide.Rows[0]["PLC_NO"].ToString();
                string strSTN_NO = "" + dtDecide.Rows[0]["STN_NO"].ToString() == "" ? "0" : dtDecide.Rows[0]["STN_NO"].ToString();

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
        public bool StartInvokeCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_STO_START_PLC(strWH_TYP, CV_PLC_3F_OLD, "[StartInvokeCheck3]", ref pRTN_MSG); }
        public bool StartInvokeCheck6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_STO_START_PLC(strWH_TYP, CV_PLC_3F_NEW, "[StartInvokeCheck6]", ref pRTN_MSG); }

        public bool RetInvokeCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_RETHS_PLC(strWH_TYP, CV_PLC_3F_OLD, "[RetInvokeCheck3]", ref pRTN_MSG); }
        public bool RetInvokeCheck6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_RETHS_PLC(strWH_TYP, CV_PLC_3F_NEW, "[RetInvokeCheck6]", ref pRTN_MSG); }

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
                strSql += CRLF + "    AND  SHD.HS_NO             = '04'                ";   // 3층 출고 HS (레거시 RANK_4)
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
                    pRTN_MSG = strTitle + "TRACK " + strHS_MC + "번[3층 출고 H/S]에서 픽킹대(" + strDEST_POS + ")로 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
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

        // SC_HS_DEF.HS_NO : 3층 출고 H/S (1층은 '02', 3층은 '04')
        private const string HS_NO_RET_3F = "04";

        // SC_DATA 에 실을 H/S 자리 번호 = 레거시 RANK 값 그대로
        private const string HSPOS_STO_3F = "3";   // RANK_3 (D176 입고 출발 H/S)
        private const string HSPOS_RET_3F = "4";   // RANK_4 (D180 출고 도착 H/S)

        // 3층 PLT 출고 작업대 - 레거시 CLib::GetRank 가 RANK_4 를 돌려주는 자리
        //   200~209 (구라인) + 212~215 (신라인)
        //   211/221/222/231/241/242/251 은 BOX 라인이라 여기서 잡지 않는다. (BOX 스레드 몫)
        private static readonly string[] STN_3F_PLT =
            { "200", "201", "202", "203", "204", "205", "206", "207",
              "208", "209", "212", "213", "214", "215" };

        // 202 피킹대 쏠림 제한에 쓰는 작업대
        private const string STN_202 = "202";
        private const string STN_201 = "201";
        private const string STN_203 = "203";

        // ── ECS *3/*6 대응 크레인 공개 함수 ──
        public bool StoHsCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScStoreRoutine(strWH_TYP, CV_PLC_3F_OLD, "[StoHsCheck3]", ref pRTN_MSG); }
        public bool StoHsCheck6(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScStoreRoutine(strWH_TYP, CV_PLC_3F_NEW, "[StoHsCheck6]", ref pRTN_MSG); }
        public bool RetCmdCheck(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScRetrieveRoutine(strWH_TYP, HS_NO_RET_3F, "[RetCmdCheck]", ref pRTN_MSG); }

        // ─────────────────────────────────────────────────────────────────
        // 3층 입고 H/S 도착 -> 크레인 입고 지시 (레거시 CSc::StoreRoutine(RANK_3) + Store)
        //   화물이 3층 입고 H/S 에 올라오면 그 크레인에 입고를 지시한다.
        //     지시 대상 크레인 : JOB_MST.DEST_POS (901~911)
        //     넣을 랙 위치     : JOB_MST.DEST_LOCATION (상위가 준 값)
        //   입고 H/S 인지는 CV_DATA.STOHS_READY_RD 로 안다. 그 값은 CV 태스크가
        //   레거시 ParsingExtraFrame 워드 +4 비트에 해당하는 자리에서 읽어 채운다.
        //   1층 판과 다른 것은 CV PLC(03/06)와 START_HSPOS(=3) 둘뿐이다.
        // ─────────────────────────────────────────────────────────────────
        private bool ScStoreRoutine(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
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
                strSql += SQL_HS_NOT_PAUSED;            // 입고 H/S 가 멈춰 있지 않은가
                strSql += SQL_SC_READY;                 // 크레인이 지시를 받을 수 있는 상태인가
                strSql += SQL_SC_STO_NOT_SUSPEND;       // 입고 정지가 아닌가
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

                if (UpdateScData(strSC_NO, JT_STO, strLUGG_NO, "", strDEST_LOC,
                                 HSPOS_STO_3F, HSPOS_RET_3F, ref pRTN_MSG) == false)
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

                pRTN_MSG = strTitle + "TRACK " + strMC_NO + "번[3층 입고 H/S]에서 SC_TASK를 통해서 "
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
        // 3층 출고 지시 (20 -> 21) - 레거시 CSc::RetrieveRoutine(RANK_4) + Retrieve
        //   상위가 넣은 출고 작업 중 3층으로 나갈 것만 골라 크레인에 넘긴다.
        //     꺼낼 곳     : JM.START_LOCATION (랙 위치)
        //     내려놓을 곳 : 그 호기의 3층 출고 H/S (SC_HS_DEF.HS_NO='04')
        //     크레인      : JM.START_POS (출고 작업은 출발지가 호기다)
        //   출고 H/S 가 비어 있어야 크레인이 내려놓을 수 있다.
        //   이어받는 곳은 CV_RETHS_PLC 다. 그쪽이 JM.HS_TRACK_NO 로 트랙을 찾으므로
        //   여기서 그 값을 채워 준다.
        // ─────────────────────────────────────────────────────────────────
        private bool ScRetrieveRoutine(string strWH_TYP, string strHS_NO, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                // ── 1) 3층으로 나갈 출고 작업만 고른다 (호기는 아직 모른다)
                //      레거시 CLib::GetRank 가 RANK_4 를 주는 작업대 + 호기간 이동
                strSql = "";
                strSql += CRLF + " SELECT LUGG_NO, JOB_TYP, START_POS, START_LOCATION, DEST_POS ";
                strSql += CRLF + "   FROM JOB_MST                                            ";
                strSql += CRLF + "  WHERE WH_TYP           = :WH_TYP                         ";
                strSql += CRLF + "    AND JOB_STATUS       = '" + ST_SC_WAIT + "'            ";   // 20 = SC 구동요구
                strSql += CRLF + "    AND ( DEST_POS IN (" + SqlInList(STN_3F_PLT) + ")      ";
                strSql += CRLF + "       OR JOB_TYP  = '" + JT_A2A + "' )                    ";   // 호기간 이동은 3층으로
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

                // ── 2) 202 피킹대 쏠림 제한에 쓸 개수를 미리 센다
                //      (레거시 CJob::FetchScRetJobByScNumberNPriority 의 앞부분)
                int nCnt202 = 0, nCntEtc = 0;
                if (Count202Picking(strWH_TYP, ref nCnt202, ref nCntEtc, ref pRTN_MSG) == false)
                    return false;
                int nLimit202 = cDefApi.GsGetLimitStn202Picking();

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

                    // 202 로 갈 작업인데 201/203 에도 대기가 있고 진행 중인 202 가 한도 이상이면 건너뛴다
                    if ((strDEST_POS == STN_202) && (nCntEtc > 0) && (nCnt202 >= nLimit202))
                        continue;

                    // ── 3) 호기를 정한다. 상위가 호기를 줬으면 그대로, 아니면 랙 뱅크에서 구한다.
                    strSC_NO = dtJob.Rows[i]["START_POS"].ToString();
                    if (IsScNo(strSC_NO) == false)
                    {
                        if (GetScNoByLocation(strSTART_LOC, ref strSC_NO) == false)
                            continue;
                    }

                    // ── 4) 그 호기와 3층 출고 H/S 가 지시를 받을 수 있는 상태인지 본다
                    strSql = "";
                    strSql += CRLF + " SELECT SHD.HS_MC_NO                                      ";
                    strSql += CRLF + "   FROM SC_DATA SD                                        ";
                    strSql += CRLF + "  INNER JOIN SC_HS_DEF SHD                                ";
                    strSql += CRLF + "     ON SHD.WH_TYP          = SD.WH_TYP                   ";
                    strSql += CRLF + "    AND SHD.SC_NO           = SD.SC_NO                    ";
                    strSql += CRLF + "    AND SHD.HS_NO           = :HS_NO                      ";   // 3층 출고 H/S
                    strSql += CRLF + "    AND SHD.HS_USE_YN       = 'Y'                         ";
                    strSql += CRLF + "  INNER JOIN CV_DATA CD                                   ";
                    strSql += CRLF + "     ON CD.WH_TYP           = SD.WH_TYP                   ";
                    strSql += CRLF + "    AND CD.MC_NO            = SHD.HS_MC_NO                ";
                    strSql += CRLF + "  WHERE SD.WH_TYP           = :WH_TYP                     ";
                    strSql += CRLF + "    AND SD.SC_NO            = :SC_NO                      ";
                    strSql += SQL_SC_READY;                 // 크레인이 지시를 받을 수 있는 상태인가
                    strSql += SQL_SC_RET_NOT_SUSPEND;       // 출고 정지가 아닌가
                    strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                         ";   // 출고 H/S 가 비어 있다
                    strSql += CRLF + "    AND CD.LUGG_NO_RD       IN ('','0','0000')            ";
                    strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                         ";
                    strSql += CRLF + "    AND CD.ERROR_CODE       IN ('0','0000')               ";
                    strSql += SQL_HS_NOT_PAUSED;            // 출고 H/S 가 멈춰 있지 않은가

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
                        continue;   // 아직 받을 수 없다

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

                if (UpdateScData(strSC_NO, JT_RET, strLUGG_NO, strSTART_LOC, "",
                                 HSPOS_STO_3F, HSPOS_RET_3F, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 크레인 미준비 - 다음 사이클 재시도
                }

                // 이어받을 CV_RETHS_PLC 가 HS_TRACK_NO 로 3층 출고 H/S 트랙을 찾는다
                if (UPDATE_JOB_DATA(ST_SC_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG,
                                    "0", "0", strHS_MC_NO) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + strSC_NO + "호기에 3층 출고를 지시하였습니다. [작업번호:" + strLUGG_NO
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

        /*
         * Count202Picking :: 202 피킹대 쏠림 제한에 쓰는 두 개수를 센다.
         *
         *   레거시 CJob::FetchScRetJobByScNumberNPriority (Job.cpp:1002) 앞부분 그대로다.
         *     nCount202 : 도착지가 202 인 피킹 작업 중 "이미 진행 중인" 것
         *                 (레거시는 상태가 JOB_STA_SC_OPER_REQUEST 가 아닌 것 = 여기서는 20 이 아닌 것)
         *                 + 202 트랙에 작업목록에 없는 피킹 화물이 얹혀 있으면 하나 더
         *     nCountEtc : 도착지가 201 또는 203 인 피킹 작업 수
         *   둘 다 피킹(JOB_TYP='3') 작업만 센다.
         */
        private bool Count202Picking(string strWH_TYP, ref int nCnt202, ref int nCntEtc, ref string pRTN_MSG)
        {
            nCnt202 = 0;
            nCntEtc = 0;

            string strSql = "";
            strSql += CRLF + " SELECT                                                        ";
            strSql += CRLF + "   ( SELECT COUNT(*) FROM JOB_MST J                            ";
            strSql += CRLF + "      WHERE J.WH_TYP     = :WH_TYP                             ";
            strSql += CRLF + "        AND J.JOB_TYP    = '" + JT_PICK + "'                   ";
            strSql += CRLF + "        AND J.DEST_POS   = '" + STN_202 + "'                   ";
            strSql += CRLF + "        AND J.JOB_STATUS <> '" + ST_SC_WAIT + "' )             ";
            strSql += CRLF + " + ( SELECT COUNT(*) FROM CV_DATA CD                           ";
            strSql += CRLF + "      WHERE CD.WH_TYP    = :WH_TYP                             ";
            strSql += CRLF + "        AND CD.MC_NO     = '" + STN_202 + "'                   ";
            strSql += CRLF + "        AND CD.JOB_TYP_RD = '" + JT_PICK + "'                  ";
            strSql += CRLF + "        AND COALESCE(CD.LUGG_NO_RD,'0') NOT IN ('','0','0000') ";
            strSql += CRLF + "        AND NOT EXISTS ( SELECT 1 FROM JOB_MST J2              ";
            strSql += CRLF + "                          WHERE J2.WH_TYP  = CD.WH_TYP         ";
            strSql += CRLF + "                            AND J2.LUGG_NO = CD.LUGG_NO_RD ) ) AS CNT202 ";
            strSql += CRLF + " , ( SELECT COUNT(*) FROM JOB_MST J                            ";
            strSql += CRLF + "      WHERE J.WH_TYP     = :WH_TYP                             ";
            strSql += CRLF + "        AND J.JOB_TYP    = '" + JT_PICK + "'                   ";
            strSql += CRLF + "        AND J.DEST_POS   IN ('" + STN_201 + "','" + STN_203 + "') ) AS CNTETC ";

            _pBdb.mComMain.CommandType = CommandType.Text;
            _pBdb.mComMain.Parameters.Clear();
            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

            int nSelCnt = _pBdb.ExcuteQry(strSql);
            if (nSelCnt < 0)
            {
                pRTN_MSG += _pBdb.ErrMsg;
                return false;
            }
            if (nSelCnt == 0) return true;

            int.TryParse(GetVal(_pBdb.mDtMain.Rows[0], "cnt202"), out nCnt202);
            int.TryParse(GetVal(_pBdb.mDtMain.Rows[0], "cntetc"), out nCntEtc);
            return true;
        }







        /// <summary>
        /// SC_DATA 이송 명령 지시 (레거시 ECS CSc::Store / CSc::Retrieve 의 명령 데이터 포팅)
        ///   레거시 Melsec 명령 D171~D192 : 명령(1=입고/2=출고), 화물번호, BANK/BAY/LEVEL, HS(=랭크)
        ///   - 입고(Store)    : 출발 = 입고 H/S(strStoHsPos), 도착 = 랙 위치(DEST_LOCATION 분해)
        ///   - 출고(Retrieve) : 출발 = 랙 위치(START_LOCATION 분해), 도착 = 출고 H/S(strRetHsPos)
        ///   위치 문자열 = BANK(2) + BAY(3) + LEVEL(2)  (레거시 LOCATION_LEN=7)
        ///   H/S 자리 번호는 곧 랭크다. 1층이면 1/2, 3층이면 3/4 를 넣는다.
        ///   (레거시 CSc::GetScSelfStoHS / GetScSelfRetHS 가 랭크를 그대로 돌려준다)
        ///   FK1 단일포크 사용, WRITE_FLAG_OD='1' = 명령 기록 완료 플래그.
        /// </summary>
        private bool UpdateScData(
                    string strScNo
            ,       string strJobTyp
            ,       string strLuggNo
            ,       string strStartLoc
            ,       string strDestLoc
            ,       string strStoHsPos
            ,       string strRetHsPos
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
                //   사용 포크 : 0 = 포크1 / 1 = 포크1,2(양쪽) / 2 = 포크2
                //   이 현장 PLT 크레인은 전부 SINGLE 이므로 0 이다.
                //   1 을 주면 크레인이 양포크 작업으로 돌아 한 건을 끝내도
                //   적재 표시가 다 안 내려가고, 다음 작업이 집기부터 시작하지 못한다.
                strSql += CRLF + "      , USE_FK_OD           = '0'                      ";   // 포크1 사용 (SINGLE 포크)
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
                _pBdb.mComMain.Parameters.Add("START_HSPOS",    DbLang.VARCHAR).Value = bStore ? strStoHsPos : "0";
                _pBdb.mComMain.Parameters.Add("DEST_BANK",      DbLang.VARCHAR).Value = bStore ? strBank : "0";
                _pBdb.mComMain.Parameters.Add("DEST_BAY",       DbLang.VARCHAR).Value = bStore ? strBay  : "0";
                _pBdb.mComMain.Parameters.Add("DEST_LEVEL",     DbLang.VARCHAR).Value = bStore ? strLev  : "0";
                _pBdb.mComMain.Parameters.Add("DEST_HSPOS",     DbLang.VARCHAR).Value = bStore ? "0" : strRetHsPos;
                _pBdb.mComMain.Parameters.Add("WH_TYP",         DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO",          DbLang.VARCHAR).Value = strScNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "SC_DATA 명령 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "지시할 SC_DATA 가 없음(SC_NO:" + strScNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }
        #endregion
        #endregion
    }
}
