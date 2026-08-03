//최초작성자	: BASE(이길문)
//작성일		: 20160829
//화면개요   : ECS송수신 MAIN폼
//수정이력   : 01. 20161228 이길문 창고정보Get추가
//             02. 20170918 권혁찬 VB -> C# 변환작업. 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.Net;
using System.Collections;
using Microsoft.VisualBasic;
using System.Drawing.Drawing2D;

namespace TSK_HostCom
{
	public partial class frmMain : Form
	{
		public frmMain()
		{
			InitializeComponent();

            //this.Paint += new System.Windows.Forms.PaintEventHandler(this.frmMain_Gradient);
            //this.pnlTop.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlTop_Gradient);
            this.pnlMsg.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlMsg_Gradient);
            this.pnlMsg2.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlMsg2_Gradient);
        }

        // 판넬들에 덮여서 안보임
        /*
        private void frmMain_Gradient(object sender, PaintEventArgs e)
        {
            LinearGradientBrush br = new LinearGradientBrush(this.ClientRectangle,
                                                                Color.FromArgb(129, 156, 203),
                                                                Color.FromArgb(204, 218, 243),
                                                                0,
                                                                false);
            e.Graphics.FillRectangle(br, this.ClientRectangle);
        }
        //*/

        //Color startColor = Color.FromArgb();
        //Color middleColor = Color.FromArgb();
        //Color endColor = Color.FromArgb();
        Color color1 = Color.FromArgb(129, 156, 203);
        Color color2 = Color.FromArgb(204, 218, 243);


        private void pnlTop_Gradient(object sender, PaintEventArgs e)
        {
            LinearGradientBrush br = new LinearGradientBrush(this.ClientRectangle,
                                                             Color.FromArgb(129, 156, 203),
                                                             Color.FromArgb(204, 218, 243),
                                                             0,
                                                             false);
            ColorBlend cb = new ColorBlend();
            cb.Positions = new[] { 0, 1 / 2f, 1 };
            cb.Colors = new[] { color1, color2, color1};
            br.InterpolationColors = cb;

            e.Graphics.FillRectangle(br, this.ClientRectangle);         
        }

        private void pnlMsg_Gradient(object sender, PaintEventArgs e)
        {
            LinearGradientBrush br = new LinearGradientBrush(this.ClientRectangle,
                                                             color1,
                                                             color2,
                                                             0,
                                                             false);
            //br.RotateTransform(90);
            e.Graphics.FillRectangle(br, this.ClientRectangle);
        }

        private void pnlMsg2_Gradient(object sender, PaintEventArgs e)
        {
            LinearGradientBrush br = new LinearGradientBrush(this.ClientRectangle,
                                                             color2,
                                                             color1,
                                                             0,
                                                             false);
            //br.RotateTransform(90);
            e.Graphics.FillRectangle(br, this.ClientRectangle);
        }
        //---------------------------------------------
		// 선언부
		//---------------------------------------------
		public Thread g_thrListenThreadObj;    // 리슨쓰레드 객체
		public Thread[] g_thrLogThreadObj = new Thread[2];   // WriteLog 쓰레드 객체

		//---------------------------------------------
		// 타이틀 표시 정보
		//---------------------------------------------
		private const string TITLE_BASE     = "WCS_TASK_HOST";                            // @.기본 타이틀
		private const string TITLE_DB_TABLE = "JOB_MST, LUGG_MST, CV_DATA, SC_DATA";       // @.주 사용 테이블
		private cTitleBar m_TitleBar;                                                      // @.타이틀 표시/스크롤 제어

		/*
		 * PsSetMainTitle
		 *   WCS_TASK_HOST [DB(DB종류) : DB명@계정/IP:PORT] [DB TABLE : 테이블명] [COMM0(SRV) => IP : PORT] [COMM1(CLI) => IP : PORT]
		 *   (HOST 는 PLC 종류를 표시하지 않는다. COMM0 은 대기(Listen), COMM1 은 상대(ECS) 접속정보)
		 *   표시내용이 현재 창 폭보다 길면 왼쪽으로 흘러간다.
		 */
		private void PsSetMainTitle()
		{
			string strTitle = TITLE_BASE;

			strTitle += " " + cTitleBar.GfDbInfo();
			strTitle += " [DB TABLE : " + TITLE_DB_TABLE + "]";
			strTitle += " [COMM0(SRV) => 0.0.0.0 : " + modDefApp.g_iListenPort.ToString() + "]";
			strTitle += " [COMM1(CLI) => " + modDefApp.g_strRemoteIP + " : " + modDefApp.g_iRemotePort.ToString() + "]";

			if (m_TitleBar == null) m_TitleBar = new cTitleBar(this);

			m_TitleBar.SetTitle(strTitle);
		}

		//---------------------------------------------
		// 메인화면 로딩 및 초기화
		//---------------------------------------------
		private void frmMain_Load(object sender, EventArgs e)
		{
			int i;
			StatusBar1.Panels[0].Text = modUpdate.__UPDATE_DATE__;
			modDefApp.g_blIsAppExit = false;
			modDefApp.g_strPcNm = SystemInformation.ComputerName;

			if (modCmLib.PrevInstance())
			{
				MessageBox.Show("이미 프로그램이 실행 중입니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
				System.Environment.Exit(0);
				return;
			}

			modDefApp.g_frmForm = this;

			modDefApp.g_strUserID = "TSK_HostCom";
			modDefApp.g_strUserName = "Host통신[G-자동창고]";

			if (Pgm_Start() == false)
			{
				System.Environment.Exit(0);
				return;
			}

			PsSetMainTitle();   // @.타이틀에 시스템 구성정보 표시

			//---------------------------------------------------------------------------------
			// 컨트롤의 System.Windows.Forms.Control.Handle 속성에 액세스하는 잘못된 스레드에 대한 
			// 호출을 catch하는지 여부를 나타내는 값을 가져오거나 설정
			//---------------------------------------------------------------------------------
			CheckForIllegalCrossThreadCalls = false;

            // Log 쓰레드 시작
            // Server WMS, Client ECS 두개의 Log 쓰레드 생성
            for (i = 0; i <= 1; i++)
			{
				modDefApp.g_arrlstLogList[i] = new ArrayList();

				modDefApp.g_areLogExitEvent[i] = new AutoResetEvent(false);
				g_thrLogThreadObj[i] = new Thread(modWorkThread.LogThread);
				g_thrLogThreadObj[i].Name = i.ToString().PadRight(3) + "Log Thread";
				g_thrLogThreadObj[i].Start();
			}

			modDefApp.g_blListenThread = true;
			modDefApp.g_blSrvThread = false;

			tmrMain.Enabled = true;


            #region 공파레트 출고 작업인지에 대한 체크 변수 초기화 
            for (i = 0; i < 10000; i++)
            {
                modDefApp.g_bEmtpyPltJob[i] = false;
            }
            #endregion
        }


		private void tmrMain_Tick(System.Object sender, System.EventArgs e)
		{

            this.Cursor = Cursors.WaitCursor;
			tmrMain.Enabled = false;

            // 리슨 쓰레드 시작
            // Server 시작 
            g_thrListenThreadObj = new Thread(modWorkThread.ListenThread);
            g_thrListenThreadObj.Name = "Socket Listen";
            g_thrListenThreadObj.Start();

            Thread.Sleep(5000);
            // Client 쓰레드 시작
            modDefApp.g_CliWork.m_thrThreadObj = new Thread(modWorkThread.CliWorkThread);
			modDefApp.g_CliWork.m_thrThreadObj.Name = "Client Thread";
			modDefApp.g_CliWork.m_thrThreadObj.Start();

			this.Cursor = Cursors.Default;

			modDefApp.g_blSTOP_REQ = false;

			tmrSTOP_REQ.Enabled = true;

		}

		private void tmrSTOP_REQ_Tick(System.Object sender, System.EventArgs e)
		{
			tmrSTOP_REQ.Enabled = false;

			//### 클라이언트 작업
			// DB LOG IN
			if (modDefApp.g_blSTOP_REQ)
			{
				modCmWork.ShowMsgClient("종료 요구가 설정 되었습니다. ", modDefApp.MSG_IMP);
				this.Close();
			}
			else
			{
				tmrSTOP_REQ.Enabled = true;
			}
		}
		//---------------------------------------------
		// 컨트롤 이벤트
		//---------------------------------------------
		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			string strSql = null;
			int iCnt = 0;

			if (modDefApp.g_blSTOP_REQ == false)
			{
				if (MessageBox.Show("종료 하시겠습니까? ", this.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}

			int i = 0;

			this.Cursor = Cursors.WaitCursor;

			modCmWork.ShowMsgClient("종료 중... 잠시 기다리세요.", modDefApp.MSG_IMP);
			this.Text = "종료 중... 잠시 기다리세요.";
			modDefApp.g_blIsAppExit = true;

			if (modDefApp.g_blSrvThread == false)
			{
				modDefApp.g_blListenThread = false;
			}
			else
			{
				modDefApp.g_blSrvThread = false;

				//서버 쓰레드 종료
				if ((modDefApp.g_SrvWork.m_thrThreadObj != null))
				{
					modCmWork.CloseSocket(ref modDefApp.g_SrvWork.m_sktSock);
					modDefApp.g_SrvWork.m_thrThreadObj.Join();
				}
			}

			//클라이언트 쓰레드 종료
			if ((modDefApp.g_CliWork.m_thrThreadObj != null))
			{
				modDefApp.g_CliWork.m_areCliExitEvent.Set();
				modDefApp.g_CliWork.m_thrThreadObj.Join();
			}

            //modDefApp.g_CliWork.m_BDb.BeginTrans();
            //modDefApp.g_CliWork.m_BDb.ParamsClear();

            //strSql = modDefApp.CRLF + "  UPDATE SVR_PRS_INF ";
            //strSql += modDefApp.CRLF + "    SET RUN_CPT_NM   = '' ";
            //strSql += modDefApp.CRLF + "      , RUN_CPT_IP   = '' ";
            //strSql += modDefApp.CRLF + "      , STOP_DT      =  " + modDateTime.SYSDATE;
            //strSql += modDefApp.CRLF + "      , STOP_REQ_YON =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd("N");
            //strSql += modDefApp.CRLF + "  WHERE SYS_GRP       =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
            //strSql += modDefApp.CRLF + "    AND PRS_ID       =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd("PRS_ID", modDefApp.PRS_ID);

            //iCnt = modDefApp.g_CliWork.m_BDb.ExcuteNonQry_Par(ref strSql);

            //if (iCnt != 1)
            //{
            //    modDefApp.g_CliWork.m_BDb.trnMain.Rollback();
            //    MessageBox.Show("TABLE[SVR_PRS_INF] Update 실패.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
            //else
            //{
            //    modDefApp.g_CliWork.m_BDb.trnMain.Commit();
            //}

			if ((modDefApp.g_CliWork.m_BDb.conMain != null))
			{
				modDefApp.g_CliWork.m_BDb.conMain.Close();
			}

			this.Cursor = Cursors.Default;

		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnDelLogClient_Click(object sender, EventArgs e)
		{
			lsvMsgClient.Items.Clear();
		}

		private void btnDelLogServer_Click(object sender, EventArgs e)
		{
			lsvMsgServer.Items.Clear();
		}

		private void lsvMsgClient_Click(object sender, EventArgs e)
		{

			try
			{
				if (lsvMsgClient.SelectedItems.Count > 0)
				{
					txtDetail.Text = lsvMsgClient.SelectedItems[0].SubItems[1].Text;
				}
			}
			catch (Exception ex)
			{
			}

		}

		private void lsvMsgServer_Click(object sender, EventArgs e)
		{

			try
			{
				if (lsvMsgServer.SelectedItems.Count > 0)
				{
					txtDetail2.Text = lsvMsgServer.SelectedItems[0].SubItems[1].Text;
				}
			}
			catch (Exception ex)
			{
			}

		}

		//---------------------------------------------
		// 함수
		//---------------------------------------------
		public void LogThreadEnd()
		{
			int i = 0;

			for (i = 0; i <= 1; i++)
			{
				modDefApp.g_areLogExitEvent[i].Set();
				g_thrLogThreadObj[i].Join();
			}
		}
        //---------------------------------------------
        // 함수
        //---------------------------------------------
        // 작업생성         - 트랜젝션 없음
        public bool InsertJobMst(CUserDb bDb 
                                , string strLuggNo
                                , string strStartPos
                                , string strStartLoc
                                , string strDestPos
                                , string strDestLoc
                                , string strJob_Define
                                , string strLotNo = ""
                                , string strPriority = "000"
                                , string strSize = "0"
                                  , bool bServerSocket = false)
        {
            string strTitle = "[InsertJobMst] .. ";
            string strSql = "";
            string strLog = "";
            #region 작업 생성
            //bDb.BeginTrans();

            //bDb.ParamsClear();

            strSql = "INSERT INTO JOB_MST                       ";
            strSql += modDefApp.CRLF + "(  WH_TYP			    ";
            strSql += modDefApp.CRLF + " , LUGG_NO			    ";
            strSql += modDefApp.CRLF + " , START_POS			";
            strSql += modDefApp.CRLF + " , START_LOCATION    	";
            strSql += modDefApp.CRLF + " , DEST_POS          	";
            strSql += modDefApp.CRLF + " , DEST_LOCATION     	";
            strSql += modDefApp.CRLF + " , JOB_TYP		        ";
            strSql += modDefApp.CRLF + " , JOB_STATUS        	";
            strSql += modDefApp.CRLF + " , LOT_NO		    	";
            strSql += modDefApp.CRLF + " , JOB_PRIORITY     	";
            strSql += modDefApp.CRLF + " , PRODUCT_SIZE     	";
            strSql += modDefApp.CRLF + " , INS_DT		    	";
            strSql += modDefApp.CRLF + " , INS_USER_ID       	";
            strSql += modDefApp.CRLF + " , REMARKS          	";
            strSql += modDefApp.CRLF + " , WC_STEP )         	";
            strSql += modDefApp.CRLF + "VALUES ('" + modDefApp.WH_TYP + "'";
            strSql += modDefApp.CRLF + "     ,  '" + strLuggNo + "'";
            strSql += modDefApp.CRLF + "     ,  '" + strStartPos + "'";
            strSql += modDefApp.CRLF + "     ,  '" + strStartLoc + "'";
            strSql += modDefApp.CRLF + "     ,  '" + strDestPos + "'";
            strSql += modDefApp.CRLF + "     ,  '" + strDestLoc + "'";
            strSql += modDefApp.CRLF + "     ,  '" + strJob_Define + "'";
            strSql += modDefApp.CRLF + "     ,  '99'";
            strSql += modDefApp.CRLF + "     ,  '" + strLotNo + "'";
            strSql += modDefApp.CRLF + "     ,  '" + strPriority + "'";
            strSql += modDefApp.CRLF + "     ,  '" + strSize + "'";
            strSql += modDefApp.CRLF + "     ,   " + modDateTime.SYSDATE;
            strSql += modDefApp.CRLF + "     ,  'HOST_TASK'";
            strSql += modDefApp.CRLF + "     ,  ''";
            strSql += modDefApp.CRLF + "     ,  '0');";

            int iSelCnt = bDb.ExcuteNonQry_Par(ref strSql);

            if (iSelCnt <= 0)
            {
                strLog = string.Format("작업 생성에 실패하였습니다. [작업번호:{0}][출발지:{1}][출발LOC:{2}][도착지:{3}][도착LOC:{4}][LOT_NO:{5}][SIZE:{6}][DB Err:{7}]",
                    strLuggNo, strStartPos, strStartLoc, strDestPos, strDestLoc, strLotNo, strSize, bDb.strErrMsg);
                if (bServerSocket == true)
                {
                    modCmWork.ShowMsgServer(strTitle + strLog, modDefApp.MSG_ERR);
                }
                else
                {
                    modCmWork.ShowMsgClient(strTitle + strLog, modDefApp.MSG_ERR);
                }
                return false;
            }
            #endregion
            return true;
        }
        //---------------------------------------------
        // 함수
        //---------------------------------------------
        // 인터페이스 기록 입력 
        public bool InsertHostIfLog(CUserDb bDb, string strMsg, string strHostCmd, string strDirection, bool bServerSocket = false)
        {
            string strTitle = "[InsertHostIfLog] .. ";
            #region HOST_IF_LOG에 기록하기
            //if (strHostCmd != "S")
            //{
                bDb.BeginTrans();

                bDb.ParamsClear();
            //}
            string strSql = "";
            string strLog = "";
            //string strMsg = System.Text.Encoding.Default.GetString(m_bytTxBuff);
            string strSql1 = "";
            string strSql2 = "";

            strSql = "INSERT INTO HOST_IF_LOG       ";
            strSql += modDefApp.CRLF + "(  WH_TYP	";
            strSql += modDefApp.CRLF + " , LOG_DATE	";
            strSql += modDefApp.CRLF + " , LOG_TIME	";
            strSql += modDefApp.CRLF + " , HOST_CMD ";
            strSql += modDefApp.CRLF + " , DIRECTION";
            strSql += modDefApp.CRLF + " , MESSAGE  ";
            strSql += modDefApp.CRLF + " , LUGG_NO	";
            strSql += modDefApp.CRLF + " , BCR_BOTTOM";
            strSql += modDefApp.CRLF + " , BCR_TOP	";
            strSql += modDefApp.CRLF + " , INS_DT   ";
            strSql += modDefApp.CRLF + " , INS_USER_ID)";
            strSql += modDefApp.CRLF + "VALUES (";
            strSql += modDefApp.CRLF + "  '" + modDefApp.WH_TYP + "'";
            strSql += modDefApp.CRLF + ",  " + modDateTime.SYSDATE_TO_CDT;
            strSql += modDefApp.CRLF + ",  " + modDateTime.SYSDATE_TO_CTM;
            strSql += modDefApp.CRLF + ", '" + strHostCmd + "'";
            strSql += modDefApp.CRLF + ", '" + strDirection + "'"; //strSql1 = strSql;
            strSql += modDefApp.CRLF + ", '" + strMsg + "'";     
            strSql += modDefApp.CRLF + ", ''";               // 작업번호 사용안함!
            strSql += modDefApp.CRLF + ", ''";               // 바코드 하단 사용안함
            strSql += modDefApp.CRLF + ", ''";               // 바코드 상단 사용안함
            strSql += modDefApp.CRLF + ", " + modDateTime.SYSDATE;
            strSql += modDefApp.CRLF + ", 'HOST_TASK')";

            //strSql = strSql1 + strSql2;
            int iSelCnt = bDb.ExcuteNonQry_Par(ref strSql);

            if (iSelCnt <= 0)
            {
                string strTemp = "";
                if (iSelCnt < 0)
                {
                    strTemp = string.Format("[DB Error : {0}", bDb.ErrMsg);
                }
                else //if (iSelCnt == 0)
                {
                    strTemp = string.Format("[HOST_CMD:{0}][MSG:{1}]", strHostCmd, strMsg);
                }

                strLog = string.Format("HOST 인터페이스 정보 입력에 실패하였습니다. {0}", strTemp);
                if (bServerSocket == true)
                {
                    modCmWork.ShowMsgServer(strTitle + strLog, modDefApp.MSG_ERR);
                }
                else
                {
                    modCmWork.ShowMsgClient(strTitle + strLog, modDefApp.MSG_ERR);
                }

                bDb.RollbackTrans();
                return false;
            }

            bDb.CommitTrans();
            return true;

            #endregion
        }
        #region 공파레트 입출고 관련 작업정보 UPDATE 하는 함수
        public bool UpdateHostEmptyPlt(CUserDb bDb, string strEmtpyPltKind, string strEmtpyPltStation, string strLuggNo, string strWhereStatus = "", string strSetStatus = "")
        {
            if (strLuggNo == "0" || strLuggNo == "")
            {
                return false;
            }
            bDb.BeginTrans();

            bDb.ParamsClear();

            string strSql = "UPDATE HOST_EMPTY_PLT ";
            strSql += modDefApp.CRLF + "    SET LUGG_NO = " + bDb.ParamsAdd("SET_LUGG_NO", strLuggNo);
            if (strSetStatus != "")
                strSql += modDefApp.CRLF + "    , STATUS  = " + bDb.ParamsAdd("SET_STATUS", strSetStatus);

            strSql += modDefApp.CRLF + "      , UPD_ID  = 'HOST_TASK'";
            strSql += modDefApp.CRLF + "      , UPD_DT  = " + modDateTime.SYSDATE;
            strSql += modDefApp.CRLF + "  WHERE WH_TYP  = " + bDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            strSql += modDefApp.CRLF + "    AND STATUS  = " + bDb.ParamsAdd("WHERE_STATUS", strWhereStatus);

            if (strWhereStatus != "" && strSetStatus != "")
            {
                strSql += modDefApp.CRLF + "    AND LUGG_NO = " + bDb.ParamsAdd("WHERE_LUGG_NO", strLuggNo);
            }
            else
            {
                strSql += modDefApp.CRLF + "    AND KIND    = " + bDb.ParamsAdd("KIND", strEmtpyPltKind);
                strSql += modDefApp.CRLF + "    AND STN     = " + bDb.ParamsAdd("STN", strEmtpyPltStation);
                strSql += modDefApp.CRLF + "    AND LUGG_NO = '0'";
            }

            int iSelCnt = bDb.ExcuteNonQry_Par(ref strSql);

            if (iSelCnt <= 0)
            {
                bDb.RollbackTrans();
                return false;
            }

            if (iSelCnt == 0)
            {
                bDb.RollbackTrans();
                return false;
            }

            if (iSelCnt > 0)
            {
                int nLuggNo = Convert.ToInt32(strLuggNo);
                // 변수 지우기
                if (strWhereStatus != "" && strSetStatus != "")
                {
                    modDefApp.g_bEmtpyPltJob[nLuggNo] = false;
                }
                else
                {
                    modDefApp.g_strEmtpyPltKind = "";
                    modDefApp.g_strEmtpyPltStation = "";
                    modDefApp.g_bEmtpyPltJob[nLuggNo] = true;
                }
            }
            bDb.CommitTrans();
            return true;
        }
        #endregion        // 작업삭제 
        public bool DeleteJobMst(CUserDb bDb, bool bTrans, string strLuggNo, bool bJobComplete = false, bool bServerSocket = false)
        {
            string strTitle = "[DeleteJobMst] .. ";
            string strSql;

            try
            {
                bDb.ParamsClear();

                strSql = "";
                strSql += modDefApp.CRLF + " SELECT *               ";
                strSql += modDefApp.CRLF + "   FROM JOB_MST         ";
                strSql += modDefApp.CRLF + "  WHERE WH_TYP        = " + bDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                strSql += modDefApp.CRLF + "    AND LUGG_NO       = " + bDb.ParamsAdd("LUGG_NO", strLuggNo);

                if (bJobComplete == true)
                {
                    strSql += modDefApp.CRLF + "    AND JOB_STATUS    IN ('19', '29')";
                }
                int nRtn = bDb.ExcuteQry_Par(ref strSql);
                if (nRtn <= 0)
                {
                    modDefApp.GM_RTN_MSG = strTitle + "JOB_MST SELECT중 DATABASE 에러., MESSAGE [" + bDb.ErrMsg + "]";
                    if (bServerSocket == true)
                    {
                        modCmWork.ShowMsgServer(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }
                    else
                    {
                        modCmWork.ShowMsgClient(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }
                    return false;
                }


                if (bTrans == true)
                    bDb.BeginTrans();
                bDb.ParamsClear();

                // 삭제 전 작업정보이력생성.
                strSql = "";
                strSql += modDefApp.CRLF + " INSERT INTO JOB_MST_HIS                                                           ";
                strSql += modDefApp.CRLF + "           ( INS_DATE                                                              ";
                strSql += modDefApp.CRLF + "           , INS_TIME                                                              ";
                strSql += modDefApp.CRLF + "           , WH_TYP                                                                ";
                strSql += modDefApp.CRLF + "           , LUGG_NO                                                               ";
                strSql += modDefApp.CRLF + "           , START_POS                                                             ";
                strSql += modDefApp.CRLF + "           , START_LOCATION                                                        ";
                strSql += modDefApp.CRLF + "           , DEST_POS                                                              ";
                strSql += modDefApp.CRLF + "           , DEST_LOCATION                                                         ";
                strSql += modDefApp.CRLF + "           , TURN                                                                  ";
                strSql += modDefApp.CRLF + "           , PRODUCT_ID                                                            ";
                strSql += modDefApp.CRLF + "           , PRODUCT_SIZE                                                          ";
                strSql += modDefApp.CRLF + "           , JOB_TYP                                                               ";
                strSql += modDefApp.CRLF + "           , BCR_TOP                                                               ";
                strSql += modDefApp.CRLF + "           , BCR_BOTTOM                                                            ";
                strSql += modDefApp.CRLF + "           , MES_WRITE_YN                                                          ";
                strSql += modDefApp.CRLF + "           , CMD_STA                                                               ";
                strSql += modDefApp.CRLF + "           , JOB_KIND                                                              ";
                strSql += modDefApp.CRLF + "           , JOB_STATUS                                                            ";
                strSql += modDefApp.CRLF + "           , JOB_PRIORITY                                                          ";
                strSql += modDefApp.CRLF + "           , MES_ERROR_CD                                                          ";
                strSql += modDefApp.CRLF + "           , OD_LAST_PAGE                                                          ";
                strSql += modDefApp.CRLF + "           , OD_LAST_USER                                                          ";
                strSql += modDefApp.CRLF + "           , JOB_START_DT                                                          ";
                strSql += modDefApp.CRLF + "           , INS_DT                                                                ";
                strSql += modDefApp.CRLF + "           , INS_USER_ID                                                           ";
                strSql += modDefApp.CRLF + "           , REMARKS                                                               ";
                strSql += modDefApp.CRLF + "           , TRAY_TYP                                                              ";
                strSql += modDefApp.CRLF + "           , TRAY_LEV                                                              ";
                strSql += modDefApp.CRLF + "           , HS_TRACK_NO                                                           ";
                strSql += modDefApp.CRLF + "           , SC_NO                                                                 ";
                strSql += modDefApp.CRLF + "           , DURATION_TIME                                                         ";
                strSql += modDefApp.CRLF + "           , SYSTEM_BYTE                                                           ";
                strSql += modDefApp.CRLF + "           , S6F12_RESPONSE                                                        ";
                strSql += modDefApp.CRLF + "           , S6F11_YON                                                             ";
                strSql += modDefApp.CRLF + "           , CEID_NAME                                                             ";
                strSql += modDefApp.CRLF + "           , FK_NO                                                                 ";
                strSql += modDefApp.CRLF + "           , AGING_TIME                                                            ";
                strSql += modDefApp.CRLF + "           , PAIR_LUGG_NO                                                          ";
                strSql += modDefApp.CRLF + "           , LOT_NO )                                                              ";
                strSql += modDefApp.CRLF + " SELECT " + modDateTime.SYSDATE_TO_CDT + " AS INS_DATE                             ";
                strSql += modDefApp.CRLF + "      , " + modDateTime.SYSDATE_TO_CTM + " AS INS_TIME, WH_TYP, LUGG_NO, START_POS ";
                strSql += modDefApp.CRLF + "      , START_LOCATION, DEST_POS, DEST_LOCATION, TURN, PRODUCT_ID, PRODUCT_SIZE    ";
                strSql += modDefApp.CRLF + "      , JOB_TYP, BCR_TOP, BCR_BOTTOM, MES_WRITE_YN, CMD_STA, JOB_KIND, JOB_STATUS  ";
                strSql += modDefApp.CRLF + "      , JOB_PRIORITY, MES_ERROR_CD, OD_LAST_PAGE, OD_LAST_USER , JOB_START_DT      ";
                strSql += modDefApp.CRLF + "      , " + modDateTime.SYSDATE + " AS INS_DT, 'HOST_TASK', REMARKS, TRAY_TYP      ";
                strSql += modDefApp.CRLF + "      , TRAY_LEV, HS_TRACK_NO, SC_NO, DURATION_TIME, SYSTEM_BYTE, S6F12_RESPONSE   ";
                strSql += modDefApp.CRLF + "      , S6F11_YON, CEID_NAME, FK_NO, AGING_TIME, PAIR_LUGG_NO, LOT_NO              ";
                strSql += modDefApp.CRLF + "   FROM JOB_MST                                                                    ";
                strSql += modDefApp.CRLF + "  WHERE WH_TYP        = " + bDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                strSql += modDefApp.CRLF + "    AND LUGG_NO       = " + bDb.ParamsAdd("LUGG_NO", strLuggNo);

                nRtn = bDb.ExcuteNonQry_Par(ref strSql);
                if (nRtn < 0)
                {
                    modDefApp.GM_RTN_MSG = strTitle + "JOB_MST_HIS INSERT중 DATABASE 에러., MESSAGE [" + bDb.ErrMsg + "]";
                    if (bServerSocket == true)
                    {
                        modCmWork.ShowMsgServer(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }
                    else
                    {
                        modCmWork.ShowMsgClient(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }

                    if (bTrans == true)
                        bDb.RollbackTrans();

                    return false;
                }

                if (nRtn == 0)
                {
                    modDefApp.GM_RTN_MSG = strTitle + "JOB_MST_HIS INSERT중 DATA가 없습니다.," +
                                "WH_TYP=[" + modDefApp.WH_TYP.ToString() + "]" +
                                "LUGG_NO=[" + strLuggNo + "]";
                    if (bServerSocket == true)
                    {
                        modCmWork.ShowMsgServer(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }
                    else
                    {
                        modCmWork.ShowMsgClient(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }

                    if (bTrans == true)
                        bDb.RollbackTrans();

                    return false;
                }

                bDb.ParamsClear();
                strSql = "";
                strSql += modDefApp.CRLF + " DELETE                                           ";
                strSql += modDefApp.CRLF + "   FROM JOB_MST                                   ";
                strSql += modDefApp.CRLF + "  WHERE WH_TYP        = " + bDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                strSql += modDefApp.CRLF + "    AND LUGG_NO       = " + bDb.ParamsAdd("LUGG_NO", strLuggNo);
                if (bJobComplete == true)
                {
                    strSql += modDefApp.CRLF + "    AND JOB_STATUS    IN ('19', '29')";
                }
                nRtn = bDb.ExcuteNonQry_Par(ref strSql);
                if (nRtn < 0)
                {
                    modDefApp.GM_RTN_MSG = strTitle + "JOB_MST DELETE중 DATABASE 에러., MESSAGE [" + bDb.ErrMsg + "]";
                    if (bServerSocket == true)
                    {
                        modCmWork.ShowMsgServer(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }
                    else
                    {
                        modCmWork.ShowMsgClient(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }

                    if (bTrans == true)
                        bDb.RollbackTrans();

                    return false;
                }

                if (nRtn == 0)
                {
                    modDefApp.GM_RTN_MSG = strTitle + "JOB_MST DELETE중 DATA가 없습니다.," +
                                "WH_TYP=[" + modDefApp.WH_TYP.ToString() + "]" +
                                "LUGG_NO=[" + strLuggNo + "]";
                    if (bServerSocket == true)
                    {
                        modCmWork.ShowMsgServer(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }
                    else
                    {
                        modCmWork.ShowMsgClient(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                    }

                    if (bTrans == true)
                        bDb.RollbackTrans();

                    return false;
                }

                modDefApp.GM_RTN_MSG = strTitle + "정상 처리되었습니다. 작업번호[" + strLuggNo + "]";
                if (bServerSocket == true)
                {
                    modCmWork.ShowMsgServer(modDefApp.GM_RTN_MSG, modDefApp.MSG_NOR);
                }
                else
                {
                    modCmWork.ShowMsgClient(modDefApp.GM_RTN_MSG, modDefApp.MSG_NOR);
                }

                if (bTrans == true)
                    bDb.CommitTrans();

            }
            catch (Exception ex)
            {
                if (bTrans == true)
                    bDb.RollbackTrans();

                modDefApp.GM_RTN_MSG = strTitle + "작업중 예외 발생 " + ex.ToString();
                if (bServerSocket == true)
                {
                    modCmWork.ShowMsgServer(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                }
                else
                {
                    modCmWork.ShowMsgClient(modDefApp.GM_RTN_MSG, modDefApp.MSG_ERR);
                }
                return false;
            }
            return true;
        }

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 프로그램시작(서버프로그램등록여부 및 실행여부 체크, 서버프로그램정보UPDATE)
		private bool Pgm_Start()
		{
			string strSql = null;
			int iCnt = 0;
			int iFor = 0;
			int iWhCnt = 0;

			if (!modCmLib.DBLogIn(ref modDefApp.g_CliWork.m_BDb.conMain))
			{
				return false;
			}

            return true;

			//--------------------------------------------------
			// IP Get 
			// 네크워크가 연결되지 않은 상태에서 에러 발생
			// DB Login후에 처리함
			//--------------------------------------------------
			modDefApp.g_strPcIp = "";
			foreach (IPAddress MyIPAddress in Dns.GetHostAddresses(Dns.GetHostName()))
			{
				if (!MyIPAddress.IsIPv6LinkLocal)
				{
					if (string.IsNullOrEmpty(modDefApp.g_strPcIp))
					{
						modDefApp.g_strPcIp = MyIPAddress.ToString();
					}
					else
					{
						if (Strings.Len(modDefApp.g_strPcIp) + Strings.Len(MyIPAddress.ToString()) <= 92)
						{
							modDefApp.g_strPcIp += ";" + MyIPAddress.ToString();
						}
					}
				}
			}

			modDefApp.g_CliWork.m_BDb.Init();
			modDefApp.g_CliWork.m_BDb.ParamsClear();

            //strSql = modDefApp.CRLF + "  SELECT * ";
            //strSql += modDefApp.CRLF + "   FROM SVR_PRS_INF ";
            //strSql += modDefApp.CRLF + "  WHERE SYS_GRP =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
            //strSql += modDefApp.CRLF + "    AND PRS_ID =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd("PRS_ID", modDefApp.PRS_ID);

            //iCnt = modDefApp.g_CliWork.m_BDb.ExcuteQry_Par(ref strSql);

            //if (iCnt < 0)
            //{
            //    modDefApp.g_CliWork.m_BDb.conMain.Close();
            //    return false;
            //}
            //if (iCnt == 0)
            //{
            //    modDefApp.g_CliWork.m_BDb.conMain.Close();
            //    MessageBox.Show("서버프로그램으로 등록되어 있지 않습니다." + modDefApp.CRLF + modDefApp.CRLF + "프로그램으로 실행 할 수 없습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return false;
            //}
            //if ("" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CHK_TYP"].ToString() != "XX")
            //{
            //    if ("" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CHK_TYP"].ToString() != "NM")
            //    {
            //        if (!string.IsNullOrEmpty("" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CPT_IP"].ToString()))
            //        {
            //            if ("" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CPT_IP"].ToString() != modDefApp.g_strPcIp)
            //            {
            //                modDefApp.g_CliWork.m_BDb.conMain.Close();
            //                MessageBox.Show("실행컴퓨터명[ " + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CPT_NM"].ToString() + ":" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CPT_IP"].ToString() + " ]에" + modDefApp.CRLF + "해당 프로그램이 이미 실행되어 있습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //                return false;
            //            }
            //        }
            //    }
            //    if ("" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CHK_TYP"].ToString() != "IP")
            //    {
            //        if (!string.IsNullOrEmpty("" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CPT_NM"].ToString()))
            //        {
            //            if ("" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CPT_NM"].ToString() != modDefApp.g_strPcNm)
            //            {
            //                modDefApp.g_CliWork.m_BDb.conMain.Close();
            //                MessageBox.Show("실행컴퓨터명[ " + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CPT_NM"].ToString() + ":" + modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["RUN_CPT_IP"].ToString() + " ]에" + modDefApp.CRLF + "해당 프로그램이 이미 실행되어 있습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //                return false;
            //            }
            //        }
            //    }
            //}

            //modDefApp.g_CliWork.m_BDb.BeginTrans();
            //modDefApp.g_CliWork.m_BDb.ParamsClear();

            //strSql = modDefApp.CRLF + "  UPDATE SVR_PRS_INF ";
            //strSql += modDefApp.CRLF + "    SET RUN_CPT_NM   =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd(modDefApp.g_strPcNm);
            //strSql += modDefApp.CRLF + "      , RUN_CPT_IP   =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd(modDefApp.g_strPcIp);
            //strSql += modDefApp.CRLF + "      , RUN_DT       =  " + modDateTime.SYSDATE;
            //strSql += modDefApp.CRLF + "      , STOP_DT      = Null ";
            //strSql += modDefApp.CRLF + "      , STOP_REQ_DT  = Null ";
            //strSql += modDefApp.CRLF + "      , STOP_REQ_YON = 'N' ";
            //strSql += modDefApp.CRLF + "      , STOP_USER_ID = Null ";
            //strSql += modDefApp.CRLF + "  WHERE SYS_GRP       =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd(modDefApp.SYS_GRP);
            //strSql += modDefApp.CRLF + "    AND PRS_ID       =  " + modDefApp.g_CliWork.m_BDb.ParamsAdd(modDefApp.PRS_ID);

            //iCnt = modDefApp.g_CliWork.m_BDb.ExcuteNonQry_Par(ref strSql);

            //if (iCnt != 1)
            //{
            //    modDefApp.g_CliWork.m_BDb.trnMain.Rollback();
            //    MessageBox.Show("TABLE[SVR_PRS_INF] Update 실패.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return false;
            //}

            //modDefApp.g_CliWork.m_BDb.trnMain.Commit();

            ////01. 20161228 이길문 창고정보Get추가
            //modDefApp.g_CliWork.m_BDb.ParamsClear();

            //strSql = "";
            //strSql += modDefApp.CRLF + " SELECT A.* ";
            //strSql += modDefApp.CRLF + "   FROM WH_MST A ";
            //strSql += modDefApp.CRLF + "  WHERE A.WH_CTG = 'AA' ";
            //strSql += modDefApp.CRLF + "  ORDER BY A.WH_CD ";
            //iCnt = modDefApp.g_CliWork.m_BDb.ExcuteQry_Par(ref strSql);
            //if (iCnt < 0)
            //{
            //    modDefApp.g_CliWork.m_BDb.conMain.Close();
            //    return false;
            //}
            //if (iCnt == 0)
            //{
            //    modDefApp.g_CliWork.m_BDb.conMain.Close();
            //    MessageBox.Show("창고정보가 등록되어 있지 않습니다." + modDefApp.CRLF + "프로그램으로 실행 할 수 없습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return false;
            //}

            //iWhCnt = -1;

            //modDefApp.g_strWH_TYP = new String[modDefApp.g_CliWork.m_BDb.dtMain.Rows.Count];
            //modDefApp.g_strWH_CD = new String[modDefApp.g_CliWork.m_BDb.dtMain.Rows.Count];
            //for (iFor = 0; iFor <= modDefApp.g_CliWork.m_BDb.dtMain.Rows.Count - 1; iFor++)
            //{
            //    iWhCnt += 1;
            //    modDefApp.g_strWH_TYP[iWhCnt] = modDefApp.g_CliWork.m_BDb.dtMain.Rows[iFor]["WH_CD"].ToString();
            //    modDefApp.g_strWH_CD[iWhCnt] = modDefApp.g_CliWork.m_BDb.dtMain.Rows[iFor]["WH_CD"].ToString();
            //}
            ////01. END

			modDefApp.g_CliWork.m_BDb.conMain.Close();

			return true;
		}

        #region 랙에 대한 체크 하는 함수(랙 위치) 
        public bool IsValidLocation(CUserDb bDb, string strScNo, string strLocation, ref string strCELL_USE_YN, ref string strSC_PLT_JOB_TYP)
        {
            bDb.ParamsClear();
            string strSql = "SELECT * FROM CELL_MST";
            strSql += modDefApp.CRLF + "WHERE SC_NO = " + bDb.ParamsAdd("SC_NO", strScNo);
            strSql += modDefApp.CRLF + "  AND BANK = " + bDb.ParamsAdd("BANK", strLocation.Substring(0, 2));
            strSql += modDefApp.CRLF + "  AND BAY = " + bDb.ParamsAdd("BAY", strLocation.Substring(3, 3));
            strSql += modDefApp.CRLF + "  AND LEV = " + bDb.ParamsAdd("LEV", strLocation.Substring(7, 2));
            strSql += modDefApp.CRLF + "  AND WH_TYP = " + bDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);

            int iSelCnt = bDb.ExcuteQry_Par(ref strSql);

            if (iSelCnt <= 0)
            {
                return false;
            }

            if (iSelCnt > 0)
            {
                strCELL_USE_YN = "" + bDb.dtMain.Rows[0]["CELL_USE_YN"].ToString();
                strSC_PLT_JOB_TYP = "" + bDb.dtMain.Rows[0]["SC_PLT_JOB_TYP"].ToString();
            }
            return true;
        }
        #endregion       
        private void chkSimMode_CheckedChanged(object sender, EventArgs e)
        {
            string strLog = "";
            string strSql = "";
            modDefApp.g_CliWork.m_BDb.BeginTrans();
            modDefApp.g_CliWork.m_BDb.ParamsClear();
            if (this.chkSimMode.Checked == true)
            {
                //int nSIM_JOB_CNT = 2;       // 입출고대의 갯수만큼 !!
                string strLocation;// = new string[] {"01-001-01", "05-001-01"};
                string strSScNum;// = new string[] {"901", "903"};
                int nJobType = 1;
                string strLuggNum;// = new string[] {"9991", "9993"};
                string[] strDPosition = new string[] {"101", "107"};

                for (int i = 0; i < strDPosition.Length; i++)       // 입출고대의 갯수만큼 !!       
                {
                    strLocation = string.Format("{0:00}-001-01",(i * 4) + 1 );
                    strSScNum = string.Format("9{0:00}", (i * 2) + 1);
                    strLuggNum = string.Format("999{0:0}", (i * 2) + 1);

                    #region 작업이 존재하는지 Check
                    strSql = modDefApp.CRLF + "   SELECT  * ";
                    strSql += modDefApp.CRLF + "    FROM  JOB_MST";
                    strSql += modDefApp.CRLF + "   WHERE  WH_TYP = '" + modDefApp.WH_TYP + "'";
                    strSql += modDefApp.CRLF + "     AND  LUGG_NO = '" + strLuggNum + "'";

                    int iCnt = modDefApp.g_CliWork.m_BDb.ExcuteQry_Par(ref strSql);

                    if (iCnt < 0)
                    {
                        strLog = modDefApp.g_CliWork.m_BDb.ErrMsg + strSql;
                        modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
                        return;
                    }

                    if (iCnt > 0)
                    {
                        //strLog = "시뮬레이션 모드 정보 업데이트에 실패 했습니다.";
                        //modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
                        continue;
                    }
                    #endregion

                    #region 입고 작업 생성
                    modDefApp.g_CliWork.m_BDb.ParamsClear();
                    if (this.InsertJobMst(modDefApp.g_CliWork.m_BDb, strLuggNum, strDPosition[i], "00-000-00", strSScNum, strLocation, nJobType.ToString()) == false)
                    {
                        modDefApp.g_CliWork.m_BDb.RollbackTrans();
                        return;
                    }

                    #endregion

                }
            }

            modDefApp.g_CliWork.m_BDb.ParamsClear();

            bool bSimMode = this.chkSimMode.Checked;
            int nSimMode = Convert.ToInt32(bSimMode);

            strSql = modDefApp.CRLF + "   UPDATE  HOST_IF_LOG";
            strSql += modDefApp.CRLF + "     SET  SIM_MODE = '" + nSimMode.ToString() + "'";
            strSql += modDefApp.CRLF + "   WHERE  WH_TYP = '" + modDefApp.WH_TYP + "'";
            strSql += modDefApp.CRLF + "     AND  INS_DT = (SELECT INS_DT FROM HOST_IF_LOG ORDER BY INS_DT DESC LIMIT 1)";

            int iiCnt = modDefApp.g_CliWork.m_BDb.ExcuteNonQry_Par(ref strSql);

            if (iiCnt < 0)
            {
                strLog = modDefApp.g_CliWork.m_BDb.ErrMsg + strSql;
                modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
                //MessageBox.Show(strLog);
                //this.chkSimMode.Checked = !this.chkSimMode.Checked;
                modDefApp.g_CliWork.m_BDb.RollbackTrans();
                return;
            }

            if (iiCnt != 1)
            {
                strLog = "시뮬레이션 모드 정보 업데이트에 실패 했습니다.";
                modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
                //MessageBox.Show(strLog);
                //this.chkSimMode.Checked = !this.chkSimMode.Checked;
                modDefApp.g_CliWork.m_BDb.RollbackTrans();
                return;
            }
            string strTemp = bSimMode ? "시작 했습니다." : "종료 했습니다.";
            strLog = "시뮬레이션 모드 " + strTemp;
            MessageBox.Show(strLog);
            modDefApp.g_CliWork.m_BDb.CommitTrans();

        }
	}
}
