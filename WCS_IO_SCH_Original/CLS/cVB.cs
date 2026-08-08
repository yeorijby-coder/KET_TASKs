using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic;

namespace TSK_COMM_IOSCH
{
    class cVB
    {
        #region @@@.[Val]::string -> decimal 변환 리턴
        public static decimal Val(string pVal)
        {
            return Convert.ToInt32(Microsoft.VisualBasic.Conversion.Val(pVal));
        }
        #endregion

        #region @@@.[cDbl]::string -> Double 변환 리턴
        public static double cDbl(string pVal)
        {
            return Convert.ToDouble (Microsoft.VisualBasic.Conversion.Val(pVal));
        }
        #endregion

        #region @@@.[IsNumeric]::숫자형식체크 
        public static bool IsNumeric(object pVal)
        {
            return Microsoft.VisualBasic.Information.IsNumeric(pVal);
        }
        #endregion
    }
}
