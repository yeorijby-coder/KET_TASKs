using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net.Sockets;
using Npgsql;

namespace WCS_TASK_Display
{
    // 전광판 프로토콜.
    //
    // CV/SC 태스크의 MelsecQ3EProtocol 과 같은 역할을 한다.
    // DB 연결(cDbPostUse)과 TCP 소켓을 모두 이 클래스가 들고 있어서,
    // 작업 스레드는 전광판 컨트롤러 하나당 통신 객체 하나만 다루면 된다.
    //
    // 전문 형식은 레거시 MFC 시스템(CDisplay::makeData, EcsSv\Display.cpp)을
    // 그대로 옮긴 것이다.
    //
    //   [STX][ID][COLOR][PRODUCT(8바이트)][CHK_HI][CHK_LO][ETX]    -> 총 14바이트
    //
    //   STX        = 0x02
    //   ID         = 0x31 + 전광판번호            (0번 -> '1', 1번 -> '2' ...)
    //   COLOR      = 0x04(빨강) / 0x05(초록) / 0x06(노랑)
    //   PRODUCT    = ASCII 8자리, 모자라면 공백채움 / 넘치면 잘라냄
    //   CHK        = STX 부터 PRODUCT 마지막 바이트까지(11바이트) 전부 XOR
    //   CHK_HI     = ((CHK >> 4) & 0x0F) | 0x30   (출력 가능한 ASCII 로 만들기 위해 니블에 0x30 을 더한다)
    //   CHK_LO     = ( CHK       & 0x0F) | 0x30
    //   ETX        = 0x03
    public class DisplayProtocol : SocketClient
    {
        public const int DISP_DATA_LEN = 8;     // @.품명 필드 고정 길이
        public const int DISP_PACKET_LEN = 14;  // @.STX+ID+COLOR+8+CHK_HI+CHK_LO+ETX

        public const byte COLOR_RED = 0x04;
        public const byte COLOR_GREEN = 0x05;
        public const byte COLOR_YELLOW = 0x06;

#if POSTGRESQL
        public NpgsqlConnection _pConObj;
        public cDbPostUse _pBdb;
#endif
        public bool m_bDBOpen;

        private string _strConnectionString;
        private string _strName;
        private string _strErrorMsg = "";

        private string _strSndHexString;
        private string _strSndAsciiString;
        private string _strRcvHexString;
        private string _strRcvAsciiString;
        private bool _Hex = true;
        private bool _Ascii = false;
        public bool IsHex { get { return _Hex; } set { _Hex = value; } }
        public bool IsAscii { get { return _Ascii; } set { _Ascii = value; } }
        public string SndHexString { get { return _strSndHexString; } set { _strSndHexString = value; } }
        public string SndAsciiString { get { return _strSndAsciiString; } set { _strSndAsciiString = value; } }
        public string RcvHexString { get { return _strRcvHexString; } set { _strRcvHexString = value; } }
        public string RcvAsciiString { get { return _strRcvAsciiString; } set { _strRcvAsciiString = value; } }

        public DisplayProtocol(string ConnectionString)
        {
            _strName = "Display";
            _strConnectionString = ConnectionString;
        }

        #region 접속 / 종료  (DB + 소켓, MelsecQ3EProtocol 과 동일 구조)
        public bool Open(ref string strRtnMsg)
        {
            string strTitle = "[Open]";
            m_bDBOpen = false;
#if POSTGRESQL
            _pConObj = new NpgsqlConnection();
            try
            {
                _pConObj.ConnectionString = _strConnectionString;
                _pConObj.Open();

                if (_pConObj.State != ConnectionState.Open)
                {
                    _pConObj.Dispose();
                    strRtnMsg = strTitle + "DataBase Open failed.";
                    return false;
                }
                _pBdb = new cDbPostUse(_pConObj, false);
                m_bDBOpen = true;

                if (!Connect(ref strRtnMsg))
                {
                    strRtnMsg = strTitle + strRtnMsg;
                    return false;
                }
                m_bSocCon = true;
                return true;
            }
            catch (Exception e)
            {
                strRtnMsg = strTitle + e.ToString();
                if (_pConObj != null) _pConObj.Dispose();
                return false;
            }
#else
            strRtnMsg = strTitle + "POSTGRESQL build symbol is required.";
            return false;
#endif
        }

