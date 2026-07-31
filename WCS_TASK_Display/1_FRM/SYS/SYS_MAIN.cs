using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Threading;
using System.Windows.Forms;
using Npgsql;

namespace WCS_TASK_Display
{
    public partial class SYS_MAIN : Form
    {
        #region members
        private maindefine m_mfgClass = new maindefine();

        public DisplayThread[] m_thDisplay = new DisplayThread[200];
        public cLogThread[] m_thLogging = new cLogThread[200];

        // per-controller configuration (parallel arrays, loaded from WCS_DB.INI [COMM*])
        public string[] m_strEQMT_TYP = new string[200];
        public string[] m_strPLC_NO = new string[200];
        public string[] m_strCOMM_IP = new string[200];
        public int[] m_nCOMM_CUR_PORT = new int[200];
        public int[] m_nCOMM_FROM_PORT = new int[200];
        public int[] m_nCOMM_TO_PORT = new int[200];
        public int[] m_nCOMM_PORT_CNT = new int[200];
        public int[] m_nCOMM_CNT = new int[200];
        public int[] m_nFrTrackNo = new int[200];
        public int[] m_nToTrackNo = new int[200];
        public string[] m_strLogPath = new string[200];
        public string[] m_strLogFileNm = new string[200];

        public string m_strConnectString;
        private string m_strRtnMsg;
        private int m_nProcessCnt;

        private bool m_bHex = true;
        private bool m_bAscii = false;
        public bool IsHex { get { return m_bHex; } set { m_bHex = value; } }
        public bool IsAscii { get { return m_bAscii; } set { m_bAscii = value; } }

        private const int MSG_MAX = 500;
        #endregion

        public SYS_MAIN()
        {
            InitializeComponent();
        }

        #region Load
        private void SYS_MAIN_Load(object sender, EventArgs e)
        {
            m_nProcessCnt = 0;

            // single-instance guard
            if (cCmLib.GfPrevInstance() == true)
            {
                cDefApp.GM_RE_START = true;
                Application.Exit();
                return;
            }

            this.IsHex = chkHex.Checked;
            this.IsAscii = chkAscii.Checked;

#if ORACLE
            cDefApi.GsGetInitPorFileDB_1(ref cDefApp.GM_DB1_PROVIDER, ref cDefApp.GM_DB1_ALIAS, ref cDefApp.GM_DB1_USERID, ref cDefApp.GM_DB1_PASSWORD, ref m_strRtnMsg);
            m_strConnectString = "Provider=" + cDefApp.GM_DB1_PROVIDER + "; Data Source=" + cDefApp.GM_DB1_ALIAS + "; User ID=" + cDefApp.GM_DB1_USERID + "; Password =" + cDefApp.GM_DB1_PASSWORD;
#endif
#if POSTGRESQL
            cDefApi.GsGetInitPorFileDB_2(ref cDefApp.GM_DB2_IP, ref cDefApp.GM_DB2_DATABASE, ref cDefApp.GM_DB2_PORT, ref cDefApp.GM_DB2_USER, ref cDefApp.GM_DB2_USER_PW, ref m_strRtnMsg);
            m_strConnectString = "host=" + cDefApp.GM_DB2_IP + ";username=" + cDefApp.GM_DB2_USER + ";password=" + cDefApp.GM_DB2_USER_PW + ";database=" + cDefApp.GM_DB2_DATABASE + ";MAXPOOLSIZE=50;";
#endif

            cDefApi.GsGetInitPorFileCNF(ref cDefApp.GM_WH_TYP, ref cDefApp.GM_USERID, ref m_strRtnMsg);
            cDefApi.GsReadInitProfileProcessCnt("PROCESS", ref m_nProcessCnt, ref m_strRtnMsg);

            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {
                string Name = "COMM" + ii.ToString();

                cDefApi.GsReadInitProfileCom(Name,
                                             ref m_strPLC_NO[ii],
                                             ref m_strCOMM_IP[ii],
                                             ref m_nCOMM_CUR_PORT[ii],
                                             ref m_nCOMM_FROM_PORT[ii],
                                             ref m_nCOMM_TO_PORT[ii],
                                             ref m_nCOMM_PORT_CNT[ii],
                                             ref m_nCOMM_CNT[ii],
                                             ref ii,
                                             ref m_nFrTrackNo[ii],
                                             ref m_nToTrackNo[ii],
                                             ref m_strLogPath[ii],
                                             ref m_strLogFileNm[ii],
                                             ref m_strEQMT_TYP[ii],
                                             ref m_strRtnMsg);

                if (m_strPLC_NO[ii] == null || m_strPLC_NO[ii] == "")
                {
                    m_strPLC_NO[ii] = null;
                    break;
                }

                // per-thread async log queue + writer thread
                cDefApp.m_LogQ[ii] = new Queue<LogParam>();
                m_thLogging[ii] = new cLogThread(m_strLogPath[ii], m_strLogFileNm[ii], ii);

                CreateStatusBox(ii);
                SetDisplay("picDspDbCn" + ii, "D");
                SetDisplay("picDspSkt" + ii, "D", "E");
            }

            cDefApi.GsReadInitProfileDelay("SND", ref cDefApp.GM_COMM_SND_TIME_OUT, ref m_strRtnMsg);
            cDefApi.GsReadInitProfileDelay("RCV", ref cDefApp.GM_COMM_RCV_TIME_OUT, ref m_strRtnMsg);

            FillManualCombos();

            cDefApp.GM_STAT_MAIN = true;
            WrkThStart();
        }
        #endregion

