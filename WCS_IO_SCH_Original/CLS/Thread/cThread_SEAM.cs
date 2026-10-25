using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data;

namespace TSK_COMM_IOSCH
{
    /// <summary>
    /// 이음새 모니터 스레드 (화면의 "이음새" 탭, 1층·3층 공통)
    ///
    /// 구 ECS 의 CopyTrackData / DeleteTrackData 에 해당하는 일 :
    ///   1F : 264→531, 556→250      3F : 352→631, 654→355
    ///   FROM 트랙에서 화물이 떠난 뒤 남은 작업데이터(작업번호/작업구분/목적지)를 TO 트랙에 써 주고, 반영되면 FROM 을 지운다.
    ///
    /// ※ 복사/삭제 판단은 층별 스케줄러(cThread_SCH_1F / _3F)가 그대로 한다. 그 코드(IOSchDB 의 COPY_TRACK_DATA / DELETE_TRACK_DATA)가
    ///   복사·삭제 한 줄을 MakeSeamMsg 로 이 탭에 보낸다.
    ///   이 스레드는 읽기만 한다 : 이음새 네 곳을 2초마다 보고, FROM 에서 화물이 떠났는데 TO 가 받을 준비가 안 된 때
    ///   (센서 / 작업번호 / 이동준비 D569.0 / OD_RQ_YN)를 상태가 바뀔 때만 한 줄 적어 "왜 안 넘어가나" 를 바로 보이게 한다.
    ///   그래서 이 스레드를 꺼도(INI [SCH] USE_SEAM=N) 이음새 동작은 그대로이고, 로그는 각 층 탭에 남는다.
    /// </summary>
    public class cThread_SEAM : IOSchDB
    {
        private bool m_Open;
        public bool IsOpen { get { return m_Open; } set { m_Open = value; } }

        public volatile bool m_bManualStop = false;
        public string m_strLogName = "";

        // 구 ECS 와 같은 네 곳 (FROM, TO)
        private static readonly string[,] SEAMS = new string[,] {
            { "264", "531" }, { "556", "250" }, { "352", "631" }, { "654", "355" } };

        public cThread_SEAM(int Id)
        {
            m_nId = Id;
            SCH_WH_TYP = ((int)cDefApp.eWHTYP.KET_WH01).ToString();
        }

        public void Thread_Doing(object value)
        {
            while (!IsDBOpen)
            {
                if (m_bManualStop) { MakeMsg("[이음새] 중지 지시로 스레드를 종료합니다."); m_Thread = null; return; }
                try
                {
                    if (DBOpen()) { MakeMsg("[이음새] DB Open 완료 - 이음새(1F 264→531, 556→250 / 3F 352→631, 654→355) 모니터 시작 (읽기 전용)"); break; }
                    MakeMsg_Error("[이음새] DB Open 실패 - 5초 후 재시도");
                }
                catch (Exception ex) { MakeMsg_Error("[이음새] DB Open 오류 - 5초 후 재시도: " + ex.Message); }
                Thread.Sleep(5000);
            }

            while (!m_bManualStop)
            {
                Thread.Sleep(2000);

                if (!IsDbAlive())
                {
                    if (!DBReopen()) Thread.Sleep(3000);
                    continue;
                }

                try
                {
                    for (int i = 0; i < SEAMS.GetLength(0); i++)
                        SeamWaitDiag(SCH_WH_TYP, SEAMS[i, 0], SEAMS[i, 1], "");
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[이음새] 처리 오류: " + ex.Message);
                }
            }

            MakeMsg("[이음새] 중지 지시로 스레드를 종료합니다.");
            m_Thread = null;
        }
    }
}
