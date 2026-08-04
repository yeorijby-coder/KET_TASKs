using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Microsoft.VisualBasic;
using Npgsql;

namespace TSK_HostCom
{
    class CSrvWork
	{
		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//화면개요   : ECS 서버 소켓용 작업 클래스
		//수정이력   : 01. 20161122 이길문 작업취소정보 수신 주석해제
		//             02. 20161122 이길문 WMS에서 먼저 작업취소처리 했을 경우 체크 추가
		//             03. 20161122 이길문 이중입고일 경우 로케이션 재지정
		//             04. 20161228 이길문 ECS상태정보수신작업대Get
		//             05. 20170105 이길문 설비지시일시추가
		//             06. 20170105 이길문 설비완료일시추가
		//             07. 20170105 이길문 응답메세지총길이변경:27->47
		//             08. 20170105 이길문 응답메세지 바코드정보추가
		//             09. 20170105 이길문 응답메세지 작업차수추가
		//             10. 20170105 이길문 작업완료보고수신 응답메세지 작업차수추가(strStep)
		//             11. 20170105 이길문 바코드보고수신 응답메세지 바코드추가(strReadingData)

		public Socket m_sktSock;//소켓
		public bool m_blSockConnected;//소켓접속여부
		public System.Threading.Thread m_thrThreadObj;//쓰레드 객체	
		
		public bool m_blDbConnted;//DB 연결 유무
		public CUserDb m_BDb = new CUserDb("Multi", false);//쓰레드별 Connection 별도
		
		private byte[] m_bytRxHead = new byte[modDefApp.MSG_HEAD_CNT + 1];	//Header
		private byte[] m_bytRxBuff = new byte[1025];//프로젝트 별로 최대 허용되는 m_bytRxBuff 설정	
		private byte[] m_bytTxBuff;//클라이언트로 보낼 메세지	
		
		private string m_strLog = "";	//로그
		private string m_strSql;	//SQL 문장
		private int m_iSelCnt;	//처리 건수 반환

		private string m_strMsgType;	//메세지 구분
		private string m_strMsgType2;

        //Command
        private string m_strHostCmd;
        //Direction
        private string m_strDirection = "W2E"; // 해당 클래스에서는 이방향으로 보냄!

        //설비상태가 연속으로 많은량 수신 될 때 SKIP을 위한 처리
		private string[] m_strScStatus = new string[modDefApp.SC_CNT];
		private string[] m_strScLuggNo1 = new string[modDefApp.SC_CNT];
		private string[] m_strScLuggNo2 = new string[modDefApp.SC_CNT];
		private string m_strCvStatus;
		private string[] m_strScWhTyp = new string[modDefApp.SC_CNT];
		private string[] m_strScNo = new string[modDefApp.SC_CNT];
		public System.DateTime m_tmSCMD_RecvTime;
				
		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 서버클래스 생성시 초기값 할당
		public CSrvWork()
		{
			m_blSockConnected = false;
			m_blDbConnted = false;
		}

        /// <summary>
        /// 현재 미사용 
        /// </summary>
        /// <returns> -1 리턴 : db객체에 이상이 있음, 0리턴 리턴 로우가 없음, 1리턴 업데이트 정상진행함</returns>
        public int UpdateConnectionState_EQP_MST_STATE()
        {
            try
            {
                int nRtn = -1;

                m_BDb.BeginTrans();
                m_BDb.ParamsClear();

                m_strSql = "";
                m_strSql += modDefApp.CRLF + " UPDATE EQP_MST ";
                m_strSql += modDefApp.CRLF + "    SET UPD_DT = " + modDateTime.SYSDATE;
                m_strSql += modDefApp.CRLF + "  WHERE WH_TYP = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                m_strSql += modDefApp.CRLF + "    AND EQP_TYP = " + m_BDb.ParamsAdd("EQP_TYP", "HOST2");
                nRtn = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                if (nRtn < 0)
                {
                    m_BDb.RollbackTrans();
                    return -1;
                }

                if (nRtn == 0)
                {
                    m_BDb.CommitTrans();
                    return 0;
                }

                nRtn = 1;
                m_BDb.CommitTrans();
            }
            catch(Exception exAll)
            {
                m_BDb.RollbackTrans();
                return -1;
            }

            return 1;
        }

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: ECS수신정보Read
		public bool ReadRequest(ref int p_iBodyLen)
		{
			string strLog = null;
			int iRxCnt = 1;
			//return false:Close socket, true:Continue

			ClearBuff(ref m_bytRxHead);
			// Close시 Return 0 or Exception 발생, Receive전 Close되었느냐 Receive대기상태일 때 Close되었는냐 등에 따라 다름
			iRxCnt = m_sktSock.Receive(m_bytRxHead, modDefApp.MSG_HEAD_CNT, SocketFlags.None);

            if (iRxCnt <= 0)
                return false;

            // Header 체크

            if (!CheckHeader(iRxCnt, ref p_iBodyLen))
			{
				// 잘못된 헤더 로그
				strLog = System.Text.Encoding.UTF8.GetString(m_bytRxHead);
				modCmWork.ShowMsgServer(strLog, modDefApp.MSG_ERR);

				// NAK 응답
				SendSock();
				return false;
			}

            // Body 체크

            iRxCnt = m_sktSock.Available;
			iRxCnt = m_sktSock.Receive(m_bytRxBuff,modDefApp.MSG_HEAD_CNT, p_iBodyLen, SocketFlags.None);
			if (!CheckBody(p_iBodyLen, iRxCnt))
			{
				// 잘못된 바디 로그
				strLog = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, 0, modDefApp.MSG_HEAD_CNT + p_iBodyLen);
				modCmWork.ShowMsgServer(strLog, modDefApp.MSG_ERR);

				// NAK 응답
				SendSock();
				return false;
			}

            ////eqp_mst 업데이트
            //if (UpdateConnectionState_EQP_MST_STATE().Equals(-1))
            //{
            //    return false;
            //}

			return true;
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 해더메세지 길이체크
		private bool CheckHeader(int p_iRxCnt, ref int p_iBodyLen)
		{
			string strTemp = null;

			m_strMsgType = " ";
			try
			{
				if (p_iRxCnt != modDefApp.MSG_HEAD_CNT)
				{
					ClearSock();
					MakeResponse("X", "", modDefApp.MSG_INVALID_HEADER_LENG);

					m_strLog = string.Format("정해진 헤더의 길이가 아닙니다.[Leng={0}]", p_iRxCnt);
					modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
					return false;
				}

				strTemp = System.Text.Encoding.UTF8.GetString(m_bytRxHead, 10, 4);

				p_iBodyLen = Convert.ToInt32(strTemp);
				if (p_iBodyLen < 3)
				{
					ClearSock();
					MakeResponse("X", "", modDefApp.MSG_INVALID_HEADER_LENG);

					m_strLog = string.Format("Body의 길이가 '3' 이하 입니다.[Leng={0}]", p_iBodyLen);
					modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
					return false;
				}
			}
			catch (Exception ex)
			{
				ClearSock();
				MakeResponse("X", "", modDefApp.MSG_INVALID_HEADER_LENG);

				m_strLog = "헤더의 정보가 틀립니다.";
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
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
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				throw ex;
			}

		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 소켓 클리어


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
					break; // TODO: 원본은 Exit For 이었음. 동작이 다를 수 있으니 확인 필요
				}
				//Debug
				//Console.WriteLine("Read fail. 회수[{0}]:ReadCnt[{1}]", i, nRxCnt)
				System.Threading.Thread.Sleep(500);
				p_iRxCnt = m_sktSock.Receive(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + p_iBodyLen - iRemain, iRemain, SocketFlags.None);
				iRemain -= p_iRxCnt;
			}

			if (iRemain != 0)
			{
				ClearSock();
				MakeResponse("X", "", modDefApp.MSG_INVALID_MSG_LENG);

				m_strLog = string.Format("정해진 메세지 길이만큼 읽지 못했습니다..[Leng={0}]", p_iBodyLen - iRemain);
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				return false;
			}

			if (m_bytRxBuff[modDefApp.MSG_HEAD_CNT] != modDefApp.STX)
			{
				ClearSock();
				MakeResponse("X", "", modDefApp.MSG_NO_STX);

				m_strLog = "메세지의 시작이 'STX'가 아닙니다.";
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				return false;
			}

			if (m_bytRxBuff[modDefApp.MSG_HEAD_CNT + p_iBodyLen - 1] != modDefApp.ETX)
			{
				ClearSock();
				MakeResponse("X", "", modDefApp.MSG_NO_ETX);

				m_strLog = "메세지의 끝이 ETX가 아닙니다.";
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				return false;
			}

