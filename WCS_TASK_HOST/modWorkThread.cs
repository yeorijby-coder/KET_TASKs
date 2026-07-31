using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Data;

namespace TSK_HostCom
{
	public struct LogMsgInfo
	{
		public string g_strTime;
		public string g_strType;
		public string g_strMsg;
	}
	class modWorkThread
	{

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: Listen Socket Thread
		public static void ListenThread()
		{
			string strLog = null;

			modDefApp.g_tcplsn = new TcpListener(System.Net.IPAddress.Any, modDefApp.g_iListenPort);
			modDefApp.g_tcplsn.Start();

			while (modDefApp.g_blListenThread)
			{
				modDefAPI.SleepA(1000);
				if (modDefApp.g_tcplsn.Pending())
				{
					// 이미 연결 되어 있으면 이전 Socket Close
					if ((modDefApp.g_SrvWork.m_sktSock != null) | modDefApp.g_SrvWork.m_blSockConnected)
					{
						modDefApp.g_SrvWork.m_blSockConnected = false;
						modCmWork.CloseSocket(ref modDefApp.g_SrvWork.m_sktSock);
					}

					// 소켓연결
					try
					{
						modDefApp.g_SrvWork.m_sktSock = modDefApp.g_tcplsn.AcceptSocket();
					}
					catch (SocketException se)
					{
						strLog = se.Message + "(" + se.ErrorCode.ToString() + ")";
						modCmWork.ShowMsgServer(strLog, modDefApp.MSG_ERR);
					}
					catch (Exception ex)
					{
						modCmWork.ShowMsgServer(ex.ToString(), modDefApp.MSG_ERR);
					}

					strLog = string.Format("통신이 연결되었습니다..");
					modCmWork.ShowMsgServer(strLog, modDefApp.MSG_IMP);

					// 서버 쓰레드 시작
					modDefApp.g_SrvWork.m_thrThreadObj = new Thread(SrvWorkThread);
					modDefApp.g_SrvWork.m_thrThreadObj.Name = "Server Thread";
					modDefApp.g_SrvWork.m_thrThreadObj.Start();
				}
			}

			modDefApp.g_tcplsn.Stop();

			modCmWork.ShowMsgServer("Socket Listener 종료.");

            // Log 쓰레드 종료
			modDefApp.g_frmForm.LogThreadEnd();
		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 서버 작업 Thread
		public static void SrvWorkThread()
		{
			string strLog = null;
			bool blResult = false;
			int iBodyLen = 0;
			string strErrMsg = null;

			modDefApp.g_blSrvThread = true;
			// 설비상태가 연속으로 많은량 수신 될 떄 SKIP을 위한 처리
			modDefApp.g_SrvWork.m_tmSCMD_RecvTime = DateTime.Now.AddSeconds(-10);

			modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picSrvCom, modDefApp.ComSts.ComNor);
            // DB 연결,연결이 끊어지면 소켓도 Close한다. 자동 재접속은 무한루프 및 부하증가

            if ((modDefApp.g_SrvWork.m_BDb.conMain != null))
            {
                modDefApp.g_SrvWork.m_BDb.conMain.Close();
                modDefAPI.SleepA(3000);
            }

            modCmWork.ShowMsgServer("DB 로그인 중...", modDefApp.MSG_IMP);

            if (!modCmLib.DBLogIn(ref modDefApp.g_SrvWork.m_BDb.conMain, ref strErrMsg))
            {
                modDefApp.g_SrvWork.m_blDbConnted = false;
                strLog = string.Format("DB 연결 실패-{0}", strErrMsg);
                modCmWork.ShowMsgServer(strLog, modDefApp.MSG_ERR);
            }
            else
            {
                modDefApp.g_SrvWork.m_blDbConnted = true;
                modCmWork.ShowMsgServer("DB 로그인 성공.", modDefApp.MSG_IMP);
                modDefApp.g_SrvWork.m_BDb.Init();
            }

            //While SrvWork.DbConnted
            while (modDefApp.g_blSrvThread)
			{
                try
                {
                    blResult = modDefApp.g_SrvWork.ReadRequest(ref iBodyLen);
                    if (!blResult)
                    {
                        break; // TODO: might not be correct. Was : Exit While
                    }

                    modDefApp.g_SrvWork.Parsing(iBodyLen);
                    // Response
                    modDefApp.g_SrvWork.SendSock();

#if ORACLE
				    //--LKM[20140430]DB연결상태체크
				    if (!string.IsNullOrEmpty(modDefApp.g_CliWork.m_BDb.ErrMsg) || modDefApp.g_CliWork.m_BDb.conMain.State == ConnectionState.Closed)
				    {
					    modDefApp.g_SrvWork.m_blDbConnted = false;
                        modDefApp.g_SrvWork.m_BDb.ErrMsg = "";
                        break; // TODO: might not be correct. Was : Exit While
				    }
				    else
				    {
					    modDefApp.g_SrvWork.m_blDbConnted = true;
				    }
				    //==LKM[20140430]DB연결상태체크
#endif
#if SQL
				    // DB연결상태체크
				    if (!string.IsNullOrEmpty(modDefApp.g_CliWork.m_BDb.ErrMsg))
				    {
					    modDefApp.g_SrvWork.m_blDbConnted = false;
                        modDefApp.g_SrvWork.m_BDb.ErrMsg = "";
                        break; // TODO: might not be correct. Was : Exit While
				    }
				    else
				    {
					    modDefApp.g_SrvWork.m_blDbConnted = true;
				    }
				    // DB연결상태체크
#endif
#if POSTGRESSQL
                    // DB연결상태체크
                    if (!string.IsNullOrEmpty(modDefApp.g_CliWork.m_BDb.ErrMsg))
                    {
                        modDefApp.g_SrvWork.m_blDbConnted = false;
                        modDefApp.g_SrvWork.m_BDb.ErrMsg = "";
                        break; // TODO: might not be correct. Was : Exit While
                    }
                    else
                    {
                        modDefApp.g_SrvWork.m_blDbConnted = true;
                    }
                    // DB연결상태체크
#endif
                }
                catch (SocketException se)
                {
                    //strLog = se.Message & "(" & se.ErrorCode.ToString & ")"
                    strLog = "리모트 시스템과 연결 실패 !" + "(" + se.ErrorCode.ToString() + ")";
                    modCmWork.ShowMsgServer(strLog, modDefApp.MSG_IMP);
                    break; // TODO: might not be correct. Was : Exit While

                }
                catch (Exception ex)
                {
                    modCmWork.ShowMsgServer(ex.ToString(), modDefApp.MSG_ERR);
                    break; // TODO: might not be correct. Was : Exit While
                }
                

			}

			if ((modDefApp.g_SrvWork.m_BDb.conMain != null))
			{
				// DB Close
				modDefApp.g_SrvWork.m_BDb.conMain.Close();
			}

			modDefApp.g_SrvWork.m_blSockConnected = false;
			modCmWork.CloseSocket(ref modDefApp.g_SrvWork.m_sktSock);

			// Remote 에서 Close 할 시( Remote Close), return 0 시 로그
			if (!blResult)
			{
				strLog = "리모트 시스템과 연결을 종료합니다.";
				modCmWork.ShowMsgServer(strLog, modDefApp.MSG_IMP);
			}

			strLog = "통신 쓰레드를 종료합니다.";
			modCmWork.ShowMsgServer(strLog, modDefApp.MSG_IMP);
			//소켓 연결종료표시
			modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picSrvCom, modDefApp.ComSts.ComErr);

