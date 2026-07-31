using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using NpgsqlTypes;

namespace TSK_COMM_IOSCH
{
    class DbLang
    {
#if ORACLE
        public const string NVL = "NVL";
        public const string SYSDATE = "SYSDATE";
		public const string SYSDATE_TO_CDT = " TO_CHAR(SYSDATE, 'YYYYMMDD') ";
		public const string SYSDATE_TO_CTM = " TO_CHAR(SYSDATE, 'HH24MISS') ";
		public const string SYSDATETIME_TO_CDTM = " TO_CHAR(SYSDATE, 'YYYY-MM-DD HH24:MI:SS') ";
		public const string SYSDATE_TO_CDTTM = " TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS') ";
        public const string II = "||"; //ii 대문자로
        public const OleDbType VARCHAR = OleDbType.VarChar;
        public const OleDbType INTEGER = OleDbType.Integer;
        public const OleDbType DATE = OleDbType.Date;
        public const OleDbType DECIMAL = OleDbType.Decimal;
        public static string TO_NUMBER(string strVALUE) 
        { 
            string strRtnValue;
            strRtnValue = "TO_NUMBER(" + strVALUE + ")";
            return strRtnValue;
        }
        public static string NEXTVAL(string strVALUE)
        {
            string strRtnValue;
            strRtnValue = "NEXTVAL." + strVALUE;
            return strRtnValue;
        }
#endif
#if POSTGRESQL
        public const string NVL = "COALESCE";
        public const string SYSDATE = "NOW()";
        public const string SYSDATE_TO_CDT = " TO_CHAR(NOW(), 'YYYYMMDD') ";
        public const string SYSDATE_TO_CTM = " TO_CHAR(NOW(), 'HH24MISS') ";
        public const string SYSDATETIME_TO_CDTM = " TO_CHAR(NOW(), 'YYYY-MM-DD HH24:MI:SS') ";
        public const string SYSDATE_TO_CDTTM = " TO_CHAR(NOW(), 'YYYYMMDDHH24MISS') ";
        public const string II = "||"; //ii 대문자로
        public const NpgsqlDbType VARCHAR = NpgsqlDbType.Varchar;
        public const NpgsqlDbType INTEGER = NpgsqlDbType.Integer;
        public const NpgsqlDbType DATE = NpgsqlDbType.Timestamp;
        public static string TO_NUMBER(string strVALUE) 
        { 
            string strRtnValue;
            strRtnValue = strVALUE + "::integer";
            return strRtnValue;
        }
        public static string NEXTVAL(string strVALUE)
        {
            string strRtnValue;
            strRtnValue = "NEXTVAL('" + strVALUE + "')";
            return strRtnValue;
        }
        
#endif
#if SQL
        public const string NVL = "ISNULL";
        public const string SYSDATE = "GETDATE()";
        public const string SYSDATE_TO_CDT = " CONVERT(VARCHAR(8), GETDATE(), 112) ";
        public const string SYSDATE_TO_CTM = " REPLACE(CONVERT(VARCHAR(8), GETDATE(), 108), ':', '') ";
        public const string SYSDATETIME_TO_CDTM = " CONVERT(VARCHAR(19), GETDATE(), 120) ";
        public const string SYSDATE_TO_CDTTM = " REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR(19), GETDATE(), 120),'-',''),':',''),' ','') ";
        public const string II = "+";
        public const System.Data.SqlDbType VARCHAR = System.Data.SqlDbType.VarChar;
        public const System.Data.SqlDbType INTEGER = System.Data.SqlDbType.Int;
        public const System.Data.SqlDbType DATE = System.Data.SqlDbType.DateTime;
        public static string TO_NUMBER(string strVALUE)
        {
            return "CAST(" + strVALUE + " AS INT)";
        }
        public static string NEXTVAL(string strVALUE)
        {
            return "NEXT VALUE FOR " + strVALUE;
        }
#endif
    }
}
