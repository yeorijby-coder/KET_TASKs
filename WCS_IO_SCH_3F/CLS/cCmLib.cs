using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Diagnostics;
using System.Windows.Forms;
 
namespace TSK_COMM_IOSCH
{
    class cCmLib
    {
        //@@@.응용 프로그램의 이전 인스턴스가 실행 중인지 여부를 확인
        public static bool GfPrevInstance()
        {
            if (Process.GetCurrentProcess().ProcessName.IndexOf(".vshost") > 0) return false;
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).GetUpperBound(0) > 0)
            {
                return true;
            }
            else
            {
                return false;
            } 
        }

        //@@@.Data Base Connection Open
        public  static bool GfDBLogIn(ref OleDbConnection pConObj,ref string  pMsg) 
        {
            try
            {
                pConObj = new OleDbConnection();
                //pConObj.ConnectionString = "Provider=MSDAORA.1; Data Source = " & _
                //                        gUser.DbAlias & "; User ID = " & _
                //                        gUser.UserID & "; Password = " & _
                //                        gUser.UserPassword
                pConObj.ConnectionString = "Provider="+ 
                            cDefApp.GM_DB_PROVIDER  +"; Data Source = " +
                            cDefApp.GM_DB_ALIAS   + "; User ID = " +
                            cDefApp.GM_DB_USERID   + "; Password = " +
                            cDefApp.GM_DB_PASSWORD ;
                pConObj.Open();
                return true;
            }
            catch ( Exception ex )
            {
                pMsg = ex.Message;
            }
            return false ;
        }

     
    }
}
