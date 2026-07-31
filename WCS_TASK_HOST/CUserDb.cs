
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

#if ORACLE
using System.Data.OleDb;
#endif
#if SQL
using System.Data.SqlClient;
#endif
#if POSTGRESSQL
using Npgsql;
#endif

namespace TSK_HostCom
{
	//*** 프로젝트 정의 DB 클래스 **************************************************************************
	//* CBaseDb 에서 상속받아 프로젝트 별로 메세지 함수 등을 재정의 하여 사용한다.
	//* (메세지 정의는 프로젝트 별로 틀릴 수가 있음)
	public class CUserDb : CBaseDb
	{

		private int _excuteQry_Par;
		// 일반적인 PC Client에서 Global Connection 객체 1개 사용 시
		public CUserDb(bool bBind = false)
			: base(ref modDefApp.g_conDb, bBind)
		{
		}

		// CBaseDb마다 Connection을 별도로 가져가야 할 경우, PDA Server
		// Backgroud Process에서 CBaseDb의 Connection객체를 사용 할 경우
		// strDummy는 Overload 함수 정의 시 구분하기 위한 파라미터, 즉 의미가 없다.
		// ex) Public BDb As New CUserDb("Multi", False)
		public CUserDb(string strDummy, bool bBind = false)
			: base(bBind)
		{
			//Backgroud Process에서 CBaseDb의 Connection객체를 사용 할 경우(' 종료시 comMain.Close를 반드시 호출)
		}

		// Connection이 2개 이상일 경우 (지정), 지정된 갯수만 필요할 때
#if ORACLE
		public CUserDb(OleDbConnection pCon, bool bBind = false)
			: base(ref pCon, bBind)
		{
		}
#endif
#if SQL
	public CUserDb(SqlConnection pCon, bool bBind = false) : base(ref pCon, bBind)
	{
	}
#endif
#if POSTGRESSQL
    public CUserDb(NpgsqlConnection pCon, bool bBind = false) : base(ref pCon, bBind)
	{
	}
#endif

        //메세지 보이기 재정의 하여 사용 할 것
		public override void ShowErrMsg(bool bMsgBox, string strERR = "DB")
		{
			string strMsg = null;

			// 메세지 바꾸기
			switch (Convert.ToInt16(ErrKind))
			{
				case DB_LOCK:
					strMsg = "잠시 후 다시 사용하세요. (DB Lock.)";
					break;
				case DB_DUP:
					strMsg = "이미 등록된 자료입니다. (DB 중복)";
					break;
				default:
					strMsg = ErrMsg;
					break;
			}

			// PDA 및 WMS 서버 프로그램을 위한 Compile Option

			/*
			#if SERVER_PROGRAM
			#if PROGRAM_NM == "CMPPROC" Or PROGRAM_NM = "PA_SCH" Or PROGRAM_NM = "PK_SCH" Or PROGRAM_NM = "ECSCOMA" Or PROGRAM_NM = "ECSCOMC" Or PROGRAM_NM = "ERPCOM"
			if (bMsgBox) {
				ShowMsgClient(strMsg, MSG_ERR);
			}
			#else
			throw new Exception(strMsg);
			#endif
			#else
			*/
			// 메세지박스 보이기를 선택 한 경우
			if (bMsgBox)
			{
				////If strMsg.IndexOf("ORA-03113") <> -1 OrElse _
				////   strMsg.IndexOf("ORA-03114") <> -1 OrElse _
				////   strMsg.IndexOf("ORA-12560") <> -1 OrElse _
				////   strMsg.IndexOf("ORA-12541") <> -1 Then
				////    ShowMsg("DB연결이 종료된 상태 입니다" & CRLF & _
				////            "프로그램을 종료 후 다시 실행 하시오." & CRLF & _
				////            "[" & strMsg & "]", "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
				////Else
				////    ShowMsg(strMsg, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
				////End If


				//MSG errMsgBox = new MSG();

				//if (strERR == "APP")
				//{
				//    errMsgBox.m_strTitle = "APP Error";
				//}
				//else
				//{
				//    errMsgBox.m_strTitle = "DB Error";
				//}

				//errMsgBox.ChkTime.Visible = true;
				//errMsgBox.ChkTime.Checked = false;
				//errMsgBox.m_blReStart = true;

				//errMsgBox.m_strMsg = strMsg + modDef.CRLF + "잠시 후 작업을 다시 처리 하세요.";
				////If strERR = "APP" Then
				////    errMsgBox.strMsg = strMsg & CRLF & _
				////                       "잠시 후 작업을 다시 처리 하세요."
				////Else
				////    errMsgBox.strMsg = strMsg & CRLF & _
				////                       "----- SQL 문장 -----" & CRLF & _
				////                       comMain.CommandText
				////End If
				//errMsgBox.ShowDialog();
			}
			modSpDb.gLast_Job_Time = DateTime.Now;
			modSpDb.gLast_Job_Start = false;
			//#endif

		}

