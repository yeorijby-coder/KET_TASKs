using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Microsoft.VisualBasic;
using System.Data;

namespace TSK_HostCom
{
	class CCliWork
	{
		public Socket m_sktSock; //소켓
		public bool m_blSockConnected;//소켓접속여부
		public System.Threading.Thread m_thrThreadObj;//쓰레드 객체    
		public System.Threading.AutoResetEvent m_areCliExitEvent = new System.Threading.AutoResetEvent(false);//종료 이벤트


		public bool m_blDbConnted;//DB 연결 유무
		public CUserDb m_BDb = new CUserDb("Multi", false);//쓰레드별 Connection 별도.

		//Header
		private byte[] m_bytRxHead = new byte[modDefApp.MSG_HEAD_CNT];
		//프로젝트 별로 최대 허용되는 m_bytRxBuff 설정
		private byte[] m_bytRxBuff = new byte[1025];
		//클라이언트로 보낼 메세지
		private byte[] m_bytTxBuff;

		//로그
		private string m_strLog = "";
		//SQL 문장
		private string m_strSql;
		//처리 건수 반환
		private int m_iSelCnt;
        //Command
        private string m_strHostCmd;
        //Direction
        private string m_strDirection = "E2W";  // 해당 클래스에서는 이방향으로 보냄!
        public bool m_bFetchSimMode;         // 시뮬레이터 모드 여부

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명			: 송신 작업정보
		struct stuSendLuggInfo
		{
			public string strMessageType;

			public string strJobDef;
			public string strLuggNo1;
			public string strStartWhTyp1;
			public string strStartStn1;
			public string strStartLoc1;
			public string strRouteStn1;
			public string strDestWhTyp1;
			public string strDestStn1;
			public string strDestLoc1;
			public string strLdCtnNo1;

			public string strLotNo1;
			public string strLuggNo2;
			public string strStartWhTyp2;
			public string strStartStn2;
			public string strStartLoc2;
			public string strRouteStn2;
			public string strDestWhTyp2;
			public string strDestStn2;
			public string strDestLoc2;
			public string strLdCtnNo2;

			public string strLotNo2;
			public string strPriority;
			public string strERRCODE;

			public string strERRKIND;

			public string strMC_NO;
			public string strMOD_YON1;

			public string strMOD_YON2;
			public string strJobRouting1;

			public string strJobRouting2;
            public string strScNo;
            public stuSendLuggInfo(string p_strInit)
			{
				strMessageType = "";
				strJobDef = "";

				strLuggNo1 = "";
				strStartStn1 = "";
				strStartLoc1 = "";
				strRouteStn1 = "000";
				strDestStn1 = "";
				strDestLoc1 = "";
				strLdCtnNo1 = "";
				strLotNo1 = "";

				strLuggNo2 = "";
				strStartStn2 = "";
				strStartLoc2 = "";
				strRouteStn2 = "000";
				strDestStn2 = "";
				strDestLoc2 = "";
				strLdCtnNo2 = "";
				strLotNo2 = "";

				strERRCODE = "0000";
				strERRKIND = "1";

				strMC_NO = "";

				strJobRouting1 = "";
				strJobRouting2 = "";

				strDestWhTyp1 = "";
				strDestWhTyp2 = "";
				strMOD_YON1 = "";
				strMOD_YON2 = "";
				strPriority = "";
				strStartWhTyp1 = "";
				strStartWhTyp2 = "";
                strScNo = "";

            }
		}

        struct stuSendEmptyPlt
        {
            public string strMessageType;

            public string strJobDef;        // 1:공파레트 입고, 2:공파레트 출고
            public string strStation;
            public stuSendEmptyPlt(string p_strInit)
            {
                strMessageType = "";
                strJobDef = "";
                strStation = "";
            }
        }

        //최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 클라이언트클래스 생성시 초기값 할당
		public CCliWork()
		{
			m_blSockConnected = false;
			m_blDbConnted = false;
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: ECS와 Socket 연결
		public bool ConnectSock()
		{
			try
			{
				System.Net.IPEndPoint ipep = default(System.Net.IPEndPoint);
				System.Net.IPAddress ipaddr = default(System.Net.IPAddress);

				ipaddr = System.Net.IPAddress.Parse(modDefApp.g_strRemoteIP);
				ipep = new System.Net.IPEndPoint(ipaddr, Convert.ToInt32(modDefApp.g_iRemotePort));
				m_sktSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				m_sktSock.Connect(ipep);

				if (m_sktSock.Connected)
				{
					modDefApp.g_CliWork.m_blSockConnected = true;
					modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picCliCom, modDefApp.ComSts.ComNor);
					m_strLog = "통신이 연결되었습니다.";
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_IMP);

					return true;
				}

				m_strLog = "리모트 시스템과 연결 실패 !";

			}
			catch (SocketException se)
			{
				//m_strLog = se.Message & "(" & se.ErrorCode.ToString & ")"
				m_strLog = "리모트 시스템과 연결 실패 !" + "(" + se.ErrorCode.ToString() + ")";
			}
			catch (Exception ex)
			{
				m_strLog = ex.ToString();
			}

			m_sktSock.Close();
			m_sktSock = null;
			modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_IMP);

