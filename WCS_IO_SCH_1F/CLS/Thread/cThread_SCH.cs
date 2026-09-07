// 작성자   : WCS Scheduler
// 층별판   : WCS_IO_SCH_Original(통합판) 의 같은 층 클래스를 클래스 이름만 바꿔 쓴다.
//            두 벌이 어긋나지 않도록 손볼 때는 통합판을 고치고 여기로 옮긴다.
//            원본 :  1층(1F) 담당 스케줄러 스레드
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
    public class cThread_SCH : IOSchDB
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
        public cThread_SCH(int Id)
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

                    // 출고 루프 출발 - 중간 도착대(151~160)와 출고위치 결정대(171)를
                    // 같은 조건으로 한 함수에서 본다. PLC 별로 자기 트랙만 본다.
                    RunSchFunc(NewStartRoutinePlc2);    // 1F 구라인(PLC 02) 출고 루프 출발
                    RunSchFunc(NewStartRoutinePlc5);    // 1F 신라인(PLC 05) 출고 루프 출발
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

        /*
         * 1층 출고 작업대 - 레거시 CLib::GetRank (Lib.cpp:2343) 가 RANK_2 를 돌려주는 자리.
         *
         *   출고 작업이 1층으로 나갈지 3층으로 나갈지는 도착 작업대 하나로 정해진다.
         *   3층 PLT(200~209, 212~215)와 BOX(211,221,222,231,241,242,251)는
         *   각각 cThread_SCH_3F / cThread_SCH_BOX 가 맡는다. 여기서 집으면 안 된다.
         *   호기간 이동은 목적지를 보지 않고 3층으로 뺀다 (레거시 2011.02.06 수정).
         */
        private static readonly string[] STN_1F_PLT = { "103", "104", "105" };


        // 구↔신 라인 이음새 트랙 (레거시 CopyTrackData2/5 : 2064→5031, 5056→2050)
        private const string SEAM_FROM_2 = "264";
        private const string SEAM_TO_2 = "531";
        private const string SEAM_FROM_5 = "556";
        private const string SEAM_TO_5 = "250";



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
        { return ScStoreRoutine(strWH_TYP, CV_PLC_1F_OLD, "[StoHsCheck2]", ref pRTN_MSG); }

        public bool StoHsCheck5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScStoreRoutine(strWH_TYP, CV_PLC_1F_NEW, "[StoHsCheck5]", ref pRTN_MSG); }

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
        { return ScRetrieveRoutine(strWH_TYP, HS_NO_RETRIEVE, "[RetCmdCheck]", ref pRTN_MSG); }


        public bool NewStartRoutinePlc2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CHECK_CV_RET_START(strWH_TYP, CV_PLC_1F_OLD, "[NewStartRoutinePlc2]", ref pRTN_MSG); }
        public bool NewStartRoutinePlc5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CHECK_CV_RET_START(strWH_TYP, CV_PLC_1F_NEW, "[NewStartRoutinePlc5]", ref pRTN_MSG); }

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
        // 출고 루프 출발  (ECS CCv::StartRoutine / ReStartRoutine2 포팅)
        //
        //   출발 자리는 두 가지다.
        //     중간 도착대 151~160 : 크레인이 출고 H/S 에서 내려놓은 자리
        //                           (SC_HS_DEF.WAIT_TRACK, HS_NO = HS_NO_RETRIEVE)
        //     출고위치 결정대 171 : 대기대에서 한 번 모이는 자리 (DEST_POS_DEF 171 = 232)
        //
        //   자리마다 "어디로 보내느냐" 는 달라도 "출발해도 되느냐" 는 같아야 한다.
        //   예전에는 대기대와 결정대를 다른 함수(CHECK_CV_RET_START /
        //   CHECK_CV_RET_RESTART)가 각자 쓴 질의로 판단했다. 조건 한 줄을 고치면
        //   한쪽만 고쳐지곤 했다. 판단은 SQL_RET_DEPART_COND 한 곳에 모으고,
        //   출발 자리는 데이터(SC_HS_DEF.WAIT_TRACK / DEST_POS_DEF 171)로 모은다.
        //   레거시 CV_STO_START_PLC 가 입고대와 도착대를 한 질의에서 STN_KIND 로
        //   갈라 보던 것과 같은 짜임이다.
        //
        //   목적지 결정 (사이클당 1건 - 유량을 사이클마다 다시 계산하기 때문) :
        //     중간 도착대 151 : 설비 구조상 결정대를 거칠 수 없다. 출고대#1(103 = 206) 직행.
        //     중간 도착대 152~160 : 출고위치 결정대(171 = 232)로 보낸다.
        //     출고위치 결정대 171 : 출고대 레인의 진입 자리를 보고 최종 출고대를 정한다.
        //       TR 205 비었으면 출고대#1(103 = 206),
        //       TR 211 비었으면 출고대#2(104 = 212),
        //       둘 다 차 있으면 그대로 기다린다 (다음 사이클 재시도).
        //
        //   출고대로 확정할 때는 JOB_MST.DEST_POS 도 같이 바꾼다. 도착 매칭이
        //   CD.HOST_STN_NO = JM.DEST_POS 라 작업정보의 도착지를 안 바꾸면 화물이
        //   출고대에 닿아도 도착 보고가 안 붙는다. 결정대는 경유지라 그대로 둔다.
        //
        //   출고대 자체가 아니라 그 앞 진입 자리를 보는 이유는 CHECK_RET_LANE_READY
        //   주석과 같다. 출고대에 화물이 있어도 진입 자리가 비어 있으면 한 대 더
        //   보낼 수 있고, 지게차가 앞의 것을 가져가면 저절로 밀려 들어간다.
        //
        //   유량 제한 (ECS m_nRetCnt + m_nRetCntNew 대응) :
        //     EQP_MST 의 CV 2호기/5호기 CV_RET_CNT 합계(PLC 가 보고하는 1층 출고 루프상
        //     화물 갯수)가 DEL_HIS_SETTING(TABLE_NAME='1f_ret_rimit') 의 CYCLE 값보다
        //     크면 루프에 새로 넣지 않는다. 이것만은 대기대에만 건다. 결정대 화물은
        //     이미 루프 안에 있어서 여기서 막으면 갯수가 영영 안 줄어 갇힌다.
        //     (출발 여부의 조건이 아니라, 루프 진입을 막는 별도의 문이다)
        //
        //   PLC 한정 : NewStartRoutinePlc2/5 가 각자 자기 PLC 것만 본다.
        //     예전에는 두 호출이 PLC 를 안 보고 같은 질의를 돌려 한 사이클에
        //     같은 자리에서 두 번 출발시켰다.
        //
        //   스테이션(103/104/151/171) → 실트랙(MC) 변환은 DEST_POS_DEF(TRACK_NO→MC_NO) 사용.
        // ─────────────────────────────────────────────────────────────────
        private bool m_bRetLimitHold = false;   // 유량 제한 보류 상태 (메시지 1회 출력용)

        /*
         * 출고 루프 출발 조건 - 중간 도착대(151~160)와 결정대(171)가 함께 쓴다.
         *
         *   재하 / 출고 준비 / 자동 / 지시 없음 / 일시정지 아님 / 무에러 / CV 구동중.
         *   여기가 두 자리의 "출발해도 되느냐" 판단 전부다. 자리마다 다른 것은
         *   어느 트랙을 후보로 삼느냐(호출부)와 어디로 보내느냐(행동)뿐이다.
         *
         *   준비 신호로 STO_READY_RD 가 아니라 RET_READY_RD 를 보는 이유 :
         *   여기는 입고대가 아니라 도착대·출고대 계열이다. 도착대는 출고대와 같은
         *   준비 신호를 쓴다. (cDefApp.STN_KIND_ARV 주석 참고)
         *
         *   같은 화물에 두 번 지시하지 않기 (LUGG_NO_OD <> LUGG_NO_RD) :
         *   OD_RQ_YN / OD_RQ_FLAG 만으로는 재지시를 못 막는다. 지시를 넣으면
         *   OD_RQ_YN='Y' 가 되고, CV 타스크가 PLC 에 쓰면서 OD_RQ_YN='N' /
         *   OD_RQ_FLAG='Y' 로 바꾼다. 그런데 그 다음 읽기 주기에 UpdateCvData 가
         *   OD_RQ_FLAG='N' 으로 되돌린다. 화물이 실제로 그 자리를 뜨는 데는 몇 초가
         *   걸리니, 그 사이 자리는 '재하 + 출고 준비 + 지시 없음' 으로 다시 보여
         *   같은 화물에 지시가 한 번 더 나갔다. (208 → 결정대, 232 → 출고대#1 이
         *   각각 두 번 찍히던 증상)
         *   LUGG_NO_OD 는 그 자리에서 마지막으로 지시한 화물이다. 지금 올라와 있는
         *   화물(LUGG_NO_RD)과 같으면 이미 내보낸 것이므로 건드리지 않는다.
         *   화물이 빠지고 다음 화물이 오면 두 값이 달라져 다시 지시된다.
         */
        private string SQL_RET_DEPART_COND(string strCd, string strJm)
        {
            string strSql = "";
            strSql += CRLF + "    AND  " + strCd + ".SENSOR0_DATA_RD    = '1'                               ";   // 재하
            strSql += CRLF + "    AND  " + strCd + ".RET_READY_RD       = '1'                               ";   // 출고 준비
            strSql += CRLF + "    AND  " + strCd + ".AUTO_MODE_RD       = '1'                               ";
            strSql += CRLF + "    AND  " + strCd + ".OD_RQ_YN           = 'N'                               ";
            strSql += CRLF + "    AND  " + strCd + ".OD_RQ_FLAG         = 'N'                               ";
            strSql += CRLF + "    AND  COALESCE(" + strCd + ".TR_PAUSE_RD,'0') IN ('0','')                  ";
            strSql += CRLF + "    AND  COALESCE(NULLIF(BTRIM(" + strCd + ".ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')  ";
            strSql += CRLF + "    AND  " + strJm + ".JOB_STATUS         = '" + ST_CV_RUN + "'               ";   // CV 구동중
            // 이 자리에서 이 화물에 이미 지시했으면 다시 하지 않는다 (위 주석 참고)
            strSql += CRLF + "    AND  COALESCE(NULLIF(BTRIM(" + strCd + ".LUGG_NO_OD, " + SQL_WS + "), ''), '0')  ";
            strSql += CRLF + "      <> COALESCE(NULLIF(BTRIM(" + strCd + ".LUGG_NO_RD, " + SQL_WS + "), ''), '0')  ";
            return strSql;
        }

        public bool CHECK_CV_RET_START(string strWH_TYP,
                                       string strPLC_NO,
                                       string strTitle,
                                   ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = (strTitle == "") ? "[CHECK_CV_RET_START]" : strTitle;

                // ── 1) 스테이션 → 실트랙(MC) 변환 (DEST_POS_DEF)
                //   트랙 번호는 출발 자리와 진입 자리의 상태를 볼 때만 쓴다.
                //   CV 에 넘기는 목적지는 작업대 번호다. (CvSim 의 DestCode 와 같다)
                string strMC_RET1 = "";     // 출고대#1 트랙 (스테이션 103 = 206)
                string strMC_RET2 = "";     // 출고대#2 트랙 (스테이션 104 = 212)
                string strMC_DECIDE = "";   // 출고위치 결정대 트랙 (스테이션 171 = 232)
                string strMC_WAIT1 = "";    // 중간 도착대 #1 트랙 (스테이션 151 = 204)
                if (GET_DEST_POS_MC(strWH_TYP, "103", ref strMC_RET1, ref pRTN_MSG) == false) return false;
                if (GET_DEST_POS_MC(strWH_TYP, "104", ref strMC_RET2, ref pRTN_MSG) == false) return false;
                if (GET_DEST_POS_MC(strWH_TYP, "171", ref strMC_DECIDE, ref pRTN_MSG) == false) return false;
                if (GET_DEST_POS_MC(strWH_TYP, "151", ref strMC_WAIT1, ref pRTN_MSG) == false) return false;

                // ── 2) 1층 출고 유량 확인
                //      현재 PLC 상 화물 갯수(CV 2호기 + 5호기의 CV_RET_CNT 합) > 제한(cycle)
                //      이면 루프에 새로 넣지 않는다. 결정대 화물은 이 문에 걸리지 않는다.
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

                bool bLoopFull = (nRetSum > nRetLimit);
                if (bLoopFull == true)
                {
                    // 제한 초과 - 대기대에서는 새로 내보내지 않는다 (보류 진입 시 1회만 메시지)
                    if (m_bRetLimitHold == false)
                    {
                        m_bRetLimitHold = true;
                        MakeMsg(strFunction + "1층 출고 유량 제한으로 대기대 출발 보류. [현재:" + nRetSum + " / 제한:" + nRetLimit + "]");
                    }
                }
                else if (m_bRetLimitHold == true)
                {
                    m_bRetLimitHold = false;
                    MakeMsg(strFunction + "1층 출고 유량 정상 - 출발 재개. [현재:" + nRetSum + " / 제한:" + nRetLimit + "]");
                }

                // ── 3) 출발 대상 조회 (도착 오래된 순)
                //      후보 자리 = 중간 도착대(SC_HS_DEF.WAIT_TRACK) + 출고위치 결정대(171)
                //      두 자리에 같은 조건(SQL_RET_DEPART_COND)을 건다.
                strSql = "";
                strSql += CRLF + " SELECT  JM.LUGG_NO, JM.JOB_TYP, JM.DEST_POS, JM.TURN                ";
                strSql += CRLF + "       , JM.PRODUCT_SIZE, JM.TRAY_LEV                                ";
                strSql += CRLF + "       , CD.MC_NO, CD.PLC_NO                                         ";
                strSql += CRLF + "       , SHD.SC_NO  AS  HS_SC_NO                                     ";
                strSql += CRLF + "   FROM  CV_DATA CD                                                  ";
                strSql += CRLF + "  INNER  JOIN JOB_MST JM                                             ";
                strSql += CRLF + "     ON  JM.WH_TYP             = CD.WH_TYP                           ";
                strSql += CRLF + "    AND  JM.LUGG_NO            = CD.LUGG_NO_RD                       ";   // 재하 화물의 작업 매치
                strSql += CRLF + "   LEFT  JOIN (SELECT WH_TYP, WAIT_TRACK, MAX(SC_NO) AS SC_NO        ";   // 출고 HS 정의의 대기 트랙
                strSql += CRLF + "                 FROM SC_HS_DEF                                      ";
                strSql += CRLF + "                WHERE HS_NO     = '" + HS_NO_RETRIEVE + "'           ";
                strSql += CRLF + "                  AND HS_USE_YN = 'Y'                                ";
                strSql += CRLF + "                  AND COALESCE(WAIT_TRACK,'') <> ''                  ";
                strSql += CRLF + "                GROUP BY WH_TYP, WAIT_TRACK) SHD                     ";
                strSql += CRLF + "     ON  SHD.WH_TYP            = CD.WH_TYP                           ";
                strSql += CRLF + "    AND  SHD.WAIT_TRACK        = CD.MC_NO                            ";
                strSql += CRLF + "  WHERE  CD.WH_TYP             = :WH_TYP                             ";
                strSql += CRLF + "    AND  CD.PLC_NO             = :CV_PLC                             ";   // 자기 PLC 것만
                strSql += CRLF + "    AND  (SHD.WAIT_TRACK IS NOT NULL OR CD.MC_NO = :MC_DECIDE)       ";   // 대기대 또는 결정대
                strSql += SQL_RET_DEPART_COND("CD", "JM");
                strSql += CRLF + "  ORDER  BY JM.UPD_DT ASC                                            ";   // 가장 빨리 도착한 화물부터
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strPLC_NO;
                _pBdb.mComMain.Parameters.Add("MC_DECIDE", DbLang.VARCHAR).Value = strMC_DECIDE;
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

                // ── 4) 출고위치 결정대(171) 가용 확인  ※ 기본은 보지 않는다
                //
                //      출발 여부는 출발 자리 자신의 조건과 유량으로 끝나야 하고,
                //      한참 떨어진 도착지(결정대)가 비었는지로 정할 일이 아니다.
                //      도착지를 보고 붙잡아 두면 대기대가 안 비고, 그러면 크레인이
                //      출고 H/S 를 못 비워 다음 출고를 시작하지 못한다.
                //      결정대 앞에 줄이 서는 것은 컨베이어가 알아서 할 일이다.
                //
                //      그래도 줄 서는 것을 원치 않는 현장을 위해 메인 폼 체크박스로
                //      켤 수 있게 남겼다.
                bool bDecideReady = true;
                if (cDefApp.GM_RET_DECIDE_WAIT == true)
                {
                    bDecideReady = false;
                    strSql = "";
                    strSql += CRLF + " SELECT COUNT(*) AS READY_CNT                                        ";
                    strSql += CRLF + "   FROM CV_DATA CD                                                   ";
                    strSql += CRLF + "  WHERE CD.WH_TYP           = :WH_TYP                                ";
                    strSql += CRLF + "    AND CD.MC_NO            = :MC_NO                                 ";
                    strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                                    ";   // 빈 상태
                    strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                                    ";
                    strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                ";
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
                }

                // ── 5) 출고대 레인 가용 확인  (출고대가 아니라 그 앞 진입 자리 205 / 211 을 본다)
                //      결정대(171)에서 최종 출고대를 고를 때만 쓴다.
                //      진입 자리가 비어 있고 그 레인으로 가는 중인 화물이 없어야 가용이다.
                //      한 사이클에 1건만 출발시키므로 루프 밖에서 한 번만 조회한다.
                bool bRet1Ready = CHECK_RET_LANE_READY(strWH_TYP, strMC_RET1, "103");
                bool bRet2Ready = CHECK_RET_LANE_READY(strWH_TYP, strMC_RET2, "104");

                // ── 6) 출발 지시
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

                    bool bAtDecide = (strWAIT_MC == strMC_DECIDE);
                    bool bAtWait1 = (strWAIT_MC == strMC_WAIT1);
                    string strFromNm = bAtDecide ? "출고위치 결정대" : "출고 대기대";

                    // 유량 제한은 루프에 새로 넣는 대기대에만 건다 (결정대는 이미 루프 안)
                    if (bLoopFull == true && bAtDecide == false)
                        continue;

                    // ── 목적지 결정 : 어디에 서 있느냐로 갈린다
                    string strDestStn = "";    // CV / JOB_MST 에 쓰는 목적지 (작업대 번호)
                    string strDestNm = "";
                    string strWhy = "";
                    if (bAtDecide == true)
                    {
                        // 결정대 : 출고대 레인의 진입 자리(205 / 211)를 보고 고른다
                        if (bRet1Ready == true) { strDestStn = "103"; strDestNm = "출고대#1"; strWhy = "진입 자리 비었음"; }
                        else if (bRet2Ready == true) { strDestStn = "104"; strDestNm = "출고대#2"; strWhy = "진입 자리 비었음"; }
                        else
                            continue;   // 두 레인 다 진입 자리가 차 있다 - 그대로 대기 (다음 사이클 재시도)
                    }
                    else if (bAtWait1 == true)
                    {
                        // 중간 도착대 #1(151) : 설비 구조상 결정대를 거칠 수 없다 - 출고대#1 직행
                        strDestStn = "103"; strDestNm = "출고대#1"; strWhy = "설비 구조상 직행";
                    }
                    // 그 외 중간 도착대(152~160) 는 아래에서 출고위치 결정대(171)로 보낸다

                    if (strDestStn != "")
                    {
                        // 최종 목적지 확정 → JOB_MST.DEST_POS 도 함께 변경 (ARRIVE_CV 도착 매칭용)
                        //   CV 목적지와 JOB_MST.DEST_POS 둘 다 작업대 번호를 쓴다.
                        //   CV 는 목적지를 작업대 번호(DestCode)로 읽고, 도착 매칭은
                        //   CD.HOST_STN_NO = JM.DEST_POS 로 맞추기 때문이다.
                        strDestMc = (strDestStn == "103") ? strMC_RET1 : strMC_RET2;

                        _pBdb.BeginTrans();
                        if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strDestStn, strIS_TURN,
                                           strLUGG_NO, strWH_TYP, strCV_PLC, strWAIT_MC, "", ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }
                        if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDestStn) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }
                        pRTN_MSG = strFunction + "TRACK " + strWAIT_MC + "번[" + strFromNm + "]에서 " + strDestNm + "(" + strDestMc + ")로 출발 지시하였습니다. [" + strWhy + " / 크레인:" + strHS_SC_NO + " / 작업번호:" + strLUGG_NO + "]";
                        _pBdb.Commit();
                        InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strWAIT_MC, strDestMc);
                        return true;
                    }

                    // 중간 도착대 152~160 → 출고위치 결정대(171) 경유
                    if (bDecideReady == false)
                        continue;           // 결정대 검사를 켠 경우만 - 사용중이면 다음 사이클 재시도

                    strDestMc = strMC_DECIDE;

                    _pBdb.BeginTrans();
                    if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, "171", strIS_TURN,
                                       strLUGG_NO, strWH_TYP, strCV_PLC, strWAIT_MC, "", ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    //   (결정대는 경유지이므로 JOB_MST.DEST_POS 는 변경하지 않음 - 최종 목적지는
                    //    결정대에 도착한 뒤 진입 자리(205 / 211)를 보고 정한다)
                    if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    pRTN_MSG = strFunction + "TRACK " + strWAIT_MC + "번[" + strFromNm + "]에서 출고위치 결정대(" + strDestMc + ")로 출발 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                    _pBdb.Commit();
                    InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strWAIT_MC, strDestMc);
                    return true;    // 사이클당 1건만 출발 (유량을 사이클마다 다시 계산한다)
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
        // 공통 코어 1 : 1층 입고대 출발 (ECS StartInvokeCheck2/5 - NEW_JOB_ORDER 의 PLC 한정판)
        //   해당 PLC 입고대(START_POS)에 재하 + 구동대기('99') 작업 → CV 지시 + 상태 '15'
        // ─────────────────────────────────────────────────────────────────


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
                strSql += CRLF + " SELECT  JM.*, CD.*, SHD.WAIT_TRACK                  ";
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
                    string strHS_TRACK_NO = "" + dtHs.Rows[i]["HS_TRACK_NO"].ToString() == "" ? "0" : dtHs.Rows[i]["HS_TRACK_NO"].ToString();


                    // 출고 계열 작업만 대상 (CHECK_CV_RETHS 와 동일 기준)
                    if (IsRetJobType(strJOB_TYP) == false)
                        continue;

                    /*
                     * 그룹 출고대(105)는 가상 자리다 - 실제 자리를 여기서 정한다.
                     *
                     *   상위는 "1층으로 빼라"만 말하고(DEST_POS=105, DEST_POS_DEF 상 MC 2101),
                     *   어느 크레인의 출고 대기대로 갈지는 WCS 가 정해야 한다. 대기대는
                     *   화물을 내려놓은 그 출고 H/S 의 짝이므로 SC_HS_DEF.WAIT_TRACK 에 있다.
                     *   (이 질의는 이미 SHD 를 HS_MC_NO 로 조인해 두었다)
                     *
                     *   전에는 JOB_MST.START_POS 를 호기로 보고 150 을 더했다. 두 군데가 틀렸다.
                     *     - START_POS 는 출고작업에서 000 으로 들어오는 일이 있다 (랙 위치는
                     *       START_LOCATION 에 있다). 그러면 0+150 = 150 이 나온다.
                     *     - 호기 표기는 9NN(901~911)이라, 값이 제대로 와도 904+150 = 1054 다.
                     *       150+n 은 호기가 1~10 일 때만 맞는 식이었다.
                     *   대기대 자리는 이미 데이터(SC_HS_DEF.WAIT_TRACK)에 있으니 계산하지 않는다.
                     *
                     *   WAIT_TRACK 은 실트랙(220)이고 PLC 에 쓸 목적지는 스테이션(154)이라
                     *   DEST_POS_DEF 로 되돌린다.
                     */
                    if (strDEST_POS == "105")
                    {
                        string strWAIT_MC = dtHs.Rows[i]["WAIT_TRACK"].ToString().Trim();
                        if (strWAIT_MC == "")
                        {
                            pRTN_MSG = strTitle + "출고 H/S(" + strHS_MC + ")에 대기대(SC_HS_DEF.WAIT_TRACK)가 없습니다. [작업번호:" + strLUGG_NO + "]";
                            InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strHS_MC, "");
                            continue;
                        }

                        if (GET_DEST_POS_STATION(strWH_TYP, strWAIT_MC, ref strDEST_POS, ref pRTN_MSG) == false)
                        {
                            pRTN_MSG = strTitle + pRTN_MSG + " [작업번호:" + strLUGG_NO + "]";
                            InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strHS_MC, strWAIT_MC);
                            continue;
                        }
                    }


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
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')               ";
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
        private bool ScRetrieveRoutine(string strWH_TYP, string strHS_NO, string strTitle, ref string pRTN_MSG)
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
                strSql += CRLF + "    AND DEST_POS IN (" + SqlInList(STN_1F_PLT) + ")            ";   // 1층으로 나갈 것만
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
                    strSql += SQL_SC_READY;                 // 크레인이 지시를 받을 수 있는 상태인가
                    strSql += SQL_SC_RET_NOT_SUSPEND;       // 출고 정지가 아닌가
                    strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                         ";   // 출고 H/S 가 비어 있다
                    strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0')       IN ('0','0000')            ";
                    strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                         ";
                    strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')               ";
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
        //        strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                       ";
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




        /*
         * 출고대 레인이 화물을 더 받을 수 있는가
         *
         *   레인은 두 자리다. 1층 출고 구간 배치(CvSim EcsLayout1.xml 좌표)는 이렇다.
         *
         *          l=51     l=52    l=53
         *     b=2   233      205     206  (출고대#1)   <- 막다른 끝
         *     b=3   232      211     212  (출고대#2)   <- 막다른 끝
         *     b=5   230
         *
         *   결정대(232)에서 갈라져 233->205->206 / 211->212 로 들어가는 막다른 지선이다.
         *   205 와 211 은 지나가는 길이 아니라 각 출고대의 진입 자리다.
         *   (실제로 돌려 보면 화물이 230 -> 232 -> 233 -> 205 -> 206 으로 간다)
         *
         *   그래서 볼 것은 "출고대(206/212)가 비었는가" 가 아니라
         *   "진입 자리(205/211)가 비었는가" 다.
         *   출고대에 화물이 있어도 진입 자리가 비어 있으면 한 대 더 보낼 수 있고,
         *   지게차가 앞의 것을 가져가면 목적지가 출고대이므로 저절로 밀려 들어간다.
         *   출고대만 보면 레인을 한 자리로만 쓰게 되어, 지게차가 올 때까지
         *   결정대와 대기대가 계속 막힌다. (대기대가 막히면 그 크레인도 못 나간다)
         *
         *   진입 중인 화물 검사는 작업대 번호로 본다. CV 에 쓰는 목적지가 작업대 번호다.
         *   그래서 한 레인에 "도착한 것 1 + 가는 중 1" 까지만 찬다.
         */
        private bool CHECK_RET_LANE_READY(string strWH_TYP, string strMC_NO, string strSTN_NO)
        {
            try
            {
                string strENTRY_MC = GET_RET_LANE_ENTRY(strWH_TYP, strMC_NO);

                string strSql = "";
                strSql += CRLF + " SELECT COUNT(*) AS READY_CNT                                        ";
                strSql += CRLF + "   FROM CV_DATA CD                                                   ";
                strSql += CRLF + "  WHERE CD.WH_TYP           = :WH_TYP                                ";
                strSql += CRLF + "    AND CD.MC_NO            = :ENTRY_MC                              ";   // 레인 진입 자리
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                                    ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                                    ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                           ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM CV_DATA CD2                        ";   // 그 레인으로 가는 중인 화물 없음
                strSql += CRLF + "              WHERE CD2.WH_TYP      = :WH_TYP2                       ";
                strSql += CRLF + "                AND CD2.DEST_POS_OD = :MC_NO2                        ";
                strSql += CRLF + "                AND CD2.OD_RQ_YN    = 'Y')                           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("ENTRY_MC", DbLang.VARCHAR).Value = strENTRY_MC;
                _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO2", DbLang.VARCHAR).Value = strSTN_NO;
                int nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt <= 0) return false;
                return Convert.ToInt32(_pBdb.mDtMain.Rows[0]["READY_CNT"]) > 0;
            }
            catch
            {
                return false;
            }
        }

        /*
         * 출고대 레인의 진입 자리 트랙 (출고대 트랙 - 1)
         *
         *   206 -> 205, 212 -> 211. 이 현장 트랙 번호는 흐름 순서대로 붙어 있다.
         *   그런 트랙이 CV_DATA 에 없으면 예전처럼 출고대 자체를 본다.
         *   (설비가 바뀌어 진입 자리가 없어져도 동작이 멈추지는 않게)
         */
        private string GET_RET_LANE_ENTRY(string strWH_TYP, string strMC_NO)
        {
            try
            {
                int nMc = 0;
                if (Int32.TryParse((strMC_NO == null) ? "" : strMC_NO.Trim(), out nMc) == false || nMc <= 0)
                    return strMC_NO;

                string strENTRY = (nMc - 1).ToString();

                string strSql = "";
                strSql += CRLF + " SELECT COUNT(*) AS CNT FROM CV_DATA                                 ";
                strSql += CRLF + "  WHERE WH_TYP = :WH_TYP AND MC_NO = :MC_NO                          ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strENTRY;
                if (_pBdb.ExcuteQry(strSql) <= 0) return strMC_NO;
                if (Convert.ToInt32(_pBdb.mDtMain.Rows[0]["CNT"]) <= 0) return strMC_NO;

                return strENTRY;
            }
            catch
            {
                return strMC_NO;
            }
        }


        #endregion


        // ─────────────────────────────────────────────────────────────────
        // DB Helper
        // ─────────────────────────────────────────────────────────────────
        #region DB Helper






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
                //   사용 포크 : 0 = 포크1 / 1 = 포크1,2(양쪽) / 2 = 포크2
                //   이 현장 크레인은 전부 SINGLE 이므로 0 이다.
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
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(ERR_CODE_RD, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                   ";

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



        #endregion
    }
}
