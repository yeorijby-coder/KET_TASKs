using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using log4net;
using log4net.Config;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WCS_TASK_CV
{
    public delegate void SampleEventDelegate(object sender, string msg);
	public delegate void DelPsMsgLog(DateTime LogDate, string strMsg, cDefApp.eLogWriteGbn eLogGbn);
    public partial class SYS_MAIN : Form
    {
        private int m_nProcessCnt;
        public string m_strConnectString;
        public CvThread[] m_thCvThread     = new CvThread[200];
        public cLogThread[] m_thLogging = new cLogThread[200];
        public string[] m_strCOMM_IP     = new string[200];
        public string[] m_strPLC_NO = new string[200];
        public string[] m_strEQMT_TYP = new string[200];
        public int[] m_nCOMM_CUR_PORT      = new int[200];
        public int[] m_nCOMM_FROM_PORT = new int[200];
        public int[] m_nCOMM_TO_PORT = new int[200];
        public int[] m_nCOMM_PORT_CNT = new int[200];
        public int[] m_nCOMM_CNT       = new int[200];
        public int[] m_nFrTrackNo = new int[200];
        public int[] m_nToTrackNo = new int[200];
        public string[] m_strLogPath = new string[200];
        public string[] m_strLogFileNm = new string[200];
        private string m_strRtnMsg          = "";
        private maindefine m_mfgClass = new maindefine();
        private bool m_bHex = true;
        private bool m_bAscii = false;
        public bool IsHex { get { return m_bHex; } set { m_bHex = value; } }
        public bool IsAscii { get { return m_bAscii; } set { m_bAscii = value; } }

        #region@@@.생성자
        /*
         * 로그 리스트뷰 열 순서. 디자이너와 동적 생성부가 같이 따라간다.
         *   Timestamp | Thread No | Cmd | FILE | FUNCTION | Message | Telegram
         * FILE / FUNCTION 은 그 로그를 남긴 소스 파일과 함수다. 값은 컴파일할 때
         * [CallerFilePath] / [CallerMemberName] 로 박히므로 실행 중 비용이 없다.
         * 헤더를 오른쪽 클릭하면 열을 켜고 끌 수 있다. (ListViewColumnMenu)
         */
        private const int COL_TIME   = 0;
        private const int COL_THREAD = 1;
        private const int COL_CMD    = 2;
        private const int COL_FILE   = 3;
        private const int COL_FUNC   = 4;
        private const int COL_MSG    = 5;
        private const int COL_TGM    = 6;

        // @.로그 열 메뉴. 탭이 동적으로 늘어나므로 목록으로 들고 있는다.
        private readonly List<ListViewColumnMenu> m_ColMenus = new List<ListViewColumnMenu>();

        public SYS_MAIN()
        {
            InitializeComponent();

            // @.헤더 오른쪽 클릭으로 열을 켜고 끈다. (디자이너에 있는 첫 탭)
            m_ColMenus.Add(new ListViewColumnMenu(this.lsvCOMM1));
        }
        #endregion

        /* 
         * SYS_MAIN_Load
         */
        #region[Event]SYS_MAIN_Load

        private void SYS_MAIN_Load(object sender, EventArgs e)
        {
			m_nProcessCnt = 0;

            this.Width = 600;
            this.Height = 600;

            //중복실행을 방지하는 함수.
            if (cCmLib.GfPrevInstance() == true)
            {
                cDefApp.GM_RE_START = true;
                Application.Exit();
            }
            
            //this.Text = Process.GetCurrentProcess().ProcessName;

            this.IsAscii = checkBox1.Checked;
            this.IsHex = checkBox2.Checked;

            // Melsec : 타이틀에 Melsec 표시, 라디오(Display) 버튼 숨김, XML 파싱 버튼 표시
            this.Text = this.Text + " [Melsec]";
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            btnXmlSync.Visible = true;

#if ORACLE
            cDefApi.GsGetInitPorFileDB_1(ref cDefApp.GM_DB1_PROVIDER, ref cDefApp.GM_DB1_ALIAS, ref cDefApp.GM_DB1_USERID, ref cDefApp.GM_DB1_PASSWORD, ref m_strRtnMsg);
            m_strConnectString = "Provider=" + cDefApp.GM_DB1_PROVIDER + "; Data Source=" + cDefApp.GM_DB1_ALIAS + "; User ID=" + cDefApp.GM_DB1_USERID + "; Password =" + cDefApp.GM_DB1_PASSWORD;
            this.Text = this.Text + " [DB:" + cDefApp.GM_DB1_ALIAS + "]";   // 접속 DB명 타이틀 표시
#endif
#if POSTGRESQL
            cDefApi.GsGetInitPorFileDB_2(ref cDefApp.GM_DB2_IP, ref cDefApp.GM_DB2_DATABASE, ref cDefApp.GM_DB2_PORT, ref cDefApp.GM_DB2_USER, ref cDefApp.GM_DB2_USER_PW, ref m_strRtnMsg);
            m_strConnectString = "host=" + cDefApp.GM_DB2_IP + ";username=" + cDefApp.GM_DB2_USER + ";password=" + cDefApp.GM_DB2_USER_PW + ";database=" + cDefApp.GM_DB2_DATABASE + ";MAXPOOLSIZE=50;";
            this.Text = this.Text + " [DB:" + cDefApp.GM_DB2_DATABASE + "@" + cDefApp.GM_DB2_IP + "]"; // 접속 DB명 타이틀 표시
#endif
#if SQL
#endif

            cDefApi.GsGetInitPorFileCNF(ref cDefApp.GM_WH_TYP, ref cDefApp.GM_USERID, ref m_strRtnMsg);
            cDefApi.GsReadInitProfileProcessCnt("PROCESS", ref m_nProcessCnt, ref m_strRtnMsg);

            //@@.CV #1 접속정보ini 읽어오기
            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {
                string Name=null;

                Name =  "COMM" + ii.ToString();
                //@@.CV #1 접속정보ini 읽어오기
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

                if (m_strPLC_NO[ii] == "")
                {
                    m_strPLC_NO[ii] = null;
                    break;
                }

                SetVisable(pnlTop, ii, "picCvDbCn" + ii.ToString(), "DB  Status #" + ii.ToString("00"));
                SetVisable(pnlTop, ii, "picCvSkt" + ii.ToString(), "Socket  Status #" + ii.ToString("00"));

                SetVisableListView(tab, ii, "tabPage" + (ii + 1).ToString(), "TabPage #" + ii.ToString("00"));

                SetDisplay(pnlTop, ii, "picCvDbCn" + ii.ToString(), "D");
                SetDisplay(pnlTop, ii, "picCvSkt" + ii.ToString(), "D", "E");

            }
         
            // @@.통신 딜레이 타임읽어오기
            cDefApi.GsReadInitProfileDelay("SND", ref cDefApp.GM_COMM_SND_TIME_OUT, ref m_strRtnMsg); // @.전송
            cDefApi.GsReadInitProfileDelay("RCV", ref cDefApp.GM_COMM_RCV_TIME_OUT, ref m_strRtnMsg); // @.수신

            // Initialize log queues to avoid NullReference when threads enqueue log messages
            int logInitCount = Math.Min(m_nProcessCnt, cDefApp.m_LogQ.Length);
            for (int ii = 0; ii < logInitCount; ii++)
            {
                if (cDefApp.m_LogQ[ii] == null)
                    cDefApp.m_LogQ[ii] = new Queue<LogParam>();
            }

            // @@.여기서 부터 쓰레드 시작
            cDefApp.GM_STAT_MAIN  = true; // @.메인 시스템 동작상태
            WrkThStart();   // @.쓰레드 시작
        }
        #endregion

        /*
         * 화면 통신 표시 제어
         */
        #region
        private void SetVisable(Panel obj, int ii, string ctrName, string tipname)
        {
            Control ctrl;
            PictureBox FindPictureBox = null;



            string msg = null;
            ctrl = m_mfgClass.PfCtlFind(ref obj, ctrName, ref msg);
            if (ctrl == null)
            {
                return;
            }

            FindPictureBox = ctrl as PictureBox;
            this.ToolTip.SetToolTip(FindPictureBox, tipname);
            FindPictureBox.Visible = true;
        }
        //ini CNT 수만큼 탭페이지가 없으면 동적 생성한다. (tabPage1/lsvCOMM1 은 디자이너 정적, 이후는 생성)
        private void SetVisableListView(TabControl obj, int ii, string ctrName, string tipname)
        {
            Control ctrl = null;
            TabPage FindTabPage = null;

            string msg = null;
            ctrl = m_mfgClass.PfCtlFindTab(ref obj, ctrName, ref msg);
            if (ctrl == null)
            {
                //=======================================================================================================================
                // 컬럼해더 작성
                //=======================================================================================================================
                ColumnHeader ColumnHeader1 = new System.Windows.Forms.ColumnHeader();
                ColumnHeader ColumnHeader2 = new System.Windows.Forms.ColumnHeader();
                ColumnHeader ColumnHeader3 = new System.Windows.Forms.ColumnHeader();
                ColumnHeader ColumnHeader4 = new System.Windows.Forms.ColumnHeader();
                ColumnHeader ColumnHeader5 = new System.Windows.Forms.ColumnHeader();
                ColumnHeader ColumnHeaderFile = new System.Windows.Forms.ColumnHeader();
                ColumnHeader ColumnHeaderFunc = new System.Windows.Forms.ColumnHeader();
                ColumnHeader1.Text = "Timestamp";
                ColumnHeader1.Width = 120;
                ColumnHeader2.Text = "Thread No";
                ColumnHeader3.Text = "Cmd";
                ColumnHeaderFile.Text = "FILE";
                ColumnHeaderFile.Width = 150;
                ColumnHeaderFunc.Text = "FUNCTION";
                ColumnHeaderFunc.Width = 200;
                ColumnHeader4.Text = "Message";
                ColumnHeader4.Width = 500;
                ColumnHeader5.Text = "Telegram";
                ColumnHeader5.Width = 900;

                //=======================================================================================================================
                // 리스트뷰 작성
                //=======================================================================================================================
                ListView lsvCOMM = new ListView();
                lsvCOMM.AllowColumnReorder = true;
                lsvCOMM.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                ColumnHeader1,
                ColumnHeader2,
                ColumnHeader3,
                ColumnHeaderFile,
                ColumnHeaderFunc,
                ColumnHeader4,
                ColumnHeader5});
                lsvCOMM.Dock = System.Windows.Forms.DockStyle.Fill;
                lsvCOMM.FullRowSelect = true;
                lsvCOMM.GridLines = true;
                lsvCOMM.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
                lsvCOMM.Location = new System.Drawing.Point(3, 3);
                lsvCOMM.MultiSelect = false;
                lsvCOMM.Name = "lsvCOMM" + (ii + 1).ToString();   //"lsvCOMM1";
                lsvCOMM.TabIndex = 790;
                lsvCOMM.UseCompatibleStateImageBehavior = false;
                lsvCOMM.View = System.Windows.Forms.View.Details;
                lsvCOMM.Click += new System.EventHandler(this.lsvMsg_Click);
                m_ColMenus.Add(new ListViewColumnMenu(lsvCOMM));

                //=======================================================================================================================
                // 탭페이지 작성
                //=======================================================================================================================
                TabPage tabPage = new TabPage();
                tabPage.Controls.Add(lsvCOMM);
                tabPage.Location = new System.Drawing.Point(4, 22);
                tabPage.Name = ctrName;                            //"tabPage2"...
                tabPage.Padding = new System.Windows.Forms.Padding(3);
                tabPage.TabIndex = ii;
                tabPage.Text = "COMM" + (ii + 1).ToString();       //"COMM1";
                tabPage.UseVisualStyleBackColor = true;
                tabPage.Visible = true;

                obj.TabPages.Add(tabPage);

                this.ToolTip.SetToolTip(tabPage, tipname);
                return;
            }

            FindTabPage = ctrl as TabPage;
            this.ToolTip.SetToolTip(FindTabPage, tipname);
            FindTabPage.Visible = true;
        }
        private void SetDisplay(Panel obj, int ii, string ctrName, params string[] opt)
        {
            Control ctrl;
            PictureBox FindPictureBox = null;

            string msg = null;
            ctrl = m_mfgClass.PfCtlFind(ref obj, ctrName, ref msg);
            if (ctrl == null)
            {
                return;
            }

            FindPictureBox = ctrl as PictureBox;

            if (opt.Length == 1)
                PfSetStatImgView(FindPictureBox, opt[0]);
            else
                PfSetStatImgView(FindPictureBox, opt[0], opt[1]);
        }
        private void SetDisplay(Panel obj, int ii, string ctrName, string opt)
        {
            Control ctrl;
            PictureBox FindPictureBox = null;



            string msg = null;
            ctrl = m_mfgClass.PfCtlFind(ref obj, ctrName, ref msg);
            if (ctrl == null)
            {
                //ini CNT 수만큼 상태 아이콘이 없으면 동적 생성한다. (picCvDbCn0/picCvSkt0 은 디자이너 정적, 이후는 생성)
                PictureBox picCvDbCn = new System.Windows.Forms.PictureBox();
                PictureBox picCvSkt = new System.Windows.Forms.PictureBox();

                int nXPoint = 7 + (22 * ii);
                //=======================================================================================================================
                // picCvDbCn 작성
                //=======================================================================================================================
                picCvDbCn.Location = new System.Drawing.Point(nXPoint, 6);
                picCvDbCn.Name = "picCvDbCn" + ii.ToString();   //"picCvDbCn0";
                picCvDbCn.Size = new System.Drawing.Size(17, 17);
                picCvDbCn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                picCvDbCn.TabIndex = 843 + (ii * 2);
                picCvDbCn.TabStop = false;
                picCvDbCn.Tag = "S";
                ToolTip.SetToolTip(picCvDbCn, "C/V #" + (ii + 1).ToString() + " Database");

                //=======================================================================================================================
                // picCvSkt 작성
                //=======================================================================================================================
                picCvSkt.Location = new System.Drawing.Point(nXPoint, 29);
                picCvSkt.Name = "picCvSkt" + ii.ToString();     //"picCvSkt0";
                picCvSkt.Size = new System.Drawing.Size(17, 17);
                picCvSkt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                picCvSkt.TabIndex = 843 + (ii * 2) + 1;
                picCvSkt.TabStop = false;
                picCvSkt.Tag = "S";
                ToolTip.SetToolTip(picCvSkt, "C/V #" + (ii + 1).ToString() + " Status");

                obj.Controls.Add(picCvDbCn);
                obj.Controls.Add(picCvSkt);

                return;
            }

            FindPictureBox = ctrl as PictureBox;

            PfSetStatImgView(FindPictureBox, opt);
        }
        #endregion

        /*
         * @@@.스레드 실행
         */
        #region
        private void WrkThStart()
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread_Timer.Enabled = true;

            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {

                //Conveyor 통신 스레드.
                m_thCvThread[ii] = new CvThread(ii,
                                                cDefApp.GM_WH_TYP,
                                                m_strEQMT_TYP[ii],
                                                m_strPLC_NO[ii],
                                                m_strCOMM_IP[ii],
                                                Convert.ToInt16("0" + m_nCOMM_CUR_PORT[ii]),
                                                Convert.ToInt16("0" + m_nCOMM_FROM_PORT[ii]),
                                                Convert.ToInt16("0" + m_nCOMM_TO_PORT[ii]),
                                                m_nCOMM_PORT_CNT[ii],
                                                m_nCOMM_CNT[ii],
                                                m_nFrTrackNo[ii],
                                                m_nToTrackNo[ii],
                                                m_strConnectString,
                                                m_strLogFileNm[ii]);

            }
        }


        //public event SampleEventDelegate DspMsg;

        private void Thread_Tick(object sender, EventArgs e)
        {
			try
			{
				Thread_Timer.Enabled = false;

                for (int ii = 0; ii < m_nProcessCnt; ii++)
                {
                    if (m_thCvThread[ii].m_thThread == null)
                    {
                        SetDisplay(pnlTop, ii, "picCvSkt" + ii.ToString(), "T");
                        SetDisplay(pnlTop, ii, "picCvDbCn" + ii.ToString(), "T");

                        m_thCvThread[ii].m_thThread = new Thread(m_thCvThread[ii].Thread_Doing);
                        m_thCvThread[ii].m_thThread.IsBackground = true;
                        m_thCvThread[ii].m_frmMain = this;
                        m_thCvThread[ii].m_thThread.Start(ii);

                        Thread.Sleep(100);
                    }
                    else
                    {
                        if (m_thCvThread[ii].IsOpen)
                        {
                            SetDisplay(pnlTop, ii, "picCvSkt" + ii.ToString(), "C");
                            SetDisplay(pnlTop, ii, "picCvDbCn" + ii.ToString(), "C");
                        }
                    }

                }

				Thread_Timer.Enabled = true;

			}
			catch (Exception ex)
			{
				Thread_Timer.Enabled = true;
			}

        }
        #endregion


        #region[Motod] @@@.쓰레드 상태를 화면에 표시
        private bool PfSetStatImgView(PictureBox  pPic, 
                                          string  pStatSkt, 
                                          string pStatOp) {
            // @.Stat Connection : C:연결, T:시도, D:비연결
            // @.Stat Operation : N:정상, W:대기, E:에러
            try {
                switch (pStatSkt + pStatOp)
                {
                    case "CN":if (pPic.Tag.ToString() != "0") pPic.Image = this.imgLstStat.Images[0]; pPic.Tag = "0"; break;
                    case "CW":if (pPic.Tag.ToString() != "1") pPic.Image = this.imgLstStat.Images[1]; pPic.Tag = "1"; break;
                    case "CE":if (pPic.Tag.ToString() != "2") pPic.Image = this.imgLstStat.Images[2]; pPic.Tag = "2"; break;
                    case "TN":if (pPic.Tag.ToString() != "3") pPic.Image = this.imgLstStat.Images[3]; pPic.Tag = "3"; break;
                    case "TW":if (pPic.Tag.ToString() != "4") pPic.Image = this.imgLstStat.Images[4]; pPic.Tag = "4"; break;
                    case "TE":if (pPic.Tag.ToString() != "5") pPic.Image = this.imgLstStat.Images[5]; pPic.Tag = "5"; break;
                    case "DN":if (pPic.Tag.ToString() != "6") pPic.Image = this.imgLstStat.Images[6]; pPic.Tag = "6"; break;
                    case "DW":if (pPic.Tag.ToString() != "7") pPic.Image = this.imgLstStat.Images[7]; pPic.Tag = "7"; break;
                    case "DE":if (pPic.Tag.ToString() != "8") pPic.Image = this.imgLstStat.Images[8]; pPic.Tag = "8"; break;
                    default: break;
                }
                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                //MsgBox(ex.Message & pPic.Name)
            }
            return false;
        }
        #endregion

        #region[Motod] @@@.DB연결 상태를 화면에 표시
        private bool PfSetStatImgView(PictureBox pPic,
                                      string pStatDbCn)
        {
            // @.Stat Connection : C:연결, T:시도, D:비연결

            try 
            {
                switch(pStatDbCn)
                {
                    case "C": if (pPic.Tag.ToString() != "0") pPic.Image = this.ImgLstBkgStat.Images[0]; pPic.Tag = "0"; break;
                    case "T": if (pPic.Tag.ToString() != "1") pPic.Image = this.ImgLstBkgStat.Images[1]; pPic.Tag = "1"; break;
                    case "D": if (pPic.Tag.ToString() != "2") pPic.Image = this.ImgLstBkgStat.Images[2]; pPic.Tag = "2"; break;
                    default: break;
                }

                return true;
            }   
            catch (Exception ex) 
            {
                string msg = ex.Message;
                //MsgBox(ex.Message & pPic.Name)
            }
            return false;
        }
        #endregion

        #region@@@.ListView에 로깅[PsMsgView();]
        // @@@.대리자 선언
		delegate void DelegateListViewItem(ListViewItem item, cDefApp.eLogWriteGbn eThGbn);

        // @@@.Client 메세지 Listview Invoke 선언
		private void PsSetMsg(ListViewItem item, cDefApp.eLogWriteGbn eThGbn)
		{
			try
			{
				string strCtrlName = "";
				if (eThGbn == cDefApp.eLogWriteGbn.COMM1)
					strCtrlName = "lsvCOMM1";
				else if (eThGbn == cDefApp.eLogWriteGbn.COMM2)
					strCtrlName = "lsvCOMM2";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM3)
                    strCtrlName = "lsvCOMM3";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM4)
                    strCtrlName = "lsvCOMM4";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM5)
                    strCtrlName = "lsvCOMM5";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM6)
                    strCtrlName = "lsvCOMM6";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM7)
                    strCtrlName = "lsvCOMM7";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM8)
                    strCtrlName = "lsvCOMM8";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM9)
                    strCtrlName = "lsvCOMM9";
				else
					strCtrlName = "";

				Control Ctrl = PfCtlFind1(splBodySkt.Panel1, strCtrlName);

				if (Ctrl == null) return;

				ListView lstView = (ListView)Ctrl;

				if (lstView.InvokeRequired == true)
				{
					DelegateListViewItem d = new DelegateListViewItem(this.PsSetMsg); // SetListview
					this.Invoke(d, item, eThGbn);
				}
				else
				{
					lstView.Items.Add(item);
					if (lstView.Items.Count > 500)
					{
						lstView.Items.RemoveAt(0);
					}

					if (this.chkShow.Checked == true)
					{
						lstView.EnsureVisible(lstView.Items.Count - 1);
					}
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

        //@@@.PsMsgView[화면에 로깅...]
		public void PsMsgView(string pMsg, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, "", "", "", cDefApp.eLogMsgType.MSG_NOR, nThGbn, strFile, strFunc);
        }
		public void PsMsgView_Error(string pMsg, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, "", "", "", cDefApp.eLogMsgType.MSG_ERR, nThGbn, strFile, strFunc);
        }
		public void PsMsgView_IMP(string pMsg, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, "", "", "", cDefApp.eLogMsgType.MSG_IMP, nThGbn, strFile, strFunc);
        }
		public void PsMsgView(string pMsg, string pObjID, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, pObjID, "", "", cDefApp.eLogMsgType.MSG_NOR, nThGbn, strFile, strFunc);
        }
		public void PsMsgView_Error(string pMsg, string pObjID, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, pObjID, "", "", cDefApp.eLogMsgType.MSG_ERR, nThGbn, strFile, strFunc);
        }
		public void PsMsgView_IMP(string pMsg, string pObjID, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, pObjID, "", "", cDefApp.eLogMsgType.MSG_IMP, nThGbn, strFile, strFunc);
        }
        public void PsMsgView(string pMsg, string pObjID, string pCommTyp, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
            PsMsgView(pMsg, pObjID, pCommTyp, "", cDefApp.eLogMsgType.MSG_NOR, nThGbn, strFile, strFunc);
        }
        public void PsMsgView_Error(string pMsg, string pObjID, string pCommTyp, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
            PsMsgView(pMsg, pObjID, pCommTyp, "", cDefApp.eLogMsgType.MSG_ERR, nThGbn, strFile, strFunc);
        }
		public void PsMsgView(string pMsg, string pObjID, string pCommTyp, string pTgm, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, pObjID, pCommTyp, pTgm, cDefApp.eLogMsgType.MSG_NOR, nThGbn, strFile, strFunc);
        }
		public void PsMsgView_Error(string pMsg, string pObjID, string pCommTyp, string pTgm, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, pObjID, pCommTyp, pTgm, cDefApp.eLogMsgType.MSG_ERR, nThGbn, strFile, strFunc);
        }
		public void PsMsgView_IMP(string pMsg, string pObjID, string pCommTyp, string pTgm, int nThGbn,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
			PsMsgView(pMsg, pObjID, pCommTyp, pTgm, cDefApp.eLogMsgType.MSG_IMP, nThGbn, strFile, strFunc);
        }
        // @.[CallerFilePath] 는 빌드한 PC 의 전체 경로다. 열에는 파일 이름만 남긴다.
        private static string ShortFileName(string strPath)
        {
            if (string.IsNullOrEmpty(strPath)) return "";

            int nPos = strPath.LastIndexOfAny(new char[] { '\\', '/' });
            return (nPos < 0) ? strPath : strPath.Substring(nPos + 1);
        }

        private void PsMsgView(string pMsg, 
                               string pObjID, 
                               string pCommTyp, 
                               string pTgm, 
                  cDefApp.eLogMsgType pMsgTyp,
							   int nThGbn,
                               string strFile,
                               string strFunc)
        {
            try
            {

                if (chkStopLog.Checked) return;

                cDefApp.stutLogMsgInfo LogMsg ;
                LogMsg.Time = DateTime.Now.ToString( "yyyy/MM/dd HH:mm:ss:ffffff");
                LogMsg.MsgTyp =  pMsgTyp.ToString(); 
                LogMsg.ID = pObjID;
                LogMsg.Com = pCommTyp;
                LogMsg.File = ShortFileName(strFile);
                LogMsg.Func = strFunc;
                LogMsg.Msg = pMsg;
                LogMsg.Tgm = pTgm;
                if( chkStopLog.Checked) return;
                ListViewItem vItem = new ListViewItem(LogMsg.Time, 0);
                vItem.SubItems.Add(LogMsg.ID);
                vItem.SubItems.Add(LogMsg.Com);
                vItem.SubItems.Add(LogMsg.File);
                vItem.SubItems.Add(LogMsg.Func);
                vItem.SubItems.Add(LogMsg.Msg);
                vItem.SubItems.Add(LogMsg.Tgm);
                switch (pMsgTyp)
                {
                    case cDefApp.eLogMsgType.MSG_IMP : vItem.BackColor = Color.Blue; vItem.ForeColor = Color.White; break; 
                    case cDefApp.eLogMsgType.MSG_ERR: vItem.BackColor = Color.Red; vItem.ForeColor = Color.White; break; 
                    default:  vItem.BackColor = Color.White; vItem.ForeColor = Color.Black; break; 

                }
				this.PsSetMsg(vItem, (cDefApp.eLogWriteGbn)nThGbn);
                return;
             }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region[Event]btnXmlSync_Click
        private void btnXmlSync_Click(object sender, EventArgs e)
        {
            FRM_XML_FIELD_SYNC frm = new FRM_XML_FIELD_SYNC(m_strConnectString);
            frm.Show();
        }
        #endregion

        #region[Event]btnDelLog_Click
        private void btnDelLog_Click(object sender, EventArgs e)
        {
            //현재 선택된 탭의 리스트뷰(동적 생성분 포함)를 비운다.
            if (tab.SelectedTab != null)
            {
                foreach (Control ctrl in tab.SelectedTab.Controls)
                {
                    if (ctrl is ListView)
                    {
                        ((ListView)ctrl).Items.Clear();
                        break;
                    }
                }
            }
            this.txtMsg.Text = "";
            this.txtTgm.Text = "";
        }
        #endregion

        #region[Event]btnDelLog_Click
        private void lsvMsg_Click(object sender, EventArgs e)
        {
            try
            {
                 //클릭된 리스트뷰(동적 생성분 포함) 기준으로 표시
                 ListView lsv = sender as ListView;
                 if (lsv == null || lsv.SelectedItems.Count == 0) return;

                 this.txtMsg.Text = lsv.SelectedItems[0].SubItems[COL_MSG].Text;
                 this.txtTgm.Text = lsv.SelectedItems[0].SubItems[COL_TGM].Text;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
            }
        }
        #endregion

        #region 종료
        private void tsbEnd_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void SYS_MAIN_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cDefApp.GM_RE_START == false)
            {
                if (cDefApp.GM_STAT_MAIN == true)
                {
                    if (MessageBox.Show(this, "종료하시겠습니까?", "질문", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        LogManager.Shutdown();
                        cDefApp.GM_STAT_MAIN = false;
                        return;
                    }
                }
                e.Cancel = true;

                //if (e.CloseReason == CloseReason.UserClosing)
                //{
                //    e.Cancel = true;
                //}
                //else
                //{
                //    return;
                //}
            }
            else
            {
                MessageBox.Show(this, "프로그램 : WCS_TASK_CV \n프로그램이 이미 실행 중 입니다.", "WCS_TASK_CV_1F");
                LogManager.Shutdown();
                cDefApp.GM_STAT_MAIN = false;
                return;
            }
        }
        #endregion

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            IsHex = checkBox2.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            IsAscii = checkBox1.Checked;
        }

		#region 컨트롤 찾기.
		public Control PfCtlFind(ref Panel pPnl, string pCtlNm)
		{
			Control[] ctl;
			try
			{
				ctl = pPnl.Controls.Find(pCtlNm, true);

				if (ctl.Length == 0)
				{
					return null;
				}
				else
				{
					return ctl[0];
				}
			}
			catch (Exception ex)
			{
			}
			return null;
		}

		public Control PfCtlFind1(SplitterPanel pPnl, string pCtlNm)
		{
			Control[] ctl;
			try
			{
				ctl = pPnl.Controls.Find(pCtlNm, true);

				if (ctl.Length == 0)
				{
					return null;
				}
				else
				{
					return ctl[0];
				}
			}
			catch (Exception ex)
			{
			}
			return null;
		}
		#endregion



    }
}
