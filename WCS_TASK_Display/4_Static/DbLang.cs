using System;
using System.Collections.Generic;
using System.Text;
using NpgsqlTypes;
using System.Data.OleDb;

namespace WCS_TASK_Display
{
    class DbLang
    {

#if ORACLE
        public const string NVL = "NVL";
        public const string SYSDATE = "SYSDATE";
        public const string II = "||";
        public const OleDbType VARCHAR = OleDbType.VarChar;
        public const OleDbType INT = OleDbType.Integer;
        public static string TO_NUMBER(string strVALUE)
        {
            string strRtnValue;
            strRtnValue = "TO_NUMBER(" + strVALUE + ")";
            return strRtnValue;
        }
#elif POSTGRESQL
        public const string NVL = "COALESCE";
        public const string SYSDATE = "NOW()";
        public const string II = "||";
        public const NpgsqlDbType VARCHAR = NpgsqlDbType.Varchar;
        public const NpgsqlDbType INT = NpgsqlDbType.Integer;
        public static string TO_NUMBER(string strVALUE) 
        { 
            string strRtnValue;
            strRtnValue = strVALUE + "::integer";
            return strRtnValue;
        }
#elif SQL
        public const string NVL = "ISNULL";
        public const string SYSDATE = "ISNULL";
        public const string II = "+";
#endif
    }
}
