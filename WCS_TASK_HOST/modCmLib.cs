using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Data.SqlClient;
using Npgsql;

namespace TSK_HostCom
{
	//*** DB Log In ***********************************************
    // DB 관련 프로젝트는 모두 사용하므로 여기에 두기로 한다.
	class modCmLib
	{
#if ORACLE
		public static bool DBLogIn(ref OleDbConnection p_ConObj)
		{
			ReadInitProfile();

			bool blDbCon = false;
			p_ConObj = new OleDbConnection();

			//-------------------------------------
			// Provider=MSDAORA.1  Login
			//-------------------------------------
			//Try
			//    pConObj.ConnectionString = "Provider=MSDAORA.1; Data Source = " & _
			//                            gUser.DbAlias & "; User ID = " & _
			//                            gUser.UserID & "; Password = " & _
			//                            gUser.UserPassword
			//    pConObj.Open()

			//    bDbCon = True
			//Catch AppErr As Exception
			blDbCon = false;
			//End Try

			//-------------------------------------
			// Provider=OraOLEDB.Oracle.1  Login
			//-------------------------------------
			if (blDbCon == false)
			{
				try
				{
					p_ConObj.ConnectionString = "Provider=OraOLEDB.Oracle.1; Data Source = " 
						+ modDefApp.g_User.g_strDbAlias + "; User ID = "
						+ modDefApp.g_User.g_strUserID + "; Password = " 
						+ modDefApp.g_User.g_strUserPassword;
					p_ConObj.Open();

				}
				catch (Exception AppErr)
				{
					MessageBox.Show(AppErr.Message, "Log In Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}
			return true;
		}

		// 로그인 실패시 백그라운드에서는 ErrMsg 리턴
		public static bool DBLogIn(ref OleDbConnection p_ConObj, ref string p_strErrMsg)
		{
			ReadInitProfile();

			bool blDbCon = false;

			p_ConObj = new OleDbConnection();

			//-------------------------------------
			// Provider=MSDAORA.1  Login
			//-------------------------------------
			//Try
			//    pConObj.ConnectionString = "Provider=MSDAORA.1; Data Source = " & _
			//                            gUser.DbAlias & "; User ID = " & _
			//                            gUser.UserID & "; Password = " & _
			//                            gUser.UserPassword
			//    pConObj.Open()

			//    bDbCon = True
			//Catch AppErr As Exception
			blDbCon = false;
			//End Try

			//-------------------------------------
			// Provider=OraOLEDB.Oracle.1  Login
			//-------------------------------------
			if (blDbCon == false)
			{
				try
				{
					p_ConObj.ConnectionString = "Provider=OraOLEDB.Oracle.1; Data Source = "
						+ modDefApp.g_User.g_strDbAlias + "; User ID = "
						+ modDefApp.g_User.g_strUserID + "; Password = "
						+ modDefApp.g_User.g_strUserPassword;
					p_ConObj.Open();

				}
				catch (Exception AppErr)
				{
					p_strErrMsg = AppErr.Message;
					return false;
				}
			}
			return true;
		}

		public static void ReadInitProfile()
		{
			StringBuilder sb = new StringBuilder(30);
			modDefAPI.GetPrivateProfileString("DB", "SERVICENAME", "", sb, sb.Capacity,modDefApp.MAIN_INI);
			modDefApp.g_User.g_strDbAlias = sb.ToString();

			modDefAPI.GetPrivateProfileString("DB", "USER", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_User.g_strUserID = sb.ToString();

			modDefAPI.GetPrivateProfileString("DB", "PASSWORD", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_User.g_strUserPassword = sb.ToString();

			modDefAPI.GetPrivateProfileString("Network", "LocalPort", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_iListenPort = int.Parse(sb.ToString());

			modDefAPI.GetPrivateProfileString("Network", "RemoteIP", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_strRemoteIP = sb.ToString();

			modDefAPI.GetPrivateProfileString("Network", "RemotePort", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_iRemotePort = int.Parse(sb.ToString());


			//이길문[20161122]이중입고재지정횟수추가
			modDefAPI.GetPrivateProfileString("Property", "RE_DRCT_CNT", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			if (Information.IsNumeric(modDefApp.g_strRE_DRCT_CNT) == false)
			{
				modDefApp.g_strRE_DRCT_CNT = "0";
			}
			else
			{
				modDefApp.g_strRE_DRCT_CNT = sb.ToString();
			}

			if (string.IsNullOrEmpty(modDefApp.g_User.g_strUserID))
				modDefApp.g_User.g_strUserID = "STD_USER";
			if (string.IsNullOrEmpty(modDefApp.g_User.g_strUserPassword))
				modDefApp.g_User.g_strUserPassword = "STD_USER";

		}
#endif
#if SQL
    public static bool DBLogIn(ref SqlConnection p_ConObj)
        {
			ReadInitProfile();

			p_ConObj = new SqlConnection();

                try
                {
					p_ConObj.ConnectionString = "Server = " +
                                               modDefApp.g_User.g_strDbAlias  + "; Trusted_Connection = False; Database = " +
                                               modDefApp.g_User.g_strDatabase   + "; User Id = " +
                                               modDefApp.g_User.g_strUserID  + "; Password = " +
                                               modDefApp.g_User.g_strUserPassword ;
					p_ConObj.Open();

                }
				catch (Exception AppErr)
				{
					MessageBox.Show(AppErr.Message, "Log In Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

            return true;

        }

		// 로그인 실패시 백그라운드에서는 ErrMsg 리턴
	public static bool DBLogIn(ref SqlConnection p_ConObj, ref string p_strErrMsg)
		{
			ReadInitProfile();

			bool blDbCon = false;

			p_ConObj = new SqlConnection();


			blDbCon = false;

			if (blDbCon == false)
			{
				try
				{
					p_ConObj.ConnectionString = "Server = " +
												modDefApp.g_User.g_strDbAlias + "; Trusted_Connection = False; Database = " +
												modDefApp.g_User.g_strDatabase + "; User Id = " +
												modDefApp.g_User.g_strUserID + "; Password = " +
												modDefApp.g_User.g_strUserPassword;

				}
				catch (Exception AppErr)
				{
					p_strErrMsg = AppErr.Message;
					return false;
				}
			}
			return true;
		}

        public static void ReadInitProfile()
        {

            string strKeyData1 = "Q1";
            StringBuilder sb = new StringBuilder(30);

            modDefAPI.GetPrivateProfileString("WMS", "INFO", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            strKeyData1 = sb.ToString();

            modDefAPI.GetPrivateProfileString("Network", "LocalPort", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_iListenPort = int.Parse(sb.ToString());

			modDefAPI.GetPrivateProfileString("Network", "RemoteIP", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_strRemoteIP = sb.ToString();

			modDefAPI.GetPrivateProfileString("Network", "RemotePort", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_iRemotePort = int.Parse(sb.ToString());

            WmsInfo.CWmsInfo.WmsDBInfo(ref modDefApp.g_User.g_strDbAlias, ref modDefApp.g_User.g_strDatabase, ref modDefApp.g_User.g_strUserID, ref modDefApp.g_User.g_strUserPassword, strKeyData1);

            //이중입고재지정횟수추가
            modDefAPI.GetPrivateProfileString("Property", "RE_DRCT_CNT", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			if (Information.IsNumeric(modDefApp.g_strRE_DRCT_CNT) == false)
			{
				modDefApp.g_strRE_DRCT_CNT = "0";
			}
			else
			{
				modDefApp.g_strRE_DRCT_CNT = sb.ToString();
			}

			if (string.IsNullOrEmpty(modDefApp.g_User.g_strUserID))
				modDefApp.g_User.g_strUserID = "STD_USER";
			if (string.IsNullOrEmpty(modDefApp.g_User.g_strUserPassword))
				modDefApp.g_User.g_strUserPassword = "STD_USER";
        }
#endif
#if POSTGRESSQL
        public static bool DBLogIn(ref NpgsqlConnection p_ConObj)
        {
			ReadInitProfile();

            p_ConObj = new NpgsqlConnection();

                try
                {
                    p_ConObj.ConnectionString = "host=" + modDefApp.g_User.g_strDbIP + 
                        ";username=" + modDefApp.g_User.g_strUserID +
                        ";password=" + modDefApp.g_User.g_strUserPassword +
                        ";database=" + modDefApp.g_User.g_strDatabase + 
                        ";MAXPOOLSIZE=50;";

                    p_ConObj.Open();

                }
				catch (Exception AppErr)
				{
					MessageBox.Show(AppErr.Message, "Log In Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

            return true;

        }

		// 로그인 실패시 백그라운드에서는 ErrMsg 리턴
        public static bool DBLogIn(ref NpgsqlConnection p_ConObj, ref string p_strErrMsg)
		{
			ReadInitProfile();

			bool blDbCon = false;

            p_ConObj = new NpgsqlConnection();


			blDbCon = false;

			if (blDbCon == false)
			{
				try
				{
                    //p_ConObj.ConnectionString = "Server = " +
                    //                            modDefApp.g_User.g_strDbAlias + "; Trusted_Connection = False; Database = " +
                    //                            modDefApp.g_User.g_strDatabase + "; User Id = " +
                    //                            modDefApp.g_User.g_strUserID + "; Password = " +
                    //                            modDefApp.g_User.g_strUserPassword;

                    p_ConObj.ConnectionString = "host=" + modDefApp.g_User.g_strDbIP +
                        ";username=" + modDefApp.g_User.g_strUserID +
                        ";password=" + modDefApp.g_User.g_strUserPassword +
                        ";database=" + modDefApp.g_User.g_strDatabase +
                        ";MAXPOOLSIZE=50;";
				}
				catch (Exception AppErr)
				{
					p_strErrMsg = AppErr.Message;
					return false;
				}
			}
			return true;
		}

        public static void ReadInitProfile()
        {

            string strKeyData1 = "Q1";
            StringBuilder sb = new StringBuilder(30);

            modDefAPI.GetPrivateProfileString("DB", "DATABASE", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            modDefApp.g_User.g_strDatabase = sb.ToString();

            modDefAPI.GetPrivateProfileString("DB", "USER", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            modDefApp.g_User.g_strUserID = sb.ToString();

            modDefAPI.GetPrivateProfileString("DB", "USER_PW", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            modDefApp.g_User.g_strUserPassword = sb.ToString();

            modDefAPI.GetPrivateProfileString("DB", "IP", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            modDefApp.g_User.g_strDbIP = sb.ToString();

            modDefAPI.GetPrivateProfileString("DB", "PORT", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            modDefApp.g_User.g_strDbPort = sb.ToString();

            modDefAPI.GetPrivateProfileString("Network", "LocalPort", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            modDefApp.g_iListenPort = int.Parse(sb.ToString());

            modDefAPI.GetPrivateProfileString("Network", "RemoteIP", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            modDefApp.g_strRemoteIP = sb.ToString();

            modDefAPI.GetPrivateProfileString("Network", "RemotePort", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            modDefApp.g_iRemotePort = int.Parse(sb.ToString());

            modDefAPI.GetPrivateProfileString("WMS", "INFO", "", sb, sb.Capacity, modDefApp.MAIN_INI);
            strKeyData1 = sb.ToString();

            modDefAPI.GetPrivateProfileString("Network", "LocalPort", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_iListenPort = int.Parse(sb.ToString());

			modDefAPI.GetPrivateProfileString("Network", "RemoteIP", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_strRemoteIP = sb.ToString();

			modDefAPI.GetPrivateProfileString("Network", "RemotePort", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			modDefApp.g_iRemotePort = int.Parse(sb.ToString());

      //      WmsInfo.CWmsInfo.WmsDBInfo(ref modDefApp.g_User.g_strDbAlias, ref modDefApp.g_User.g_strDatabase, ref modDefApp.g_User.g_strUserID, ref modDefApp.g_User.g_strUserPassword, strKeyData1);

            //이중입고재지정횟수추가
            modDefAPI.GetPrivateProfileString("Property", "RE_DRCT_CNT", "", sb, sb.Capacity, modDefApp.MAIN_INI);
			if (Information.IsNumeric(modDefApp.g_strRE_DRCT_CNT) == false)
			{
				modDefApp.g_strRE_DRCT_CNT = "0";
			}
			else
			{
				modDefApp.g_strRE_DRCT_CNT = sb.ToString();
			}

			if (string.IsNullOrEmpty(modDefApp.g_User.g_strUserID))
				modDefApp.g_User.g_strUserID = "STD_USER";
			if (string.IsNullOrEmpty(modDefApp.g_User.g_strUserPassword))
				modDefApp.g_User.g_strUserPassword = "STD_USER";
        }
#endif


        public static bool WriteLog(ref LogMsgInfo p_MsgLog)
		{
			System.IO.FileStream fs = default(System.IO.FileStream);
			System.IO.StreamWriter sw = default(System.IO.StreamWriter);
			string strFileName = null;

			strFileName = modDefApp.LOG_DIR + Strings.Format(DateTime.Now, "yyyyMMdd") + ".Log";
			if (p_MsgLog.g_strType == modDefApp.MSG_NOR)
			{
				//Return True
			}

			try
			{
				fs = new System.IO.FileStream(strFileName, System.IO.FileMode.Append);
				sw = new System.IO.StreamWriter(fs);

				sw.WriteLine(p_MsgLog.g_strTime + ";" + p_MsgLog.g_strType + ";" + p_MsgLog.g_strMsg);
			}
			catch (Exception ex)
			{
				string strMsg = null;

				strMsg = "Log 파일 오류(" + ex.Message + ")";
				modCmWork.ShowMsgClient(strMsg, modDefApp.MSG_ERR, false);
			}

			if ((sw != null))
			{
				sw.Close();
			}
			if ((fs != null))
			{
				fs.Close();
			}

			return true;
		}

		public static bool DelLog()
		{
			// 제작년 파일 삭제
			string strFileName = null;

			strFileName = modDefApp.LOG_DIR + Strings.Format(DateTime.Now.AddYears(-2), "yyyy") + "*.Log";

			try
			{
				FileSystem.Kill(strFileName);
			}
			catch (Exception ex)
			{
				//Dim strMsg As String

				//strMsg = "Log 파일 삭제 오류(" & ex.Message & ")"
				//ShowMsgClient(strMsg, MSG_ERR, False)
			}
			return true;
		}

		public static object GetFormTitle(ref Form p_Form)
		{
			return p_Form.Text + "   (" + p_Form.Name.Remove(0, 3) + ") ";
		}

		//*** 응용 프로그램의 이전 인스턴스가 실행 중인지 여부를 확인 ***
		public static bool PrevInstance()
		{
			if (Information.UBound(System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName)) > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		//*** ECS 응답 에러 정보 ***
		public static string GetEcsErrInfo(string p_strErrCode)
		{
			switch (p_strErrCode)
			{
			case "01":					return "No STX";
			case "02":					return "No ETX";
			case "03":					return "Luggage No. Duplicated";
			case "04":					return "Not Exist Job";
			case "05":					return "ECS Buffer Full";
			case "06":					return "Invalid Header Length";
			case "07":					return "Invalid Message Length";
			case "08":					return "Invalid Location";
			case "09":					return "Invalid Station No.";
			case "10":					return "Invalid Luggage No.";
			case "11":					return "Invalid Job Define";
			case "12":					return "Inhibited Location";
			case "13":					return "Invalid MailBox Name";
			case "14":					return "Already Invoked Job";
			case "15":					return "Invalid Content";
			case "16":					return "Unknown Message Type";
			case "17":					return "Improper Handshake";
			case "18":					return "No Response from Peer";
			case "19":					return "Socket Error";
			case "99":					return "Internal Error";
			default:					return "미정의 에러코드";
			}
		}


		//### 문자열 길이 얻기(한글2바이트 처리) ###
		public static int LenHan(string p_strChkBuf)
		{
			int iCnt;
			int iLen;
			int iLenHan = 0;
			char cTmp;

			iLen = p_strChkBuf.Length - 1;

			for (iCnt = 0; iCnt <= iLen; iCnt++)
			{
				cTmp = p_strChkBuf[iCnt];

				if (cTmp >= (char)13 && cTmp <= (char)128)
				{
					iLenHan += 1;
				}
				else
				{
					iLenHan += 2;
				}
			}
			return iLenHan;
		}

		public static string CopyHan(string p_strText, int p_iStartPos, int p_iCopyLen)
		{
			byte[] bytTempByte = System.Text.ASCIIEncoding.GetEncoding(949).GetBytes(p_strText);
			//Dim TempByte() As Byte = System.Text.Encoding.UTF8.GetBytes(pText) 
			byte[] bytTempByte2 = new byte[bytTempByte.Length];
			string strResult = "";

			if (p_iStartPos < 1 | p_iCopyLen < 1)
				return strResult;
			if (bytTempByte.Length < p_iStartPos)
				return strResult;

			Array.Reverse(bytTempByte);
			//ReDim Preserve TempByte(TempByte.Length - 1)


			strResult = System.Text.ASCIIEncoding.GetEncoding(949).GetString(bytTempByte);

			bytTempByte2 = new byte[bytTempByte.Length]; //20170920 권혁찬 VB와는 다르게 배열의 길이를 미리 선언해야함.
			Array.Copy(bytTempByte, bytTempByte2, bytTempByte.Length - (p_iStartPos - 1));
			strResult = System.Text.ASCIIEncoding.GetEncoding(949).GetString(bytTempByte2);

			Array.Resize(ref bytTempByte2, (bytTempByte.Length - (p_iStartPos - 1)));
			Array.Reverse(bytTempByte2);
			strResult = System.Text.ASCIIEncoding.GetEncoding(949).GetString(bytTempByte2);

			byte[] bytTempByte3 = new byte[bytTempByte2.Length];

			if (p_iCopyLen > bytTempByte2.Length)
			{
				Array.Copy(bytTempByte2, bytTempByte3, bytTempByte2.Length);
			}
			else
			{
				Array.Copy(bytTempByte2, bytTempByte3, p_iCopyLen);
			}

			strResult = System.Text.ASCIIEncoding.GetEncoding(949).GetString(bytTempByte3);

			return strResult;

			int iLoop = 0;
			int iCopy = 0;
			iLoop = p_iStartPos - 1;
			iCopy = 0;
			while (true)
			{
				bytTempByte2[iCopy] = bytTempByte[iLoop];
				iLoop += 1;
				iCopy += 1;
				if ((iCopy == p_iCopyLen) | (iLoop > bytTempByte.Length - 1))
				{
					break; // TODO: 원본은 Exit While 이었음. 동작이 다를 수 있으니 확인 필요
				}
			}

			//Result = System.Text.Encoding.UTF8.GetString(TempByte2)
			strResult = System.Text.ASCIIEncoding.GetEncoding(949).GetString(bytTempByte2);

			return strResult;
		}

	}
}
