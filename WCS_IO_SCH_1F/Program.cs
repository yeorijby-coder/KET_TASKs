using System;
using System.Collections.Generic;
using System.Windows.Forms;
//using Utilities;
namespace TSK_COMM_IOSCH
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //ExceptionLogger exceptionLogger;

            //TextFileLogger textfileLogger;

            //exceptionLogger = new ExceptionLogger();

            //textfileLogger = new TextFileLogger();
            //// 내부에서 사용할 Logger로는 TextFileLogger을 지정합니다.

            //exceptionLogger.AddLogger(textfileLogger);



            // 응용 프로그램 실행

            Application.EnableVisualStyles();

            Application.SetCompatibleTextRenderingDefault(false);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SYS_MAIN());

            //Application.Run(new Form1());			

        }
    }
}