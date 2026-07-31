using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WCS_TASK_CV
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

            //INI [COM_SETTING] Setting=1 이면 [COMM0] 기준으로 COMM 섹션 재구성 후 실행
            cComSetting.Apply();

            Application.Run(new SYS_MAIN());
        }
    }
}