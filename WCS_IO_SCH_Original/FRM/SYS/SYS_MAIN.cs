//프로그램작성자	: 주영기
//작성일		: 20190722
//화면개요	    : 입/출고 스케쥴러 작업처리
//변경이력	    : 이승범 2019.09/19 S/C의 작업처리상태 LEVEL정보를 해당 상태에 맞게 변경.
//              : 2019.11.01 입고분과 변경추가.
//              : 2019.11.02 화면감시기능 변경추가. / FireRet = 8 (추가완료) 상태값..
//              : 2019.11.12 입고분과 빈 셀에러 발생 시 CELL_STA에 MARKING 후 화면감시에서 상태를 자동으로 처리하도록 보완.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.Net.Sockets;
using System.Data.OleDb;
using Samoh_Lib;
using log4net;
using log4net.Config;
using System.Collections;
using System.Diagnostics;


namespace TSK_COMM_IOSCH
{
    public delegate void PsMsgView(string pMsg, string pObjID, string pCommTyp, string pTgm, int nId, cDefApp.eLogMsgType pMsgTyp);
	public delegate void DelPsMsgLog(DateTime LogDate, string strMsg, int nId);

	public partial class SYS_MAIN : Form
	{
		private string m_StrConnecString; //DataBase Connection 문자열.
        // (기존 설비 스레드 cThread_CV/cThread_SC/cThread_R 은 현장 구성에서 제외됨 - 참고용으로만 보존)
        /*
         * 통합판 : 층별 스케줄러 스레드 3개
         *
         *   1F / 3F / BOX 는 호출하는 스케줄 함수 목록도, 코어 로직도 서로 달라
         *   클래스를 층별로 나눠 두었다. (cThread_SCH_1F / _3F / _BOX)
         *   화면에서는 슬롯 0 / 1 / 2 로 다룬다.
         */
        private cThread_SCH_1F  m_Sch1F  = null;
        private cThread_SCH_3F  m_Sch3F  = null;
        private cThread_SCH_BOX m_SchBOX = null;

        private const int SCH_CNT = 3;
        private static readonly string[] SCH_NAME = new string[] { "1층(1F)", "3층(3F)", "BOX" };
        private static readonly int[] SCH_THGBN = new int[] {
            (int)cDefApp.eThGbn.SCH_GR01,
            (int)cDefApp.eThGbn.SCH_GR02,
            (int)cDefApp.eThGbn.SCH_GR03 };

        // @.슬롯의 스레드 객체를 얻는다. (없으면 null = 중지 상태)
        private Thread GetSchThread(int slot)
        {
            switch (slot)
            {
                case 0: return (m_Sch1F  == null) ? null : m_Sch1F.m_Thread;
                case 1: return (m_Sch3F  == null) ? null : m_Sch3F.m_Thread;
                case 2: return (m_SchBOX == null) ? null : m_SchBOX.m_Thread;
            }
            return null;
        }

        // @.슬롯의 수동 중지 지시 플래그
        private bool GetSchStop(int slot)
        {
            switch (slot)
            {
                case 0: return (m_Sch1F  != null) && m_Sch1F.m_bManualStop;
                case 1: return (m_Sch3F  != null) && m_Sch3F.m_bManualStop;
                case 2: return (m_SchBOX != null) && m_SchBOX.m_bManualStop;
            }
            return false;
        }
        private void SetSchStop(int slot, bool v)
        {
            switch (slot)
            {
                case 0: if (m_Sch1F  != null) m_Sch1F.m_bManualStop  = v; break;
                case 1: if (m_Sch3F  != null) m_Sch3F.m_bManualStop  = v; break;
                case 2: if (m_SchBOX != null) m_SchBOX.m_bManualStop = v; break;
            }
        }

