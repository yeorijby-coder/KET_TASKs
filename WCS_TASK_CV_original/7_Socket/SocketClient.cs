using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace WCS_TASK_CV
{
    public class SocketClient : maindefine
    {
        protected Socket _Socket;
        private DateTime _rcvdate = System.DateTime.Now;
        private bool _IsConnect = false;
        private int _time_out;
        private string _strIp;
        private int _nPort;

        public bool m_bSocCon { get { return _IsConnect; } set { _IsConnect = value; } }
        public int time_out { get { return _time_out; } set { _time_out = value; } }
        public string Ip { get { return _strIp; } set { _strIp = value; } }
        public int Port { get { return _nPort; } set { _nPort = value; } }

        private string _receiveMessage = string.Empty;
        public string ReceiveMessage { get { return _receiveMessage; } set { _receiveMessage = value; } }
        private int _RcvLen;
        public int RcvLen { get { return _RcvLen; } set { _RcvLen = value; } }




        private IPEndPoint _ipEndPoint;
        public void SetConfig(string server_ip, int server_port, int time_out)
        {
            Ip =server_ip;
            IPAddress serverIP = IPAddress.Parse(server_ip);
            _ipEndPoint = new IPEndPoint(serverIP, server_port);
            Port = server_port;
            _time_out = time_out;
        }

        public int GetDate()
        {
            return (System.DateTime.Now - _rcvdate).Seconds;
        }

        public bool Connect(ref string msg)
        {
            string strTitle = "[Connect]";

            try
            {
                if (!m_bSocCon)
                {
                    _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _Socket.ReceiveTimeout = time_out;

                    _Socket.Connect(_ipEndPoint);
                    _rcvdate = System.DateTime.Now;
                    this.m_bSocCon = true;
                    return true;
                }
            }
            catch (SocketException sex)
            {
                this.m_bSocCon = false;
                msg = strTitle + sex.ToString();
                return false;
            }
            catch (Exception ex)
            {
                msg = strTitle + ex.ToString();
                return false;
            }
            return true;
        }

        public bool ThreadStop(ref string msg)
        {
            try
            {
                _Socket.Shutdown(SocketShutdown.Send);
                _Socket.Close();
                this.m_bSocCon = false;
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                msg = ex.Message;
                return false;
            }
            finally
            {
                // @.Shutdown/Close 에서 예외가 나도 접속 상태는 반드시 내려야 한다.
                //   여기서 true 로 남으면 Connect() 가 이미 접속된 것으로 보고 건너뛰고,
                //   작업 스레드도 접속 시도와 폴링을 모두 건너뛰어 재접속하지 못한다.
                this.m_bSocCon = false;
            }
        }
        private void DoShedule()
        {
            try
            {
                string receiveMessage = string.Empty;
                byte[] receiveCharacter;

                while (true)
                {
                    if (_Socket.Available > 0)
                    {
                        _rcvdate = System.DateTime.Now;
                        receiveCharacter = new byte[_Socket.Available];
                        _Socket.Receive(receiveCharacter);
                        receiveMessage = System.Text.Encoding.Default.GetString(receiveCharacter);
                        this.ReceiveMessage = receiveMessage;

                        _rcvdate = System.DateTime.Now;
                    }
                    Thread.Sleep(300);
                }
            }
            catch (Exception ex)
            {
                string xxx = ex.Message;
                //throw ex;
            }
        }
        public bool SendRst(char[] message, ref string msg)
        {
            try
            {
                byte[] Msg = new byte[message.Length];

                Msg = System.Text.Encoding.Default.GetBytes(message);
                if (!_Socket.Poll(200, SelectMode.SelectWrite)) return false;
                int nWrtLen = _Socket.Send(Msg);
                if (nWrtLen != message.Length) return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }


            return true;
        }
        public bool SendRst(string message, ref string msg)
        {
            try
            {
                byte[] Msg = new byte[message.Length];

                Msg = System.Text.Encoding.Default.GetBytes(message);
                if (!_Socket.Poll(200, SelectMode.SelectWrite)) return false;
                int nWrtLen = _Socket.Send(Msg);
                if (nWrtLen != message.Length) return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }


            return true;
        }
        public bool SendRst(byte[] message, ref string msg)
        {
            try
            {
                if (!_Socket.Poll(200, SelectMode.SelectWrite)) return false;
                int nWrtLen = _Socket.Send(message);
                if (nWrtLen != message.Length) return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

            return true;
        }

        public bool SendRst(char[] message, int len, ref string msg)
        {
            try
            {
                if (!_Socket.Poll(200, SelectMode.SelectWrite)) return false;
                byte[] Msg = new byte[message.Length];
                Msg = System.Text.Encoding.Default.GetBytes(message);
                int nWrtLen = _Socket.Send(Msg, len, SocketFlags.None);
                if (nWrtLen != message.Length) return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }


            return true;
        }
        public bool SendRst(string message,  int len,ref string msg)
        {
            try
            {
                if (!_Socket.Poll(200, SelectMode.SelectWrite)) return false;
                byte[] Msg = new byte[len];
                Msg = System.Text.Encoding.Default.GetBytes(message);
                int nWrtLen = _Socket.Send(Msg, len, SocketFlags.None);
                if (nWrtLen != message.Length) return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }


            return true;
        }
        public bool SendRst(byte[] message, int len,ref string msg)
        {
            try
            {
                if (!_Socket.Poll(200, SelectMode.SelectWrite)) return false;
                int nWrtLen = _Socket.Send(message, len, SocketFlags.None);
                if (nWrtLen != len) return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

            return true;
        }
        public void Clearbuffer()
        {
            byte[] receiveCharacter = new byte[2048];


            // @.남은 바이트를 다 비운다. 예전에는 한 번만 받고 끝나 2048 바이트가 넘게 남으면 뒤가 남았다. (2026-11-02)
            try
            {
                while (_Socket != null && _Socket.Available > 0)
                {
                    int nlen = _Socket.Receive(receiveCharacter, receiveCharacter.Length, SocketFlags.None);
                    if (nlen <= 0) break;
                }
            }
            catch (Exception) { }
        }
        // 응답을 기다릴 때 소켓을 다시 보는 간격(ms). 0 이하면 짧게 쉬지 않고 계속 본다. 기본 30. (CvThread 가 PLC 별로 정한다)
        public int RecvPollMs = 30;

        public bool RecvRst(ref byte[] message, int rcvlen, ref string msg)
        {
            try
            {
                byte[] receiveCharacter = new byte[rcvlen+1];
                _rcvdate = System.DateTime.Now;
                int nreadlen = rcvlen;
                RcvLen = 0;
                System.Diagnostics.Stopwatch swWait = System.Diagnostics.Stopwatch.StartNew();

                while (true)
                {
                    if (_Socket.Available > 0)
                    {
                        int nlen = _Socket.Receive(receiveCharacter, RcvLen, rcvlen, SocketFlags.None);

                        RcvLen = RcvLen + nlen;
                        rcvlen = rcvlen - nlen;
                    }

                    if (rcvlen == 0)
                    {
                        Buffer.BlockCopy(receiveCharacter, 0, message, 0, RcvLen);
                        break;
                    }

                    if (GetDate() > _time_out)
                    {
                        Buffer.BlockCopy(receiveCharacter, 0, message, 0, RcvLen);
                        msg = "시간초과입니다.";
                        return false;
                    }

                    // @.응답이 아직 안 왔다. 기본은 30ms 쉬고 다시 본다. RecvPollMs 가 0 이하면 50ms 까지는 쉬지 않고 계속 보고, 그 뒤에는 1ms 씩 쉰다.
                    //   (2026-11-15 : 센서 트래킹 구간 PLC 의 한 바퀴를 줄이려고. 한 번 읽을 때마다 최소 30ms 를 기다리던 것을 없앤다)
                    if (RecvPollMs <= 0)
                    {
                        /*
                         * @.쉬지 않고 도는 대신, 응답이 올 때까지 소켓에서 기다린다. (2026-11-26)
                         *
                         *   전에는 Thread.Yield() 로 50ms 까지 계속 돌면서 소켓을 봤다. 양보받을 스레드가 없으면 Yield 는 바로
                         *   돌아오므로, 응답을 기다리는 동안 그 스레드가 CPU 코어 하나를 다 썼다. RECV_POLL_MS=0 인 PLC 가 5개면
                         *   코어 4~5개분이다. (로컬 측정 : CV TASK 혼자 코어 4.7개분, 12코어 PC 의 39%)
                         *   Poll 은 응답이 도착하면 바로 깨어나고, 기다리는 동안에는 CPU 를 쓰지 않는다. 응답을 받는 빠르기는 같다.
                         */
                        bool bReadable = false;
                        try { bReadable = _Socket.Poll(20000, SelectMode.SelectRead); }     // 최대 20ms. 그 안에 오면 바로 돌아온다 (시간초과를 보려고 끊어 기다린다)
                        catch (Exception) { Thread.Sleep(1); }                              // 소켓이 닫혔다 - 돌지 않게 쉰다 (아래 시간초과가 정리한다)
                        // 끊긴 소켓은 읽을 것이 없는데도 "읽을 수 있음" 으로 나온다. 그대로 두면 다시 쉬지 않고 돌게 되므로 잠깐 쉰다.
                        if (bReadable && _Socket.Available <= 0) Thread.Sleep(1);
                    }
                    else
                        Thread.Sleep(RecvPollMs);
                }
                return true;

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }
    

        public bool RecvRstLoop(ref byte[] message, ref string msg, int sleep)
        {
            try
            {
                byte[] receiveCharacter = new byte[1000];
                RcvLen = 0;
                Thread.Sleep(sleep);
                while (true)
                {
                    if (_Socket.Available > 0)
                    {
                        RcvLen = _Socket.Receive(receiveCharacter, 0, _Socket.Available, SocketFlags.None);
                        if (RcvLen > 0)
                        {
                            Buffer.BlockCopy(receiveCharacter, 0, message, 0, RcvLen);
                            break;
                        }
                    }
                    if (GetDate() > _time_out)
                    {
                        msg = "시간초과입니다.";
                        return false;
                    }

                    Thread.Sleep(10);
                }


                return true;

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public int Available()
        {
            return _Socket.Available;
        }
    }
}
