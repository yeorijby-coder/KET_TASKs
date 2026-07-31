using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace WCS_TASK_SC
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


	        for(;;)
	        {
                if (_Socket.Available > 0)
                {
                    int nlen = _Socket.Receive(receiveCharacter, receiveCharacter.Length, SocketFlags.None);
		        }
		        break;
	        }  
        }
        public bool RecvRst(ref byte[] message, int rcvlen, ref string msg)
        {
            try
            {
                byte[] receiveCharacter = new byte[rcvlen+1];
                _rcvdate = System.DateTime.Now;
                int nreadlen = rcvlen;
                RcvLen = 0;

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

                    Thread.Sleep(30);
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
