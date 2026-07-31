using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WCS_TASK_SC
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //[COM_SETTING] Setting=1 이면 [COMM0] 기준으로 [COMM?] 섹션을 재구성한 후 프로그램을 시작한다.
            string strRtnMsg = "";
            if (!cDefApi.GsComSettingInit(ref strRtnMsg))
            {
                MessageBox.Show(strRtnMsg, "COM_SETTING 오류");
                return;
            }

            Application.Run(new SYS_MAIN());
        }
    }
}