        #region status boxes (BackColor based, replaces image-list)
        private void CreateStatusBox(int ii)
        {
            int x = 8 + (ii * 70);
            Label lbl = new Label();
            lbl.AutoSize = false;
            lbl.Text = "#" + ii.ToString("00");
            lbl.Location = new Point(x, 22);
            lbl.Size = new Size(60, 14);
            pnlTop.Controls.Add(lbl);

            PictureBox picDb = new PictureBox();
            picDb.Name = "picDspDbCn" + ii;
            picDb.BorderStyle = BorderStyle.FixedSingle;
            picDb.Location = new Point(x, 38);
            picDb.Size = new Size(26, 26);
            picDb.Tag = "";
            pnlTop.Controls.Add(picDb);
            ToolTip.SetToolTip(picDb, "DB Status #" + ii.ToString("00"));

            PictureBox picSk = new PictureBox();
            picSk.Name = "picDspSkt" + ii;
            picSk.BorderStyle = BorderStyle.FixedSingle;
            picSk.Location = new Point(x + 30, 38);
            picSk.Size = new Size(26, 26);
            picSk.Tag = "";
            pnlTop.Controls.Add(picSk);
            ToolTip.SetToolTip(picSk, "Socket Status #" + ii.ToString("00"));
        }

        private PictureBox FindPic(string ctrName)
        {
            string msg = null;
            Panel p = pnlTop;
            Control ctrl = m_mfgClass.PfCtlFind(ref p, ctrName, ref msg);
            return ctrl as PictureBox;
        }

        private void SetDisplay(string ctrName, params string[] opt)
        {
            PictureBox pic = FindPic(ctrName);
            if (pic == null) return;
            if (opt.Length == 1) PfSetStatImgView(pic, opt[0]);
            else PfSetStatImgView(pic, opt[0], opt[1]);
        }

        // C:Connected, T:Trying, D:Disconnected
        private void PfSetStatImgView(PictureBox pPic, string pStat)
        {
            try
            {
                switch (pStat)
                {
                    case "C": pPic.BackColor = Color.LimeGreen; break;
                    case "T": pPic.BackColor = Color.Gold; break;
                    case "D": pPic.BackColor = Color.Red; break;
                    default: pPic.BackColor = Color.Gray; break;
                }
                pPic.Tag = pStat;
            }
            catch { }
        }

        // Stat Connection + Operation (N:normal, W:writing, E:error)
        private void PfSetStatImgView(PictureBox pPic, string pStatSkt, string pStatOp)
        {
            PfSetStatImgView(pPic, pStatSkt);
        }
        #endregion