        // @.슬롯의 스레드를 새로 만들어 기동한다.
        private void StartSchThread(int slot)
        {
            int thGbn = SCH_THGBN[slot];
            switch (slot)
            {
                case 0:
                    m_Sch1F.m_Thread = new Thread(new ParameterizedThreadStart(m_Sch1F.Thread_Doing));
                    m_Sch1F.m_Thread.IsBackground = true;
                    m_Sch1F.m_Thread.Start(thGbn);
                    break;
                case 1:
                    m_Sch3F.m_Thread = new Thread(new ParameterizedThreadStart(m_Sch3F.Thread_Doing));
                    m_Sch3F.m_Thread.IsBackground = true;
                    m_Sch3F.m_Thread.Start(thGbn);
                    break;
                case 2:
                    m_SchBOX.m_Thread = new Thread(new ParameterizedThreadStart(m_SchBOX.Thread_Doing));
                    m_SchBOX.m_Thread.IsBackground = true;
                    m_SchBOX.m_Thread.Start(thGbn);
                    break;
            }
        }

        /*
         * 스레드 상태 표시 클릭 - 스레드 중지/시작 수동 제어
         *
         *   동작 중이면  "스레드를 중지하시겠습니까?"
         *   중지 상태면  "스레드를 시작 하시겠습니까?"
         *   를 물어보고, 예 를 고르면 해당 스레드를 중지하거나 시작한다.
         */
        private void PsHookSchClick(int slot)
        {
            Panel pnl = pnlTop;
            Control ctrl = m_Maindefine.PfCtlFind(ref pnl, "picDbCn" + slot.ToString());
            if (ctrl == null) return;

            // @.중복 배선 방지 후 걸기
            ctrl.Click -= picSch_Click;
            ctrl.Click += picSch_Click;
        }

        private void picSch_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            if (pic == null) return;

            int slot;
            if (!int.TryParse(pic.Name.Substring("picDbCn".Length), out slot)) return;
            if (slot < 0 || slot >= SCH_CNT) return;

            Thread th = GetSchThread(slot);
            bool bRunning = (th != null) && th.IsAlive && !GetSchStop(slot);

