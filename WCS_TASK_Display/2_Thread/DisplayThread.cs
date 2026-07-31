using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data;

namespace WCS_TASK_Display
{
    // In-memory per-display state (change detection + color cycling), the C# DB-centric
    // equivalent of the legacy CDisplayInfo (which cached the last luggage shown).
    public class DispState
    {
        public string LAST_LUGG = "";   // last luggage number that was sent
        public int COLOR_IDX = 2;       // 0=Red,1=Green,2=Yellow  (start Yellow, like legacy)
    }

    // Worker thread for ONE display controller. Architecturally identical to CvThread:
    //   - owns a DisplayProtocol (socket + DB) object
    //   - connects to the board with port cycling
    //   - runs a 200ms poll loop over the controller's displays
    //   - AUTO : reads DISPLAY_DATA, on luggage change sends the content to the board
    //   - MANUAL: reads DISPLAY_DATA rows with CMD_RQ_YN='Y' (written by Client) and sends them
    //   - reflects connection status into EQP_MST, writes audit rows to WCS_LOG_PGR
    public class DisplayThread : maindefine
    {
        #region members
        private string m_strWh_typ;
        private string m_strEqmt_typ;   // "DISPLAY"
        private string m_strPlc_No;     // controller id
        private string m_strIp;
        private int m_nCurPort;
        private int m_nFromPort;
        private int m_nToPort;
        private int m_nPortCnt;
        public int m_nCnt;              // number of displays on this controller
        public int m_nFrTrackNo;
        public int m_nToTrackNo;
        public int m_nthNo;
        public string m_strRtnMsg;
        public string m_strLogFileNm;
        public bool m_blConnectYn = false;

        private string m_strConnectString;
        private DisplayProtocol m_dsp;
        public Thread m_thThread;
        public SYS_MAIN m_frmMain;
        private bool m_bOpen;
        public bool IsOpen { get { return m_bOpen; } set { m_bOpen = value; } }

        // per-display state keyed by DSP_NO
        private Dictionary<int, DispState> DspDic = new Dictionary<int, DispState>();

        string strSql = "";
        string CRLF = "\r\n";
        int nSelCnt = 0;
        private string _strErrorMsg = "";
        #endregion

        #region ctor
        public DisplayThread(int nThNo,
                             string strWh_typ,
                             string Eqmt_typ,
                             string Plc_No,
                             string Ip,
                             int CurPort,
                             int FromPort,
                             int ToPort,
                             int PortCnt,
                             int Cnt,
                             int FrTrackNo,
                             int ToTrackNo,
                             string ConnectString,
                             string strLogFileNm)
        {
            m_nthNo = nThNo;
            m_strWh_typ = strWh_typ;
            m_strEqmt_typ = Eqmt_typ;
            m_strPlc_No = Plc_No;
            m_strIp = Ip;
            m_nCurPort = CurPort;
            m_nFromPort = FromPort;
            m_nToPort = ToPort;
            m_nPortCnt = PortCnt;
            m_nCnt = Cnt;
            m_nFrTrackNo = FrTrackNo;
            m_nToTrackNo = ToTrackNo;
            m_strConnectString = ConnectString;
            m_strLogFileNm = strLogFileNm;
            IsOpen = false;

            m_dsp = new DisplayProtocol(m_strConnectString);
            m_dsp.IsHex = true;
        }
        #endregion

