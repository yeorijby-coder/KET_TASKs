using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace TSK_HostCom
{
	class CLog
	{
		
		// 파일이름
		private string m_strSubDir;
		
		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 로그쓰레드 생성시 초기값 할당
		public CLog(string p_strSubDir)
		{
			m_strSubDir = p_strSubDir;
		
			//----------------------------------
			// SUb Dir이 없으면 생성함
			//----------------------------------
			System.IO.DirectoryInfo diLog = new System.IO.DirectoryInfo(modDefApp.LOG_DIR + m_strSubDir);
			if (!diLog.Exists)
				diLog.Create();
			diLog = null;
		}


		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 로그파일쓰기
		public bool WriteLog(ref LogMsgInfo p_MsgLog)
		{
			System.IO.FileStream fs = default(System.IO.FileStream);
			System.IO.StreamWriter sw = default(System.IO.StreamWriter);
			string strFileName = null;

			strFileName = modDefApp.LOG_DIR + m_strSubDir + Strings.Format(DateTime.Now, "yyyyMMdd") + ".Log";
			if (p_MsgLog.g_strType == modDefApp.MSG_NOR)
			{
				//Return True
			}

			try
			{
				fs = new System.IO.FileStream(strFileName, System.IO.FileMode.Append);
				//sw = New System.IO.StreamWriter(fs)
				sw = new System.IO.StreamWriter(fs, System.Text.Encoding.GetEncoding(949));

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


		//최초작성자	: BASE(이길문)
		//작성일		: 20160829
		//설명		: 로그파일삭제
		public bool DelLog()
		{
			// 제작년 파일 삭제
			string strFileName = null;

			strFileName = modDefApp.LOG_DIR + m_strSubDir + Strings.Format(DateTime.Now.AddYears(-2), "yyyy") + "*.Log";

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

	}
}