            if (bRunning)
            {
                if (MessageBox.Show(this,
                        SCH_NAME[slot] + " 스레드를 중지하시겠습니까?",
                        "스레드 중지",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;

                SetSchStop(slot, true);

                // @.스레드가 내려가기 전이라도 중지 지시를 화면에 먼저 보여준다.
                SetDisplay(pnlTop, slot, "picDbCn" + slot.ToString(), "D");
                PsMsgViewMain("[수동 중지] " + SCH_NAME[slot] + " 스레드 중지를 지시했습니다.", slot);
            }
            else
            {
                if (MessageBox.Show(this,
                        SCH_NAME[slot] + " 스레드를 시작 하시겠습니까?",
                        "스레드 시작",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;

                SetSchStop(slot, false);

                // @.내려간 상태면 바로 띄우고, 아직 정리 중이면 Thread_Tick(5초)이 이어받는다.
                if (GetSchThread(slot) == null) StartSchThread(slot);

                SetDisplay(pnlTop, slot, "picDbCn" + slot.ToString(), "T");
                PsMsgViewMain("[수동 시작] " + SCH_NAME[slot] + " 스레드 시작을 지시했습니다.", slot);
            }
        }

        private cLogThread[] m_thLogging = new cLogThread[200]; //Thread 객체.

		private MainClass m_Maindefine = new MainClass();
		private string m_strRtnMsg = ""; //리턴 문자열.
        private Object thisLock = new object();


        // @@.타이틀 표시 정보
        private const string TITLE_BASE = "WCS_IO_SCH";   // @.기본 타이틀
        private cTitleBar m_TitleBar;                       // @.타이틀 표시/스크롤 제어

        /*
         * PsSetMainTitle
         *   WCS_IO_SCH_* [DB(DB종류) : DB명@계정/IP:PORT]
         *   표시내용이 현재 창 폭보다 길면 왼쪽으로 흘러간다.
         */
        #region[Method]@@@.타이틀에 시스템 구성정보 표시
        private void PsSetMainTitle()
        {
            string strTitle = TITLE_BASE + " " + cTitleBar.GfDbInfo();

            if (m_TitleBar == null) m_TitleBar = new cTitleBar(this);

            m_TitleBar.SetTitle(strTitle);
        }
        #endregion

		#region@@@.생성자
		public SYS_MAIN()
		{
			InitializeComponent();
		}
		#endregion

		#region[Event]SYS_MAIN_Load
		private void SYS_MAIN_Load(object sender, EventArgs e)
		{
            //MainThread가 시작되었다는것을 나타내는 Bool값.
			cDefApp.GM_STAT_MAIN = true;

            //중복실행을 방지하는 함수.
            if (cCmLib.GfPrevInstance() == true)
            {
                cDefApp.GM_RE_START = true;
                Application.Exit();
            }

           // this.Text = Process.GetCurrentProcess().ProcessName;

            //설정파일 불러오기
#if ORACLE
            cDefApi.GsGetInitPorFileDB(ref cDefApp.GM_DB_PROVIDER, ref cDefApp.GM_DB_ALIAS, ref cDefApp.GM_DB_USERID, ref cDefApp.GM_DB_PASSWORD, ref cDefApp.GM_LOG_PATH, ref cDefApp.GM_FILENAME, ref m_strRtnMsg);
            m_StrConnecString = "Provider=" + cDefApp.GM_DB_PROVIDER + "; Data Source=" + cDefApp.GM_DB_ALIAS + "; User ID=" + cDefApp.GM_DB_USERID + "; Password =" + cDefApp.GM_DB_PASSWORD;
#endif
#if POSTGRESQL
            cDefApi.GsGetInitPorFilePDB(ref cDefApp.GM_PDB_IP, ref cDefApp.GM_PDB_PORT, ref cDefApp.GM_PDB_DATABASE, ref cDefApp.GM_PDB_USER, ref cDefApp.GM_PDB_USER_PW, ref cDefApp.GM_LOG_PATH, ref cDefApp.GM_FILENAME, ref m_strRtnMsg);
            m_StrConnecString = "Server=" + cDefApp.GM_PDB_IP + ";Port=" + cDefApp.GM_PDB_PORT + ";User ID=" + cDefApp.GM_PDB_USER + ";Password=" + cDefApp.GM_PDB_USER_PW + ";Database=" + cDefApp.GM_PDB_DATABASE + ";MaxPoolSize=50;SSL=false;";
#endif
#if SQL
            // MS-SQL 연결정보는 PostgreSQL과 동일하게 ENV_IOSCH.INI [P_DB] 에서 읽는다.
            //   IP=Server(예: localhost\SQLEXPRESS), DATABASE=DB명, USER/USER_PW=SQL 로그인
            cDefApi.GsGetInitPorFilePDB(ref cDefApp.GM_PDB_IP, ref cDefApp.GM_PDB_PORT, ref cDefApp.GM_PDB_DATABASE, ref cDefApp.GM_PDB_USER, ref cDefApp.GM_PDB_USER_PW, ref cDefApp.GM_LOG_PATH, ref cDefApp.GM_FILENAME, ref m_strRtnMsg);
            m_StrConnecString = "Server=" + cDefApp.GM_PDB_IP + ";Database=" + cDefApp.GM_PDB_DATABASE + ";User ID=" + cDefApp.GM_PDB_USER + ";Password=" + cDefApp.GM_PDB_USER_PW + ";";
#endif
            // @.1층 출고 : 결정대가 비어야 출발할지 (ENV_IOSCH.INI [1F_RET] DECIDE_WAIT, 기본 N)
            cDefApp.GM_RET_DECIDE_WAIT = cDefApi.GsGetRetDecideWait();
            chkRetDecideWait.Checked = cDefApp.GM_RET_DECIDE_WAIT;

			//Main에서 동작중 상태를 나타내기위해 표시.
            // Scheduler 상태 LED (슬롯마다 하나 : picDbCn0 / 1 / 2)
            for (int slot = 0; slot < SCH_CNT; slot++)
            {
                PsEnsureSchLed(slot);   // @.디자이너에 없는 슬롯(1,2)의 LED 를 만든다
                SetVisable(pnlTop, slot, "picDbCn" + slot.ToString(), "Scheduler " + SCH_NAME[slot]);
                SetDisplay(pnlTop, slot, "picDbCn" + slot.ToString(), "D");

                // @.상태 표시를 눌러 스레드를 중지/시작할 수 있게 한다.
                PsHookSchClick(slot);
            }

			PsSetMainTitle();   // @.타이틀에 시스템 구성정보 표시

			WrkThStart();   // @.스레드 시작
			Thread_Timer.Enabled = true;
		}
		#endregion

		#region Thread 동작상태를 가져온다.
		private void WrkThStart()
		{
			//크로스 스레드 검사 해제
			CheckForIllegalCrossThreadCalls = false;

            // @.층별 스케줄러 3개를 만들고 기동한다.
            m_Sch1F  = new cThread_SCH_1F (SCH_THGBN[0]);
            m_Sch3F  = new cThread_SCH_3F (SCH_THGBN[1]);
            m_SchBOX = new cThread_SCH_BOX(SCH_THGBN[2]);

            m_Sch1F.ConnectionString  = m_StrConnecString;
            m_Sch3F.ConnectionString  = m_StrConnecString;
            m_SchBOX.ConnectionString = m_StrConnecString;

            m_Sch1F.callPsMsgView  = new PsMsgView(PsMsgView);
            m_Sch3F.callPsMsgView  = new PsMsgView(PsMsgView);
            m_SchBOX.callPsMsgView = new PsMsgView(PsMsgView);

            for (int slot = 0; slot < SCH_CNT; slot++)
            {
                int thGbn = SCH_THGBN[slot];

                // Scheduler 로그 큐 및 로깅 스레드 초기화
                cDefApp.m_LogQ[thGbn] = new Queue<LogParam>();
                m_thLogging[thGbn] = new cLogThread(cDefApp.GM_LOG_PATH
                                                  , cDefApp.GM_FILENAME + getFileName(thGbn)
                                                  , thGbn);

                StartSchThread(slot);
            }

		}
		#endregion

		/*
		 * 화면 상태 표시 관련
		 */
		#region SetVisable, SetDisplay
		/*
         * 슬롯의 상태 LED 를 준비한다.
         *
         *   디자이너에는 picDbCn0 하나만 있으므로, 통합판에서 늘어난 슬롯(1,2)의
         *   PictureBox 는 여기서 만들어 pnlTop 에 붙인다. 이름 규칙은 picDbCn{slot}.
         */
        private void PsEnsureSchLed(int slot)
        {
            Panel pnl = pnlTop;
            if (m_Maindefine.PfCtlFind(ref pnl, "picDbCn" + slot.ToString()) != null) return;

            PictureBox pic = new System.Windows.Forms.PictureBox();
            pic.Location = new System.Drawing.Point(12 + (24 * slot), 18);
            pic.Name = "picDbCn" + slot.ToString();
            pic.Size = new System.Drawing.Size(18, 24);
            pic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pic.TabIndex = 813 + slot;
            pic.TabStop = false;
            pic.Tag = "S";
            pic.Visible = false;
            pnlTop.Controls.Add(pic);
        }

        // @.스레드 제어 알림을 화면 로그에 남긴다. (PsMsgView 의 인자를 채워 부르는 얇은 껍데기)
        private void PsMsgViewMain(string pMsg, int slot)
        {
            try
            {
                PsMsgView(pMsg, SCH_NAME[slot], "", "", SCH_THGBN[slot], cDefApp.eLogMsgType.MSG_IMP);
            }
            catch { }
        }

		private void SetVisable(Panel obj, int ii, string ctrName, string tipname)
		{
			Control ctrl;
			PictureBox FindPictureBox = null;

			ctrl = m_Maindefine.PfCtlFind(ref obj, ctrName);
			if (ctrl == null)
			{
				return;
			}

			FindPictureBox = ctrl as PictureBox;
			this.ToolTip.SetToolTip(FindPictureBox, tipname);
			FindPictureBox.Visible = true;
		}
		private void SetDisplay(Panel obj, int ii, string ctrName, params string[] opt)
		{
			Control ctrl;
			PictureBox FindPictureBox = null;


			ctrl = m_Maindefine.PfCtlFind(ref obj, ctrName);
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
		private void SetDisplay(Panel obj,int ii, string ctrName, string opt)
		{
			Control ctrl;
			PictureBox FindPictureBox = null;

			ctrl = m_Maindefine.PfCtlFind(ref obj, ctrName);
			if (ctrl == null)
			{
				return;
			}

			FindPictureBox = ctrl as PictureBox;

			PfSetStatImgView(FindPictureBox, opt);
		}
		#endregion SetVisable, SetDisplay

		/*
         * Thread Timer
         */
		#region Thread_Tick
		private void Thread_Tick(object sender, EventArgs e)
		{
			Thread_Timer.Enabled = false;
			Thread_Timer.Interval = 5000;

			try
			{
				#region IOTASK
                for (int slot = 0; slot < SCH_CNT; slot++)
                {
                    int thGbn = SCH_THGBN[slot];
                    string picName = "picDbCn" + slot.ToString();

                    /*
                     * 중지 지시가 선 슬롯은 재기동하지 않는다.
                     * (이 검사가 없으면 어떤 중지도 5초 안에 자동 재기동으로 무효화된다)
                     */
                    if (GetSchStop(slot))
                    {
                        if (GetSchThread(slot) == null)
                            SetDisplay(pnlTop, slot, picName, "D");   // 중지 완료
                        continue;
                    }

                    Thread th = GetSchThread(slot);
                    if (th == null || !th.IsAlive)
                    {
                        //@@.스레드 상태표시[C:동작중, T:시도, D:중지]
                        SetDisplay(pnlTop, slot, picName, "T");

                        // @.죽은 스레드는 되살린다. (기존 감시견 동작 유지)
                        if (th == null) StartSchThread(slot);
                    }
                    else
                    {
                        SetDisplay(pnlTop, slot, picName, "C");
                    }

                    // Scheduler 로깅 스레드 기동
                    if (m_thLogging[thGbn] != null && m_thLogging[thGbn].m_thThread == null)
                    {
                        m_thLogging[thGbn].m_thThread = new Thread(m_thLogging[thGbn].LogQueThread);
                        m_thLogging[thGbn].m_thThread.IsBackground = true;
                        m_thLogging[thGbn].m_thThread.Start();
                        Thread.Sleep(10);
                    }
                }
				#endregion

                long mem = GC.GetTotalMemory(true);
                Console.WriteLine("MAIN-Current Memory : {0}", mem);
                //GC.Collect(0, GCCollectionMode.Forced);

			}
			catch (Exception ex)
			{
			}

			//다시 타이머 Enable을 true로 한다.
			Thread_Timer.Enabled = true;
		}
		#endregion Thread_Tick

        public string getFileName(int nThNo)
        {
            string strLogName = "";

            switch (nThNo)
            {
                //case (int) cDefApp.eThGbn.R_GR01:
                //    strLogName = "RETRY_JOB";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR01:
                //    strLogName = "CONVEYOR1F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR02:
                //    strLogName = "CONVEYOR2F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR03:
                //    strLogName = "CONVEYOR3F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR04:
                //    strLogName = "CONVEYOR4F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR05:
                //    strLogName = "CONVEYOR5F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR06:
                //    strLogName = "CONVEYOR6F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR07:
                //    strLogName = "CONVEYOR7F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR08:
                //    strLogName = "CONVEYOR8F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR09:
                //    strLogName = "CONVEYOR9F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR10:
                //    strLogName = "CONVEYOR10F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR11:
                //    strLogName = "CONVEYOR11F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR12:
                //    strLogName = "CONVEYOR12F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR13:
                //    strLogName = "CONVEYOR13F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR14:
                //    strLogName = "CONVEYOR14F";
                //    break;
                //case (int)cDefApp.eThGbn.CV_GR15:
                //    strLogName = "CONVEYOR15F";
                //    break;
                //case (int)cDefApp.eThGbn.SC_GR22:
                //    strLogName = "STACKER CRANE";
                //    break;
                case (int)cDefApp.eThGbn.SCH_GR01:
                    strLogName = "SCHEDULER";
                    break;
                default:
                    strLogName = "NO_DEFINE";
                    break;
            }

            return strLogName;
        }

		#region[Method] @@@.스레드 상태를 화면에 표시
		private bool PfSetStatImgView(PictureBox pPic,
										  string pStatSkt,
										  string pStatOp)
		{
			// @.Stat Connection : C:연결, T:시도, D:비연결
			// @.Stat Operation : N:정상, W:대기, E:에러
			try
			{
				switch (pStatSkt + pStatOp)
				{
					case "CN": if (pPic.Tag.ToString() != "0") pPic.Image = this.imgLstStat.Images[0]; pPic.Tag = "0"; break;
					case "CW": if (pPic.Tag.ToString() != "1") pPic.Image = this.imgLstStat.Images[1]; pPic.Tag = "1"; break;
					case "CE": if (pPic.Tag.ToString() != "2") pPic.Image = this.imgLstStat.Images[2]; pPic.Tag = "2"; break;
					case "TN": if (pPic.Tag.ToString() != "3") pPic.Image = this.imgLstStat.Images[3]; pPic.Tag = "3"; break;
					case "TW": if (pPic.Tag.ToString() != "4") pPic.Image = this.imgLstStat.Images[4]; pPic.Tag = "4"; break;
					case "TE": if (pPic.Tag.ToString() != "5") pPic.Image = this.imgLstStat.Images[5]; pPic.Tag = "5"; break;
					case "DN": if (pPic.Tag.ToString() != "6") pPic.Image = this.imgLstStat.Images[6]; pPic.Tag = "6"; break;
					case "DW": if (pPic.Tag.ToString() != "7") pPic.Image = this.imgLstStat.Images[7]; pPic.Tag = "7"; break;
					case "DE": if (pPic.Tag.ToString() != "8") pPic.Image = this.imgLstStat.Images[8]; pPic.Tag = "8"; break;
					default: break;
				}
				return true;
			}
			catch (Exception ex)
			{
				string msg;
				msg = ex.Message;
			}
			return false;
		}
		#endregion

		#region[Method] @@@.DB연결 상태를 화면에 표시
		private bool PfSetStatImgView(PictureBox pPic,
									  string pStatDbCn)
		{
			// @.Stat Connection : C:연결, T:시도, D:비연결

			try
			{
				switch (pStatDbCn)
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
				string msg;
				msg = ex.Message;
			}
			return false;
		}
		#endregion

		#region@@@.ListView에 로그[PsMsgView();]
		// @@@.대리자 선언
		delegate void DelegateListViewItem(ListViewItem item, int nId);

		// @@@.Client 메세지 Listview Invoke 선언
		private void PsSetMsg(ListViewItem item,int nId)
		{
			try
			{
				string strCtrlName = "";
                if (nId == (int)cDefApp.eThGbn.SCH_GR01) 
                    strCtrlName = "lsvR";
				else
					strCtrlName = "";

				Control Ctrl = m_Maindefine.PfCtlFind1(splitContainer1.Panel1, strCtrlName);

				if (Ctrl == null) return;

				ListView lstView = (ListView)Ctrl;

				if (lstView.InvokeRequired == true)
				{
					DelegateListViewItem d = new DelegateListViewItem(this.PsSetMsg); // SetListview
					this.Invoke(d, item, nId);
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
				//MessageBox.Show(ex.Message);
			}
		}

		private void PsMsgView(string pMsg,
							   string pObjID,
							   string pCommTyp,
							   string pTgm,
							   int nId,
				  cDefApp.eLogMsgType pMsgTyp)
		{
			try
			{

				if (chkStopLog.Checked) return;

				cDefApp.stutLogMsgInfo LogMsg;
				LogMsg.Time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffffff");
				LogMsg.MsgTyp = pMsgTyp.ToString();
				LogMsg.ID = pObjID;
				LogMsg.Com = pCommTyp;
				LogMsg.Msg = pMsg;
				LogMsg.Tgm = pTgm;
				if (chkStopLog.Checked) return;
				ListViewItem vItem = new ListViewItem(LogMsg.Time, 0);
				vItem.SubItems.Add(LogMsg.ID);
				vItem.SubItems.Add(LogMsg.Com);
				vItem.SubItems.Add(LogMsg.Msg);
				vItem.SubItems.Add(LogMsg.Tgm);

				switch (pMsgTyp)
				{
					case cDefApp.eLogMsgType.MSG_IMP: vItem.BackColor = Color.YellowGreen; vItem.ForeColor = Color.White; break;
					case cDefApp.eLogMsgType.MSG_ERR: vItem.BackColor = Color.Red; vItem.ForeColor = Color.White; break;
					default: vItem.BackColor = Color.White; vItem.ForeColor = Color.Black; break;
				}

				this.PsSetMsg(vItem, nId);

				return;

			}
			catch (Exception ex)
			{
				//MessageBox.Show(ex.Message);
			}
		}
		#endregion

		#region[Event]btnDelLog_Click
		private void btnDelLog_Click(object sender, EventArgs e)
		{
            // Scheduler 로그 CLEAR 처리.
            this.lsvR.Items.Clear();
            this.txtMsg.Text = "";
		}
		#endregion

		#region[Event]chkRetDecideWait_CheckedChanged
        /*
         * 1층 출고 : 결정대가 비어야 대기대에서 출발 (추가 제한)
         *
         *   끔(기본) : 결정대 상태를 보지 않고 대기대 자신의 조건으로만 출발시킨다.
         *              대기대는 크레인 출고 H/S 바로 다음 트랙이라, 붙잡아 두면
         *              크레인이 출고 H/S 를 못 비워 다음 출고를 시작하지 못한다.
         *              루프 화물 수는 1층 출고 유량 제한이 따로 맡는다.
         *   켬     : 출고위치 결정대(트랙 232)가 비어 있고 그리로 가는 화물이
         *            없을 때만 출발시킨다. 결정대 앞에 줄 서는 것을 원치 않는
         *            현장에서 쓴다.
         *
         *   바꾸면 바로 반영되고 ENV_IOSCH.INI 에 남아 다음 기동에도 이어진다.
         */
		private void chkRetDecideWait_CheckedChanged(object sender, EventArgs e)
		{
            cDefApp.GM_RET_DECIDE_WAIT = chkRetDecideWait.Checked;
            cDefApi.GsSetRetDecideWait(cDefApp.GM_RET_DECIDE_WAIT);

            PsMsgViewMain("1층 출고 : 결정대가 비어야 출발 = "
                          + (cDefApp.GM_RET_DECIDE_WAIT ? "켬" : "끔"), 0);
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
                    if (MessageBox.Show(this, "종료하시겠습니까?", "종료", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        LogManager.Shutdown();
                        cDefApp.GM_STAT_MAIN = false;
                        return;
                    }
                }
                e.Cancel = true;

                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                }
                else
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show(this, "프로그램 : IO_TASK_SEMI_FINISH \n프로그램이 이미 종료 중 입니다.", "IO_TASK_SEMI_FINISH");
                LogManager.Shutdown();
                cDefApp.GM_STAT_MAIN = false;
                return;
            }
		}
		#endregion

		//LIST VIEW를 클릭했을때 텍스트박스 하단에 표시하는것.
		private void lsvMsg_Click(object sender, EventArgs e)
		{
			try
			{
				Control Ctrl = m_Maindefine.PfCtlFind1(splitContainer1.Panel1, ((ListView)sender).Name);

				ListView LvCtrl = (ListView)Ctrl;

				this.txtMsg.Text = LvCtrl.SelectedItems[0].SubItems[3].Text;
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
		}
	}
}