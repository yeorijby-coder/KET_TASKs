using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Samoh_Lib;
using System.Data;
using System.Data.OleDb;
using log4net;
using log4net.Config;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using NpgsqlTypes;

namespace WCS_TASK_SC
{
	//2014 조형준 메모리에 Cv 상태를 저장한다.
	public class SCData
	{
		private string HexaScValue;
		public string HEXA_SC_VALUE
		{
			get { return HexaScValue; }
			set { HexaScValue = value; }
		}

        private int ScErrCd;
        public int ERR_CODE_RD
        {
            get { return ScErrCd; }
            set { ScErrCd = value; }
        }

        private string OpMode;
        public string OP_MODE
        {
            get { return OpMode; }
            set { OpMode = value; }
        }

        private string OnlineModeRd;
        public string ONLINE_MODE_RD 
        {
            get { return OnlineModeRd; }
            set { OnlineModeRd = value; }
        }

        private string AutoModeRd;
        public string AUTO_MODE_RD
        {
            get { return AutoModeRd; }
            set { AutoModeRd = value; }
        }

        private string UcstatusRd;
        public string UCSTATUS_RD
        {
            get { return UcstatusRd; }
            set { UcstatusRd = value; }
        }

        private string ActiveModeRd;
        public string ACTIVE_MODE_RD
        {
            get { return ActiveModeRd; }
            set { ActiveModeRd = value; }
        }

		public SCData()
		{
			HEXA_SC_VALUE = "";
            OP_MODE = "";
            ONLINE_MODE_RD = "";
            AUTO_MODE_RD = "";
            UCSTATUS_RD = "";
            ACTIVE_MODE_RD = "";
		}
	}

    public class ScThread : maindefine
    {

        public PsMsgView callPsMsgView = null;
        public PfSetStatImgViewDB callPicDb = null;
        public PfSetStatImgViewSOCKET callPicSocket = null;

        public int m_nThNo = 0;
        public string m_strWh_typ = "";
        public string m_strEqmtTyp = "";
        public string m_strPlc_No = "";
        public string m_strScNo = "";
        public string m_strMcNo = "";
        public string m_strScGrpNo = "";
        public string m_strIp = "";
        public string m_strCurPort = "";
        public string m_strFromPort = "";
        public string m_strToPort = "";
        public int m_nPortCnt = 0;
        public string m_strConnectString = "";
        public string m_strLogFileNm = "";
        public int m_nCnt = 0;
        public int m_nCurPort = 0;
        public int m_nFromPort = 0;
        public int m_nToPort = 0;
        int nSelCnt = 0;
        public string m_strLogMsg = "";
        public bool m_blHostErrSendYN = false;
        public bool m_blHostSendYN = false;
        public bool m_blConnectYn = false;
       

        public string m_firstErr = "";
        public int m_firstErrChk = 0;
        public int m_nRetCd = 0;
        private string _strErrorMsg = "";
        public string m_strRtnMsg;
        private MelsecQ3EProtocol m_msQPlc;
        public Thread m_thThread;
        public SYS_MAIN m_frmMain;

        /*
         * 수동 통신 절단 지시. (CV 통합판과 동일한 방식)
         *
         *   화면의 소켓 상태 아이콘을 눌러 "통신연결 해제" 를 고르면 참이 된다.
         *   - 스레드 본체(Thread_Doing)는 이 플래그를 보고 소켓을 닫고 내려간다.
         *   - 메인폼의 Thread_Tick 은 이 플래그가 선 슬롯을 재기동하지 않는다.
         *   UI 스레드가 쓰고 통신 스레드가 읽으므로 volatile 로 가시성을 보장한다.
         */
        public volatile bool m_bManualStop = false;

        private bool m_bOpen;
        public bool IsOpen { get { return m_bOpen; } set { m_bOpen = value; } } //프로그램 화면표시용.

        public PictureBox m_picStatOp = null;
        public PictureBox m_picStatDbCn = null;

		//Dictionary 객체를 생성함.
		//2014 조형준.
		Dictionary<int, SCData> ScDic = new Dictionary<int, SCData>();

		string strSql = "";
		string CRLF = "\r\n";
		int ReqCnt = 0;

        public ScThread(int nThNo,
						string strWh_Typ,
                        string strEqmtTyp,
                        string strPlc_No,
                        string strSc_No,
                        string strMc_No,
                        string strSc_Grp_No,
                        string strIp,
                        string strCurPort,
                        string strFromPort,
                        string strToPort,
                           int nPortCnt,
                        string strConnectString,
                        string strLogFileNm)
        {
            m_nThNo = nThNo;
            m_strWh_typ = strWh_Typ;
            m_strEqmtTyp = strEqmtTyp;
            m_strPlc_No = strPlc_No;
            m_strScNo = strSc_No;
            m_strMcNo = strMc_No;
            m_strScGrpNo = strSc_Grp_No;
            m_strIp = strIp;
            m_strCurPort = strCurPort;
            m_strFromPort = strFromPort;
            m_strToPort = strToPort;

            m_nCurPort = Convert.ToInt32(0 + m_strCurPort);
            m_nFromPort = Convert.ToInt32(0 + m_strFromPort);
            m_nToPort = Convert.ToInt32(0 + m_strToPort);

            m_strConnectString = strConnectString;
            m_strLogFileNm = strLogFileNm;
            m_nPortCnt = nPortCnt;

            IsOpen = false;
            m_msQPlc = new MelsecQ3EProtocol(m_strConnectString);
            m_msQPlc.IsHex = true;
        }

