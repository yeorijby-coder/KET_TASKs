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

        #endregion

        #region 상수 정의




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
        #region 도착 검사 전용 스레드
        /*
         * 도착 검사(ArrivedCheck2/5, 상태 11 -> 19)는 시간이 급하다.
         *   현장은 지게차 작업자가 2~3초 안에 출고대 화물을 떠가고, 시험기(CvSim)도 도착 약 5초 뒤 트랙 데이터를 지운다.
         *   그런데 스케줄러 한 바퀴는 약 16개 루틴을 차례로 돌아 부하가 걸리면 1~3초가 되므로,
         *   도착 검사만 따로 0.1초 주기로 도는 스레드를 둔다. (자기 DB 연결을 쓴다)
         *   메인 바퀴에서는 도착 검사를 뺐다. 두 곳에서 같은 도착을 동시에 처리하지 않게 하려는 것이다.
         */
        private volatile bool m_bArriveStop = false;
        private int m_nArvN = 0, m_nArvSum = 0, m_nArvMax = 0, m_nArvLast = 0;
        private Thread m_thArrive = null;
        private cThread_SCH_1F m_arrive = null;

        private void StartArriveWorker()
        {
            try
            {
                StopArriveWorker();
                m_arrive = new cThread_SCH_1F(m_nId);
                m_arrive.m_strWorkerTag = "[도착] ";
                m_arrive.ConnectionString = this.ConnectionString;
                m_arrive.callPsMsgView = this.callPsMsgView;
                m_arrive.m_Main = this.m_Main;
                m_thArrive = new Thread(m_arrive.ArriveLoop);
                m_thArrive.IsBackground = true;
                m_thArrive.Name = "SCH_ARRIVE_" + m_nId;
                m_thArrive.Start();
            }
            catch (Exception ex)
            {
                MakeMsg_Error("[SCH] 도착 검사 전용 스레드 시작 오류: " + ex.Message);
            }
        }

        private void StopArriveWorker()
        {
            try
            {
                if (m_arrive != null) m_arrive.m_bArriveStop = true;
                if (m_thArrive != null && m_thArrive.IsAlive) m_thArrive.Join(3000);
            }
            catch { }
            m_arrive = null;
            m_thArrive = null;
        }

        public void ArriveLoop()
        {
            while (!m_bArriveStop && !IsDBOpen)
            {
                try { if (DBOpen()) break; } catch { }
                Thread.Sleep(2000);
            }
            if (m_bArriveStop) return;
            MakeMsg("[SCH] 도착 검사 전용 스레드 시작 (0.1초 주기)");

            while (!m_bArriveStop)
            {
                try
                {
                    if (!IsDbAlive())
                    {
                        if (!DBReopen()) Thread.Sleep(3000);
                        continue;
                    }
                    int nC0 = Environment.TickCount;
                    RunSchFunc(ArrivedCheck2);          // 1F 구라인 픽킹대 도착보고
                    RunSchFunc(ArrivedCheck5);          // 1F 신라인 픽킹대 도착보고
                    int nC2 = Environment.TickCount;
                    // @.계측 : 도착 검사 스레드의 한 바퀴(검사2 + 검사5)를 30초마다 IoSlow.log 에 요약한다.
                    int nCyc = unchecked(nC2 - nC0);
                    if (m_nArvLast == 0) m_nArvLast = nC2;
                    m_nArvN++; m_nArvSum += nCyc; if (nCyc > m_nArvMax) m_nArvMax = nCyc;
                    if (unchecked(nC2 - m_nArvLast) >= 30000)
                    {
                        try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IoSlow.log"), DateTime.Now.ToString("HH:mm:ss") + " 도착스레드 30초 요약 : 검사 " + m_nArvN + "회 / 검사 1회(2+5) 평균 " + (m_nArvSum / m_nArvN) + " ms, 최대 " + m_nArvMax + " ms" + Environment.NewLine); } catch { }
                        m_nArvN = 0; m_nArvSum = 0; m_nArvMax = 0; m_nArvLast = nC2;
                    }
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[SCH] 도착 검사 오류: " + ex.Message);
                    Thread.Sleep(1000);
                }
                Thread.Sleep(100);
            }
            MakeMsg("[SCH] 도착 검사 전용 스레드를 종료했습니다.");
        }
        #endregion

        #region 지시 전용 스레드
        /*
         * @.WMS 지시 -> 설비 구동까지의 지연을 줄이려고 "지시를 내는 함수"만 따로 0.05초 주기로 돈다. (2026-11-08)
         *   전에는 메인 바퀴(함수 20여 개 + 함수마다 10ms + 바퀴 200ms)를 한 번 돌아야 접수(99)->지시(20->21)가 이어졌다.
         *   여기서 도는 것 : JobAccept, StartInvokeCheck2, StartInvokeCheck5, StoHsCheck2, StoHsCheck5, RetCmdCheck, RackToRackCheck
         *   나머지(도착 검사, 완료, 출고 H/S, 청소 등)는 메인 바퀴에 그대로 둔다. 같은 함수가 두 곳에서 돌지 않게 메인에서는 뺐다.
         *   자기 DB 연결을 쓴다.
         */
        protected override IOSchDB CreateWorker() { return new cThread_SCH_1F(m_nId); }

        protected override SchFunc[] CmdFuncs()
        {
            return new SchFunc[] { JobAccept, StartInvokeCheck2, StartInvokeCheck5, StoHsCheck2, StoHsCheck5, RetCmdCheck, RackToRackCheck };
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

            StartArriveWorker();   // 도착 검사 전용 스레드

            StartCmdWorker();   // 지시 전용 스레드

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

                    int nLoopT0 = Environment.TickCount;
                    // 도착 검사(ArrivedCheck2/5)는 전용 스레드(ArriveLoop)가 0.1초 주기로 돈다. 현장은 지게차가 2~3초 안에 화물을 떠간다.

                    RunSchFunc(RetInvokeCheck2);        // 1F 구라인 출고HS → 픽킹대 지시
                    RunSchFunc(RetInvokeCheck5);        // 1F 신라인 출고HS → 픽킹대 지시


                    RunSchFunc(ScCompleteCheck);        // 크레인 작업 완료 -> 29

                    RunSchFunc(CopyTrackData2);         // 이음새 352→631 작업 데이터 복사
                    RunSchFunc(DeleteTrackData2);       // 이음새 352 작업 데이터 삭제
                    RunSchFunc(CopyTrackData5);         // 이음새 654→355 작업 데이터 복사
                    RunSchFunc(DeleteTrackData5);       // 이음새 654 작업 데이터 삭제

                    // 출고 루프 출발 - 중간 도착대(151~160)와 출고위치 결정대(171)를
                    // 같은 조건으로 한 함수에서 본다. PLC 별로 자기 트랙만 본다.
                    RunSchFunc(NewStartRoutinePlc2);    // 1F 구라인(PLC 02) 출고 루프 출발
                    RunSchFunc(NewStartRoutinePlc5);    // 1F 신라인(PLC 05) 출고 루프 출발
                    int nLoopMs = unchecked(Environment.TickCount - nLoopT0);
                    if (nLoopMs >= 1000)
                    {
                        MakeMsg("[SCH] 한 바퀴 " + nLoopMs + " ms 걸림 (도착 검사 지연 주의)");
                        try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IoSlow.log"), DateTime.Now.ToString("HH:mm:ss") + " 한 바퀴 " + nLoopMs + " ms" + Environment.NewLine); } catch { }
                    }
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
            StopArriveWorker();
            StopCmdWorker();
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

        // @.H/S 자리 번호(HSPOS_*_1F)와 1층 출고 작업대(STN_1F_PLT)는 1F/3F 공통 출고 지시와 함께 IOSchDB 로 옮겼다.



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
        { return CV_RETHS_PLC(strWH_TYP, CV_PLC_1F_OLD, EN_RET_FLOOR.F1, "[RetInvokeCheck2]", ref pRTN_MSG); }
        public bool RetInvokeCheck5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_RETHS_PLC(strWH_TYP, CV_PLC_1F_NEW, EN_RET_FLOOR.F1, "[RetInvokeCheck5]", ref pRTN_MSG); }

        public bool ArrivedCheck2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_ARRIVE_PLC(strWH_TYP, CV_PLC_1F_OLD, "[ArrivedCheck2]", ref pRTN_MSG); }
        public bool ArrivedCheck5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CV_ARRIVE_PLC(strWH_TYP, CV_PLC_1F_NEW, "[ArrivedCheck5]", ref pRTN_MSG); }

        public bool StoHsCheck2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScStoreRoutine(strWH_TYP, CV_PLC_1F_OLD, HSPOS_STO_1F, HSPOS_RET_1F, "[StoHsCheck2]", ref pRTN_MSG); }

        public bool StoHsCheck5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScStoreRoutine(strWH_TYP, CV_PLC_1F_NEW, HSPOS_STO_1F, HSPOS_RET_1F, "[StoHsCheck5]", ref pRTN_MSG); }

        /*
         * 신규 작업 접수(99 → 10 / 20)는 층이 아니라 작업 단위의 일이라
         * 본체를 공통(IOSchDB.JOB_ACCEPT)으로 올렸다. 세 스레드가 다 이 래퍼를
         * 갖고, 한 주기에는 잠금을 먼저 잡은 하나만 실제로 접수한다.
         * (전에는 1F 에만 있어 USE_1F=N 이면 3층·BOX 가 통째로 멈췄다)
         */
        public bool JobAccept(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return JOB_ACCEPT(strWH_TYP, "[JobAccept]", ref pRTN_MSG); }


        /*
         * 출고 지시도 크레인 완료와 마찬가지로 층이 아니라 작업에 속한 일이다.
         * 다만 내려놓을 출고 H/S 는 층마다 다르므로(SC_HS_DEF.HS_NO) 층 값을 넘긴다.
         * 1층은 '02' 다. 세 스레드가 겹치지 않도록 여기서만 돌린다.
         */
        public bool RetCmdCheck(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScRetrieveRoutine(strWH_TYP, HS_NO_RETRIEVE, EN_RET_FLOOR.F1, "[RetCmdCheck]", ref pRTN_MSG); }


        public bool NewStartRoutinePlc2(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CHECK_CV_RET_START(strWH_TYP, CV_PLC_1F_OLD, "[NewStartRoutinePlc2]", ref pRTN_MSG); }
        public bool NewStartRoutinePlc5(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return CHECK_CV_RET_START(strWH_TYP, CV_PLC_1F_NEW, "[NewStartRoutinePlc5]", ref pRTN_MSG); }


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

        // 결정대(171) 에서 마지막에 보낸 출고대 (0 = 아직 없음, 1 = #1(103), 2 = #2(104)). 번갈아 보내는 데 쓴다. (2026-11-15)
        private static int s_nDecideLast = 0;

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
            /*
             * @.준비 신호는 RTV_DEPARTHS_READY_RD (D554, RGV 적재 H/S 준비) 다. (2026-10-20)
             *   원본 NewStartRoutinePlc2/5 는 IsStoHomeStandReady() 를 본다. 원본 ParsingExtraFrame 의
             *   "Loading RGV HS : 4" 가 그 신호이고, CV 태스크 DeviceMap 의 RtvDepartHS(D554)가 같은 자리다.
             *   중간 도착대(151~160)와 결정대(171)에는 출고대(D551) 비트가 없다. 예전에는 RET_READY_RD 를 봐서
             *   시뮬레이터에서만 돌고(D558 에 겹쳐 둔 RetStation) 현장 PLC 에서는 출발 지시가 나가지 않았다.
             */
            strSql += CRLF + "    AND  " + strCd + ".RTV_DEPARTHS_READY_RD = '1'                           ";   // RGV 적재 H/S 준비
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
                //      현재 PLC 상 화물 갯수(CV 2호기 + 5호기의 CV_RET_CNT 합) + 출발 트랙 중 목적지 171 인 것 >= 제한(cycle)
                //      이면 루프에 새로 넣지 않는다. 결정대 화물은 이 문에 걸리지 않는다.
                strSql = "";
                strSql += CRLF + " SELECT COALESCE(SUM(CAST(NULLIF(EM.CV_RET_CNT,'') AS INT)), 0) AS RET_SUM   ";
                // @.원본 ECS(CJob::NewStartRoutine)의 nProdCnt : 출발 트랙 10곳 중 목적지가 171 인 것 + 2004 의 목적지가 105 이면 1.
                //   출발 지시를 냈지만 PLC 개수에 아직 안 잡힌 화물을 더해 한 사이클에 중복으로 내보내지 않게 한다.
                strSql += CRLF + "      , (SELECT COUNT(*) FROM CV_DATA CP                                      ";
                strSql += CRLF + "          WHERE CP.WH_TYP = :WH_TYP3                                          ";
                // @.메인 폼 "트랙 204 유량 제한 제외" 가 켜져 있으면 204(트랙 2004)는 이 집계에서도 뺀다.
                string strProdTr = (cDefApp.GM_RET_EXCLUDE_204 == true)
                                 ? "'2008','2016','2020','2059','2049','5006','5012','5018','5024'"
                                 : "'2004','2008','2016','2020','2059','2049','5006','5012','5018','5024'";
                string strProd105 = (cDefApp.GM_RET_EXCLUDE_204 == true)
                                 ? ""
                                 : " OR (CP.TRACK_NO = '2004' AND BTRIM(CP.DEST_POS_RD) = '105')";
                strSql += CRLF + "            AND ((CP.TRACK_NO IN (" + strProdTr + ") ";
                strSql += CRLF + "                  AND BTRIM(CP.DEST_POS_RD) = '171')                          ";
                strSql += CRLF + "              " + strProd105 + ")) AS PROD_CNT ";
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
                _pBdb.mComMain.Parameters.Add("WH_TYP3", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                int nRetSum = 0;
                int nProdCnt = 0;
                int nRetLimit = 0;
                if (nSelCnt > 0)
                {
                    nRetSum = _pBdb.mDtMain.Rows[0]["RET_SUM"] == DBNull.Value ? 0 : Convert.ToInt32(_pBdb.mDtMain.Rows[0]["RET_SUM"]);
                    nProdCnt = _pBdb.mDtMain.Rows[0]["PROD_CNT"] == DBNull.Value ? 0 : Convert.ToInt32(_pBdb.mDtMain.Rows[0]["PROD_CNT"]);
                    nRetLimit = _pBdb.mDtMain.Rows[0]["RET_LIMIT"] == DBNull.Value ? 0 : Convert.ToInt32(_pBdb.mDtMain.Rows[0]["RET_LIMIT"]);
                }

                // @.원본 ECS : m_nRetCnt + m_nRetCntNew + nProdCnt >= 제한 이면 진입 불가 (이상 - 이전 WCS 는 초과(>) 였다)
                nRetSum += nProdCnt;
                bool bLoopFull = (nRetSum >= nRetLimit);
                if (bLoopFull == true)
                {
                    // 제한 초과 - 대기대에서는 새로 내보내지 않는다 (보류 진입 시 1회만 메시지)
                    if (m_bRetLimitHold == false)
                    {
                        m_bRetLimitHold = true;
                        MakeMsg_Orange(strFunction + "1층 출고 유량 제한으로 대기대 출발 보류. [현재:" + nRetSum + " / 제한:" + nRetLimit + "]");
                    }
                }
                else if (m_bRetLimitHold == true)
                {
                    m_bRetLimitHold = false;
                    MakeMsg_Blue(strFunction + "1층 출고 유량 정상 - 출발 재개. [현재:" + nRetSum + " / 제한:" + nRetLimit + "]");
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
                /*
                 * (2026-11-23) 기존 ECS(CCv::ReStartRoutine2)와 똑같이 맞췄다.
                 *
                 *   기존 ECS 는 트랙 2005 / 2011(진입 자리 205 / 211)의 IsRetHomeStandReady() 만 본다.
                 *   그 신호는 ParsingExtraFrame 의 "Unloading RGV HS : 5" 이고, CV 태스크 DeviceMap 의
                 *   RtvArriveHS(D555) = CV_DATA.RTV_ARRIVEHS_READY_RD 가 같은 자리다.
                 *
                 *     2005 준비 됨            -> 출고대#1(103)   (2011 이 어떻든 #1 이 먼저다)
                 *     2005 준비 안 됨         -> 출고대#2(104)   (2011 도 준비 안 됐어도 #2 로 보낸다. 결정대에서 기다리지 않는다)
                 *
                 *   전에는 진입 자리 센서와 가는 중인 화물을 보고, 둘 다 비면 #1 / #2 를 번갈아 보내고,
                 *   둘 다 차면 233 까지만 쌓고 결정대에서 기다렸다. (CHECK_RET_LANE_READY / CHECK_RET_LANE_QUEUE_ROOM 은 남겨 둔다)
                 */
                bool bRet1Ready = CHECK_RET_HS_READY(strWH_TYP, GET_RET_LANE_ENTRY(strWH_TYP, strMC_RET1));
                bool bRet2Ready = CHECK_RET_HS_READY(strWH_TYP, GET_RET_LANE_ENTRY(strWH_TYP, strMC_RET2));

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
                    //   메인 폼 "트랙 204 유량 제한 제외" 가 켜져 있으면 204(중간 도착대 #1)는 유량 제한에 걸리지 않는다.
                    if (bLoopFull == true && bAtDecide == false &&
                        !(bAtWait1 == true && cDefApp.GM_RET_EXCLUDE_204 == true))
                        continue;

                    // ── 목적지 결정 : 어디에 서 있느냐로 갈린다
                    string strDestStn = "";    // CV / JOB_MST 에 쓰는 목적지 (작업대 번호)
                    string strDestNm = "";
                    string strWhy = "";
                    if (bAtDecide == true)
                    {
                        // 결정대 : 기존 ECS 와 같다. 출고대#1 진입 자리(205)가 준비됐으면 #1, 아니면 #2. (2026-11-23)
                        if (bRet1Ready == true) { strDestStn = "103"; strDestNm = "출고대#1"; strWhy = "출고대#1 진입 자리 준비됨"; }
                        else                    { strDestStn = "104"; strDestNm = "출고대#2"; strWhy = (bRet2Ready == true) ? "출고대#1 준비 안 됨, #2 준비됨" : "둘 다 준비 안 됨 - #2 로 보냄"; }
                        s_nDecideLast = (strDestStn == "103") ? 1 : 2;      // 마지막에 보낸 출고대 (기록용)
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






        // 출고대 레인 진입 자리(205 / 211)의 PLC 준비 신호. 기존 ECS 의 IsRetHomeStandReady() 와 같은 자리다. (2026-11-23)
        private bool CHECK_RET_HS_READY(string strWH_TYP, string strENTRY_MC)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT COUNT(*) AS READY_CNT                                        ";
                strSql += CRLF + "   FROM CV_DATA CD                                                   ";
                strSql += CRLF + "  WHERE CD.WH_TYP                = :WH_TYP                           ";
                strSql += CRLF + "    AND CD.MC_NO                 = :ENTRY_MC                         ";
                strSql += CRLF + "    AND CD.RTV_ARRIVEHS_READY_RD = '1'                               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("ENTRY_MC", DbLang.VARCHAR).Value = strENTRY_MC;
                int nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt <= 0) return false;
                return Convert.ToInt32(_pBdb.mDtMain.Rows[0]["READY_CNT"]) > 0;
            }
            catch
            {
                return false;
            }
        }

        // 출고대#1 레인(결정대 232 -> 233 -> 205 -> 206)의 마지막 대기 자리. 진입 자리가 찬 뒤에도 여기까지는 쌓는다. (2026-11-15)
        private const string RET1_QUEUE_MC = "233";

        // 대기 자리(233)가 비었고(센서 0, 자동, 에러 없음) 출고대#1 로 가는 중인 화물이 없으면 한 대 더 보낼 수 있다.
        private bool CHECK_RET_LANE_QUEUE_ROOM(string strWH_TYP, string strQUEUE_MC, string strSTN_NO)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT COUNT(*) AS READY_CNT                                        ";
                strSql += CRLF + "   FROM CV_DATA CD                                                   ";
                strSql += CRLF + "  WHERE CD.WH_TYP           = :WH_TYP                                ";
                strSql += CRLF + "    AND CD.MC_NO            = :QUEUE_MC                              ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                                    ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                                    ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                           ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM CV_DATA CD2                        ";
                strSql += CRLF + "              WHERE CD2.WH_TYP      = :WH_TYP2                       ";
                strSql += CRLF + "                AND CD2.DEST_POS_OD = :MC_NO2                        ";
                strSql += CRLF + "                AND CD2.OD_RQ_YN    = 'Y')                           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("QUEUE_MC", DbLang.VARCHAR).Value = strQUEUE_MC;
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
            // 보는 자리 = 레인 진입 자리, 가는 중인 화물의 목적지 = 작업대 번호 (공통 검사는 IOSchDB.CHECK_LANE_FREE)
            return CHECK_LANE_FREE(strWH_TYP, GET_RET_LANE_ENTRY(strWH_TYP, strMC_NO), strSTN_NO);
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
        //*/



        #endregion
    }
}
