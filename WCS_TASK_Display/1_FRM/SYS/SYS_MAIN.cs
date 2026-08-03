using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Npgsql;

namespace WCS_TASK_Display
{
    public partial class SYS_MAIN : Form
    {
        #region 멤버
        private maindefine m_mfgClass = new maindefine();

        public DisplayThread[] m_thDisplay = new DisplayThread[200];
        public cLogThread[] m_thLogging = new cLogThread[200];

        // @@.컨트롤러별 설정 (WCS_DB.INI 의 [COMM*] 에서 읽어오는 병렬 배열)
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


        // @@.타이틀 표시 정보
        private const string TITLE_BASE      = "WCS_TASK_Display";  // @.기본 타이틀
        private const string TITLE_DB_TABLE  = "DISPLAY_DATA";      // @.주 사용 테이블
        private cTitleBar m_TitleBar;                               // @.타이틀 표시/스크롤 제어

        // @@.폼 아이콘 : disp.ico 를 EmbeddedResource 로 넣어둔 이름 (RootNamespace + 파일명)
        private const string ICON_RESOURCE = "WCS_TASK_Display.disp.ico";

        /*
         * PsSetMainTitle
         *   WCS_TASK_* [DB(DB종류) : DB명@계정/IP:PORT] [DB TABLE : 테이블명] [COMM0 => IP : PORT] ...
         *   (Display 는 PLC 종류를 표시하지 않는다)
         *   표시내용이 현재 창 폭보다 길면 왼쪽으로 흘러간다.
         */
        #region[Method]@@@.타이틀에 시스템 구성정보 표시
        private void PsSetMainTitle()
        {
            string strTitle = TITLE_BASE;

            strTitle += " " + cTitleBar.GfDbInfo();
            strTitle += " [DB TABLE : " + TITLE_DB_TABLE + "]";

            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {
                if (m_strPLC_NO[ii] == null) break;

                strTitle += " [COMM" + ii.ToString() + " => " + m_strCOMM_IP[ii] + " : " + m_nCOMM_CUR_PORT[ii].ToString() + "]";
            }

            if (m_TitleBar == null) m_TitleBar = new cTitleBar(this);

            m_TitleBar.SetTitle(strTitle);
        }
        #endregion

        private const int MSG_MAX = 500;

        // @@.[접속 끊기] 상태. true 면 스레드를 다시 띄우지 않는다.
        private bool m_bDisconnected = false;
        #endregion

        public SYS_MAIN()
        {
            InitializeComponent();
            PsSetFormIcon();    // @.창 제목표시줄 / 작업표시줄 아이콘
        }

        /*
         * PsSetFormIcon
         *   폼에 Icon 을 지정하지 않으면 WinForms 기본 아이콘(wfc.ico)이 그대로 쓰인다.
         *   .csproj 의 ApplicationIcon(disp.ico)은 탐색기에 보이는 실행파일 아이콘일 뿐,
         *   창 제목표시줄과 작업표시줄에는 반영되지 않는다.
         *   그래서 실행파일에 포함시킨 disp.ico 를 직접 읽어 지정한다.
         *   (CV / SC / HOST / IO_SCH 는 SYS_MAIN.resx 에 아이콘을 넣어 같은 효과를 낸다)
         */
        #region[Method]@@@.폼 아이콘 지정
        private void PsSetFormIcon()
        {
            try
            {
                using (Stream st = Assembly.GetExecutingAssembly().GetManifestResourceStream(ICON_RESOURCE))
                {
                    if (st != null) this.Icon = new Icon(st);
                }
            }
            catch
            {
                // @.아이콘을 못 읽어도 기동에는 영향이 없어야 하므로 무시한다.
            }
        }
        #endregion

        #region 폼 로드
        private void SYS_MAIN_Load(object sender, EventArgs e)
        {
            m_nProcessCnt = 0;

            // @.중복실행 방지
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

                // @.스레드별 비동기 로그 큐와 기록 스레드
                cDefApp.m_LogQ[ii] = new Queue<LogParam>();
                m_thLogging[ii] = new cLogThread(m_strLogPath[ii], m_strLogFileNm[ii], ii);

                CreateStatusBox(ii);
                SetDisplay("picDispDbCn" + ii, "D");
                SetDisplay("picDispSkt" + ii, "D", "E");
            }