			return true;
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 응답메세지(두 작업번호일 경우)
		private void MakeResponse(string p_strMsgType, string p_LuggNo, byte p_bytReasonCode1)
		{
			string strTemp = null;
			byte[] bytTempByte = null;
			int iTxCnt = 0;
			string strAckNak1 = null;
			int iRspsLen = 0;

			// @.문서 III.A 응답 메시지
			//   STX + 타입(소문자) + ACK/NAK + Reason(2) + 작업번호(4) + 장비번호(3) + ETX
			//   = 12바이트. 예전에는 장비번호 3자리가 빠져 10바이트로 보냈다.
			iRspsLen = 13;
			iTxCnt = modDefApp.MSG_HEAD_CNT + iRspsLen;

			m_bytTxBuff = new byte[iTxCnt];

			//### Header ###
			strTemp = string.Format("ECS_MBX   {0:0000} ", iRspsLen);
			bytTempByte = System.Text.Encoding.UTF8.GetBytes(strTemp);

			Array.Copy(bytTempByte, 0, m_bytTxBuff, 0, modDefApp.MSG_HEAD_CNT);

			//### Body ###
			if (p_bytReasonCode1 == modDefApp.MSG_NO_ERROR)
			{
				strAckNak1 = modDefApp.MSG_ACK;
			}
			else
			{
				strAckNak1 = modDefApp.MSG_NAK;
			}

			m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;

			strTemp = string.Format("{0,1}{1,1}{2:00}", p_strMsgType.ToLower(), strAckNak1, p_bytReasonCode1); // ACK/NAK #1, RIMSon Code #1
			strTemp += p_LuggNo.PadLeft(4, char.Parse("0")); //Luggage No #1.
			strTemp += "000";                                //Device No. (해당 없음)
			
			bytTempByte = System.Text.Encoding.UTF8.GetBytes(strTemp);
			Array.Copy(bytTempByte, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, iTxCnt - modDefApp.MSG_HEAD_CNT - 2);

			m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 소켓전송
		public void SendSock()
		{

            m_sktSock.Send(m_bytTxBuff, SocketFlags.None);
            m_strLog = System.Text.Encoding.UTF8.GetString(m_bytTxBuff);
            modCmWork.ShowMsgServer(m_strLog);

            bool bResult = modDefApp.g_frmForm.InsertHostIfLog(m_BDb, m_strLog, m_strHostCmd, m_strDirection);     // 함수안에서 Transaction 처리함!

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
		//설명		: 메세지 분석 후 DB 작업
		public bool Parsing(int p_iBodyCnt)
		{

			m_strLog = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, 0, modDefApp.MSG_HEAD_CNT + p_iBodyCnt);
			modCmWork.ShowMsgServer(m_strLog);
			m_strHostCmd = m_strMsgType = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 1, 1);

            // IMS 메시지 LOG 추가
            modDefApp.g_frmForm.InsertHostIfLog(m_BDb, m_strLog, m_strMsgType, "W2E");

            switch (m_strMsgType)
            {
                #region 기존 WMS의 통신 TASK 일때 메세지들 

                //case "A":
                //    // ECS Job Arrived H/S(송수신 테스트 완료)
                //    ParseArrived(p_iBodyCnt);
                //    break;
                //case "D":
                //    // ECS Job Cancel(송수신 테스트 완료)
                //    ParseCancel(p_iBodyCnt);
                //    break;
                //case "S":
                //    // ECS Status(송수신 테스트 완료)
                //    ParseStatus(p_iBodyCnt);
                //    break;
                //case "E":
                //    // Error Reports(송수신 테스트 완료)
                //    ParseError(p_iBodyCnt);
                //    break;
                //case "F":
                //    // Job Complete(송수신 테스트 완료)
                //    ParseComplete(p_iBodyCnt);
                //    break;
                //case "P":
                //    // Pallet Call Report(송수신 테스트 완료)
                //    ParsePallet(p_iBodyCnt);
                //    break;
                //case "B":
                //    // 바코드정보 수신(미사용)
                //    ParseBcrData(p_iBodyCnt);
                //    break;
                //case "U":
                //    // ECS Throughput Report(미사용)
                //    ParseThroughput(p_iBodyCnt);
                //    break;
                #endregion 
                case "O":
                case "R":
                    // 작업지시 메세지 
                    ParseOorR(p_iBodyCnt);
                    break;
                case "D":
                    // 작업삭제 메세지 
                    ParseCancel(p_iBodyCnt);
                    break;
                default:
					m_strLog = string.Format("알수 없는 MsgType입니다.[{0}]", m_strMsgType);
					modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                    m_strHostCmd = "X";
					MakeResponse("X", "", modDefApp.MSG_UNKNOWN_MSG_TYPE);
					break;
			}
            DataTable_Dispose();

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

        /*
         * GfRowValue :: 조회 결과에서 컬럼 값을 꺼낸다. 컬럼이 없으면 기본값.
         *
         *   SC_PLT_JOB_TYP(파렛트 종류 구분) 같은 항목은 다른 현장 스키마에는
         *   있지만 이 현장 CV_DATA 에는 없다. 예전에는 작업대 검증이 그 앞에서
         *   항상 실패해 이 줄까지 오지 않아 드러나지 않았다.
         */
        private string GfRowValue(int p_nRow, string p_strCol, string p_strDefault)
        {
            if (m_BDb.dtMain == null) return p_strDefault;
            if (!m_BDb.dtMain.Columns.Contains(p_strCol)) return p_strDefault;
            if (p_nRow < 0 || p_nRow >= m_BDb.dtMain.Rows.Count) return p_strDefault;

            string strValue = m_BDb.dtMain.Rows[p_nRow][p_strCol].ToString().Trim();
            return (strValue.Length == 0) ? p_strDefault : strValue;
        }

        #region 작업대에 대한 Validation
        private bool IsValidStation(int nJobPattern, string strStation, bool bStart, ref string strCV_PLT_JOB_TYP)
        {
            m_BDb.ParamsClear();

            m_strSql = "SELECT * FROM CV_DATA";
            m_strSql += modDefApp.CRLF + "WHERE MC_NO = " + m_BDb.ParamsAdd("MC_NO", strStation);
            m_strSql += modDefApp.CRLF + "  AND WH_TYP = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);

            m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (m_iSelCnt <= 0)
            {
                return false;
            }

            for (int ii = 0; ii < m_iSelCnt; ii++)
            {
                string strKIND = "" + m_BDb.dtMain.Rows[ii]["STN_KIND"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[ii]["STN_KIND"].ToString();
                string strID = "" + m_BDb.dtMain.Rows[ii]["MC_NO"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[ii]["MC_NO"].ToString();

                int nKind = Convert.ToInt32(strKIND);

                 bool bValid = false;
                if (bStart == true)
                {
                    if (IsValidStartStation(nKind, strID, nJobPattern, strStation, ref bValid) == false)
                    {
                        continue;
                    }
                    else 
                    {
                        strCV_PLT_JOB_TYP = GfRowValue(ii, "SC_PLT_JOB_TYP", "0");
                        return bValid;
                    }
                }
                else
                {
                    //return IsValidDestStation(nKind, strID, nJobPattern);                   
                    if (IsValidDestStation(nKind, strID, nJobPattern, strStation, ref bValid) == false)
                    {
                        continue;
                    }
                    else
                    {
                        strCV_PLT_JOB_TYP = GfRowValue(ii, "SC_PLT_JOB_TYP", "0");
                        return bValid;
                    }
                }
            }

            return false;
        }
        #endregion
        #region 출발 작업대에 대한 Validation
        private bool IsValidStartStation(int nKIND, string strID, int nJobPattern, string strStationID, ref bool bValid)
        {
            bValid = false;
		    switch (nJobPattern)
		    {
		    // @.입고 / 이동은 입고대에서 출발한다.
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternSto:
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternMove:
			    if ((nKIND & modDefApp.STN_KIND_STO) == 0)
                {
				    return false;
                }
			    break;

		    // @.출고 / 호기간 이동은 크레인 H/S 에서 출발한다.
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternRet:
    //		case (int)modDefApp.EN_JOB_PATTERN.enJobPatternPR:
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternW2W:
			    if ((nKIND & modDefApp.STN_KIND_SC) == 0)
                {
				    return false;
                }
			    break;
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternR2R:
                bValid = true;
			    return true;
		    default:
			    //DEBUGER_ASSERT_VALID_LOG(FALSE, CConvert::ToString(nJobPattern));
				return false;
		    }

		    if (strID == strStationID)
            {
                bValid = true;
			    return true;
            }

            return true;
        }
        #endregion
        #region 도착 작업대에 대한 Validation
        private bool IsValidDestStation(int nKIND, string strID, int nJobPattern, string strStationID, ref bool bValid)
        {
            bValid = false;

		    switch (nJobPattern)
		    {
		    // @.입고 / 호기간 이동 / 랙간 이동은 크레인 H/S 로 도착한다.
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternSto:
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternW2W:
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternR2R:
                if ((nKIND & modDefApp.STN_KIND_SC) == 0)
                {
                    //continue;
                    return false;
                }
			    break;

		    // @.출고 / 이동은 출고대로 도착한다.
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternRet:
    //		case (int)modDefApp.EN_JOB_PATTERN.enJobPatternPR:
		    case (int)modDefApp.EN_JOB_PATTERN.enJobPatternMove:
                if ((nKIND & modDefApp.STN_KIND_RET) == 0)
                {
                    //continue;
                    return false;
                }
			    break;
	
		    default:
                //DEBUGER_ASSERT_VALID_LOG(FALSE, CConvert::ToString(nJobPattern));
                //continue;
                return false;
            }

		    if (strID == strStationID)
            {
                bValid = true;
			    return true;
            }

            return true;
        }
        #endregion

        /*
         * GfField :: 수신 버퍼에서 전문 항목 하나를 꺼낸다.
         *
         *   p_nOffset 은 STX 를 0 으로 볼 때의 자리이다.
         *   ([09]Interface목록서 의 Offset 은 헤더를 0 으로 보므로 15 를 뺀 값)
         *   짧게 들어온 전문에서도 예외가 나지 않도록 버퍼 범위를 넘지 않게 자른다.
         */
        private string GfField(int p_nOffset, int p_nSize)
        {
            int nStart = modDefApp.MSG_HEAD_CNT + p_nOffset;
            if (nStart >= m_bytRxBuff.Length) return "";

            int nSize = p_nSize;
            if (nStart + nSize > m_bytRxBuff.Length) nSize = m_bytRxBuff.Length - nStart;
            if (nSize <= 0) return "";

            return System.Text.Encoding.Default.GetString(m_bytRxBuff, nStart, nSize);
        }

        //최초작성자	: BASE(정복열)
        //작성일		: 20200508
        //설명		    : 상위로부터 작업지시 받기
        private void ParseOorR(int p_iBodyCnt)
        {
            //string strFunctionName = "ParseOorR .. ";
            string strTitle = "[ParseOorR] .. ";
            string strMsg = "";
            m_strMsgType = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 1, 1);
            // @.수신한 만큼만 읽는다. (예전에는 77바이트로 고정돼 있었다)
            int nMsgLen = modDefApp.MSG_HEAD_CNT + p_iBodyCnt;
            if (nMsgLen > m_bytRxBuff.Length) nMsgLen = m_bytRxBuff.Length;
            strMsg = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, 0, nMsgLen);//상위에서 내린 작업 메시지

            #region 수신 기본 데이터 파싱
            string strO_Priority = "";
            string strO_LotNo = "";
            string strO_Size = "";
            string strR_Kind = "";
            string strR_ScNum = "";

            /*
             * 문서 IV.1 작업지시 / IV.2 재작업지시
             *   STX 를 0 으로 볼 때의 자리
             *     +1  (1) Message Type
             *     +2  (1) Job Define
             *     +3  (1) WareHouse Define      <-- 예전 코드에 이 항목이 없어
             *     +4  (4) Luggage No.               뒤 항목이 전부 한 칸씩 밀려 있었다
             *   이후는 O 와 R 이 서로 다르다.
             *     O : +8 (4) Seq  +12(20) PalletNo  +32(3) SourceStn ...
             *     R : +8 (3) SourceStn ...
             */
            string strWhDef = GfField(1, 1);
            string strJob_Define = GfField(2, 1);
            string strLuggNo = GfField(4, 4);

            string strStartPos, strStartBank, strStartBay, strStartLevel;
            string strDestPos, strDestBank, strDestBay, strDestLevel;
            string strRouteStn = "";

            if (m_strMsgType == "O")
            {
                #region 작업지시(O) 자리
                strStartPos   = GfField(32, 3);
                strStartBank  = GfField(35, 2);
                strStartBay   = GfField(37, 3);
                strStartLevel = GfField(40, 2);
                strRouteStn   = GfField(42, 3);
                strDestPos    = GfField(45, 3);
                strDestBank   = GfField(48, 2);
                strDestBay    = GfField(50, 3);
                strDestLevel  = GfField(53, 2);
                #endregion
            }
            else
            {
                #region 재작업지시(R) 자리
                strStartPos   = GfField(8, 3);
                strStartBank  = GfField(11, 2);
                strStartBay   = GfField(13, 3);
                strStartLevel = GfField(16, 2);
                strDestPos    = GfField(18, 3);
                strDestBank   = GfField(21, 2);
                strDestBay    = GfField(23, 3);
                strDestLevel  = GfField(26, 2);
                #endregion
            }

            // @.창고구분은 지금 로직에서 쓰지 않지만 자리를 맞추기 위해 읽어 둔다.
            if (strWhDef.Length == 0) strWhDef = modDefApp.WH_DEF;
            if (strRouteStn.Length == 0) strRouteStn = "000";

            string strStartLoc = strStartBank + "-" + strStartBay + "-" + strStartLevel;
            string strDestLoc = strDestBank + "-" + strDestBay + "-" + strDestLevel;

            #region Convert S/C No
            // 크레인 일때 크레인 번호로 저장
            int nStartPos = Convert.ToInt32(strStartPos);
            if (nStartPos < 100 && nStartPos != 0)
                strStartPos = string.Format("9{0:00}", nStartPos);

            // 크레인 일때 크레인 번호로 저장
            int nDestPos = Convert.ToInt32(strDestPos);
            if (nDestPos < 100 && nDestPos != 0)
                strDestPos = string.Format("9{0:00}", nDestPos);
            #endregion

            #endregion

            #region 수신 데이터 검증
            // 작업번호가 올바른지 체크!(온라인 작업번호)
            int nLuggNum = Convert.ToInt32(strLuggNo);
            //if ((nLuggNum > 0) && (nLuggNum < 9000))
            if (modDefApp.IsOnlineLuggNum(nLuggNum) == false)
            {
                m_strLog = string.Format("작업번호가 올바르지 않습니다.[{0}]", nLuggNum);
                modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LUGG_NO);
                return;
            }

            // 작업구분이 올바른지 체크!(온라인 작업구분)
            int nJobType = Convert.ToInt32(strJob_Define);
            if (modDefApp.IsOnlineJobType(nJobType) == false)
            {
                m_strLog = string.Format("작업구분이 올바르지 않습니다.[{0}]", strJob_Define);
                modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_UNKNOWN_JOB_TYPE);
                return;
            }

            // 출발지 올바른지 체크! - 작업구분에 따라 나중에
            // 도착지 올바른지 체크! - 작업구분에 따라 나중에

            // 출발LOC 올바른지 체크! - 작업구분에 따라 나중에 
            // 도착LOC 올바른지 체크! - 작업구분에 따라 나중에
            #endregion

            bool bExistJob = false;
            //string strWhTyp = "10";
            string strJOB_TYPE = "";
            string strJOB_STATUS = "";
            string strSTART_LOC = "";
            string strDEST_LOC = "";
            string strLUGG_NO = "";

            #region 작업번호 중복 체크
            m_strSql = "SELECT * FROM JOB_MST";
            m_strSql += modDefApp.CRLF + "WHERE WH_TYP = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "  AND LUGG_NO = " + m_BDb.ParamsAdd("LUGG_NO", strLuggNo);

            m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

