using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using log4net.Config;


namespace WCS_TASK_SC
{
    public class Log
    {
        private static log4net.ILog log = null;

        public static log4net.ILog Logger
        {
            get
            {
                return log;
            }
        }

        static Log()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo("CVlog4net.xml"));
            log = LogManager.GetLogger("CvLogger");
        }
        
        public static void Job(string msg)
        {
            log.Info(msg);
        }
        public static void JobFormat(string msg, params object[] args)
        {
            log.InfoFormat(msg, args);
        }
        public static void Event(string msg)
        {
            log.Info(msg);
        }
        public static void EventFormat(string msg, params object[] args)
        {
            log.InfoFormat(msg, args);
        }
        public static void Debug(string msg)
        {
            log.Debug(msg);
        }
        public static void DebugFormat(string msg, params object[] args)
        {
            log.DebugFormat(msg, args);
        }
        public static void Error(string msg)
        {
            log.Error(msg);
        }
        public static void ErrorFormat(string msg, params object[] args)
        {
            log.ErrorFormat(msg, args);
        }
        public static void Alaram(string msg)
        {
            log.Warn(msg);
        }
        public static void AlaramFormat(string msg, params object[] args)
        {
            log.WarnFormat(msg, args);
        }
    }
}
