using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data;
namespace WCS_TASK_SC
{
    class cWrkWcSkt
    {
        // @@@.선언
        public string mDVC_NO = "001";                      // @.PLC 번호 
        public string mStatComm  = "D" ;                       // @.PLC와 통신하는 쓰레드 연결상태[C:연결, T:시도, D:비연결]
        public string mStatOp  = "W" ;                         // @.PLC와 통신하는 쓰레드 동작상태[N:정상, W:대기, E:에러]

        public cDbUse mDbWrk = new cDbUse("Multi", false ) ;            // @.DB Class[Work]
        public cDbUse mDbLog = new cDbUse("Multi", false );             // @.DB Class[Log]
        private DataTable mDtReqIF = new DataTable();                  // @.DataTable[요청]
        public string mRcvTgm   = "";                           // @.수신Tgm
        public string mMsg ="";                                   // @.메세지

        // @@.소켓에서 사용되는 변수선언
        public Socket  mSkt;                                   // @.소켓객체선언
        //public bool  mSktConnected= false ;                         // @.소켓접속여부
        public int mReSndCnt  = 3;                         // @.재전송횟수
        //public byte [] mByRxBuff;                               // @.수신버퍼
        public byte[] mByTxBuff;                               // @.전송버퍼
        //public byte[] mReqTxBuff;                              // @.전송데이터버퍼

        // @@.WGT 통신 상수선언
        public const int  MC_SND_TGM_BYTE_LEN   = 6;         // @.전송Tgm 길이[Byte]
        public const int MC_RCV_TGM_BYTE_LEN   = 22;        // @.수신할Tgm 길이[Byte]

        // @@.안정계측이 아닐경우 여러번 재계측을 시도한다. 
        public int mCnttryWgt  = 0   ;                      // @.계측시도 카운트
        public double  mMaxtryWgt  = 0    ;                     // @.불안정중량 최대값

        // @@@.생성자
        public cWrkWcSkt(string pDVC_NO)
        {
            mDVC_NO = pDVC_NO;     // @.System ID 
            if(cCmLib.GfDBLogIn (ref this.mDbWrk.mCnMain, ref mMsg) == true) this.mDbWrk.Init(); // @.DB Connetction
            if(cCmLib.GfDBLogIn (ref this.mDbLog.mCnMain, ref mMsg) == true) this.mDbLog.Init(); // @.DB Connetction
        }

        // @@@.Db Connection Close
        public void DBLogOut(){
            if(this.mDbWrk.mCnMain.State == ConnectionState.Open ) this.mDbWrk.mCnMain.Close(); this.mDbWrk.DbConnted = false;
            if(this.mDbLog.mCnMain.State == ConnectionState.Open ) this.mDbLog.mCnMain.Close(); this.mDbLog.DbConnted = false;
        }

        // @@@.Db Connection Open
        public void DBLogIn(){
            this.DBLogOut();
            if (cCmLib.GfDBLogIn(ref this.mDbWrk.mCnMain, ref mMsg) == true) this.mDbWrk.Init();
            if (cCmLib.GfDBLogIn(ref this.mDbLog.mCnMain, ref mMsg) == true) this.mDbLog.Init();
        }

        #region @@@.속성[IsSktConnect]
        private bool  mSktConnected = false;      // @.IsSktConnect
        public bool IsSktConnect 
        {
          get {
            return mSktConnected;
          }
        }
        #endregion

        // @@@.SktDisconnect
        public void  SktDisconnect() 
        {
            if( IsSktConnect==false)
            {
                this.SktClose(ref mMsg);
                this.mSktConnected = false;
                this.mStatOp = "E";
            }
        }
 