        #region worker threads
        private void WrkThStart()
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread_Timer.Enabled = true;

            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {
                if (m_strPLC_NO[ii] == null) continue;

                m_thDisplay[ii] = new DisplayThread(ii,
                                                    cDefApp.GM_WH_TYP,
                                                    m_strEQMT_TYP[ii],
                                                    m_strPLC_NO[ii],
                                                    m_strCOMM_IP[ii],
                                                    m_nCOMM_CUR_PORT[ii],
                                                    m_nCOMM_FROM_PORT[ii],
                                                    m_nCOMM_TO_PORT[ii],
                                                    m_nCOMM_PORT_CNT[ii],
                                                    m_nCOMM_CNT[ii],
                                                    m_nFrTrackNo[ii],
                                                    m_nToTrackNo[ii],
                                                    m_strConnectString,
                                                    m_strLogFileNm[ii]);
            }
        }

        private void Thread_Tick(object sender, EventArgs e)
        {
            try
            {
                Thread_Timer.Enabled = false;

                for (int ii = 0; ii < m_nProcessCnt; ii++)
                {
                    if (m_thDisplay[ii] == null) continue;

                    // start (or restart) the worker + its log thread when not running
                    if (m_thDisplay[ii].m_thThread == null)
                    {
                        SetDisplay("picDspSkt" + ii, "T");
                        SetDisplay("picDspDbCn" + ii, "T");

                        m_thDisplay[ii].m_thThread = new Thread(m_thDisplay[ii].Thread_Doing);
                        m_thDisplay[ii].m_thThread.IsBackground = true;
                        m_thDisplay[ii].m_frmMain = this;
                        m_thDisplay[ii].m_thThread.Start(ii);

                        if (m_thLogging[ii] != null && m_thLogging[ii].m_thThread == null)
                        {
                            m_thLogging[ii].m_frmMain = this;
                            m_thLogging[ii].m_thThread = new Thread(m_thLogging[ii].LogQueThread);
                            m_thLogging[ii].m_thThread.IsBackground = true;
                            m_thLogging[ii].m_thThread.Start();
                        }

                        Thread.Sleep(100);
                    }
                    else
                    {
                        if (m_thDisplay[ii].IsOpen)
                        {
                            SetDisplay("picDspSkt" + ii, "C");
                            SetDisplay("picDspDbCn" + ii, "C");
                        }
                    }
                }

                Thread_Timer.Enabled = true;
            }
            catch
            {
                Thread_Timer.Enabled = true;
            }
        }
        #endregion

        #region message view (ListView), thread-safe via CheckForIllegalCrossThreadCalls=false
        public void PsMsgView(string pMsg, int nThGbn) { AddMsg("NOR", "", pMsg); }
        public void PsMsgView_Error(string pMsg, int nThGbn) { AddMsg("ERR", "", pMsg); }
        public void PsMsgView_IMP(string pMsg, int nThGbn) { AddMsg("IMP", "", pMsg); }

        public void PsMsgView(string pMsg, string pObjID, int nThGbn) { AddMsg("NOR", pObjID, pMsg); }
        public void PsMsgView_Error(string pMsg, string pObjID, int nThGbn) { AddMsg("ERR", pObjID, pMsg); }
        public void PsMsgView_IMP(string pMsg, string pObjID, int nThGbn) { AddMsg("IMP", pObjID, pMsg); }