		public override void ShowErrMsg_Call(string parCALL, bool bMsgBox, string strERR = "DB")
		{
			string strMsg = null;

			// 메세지 바꾸기
			switch (Convert.ToInt16(ErrKind))
			{
				case DB_LOCK:
					strMsg = "잠시 후 다시 사용하세요. (DB Lock.)";
					break;
				case DB_DUP:
					strMsg = "이미 등록된 자료입니다. (DB 중복)";
					break;
				default:
					strMsg = ErrMsg;
					break;
			}

			// PDA 및 WMS 서버 프로그램을 위한 Compile Option

			/*
			#if SERVER_PROGRAM
			#if PROGRAM_NM = "CMPPROC" Or PROGRAM_NM = "IN_SCH" Or PROGRAM_NM = "OUT_SCH" or PROGRAM_NM = "ECSCOMA" Or PROGRAM_NM = "ECSCOMC" Or PROGRAM_NM = "ERPCOM"
			#if PROGRAM_NM = "ECSCOMA" Or PROGRAM_NM = "ECSCOMC"
			if (bMsgBox) {
				if (parCALL.ToUpper == "CLIENT") {
					ShowMsgClient(strMsg, MSG_ERR);
				} else {
					ShowMsgServer(strMsg, MSG_ERR);
				}
			}
			#else
			if (bMsgBox) {
				ShowMsgClient(strMsg, MSG_ERR);
			}
			#endif
			#else
			throw new Exception(strMsg);
			#endif
			#else
			 */
			// 메세지박스 보이기를 선택 한 경우
			//if (bMsgBox)
			//{
			//    //If strMsg.IndexOf("ORA-03113") <> -1 OrElse _
			//    //   strMsg.IndexOf("ORA-03114") <> -1 OrElse _
			//    //   strMsg.IndexOf("ORA-12560") <> -1 OrElse _
			//    //   strMsg.IndexOf("ORA-12541") <> -1 Then
			//    //    ShowMsg("DB연결이 종료된 상태 입니다" & CRLF & _
			//    //            "프로그램을 종료 후 다시 실행 하시오." & CRLF & _
			//    //            "[" & strMsg & "]", "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
			//    //Else
			//    //    ShowMsg(strMsg, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
			//    //End If
			//    MSG errMsgBox = new MSG();

			//    if (strERR == "APP")
			//    {
			//        errMsgBox.m_strTitle = "APP Error";
			//    }
			//    else
			//    {
			//        errMsgBox.m_strTitle = "DB Error";
			//    }

			//    errMsgBox.ChkTime.Visible = true;
			//    errMsgBox.ChkTime.Checked = false;
			//    errMsgBox.m_blReStart = true;

			//    if (strERR == "APP")
			//    {
			//        errMsgBox.m_strMsg = strMsg + modDef.CRLF + "잠시 후 작업을 다시 처리 하세요.";
			//    }
			//    else
			//    {
			//        errMsgBox.m_strMsg = strMsg + modDef.CRLF + "----- SQL 문장 -----" + modDef.CRLF + comMain.CommandText;
			//    }
			//    errMsgBox.ShowDialog();
			//}
			modSpDb.gLast_Job_Time = DateTime.Now;
			modSpDb.gLast_Job_Start = false;
			//#endif

		}

	}
}

