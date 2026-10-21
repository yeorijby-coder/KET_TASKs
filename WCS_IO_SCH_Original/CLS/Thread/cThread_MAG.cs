using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data;

namespace TSK_COMM_IOSCH
{
    /// <summary>
    /// 팔레트 매거진(3층, 스테이션 207) 모니터 스레드 (화면의 "매거진(3F)" 탭)
    ///
    /// ※ 읽기 전용이다. 매거진 작업을 만들고 움직이는 판단은 3층 스케줄러(cThread_SCH_3F)와 HOST 가 그대로 한다.
    ///   작업 상태가 바뀌는 순간(99→10 접수, 10→11 CV_TASK 에 기록, 매거진 도착)의 기록은
    ///   그 코드(IOSchDB 의 JOB_ACCEPT / CV_STO_START_PLC / CV_ARRIVE_PLC)가 MakeMagMsg 로 이 탭에 보낸다.
    ///   이 스레드는 그 가운데 DB 에서만 알 수 있는 것, 즉 HOST 가 보낸 공파렛트 입고 요구(N 전문)를 이 탭에 보여 준다.
    ///   그래서 이 스레드를 꺼도(INI [SCH] USE_MAG=N) 매거진 동작은 그대로다. N 전문 표시만 사라진다.
    /// </summary>
    public class cThread_MAG : IOSchDB
    {
        private bool m_Open;
        public bool IsOpen { get { return m_Open; } set { m_Open = value; } }

        public volatile bool m_bManualStop = false;
        public string m_strLogName = "";

        private DateTime m_dtLast = DateTime.MinValue;      // 마지막으로 본 N 전문의 시각

        public cThread_MAG(int Id)
        {
            m_nId = Id;
            SCH_WH_TYP = ((int)cDefApp.eWHTYP.KET_WH01).ToString();
        }

        public void Thread_Doing(object value)
        {
            while (!IsDBOpen)
            {
                if (m_bManualStop) { MakeMsg("[매거진] 중지 지시로 스레드를 종료합니다."); m_Thread = null; return; }
                try
                {
                    if (DBOpen()) { MakeMsg("[매거진] DB Open 완료 - 팔레트 매거진(207) 모니터 시작 (읽기 전용)"); break; }
                    MakeMsg_Error("[매거진] DB Open 실패 - 5초 후 재시도");
                }
                catch (Exception ex) { MakeMsg_Error("[매거진] DB Open 오류 - 5초 후 재시도: " + ex.Message); }
                Thread.Sleep(5000);
            }

            // 기동 이후에 생기는 N 전문부터 본다.
            m_dtLast = DateTime.Now;

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
                    CheckPmRequest();
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[매거진] 처리 오류: " + ex.Message);
                }
            }

            MakeMsg("[매거진] 중지 지시로 스레드를 종료합니다.");
            m_Thread = null;
        }

        // @.HOST 가 보낸 공파렛트 입고 요구(N)와 그 응답 전문을 이 탭에 보여 준다.
        private void CheckPmRequest()
        {
            string strFrom = m_dtLast.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            string strSql = " SELECT INS_DT, DIRECTION, MESSAGE FROM HOST_IF_LOG "
                          + "  WHERE WH_TYP = '" + SCH_WH_TYP + "' AND HOST_CMD = 'N' AND INS_DT > '" + strFrom + "'::TIMESTAMP "
                          + "  ORDER BY INS_DT LIMIT 50";
            DataTable dt = new DataTable();
            int nCnt = _pBdb.ExcuteQry(dt, strSql);
            if (nCnt < 0) { MakeMsg_Error("[매거진] N 전문 조회 실패 : " + _pBdb.ErrMsg); return; }

            for (int i = 0; i < nCnt; i++)
            {
                DateTime dtIns = Convert.ToDateTime(dt.Rows[i]["INS_DT"]);
                string strDir = dt.Rows[i]["DIRECTION"] == DBNull.Value ? "" : dt.Rows[i]["DIRECTION"].ToString().Trim();
                string strMsg = dt.Rows[i]["MESSAGE"] == DBNull.Value ? "" : dt.Rows[i]["MESSAGE"].ToString().Trim();
                MakeMagMsg("[매거진] 공파렛트 입고 요구(N) 전문 [" + strDir + "] " + strMsg + "  (" + dtIns.ToString("HH:mm:ss") + ")");
                if (dtIns > m_dtLast) m_dtLast = dtIns;
            }
        }
    }
}
