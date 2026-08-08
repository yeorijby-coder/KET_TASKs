using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Diagnostics;
using System.Windows.Forms;
 
namespace WCS_TASK_SC
{
    class cCmLib
    {
        //@@@.응용 프로그램의 이전 인스턴스가 실행 중인지 여부를 확인
        /*
         * GfPrevInstance :: 같은 프로그램이 이미 실행 중인지
         *
         *   예전에는 프로세스 "이름"만 봤다. 그런데 CV 1F/3F/BOX 와 IO_SCH 3형제는
         *   exe 이름이 서로 같아서, 한 층 것이 떠 있으면 다른 층 것이
         *   "이미 실행 중" 으로 오인되어 뜨지 못했다.
         *   실행 파일 경로까지 같은 것만 이전 인스턴스로 본다. (HOST 태스크와 동일)
         */
        public static bool GfPrevInstance()
        {
            Process prcMe = Process.GetCurrentProcess();

            if (prcMe.ProcessName.IndexOf(".vshost") > 0) return false;

            string strMyPath = "";
            try { strMyPath = prcMe.MainModule.FileName; }
            catch { strMyPath = ""; }

            // @.내 경로를 못 얻으면 예전처럼 이름만으로 판단한다.
            if (string.IsNullOrEmpty(strMyPath))
            {
                return (Process.GetProcessesByName(prcMe.ProcessName).GetUpperBound(0) > 0);
            }

            foreach (Process prc in Process.GetProcessesByName(prcMe.ProcessName))
            {
                if (prc.Id == prcMe.Id) continue;

                string strPath = "";
                // @.경로를 못 읽는 프로세스(권한/비트수 차이)는 남의 것으로 본다.
                try { strPath = prc.MainModule.FileName; }
                catch { continue; }

                if (string.Compare(strPath, strMyPath, true) == 0) return true;
            }

            return false;
        }

        //@@@.데이터베이스 접속
        public  static bool GfDBLogIn(ref OleDbConnection pConObj,ref string  pMsg) 
        {
            try
            {
                pConObj = new OleDbConnection();
                //pConObj.ConnectionString = "Provider=MSDAORA.1; Data Source = " & _
                //                        gUser.DbAlias & "; User ID = " & _
                //                        gUser.UserID & "; Password = " & _
                //                        gUser.UserPassword
                //pConObj.ConnectionString = "Provider="+ 
                //            cDefApp.GM_DB_PROVIDER  +"; Data Source = " +
                //            cDefApp.GM_DB_ALIAS   + "; User ID = " +
                //            cDefApp.GM_DB_USERID   + "; Password = " +
                //            cDefApp.GM_DB_PASSWORD ;
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