            if (m_iSelCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INTERNAL_ERROR);
                return;
            }
            else if (m_iSelCnt == 0)
            {
                bExistJob = false;
            }
            else //if (m_iSelCnt > 0)
            {
                bExistJob = true;
                strJOB_TYPE = "" + m_BDb.dtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["JOB_TYP"].ToString();
                strJOB_STATUS = "" + m_BDb.dtMain.Rows[0]["JOB_STATUS"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["JOB_STATUS"].ToString();
                strSTART_LOC = "" + m_BDb.dtMain.Rows[0]["START_LOCATION"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["START_LOCATION"].ToString();
                strDEST_LOC = "" + m_BDb.dtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["DEST_LOCATION"].ToString();
                strLUGG_NO = "" + m_BDb.dtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : m_BDb.dtMain.Rows[0]["LUGG_NO"].ToString();
            }
            #endregion
            
            switch (m_strMsgType)
            { 
            case "O":
                /*
                 * 문서 IV.1 의 뒷부분
                 *   +55(4) Pair Luggage No.  +59(3) Priority  +62(1) Job Routing
                 *   +63(20) Product ID       +83(20) User Data
                 *
                 *   ※ 예전 C# 은 우선순위(3) 다음에 LOT(30) / 사이즈(1) 를 읽었는데
                 *     문서에는 그 두 항목이 없다. 성격이 가장 가까운
                 *     Product ID 를 LOT_NO 자리에, Job Routing 을 사이즈 자리에 쓴다.
                 */
                strO_Priority = GfField(59, 3);
                strO_Size     = GfField(62, 1);
                strO_LotNo    = GfField(63, 20).TrimEnd();

                #region 존재하는 작업인지 체크
                #region 작업번호가 존재하는 경우 에러 로깅
                if (bExistJob == true)
                {
                    m_strLog = string.Format("이미 지시된 작업입니다. [창고 타입:{0}][작업번호:{1}]", modDefApp.WH_TYP, strLuggNo);
                    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_ALREADY_INVOKED_JOB);
                    return;
                }
                #endregion
                #region 작업정보가 완전히 같은 작업존재하는지 체크
                m_strSql = "SELECT * FROM JOB_MST";
                m_strSql += modDefApp.CRLF + "WHERE WH_TYP = '" + modDefApp.WH_TYP + "'";// m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                m_strSql += modDefApp.CRLF + "  AND START_POS = '" + strStartPos + "'";// mm_BDb.ParamsAdd("START_POS", strStartPos);
                m_strSql += modDefApp.CRLF + "  AND START_LOCATION = '" + strStartLoc + "'";// mm_BDb.ParamsAdd("START_LOCATION", strStartLoc);
                m_strSql += modDefApp.CRLF + "  AND DEST_POS = '" + strDestPos + "'";// mm_BDb.ParamsAdd("DEST_POS", strDestPos);
                m_strSql += modDefApp.CRLF + "  AND DEST_LOCATION = '" + strDestLoc + "'";// mm_BDb.ParamsAdd("DEST_LOCATION", strDestLoc);
                m_strSql += modDefApp.CRLF + "  AND LOT_NO = '" + strO_LotNo + "'";// mm_BDb.ParamsAdd("LOT_NO", strO_LotNo);
                m_strSql += modDefApp.CRLF + "  AND PRODUCT_SIZE = '" + strO_Size + "'";// mm_BDb.ParamsAdd("LOT_NO", strO_LotNo);

                m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

                if (m_iSelCnt > 0)
                {
                    m_strLog = string.Format("이미 존재하는 작업입니다.[출발지:{0}][출발LOC:{1}][도착지:{2}][도착LOC:{3}][LOT_NO:{4}]",
                        strStartPos, strStartLoc, strDestPos, strDestLoc, strO_LotNo);
                    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_LUGG_NO_DUPLICATED);
                    return;
                }
                #endregion
                #endregion

                #region 작업 구분 별로 확인해야 하는 부분 처리
                string strCELL_USE_YN = "";
                string strSC_PLT_JOB_TYP = "";
                string strCV_PLT_JOB_TYP = "";
                string strSC_PLT_JOB_TYP1 = "";
                string strCV_PLT_JOB_TYP1 = "";
                string strSC_PLT_JOB_TYP2 = "";
                string strCV_PLT_JOB_TYP2 = "";
                switch (strJob_Define)
                {
                    case "1":       // 입고
                        #region 입고시에 출발지와 도착지가 정상적인지 체크
                        // 출발지 올바른지 체크! 
                        if (IsValidStation(modDefApp.ConvertJobPattern(nJobType), strStartPos, true, ref strCV_PLT_JOB_TYP) == false)
                        {
                            m_strLog = string.Format("출발지가 올바르지 않습니다.[작업번호:{0}][출발지:{1}]", nLuggNum, strStartPos);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_STN_NO);
                            return;
                        }

                        // 도착LOC 올바른지 체크!
                        if (modDefApp.g_frmForm.IsValidLocation(m_BDb, strDestPos, strDestLoc, ref strCELL_USE_YN, ref strSC_PLT_JOB_TYP) == false)
                        {
                            m_strLog = string.Format("도착 Location이 올바르지 않습니다.[작업번호:{0}][도착지:{1}][도착LOC:{2}]", nLuggNum, strDestPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }

                        if (strCELL_USE_YN != "Y")
                        {
                            m_strLog = string.Format("도착 Location이 금지랙 입니다.[작업번호:{0}][도착지:{1}][도착LOC:{2}]", nLuggNum, strDestPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }

                        if (strCV_PLT_JOB_TYP != strSC_PLT_JOB_TYP)
                        {
                            m_strLog = string.Format("해당 출발지에서는 도착 Location로 이동할수 없습니다. ROLL입고대 <-> PLT 랙[작업번호:{0}][출발지:{1}][도착LOC:{2}]", nLuggNum, strStartPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }

                        #endregion
                        #region 해당 출발지에 지시하지 않은 작업이 존재할때 작업 생성하지 않기
                        m_strSql = "";
                        m_strSql += modDefApp.CRLF + " SELECT CD.*, JM.*                            ";
                        m_strSql += modDefApp.CRLF + "   FROM CV_DATA CD                            ";
                        m_strSql += modDefApp.CRLF + "  INNER JOIN JOB_MST JM                       ";
                        m_strSql += modDefApp.CRLF + "     ON CD.TRACK_NO = JM.START_POS            ";
                        m_strSql += modDefApp.CRLF + "    AND JM.JOB_STATUS = '99'                  ";
                        m_strSql += modDefApp.CRLF + "  WHERE CD.PLC_NO	= :PLC_NO                   ";
                        m_strSql += modDefApp.CRLF + "    AND CD.LUGG_NO_RD 	= '0'               ";
                        m_strSql += modDefApp.CRLF + "    AND JM.START_POS 	    = :START_POS        ";
                        m_strSql += modDefApp.CRLF + "    AND CD.OD_RQ_YN		= 'N'               ";
                        m_strSql += modDefApp.CRLF + "    AND CD.OD_RQ_FLAG		= 'N'               ";
                        m_strSql += modDefApp.CRLF + "    AND CD.WH_TYP		    = :WH_TYP           ";
                        m_strSql += modDefApp.CRLF + "    AND 0 = (SELECT COUNT(*)                  ";
                        m_strSql += modDefApp.CRLF + "               FROM JOB_MST                   ";
                        m_strSql += modDefApp.CRLF + "              WHERE LUGG_NO = CD.LUGG_NO_RD)  ";

                        m_BDb.comMain.CommandType = System.Data.CommandType.Text;
                        m_BDb.comMain.Parameters.Clear();       
                        m_BDb.comMain.Parameters.Add("PLC_NO", modSpDb.DbTypeChar).Value = "01";
                        m_BDb.comMain.Parameters.Add("WH_TYP", modSpDb.DbTypeChar).Value = modDefApp.WH_TYP;
                        m_BDb.comMain.Parameters.Add("START_POS", modSpDb.DbTypeChar).Value = strStartPos;

                        m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

                        if (m_iSelCnt > 0)
                        {
                            m_strLog = string.Format("해당 출발지에 지시하지 않은 작업이 존재합니다. [신규 출발지:{0}][신규 작업번호:{1}][기존 작업번호:{2}]",
                                strStartPos, strLuggNo, m_BDb.dtMain.Rows[0]["LUGG_NO"]);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_LUGG_NO_DUPLICATED);
                            return;
                        }

                        #endregion
                        break;
                    case "2":       // 출고
                        #region 출고시에 출발지와 도착지가 정상적인지 체크
                        // 출발LOC 올바른지 체크!
                        if (modDefApp.g_frmForm.IsValidLocation(m_BDb, strStartPos, strStartLoc, ref strCELL_USE_YN, ref strSC_PLT_JOB_TYP) == false)
                        {
                            m_strLog = string.Format("출발 Location이 올바르지 않습니다.[작업번호:{0}][출발지:{1}][출발LOC:{2}]", nLuggNum, strStartPos, strStartLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }

                        if (strCELL_USE_YN != "Y")
                        {
                            m_strLog = string.Format("출발 Location이 금지랙 입니다.[작업번호:{0}][출발지:{1}][출발LOC:{2}]", nLuggNum, strStartPos, strStartLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }

                        // 도착지 올바른지 체크! 
                        if (IsValidStation(modDefApp.ConvertJobPattern(nJobType), strDestPos, false, ref strCV_PLT_JOB_TYP) == false)
                        {
                            m_strLog = string.Format("도착지가 올바르지 않습니다.[작업번호:{0}][출발지:{1}]", nLuggNum, strDestPos);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_STN_NO);
                            return;
                        }

                        if (strCV_PLT_JOB_TYP != strSC_PLT_JOB_TYP)
                        {
                            m_strLog = string.Format("해당 Location에서는 도착지로 이동할수 없습니다. ROLL입고대 <-> PLT 랙[작업번호:{0}][출발LOC:{1}][도착지:{2}]", nLuggNum, strStartLoc, strDestPos);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }
                        #endregion
                        break;
                    //case "3":       // 피킹 출고
                    //    break;
                    case "4":       // 랙투랙
                        #region 랙투랙시에 출발지와 도착지가 정상적인지 체크
                        // 출발LOC 올바른지 체크!
                        if (modDefApp.g_frmForm.IsValidLocation(m_BDb, strStartPos, strStartLoc, ref strCELL_USE_YN, ref strSC_PLT_JOB_TYP1) == false)
                        {
                            m_strLog = string.Format("출발 Location이 올바르지 않습니다.[작업번호:{0}][출발지:{1}][출발LOC:{2}]", nLuggNum, strStartPos, strStartLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }

                        if (strCELL_USE_YN != "Y")
                        {
                            m_strLog = string.Format("출발 Location이 금지랙 입니다.[작업번호:{0}][출발지:{1}][출발LOC:{2}]", nLuggNum, strStartPos, strStartLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }

                        // 도착LOC 올바른지 체크!
                        if (modDefApp.g_frmForm.IsValidLocation(m_BDb, strDestPos, strDestLoc, ref strCELL_USE_YN, ref strSC_PLT_JOB_TYP2) == false)
                        {
                            m_strLog = string.Format("도착 Location이 올바르지 않습니다.[작업번호:{0}][도착지:{1}][도착LOC:{2}]", nLuggNum, strDestPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }

                        if (strCELL_USE_YN != "Y")
                        {
                            m_strLog = string.Format("도착 Location이 금지랙 입니다.[작업번호:{0}][도착지:{1}][도착LOC:{2}]", nLuggNum, strDestPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }

                        // 출발 랙과 도착 랙의 SC 번호가 다른지 체크 
                        if (strStartPos != strDestPos)
                        {
                            m_strLog = string.Format("출발 S/C와 도착 S/C가 다릅니다.[작업번호:{0}][출발SC:{1}][도착SC:{2}]", nLuggNum, strStartPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }
                        // 출발 Location과 도착 Location의 SC 번호가 다른지 체크 
                        if (modDefApp.GetStackerNum(10, strStartLoc) != modDefApp.GetStackerNum(10, strDestLoc))
                        {
                            m_strLog = string.Format("출발 S/C와 도착 S/C가 다릅니다.[작업번호:{0}][출발Location:{1}][도착Location:{2}]", nLuggNum, strStartLoc, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }

                        if (strSC_PLT_JOB_TYP1 != strSC_PLT_JOB_TYP2)
                        {
                            m_strLog = string.Format("해당 Location에서는 도착Location으로 이동할수 없습니다. ROLL랙 <-> PLT 랙[작업번호:{0}][출발LOC:{1}][도착LOC:{2}]", nLuggNum, strStartLoc, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }
                        
                        #endregion
                        break;
                    case "5":       // 호기간 이동
                        #region 호기간 이동시에 출발지와 도착지가 정상적인지 체크
                        // 출발LOC 올바른지 체크!
                        if (modDefApp.g_frmForm.IsValidLocation(m_BDb, strStartPos, strStartLoc, ref strCELL_USE_YN, ref strSC_PLT_JOB_TYP1) == false)
                        {
                            m_strLog = string.Format("출발 Location이 올바르지 않습니다.[작업번호:{0}][출발지:{1}][출발LOC:{2}]", nLuggNum, strStartPos, strStartLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }

                        if (strCELL_USE_YN != "Y")
                        {
                            m_strLog = string.Format("출발 Location이 금지랙 입니다.[작업번호:{0}][출발지:{1}][출발LOC:{2}]", nLuggNum, strStartPos, strStartLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }

                        // 도착LOC 올바른지 체크!
                        if (modDefApp.g_frmForm.IsValidLocation(m_BDb, strDestPos, strDestLoc, ref strCELL_USE_YN, ref strSC_PLT_JOB_TYP2) == false)
                        {
                            m_strLog = string.Format("도착 Location이 올바르지 않습니다.[작업번호:{0}][도착지:{1}][도착LOC:{2}]", nLuggNum, strDestPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }

                        if (strCELL_USE_YN != "Y")
                        {
                            m_strLog = string.Format("도착 Location이 금지랙 입니다.[작업번호:{0}][도착지:{1}][도착LOC:{2}]", nLuggNum, strDestPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }


                        // 출발 랙과 도착 랙의 SC 번호가 다른지 체크 
                        if (strStartPos == strDestPos)
                        {
                            m_strLog = string.Format("출발 S/C와 도착 S/C가 같습니다.[작업번호:{0}][출발SC:{1}][도착SC:{2}]", nLuggNum, strStartPos, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }
                        // 출발 Location과 도착 Location의 SC 번호가 다른지 체크 
                        if (modDefApp.GetStackerNum(10, strStartLoc) != modDefApp.GetStackerNum(10, strDestLoc))
                        {
                            m_strLog = string.Format("출발 S/C와 도착 S/C가 같습니다.[작업번호:{0}][출발Location:{1}][도착Location:{2}]", nLuggNum, strStartLoc, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                            return;
                        }

                        if (strSC_PLT_JOB_TYP1 != strSC_PLT_JOB_TYP2)
                        {
                            m_strLog = string.Format("해당 Location에서는 도착Location으로 이동할수 없습니다. ROLL랙 <-> PLT 랙[작업번호:{0}][출발LOC:{1}][도착LOC:{2}]", nLuggNum, strStartLoc, strDestLoc);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INHIBITED_LOC);
                            return;
                        }

                        #endregion
                        break;
                    case "6":       // 작업대 이동
                        #region 작업대 이동시에 출발지와 도착지가 정상적인지 체크
                        // 출발지 올바른지 체크! 
                        if (IsValidStation(modDefApp.ConvertJobPattern(nJobType), strStartPos, true, ref strCV_PLT_JOB_TYP) == false)
                        {
                            m_strLog = string.Format("출발지가 올바르지 않습니다.[작업번호:{0}][출발지:{1}]", nLuggNum, strStartPos);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_STN_NO);
                            return;
                        }

                        // 도착지 올바른지 체크! 
                        if (IsValidStation(modDefApp.ConvertJobPattern(nJobType), strDestPos, false, ref strCV_PLT_JOB_TYP) == false)
                        {
                            m_strLog = string.Format("도착지가 올바르지 않습니다.[작업번호:{0}][도착지:{1}]", nLuggNum, strDestPos);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_STN_NO);
                            return;
                        }

                        if (strSC_PLT_JOB_TYP1 != strSC_PLT_JOB_TYP2)
                        {
                            m_strLog = string.Format("해당 출발지에서는 도착지로 이동할수 없습니다. ROLL CV <-> PLT CV[작업번호:{0}][출발지:{1}][도착지:{2}]", nLuggNum, strStartPos, strDestPos);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_STN_NO);
                            return;
                        }
                        #endregion
                        #region 해당 출발지에 지시하지 않은 작업이 존재할때 작업 생성하지 않기
                        m_strSql = "";
                        m_strSql += modDefApp.CRLF + " SELECT CD.*, JM.*                            ";
                        m_strSql += modDefApp.CRLF + "   FROM CV_DATA CD                            ";
                        m_strSql += modDefApp.CRLF + "  INNER JOIN JOB_MST JM                       ";
                        m_strSql += modDefApp.CRLF + "     ON CD.TRACK_NO = JM.START_POS            ";
                        m_strSql += modDefApp.CRLF + "    AND JM.JOB_STATUS = '99'                  ";
                        m_strSql += modDefApp.CRLF + "  WHERE CD.PLC_NO	= :PLC_NO                   ";
                        m_strSql += modDefApp.CRLF + "    AND CD.LUGG_NO_RD 	= '0'               ";
                        m_strSql += modDefApp.CRLF + "    AND JM.START_POS 	    = :START_POS        ";
                        m_strSql += modDefApp.CRLF + "    AND CD.OD_RQ_YN		= 'N'               ";
                        m_strSql += modDefApp.CRLF + "    AND CD.OD_RQ_FLAG		= 'N'               ";
                        m_strSql += modDefApp.CRLF + "    AND CD.WH_TYP		    = :WH_TYP           ";
                        m_strSql += modDefApp.CRLF + "    AND 0 = (SELECT COUNT(*)                  ";
                        m_strSql += modDefApp.CRLF + "               FROM JOB_MST                   ";
                        m_strSql += modDefApp.CRLF + "              WHERE LUGG_NO = CD.LUGG_NO_RD)  ";

                        m_BDb.comMain.CommandType = System.Data.CommandType.Text;
                        m_BDb.comMain.Parameters.Clear();       
                        m_BDb.comMain.Parameters.Add("PLC_NO", modSpDb.DbTypeChar).Value = "01";
                        m_BDb.comMain.Parameters.Add("WH_TYP", modSpDb.DbTypeChar).Value = modDefApp.WH_TYP;
                        m_BDb.comMain.Parameters.Add("START_POS", modSpDb.DbTypeChar).Value = strStartPos;

                        m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

                        if (m_iSelCnt > 0)
                        {
                            m_strLog = string.Format("해당 출발지에 지시하지 않은 작업이 존재합니다. [신규 출발지:{0}][신규 작업번호:{1}][기존 작업번호:{1}]",
                                strStartPos, strLuggNo, m_BDb.dtMain.Rows[0]["LUGG_NO"]);
                            modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_LUGG_NO_DUPLICATED);
                            return;
                        }

                        #endregion
                        break;
                    default:
                        m_strLog = string.Format("작업구분이 올바르지 않습니다.[{0}]", strJob_Define);
                        modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                        MakeResponse(m_strMsgType, "", modDefApp.MSG_UNKNOWN_JOB_TYPE);
                        return;
                }
                #endregion

                #region 작업 생성
                m_BDb.BeginTrans();

                m_BDb.ParamsClear();
                if (modDefApp.g_frmForm.InsertJobMst(m_BDb, strLuggNo, strStartPos, strStartLoc, strDestPos, strDestLoc, strJob_Define, strO_LotNo, strO_Priority, strO_Size, true) == false)
                {
                    // 함수안에서 실패시 화면에 메세지 출력함!
                    MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_ECS_BUFFER_FULL);
                    m_BDb.RollbackTrans();
                    break;
                }

                    /*
                m_strSql = "INSERT INTO JOB_MST     ";
                m_strSql += modDefApp.CRLF + "(  WH_TYP			    ";
                m_strSql += modDefApp.CRLF + " , LUGG_NO			";
                m_strSql += modDefApp.CRLF + " , START_POS			";
                m_strSql += modDefApp.CRLF + " , START_LOCATION    	";
                m_strSql += modDefApp.CRLF + " , DEST_POS          	";
                m_strSql += modDefApp.CRLF + " , DEST_LOCATION     	";
                m_strSql += modDefApp.CRLF + " , JOB_TYP		    ";
                m_strSql += modDefApp.CRLF + " , JOB_STATUS        	";
                m_strSql += modDefApp.CRLF + " , LOT_NO		    	";
                m_strSql += modDefApp.CRLF + " , JOB_PRIORITY     	";
                m_strSql += modDefApp.CRLF + " , INS_DT		    	";
                m_strSql += modDefApp.CRLF + " , INS_USER_ID       	";
                m_strSql += modDefApp.CRLF + " , REMARKS )         	";
                m_strSql += modDefApp.CRLF + "VALUES ('" + modDefApp.WH_TYP + "'";
                m_strSql += modDefApp.CRLF + "     ,  '" + strLuggNo + "'";
                m_strSql += modDefApp.CRLF + "     ,  '" + strStartPos + "'";
                m_strSql += modDefApp.CRLF + "     ,  '" + strStartLoc + "'";
                m_strSql += modDefApp.CRLF + "     ,  '" + strDestPos + "'";
                m_strSql += modDefApp.CRLF + "     ,  '" + strDestLoc + "'";
                m_strSql += modDefApp.CRLF + "     ,  '" + strJob_Define + "'";
                m_strSql += modDefApp.CRLF + "     ,  '99'";
                m_strSql += modDefApp.CRLF + "     ,  '" + strO_LotNo + "'";
                m_strSql += modDefApp.CRLF + "     ,  '" + strO_Priority + "'";
                m_strSql += modDefApp.CRLF + "     ,   " + modDateTime.SYSDATE;
                m_strSql += modDefApp.CRLF + "     ,  'HOST_TASK'"; 
                m_strSql += modDefApp.CRLF + "     ,  '');";

                m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                if (m_iSelCnt <= 0)
                {
                    m_strLog = string.Format("작업 생성에 실패하였습니다. [작업번호:{0}][출발지:{1}][출발LOC:{2}][도착지:{3}][도착LOC:{4}][LOT_NO:{5}][DB Err:{6}]",
                        strLuggNo, strStartPos, strStartLoc, strDestPos, strDestLoc, strO_LotNo, m_BDb.strErrMsg);
                    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_ECS_BUFFER_FULL);
                    m_BDb.RollbackTrans();
                    return;
                }
                    //*/
                m_BDb.CommitTrans();

                modCmWork.ShowMsgServer(strTitle + "작업 추가 되었습니다.[작업번호:" + strLuggNo + "]", modDefApp.MSG_IMP);

                #endregion

                #region 공파레트 입고, 출고 요청이 있을때 해당 작업인지 체크할것!
                if (modDefApp.g_strEmtpyPltKind != "" && modDefApp.g_strEmtpyPltStation != "")
                {
                    switch (strJob_Define)
                    {
                    case "1":
                        if (modDefApp.g_strEmtpyPltKind == strJob_Define && modDefApp.g_strEmtpyPltStation == strStartPos)
                        {
                            modDefApp.g_frmForm.UpdateHostEmptyPlt(m_BDb, modDefApp.g_strEmtpyPltKind, modDefApp.g_strEmtpyPltStation, strLuggNo, "Q");
                        }
                                
                        break;

                    case "2":
                        if (modDefApp.g_strEmtpyPltKind == strJob_Define && modDefApp.g_strEmtpyPltStation == strDestPos)
                        {
                            modDefApp.g_frmForm.UpdateHostEmptyPlt(m_BDb, modDefApp.g_strEmtpyPltKind, modDefApp.g_strEmtpyPltStation, strLuggNo, "Q");
                        }
                                
                        break;

                    default:
                        break;
                    }
                }
                #endregion
                break;
            case "R":
                // @.문서 IV.2 : +28(1) Kind, +29(2) Stacker Number
                strR_Kind  = GfField(28, 1);
                strR_ScNum = GfField(29, 2);

                #region 존재하는 작업인지 체크
                #region 이중입고 작업번호가 존재하지 않을경우 에러로깅
                if (bExistJob == false)
                {
                    m_strLog = string.Format("이중입고 에러 작업정보가 존재하지 않음. [작업번호:{0}]", strLuggNo);
                    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_NO_EXIST_JOB);
                    return;
                }
                #endregion
                #region 작업 구분이 같은지 체크 
                if (strJOB_TYPE != "" && strJOB_TYPE != strJob_Define)
                {
                    m_strLog = string.Format("작업구분 불일치 [작업번호:{0}][OLD:{1}][NEW:{2}]", strLuggNo, strJOB_TYPE, strJob_Define);
                    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_UNKNOWN_JOB_TYPE);
                    return;
                }
                #endregion
                #region 작업상태가 재지정 가능한 상태인지 체크 - 체크하징 않음
                //if (strJOB_STATUS != "")
                //{
                //    int nJobStatus = Convert.ToInt32(strJOB_STATUS);
                //    if (nJobStatus != (int)modDefApp.EN_JOB_STATUS.enJobStatusErrorDualStore &&
                //        nJobStatus != (int)modDefApp.EN_JOB_STATUS.enJobStatusErrorDualStore2 &&
                //        nJobStatus != (int)modDefApp.EN_JOB_STATUS.enJobStatusErrorEmptyRetrieve &&
                //        nJobStatus != (int)modDefApp.EN_JOB_STATUS.enJobStatusErrorEmptyRetrieve2)
                //    {
                //        m_strLog = string.Format("재지정을 할 수 있는 에러 상태가 아님 [작업번호:{0}][작업상태:{1}]",
                //            strLuggNo, modDefApp.GetJobStatusString(nJobStatus));
                //        modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                //        MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_NO_EXIST_JOB);
                //        return;
                //    }
                //}
                #endregion
                #endregion

                #region 재지정 종류에 따라 동일 호기 작업인지 체크 
                switch (strR_Kind)
                {
                case "1":       // 입고 재지정
                    // 기존 도착 Location과 신규 도착 Location의 SC 번호가 다른지 체크 
                    if (modDefApp.GetStackerNum(10, strDEST_LOC) != modDefApp.GetStackerNum(10, strDestLoc))
                    {
                        m_strLog = string.Format("S/C 동일호기 이중입고 재지정 작업이 아닙니다.[작업번호:{0}][OLD LOC:{1}][NEW LOC:{2}]", nLuggNum, strDEST_LOC, strDestLoc);
                        modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                        MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                        return;
                    }
                    if (strDEST_LOC == strDestLoc)
                    {
                        m_strLog = string.Format("재지정 받은 LOCATION이 기존과 같습니다.[LOCATION:{0}][OLD LUGG:{1}][NEW LUGG:{2}]", strDEST_LOC, strLUGG_NO, nLuggNum);
                        modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                        MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                        return;
                    }

                    m_BDb.BeginTrans();

                    m_BDb.ParamsClear();

                    m_strSql = modDefApp.CRLF + "  UPDATE JOB_MST ";
                    m_strSql += modDefApp.CRLF + "    SET DEST_LOCATION = " + m_BDb.ParamsAdd("DEST_LOCATION", strDestLoc);
                    m_strSql += modDefApp.CRLF + "      , JOB_STATUS    = " + m_BDb.ParamsAdd("JOB_STATUS", ((int)modDefApp.EN_JOB_STATUS.enJobStatusDualStoreRetry).ToString());
                    m_strSql += modDefApp.CRLF + "      , UPD_DT        = " + modDateTime.SYSDATE;
                    m_strSql += modDefApp.CRLF + "      , UPD_USER_ID   = 'HOST_TASK'";
                    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP        = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                    m_strSql += modDefApp.CRLF + "    AND LUGG_NO        = " + m_BDb.ParamsAdd("LUGG_NO", strLuggNo);
                    m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
                    if (m_iSelCnt < 0)
                    {
                        m_strLog = m_BDb.ErrMsg + m_strSql;
                        modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                        MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INTERNAL_ERROR);
                        m_BDb.RollbackTrans();
                        return;
                    }

                    m_strLog = string.Format("입고 재지정 수신 및 작업정보 변경했습니다.[작업번호:{0}]", nLuggNum);
                    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_NOR);
                    m_BDb.CommitTrans();

                    break;
                case "2":       // 출고 재지정
                    // 기존 출발 Location과 신규 출발 Location의 SC 번호가 다른지 체크 
                    if (modDefApp.GetStackerNum(10, strSTART_LOC) != modDefApp.GetStackerNum(10, strStartLoc))
                    {
                        m_strLog = string.Format("S/C 동일호기 이중입고 재지정 작업이 아닙니다.[작업번호:{0}][OLD LOC:{1}][NEW LOC:{2}]", nLuggNum, strSTART_LOC, strStartLoc);
                        modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                        MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                        return;
                    }
                    if (strSTART_LOC == strStartLoc)
                    {
                        m_strLog = string.Format("재지정 받은 LOCATION이 기존과 같습니다.[LOCATION:{0}][OLD LUGG:{1}][NEW LUGG:{2}]", strSTART_LOC, strLUGG_NO, nLuggNum);
                        modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                        MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                        return;
                    }

                    m_BDb.BeginTrans();

                    m_BDb.ParamsClear();

                    m_strSql = modDefApp.CRLF + "  UPDATE LUGG_MST ";
                    m_strSql += modDefApp.CRLF + "    SET START_LOCATION = " + m_BDb.ParamsAdd("START_LOCATION", strStartLoc);
                    m_strSql += modDefApp.CRLF + "      , JOB_STATUS    = " + Convert.ToInt32((int)modDefApp.EN_JOB_STATUS.enJobStatusEmptyRetrieveRetry);
                    m_strSql += modDefApp.CRLF + "      , UPD_DT        = " + modDateTime.SYSDATE;
                    m_strSql += modDefApp.CRLF + "      , UPD_USER_ID   = 'HOST_TASK'";
                    m_strSql += modDefApp.CRLF + "  WHERE WH_TYP        = " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
                    m_strSql += modDefApp.CRLF + "    AND LUGGNO        = " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
                    m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
                    if (m_iSelCnt < 0)
                    {
                        m_strLog = m_BDb.ErrMsg + m_strSql;
                        modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                        MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INTERNAL_ERROR);
                        m_BDb.RollbackTrans();
                        return;
                    }

                    m_strLog = string.Format("출고 재지정 수신 및 작업정보 변경했습니다.[작업번호:{0}]", nLuggNum);
                    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_NOR);
                    m_BDb.CommitTrans();
                    break;
                default:
                    m_strLog = string.Format("유효하지 않는 재지정 종류입니다.[작업번호:{0}][재지정 종류:{1}]", nLuggNum, strR_Kind);
                    modCmWork.ShowMsgServer(strTitle + m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INVALID_LOC);
                    return;
                }
                #endregion
                break;
            }

            // 죄종적으로 트랜잭션 커밋하기!
            //m_BDb.CommitTrans();
            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_NO_ERROR);
        }

        //최초작성자	: BASE(원효재)
        //작성일		: 20170929
        //설명		    : 입/출고 홈스텐드 도착
        private void ParsePallet(int p_iBodyCnt)
        {
            string strJob_Define = null;
            string strStation = null;
            string strWH_CD = "AA06";
            string strMC_NO = "310";

            strJob_Define = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2, 1);
            strStation = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 3, 3);

            //공파렛트 입고
            if (strJob_Define == "1")
            {
                m_BDb.BeginTrans();

                m_BDb.comMain.Parameters.Clear();

                m_strSql = "SELECT * FROM ECS_REQ_INF";
                m_strSql += modDefApp.CRLF + "WHERE COMPANY_CD = " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                m_strSql += modDefApp.CRLF + "  AND AREA_CD = " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                m_strSql += modDefApp.CRLF + "  AND WH_CD = " + m_BDb.ParamsAdd("WH_CD", strWH_CD);
                m_strSql += modDefApp.CRLF + "  AND MC_TYP = 'CV'";
                m_strSql += modDefApp.CRLF + "  AND ECS_REQ_TYP = 'LD_PA'";
                m_strSql += modDefApp.CRLF + "  AND MC_NO = " + m_BDb.ParamsAdd("MC_NO", strMC_NO);

                m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

                if (m_iSelCnt == 0)
                {
                    m_BDb.comMain.Parameters.Clear();

                    m_strSql = "INSERT INTO ECS_REQ_INF";
                    m_strSql += modDefApp.CRLF + "(";
                    m_strSql += modDefApp.CRLF + "ECS_REQ_DATE";
                    m_strSql += modDefApp.CRLF + ", ECS_REQ_TIME";
                    m_strSql += modDefApp.CRLF + ", ECS_REQ_SEQ";
                    m_strSql += modDefApp.CRLF + ", COMPANY_CD";
                    m_strSql += modDefApp.CRLF + ", AREA_CD";
                    m_strSql += modDefApp.CRLF + ", WH_CD";
                    m_strSql += modDefApp.CRLF + ", MC_TYP";
                    m_strSql += modDefApp.CRLF + ", MC_NO";
                    m_strSql += modDefApp.CRLF + ", ECS_REQ_TYP";
                    m_strSql += modDefApp.CRLF + ", ECS_REQ_DAT";
                    m_strSql += modDefApp.CRLF + ") ";
                    m_strSql += modDefApp.CRLF + "VALUES (";
                    m_strSql += modDefApp.CRLF + "  CONVERT(varchar,GETDATE(),112)";
                    m_strSql += modDefApp.CRLF + ", REPLACE(CONVERT(varchar,GETDATE(),108),':','')";
                    m_strSql += modDefApp.CRLF + ", 1";
                    m_strSql += modDefApp.CRLF + ", " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                    m_strSql += modDefApp.CRLF + ", " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                    m_strSql += modDefApp.CRLF + ", " + m_BDb.ParamsAdd("WH_CD", strWH_CD);
                    m_strSql += modDefApp.CRLF + ", 'CV'";
                    m_strSql += modDefApp.CRLF + ", " + m_BDb.ParamsAdd("MC_NO", strMC_NO);
                    m_strSql += modDefApp.CRLF + ", 'LD_PA'";
                    m_strSql += modDefApp.CRLF + ", NULL";
                    m_strSql += modDefApp.CRLF + ")";

                    m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                    if (m_iSelCnt != 1)
                    {
                        m_BDb.RollbackTrans();
                        MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                        return;
                    }
                }
            }

            //공파렛트 출고
            if (strJob_Define == "2")
            {
                m_BDb.comMain.Parameters.Clear();

                m_BDb.BeginTrans();

                m_strSql = "SELECT * FROM ECS_REQ_INF";
                m_strSql += modDefApp.CRLF + "WHERE COMPANY_CD = " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                m_strSql += modDefApp.CRLF + "  AND AREA_CD = " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                m_strSql += modDefApp.CRLF + "  AND WH_CD = " + m_BDb.ParamsAdd("WH_CD", strWH_CD);
                m_strSql += modDefApp.CRLF + "  AND MC_TYP = 'CV'";
                m_strSql += modDefApp.CRLF + "  AND ECS_REQ_TYP = 'LD_PK'";
                m_strSql += modDefApp.CRLF + "  AND MC_NO = " + m_BDb.ParamsAdd("MC_NO", strMC_NO);

                m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);

                if (m_iSelCnt == 0)
                {
                    m_BDb.comMain.Parameters.Clear();

                    m_strSql = "INSERT INTO ECS_REQ_INF";
                    m_strSql += modDefApp.CRLF + "(";
                    m_strSql += modDefApp.CRLF + "ECS_REQ_DATE";
                    m_strSql += modDefApp.CRLF + ", ECS_REQ_TIME";
                    m_strSql += modDefApp.CRLF + ", ECS_REQ_SEQ";
                    m_strSql += modDefApp.CRLF + ", COMPANY_CD";
                    m_strSql += modDefApp.CRLF + ", AREA_CD";
                    m_strSql += modDefApp.CRLF + ", WH_CD";
                    m_strSql += modDefApp.CRLF + ", MC_TYP";
                    m_strSql += modDefApp.CRLF + ", MC_NO";
                    m_strSql += modDefApp.CRLF + ", ECS_REQ_TYP";
                    m_strSql += modDefApp.CRLF + ", ECS_REQ_DAT";
                    m_strSql += modDefApp.CRLF + ") ";
                    m_strSql += modDefApp.CRLF + "VALUES (";
                    m_strSql += modDefApp.CRLF + "  CONVERT(varchar,GETDATE(),112)";
                    m_strSql += modDefApp.CRLF + ", REPLACE(CONVERT(varchar,GETDATE(),108),':','')";
                    m_strSql += modDefApp.CRLF + ", 1";
                    m_strSql += modDefApp.CRLF + ", " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                    m_strSql += modDefApp.CRLF + ", " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                    m_strSql += modDefApp.CRLF + ", " + m_BDb.ParamsAdd("WH_CD", strWH_CD);
                    m_strSql += modDefApp.CRLF + ", 'CV'";
                    m_strSql += modDefApp.CRLF + ", " + m_BDb.ParamsAdd("MC_NO", strMC_NO);
                    m_strSql += modDefApp.CRLF + ", 'LD_PK'";
                    m_strSql += modDefApp.CRLF + ", NULL";
                    m_strSql += modDefApp.CRLF + ")";

                    m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                    if (m_iSelCnt != 1)
                    {
                        m_BDb.RollbackTrans();
                        MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                        return;
                    }
                }

            }

            m_BDb.CommitTrans();

            MakeResponse(m_strMsgType, "", modDefApp.MSG_NO_ERROR);

        }

        //최초작성자	: BASE(원효재)
        //작성일		: 20170929
        //설명		    : 입/출고 홈스텐드 도착
        private void ParseArrived(int p_iBodyCnt)
        {
            string strJob_Define = null;
            string strLuggNo = null;
            string strStation = null;


            strJob_Define = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2, 1);
            strLuggNo = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 3, 4);
            strStation = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 7, 3);

            m_BDb.BeginTrans();

            m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "  UPDATE LUGG_MST ";
            m_strSql += modDefApp.CRLF + "    SET ARR_COMP    = '10'";
            m_strSql += modDefApp.CRLF + "      , UPD_DT      =  " + modDateTime.SYSDATE;
            m_strSql += modDefApp.CRLF + "      , UPD_USER_ID =  " + m_BDb.ParamsAdd(modDefApp.g_strUserID);
            m_strSql += modDefApp.CRLF + "  WHERE COMPANY_CD  =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
            m_strSql += modDefApp.CRLF + "    AND AREA_CD     =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
            m_strSql += modDefApp.CRLF + "    AND SYS_GRP     =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
            m_strSql += modDefApp.CRLF + "    AND LUGGNO      =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
            m_strSql += modDefApp.CRLF + "    AND ARR_COMP    =  '00'";  // RYU 추가
            m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
            if (m_iSelCnt < 0)
            {
                m_strLog = m_BDb.ErrMsg + m_strSql;
                modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);

                MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INTERNAL_ERROR);
                m_BDb.trnMain.Rollback();
                return;
            }

            m_BDb.trnMain.Commit();
            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_NO_ERROR);

        }

        //최초작성자	: BASE(이길문)
        //작성일		: 20160829
        //설명		: 작업취소정보 수신처리
        private void ParseCancel(int p_iBodyCnt)
		{
			string strJob_Define = null;
			string strWorkNo = null;

			//If Not CheckBodyLen(nBodyCnt, 37) Then Exit Sub

			strJob_Define = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2, 1);
			strWorkNo = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 3, 4);

			m_BDb.ParamsClear();

            m_strSql = modDefApp.CRLF + "  SELECT * ";
            m_strSql += modDefApp.CRLF + "   FROM JOB_MST ";
            m_strSql += modDefApp.CRLF + "  WHERE WH_TYP     =  " + m_BDb.ParamsAdd("WH_TYP", modDefApp.WH_TYP);
            m_strSql += modDefApp.CRLF + "    AND LUGG_NO    =  " + m_BDb.ParamsAdd("LUGG_NO", strWorkNo);
            m_strSql += modDefApp.CRLF + "    AND JOB_STATUS IN ('99', '10', '20')";
            m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
			if (m_iSelCnt < 0)
			{
				m_strLog = m_BDb.ErrMsg + m_strSql;
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);

                MakeResponse(m_strMsgType, strWorkNo, modDefApp.MSG_INTERNAL_ERROR);
				return;
			}
            if (m_iSelCnt == 1)
            {
                bool bResult = modDefApp.g_frmForm.DeleteJobMst(m_BDb, true, strWorkNo, false, true);     // 함수안에서 Transaction 처리함!
                if (bResult == false)
                {
                    m_strLog = string.Format("작업 삭제 실패하였습니다. [작업번호:{0}][실패내용:{1}]", strWorkNo, modDefApp.GM_RTN_MSG);
                    modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);

                    MakeResponse(m_strMsgType, strWorkNo, modDefApp.MSG_INTERNAL_ERROR);
                    return;
                }
            }
            else if (m_iSelCnt == 0)
            {
                m_strLog = "삭제할 작업정보가 존재하지 않습니다.- 삭제 불가능한 경우(이미 진행중인 작업) 포함[작업번호:" + strWorkNo + "]";
                modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);

                MakeResponse(m_strMsgType, strWorkNo, modDefApp.MSG_NO_EXIST_JOB);
                return;            
            }
            else 
            {
                m_strLog = "작업정보가 2개 이상입니다.[작업번호:" + strWorkNo + "]";
                modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);

                MakeResponse(m_strMsgType, strWorkNo, modDefApp.MSG_ALREADY_INVOKED_JOB);
                return;
            }
            m_strLog = string.Format("작업 삭제 성공하였습니다. [작업번호:{0}]", strWorkNo);
            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_NOR);

            MakeResponse(m_strMsgType, strWorkNo, modDefApp.MSG_NO_ERROR);
		}

        //최초작성자	: BASE(이길문)
        //작성일		: 20160829
        //설명		: 상태정보 수신처리(C/V, S/C)
        private void ParseStatus(int p_iBodyCnt)
        {
            int iCnt = 0;
            int iFor = 0;
            string[] strScStatus = new string[modDefApp.SC_CNT];
            string[] strScLuggNo1 = new string[modDefApp.SC_CNT];

            //S/C 하나당 1개의 작업번호를 가진다
            //string[] strScLuggNo2 = new string[modDefApp.SC_CNT];

            string strCvStatus = null;
            string strWeight = "";
            string strPushRemote = "";
            string strCvPltLoadYON = "0";

            int iCvCnt = 0;

            //한개의 창고에 대해서는 미 사용
            string[] strScNo = new string[modDefApp.SC_CNT];
            string[] strWhCd = new string[modDefApp.SC_CNT];

            //SYS_GRP 선언이 불가능하여 고정 값으로 정의
            strWhCd[0] = modDefApp.WH_CD;
            strWhCd[1] = modDefApp.WH_CD;
            //string strCvWhTyp = null;

            //04. 20161228 이길문 ECS상태정보수신작업대Get
            if (modDefApp.g_blGetCvNo == false)
            {

                m_BDb.ParamsClear();

                m_strSql = "";
                m_strSql += modDefApp.CRLF + " SELECT A.* ";
                m_strSql += modDefApp.CRLF + "   FROM MC_STA_MST A ";
                m_strSql += modDefApp.CRLF + "  WHERE A.COMPANY_CD  =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                m_strSql += modDefApp.CRLF + "    AND A.AREA_CD     =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                m_strSql += modDefApp.CRLF + "    AND A.SYS_GRP     =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                m_strSql += modDefApp.CRLF + "    AND A.MC_TYP      =  " + m_BDb.ParamsAdd("MC_TYP", "CV");
                m_strSql += modDefApp.CRLF + "    AND A.STA_RCV_YON =  " + m_BDb.ParamsAdd("STA_RCV_YON", "Y");
                m_strSql += modDefApp.CRLF + "  ORDER BY A.WH_CD, A.MC_NO ";

                m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
                if (m_iSelCnt < 0) {
                    m_strLog = m_BDb.ErrMsg + m_strSql;
                    modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                    return;
                }
                if (m_iSelCnt == 0) {
                    m_strLog = "작업대 정보 미존재";
                    modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                    return;
                }

                iCvCnt = -1;
                modDefApp.g_strCvNo = new string[m_BDb.dtMain.Rows.Count];
                modDefApp.g_strCvSta = new string[m_BDb.dtMain.Rows.Count];

                for (iFor = 0; iFor <= m_BDb.dtMain.Rows.Count - 1; iFor++) {
                    iCvCnt += 1;
                    modDefApp.g_strCvNo[iCvCnt] = m_BDb.dtMain.Rows[iFor]["MC_NO"].ToString();
                }

                modDefApp.g_blGetCvNo = true;
            }
            //04. END

            // 설비상태가 연속으로 많은량 수신 될 때 SKIP을 위한 처리
            // 10초가 지나면 한 번은 UPDATE 할 수 있도록 이전 상태 초기화 시킴.
            if (m_tmSCMD_RecvTime.AddSeconds(10) <= DateTime.Now) {
                for (iCnt = 0; iCnt <= modDefApp.SC_CNT - 1; iCnt++)
                {
                    m_strScWhTyp[iCnt] = "00";
                    m_strScNo[iCnt] = "00";
                    m_strScStatus[iCnt] = Strings.Space(1);
                    m_strScLuggNo1[iCnt] = "0000";
                    //S/C 하나당 1개의 작업번호를 가진다
                    //m_strScLuggNo2[iCnt] = "0000";
                }

                m_strCvStatus = Strings.Space(500);
                m_tmSCMD_RecvTime = DateTime.Now;
            }

            for (iCnt = 0; iCnt <= modDefApp.SC_CNT - 1; iCnt++)
            {

                strScNo[iCnt] = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2 + (iCnt * 7), 2).PadLeft(3, '0');
                strScStatus[iCnt] = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 4 + (iCnt * 7), 1);
                strScLuggNo1[iCnt] = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 5 + (iCnt * 7), 4);

                if (int.Parse(strScStatus[iCnt].ToString()) < 0 || int.Parse(strScStatus[iCnt].ToString()) > 8) {
                    m_strLog = string.Format("미정의 SC 상태 코드입니다.[{0}]", strScStatus[iCnt]);
                    modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                    MakeResponse(m_strMsgType, "", modDefApp.MSG_INVALID_CONTENT);
                    return;
                }

            }

            strCvStatus = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2 + (modDefApp.SC_CNT * 7), modDefApp.g_strCvNo.GetUpperBound(0) + 1);
            strWeight = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2 + (modDefApp.SC_CNT * 7) + 2, 7);
            strPushRemote = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2 + (modDefApp.SC_CNT * 7) + 2 + 7, 1);

            m_BDb.BeginTrans();

            //### S/C Status UPDATE
            if (modDefApp.g_frmForm.chkScSta.Checked == true)
            {
                for (iCnt = 0; iCnt <= modDefApp.SC_CNT - 1; iCnt++)
                {
                    // 설비상태가 연속으로 많은량 수신 될 때 SKIP을 위한 처리

                    if ((m_strScNo[iCnt] != strScNo[iCnt]) || (m_strScStatus[iCnt] != strScStatus[iCnt]) || (m_strScLuggNo1[iCnt] != strScLuggNo1[iCnt]))
                    {
                        m_BDb.ParamsClear();

                        m_strSql = modDefApp.CRLF + "  UPDATE SC_STA_MST ";
                        m_strSql += modDefApp.CRLF + "    SET SC_STA  =  " + m_BDb.ParamsAdd(strScStatus[iCnt]);
                        m_strSql += modDefApp.CRLF + "      , LUGGNO1 =  " + m_BDb.ParamsAdd(strScLuggNo1[iCnt]);
                        m_strSql += modDefApp.CRLF + "      , LUGGNO2 =  NULL";
                        m_strSql += modDefApp.CRLF + "  WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                        m_strSql += modDefApp.CRLF + "    AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                        m_strSql += modDefApp.CRLF + "    AND WH_CD      =  " + m_BDb.ParamsAdd("WH_CD", strWhCd[iCnt]);
                        m_strSql += modDefApp.CRLF + "    AND SC_NO      =  " + m_BDb.ParamsAdd("SC_NO", strScNo[iCnt]);
                        m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                        if (m_iSelCnt < 0)
                        {
                            m_strLog = m_BDb.ErrMsg + m_strSql;
                            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                            m_BDb.trnMain.Rollback();
                            MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                            return;
                        }
                        if (m_iSelCnt != 1)
                        {
                            m_strLog = "ECS Status, S/C 상태 수정 실패 " + modDefApp.CRLF + m_strSql;
                            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                            m_BDb.trnMain.Rollback();
                            MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                            return;
                        }
                    }


                    m_strScNo[iCnt] = strScNo[iCnt];
                    m_strScStatus[iCnt] = strScStatus[iCnt];
                    m_strScLuggNo1[iCnt] = strScLuggNo1[iCnt];

                }
            }

            //### C/V Status UPDATE
            for (iCnt = 0; iCnt <= modDefApp.g_strCvNo.GetUpperBound(0); iCnt++)
            {

                // 설비상태가 연속으로 많은량 수신 될 떄 SKIP을 위한 처리
                if ((modDefApp.g_strCvSta[iCnt] != Strings.Mid(strCvStatus, iCnt + 1, 1)) || strPushRemote == "1")
                {
                    modDefApp.g_strCvSta[iCnt] = Strings.Mid(strCvStatus, iCnt + 1, 1);

                    
                    if (iCnt == 1)
                    {
                        m_BDb.ParamsClear();

                        if (Strings.Mid(strCvStatus, iCnt + 1, 1) == "0")
                        {
                            strCvStatus = "61";
                        }
                        else
                        {
                            strCvStatus = "21";
                        }

                        m_strSql = "";
                        m_strSql = modDefApp.CRLF + "  UPDATE MC_STA_MST ";
                        m_strSql += modDefApp.CRLF + "    SET CV_IO_TYP =  " + m_BDb.ParamsAdd("CV_IO_TYP", strCvStatus);
                        m_strSql += modDefApp.CRLF + " WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                        m_strSql += modDefApp.CRLF + "   AND AREA_CD =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                        m_strSql += modDefApp.CRLF + "   AND SYS_GRP  =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                        m_strSql += modDefApp.CRLF + "   AND MC_TYP   =  " + m_BDb.ParamsAdd("MC_TYP", "CV");
                        m_strSql += modDefApp.CRLF + "   AND MC_NO    =  " + m_BDb.ParamsAdd("MC_NO", modDefApp.g_strCvNo[iCnt]);

                        m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                        if (m_iSelCnt < 0)
                        {
                            m_strLog = m_BDb.ErrMsg + m_strSql;
                            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                            m_BDb.trnMain.Rollback();
                            MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                            return;
                        }
                        if (m_iSelCnt != 1)
                        {
                            m_strLog = "ECS Status, C/V 상태 수정 실패 " + modDefApp.CRLF + m_strSql;
                            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                            m_BDb.trnMain.Rollback();
                            MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                            return;
                        }
                    }
                    
                    if (iCnt == 0)
                    {
                        m_BDb.ParamsClear();

                        strCvPltLoadYON = "0";

                        if (strPushRemote == "1" && Strings.Mid(strCvStatus, iCnt + 1, 1) == "1")
                        {
                            strCvPltLoadYON = "1";
                        }

                        m_strSql = "";
                        m_strSql = modDefApp.CRLF + "  UPDATE MC_STA_MST ";


                        m_strSql += modDefApp.CRLF + "    SET MC_STA =  " + m_BDb.ParamsAdd(strCvPltLoadYON);
                        m_strSql += modDefApp.CRLF + "      , WEIGHT =  " + m_BDb.ParamsAdd(Convert.ToDecimal(strWeight) / 1000);

                        if (Strings.Mid(strCvStatus, iCnt + 1, 1) == "0")
                        {
                            m_strSql += modDefApp.CRLF + "  , LUGGNO = 0   ";
                        }

                        m_strSql += modDefApp.CRLF + " WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                        m_strSql += modDefApp.CRLF + "   AND AREA_CD =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                        m_strSql += modDefApp.CRLF + "   AND SYS_GRP  =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                        m_strSql += modDefApp.CRLF + "   AND MC_TYP   =  " + m_BDb.ParamsAdd("MC_TYP", "CV");
                        m_strSql += modDefApp.CRLF + "   AND MC_NO    =  " + m_BDb.ParamsAdd("MC_NO", modDefApp.g_strCvNo[iCnt]);

                        m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

                        if (m_iSelCnt < 0)
                        {
                            m_strLog = m_BDb.ErrMsg + m_strSql;
                            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                            m_BDb.trnMain.Rollback();
                            MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                            return;
                        }
                        if (m_iSelCnt != 1)
                        {
                            m_strLog = "ECS Status, C/V 상태 수정 실패 " + modDefApp.CRLF + m_strSql;
                            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                            m_BDb.trnMain.Rollback();
                            MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
                            return;
                        }
                    }

                   
                }
		
			}

			m_strCvStatus = strCvStatus;
			MakeResponse(m_strMsgType, "", modDefApp.MSG_NO_ERROR);
			m_BDb.trnMain.Commit();
		
		}
			
		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 에러리포트정보 수신처리
		private void ParseError(int p_iBodyCnt)
		{
			int i = 0;
			string strDevClass = null;
			string strErrDevNo = null;
			string strErrKind = null;
			string strErrCode = null;
			string strErrBank = null;
			string strErrBay = null;
			string strErrLev = null;
			string strIMSI_JOB_KIND = null;
			string strLuggNo = null;
			string[] strScErr = new string[modDefApp.SC_CNT + 1];
			modSpDb.sp_Cell_Info Dest_Cell = default(modSpDb.sp_Cell_Info);
			string strWH_CD = null;
            string strAREA = null;
            string strLD_CTN_NO = null;
            int nRE_DRCT_CNT = 0;

            strDevClass = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2, 1);
			//1:S/C,  2:C/V,  3:X-CAR,  4:Y-CAR,  5:전광판,  6:BCR
			if (strDevClass == "1")
				strDevClass = "SC";
			if (strDevClass == "2")
				strDevClass = "CV";
			if (strDevClass == "3")
				strDevClass = "LGV";
			if (strDevClass == "4")
				strDevClass = "RGV";
			if (strDevClass == "5")
				strDevClass = "DSP";
			//미사용
			if (strDevClass == "6")
				strDevClass = "BCR";
		
			strErrDevNo = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 3, 3);
			strErrKind = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 6, 1);
			strErrCode = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 7, 4).Trim();
			strLuggNo = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 11, 4);
			strErrBank = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 15, 2);
			strErrBay = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 17, 3);
			strErrLev = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 20, 2);

            strLuggNo = Convert.ToInt16(strLuggNo).ToString();

            //단일창고 정보 수신 안함
            //strWH_CD = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 22, 2);

            m_BDb.ParamsClear();
		    
			m_strSql = modDefApp.CRLF + "  SELECT JOB_KIND, RE_DRCT_CNT, LD_CTN_NO, SOUR_WH_CD, SOUR_AREA, DEST_WH_CD, DEST_AREA ";
			m_strSql += modDefApp.CRLF + "   FROM LUGG_MST ";
            m_strSql += modDefApp.CRLF + "  WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
            m_strSql += modDefApp.CRLF + "    AND AREA_CD =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
            m_strSql += modDefApp.CRLF + "    AND SYS_GRP =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
			m_strSql += modDefApp.CRLF + "    AND LUGGNO  =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
			m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
			if (m_iSelCnt < 0) {
				m_strLog = m_BDb.ErrMsg + m_strSql;
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
		
				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}
		
			m_BDb.BeginTrans();
		
			if (m_iSelCnt == 1) {
                strIMSI_JOB_KIND = m_BDb.dtMain.Rows[0]["JOB_KIND"].ToString();
                strLD_CTN_NO = m_BDb.dtMain.Rows[0]["LD_CTN_NO"].ToString();
                nRE_DRCT_CNT = Convert.ToInt16(m_BDb.dtMain.Rows[0]["RE_DRCT_CNT"]);

                if (strErrKind == "1")
                {
                    strWH_CD = m_BDb.dtMain.Rows[0]["DEST_WH_CD"].ToString();
                    strAREA = m_BDb.dtMain.Rows[0]["DEST_AREA"].ToString();
                }
                else
                {
                    strWH_CD = m_BDb.dtMain.Rows[0]["SOUR_WH_CD"].ToString();
                    strAREA = m_BDb.dtMain.Rows[0]["SOUR_AREA"].ToString();
                }


                //------------------------------------------------
                //작업정보 에러 업데이트
                //------------------------------------------------
                m_BDb.ParamsClear();
		
				m_strSql = "";
				m_strSql += modDefApp.CRLF + "  UPDATE LUGG_MST ";
				m_strSql += modDefApp.CRLF + "    SET JOB_STA    = '3' ";
				m_strSql += modDefApp.CRLF + "      , ERR_KIND   =  " + m_BDb.ParamsAdd(strErrKind);
				m_strSql += modDefApp.CRLF + "      , ERR_CODE   =  " + m_BDb.ParamsAdd(strErrCode);
				m_strSql += modDefApp.CRLF + "      , ERR_MC_TYP =  " + m_BDb.ParamsAdd(strDevClass);
				if (strDevClass == "SC") {
					m_strSql += modDefApp.CRLF + "  , ERR_MC_NO =  " + m_BDb.ParamsAdd(Strings.Mid(strErrDevNo, 2, 3));
				} else {
					m_strSql += modDefApp.CRLF + "  , ERR_MC_NO =  " + m_BDb.ParamsAdd(strErrDevNo);
				}
				m_strSql += modDefApp.CRLF + "      , UPD_DT      =  " + modDateTime.SYSDATE;
				m_strSql += modDefApp.CRLF + "      , UPD_WIN_ID  =  " + m_BDb.ParamsAdd(modDefApp.PRS_ID);
				m_strSql += modDefApp.CRLF + "      , UPD_USER_ID =  " + m_BDb.ParamsAdd(modDefApp.g_strUserID);
                m_strSql += modDefApp.CRLF + "  WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                m_strSql += modDefApp.CRLF + "    AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                m_strSql += modDefApp.CRLF + "    AND SYS_GRP    =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
				m_strSql += modDefApp.CRLF + "    AND LUGGNO     =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
				m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
				if (m_iSelCnt < 0) {
					m_strLog = m_BDb.ErrMsg + m_strSql;
					modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
		
					m_BDb.trnMain.Rollback();
					MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
					return;
				}
				if (m_iSelCnt != 1) {
					m_strLog = "Error Report, LUGG_MST 에러상태 수정 실패 ";
					modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
		
					m_BDb.trnMain.Rollback();
					MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
					return;
				}
		
				//03. 20161122 이길문 이중입고일 경우 로케이션 재지정
				if (strErrKind == "1") {
					//------------------------------------------------
					// 기존저장위치 이중입고 마킹
					//------------------------------------------------
					m_BDb.ParamsClear();
		
					m_strSql = "";
					m_strSql += modDefApp.CRLF + " UPDATE CELL_MST                                                                      ";
					m_strSql += modDefApp.CRLF + "    SET CELL_STA = '4'                                                                ";
					m_strSql += modDefApp.CRLF + "  WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);  // RYU 추가
                    m_strSql += modDefApp.CRLF + "    AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                    m_strSql += modDefApp.CRLF + "    AND CELL_NO    =  " + m_BDb.ParamsAdd(strWH_CD + strAREA);
					m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
					if (m_iSelCnt < 0) {
						m_strLog = m_BDb.ErrMsg + m_strSql;
						modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
		
						m_BDb.trnMain.Rollback();
						MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
						return;
					}
					if (m_iSelCnt != 1) {
						m_strLog = "Error Report, CELL_MST 셀상태 수정 실패 ";
						modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
		
						m_BDb.trnMain.Rollback();
						MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
						return;
					}
					//------------------------------------------------
					//ini에 정의된 횟수만큼만 재지정
					//------------------------------------------------
					if (Convert.ToInt32(nRE_DRCT_CNT) < Convert.ToInt32(modDefApp.g_strRE_DRCT_CNT)) {
                        //------------------------------------------------
                        // 입고가능한 새로운 저장위치Get
                        //------------------------------------------------
                        if (!modSpDb.Pro_Lugg_Logging(ref m_BDb, modDefApp.SYS_GRP, strLuggNo, "N", false, false))
                        {
                            m_strLog = " 이중입고 상태 작업번호 Logging 실패-" + modSpDb.SP_Err_Msg;
                            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
                            m_BDb.trnMain.Rollback();
                            MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);

                            return;
                        }

                        if (!modSpDb.Get_Pa_Cell_Double(ref m_BDb, strWH_CD, strAREA, strLD_CTN_NO, ref Dest_Cell, false, false))
                        {
                            m_strLog = "이중입고 신규 저장위치 Get실패-" + modSpDb.SP_Err_Msg;
                            modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);

                            //m_BDb.trnMain.Rollback();
                            //MakeResponse(m_strMsgType, "","", "", "", modDefApp.MSG_INTERNAL_ERROR);
                            //return;
                        }

                        //------------------------------------------------
                        // 새로운 저장위치를 구하였을 경우
                        //------------------------------------------------
                        if (!string.IsNullOrEmpty(Dest_Cell.WH_TYP)) {
							//------------------------------------------------
							// 새로운 저장위치로 재지시 작업정보 업데이트
							//------------------------------------------------
							m_BDb.ParamsClear();
		
							m_strSql = "";
							m_strSql += modDefApp.CRLF + " UPDATE LUGG_MST                                                     ";
							m_strSql += modDefApp.CRLF + "    SET JOB_STA     = '0'                                            ";
							m_strSql += modDefApp.CRLF + "      , IF_COMMAND  = 'R'                                            ";
							m_strSql += modDefApp.CRLF + "      , IF_ERR_CODE = IF_SYS_TYP " + modDefApp.II + " '00'                ";
							m_strSql += modDefApp.CRLF + "      , PRIORITY    = '150'                                          ";
							m_strSql += modDefApp.CRLF + "      , CELL_TYP    =  " + m_BDb.ParamsAdd(Dest_Cell.CELL_TYP);
							m_strSql += modDefApp.CRLF + "      , DEST_AREA   =  " + m_BDb.ParamsAdd(Dest_Cell.AREA);
							m_strSql += modDefApp.CRLF + "      , DEST_SITE   =  " + m_BDb.ParamsAdd(Dest_Cell.SC_NO);
							m_strSql += modDefApp.CRLF + "      , DEST_BANK   =  " + m_BDb.ParamsAdd(Dest_Cell.BANK);
							m_strSql += modDefApp.CRLF + "      , DEST_BAY    =  " + m_BDb.ParamsAdd(Dest_Cell.BAY);
							m_strSql += modDefApp.CRLF + "      , DEST_LEV    =  " + m_BDb.ParamsAdd(Dest_Cell.LEV);
							m_strSql += modDefApp.CRLF + "      , ERR_CODE    = '0000'                                         ";
							m_strSql += modDefApp.CRLF + "      , UPD_DT      =  " + modDateTime.SYSDATE;
							m_strSql += modDefApp.CRLF + "      , UPD_WIN_ID  =  " + m_BDb.ParamsAdd(modDefApp.PRS_ID);
							m_strSql += modDefApp.CRLF + "      , UPD_USER_ID =  " + m_BDb.ParamsAdd(modDefApp.g_strUserID);
							//재지정횟수 증가
							m_strSql += modDefApp.CRLF + "      , RE_DRCT_CNT = RE_DRCT_CNT + 1 ";
                            m_strSql += modDefApp.CRLF + "  WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                            m_strSql += modDefApp.CRLF + "    AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                            m_strSql += modDefApp.CRLF + "    AND SYS_GRP    =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
							m_strSql += modDefApp.CRLF + "    AND LUGGNO     =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
							m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
							if (m_iSelCnt < 0) {
								m_strLog = m_BDb.ErrMsg + m_strSql;
								modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
		
								m_BDb.trnMain.Rollback();
								MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
								return;
							}
							if (m_iSelCnt != 1) {
								m_strLog = "Error Report, LUGG_MST 재지시정보 수정 실패 ";
								modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
		
								m_BDb.trnMain.Rollback();
								MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
								return;
							}
						}
					}
				}
				//03. END
			} else {
				strIMSI_JOB_KIND = "0";
			}
		
			m_BDb.ParamsClear();
		
			m_strSql = modDefApp.CRLF + "  INSERT INTO MC_ERR_LOG (";
			m_strSql += modDefApp.CRLF + "   LOG_DTTM       ";
			m_strSql += modDefApp.CRLF + " , LOG_DATE       ";
			m_strSql += modDefApp.CRLF + " , LOG_TIME       ";
			m_strSql += modDefApp.CRLF + " , LOG_SEQ        ";
			m_strSql += modDefApp.CRLF + " , COMPANY_CD     ";
            m_strSql += modDefApp.CRLF + " , AREA_CD        ";
            m_strSql += modDefApp.CRLF + " , SYS_GRP        ";
            m_strSql += modDefApp.CRLF + " , LUGGNO         ";
            m_strSql += modDefApp.CRLF + " , JOB_KIND       ";
			m_strSql += modDefApp.CRLF + " , ERR_WH_CD      ";
			m_strSql += modDefApp.CRLF + " , ERR_MC_TYP     ";
			m_strSql += modDefApp.CRLF + " , ERR_MC_NO      ";
			m_strSql += modDefApp.CRLF + " , ERR_BANK       ";
			m_strSql += modDefApp.CRLF + " , ERR_BAY        ";
			m_strSql += modDefApp.CRLF + " , ERR_LEV        ";
			m_strSql += modDefApp.CRLF + " , ERR_KIND       ";
			m_strSql += modDefApp.CRLF + " , ERR_CODE       ";
			m_strSql += modDefApp.CRLF + " , LOG_YWEEK      ";
			m_strSql += modDefApp.CRLF + " , LOG_MWEEK      ";
			m_strSql += modDefApp.CRLF + " , LOG_DAY        ";
			//04. END
			m_strSql += modDefApp.CRLF + " ) ";
			m_strSql += modDefApp.CRLF + " VALUES (";
			m_strSql += modDefApp.CRLF + "    " + modDateTime.SYSDATE_TO_CDTTM;
			m_strSql += modDefApp.CRLF + " ,  " + modDateTime.SYSDATE_TO_CDT;
			m_strSql += modDefApp.CRLF + " ,  " + modDateTime.SYSDATE_TO_CTM;
            m_strSql += modDefApp.CRLF + " ,  '1'";
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("JOB_KIND", strIMSI_JOB_KIND);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("WH_CD", modDefApp.WH_CD);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("ERR_MC_TYP", strDevClass);
            if (strDevClass == "SC") {
				m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("ERR_MC_NO", Strings.Mid(strErrDevNo, 2, 3));
            } else {
				m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("ERR_MC_NO", strErrDevNo);
            }
			m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("ERR_BANK", strErrBank);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("ERR_BAY", strErrBay);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("ERR_LEV", strErrLev);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("ERR_KIND", strErrKind);
            m_strSql += modDefApp.CRLF + " ,  " + m_BDb.ParamsAdd("ERR_CODE", strErrCode);
                                                                    //04. 이길문 설비에러내역컬럼추가
