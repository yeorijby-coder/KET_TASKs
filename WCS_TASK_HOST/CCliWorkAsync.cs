using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TSK_HostCom
{
    /*
     * CCliWork 의 비동기 송신 (2026-11-15)
     *
     *   예전에는 보고 한 건을 보내고 ACK 가 올 때까지(최대 TIME_OUT 5초) CliWorkThread 가 그 자리에서 기다렸다.
     *   그동안 다른 보고도, DB 조회도 모두 멈췄다.
     *
     *   지금은 이렇게 나눈다.
     *     CliWorkThread (DB 조회 / 보고 만들기)  -> QueueSrv()  -> [보낼 큐]
     *     송신 스레드(SenderLoop)                 : 큐에서 꺼내 보내고, 소켓에서 ACK 를 읽어 [끝난 목록]에 올린다.
     *     CliWorkThread 가 매 바퀴 처음에 ProcessSendDone() 으로 [끝난 목록]을 처리한다.
     *
     *   DB 는 CliWorkThread 만 쓴다. (m_BDb 는 한 연결이라 스레드 둘이 같이 쓰면 안 된다)
     *   그래서 ACK 뒤에 하던 일(작업 삭제, 보냈다는 표시)은 콜백으로 넘기고, ProcessSendDone() 안에서 CliWorkThread 가 실행한다.
     *   인터페이스 로그(HOST_IF_LOG) 기록도 같은 곳에서 한다.
     *
     *   WMS 와의 규칙(한 건 보내고 ACK 확인)은 그대로다. 동시에 응답을 기다리는 건수는 ECSCOMA.ini 의
     *     [Property]
     *     SEND_WINDOW=1      ; 기본 1 = 한 건 보내고 ACK 를 받은 뒤 다음 건 (예전과 같은 규칙)
     *   로 정한다. 1 보다 크게 하려면 상위(WMS)가 응답 전에 다음 전문을 받는지 확인한 뒤에 올린다.
     *   (한 보고 안의 작업 선택은 그대로라, 같은 작업의 보고는 ACK 가 오기 전에는 다시 나가지 않는다)
     *
     *   응답이 TIME_OUT 안에 안 오거나 응답 프레임이 틀리면 예전처럼 소켓을 닫는다.
     *   그 위에서 기다리던 보고는 모두 실패(콜백 false)로 끝나고 다시 접속한 뒤 CliWorkThread 가 다시 만든다.
     */
    partial class CCliWork
    {
        private sealed class SendItem
        {
            public byte[] Frame;
            public string Cmd;
            public string Key;
            public string TxText;
            public string RxText;
            public Action<bool> OnDone;
            public DateTime SentAt;
            public bool Sent;
            public bool Ok;
        }

        private readonly object m_objSendLock = new object();
        private readonly List<SendItem> m_lstSendQueue = new List<SendItem>();   // 아직 안 보낸 것
        private readonly List<SendItem> m_lstSendWait = new List<SendItem>();    // 보냈고 ACK 를 기다리는 것
        private readonly List<SendItem> m_lstSendDone = new List<SendItem>();    // 끝난 것 (CliWorkThread 가 처리)
        private readonly HashSet<string> m_setSendKey = new HashSet<string>();   // 큐 / 대기 / 끝난 목록에 있는 전문 (같은 전문 중복 방지)

        private Thread m_thrSender;
        private int m_nSendWindow = 1;

        // 상태 보고(S) 한 묶음이 아직 안 끝났다. 끝나기 전에는 다시 만들지 않는다.
        private bool m_bStatusInFlight = false;

        /*
         * QueueSrv :: m_bytTxBuff(헤더+STX+본문+ETX) 를 보낼 큐에 넣는다. 보내고 ACK 를 기다리지 않는다.
         *   onDone : ACK 를 받았으면 true, 못 받았거나 연결이 끊겼으면 false. CliWorkThread 에서 불린다. 없으면 null.
         *   bDup   : 똑같은 전문이 이미 가는 중이라 새로 넣지 않았다. (그 전문의 콜백이 일을 한다. 이때 onDone 은 불리지 않는다)
         *   반환   : 연결이 끊겨 넣지 못했으면 false.
         */
        private bool QueueSrv(Action<bool> onDone, out bool bDup)
        {
            bDup = false;
            if (!m_blSockConnected || m_sktSock == null || m_bytTxBuff == null)
                return false;

            byte[] bytFrame = (byte[])m_bytTxBuff.Clone();
            string strCmd = m_strHostCmd;
            string strText = Encoding.Default.GetString(bytFrame);
            string strKey = strCmd + "|" + strText;

            lock (m_objSendLock)
            {
                if (m_setSendKey.Contains(strKey))
                {
                    bDup = true;
                    return true;
                }
                m_setSendKey.Add(strKey);
                m_lstSendQueue.Add(new SendItem { Frame = bytFrame, Cmd = strCmd, Key = strKey, TxText = strText, OnDone = onDone });
            }

            EnsureSender();
            return true;
        }

        // 본문(STX/ETX 안쪽)을 헤더+STX+ETX 로 감싸 큐에 넣는다. (SendBody 의 비동기판)
        private bool SendBodyQ(string p_strBody, Action<bool> onDone, out bool bDup)
        {
            int iTxCnt = modDefApp.MSG_HEAD_CNT + p_strBody.Length + 2;
            m_bytTxBuff = new byte[iTxCnt];

            MakeHeader(p_strBody.Length);

            m_bytTxBuff[modDefApp.MSG_HEAD_CNT] = modDefApp.STX;
            byte[] bytBody = Encoding.Default.GetBytes(p_strBody);
            Array.Copy(bytBody, 0, m_bytTxBuff, modDefApp.MSG_HEAD_CNT + 1, p_strBody.Length);
            m_bytTxBuff[iTxCnt - 1] = modDefApp.ETX;

            return QueueSrv(onDone, out bDup);
        }

        private void EnsureSender()
        {
            if (m_thrSender != null && m_thrSender.IsAlive)
                return;

            lock (m_objSendLock)
            {
                if (m_thrSender != null && m_thrSender.IsAlive)
                    return;

                int nWin = modDefAPI.GetPrivateProfileInt("Property", "SEND_WINDOW", 1, modDefApp.MAIN_INI);
                m_nSendWindow = (nWin < 1) ? 1 : nWin;

                m_thrSender = new Thread(SenderLoop);
                m_thrSender.Name = "Host Sender Thread";
                m_thrSender.IsBackground = true;
                m_thrSender.Start();
            }
        }

        /*
         * ProcessSendDone :: 끝난 전송을 처리한다. CliWorkThread 가 매 바퀴 처음에 부른다.
         *   인터페이스 로그를 적고, 콜백(ACK 뒤에 하던 일)을 실행한다.
         */
        public void ProcessSendDone()
        {
            List<SendItem> lstDone = null;
            lock (m_objSendLock)
            {
                if (m_lstSendDone.Count == 0)
                    return;
                lstDone = new List<SendItem>(m_lstSendDone);
                m_lstSendDone.Clear();
            }

            for (int i = 0; i < lstDone.Count; i++)
            {
                SendItem it = lstDone[i];
                try
                {
                    if (it.Sent)
                    {
                        modCmWork.ShowMsgClient(it.TxText);
                        modDefApp.g_frmForm.InsertHostIfLog(m_BDb, it.TxText, it.Cmd, m_strDirection);
                    }
                    if (it.Ok)
                    {
                        modCmWork.ShowMsgClient(it.RxText, modDefApp.MSG_NOR);
                        modDefApp.g_frmForm.InsertHostIfLog(m_BDb, it.RxText, it.Cmd, "W2E");
                    }
                }
                catch (Exception ex)
                {
                    modCmWork.ShowMsgClient("[ProcessSendDone] 로그 기록 중 예외 : " + ex.Message, modDefApp.MSG_ERR);
                }

                lock (m_objSendLock)
                    m_setSendKey.Remove(it.Key);

                try
                {
                    if (it.OnDone != null)
                        it.OnDone(it.Ok);
                }
                catch (Exception ex)
                {
                    modCmWork.ShowMsgClient("[ProcessSendDone] 후처리 중 예외 : " + it.Cmd + " " + ex.Message, modDefApp.MSG_ERR);
                }
            }
        }

        // 보낸 것과 보낼 것을 모두 실패로 끝낸다.
        private void FailAllSend()
        {
            lock (m_objSendLock)
            {
                for (int i = 0; i < m_lstSendWait.Count; i++)
                {
                    m_lstSendWait[i].Ok = false;
                    m_lstSendDone.Add(m_lstSendWait[i]);
                }
                m_lstSendWait.Clear();

                for (int i = 0; i < m_lstSendQueue.Count; i++)
                {
                    m_lstSendQueue[i].Ok = false;
                    m_lstSendQueue[i].Sent = false;
                    m_lstSendDone.Add(m_lstSendQueue[i]);
                }
                m_lstSendQueue.Clear();
            }
        }

        // 보냈는데 ACK 를 못 받은 것만 실패로 끝낸다. (소켓이 새로 바뀌었을 때)
        private void FailWaitingSend()
        {
            lock (m_objSendLock)
            {
                for (int i = 0; i < m_lstSendWait.Count; i++)
                {
                    m_lstSendWait[i].Ok = false;
                    m_lstSendDone.Add(m_lstSendWait[i]);
                }
                m_lstSendWait.Clear();
            }
        }

        // 예전 RequestSrv 와 같이 통신장애로 보고 소켓을 닫는다.
        private void CloseAndFailAllSend()
        {
            m_blSockConnected = false;
            modCmWork.CloseSocket(ref m_sktSock);
            modCmWork.SetSocketCon(ref modDefApp.g_frmForm.picCliCom, modDefApp.ComSts.ComErr);
            FailAllSend();
        }

        private string ExtractRxText()
        {
            string strMsg = Encoding.Default.GetString(m_bytRxBuff);
            int nEnd = strMsg.IndexOf(Convert.ToChar(0x03));
            return (nEnd >= 0) ? strMsg.Substring(0, nEnd + 1) : strMsg.TrimEnd('\0');
        }

        /*
         * @.상태 보고(S) 전송 시각 기록 (2026-11-23) - 지연을 잴 때만 켠다.
         *     ECSCOMA.ini [Property] STATUS_SEND_LOG=1   (기본 0 = 남기지 않음)
         *   상태 보고는 건수가 많아 HOST_IF_LOG 에 남기지 않는다. 그래서 언제 나갔는지 알 길이 없었다.
         *   켜면 실행 폴더의 StatusSend.log 에 "시각<TAB>전문" 을 한 줄씩 남긴다.
         */
        private int m_nStatusSendLog = -1;      // -1 = 아직 안 읽음
        private void StatusSendLog(SendItem it)
        {
            try
            {
                if (m_nStatusSendLog < 0)
                    m_nStatusSendLog = modDefAPI.GetPrivateProfileInt("Property", "STATUS_SEND_LOG", 0, modDefApp.MAIN_INI);
                if (m_nStatusSendLog == 0 || it == null || it.Cmd != "S")
                    return;
                System.IO.File.AppendAllText("StatusSend.log",
                    it.SentAt.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" + it.TxText.Replace("\u0002", "<").Replace("\u0003", ">") + "\r\n");
            }
            catch { }
        }

        private void SenderLoop()
        {
            Socket sktLast = null;

            while (true)
            {
                try
                {
                    Socket skt = m_sktSock;
                    if (skt == null || !m_blSockConnected)
                    {
                        FailAllSend();
                        sktLast = null;
                        Thread.Sleep(20);
                        continue;
                    }

                    if (!object.ReferenceEquals(skt, sktLast))
                    {
                        // 새로 붙은 소켓. 옛 소켓에서 응답을 기다리던 것만 끝낸다. 큐는 그대로 보낸다.
                        FailWaitingSend();
                        sktLast = skt;
                        try { skt.ReceiveTimeout = 3000; } catch { }
                    }

                    // 1) 응답 받기
                    if (skt.Available > 0)
                    {
                        ClearBuff(ref m_bytRxBuff);
                        if (!ReadSock())
                        {
                            modCmWork.ShowMsgClient("[SenderLoop] 응답 프레임이 틀려 리모트 시스템과 연결을 종료합니다.", modDefApp.MSG_IMP);
                            CloseAndFailAllSend();
                            continue;
                        }

                        string strRx = ExtractRxText();
                        lock (m_objSendLock)
                        {
                            if (m_lstSendWait.Count > 0)
                            {
                                SendItem itAck = m_lstSendWait[0];
                                m_lstSendWait.RemoveAt(0);
                                itAck.Ok = true;
                                itAck.RxText = strRx;
                                m_lstSendDone.Add(itAck);
                            }
                        }
                        continue;
                    }

                    // 2) 응답 시간 초과
                    SendItem itHead = null;
                    lock (m_objSendLock)
                    {
                        if (m_lstSendWait.Count > 0)
                            itHead = m_lstSendWait[0];
                    }
                    if (itHead != null && (DateTime.Now - itHead.SentAt).TotalSeconds >= modDefApp.TIME_OUT)
                    {
                        modCmWork.ShowMsgClient("[SenderLoop] " + itHead.Cmd + " 메세지의 응답이 없습니다.", modDefApp.MSG_ERR);
                        CloseAndFailAllSend();
                        continue;
                    }

                    // 3) 보내기
                    while (true)
                    {
                        SendItem itNext = null;
                        lock (m_objSendLock)
                        {
                            if (m_lstSendWait.Count < m_nSendWindow && m_lstSendQueue.Count > 0)
                            {
                                itNext = m_lstSendQueue[0];
                                m_lstSendQueue.RemoveAt(0);
                            }
                        }
                        if (itNext == null)
                            break;

                        try
                        {
                            skt.Send(itNext.Frame, itNext.Frame.Length, SocketFlags.None);
                        }
                        catch (Exception exSend)
                        {
                            modCmWork.ShowMsgClient("[SenderLoop] 전송 실패 : " + exSend.Message, modDefApp.MSG_ERR);
                            itNext.Ok = false;
                            itNext.Sent = false;
                            lock (m_objSendLock)
                                m_lstSendDone.Add(itNext);
                            CloseAndFailAllSend();
                            break;
                        }

                        itNext.Sent = true;
                        itNext.SentAt = DateTime.Now;
                        StatusSendLog(itNext);
                        lock (m_objSendLock)
                            m_lstSendWait.Add(itNext);
                    }

                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    // 소켓이 다른 곳에서 닫혔거나 받는 도중 끊긴 경우 : 연결 끊김으로 본다.
                    try { modCmWork.ShowMsgClient("[SenderLoop] 예외 : " + ex.Message, modDefApp.MSG_ERR); } catch { }
                    try { CloseAndFailAllSend(); } catch { }
                    Thread.Sleep(100);
                }
            }
        }
    }
}