        #region Thread_Doing
        /*
         * 화면 표시용
         */
        #region 화면 표시용.
        private void MakeMsg(string msg, int nThGbn,
                                [CallerFilePath] string strFile = "",
                                [CallerMemberName] string strFunc = "")
        {
            try
            {
                m_frmMain.PsMsgView(msg, m_strPlc_No.ToString(), nThGbn, strFile, strFunc);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void MakeMsg_Error(string msg, int nThGbn,
                                [CallerFilePath] string strFile = "",
                                [CallerMemberName] string strFunc = "")
        {
            try
            {
                m_frmMain.PsMsgView_Error(msg, m_strPlc_No.ToString(), nThGbn, strFile, strFunc);
                lock (cDefApp.m_LogQ[m_nThNo])
                {
                    cDefApp.m_LogQ[m_nThNo].Enqueue(new LogParam(DateTime.Now, msg));
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void MakeMsg_Imp(string msg, int nThGbn,
                                [CallerFilePath] string strFile = "",
                                [CallerMemberName] string strFunc = "")
        {
            try
            {
                m_frmMain.PsMsgView_IMP(msg, m_strPlc_No.ToString(), nThGbn, strFile, strFunc);
                lock (cDefApp.m_LogQ[m_nThNo])
                {
                    cDefApp.m_LogQ[m_nThNo].Enqueue(new LogParam(DateTime.Now, msg));
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void SetErrorMsg(string strMsg)
        {
            _strErrorMsg = strMsg;
            Log.Error(_strErrorMsg);
        }

        private void PicDBChange(string strDBStat)
        {
            callPicDb(m_picStatDbCn, strDBStat);
        }

        private void PicSocketChange(string strStatSocket, string strStatOp)
        {
            callPicSocket(m_picStatOp, strStatSocket, strStatOp);
        }
        #endregion
        /* 
         * 실구동용
         */
        #region
        public void Thread_Doing(object value)
        {
            string strTitle = "[Thread_Doing]";
            try
            {
                // @.수동 절단 지시가 선 채로 (타이머 경합 등으로) 기동됐으면 바로 내려간다.
                if (m_bManualStop)
                {
                    throw new Exception("수동 절단 지시로 인한 쓰레드 종료");
                }

                MakeMsg_Imp("DB/Socket Connectting", m_nThNo);


                if (m_msQPlc.m_bSocCon == false && m_msQPlc.m_bDBOpen == false)
                {
                    // open된 포트개수 만큼 재연결
                    for (int i = 0; i <= m_nToPort - m_nFromPort; i++)
                    {
                        // @.재시도 도중 절단 지시가 서면 접속을 성립시키지 않고 바로 내려간다.
                        if (m_bManualStop) goto EXIT_LBL;

                        if (m_nCurPort > m_nToPort)
                        {
                            m_nCurPort = m_nFromPort;
                        }
                        for (int j = 0; j < m_nPortCnt; j++)
                        {
                            if (m_bManualStop) goto EXIT_LBL;

                            MakeMsg_Imp(string.Format("IP [{0}] PORT [{1}] 접속시도", m_strIp, m_nCurPort.ToString()), m_nThNo);
                            m_msQPlc.SetConfig(m_strIp, m_nCurPort, 2);

                            if (!m_msQPlc.Open(ref m_strRtnMsg))
                            {
                                SetErrorMsg("Comm" + m_nThNo + " :" + m_strRtnMsg);
                                MakeMsg_Error(m_strRtnMsg, m_nThNo);

                                //DB는 접속 되었는데 설비와 연결이 안되어 있는 경우 LOG남기기
                                if (m_msQPlc.m_bSocCon == false && m_msQPlc.m_bDBOpen == true)
                                {
                                    InsertWcsLogPgr("", "[Thread_Doing] 소켓 연결 에러");
                                }

                                m_msQPlc.Close(ref m_strRtnMsg);

                                if (j == m_nPortCnt - 1)
                                {
                                    m_nCurPort = m_nCurPort + 1;
                                }
                                m_blConnectYn = false;

                                Thread.Sleep(2000);
                                continue;
                            }
                            else
                            {
                                // ini에 현재 설정된 포트값 쓰기
                                string strCOMM = "COMM" + m_nThNo;
                                cDefApi.WritePrivateProfileString(strCOMM, "CUR_PORT", Convert.ToString("" + m_nCurPort), cDefApp.GM_ENV_INI);

                                //접속 성공 로그 남기기 
                                InsertWcsLogPgr("", "[Thread_Doing] SC 번호 : " + m_strScNo + ", 연결포트 : " + m_nCurPort + " 접속 성공");

                                //접속이 성공하거나 시도횟수를 OVER하면 빠져나간다.
                                m_blConnectYn = true;
                                break;
                            }
                        }
                        //연결이 성공하면 빠져나가기.
                        if (m_blConnectYn == true)
                        {
                            break;
                        }
                    }
                    //Thread.Sleep(5000); 
                }

               // Thread.Sleep(5000); //2000

                if (m_msQPlc.m_bSocCon == true && m_msQPlc.m_bDBOpen == true)
                {
                    IsOpen = true;
                    MakeMsg_Imp("DB login Ok!", m_nThNo);

                    while (!m_bManualStop)
                    {

                        if (cDefApp.GM_STAT_MAIN == false)
                        {
                            throw new Exception("서비스 종료됨");
                        }
                        this.m_msQPlc.IsAscii = m_frmMain.IsAscii;
                        this.m_msQPlc.IsHex = m_frmMain.IsHex;

                        if (!ScReadStatus()) goto EXIT_LBL;

                        if (!SC_CMD_RQ_YN()) goto EXIT_LBL;

                        if (!SC_OD_RQ_YN()) goto EXIT_LBL;

                        Thread.Sleep(200); //2000
                    }
                }

            EXIT_LBL:
                {
                    if (m_bManualStop)
                    {
                        MakeMsg_Imp("수동 절단 지시로 통신을 해제합니다.", m_nThNo);
                        InsertWcsLogPgr("", "[Thread_Doing] 수동 절단 지시로 통신 해제 (SC " + m_strScNo + ")");

                        // @.수동 절단도 EQP_MST 통신상태를 'N' 으로 내린다.
                        //   (정상 폴링은 ScReadStatus 가 'Y' 로 유지하므로, 여기서 안 내리면
                        //    상위 시스템이 절단된 설비를 계속 접속중으로 오판한다)
                        Communication("N", m_strWh_typ, m_strEqmtTyp, m_strPlc_No);
                    }
                    SetErrorMsg("CoMM" + m_nThNo + " DB & Socket logoff!");
                    MakeMsg_Imp("DB & Socket logoff!", m_nThNo);
                }

            }
            catch (Exception ex)
            {
                MakeMsg_Error(ex.Message, m_nThNo);
            }

            IsOpen = false;
            m_msQPlc.Close(ref m_strRtnMsg);
            MakeMsg_Imp(m_strRtnMsg, m_nThNo);
            m_thThread = null;
            m_blConnectYn = false;
        }
        #endregion
        #endregion Thread_Doing

        #region[ScReadStatus] :: SC READ 후 변경 값 DB에 입력
        private bool ScReadStatus()
        {
            string strTitle = "[ScReadStatus]";

            try
            {
                byte[] byRxBuff = new byte[2000];
                int nReadLenth = 36; //D95~130
                int nReadAddress = 95; //D96부터

                /*
                 * 40개의 트랙식 읽음.
                 */
                MakeMsg(strTitle + "SC 통신", m_nThNo);

                //접속 직후 PLC측에 이전 세션이 남아 응답이 없을 수 있으므로 소켓 재접속을 포함하여 재시도한다.
                bool bReadOk = false;
                for (int nTry = 0; nTry < 3; nTry++)
                {
                    Array.Clear(byRxBuff, 0x00, byRxBuff.Length);
                    if (m_msQPlc.READ((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                            (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                            nReadAddress,
                            nReadLenth,
                            ref byRxBuff))
                    {
                        bReadOk = true;
                        break;
                    }

                    //응답이 없으면 소켓만 끊었다 다시 접속하여 PLC측 잔류 세션을 정리한 후 재시도한다.
                    string strSockMsg = "";
                    m_msQPlc.ThreadStop(ref strSockMsg);
                    Thread.Sleep(300);
                    if (!m_msQPlc.Connect(ref strSockMsg))
                    {
                        break;
                    }
                    MakeMsg_Imp(strTitle + "SC 응답없음 - 소켓 재접속 후 재시도 (" + (nTry + 1) + "/3)", m_nThNo);
                }

                if (!bReadOk)
                {
                    //설비 통신상태 업데이트(N)
                    Communication("N", m_strWh_typ, m_strEqmtTyp, m_strPlc_No);

                    throw new Exception("SC 상태 READ 실패(D" + nReadAddress + " ~ " + nReadLenth + " WORD, 3회 재시도) - 소켓 재접속 시도");
                }

                //설비 통신상태 업데이트(Y)
                Communication("Y", m_strWh_typ, m_strEqmtTyp, m_strPlc_No);

                int nReadLen = 72;

                MakeMsg("상태값 DB저장", m_nThNo);

                if (!ScDic.ContainsKey(m_nThNo))
                {
                    ScDic.Add(m_nThNo, new SCData()); //Key를 추가한다.
                }

                //최초 상태값 DIC에 넣기.
                if (ScDic[m_nThNo].ONLINE_MODE_RD == "" &&
                    ScDic[m_nThNo].AUTO_MODE_RD == "" &&
                    ScDic[m_nThNo].UCSTATUS_RD == "" &&
                    ScDic[m_nThNo].ACTIVE_MODE_RD == "" &&
                    ScDic[m_nThNo].ERR_CODE_RD == 0)
                {
                    int nSelCnt = 0;

                    strSql = "";
                    strSql += CRLF + "SELECT *                   ";
                    strSql += CRLF + "  FROM SC_DATA             ";
                    strSql += CRLF + " WHERE WH_TYP = :WH_TYP    ";
                    strSql += CRLF + "   AND PLC_NO = :PLC_NO    ";
                    strSql += CRLF + "   AND SC_NO  = :SC_NO     ";

                    m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                    m_msQPlc._pBdb.mComMain.Parameters.Clear();
                    m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                    m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                    m_msQPlc._pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR, 255).Value = m_strScNo;

                    nSelCnt = m_msQPlc._pBdb.ExcuteQry(strSql);

                    if (nSelCnt < 0)
                    {
                        MakeMsg_Error(strTitle + "최초 SC 정보 읽는 중 에러(SC_DATA)", m_nThNo);
                        return false;
                    }

                    ScDic[m_nThNo].ONLINE_MODE_RD = m_msQPlc._pBdb.mDtMain.Rows[0]["ONLINE_MODE_RD"].ToString();
                    ScDic[m_nThNo].AUTO_MODE_RD = m_msQPlc._pBdb.mDtMain.Rows[0]["AUTO_MODE_RD"].ToString();
                    ScDic[m_nThNo].UCSTATUS_RD = m_msQPlc._pBdb.mDtMain.Rows[0]["UCSTATUS_RD"].ToString();
                    ScDic[m_nThNo].ACTIVE_MODE_RD = m_msQPlc._pBdb.mDtMain.Rows[0]["ACTIVE_MODE_RD"].ToString();
                    ScDic[m_nThNo].ERR_CODE_RD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[0]["ERR_CODE_RD"].ToString());
                }

                //Hexa string 값으로 가져온다.
                string strCvHexVal = BytesToHexs(byRxBuff, 0, nReadLen);

                //Conveyor상태값 또는 BCR 값이 다를 때만 Update.
                if (ScDic[m_nThNo].HEXA_SC_VALUE != strCvHexVal)
                {
                    ScDic[m_nThNo].HEXA_SC_VALUE = strCvHexVal; //Dictionary 값을 변경한다.

                    //D95
                    int nONLINE_MODE_RD = (byRxBuff[1] << 8) + byRxBuff[0]; //지상반 동작모드 ONLINE/REMOTE
                    string strONLINE_MODE_RD = Convert.ToString("" + nONLINE_MODE_RD);
                    if (ScDic[m_nThNo].ONLINE_MODE_RD != strONLINE_MODE_RD)
                    {
                        ScDic[m_nThNo].ONLINE_MODE_RD = strONLINE_MODE_RD;
                        m_blHostSendYN = true;
                    }

                    //D96~D99 사용안함
                    //D96
                    int nSC_PLT_JOB_TYP_RD = (byRxBuff[3] << 8) + byRxBuff[2]; //SC PLT 정보(오버사이즈)
                    string strSC_PLT_JOB_TYP_RD = Convert.ToString("" + nSC_PLT_JOB_TYP_RD);

                    //D97
                    int nD97 = (byRxBuff[5] << 8) + byRxBuff[4]; //SPARE 

                    //D98
                    int nD98 = (byRxBuff[7] << 8) + byRxBuff[6]; //SPARE

                    //D99
                    int nD99 = (byRxBuff[9] << 8) + byRxBuff[8]; //사용금지

                    //D100
                    int nAUTO_MODE_RD = Convert.ToInt16(byRxBuff[10]);   //SC동작모드
                    string strAUTO_MODE_RD = Convert.ToString("" + nAUTO_MODE_RD);
                    if (ScDic[m_nThNo].AUTO_MODE_RD != strAUTO_MODE_RD)
                    {
                        ScDic[m_nThNo].AUTO_MODE_RD = strAUTO_MODE_RD;
                        m_blHostSendYN = true;
                    }

                    int nSENSOR_FK_RD = Convert.ToInt16(byRxBuff[11]);   //화물유무
                    string strSENSOR_FK_RD = Convert.ToString("" + nSENSOR_FK_RD);

                    //D101
                    int nUCSTATUS_RD = (byRxBuff[13] << 8) + byRxBuff[12]; //SC동작상태
                    string strUCSTATUS_RD = Convert.ToString("" + nUCSTATUS_RD);
                    if (ScDic[m_nThNo].UCSTATUS_RD != strUCSTATUS_RD)
                    {
                        ScDic[m_nThNo].UCSTATUS_RD = strUCSTATUS_RD;
                        m_blHostSendYN = true;
                    }

                    //D102
                    int nPOS_H_RD = (byRxBuff[15] << 8) + byRxBuff[14]; //현재 주행위치
                    string strPOS_H_RD = Convert.ToString("" + nPOS_H_RD);

                    //D103
                    int nPOS_V_RD = (byRxBuff[17] << 8) + byRxBuff[16]; //현재 승강위치
                    string strPOS_V_RD = Convert.ToString("" + nPOS_V_RD);

                    //D104
                    int nFORKPOS_FK1_RD = Convert.ToInt16(byRxBuff[18]); //포크#1 위치
                    string strFORKPOS_FK1_RD = Convert.ToString("" + nFORKPOS_FK1_RD);

                    int nFORKPOS_FK2_RD = Convert.ToInt16(byRxBuff[19]); //포크#2 위치
                    string strFORKPOS_FK2_RD = Convert.ToString("" + nFORKPOS_FK2_RD);

                    //D105
                    int nERR_CODE_RD = (byRxBuff[21] << 8) + byRxBuff[20]; //에러코드
                    string strERR_CODE_RD = ((byRxBuff[21] << 8) + byRxBuff[20]).ToString("0000");

                    //D106
                    int nERR_STA_FK1_RD = Convert.ToInt16(byRxBuff[22]); //FORK#1 이중입고 여부.
                    string strERR_STA_FK1_RD = Convert.ToString("" + nERR_STA_FK1_RD);

                    int nERR_STA_FK2_RD = Convert.ToInt16(byRxBuff[23]); //FORK#1 이중입고 여부.
                    string strERR_STA_FK2_RD = Convert.ToString("" + nERR_STA_FK2_RD);

                    //int nSC_FORK2_REASS_STAT = (byRxBuff[23] >> 0) & 0x01; //FORK#2 에러발생 재지정 READY.
                    //string strSC_FORK2_REASS_STAT = Convert.ToString("" + nSC_FORK2_REASS_STAT);

                    //D107 - SKI 미사용
                    //int nCV_STN_JOB_YON1 = (byRxBuff[25] << 8) + byRxBuff[24]; //CV작업대 포크작업유무
                    //string strCV_STN_JOB_YON1 = Convert.ToString("" + nCV_STN_JOB_YON1);

                    //D108 - SKI 미사용
                    //int nCV_STN_JOB_YON2 = (byRxBuff[27] << 8) + byRxBuff[26]; //CV작업대 포크작업유무(SPARE)
                    //string strCV_STN_JOB_YON2 = Convert.ToString("" + nCV_STN_JOB_YON2);

                    //D109
                    int nACTIVE_MODE_RD = (byRxBuff[29] << 8) + byRxBuff[28]; //기상반 ACTIVE 상태
                    string strACTIVE_MODE_RD = Convert.ToString("" + nACTIVE_MODE_RD);
                    if (ScDic[m_nThNo].ACTIVE_MODE_RD != strACTIVE_MODE_RD)
                    {
                        ScDic[m_nThNo].ACTIVE_MODE_RD = strACTIVE_MODE_RD;
                        m_blHostSendYN = true;
                    }

                    //D110
                    int nCOMPLETE_RD = (byRxBuff[31] << 8) + byRxBuff[30]; //작업완료표시
                    string strCOMPLETE_RD = Convert.ToString("" + nCOMPLETE_RD);

                    //D111
                    int nJOB_TYP_RD = (byRxBuff[33] << 8) + byRxBuff[32]; //SC 작업구분
                    string strJOB_TYP_RD = Convert.ToString("" + nJOB_TYP_RD);

                    //D112
                    int nLUGG_NO_FK1_RD = (byRxBuff[35] << 8) + byRxBuff[34]; //FORK1 작업번호
                    //string strLUGG_NO_FK1_RD = Convert.ToString("" + nLUGG_NO_FK1_RD);
                    string strLUGG_NO_FK1_RD = nLUGG_NO_FK1_RD.ToString("0000");

                    //D113
                    int nSTART_BANK_FK1_RD = (byRxBuff[37] << 8) + byRxBuff[36]; //FORK1 출발지 열
                    string strSTART_BANK_FK1_RD = Convert.ToString("" + nSTART_BANK_FK1_RD);

                    //D114
                    int nSTART_BAY_FK1_RD = (byRxBuff[39] << 8) + byRxBuff[38]; //FORK1 출발지 행
                    string strSTART_BAY_FK1_RD = Convert.ToString("" + nSTART_BAY_FK1_RD);

                    //D115
                    int nSTART_LEVEL_FK1_RD = (byRxBuff[41] << 8) + byRxBuff[40]; //FORK1 출발지 단
                    string strSTART_LEVEL_FK1_RD = Convert.ToString("" + nSTART_LEVEL_FK1_RD);

                    //D116
                    int nSTART_HSPOS_FK1_RD = (byRxBuff[43] << 8) + byRxBuff[42]; //FORK1 출발지 작업대
                    string strSTART_HSPOS_FK1_RD = Convert.ToString("" + nSTART_HSPOS_FK1_RD);

                    //D117
                    int nDEST_BANK_FK1_RD = (byRxBuff[45] << 8) + byRxBuff[44]; //FORK1 도착지 열
                    string strDEST_BANK_FK1_RD = Convert.ToString("" + nDEST_BANK_FK1_RD);

                    //D118
                    int nDEST_BAY_FK1_RD = (byRxBuff[47] << 8) + byRxBuff[46]; //FORK1 도착지 행
                    string strDEST_BAY_FK1_RD = Convert.ToString("" + nDEST_BAY_FK1_RD);

                    //D119
                    int nDEST_LEVEL_FK1_RD = (byRxBuff[49] << 8) + byRxBuff[48]; //FORK1 도착지 단
                    string strDEST_LEVEL_FK1_RD = Convert.ToString("" + nDEST_LEVEL_FK1_RD);

                    //D120
                    int nDEST_HSPOS_FK1_RD = (byRxBuff[51] << 8) + byRxBuff[50]; //FORK1 도착지 작업대
                    string strDEST_HSPOS_FK1_RD = Convert.ToString("" + nDEST_HSPOS_FK1_RD);

                    //D121
                    int nUSE_FK_RD = (byRxBuff[53] << 8) + byRxBuff[52]; //???
                    string strUSE_FK_RD = Convert.ToString("" + nUSE_FK_RD);

                    //D122
                    int nLUGG_NO_FK2_RD = (byRxBuff[55] << 8) + byRxBuff[54]; //FORK2 작업번호
                    //string strLUGG_NO_FK2_RD = Convert.ToString("" + nLUGG_NO_FK2_RD);
                    string strLUGG_NO_FK2_RD = nLUGG_NO_FK2_RD.ToString("0000");

                    //D123
                    int nSTART_BANK_FK2_RD = (byRxBuff[57] << 8) + byRxBuff[56]; //FORK2 출발지 열
                    string strSTART_BANK_FK2_RD = Convert.ToString("" + nSTART_BANK_FK2_RD);

                    //D124
                    int nSTART_BAY_FK2_RD = (byRxBuff[59] << 8) + byRxBuff[58]; //FORK2 출발지 행
                    string strSTART_BAY_FK2_RD = Convert.ToString("" + nSTART_BAY_FK2_RD);

                    //D125
                    int nSTART_LEVEL_FK2_RD = (byRxBuff[61] << 8) + byRxBuff[60]; //FORK2 출발지 단
                    string strSTART_LEVEL_FK2_RD = Convert.ToString("" + nSTART_LEVEL_FK2_RD);

                    //D126
                    int nSTART_HSPOS_FK2_RD = (byRxBuff[63] << 8) + byRxBuff[62]; //FORK2 출발지 작업대
                    string strSTART_HSPOS_FK2_RD = Convert.ToString("" + nSTART_HSPOS_FK2_RD);

                    //D127
                    int nDEST_BANK_FK2_RD = (byRxBuff[65] << 8) + byRxBuff[64]; //FORK2 도착지 열
                    string strDEST_BANK_FK2_RD = Convert.ToString("" + nDEST_BANK_FK2_RD);

                    //D128
                    int nDEST_BAY_FK2_RD = (byRxBuff[67] << 8) + byRxBuff[66]; //FORK2 도착지 행
                    string strDEST_BAY_FK2_RD = Convert.ToString("" + nDEST_BAY_FK2_RD);

                    //D129
                    int nDEST_LEVEL_FK2_RD = (byRxBuff[69] << 8) + byRxBuff[68]; //FORK2 도착지 단
                    string strDEST_LEVEL_FK2_RD = Convert.ToString("" + nDEST_LEVEL_FK2_RD);

                    //D130
                    int nDEST_HSPOS_FK2_RD = (byRxBuff[71] << 8) + byRxBuff[70]; //FORK2 도착지 작업대
                    string strDEST_HSPOS_FK2_RD = Convert.ToString("" + nDEST_HSPOS_FK2_RD);

                    //에러코드가 있고 전에 에러코드와 다를때만.
                    if (ScDic[m_nThNo].ERR_CODE_RD != nERR_CODE_RD)
                    {
                        //에러 이력 남기기
                        if (!InsertEQMT_ERR_LOG(m_strWh_typ, m_strEqmtTyp, m_strMcNo, strERR_CODE_RD, strLUGG_NO_FK1_RD))
                        {
                            return false;
                        }

                        //에러코드를 SET한다.
                        ScDic[m_nThNo].ERR_CODE_RD = nERR_CODE_RD;
                        m_blHostErrSendYN = true;
                    }

                    //TRACK정보 UPDATE.
                    
                    if (!UpdateSC_DATA(strONLINE_MODE_RD       
                                       ,strAUTO_MODE_RD      
                                       ,strSENSOR_FK_RD      
                                       ,strUCSTATUS_RD       
                                       ,strPOS_H_RD          
                                       ,strPOS_V_RD          
                                       ,strFORKPOS_FK1_RD    
                                       ,strFORKPOS_FK2_RD    
                                       ,strERR_CODE_RD       
                                       ,strERR_STA_FK1_RD    
                                       ,strERR_STA_FK2_RD    
                                       ,strACTIVE_MODE_RD    
                                       ,strCOMPLETE_RD       
                                       ,strJOB_TYP_RD        
                                       ,strLUGG_NO_FK1_RD    
                                       ,strSTART_BANK_FK1_RD 
                                       ,strSTART_HSPOS_FK1_RD
                                       ,strDEST_BANK_FK1_RD  
                                       ,strDEST_HSPOS_FK1_RD 
                                       ,strUSE_FK_RD         
                                       ,strLUGG_NO_FK2_RD    
                                       ,strSTART_BANK_FK2_RD 
                                       ,strSTART_HSPOS_FK2_RD
                                       ,strDEST_BANK_FK2_RD  
                                       ,strDEST_HSPOS_FK2_RD 
                                       ,strSTART_BAY_FK1_RD  
                                       ,strSTART_LEVEL_FK1_RD
                                       ,strSTART_BAY_FK2_RD  
                                       ,strSTART_LEVEL_FK2_RD
                                       ,strDEST_BAY_FK1_RD   
                                       ,strDEST_LEVEL_FK1_RD 
                                       ,strDEST_BAY_FK2_RD   
                                       ,strDEST_LEVEL_FK2_RD
                                       , strSC_PLT_JOB_TYP_RD))
                    {
                        m_blHostSendYN = false;
                        m_blHostErrSendYN = false;
                        return false;
                    }
                }
                m_blHostSendYN = false;
                m_blHostErrSendYN = false;
                //설비 통신상태 업데이트
                Communication("Y", m_strWh_typ, m_strEqmtTyp, m_strPlc_No);
            }
            catch (Exception ex)
            {
                //EQP_MST에 CONNECTION = 'N' 업데이트
                Communication("N", m_strWh_typ, m_strEqmtTyp, m_strPlc_No);
                //LOG남기기
                InsertWcsLogPgr(m_strScNo, strTitle + " SC_NO : [" + m_strScNo + "] 데이터 읽기 중 에러");

                //화면표시
                SetErrorMsg("Comm" + m_nThNo + strTitle + "Exception Error" + ex.Message);
                MakeMsg_Error(strTitle + m_strScNo + "Exception Error" + ex.Message, m_nThNo);

                m_blHostSendYN = false;
                m_blHostErrSendYN = false;
                return false;
            }
            return true;
        }
        #endregion

        #region [SC_CMD_RQ_YN] :: SC_DATA에서 CMD_RQ_YN = 'Y'인거 찾아서 CMD 별 수행
        private bool SC_CMD_RQ_YN()
        {
            string strTitle = "[SC_CMD_RQ_YN]";

            try
            {
                byte[] byTxBuff = new byte[1000];

                int nUpdCount = 0;

                //요청 조회
                strSql = "";
                strSql += CRLF + "SELECT SD.*                                          ";
                strSql += CRLF + "  FROM SC_DATA SD                                    ";
                strSql += CRLF + " WHERE SD.WH_TYP = :WH_TYP                           ";
                strSql += CRLF + "   AND SD.PLC_NO = :PLC_NO                           ";
                strSql += CRLF + "   AND SD.SC_NO  = :SC_NO                            ";
                strSql += CRLF + "   AND SD.CMD_RQ_YN = 'Y'                            ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR, 255).Value = m_strScNo;

                nUpdCount = m_msQPlc._pBdb.ExcuteQry(strSql);

                if (nUpdCount < 0)
                {
                    MakeMsg_Error(strTitle + "SC정보 읽는 중 에러(SC_DATA)", m_nThNo);
                    return false;
                }


                for (int nRows = 0; nRows < nUpdCount; nRows++)
                {
                    string strWriteMsg = "";

                    //변수설정
                    string strCMD_RQ_ID = Convert.ToString("" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["CMD_RQ_ID"]);


                    int nODVal = 0;
                    int nWriteLen = 0;
                    int nWriteAddr = 0;
                    Array.Clear(byTxBuff, 0, byTxBuff.Length);

                    switch (strCMD_RQ_ID)
                    {
                        //비상정지.
                        case "EMERGENCY":
                            nODVal = 1;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);

                            strWriteMsg = "EMERGENCY 명령";
                            break;

                        //ACTIVE
                        case "ACTIVE":
                            nODVal = 2;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);

                            strWriteMsg = "ACTIVE 명령";
                            break;

                        //PAUSE
                        case "PAUSE":
                            nODVal = 4;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);

                            strWriteMsg = "PAUSE 명령";
                            break;

                        //ERROR RESET
                        case "RESET":
                            nODVal = 8;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);

                            strWriteMsg = "ERROR RESET 명령";
                            break;

                        //FORK DATA 삭제.
                        case "DELFK1":
                            //1번 포크만
                            nODVal = 16;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            strWriteMsg = "FORK#1 DATA삭제 명령";
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);
                            break;

                        case "DELFK2":
                            //2번 포크만
                            nODVal = 32;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            strWriteMsg = "FORK#2 DATA삭제 명령";
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);
                            break;

                        case "DELFK12":
                            //양쪽 포크만
                            nODVal = 64;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            strWriteMsg = "FORK#1,2 DATA삭제 명령";
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);
                            break;

                        //지상반 ONLINE
                        case "ONL":
                            nODVal = 128;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);
                            strWriteMsg = "지상반 ONLINE 명령";
                            break;

                        //지상반 OFFLINE
                        case "OFL":
                            nODVal = 256;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);
                            strWriteMsg = "지상반 OFFLINE 명령";
                            break;

                        //지상반 REMOTE
                        case "REM":
                            nODVal = 512;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);
                            strWriteMsg = "지상반 REMOTE 명령";
                            break;

                        //지상반 ERROR
                        case "GRE":
                            nODVal = 1024;
                            nWriteLen = 1;
                            nWriteAddr = 199;
                            byTxBuff[0] = (byte)(nODVal >> 0);
                            byTxBuff[1] = (byte)(nODVal >> 8);
                            strWriteMsg = "지상반 ERROR 명령";
                            break;
                        case "FCMP":
                            //ㅇ로그 남기기
                            m_strLogMsg = strTitle + " SC_NO : [" + m_strScNo + "] CMD_RQ_ID : [" + strCMD_RQ_ID + "] 클라이언트 명령 실행.";
                            if (!InsertWcsLogPgr(m_strScNo, m_strLogMsg))
                            {
                                return false;
                            }
                            return true;
                        default:
                            //ㅇ로그 남기기
                            m_strLogMsg = strTitle + " SC_NO : [" + m_strScNo + "] CMD_RQ_ID : [" + strCMD_RQ_ID + "] 미정의된 CMD.";
                            if (!InsertWcsLogPgr(m_strScNo, m_strLogMsg))
                            {
                                return false;
                            }
                            return true;
                    }

                    //쓸 내용 및 메세지 표시.
                    MakeMsg_Imp(strTitle + strWriteMsg, m_nThNo);

                    if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                       (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                       nWriteAddr,
                                                       nWriteLen,
                                                       byTxBuff) == false)
                    {
                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Error(strTitle + "SC정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nThNo);
                            MakeMsg_Error(strTitle + "SC정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nThNo);
                        }
                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Error(strTitle + "SC정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nThNo);
                            MakeMsg_Error(strTitle + "SC정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nThNo);
                        }

                        m_strLogMsg = strTitle + " SC_NO : [" + m_strScNo + "] CMD_RQ_ID : [" + strCMD_RQ_ID + "] 커맨드 지시 실패";
                        if (!InsertWcsLogPgr(m_strScNo, m_strLogMsg))
                        {
                            return false;
                        }

                        return false;
                    }
                    if (this.m_msQPlc.IsHex)
                    {
                        MakeMsg_Imp(strTitle + "SC정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nThNo);
                        MakeMsg_Imp(strTitle + "SC정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nThNo);
                    }
                    if (this.m_msQPlc.IsAscii)
                    {
                        MakeMsg_Imp(strTitle + "SC정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nThNo);
                        MakeMsg_Imp(strTitle + "SC정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nThNo);
                    }

                    //성공 로그 남기기
                    m_strLogMsg = strTitle + " SC_NO : [" + m_strScNo + "] CMD_RQ_ID : [" + strCMD_RQ_ID + "] 커맨드 지시 성공";
                    if (!InsertWcsLogPgr(m_strScNo, m_strLogMsg))
                    {
                        return false;
                    }


                    //CMD_RQ_YN = 'N' 업데이트
                    if (!UpdateSC_CMD_RQ_YN(strCMD_RQ_ID))
                    {
                        return false;
                    }


                }

                return true;
            }
            catch (Exception ex)
            {
                string msg = strTitle + " " + ex.Message;
                SetErrorMsg("Comm" + m_nThNo + ex.Message);
                MakeMsg_Error(ex.Message, m_nThNo);
                InsertWcsLogPgr(m_strScNo, msg);
                return false;
            }
        }
        #endregion

        #region [SC_OD_RQ_YN] :: SC_DATA에서 OD_RQ_YN = 'Y'인거 찾아서 SC 지시
        public bool SC_OD_RQ_YN()
        {
            string strTitle = "[SC_OD_RQ_YN]";

            try
            {
                byte[] byTxBuff = new byte[1000];

                int nUpdCount = 0;
                string strWriteMsg = "";

                //요청 조회
                strSql = "";
                strSql += CRLF + "SELECT SD.*                                          ";
                strSql += CRLF + "  FROM SC_DATA SD                                    ";
                strSql += CRLF + " WHERE SD.WH_TYP = :WH_TYP                           ";
                strSql += CRLF + "   AND SD.PLC_NO = :PLC_NO                           ";
                strSql += CRLF + "   AND SD.SC_NO  = :SC_NO                            ";
                strSql += CRLF + "   AND SD.OD_RQ_YN = 'Y'                             ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR, 255).Value = m_strScNo;


                nUpdCount = m_msQPlc._pBdb.ExcuteQry(strSql);

                if (nUpdCount < 0)
                {
                    MakeMsg_Error(strTitle + "SC 정보 읽는 중 에러(SC_DATA)", m_nThNo);
                    return false;
                }


                for (int nRows = 0; nRows < nUpdCount; nRows++)
                {
                    int n_JOB_TYP_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["JOB_TYP_OD"].ToString());
                    int n_LUGG_NO_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["LUGG_NO_FK1_OD"].ToString());
                    string str_LUGG_NO_FK1_OD = n_LUGG_NO_FK1_OD.ToString("0000");
                    int n_START_BANK_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["START_BANK_FK1_OD"].ToString());
                    int n_START_BAY_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["START_BAY_FK1_OD"].ToString());
                    int n_START_LEVEL_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["START_LEVEL_FK1_OD"].ToString());
                    int n_START_HSPOS_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["START_HSPOS_FK1_OD"].ToString());
                    int n_DEST_BANK_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_BANK_FK1_OD"].ToString());
                    int n_DEST_BAY_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_BAY_FK1_OD"].ToString());
                    int n_DEST_LEVEL_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_LEVEL_FK1_OD"].ToString());
                    int n_DEST_HSPOS_FK1_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_HSPOS_FK1_OD"].ToString());
                    int n_USE_FK_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["USE_FK_OD"].ToString());
                    int n_LUGG_NO_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["LUGG_NO_FK2_OD"].ToString());
                    string str_LUGG_NO_FK2_OD = n_LUGG_NO_FK2_OD.ToString("0000");
                    int n_START_BANK_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["START_BANK_FK2_OD"].ToString());
                    int n_START_BAY_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["START_BAY_FK2_OD"].ToString());
                    int n_START_LEVEL_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["START_LEVEL_FK2_OD"].ToString());
                    int n_START_HSPOS_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["START_HSPOS_FK2_OD"].ToString());
                    int n_DEST_BANK_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_BANK_FK2_OD"].ToString());
                    int n_DEST_BAY_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_BAY_FK2_OD"].ToString());
                    int n_DEST_LEVEL_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_LEVEL_FK2_OD"].ToString());
                    int n_DEST_HSPOS_FK2_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_HSPOS_FK2_OD"].ToString());
                    int n_SC_FIRE_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["SC_FIRE_OD"].ToString()); //중국 SKI사용
                    int n_SC_PLT_JOB_TYP_OD = Convert.ToInt32(0 + m_msQPlc._pBdb.mDtMain.Rows[nRows]["SC_PLT_JOB_TYP_OD"].ToString()); //


                    strWriteMsg = "FORK#1,2 작업지시 ";

                    int nWriteLen = 29;
                    int nWriteAddr = 171;

                    //작업구분[D171][01:입고, 02:출고, 03:직출고, 04:재배치, 05:홈복귀][
                    byTxBuff[0] = (byte)(n_JOB_TYP_OD >> 0);
                    byTxBuff[1] = (byte)(n_JOB_TYP_OD >> 8);
                    strWriteMsg += "SC_JOB_TYP [" + n_JOB_TYP_OD + "] ";

                    //포크#1작업번호[D172]
                    byTxBuff[2] = (byte)(n_LUGG_NO_FK1_OD >> 0);
                    byTxBuff[3] = (byte)(n_LUGG_NO_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_LUG_NO [" + n_LUGG_NO_FK1_OD + "] ";

                    //포크#1출발지 열(Bank)[D173]
                    byTxBuff[4] = (byte)(n_START_BANK_FK1_OD >> 0);
                    byTxBuff[5] = (byte)(n_START_BANK_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_FROM_BANK [" + n_START_BANK_FK1_OD + "] ";

                    //포크#1출발지 행(Bay)[D174]
                    byTxBuff[6] = (byte)(n_START_BAY_FK1_OD >> 0);
                    byTxBuff[7] = (byte)(n_START_BAY_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_FROM_BAY [" + n_START_BAY_FK1_OD + "] ";

                    //포크#1출발지 단(Level)[D175]
                    byTxBuff[8] = (byte)(n_START_LEVEL_FK1_OD >> 0);
                    byTxBuff[9] = (byte)(n_START_LEVEL_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_FROM_LEV [" + n_START_LEVEL_FK1_OD + "] ";

                    //포크#1출발지 작업대[D176]
                    byTxBuff[10] = (byte)(n_START_HSPOS_FK1_OD >> 0);
                    byTxBuff[11] = (byte)(n_START_HSPOS_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_FROM_SITE [" + n_START_HSPOS_FK1_OD + "] ";

                    //포크#1도착지 열(Bank)[D177]
                    byTxBuff[12] = (byte)(n_DEST_BANK_FK1_OD >> 0);
                    byTxBuff[13] = (byte)(n_DEST_BANK_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_TO_BANK [" + n_DEST_BANK_FK1_OD + "] ";

                    //포크#1도착지 행(Bay)[D178]
                    byTxBuff[14] = (byte)(n_DEST_BAY_FK1_OD >> 0);
                    byTxBuff[15] = (byte)(n_DEST_BAY_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_TO_BAY [" + n_DEST_BAY_FK1_OD + "] ";

                    //포크#1도착지 단(Level)[D179]
                    byTxBuff[16] = (byte)(n_DEST_LEVEL_FK1_OD >> 0);
                    byTxBuff[17] = (byte)(n_DEST_LEVEL_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_TO_LEV [" + n_DEST_LEVEL_FK1_OD + "] ";

                    //포크#1도착지 작업대[D180]
                    byTxBuff[18] = (byte)(n_DEST_HSPOS_FK1_OD >> 0);
                    byTxBuff[19] = (byte)(n_DEST_HSPOS_FK1_OD >> 8);
                    strWriteMsg += "SC_FORK1_TO_SITE [" + n_DEST_HSPOS_FK1_OD + "] ";

                    //포크사용 구분[D181] [0:포크#1만 사용, 1:포크#1~포크#2 동시사용, 2:포크#2만 사용 ]
                    byTxBuff[20] = (byte)(n_USE_FK_OD >> 0);
                    byTxBuff[21] = (byte)(n_USE_FK_OD >> 8);
                    strWriteMsg += "FORK사용구분 [1] ";

                    //포크#2작업번호[D182]
                    byTxBuff[22] = (byte)(n_LUGG_NO_FK2_OD >> 0);
                    byTxBuff[23] = (byte)(n_LUGG_NO_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_LUG_NO [" + n_LUGG_NO_FK2_OD + "] ";

                    //포크#2출발지 열(Bank)[D183]
                    byTxBuff[24] = (byte)(n_START_BANK_FK2_OD >> 0);
                    byTxBuff[25] = (byte)(n_START_BANK_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_FROM_BANK [" + n_START_BANK_FK2_OD + "] ";

                    //포크#2출발지 행(Bay)[D184]
                    byTxBuff[26] = (byte)(n_START_BAY_FK2_OD >> 0);
                    byTxBuff[27] = (byte)(n_START_BAY_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_FROM_BAY [" + n_START_BAY_FK2_OD + "] ";

                    //포크#2출발지 단(Level)[D185]
                    byTxBuff[28] = (byte)(n_START_LEVEL_FK2_OD >> 0);
                    byTxBuff[29] = (byte)(n_START_LEVEL_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_FROM_LEV [" + n_START_LEVEL_FK2_OD + "] ";

                    //포크#2출발지 작업대[D186]
                    byTxBuff[30] = (byte)(n_START_HSPOS_FK2_OD >> 0);
                    byTxBuff[31] = (byte)(n_START_HSPOS_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_FROM_SITE [" + n_START_HSPOS_FK2_OD + "] ";

                    //포크#2도착지 열(Bank)[D187]
                    byTxBuff[32] = (byte)(n_DEST_BANK_FK2_OD >> 0);
                    byTxBuff[33] = (byte)(n_DEST_BANK_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_TO_BANK [" + n_DEST_BANK_FK2_OD + "] ";

                    //포크#2도착지 행(Bay)[D188]
                    byTxBuff[34] = (byte)(n_DEST_BAY_FK2_OD >> 0);
                    byTxBuff[35] = (byte)(n_DEST_BAY_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_TO_BAY [" + n_DEST_BAY_FK2_OD + "] ";

                    //포크#2도착지 단(Level)[D189]
                    byTxBuff[36] = (byte)(n_DEST_LEVEL_FK2_OD >> 0);
                    byTxBuff[37] = (byte)(n_DEST_LEVEL_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_TO_LEV [" + n_DEST_LEVEL_FK2_OD + "] ";

                    //포크#2도착지 작업대[D190]
                    byTxBuff[38] = (byte)(n_DEST_HSPOS_FK2_OD >> 0);
                    byTxBuff[39] = (byte)(n_DEST_HSPOS_FK2_OD >> 8);
                    strWriteMsg += "SC_FORK2_TO_SITE [" + n_DEST_HSPOS_FK2_OD + "] ";

                    //작업데이트Write Flag[D191][1로 쓰면 기상반에서 0으로 리셋] [D191]
                    byTxBuff[40] = (byte)(1 >> 0);
                    byTxBuff[41] = (byte)(1 >> 8);
                    strWriteMsg += "작업DATA WRITE [1] ";

                    //[D192]
                    byTxBuff[42] = (byte)(n_SC_PLT_JOB_TYP_OD >> 0);
                    byTxBuff[43] = (byte)(n_SC_PLT_JOB_TYP_OD >> 8);
                    strWriteMsg += "SC_PLT_JOB_TYP_OD [" + n_SC_PLT_JOB_TYP_OD + "] ";

                    //[D193]
                    byTxBuff[44] = (byte)(0 >> 0);
                    byTxBuff[45] = (byte)(0 >> 8);
                    strWriteMsg += "작업DATA WRITE [0] ";

                    //[D194]
                    byTxBuff[46] = (byte)(0 >> 0);
                    byTxBuff[47] = (byte)(0 >> 8);
                    strWriteMsg += "작업DATA WRITE [0] ";

                    //[D195]
                    byTxBuff[48] = (byte)(0 >> 0);
                    byTxBuff[49] = (byte)(0 >> 8);
                    strWriteMsg += "작업DATA WRITE [0] ";

                    //[D196]
                    byTxBuff[50] = (byte)(0 >> 0);
                    byTxBuff[51] = (byte)(0 >> 8);
                    strWriteMsg += "작업DATA WRITE [0] ";

                    //[D197]
                    byTxBuff[52] = (byte)(0 >> 0);
                    byTxBuff[53] = (byte)(0 >> 8);
                    strWriteMsg += "작업DATA WRITE [0] ";

                    //[D198]
                    byTxBuff[54] = (byte)(0 >> 0);
                    byTxBuff[55] = (byte)(0 >> 8);
                    strWriteMsg += "작업DATA WRITE [0] ";

                    //[D199]
                    byTxBuff[56] = (byte)(0 >> 0);
                    byTxBuff[57] = (byte)(0 >> 8);
                    strWriteMsg += "작업DATA WRITE [0] ";

                    //쓸 내용 및 메세지 표시.
                    MakeMsg_Imp(strTitle + strWriteMsg, m_nThNo);

                    if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                       (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                       nWriteAddr,
                                                       nWriteLen,
                                                       byTxBuff) == false)
                    {
                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Error(strTitle + "SC정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nThNo);
                            MakeMsg_Error(strTitle + "SC정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nThNo);
                        }
                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Error(strTitle + "SC정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nThNo);
                            MakeMsg_Error(strTitle + "SC정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nThNo);
                        }

                        m_strLogMsg = strTitle + " SC_NO : [" + m_strScNo + "], 작업번호 : [" + str_LUGG_NO_FK1_OD + "], 작업구분 : [" + Convert.ToString("" + n_JOB_TYP_OD) + "], SC 지시 실패";
                        if (!InsertWcsLogPgr(m_strScNo, m_strLogMsg))
                        {
                            return false;
                        }

                        return false;
                    }
                    if (this.m_msQPlc.IsHex)
                    {
                        MakeMsg_Imp(strTitle + "SC정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nThNo);
                        MakeMsg_Imp(strTitle + "SC정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nThNo);
                    }
                    if (this.m_msQPlc.IsAscii)
                    {
                        MakeMsg_Imp(strTitle + "SC정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nThNo);
                        MakeMsg_Imp(strTitle + "SC정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nThNo);
                    }

                    //성공 로그 남기기
                    m_strLogMsg = strTitle + " SC_NO : [" + m_strScNo + "], 작업번호 : [" + str_LUGG_NO_FK1_OD + "], 작업구분 : [" + Convert.ToString("" + n_JOB_TYP_OD) + "], SC 지시 성공";
                    if (!InsertWcsLogPgr(m_strScNo, m_strLogMsg))
                    {
                        return false;
                    }

                    //OD_RQ_YN = 'N' 업데이트
                    if (!UpdateSC_OD_RQ_YN(str_LUGG_NO_FK1_OD, "0"))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                string msg = strTitle + " " + ex.Message;
                SetErrorMsg("Comm" + m_nThNo + ex.Message);
                MakeMsg_Error(ex.Message, m_nThNo);
                InsertWcsLogPgr(m_strScNo, msg);
                return false;
            }
        }
        #endregion

        #region [UpdateSC_DATA] :: SC_DATA 업데이트 함수
        public bool UpdateSC_DATA(string  strONLINE_MODE_RD       
                                  ,string strAUTO_MODE_RD      
                                  ,string strSENSOR_FK_RD      
                                  ,string strUCSTATUS_RD       
                                  ,string strPOS_H_RD          
                                  ,string strPOS_V_RD          
                                  ,string strFORKPOS_FK1_RD    
                                  ,string strFORKPOS_FK2_RD    
                                  ,string strERR_CODE_RD       
                                  ,string strERR_STA_FK1_RD    
                                  ,string strERR_STA_FK2_RD    
                                  ,string strACTIVE_MODE_RD    
                                  ,string strCOMPLETE_RD       
                                  ,string strJOB_TYP_RD        
                                  ,string strLUGG_NO_FK1_RD    
                                  ,string strSTART_BANK_FK1_RD 
                                  ,string strSTART_HSPOS_FK1_RD
                                  ,string strDEST_BANK_FK1_RD  
                                  ,string strDEST_HSPOS_FK1_RD 
                                  ,string strUSE_FK_RD         
                                  ,string strLUGG_NO_FK2_RD    
                                  ,string strSTART_BANK_FK2_RD 
                                  ,string strSTART_HSPOS_FK2_RD
                                  ,string strDEST_BANK_FK2_RD  
                                  ,string strDEST_HSPOS_FK2_RD 
                                  ,string strSTART_BAY_FK1_RD  
                                  ,string strSTART_LEVEL_FK1_RD
                                  ,string strSTART_BAY_FK2_RD  
                                  ,string strSTART_LEVEL_FK2_RD
                                  ,string strDEST_BAY_FK1_RD   
                                  ,string strDEST_LEVEL_FK1_RD 
                                  ,string strDEST_BAY_FK2_RD   
                                  ,string strDEST_LEVEL_FK2_RD
                                  , string strSC_PLT_JOB_TYP_RD)
		{
            string strTitle = "[UpdateSC_DATA] ";

            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + " UPDATE SC_DATA                                           ";
                strSql += CRLF + "    SET ONLINE_MODE_RD           = :ONLINE_MODE_RD        ";
                strSql += CRLF + "        ,AUTO_MODE_RD            = :AUTO_MODE_RD          ";
                strSql += CRLF + "        ,SENSOR_FK_RD            = :SENSOR_FK_RD          ";
                strSql += CRLF + "        ,UCSTATUS_RD             = :UCSTATUS_RD           ";
                strSql += CRLF + "        ,POS_H_RD                = :POS_H_RD              ";
                strSql += CRLF + "        ,POS_V_RD                = :POS_V_RD              ";
                strSql += CRLF + "        ,FORKPOS_FK1_RD          = :FORKPOS_FK1_RD        ";
                strSql += CRLF + "        ,FORKPOS_FK2_RD          = :FORKPOS_FK2_RD        ";
                strSql += CRLF + "        ,ERR_CODE_RD             = :ERR_CODE_RD           ";
                strSql += CRLF + "        ,ERR_STA_FK1_RD          = :ERR_STA_FK1_RD        ";
                strSql += CRLF + "        ,ERR_STA_FK2_RD          = :ERR_STA_FK2_RD        ";
                strSql += CRLF + "        ,ACTIVE_MODE_RD          = :ACTIVE_MODE_RD        ";
                strSql += CRLF + "        ,COMPLETE_RD             = :COMPLETE_RD           ";
                strSql += CRLF + "        ,JOB_TYP_RD              = :JOB_TYP_RD            ";
                strSql += CRLF + "        ,LUGG_NO_FK1_RD          = :LUGG_NO_FK1_RD        ";
                strSql += CRLF + "        ,START_BANK_FK1_RD       = :START_BANK_FK1_RD     ";
                strSql += CRLF + "        ,START_HSPOS_FK1_RD      = :START_HSPOS_FK1_RD    ";
                strSql += CRLF + "        ,DEST_BANK_FK1_RD        = :DEST_BANK_FK1_RD      ";
                strSql += CRLF + "        ,DEST_HSPOS_FK1_RD       = :DEST_HSPOS_FK1_RD     ";
                strSql += CRLF + "        ,USE_FK_RD               = :USE_FK_RD             ";
                strSql += CRLF + "        ,LUGG_NO_FK2_RD          = :LUGG_NO_FK2_RD        ";
                strSql += CRLF + "        ,START_BANK_FK2_RD       = :START_BANK_FK2_RD     ";
                strSql += CRLF + "        ,START_HSPOS_FK2_RD      = :START_HSPOS_FK2_RD    ";
                strSql += CRLF + "        ,DEST_BANK_FK2_RD        = :DEST_BANK_FK2_RD      ";
                strSql += CRLF + "        ,DEST_HSPOS_FK2_RD       = :DEST_HSPOS_FK2_RD     ";
                strSql += CRLF + "        ,START_BAY_FK1_RD        = :START_BAY_FK1_RD      ";
                strSql += CRLF + "        ,START_LEVEL_FK1_RD      = :START_LEVEL_FK1_RD    ";
                strSql += CRLF + "        ,START_BAY_FK2_RD        = :START_BAY_FK2_RD      ";
                strSql += CRLF + "        ,START_LEVEL_FK2_RD      = :START_LEVEL_FK2_RD    ";
                strSql += CRLF + "        ,DEST_BAY_FK1_RD         = :DEST_BAY_FK1_RD       ";
                strSql += CRLF + "        ,DEST_LEVEL_FK1_RD       = :DEST_LEVEL_FK1_RD     ";
                strSql += CRLF + "        ,DEST_BAY_FK2_RD         = :DEST_BAY_FK2_RD       ";
                strSql += CRLF + "        ,DEST_LEVEL_FK2_RD       = :DEST_LEVEL_FK2_RD     ";
                strSql += CRLF + "        ,SC_PLT_JOB_TYP_RD       = :SC_PLT_JOB_TYP_RD     ";
                strSql += CRLF + "        ,READ_UPD_DT             = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "        ,OD_RQ_FLAG              = 'N'                    ";
                if (m_blHostSendYN == true)
                {
                    strSql += CRLF + "        ,HOST_SEND_YN             = 'N'               ";
                }
                if (m_blHostErrSendYN == true)
                {
                    strSql += CRLF + "        ,HOST_ERR_SEND_YN         = 'N'               ";
                }
                strSql += CRLF + " WHERE WH_TYP = :WH_TYP                                   ";
                strSql += CRLF + "   AND PLC_NO = :PLC_NO                                   ";
                strSql += CRLF + "   AND SC_NO  = :SC_NO                                    ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("ONLINE_MODE_RD", DbLang.VARCHAR, 255).Value = strONLINE_MODE_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("AUTO_MODE_RD", DbLang.VARCHAR, 255).Value = strAUTO_MODE_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("SENSOR_FK_RD", DbLang.VARCHAR, 255).Value = strSENSOR_FK_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("UCSTATUS_RD", DbLang.VARCHAR, 255).Value = strUCSTATUS_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("POS_H_RD", DbLang.VARCHAR, 255).Value = strPOS_H_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("POS_V_RD", DbLang.VARCHAR, 255).Value = strPOS_V_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("FORKPOS_FK1_RD", DbLang.VARCHAR, 255).Value = strFORKPOS_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("FORKPOS_FK2_RD", DbLang.VARCHAR, 255).Value = strFORKPOS_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("ERR_CODE_RD", DbLang.VARCHAR, 255).Value = strERR_CODE_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("ERR_STA_FK1_RD", DbLang.VARCHAR, 255).Value = strERR_STA_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("ERR_STA_FK2_RD", DbLang.VARCHAR, 255).Value = strERR_STA_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("ACTIVE_MODE_RD", DbLang.VARCHAR, 255).Value = strACTIVE_MODE_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("COMPLETE_RD", DbLang.VARCHAR, 255).Value = strCOMPLETE_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("JOB_TYP_RD", DbLang.VARCHAR, 255).Value = strJOB_TYP_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_RD", DbLang.VARCHAR, 255).Value = strLUGG_NO_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("START_BANK_FK1_RD", DbLang.VARCHAR, 255).Value = strSTART_BANK_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("START_HSPOS_FK1_RD", DbLang.VARCHAR, 255).Value = strSTART_HSPOS_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("DEST_BANK_FK1_RD", DbLang.VARCHAR, 255).Value = strDEST_BANK_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("DEST_HSPOS_FK1_RD", DbLang.VARCHAR, 255).Value = strDEST_HSPOS_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("USE_FK_RD", DbLang.VARCHAR, 255).Value = strUSE_FK_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("LUGG_NO_FK2_RD", DbLang.VARCHAR, 255).Value = strLUGG_NO_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("START_BANK_FK2_RD", DbLang.VARCHAR, 255).Value = strSTART_BANK_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("START_HSPOS_FK2_RD", DbLang.VARCHAR, 255).Value = strSTART_HSPOS_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("DEST_BANK_FK2_RD", DbLang.VARCHAR, 255).Value = strDEST_BANK_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("DEST_HSPOS_FK2_RD", DbLang.VARCHAR, 255).Value = strDEST_HSPOS_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("START_BAY_FK1_RD", DbLang.VARCHAR, 255).Value = strSTART_BAY_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("START_LEVEL_FK1_RD", DbLang.VARCHAR, 255).Value = strSTART_LEVEL_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("START_BAY_FK2_RD", DbLang.VARCHAR, 255).Value = strSTART_BAY_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("START_LEVEL_FK2_RD", DbLang.VARCHAR, 255).Value = strSTART_LEVEL_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("DEST_BAY_FK1_RD", DbLang.VARCHAR, 255).Value = strDEST_BAY_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK1_RD", DbLang.VARCHAR, 255).Value = strDEST_LEVEL_FK1_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("DEST_BAY_FK2_RD", DbLang.VARCHAR, 255).Value = strDEST_BAY_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK2_RD", DbLang.VARCHAR, 255).Value = strDEST_LEVEL_FK2_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("SC_PLT_JOB_TYP_RD", DbLang.VARCHAR, 255).Value = strSC_PLT_JOB_TYP_RD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR, 255).Value = m_strScNo;

                ReqCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (ReqCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nThNo + " :" + strTitle + "SC정보 변경 중 에러(SC_DATA)., MSG [" + m_msQPlc._pBdb.ErrMsg + "]");
                    MakeMsg_Error(strTitle + "SC정보 변경 중 에러(SC_DATA)., MSG [" + m_msQPlc._pBdb.ErrMsg + "]", m_nThNo);
                    return false;
                }

                if (ReqCnt == 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nThNo + " :" + strTitle + "SC정보 변경 중 DATA가 없습니다., PLC_NO [" + m_strPlc_No + "] " + "SC_NO [" + m_strScNo + "]");
                    MakeMsg_Error(strTitle + "SC정보 변경 중 DATA가 없습니다., SC_NO [" + m_strScNo.ToString() + "]", m_nThNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                SetErrorMsg("Comm" + m_nThNo + " :" + strTitle + "SC정보 변경 중 에러(SC_DATA)., EXCEPTION MSG [" + ex.ToString() + "]");
                MakeMsg_Error(strTitle + "SC정보 변경 중 에러(SC_DATA)., EXCEPTION MSG [" + ex.ToString() + "]", m_nThNo);
                return false;
            }
		}
        #endregion

        #region [InsertWcsLogPgr] :: WCS_LOG_PGR에 LOG 남기기
        public bool InsertWcsLogPgr(string strTRACK_NO, string strLOG_MSG)
        {
            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "INSERT INTO WCS_LOG_PGR (WH_TYP                ";
                strSql += CRLF + "						  ,INS_DT                ";
                strSql += CRLF + "						  ,LOG_SEQ               ";
                strSql += CRLF + "						  ,LUGG_NO               ";
                strSql += CRLF + "						  ,BCR_BOTTOM            ";
                strSql += CRLF + "						  ,BCR_TOP               ";
                strSql += CRLF + "						  ,PGR_NM                ";
                strSql += CRLF + "						  ,LOG_KOR               ";
                strSql += CRLF + "						  ,TRACK_FROM            ";
                strSql += CRLF + "						  ,TRACK_TO              ";
                strSql += CRLF + "						  ,JOB_STA               ";
                strSql += CRLF + "						  ,RQ_INS_ID             ";
                strSql += CRLF + "						  ,RQ_INS_DT             ";
                strSql += CRLF + "						  ,EQP_TYP )             ";
                strSql += CRLF + "				VALUES    (:WH_TYP               ";
                strSql += CRLF + "						  ," + DbLang.SYSDATE + "";
                strSql += CRLF + "						  ,NEXTVAL('LOG_SEQ')    ";
                strSql += CRLF + "						  ,NULL                  ";
                strSql += CRLF + "						  ,NULL                  ";
                strSql += CRLF + "						  ,NULL                  ";
                strSql += CRLF + "						  ,:PGR_NM               ";
                strSql += CRLF + "						  ,:LOG_KOR              ";
                strSql += CRLF + "						  ,NULL                  ";
                strSql += CRLF + "						  ,NULL                  ";
                strSql += CRLF + "						  ,:JOB_STA              ";
                strSql += CRLF + "						  ,:RQ_INS_ID            ";
                strSql += CRLF + "						  ," + DbLang.SYSDATE + "";
                strSql += CRLF + "						  ,:EQP_TYP )            ";


                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();

                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PGR_NM", DbLang.VARCHAR, 255).Value = m_strLogFileNm;
                m_msQPlc._pBdb.mComMain.Parameters.Add("LOG_KOR", DbLang.VARCHAR, 255).Value = strLOG_MSG;
                m_msQPlc._pBdb.mComMain.Parameters.Add("JOB_STA", DbLang.VARCHAR, 255).Value = "999";
                m_msQPlc._pBdb.mComMain.Parameters.Add("RQ_INS_ID", DbLang.VARCHAR, 255).Value = strTRACK_NO;
                m_msQPlc._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = m_strEqmtTyp;
                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nThNo + " :[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + strTRACK_NO + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]");
                    MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + strTRACK_NO + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]", m_nThNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nThNo + " :[InsertWcsLogPgr]쓰기지시 후 상태값 변경중 DATA가 없습니다., PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + strTRACK_NO + "]");
                    MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 DATA가 없습니다.,PLC_NO [" + m_strPlc_No + "]  TRACK_NO [" + strTRACK_NO + "]", m_nThNo);
                    return false;

                }

                m_msQPlc._pBdb.Commit();
                return true;

            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                SetErrorMsg("Comm" + m_nThNo + " :[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., PLC_NO [" + m_strPlc_No + "] TRACK_NO  [" + strTRACK_NO + "] MSG [" + ex.ToString() + "]");
                MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., PLC_NO [" + strTRACK_NO + "] MSG [" + ex.ToString() + "]", m_nThNo);
                return false;
            }
        }
        #endregion

        #region [Communication] :: EQP_MST의 CONNECT 여부 설정
        public bool Communication(string CONNECTED_YN, string WH_TYP, string EQP_TYP, string PLC_NO)
        {
            string strTitle = "[Communication]";

            try
            {
                m_msQPlc._pBdb.BeginTrans();

                string strSql = "";
                string CRLF = "\r\n";
                int nSelCnt;

                MakeMsg("PLC 통신 OK", m_nThNo);

                strSql = "";
                strSql += CRLF + "UPDATE EQP_MST                                    ";
                strSql += CRLF + "   SET CONNECTED_YN      = :CONNECTED_YN          ";
                strSql += CRLF + "      ,UPD_DT            = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      ,PLC_PORT          = :PLC_PORT              ";
                strSql += CRLF + "WHERE  WH_TYP            = :WH_TYP                ";
                strSql += CRLF + "AND    EQP_TYP           = :EQP_TYP               ";
                strSql += CRLF + "AND    PLC_NO            = :PLC_NO                ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("CONNECTED_YN", DbLang.VARCHAR).Value = CONNECTED_YN;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_PORT", DbLang.VARCHAR, 255).Value = Convert.ToString("" + m_nCurPort);
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = WH_TYP;
                m_msQPlc._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = EQP_TYP;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = PLC_NO;
                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "PLC정보 변경중 ERROR. ErrorMsg [" + m_msQPlc._pBdb.ErrMsg + "] WH_TYP [" + WH_TYP + "] EQP_TYP [" + EQP_TYP + "]  PLC_NO [" + PLC_NO + "]", m_nThNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "PLC정보 변경중 Data가 없습니다.WH_TYP [" + WH_TYP + "] EQP_TYP [" + EQP_TYP + "] PLC_NO [" + PLC_NO + "] CONNECTED_YN [" + CONNECTED_YN + "]", m_nThNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error(strTitle + "Exception Error" + ex.Message, m_nThNo);
                return false;
            }
        }
        #endregion

        #region [InsertEQMT_ERR_LOG] :: SC 에러상태면 이력에 남기기
        public bool InsertEQMT_ERR_LOG(string pWH_TYP,
                                       string pEQP_TYP,
                                       string pEQP_NO,
                                       string pEQP_ERR_CD,
                                       string pLUGG_NO)
        {
            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "INSERT INTO EQP_ERR_HIS (WH_TYP                 ";
                strSql += CRLF + "                       , EQP_TYP                ";
                strSql += CRLF + "                       , EQP_NO                 ";
                strSql += CRLF + "                       , ERROR_DT               ";
                strSql += CRLF + "                       , EQP_ERR_CD             ";
                strSql += CRLF + "                       , BCR_BOTTOM             ";
                strSql += CRLF + "                       , BCR_TOP                ";
                strSql += CRLF + "                       , LUGG_NO)               ";
                strSql += CRLF + "                VALUES  (:WH_TYP                ";
                strSql += CRLF + "                       , :EQP_TYP               ";
                strSql += CRLF + "                       , :EQP_NO                ";
                strSql += CRLF + "                       , " + DbLang.SYSDATE + " ";
                strSql += CRLF + "                       , :EQP_ERR_CD            ";
                strSql += CRLF + "                       , null                   ";
                strSql += CRLF + "                       , null                   ";
                strSql += CRLF + "                       , :LUGG_NO)              ";



                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = pWH_TYP;
                m_msQPlc._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = pEQP_TYP;
                m_msQPlc._pBdb.mComMain.Parameters.Add("EQP_NO", DbLang.VARCHAR, 255).Value = pEQP_NO;
                m_msQPlc._pBdb.mComMain.Parameters.Add("EQP_ERR_CD", DbLang.VARCHAR, 255).Value = pEQP_ERR_CD;
                m_msQPlc._pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR, 255).Value = pLUGG_NO;
                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error("[InsertEQMT_ERR_LOG]:: Error:PLC설비 에러 로깅 실패 ", m_nThNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error("[InsertEQMT_ERR_LOG]:: Error:PLC설비 에러 로깅 Exception 에러 실패 ", m_nThNo);
                return false;
            }
        }
        #endregion

        #region [UpdateSC_CMD_RQ_YN] :: SC_DATA의 CMD_RQ_YN = 'N'으로 업데이트
        public bool UpdateSC_CMD_RQ_YN(string CMD_RQ_ID)
        {
            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "UPDATE SC_DATA                               ";
                strSql += CRLF + "   SET CMD_RQ_YN = 'N'                       ";
                strSql += CRLF + "      ,WRITE_UPD_DT = " + DbLang.SYSDATE + " ";
                if (CMD_RQ_ID == "DELFK1")
                {
                    strSql += CRLF + "       ,ITN_LUGG_FK1 = '0'    ";
                }
                if (CMD_RQ_ID == "DELFK2")
                {
                    strSql += CRLF + "       ,ITN_LUGG_FK2 = '0'    ";
                }
                if (CMD_RQ_ID == "DELFK12")
                {
                    strSql += CRLF + "       ,ITN_LUGG_FK1 = '0'    ";
                    strSql += CRLF + "       ,ITN_LUGG_FK2 = '0'    ";
                }
                strSql += CRLF + " WHERE WH_TYP = :WH_TYP           ";
                strSql += CRLF + "   AND PLC_NO = :PLC_NO           ";
                strSql += CRLF + "   AND SC_NO = :SC_NO             ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR, 255).Value = m_strScNo;
                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error("[UpdateSC_CMD_RQ_YN]:: Error:CMD_RQ_YN 변경 실패 ", m_nThNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error("[UpdateSC_CMD_RQ_YN]:: Error:CMD_RQ_YN 변경 실패 ", m_nThNo);
                return false;
            }
        }
        #endregion

        #region [UpdateSC_OD_RQ_YN] :: SC_DATA의 OD_RQ_YN = 'N'으로 업데이트
        public bool UpdateSC_OD_RQ_YN(string pITN_LUGG_FK1, string pITN_LUGG_FK2)
        {
            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "UPDATE SC_DATA                                ";
                strSql += CRLF + "   SET OD_RQ_YN      = 'N'                    ";
                strSql += CRLF + "       ,ITN_LUGG_FK1 = :ITN_LUGG_FK1          ";
                strSql += CRLF + "       ,ITN_LUGG_FK2 = :ITN_LUGG_FK2          ";
                strSql += CRLF + "       ,WRITE_UPD_DT = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "       ,OD_RQ_FLAG   = 'Y'                    ";
                strSql += CRLF + " WHERE WH_TYP = :WH_TYP                       ";
                strSql += CRLF + "   AND PLC_NO = :PLC_NO                       ";
                strSql += CRLF + "   AND SC_NO = :SC_NO                         ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("ITN_LUGG_FK1", DbLang.VARCHAR, 255).Value = pITN_LUGG_FK1;
                m_msQPlc._pBdb.mComMain.Parameters.Add("ITN_LUGG_FK2", DbLang.VARCHAR, 255).Value = pITN_LUGG_FK2;
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR, 255).Value = m_strScNo;
                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error("[UpdateSC_OD_RQ_YN]:: Error:OD_RQ_YN 변경 실패 ", m_nThNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error("[UpdateSC_OD_RQ_YN]:: Error:OD_RQ_YN 변경 실패 ", m_nThNo);
                return false;
            }
        }
        #endregion
    }
}
