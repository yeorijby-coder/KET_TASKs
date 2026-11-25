using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace TSK_COMM_IOSCH
{
    /// <summary>
    /// 공통 스레드 (화면의 "공통" 탭) : 층과 무관한 일을 한 곳에서 맡는다.
    ///   1) 신규 작업 접수(99 -> 10/20)  2) 랙투랙 지시(20 -> 21)  3) 오래된 로그 삭제(자식 스레드)
    ///   접수/랙투랙은 0.1~0.2초 주기라 오래 걸리는 로그 삭제와 한 스레드에 두면 삭제 동안 접수가 멈춘다.
    ///   그래서 로그 삭제는 자기 DB 연결을 쓰는 자식 스레드로 따로 돌리고, 같은 탭에 남긴다.
    ///   이 스레드가 살아 있는 동안(맥박 3초) 층 스레드는 접수/랙투랙을 하지 않는다. 멈추면 층 스레드가 대신한다.
    ///
    /// 아래는 로그 삭제 부분의 원래 설명이다. (화면의 "LOG삭제")
    ///
    /// DEL_HIS_SETTING 의 설정(TABLE_NAME / CYCLE / UNIT / KEY_COLUMN)대로 기간이 지난 로그를 지운다.
    ///   - 전에는 cThread_R.CHECK_DEL_HIS 가 하던 일인데, 그 스레드는 현장 구성에서 빌드 제외돼
    ///     한 번도 돌지 않았다. 그래서 새 스레드로 분리했다.
    ///   - 삭제 대상이 아닌 설정 행(1f_ret_rimit = 유량 제한 개수, empty_ret_auto_del = Y/N 등)은 건너뛴다.
    ///   - 실행 시각 : ENV_IOSCH.INI [LOG_DEL] 의 HOUR/MINUTE/SECOND 부터 ADDVALUE ADDTYPE(예: 30 MINUTE) 사이에 하루 한 번.
    ///                 [LOG_DEL] RUN_AT_START=Y(기본) 이면 기동 직후에도 한 번 돈다.
    ///   - 큰 테이블(host_if_log 등)은 한 번에 지우지 않고 10만 건씩 나눠 커밋한다.
    /// </summary>
    public class cThread_LOG : IOSchDB
    {
        #region 변수
        private bool m_Open;
        public bool IsOpen { get { return m_Open; } set { m_Open = value; } }

        /// <summary>화면의 스레드 상태 표시에서 "중지" 를 고르면 참 (UI 가 쓰고 이 스레드가 읽는다)</summary>
        public volatile bool m_bManualStop = false;
        public string m_strLogName = "";

        private DateTime m_dtLastRun = DateTime.MinValue;       // 마지막으로 삭제를 돌린 시각
        private DateTime m_dtLastIdleMsg = DateTime.MinValue;   // 대기 중 안내를 마지막으로 낸 시각
        private bool m_bStartRunDone = false;                   // 기동 직후 실행을 했는지

        private const int DEL_CHUNK = 100000;                   // 한 번에 지우는 최대 건수

        // @.공통 스레드 본체와 로그 삭제 자식 스레드를 같은 클래스로 만든다. 자식은 삭제만 한다.
        public bool m_bDeleteOnly = false;
        private cThread_LOG m_del = null;
        private Thread m_thDel = null;

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion

        #region 생성자
        public cThread_LOG(int Id)
        {
            m_nId = Id;
            SCH_WH_TYP = ((int)cDefApp.eWHTYP.KET_WH01).ToString();
        }
        #endregion

        private static string GetIni(string strKey, string strDef)
        {
            StringBuilder sb = new StringBuilder(64);
            GetPrivateProfileString("LOG_DEL", strKey, strDef, sb, sb.Capacity, cDefApp.GM_ENV_INI);
            return sb.ToString().Trim();
        }

        #region 공통 스레드 본체
        // 접수는 두 창고(파렛트 10 / P-BOX 20)를 다 본다. 랙투랙은 파렛트 창고만.
        private const string WH_PLT = "10";
        private const string WH_BOX = "20";
        public bool AcceptPlt(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return JOB_ACCEPT(WH_PLT, "[JobAccept]", ref pRTN_MSG, true); }
        public bool AcceptBox(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return JOB_ACCEPT(WH_BOX, "[JobAccept]", ref pRTN_MSG, true); }
        public bool ScAutoRecoverPlt(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScAutoRecover(WH_PLT, strPLC_NO, ref pRTN_MSG); }
        public bool ScAutoRecoverBox(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScAutoRecover(WH_BOX, strPLC_NO, ref pRTN_MSG); }
        // @.이중입고 / 공출고 재지정 뒤처리 (2026-11-05)
        public bool ScDualStoreRetryPlt(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScDualStoreRetry(WH_PLT, strPLC_NO, ref pRTN_MSG); }
        public bool ScDualStoreRetryBox(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return ScDualStoreRetry(WH_BOX, strPLC_NO, ref pRTN_MSG); }
        public bool RackToRackCommon(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return RACK_TO_RACK_ROUTINE(WH_PLT, "[RackToRackCheck]", ref pRTN_MSG, true); }

        private void StartDeleteChild()
        {
            try
            {
                StopDeleteChild();
                m_del = new cThread_LOG(m_nId);
                m_del.m_bDeleteOnly = true;
                m_del.ConnectionString = this.ConnectionString;
                m_del.callPsMsgView = this.callPsMsgView;
                m_del.m_Main = this.m_Main;
                m_thDel = new Thread(m_del.Thread_Doing);
                m_thDel.IsBackground = true;
                m_thDel.Name = "COMMON_LOGDEL_" + m_nId;
                m_thDel.Start();
            }
            catch (Exception ex) { MakeMsg_Error("[공통] 로그 삭제 스레드 시작 오류: " + ex.Message); }
        }

        private void StopDeleteChild()
        {
            try
            {
                if (m_del != null) m_del.m_bManualStop = true;
                if (m_thDel != null && m_thDel.IsAlive) m_thDel.Join(3000);
            }
            catch { }
            m_del = null; m_thDel = null;
        }

        private void CommonBody()
        {
            while (!IsDBOpen)
            {
                if (m_bManualStop) { MakeMsg("[공통] 중지 지시로 스레드를 종료합니다."); m_Thread = null; return; }
                try
                {
                    if (DBOpen())
                    {
                        MakeMsg("[공통] DB Open 완료 - 공통 스레드 시작 (신규 작업 접수 / 랙투랙 지시 / 오래된 로그 삭제)");
                        break;
                    }
                    MakeMsg_Error("[공통] DB Open 실패 - 5초 후 재시도");
                }
                catch (Exception ex) { MakeMsg_Error("[공통] DB Open 오류 - 5초 후 재시도: " + ex.Message); }
                Thread.Sleep(5000);
            }

            StartDeleteChild();

            int nLastRecover = Environment.TickCount - 5000;
            int nLastDualRetry = Environment.TickCount;
            while (!m_bManualStop)
            {
                // 맥박 : 이 스레드가 살아 있는 동안 층 스레드는 접수/랙투랙을 하지 않는다
                CommonBeat();
                Thread.Sleep(100);

                if (!IsDbAlive())
                {
                    if (!DBReopen()) Thread.Sleep(3000);
                    continue;
                }

                try
                {
                    RunSchFunc(AcceptPlt);          // 신규 작업 접수 (파렛트 창고)
                    RunSchFunc(AcceptBox);          // 신규 작업 접수 (P-BOX 창고)
                    RunSchFunc(RackToRackCommon);   // 랙투랙 지시 20 -> 21

                    // 이중입고 / 공출고 재지정 뒤처리 : 1초마다
                    if (unchecked(Environment.TickCount - nLastDualRetry) >= 1000)
                    {
                        nLastDualRetry = Environment.TickCount;
                        RunSchFunc(ScDualStoreRetryPlt);
                        RunSchFunc(ScDualStoreRetryBox);
                    }

                    // 크레인 자동 에러 해제 / 자동 ACTIVE : 5초마다 (체크박스가 꺼져 있으면 아무것도 안 한다)
                    if (unchecked(Environment.TickCount - nLastRecover) >= 5000)
                    {
                        nLastRecover = Environment.TickCount;
                        RunSchFunc(ScAutoRecoverPlt);
                        RunSchFunc(ScAutoRecoverBox);   // P-BOX 창고 크레인(911)도 본다
                    }
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[공통] 처리 오류: " + ex.Message);
                    Thread.Sleep(1000);
                }
            }

            StopDeleteChild();
            MakeMsg("[공통] 중지 지시로 스레드를 종료합니다. (이후 접수/랙투랙은 층 스레드가 대신합니다)");
            m_Thread = null;
        }
        #endregion

        #region Thread_Doing (로그 삭제 본체 / 공통 본체 선택)
        public void Thread_Doing(object value)
        {
            if (!m_bDeleteOnly) { CommonBody(); return; }
            while (!IsDBOpen)
            {
                if (m_bManualStop) { MakeMsg("[LOG삭제] 중지 지시로 스레드를 종료합니다."); m_Thread = null; return; }

                try
                {
                    if (DBOpen())
                    {
                        MakeMsg("[LOG삭제] DB Open 완료 - 오래된 로그 삭제 스레드 시작 (DEL_HIS_SETTING)");
                        break;
                    }
                    MakeMsg_Error("[LOG삭제] DB Open 실패 - 5초 후 재시도");
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[LOG삭제] DB Open 오류 - 5초 후 재시도: " + ex.Message);
                }
                Thread.Sleep(5000);
            }

            while (!m_bManualStop)
            {
                Thread.Sleep(1000);

                if (!IsDbAlive())
                {
                    if (!DBReopen()) Thread.Sleep(3000);
                    continue;
                }

                try
                {
                    DateTime dtNow = DateTime.Now;
                    bool bRun = false;
                    string strWhy = "";

                    // 1) 기동 직후 한 번
                    if (m_bStartRunDone == false)
                    {
                        m_bStartRunDone = true;
                        if (GetIni("RUN_AT_START", "Y").ToUpper() != "N")
                        {
                            bRun = true;
                            strWhy = "기동 직후";
                        }
                    }

                    // 2) 정해진 시간대(하루 한 번)
                    if (bRun == false && IsInWindow(dtNow) && m_dtLastRun.Date != dtNow.Date)
                    {
                        bRun = true;
                        strWhy = "정해진 시각";
                    }

                    if (bRun)
                    {
                        MakeMsg_Imp("[LOG삭제] 오래된 로그 삭제를 시작합니다. (" + strWhy + ")");
                        m_dtLastRun = DateTime.Now;
                        string strMsg = "";
                        if (RunDelete(SCH_WH_TYP, ref strMsg) == false && strMsg != "")
                            MakeMsg_Error(strMsg);
                        continue;
                    }

                    // 대기 중 안내 (30분마다)
                    if ((dtNow - m_dtLastIdleMsg).TotalMinutes >= 30)
                    {
                        m_dtLastIdleMsg = dtNow;
                        MakeMsg("[LOG삭제] 대기 중 - 다음 실행 " + GetWindowText() + " (마지막 실행 : "
                                + (m_dtLastRun == DateTime.MinValue ? "없음" : m_dtLastRun.ToString("yyyy-MM-dd HH:mm:ss")) + ")");
                    }
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[LOG삭제] 처리 오류: " + ex.Message);
                }
            }

            MakeMsg("[LOG삭제] 중지 지시로 스레드를 종료합니다.");
            m_Thread = null;
        }
        #endregion

        #region 실행 시간대
        // @.INI [LOG_DEL] HOUR/MINUTE/SECOND 부터 ADDVALUE ADDTYPE 사이
        private bool TryGetWindow(DateTime dtNow, out DateTime dtFrom, out DateTime dtTo)
        {
            dtFrom = dtNow; dtTo = dtNow;
            int h, m, s, add;
            if (!int.TryParse(GetIni("HOUR", ""), out h)) return false;
            if (!int.TryParse(GetIni("MINUTE", "0"), out m)) m = 0;
            if (!int.TryParse(GetIni("SECOND", "0"), out s)) s = 0;
            if (!int.TryParse(GetIni("ADDVALUE", "30"), out add)) add = 30;
            if (h < 0 || h > 23 || m < 0 || m > 59 || s < 0 || s > 59) return false;

            dtFrom = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, h, m, s);
            switch (GetIni("ADDTYPE", "MINUTE").ToUpper())
            {
                case "SECOND": dtTo = dtFrom.AddSeconds(add); break;
                case "HOUR": dtTo = dtFrom.AddHours(add); break;
                case "DAY": dtTo = dtFrom.AddDays(add); break;
                case "MONTH": dtTo = dtFrom.AddMonths(add); break;
                case "YEAR": dtTo = dtFrom.AddYears(add); break;
                default: dtTo = dtFrom.AddMinutes(add); break;
            }
            return true;
        }

        private bool IsInWindow(DateTime dtNow)
        {
            DateTime dtFrom, dtTo;
            if (!TryGetWindow(dtNow, out dtFrom, out dtTo)) return false;
            return (dtNow >= dtFrom && dtNow < dtTo);
        }

        private string GetWindowText()
        {
            DateTime dtFrom, dtTo;
            DateTime dtNow = DateTime.Now;
            if (!TryGetWindow(dtNow, out dtFrom, out dtTo)) return "(INI [LOG_DEL] HOUR 없음 - 정해진 시각 실행 안 함)";
            if (dtNow >= dtTo) dtFrom = dtFrom.AddDays(1);
            return dtFrom.ToString("yyyy-MM-dd HH:mm:ss");
        }
        #endregion

        #region 삭제
        private static readonly Regex m_reName = new Regex("^[A-Za-z0-9_]+$");

        // @.DEL_HIS_SETTING 대로 지운다. 지운 건수는 화면(LOG삭제 탭)과 WCS_LOG_PGR 에 남긴다.
        private bool RunDelete(string strWH_TYP, ref string pRTN_MSG)
        {
            pRTN_MSG = "";
            DataTable dt = new DataTable();
            string strSql = " SELECT TABLE_NAME, CYCLE, UNIT, KEY_COLUMN FROM DEL_HIS_SETTING WHERE WH_TYP = '" + strWH_TYP + "' ORDER BY TABLE_NAME";
            int nCnt = _pBdb.ExcuteQry(dt, strSql);
            if (nCnt < 0) { pRTN_MSG = "[LOG삭제] DEL_HIS_SETTING 조회 실패 : " + _pBdb.ErrMsg; return false; }
            if (nCnt == 0) { MakeMsg("[LOG삭제] DEL_HIS_SETTING 에 설정이 없습니다."); return true; }

            long nTotal = 0;
            for (int i = 0; i < nCnt && !m_bManualStop; i++)
            {
                string strTable = (dt.Rows[i]["TABLE_NAME"] == DBNull.Value ? "" : dt.Rows[i]["TABLE_NAME"].ToString()).Trim();
                string strCycle = (dt.Rows[i]["CYCLE"] == DBNull.Value ? "" : dt.Rows[i]["CYCLE"].ToString()).Trim();
                string strUnit = (dt.Rows[i]["UNIT"] == DBNull.Value ? "" : dt.Rows[i]["UNIT"].ToString()).Trim().ToUpper();
                string strKey = (dt.Rows[i]["KEY_COLUMN"] == DBNull.Value ? "" : dt.Rows[i]["KEY_COLUMN"].ToString()).Trim();

                // @.삭제 대상이 아닌 설정 행은 건너뛴다. (기준 컬럼이 없거나, 단위가 시간 단위가 아니거나, 주기가 숫자가 아님)
                int nCycle;
                bool bTimeUnit = (strUnit == "YEAR" || strUnit == "MONTH" || strUnit == "DAY" || strUnit == "HOUR" || strUnit == "MINUTE" || strUnit == "SECOND");
                if (strTable == "" || strKey == "" || !bTimeUnit || !int.TryParse(strCycle, out nCycle) || nCycle <= 0)
                {
                    MakeMsg("[LOG삭제] 건너뜀 (삭제 대상 아님) : " + strTable + " [CYCLE=" + strCycle + " UNIT=" + strUnit + " KEY=" + strKey + "]");
                    continue;
                }
                if (!m_reName.IsMatch(strTable) || !m_reName.IsMatch(strKey))
                {
                    MakeMsg_Error("[LOG삭제] 건너뜀 (이름이 올바르지 않음) : " + strTable + " / " + strKey);
                    continue;
                }

                long nDel = 0;
                while (!m_bManualStop)
                {
                    string strDel = " DELETE FROM " + strTable
                                  + "  WHERE ctid IN (SELECT ctid FROM " + strTable
                                  + "                  WHERE WH_TYP = '" + strWH_TYP + "'"
                                  + "                    AND " + strKey + " <= " + DbLang.SYSDATE + "::TIMESTAMP - '" + nCycle.ToString() + " " + strUnit + "'::INTERVAL"
                                  + "                  LIMIT " + DEL_CHUNK.ToString() + ")";
                    _pBdb.BeginTrans();
                    int n = _pBdb.ExcuteNonQry(strDel);
                    if (n < 0)
                    {
                        string strErr = _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        MakeMsg_Error("[LOG삭제] 삭제 실패 [" + strTable + "] : " + strErr);
                        break;
                    }
                    _pBdb.Commit();
                    nDel += n;
                    if (n < DEL_CHUNK) break;
                    Thread.Sleep(100);
                }

                nTotal += nDel;
                string strLog = "[LOG삭제] " + strTable + " : " + nCycle.ToString() + " " + strUnit + " 이상 경과한 자료 " + nDel.ToString() + "건을 삭제했습니다.";
                if (nDel > 0)
                {
                    MakeMsg_Imp(strLog);
                    try { InsertLog(strWH_TYP, strLog); } catch { }
                }
                else
                {
                    MakeMsg(strLog);
                }
            }

            MakeMsg_Imp("[LOG삭제] 삭제를 마쳤습니다. 합계 " + nTotal.ToString() + "건");
            return true;
        }
        #endregion
    }
}
