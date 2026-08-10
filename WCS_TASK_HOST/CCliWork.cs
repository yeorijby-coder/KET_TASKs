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
                        , ref string strLuggNum
                        , string strJobTypNotIn = "")
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
                if (strJobTypNotIn != "")
                {
                    // 해당 상태가 최종이 아닌 작업구분을 뺀다
                    m_strSql += modDefApp.CRLF + "    AND JOB_TYP NOT IN (" + strJobTypNotIn + ")";
                }
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

                    // 피킹
                    //   원본 CLib::ConvertJobTypeToPattern(EcsCL/Lib.cpp) 에서
                    //   PICKING(3) 은 UNIT_RET(2) 와 같은 JOB_PATTERN_RET 로 묶여 있다.
                    //   원본 CJobItem::GetLog 의 RET 패턴도 출발=[창고][위치][로케이션] 도착=[창고][위치] 이므로
                    //   출고(case 2)와 똑같이 출발은 로케이션, 도착은 스테이션을 쓴다.
                    case 3:
                        strSPosition = m_BDb.dtMain.Rows[0]["START_LOCATION"].ToString();
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
                // @.상태 보고(S). 안에서 30초 경과 여부를 보고 정기보고/변경보고를 가른다.
                //   원본 문서 : "상태가 변경되면 즉시 + 정기적으로 30초에 1회"
                GetStatusReport();      // @.상태 보고(S)          문서 IV.3
                GetErrorReport();       // @.에러 보고(E)          문서 IV.4
                GetPmStoRequest();      // @.공파렛트 입고 요구(N) 문서 IV.8
                GetBoxStoRequest();     // @.P-BOX 입고 요구(L)    문서 IV.9

                /*
                 * 아래 두 가지는 없앴다.
                 *   GetWeightReport()    : 무게 보고(UG). 문서에도 원본에도 없는 전문이다.
                 *   GetEmptyPltRequest() : 문서 IV.8 의 공파렛트 입고 요구는
                 *                          GetPmStoRequest() 가 담당한다. 중복이었다.
                 */
            }
            GetJobCompleteReport();     // @.작업 완료 보고(F) 1차완료   문서 IV.5
            GetLoadArrivalReport();     // @.도착 보고. 원본은 완료보고(F) 최종완료로 보낸다

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

            // @.설비별로 만들어질 전문 본문들. 문서 IV.3 상태 보고 참고.
            System.Collections.Generic.List<string> lstStatusFrame = new System.Collections.Generic.List<string>();
            // @.C/V 는 작업유무 조회 때문에 값만 모았다가 루프 뒤에서 본문을 만든다.
            System.Collections.Generic.List<int[]> lstCvPending = new System.Collections.Generic.List<int[]>();

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

                // @.S/C 는 장비분류 1, 장비번호는 호기번호.
                string strScNo = m_BDb.dtMain.Rows[ii]["SC_NO"].ToString().Trim();
                if (strScNo.Length == 0) strScNo = strPLC_NO;

                int nScNo = 0;   int.TryParse(strScNo, out nScNo);
                // @.크레인 번호는 DB 안에서 9xx 로 저장한다(CSrvWork 의 변환과 짝).
                //   문서 IV.3 의 장비번호는 호기번호이므로 900 을 뺀다.
                if (nScNo > 900) nScNo -= 900;
                int nScLugg = 0; int.TryParse(strITN_LUGG_FK1.Trim(), out nScLugg);

                lstStatusFrame.Add(GfMakeStatusBody(modDefApp.DEV_CLASS_SC, nScNo, nScStatus,
                                                    nScLugg,
                                                    0,                          // @.사이즈체크는 S/C 에 해당 없음
                                                    (nScLugg > 0) ? 1 : 0));

                strTempFrame = string.Format("{0:00}{1:0}{2:0000}", Convert.ToInt32(strPLC_NO), nScStatus, Convert.ToInt32(strITN_LUGG_FK1));

                strTemp1 += strTempFrame;
            }
            #endregion

            #region CV 상태값을 가져온다
            m_BDb.ParamsClear();
            
            // 다행 스럽게도 리모콘과 모드와 무게값이 모두 입고대에 표현된다 
            // 혹시라도 다른 조건이 필요하다면 쿼리 수정 요망 
            // @.무게는 문서의 상태 보고 항목이 아니어서 WC_DATA 조인을 뺐다.
            m_strSql = modDefApp.CRLF + "     SELECT  CD.*                          ";
            m_strSql += modDefApp.CRLF + "      FROM  CV_DATA CD                    ";
            /*
             * 보고 대상 작업대 고르기 (원본 CCvTrackInfo::StatusReport)
             *
             *   원본은 m_bReportTrack 이 TRUE 인 트랙만 보고한다.
             *   그 값은 InitTrackInfo 에서 입고대(GetBeStoStn) 또는 출고대(GetBeRetStn)
             *   일 때 TRUE 가 되고, 이어서 스테이션 221 / 222 는 따로 제외한다.
             *
             *   STN_KIND 비트는 IO_SCH 와 같다.  0x01 입고대, 0x02 출고대
             *   따라서 (STN_KIND & 3) <> 0 이 원본의 m_bReportTrack 에 해당한다.
             *
             *   ※ 예전에는 여기에 CD.COMP_VR = 'Y' 조건이 있었는데
             *     COMP_VR 컬럼은 어느 DB 에도 없고 코드 어디에서도 넣지 않는다.
             *     이 조건 때문에 상태보고 조회가 매번 42703 오류로 실패했다.
             */
            m_strSql += modDefApp.CRLF + "     WHERE  CD.WH_TYP       =  " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "       AND (COALESCE(NULLIF(TRIM(CD.STN_KIND),''),'0')::INTEGER & 3) <> 0 ";
            m_strSql += modDefApp.CRLF + "       AND (CD.HOST_STN_NO IS NULL                                        ";
            m_strSql += modDefApp.CRLF + "            OR CD.HOST_STN_NO NOT IN ('221','222'))                       ";
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
                if (strSTO_READY_RD == "1")
                {
                    nCvStatus = 1;
                }

                /*
                 * @.C/V 는 장비분류 2, 장비번호는 작업대 번호.
                 *   원본은 ConvertPositionToCustom(트랙번호) 로 작업대 번호를 구한다.
                 *   여기서는 그 매칭을 CV_DATA.HOST_STN_NO 에 두었으므로 그것을 쓰고,
                 *   아직 매칭이 없으면 예전처럼 MC_NO 로 대신한다.
                 */
                string strHostStn = m_BDb.dtMain.Rows[iii]["HOST_STN_NO"].ToString().Trim();
                if (strHostStn.Length == 0) strHostStn = strMC_NO;

                int nCvStn = 0;  int.TryParse(strHostStn, out nCvStn);
                int nCvLugg = 0; int.TryParse(m_BDb.dtMain.Rows[iii]["LUGG_NO_RD"].ToString().Trim(), out nCvLugg);

                // @.여기서 DB 를 다시 조회하면 m_BDb.dtMain 이 교체되어
                //   다음 회차의 Rows[iii] 가 사라진다. 값만 모아 두고 루프 뒤에서 만든다.
                lstCvPending.Add(new int[] { nCvStn, nCvStatus, nCvLugg });

                strTempFrame = string.Format("{0:0}", nCvStatus);

                strTemp2 += strTempFrame;
            }
            #endregion

            #region C/V 본문 만들기 (조회가 dtMain 을 건드리므로 루프 밖에서)
            for (int nCv = 0; nCv < lstCvPending.Count; nCv++)
            {
                int[] arrCv = lstCvPending[nCv];
                lstStatusFrame.Add(GfMakeStatusBody(modDefApp.DEV_CLASS_CV,
                                                    arrCv[0], arrCv[1], arrCv[2],
                                                    0,                          // @.사이즈체크 센서값은 DB 에 없다
                                                    GfHasUnExcutedJob(arrCv[0]) ? 1 : 0));
            }
            #endregion

            #region 상위에 보낼 메세지 부분 구성 / 보내기
            /*
             * 문서 IV.3 상태 보고 : 설비 1대당 전문 1건이다.
             *   STX + 'S' + 창고구분 + 장비분류 + 장비번호(3) + 상태 + 작업번호(4)
             *       + 사이즈체크 + ECS작업유무 + ETX      (본문 13자)
             *
             * 예전에는 모든 설비를 한 전문(94자)에 몰아 담았는데
             * 그런 형식은 문서에도 원본(CCvTrackInfo::StatusReport)에도 없다.
             */
            for (int nFrame = 0; nFrame < lstStatusFrame.Count; nFrame++)
            {
                if (!SendBody(lstStatusFrame[nFrame])) return false;
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

            /*
             * 문서 IV.4 에러 상태 보고
             *   STX + 'E' + 창고구분 + 장비분류 + 장비번호(3) + 에러종류 + 에러코드(4)
             *       + 작업번호(4) + Bank(2) + Bay(3) + Level(2) + ETX      (본문 22자)
             *   예전 코드는 창고구분이 빠져 뒤 항목이 전부 한 칸씩 밀려 있었다.
             */
            strTemp = string.Format("E{0}{1:0}{2:000}{3:0}{4:0000}{5:0000}{6:00}{7:000}{8:00}",
                modDefApp.WH_DEF,
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

            /*
             * 29(SC 구동완료)가 최종인 것은 입고다.
             * 출고/픽킹은 29 뒤에 컨베이어 구간(11 → 19)이 남으므로 여기서 보고하면 안 된다.
             *   이동(6)   99 → 10 → 11 → 19
             *   입고(1)   99 → 10 → 11 → 21 → 29
             *   출고(2,3) 99 → 20 → 21 → 29 → 11 → 19
             */
            string strJobTypNotIn = (nJobStatus == 29) ? "'2','3'" : "";

            int nJobType = 0;
            string strUserID = "";
            string strSScNum = "";
            string strDScNum = "";
            string strSPosition = "";
            string strDPosition = "";
            string strLuggNum = "";

            #region 완료 보고해야할 작업이 있는지?

            int nResult = IsJobExist(nJobStatus, ref nJobType, ref strUserID, ref strSScNum, ref strDScNum, ref strSPosition, ref strDPosition, ref strLuggNum, strJobTypNotIn);
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
            /*
             * 작업타입(nJobType) -> 작업구분(nClass) / 스테이션(nStation)
             *
             *   원본 Common/Include/Ecs/EcsEnv.h 의 JOB_TYPE_*
             *     1 UNIT_STO  2 UNIT_RET  3 PICKING  4 RACK_TO_RACK  5 AISLE_TO_AISLE  6 SITE_TO_SITE
             *
             *   작업구분은 원본 CHostCl::CompleteReport 가
             *     GetPattern() == JOB_PATTERN_RET ? 2 : 3
             *   으로 정하고, GetPattern() 은 CLib::ConvertJobTypeToPattern(EcsCL/Lib.cpp) 이다.
             *   거기서 PICKING(3) 은 UNIT_RET 와 같은 JOB_PATTERN_RET 로 묶여 있다.
             *   즉 피킹은 출고와 같은 취급이므로 작업구분 2, 스테이션은 도착지를 쓴다.
             */
            switch (nJobType)
            {
                case 1: nStation = Convert.ToInt16(strSPosition); nClass = 1;  break;
                case 2: nStation = Convert.ToInt16(strDPosition); nClass = 2;  break;
                case 3: nStation = Convert.ToInt16(strDPosition); nClass = 2;  break; // @.피킹. 원본에서 출고(JOB_PATTERN_RET)와 동일
                case 4: nStation = 0;                             nClass = 3; break; //랙투랙 부분 조한성 수정 0608
                case 5: nStation = 0;                             nClass = 3; break;
                case 6: nStation = Convert.ToInt32(strDPosition); nClass = 3; break;
                default:
                    m_strLog = "작업정보는 존재하지만 잘못된 작업 정보입니다.[작업 타입:" + nJobType.ToString() + "]";
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                    return false;
            }




            /*
             * 문서 IV.5 작업 완료 보고
             *   STX + 'F' + 작업구분 + 창고구분 + 작업번호(4) + 완료구분 + 완료차수
             *       + 도착작업대(3) + ETX      (본문 12자)
             *   예전 코드는 창고구분과 도착작업대가 빠지고 완료차수 자리에 '1' 을
             *   그냥 붙여 두어, 받는 쪽이 작업번호부터 어긋나게 읽었다.
             */
            strTemp = string.Format("F{0:0}{1}{2:0000}{3:0}{4:0}{5:000}",
                                    nJobType, modDefApp.WH_DEF, nLuggNum, nClass,
                                    modDefApp.STEP_FIRST, nStation);

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
                        
                        /*
                         * 시험용 랙 범위
                         *
                         *   예전에는 CELL_MST 에서 BANK/BAY/LEV 의 최대·최소를 읽었다.
                         *   이 현장은 재고관리를 쓰지 않아 그 표가 없다.
                         *   SIM MODE 는 상위 없이 혼자 작업을 이어 붙이는 시험 모드이고,
                         *   여기서 정하는 위치는 실제 재고와 맞출 필요가 없다.
                         *   범위만 ECSCOMA.ini 에서 읽는다.
                         *   기본값은 이 현장 CELL_MST 에 들어 있던 값 그대로다.
                         */
                        int nMinBank  = modDefAPI.GetPrivateProfileInt("SIM", "MIN_BANK",   1, modDefApp.MAIN_INI);
                        int nMaxBank  = modDefAPI.GetPrivateProfileInt("SIM", "MAX_BANK",   8, modDefApp.MAIN_INI);
                        int nMaxBay   = modDefAPI.GetPrivateProfileInt("SIM", "MAX_BAY",   17, modDefApp.MAIN_INI);
                        int nMaxLevel = modDefAPI.GetPrivateProfileInt("SIM", "MAX_LEVEL", 15, modDefApp.MAIN_INI);

                        if (nMinBank  < 1) nMinBank  = 1;
                        if (nMaxBank  < nMinBank) nMaxBank  = nMinBank;
                        if (nMaxBay   < 1) nMaxBay   = 1;
                        if (nMaxLevel < 1) nMaxLevel = 1;

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

                        
                        // @.이 현장은 재고관리(CELL_MST)를 쓰지 않으므로 생성된
                        //   Location 을 확인할 근거가 없다. 그대로 쓴다.
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

                        /*
             * 공파렛트 요청 관련 후처리. GetEmptyPltRequest() 를 없애면서
             * g_strEmtpyPltKind / g_strEmtpyPltStation / g_bEmtpyPltJob 에
             * 값을 넣는 곳이 사라져 더 이상 타지 않는 자리가 되었다.
             * 문서 IV.8 의 공파렛트 입고 요구는 GetPmStoRequest() 가 담당한다.
             */
//#region 공파레트 출고 작업이었을 경우 요청상태를 변경한다.
            ////int nLuggNum = Convert.ToInt32(strLuggNum);
            //
            //if (modDefApp.g_bEmtpyPltJob[nLuggNum] == true)
            //{
            //if (modDefApp.g_frmForm.UpdateHostEmptyPlt(m_BDb, "", "", strLuggNum, "Q", "C") == false)
            //{
            //m_strLog = string.Format("공파레트 출고요청 관련 완료처리를 하지 못했습니다. [작업번호:{0}][실패내용:{1}]", strLuggNum, modDefApp.GM_RTN_MSG);
            //modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
            //
            //return false;
            //}
            //m_strLog = string.Format("공파레트 출고요청 관련 완료 하였습니다. [작업번호:{0}]", strLuggNum);
            //modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
            //}
            //#endregion         
            return true;
        }
        //최초작성자	: BASE(정복열)
        //작성일		: 20200515
        //설명		    : 공파레트 입출고 요청 
        /*
         * GfMakeStatusBody :: 상태 보고(S) 전문 본문 1건
         *
         *   문서 IV.3 : 창고구분 + 장비분류 + 장비번호(3) + 상태 + 작업번호(4)
         *               + 사이즈체크 + ECS작업유무      (본문 13자)
         */
        private string GfMakeStatusBody(int nDevClass, int nDevNo, int nStatus,
                                        int nLuggNum, int nSizeChk, int nJobExist)
        {
            return string.Format("S{0}{1:0}{2:000}{3:0}{4:0000}{5:0}{6:0}",
                                 modDefApp.WH_DEF, nDevClass, nDevNo, nStatus,
                                 nLuggNum, nSizeChk, nJobExist);
        }

        /*
         * SendBody :: 본문을 헤더 + STX + 본문 + ETX 로 감싸 보낸다.
         *   전문마다 똑같이 반복되던 부분이라 한곳으로 모았다.
         */
        private bool SendBody(string p_strBody)
        {
            int iTxCnt = modDefApp.MSG_HEAD_CNT + p_strBody.Length + 2;
            m_bytTxBuff = new byte[iTxCnt];

            MakeHeader(p_strBody.Length);

            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;
            byte[] bytBody = System.Text.Encoding.Default.GetBytes(p_strBody);
            Array.Copy(bytBody, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, p_strBody.Length);
            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            return RequestSrv(iTxCnt.ToString());
        }

        /*
         * GetPmStoRequest :: 공파렛트 입고 요구 (Message Type 'N')
         *
         *   원본 : CCvTrackInfo::SendPMStoRequest / GetPalletMagazineStoRequest
         *     조건  IsPalletMagazine(트랙) && m_bStoStationReady && m_nLuggNum == 0
         *     전문  STX + 'N' + '1'(입고) + Station No(3) + User Data(0x20) + ETX
         *
         *   대상 작업대는 CV_DATA.HOST_STN_NO 로 찾는다.
         *   (WMS 전문은 작업대 번호를 쓰고, WCS 내부는 트랙번호를 쓰므로
         *    그 매칭을 CV_DATA.HOST_STN_NO 한곳에 모아 두었다)
         */
        private void GetPmStoRequest()
        {
            string strTitle = "[GetPmStoRequest] .. ";

            if (!m_blSockConnected) return;

            m_BDb.ParamsClear();
            m_strSql = modDefApp.CRLF + "  SELECT HOST_STN_NO                                                 ";
            m_strSql += modDefApp.CRLF + "    FROM CV_DATA                                                    ";
            m_strSql += modDefApp.CRLF + "   WHERE WH_TYP        = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "     AND HOST_STN_NO   = " + m_BDb.ParamsAdd("HOST_STN_NO", modDefApp.ECS_STN_POS_3F_PLT_207.ToString());
            m_strSql += modDefApp.CRLF + "     AND STO_READY_RD  = '1'                                        ";   // @.입고대 준비완료
            m_strSql += modDefApp.CRLF + "     AND COALESCE(LUGG_NO_RD,'0')::INTEGER = 0                       ";   // @.적재물 없음

            int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
            if (iCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                return;
            }
            if (iCnt == 0) return;      // @.요구 조건이 아님

            GetPmStoRequest(1, modDefApp.ECS_STN_POS_3F_PLT_207);
        }

        private bool GetPmStoRequest(int nKind, int nStation)
        {
            string strTitle = "[GetPmStoRequest] .. ";
            m_strHostCmd = "N";

            if (!m_blSockConnected) return false;

            // @.전문 : 'N' + 작업구분(1) + 스테이션(3) + User Data(0x20)
            string strTemp = string.Format("N{0}{1:000}{2}", nKind, nStation, (char)0x20);

            int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            m_bytTxBuff = new byte[iTxCnt];

            MakeHeader(strTemp.Length);

            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;
            byte[] bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);
            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            if (!RequestSrv(iTxCnt.ToString())) return false;

            m_strLog = string.Format("공파렛트 입고 요구.. 스테이션=[{0}]", nStation);
            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
            return true;
        }

        /*
         * GetBoxStoRequest :: P-BOX 입고 요구 (Message Type 'L')
         *
         *   원본 : CHostCl::RequestBoxStore(nFork1LuggNum, nFork2LuggNum)
         *     전문  STX + 'L'
         *           + Fork2 작업번호(4) + 스테이션221 미실행작업 유무(1)
         *           + Fork1 작업번호(4) + 스테이션222 미실행작업 유무(1)
         *           + ETX
         *
         *   ※ 원본은 Fork2 를 먼저 싣고 스테이션 221 플래그와 짝지운다.
         *      (221 이 자동입고대기 #1 이므로 짝이 어긋나 보이지만 원본을 그대로 따랐다)
         *
         *   대상 작업대(221 / 222)는 CV_DATA.HOST_STN_NO 로 찾는다.
         */
        private void GetBoxStoRequest()
        {
            string strTitle = "[GetBoxStoRequest] .. ";

            if (!m_blSockConnected) return;

            int nFork1 = GfGetStnLuggNo(modDefApp.ECS_STN_POS_3F_BOX_221);
            int nFork2 = GfGetStnLuggNo(modDefApp.ECS_STN_POS_3F_BOX_222);

            // @.두 대기대 모두 비어 있으면 요구하지 않는다
            if (nFork1 <= 0 && nFork2 <= 0) return;

            GetBoxStoRequest(nFork1, nFork2);
        }

        private bool GetBoxStoRequest(int nFork1LuggNum, int nFork2LuggNum)
        {
            string strTitle = "[GetBoxStoRequest] .. ";
            m_strHostCmd = "L";

            if (!m_blSockConnected) return false;

            // @.원본 순서 그대로 : Fork2 + 221플래그, Fork1 + 222플래그
            /*
             * 문서 IV.9 P-BoxRack 입고 요구
             *   STX + 'L' + 작업번호#1(4) + 작업번호#2(4) + ETX      (본문 9자)
             *   #1 은 자동입고대기#1(221), #2 는 자동입고대기#2(222) 의 작업번호.
             *
             *   ※ 원본 CHostCl::RequestBoxStore 는 각 작업번호 뒤에 미실행작업 유무
             *     1자리를 더 붙여 11자로 보낸다. 문서에 그 항목이 없어 문서를 따랐다.
             *     (미실행 유무는 상태보고 S 의 ECS작업유무로도 전달된다)
             */
            string strTemp = string.Format("L{0:0000}{1:0000}", nFork1LuggNum, nFork2LuggNum);

            int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            m_bytTxBuff = new byte[iTxCnt];

            MakeHeader(strTemp.Length);

            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;
            byte[] bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);
            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            if (!RequestSrv(iTxCnt.ToString())) return false;

            m_strLog = string.Format("P-BOX 입고 요구.. Fork1=[{0}] Fork2=[{1}]", nFork1LuggNum, nFork2LuggNum);
            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
            return true;
        }

        // @@.해당 작업대에 올라와 있는 적재물번호. 없으면 0.
        private int GfGetStnLuggNo(int nStation)
        {
            m_BDb.ParamsClear();
            m_strSql = modDefApp.CRLF + "  SELECT COALESCE(LUGG_NO_RD,'0') AS LUGG_NO_RD ";
            m_strSql += modDefApp.CRLF + "    FROM CV_DATA                                ";
            m_strSql += modDefApp.CRLF + "   WHERE WH_TYP      = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "     AND HOST_STN_NO = " + m_BDb.ParamsAdd("HOST_STN_NO", nStation.ToString());

            int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
            if (iCnt <= 0) return 0;

            int nLugg = 0;
            int.TryParse(m_BDb.dtMain.Rows[0]["LUGG_NO_RD"].ToString().Trim(), out nLugg);
            return nLugg;
        }

        // @@.해당 출발 작업대에 아직 실행되지 않은 작업이 있는지.(원본 GetUnExcutedJob 에 해당)
        private bool GfHasUnExcutedJob(int nStation)
        {
            m_BDb.ParamsClear();
            m_strSql = modDefApp.CRLF + "  SELECT COUNT(*) AS CNT                          ";
            m_strSql += modDefApp.CRLF + "    FROM JOB_MST                                 ";
            m_strSql += modDefApp.CRLF + "   WHERE WH_TYP    = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "     AND START_POS = " + m_BDb.ParamsAdd("START_POS", nStation.ToString());
            m_strSql += modDefApp.CRLF + "     AND COALESCE(JOB_STATUS,'0')::INTEGER < " + ((int)modDefApp.EN_JOB_STATUS.enJobStatusComplete).ToString();

            int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
            if (iCnt <= 0) return false;

            int nCnt = 0;
            int.TryParse(m_BDb.dtMain.Rows[0]["CNT"].ToString().Trim(), out nCnt);
            return (nCnt > 0);
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
            // @.원본과 같이 완료보고 전문으로 보낸다.(완료차수만 2)
            m_strHostCmd = "F";

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

            /*
             * @.소켓이 붙기 전이면 여기서 그만둔다.
             *
             *   아래에서 JOB_STATUS 를 먼저 올려놓고 그 뒤에 소켓을 확인했었다.
             *   그래서 기동 직후처럼 아직 연결이 안 된 시점에 한 번 돌면
             *   보고는 못 보냈는데 상태만 올라가, 그 작업의 도착보고가 영영 나가지
             *   않았다. 원본은 보고를 보낸 뒤에 상태를 정리한다.
             */
            if (!m_blSockConnected)
            {
                return false;
            }

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
            /*
             * 도착 보고는 별도 전문이 아니다.
             *
             *   원본 CHostCl::ArrivedReport(EcsSv/HostCl.cpp) 는 완료보고와 똑같은
             *   전문을 쓰되 완료차수(Step Count)만 2(최종완료)로 보낸다.
             *     STX + CMD_COMPLETE + 작업구분 + 창고구분 + 작업번호(4)
             *         + 완료구분 + 2 + 도착작업대(3) + ETX
             *   문서 IV.5 도 완료차수를 "1:1차완료, 2:최종완료" 로 정의한다.
             *
             *   예전 코드는 문서에도 원본에도 없는 'A' 전문을 만들고 있었다.
             *   (EcsEnv.h 에 CMD_LOAD_ARRV='A' 가 있긴 하나 원본 ECS 는 쓰지 않는다)
             *
             *   작업구분 -> 완료구분 / 작업대 매핑은 완료보고와 같다.
             */
            int nClass = 0;
            switch (nJobType)
            {
                case 1: nStation = Convert.ToInt16(strSPosition); nClass = 1; break;
                case 2: nStation = Convert.ToInt16(strDPosition); nClass = 2; break;
                case 3: nStation = Convert.ToInt16(strDPosition); nClass = 2; break; // @.피킹은 출고와 동일
                case 4: nStation = 0;                             nClass = 3; break;
                case 5: nStation = 0;                             nClass = 3; break;
                case 6: nStation = Convert.ToInt32(strDPosition); nClass = 3; break;
                default:
                    m_strLog = "작업정보는 존재하지만 잘못된 작업 정보입니다.[작업 타입:" + nJobType.ToString() + "]";
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                    return false;
            }

            strTemp = string.Format("F{0:0}{1}{2:0000}{3:0}{4:0}{5:000}",
                                    nJobType, modDefApp.WH_DEF, nLuggNum, nClass,
                                    modDefApp.STEP_FINAL, nStation);

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