        // @@@.SktConnect
        public bool SktConnect(string pIPAdress ,
                                   int pPort,
                          ref string  pMsg ) {
             System.Net.IPEndPoint rEP;
             System.Net.IPAddress rIP;

            try
            {
                pMsg = "";

                rIP = System.Net.IPAddress.Parse(pIPAdress);
                rEP = new System.Net.IPEndPoint(rIP, pPort);
                mSkt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSkt.Connect(rEP);

                if( mSkt.Connected == false ) throw new Exception();

                mStatOp = "W";
                this.mSktConnected = true;
                pMsg = "Socket Connected";
                return  true;
            }
            catch(SocketException se) 
            {
                pMsg = se.Message;
            }
            catch(Exception ex)
            { 
                pMsg = ex.Message;
            }
            mStatOp = "E";
            mSktConnected = false;
            pMsg = "Fail Socket Connect::" + pMsg;
            return  false;
        }

        // @@@.소켓 닫기
        private bool SktClose(ref string  pMsg)
        {
            try
            {
                pMsg = "";
                if(mSkt!=null){
                    if( mSkt.Connected==true){
                        mSkt.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    }
                    mSkt.Close();
                    mSkt = null; // @.프로그램내에서 강제종료시 소켓 객체 사용여부 판단
                }

                pMsg = "Socket Close";
                return true;
            }
            catch(Exception ex) 
            {
                pMsg = ex.Message;
            }
            pMsg = "Fail Socket Close" + pMsg;
            return false;
        }

    // @@@.W/C에 WGT계량 요청Tgm 전송
    public bool SndReqWC(ref string  pMsg) 
    {
        mByTxBuff = new byte[MC_SND_TGM_BYTE_LEN];
        string  sSnd = "";

        try
        {
            pMsg = "";
            sSnd = this.mDVC_NO.Substring(this.mDVC_NO.Length - 2, 2) + "RW" + cDefApp.CRLF;
            //sSnd = "01" + "RW" + CRLF
            mByTxBuff = System.Text.Encoding.Default.GetBytes(sSnd);

            //mByTxBuff(0) = CByte(Hex(Asc("0")))
            //mByTxBuff(1) = CByte(Hex(Asc("2")))
            //mByTxBuff(2) = CByte(Hex(Asc("R")))
            //mByTxBuff(3) = CByte(Hex(Asc("W")))
            //mByTxBuff(4) = &HD
            //mByTxBuff(5) = &HA

            if (Snd(mByTxBuff, mByTxBuff.Length, SocketFlags.None, false, cDefApp.GM_COMM_SND_TIME_OUT, ref pMsg) == false) throw new Exception();

            pMsg = "W/C에 중량계측 요청Tgm 전송 성공";
            return true;
        }
        catch(Exception ex) 
        {
            pMsg = ex.Message;
        }    
            pMsg = "W/C에 중량계측 요청Tgm 전송 실패::" + pMsg;
            return false;
    }

    // @@@.W/C가 송신한 중량 Tgm수신
    public bool RcvAckWgt(ref byte[] pRxBuff,
                           int pReadByteLen, 
                   ref string  pMsg) 
    {
        pRxBuff = new byte[pReadByteLen];
        //Dim sRcvTgm As String = ""

        try
    {
            pMsg = "";

            // @.수신된Tgm 을 읽어온다.[실패시 빠져나감]
            if (Rcv(ref pRxBuff, pRxBuff.Length, SocketFlags.None, cDefApp.GM_COMM_RCV_TIME_OUT, ref pMsg) == false) throw new Exception(); 

            pMsg = "WGT에서 송신한 중량Tgm 수신 성공";
            return true;
    }
        catch(Exception ex) 
    {
            pMsg = ex.Message;
            }
        pMsg = "WGT에서 송신한 중량Tgm 수신 실패::" + pMsg;
        return false;
    }

    // @@@.WGT에 Tgm을 전송처리한다.
    private bool  Snd(byte[] pTxBuff,
                         int pSize, 
                SocketFlags  pFlag , 
                bool  pReSnd ,
                int pTmOutMil,
                ref string  pMsg )
    {
        int nSnd=0;

        try
        {
            pMsg = "";
            while (true)
            {
                if (nSnd > mReSndCnt)
                {
                    pMsg = "Send Count Over " + mReSndCnt.ToString() + " Times";
                    break;
                }

                nSnd += 1;

                // @.재 정의 수정 할 것
                if( mSkt.Send(pTxBuff, pSize, pFlag) < 1)
                {
                    if( pReSnd == true )
                    {
                        Thread.Sleep(pTmOutMil);
                        continue;
                        }

                    pMsg = "Send Time Out";
                    break;
                }

                break;
            }
        pMsg = "Success Send Socket";
        return true;
                        }
        catch(SocketException se) 
        {
            pMsg = se.Message;
        }
        catch(Exception ex) 
        {
            pMsg = ex.Message;
        }
        
        pMsg = "Fail Send Socket::" + pMsg;
        return false;
    }

