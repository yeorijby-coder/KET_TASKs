using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TSK_COMM_IOSCH
{
    public class cLogThread
    {
        public CLogging m_gmsLog;
        public Queue<LogParam> m_LogQ = new Queue<LogParam>();
        public Thread m_thThread;
        public int m_nthno;

        public cLogThread(string strPath, string strFileName, int nthno)
        {
            this.m_nthno = nthno;
            m_gmsLog = new CLogging(strPath, strFileName, 20);
        }

        public cLogThread()
        {
        }
        public void LogQueThread()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(100);

                    LogParam Lq;
                    if (m_thThread == null) break;
                    if (cDefApp.m_LogQ[m_nthno].Count <= 0) continue;

                    Lq = (LogParam)cDefApp.m_LogQ[m_nthno].Dequeue();

                    m_gmsLog.Write(Lq.DtCurTime, Lq.StrMessage);
                }
            }
            catch (Exception ex)
            {
                cDefApp.m_LogQ[m_nthno].Clear();
                m_thThread = null;
            }
        }
    }

    public class LogParam
    {
        string m_strMessage;

        public string StrMessage
        {
            get { return m_strMessage; }
            set { m_strMessage = value; }
        }
        DateTime m_dtCurTime;

        public DateTime DtCurTime
        {
            get { return m_dtCurTime; }
            set { m_dtCurTime = value; }
        }

        private cDefApp.eLogMsgType m_eLogMsgTyp;

        public cDefApp.eLogMsgType ELogMsgTyp
        {
            get { return m_eLogMsgTyp; }
            set { m_eLogMsgTyp = value; }
        }

        public LogParam() { }

        public LogParam(DateTime DtCurTime, string strMessage)
        {
            this.DtCurTime = DtCurTime;
            this.StrMessage = strMessage;
        }
    }
}