			return false;
		}


		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 해더메세지 길이체크
		private bool CheckHeader(int p_iRxCnt, ref int p_iBodyLen)
		{
			string strTemp = null;

			try
			{
				if (p_iRxCnt != modDefApp.MSG_HEAD_CNT)
				{
					ClearSock();

					m_strLog = string.Format("정해진 헤더의 길이가 아닙니다.[Leng={0}]", p_iRxCnt);
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					return false;
				}

				strTemp = System.Text.Encoding.UTF8.GetString(m_bytRxHead, 10, 4);

				p_iBodyLen = Convert.ToInt32(strTemp);
				if (p_iBodyLen < 3)
				{
					ClearSock();

					m_strLog = string.Format("Body의 길이가 '3' 이하 입니다.[Leng={0}]", p_iBodyLen);
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					return false;
				}
			}
			catch (Exception ex)
			{
				ClearSock();

				m_strLog = "헤더의 정보가 틀립니다.";
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				return false;
			}

			ClearBuff(ref m_bytRxBuff);
			m_bytRxHead.CopyTo(m_bytRxBuff, 0);

			return true;

		}
		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 소켓 클리어
		private void ClearSock()
		{
			byte[] bytTempByte = null;
			int i = 0;

			try
			{
				// data가 있으면 클리어
				System.Threading.Thread.Sleep(1000);
				i = m_sktSock.Available;
				if (i > 0)
				{
					bytTempByte = new byte[i + 1];
					m_sktSock.Receive(bytTempByte, i, SocketFlags.None);
				}
			}
			catch (Exception ex)
			{
				m_strLog = "소켓 클리어 실패.";
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				throw ex;
			}
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 소켓 클리어
		private void ClearSock(ref System.Net.Sockets.NetworkStream p_ntstrm)
		{
			byte[] bytTempByte = new byte[2];

			while (true)
			{
				if (p_ntstrm.DataAvailable)
				{
					p_ntstrm.Read(bytTempByte, 0, 1);
				}
				else
				{
					return;
				}
			}
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 메세지 Body체크
		private bool CheckBody(int p_iBodyLen, int p_iRxCnt)
		{
			string strTemp = null;
			// 3회 반복
			int i = 0;
			int iRemain = 0;

			iRemain = p_iBodyLen - p_iRxCnt;
			for (i = 0; i <= 1; i++)
			{
				if (iRemain == 0)
				{
					break; 
				}
				//Debug
				//Console.WriteLine("Read fail. 회수[{0}]:ReadCnt[{1}]", i, nRxCnt)
				System.Threading.Thread.Sleep(500);
				m_sktSock.ReceiveTimeout = 3000; 
				p_iRxCnt = m_sktSock.Receive(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + p_iBodyLen - iRemain, iRemain, SocketFlags.None);
				iRemain -= p_iRxCnt;
			}

			if (iRemain != 0)
			{
				ClearSock();

				m_strLog = string.Format("정해진 메세지 길이만큼 읽지 못했습니다..[Leng={0}]", p_iBodyLen - iRemain);
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				return false;
			}

			if (m_bytRxBuff[modDefApp.MSG_HEAD_CNT] != modDefApp.STX)
			{
				ClearSock();

				m_strLog = "메세지의 시작이 'STX'가 아닙니다.";
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				return false;
			}

			if (m_bytRxBuff[modDefApp.MSG_HEAD_CNT + p_iBodyLen - 1] != modDefApp.ETX)
			{
				ClearSock();

				m_strLog = "메세지의 끝이 ETX가 아닙니다.";
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				return false;
			}

			return true;
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 소켓전송
		public void SendSock()
		{
			m_sktSock.Send(m_bytTxBuff, SocketFlags.None);
			m_strLog = System.Text.Encoding.UTF8.GetString(m_bytTxBuff);

			modCmWork.ShowMsgClient(m_strLog);

		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 응답 Header만들기
		//           # Body의 요소가 1개 이상일 경우(한글 처리 때문에), m_bytTxBuff를 밖에서 설정한다. 
		//           # 주로 Data가 있는 경우이고 반드시 ACK이다
		private void MakeHeader(int p_iBodyCnt)
		{
			string strTemp = null;
			byte[] bytTempByte = null;

			//### Header ###
			strTemp = string.Format("ECS_MBX   {0:0000} ", p_iBodyCnt + 2);
			bytTempByte = System.Text.Encoding.UTF8.GetBytes(strTemp);
			Array.Copy(bytTempByte, 0, m_bytTxBuff, 0, modDefApp.MSG_HEAD_CNT);

		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: Buff Clear
		private void ClearBuff(ref byte[] p_bytBuff)
		{
			Array.Clear(p_bytBuff, 0, p_bytBuff.Length);
		}


		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 메세지Body길이 체크
		private bool CheckBodyLen(int p_iBodyCnt, int p_iCheckCnt)
		{
			// 길이 체크
			if (p_iBodyCnt != p_iCheckCnt)
			{
				m_strLog = "정해진 메세지의 길이와 읽은 길이가 틀립니다";
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				return false;
			}

			return true;
		}

        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 도착보고 및 완료 보고를 위한 작업정보 존재여부 체크 
        // 12 : 입고 H/S 도착보고  
        // 22 : 출고 H/S 도착보고  
        // 19 : 출고작업 완료 보고(CV 완료)
        // 29 : 입고작업 완료 보고(SC 완료)
        public int IsJobExist(int nJobStatus
                        , ref int nJobType
                        , ref string strUserID
                        , ref string strSScNum
                        , ref string strDScNum
                        , ref string strSPosition
                        , ref string strDPosition
                        , ref string strLuggNum)
        {
            string strTitle = "[IsJobExist]";
            string strTemp;

            try
            {
                m_BDb.ParamsClear();

                m_strSql = "";

                m_strSql = modDefApp.CRLF + " SELECT * FROM JOB_MST                   ";
                m_strSql += modDefApp.CRLF + "  WHERE JOB_STATUS = " + m_BDb.ParamsAdd("JOB_STATUS", nJobStatus.ToString());
                m_strSql += modDefApp.CRLF + "    AND WH_TYP     = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                int nSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
                if (nSelCnt < 0) 
                { 
                    return 0; 
                }
                if (nSelCnt == 0)
                {
                    return -2;       // 완료보고할 작업이 없는 상황도 정상이긴함!
                }
                nJobType = Convert.ToInt32(m_BDb.dtMain.Rows[0]["JOB_TYP"].ToString());

                switch (nJobType)
                {
                    // 입고
                    case 1:
                        strSPosition = m_BDb.dtMain.Rows[0]["START_POS"].ToString();
                        strDPosition = m_BDb.dtMain.Rows[0]["DEST_LOCATION"].ToString();
                        //strDPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        if (nJobStatus == 12)
                        {
                            strSPosition = m_BDb.dtMain.Rows[0]["HS_TRACK_NO"].ToString();
                        }
                        strDScNum = m_BDb.dtMain.Rows[0]["DEST_POS"].ToString();
                        break;

                    // 출고
                    case 2:
                        strSPosition = m_BDb.dtMain.Rows[0]["START_LOCATION"].ToString();
                        //strSPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strDPosition = m_BDb.dtMain.Rows[0]["DEST_POS"].ToString();
                        if (nJobStatus == 22)
                        {
                            strDPosition = m_BDb.dtMain.Rows[0]["HS_TRACK_NO"].ToString();
                        }
                        strSScNum = m_BDb.dtMain.Rows[0]["START_POS"].ToString();
                        break;

                    // RACK TO RACK (호기내)
                    case 4:
                        strSPosition = m_BDb.dtMain.Rows[0]["START_LOCATION"].ToString();
                        //strSPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strDPosition = m_BDb.dtMain.Rows[0]["DEST_LOCATION"].ToString();
                        //strDPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strSScNum = strDScNum = m_BDb.dtMain.Rows[0]["START_POS"].ToString();
                        break;

                    // RACK TO RACK (호기간 출고)
                    case 5:
                        strSPosition = m_BDb.dtMain.Rows[0]["START_LOCATION"].ToString();
                        //strSPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strDPosition = m_BDb.dtMain.Rows[0]["DEST_LOCATION"].ToString();
                        //strDPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strSScNum = m_BDb.dtMain.Rows[0]["START_POS"].ToString();
                        strDScNum = m_BDb.dtMain.Rows[0]["DEST_POS"].ToString();
                        if (nJobStatus == 12)
                        {
                            strDPosition = m_BDb.dtMain.Rows[0]["HS_TRACK_NO"].ToString();
                        }
                        if (nJobStatus == 22)
                        {
                            strSPosition = m_BDb.dtMain.Rows[0]["HS_TRACK_NO"].ToString();
                        }
                        break;
                    case 6:
                        strSPosition = m_BDb.dtMain.Rows[0]["START_POS"].ToString();
                        strDPosition = m_BDb.dtMain.Rows[0]["DEST_POS"].ToString();
                        break;
                }
                strUserID = m_BDb.dtMain.Rows[0]["UPD_USER_ID"].ToString();
                strLuggNum = m_BDb.dtMain.Rows[0]["LUGG_NO"].ToString();

            }
            catch (Exception ex)
            {
                m_strLog = strTitle + "실행중 예외 발생!" + m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return 0;
            }

            return Convert.ToInt32(strLuggNum);
        }

        // 설비에러상태변경정보 송신 전 Data 존재여부 Check.
        // 설비 내 Alarm 발생/해제시 Host로 Message 전송.
        public int IsEquip_ERROR_Modified(string strEQP_TYP
                                    , ref string strDeviceNo
                                    , ref string strDeviceClass
                                    , ref string strErrorCode
                                    , ref string strErrorKind
                                    , ref string strLuggNo
                                    , ref string strBank
                                    , ref string strBay
                                    , ref string strLevel)
        {
            string strTitle = "[IsEquip_ERROR_Modified]";
            string strSql = "";
            int nSelCnt = 0;

            try
            {
                if (strEQP_TYP == "SC")
                {
                    m_BDb.ParamsClear();

                    // 1.ERROR CODE에 대해 존재여부확인.
                    m_strSql = "";
                    m_strSql += modDefApp.CRLF + " SELECT " + modDefApp.NVL + "(B.EQP_ERR_CD, '0000') AS MC_ERR_CD  ";
                    m_strSql += modDefApp.CRLF + "      , A.*                                                       ";
                    m_strSql += modDefApp.CRLF + "   FROM SC_DATA A                                                 ";
                    m_strSql += modDefApp.CRLF + "   LEFT OUTER JOIN EQP_ECD_MST B                                  ";
                    m_strSql += modDefApp.CRLF + "     ON A.ERR_CODE_RD         = B.EQP_ERR_CD                      ";
                    m_strSql += modDefApp.CRLF + "    AND B.EQP_TYP             = " + m_BDb.ParamsAdd("EQP_TYP", strEQP_TYP);
                    m_strSql += modDefApp.CRLF + "  WHERE A.WH_TYP              = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                    m_strSql += modDefApp.CRLF + "    AND A.HOST_ERR_SEND_YN     = 'N'           ";
                    nSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
                    if (nSelCnt < 0) 
                    { 
                        return -2; 
                    }
                    if (nSelCnt == 0) 
                    { 
                        return -1; 
                    }

                    strDeviceNo = "" + m_BDb.dtMain.Rows[0]["MC_NO"].ToString();
                    strLuggNo = "" + m_BDb.dtMain.Rows[0]["LUGG_NO_FK1_RD"].ToString();
                    strErrorCode = "" + m_BDb.dtMain.Rows[0]["MC_ERR_CD"].ToString();


                    string strSBank = "" + m_BDb.dtMain.Rows[0]["START_BANK_FK1_RD"].ToString();
                    string strSBay = "" + m_BDb.dtMain.Rows[0]["START_BAY_FK1_RD"].ToString();
                    string strSLevel = "" + m_BDb.dtMain.Rows[0]["START_LEVEL_FK1_RD"].ToString();
                    string strDBank = "" + m_BDb.dtMain.Rows[0]["DEST_BANK_FK1_RD"].ToString();
                    string strDBay = "" + m_BDb.dtMain.Rows[0]["DEST_BAY_FK1_RD"].ToString();
                    string strDLevel = "" + m_BDb.dtMain.Rows[0]["DEST_LEVEL_FK1_RD"].ToString();

                    strBank = "00";
                    strBay = "000";
                    strLevel = "00";
                    
                    if ((strSBank != "0" && strSBay != "0" && strSLevel != "0") &&
                        (strSBank != "00" && strSBay != "000" && strSLevel != "00"))
                    {
                        strBank = strSBank;
                        strBay = strSBay;
                        strLevel = strSLevel;
                    }
                    if ((strDBank != "0" && strDBay != "0" && strDLevel != "0") &&
                        (strDBank != "00" && strDBay != "000" && strDLevel != "00"))
                    {
                        strBank = strDBank;
                        strBay = strDBay;
                        strLevel = strDLevel;
                    }

                    strErrorKind = "0";         // 기계적 에러 
                    strDeviceClass = "1";       // SC

                    switch (strErrorCode)
                    {
                    case "0054": strErrorKind = "1"; break;     // 이중입고
                    case "0055": strErrorKind = "1"; break;     // 이중입고
                    case "0056": strErrorKind = "2"; break;     // 입고장애
                    case "0057": strErrorKind = "4"; break;     // 출고장애
                    case "0058": strErrorKind = "3"; break;     // 공출고
                    case "0059": strErrorKind = "3"; break;     // 공출고
                    }

                    // 2.MES SEND STATUS ('N' -> 'Y') UPDATE
                    m_BDb.BeginTrans();
                    m_BDb.ParamsClear();

                    m_strSql = "";
                    m_strSql += modDefApp.CRLF + " UPDATE SC_DATA                       ";
                    m_strSql += modDefApp.CRLF + "    SET HOST_ERR_SEND_YN = 'Y'         ";
                    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP          = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                    m_strSql += modDefApp.CRLF + "    AND MC_NO           = " + m_BDb.ParamsAdd("MC_NO", strDeviceNo);
                    m_strSql += modDefApp.CRLF + "    AND HOST_ERR_SEND_YN = 'N'         ";
                    int nRtn = m_BDb.ExcuteNonQry_Par(ref m_strSql);
                    if (nRtn < 0) 
                    {
                        m_BDb.RollbackTrans(); 
                        return -2; 
                    }
                    if (nRtn == 0) 
                    {
                        m_BDb.RollbackTrans(); 
                        return -1; 
                    }

                    m_BDb.CommitTrans();
                    return 1;
                }
                else if (strEQP_TYP == "CV")
                {
                    // 1.ERROR CODE에 대해 존재여부확인.
                    m_strSql = "";
                    m_strSql += modDefApp.CRLF + " SELECT " + modDefApp.NVL + "(B.EQP_ERR_CD, '0000') AS MC_ERR_CD ";
                    m_strSql += modDefApp.CRLF + "      , A.*           ";
                    m_strSql += modDefApp.CRLF + "   FROM CV_DATA A     ";
                    m_strSql += modDefApp.CRLF + "   LEFT OUTER JOIN EQP_ECD_MST B ";
                    m_strSql += modDefApp.CRLF + "     ON A.ERROR_CODE           = B.EQP_ERR_CD   ";
                    m_strSql += modDefApp.CRLF + "    AND B.EQP_TYP              = " + m_BDb.ParamsAdd("EQP_TYP", strEQP_TYP);
                    m_strSql += modDefApp.CRLF + "  WHERE A.WH_TYP               = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                    m_strSql += modDefApp.CRLF + "    AND A.HOST_ERR_SEND_YN     = 'N'           ";

                    nSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
                    if (nSelCnt < 0) 
                    { 
                        return -2; 
                    }
                    if (nSelCnt == 0) 
                    { 
                        return -1; 
                    }

                    strDeviceNo = "" + m_BDb.dtMain.Rows[0]["MC_NO"].ToString();
                    strLuggNo = "" + m_BDb.dtMain.Rows[0]["LUGG_NO_RD"].ToString();
                    strErrorCode = "" + m_BDb.dtMain.Rows[0]["MC_ERR_CD"].ToString();
                    strErrorKind = "0";         // 기계적 에러 
                    strDeviceClass = "2";       // CV
                    strBank = "00";
                    strBay = "000";
                    strLevel = "00";


                    // 2.MES SEND STATUS ('N' -> 'Y') UPDATE
                    m_BDb.BeginTrans();
                    m_BDb.ParamsClear();

                    m_strSql = "";
                    m_strSql += modDefApp.CRLF + " UPDATE CV_DATA                       ";
                    m_strSql += modDefApp.CRLF + "    SET HOST_ERR_SEND_YN = 'Y'         ";
                    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP           = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                    m_strSql += modDefApp.CRLF + "    AND MC_NO            = " + m_BDb.ParamsAdd("MC_NO", strDeviceNo);
                    m_strSql += modDefApp.CRLF + "    AND HOST_ERR_SEND_YN = 'N'         ";
                    int nRtn = m_BDb.ExcuteNonQry_Par(ref m_strSql);
                    if (nRtn < 0) 
                    {
                        m_BDb.RollbackTrans(); 
                        return -2; 
                    }
                    if (nRtn == 0) 
                    {
                        m_BDb.RollbackTrans();
                        return -1; 
                    }

                    m_BDb.CommitTrans();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                //cDefApp.g_SecsThread.MakeMsg_Error(strTitle + "실행중 예외 발생! " + ex.ToString());
                m_strLog = strTitle + "실행중 예외 발생!" + m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return 0;
            }
            return 1;
        }


		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: ECS송신정보 구하기
		public bool GetSendData()
		{
            DataTable_Dispose();

            if (modDefApp.g_frmForm.chkSimMode.Checked == false)
            {
                //GetStatusReport();
                GetErrorReport();
                GetWeightReport();
                GetEmptyPltRequest();
            }
            GetJobCompleteReport();
            GetLoadArrivalReport();

			return true;
		}
        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 상태 보고  
        private void GetStatusReport()
        {
            string strTitle = "[GetStatusReport] .. ";
            #region HOST로 최종 인터페이스한 시간을 가져온다
            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "   SELECT  *                                                                               ";
            m_strSql += modDefApp.CRLF + "    FROM  EQP_MST                                                                         ";
            m_strSql += modDefApp.CRLF + "   WHERE  WH_TYP    = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "     AND  TO_CHAR(" + modDateTime.SYSDATE + " - UPD_DT, 'YYYYMMDDHH24MISS')::INTEGER < 30 ";       // 30초       
            m_strSql += modDefApp.CRLF + "     AND  EQP_TYP   = 'HOST2'                                                             ";
            m_strSql += modDefApp.CRLF + "ORDER BY  UPD_DT DESC                                                                     ";

            int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (iCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return;
            }

            #endregion
            #region 30초가 지났는지에 따라 보내는 파라미터가 달라진다.

            if (iCnt == 0)
            {
                // 30초가 지났다.
                GetStatusReport(true);
            }
            else 
            {
                // 30초가 안지났다.
                GetStatusReport(false);
            }
            #endregion
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 완료 보고 
        private bool GetStatusReport(bool bTimeReport = false)
        {
            string strTitle = "[GetStatusReport] .. ";
            m_strHostCmd = "S";

            if (!m_blSockConnected)
            {
                return false;
            }

            int nJobType = 0;
            string strTempFrame = null;

            #region SC 상태값을 가져온다
            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "   SELECT  *                 ";
            m_strSql += modDefApp.CRLF + "    FROM  SC_DATA           ";
            m_strSql += modDefApp.CRLF + "   WHERE  WH_TYP = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            //m_strSql += modDefApp.CRLF + "     AND  HOST_SEND_YN = 'N'";          // 모든 크레인의 정보를 가져와야함!
            m_strSql += modDefApp.CRLF + "ORDER BY  SC_NO             ";

            int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (iCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return false;
            }

            // SC 가 1대도 설정되어있지 않을경우  
            if (iCnt == 0)
            {
                //m_strLog = "이미 요청 중인 작업이 있으므로 요청하지 않음![요청시간 : " + m_BDb.dtMain.Rows[0]["UDT_DT"] + "]";
                m_strLog = "SC_DATA를 가져오지 못했습니다.";
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return false;
            }

            #endregion
            #region SC 상태값으로 인한 상위에 보낼 메세지 부분 구성

            bool bScTemp = false;
            string strSC_HOST_SEND_YN = "";
            for (int i = 0; i < iCnt; i++)
            {
                strSC_HOST_SEND_YN = "" + m_BDb.dtMain.Rows[i]["HOST_SEND_YN"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[i]["HOST_SEND_YN"].ToString();

                if (strSC_HOST_SEND_YN == "N")
                {
                    bScTemp = true;
                    break;
                }
            }

            string strTemp1 = "";

            int nScStatus = modDefApp.SC_STATUS_NORMAL_WAIT;

            string strONLINE_MODE_RD = "";
            string strAUTO_MODE_RD = "";
            string strACTIVE_MODE_RD = "";
            string strSUSPEND = "";
            string strUCSTATUS_RD = "";
            string strITN_LUGG_FK1 = "";
            string strITN_LUGG_FK2 = "";
            string strJOB_TYP_RD = "";
            string strPLC_NO = "";

            int nScCnt = iCnt;
            for (int ii = 0; ii < iCnt; ii++)
            {
                nScStatus = modDefApp.SC_STATUS_NORMAL_WAIT;

                strONLINE_MODE_RD = "" + m_BDb.dtMain.Rows[ii]["ONLINE_MODE_RD"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[ii]["ONLINE_MODE_RD"].ToString();
                strAUTO_MODE_RD = "" + m_BDb.dtMain.Rows[ii]["AUTO_MODE_RD"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[ii]["AUTO_MODE_RD"].ToString();
                strACTIVE_MODE_RD = "" + m_BDb.dtMain.Rows[ii]["ACTIVE_MODE_RD"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[ii]["ACTIVE_MODE_RD"].ToString();
                strSUSPEND = "" + m_BDb.dtMain.Rows[ii]["SUSPEND"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[ii]["SUSPEND"].ToString();
                strUCSTATUS_RD = "" + m_BDb.dtMain.Rows[ii]["UCSTATUS_RD"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[ii]["UCSTATUS_RD"].ToString();
                strITN_LUGG_FK1 = "" + m_BDb.dtMain.Rows[ii]["ITN_LUGG_FK1"].ToString() == "" ? "0000" : m_BDb.dtMain.Rows[ii]["ITN_LUGG_FK1"].ToString();
                strITN_LUGG_FK2 = "" + m_BDb.dtMain.Rows[ii]["ITN_LUGG_FK2"].ToString() == "" ? "0000" : m_BDb.dtMain.Rows[ii]["ITN_LUGG_FK2"].ToString();       //
                strJOB_TYP_RD = "" + m_BDb.dtMain.Rows[ii]["JOB_TYP_RD"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[ii]["JOB_TYP_RD"].ToString();
                strPLC_NO = "" + m_BDb.dtMain.Rows[ii]["PLC_NO"].ToString() == "" ? "00" : m_BDb.dtMain.Rows[ii]["PLC_NO"].ToString();
                

                if (strITN_LUGG_FK1 != "0")
                {
                    switch (strJOB_TYP_RD)
                    {
                        case "1": nScStatus = modDefApp.SC_STATUS_STORING; break;
                        case "2": nScStatus = modDefApp.SC_STATUS_RETRIEVING; break;
                        //case "3": nScStatus = modDefApp.SC_STATUS_STORING; break;
                        case "4": //nScStatus = modDefApp.SC_STATUS_STORING; break;
                        case "5": nScStatus = modDefApp.SC_STATUS_RACK_TO_RACK; break;
                    }
                }


                if (strONLINE_MODE_RD != "1" ||
                    strAUTO_MODE_RD != "1" ||
                    strACTIVE_MODE_RD != "1")
                {
                    nScStatus = modDefApp.SC_STATUS_NO_ONLINE;
                }
                switch (strSUSPEND)
                {
                case "1": nScStatus = modDefApp.SC_STATUS_SUSPEND_STO; break;
                case "2": nScStatus = modDefApp.SC_STATUS_SUSPEND_RET; break;
                case "3": nScStatus = modDefApp.SC_STATUS_SUSPEND_ALL; break;
                }
                if (strUCSTATUS_RD == "4")
                {
                    nScStatus = modDefApp.SC_STATUS_ERROR;
                }

                strTempFrame = string.Format("{0:00}{1:0}{2:0000}", Convert.ToInt32(strPLC_NO), nScStatus, Convert.ToInt32(strITN_LUGG_FK1));

                strTemp1 += strTempFrame;
            }
            #endregion

            #region CV 상태값을 가져온다
            m_BDb.ParamsClear();
            
            // 다행 스럽게도 리모콘과 모드와 무게값이 모두 입고대에 표현된다 
            // 혹시라도 다른 조건이 필요하다면 쿼리 수정 요망 
            m_strSql = modDefApp.CRLF + "     SELECT  WD.WEIGHT_RCV_VAL             ";
            m_strSql += modDefApp.CRLF + "         ,  CD.*                          ";
            m_strSql += modDefApp.CRLF + "      FROM  CV_DATA CD                    ";
            m_strSql += modDefApp.CRLF + "      LEFT  OUTER    JOIN      WC_DATA WD ";
            m_strSql += modDefApp.CRLF + "        ON  CD.MC_NO        =  WD.WC_MC_NO";
            m_strSql += modDefApp.CRLF + "     WHERE  CD.WH_TYP       =  " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "       AND (CD.STN_KIND::INTEGER & 1 = 1) ";
            m_strSql += modDefApp.CRLF + "       AND  CD.COMP_VR      = 'Y'         ";
            m_strSql += modDefApp.CRLF + "  ORDER BY  CD.PLC_NO,     CD.MC_NO       ";

            iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (iCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return false;
            }

            // CV 가 1대도 설정되어있지 않을경우  
            if (iCnt == 0)
            {
                //m_strLog = "이미 요청 중인 작업이 있으므로 요청하지 않음![요청시간 : " + m_BDb.dtMain.Rows[0]["UDT_DT"] + "]";
                m_strLog = "CV_DATA를 가져오지 못했습니다.";
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return false;
            }

            #endregion
            #region CV 상태값으로 인한 상위에 보낼 메세지 부분 구성

            bool bCvTemp = false;
            string strCV_HOST_SEND_YN = "";
            for (int i = 0; i < iCnt; i++)
            {
                strCV_HOST_SEND_YN = "" + m_BDb.dtMain.Rows[i]["HOST_SEND_YN"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[i]["HOST_SEND_YN"].ToString();

                if (strCV_HOST_SEND_YN == "N")      // CV TASK에서 수정했음!
                {
                    bCvTemp = true;
                    break;
                }
            }

            // 설비의 변경이 일어나지 않았을 경우 보고하지 않음 - 30초 마다 보고하는 부분을 제외하고...
            if ((bTimeReport == false) && 
                ((bScTemp == false) && (bCvTemp == false)))
            {
                return false;
            }

            string strTemp2 = "";

            int nCvStatus = 0;

            string strSTO_READY_RD = "";
            string strSTN_KIND = "";
            string strMC_NO = "";
            string strMC_NO_NM = "";

            string strMODE = "0";
            string strREMOTE_CONTROL = "0";
            string strWEIGHT_RCV_VAL = "0000000";
            string strROLL_MODE = "0";

            string strMC_NO_LIST = "'0'";

            int nStoStnCnt = 0;
            for (int iii = 0; iii < iCnt; iii++)        // 전체 작업대 갯수만큼
            {
                nCvStatus = 0;

                strSTO_READY_RD = "" + m_BDb.dtMain.Rows[iii]["STO_READY_RD"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[iii]["STO_READY_RD"].ToString();
                strSTN_KIND = "" + m_BDb.dtMain.Rows[iii]["STN_KIND"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[iii]["STN_KIND"].ToString();
                strMC_NO = "" + m_BDb.dtMain.Rows[iii]["MC_NO"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[iii]["MC_NO"].ToString();
                strMC_NO_NM = "" + m_BDb.dtMain.Rows[iii]["MC_NO_NM"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[iii]["MC_NO_NM"].ToString();
                
                strMC_NO_LIST += ",'" + strMC_NO + "'";
                
                // 모드 정보와 무게 정보 리모콘 정보 
                switch (strMC_NO)
                {
                    case "101":
                        strREMOTE_CONTROL = "" + m_BDb.dtMain.Rows[iii]["REMOTE_CONTROL"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[iii]["REMOTE_CONTROL"].ToString();
                        strWEIGHT_RCV_VAL = "" + m_BDb.dtMain.Rows[iii]["WEIGHT_RCV_VAL"].ToString() == "" ? "0000000" : m_BDb.dtMain.Rows[iii]["WEIGHT_RCV_VAL"].ToString(); 
                        break;
                    case "149":
                        strMODE = "" + m_BDb.dtMain.Rows[iii]["STOCK_MODE"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[iii]["STOCK_MODE"].ToString(); 
                        break;
                    case "154":
                        strROLL_MODE = "" + m_BDb.dtMain.Rows[iii]["ROLL_MODE"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[iii]["ROLL_MODE"].ToString();
                        break;
                }


                if (strSTO_READY_RD == "1")
                {
                    nCvStatus = 1;
                }

                strTempFrame = string.Format("{0:0}", nCvStatus);

                strTemp2 += strTempFrame;
            }
            #endregion

            #region 상위에 보낼 메세지 부분 구성
            //전문 작성
            string strTemp = null;
            byte[] bytTempByte = null;

            strTemp = string.Format("S{0:0000000000000000000000000000}{1:000000}{2:0}{3:0}{4:0000000}{5:0}",
                strTemp1, Convert.ToInt32(strTemp2), Convert.ToInt32(strMODE), Convert.ToInt32(strROLL_MODE), Convert.ToInt32(strWEIGHT_RCV_VAL), Convert.ToInt32(strREMOTE_CONTROL));

            int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            //MSG_ORDER_CNT

            m_bytTxBuff = new byte[iTxCnt];

            //### Header ###
            MakeHeader(strTemp.Length);
            //MSG_ORDER_CNT


            //### Body ###
            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;


            bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);

            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            #endregion
            #region 메세지 보내기
            if (!RequestSrv(iTxCnt.ToString()))
            {
                return false;
            }
            #endregion

            #region 응답 받고 SC_DATA와 CV_DATA의 HOST_SEND_YN을 Y로 변경
            #region SC_DATA의 HOST_SEND_YN을 Y로 변경
            // 2.MES SEND STATUS ('N' -> 'Y') UPDATE
            m_BDb.BeginTrans();
            m_BDb.ParamsClear();

            m_strSql = "";
            m_strSql += modDefApp.CRLF + " UPDATE SC_DATA                    ";
            m_strSql += modDefApp.CRLF + "    SET HOST_SEND_YN = 'Y'         ";
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            //m_strSql += modDefApp.CRLF + "    AND PLC_NO       = " + m_BDb.ParamsAdd("PLC_NO", strPLC_NO);
            m_strSql += modDefApp.CRLF + "    AND HOST_SEND_YN = 'N'         ";
            int nRtn = m_BDb.ExcuteNonQry_Par(ref m_strSql);
            if (nRtn < 0)
            {
                m_BDb.RollbackTrans();
                return false;
            }
            //if (nRtn == 0)
            //{
            //    m_BDb.RollbackTrans();
            //    return false;
            //}

            #endregion
            #region CV_DATA의 HOST_SEND_YN을 Y로 변경
            m_BDb.ParamsClear();

            m_strSql = "";
            m_strSql += modDefApp.CRLF + " UPDATE CV_DATA                    ";
            m_strSql += modDefApp.CRLF + "    SET HOST_SEND_YN = 'Y'         ";
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "    AND MC_NO        IN (" + strMC_NO_LIST + ") ";
            m_strSql += modDefApp.CRLF + "    AND HOST_SEND_YN = 'N'         ";
            nRtn = m_BDb.ExcuteNonQry_Par(ref m_strSql);
            if (nRtn < 0)
            {
                m_BDb.RollbackTrans();
                return false;
            }
            //if (nRtn == 0)
            //{
            //    m_BDb.RollbackTrans();
            //    return false;
            //}


            #endregion
            #region EQP_MST의 HOST_SEND_YN을 Y로 변경
            m_BDb.ParamsClear();

            m_strSql = "";
            m_strSql += modDefApp.CRLF + " UPDATE EQP_MST                                                       ";
            m_strSql += modDefApp.CRLF + "    SET UPD_DT       = " + modDateTime.SYSDATE;
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "    AND EQP_TYP      = 'HOST2'                                        ";
            nRtn = m_BDb.ExcuteNonQry_Par(ref m_strSql);
            if (nRtn < 0)
            {
                m_BDb.RollbackTrans();
                return false;
            }
            //if (nRtn == 0)
            //{
            //    m_BDb.RollbackTrans();
            //    return false;
            //}
            #endregion
            m_BDb.CommitTrans();

            #endregion

            return true;
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200727
        //설명		    : 무게 보고 
        private bool GetWeightReport()
        {
            string strTitle = "[GetWeightReport] .. ";
            m_strHostCmd = "U";

            //if (!m_blSockConnected)
            //{
            //    return false;
            //}

            int nJobType = 0;

            #region 무게 보고해야할 작업이 있는지?
            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "  SELECT  WD.*, CD.LUGG_NO_RD ";
            m_strSql += modDefApp.CRLF + "   FROM  WC_DATA WD";
            m_strSql += modDefApp.CRLF + "  INNER  JOIN CV_DATA CD";
            m_strSql += modDefApp.CRLF + "     ON  CD.MC_NO = WD.WC_MC_NO            ";
            m_strSql += modDefApp.CRLF + "  WHERE  WD.WH_TYP   = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            //m_strSql += modDefApp.CRLF + "    AND  WD.WC_MC_NO = '104'";                  // 어차피 WC가 1개 뿐이므로 1개만 가져올 것이므로 주석 처리 함!
            //m_strSql += modDefApp.CRLF + "    AND  WD.OD_RQ_ID = 'RQ'";
            m_strSql += modDefApp.CRLF + "    AND  CD.LUGG_NO_RD   NOT IN ('0000', '0')";
            m_strSql += modDefApp.CRLF + "    AND  WD.USE_YN   = 'Y'";
            m_strSql += modDefApp.CRLF + "  ORDER  BY       WD.UPD_DT DESC LIMIT 1";             // 입력할때 Update 시간도 함께 처리

            int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (iCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return false;
            }

            // 보고할 작업이 없으면
            if (iCnt == 0)
            {
                return true;
            }
            #endregion

            string strLUGG_NO_RD = "" + m_BDb.dtMain.Rows[0]["LUGG_NO_RD"].ToString() == "" ? "0000" : m_BDb.dtMain.Rows[0]["LUGG_NO_RD"].ToString();
            string strWEIGHT_DATA = "" + m_BDb.dtMain.Rows[0]["WEIGHT_RCV_VAL"].ToString() == "" ? "0000000" : m_BDb.dtMain.Rows[0]["WEIGHT_RCV_VAL"].ToString();
            string strCHK_BYPASS_YN = "" + m_BDb.dtMain.Rows[0]["CHK_BYPASS_YN"].ToString() == "" ? "N" : m_BDb.dtMain.Rows[0]["CHK_BYPASS_YN"].ToString();
            string strOD_RQ_ID = "" + m_BDb.dtMain.Rows[0]["OD_RQ_ID"].ToString() == "" ? "OK" : m_BDb.dtMain.Rows[0]["OD_RQ_ID"].ToString();
            string strWC_MC_NO = "" + m_BDb.dtMain.Rows[0]["WC_MC_NO"].ToString() == "" ? "000" : m_BDb.dtMain.Rows[0]["WC_MC_NO"].ToString();

            #region 메세지 보내기 체크 
            if (strCHK_BYPASS_YN != "Y")
            {
                if (strOD_RQ_ID != "RQ")
                {
                    return true;
                }
                #region 상위에 보낼 메세지 구성
                //전문 작성
                string strTemp = null;
                byte[] bytTempByte = null;

                strTemp = string.Format("UG{0:0000}00000000{1:0000000}", Convert.ToInt32(strLUGG_NO_RD), Convert.ToInt32(strWEIGHT_DATA));

                int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
                //MSG_ORDER_CNT

                m_bytTxBuff = new byte[iTxCnt];

                //### Header ###
                MakeHeader(strTemp.Length);
                //MSG_ORDER_CNT


                //### Body ###
                m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;


                bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
                Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);

                m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;
                #endregion

                #region 메세지 보내기
                if (!RequestSrv(iTxCnt.ToString()))
                {
                    return false;
                }

                #endregion

                #region 보고 상태를 OK로 상태 변경
                m_BDb.BeginTrans();
                m_BDb.ParamsClear();

                m_strSql = modDefApp.CRLF + "  UPDATE WC_DATA";
                m_strSql += modDefApp.CRLF + "    SET OD_RQ_ID  = 'OK'";
                m_strSql += modDefApp.CRLF + "      , UPD_USER_ID  = 'HOST_TASK'";
                m_strSql += modDefApp.CRLF + "      , UPD_DT  = " + modDateTime.SYSDATE;
                m_strSql += modDefApp.CRLF + "  WHERE WH_TYP  = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                m_strSql += modDefApp.CRLF + "    AND OD_RQ_ID = 'RQ'";
                m_strSql += modDefApp.CRLF + "    AND USE_YN   = 'Y'";
                //m_strSql += modDefApp.CRLF + "    AND  WD.WC_MC_NO = '104'";       // 어차피 WC가 1개 뿐이므로 1개만 가져올 것이므로 주석 처리 함!
                //m_strSql += modDefApp.CRLF + "  ORDER  BY       WD.UPD_DT";                                               // 입력할때 Update 시간도 함께 처리

                m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                if (m_iSelCnt < 0)
                {
                    m_strLog = m_BDb.ErrMsg + m_strSql;
                    modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                    m_BDb.RollbackTrans();
                    return false;
                }
                if (m_iSelCnt == 0)
                {
                    m_strLog = "WC_DATA UPDATE 처리 실패";
                    modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                    m_BDb.RollbackTrans();
                    return false;
                }
                #endregion

            }

            #endregion

            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "  SELECT  CD.*                                     ";
            m_strSql += modDefApp.CRLF + "   FROM  CV_DATA CD";
            m_strSql += modDefApp.CRLF + "  INNER  JOIN JOB_MST JM";
            m_strSql += modDefApp.CRLF + "     ON  CD.LUGG_NO_RD    = JM.LUGG_NO            ";
            m_strSql += modDefApp.CRLF + "    AND  CD.WH_TYP        = JM.WH_TYP            ";
            m_strSql += modDefApp.CRLF + "  WHERE  CD.WH_TYP        = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "    AND  CD.DEST_POS_RD   = '" + strWC_MC_NO + "'";
            //m_strSql += modDefApp.CRLF + "    AND  CD.DEST_POS_OD   = '" + strWC_MC_NO + "'";
            m_strSql += modDefApp.CRLF + "    AND  CD.RET_READY_RD  = '1'";
            m_strSql += modDefApp.CRLF + "    AND  JM.JOB_STATUS    = '11'";
            m_strSql += modDefApp.CRLF + "    AND  JM.WC_STEP       = '0'";

            iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (iCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                m_BDb.RollbackTrans();
                return false;
            }

            // 보고할 작업이 없으면
            if (iCnt == 0)
            {
                m_BDb.RollbackTrans();
                return true;
            }

            #region 작업상태를 16으로 변경 
            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "  UPDATE JOB_MST";
            m_strSql += modDefApp.CRLF + "    SET JOB_STATUS  = '16'";
            m_strSql += modDefApp.CRLF + "      , WC_STEP     = '1'";
            m_strSql += modDefApp.CRLF + "      , UPD_USER_ID = 'HOST_TASK'";
            m_strSql += modDefApp.CRLF + "      , UPD_DT  = " + modDateTime.SYSDATE;
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP  = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "    AND LUGG_NO = " + m_BDb.ParamsAdd("LUGG_NO", strLUGG_NO_RD);
            m_strSql += modDefApp.CRLF + "    AND WC_STEP = '0'";

            m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

            if (m_iSelCnt < 0)
            {
                m_strLog = strTitle + m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                m_BDb.RollbackTrans();
                return false;
            }
            if (m_iSelCnt == 0)
            {
                m_strLog = strTitle + "JOB_MST UPDATE 처리 실패";
                modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                m_BDb.RollbackTrans();
                return false;
            }

            string strTempTemp = (strCHK_BYPASS_YN == "Y") ? "[BYPASS 모드]" : "[무게:" + strWEIGHT_DATA + "]";

            m_strLog = strTitle + "무게 보고 했습니다.[작업번호:" + strLUGG_NO_RD + "]" + strTempTemp;
            modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_IMP);
            m_BDb.CommitTrans();

            #endregion

            return true;
        }



        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 에러 보고  
        private void GetErrorReport()
        {
            string[] strMC_TYP = new string[] { "CV", "SC" };

            int nCount = strMC_TYP.Length;
            for (int i = 0; i < nCount; i++)
            {
                GetErrorReport(strMC_TYP[i]);
            }
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 에러 보고 
        private bool GetErrorReport(string strEQP_TYP)
        {
            string strTitle = "[GetErrorReport] .. ";
            m_strHostCmd = "E";

            if (!m_blSockConnected)
            {
                return false;
            }

            int nJobType = 0;

            #region 에러 보고해야할 작업이 있는지?

            string strDeviceNo = "";
            string strDeviceClass = "";
            string strErrorCode = "";
            string strErrorKind = "";
            string strLuggNo = "";
            string strBank = "";
            string strBay = "";
            string strLevel = "";

            int nResult = IsEquip_ERROR_Modified(strEQP_TYP, ref strDeviceNo, ref strDeviceClass, ref strErrorCode, ref strErrorKind, ref strLuggNo, ref strBank, ref strBay, ref strLevel);
            if (nResult == 0)
            {
                // 이미 함수내에서 리스트 박스에 디스플레이함!    - 작업해야 할 부분 
                return false;
            }
            if (nResult < 0)
            {
                return true;
            }
            #endregion

            #region 상위에 보낼 메세지 구성
            //전문 작성
            string strTemp = null;
            byte[] bytTempByte = null;

            strTemp = string.Format("E{0:0}{1:000}{2:0}{3:0000}{4:0000}{5:00}{6:000}{7:00}",
                Convert.ToInt32(strDeviceClass), Convert.ToInt32(strDeviceNo), Convert.ToInt32(strErrorKind), 
                Convert.ToInt32(strErrorCode), Convert.ToInt32(strLuggNo), 
                Convert.ToInt32(strBank), Convert.ToInt32(strBay), Convert.ToInt32(strLevel));

            int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            //MSG_ORDER_CNT

            m_bytTxBuff = new byte[iTxCnt];

            //### Header ###
            MakeHeader(strTemp.Length);
            //MSG_ORDER_CNT


            //### Body ###
            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;


            bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);

            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            #endregion

            #region 메세지 보내기
            if (!RequestSrv(iTxCnt.ToString()))
            {
                return false;
            }
            #endregion

            #region 응답 받고 특별히 하는 작업 없음!
            //nResult = modDefApp.g_frmForm.DeleteJobMst();     // 함수안에서 Transaction 처리함!
            //if (nResult != 1)
            //{
            //    m_strLog = string.Format("작업 삭제 실패하였습니다. [작업번호:{0}][실패구분:{1}][실패내용:{2}]", strLuggNo, nResult, modDefApp.GM_RTN_MSG);
            //    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);

            //    return false;
            //}

            //m_strLog = string.Format("작업 삭제 성공하였습니다. [작업번호:{0}]", strLuggNo);
            //modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_IMP);
            #endregion

            return true;
        }


        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 완료 보고  
        private void GetJobCompleteReport()
        {
            int[] nStation = new int[] { 19, 29 };

            int nCount = nStation.Length;
            for (int i = 0; i < nCount; i++)
            {
                GetJobCompleteReport(nStation[i]);
            }
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 완료 보고 
        private bool GetJobCompleteReport(int nJobStatus)
        {
            string strTitle = "[GetJobCompleteReport] .. ";
            m_strHostCmd = "F";

            int nJobType = 0;
            string strUserID = "";
            string strSScNum = "";
            string strDScNum = "";
            string strSPosition = "";
            string strDPosition = "";
            string strLuggNum = "";

            #region 완료 보고해야할 작업이 있는지?

            int nResult = IsJobExist(nJobStatus, ref nJobType, ref strUserID, ref strSScNum, ref strDScNum, ref strSPosition, ref strDPosition, ref strLuggNum);
            if (nResult == 0)
            {
                // 이미 함수내에서 리스트 박스에 디스플레이함!
                return false;
            }
            if (nResult < 0)
            {
                return true;
            }
            #endregion

            #region 상위에 보낼 메세지 구성
            //전문 작성
            string strTemp = null;
            byte[] bytTempByte = null;
            int nClass = 0;
            int nStation = 0;

            int nLuggNum = Convert.ToInt32(strLuggNum);
            bool bResult = false;
            if (nLuggNum >= 9000 && modDefApp.g_frmForm.chkSimMode.Checked == false)
            {
                m_strLog = "작업정보는 존재하지만 온라인 작업이 아니므로 작업완료 처리합니다. [작업 번호:" + strLuggNum + "]";
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_IMP);

                #region 반자동 작업은 그냥 작업 삭제
                //modDefApp.g_frmForm.DeleteJobMst(false);

                bResult = modDefApp.g_frmForm.DeleteJobMst(m_BDb, true, strLuggNum);     // 함수안에서 Transaction 처리함!
                if (bResult == false)
                {
                    m_strLog = string.Format("작업 삭제 실패하였습니다. [작업번호:{0}][실패내용:{1}]", strLuggNum, modDefApp.GM_RTN_MSG);
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);

                    return false;
                }

                m_strLog = string.Format("작업 삭제 성공하였습니다. [작업번호:{0}]", strLuggNum);
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
                #endregion

                return true;                
            }
            //string strLuggNum = "" + m_BDb.dtMain.Rows[0]["LUGG_NO"];
            switch (nJobType)
            {
                case 1: nStation = Convert.ToInt16(strSPosition); nClass = 1;  break;
                case 2: nStation = Convert.ToInt16(strDPosition); nClass = 2;  break;
                //case 3: nStation = Convert.ToInt16(strDPosition); break;
                case 4: nStation = 0;                             nClass = 3; break; //랙투랙 부분 조한성 수정 0608
                case 5: nStation = 0;                             nClass = 3; break; 
                case 6: nStation = Convert.ToInt32(strDPosition); nClass = 3; break;
                default:
                    m_strLog = "작업정보는 존재하지만 잘못된 작업 정보입니다.[작업 타입:" + nJobType.ToString() + "]";
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                    return false;
            }




            strTemp = string.Format("F{0:0}{1:0000}{2:0}1", nJobType, nLuggNum, nClass);

            int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            //MSG_ORDER_CNT

            m_bytTxBuff = new byte[iTxCnt];

            //### Header ###
            MakeHeader(strTemp.Length);
            //MSG_ORDER_CNT


            //### Body ###
            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;


            bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);

            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            #endregion

            #region 시뮬레이터 모드일때는 상위로 메세지를 보내지 않고, 새로운 작업을 생성한다.
            if (modDefApp.g_frmForm.chkSimMode.Checked == true)
            {
                m_BDb.BeginTrans();

                int nNewJobType = 0;
                switch (nJobType)
                {
                    case 1:                        
                        #region 기존 작업 삭제
                        if (modDefApp.g_frmForm.DeleteJobMst(m_BDb, false, strLuggNum) == false)
                        {
                            m_strLog = string.Format("SIM MODE 작업 (완료)삭제 실패하였습니다. [작업번호:{0}][실패내용:{1}]", strLuggNum, modDefApp.GM_RTN_MSG);
                            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);

                            m_BDb.RollbackTrans();
                            return false;
                        }

                        m_strLog = string.Format("SIM MODE 작업 (완료)삭제 성공하였습니다. [작업번호:{0}]", strLuggNum);
                        modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
                        #endregion
                        #region 출고 작업 생성
                        nNewJobType = 2;
                        //m_BDb.BeginTrans();

                        m_BDb.ParamsClear();
                        if (modDefApp.g_frmForm.InsertJobMst(m_BDb, strLuggNum, strDScNum, strDPosition, strSPosition, "00-000-00", nNewJobType.ToString()) == false)
                        {
                            // 함수안에서 실패시 화면에 메세지 출력함!
                            //MakeResponse(m_strMsgType, strLuggNum, modDefApp.MSG_ECS_BUFFER_FULL);
                            m_BDb.RollbackTrans();
                            break;
                        }
                        m_BDb.CommitTrans();

                        modCmWork.ShowMsgServer(strTitle + "SIM MODE 작업 추가 되었습니다.[작업번호:" + strLuggNum + "]", modDefApp.MSG_NOR);

                        #endregion
                        break;
                    case 2:      
                        #region 입고 작업 생성
                        nNewJobType = 1;
                        
                        #region CELL MST에서 BANK와 BAY와 LEVEL의 최대값을 가져온다.
                        m_BDb.ParamsClear();

                        m_strSql = modDefApp.CRLF + "   SELECT  MAX(BANK) AS MAX_BANK";
                        m_strSql += modDefApp.CRLF + "       ,  MIN(BANK) AS MIN_BANK";
                        m_strSql += modDefApp.CRLF + "       ,  MAX(BAY) AS MAX_BAY";
                        m_strSql += modDefApp.CRLF + "       ,  MAX(LEV) AS MAX_LEVEL";
                        m_strSql += modDefApp.CRLF + "    FROM  CELL_MST          ";
                        m_strSql += modDefApp.CRLF + "   WHERE  WH_TYP = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                        m_strSql += modDefApp.CRLF + "     AND  CELL_SC_NO = '" + strSScNum + "'"; 
                        m_strSql += modDefApp.CRLF + "GROUP BY  SC_NO             ";
                        m_strSql += modDefApp.CRLF + "ORDER BY  SC_NO             ";

                        int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

                        if (iCnt < 0)
                        {
                            m_strLog = m_BDb.ErrMsg + m_strSql;
                            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                            return false;
                        }

                        // CELL_MST에서 각 조건에 맞는 최대값을 가져오지 못할수는 없을것이다.   
                        if (iCnt != 1)
                        {
                            m_strLog = "LOCATION 정보를 바르게 가져오지 못했습니다.";
                            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                            return false;
                        }

                        string strMIN_BANK = "" + m_BDb.dtMain.Rows[0]["MIN_BANK"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["MIN_BANK"].ToString();
                        string strMAX_BANK = "" + m_BDb.dtMain.Rows[0]["MAX_BANK"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["MAX_BANK"].ToString();
                        string strMAX_BAY = "" + m_BDb.dtMain.Rows[0]["MAX_BAY"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["MAX_BAY"].ToString();
                        string strMAX_LEVEL = "" + m_BDb.dtMain.Rows[0]["MAX_LEVEL"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["MAX_LEVEL"].ToString();

                        int nMinBank = Convert.ToInt32(strMIN_BANK);
                        int nMaxBank = Convert.ToInt32(strMAX_BANK);
                        int nMaxBay = Convert.ToInt32(strMAX_BAY);
                        int nMaxLevel = Convert.ToInt32(strMAX_LEVEL);

                        #endregion
                        #region 현재 로케이션의 BANK, BAY, LEVEL을 추출한다. 
                        string strCurBank = strSPosition.Substring(0, 2);
                        string strCurBay = strSPosition.Substring(3, 3);
                        string strCurLevel = strSPosition.Substring(7, 2);

                        int nCurBank = Convert.ToInt32(strCurBank);
                        int nCurBay = Convert.ToInt32(strCurBay);
                        int nCurLevel = Convert.ToInt32(strCurLevel);
                        #endregion
                        #region BAY -> LEVEL -> BANK 순으로 증가한다.


                        if (++nCurBay > nMaxBay)
                        {
                            nCurBay = 1;
                            ++nCurLevel;
                        }
                        
                        if (nCurLevel > nMaxLevel)
                        {
                            nCurLevel = 1;
                            ++nCurBank;
                        }

                        if (nCurBank > nMaxBank)
                        {
                            nCurBank = nMinBank;
                        }

                        #endregion

                        // 증가된 값을 가져온다
                        string strLocation = "";
                        strLocation = string.Format("{0:00}-{1:000}-{2:00}", nCurBank, nCurBay, nCurLevel);

                        
                        // 생성된 Location이  올바른지 체크!
                        string strCELL_USE_YN = "";
                        string strSC_PLT_JOB_TYP = "";
                        if (modDefApp.g_frmForm.IsValidLocation(m_BDb, strSScNum, strLocation, ref strCELL_USE_YN, ref strSC_PLT_JOB_TYP) == false)
                        {
                            m_strLog = string.Format("Sim Test를 위해 생성된 Location이 올바르지 않습니다.[작업번호:{0}][도착지:{1}][도착LOC:{2}]", nLuggNum, strSScNum, strLocation);
                            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                            //MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            m_BDb.RollbackTrans();
                            break;
                        }

                        if (strCELL_USE_YN != "Y")
                        {
                            m_strLog = string.Format("도착 Location이 금지랙 입니다. 다음 로케이션을 검색하도록 준비합니다.[작업번호:{0}][도착지:{1}][다음도착LOC:{2}]", nLuggNum, strSScNum, strLocation);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);

                            m_BDb.ParamsClear();

                            m_strSql = modDefApp.CRLF + "  UPDATE JOB_MST ";
                            m_strSql += modDefApp.CRLF + "    SET START_LOCATION= " + m_BDb.ParamsAdd("START_LOCATION", strLocation);     
                            m_strSql += modDefApp.CRLF + "      , UPD_USER_ID  = 'HOST_TASK'";
                            m_strSql += modDefApp.CRLF + "      , UPD_DT       = " + modDateTime.SYSDATE;
                            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                            m_strSql += modDefApp.CRLF + "    AND LUGG_NO      = " + m_BDb.ParamsAdd("LUGG_NO", strLuggNum);

                            iCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                            if (iCnt < 0)
                            {
                                m_strLog = m_BDb.ErrMsg + m_strSql;
                                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                                m_BDb.RollbackTrans();
                                return false;
                            }

                            // CELL_MST에서 각 조건에 맞는 최대값을 가져오지 못할수는 없을것이다.   
                            if (iCnt != 1)
                            {
                                m_strLog = "작업 정보를 바르게 가져오지 못했습니다.[작업번호:" + strLuggNum + "]";
                                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                                m_BDb.RollbackTrans();
                                return false;
                            }
                            m_strLog = "금지랙이라서 로케이션을 수정합니다.[작업번호:" + strLuggNum + "][현재LOC:" + strSPosition + "][신규LOC:" + strLocation + "]";
                            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_IMP);
                                
                            m_BDb.CommitTrans();
                            return false;
                        }

                        #region 기존 작업 삭제
                        if (modDefApp.g_frmForm.DeleteJobMst(m_BDb, false, strLuggNum) == false)
                        {
                            m_strLog = string.Format("SIM MODE 작업 (완료)삭제 실패하였습니다. [작업번호:{0}][실패내용:{1}]", strLuggNum, modDefApp.GM_RTN_MSG);
                            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);

                            m_BDb.RollbackTrans();
                            return false;
                        }

                        m_strLog = string.Format("SIM MODE 작업 (완료)삭제 성공하였습니다. [작업번호:{0}]", strLuggNum);
                        modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
                        #endregion          
              
                        m_BDb.ParamsClear();
                        if (modDefApp.g_frmForm.InsertJobMst(m_BDb, strLuggNum, strDPosition, "00-000-00", strSScNum, strLocation, nNewJobType.ToString()) == false)
                        {
                            // 함수안에서 실패시 화면에 메세지 출력함!
                            //MakeResponse(m_strMsgType, strLuggNum, modDefApp.MSG_ECS_BUFFER_FULL);
                            m_BDb.RollbackTrans();
                            break;
                        }
                        m_BDb.CommitTrans();

                        modCmWork.ShowMsgClient(strTitle + "SIM MODE 작업 추가 되었습니다.[작업번호:" + strLuggNum + "]", modDefApp.MSG_NOR);
                        #endregion
                        break;
                }
                return true;
            }
            #endregion

            if (!m_blSockConnected)
            {
                return false;
            }
            
            #region 메세지 보내기
            if (!RequestSrv(iTxCnt.ToString()))
            {
                return false;
            }
            #endregion

            #region 응답 받고 작업 삭제
            //modDefApp.g_frmForm.DeleteJobMst(false);

            bResult = modDefApp.g_frmForm.DeleteJobMst(m_BDb, true, strLuggNum);     // 함수안에서 Transaction 처리함!
            if (bResult == false)
            {
                m_strLog = string.Format("작업 삭제 실패하였습니다. [작업번호:{0}][실패내용:{1}]", strLuggNum, modDefApp.GM_RTN_MSG);
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);

                return false;
            }

            m_strLog = string.Format("작업 삭제 성공하였습니다. [작업번호:{0}]", strLuggNum);
            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
            #endregion

            #region 공파레트 출고 작업이었을 경우 요청상태를 변경한다. 
            //int nLuggNum = Convert.ToInt32(strLuggNum);

            if (modDefApp.g_bEmtpyPltJob[nLuggNum] == true)
            {
                if (modDefApp.g_frmForm.UpdateHostEmptyPlt(m_BDb, "", "", strLuggNum, "Q", "C") == false)
                {
                    m_strLog = string.Format("공파레트 출고요청 관련 완료처리를 하지 못했습니다. [작업번호:{0}][실패내용:{1}]", strLuggNum, modDefApp.GM_RTN_MSG);
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);

                    return false;                   
                }
                m_strLog = string.Format("공파레트 출고요청 관련 완료 하였습니다. [작업번호:{0}]", strLuggNum);
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
            }
            #endregion         
            return true;
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200518
        //설명		    : 공파레트 입출고 요청 
        private void GetEmptyPltRequest()
        {
            int[] nKind = new int[] { 1, 1, 2, 2 };
            int[] nStation = new int[] { 108, 149, 108, 149 };

            //bool bResult = false;
            int nCount = nKind.Length;
            for (int i = 0; i < nCount; i++)
            {
                GetEmptyPltRequest(nKind[i], nStation[i]);
            }
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200515
        //설명		    : 공파레트 입출고 요청 
        private bool GetEmptyPltRequest(int nKind, int nStation)
        {
            string strTitle = "[GetEmptyPltRequest] .. ";
            m_strHostCmd = "P";

            if (!m_blSockConnected)
            {
                return false;
            }

            int iSend_Max = 3;
            #region 요청중인 작업이 있는지? [주석]
            //m_BDb.ParamsClear();
            //
            //m_strSql = modDefApp.CRLF + "  SELECT  * ";
            //m_strSql += modDefApp.CRLF + "   FROM  HOST_EMPTY_PLT ";
            //m_strSql += modDefApp.CRLF + "  WHERE  WH_TYP = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            //m_strSql += modDefApp.CRLF + "    AND  KIND   = " + m_BDb.ParamsAdd("KIND", nKind.ToString());
            //m_strSql += modDefApp.CRLF + "    AND  STN    = " + m_BDb.ParamsAdd("STN", nStation.ToString());
            //m_strSql += modDefApp.CRLF + "    AND  STATUS <> 'C'";                                                  // 요청중
            //m_strSql += modDefApp.CRLF + "    AND (STATUS = 'Q' OR LUGG_NO <> '0')";                                                  // 요청중
            //m_strSql += modDefApp.CRLF + "  ORDER  BY     UPD_DT";                                               // 입력할때 Update 시간도 함께 처리
            //
            //int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
            //
            //if (iCnt < 0)
            //{
            //    m_strLog = m_BDb.ErrMsg + m_strSql;
            //    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
            //    return false;
            //}
            //
            //// 요청중인 작업이 1개라도 있으면 
            //if (iCnt > 0)
            //{
            //    //m_strLog = "이미 요청 중인 작업이 있으므로 요청하지 않음![요청시간 : " + m_BDb.dtMain.Rows[0]["UDT_DT"] + "]";
            //    //modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
            //    return false;
            //}
            //#endregion
            //
            //#region 진행중인 작업이 있는지?  
            //m_BDb.ParamsClear();
            //
            //m_strSql = modDefApp.CRLF + "  SELECT * ";
            //m_strSql += modDefApp.CRLF + "   FROM JOB_MST ";
            //m_strSql += modDefApp.CRLF + "  WHERE WH_TYP    = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            //switch (nKind)
            //{
            //    case 1:
            //        m_strSql += modDefApp.CRLF + "AND START_POS = " + m_BDb.ParamsAdd("STN", nStation.ToString());
            //        iSend_Max = 1;      // 입고시에는 1개
            //        break;
            //    case 2:
            //        m_strSql += modDefApp.CRLF + "AND DEST_POS = " + m_BDb.ParamsAdd("STN", nStation.ToString());
            //        iSend_Max = 1;      // 출고시에는 1개???
            //        break;
            //    default:
            //        return false;
            //}
            //
            //iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
            //
            //if (iCnt < 0)
            //{
            //    m_strLog = m_BDb.ErrMsg + m_strSql;
            //    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
            //    return false;
            //}
            //
            //// 진행중인 작업이 최대작업보다 크거나 같을 경우  
            //if (iCnt >= iSend_Max)
            //{
            //    //m_strLog = "이미 진행 중인 작업이 있으므로 요청하지 않음![작업갯수 : " + iCnt.ToString() + "]";
            //    //modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
            //    return false;
            //}
            #endregion

            #region 요청해야할 작업이 있는지?
            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "  SELECT * ";
            m_strSql += modDefApp.CRLF + "   FROM HOST_EMPTY_PLT ";
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP  = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "    AND KIND    = " + m_BDb.ParamsAdd("KIND", nKind.ToString());
            m_strSql += modDefApp.CRLF + "    AND STN     = " + m_BDb.ParamsAdd("STN", nStation.ToString());
            m_strSql += modDefApp.CRLF + "    AND STATUS  = 'N'";                                                // 신규
            m_strSql += modDefApp.CRLF + "    AND LUGG_NO = '0'";                                                // 작업번호가 없음!
            m_strSql += modDefApp.CRLF + "  ORDER BY     UPD_DT";                                                // 입력할때 Update 시간도 함께 처리

            int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (iCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return false;
            }

            // 요청중인 작업이 없을때
            if (iCnt == 0)
            {
                //m_strLog = "요청 중인 작업이 없음![요청시간 : " + m_BDb.dtMain.Rows[0]["UDT_DT"] + "]";
                //modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return true;
            }
            #endregion

            //요청중인 작업이 1개라도 있으면 
            #region 상위에 보낼 메세지 구성
            //전문 작성
            string strTemp = null;
            byte[] bytTempByte = null;

            strTemp = string.Format("P{0}{1:000}", nKind, nStation);

            int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            //MSG_ORDER_CNT

            m_bytTxBuff = new byte[iTxCnt];

            //### Header ###
            MakeHeader(strTemp.Length);
            //MSG_ORDER_CNT


            //### Body ###
            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;


            bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);

            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            #endregion

            #region 메세지 보내기
            if (!RequestSrv(iTxCnt.ToString()))
            {
                return false;
            }
            #endregion

            #region 요청 중으로 상태 변경 
            m_BDb.BeginTrans();

            //### manual_temp 갱신
            //### Status UPDATE
            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "  UPDATE HOST_EMPTY_PLT ";
            m_strSql += modDefApp.CRLF + "    SET STATUS  = 'Q'";
            m_strSql += modDefApp.CRLF + "      , UPD_ID  = 'HOST_TASK'";
            m_strSql += modDefApp.CRLF + "      , UPD_DT  = " + modDateTime.SYSDATE;
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP  = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "    AND KIND    = " + m_BDb.ParamsAdd("KIND", nKind);
            m_strSql += modDefApp.CRLF + "    AND STN     = " + m_BDb.ParamsAdd("STN", nStation);
            m_strSql += modDefApp.CRLF + "    AND STATUS  = 'N'";
            m_strSql += modDefApp.CRLF + "    AND LUGG_NO = '0'";

            m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

            if (m_iSelCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                m_BDb.RollbackTrans();
                return false;
            }
            if (m_iSelCnt != 1)
            {
                strTemp = (nKind == 1) ? "공파레트 입고, 작업대 : " : "공파레트 출고, 작업대 : ";
                m_strLog = "작업 처리 실패,[" + strTemp + nStation.ToString() + "]" + modDefApp.CRLF + m_strSql;
                modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                m_BDb.RollbackTrans();
                return false;
            }

            modDefApp.g_strEmtpyPltKind = nKind.ToString();             // 작업 생성될때 확인
            modDefApp.g_strEmtpyPltStation = nStation.ToString();       // 작업 생성될때 확인

            m_BDb.CommitTrans();

            #endregion

            return true;
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200518
        //설명		    : 도착 보고  
        private void GetLoadArrivalReport()
        {
            int[] nStation = new int[] { 12, 22};

            int nCount = nStation.Length;
            for (int i = 0; i < nCount; i++)
            {
                GetLoadArrivalReport(nStation[i]);
            }
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200515
        //설명		    : 도착보고 
        private bool GetLoadArrivalReport(int nJobStatus)
        {
            string strTitle = "[GetLoadArrivalReport] .. ";
            m_strHostCmd = "A";

            int nJobType = 0;
            string strUserID = "";
            string strSScNum = "";
            string strDScNum = "";
            string strSPosition = "";
            string strDPosition = "";
            string strLuggNum = "";

            #region 도착 보고해야할 작업이 있는지?

            int nResult = IsJobExist(nJobStatus, ref nJobType, ref strUserID, ref strSScNum, ref strDScNum, ref strSPosition, ref strDPosition, ref strLuggNum);
            if (nResult == 0)
            {
                return false;
            }
            if (nResult < 0)
            {
                return true;
            }
            #endregion

            #region 보고를 못했어도 그냥 지나감! - 다시 보고안하기 위해서 먼저 업데이트 하고 넘어감
            m_BDb.BeginTrans();

            //### manual_temp 갱신
            //### Status UPDATE
            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "  UPDATE JOB_MST ";
            m_strSql += modDefApp.CRLF + "    SET JOB_STATUS   = " + m_BDb.ParamsAdd("JOB_STATUS", nJobStatus + 6);      // 도착 보고에서 도착보고완료로 수정 
            m_strSql += modDefApp.CRLF + "      , UPD_USER_ID  = 'HOST_TASK'";
            m_strSql += modDefApp.CRLF + "      , UPD_DT       = " + modDateTime.SYSDATE;
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "    AND LUGG_NO      = " + m_BDb.ParamsAdd("LUGG_NO", strLuggNum);

            m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

            if (m_iSelCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                m_BDb.RollbackTrans();
                return false;
            }
            if (m_iSelCnt != 1)
            {
                m_strLog = "도착 보고 실패,[작업번호 : " + strLuggNum + "] " + m_strSql;
                modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                m_BDb.RollbackTrans();
                return false;
            }

            m_BDb.CommitTrans();

            #endregion


            if (!m_blSockConnected)
            {
                return false;
            }

            #region 상위에 보낼 메세지 구성
            //전문 작성
            string strTemp = null;
            byte[] bytTempByte = null;
            int nKind = 0;
            int nStation = 0;

            int nLuggNum = Convert.ToInt32(strLuggNum);

            if (nLuggNum >= 9000 && modDefApp.g_frmForm.chkSimMode.Checked == false)
            {
                m_strLog = "작업정보는 존재하지만 온라인 작업이 아닙니다. [작업 번호:" + strLuggNum + "]";
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return false;
            }
            //string strLuggNum = "" + m_BDb.dtMain.Rows[0]["LUGG_NO"];
            switch (nJobType)
            {
                case 1: nStation = Convert.ToInt16(strSPosition); break;
                case 2: nStation = Convert.ToInt16(strDPosition); break;
                //case 3: nStation = Convert.ToInt16(strDPosition); break;
                //case 4: nStation = Convert.ToInt16(strDPosition); break;
                //case 5: nStation = Convert.ToInt16(strDPosition); break;
                //case 6: nStation = Convert.ToInt16(strDPosition); break;
            }


            strTemp = string.Format("A{0}{1:0000}{2:000}", nJobType, Convert.ToInt32(strLuggNum), nStation);

            int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            //MSG_ORDER_CNT

            m_bytTxBuff = new byte[iTxCnt];

            //### Header ###
            MakeHeader(strTemp.Length);
            //MSG_ORDER_CNT


            //### Body ###
            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;


            bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);

            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            #endregion

            #region 시뮬레이터 모드일때는 상위로 메세지를 보내지 않고, 작업상태를 변경한다. 
            
            //if (modDefApp.g_frmForm.chkSimMode.Checked == true)
            //{
            //    return true;
            //    //*/
            //}
            #endregion


            #region 메세지 보내기
            if (!RequestSrv(iTxCnt.ToString()))
            { 
                return false;
            }
            #endregion

            return true;
        }


		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 데이타테이블 초기화
		private void DataTable_Dispose()
		{
			m_BDb.dtMain.Dispose();
			m_BDb.dtMain.Reset();
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: ECS직접지시정보구하기
		public bool GetDirOrder()
		{
			int iSend_Loop = 0;
			int iSend_Max = 0;
			int iCnt = 0;
			string strReasonCode = "00";
			//이상없음
			string strReasoneInfo = null;
			string strECS_DIR_DATE = null;
			string strECS_DIR_TIME = null;
			string strECS_DIR_SEQ = null;
			string strECS_DIR_DAT = null;

			if (!m_blSockConnected)
			{
				return false;
			}

			m_BDb.ParamsClear();

			m_strSql = modDefApp.CRLF + "  SELECT * ";
			m_strSql += modDefApp.CRLF + "   FROM ECS_DIR_INF ";
			m_strSql += modDefApp.CRLF + "  WHERE SYS_GRP     =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
			m_strSql += modDefApp.CRLF + "    AND IF_ERR_CODE = 'ECS00' ";
			m_strSql += modDefApp.CRLF + "  Order By ECS_DIR_DATE, ECS_DIR_TIME, ECS_DIR_SEQ ";

			iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

			if (iCnt < 0)
			{
				m_strLog = m_BDb.ErrMsg + m_strSql;
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				return false;
			}
			if (iCnt == 0)
			{
				return false;
			}

			iSend_Max = 1;
			for (iSend_Loop = 0; iSend_Loop <= iCnt - 1; iSend_Loop++)
			{
				if (!m_blSockConnected)
				{
					return false;
				}
				if (iSend_Loop >= iSend_Max)
				{
					break; // TODO: 원본은 Exit For 이었음. 동작이 다를 수 있으니 확인 필요
				}

				strECS_DIR_DATE = "" + m_BDb.dtMain.Rows[iSend_Loop]["ECS_DIR_DATE"];
				strECS_DIR_TIME = "" + m_BDb.dtMain.Rows[iSend_Loop]["ECS_DIR_TIME"];
				strECS_DIR_SEQ = "" + m_BDb.dtMain.Rows[iSend_Loop]["ECS_DIR_SEQ"];
				strECS_DIR_DAT = "" + m_BDb.dtMain.Rows[iSend_Loop]["ECS_DIR_DAT"];

				strReasonCode = "00";//이상없음
				//### 전송
				if (!SendDirOrder(strECS_DIR_DAT))
				{
					return false;
				}

				//### 수신 전문 로그
				m_strLog = System.Text.Encoding.UTF8.GetString(m_bytRxBuff);
				m_strLog = m_strLog.TrimEnd(ControlChars.NullChar);
				if (m_bytRxBuff[modDefApp.MSG_HEAD_CNT + 2] == modDefApp.TRANS_NAK)
				{
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					strReasonCode = m_strLog.Substring(modDefApp.MSG_HEAD_CNT + 3, 2);
					strReasoneInfo = "응답 코드[" + modCmLib.GetEcsErrInfo(strReasonCode) + "]";
					modCmWork.ShowMsgClient(strReasoneInfo, modDefApp.MSG_ERR);
					//Return False
				}
				else
				{
					modCmWork.ShowMsgClient(m_strLog);
				}

				//### 분석
				if (m_strLog.Length != 42)
				{
					m_strLog = string.Format("정의된 메세지의 길이가 아닙니다.[{0}]", m_strLog.Length);
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					strReasonCode = "99";
					return false;
				}

				m_BDb.BeginTrans();

				//### manual_temp 갱신
				//### Status UPDATE
				m_BDb.ParamsClear();

				if (strReasonCode == "00")
				{
					m_strSql = modDefApp.CRLF + "  DELETE FROM ECS_DIR_INF ";
				}
				else
				{
					m_strSql = modDefApp.CRLF + "  UPDATE ECS_DIR_INF ";
					m_strSql += modDefApp.CRLF + "    SET IF_ERR_CODE = 'ECS" + strReasonCode + "' ";
				}
				m_strSql += modDefApp.CRLF + "  WHERE SYS_GRP      =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
				m_strSql += modDefApp.CRLF + "    AND ECS_DIR_DATE =  " + m_BDb.ParamsAdd("ECS_DIR_DATE", strECS_DIR_DATE);
				m_strSql += modDefApp.CRLF + "    AND ECS_DIR_TIME =  " + m_BDb.ParamsAdd("ECS_DIR_TIME", strECS_DIR_TIME);
				m_strSql += modDefApp.CRLF + "    AND ECS_DIR_SEQ  =  " + m_BDb.ParamsAdd("ECS_DIR_SEQ", strECS_DIR_SEQ);

				m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

				if (m_iSelCnt < 0)
				{
					m_strLog = m_BDb.ErrMsg + m_strSql;
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					m_BDb.trnMain.Rollback();
					return false;
				}
				if (m_iSelCnt != 1)
				{
					m_strLog = "작업, ECS_DIR_INF 처리 실패,[" + strECS_DIR_DATE + "," + strECS_DIR_TIME + "," + strECS_DIR_SEQ + "]" + modDefApp.CRLF + m_strSql;
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					m_BDb.trnMain.Rollback();
					return false;
				}
				m_BDb.trnMain.Commit();

			}

			return true;
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명			: 작업정보구하기
		//수정이력      : @@@ 원효재 20170928 IF_JOB_FK컬럼이 현재 표준화테이블에는 존재하지않음.
		public bool GetLuggOrder()
		{
			int iSend_Loop = 0;
			int iSend_Max = 0;
			int iCnt = 0;
			string strReasonCode1 = null;
			string strReasonCode2 = null;
			string strReasoneInfo1 = null;
			string strReasoneInfo2 = null;
			string strRecvLuggNo1 = null;
			string strRecvLuggNo2 = null;
			stuSendLuggInfo JobInfo = new stuSendLuggInfo("");
			DataTable dtLUGG_MST1 = new DataTable();
			DataTable dtLUGG_MST2 = new DataTable();
			bool blPAIR_SEND = false;
			string strPAIR_LUGGNO = null;
			int iMOD_CNT = 0;
            string strGRADE = "";
            string strMATERIAL = "";


			if (!m_blSockConnected)
			{
				return false;
			}

			m_BDb.ParamsClear();

			m_strSql = modDefApp.CRLF + "  SELECT TOP 1 A.* ";
			m_strSql += modDefApp.CRLF + "      , B.WH_CD AS SOUR_WH_CD, C.WH_CD AS DEST_WH_CD, D.MATERIAL ";
			m_strSql += modDefApp.CRLF + "      , '1' AS IF_JOB_FK ";
            m_strSql += modDefApp.CRLF + "   FROM LUGG_MST A INNER JOIN WH_MST B";
            m_strSql += modDefApp.CRLF + "                           ON A.SYS_GRP = B.SYS_GRP AND A.SOUR_WH_CD = B.WH_CD ";
            m_strSql += modDefApp.CRLF + "                   INNER JOIN WH_MST C";
            m_strSql += modDefApp.CRLF + "                           ON A.SYS_GRP = C.SYS_GRP AND A.DEST_WH_CD = C.WH_CD ";
            m_strSql += modDefApp.CRLF + "                   INNER JOIN LUGG_DTL D";
            m_strSql += modDefApp.CRLF + "                           ON A.COMPANY_CD = D.COMPANY_CD";
            m_strSql += modDefApp.CRLF + "                           AND A.AREA_CD = D.AREA_CD";
            m_strSql += modDefApp.CRLF + "                           AND A.SYS_GRP = D.SYS_GRP";
            m_strSql += modDefApp.CRLF + "                           AND A.LUGGNO = D.LUGGNO";
            m_strSql += modDefApp.CRLF + "  WHERE A.COMPANY_CD  =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
            m_strSql += modDefApp.CRLF + "    AND A.AREA_CD     =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
            m_strSql += modDefApp.CRLF + "    AND A.SYS_GRP     =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
			m_strSql += modDefApp.CRLF + "    AND A.JOB_STA     = '0' ";
			m_strSql += modDefApp.CRLF + "    AND A.IF_ERR_CODE = 'ECS00' ";
			m_strSql += modDefApp.CRLF + "  ORDER BY A.PRIORITY DESC, A.LUGG_DATE, A.LUGG_TIME, A.LUGGNO ";

			iCnt = m_BDb.ExcuteQry_Par(ref dtLUGG_MST1,ref m_strSql);

			if (iCnt < 0)
			{
				m_strLog = m_BDb.ErrMsg + m_strSql;
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				return false;
			}
			if (iCnt == 0)
			{
				return false;
			}

			iSend_Loop = 0;
			strPAIR_LUGGNO = "" + dtLUGG_MST1.Rows[iSend_Loop]["PAIR_LUGGNO"].ToString();

			if (!string.IsNullOrEmpty(strPAIR_LUGGNO) || strPAIR_LUGGNO != "0")
			{
				m_BDb.ParamsClear();

				m_strSql =  modDefApp.CRLF + "  SELECT A.* ";
				m_strSql += modDefApp.CRLF + "      , B.WH_CD AS SOUR_WH_CD, C.WH_CD AS DEST_WH_CD, D.MATERIAL ";
                m_strSql += modDefApp.CRLF + "   FROM LUGG_MST A INNER JOIN WH_MST B";
                m_strSql += modDefApp.CRLF + "                           ON A.SYS_GRP = B.SYS_GRP AND A.SOUR_WH_CD = B.WH_CD ";
                m_strSql += modDefApp.CRLF + "                   INNER JOIN WH_MST C";
                m_strSql += modDefApp.CRLF + "                           ON A.SYS_GRP = C.SYS_GRP AND A.DEST_WH_CD = C.WH_CD ";
                m_strSql += modDefApp.CRLF + "                   INNER JOIN LUGG_DTL D";
                m_strSql += modDefApp.CRLF + "                           ON A.COMPANY_CD = D.COMPANY_CD";
                m_strSql += modDefApp.CRLF + "                           AND A.AREA_CD = D.AREA_CD";
                m_strSql += modDefApp.CRLF + "                           AND A.SYS_GRP = D.SYS_GRP";
                m_strSql += modDefApp.CRLF + "                           AND A.LUGGNO = D.LUGGNO";

                m_strSql += modDefApp.CRLF + "  WHERE A.COMPANY_CD  =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                m_strSql += modDefApp.CRLF + "    AND A.AREA_CD     =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                m_strSql += modDefApp.CRLF + "    AND A.SYS_GRP     =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                m_strSql += modDefApp.CRLF + "    AND A.JOB_STA     = '0' ";
				m_strSql += modDefApp.CRLF + "    AND A.IF_ERR_CODE = 'ECS00' ";
				m_strSql += modDefApp.CRLF + "    AND A.LUGGNO =  " + m_BDb.ParamsAdd("LUGGNO", strPAIR_LUGGNO);
				m_strSql += modDefApp.CRLF + "  ORDER BY A.PRIORITY DESC, A.LUGG_DATE, A.LUGG_TIME, A.LUGGNO ";

				iCnt = m_BDb.ExcuteQry_Par(ref dtLUGG_MST2,ref m_strSql);

				if (iCnt < 0)
				{
					m_strLog = m_BDb.ErrMsg + m_strSql;
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					return false;
				}
				if (iCnt > 0)
				{
					blPAIR_SEND = true;
				}
			}



            strMATERIAL = dtLUGG_MST1.Rows[0]["MATERIAL"] + "".ToString();

            m_BDb.ParamsClear();

            m_strSql = "SELECT TOP 1 GRADE FROM ITEM_MST";
            m_strSql += modDefApp.CRLF + "WHERE MATERIAL = " + m_BDb.ParamsAdd("MATERIAL", strMATERIAL);

            iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (iCnt == 1)
                strGRADE = m_BDb.dtMain.Rows[0]["GRADE"].ToString();



            JobInfo.strMessageType = "" + dtLUGG_MST1.Rows[iSend_Loop]["IF_COMMAND"].ToString();
			JobInfo.strJobDef = "" + dtLUGG_MST1.Rows[iSend_Loop]["JOB_KIND"].ToString();
			JobInfo.strLuggNo1 = "0000";
			JobInfo.strStartWhTyp1 = "00";
			JobInfo.strStartStn1 = "000";
			JobInfo.strStartLoc1 = "0000000";
			JobInfo.strRouteStn1 = "000";
			JobInfo.strDestWhTyp1 = "00";
			JobInfo.strDestStn1 = "000";
			JobInfo.strDestLoc1 = "0000000";
			JobInfo.strLdCtnNo1 = Strings.Space(20);
            JobInfo.strLotNo1 = strGRADE;


            // FORK #1 만 사용하는 창고는 미사용
            /*
			JobInfo.strLuggNo2 = "0000";
			JobInfo.strStartWhTyp2 = "00";
			JobInfo.strStartStn2 = "000";
			JobInfo.strStartLoc2 = "0000000";
			JobInfo.strRouteStn2 = "000";
			JobInfo.strDestWhTyp2 = "00";
			JobInfo.strDestStn2 = "000";
			JobInfo.strDestLoc2 = "0000000";
			JobInfo.strLdCtnNo2 = Strings.Space(20);
			JobInfo.strProdID2 = Strings.Space(20);
			JobInfo.strMC_NO = "000";
			JobInfo.strMOD_YON1 = "0";
			JobInfo.strMOD_YON2 = "0";
			JobInfo.strJobRouting1 = Strings.Space(1);
			JobInfo.strJobRouting2 = Strings.Space(1);
            */

            if (dtLUGG_MST1.Rows[iSend_Loop]["IF_JOB_FK"].ToString() == "1")
			{
                JobInfo.strScNo = "" + dtLUGG_MST1.Rows[iSend_Loop]["SC_NO"].ToString();
                JobInfo.strLuggNo1 = "" + string.Format(dtLUGG_MST1.Rows[iSend_Loop]["LUGGNO"].ToString(), "0000");
                JobInfo.strStartWhTyp1 = "" + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_WH_CD"].ToString();
				JobInfo.strStartStn1 = "" + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_SITE"].ToString();
				JobInfo.strStartLoc1 = "" + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_BANK"].ToString() + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_BAY"].ToString() + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_LEV"].ToString();
				JobInfo.strDestWhTyp1 = "" + dtLUGG_MST1.Rows[iSend_Loop]["DEST_WH_CD"].ToString();
				JobInfo.strDestStn1 = "" + dtLUGG_MST1.Rows[iSend_Loop]["DEST_SITE"].ToString();
				JobInfo.strDestLoc1 = "" + dtLUGG_MST1.Rows[iSend_Loop]["DEST_BANK"].ToString() + dtLUGG_MST1.Rows[iSend_Loop]["DEST_BAY"].ToString() + dtLUGG_MST1.Rows[iSend_Loop]["DEST_LEV"].ToString();
				JobInfo.strLdCtnNo1 = "" + dtLUGG_MST1.Rows[iSend_Loop]["LD_CTN_NO"].ToString();
				if (JobInfo.strLdCtnNo1.Length > 20)
				{
					JobInfo.strLdCtnNo1 = Strings.Left(JobInfo.strLdCtnNo1, 30);
				}
				else
				{
					JobInfo.strLdCtnNo1 = JobInfo.strLdCtnNo1 + Strings.Space(30 - JobInfo.strLdCtnNo1.Length);
				}

				if (JobInfo.strLotNo1.Length > 30)
				{
					JobInfo.strLotNo1 = Strings.Left(JobInfo.strLotNo1, 30);
				}
				else
				{
					JobInfo.strLotNo1 = JobInfo.strLotNo1 + Strings.Space(30 - JobInfo.strLotNo1.Length);
				}
			}
			else
			{
                JobInfo.strScNo = "" + dtLUGG_MST1.Rows[iSend_Loop]["SC_NO"].ToString();
                JobInfo.strLuggNo2 = "" + string.Format(dtLUGG_MST1.Rows[iSend_Loop]["LUGGNO"].ToString(), "0000");
                JobInfo.strStartWhTyp2 = "" + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_WH_CD"].ToString();
				JobInfo.strStartStn2 = "" + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_SITE"].ToString();
				JobInfo.strStartLoc2 = "" + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_BANK"] + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_BAY"].ToString() + dtLUGG_MST1.Rows[iSend_Loop]["SOUR_LEV"].ToString();
				JobInfo.strDestWhTyp2 = "" + dtLUGG_MST1.Rows[iSend_Loop]["DEST_WH_CD"].ToString();
				JobInfo.strDestStn2 = "" + dtLUGG_MST1.Rows[iSend_Loop]["DEST_SITE"].ToString();
				JobInfo.strDestLoc2 = "" + dtLUGG_MST1.Rows[iSend_Loop]["DEST_BANK"].ToString() + dtLUGG_MST1.Rows[iSend_Loop]["DEST_BAY"].ToString() + dtLUGG_MST1.Rows[iSend_Loop]["DEST_LEV"].ToString();
				JobInfo.strLdCtnNo2 = "" + dtLUGG_MST1.Rows[iSend_Loop]["LD_CTN_NO"].ToString();
				if (JobInfo.strLdCtnNo2.Length > 20)
				{
					JobInfo.strLdCtnNo2 = Strings.Left(JobInfo.strLdCtnNo2, 20);
				}
				else
				{
					JobInfo.strLdCtnNo2 = JobInfo.strLdCtnNo2 + Strings.Space(20 - JobInfo.strLdCtnNo2.Length);
				}

				if (JobInfo.strLotNo2.Length > 20)
				{
					JobInfo.strLotNo2 = Strings.Left(JobInfo.strLotNo2, 20);
				}
				else
				{
					JobInfo.strLotNo2 = JobInfo.strLotNo2 + Strings.Space(20 - JobInfo.strLotNo2.Length);
				}
			}

			if (blPAIR_SEND == true)
			{
				if (dtLUGG_MST2.Rows[0]["IF_JOB_FK"].ToString() == "1")
				{
                    JobInfo.strScNo = "" + dtLUGG_MST2.Rows[iSend_Loop]["SC_NO"].ToString();
                    JobInfo.strLuggNo1 = "" + string.Format(dtLUGG_MST2.Rows[0]["LUGGNO"].ToString(), "0000");
					JobInfo.strStartWhTyp1 = "" + dtLUGG_MST2.Rows[0]["SOUR_WH_CD"].ToString();
					JobInfo.strStartStn1 = string.Format("" + dtLUGG_MST2.Rows[0]["SOUR_SITE"].ToString(), "000");
					JobInfo.strStartLoc1 = "" + dtLUGG_MST2.Rows[0]["SOUR_BANK"].ToString() + dtLUGG_MST2.Rows[0]["SOUR_BAY"].ToString() + dtLUGG_MST2.Rows[0]["SOUR_LEV"].ToString();
					JobInfo.strDestStn1 = string.Format("" + dtLUGG_MST2.Rows[0]["DEST_SITE"].ToString(), "000");
					JobInfo.strDestLoc1 = "" + dtLUGG_MST2.Rows[0]["DEST_BANK"].ToString() + dtLUGG_MST2.Rows[0]["DEST_BAY"].ToString() + dtLUGG_MST2.Rows[0]["DEST_LEV"].ToString();
					JobInfo.strLdCtnNo1 = "" + dtLUGG_MST2.Rows[0]["LD_CTN_NO"].ToString();
					if (JobInfo.strLdCtnNo1.Length > 20)
					{
						JobInfo.strLdCtnNo1 = Strings.Left(JobInfo.strLdCtnNo1, 20);
					}
					else
					{
						JobInfo.strLdCtnNo1 = JobInfo.strLdCtnNo1 + Strings.Space(20 - JobInfo.strLdCtnNo1.Length);
					}

					if (JobInfo.strLotNo1.Length > 20)
					{
						JobInfo.strLotNo1 = Strings.Left(JobInfo.strLotNo1, 20);
					}
					else
					{
						JobInfo.strLotNo1 = JobInfo.strLotNo1 + Strings.Space(20 - JobInfo.strLotNo1.Length);
					}
				}
				else
				{
                    JobInfo.strScNo = "" + dtLUGG_MST2.Rows[iSend_Loop]["SC_NO"].ToString();
                    JobInfo.strLuggNo2 = "" + string.Format(dtLUGG_MST2.Rows[0]["LUGGNO"].ToString(), "0000");
					JobInfo.strStartStn2 = "" + dtLUGG_MST2.Rows[0]["SOUR_SITE"].ToString();
					JobInfo.strStartLoc2 = "" + dtLUGG_MST2.Rows[0]["SOUR_BANK"].ToString() + dtLUGG_MST2.Rows[0]["SOUR_BAY"].ToString() + dtLUGG_MST2.Rows[0]["SOUR_LEV"].ToString();
					JobInfo.strDestStn2 = "" + dtLUGG_MST2.Rows[0]["DEST_SITE"].ToString();
					JobInfo.strDestLoc2 = "" + dtLUGG_MST2.Rows[0]["DEST_BANK"].ToString() + dtLUGG_MST2.Rows[0]["DEST_BAY"].ToString() + dtLUGG_MST2.Rows[0]["DEST_LEV"].ToString();
					JobInfo.strLdCtnNo2 = "" + dtLUGG_MST2.Rows[0]["LD_CTN_NO"].ToString();
					if (JobInfo.strLdCtnNo2.Length > 20)
					{
						JobInfo.strLdCtnNo2 = Strings.Left(JobInfo.strLdCtnNo2, 20);
					}
					else
					{
						JobInfo.strLdCtnNo2 = JobInfo.strLdCtnNo2 + Strings.Space(20 - JobInfo.strLdCtnNo2.Length);
					}

					if (JobInfo.strLotNo2.Length > 20)
					{
						JobInfo.strLotNo2 = Strings.Left(JobInfo.strLotNo2, 20);
					}
					else
					{
						JobInfo.strLotNo2 = JobInfo.strLotNo2 + Strings.Space(20 - JobInfo.strLotNo2.Length);
					}
				}

			}

			JobInfo.strPriority = "" + dtLUGG_MST1.Rows[iSend_Loop]["PRIORITY"].ToString();
			JobInfo.strERRCODE = "" + dtLUGG_MST1.Rows[iSend_Loop]["ERR_CODE"].ToString();
			JobInfo.strERRKIND = "" + dtLUGG_MST1.Rows[iSend_Loop]["ERR_KIND"].ToString();

			strReasonCode1 = "00";
			strReasonCode2 = "00";

			//### 전송
			{
				if (!SendLuggOrder(JobInfo))
				{
					return false;
				}
			}

			//### 수신 전문 로그
			{
				m_strLog = System.Text.Encoding.UTF8.GetString(m_bytRxBuff);
				m_strLog = m_strLog.TrimEnd(ControlChars.NullChar);
				modCmWork.ShowMsgClient(m_strLog);
			}


			//### 분석
			{
				if (m_strLog.Length != 25)
				{
					strReasoneInfo1 = string.Format("작업, 정의된 메세지의 길이가 아닙니다.[{0}]", m_strLog.Length);
					modCmWork.ShowMsgClient(strReasoneInfo1, modDefApp.MSG_ERR);
					strReasonCode1 = "99";
					strReasonCode2 = "99";
					return false;
				}
			}

			if (m_bytRxBuff[modDefApp.MSG_HEAD_CNT + 2] == modDefApp.TRANS_NAK)
			{
				strReasonCode1 = m_strLog.Substring(modDefApp.MSG_HEAD_CNT + 3, 2);
				strReasoneInfo1 = "응답 코드 #1[" + modCmLib.GetEcsErrInfo(strReasonCode1) + "]";
				modCmWork.ShowMsgClient(strReasoneInfo1, modDefApp.MSG_ERR);
				//Return False
			}

            /*
			if (m_bytRxBuff[modDefApp.MSG_HEAD_CNT + 9] == modDefApp.TRANS_NAK)
			{
				strReasonCode2 = m_strLog.Substring(modDefApp.MSG_HEAD_CNT + 10, 2);
				strReasoneInfo2 = "응답 코드 #2[" + modCmLib.GetEcsErrInfo(strReasonCode2) + "]";
				modCmWork.ShowMsgClient(strReasoneInfo2, modDefApp.MSG_ERR);
				//Return False
			}
            */

			strRecvLuggNo1 = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, 20, 4);

            /*
			strRecvLuggNo2 = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, 27, 4);
            */

			if (Convert.ToDecimal(strRecvLuggNo1) != Convert.ToDecimal(JobInfo.strLuggNo1) & Convert.ToDecimal(JobInfo.strLuggNo1) != 0)
			{
				m_strLog = string.Format("작업, 전송한 작업번호 #1와 틀립니다.[{0}]", strRecvLuggNo1);
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				strReasonCode1 = "99";
			}

            /*
			if (strRecvLuggNo2 != JobInfo.strLuggNo2)
			{
				m_strLog = string.Format("작업, 전송한 작업번호 #2와 틀립니다.[{0}]", strRecvLuggNo2);
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
				strReasonCode1 = "99";
				//Return False
			}
            */

			m_BDb.BeginTrans();

			//### Status UPDATE
			if (Convert.ToDecimal(strRecvLuggNo1) == Convert.ToDecimal(JobInfo.strLuggNo1) & Convert.ToDecimal(JobInfo.strLuggNo1) != 0)
			{
				m_BDb.ParamsClear();

				m_strSql = modDefApp.CRLF + "  UPDATE LUGG_MST ";
				//01. 20161122 이길문 작업취소일 경우 추가
				if (JobInfo.strMessageType == "D")
				{
					if (strReasonCode1 == "00" | strReasonCode1 == "04")
					{
						m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '1' ";
						m_strSql += modDefApp.CRLF + "      , ERR_KIND   = ''  ";
						m_strSql += modDefApp.CRLF + "      , ERR_CODE   = '0000' ";
						m_strSql += modDefApp.CRLF + "      , ERR_MC_TYP = '' ";
						m_strSql += modDefApp.CRLF + "      , ERR_MC_NO  = '' ";
						m_strSql += modDefApp.CRLF + "      , CAN_KIND   = '11' ";
					}
					else
					{
						m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '2' ";
					}
					//01. END
				}
				else
				{
					if (strReasonCode1 == "00")
					{
						m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '1' ";
						m_strSql += modDefApp.CRLF + "      , ERR_KIND   = ''  ";
						m_strSql += modDefApp.CRLF + "      , ERR_CODE   = '0000' ";
						m_strSql += modDefApp.CRLF + "      , ERR_MC_TYP = '' ";
						m_strSql += modDefApp.CRLF + "      , ERR_MC_NO  = '' ";
					}
					else
					{
						m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '2' ";
					}
				}

				m_strSql += modDefApp.CRLF + "      , IF_ERR_CODE    = 'ECS" + strReasonCode1 + "' ";
				m_strSql += modDefApp.CRLF + "      , SND_FT_DTTM    = CASE WHEN " + modDefApp.NVL + "(SND_FT_DTTM, '') = '' THEN SND_FT_DTTM ELSE " +  modDateTime.SYSDATE_TO_CDTTM + " END ";
                m_strSql += modDefApp.CRLF + "  WHERE COMPANY_CD  =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                m_strSql += modDefApp.CRLF + "    AND AREA_CD     =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                m_strSql += modDefApp.CRLF + "    AND SYS_GRP     =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                m_strSql += modDefApp.CRLF + "    AND LUGGNO      =  " + m_BDb.ParamsAdd("LUGGNO", strRecvLuggNo1);

				m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

				if (m_iSelCnt < 0)
				{
					m_strLog = m_BDb.ErrMsg + m_strSql;
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					m_BDb.trnMain.Rollback();
					return false;
				}
				if (m_iSelCnt != 1)
				{
					m_strLog = "작업 #1, LUGG_MST 수정 실패,작업번호[" + strRecvLuggNo1 + "]" + modDefApp.CRLF + m_strSql;
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					m_BDb.trnMain.Rollback();
					return false;
				}
			}

            /*
			if (strRecvLuggNo2 == JobInfo.strLuggNo2 & JobInfo.strLuggNo2 != "0000")
			{
				m_BDb.ParamsClear();

				m_strSql = modDefApp.CRLF + "  UPDATE LUGG_MST ";
				//01. 20161122 이길문 작업취소일 경우 추가
				if (JobInfo.strMessageType == "D")
				{
					if (strReasonCode2 == "00" | strReasonCode2 == "04")
					{
						m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '1' ";
						m_strSql += modDefApp.CRLF + "      , ERR_KIND   = ''  ";
						m_strSql += modDefApp.CRLF + "      , ERR_CODE   = '0000' ";
						m_strSql += modDefApp.CRLF + "      , ERR_MC_TYP = '' ";
						m_strSql += modDefApp.CRLF + "      , ERR_MC_NO  = '' ";
						m_strSql += modDefApp.CRLF + "      , CAN_KIND   = '11' ";
					}
					else
					{
						m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '2' ";
					}
					//01. END
				}
				else
				{
					if (strReasonCode1 == "00")
					{
						m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '1' ";
						m_strSql += modDefApp.CRLF + "      , ERR_KIND   = ''  ";
						m_strSql += modDefApp.CRLF + "      , ERR_CODE   = '0000' ";
						m_strSql += modDefApp.CRLF + "      , ERR_MC_TYP = '' ";
						m_strSql += modDefApp.CRLF + "      , ERR_MC_NO  = '' ";
					}
					else
					{
						m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '2' ";
					}
				}
				m_strSql += modDefApp.CRLF + "      , SND_FT_DTTM    = CASE WHEN " + modDefApp.NVL + "(SND_FT_DTTM, '') = '' THEN SND_FT_DTTM ELSE " + modDateTime.SYSDATE_TO_CDTTM + " END ";
				m_strSql += modDefApp.CRLF + "      , IF_ERR_CODE    = 'ECS" + strReasonCode2 + "' ";
				m_strSql += modDefApp.CRLF + "  WHERE SYS_GRP        =  " + m_BDb.ParamsAdd(modDefApp.SYS_GRP);
				m_strSql += modDefApp.CRLF + "    AND LUGGNO         =  " + m_BDb.ParamsAdd(strRecvLuggNo2);

				m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

				if (m_iSelCnt < 0)
				{
					m_strLog = m_BDb.ErrMsg + m_strSql;
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					m_BDb.trnMain.Rollback();
					return false;
				}
				if (m_iSelCnt != 1)
				{
					m_strLog = "작업 #2, LUGG_MST 수정 실패,작업번호[" + strRecvLuggNo2 + "]" + modDefApp.CRLF + m_strSql;
					modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
					m_BDb.trnMain.Rollback();
					return false;
				}
			}
            */
			m_BDb.trnMain.Commit();

			return true;
		}

        //최초작성자	: BASE(이길문)
        //작성일		: 20160829
        //설명		: 직접지시정보송신
        private bool SendDirOrder(string p_strDIR_DAT)
        {
            m_strHostCmd = "R";
            //전문 작성
            string strTemp = null;
            byte[] bytTempByte = null;
            int iTxCnt = 0;

            strTemp = p_strDIR_DAT;

            iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            //MSG_ORDER_CNT
            m_bytTxBuff = new byte[iTxCnt - 1];

            //### Header ###
            MakeHeader(strTemp.Length);
            //MSG_ORDER_CNT


            //### Body ###
            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;


            //TempByte = System.Text.Encoding.UTF8.GetBytes(strTemp)
            bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);
            //TempByte.Copy(TempByte, 0, m_bytTxBuff, MSG_HEAD_CNT + 1, LenHan(strTemp))

            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            if (!RequestSrv(iTxCnt.ToString()))
            {
                return false;
            }

            return true;
        }

        //최초작성자	: BASE(이길문)
        //작성일		: 20160829
        //설명		: 작업정보송신
        private bool SendLuggOrder(stuSendLuggInfo p_JobInfo)
        {
            m_strHostCmd = "O";
            //전문 작성
            string strTemp = null;
            byte[] bytTempByte = null;
            int iTxCnt = 0;

            strTemp = p_JobInfo.strMessageType;
            strTemp += p_JobInfo.strJobDef;
            if (p_JobInfo.strMessageType == "O")
            {
                strTemp += p_JobInfo.strLuggNo1.PadLeft(4, '0');
                strTemp += p_JobInfo.strStartStn1.PadLeft(3, '0');
                strTemp += p_JobInfo.strStartLoc1.PadLeft(7, '0');
                strTemp += p_JobInfo.strDestStn1.PadLeft(3, '0');
                strTemp += p_JobInfo.strDestLoc1.PadLeft(7, '0');
                strTemp += p_JobInfo.strPriority;
                strTemp += p_JobInfo.strLotNo1.PadLeft(30, ' ');
            }
            if (p_JobInfo.strMessageType == "D")
            {
                strTemp += p_JobInfo.strLuggNo1.PadLeft(4, '0');
            }
            if (p_JobInfo.strMessageType == "R")
            {
                //입고 재지성시 DEST_SITE 는 '000'
                p_JobInfo.strDestStn1 = "000";

                strTemp += p_JobInfo.strLuggNo1.PadLeft(4, '0');
                strTemp += p_JobInfo.strStartStn1.PadLeft(3, '0');
                strTemp += p_JobInfo.strStartLoc1.PadLeft(7, '0');
                strTemp += p_JobInfo.strDestStn1.PadLeft(3, '0');
                strTemp += p_JobInfo.strDestLoc1.PadLeft(7, '0');
                strTemp += "1";
                strTemp += p_JobInfo.strScNo.PadLeft(2, '0');
            }

            iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            //MSG_ORDER_CNT
            m_bytTxBuff = new byte[iTxCnt];

            //### Header ###
            MakeHeader(strTemp.Length);
            //MSG_ORDER_CNT
            ;
            //### Body ###
            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;

            bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, iTxCnt - modDefApp.MSG_HEAD_CNT - 2);

            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            if (!RequestSrv(iTxCnt.ToString()))
            {
                return false;
            }

            return true;
        }
        
        //최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 요청전송
		public bool RequestSrv(string p_strWriteCnt, int p_iTimeOut = modDefApp.TIME_OUT)
		{
            string strTitle = "[RequestSrv] ... ";

			//2회 반복은 무의미, 실제 1회 전송 후 대기시간을 길게 하는게 효과적
			ClearBuff(ref m_bytRxBuff);
			try
			{
				SendSock(int.Parse(p_strWriteCnt));
				if (!CheckRecvSock(p_iTimeOut))
				{
					//타임아웃
					m_strLog = m_strHostCmd + " 메세지의 응답이 없습니다.";
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);

					m_blSockConnected = false;
					modCmWork.CloseSocket(ref m_sktSock);
					modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picCliCom, modDefApp.ComSts.ComErr);
					return false;
				}

				if (!ReadSock())
				{
                    m_strLog = m_strHostCmd + " 메세지로 인해서 리모트 시스템과 연결을 종료합니다.";
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_IMP);

					m_blSockConnected = false;
					modCmWork.CloseSocket(ref m_sktSock);
					modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picCliCom, modDefApp.ComSts.ComErr);
					return false;
				}
                
                // 로그 입력하기... 
                string strMsg = "";
                strMsg = System.Text.Encoding.Default.GetString(m_bytRxBuff);
                int nLength = strMsg.Length;
                strMsg.Trim();
                nLength = strMsg.Length;
                strMsg.TrimEnd();
                nLength = strMsg.Length;


                int IndexValue = strMsg.IndexOf(Convert.ToChar(0x03));

                string strTemp = strMsg.Substring(0, IndexValue + 1);


                modCmWork.ShowMsgClient(strTemp, modDefApp.MSG_NOR);
                bool bResult = modDefApp.g_frmForm.InsertHostIfLog(m_BDb, strTemp, m_strHostCmd, "W2E");     // 함수안에서 Transaction 처리함!

			}
			catch (SocketException se)
			{
				//m_strLog = se.Message & "(" & se.ErrorCode.ToString & ")"
				m_strLog = "리모트 시스템과 연결을 종료합니다." + "(" + se.ErrorCode.ToString() + ")";
				modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);

				modDefApp.g_CliWork.m_blSockConnected = false;
				modCmWork.CloseSocket(ref m_sktSock);
				modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picCliCom, modDefApp.ComSts.ComErr);

				return false;
			}
			catch (Exception ex)
			{
				modCmWork.ShowMsgClient(ex.ToString(), modDefApp.MSG_ERR);

				m_blSockConnected = false;
				modCmWork.CloseSocket(ref m_sktSock);
				modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picCliCom, modDefApp.ComSts.ComErr);

				return false;
			}
			return true;
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 소켓전송(사이즈 파라미터)
		public void SendSock(int p_iWriteCnt)
		{
			//Close시 Send에서 Exception발생 (처음에는 발생 안할 수 있음)
			m_sktSock.Send(m_bytTxBuff, p_iWriteCnt, SocketFlags.None);

			//m_strLog = System.Text.Encoding.UTF8.GetString(m_bytTxBuff)
			m_strLog = System.Text.Encoding.Default.GetString(m_bytTxBuff);

			modCmWork.ShowMsgClient(m_strLog);

            bool bResult = modDefApp.g_frmForm.InsertHostIfLog(m_BDb, m_strLog, m_strHostCmd, m_strDirection);     // 함수안에서 Transaction 처리함!
        }


		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 수신소켓체크
		public bool CheckRecvSock(int p_iTimeOut = modDefApp.TIME_OUT)
		{
			// 로그인 시는 DB연결 등 시간이 지연, time out을 달리 가져간다.
			System.DateTime tmRecvTime = default(System.DateTime);

			// Socket 에러이든 프레임 에러이든 통신장애로 보고 Close
			// 응답이 없을 경우만 2회 반복
			tmRecvTime = DateTime.Now.AddSeconds(p_iTimeOut);
			// db 리턴 고려.. 
			// close되도 0 이지만 time_out을 주기위해서 사용, time_out되면 close 처리
			while (m_sktSock.Available <= 0)
			{
				if (tmRecvTime <= DateTime.Now)
				{
					// 시간초과
					return false;
				}
				System.Threading.Thread.Sleep(10);
			}

			return true;
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 수신소켓읽기
		public bool ReadSock()
		{
			int iBodyLen = 0;

			string strLog = null;
			int iRxCnt = 1;
			//return false:Close socket, true:Continue

			ClearBuff(ref m_bytRxHead);
            // Close시 Return 0 or Exception 발생, Receive전 Close되었느냐 Receive대기상태일 때 Close되었는냐 등에 따라 다름

            iRxCnt = m_sktSock.Receive(m_bytRxHead, modDefApp.MSG_HEAD_CNT, SocketFlags.None);
			if (iRxCnt <= 0)
				return false;

			// Header 체크
			if (!CheckHeader(iRxCnt, ref iBodyLen))
			{
				// 잘못된 헤더 로그
				strLog = System.Text.Encoding.UTF8.GetString(m_bytRxHead);
				modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
				return false;
			}

			// Body 체크
			iRxCnt = m_sktSock.Receive(m_bytRxBuff, modDefApp.MSG_HEAD_CNT, iBodyLen, SocketFlags.None);
			if (!CheckBody(iBodyLen, iRxCnt))
			{
				// 잘못된 바디 로그
				strLog = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, 0, modDefApp.MSG_HEAD_CNT + iBodyLen);
				modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
				return false;
			}

			return true;
		}

	}
}
