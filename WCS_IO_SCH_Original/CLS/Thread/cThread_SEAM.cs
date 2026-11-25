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

            int nTick = 0;
            while (!m_bManualStop)
            {
                // @.통과 기록은 0.5초마다 본다 (화물이 TO 트랙에 2~3초만 머문다). 대기 진단은 전처럼 2초마다.
                Thread.Sleep(500);
                nTick++;

                if (!IsDbAlive())
                {
                    if (!DBReopen()) Thread.Sleep(3000);
                    continue;
                }

                try
                {
                    for (int i = 0; i < SEAMS.GetLength(0); i++)
                    {
                        SeamPassLog(SEAMS[i, 0], SEAMS[i, 1]);
                        if (nTick % 4 == 0) SeamWaitDiag(SCH_WH_TYP, SEAMS[i, 0], SEAMS[i, 1], "");
                    }
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[이음새] 처리 오류: " + ex.Message);
                }
            }

            MakeMsg("[이음새] 중지 지시로 스레드를 종료합니다.");
            m_Thread = null;
        }

        // @.이음새 통과 기록 (2026-11-05). TO 트랙에 새 작업번호가 올라오면 한 줄 남긴다. (읽기 전용)
        //   전에는 IO 가 복사/삭제를 지시하거나 대기할 때만 이 탭에 글이 남았다. PLC(시뮬레이터 포함)가 데이터를 직접 넘기면
        //   IO 가 할 일이 없어 화물이 계속 지나가도 탭이 비어 있었다. 누가 넘겼든 지나간 화물은 보이게 한다.
        //   기동 뒤 처음 본 값은 기준으로만 삼고 적지 않는다.
        //   TO 트랙은 이음새가 아닌 쪽(예: 249→250)에서도 화물을 받는다. FROM 트랙에서 방금 본 작업번호가 TO 에 올라온 때만 적는다.
        private readonly Dictionary<string, string> m_dicSeamPass = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> m_dicSeamFromSeen = new Dictionary<string, List<string>>();
        private void SeamPassLog(string strFROM_MC, string strTO_MC)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT MC_NO, LUGG_NO_RD, JOB_TYP_RD, DEST_POS_RD, SENSOR0_DATA_RD ";
                strSql += CRLF + "   FROM CV_DATA                                                     ";
                strSql += CRLF + "  WHERE WH_TYP = :WH_TYP AND MC_NO IN (:FROM_MC, :TO_MC)            ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("FROM_MC", DbLang.VARCHAR).Value = strFROM_MC;
                _pBdb.mComMain.Parameters.Add("TO_MC", DbLang.VARCHAR).Value = strTO_MC;
                if (_pBdb.ExcuteQry(strSql) <= 0) return;
                DataRow r = null;
                string strFromLugg = "";
                foreach (DataRow row in _pBdb.mDtMain.Rows)
                {
                    if (("" + row["MC_NO"]).Trim() == strTO_MC) r = row;
                    else strFromLugg = ("" + row["LUGG_NO_RD"]).Trim().TrimStart('0');
                }
                string strKey = strFROM_MC + "_" + strTO_MC;

                // FROM 트랙에서 본 작업번호를 최근 5개까지 기억한다
                List<string> lstSeen;
                if (!m_dicSeamFromSeen.TryGetValue(strKey, out lstSeen)) { lstSeen = new List<string>(); m_dicSeamFromSeen[strKey] = lstSeen; }
                if (strFromLugg != "" && !lstSeen.Contains(strFromLugg))
                {
                    lstSeen.Add(strFromLugg);
                    if (lstSeen.Count > 5) lstSeen.RemoveAt(0);
                }
                if (r == null) return;

                string strLugg = ("" + r["LUGG_NO_RD"]).Trim().TrimStart('0');
                string strLast;
                bool bFirst = !m_dicSeamPass.TryGetValue(strKey, out strLast);
                if (!bFirst && strLast == strLugg) return;
                m_dicSeamPass[strKey] = strLugg;
                if (bFirst || strLugg == "") return;        // 처음 본 값 / 비워짐은 적지 않는다
                if (!lstSeen.Contains(strLugg)) return;     // 이음새가 아닌 쪽에서 들어온 화물
                lstSeen.Remove(strLugg);
                int nJob = 0, nDest = 0;
                int.TryParse(("" + r["JOB_TYP_RD"]).Trim(), out nJob);
                int.TryParse(("" + r["DEST_POS_RD"]).Trim(), out nDest);
                MakeSeamMsg("[" + SeamFloor(strFROM_MC) + " 이음새] " + strFROM_MC + "→" + strTO_MC + " 통과 - 작업번호 " + strLugg
                          + " 작업구분 " + nJob + " 목적지 " + nDest + " (센서 " + ("" + r["SENSOR0_DATA_RD"]).Trim() + ")");
            }
            catch { }
        }
    }
}