        private void AddMsg(string strType, string strId, string strMsg)
        {
            try
            {
                ListViewItem item = new ListViewItem(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                item.SubItems.Add(strType);
                item.SubItems.Add(strId);
                item.SubItems.Add(strMsg);
                if (strType == "ERR") item.ForeColor = Color.Red;
                else if (strType == "IMP") item.ForeColor = Color.Blue;

                lvMsg.Items.Add(item);

                while (lvMsg.Items.Count > MSG_MAX) lvMsg.Items.RemoveAt(0);
                item.EnsureVisible();
            }
            catch { }
        }
        #endregion

        #region manual control (Client manual command -> DISPLAY_DATA)
        private void FillManualCombos()
        {
            cmbController.Items.Clear();
            int maxDsp = 1;
            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {
                if (m_strPLC_NO[ii] == null) continue;
                cmbController.Items.Add(m_strPLC_NO[ii]);
                if (m_nCOMM_CNT[ii] > maxDsp) maxDsp = m_nCOMM_CNT[ii];
            }
            if (cmbController.Items.Count > 0) cmbController.SelectedIndex = 0;

            cmbDspNo.Items.Clear();
            if (maxDsp < 1) maxDsp = 1;
            for (int d = 1; d <= maxDsp; d++) cmbDspNo.Items.Add(d.ToString());
            if (cmbDspNo.Items.Count > 0) cmbDspNo.SelectedIndex = 0;

            cmbColor.Items.Clear();
            cmbColor.Items.Add("RED(4)");
            cmbColor.Items.Add("GREEN(5)");
            cmbColor.Items.Add("YELLOW(6)");
            cmbColor.SelectedIndex = 2; // Yellow default (legacy)
        }

        private int SelectedColor()
        {
            // RED=4, GREEN=5, YELLOW=6
            return cmbColor.SelectedIndex < 0 ? 6 : (DisplayProtocol.COLOR_RED + cmbColor.SelectedIndex);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            WriteManualCmd("DATA", txtData.Text);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            WriteManualCmd("CLEAR", "");
        }

        private void WriteManualCmd(string strCmd, string strData)
        {
            if (cmbController.SelectedItem == null || cmbDspNo.SelectedItem == null)
            {
                MessageBox.Show("Select controller and display number.");
                return;
            }

            string strPlc = cmbController.SelectedItem.ToString();
            string strDsp = cmbDspNo.SelectedItem.ToString();
            int nColor = SelectedColor();

#if POSTGRESQL
            NpgsqlConnection cn = null;
            try
            {
                cn = new NpgsqlConnection(m_strConnectString);
                cn.Open();
                cDbPostUse db = new cDbPostUse(cn, false);

                string CRLF = "\r\n";
                string strSql = "";
                strSql += CRLF + "UPDATE DISPLAY_DATA              ";
                strSql += CRLF + "   SET CMD_RQ_YN  = 'Y'          ";
                strSql += CRLF + "      ,CMD_RQ_ID  = :CMD_RQ_ID    ";
                strSql += CRLF + "      ,CMD_DATA   = :CMD_DATA     ";
                strSql += CRLF + "      ,CMD_COLOR  = :CMD_COLOR    ";
                strSql += CRLF + "      ,UPD_DT     = " + DbLang.SYSDATE + "";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP          ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO          ";
                strSql += CRLF + "AND    DSP_NO = :DSP_NO          ";

                db.mComMain.CommandType = CommandType.Text;
                db.mComMain.Parameters.Clear();
                db.mComMain.Parameters.Add("CMD_RQ_ID", DbLang.VARCHAR, 255).Value = strCmd;
                db.mComMain.Parameters.Add("CMD_DATA", DbLang.VARCHAR, 255).Value = strData;
                db.mComMain.Parameters.Add("CMD_COLOR", DbLang.VARCHAR, 255).Value = nColor.ToString();
                db.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = cDefApp.GM_WH_TYP;
                db.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = strPlc;
                db.mComMain.Parameters.Add("DSP_NO", DbLang.VARCHAR, 255).Value = strDsp;

                int n = db.ExcuteNonQry(strSql);
                if (n <= 0)
                    AddMsg("ERR", strPlc, "Manual " + strCmd + " : no DISPLAY_DATA row (DSP_NO=" + strDsp + "). Check table.");
                else
                    AddMsg("IMP", strPlc, "Manual " + strCmd + " requested DSP_NO=" + strDsp + " DATA[" + strData + "] COLOR=" + nColor);
            }
            catch (Exception ex)
            {
                AddMsg("ERR", strPlc, "Manual command DB error: " + ex.Message);
            }
            finally
            {
                if (cn != null) { try { cn.Close(); } catch { } }
            }
#endif
        }
        #endregion

        #region hex / ascii toggles
        private void chkHex_CheckedChanged(object sender, EventArgs e) { IsHex = chkHex.Checked; }
        private void chkAscii_CheckedChanged(object sender, EventArgs e) { IsAscii = chkAscii.Checked; }
        #endregion
    }
}