        #region UI message helpers (delegate to the form, same contract as CvThread)
        private void MakeMsg(string msg, int nThGbn)
        {
            try { m_frmMain.PsMsgView(msg, m_strPlc_No.ToString(), nThGbn); }
            catch { return; }
        }
        private void MakeMsg_Error(string msg, int nThGbn)
        {
            try
            {
                m_frmMain.PsMsgView_Error(msg, m_strPlc_No.ToString(), nThGbn);
                cDefApp.m_LogQ[m_nthNo].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch { return; }
        }
        private void MakeMsg_Imp(string msg, int nThGbn)
        {
            try
            {
                m_frmMain.PsMsgView_IMP(msg, m_strPlc_No.ToString(), nThGbn);
                cDefApp.m_LogQ[m_nthNo].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch { return; }
        }
        public void SetErrorMsg(string strMsg) { _strErrorMsg = strMsg; Log.Error(_strErrorMsg); }
        #endregion

        #region Thread_Doing
        public void Thread_Doing(object value)
        {
            try
            {
                if (cDefApp.GM_STAT_MAIN == false)
                    throw new Exception("Main system not running");

                MakeMsg_Imp("DB/Socket Connecting", m_nthNo);

                // ---- connect with port cycling (identical strategy to CvThread) ----
                if (m_dsp.m_bSocCon == false && m_dsp.m_bDBOpen == false)
                {
                    for (int i = 0; i < m_nToPort - m_nFromPort; i++)
                    {
                        if (m_nCurPort > m_nToPort) m_nCurPort = m_nFromPort;

                        for (int j = 0; j < m_nPortCnt; j++)
                        {
                            MakeMsg_Imp(string.Format("IP [{0}] PORT [{1}] connect try", m_strIp, m_nCurPort), m_nthNo);
                            m_dsp.SetConfig(m_strIp, m_nCurPort, 2);

                            if (!m_dsp.Open(ref m_strRtnMsg))
                            {
                                SetErrorMsg("Comm" + m_nthNo + " :" + m_strRtnMsg);
                                MakeMsg_Error(m_strRtnMsg, m_nthNo);

                                if (m_dsp.m_bSocCon == false && m_dsp.m_bDBOpen == true)
                                    InsertWcsLogPgr("", "[Thread_Doing] socket connect fail");

                                m_dsp.Close(ref m_strRtnMsg);

                                if (j == m_nPortCnt - 1) m_nCurPort = m_nCurPort + 1;
                                m_blConnectYn = false;
                                Thread.Sleep(500);
                                continue;
                            }
                            else
                            {
                                string strCOMM = "COMM" + m_nthNo;
                                cDefApi.WritePrivateProfileString(strCOMM, "CUR_PORT", Convert.ToString("" + m_nCurPort), cDefApp.GM_ENV_INI);
                                InsertWcsLogPgr("", "[Thread_Doing] DISPLAY PLC_NO : " + m_strPlc_No + ", PORT : " + m_nCurPort + " connected");
                                m_blConnectYn = true;
                                break;
                            }
                        }
                        if (m_blConnectYn == true) break;
                    }
                }

                // ---- main poll loop ----
                if (m_dsp.m_bSocCon == true && m_dsp.m_bDBOpen == true)
                {
                    IsOpen = true;
                    MakeMsg_Imp("DB login Ok!", m_nthNo);
                    Communication("Y", m_strWh_typ, m_strEqmt_typ, m_strPlc_No);

                    while (true)
                    {
                        if (cDefApp.GM_STAT_MAIN == false) goto EXIT_LBL;

                        m_dsp.IsAscii = m_frmMain.IsAscii;
                        m_dsp.IsHex = m_frmMain.IsHex;

                        if (!DspManual()) goto EXIT_LBL; // manual (Client) command first
                        if (!DspAuto()) goto EXIT_LBL;   // auto track-change display

                        Thread.Sleep(200);
                    }
                }

            EXIT_LBL:
                SetErrorMsg("Comm" + m_nthNo + " DB & Socket logoff!");
                MakeMsg_Imp("DB & Socket logoff!", m_nthNo);
            }
            catch (Exception ex)
            {
                MakeMsg_Error(ex.Message, m_nthNo);
            }

            IsOpen = false;
            Communication("N", m_strWh_typ, m_strEqmt_typ, m_strPlc_No);
            m_dsp.Close(ref m_strRtnMsg);
            MakeMsg_Imp(m_strRtnMsg, m_nthNo);
            m_thThread = null;
        }
        #endregion

        #region color cycle
        private byte NextColor(DispState st)
        {
            st.COLOR_IDX = (st.COLOR_IDX + 1) % 3;          // Red -> Green -> Yellow -> ...
            return (byte)(DisplayProtocol.COLOR_RED + st.COLOR_IDX);
        }
        private DispState GetState(int nDspNo)
        {
            if (!DspDic.ContainsKey(nDspNo)) DspDic.Add(nDspNo, new DispState());
            return DspDic[nDspNo];
        }
        #endregion

        #region DspAuto :: read DISPLAY_DATA, send on luggage change
        private bool DspAuto()
        {
            string strTitle = "[DspAuto]";
            try
            {
                strSql = "";
                strSql += CRLF + "SELECT DSP_NO                          ";
                strSql += CRLF + "      ,COALESCE(DISP_DATA,'') AS DISP_DATA ";
                strSql += CRLF + "      ,COALESCE(LUGG_NO,'')   AS LUGG_NO   ";
                strSql += CRLF + "      ,COALESCE(COLOR,0)      AS COLOR     ";
                strSql += CRLF + "FROM   DISPLAY_DATA                    ";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP                ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO                ";
                strSql += CRLF + "AND    COALESCE(CMD_RQ_YN,'N') <> 'Y'  ";
                strSql += CRLF + "ORDER  BY DSP_NO                       ";

                m_dsp._pBdb.mComMain.CommandType = CommandType.Text;
                m_dsp._pBdb.mComMain.Parameters.Clear();
                m_dsp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_dsp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                nSelCnt = m_dsp._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    MakeMsg_Error(strTitle + " DISPLAY_DATA select error [" + m_dsp._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }
                if (nSelCnt == 0) return true; // nothing configured for this controller

                DataTable dt = m_dsp._pBdb.mDtMain;
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    int nDspNo = Convert.ToInt32("0" + dt.Rows[r]["DSP_NO"].ToString());
                    string strData = dt.Rows[r]["DISP_DATA"].ToString();
                    string strLugg = dt.Rows[r]["LUGG_NO"].ToString();
                    int nColor = Convert.ToInt32("0" + dt.Rows[r]["COLOR"].ToString());

                    DispState st = GetState(nDspNo);
                    if (st.LAST_LUGG == strLugg) continue; // no change -> skip

                    byte byColor;
                    string strSend;
                    if (strLugg == "" || strLugg == "0")
                    {
                        strSend = "";                 // empty track -> 8 spaces
                        byColor = DisplayProtocol.COLOR_YELLOW;
                    }
                    else
                    {
                        strSend = strData;
                        byColor = (nColor >= DisplayProtocol.COLOR_RED && nColor <= DisplayProtocol.COLOR_YELLOW)
                                  ? (byte)nColor : NextColor(st);
                    }

                    string msg = "";
                    if (m_dsp.SendDisplay(nDspNo - 1, byColor, strSend, ref msg))
                    {
                        st.LAST_LUGG = strLugg;
                        UpdateSentStatus(nDspNo, DisplayProtocol.FitProduct(strSend), strLugg);
                        MakeMsg_Imp(string.Format("DSP[{0}] LUGG[{1}] DATA[{2}] sent", nDspNo, strLugg, strSend), m_nthNo);
                    }
                    else
                    {
                        MakeMsg_Error(string.Format("DSP[{0}] send fail [{1}]", nDspNo, msg), m_nthNo);
                        return false; // comm broken -> reconnect
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + " Exception [" + ex.Message + "]", m_nthNo);
                return false;
            }
        }
        #endregion

        #region DspManual :: read DISPLAY_DATA CMD_RQ_YN='Y' (Client manual command)
        private bool DspManual()
        {
            string strTitle = "[DspManual]";
            try
            {
                strSql = "";
                strSql += CRLF + "SELECT DSP_NO                              ";
                strSql += CRLF + "      ,COALESCE(CMD_RQ_ID,'DATA') AS CMD_RQ_ID ";
                strSql += CRLF + "      ,COALESCE(CMD_DATA,'')      AS CMD_DATA  ";
                strSql += CRLF + "      ,COALESCE(CMD_COLOR,6)      AS CMD_COLOR ";
                strSql += CRLF + "FROM   DISPLAY_DATA                        ";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP                    ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO                    ";
                strSql += CRLF + "AND    CMD_RQ_YN = 'Y'                     ";
                strSql += CRLF + "ORDER  BY DSP_NO                           ";

                m_dsp._pBdb.mComMain.CommandType = CommandType.Text;
                m_dsp._pBdb.mComMain.Parameters.Clear();
                m_dsp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_dsp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                nSelCnt = m_dsp._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    MakeMsg_Error(strTitle + " DISPLAY_DATA cmd select error [" + m_dsp._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }
                if (nSelCnt == 0) return true;

                DataTable dt = m_dsp._pBdb.mDtMain;
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    int nDspNo = Convert.ToInt32("0" + dt.Rows[r]["DSP_NO"].ToString());
                    string strCmd = dt.Rows[r]["CMD_RQ_ID"].ToString().ToUpper();
                    string strData = dt.Rows[r]["CMD_DATA"].ToString();
                    int nColor = Convert.ToInt32("0" + dt.Rows[r]["CMD_COLOR"].ToString());

                    if (strCmd == "CLEAR") strData = "";
                    byte byColor = (nColor >= DisplayProtocol.COLOR_RED && nColor <= DisplayProtocol.COLOR_YELLOW)
                                   ? (byte)nColor : DisplayProtocol.COLOR_YELLOW;

                    string msg = "";
                    if (m_dsp.SendDisplay(nDspNo - 1, byColor, strData, ref msg))
                    {
                        ClearManualCmd(nDspNo, DisplayProtocol.FitProduct(strData), byColor);
                        GetState(nDspNo); // ensure state exists
                        MakeMsg_Imp(string.Format("DSP[{0}] MANUAL[{1}] DATA[{2}] sent", nDspNo, strCmd, strData), m_nthNo);
                    }
                    else
                    {
                        MakeMsg_Error(string.Format("DSP[{0}] manual send fail [{1}]", nDspNo, msg), m_nthNo);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + " Exception [" + ex.Message + "]", m_nthNo);
                return false;
            }
        }
        #endregion

        #region DB updates
        private bool UpdateSentStatus(int nDspNo, string strSentData, string strLugg)
        {
            try
            {
                m_dsp._pBdb.BeginTrans();
                strSql = "";
                strSql += CRLF + "UPDATE DISPLAY_DATA                          ";
                strSql += CRLF + "   SET SEND_YN        = 'Y'                   ";
                strSql += CRLF + "      ,LAST_SENT_DATA = :LAST_SENT_DATA       ";
                strSql += CRLF + "      ,LAST_SENT_LUGG = :LAST_SENT_LUGG       ";
                strSql += CRLF + "      ,SEND_DT        = " + DbLang.SYSDATE + "";
                strSql += CRLF + "      ,UPD_DT         = " + DbLang.SYSDATE + "";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP                       ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO                       ";
                strSql += CRLF + "AND    DSP_NO = :DSP_NO                       ";

                m_dsp._pBdb.mComMain.CommandType = CommandType.Text;
                m_dsp._pBdb.mComMain.Parameters.Clear();
                m_dsp._pBdb.mComMain.Parameters.Add("LAST_SENT_DATA", DbLang.VARCHAR, 255).Value = strSentData;
                m_dsp._pBdb.mComMain.Parameters.Add("LAST_SENT_LUGG", DbLang.VARCHAR, 255).Value = strLugg;
                m_dsp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_dsp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_dsp._pBdb.mComMain.Parameters.Add("DSP_NO", DbLang.VARCHAR, 255).Value = nDspNo.ToString();
                nSelCnt = m_dsp._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0) { m_dsp._pBdb.Rollback(); return false; }
                m_dsp._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_dsp._pBdb.Rollback();
                SetErrorMsg("[UpdateSentStatus] " + ex.Message);
                return false;
            }
        }

        private bool ClearManualCmd(int nDspNo, string strData, int nColor)
        {
            try
            {
                m_dsp._pBdb.BeginTrans();
                strSql = "";
                strSql += CRLF + "UPDATE DISPLAY_DATA                          ";
                strSql += CRLF + "   SET CMD_RQ_YN      = 'N'                   ";
                strSql += CRLF + "      ,DISP_DATA      = :DISP_DATA            ";
                strSql += CRLF + "      ,COLOR          = :COLOR                ";
                strSql += CRLF + "      ,SEND_YN        = 'Y'                   ";
                strSql += CRLF + "      ,LAST_SENT_DATA = :DISP_DATA            ";
                strSql += CRLF + "      ,SEND_DT        = " + DbLang.SYSDATE + "";
                strSql += CRLF + "      ,UPD_DT         = " + DbLang.SYSDATE + "";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP                       ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO                       ";
                strSql += CRLF + "AND    DSP_NO = :DSP_NO                       ";

                m_dsp._pBdb.mComMain.CommandType = CommandType.Text;
                m_dsp._pBdb.mComMain.Parameters.Clear();
                m_dsp._pBdb.mComMain.Parameters.Add("DISP_DATA", DbLang.VARCHAR, 255).Value = strData;
                m_dsp._pBdb.mComMain.Parameters.Add("COLOR", DbLang.VARCHAR, 255).Value = nColor.ToString();
                m_dsp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_dsp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_dsp._pBdb.mComMain.Parameters.Add("DSP_NO", DbLang.VARCHAR, 255).Value = nDspNo.ToString();
                nSelCnt = m_dsp._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0) { m_dsp._pBdb.Rollback(); return false; }
                m_dsp._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_dsp._pBdb.Rollback();
                SetErrorMsg("[ClearManualCmd] " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Communication :: EQP_MST CONNECTED_YN  (same as CvThread)
        public bool Communication(string CONNECTED_YN, string WH_TYP, string EQP_TYP, string PLC_NO)
        {
            string strTitle = "[Communication]";
            try
            {
                m_dsp._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "UPDATE EQP_MST                                    ";
                strSql += CRLF + "   SET CONNECTED_YN      = :CONNECTED_YN          ";
                strSql += CRLF + "      ,UPD_DT            = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      ,PLC_PORT          = :PLC_PORT              ";
                strSql += CRLF + "WHERE  WH_TYP            = :WH_TYP                ";
                strSql += CRLF + "AND    EQP_TYP           = :EQP_TYP               ";
                strSql += CRLF + "AND    PLC_NO            = :PLC_NO                ";

                m_dsp._pBdb.mComMain.CommandType = CommandType.Text;
                m_dsp._pBdb.mComMain.Parameters.Clear();
                m_dsp._pBdb.mComMain.Parameters.Add("CONNECTED_YN", DbLang.VARCHAR).Value = CONNECTED_YN;
                m_dsp._pBdb.mComMain.Parameters.Add("PLC_PORT", DbLang.VARCHAR, 255).Value = Convert.ToString("" + m_nCurPort);
                m_dsp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = WH_TYP;
                m_dsp._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = EQP_TYP;
                m_dsp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = PLC_NO;
                nSelCnt = m_dsp._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    m_dsp._pBdb.Rollback();
                    MakeMsg_Error(strTitle + " EQP_MST update error [" + m_dsp._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }
                m_dsp._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                try { m_dsp._pBdb.Rollback(); }
                catch { }
                SetErrorMsg(strTitle + " " + ex.Message);
                return false;
            }
        }
        #endregion

        #region InsertWcsLogPgr :: WCS_LOG_PGR audit  (same columns as CvThread)
        public bool InsertWcsLogPgr(string strTRACK_NO, string strLOG_MSG)
        {
            try
            {
                m_dsp._pBdb.BeginTrans();

                strSql = "";
                strSql += cDefApp.CRLF + "INSERT INTO WCS_LOG_PGR (WH_TYP    ";
                strSql += cDefApp.CRLF + "                        ,INS_DT    ";
                strSql += cDefApp.CRLF + "                        ,LOG_SEQ   ";
                strSql += cDefApp.CRLF + "                        ,LUGG_NO   ";
                strSql += cDefApp.CRLF + "                        ,BCR_BOTTOM";
                strSql += cDefApp.CRLF + "                        ,BCR_TOP   ";
                strSql += cDefApp.CRLF + "                        ,PGR_NM    ";
                strSql += cDefApp.CRLF + "                        ,LOG_KOR   ";
                strSql += cDefApp.CRLF + "                        ,TRACK_FROM";
                strSql += cDefApp.CRLF + "                        ,TRACK_TO  ";
                strSql += cDefApp.CRLF + "                        ,JOB_STA   ";
                strSql += cDefApp.CRLF + "                        ,RQ_INS_ID ";
                strSql += cDefApp.CRLF + "                        ,RQ_INS_DT ";
                strSql += cDefApp.CRLF + "                        ,EQP_TYP ) ";
                strSql += cDefApp.CRLF + "    VALUES (:WH_TYP                ";
                strSql += cDefApp.CRLF + "           ," + DbLang.SYSDATE + " ";
                strSql += cDefApp.CRLF + "           ,NEXTVAL('LOG_SEQ')     ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,:PGR_NM                ";
                strSql += cDefApp.CRLF + "           ,:LOG_KOR               ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,:JOB_STA               ";
                strSql += cDefApp.CRLF + "           ,:RQ_INS_ID             ";
                strSql += cDefApp.CRLF + "           ," + DbLang.SYSDATE + " ";
                strSql += cDefApp.CRLF + "           ,:EQP_TYP )             ";

                m_dsp._pBdb.mComMain.CommandType = CommandType.Text;
                m_dsp._pBdb.mComMain.Parameters.Clear();
                m_dsp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_dsp._pBdb.mComMain.Parameters.Add("PGR_NM", DbLang.VARCHAR, 255).Value = m_strLogFileNm;
                m_dsp._pBdb.mComMain.Parameters.Add("LOG_KOR", DbLang.VARCHAR, 255).Value = strLOG_MSG;
                m_dsp._pBdb.mComMain.Parameters.Add("JOB_STA", DbLang.VARCHAR, 255).Value = "999";
                m_dsp._pBdb.mComMain.Parameters.Add("RQ_INS_ID", DbLang.VARCHAR, 255).Value = strTRACK_NO;
                m_dsp._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = m_strEqmt_typ;
                nSelCnt = m_dsp._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_dsp._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nthNo + " :[InsertWcsLogPgr] error [" + m_dsp._pBdb.ErrMsg + "]");
                    return false;
                }
                m_dsp._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                try { m_dsp._pBdb.Rollback(); }
                catch { }
                SetErrorMsg("Comm" + m_nthNo + " :[InsertWcsLogPgr] " + ex.ToString());
                return false;
            }
        }
        #endregion
    }
}