            //--LKM[20140509]프로그램종료시 서버쓰레드종료 후 리슨쓰레드종료
            if (modDefApp.g_blSrvThread == false)
            {
                modDefApp.g_blListenThread = false;
            }
        }

        //최초작성자	: BASE(이길문)
        //작성일		: 20160829
        //설명		: 로그 Thread
        public static void LogThread()
		{
			LogMsgInfo LogMsg = default(LogMsgInfo);
			int iDelCnt = 0;
			int iID = 0;
			CLog Log = default(CLog);
			LogMsgInfo LogMsg1, LogMsg2;

			//----------------------------------
			// Log Dir이 없으면 생성함
			//----------------------------------
			System.IO.DirectoryInfo diLog = new System.IO.DirectoryInfo(modDefApp.LOG_DIR);
			if (!diLog.Exists)
				diLog.Create();
			diLog = null;

			iID = Convert.ToInt32(Thread.CurrentThread.Name.Substring(0, 3));
			if (iID == 0)
			{
				Log = new CLog("Cli\\\\");
			}
			else
			{
				Log = new CLog("Srv\\\\");
			}


			while (!modDefApp.g_areLogExitEvent[iID].WaitOne(1000, false))
			{
				while ((modDefApp.g_arrlstLogList[iID].Count > 0))
				{
					//20170919 권혁찬 Object형식으로는 파라메터를 넘길 수 없어 변수에 캐스팅 후 값을 넘김.
					LogMsg1 = (LogMsgInfo)modDefApp.g_arrlstLogList[iID][0];
					Log.WriteLog(ref LogMsg1);
					modDefApp.g_arrlstLogList[iID].RemoveAt(0);
				}

				//삭제
				iDelCnt += 1;
				if (iDelCnt > 3600)
				{
					modCmLib.DelLog();
					iDelCnt = 0;
				}

			}

			if (iID == 0)
			{
				modCmWork.ShowMsgClient("Write Log Thread 종료.");
			}
			else
			{
				modCmWork.ShowMsgServer("Write Log Thread 종료.");
			}

			//이전로그 기록
			while ((modDefApp.g_arrlstLogList[iID].Count > 0))
			{
				//20170919 권혁찬 Object형식으로는 파라메터를 넘길 수 없어 변수에 캐스팅 후 값을 넘김.
				LogMsg2 = (LogMsgInfo)modDefApp.g_arrlstLogList[iID][0];
				Log.WriteLog(ref LogMsg2);
				modDefApp.g_arrlstLogList[iID].RemoveAt(0);
			}

		}

		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 클라이언트 작업 Thread
		public static void CliWorkThread()
		{
			string strLog = null;
			int iCnt = 0;
			string strErrMsg = null;

			while (!modDefApp.g_CliWork.m_areCliExitEvent.WaitOne(1000, false))
			{
				try
				{
					//--LKM[20140430]DB연결
					if (modDefApp.g_CliWork.m_blDbConnted == false)
					{
						modCmWork.ShowMsgClient("DB 로그인 중...", modDefApp.MSG_IMP);

						if (!modCmLib.DBLogIn(ref modDefApp.g_CliWork.m_BDb.conMain, ref strErrMsg))
						{
							modDefApp.g_CliWork.m_blDbConnted = false;
							strLog = string.Format("DB 연결 실패-{0}", strErrMsg);
							modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
							modDefAPI.SleepA(3000);
                            modDefApp.g_CliWork.m_bFetchSimMode = false;
                            continue;
						}
						else
						{
							modDefApp.g_CliWork.m_blDbConnted = true;
							modCmWork.ShowMsgClient("DB 로그인 성공.", modDefApp.MSG_IMP);
							modDefApp.g_CliWork.m_BDb.Init();
                            modDefApp.g_CliWork.m_bFetchSimMode = true;
						}
					}
					//==LKM[20140430]DB연결

                    if (!modDefApp.g_CliWork.m_blSockConnected && modDefApp.g_frmForm.chkSimMode.Checked == false)
					{
						iCnt += 1;
						// 3초
						if (iCnt == 3)      // 15
						{
							iCnt = 0;
							modDefApp.g_CliWork.ConnectSock();
						}
					}

                    string strSql = "";
                    #region 시뮬레이션 모드인지 확인해서 가져오기 - 사용안함
                    /*
                    if (modDefApp.g_CliWork.m_bFetchSimMode == true)
                    {
                        modDefApp.g_CliWork.m_BDb.ParamsClear();

                        //string strLog = "";
                        strSql = modDefApp.CRLF + "   SELECT  SIM_MODE";
                        strSql += modDefApp.CRLF + "    FROM  HOST_IF_LOG          ";
                        strSql += modDefApp.CRLF + "   WHERE  WH_TYP = '" + modDefApp.WH_TYP + "'";
                        strSql += modDefApp.CRLF + "ORDER BY  INS_DT DESC     LIMIT 1";

                        int iiCnt = modDefApp.g_CliWork.m_BDb.ExcuteQry_Par(ref strSql);

                        // 여기서는 DB가 에러가 나도 일단 시뮬레이터 모드가 아닌것으로 체크박스를 만들것 
                        if (iiCnt < 0)
                        {
                            strLog = modDefApp.g_CliWork.m_BDb.ErrMsg + strSql;
                            modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
                            //MessageBox.Show(strLog);
                            modDefApp.g_frmForm.chkSimMode.Checked = false;
                            //return;
                        }

                        if (iiCnt == 0)
                        {
                            strLog = "시뮬레이션 모드 정보를 가져오지 못했습니다.";
                            modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
                            //MessageBox.Show(strLog);
                            modDefApp.g_frmForm.chkSimMode.Checked = false;
                            //return;
                        }

                        // 디버깅을 위해서 변수를 생성함!
                        int nSimMode = Convert.ToInt32(modDefApp.g_CliWork.m_BDb.dtMain.Rows[0]["SIM_MODE"].ToString());
                        bool bSimMode = Convert.ToBoolean(nSimMode);
                        modDefApp.g_frmForm.chkSimMode.Checked = bSimMode;
                        modDefApp.g_CliWork.m_bFetchSimMode = false;
                    }
                    //*/
                    #endregion

                    #region 현재 작업이 존재하는지 체크해서 작업이 없을때만 시뮬레이션 모드 활성화 - 사용안함
                    /*
                    modDefApp.g_CliWork.m_BDb.ParamsClear();

                    //string strLog = "";
                    //string strSql = "";
                    strSql = modDefApp.CRLF + "   SELECT  *";
                    strSql += modDefApp.CRLF + "    FROM  JOB_MST          ";
                    strSql += modDefApp.CRLF + "   WHERE  WH_TYP = '" + modDefApp.WH_TYP + "'";         

                    int iSqlCnt = modDefApp.g_CliWork.m_BDb.ExcuteQry_Par(ref strSql);

                    //// 쿼리가 정상 실행 되고 작업이 0개 일때만 시뮬레이션 모드 실행 가능 (DB Error 시와 작업갯수가 있을때는 시뮬레이션 모드 실행 불가)
                    //if (iSqlCnt < 0)
                    //{
                    //    strLog = modDefApp.g_CliWork.m_BDb.ErrMsg + strSql;
                    //    //modCmWork.ShowMsgClient(strLog, modDefApp.MSG_ERR);
                    //    return false;
                    //}

                    //if (iSqlCnt == 0)
                    //{
                    //    modDefApp.g_frmForm.chkSimMode.Enabled = true;
                    //}
                    //else
                    //{
                    //    modDefApp.g_frmForm.chkSimMode.Enabled = false;
                    //}
                    //*/
                    #endregion

					if (modDefApp.g_blSTOP_REQ == false)
					{
						modDefApp.g_CliWork.GetSendData();
					}


#if ORACLE
					//--LKM[20140430]DB연결상태체크
					if (!string.IsNullOrEmpty(modDefApp.g_CliWork.m_BDb.ErrMsg) || modDefApp.g_CliWork.m_BDb.conMain.State == ConnectionState.Closed)
					{
						modDefApp.g_CliWork.m_blDbConnted = false;
					}
					else
					{
						modDefApp.g_CliWork.m_blDbConnted = true;
					}
					//==LKM[20140430]DB연결상태체크
#endif
#if SQL
					// DB연결상태체크
					if (!string.IsNullOrEmpty(modDefApp.g_CliWork.m_BDb.ErrMsg))
					{
						modDefApp.g_CliWork.m_blDbConnted = false;
					}
					else
					{
						modDefApp.g_CliWork.m_blDbConnted = true;
					}
					// DB연결상태체크
#endif
#if POSTGRESSQL
                    // DB연결상태체크
					if (!string.IsNullOrEmpty(modDefApp.g_CliWork.m_BDb.ErrMsg))
					{
						modDefApp.g_CliWork.m_blDbConnted = false;
					}
					else
					{
						modDefApp.g_CliWork.m_blDbConnted = true;
					}
					// DB연결상태체크
#endif


                }
				catch (Exception ex)
				{
					modCmWork.ShowMsgClient(ex.Message, modDefApp.MSG_ERR);
					modDefApp.g_CliWork.m_blDbConnted = false;
				}

				//--LKM[20140430]DB연결해제, 통신종료
				if (modDefApp.g_CliWork.m_blDbConnted == false)
				{
					modDefApp.g_CliWork.m_BDb.ErrMsg = "";
					modCmWork.ShowMsgClient("이상 발생.DB Logout...", modDefApp.MSG_IMP);
					if ((modDefApp.g_CliWork.m_BDb.conMain != null))
					{
						// DB Close
						modDefApp.g_CliWork.m_BDb.conMain.Close();
					}

					modDefApp.g_CliWork.m_blSockConnected = false;
					modCmWork.CloseSocket(ref modDefApp.g_CliWork.m_sktSock);

					strLog = "통신 쓰레드를 종료합니다.";
					modCmWork.ShowMsgClient(strLog, modDefApp.MSG_IMP);
					//소켓 연결종료표시
					modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picCliCom, modDefApp.ComSts.ComErr);

					modDefAPI.SleepA(3000);
				}
				// DB연결해제, 통신종료
			}

			modDefApp.g_CliWork.m_blSockConnected = false;
			modCmWork.CloseSocket(ref modDefApp.g_CliWork.m_sktSock);

			strLog = "통신 쓰레드를 종료합니다.";
			modCmWork.ShowMsgClient(strLog, modDefApp.MSG_IMP);
			//소켓 연결종료표시
			modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picCliCom, modDefApp.ComSts.ComErr);
		}
	}
}