            cDefApi.GsReadInitProfileDelay("SND", ref cDefApp.GM_COMM_SND_TIME_OUT, ref m_strRtnMsg);
            cDefApi.GsReadInitProfileDelay("RCV", ref cDefApp.GM_COMM_RCV_TIME_OUT, ref m_strRtnMsg);

            FillManualCombos();

            PsSetMainTitle();   // @.타이틀에 시스템 구성정보 표시

            cDefApp.GM_STAT_MAIN = true;
            WrkThStart();
        }
        #endregion

        #region 상태 표시 박스 (이미지리스트 대신 BackColor 로 표시)
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
            picDb.Name = "picDispDbCn" + ii;
            picDb.BorderStyle = BorderStyle.FixedSingle;
            picDb.Location = new Point(x, 38);
            picDb.Size = new Size(26, 26);
            picDb.Tag = "";
            pnlTop.Controls.Add(picDb);
            ToolTip.SetToolTip(picDb, "DB Status #" + ii.ToString("00"));

            PictureBox picSk = new PictureBox();
            picSk.Name = "picDispSkt" + ii;
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

        // @@.C:연결, T:시도중, D:끊김
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

        // @@.접속상태 + 동작상태 (N:정상, W:기록중, E:에러)
        private void PfSetStatImgView(PictureBox pPic, string pStatSkt, string pStatOp)
        {
            PfSetStatImgView(pPic, pStatSkt);
        }
        #endregion

        #region 작업 스레드
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

                    // @.[접속 끊기] 상태에서는 재접속하지 않는다
                    if (m_bDisconnected)
                    {
                        SetDisplay("picDispSkt" + ii, "D", "E");
                        SetDisplay("picDispDbCn" + ii, "D");
                        continue;
                    }

                    // @.스레드가 떠 있지 않으면 작업 스레드와 로그 스레드를 시작(또는 재시작)한다
                    if (m_thDisplay[ii].m_thThread == null)
                    {
                        SetDisplay("picDispSkt" + ii, "T");
                        SetDisplay("picDispDbCn" + ii, "T");

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
                            SetDisplay("picDispSkt" + ii, "C");
                            SetDisplay("picDispDbCn" + ii, "C");
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

        #region 메시지 출력 (ListView). CheckForIllegalCrossThreadCalls=false 로 스레드 제약을 푼다
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
                // @.[로그 정지] 체크시 화면 목록에만 쌓지 않는다. 파일 로그와 WCS_LOG_PGR 은 계속 남는다.
                if (chkLogStop.Checked) return;

                ListViewItem item = new ListViewItem(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                item.SubItems.Add(strType);
                item.SubItems.Add(strId);
                item.SubItems.Add(strMsg);
                if (strType == "ERR") item.ForeColor = Color.Red;
                else if (strType == "IMP") item.ForeColor = Color.Blue;

                if (chkLatestFirst.Checked)
                {
                    // @.최신을 맨 위에 : 위에 끼워넣고, 넘치면 오래된 아래쪽을 버린다
                    lvMsg.Items.Insert(0, item);
                    while (lvMsg.Items.Count > MSG_MAX) lvMsg.Items.RemoveAt(lvMsg.Items.Count - 1);
                }
                else
                {
                    lvMsg.Items.Add(item);
                    while (lvMsg.Items.Count > MSG_MAX) lvMsg.Items.RemoveAt(0);
                }

                item.EnsureVisible();
            }
            catch { }
        }
        #endregion

        #region 수동 조작 (Client 수동지령 -> DISPLAY_DATA)
        private void FillManualCombos()
        {
            cmbController.Items.Clear();
            int maxDisp = 1;
            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {
                if (m_strPLC_NO[ii] == null) continue;
                cmbController.Items.Add(m_strPLC_NO[ii]);
                if (m_nCOMM_CNT[ii] > maxDisp) maxDisp = m_nCOMM_CNT[ii];
            }
            if (cmbController.Items.Count > 0) cmbController.SelectedIndex = 0;

            cmbDispNo.Items.Clear();
            if (maxDisp < 1) maxDisp = 1;
            for (int d = 1; d <= maxDisp; d++) cmbDispNo.Items.Add(d.ToString());
            if (cmbDispNo.Items.Count > 0) cmbDispNo.SelectedIndex = 0;

            cmbColor.Items.Clear();
            cmbColor.Items.Add("RED(4)");
            cmbColor.Items.Add("GREEN(5)");
            cmbColor.Items.Add("YELLOW(6)");
            cmbColor.SelectedIndex = 2; // @.레거시와 같이 노랑을 기본으로 둔다
        }

