using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Microsoft.VisualBasic;
using System.Data;

namespace TSK_HostCom
{
	partial class CCliWork
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
        // @.S/C 상태보고용 크레인별 마지막 작업번호 (2026-11-05). 완료 때 IO 가 포크 데이터 삭제(DELFK1)를 내고 SC 태스크가 PLC D199 에 써서
        //   PLC 의 포크 작업번호(LUGG_NO_FK1_RD)가 0 이 된다. 구 ECS 때처럼 완료 뒤에도 WMS 에 번호가 남도록, PLC 값과 내부 값이 모두 0 이면
        //   마지막으로 보고한 번호를 그대로 싣는다. 새 작업이 잡히면 그 번호로 바뀐다. (키 = PLC_NO + SC_NO)
        private System.Collections.Generic.Dictionary<string, int> m_dicScLastLugg = new System.Collections.Generic.Dictionary<string, int>();
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
			string strErrMsg = "";

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

					// @.상위와 붙었다. 클라이언트가 보는 접속상태를 남긴다.
					if (!UpdateEQP_MST(ref m_BDb, "10", "HOST", "01", "Y", ref strErrMsg))
					{
						throw new Exception(strErrMsg);
					}

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

			// @.끊겼다. 접속상태를 내린다.
			UpdateEQP_MST(ref m_BDb, "10", "HOST", "01", "N", ref strErrMsg);

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
        private DataTable m_dtJobSnap = null;      // IsJobExist 가 조회한 행들의 복사본