    // @@@.PLC에서 전송한 Tgm을 수신처리한다.
    public bool Rcv(ref byte[] pRxBuff, 
                        int pReadCnt, 
                        SocketFlags  pFlag, 
               double  pTmOutMil, 
               ref  string  pMsg ) 
    {
        DateTime  dtRcvTm;
        int  nRemain;
        byte[] TmpRxBuff;

        try
        {
            pMsg = "";
            TmpRxBuff = new byte[pReadCnt];
            pRxBuff = new byte[pReadCnt];

            // @.WCS <-> WMS(S) 통신 에러 체크
            if (mSkt.Poll(1, SelectMode.SelectError) == true)
            {
                SktDisconnect();
                pMsg = "This Socket has an error";
                throw new Exception();
             }

            dtRcvTm = DateTime.Now.AddMilliseconds (pTmOutMil);

            while (true)
            {
                if (dtRcvTm < DateTime.Now)
                {  
                    pMsg = "Time Out";
                    break;
                }

                // @.WCS <- WMS(S) 통신 수신 정보 확인
                if( mSkt.Poll(Convert.ToInt32 (pTmOutMil), SelectMode.SelectRead) == false)
                {
                    continue;
                }

                if (mSkt.Available == 0)
                {
                     SktDisconnect();
                    pMsg = "This Socket Disconnect";
                    break;
                }

                if( mSkt.Available >= pReadCnt)
                {
                    break;
                }
            }

            nRemain = mSkt.Receive(TmpRxBuff, pReadCnt, SocketFlags.None);
            System.Buffer.BlockCopy(TmpRxBuff, 0, pRxBuff, 0, nRemain);

            pMsg = "Success Read Socket";
            return true;
        }
        catch(SocketException se) 
        {
            pMsg = se.Message;
            }
        catch(Exception ex) 
        {
            pMsg = ex.Message;
        }
    
        pMsg = "Fail Read Socket::" + pMsg;
        return false;
    }

    // @@@.소켓 더미데이터 읽기
    public int  RcvDum(ref byte[] pByRxBuff,
                  ref string  pMsg )
    {
        int nRemain ;
        byte[] TmpRxBuff;
        int  nDum = 0;

        try
        {
            pMsg = "";
            // @.WCS <-> WMS(S) 통신 에러 체크
            if(mSkt.Poll(1, SelectMode.SelectError) == true)
            {
                this.SktDisconnect();
                pMsg = "This Socket has an error";
                throw new Exception();
            }

            // @.WCS <- WMS(S) 통신 수신 정보 확인
            if( mSkt.Poll(1, SelectMode.SelectRead) == false)
            {
                throw new Exception();
            }

            if (mSkt.Available == 0)
            {
                this.SktDisconnect();
                pMsg = "This Socket Disconnect";
                throw new Exception();
            }
            else
            {
                nDum = mSkt.Available;
            }
                
            TmpRxBuff = new byte[nDum];
            pByRxBuff= new byte[nDum];

            nRemain = mSkt.Receive(TmpRxBuff, nDum, SocketFlags.None);
            System.Buffer.BlockCopy(TmpRxBuff, 0, pByRxBuff, 0, nRemain);

            pMsg = "Success Read Dummy[" + nDum.ToString() + "]";
            return  nDum;
                }
        catch (SocketException se)
        {
            pMsg = se.Message;
        }
        catch(Exception ex) {
            pMsg = ex.Message;
        
                }
        pMsg = "Fail Read Socket Dummy::" + pMsg;
        return 0;
        }
    }
}
                                                                                                                                                                                                                                     