        private int SelectedColor()
        {
            // @.빨강=4, 초록=5, 노랑=6
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
            if (cmbController.SelectedItem == null || cmbDispNo.SelectedItem == null)
            {
                MessageBox.Show("Select controller and display number.");
                return;
            }

            string strPlc = cmbController.SelectedItem.ToString();
            string strDisp = cmbDispNo.SelectedItem.ToString();
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
                strSql += CRLF + "AND    DISP_NO = :DISP_NO          ";

                db.mComMain.CommandType = CommandType.Text;
                db.mComMain.Parameters.Clear();
                db.mComMain.Parameters.Add("CMD_RQ_ID", DbLang.VARCHAR, 255).Value = strCmd;
                db.mComMain.Parameters.Add("CMD_DATA", DbLang.VARCHAR, 255).Value = strData;
                // @.CMD_COLOR 는 INTEGER 컬럼이다. Varchar 파라미터로 넘기면 거부당하므로(42804) INT 로 바인딩한다.
                db.mComMain.Parameters.Add("CMD_COLOR", DbLang.INT).Value = nColor;
                db.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = cDefApp.GM_WH_TYP;
                db.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = strPlc;
                db.mComMain.Parameters.Add("DISP_NO", DbLang.VARCHAR, 255).Value = strDisp;

                int n = db.ExcuteNonQry(strSql);
                if (n <= 0)
                    AddMsg("ERR", strPlc, "Manual " + strCmd + " : no DISPLAY_DATA row (DISP_NO=" + strDisp + "). Check table.");
                else
                    AddMsg("IMP", strPlc, "Manual " + strCmd + " requested DISP_NO=" + strDisp + " DATA[" + strData + "] COLOR=" + nColor);
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

        #region 접속 제어 / 로그 표시 제어
        /*
         * btnDisconnect_Click
         *   [접속 끊기] : 모든 컨트롤러의 작업 스레드에 중단을 요청한다.
         *                 스레드는 소켓/DB 를 닫고 EQP_MST 접속상태를 'N' 으로 기록한 뒤 끝난다.
         *                 끊긴 상태에서는 Thread_Tick 이 스레드를 다시 띄우지 않는다.
         *   [접속]     : 중단 요청을 풀면 Thread_Tick 이 다음 주기에 다시 접속한다.
         */
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            m_bDisconnected = !m_bDisconnected;

            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {
                if (m_thDisplay[ii] == null) continue;
                m_thDisplay[ii].m_bStopReq = m_bDisconnected;
            }

            if (m_bDisconnected)
            {
                btnDisconnect.Text = "접속";
                AddMsg("IMP", "", "[접속 끊기] 요청 - 모든 컨트롤러 접속을 끊습니다.");
            }
            else
            {
                btnDisconnect.Text = "접속 끊기";
                AddMsg("IMP", "", "[접속] 요청 - 재접속을 시작합니다.");
            }
        }

        // @@.화면 로그 지우기
        private void btnLogClear_Click(object sender, EventArgs e)
        {
            lvMsg.Items.Clear();
        }

        // @@.표시 순서를 바꾸면 이미 쌓인 목록도 뒤집어 준다
        private void chkLatestFirst_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (lvMsg.Items.Count < 2) return;

                ListViewItem[] items = new ListViewItem[lvMsg.Items.Count];
                for (int i = 0; i < lvMsg.Items.Count; i++) items[lvMsg.Items.Count - 1 - i] = lvMsg.Items[i];

                lvMsg.BeginUpdate();
                lvMsg.Items.Clear();
                lvMsg.Items.AddRange(items);
                lvMsg.EndUpdate();
            }
            catch { }
        }
        #endregion

        #region Hex / Ascii 표시 선택
        private void chkHex_CheckedChanged(object sender, EventArgs e) { IsHex = chkHex.Checked; }
        private void chkAscii_CheckedChanged(object sender, EventArgs e) { IsAscii = chkAscii.Checked; }
        #endregion
    }
}