        public int IsJobExist(int nJobStatus
                        , ref int nJobType
                        , ref string strUserID
                        , ref string strSScNum
                        , ref string strDScNum
                        , ref string strSPosition
                        , ref string strDPosition
                        , ref string strLuggNum
                        , string strJobTypNotIn = ""
                        , int nRow = 0)
        {
            string strTitle = "[IsJobExist]";
            string strTemp;

            try
            {
                // @.(2026-11-15) 한 바퀴에 여러 건을 보내려고 nRow 번째 행을 돌려준다.
                //   첫 행(nRow 0)에서 조회하고 결과를 복사해 둔다. 뒤의 행은 그 복사본에서 꺼낸다.
                //   (보고를 만드는 동안 다른 조회가 m_BDb.dtMain 을 바꿔도 복사본은 그대로다)
                if (nRow == 0)
                {
                    m_BDb.ParamsClear();

                    m_strSql = "";

                    m_strSql = modDefApp.CRLF + " SELECT * FROM JOB_MST                   ";
                    m_strSql += modDefApp.CRLF + "  WHERE JOB_STATUS = " + m_BDb.ParamsAdd("JOB_STATUS", nJobStatus.ToString());
                    m_strSql += modDefApp.CRLF + "    AND WH_TYP     IN " + modDefApp.GsWhTypList();
                    if (strJobTypNotIn != "")
                    {
                        // 해당 상태가 최종이 아닌 작업구분을 뺀다
                        m_strSql += modDefApp.CRLF + "    AND JOB_TYP NOT IN (" + strJobTypNotIn + ")";
                    }
                    m_strSql += modDefApp.CRLF + "  ORDER BY UPD_DT, LUGG_NO";
                    int nSelCnt0 = m_BDb.ExcuteQry_Par(ref m_strSql);
                    if (nSelCnt0 < 0)
                    {
                        return 0;
                    }
                    m_dtJobSnap = (nSelCnt0 == 0) ? null : m_BDb.dtMain.Copy();
                }

                if (m_dtJobSnap == null || nRow >= m_dtJobSnap.Rows.Count)
                {
                    return -2;       // 완료보고할 작업이 없는 상황도 정상이긴함!
                }
                DataRow rowJ = m_dtJobSnap.Rows[nRow];
                nJobType = Convert.ToInt32(rowJ["JOB_TYP"].ToString());

                // @.완료보고에 실을 창고구분. 기존 ECS 도 완료보고에는
                //   JobItem.m_nStartWareHouse(작업의 창고) 를 싣는다.
                m_strJobWhDef = modDefApp.GsWhDefOf(rowJ["WH_TYP"].ToString());

                switch (nJobType)
                {
                    // 입고
                    // @.입고 - 반자동(11)도 같이 받는다
                    case 11:
                    case 1:
                        strSPosition = rowJ["START_POS"].ToString();
                        strDPosition = rowJ["DEST_LOCATION"].ToString();
                        //strDPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        if (nJobStatus == 12)
                        {
                            strSPosition = rowJ["HS_TRACK_NO"].ToString();
                        }
                        strDScNum = rowJ["DEST_POS"].ToString();
                        break;

                    // 출고
                    // @.출고 - 반자동(12)도 같이 받는다
                    case 12:
                    case 2:
                        strSPosition = rowJ["START_LOCATION"].ToString();
                        //strSPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strDPosition = rowJ["DEST_POS"].ToString();
                        if (nJobStatus == 22)
                        {
                            strDPosition = rowJ["HS_TRACK_NO"].ToString();
                        }
                        strSScNum = rowJ["START_POS"].ToString();
                        break;

                    // 피킹
                    //   원본 CLib::ConvertJobTypeToPattern(EcsCL/Lib.cpp) 에서
                    //   PICKING(3) 은 UNIT_RET(2) 와 같은 JOB_PATTERN_RET 로 묶여 있다.
                    //   원본 CJobItem::GetLog 의 RET 패턴도 출발=[창고][위치][로케이션] 도착=[창고][위치] 이므로
                    //   출고(case 2)와 똑같이 출발은 로케이션, 도착은 스테이션을 쓴다.
                    // @.피킹출고 - 반자동(13)도 같이 받는다
                    case 13:
                    case 3:
                        strSPosition = rowJ["START_LOCATION"].ToString();
                        strDPosition = rowJ["DEST_POS"].ToString();
                        if (nJobStatus == 22)
                        {
                            strDPosition = rowJ["HS_TRACK_NO"].ToString();
                        }
                        strSScNum = rowJ["START_POS"].ToString();
                        break;

                    // RACK TO RACK (호기내)
                    // @.랙투랙 - 반자동(14)도 같이 받는다
                    case 14:
                    case 4:
                        strSPosition = rowJ["START_LOCATION"].ToString();
                        //strSPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strDPosition = rowJ["DEST_LOCATION"].ToString();
                        //strDPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strSScNum = strDScNum = rowJ["START_POS"].ToString();
                        break;

                    // RACK TO RACK (호기간 출고)
                    // @.호기간이동 - 반자동(15)도 같이 받는다
                    case 15:
                    case 5:
                        strSPosition = rowJ["START_LOCATION"].ToString();
                        //strSPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strDPosition = rowJ["DEST_LOCATION"].ToString();
                        //strDPosition = strTemp.Substring(0, 2) + strTemp.Substring(4, 2) + strTemp.Substring(7, 2);
                        strSScNum = rowJ["START_POS"].ToString();
                        strDScNum = rowJ["DEST_POS"].ToString();
                        if (nJobStatus == 12)
                        {
                            strDPosition = rowJ["HS_TRACK_NO"].ToString();
                        }
                        if (nJobStatus == 22)
                        {
                            strSPosition = rowJ["HS_TRACK_NO"].ToString();
                        }
                        break;
                    // @.이동 - 반자동(10)도 같이 받는다
                    case 10:
                    case 6:
                        strSPosition = rowJ["START_POS"].ToString();
                        strDPosition = rowJ["DEST_POS"].ToString();
                        break;
                }
                strUserID = rowJ["UPD_USER_ID"].ToString();
                strLuggNum = rowJ["LUGG_NO"].ToString();

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
        //------------------------------------------------------------------
        //설명         : 에러가 아닌데 HOST_ERR_SEND_YN='N' 으로 서 있는 행을
        //               상위에 보고하지 않고 'Y' 로만 내린다.
        //                 SC : 상태가 에러(UCSTATUS_RD='4')가 아니거나 에러코드가 0
        //                 CV : 에러코드가 0
        //               원본 ECS 의 CSc::ErrorCheck 와 같은 규칙이다.
        //------------------------------------------------------------------
        private bool ClearNoErrorFlag(string strEQP_TYP)
        {
            string strTitle = "[ClearNoErrorFlag] .. ";

            try
            {
                m_BDb.BeginTrans();
                m_BDb.ParamsClear();

                m_strSql = "";
                if (strEQP_TYP == "SC")
                {
                    m_strSql += modDefApp.CRLF + " UPDATE SC_DATA                                              ";
                    m_strSql += modDefApp.CRLF + "    SET HOST_ERR_SEND_YN = 'Y'                               ";
                    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP           IN " + modDefApp.GsWhTypList();
                    m_strSql += modDefApp.CRLF + "    AND HOST_ERR_SEND_YN = 'N'                               ";
                    m_strSql += modDefApp.CRLF + "    AND (    " + modDefApp.NVL + "(ERR_CODE_RD, '0') IN ('0', '0000', '')  ";
                    m_strSql += modDefApp.CRLF + "          OR " + modDefApp.NVL + "(UCSTATUS_RD, '0') <> '4' )              ";
                }
                else if (strEQP_TYP == "CV")
                {
                    m_strSql += modDefApp.CRLF + " UPDATE CV_DATA                                              ";
                    m_strSql += modDefApp.CRLF + "    SET HOST_ERR_SEND_YN = 'Y'                               ";
                    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP           IN " + modDefApp.GsWhTypList();
                    m_strSql += modDefApp.CRLF + "    AND HOST_ERR_SEND_YN = 'N'                               ";
                    m_strSql += modDefApp.CRLF + "    AND " + modDefApp.NVL + "(ERROR_CODE, '0') IN ('0', '0000', '')        ";
                }
                else
                {
                    m_BDb.RollbackTrans();
                    return true;
                }

                if (m_BDb.ExcuteNonQry_Par(ref m_strSql) < 0)
                {
                    m_BDb.RollbackTrans();
                    modCmWork.ShowMsgClient(strTitle + m_BDb.ErrMsg, modDefApp.MSG_ERR);
                    return false;
                }

                m_BDb.CommitTrans();
                return true;
            }
            catch (Exception ex)
            {
                modCmWork.ShowMsgClient(strTitle + "실행중 예외 발생! " + ex.Message, modDefApp.MSG_ERR);
                return false;
            }
        }


        /*
         * 상위에 보낼 에러 정보를 하나 집어 온다.
         *
         *   strDeviceNo   : DB 를 다시 찾을 때 쓰는 열쇠(SC_DATA/CV_DATA 의 MC_NO)
         *   strHostDevNo  : 상위에 실제로 보낼 장비번호.
         *                   이 둘이 다르다. 상태 보고(IV.3)는 이미 구분해 쓰고 있었는데
         *                   에러 보고(IV.4)만 MC_NO 를 그대로 보내고 있었다.
         *                     S/C : DB 는 901~911, 상위는 호기번호 1~11
         *                     C/V : DB 는 트랙번호(358), 상위는 작업대번호(209)
         *                   같은 설비가 전문마다 다른 번호로 나가고 있었다.
         */
        public int IsEquip_ERROR_Modified(string strEQP_TYP
                                    , ref string strDeviceNo
                                    , ref string strHostDevNo
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
                    //  마스터에 없는 코드를 '0000'(정상)으로 바꿔 버리면 진짜 에러가 정상으로 보고된다.
                    //  (SC 0141 은 EQP_ECD_MST 에 EQP_TYP='SC' 로 등록되어 있지 않다)
                    m_strSql += modDefApp.CRLF + " SELECT " + modDefApp.NVL + "(B.EQP_ERR_CD, A.ERR_CODE_RD) AS MC_ERR_CD  ";
                    m_strSql += modDefApp.CRLF + "      , A.*                                                       ";
                    m_strSql += modDefApp.CRLF + "   FROM SC_DATA A                                                 ";
                    m_strSql += modDefApp.CRLF + "   LEFT OUTER JOIN EQP_ECD_MST B                                  ";
                    m_strSql += modDefApp.CRLF + "     ON A.ERR_CODE_RD         = B.EQP_ERR_CD                      ";
                    m_strSql += modDefApp.CRLF + "    AND B.EQP_TYP             = " + m_BDb.ParamsAdd("EQP_TYP", strEQP_TYP);
                    m_strSql += modDefApp.CRLF + "  WHERE A.WH_TYP              IN " + modDefApp.GsWhTypList();
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
                    string strLuggFk2 = "" + m_BDb.dtMain.Rows[0]["LUGG_NO_FK2_RD"].ToString();
                    string strItnFk1 = "" + m_BDb.dtMain.Rows[0]["ITN_LUGG_FK1"].ToString();
                    string strItnFk2 = "" + m_BDb.dtMain.Rows[0]["ITN_LUGG_FK2"].ToString();
                    string strJobTypOd = ("" + m_BDb.dtMain.Rows[0]["JOB_TYP_OD"].ToString()).Trim();   // 우리가 지시한 작업구분 (1 입고, 2 출고, 4 랙투랙)
                    bool bNoReport = false;     // 구 ECS 가 보고하지 않던 경우 (아래 이중입고 / 공출고)
                    strErrorCode = "" + m_BDb.dtMain.Rows[0]["MC_ERR_CD"].ToString();

                    // @.상위에 보낼 장비번호는 호기번호다. DB 의 9xx 에서 900 을 뺀다.
                    //   (상태 보고 GetStatusReport 가 하는 것과 같다)
                    string strScNo = "" + m_BDb.dtMain.Rows[0]["SC_NO"].ToString().Trim();
                    if (strScNo.Length == 0) strScNo = strDeviceNo;

                    int nScNo = 0; int.TryParse(strScNo, out nScNo);
                    if (nScNo > 900) nScNo -= 900;
                    strHostDevNo = nScNo.ToString();
                    m_strEqpWhDef = modDefApp.GsWhDefOf(m_BDb.dtMain.Rows[0]["WH_TYP"].ToString());
                    m_nEqpPlcId = 0;            // 크레인 에러의 PLC ID 는 구 ECS 에서 늘 0 이었다


                    string strSBank = "" + m_BDb.dtMain.Rows[0]["START_BANK_FK1_RD"].ToString();
                    string strSBay = "" + m_BDb.dtMain.Rows[0]["START_BAY_FK1_RD"].ToString();
                    string strSLevel = "" + m_BDb.dtMain.Rows[0]["START_LEVEL_FK1_RD"].ToString();
                    string strDBank = "" + m_BDb.dtMain.Rows[0]["DEST_BANK_FK1_RD"].ToString();
                    string strDBay = "" + m_BDb.dtMain.Rows[0]["DEST_BAY_FK1_RD"].ToString();
                    string strDLevel = "" + m_BDb.dtMain.Rows[0]["DEST_LEVEL_FK1_RD"].ToString();

                    strBank = "00";
                    strBay = "000";
                    strLevel = "00";
                    
                    /*
                     * 위치는 지금 들고 있는 작업의 것이다.
                     *
                     *   START_*_FK1_RD / DEST_*_FK1_RD 는 크레인이 마지막으로 읽어 준 값이라
                     *   작업이 끝난 뒤에도 그대로 남아 있다. 작업번호가 없는데 그 값을 실어
                     *   보내면 상위는 있지도 않은 작업의 위치에서 에러가 난 것으로 받는다.
                     *   (원본 CHostCl::Error 도 위치는 작업에서 가져온다)
                     */
                    int nScLugg = 0; int.TryParse(strLuggNo.Trim(), out nScLugg);
                    if (nScLugg > 0)
                    {
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
                    }

                    strErrorKind = "0";         // 기계적 에러 
                    strDeviceClass = "1";       // SC

                    switch (strErrorCode)
                    {
                    case "0054": strErrorKind = "1"; break;     // 이중입고
                    case "0055": strErrorKind = "1"; break;     // 이중입고
                    // @.입고장애 56 / 출고장애 57 은 기계적 에러(종류 0)로 보낸다 (2026-11-06).
                    //   문서에는 종류 2(입고장애) / 4(출고장애)가 있으나 구 ECS(CScInfo::SetErrorCode)는
                    //   54/55/58/59 를 뺀 모든 코드를 종류 0 으로 보냈다. 상위는 종류 2 / 4 를 받아 본 적이 없다.
                    case "0058": strErrorKind = "3"; break;     // 공출고
                    case "0059": strErrorKind = "3"; break;     // 공출고
                    }

                    /*
                     * @.이중입고 / 공출고 보고는 구 ECS(Sc.cpp CSc::ErrorCheck) 와 같게 한다 (2026-11-05).
                     *
                     *   구 ECS 는 이 네 코드(54/55/58/59)를 기계적 에러 보고(CScInfo::SetErrorCode)에서 빼고 ErrorCheck 에서만 보고했다.
                     *     - 작업번호는 크레인이 읽어 준 값이 아니라 우리가 지시해 기억해 둔 번호(m_nInternalLuggNum)다.
                     *       포크#2 에러(55 / 59)면 포크#2 의 번호다. 지금은 ITN_LUGG_FK1 / FK2 가 그 값이다.
                     *     - 지시한 작업이 없으면(번호 0) 로그만 남기고 보고하지 않았다.
                     *     - 출고 작업 중의 이중입고, 입고 작업 중의 공출고는 "SC 로직체크" 로그만 남기고 보고하지 않았다.
                     *     - 에러종류는 이중입고 1, 공출고 3 이다.
                     *   전에는 크레인이 읽어 준 번호를 먼저 썼고, 위 두 경우에도 보고가 나갔다.
                     */
                    if (strErrorKind == "1" || strErrorKind == "3")
                    {
                        bool bFk2Err = (strErrorCode == "0055" || strErrorCode == "0059");
                        int nJobLugg = 0; int.TryParse((bFk2Err ? strItnFk2 : strItnFk1).Trim(), out nJobLugg);

                        string strSkip = "";
                        if (nJobLugg <= 0) strSkip = "지시한 작업이 없음";
                        else if (strErrorKind == "1" && strJobTypOd == "2") strSkip = "출고 작업 중 이중입고 (SC 로직체크)";
                        else if (strErrorKind == "3" && strJobTypOd == "1") strSkip = "입고 작업 중 공출고 (SC 로직체크)";

                        if (strSkip != "")
                        {
                            bNoReport = true;
                            m_strLog = string.Format("에러 보고 생략.. SC {0}호기 에러코드=[{1}] 작업번호=[{2}] 사유=[{3}] (구 ECS 와 같음)",
                                strHostDevNo, strErrorCode, nJobLugg, strSkip);
                            modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
                        }
                        else
                        {
                            strLuggNo = nJobLugg.ToString("0000");

                            // 위치 : 이중입고는 작업의 도착 위치, 공출고는 작업의 출발 위치 (JOB_MST 의 참 뱅크).
                            //   크레인이 읽어 준 BANK 는 1 / 2 로 접힌 값이라 쓰지 않는다.
                            m_BDb.ParamsClear();
                            m_strSql = "";
                            m_strSql += modDefApp.CRLF + " SELECT START_LOCATION, DEST_LOCATION ";
                            m_strSql += modDefApp.CRLF + "   FROM JOB_MST                        ";
                            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP IN " + modDefApp.GsWhTypList();
                            m_strSql += modDefApp.CRLF + "    AND LUGG_NO = " + m_BDb.ParamsAdd("LUGG_NO", nJobLugg.ToString("0000"));
                            if (m_BDb.ExcuteQry_Par(ref m_strSql) > 0)
                            {
                                string strJobLoc = "" + m_BDb.dtMain.Rows[0][(strErrorKind == "1") ? "DEST_LOCATION" : "START_LOCATION"].ToString();
                                string strDigit = "";
                                foreach (char ch in strJobLoc) if (ch >= '0' && ch <= '9') strDigit += ch;
                                if (strDigit.Length == 7 && strDigit != "0000000")
                                {
                                    strBank  = strDigit.Substring(0, 2);
                                    strBay   = strDigit.Substring(2, 3);
                                    strLevel = strDigit.Substring(5, 2);
                                }
                            }
                        }
                    }

                    // 2.MES SEND STATUS ('N' -> 'Y') UPDATE
                    m_BDb.BeginTrans();
                    m_BDb.ParamsClear();

                    m_strSql = "";
                    m_strSql += modDefApp.CRLF + " UPDATE SC_DATA                       ";
                    m_strSql += modDefApp.CRLF + "    SET HOST_ERR_SEND_YN = 'Y'         ";
                    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP          IN " + modDefApp.GsWhTypList();
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
                    if (bNoReport) return -1;       // 보고 대기 표시만 내리고 전문은 보내지 않는다
                    return 1;
                }
                else if (strEQP_TYP == "CV")
                {
                    m_BDb.ParamsClear();

                    // 1.ERROR CODE에 대해 존재여부확인.
                    m_strSql = "";
                    m_strSql += modDefApp.CRLF + " SELECT " + modDefApp.NVL + "(B.EQP_ERR_CD, A.ERROR_CODE) AS MC_ERR_CD ";
                    m_strSql += modDefApp.CRLF + "      , A.*           ";
                    m_strSql += modDefApp.CRLF + "   FROM CV_DATA A     ";
                    m_strSql += modDefApp.CRLF + "   LEFT OUTER JOIN EQP_ECD_MST B ";
                    m_strSql += modDefApp.CRLF + "     ON A.ERROR_CODE           = B.EQP_ERR_CD   ";
                    m_strSql += modDefApp.CRLF + "    AND B.EQP_TYP              = " + m_BDb.ParamsAdd("EQP_TYP", strEQP_TYP);
                    m_strSql += modDefApp.CRLF + "  WHERE A.WH_TYP               IN " + modDefApp.GsWhTypList();
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

                    // @.상위에 보낼 장비번호는 작업대번호다. 트랙번호(MC_NO)가 아니다.
                    //   매칭은 CV_DATA.HOST_STN_NO 에 있다. (상태 보고와 같은 자리)
                    strHostDevNo = "" + m_BDb.dtMain.Rows[0]["HOST_STN_NO"].ToString().Trim();
                    if (strHostDevNo.Length == 0) strHostDevNo = strDeviceNo;
                    m_strEqpWhDef = modDefApp.GsWhDefOf(m_BDb.dtMain.Rows[0]["WH_TYP"].ToString());
                    strErrorKind = "0";         // 기계적 에러 
                    // 컨베이어 에러의 PLC ID 는 그 트랙이 속한 컨베이어 PLC 번호다 (구 ECS CCvTrackInfo::m_nPlcID, 1 ~ 8)
                    m_nEqpPlcId = 0; int.TryParse(("" + m_BDb.dtMain.Rows[0]["PLC_NO"].ToString()).Trim(), out m_nEqpPlcId);
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
                    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP           IN " + modDefApp.GsWhTypList();
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
        // @.보고 주기 분리 (2026-11-09)
        //   통신 스레드는 0.25초마다 돈다. 에러(E)/공파렛트·P-BOX 입고 요구는 전처럼 1초에 한 번만 하고(상태(S)는 2026-11-11 부터 매 바퀴),
        //   완료(F)/도착 보고는 매 바퀴 본다. 전에는 모두 1초에 한 번이라 이동 완료(11)가 된 뒤 F 보고가 최대 1초 늦었다.
        private DateTime m_dtSlowRpt = DateTime.MinValue;
        private const int DEF_SLOW_RPT_MS = 1000;
        private DateTime m_dtStatusRpt = DateTime.MinValue;
        private const int DEF_STATUS_RPT_MS = 50;      // (2026-11-23) 0.25초 -> 0.05초. 변경 보고가 바뀐 설비만 보내게 되어 매 바퀴 봐도 가볍다

		public bool GetSendData()
		{
            DataTable_Dispose();

            // @.비동기 송신 : 응답을 받은(또는 실패한) 전송을 먼저 마무리한다. 작업 삭제 같은 ACK 뒤의 일이 여기서 돈다. (2026-11-15)
            ProcessSendDone();

            bool bSlowRpt = (DateTime.Now - m_dtSlowRpt).TotalMilliseconds >= DEF_SLOW_RPT_MS;
            if (bSlowRpt) m_dtSlowRpt = DateTime.Now;

            /*
             * @.완료(F)/도착 보고를 한 바퀴의 맨 앞으로 옮겼다. (2026-11-22)
             *
             *   IO 가 도착을 잡아 작업 상태를 19 로 바꾼 뒤 WMS 에 F 가 나가기까지가 현장에서 0.3~1.1초(중앙값 0.77초)였다.
             *   바퀴 사이 대기 0.25초에 더해, 한 바퀴 안에서 완료 보고가 상태/에러 보고 조회 뒤에 서 있어
             *   그 조회들이 끝나야 완료 보고에 닿았다. 기존 ECS 는 도착 판정과 같은 프로세스에서 바로 보고했다.
             *   완료/도착 보고는 매 바퀴 먼저 보고, 상태/에러 보고는 뒤에서 하되 상태 보고는 전처럼 0.25초에 한 번으로 묶는다.
             */
            GetJobCompleteReport();     // @.작업 완료 보고(F) 1차완료   문서 IV.5
            GetLoadArrivalReport();     // @.도착 보고. 원본은 완료보고(F) 최종완료로 보낸다

            bool bStatusRpt = (DateTime.Now - m_dtStatusRpt).TotalMilliseconds >= DEF_STATUS_RPT_MS;
            if (bStatusRpt) m_dtStatusRpt = DateTime.Now;

            if (modDefApp.g_frmForm.chkSimMode.Checked == false)
            {
                // @.상태 보고(S). 안에서 30초 경과 여부를 보고 정기보고/변경보고를 가른다.
                //   원본 문서 : "상태가 변경되면 즉시 + 정기적으로 30초에 1회"
                //   (2026-11-11) 상태 변경을 빨리 알리려고 0.25초마다 본다. 정기보고 30초 규칙은 그대로다.
                //   (2026-11-22) 바퀴가 0.05초로 짧아져서 상태 조회만 0.25초에 한 번으로 묶는다.
                if (bStatusRpt) GetStatusReport();      // @.상태 보고(S)          문서 IV.3
                if (bSlowRpt)
                {
                    GetErrorReport();       // @.에러 보고(E)          문서 IV.4
                    GetPmStoRequest();      // @.공파렛트 입고 요구(N) 문서 IV.8
                    GetBoxStoRequest();     // @.P-BOX 입고 요구(L)    문서 IV.9
                }

                /*
                 * 아래 두 가지는 없앴다.
                 *   GetWeightReport()    : 무게 보고(UG). 문서에도 원본에도 없는 전문이다.
                 *   GetEmptyPltRequest() : 문서 IV.8 의 공파렛트 입고 요구는
                 *                          GetPmStoRequest() 가 담당한다. 중복이었다.
                 */
            }

			return true;
		}
        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 상태 보고  
        /*
         * @.정기 보고(30초) 판정을 메모리의 시각으로 한다. (2026-11-23)
         *
         *   전에는 EQP_MST 의 EQP_TYP = 'HOST2' 행의 UPD_DT 가 30초 안쪽인지를 조회해서 정했다.
         *   그런데 그 행이 로컬 DB 에도 현장 DB(2026-11-22 덤프)에도 없다. 그래서 조회는 늘 0건이고
         *   "30초가 지났다" 로 판정되어, 바뀐 것이 없어도 전체 설비(크레인 11 + 작업대 24 = 35건)의 상태 전문이
         *   쉬지 않고 계속 나갔다. 상위와는 한 건씩 응답을 받고 보내므로 완료 / 도착 보고(F)도 이 줄 뒤에 서서 늦어졌다.
         *   (로컬 측정 : 상태 전문이 초당 약 60건씩 끊임없이 나갔다)
         *
         *   이제 마지막으로 전체를 보낸 시각을 기억해 두고, 30초가 지났을 때만 전체를 보낸다. 그 사이에는 바뀐 설비만 보낸다.
         *   처음 켰을 때와 상위에 다시 붙었을 때는 시각이 비어 있어 전체를 한 번 보낸다.
         */
        private DateTime m_dtStatusFull = DateTime.MinValue;
        private const int DEF_STATUS_FULL_S = 30;

        private void GetStatusReport()
        {
            if (!m_blSockConnected)
            {
                m_dtStatusFull = DateTime.MinValue;     // 다시 붙으면 전체를 한 번 보낸다
                return;
            }

            bool bTime = (DateTime.Now - m_dtStatusFull).TotalSeconds >= DEF_STATUS_FULL_S;
            if (bTime)
            {
                // 앞 묶음이 가는 중이면 GetStatusReport(true) 는 아무것도 하지 않고 true 를 돌려준다. 그때는 시각을 찍지 않는다.
                bool bWasInFlight = m_bStatusInFlight;
                if (GetStatusReport(true) && !bWasInFlight)
                    m_dtStatusFull = DateTime.Now;
            }
            else
            {
                GetStatusReport(false);
            }
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

            // @.앞 상태 보고 묶음이 ACK 를 기다리는 중이면 다시 만들지 않는다. (2026-11-15)
            if (m_bStatusInFlight)
                return true;

            int nJobType = 0;
            string strTempFrame = null;

            #region SC 상태값을 가져온다
            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "   SELECT  *                 ";
            m_strSql += modDefApp.CRLF + "    FROM  SC_DATA           ";
            m_strSql += modDefApp.CRLF + "   WHERE  WH_TYP IN " + modDefApp.GsWhTypList();
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
            // @.작업대번호 / 상태 / 적재물번호 / 창고구분. 창고는 작업대마다 다르다.
            System.Collections.Generic.List<object[]> lstCvPending = new System.Collections.Generic.List<object[]>();

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

            /*
             * @.변경 보고는 바뀐 설비만 보낸다. (2026-11-23)
             *
             *   전에는 설비 하나만 바뀌어도 크레인 11대 + 작업대 24곳 = 35건을 모두 다시 보냈다. 상위와는 한 건 보내고
             *   응답을 받은 뒤 다음 건을 보내므로, 바뀐 작업대의 전문은 앞선 전문들이 다 나간 뒤에야 나갔다. 묶음이 가는
             *   중에는 다음 묶음을 만들지도 않는다. (로컬 측정 : 바뀐 뒤 그 작업대 전문이 나가기까지 중앙값 0.69초, 최대 1.0초)
             *   기존 ECS 는 트랙마다 상태가 바뀔 때 그 트랙만 보고했다. (CCvTrackInfo::StatusReport)
             *
             *   변경 보고(bTimeReport == false) : HOST_SEND_YN = 'N' 인 설비만 보낸다.
             *       큐에 넣기 전에 그 설비들을 'Y' 로 바꿔 둔다. 가는 도중에 값이 또 바뀌면 설비 태스크가 다시 'N' 으로
             *       세우므로 다음 바퀴에 한 번 더 나간다. (전에는 응답을 받은 뒤에 'Y' 로 바꿔, 그 사이의 변경이 묻혔다)
             *       전송이 실패하면 다시 'N' 으로 되돌려 다음에 다시 보낸다.
             *   정기 보고(30초, bTimeReport == true) : 전처럼 전체를 보낸다.
             */
            bool bPartial = (bTimeReport == false);
            string strScSentList = "'0'";

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
                int nScItn = 0; int.TryParse(strITN_LUGG_FK1.Trim(), out nScItn);
                // @.보고하는 작업번호는 S/C PLC 가 읽어 올린 값(LUGG_NO_FK1_RD)이다. 작업이 끝나도 PLC 는 번호를 지우지 않으므로
                //   WMS 에서 계속 보인다. (구 ECS ScInfo.m_nLuggNum = PLC 값을 그대로 보고)
                //   예전에는 내부 작업번호(ITN_LUGG_FK1)를 보고했는데 이것은 완료 때 0 으로 지워져 완료 뒤 번호가 사라졌다.
                int nScLugg = 0; int.TryParse(("" + m_BDb.dtMain.Rows[ii]["LUGG_NO_FK1_RD"].ToString()).Trim(), out nScLugg);
                if (nScLugg == 0) nScLugg = nScItn;
                // @.완료 뒤 포크 데이터 삭제로 PLC 값이 0 이 되어도 마지막 작업번호를 계속 보고한다 (2026-11-05)
                string strScKey = strPLC_NO + "_" + strScNo;
                if (nScLugg > 0)
                    m_dicScLastLugg[strScKey] = nScLugg;
                else if (m_dicScLastLugg.ContainsKey(strScKey))
                    nScLugg = m_dicScLastLugg[strScKey];

                // @.그 크레인이 속한 창고. 11호기(P-BOX)는 'B' 로 나간다.
                string strScWhDef = modDefApp.GsWhDefOf(m_BDb.dtMain.Rows[ii]["WH_TYP"].ToString());

                bool bScChanged = (("" + m_BDb.dtMain.Rows[ii]["HOST_SEND_YN"]).Trim() == "N");
                if (bPartial == false || bScChanged)
                {
                    lstStatusFrame.Add(GfMakeStatusBody(strScWhDef, modDefApp.DEV_CLASS_SC, nScNo, nScStatus,
                                                        nScLugg,
                                                        0,                          // @.사이즈체크는 S/C 에 해당 없음
                                                        (nScItn > 0) ? 1 : 0));    // @.ECS작업유무는 진행 중인 작업이 있을 때만
                    if (bScChanged)
                        strScSentList += ",'" + ("" + m_BDb.dtMain.Rows[ii]["SC_NO"]).Trim().Replace("'", "") + "'";
                }

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
            m_strSql += modDefApp.CRLF + "     WHERE  CD.WH_TYP       IN " + modDefApp.GsWhTypList();
            /*
             * @.현장 DB 는 피킹대 / 사이즈체크 / 팔레트 매거진의 STN_KIND 가 64(도착대)뿐이다. (입고대 1, 출고대 2 비트가 없다)
             *   그래서 (STN_KIND & 3) 만 보면 이 작업대들이 상태 보고에서 빠진다. (2026-10-19 현장 DB 대조)
             *   원본은 이 작업대를 입고대/출고대로 정의해 보고했으므로 HOST_STN_NO 목록으로 함께 고른다.
             */
            m_strSql += modDefApp.CRLF + "       AND ((COALESCE(NULLIF(TRIM(CD.STN_KIND),''),'0')::INTEGER & 3) <> 0 ";
            m_strSql += modDefApp.CRLF + "            OR CD.HOST_STN_NO IN ('200','201','202','203','205','206','207','209','212','213','214','215')) ";
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
                
                // @.변경 보고는 바뀐 작업대만 (2026-11-23)
                if (bPartial && ("" + m_BDb.dtMain.Rows[iii]["HOST_SEND_YN"]).Trim() != "N")
                    continue;
                strMC_NO_LIST += ",'" + strMC_NO + "'";
                /*
                 * @.상태값 (원본 GetCvStoWorkingStatus / GetCvRetWorkingStatus / GetCvWorkingStationStatus / GetCvPickingStationStatus)
                 *   입고대만(STN_KIND&3 == 1) : 입고 준비(STO_READY_RD)
                 *   그 밖의 출고대 / 입출고대 / 피킹대 : 화물감지 센서(SENSOR0_DATA_RD)
                 *   예전에는 모든 작업대를 입고 준비 값으로만 보고해서 피킹대 / 출고대의 화물감지가 상위에 올라가지 않았다.
                 */
                int nKindVal = 0; int.TryParse(strSTN_KIND.Trim(), out nKindVal);
                bool bStoOnly = ((nKindVal & 3) == 1);
                string strHostStnNo = ("" + m_BDb.dtMain.Rows[iii]["HOST_STN_NO"]).Trim();
                bool bPalletMagazine = (strHostStnNo == "207");                 // 팔레트 매거진 : 입고 준비, 사이즈 0
                bool bSizeChecker = (strHostStnNo == "205" || strHostStnNo == "206" || strHostStnNo == "209" ||
                                     strHostStnNo == "214" || strHostStnNo == "215");
                bool bPickingStn = (strHostStnNo == "200" || strHostStnNo == "201" || strHostStnNo == "202" ||
                                    strHostStnNo == "203" || strHostStnNo == "212" || strHostStnNo == "213");
                if (bPalletMagazine) bStoOnly = true;
                else if (bSizeChecker || bPickingStn) bStoOnly = false;         // 화물감지 센서로 보고
                string strSensor = ("" + m_BDb.dtMain.Rows[iii]["SENSOR0_DATA_RD"]).Trim();
                if (bStoOnly ? (strSTO_READY_RD == "1") : (strSensor == "1"))
                {
                    nCvStatus = 1;
                }
                // @.사이즈체크 : 센서 워드 비트 10(저) / 11(고). 고가 우선 -> 2, 저 -> 1, 없으면 0 (원본과 같다)
                int nCvSize = 0;
                if (bPalletMagazine) nCvSize = 0;
                else if (("" + m_BDb.dtMain.Rows[iii]["SZ_CHK_HIGH_RD"]).Trim() == "1") nCvSize = 2;
                else if (("" + m_BDb.dtMain.Rows[iii]["SZ_CHK_LOW_RD"]).Trim() == "1") nCvSize = 1;

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
                lstCvPending.Add(new object[] { nCvStn, nCvStatus, nCvLugg,
                                                modDefApp.GsWhDefOf(m_BDb.dtMain.Rows[iii]["WH_TYP"].ToString()), nCvSize });

                strTempFrame = string.Format("{0:0}", nCvStatus);

                strTemp2 += strTempFrame;
            }
            #endregion

            #region C/V 본문 만들기 (조회가 dtMain 을 건드리므로 루프 밖에서)
            for (int nCv = 0; nCv < lstCvPending.Count; nCv++)
            {
                object[] arrCv = lstCvPending[nCv];
                int nStn = (int)arrCv[0];
                lstStatusFrame.Add(GfMakeStatusBody((string)arrCv[3], modDefApp.DEV_CLASS_CV,
                                                    nStn, (int)arrCv[1], (int)arrCv[2],
                                                    (int)arrCv[4],              // @.사이즈체크 (SZ_CHK_HIGH/LOW_RD)
                                                    GfHasUnExcutedJob(nStn) ? 1 : 0));
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
            // @.비동기 송신 (2026-11-15) : 전문을 큐에 넣고, 설비 전문이 모두 ACK 를 받으면 보냈다는 표시(HOST_SEND_YN)를 한다.
            //   한 묶음이 끝나기 전에는 다시 만들지 않는다(m_bStatusInFlight).
            if (bPartial)
            {
                if (lstStatusFrame.Count == 0)
                    return true;
                // 보내기 전에 보냈다는 표시를 먼저 한다. 가는 도중의 변경은 설비 태스크가 다시 'N' 으로 세운다.
                if (!StatusMarkSent(strScSentList, strMC_NO_LIST, "Y"))
                    return false;
            }
            else if (lstStatusFrame.Count == 0)
                return StatusAckUpdate(strMC_NO_LIST);

            string strMcListForAck = strMC_NO_LIST;
            string strScListForAck = strScSentList;
            int nStatusRemain = lstStatusFrame.Count;
            bool bStatusAllOk = true;
            m_bStatusInFlight = true;
            for (int nFrame = 0; nFrame < lstStatusFrame.Count; nFrame++)
            {
                bool bDupS = false;
                bool bQueued = SendBodyQ(lstStatusFrame[nFrame], delegate (bool bOk)
                {
                    if (!bOk) bStatusAllOk = false;
                    nStatusRemain--;
                    if (nStatusRemain == 0)
                    {
                        m_bStatusInFlight = false;
                        if (bPartial)
                        {
                            if (!bStatusAllOk) StatusMarkSent(strScListForAck, strMcListForAck, "N");   // 못 보냈다 : 다음에 다시
                        }
                        else if (bStatusAllOk) StatusAckUpdate(strMcListForAck);
                    }
                }, out bDupS);

                if (!bQueued)
                {
                    // 연결이 끊겨 넣지 못했다. 이미 넣은 것의 콜백은 실패로 끝난다. 못 넣은 나머지는 세지 않는다.
                    bStatusAllOk = false;
                    nStatusRemain -= (lstStatusFrame.Count - nFrame);
                    if (nStatusRemain <= 0)
                    {
                        m_bStatusInFlight = false;
                        if (bPartial) StatusMarkSent(strScListForAck, strMcListForAck, "N");
                    }
                    return false;
                }
                if (bDupS)
                    nStatusRemain--;        // 같은 전문이 이미 가는 중이다. 그쪽 콜백이 일한다.
            }
            if (nStatusRemain <= 0) m_bStatusInFlight = false;
            #endregion

            return true;
        }

        // 변경 보고용 : 고른 크레인 / 작업대의 HOST_SEND_YN 을 바꾼다. (2026-11-23)
        //   strYn = "Y" : 보내기 직전에 (N -> Y).   strYn = "N" : 전송이 실패해 되돌릴 때 (Y -> N)
        //   정기 보고의 시각(EQP_MST HOST2)은 건드리지 않는다. 변경 보고가 잦아도 30초 정기 보고는 그대로 나가야 한다.
        private bool StatusMarkSent(string strSC_NO_LIST, string strMC_NO_LIST, string strYn)
        {
            string strFrom = (strYn == "Y") ? "N" : "Y";
            try
            {
                m_BDb.BeginTrans();

                m_BDb.ParamsClear();
                m_strSql = "";
                m_strSql += modDefApp.CRLF + " UPDATE SC_DATA                    ";
                m_strSql += modDefApp.CRLF + "    SET HOST_SEND_YN = '" + strYn + "' ";
                m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       IN " + modDefApp.GsWhTypList();
                m_strSql += modDefApp.CRLF + "    AND SC_NO        IN (" + strSC_NO_LIST + ") ";
                m_strSql += modDefApp.CRLF + "    AND HOST_SEND_YN = '" + strFrom + "' ";
                if (m_BDb.ExcuteNonQry_Par(ref m_strSql) < 0) { m_BDb.RollbackTrans(); return false; }

                m_BDb.ParamsClear();
                m_strSql = "";
                m_strSql += modDefApp.CRLF + " UPDATE CV_DATA                    ";
                m_strSql += modDefApp.CRLF + "    SET HOST_SEND_YN = '" + strYn + "' ";
                m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       IN " + modDefApp.GsWhTypList();
                m_strSql += modDefApp.CRLF + "    AND MC_NO        IN (" + strMC_NO_LIST + ") ";
                m_strSql += modDefApp.CRLF + "    AND HOST_SEND_YN = '" + strFrom + "' ";
                if (m_BDb.ExcuteNonQry_Par(ref m_strSql) < 0) { m_BDb.RollbackTrans(); return false; }

                m_BDb.CommitTrans();
                return true;
            }
            catch
            {
                try { m_BDb.RollbackTrans(); } catch { }
                return false;
            }
        }

        // 상태 보고(S)를 모두 ACK 받은 뒤 : SC_DATA / CV_DATA 의 HOST_SEND_YN 을 Y 로, EQP_MST(HOST2) 의 시각을 갱신한다. (CliWorkThread 에서 불린다)
        private bool StatusAckUpdate(string strMC_NO_LIST)
        {
            #region 응답 받고 SC_DATA와 CV_DATA의 HOST_SEND_YN을 Y로 변경
            #region SC_DATA의 HOST_SEND_YN을 Y로 변경
            // 2.MES SEND STATUS ('N' -> 'Y') UPDATE
            m_BDb.BeginTrans();
            m_BDb.ParamsClear();

            m_strSql = "";
            m_strSql += modDefApp.CRLF + " UPDATE SC_DATA                    ";
            m_strSql += modDefApp.CRLF + "    SET HOST_SEND_YN = 'Y'         ";
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       IN " + modDefApp.GsWhTypList();
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
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       IN " + modDefApp.GsWhTypList();
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
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       IN " + modDefApp.GsWhTypList();
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
        /*
         * GfToInt :: 설비가 준 문자열을 숫자로. 빈 값이나 숫자가 아니면 0 이다.
         *   전문 자리에 넣을 값을 만들 때만 쓴다. 여기서 예외가 나면 스레드가 죽는다.
         */
        private static int GfToInt(string strValue)
        {
            int nValue = 0;
            if (strValue == null) return 0;
            int.TryParse(strValue.Trim(), out nValue);
            return nValue;
        }

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

            //  에러가 아닌데 보고 대기('N')로 서 있는 행은 보고하지 않고 플래그만 내린다.
            //  설비 스레드는 에러코드가 "바뀔 때마다" HOST_ERR_SEND_YN='N' 을 세운다.
            //  그래서 에러가 풀릴 때(예: 0141 -> 0000)에도 서고, 씨앗 데이터도 'N' 으로 들어온다.
            //  그것까지 보고하면 상위에는 코드 0000(정상)짜리 에러 보고가 끝없이 나간다.
            //  원본 ECS(Sc.cpp CSc::ErrorCheck)는 상태가 ERROR 이고 코드가 0 이 아닐 때만 보고했다.
            ClearNoErrorFlag(strEQP_TYP);

            int nJobType = 0;

            #region 에러 보고해야할 작업이 있는지?

            string strDeviceNo = "";
            string strHostDevNo = "";
            string strDeviceClass = "";
            string strErrorCode = "";
            string strErrorKind = "";
            string strLuggNo = "";
            string strBank = "";
            string strBay = "";
            string strLevel = "";

            int nResult = IsEquip_ERROR_Modified(strEQP_TYP, ref strDeviceNo, ref strHostDevNo, ref strDeviceClass, ref strErrorCode, ref strErrorKind, ref strLuggNo, ref strBank, ref strBay, ref strLevel);
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
            /*
             * @.Convert.ToInt32 를 쓰면 안 된다.
             *
             *   설비가 아직 값을 안 채운 칸은 빈 문자열로 들어온다.
             *   (크레인 LUGG_NO_FK1_RD 가 대표적이다)
             *   Convert.ToInt32("") 는 FormatException 을 던지고, 이 함수에는 try 가
             *   없어 그대로 위로 올라가 통신 스레드가 통째로 죽었다.
             *   화면에는 "입력 문자열의 형식이 잘못되었습니다" 만 찍히고 곧바로
             *   "이상 발생. DB Logout" 뒤 재접속 - 이것이 끝없이 돌았다.
             *   못 읽는 칸은 0 으로 둔다. 보고를 못 하는 것보다 낫다.
             */
            // @.끝에 PLC ID 를 붙인다 (2026-11-06). 문서 표에는 없는 칸이지만 구 ECS(CHostCl::Error)는
            //   Level 뒤에 "%d" 로 PLC ID 를 붙여 보냈다. 크레인은 0, 컨베이어는 PLC 번호다. (본문 23자)
            //   화면의 "E PLC ID" 체크(기본 체크)를 풀면 붙이지 않는다. (문서 표대로 본문 22자)
            bool bSendPlcId = modDefApp.g_frmForm.chkErrPlcId.Checked;
            strTemp = string.Format("E{0}{1:0}{2:000}{3:0}{4:0000}{5:0000}{6:00}{7:000}{8:00}{9}",
                m_strEqpWhDef,
                GfToInt(strDeviceClass), GfToInt(strHostDevNo), GfToInt(strErrorKind),
                GfToInt(strErrorCode), GfToInt(strLuggNo),
                GfToInt(strBank), GfToInt(strBay), GfToInt(strLevel),
                bSendPlcId ? m_nEqpPlcId.ToString() : "");

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

            #region 메세지 보내기 (비동기 : 큐에 넣는다)
            bool bDupE = false;
            if (!QueueSrv(null, out bDupE))
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
            // @.출고/피킹출고의 1차 완료 보고(크레인 완료 때). 19(도착) 보고보다 먼저 돈다.
            GetRetFirstReport();

            int[] nStation = new int[] { 19, 29 };

            int nCount = nStation.Length;
            for (int i = 0; i < nCount; i++)
            {
                GetJobCompleteReport(nStation[i]);
            }
        }

        /*
         * 출고/피킹출고(2, 3, 12, 13)의 1차 완료 보고 - 크레인이 출고를 끝냈을 때.
         *
         *   원본 ECS 는 크레인 완료(CSc::CompleteCheck)에서 JOB->Complete(번호, 출고, 1) 로
         *   F 전문(완료차수 1)을 먼저 보내고, 화물이 출고대에 도착하면(JOB->Arrived) 같은 전문을
         *   완료차수 2 로 한 번 더 보낸다. 신규는 도착 때 차수 1 하나만 보내 상위가 크레인 완료를 몰랐다.
         *
         *   출고의 상태 흐름은 99 -> 20 -> 21 -> 29(크레인 완료) -> 11(CV) -> 19(도착) 이다.
         *   IO 스케줄러가 29 를 1초 안에 11 로 넘기므로 29 에서만 찾으면 놓친다. 크레인 완료 뒤인
         *   29 / 11 / 19 를 모두 보고, 아직 1차를 안 보낸 것(WC_STEP <> '1')을 보낸다.
         *   보낸 뒤 WC_STEP = '1' 로 적어 한 번만 나가게 한다. 상태(JOB_STATUS)는 건드리지 않는다.
         *   19 의 최종(차수 2) 보고는 WC_STEP = '1' 일 때만 나간다. (1 -> 2 순서 보장)
         */
        private bool GetRetFirstReport()
        {
            for (int n = 0; n < DEF_REPORT_QUEUE_MAX; n++)
            {
                bool bNoMore = false;
                bool bOk = GetRetFirstReportOne(ref bNoMore);
                if (bNoMore || !bOk) break;
            }
            return true;
        }

        private bool GetRetFirstReportOne(ref bool bNoMore)
        {
            string strTitle = "[GetRetFirstReport] .. ";
            m_strHostCmd = "F";
            try
            {


                m_BDb.ParamsClear();
                m_strSql = "";
                m_strSql += modDefApp.CRLF + " SELECT * FROM JOB_MST                                         ";
                m_strSql += modDefApp.CRLF + "  WHERE JOB_STATUS IN ('29', '11', '19', '28')                       ";
                m_strSql += modDefApp.CRLF + "    AND JOB_TYP    IN ('2', '3', '12', '13', '5', '15')                   ";
                m_strSql += modDefApp.CRLF + "    AND COALESCE(NULLIF(TRIM(WC_STEP), ''), '0') <> '1'        ";
                m_strSql += modDefApp.CRLF + "    AND WH_TYP     IN " + modDefApp.GsWhTypList();
                m_strSql += modDefApp.CRLF + "  ORDER BY UPD_DT                                              ";
                m_strSql += modDefApp.CRLF + "  LIMIT 1                                                      ";
                int nSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
                if (nSelCnt <= 0) { bNoMore = true; return true; }


                string strLuggNum = m_BDb.dtMain.Rows[0]["LUGG_NO"].ToString().Trim();
                int nJobType = Convert.ToInt32(m_BDb.dtMain.Rows[0]["JOB_TYP"].ToString().Trim());
                string strDPosition = m_BDb.dtMain.Rows[0]["DEST_POS"].ToString().Trim();
                string strWhTyp = m_BDb.dtMain.Rows[0]["WH_TYP"].ToString().Trim();
                int nLuggNum = Convert.ToInt32(strLuggNum);
                m_strJobWhDef = modDefApp.GsWhDefOf(strWhTyp);

                if (nJobType != 5 && nJobType != 15)
                {
                    if (!m_blSockConnected) return false;
                }

                // 반자동(9000 이상)이거나 SIM MODE 는 보고하지 않는다. 표만 해 두어 다시 찾지 않게 한다.
                bool bNoSend = (nLuggNum >= 9000 && modDefApp.g_frmForm.chkSimMode.Checked == false)
                               || modDefApp.g_frmForm.chkSimMode.Checked == true;

                if (!bNoSend)
                {
                    string strTemp = string.Format("F{0:0}{1}{2:0000}{3:0}{4:0}{5:000}",
                                                   modDefApp.GsJobDefineOfReport(nJobType), m_strJobWhDef, nLuggNum, 2,
                                                   modDefApp.STEP_FIRST, Convert.ToInt32(strDPosition));
                    int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
                    m_bytTxBuff = new byte[iTxCnt];
                    MakeHeader(strTemp.Length);
                    m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;
                    byte[] bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
                    Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);
                    m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

                    // @.1차 보고는 응답이 없어도 최종(차수 2) 보고를 막지 않는다.
                    //   상위는 차수 2 만 받아도 1차를 함께 처리한다. 응답이 없으면(RequestSrv 가 거짓) 소켓이
                    //   닫히지만 1차는 다시 보내지 않고 "보냈다"고 표시한 채 넘어가, 도착 때 차수 2 가 나가게 한다.
                    //   (연결이 끊겨 보내지도 못한 경우는 위의 연결 확인에서 먼저 돌아가므로 표시하지 않는다)
                    //   (2026-11-15) 비동기 : 큐에 넣고 바로 넘어간다. 응답이 없으면 콜백에서 같은 로그를 남긴다.
                    {
                        string strLuggFirst = strLuggNum;
                        bool bDupF = false;
                        QueueSrv(delegate (bool bOkF)
                        {
                            if (!bOkF)
                            {
                                m_strLog = string.Format("출고 1차 완료 보고에 응답이 없습니다. 다시 보내지 않고 넘어갑니다. [작업번호:{0}]", strLuggFirst);
                                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                            }
                        }, out bDupF);
                    }
                }

                m_BDb.BeginTrans();
                m_BDb.ParamsClear();
                m_strSql = "";
                m_strSql += modDefApp.CRLF + "  UPDATE JOB_MST                                               ";
                m_strSql += modDefApp.CRLF + "     SET WC_STEP  = '1'                                        ";
                m_strSql += modDefApp.CRLF + "   WHERE WH_TYP   IN " + modDefApp.GsWhTypList();
                m_strSql += modDefApp.CRLF + "     AND LUGG_NO  = " + m_BDb.ParamsAdd("LUGG_NO", strLuggNum);
                m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
                if (m_iSelCnt < 0)
                {
                    m_strLog = m_BDb.ErrMsg + m_strSql;
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                    m_BDb.RollbackTrans();
                    return false;
                }
                m_BDb.CommitTrans();

                m_strLog = string.Format("출고 1차 완료 보고(크레인 완료) 하였습니다. [작업번호:{0}]", strLuggNum);
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
            }
            catch (Exception ex)
            {
                m_strLog = strTitle + "실행중 예외 발생!" + ex.Message;
                modCmWork.ShowMsgClient(m_strLog, modDefApp.MSG_ERR);
                return false;
            }
            return true;
        }


        //최초작성자	: BASE(정복열)
        //작성일		: 20200519
        //설명		    : 완료 보고 
        // 한 바퀴에 보고를 큐에 넣는 최대 건수 / 훑어볼 최대 행 수 (2026-11-15)
        private const int DEF_REPORT_QUEUE_MAX = 20;
        private const int DEF_REPORT_SCAN_MAX = 60;

        // 지금 큐 / 대기 / 끝난 목록에 있는 전문 수 (새로 큐에 들어갔는지 세는 데 쓴다)
        private int PendingSendCount()
        {
            lock (m_objSendLock)
                return m_setSendKey.Count;
        }

        // 한 바퀴에 여러 건을 큐에 넣는다. 이미 가는 중인 건은 QueueSrv 가 건너뛴다.
        private bool GetJobCompleteReport(int nJobStatus)
        {
            int nQueued = 0;
            for (int nRow = 0; nRow < DEF_REPORT_SCAN_MAX && nQueued < DEF_REPORT_QUEUE_MAX; nRow++)
            {
                bool bNoMore = false;
                int nBefore = PendingSendCount();
                GetJobCompleteReportOne(nJobStatus, nRow, ref bNoMore);
                if (bNoMore) break;
                if (PendingSendCount() > nBefore) nQueued++;
            }
            return true;
        }

        private bool GetJobCompleteReportOne(int nJobStatus, int nRow, ref bool bNoMore)
        {
            string strTitle = "[GetJobCompleteReport] .. ";
            m_strHostCmd = "F";

            /*
             * 29(SC 구동완료)가 최종인 것은 입고다.
             * 출고/픽킹은 29 뒤에 컨베이어 구간(11 → 19)이 남으므로 여기서 보고하면 안 된다.
             *   이동(6/10)     99 → 10 → 11 → 19
             *   입고(1/11)     99 → 10 → 11 → 21 → 29
             *   출고(2,3/12,13) 99 → 20 → 21 → 29 → 11 → 19
             *
             * @.반자동도 짝으로 뺀다. 12/13 이 빠져 있어 반자동 출고·픽킹이 29 에서
             *   완료보고 대상으로 잡혔다. (뒤에 남은 CV 구간을 건너뛰게 된다)
             */
            string strJobTypNotIn = (nJobStatus == 29) ? "'2','3','12','13'" : "";

            int nJobType = 0;
            string strUserID = "";
            string strSScNum = "";
            string strDScNum = "";
            string strSPosition = "";
            string strDPosition = "";
            string strLuggNum = "";

            #region 완료 보고해야할 작업이 있는지?

            int nResult = IsJobExist(nJobStatus, ref nJobType, ref strUserID, ref strSScNum, ref strDScNum, ref strSPosition, ref strDPosition, ref strLuggNum, strJobTypNotIn, nRow);
            if (nResult == 0)
            {
                // 이미 함수내에서 리스트 박스에 디스플레이함!
                bNoMore = true;
                return false;
            }
            if (nResult < 0)
            {
                bNoMore = true;
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
            // @.출고/피킹출고가 CV 에 도착했다(19)면 최종(차수 2) 보고다. 크레인 완료 때 1차를 보낸 뒤(WC_STEP='1')에만 나간다.
            /*
             * @.입고 / 이동 / 랙투랙의 완료차수 (2026-11-23)
             *
             *   기본은 1 이다. 현장 상위(WMS)가 지금까지 받아 온 값이다.
             *   ECSCOMA.ini [Property] COMPLETE_STEP_FINAL=1 로 두면 기존 ECS 와 같이 2 로 보낸다.
             *
             *   기존 ECS 가 차수 1 로 보내는 것은 출고 / 피킹출고의 크레인 완료 하나뿐이다
             *   (Sc.cpp CompleteCheck : JOB->Complete(번호, 출고, 1)). 그 보고는 GetRetFirstReport 가 따로 보낸다.
             *   나머지는 모두 차수 2 다.
             *     입고 / 랙투랙 크레인 완료 : JOB->Complete(번호, 입고)  -> nStep 기본값 2
             *     이동 / 출고 도착          : JOB->Arrived -> ArrivedReport -> 차수 2 고정
             *     작업정보 창의 강제완료 / 도착보고 : 위와 같은 함수를 부른다 -> 차수 2
             *   출고 도착(19)과 호기간 이동(29)은 이 설정과 상관없이 아래에서 2 로 보낸다.
             */
            int nReportStep = GfIsCompleteStepFinal() ? modDefApp.STEP_FINAL : modDefApp.STEP_FIRST;
            if (nJobStatus == 19 && (nJobType == 2 || nJobType == 3 || nJobType == 12 || nJobType == 13))
            {
                // 1차 보고(GetRetFirstReport)가 응답을 받았든 못 받았든 최종(차수 2)은 나간다. 상위는 차수 2 만 받아도 1차를 처리한다.
                nReportStep = modDefApp.STEP_FINAL;
            }
            if (nJobStatus == 29 && (nJobType == 5 || nJobType == 15))
            {
                // 1차 보고(GetRetFirstReport)가 응답을 받았든 못 받았든 최종(차수 2)은 나간다. 상위는 차수 2 만 받아도 1차를 처리한다.
                nReportStep = modDefApp.STEP_FINAL;
            }
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
                // @.입고 - 반자동(11)도 같이 받는다
                case 11:
                case 1: nStation = Convert.ToInt16(strSPosition); nClass = 1;  break;
                // @.출고 - 반자동(12)도 같이 받는다
                case 12:
                case 2: nStation = Convert.ToInt16(strDPosition); nClass = 2;  break;
                // @.피킹출고 - 반자동(13)도 같이 받는다
                case 13:
                case 3: nStation = Convert.ToInt16(strDPosition); nClass = 2;  break; // @.피킹. 원본에서 출고(JOB_PATTERN_RET)와 동일
                // @.랙투랙 - 반자동(14)도 같이 받는다
                case 14:
                case 4: nStation = 0;                             nClass = 3; break; //랙투랙 부분 조한성 수정 0608
                // @.호기간이동 - 반자동(15)도 같이 받는다
                case 15:
                case 5: nStation = 0;                             nClass = 3; break;
                // @.이동 - 반자동(10)도 같이 받는다
                case 10:
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
                                    modDefApp.GsJobDefineOfReport(nJobType), m_strJobWhDef, nLuggNum, nClass,
                                    nReportStep, nStation);

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
                    // @.입고 - 반자동(11)도 같이 받는다
                    case 11:
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
                        // @.뒤따르는 작업은 바탕이 된 작업과 같은 창고에 둔다.
                        if (modDefApp.g_frmForm.InsertJobMst(m_BDb, strLuggNum, strDScNum, strDPosition, strSPosition, "00-000-00", nNewJobType.ToString()
                                                            , "", "000", "0", false, "0", modDefApp.GsWhTypOf(m_strJobWhDef)) == false)
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
                    // @.출고 - 반자동(12)도 같이 받는다
                    // @.피킹출고(3 / 반자동 13)도 같이 받는다.
                    //   원본 CLib::ConvertJobTypeToPattern 에서 피킹은 출고와 같은
                    //   JOB_PATTERN_RET 로 묶여 있고, 앞의 IsJobExist 도 case 3 을
                    //   case 2 와 똑같이 채운다. 여기만 빠져 있어 피킹출고를
                    //   완료하면 다음 작업이 생기지 않아 순환이 거기서 멈췄다.
                    case 13:
                    case 3:
                    case 12:
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
                        // @.뒤따르는 작업은 바탕이 된 작업과 같은 창고에 둔다.
                        if (modDefApp.g_frmForm.InsertJobMst(m_BDb, strLuggNum, strDPosition, "00-000-00", strSScNum, strLocation, nNewJobType.ToString()
                                                            , "", "000", "0", false, "0", modDefApp.GsWhTypOf(m_strJobWhDef)) == false)
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
                    // @.다루지 않는 작업구분이 오면 열어 둔 트랜잭션을 닫는다.
                    //   전에는 그대로 return 해 버려 트랜잭션이 떠 있었다.
                    default:
                        m_BDb.RollbackTrans();
                        break;
                }
                return true;
            }
            #endregion

            if (!m_blSockConnected)
            {
                bNoMore = true;
                return false;
            }
            
            #region 메세지 보내기 (비동기 : 큐에 넣고, ACK 를 받으면 CliWorkThread 에서 작업을 삭제한다. 2026-11-15)
            string strLuggForDel = strLuggNum;
            bool bDupC = false;
            if (!QueueSrv(delegate (bool bOkC)
            {
                if (!bOkC)
                    return;         // 응답이 없다. 작업이 남아 있으므로 다음에 다시 만든다.

                #region 응답 받고 작업 삭제
                bool bDel = modDefApp.g_frmForm.DeleteJobMst(m_BDb, true, strLuggForDel);     // 함수안에서 Transaction 처리함!
                if (bDel == false)
                {
                    m_strLog = string.Format("작업 삭제 실패하였습니다. [작업번호:{0}][실패내용:{1}]", strLuggForDel, modDefApp.GM_RTN_MSG);
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                    return;
                }

                m_strLog = string.Format("작업 삭제 성공하였습니다. [작업번호:{0}]", strLuggForDel);
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
                #endregion
            }, out bDupC))
            {
                return false;
            }
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
        // @.창고구분은 설비마다 다르다. 그 설비 행의 WH_TYP 에서 뽑아 넘긴다.
        //   P-BOX(WH_TYP 20) 는 'B', 팔레트(10) 는 'A' 다.
        private string GfMakeStatusBody(string strWhDef, int nDevClass, int nDevNo, int nStatus,
                                        int nLuggNum, int nSizeChk, int nJobExist)
        {
            return string.Format("S{0}{1:0}{2:000}{3:0}{4:0000}{5:0}{6:0}",
                                 strWhDef, nDevClass, nDevNo, nStatus,
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
        private bool m_bPmReqSent = false;     // @.공파렛트 입고 요구(N)를 이번 켜짐에 이미 보냈는가
        private void GetPmStoRequest()
        {
            string strTitle = "[GetPmStoRequest] .. ";

            if (!m_blSockConnected) return;

            m_BDb.ParamsClear();
            m_strSql = modDefApp.CRLF + "  SELECT HOST_STN_NO                                                 ";
            m_strSql += modDefApp.CRLF + "    FROM CV_DATA                                                    ";
            m_strSql += modDefApp.CRLF + "   WHERE WH_TYP        IN " + modDefApp.GsWhTypList();
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
            if (iCnt == 0) { m_bPmReqSent = false; return; }      // @.요구 조건이 아님. 다음에 켜질 때 다시 보낸다
            // @.원본은 입고대 준비 신호가 바뀌는 순간(SetStoStationReady)에만 한 번 보낸다. 켜져 있는 동안 계속 보내지 않는다.
            if (m_bPmReqSent) return;
            if (GetPmStoRequest(1, modDefApp.ECS_STN_POS_3F_PLT_207)) m_bPmReqSent = true;
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

            int nStationForLog = nStation;
            bool bDupN = false;
            if (!QueueSrv(delegate (bool bOkN)
            {
                if (!bOkN)
                {
                    m_bPmReqSent = false;       // 응답이 없다. 다음에 다시 보낸다.
                    return;
                }
                m_strLog = string.Format("공파렛트 입고 요구.. 스테이션=[{0}]", nStationForLog);
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
            }, out bDupN)) return false;
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
         *   ※ 포크와 작업대의 짝  (원본 EcsSv 를 확인한 결과)
         *
         *        Fork1 = 222 (트랙 420, 안쪽)
         *        Fork2 = 221 (트랙 419, 바깥)
         *
         *      원본 CCv::CheckBoxStoArrived 의 머리말이 이렇게 적어 두었다.
         *        "Twinfork 입고 대기대 #2(안쪽, Fork#1)에 화물이 도착했을 때..."
         *      그리고 같은 함수가 222 의 화물을 First 로 읽어 RequestBoxStore 의
         *      첫 인자(nFork1LuggNum)로 넘긴다. 두 군데가 같은 말을 한다.
         *
         *      전문에 싣는 차례는 그와 별개다. 원본은 첫 자리에 nFork2LuggNum 을
         *      싣고 221 의 미실행 플래그와 짝지운다. 즉
         *        전문 첫 자리 = 221 의 작업번호,  둘째 자리 = 222 의 작업번호
         *      이다. 우리도 그대로 보내고 있으므로 전문 내용은 원본과 같다.
         *
         *      다만 이름을 거꾸로 달아 두어서, 로그가
         *        "Fork1=[0] Fork2=[1077]"  (221 이 비고 222 에 1077)
         *      로 찍혀 마치 바깥 포크에 실린 것처럼 읽혔다. 실제로는 안쪽(Fork1)이다.
         *      이름만 바로잡는다. 보내는 바이트는 달라지지 않는다.
         *
         *   대상 작업대(221 / 222)는 CV_DATA.HOST_STN_NO 로 찾는다.
         */
        /*
         * GetBoxStoRequest :: 언제 요구를 낼지 고른다. (원본 CCv::CheckBoxStoArrived)
         *
         *   대기대가 둘이고 한 크레인이 두 포크로 집는다.
         *     Fork1 = 222 (트랙 420, 안쪽)
         *     Fork2 = 221 (트랙 419, 바깥)
         *
         *   안쪽부터 채운다. 그래서 원본은 이렇게 가른다.
         *
         *     222   221   하는 일
         *     ----  ----  ------------------------------------------------
         *      X     X    아무것도 안 한다. 기다린 시각을 지운다.
         *      O     X    30초 기다렸다가 Fork1 만 요구한다.
         *      X     O    아무것도 안 한다.
         *      O     O    아직 안 받은 쪽만 골라 30초 뒤 요구한다.
         *
         *   바깥(221)에만 화물이 선 경우를 비워 둔 것은 원본 그대로다.
         *   그 자리에 혼자 섰다는 것은 안쪽으로 갈 화물이 아직 오는 중이라는
         *   뜻이므로, 서둘러 요구하면 바깥 포크(Fork2)로 작업이 잡힌다.
         *   (원본에도 그 분기가 주석으로 막혀 있다)
         *
         *   30초를 두는 것은, 짝이 뒤따라 올 수 있으니 한 번에 둘을 집게
         *   하려는 것이다. 그 안에 짝이 서면 두 개를 함께 요구한다.
         *
         *   목적지가 이미 크레인 H/S(11) 면 작업을 받은 것이므로 건너뛴다.
         */
        private const int DEF_BOX_HS_SC  = 11;    // 크레인 H/S. 이미 작업을 받은 상태
        private const int DEF_BOX_WAIT_S = 30;    // 짝을 기다리는 시간(초)

        private DateTime m_dtBoxRequested = DateTime.MinValue;

        /*
         * 방금 읽은 작업·설비가 어느 창고인지.
         *
         *   전문의 창고구분은 설비마다 작업마다 다르다. 그런데 IsJobExist /
         *   IsEquip_ERROR_Modified 는 ref 인자가 이미 많아 하나를 더 끼우면
         *   부르는 쪽을 다 고쳐야 한다. 읽을 때 여기 적어 두고 전문 만들 때 쓴다.
         *   (두 함수와 그것을 쓰는 전문 만들기는 같은 스레드에서 잇달아 돈다)
         */
        private string m_strJobWhDef = modDefApp.WH_DEF;   // 작업의 창고
        private string m_strEqpWhDef = modDefApp.WH_DEF;   // 설비의 창고
        private int m_nEqpPlcId = 0;                        // 에러 보고 끝에 붙는 PLC ID (크레인 0, 컨베이어는 PLC 번호)

        /*
         * P-BOX 입고 요구 전문에 미실행 유무를 붙일지.
         *
         *   원본 CHostCl::RequestBoxStore 는 작업번호 뒤마다 한 자리를 더 붙인다.
         *     STX L 작업번호#1(4) 미실행#1(1) 작업번호#2(4) 미실행#2(1) ETX   = 11자
         *   문서 [09]Interface목록서 IV.9 에는 그 항목이 없다.
         *     STX L 작업번호#1(4) 작업번호#2(4) ETX                            =  9자
         *
         *   현장 상위가 어느 쪽을 읽는지 몰라 ECSCOMA.ini 에서 고르게 한다.
         *   기본값은 1(붙인다) 이다. 현장에서 도는 것이 원본이기 때문이다.
         *
         *     [Property]
         *     BOX_STO_REQ_FLAG=1     ; 1=미실행 유무 포함(11자), 0=제외(9자)
         *
         *   미실행 유무는 그 출발 작업대에 아직 지시되지 않은 작업이 있는지다.
         *   받는 쪽이 그 자리에서 작업을 또 내면 거절되므로(중복 출발지), 미리
         *   알려 주는 값이다.
         */
        private int m_nBoxStoReqFlag = -1;      // -1 = 아직 안 읽음

        // @.입고 / 이동 / 랙투랙 완료 보고의 완료차수 (2026-11-23)
        //     [Property]
        //     COMPLETE_STEP_FINAL=0     ; 0(기본) = 완료차수 1, 1 = 완료차수 2 (기존 ECS 와 같음)
        //   한 번만 읽는다. 바꾸면 HOST 태스크를 다시 켠다.
        private int m_nCompleteStepFinal = -1;  // -1 = 아직 안 읽음
        private bool GfIsCompleteStepFinal()
        {
            if (m_nCompleteStepFinal < 0)
                m_nCompleteStepFinal = modDefAPI.GetPrivateProfileInt("Property", "COMPLETE_STEP_FINAL", 0, modDefApp.MAIN_INI);

            return (m_nCompleteStepFinal != 0);
        }

        // @@.INI 를 한 번만 읽는다. 전문을 낼 때마다 파일을 열 까닭이 없다.
        private bool GfIsBoxStoReqFlagUsed()
        {
            if (m_nBoxStoReqFlag < 0)
                m_nBoxStoReqFlag = modDefAPI.GetPrivateProfileInt("Property", "BOX_STO_REQ_FLAG", 1, modDefApp.MAIN_INI);

            return (m_nBoxStoReqFlag != 0);
        }

        private void GetBoxStoRequest()
        {
            if (!m_blSockConnected) return;

            // @.Fork1 = 222(안쪽), Fork2 = 221(바깥). 원본 EcsSv 를 따른다.
            int nLugg1 = 0, nDest1 = 0; bool bReady1 = false;
            int nLugg2 = 0, nDest2 = 0; bool bReady2 = false;

            GfGetBoxStnInfo(modDefApp.ECS_STN_POS_3F_BOX_222, ref nLugg1, ref nDest1, ref bReady1);
            GfGetBoxStnInfo(modDefApp.ECS_STN_POS_3F_BOX_221, ref nLugg2, ref nDest2, ref bReady2);

            // @.둘 다 준비되지 않았다. 기다린 시각을 지우고 돌아간다.
            if (!bReady1 && !bReady2)
            {
                m_dtBoxRequested = DateTime.MinValue;
                return;
            }

            // @.바깥(221)에만 섰다. 안쪽으로 갈 화물이 아직 오는 중이다. 기다린다.
            if (!bReady1 && bReady2)
                return;

            // @.안쪽(222)만 섰다.
            if (bReady1 && !bReady2)
            {
                if (nDest1 == DEF_BOX_HS_SC) return;
                if (!GfIsBoxWaitOver()) return;

                GetBoxStoRequest(nLugg1, 0);
                return;
            }

            // @.둘 다 섰다. 아직 작업을 안 받은 쪽만 올린다.
            bool bTake1 = (nDest1 != DEF_BOX_HS_SC);
            bool bTake2 = (nDest2 != DEF_BOX_HS_SC);

            if (!bTake1 && !bTake2) return;
            if (!GfIsBoxWaitOver()) return;

            GetBoxStoRequest(bTake1 ? nLugg1 : 0, bTake2 ? nLugg2 : 0);
        }

        // @@.마지막으로 요구한 지 DEF_BOX_WAIT_S 초가 지났는가.
        //    한 번도 안 냈으면 바로 낼 수 있다고 본다.
        private bool GfIsBoxWaitOver()
        {
            if (m_dtBoxRequested == DateTime.MinValue) return true;

            return ((DateTime.Now - m_dtBoxRequested).TotalSeconds >= DEF_BOX_WAIT_S);
        }

        // @@.대기대 하나의 지금 상태. 적재물번호 / 목적지 / 출고대 준비 신호.
        private void GfGetBoxStnInfo(int nStation, ref int nLuggNo, ref int nDestPos, ref bool bRetReady)
        {
            nLuggNo = 0; nDestPos = 0; bRetReady = false;

            m_BDb.ParamsClear();
            m_strSql  = modDefApp.CRLF + "  SELECT COALESCE(LUGG_NO_RD,'0')   AS LUGG_NO_RD   ";
            m_strSql += modDefApp.CRLF + "       , COALESCE(DEST_POS_RD,'0')  AS DEST_POS_RD  ";
            m_strSql += modDefApp.CRLF + "       , COALESCE(RET_READY_RD,'0') AS RET_READY_RD ";
            m_strSql += modDefApp.CRLF + "    FROM CV_DATA                                    ";
            m_strSql += modDefApp.CRLF + "   WHERE WH_TYP      IN " + modDefApp.GsWhTypList();
            m_strSql += modDefApp.CRLF + "     AND HOST_STN_NO = " + m_BDb.ParamsAdd("HOST_STN_NO", nStation.ToString());

            int iCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
            if (iCnt <= 0) return;

            int.TryParse(m_BDb.dtMain.Rows[0]["LUGG_NO_RD"].ToString().Trim(),  out nLuggNo);
            int.TryParse(m_BDb.dtMain.Rows[0]["DEST_POS_RD"].ToString().Trim(), out nDestPos);

            bRetReady = (m_BDb.dtMain.Rows[0]["RET_READY_RD"].ToString().Trim() == "1");
        }
        private bool GetBoxStoRequest(int nFork1LuggNum, int nFork2LuggNum)
        {
            string strTitle = "[GetBoxStoRequest] .. ";
            m_strHostCmd = "L";

            if (!m_blSockConnected) return false;

            /*
             * 전문 자리차례는 원본 CHostCl::RequestBoxStore 를 그대로 따른다.
             *
             *   첫 자리 = 221(자동입고대기 #1, 바깥, Fork2)
             *   둘째 자리 = 222(자동입고대기 #2, 안쪽, Fork1)
             *
             * 이름(Fork1/Fork2)과 싣는 차례가 서로 반대인 것은 원본이 그렇다.
             * 이름을 바로잡아도 나가는 바이트는 같다.
             */
            string strTemp;

            if (GfIsBoxStoReqFlagUsed())
            {
                // @.원본 형식(11자). 작업번호 뒤마다 그 작업대의 미실행 유무 한 자리.
                int nFlag221 = GfHasUnExcutedJob(modDefApp.ECS_STN_POS_3F_BOX_221) ? 1 : 0;
                int nFlag222 = GfHasUnExcutedJob(modDefApp.ECS_STN_POS_3F_BOX_222) ? 1 : 0;

                strTemp = string.Format("L{0:0000}{1:0}{2:0000}{3:0}",
                    nFork2LuggNum, nFlag221, nFork1LuggNum, nFlag222);
            }
            else
            {
                // @.문서 IV.9 형식(9자). 미실행 유무를 뺀다.
                strTemp = string.Format("L{0:0000}{1:0000}", nFork2LuggNum, nFork1LuggNum);
            }

            int iTxCnt = modDefApp.MSG_HEAD_CNT + strTemp.Length + 2;
            m_bytTxBuff = new byte[iTxCnt];

            MakeHeader(strTemp.Length);

            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;
            byte[] bytTempByte = System.Text.Encoding.Default.GetBytes(strTemp);
            Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, strTemp.Length);
            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            int nF1 = nFork1LuggNum, nF2 = nFork2LuggNum;
            string strTempForLog = strTemp;
            bool bDupL = false;
            if (!QueueSrv(delegate (bool bOkL)
            {
                if (!bOkL) return;      // 응답이 없다. 낸 시각을 적지 않아 다음에 다시 낸다.

                // @.낸 시각을 적어 둔다. 다음 요구는 DEF_BOX_WAIT_S 초 뒤에 나간다.
                m_dtBoxRequested = DateTime.Now;

                m_strLog = string.Format("P-BOX 입고 요구.. Fork1(222)=[{0}] Fork2(221)=[{1}] 전문=[{2}]",
                    nF1, nF2, strTempForLog);
                modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_NOR);
            }, out bDupL)) return false;
            return true;
        }

        // @@.해당 작업대에 올라와 있는 적재물번호. 없으면 0.
        private int GfGetStnLuggNo(int nStation)
        {
            m_BDb.ParamsClear();
            m_strSql = modDefApp.CRLF + "  SELECT COALESCE(LUGG_NO_RD,'0') AS LUGG_NO_RD ";
            m_strSql += modDefApp.CRLF + "    FROM CV_DATA                                ";
            m_strSql += modDefApp.CRLF + "   WHERE WH_TYP      IN " + modDefApp.GsWhTypList();
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
            m_strSql += modDefApp.CRLF + "   WHERE WH_TYP    IN " + modDefApp.GsWhTypList();
            m_strSql += modDefApp.CRLF + "     AND START_POS = " + m_BDb.ParamsAdd("START_POS", nStation.ToString());
            // @.신규(아직 지시 전)만 센다. 원본 GetUnExcutedJob 이 JOB_STA_NEW /
            //   JOB_STA_CV_NEW 만 보는 것과 같다.
            //
            //   예전에는 EN_JOB_STATUS.enJobStatusComplete(=2) 보다 작은가를 봤다.
            //   그 열거형은 전문에 싣는 번호지 JOB_MST.JOB_STATUS 의 값이 아니다.
            //   DB 는 신규를 '99' 로 적으므로 이 조건은 한 건도 세지 못했다.
            //   (CSrvWork 의 중복 출발지 검사도 '99' 로 본다. 같은 뜻이어야 한다)
            m_strSql += modDefApp.CRLF + "     AND JOB_STATUS  = '99'                          ";

            // @.원본은 입고와 작업대이동만 센다.(JOB_PATTERN_STO / JOB_PATTERN_SITE)
            m_strSql += modDefApp.CRLF + "     AND COALESCE(NULLIF(TRIM(JOB_TYP),''),'0')::INTEGER IN (1, 11, 6, 10) ";

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
            for (int n = 0; n < DEF_REPORT_QUEUE_MAX; n++)
            {
                bool bNoMore = false;
                bool bOk = GetLoadArrivalReportOne(nJobStatus, ref bNoMore);
                if (bNoMore || !bOk) break;
            }
            return true;
        }

        private bool GetLoadArrivalReportOne(int nJobStatus, ref bool bNoMore)
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
                bNoMore = true;
                return false;
            }
            if (nResult < 0)
            {
                bNoMore = true;
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

            if (nJobType != 5)
            {
                if (!m_blSockConnected)
                {
                    return false;
                }
            }

            #region 보고를 못했어도 그냥 지나감! - 다시 보고안하기 위해서 먼저 업데이트 하고 넘어감
            m_BDb.BeginTrans();

            //### manual_temp 갱신
            //### Status UPDATE
            m_BDb.ParamsClear();

            if (nJobType != 5)
            {
                m_strSql = modDefApp.CRLF + "  UPDATE JOB_MST ";
                m_strSql += modDefApp.CRLF + "    SET JOB_STATUS   = " + m_BDb.ParamsAdd("JOB_STATUS", nJobStatus + 6);      // 도착 보고에서 도착보고완료로 수정 
                m_strSql += modDefApp.CRLF + "      , UPD_USER_ID  = 'HOST_TASK'";
                m_strSql += modDefApp.CRLF + "      , UPD_DT       = " + modDateTime.SYSDATE;
                m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       IN " + modDefApp.GsWhTypList();
                m_strSql += modDefApp.CRLF + "    AND LUGG_NO      = " + m_BDb.ParamsAdd("LUGG_NO", strLuggNum);
            }
            //if (nJobType == 5)
            //{
            //    m_strSql = modDefApp.CRLF + "  UPDATE JOB_MST ";
            //    m_strSql += modDefApp.CRLF + "    SET MES_WRITE_YN = " + m_BDb.ParamsAdd("MES_WRITE_YN", "Y");      // 도착 보고에서 도착보고완료로 수정 
            //    m_strSql += modDefApp.CRLF + "      , UPD_USER_ID  = 'HOST_TASK'";
            //    m_strSql += modDefApp.CRLF + "      , UPD_DT       = " + modDateTime.SYSDATE;
            //    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP       IN " + modDefApp.GsWhTypList();
            //    m_strSql += modDefApp.CRLF + "    AND LUGG_NO      = " + m_BDb.ParamsAdd("LUGG_NO", strLuggNum);
            //}

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
                // @.입고 - 반자동(11)도 같이 받는다
                case 11:
                case 1: nStation = Convert.ToInt16(strSPosition); nClass = 1; break;
                // @.출고 - 반자동(12)도 같이 받는다
                case 12:
                case 2: nStation = Convert.ToInt16(strDPosition); nClass = 2; break;
                // @.피킹출고 - 반자동(13)도 같이 받는다
                case 13:
                case 3: nStation = Convert.ToInt16(strDPosition); nClass = 2; break; // @.피킹은 출고와 동일
                // @.랙투랙 - 반자동(14)도 같이 받는다
                case 14:
                case 4: nStation = 0;                             nClass = 3; break;
                // @.호기간이동 - 반자동(15)도 같이 받는다
                case 15:
                case 5: nStation = 0;                             nClass = 3; break;
                // @.이동 - 반자동(10)도 같이 받는다
                case 10:
                case 6: nStation = Convert.ToInt32(strDPosition); nClass = 3; break;
                default:
                    m_strLog = "작업정보는 존재하지만 잘못된 작업 정보입니다.[작업 타입:" + nJobType.ToString() + "]";
                    modCmWork.ShowMsgClient(strTitle + m_strLog, modDefApp.MSG_ERR);
                    return false;
            }

            strTemp = string.Format("F{0:0}{1}{2:0000}{3:0}{4:0}{5:000}",
                                    modDefApp.GsJobDefineOfReport(nJobType), m_strJobWhDef, nLuggNum, nClass,
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


            #region 메세지 보내기 (비동기 : 큐에 넣는다)
            bool bDupA = false;
            if (!QueueSrv(null, out bDupA))
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


        /*
         * HeartBeatEqpMst :: 살아 있다고 EQP_MST 에 적는다.
         *
         *   관제 Client 가 통신 램프를 이렇게 가린다.
         *     CASE WHEN TO_CHAR(NOW() - UPD_DT, 'YYYYMMDDHH24MISS')::INTEGER > 120
         *          THEN 121 ELSE 0 END   (Ecs/ConnectStatus.cpp)
         *   UPD_DT 가 오래되면 빨강이다. CONNECTED_YN 이 'Y' 여도 소용없다.
         *
         *   C/V·S/C 태스크는 폴링을 돌 때마다 Communication() 으로 이 시각을
         *   갱신한다. 그런데 HOST 는 붙을 때와 끊길 때 한 번씩만 썼다.
         *   그래서 붙어 있는데도 1~2분이면 램프가 빨강으로 떨어졌다.
         *
         *   너무 자주 쓸 까닭은 없다. 판정이 분 단위라 10초면 넉넉하다.
         */
        private DateTime m_dtHeartBeat = DateTime.MinValue;
        //   간격은 Client 의 판정보다 넉넉히 짧아야 한다.
        //   Client 는 UPD_DT 가 5초보다 오래면 빨강으로 본다.
        //     Ecs/EcsDoc.cpp GetHostConnectInfo
        //       EXTRACT(EPOCH FROM NOW() - EM.UPD_DT) > 5 THEN 6 ELSE 0
        //   10초로 두면 0~5초 사이에는 파랑, 6~10초 사이에는 빨강이 되어
        //   10초 주기로 램프가 깜빡인다. 그래서 2초로 줄였다.
        //   C/V·S/C 태스크는 폴링마다(1초 안쪽) 갱신하므로 이 쪽이 그 결과 맞다.
        private const int DEF_HEARTBEAT_S = 2;

        public void HeartBeatEqpMst()
        {
            if (!m_blDbConnted) return;
            if ((DateTime.Now - m_dtHeartBeat).TotalSeconds < DEF_HEARTBEAT_S) return;

            m_dtHeartBeat = DateTime.Now;

            string strErrMsg = "";
            string strYn = m_blSockConnected ? "Y" : "N";
            UpdateEQP_MST(ref m_BDb, "10", "HOST", "01", strYn, ref strErrMsg);
        }

        private bool UpdateEQP_MST(ref CUserDb p_Bdb, 
                                   string p_strWH_TYP,
                                   string p_strEQP_TYP,
                                   string p_strPLC_NO,
                                   string p_strCONNECTED_YN,
                                   ref string p_strRtnMsg)
        {
            try
            {
                m_strSql = "";
                m_strSql += modDefApp.CRLF + "     UPDATE EQP_MST           ";
                m_strSql += modDefApp.CRLF + "        SET CONNECTED_YN  = '" + p_strCONNECTED_YN + "'   ";
                m_strSql += modDefApp.CRLF + "      , UPD_DT            = " + modDateTime.SYSDATE;
                m_strSql += modDefApp.CRLF + "  WHERE WH_TYP            = '" + p_strWH_TYP + "'         ";
                m_strSql += modDefApp.CRLF + "    AND EQP_TYP           = '" + p_strEQP_TYP + "'        ";
                m_strSql += modDefApp.CRLF + "    AND PLC_NO            = '" + p_strPLC_NO + "'         ";

                m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                if (m_iSelCnt != 1)
                {
                    throw new Exception("EQP MST의 CONNECTED_YN을 수정하는데 실패하였습니다." + p_Bdb.ErrMsg);
                }

                return true;

            }
            catch (Exception ex)
            {
                p_strRtnMsg = ex.Message;
                return false;
            }

        }

	}
}
