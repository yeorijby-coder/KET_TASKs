
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
//**************************************************
//
// Date, Time 관련 정의
//
//**************************************************

namespace TSK_HostCom
{
	public static class modDateTime
	{

#if ORACLE
		public const string SYSDATE = " SYSDATE ";
		public const string SYSDATE_TO_CDT = " TO_CHAR(SYSDATE, 'YYYYMMDD') ";
		public const string SYSDATE_TO_CTM = " TO_CHAR(SYSDATE, 'HH24MISS') ";
		public const string SYSDATETIME_TO_CDTM = " TO_CHAR(SYSDATE, 'YYYY-MM-DD HH24:MI:SS') ";
		public const string SYSDATE_TO_CDTTM = " TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS') ";
#endif
#if SQL   
		public const string SYSDATE = " GETDATE() ";
		public const string SYSDATE_TO_CDT = " CONVERT(varchar,GETDATE(),112) ";
		public const string SYSDATE_TO_CTM = " REPLACE(CONVERT(varchar,GETDATE(),108),':','') ";
		public const string SYSDATETIME_TO_CDTM = " CONVERT(varchar,GETDATE(),120) ";
		public const string SYSDATE_TO_CDTTM = " CONVERT(varchar,GETDATE(),112) + REPLACE(CONVERT(varchar,GETDATE(),108),':','') ";
#endif
#if POSTGRESSQL
        public const string SYSDATE = " NOW() ";
        public const string SYSDATE_TO_CDT = " TO_CHAR(NOW(), 'YYYYMMDD') ";
        public const string SYSDATE_TO_CTM = " TO_CHAR(NOW(), 'HH24MISS') ";
        public const string SYSDATETIME_TO_CDTM = " TO_CHAR(NOW(), 'YYYY-MM-DD HH24:MI:SS') ";
        public const string SYSDATE_TO_CDTTM = " TO_CHAR(NOW(), 'YYYYMMDDHH24MISS') ";
#endif


