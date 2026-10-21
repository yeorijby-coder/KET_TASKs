using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data;

namespace TSK_COMM_IOSCH
{
    /// <summary>
    /// 1층 출고 유량 모니터 스레드 (화면의 "유량(1F)" 탭)
    ///
    /// ※ 읽기 전용이다. 유량 제한으로 출발을 막고 푸는 판단은 1층 스케줄러(cThread_SCH_1F)가 그대로 한다.
    ///    이 스레드는 같은 값(CV 2/5호기 CV_RET_CNT 합 / DEL_HIS_SETTING '1f_ret_rimit')을 읽어
    ///    상태가 바뀔 때(제한 걸림 / 해제)와 주기적인 현재값을 이 탭에만 보여 준다.
    ///    그래서 이 스레드를 꺼도(INI [SCH] USE_FLOW=N) 유량 제한은 그대로 걸린다. 화면 표시만 사라진다.
    ///
    ///   제한 걸림 : 현재 합 > 제한  -> 주황색
    ///   제한 해제 : 합 <= 제한      -> 하늘색
    /// </summary>
    public class cThread_FLOW : IOSchDB
    {
        private bool m_Open;
        public bool IsOpen { get { return m_Open; } set { m_Open = value; } }

        public volatile bool m_bManualStop = false;
        public string m_strLogName = "";

        private int m_nHoldState = -1;                          // -1 = 아직 모름, 0 = 정상, 1 = 제한 중
        private int m_nLastSum = int.MinValue;
        private int m_nLastLimit = int.MinValue;
        private DateTime m_dtLastLine = DateTime.MinValue;      // 현재값 줄을 마지막으로 낸 시각
        private DateTime m_dtHoldSince = DateTime.MinValue;     // 지금 상태가 된 시각

        public cThread_FLOW(int Id)
        {
            m_nId = Id;
            SCH_WH_TYP = ((int)cDefApp.eWHTYP.KET_WH01).ToString();
        }

        public void Thread_Doing(object value)
        {
            while (!IsDBOpen)
            {
                if (m_bManualStop) { MakeMsg("[유량] 중지 지시로 스레드를 종료합니다."); m_Thread = null; return; }
                try
                {
                    if (DBOpen()) { MakeMsg("[유량] DB Open 완료 - 1층 출고 유량 모니터 시작 (읽기 전용)"); break; }
                    MakeMsg_Error("[유량] DB Open 실패 - 5초 후 재시도");
                }
                catch (Exception ex) { MakeMsg_Error("[유량] DB Open 오류 - 5초 후 재시도: " + ex.Message); }
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
                    CheckFlow();
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[유량] 처리 오류: " + ex.Message);
                }
            }

            MakeMsg("[유량] 중지 지시로 스레드를 종료합니다.");
            m_Thread = null;
        }

        // @.1층 스케줄러(cThread_SCH_1F)와 같은 식으로 합계와 제한값을 읽는다.
        private void CheckFlow()
        {
            string strSql = "";
            strSql += " SELECT COALESCE(SUM(CAST(NULLIF(EM.CV_RET_CNT,'') AS INT)), 0) AS RET_SUM ";
            strSql += "      , (SELECT CAST(NULLIF(DHS.CYCLE,'') AS INT) FROM DEL_HIS_SETTING DHS ";
            strSql += "          WHERE DHS.WH_TYP = '" + SCH_WH_TYP + "' AND DHS.TABLE_NAME = '1f_ret_rimit') AS RET_LIMIT ";
            strSql += "   FROM EQP_MST EM ";
            strSql += "  WHERE EM.WH_TYP = '" + SCH_WH_TYP + "' AND EM.EQP_TYP = 'CV' AND EM.PLC_NO IN ('02','05') ";

            DataTable dt = new DataTable();
            int nCnt = _pBdb.ExcuteQry(dt, strSql);
            if (nCnt < 0) { MakeMsg_Error("[유량] 조회 실패 : " + _pBdb.ErrMsg); return; }
            if (nCnt == 0) return;

            int nSum = dt.Rows[0]["RET_SUM"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[0]["RET_SUM"]);
            int nLimit = dt.Rows[0]["RET_LIMIT"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[0]["RET_LIMIT"]);
            int nHold = (nSum > nLimit) ? 1 : 0;
            DateTime dtNow = DateTime.Now;

            if (nHold != m_nHoldState)
            {
                string strDur = "";
                if (m_nHoldState >= 0 && m_dtHoldSince != DateTime.MinValue)
                    strDur = " (이전 상태 " + ((int)(dtNow - m_dtHoldSince).TotalSeconds).ToString() + "초 지속)";

                if (nHold == 1)
                    MakeMsg_Orange("[유량 제한] 걸림 : 현재 " + nSum.ToString() + " > 제한 " + nLimit.ToString() + " - 대기대 출발 보류" + strDur);
                else
                    MakeMsg_Blue("[유량 제한] 해제 : 현재 " + nSum.ToString() + " <= 제한 " + nLimit.ToString() + " - 출발 재개" + strDur);

                m_nHoldState = nHold;
                m_dtHoldSince = dtNow;
                m_dtLastLine = dtNow;
                m_nLastSum = nSum; m_nLastLimit = nLimit;
                return;
            }

            // 상태가 그대로일 때는 값이 바뀌었거나 제한값이 바뀐 경우에만, 그리고 10초에 한 줄까지만
            if ((nSum != m_nLastSum || nLimit != m_nLastLimit) && (dtNow - m_dtLastLine).TotalSeconds >= 10)
            {
                string strState = (nHold == 1) ? "제한 중" : "정상";
                if (nHold == 1)
                    MakeMsg_Orange("[유량] 현재 " + nSum.ToString() + " / 제한 " + nLimit.ToString() + " (" + strState + ")");
                else
                    MakeMsg("[유량] 현재 " + nSum.ToString() + " / 제한 " + nLimit.ToString() + " (" + strState + ", 여유 " + (nLimit - nSum).ToString() + ")");
                m_dtLastLine = dtNow;
                m_nLastSum = nSum; m_nLastLimit = nLimit;
            }
        }
    }
}