#if ORACLE
			m_strSql += modDefApp.CRLF + " , TO_CHAR(SYSDATE, 'ww') ";
			m_strSql += modDefApp.CRLF + " , CEIL((TO_NUMBER(TO_CHAR(SYSDATE, 'DD')) + 7 - TO_NUMBER(TO_CHAR(SYSDATE, 'D'))) / 7) ";
			m_strSql += modDefApp.CRLF + " , TO_NUMBER(TO_CHAR(SYSDATE,'D')) ";
#endif
#if SQL
            m_strSql += modDefApp.CRLF + " , DATEPART(WEEK, GETDATE()) ";
			m_strSql += modDefApp.CRLF + " , CEILING((DAY(GETDATE()) + DATEPART(DW,LEFT(CONVERT(VARCHAR(10), GETDATE(), 112),6) + '01')-1)/7.0) ";
			m_strSql += modDefApp.CRLF + " , DATEPART(DW, GETDATE()) ";
#endif
			//04. END
			m_strSql += modDefApp.CRLF + " ) ";
			m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
			if (m_iSelCnt < 0) {
				m_strLog = m_BDb.ErrMsg + m_strSql;
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
		
				m_BDb.trnMain.Rollback();
				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}
		
			m_BDb.trnMain.Commit();
		
			MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_NO_ERROR);
		
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 작업완료정보수신처리
		// 작업 완료  
		private void ParseComplete(int p_iBodyCnt)
		{
			int i = 0;
			string strJobType = null;
			string strLuggNo = null;
			string strStep = null;
			string strTemp = null;
			string strWhcd = null;
			string strViaSite = null;
			string strMC_ORD_DTTM = null;
			string strMC_CMP_DTTM = null;
            
			strLuggNo = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 3, 4);
			strStep = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 8, 1);
			//06. END

			m_BDb.ParamsClear();

			m_strSql = modDefApp.CRLF + "  SELECT * ";
			m_strSql += modDefApp.CRLF + "   FROM LUGG_MST ";
            m_strSql += modDefApp.CRLF + " WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
            m_strSql += modDefApp.CRLF + "   AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
            m_strSql += modDefApp.CRLF + "   AND SYS_GRP    =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
			m_strSql += modDefApp.CRLF + "   AND LUGGNO     =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
			m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
			if (m_iSelCnt < 0)
			{
				m_strLog = m_BDb.ErrMsg + m_strSql;
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);

				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}

			if (m_iSelCnt == 1)
			{
				strJobType = "" + m_BDb.dtMain.Rows[0]["JOB_KIND"].ToString();
				strViaSite = "" + m_BDb.dtMain.Rows[0]["VIA_SITE"].ToString();

				m_BDb.ParamsClear();

				switch (strJobType)
				{
					case "1":

                        m_strSql = modDefApp.CRLF + "  UPDATE LUGG_MST ";
                        m_strSql += modDefApp.CRLF + "    SET IN_COMP = '10' ";
                        m_strSql += modDefApp.CRLF + "      , MC_CMP_DTTM =  " + modDateTime.SYSDATE_TO_CDTTM;
                        m_strSql += modDefApp.CRLF + " WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                        m_strSql += modDefApp.CRLF + "   AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                        m_strSql += modDefApp.CRLF + "   AND SYS_GRP    =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                        m_strSql += modDefApp.CRLF + "   AND LUGGNO     =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
                        m_strSql += modDefApp.CRLF + "   AND IN_COMP = '00' ";
                        break;
                    case "3":
                    case "2":

                        m_strSql = modDefApp.CRLF + "  UPDATE LUGG_MST ";
                        m_strSql += modDefApp.CRLF + "    SET OUT_COMP    = '10' ";
                        m_strSql += modDefApp.CRLF + "      , MC_CMP_DTTM =  " + modDateTime.SYSDATE_TO_CDTTM;
                        m_strSql += modDefApp.CRLF + " WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                        m_strSql += modDefApp.CRLF + "   AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                        m_strSql += modDefApp.CRLF + "   AND SYS_GRP    =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                        m_strSql += modDefApp.CRLF + "   AND LUGGNO     =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
                        m_strSql += modDefApp.CRLF + "   AND OUT_COMP    = '00' ";
                        break;
					case "4":

                        m_strSql = modDefApp.CRLF + "  UPDATE LUGG_MST ";
                        m_strSql += modDefApp.CRLF + "    SET IN_COMP    = '10' ";
                        m_strSql += modDefApp.CRLF + "      , MC_CMP_DTTM =  " + modDateTime.SYSDATE_TO_CDTTM;
                        m_strSql += modDefApp.CRLF + " WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                        m_strSql += modDefApp.CRLF + "   AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                        m_strSql += modDefApp.CRLF + "   AND SYS_GRP    =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                        m_strSql += modDefApp.CRLF + "   AND LUGGNO     =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
                        m_strSql += modDefApp.CRLF + "   AND IN_COMP    = '00' ";
                        break;
					case "5":
						break;
					case "6":

                        m_strSql = modDefApp.CRLF + "  UPDATE LUGG_MST ";
                        m_strSql += modDefApp.CRLF + "    SET IN_COMP = '10' ";
                        m_strSql += modDefApp.CRLF + "      , MC_CMP_DTTM =  " + modDateTime.SYSDATE_TO_CDTTM;
                        m_strSql += modDefApp.CRLF + " WHERE COMPANY_CD =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
                        m_strSql += modDefApp.CRLF + "   AND AREA_CD    =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
                        m_strSql += modDefApp.CRLF + "   AND SYS_GRP    =  " + m_BDb.ParamsAdd("SYS_GRP", modDefApp.SYS_GRP);
                        m_strSql += modDefApp.CRLF + "   AND LUGGNO     =  " + m_BDb.ParamsAdd("LUGGNO", strLuggNo);
                        m_strSql += modDefApp.CRLF + "   AND IN_COMP = '00' ";
                        break;
                    case "7":
						break;
					default:
						m_strLog = "Job Complete, 미정의 작업종류[" + strJobType + "]";
						modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
						MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
						return;
				}

				m_BDb.BeginTrans();

				m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);

				if (m_iSelCnt < 0)
				{
					m_strLog = m_BDb.ErrMsg + m_strSql;
					modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);

					m_BDb.trnMain.Rollback();
					MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_INTERNAL_ERROR);
					return;
				}

				m_BDb.trnMain.Commit();
			}

            MakeResponse(m_strMsgType, strLuggNo, modDefApp.MSG_NO_ERROR);

		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		    : 바코드정보 수신처리
		private void ParseBcrData(int p_iBodyCnt)
		{
			string strMC_NO = null;
			string strReadingData = null;
			string strWeight = null;
			string strBrand = null;
			bool blSucYon = false;
			int iPassCd = 0;
			int iEmSite = 0;
			string strMY_NAME = "바코드정보 수신";

			strMC_NO = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2, 3);
			strReadingData = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 5, 20).Trim();
			strWeight = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 25, 20).Trim();
			blSucYon = true;
			iPassCd = 0;
			iEmSite = 0;
			strBrand = Strings.Space(5);

			if (string.IsNullOrEmpty(strReadingData))
			{
				m_strLog = "바코드 수신정보가 없습니다.";
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}

			m_BDb.ParamsClear();

			m_strSql = modDefApp.CRLF + " SELECT * ";
			m_strSql += modDefApp.CRLF + "  FROM LD_CTN_MST ";
			m_strSql += modDefApp.CRLF + " WHERE LD_CTN_NO  =  " + m_BDb.ParamsAdd("LD_CTN_NO", strReadingData);
			m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
			if (m_iSelCnt < 0)
			{
				m_strLog = "적재용기정보조회실패-" + m_BDb.ErrMsg;
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}
			if (m_iSelCnt == 0)
			{
				m_strLog = "적재용기정보 없음[" + strReadingData + "]";
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}
			else
			{
				m_BDb.ParamsClear();

				m_strSql = modDefApp.CRLF + " SELECT * ";
				m_strSql += modDefApp.CRLF + "  FROM MC_STA_MST ";
				m_strSql += modDefApp.CRLF + " WHERE MC_NO  =  " + m_BDb.ParamsAdd("MC_NO", strMC_NO);
				m_iSelCnt = m_BDb.ExcuteQry_Par(ref m_strSql);
				if (m_iSelCnt < 0)
				{
					m_strLog = "설비상태정보 조회실패:" + m_BDb.ErrMsg;
					modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
					MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
					return;
				}
				if (m_iSelCnt == 0)
				{
					m_strLog = "설비상태정보가 존재하지 않습니다.작업대[" + strMC_NO + "]";
					modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
					MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
					return;
				}
				else
				{
                    //자동입고대 [13], 출고대 [21]
					if (m_BDb.dtMain.Rows[0]["CV_IO_TYP"].ToString() == "13")
					{
						if (m_BDb.dtMain.Rows[0]["MC_USE_DEF"].ToString() == "1" || m_BDb.dtMain.Rows[0]["MC_USE_DEF"].ToString() == "3")
						{
							m_strLog = "작업대 사용금지[" + strMC_NO + "]";
							modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
							MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
							return;
						}
					}
					else if (m_BDb.dtMain.Rows[0]["CV_IO_TYP"].ToString() == "21")
					{
						if (m_BDb.dtMain.Rows[0]["MC_USE_DEF"].ToString() == "2" || m_BDb.dtMain.Rows[0]["MC_USE_DEF"].ToString() == "3")
						{
							m_strLog = "작업대 사용금지[" + strMC_NO + "]";
							modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
							MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
							return;
						}
					}
				}
			}

			try
			{
				m_BDb.BeginTrans();

				if (UPD_MC_STA_MST(strMC_NO, strReadingData, ref m_strLog) == true)
				{
					MakeResponse(m_strMsgType, "", modDefApp.MSG_NO_ERROR);
					m_BDb.trnMain.Commit();
					return;
				}	
			}
			catch (Exception ex)
			{
				modCmWork.ShowMsgServer(ex.ToString(), modDefApp.MSG_ERR);

				m_BDb.trnMain.Rollback();
				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}

			m_BDb.trnMain.Rollback();
			modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
			MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
		}
		
		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명	     	: ECS Throughput Report
		private void ParseThroughput(int nBodyCnt)
		{
			modCmWork.ShowMsgServer("ParseThroughput Start...", modDefApp.MSG_NOR);

			string strLOG_DTHH = null;
			string strWH_TYP = null;
			string strMC_TYP = null;
			string strMC_NO = null;
			string strLUG_CNT = null;
			int intLUG_CNT;
			string strRUN_TM = null;
			int intRUN_TM;
			string strERR_TM = null;
			int intERR_TM;
			string strHLD_TM = null;
			int intHLD_TM;

			string strWH_CD = null;
			string strErrMsg = null;

			strLOG_DTHH = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 2, 10);
			strMC_TYP = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 12, 1);
			//1:S/C,  2:C/V,  3:LGV,  4:RTV,  5:파렛타이저,  6:BCR
			if (strMC_TYP == "1")
				strMC_TYP = "SC";
			if (strMC_TYP == "2")
				strMC_TYP = "CV";
			if (strMC_TYP == "3")
				strMC_TYP = "LGV";
			if (strMC_TYP == "4")
				strMC_TYP = "RTV";
			if (strMC_TYP == "5")
				strMC_TYP = "PT";
			if (strMC_TYP == "6")
				strMC_TYP = "BCR";

			strMC_NO = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 13, 3);
			strLUG_CNT = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 16, 4);
			strRUN_TM = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 20, 4);
			strERR_TM = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 24, 4);
			strHLD_TM = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 28, 4);
			strWH_CD = System.Text.Encoding.UTF8.GetString(m_bytRxBuff, modDefApp.MSG_HEAD_CNT + 32, 2);

			if (Information.IsNumeric(strLUG_CNT.Trim()))
			{
				intLUG_CNT = Convert.ToInt32(strLUG_CNT.Trim());
			}
			else
			{
				intLUG_CNT = 0;
			}
			if (Information.IsNumeric(strRUN_TM.Trim()))
			{
				intRUN_TM = Convert.ToInt32(strRUN_TM.Trim());
			}
			else
			{
				intRUN_TM = 0;
			}
			if (Information.IsNumeric(strERR_TM.Trim()))
			{
				intERR_TM = Convert.ToInt32(strERR_TM.Trim());
			}
			else
			{
				intERR_TM = 0;
			}
			if (Information.IsNumeric(strHLD_TM.Trim()))
			{
				intHLD_TM = Convert.ToInt32(strHLD_TM.Trim());
			}
			else
			{
				intHLD_TM = 0;
			}

			if (GetWH_TYP(strWH_CD,ref strWH_TYP, ref strErrMsg) == false)
			{
				m_strLog = strErrMsg + "[" + strWH_TYP + "]";
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				m_BDb.RollbackTrans();
				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}

			m_BDb.BeginTrans();

			if (!modSpDb.Pro_Mc_Elps_Log(ref m_BDb, strLOG_DTHH, strWH_TYP, strMC_TYP, strMC_NO, strLUG_CNT, strRUN_TM, strERR_TM, strHLD_TM, false,
			false))
			{
				m_strLog = "설비 가동 LOGGING처리 실패 " + modDefApp.CRLF + modSpDb.SP_Err_Msg;
				modCmWork.ShowMsgServer(m_strLog, modDefApp.MSG_ERR);
				m_BDb.RollbackTrans();
				MakeResponse(m_strMsgType, "", modDefApp.MSG_INTERNAL_ERROR);
				return;
			}

			m_BDb.CommitTrans();

			MakeResponse(m_strMsgType, "", modDefApp.MSG_NO_ERROR);

		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 통계일자, Shift 구하기

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 수신바코드정보 UPDATE
		private bool UPD_MC_STA_MST(string p_strMC_NO, string p_strLD_CTN_NO, ref string p_strErrMsg)
		{
			m_BDb.ParamsClear();

			m_strSql = modDefApp.CRLF + "  UPDATE MC_STA_MST ";
			m_strSql += modDefApp.CRLF + "    SET LD_CTN_NO   =  " + m_BDb.ParamsAdd(p_strLD_CTN_NO);
			m_strSql += modDefApp.CRLF + "      , SCH_PRC_STA = '00' ";
			m_strSql += modDefApp.CRLF + "      , PRC_CFM_STA = '00' ";
			m_strSql += modDefApp.CRLF + "      , MC_RCV_DT   =  " + modDateTime.SYSDATE;
            m_strSql += modDefApp.CRLF + " WHERE COMPANY_CD  =  " + m_BDb.ParamsAdd("COMPANY_CD", modDefApp.COMPANY_CD);
            m_strSql += modDefApp.CRLF + "   AND AREA_CD     =  " + m_BDb.ParamsAdd("AREA_CD", modDefApp.AREA_CD);
            m_strSql += modDefApp.CRLF + "   AND WH_CD       =  " + m_BDb.ParamsAdd("WH_CD", modDefApp.WH_CD);
			m_strSql += modDefApp.CRLF + "   AND MC_TYP      =  " + m_BDb.ParamsAdd("MC_TYP", "CV");
			m_strSql += modDefApp.CRLF + "   AND MC_NO       =  " + m_BDb.ParamsAdd("MC_NO", p_strMC_NO);
			m_iSelCnt = m_BDb.ExcuteNonQry_Par(ref m_strSql);
			if (m_iSelCnt < 0)
			{
				p_strErrMsg = "MC_STA_MST UPDATE 실패-" + m_BDb.ErrMsg;
				return false;
			}
			if (m_iSelCnt == 0)
			{
				p_strErrMsg = "UPDATE된 MC_STA_MST정보 없음";
				return false;
			}

			return true;
		}

        private bool GetWH_TYP(string p_strRCV_WH_CD, ref string p_strRTN_WH_TYP, ref string p_strRtnErrMsg)
		{
			int iFor = 0;

			try
			{
				for (iFor = 0; iFor <= modDefApp.g_strWH_CD.GetUpperBound(0); iFor++)
				{
					if (p_strRCV_WH_CD == modDefApp.g_strWH_CD[iFor])
					{
						p_strRTN_WH_TYP = modDefApp.g_strWH_TYP[iFor];
						break; 
					}
				}

				if (string.IsNullOrEmpty(p_strRTN_WH_TYP))
				{
					p_strRtnErrMsg = "정의된 창고코드 미존재";
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				p_strRtnErrMsg = "창고코드 Get실패-" + ex.Message;
				return false;
			}
		}

	}
}