        public static bool GetSysDate(ref CUserDb p_Bdb)
		{
			//*********************************************************************************
			// DB Date 구하시
			// Parameter: bRerutnErr (에러 발생시, 에러를 Return할 지 여부)
			// Return: 성공, 실패
			string strSql;
			int iSelCnt;

#if ORACLE
			strSql = modDefApp.CRLF + "";
			strSql += modDefApp.CRLF + "   SELECT TO_CHAR(SYSDATE, 'YYYYMMDD') AS GDATE                         ";
			strSql += modDefApp.CRLF + "        , TO_CHAR(SYSDATE, 'HH24MISS') AS GTIME                         ";
			strSql += modDefApp.CRLF + "        , " + SYSDATE + "         AS g_strSYSDATE                  ";
			strSql += modDefApp.CRLF + "     FROM DUAL                                                          ";
#endif
#if SQL
            strSql = modDefApp.CRLF + "";
            strSql += modDefApp.CRLF + "   SELECT CONVERT(varchar,GETDATE(),112)                 AS GDATE       ";
            strSql += modDefApp.CRLF + "        , REPLACE(CONVERT(varchar,GETDATE(),108),':','') AS GTIME       ";
            strSql += modDefApp.CRLF + "        , GETDATE()                                      AS g_strSYSDATE";
#endif
#if POSTGRESSQL
            strSql = modDefApp.CRLF + "";
            strSql += modDefApp.CRLF + "   SELECT TO_CHAR(NOW(), 'YYYYMMDD') AS GDATE                         ";
            strSql += modDefApp.CRLF + "        , TO_CHAR(NOW(), 'HH24MISS') AS GTIME                         ";
            strSql += modDefApp.CRLF + "        , " + SYSDATE + "         AS g_strSYSDATE                  ";
            strSql += modDefApp.CRLF + "     FROM DUAL                                                          ";
#endif
            iSelCnt = p_Bdb.ExcuteQry(strSql);

			return iSelCnt == 1 ? true : false;
		}
		public static void getServerDate(ref CUserDb p_Bdb,
								  DateTimePicker p_dtpDate1,
								  DateTimePicker p_dtpDate2 = null,
								  DateTimePicker p_dtpDate3 = null,
								  DateTimePicker p_dtpDate4 = null)
		{
			if (GetSysDate(ref p_Bdb) == false)
			{
				return;
			}
			//p_dtpDate1.Value = p_Bdb.dtMain.Rows[0]["g_strSYSDATE"];
			//if (p_dtpDate2 != null)
			//{
			//    p_dtpDate2.Value = p_Bdb.dtMain.Rows[0]["g_strSYSDATE"];
			//}
			//if (p_dtpDate3 != null)
			//{
			//    p_dtpDate3.Value = p_Bdb.dtMain.Rows[0]["g_strSYSDATE"];
			//}
			//if (p_dtpDate4 != null)
			//{
			//    p_dtpDate4.Value = p_Bdb.dtMain.Rows[0]["g_strSYSDATE"];
			//}
		}
		public static string GetDateCode(DateTime aDate)
		{
			return aDate.ToString("yyyyMMdd");
		}
		public static string GetDateCode(DateTimePicker dtp)
		{
			return GetDateCode(dtp.Value);
		}
		public static string GetTimeCode(DateTime aTime)
		{
			return aTime.ToString("HHmmss");
		}
		public static string GetTimeCode(DateTimePicker dtp)
		{
			return GetTimeCode(dtp.Value);
		}
		public static string GetDateTimeCode(DateTime aDate, DateTime aTime)
		{
			return GetDateCode(aDate) + GetTimeCode(aTime);
		}
		public static string GetDateTimeCode(DateTimePicker dtpDate, DateTimePicker dtpTime)
		{
			return GetDateCode(dtpDate.Value) + GetTimeCode(dtpTime.Value);
		}
		public static bool IsDateTime(string p_strwkdt,
									  bool p_blchkDate = true,
									  bool p_blchkTime = true)
		{
			string strDt = "";
			string strTm = "";

			if (p_blchkDate == true & p_blchkTime == true)
			{
				if (Strings.Len(p_strwkdt) != 14 & Strings.Len(p_strwkdt) != 19)
				{
					if (Strings.Len(p_strwkdt) == 14)
					{
						strDt = Strings.Left(p_strwkdt, 8);
						strTm = Strings.Right(p_strwkdt, 6);
					}
					else
					{
						strDt = Strings.Left(p_strwkdt, 10);
						strTm = Strings.Right(p_strwkdt, 8);
						strTm = Strings.Mid(strTm, 1, 2) + Strings.Mid(strTm, 4, 2) + Strings.Mid(strTm, 7, 2);
					}
				}
				else
				{
					if (p_blchkDate == true)
					{
						if (Strings.Len(p_strwkdt) != 6 & Strings.Len(p_strwkdt) != 8)
						{
							return false;
						}
						if (Strings.Len(p_strwkdt) == 6)
						{
							strTm = p_strwkdt;
						}
						else
						{
							strTm = Strings.Mid(p_strwkdt, 1, 2) + Strings.Mid(strTm, 4, 2) + Strings.Mid(strTm, 7, 2);
						}
					}
				}
			}
			if (p_blchkDate == true)
			{
				if (Strings.Len(strDt) == 8)
				{
					if (!IsDate(CUserDate(strDt)))
					{
						return false;
					}
				}
				else
				{
					if (!IsDate(strDt))
					{
						return false;
					}
				}
			}
			if (p_blchkTime == true)
			{
				if (!IsNumeric(Strings.Mid(strTm, 1, 2)))
				{
					return false;
				}
				if (!IsNumeric(Strings.Mid(strTm, 3, 2)))
				{
					return false;
				}
				if (!IsNumeric(Strings.Mid(strTm, 5, 2)))
				{
					return false;
				}
				if (Convert.ToInt64(Strings.Mid(strTm, 1, 2)) < 0 || Convert.ToInt64(Strings.Mid(strTm, 1, 2)) > 23)
				{
					return false;
				}
				if (Convert.ToInt64(Strings.Mid(strTm, 3, 2)) < 0 || Convert.ToInt64(Strings.Mid(strTm, 3, 2)) > 59)
				{
					return false;
				}
				if (Convert.ToInt64(Strings.Mid(strTm, 5, 2)) < 0 || Convert.ToInt64(Strings.Mid(strTm, 5, 2)) > 59)
				{
					return false;
				}
			}
			return true;

		}
		public static Boolean IsNumeric(string stringToTest)
		{
			int result;
			return int.TryParse(stringToTest, out result);
		}
		public static bool IsDate(string p_strDate)
		{
			DateTime dt;
			bool isDate = true;

			try
			{
				dt = DateTime.Parse(p_strDate);
			}
			catch
			{
				isDate = false;
			}

			return isDate;
		}
		public static string CUserDate(string strDate, string pSEP_CHAR = "-")
		{
			if (strDate.Length == 8)
			{
				return strDate.Substring(0, 4) + pSEP_CHAR + strDate.Substring(4, 2) + pSEP_CHAR + strDate.Substring(6);
			}
			else
			{
				return "";
			}
		}
	}

}


