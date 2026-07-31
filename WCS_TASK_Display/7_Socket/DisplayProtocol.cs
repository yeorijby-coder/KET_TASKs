using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net.Sockets;
using Npgsql;

namespace WCS_TASK_Display
{
    // Display(electric-board / Jeon-gwang-pan) protocol.
    //
    // This class plays the same architectural role as MelsecQ3EProtocol does for the
    // CV/SC tasks: it owns BOTH the DB connection (cDbPostUse) and the TCP socket so the
    // worker thread holds a single comm object per display controller.
    //
    // The on-the-wire packet is reproduced exactly from the legacy MFC system
    // (CDisplay::makeData, EcsSv\Display.cpp):
    //
    //   [STX][ID][COLOR][PRODUCT(8 bytes)][CHK_HI][CHK_LO][ETX]    -> 14 bytes
    //
    //   STX        = 0x02
    //   ID         = 0x31 + nDisplayNo            (display 0 -> '1', display 1 -> '2' ...)
    //   COLOR      = 0x04(Red) / 0x05(Green) / 0x06(Yellow)
    //   PRODUCT    = 8 ASCII chars, space padded / truncated to 8
    //   CHK        = XOR of every byte from STX through the last PRODUCT byte (11 bytes)
    //   CHK_HI     = ((CHK >> 4) & 0x0F) | 0x30   (printable ASCII nibble)
    //   CHK_LO     = ( CHK       & 0x0F) | 0x30
    //   ETX        = 0x03
    public class DisplayProtocol : SocketClient
    {
        public const int DSP_DATA_LEN = 8;     // fixed product field length
        public const int DSP_PACKET_LEN = 14;  // STX+ID+COLOR+8+CHK_HI+CHK_LO+ETX

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

        #region Open / Close  (DB + Socket, mirror of MelsecQ3EProtocol)
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
        }
        #endregion

        #region Packet build / send
        public void ResetError() { _strErrorMsg = ""; }
        public void SetErrorMsg(string strMsg) { _strErrorMsg = strMsg; }
        public string ErrorMsg { get { return _strErrorMsg; } }

        // Pad/truncate the product field to exactly 8 chars (legacy behaviour).
        public static string FitProduct(string strData)
        {
            if (strData == null) strData = "";
            if (strData.Length > DSP_DATA_LEN) return strData.Substring(0, DSP_DATA_LEN);
            return strData.PadRight(DSP_DATA_LEN, ' ');
        }

        // Build the 14-byte display packet. Identical to CDisplay::makeData.
        public byte[] MakeData(int nDisplayNo, byte byColor, string strData)
        {
            string strFit = FitProduct(strData);
            byte[] byData = Encoding.ASCII.GetBytes(strFit); // 8 bytes

            byte[] pBuff = new byte[DSP_PACKET_LEN];
            pBuff[0] = cDefApp.STX;                         // 0x02
            pBuff[1] = (byte)(0x31 + nDisplayNo);           // display id
            pBuff[2] = byColor;                             // color
            Buffer.BlockCopy(byData, 0, pBuff, 3, DSP_DATA_LEN);

            // checksum = XOR over STX .. last product byte (index 0..10)
            byte byChk = 0;
            for (int i = 0; i <= 2 + DSP_DATA_LEN; i++) byChk ^= pBuff[i];

            pBuff[3 + DSP_DATA_LEN] = (byte)(((byChk >> 4) & 0x0F) | 0x30); // CHK_HI
            pBuff[4 + DSP_DATA_LEN] = (byte)((byChk & 0x0F) | 0x30);        // CHK_LO
            pBuff[5 + DSP_DATA_LEN] = cDefApp.ETX;                          // 0x03
            return pBuff;
        }

        // Build + send a display packet to the board.
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