        public void Close(ref string strRtnMsg)
        {
            try
            {
                if (m_bSocCon)
                {
                    string msg = "";
                    ThreadStop(ref msg);
                }
#if POSTGRESQL
                if (m_bDBOpen)
                {
                    _pConObj.Close();
                    _pConObj.Dispose();
                    m_bDBOpen = false;
                }
#endif
                strRtnMsg = "[Close] DB, Socket Close. Success";
            }
            catch (SocketException sex)
            {
                strRtnMsg = "[Close] Socket Exception [" + sex.ToString() + "]";
            }
            catch (Exception ex)
            {
                strRtnMsg = "[Close] Exception [" + ex.ToString() + "]";
            }
            finally
            {
                // @.닫는 도중 예외가 나도 상태는 반드시 초기화한다.
                //   이미 끊긴 소켓을 닫으면 Shutdown 에서 예외가 나는데,
                //   그때 m_bSocCon 이 true 로 남으면 Thread_Doing 이
                //   접속 시도와 폴링 루프를 모두 건너뛰어 영영 재접속하지 못한다.
                m_bSocCon = false;
                m_bDBOpen = false;
            }
        }
        #endregion

        #region 전문 작성 / 전송
        public void ResetError() { _strErrorMsg = ""; }
        public void SetErrorMsg(string strMsg) { _strErrorMsg = strMsg; }
        public string ErrorMsg { get { return _strErrorMsg; } }

        // @@.품명 필드를 정확히 8자리로 맞춘다.(레거시와 동일 동작 : 모자라면 공백채움, 넘치면 잘라냄)
        public static string FitProduct(string strData)
        {
            if (strData == null) strData = "";
            if (strData.Length > DISP_DATA_LEN) return strData.Substring(0, DISP_DATA_LEN);
            return strData.PadRight(DISP_DATA_LEN, ' ');
        }

        // @@.14바이트 전광판 전문을 만든다.(CDisplay::makeData 와 동일)
        public byte[] MakeData(int nDisplayNo, byte byColor, string strData)
        {
            string strFit = FitProduct(strData);
            byte[] byData = Encoding.ASCII.GetBytes(strFit); // @.8바이트

            byte[] pBuff = new byte[DISP_PACKET_LEN];
            pBuff[0] = cDefApp.STX;                         // @.0x02
            pBuff[1] = (byte)(0x31 + nDisplayNo);           // @.전광판 번호
            pBuff[2] = byColor;                             // @.색상
            Buffer.BlockCopy(byData, 0, pBuff, 3, DISP_DATA_LEN);

            // @.체크섬 = STX 부터 품명 마지막 바이트까지(index 0~10) 전부 XOR
            byte byChk = 0;
            for (int i = 0; i <= 2 + DISP_DATA_LEN; i++) byChk ^= pBuff[i];

            pBuff[3 + DISP_DATA_LEN] = (byte)(((byChk >> 4) & 0x0F) | 0x30); // @.CHK_HI
            pBuff[4 + DISP_DATA_LEN] = (byte)((byChk & 0x0F) | 0x30);        // @.CHK_LO
            pBuff[5 + DISP_DATA_LEN] = cDefApp.ETX;                          // @.0x03
            return pBuff;
        }

        // @@.전문을 만들어 전광판으로 전송한다.
        public bool SendDisplay(int nDisplayNo, byte byColor, string strData, ref string msg)
        {
            try
            {
                ResetError();
                byte[] pBuff = MakeData(nDisplayNo, byColor, strData);

                SndHexString = "";
                SndAsciiString = "";
                if (IsHex) SndHexString = BytesToHexs(pBuff, pBuff.Length);
                if (IsAscii) SndAsciiString = Encoding.Default.GetString(pBuff, 0, pBuff.Length);

                Clearbuffer();
                if (!SendRst(pBuff, pBuff.Length, ref msg))
                {
                    SetErrorMsg("SendDisplay send fail [" + msg + "]");
                    ThreadStop(ref msg);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                SetErrorMsg("SendDisplay Exception [" + ex.Message + "]");
                return false;
            }
        }
        #endregion
    }
}
