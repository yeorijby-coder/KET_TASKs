using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Samoh_Lib;
using System.Data;
using System.Data.OleDb;
using log4net;
using log4net.Config;
using NpgsqlTypes;
using Npgsql;

namespace WCS_TASK_CV
{
	//2014 조형준 메모리에 Cv 상태를 저장한다.
	public class CVData
	{
		private string CvStatHexVal;
		public string CVSTATHEXVAL
		{
			get { return CvStatHexVal; }
			set { CvStatHexVal = value; }
		}
        private string CvPosStatHexVal;
        public string CVPOSSTATHEXVAL
        {
            get { return CvPosStatHexVal; }
            set { CvPosStatHexVal = value; }
        }
		private string CvBcrVal;
		public string CVBCRVAL
		{
			get { return CvBcrVal; }
			set { CvBcrVal = value; }
		}
        private string CvOpMode;
        public string CVOPMODE
        {
            get { return CvOpMode; }
            set { CvOpMode = value; }
        }
		private int CvErrCd;
		public int CVERRCD
		{
			get { return CvErrCd; }
			set { CvErrCd = value; }
		}

        private string AutoModeRd;
        public string AUTO_MODE_RD
        {
            get { return AutoModeRd; }
            set { AutoModeRd = value; }
        }

        private string StoReadyRd;
        public string STO_READY_RD
        {
            get { return StoReadyRd; }
            set { StoReadyRd = value; }
        }
        private string RetReadyRd;
        public string RET_READY_RD
        {
            get { return RetReadyRd; }
            set { RetReadyRd = value; }
        }
        private string StohsReadyRd;
        public string STOHS_READY_RD
        {
            get { return StohsReadyRd; }
            set { StohsReadyRd = value; }
        }
        private string RethsReadyRd;
        public string RETHS_READY_RD
        {
            get { return RethsReadyRd; }
            set { RethsReadyRd = value; }
        }
        private string Sensor0DataRd;
        public string SENSOR0_DATA_RD
        {
            get { return Sensor0DataRd; }
            set { Sensor0DataRd = value; }
        }
        private string Sensor1DataRd;
        public string SENSOR1_DATA_RD
        {
            get { return Sensor1DataRd; }
            set { Sensor1DataRd = value; }
        }
        private string Sensor2DataRd;
        public string SENSOR2_DATA_RD
        {
            get { return Sensor2DataRd; }
            set { Sensor2DataRd = value; }
        }
        private string DeleteTrackRd;
        public string DELETE_TRACK_RD
        {
            get { return DeleteTrackRd; }
            set { DeleteTrackRd = value; }
        }
        private string aTurnYn;
        public string A_TURN_YN
        {
            get { return aTurnYn; }
            set { aTurnYn = value; }
        }
        private string bTurnYn;
        public string B_TURN_YN
        {
            get { return bTurnYn; }
            set { bTurnYn = value; }
        }
        private string RemoteControl;
        public string REMOTE_CONTROL
        {
            get { return RemoteControl; }
            set { RemoteControl = value; }
        }
        private string StockMode;
        public string STOCK_MODE
        {
            get { return StockMode; }
            set { StockMode = value; }
        }
        private string RollMode;
        public string ROLL_MODE
        {
            get { return RollMode; }
            set { RollMode = value; }
        }
        private string ScLockSensor;
        public string SC_LOCK_SENSOR
        {
            get { return ScLockSensor; }
            set { ScLockSensor = value; }
        }
        private string SimMode;
        public string SIM_MODE
        {
            get { return SimMode; }
            set { SimMode = value; }
        }
        private string StoStatus;
        public string STO_STATUS
        {
            get { return StoStatus; }
            set { StoStatus = value; }
        }

        // DeviceMap XML 필드별 직전 값 (컬럼명 -> 값). 상위 보고 여부 판단용
        public Dictionary<string, string> PrevVals = new Dictionary<string, string>();

        // M 비트 이벤트 ACK 상태 (PPT 시나리오 Load/Unload Complete 핸드셰이크)
        public bool UnloadComp1Acked = false; // Unload Complete #1 ACK 전송 여부 (RGV→CV 언로드)
        public bool LoadComp1Acked   = false; // Load Complete #1 ACK 전송 여부  (RGV→CV 로드)
        public bool UnloadComp2Acked = false; // Unload Complete #2 ACK 전송 여부 (작업자 반출)
        public bool LoadComp2Acked   = false; // Load Complete #2 ACK 전송 여부  (입고쪽 로드)
        public bool AlarmSetAcked    = false; // 알람 세트 ACK 전송 여부
        public bool AlarmRstAcked    = false; // 알람 리셋 ACK 전송 여부

		public CVData()
		{
			CVSTATHEXVAL = "";
			CVBCRVAL = "";
			CVERRCD = 0;
            CVOPMODE = "";
            AUTO_MODE_RD = "";
            STO_READY_RD = "";
            RET_READY_RD = "";
            STOHS_READY_RD = "";
            RETHS_READY_RD = "";
            SENSOR0_DATA_RD = "";
            SENSOR1_DATA_RD = "";
            SENSOR2_DATA_RD = "";
            DELETE_TRACK_RD = "";
            A_TURN_YN = "";
            B_TURN_YN = "";
            REMOTE_CONTROL = "";
            STOCK_MODE = "";
            SC_LOCK_SENSOR = "";
            SIM_MODE = "";
            STO_STATUS = "";
		}
	}

    public class CvThread : maindefine
    {
        #region 변수정의
        #region ㅇㅇ
        #endregion
        private string m_strWh_typ;
        private string m_strEqmt_typ;
        private string m_strPlc_No;
        private string m_strMc_No;
        private string m_strPlcNo;
        private string m_strIp;
        private int m_nCurPort;
        private int m_nFromPort;
        private int m_nToPort;
        private int m_nPortCnt;
        public int m_nCnt;
        public int m_nFrTrackNo;
        public int m_nToTrackNo;
        public int m_nthNo;
        public string m_strRtnMsg;
        public string m_strLogFileNm;
        public string m_strLogMsg;
        public bool m_blHostErrSendYN = false;
        public bool m_blHostSendYN = false;
        public bool m_blSimModeWrite = false;
        public bool m_blConnectYn = false;

        public string m_strCvNo;
        private string m_strAddress;
        private int m_nAddress;
        private string m_strConnectString;
        private MelsecQ3EProtocol m_msQPlc;
        private cDeviceMapRuntime m_devMap;   // DeviceMap XML 런타임 파서
        public Thread m_thThread;
        public SYS_MAIN m_frmMain;

        /*
         * 수동 통신 절단 지시.
         *
         *   화면의 소켓 상태 아이콘을 눌러 "통신연결 해제" 를 고르면 참이 된다.
         *   - 스레드 본체(Thread_Doing)는 이 플래그를 보고 소켓을 닫고 내려간다.
         *   - 메인폼의 Thread_Tick 은 이 플래그가 선 슬롯을 재기동하지 않는다.
         *   UI 스레드가 쓰고 통신 스레드가 읽으므로 volatile 로 가시성을 보장한다.
         */
        public volatile bool m_bManualStop = false;

        private bool m_bOpen;
        public bool IsOpen { get { return m_bOpen; } set { m_bOpen = value; } } //프로그램 화면표시용.

        //Dictionary 객체를 생성함.
        Dictionary<int, CVData> CvDic = new Dictionary<int, CVData>();

        string strSql = "";
        string CRLF = "\r\n";
        int nSelCnt = 0;
        private string _strErrorMsg = "";

        #endregion
        #region CvThread
        public CvThread(int nThNo,
                        string strWh_typ,
                        string Eqmt_typ,
                        string Plc_No,
                        string Ip,
                        int CurPort,
                        int FromPort,
                        int ToPort,
                        int PortCnt,
                        int Cnt,
                        int FrTrackNo,
                        int ToTrackNo,
                        string ConnectString,
                        string strLogFileNm)
        {
            #region 변수값 세팅
            m_nthNo = nThNo;
            m_strWh_typ = strWh_typ;
            m_strEqmt_typ = Eqmt_typ;
            m_strPlc_No = Plc_No;
            m_strIp = Ip;

            m_nCurPort = CurPort;
            m_nFromPort = FromPort;
            m_nToPort = ToPort;

            m_nPortCnt = PortCnt;
            m_nFrTrackNo = FrTrackNo;
            m_nToTrackNo = ToTrackNo;
            m_strConnectString = ConnectString;
            m_strLogFileNm = strLogFileNm;
            IsOpen = false;
            m_msQPlc = new MelsecQ3EProtocol(m_strConnectString);

            m_msQPlc.IsHex = true;
            m_nCnt = Cnt;

            #endregion
        }
        #endregion
        #region Thread_Doing
        /*
         * 화면 표시용
         */
        #region
        private void MakeMsg(string msg, int nThGbn)
        {
            try
            {
                if (m_frmMain == null) return;
                m_frmMain.PsMsgView(msg, m_strPlc_No.ToString(), nThGbn);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void MakeMsg_Error(string msg, int nThGbn)
        {
            try
            {
                if (m_frmMain == null) return;
                m_frmMain.PsMsgView_Error(msg, m_strPlc_No.ToString(), nThGbn);
                cDefApp.m_LogQ[m_nthNo].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void MakeMsg_Imp(string msg, int nThGbn)
        {
            try
            {
                if (m_frmMain == null) return;
                m_frmMain.PsMsgView_IMP(msg, m_strPlc_No.ToString(), nThGbn);
                cDefApp.m_LogQ[m_nthNo].Enqueue(new LogParam(DateTime.Now, msg));
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
        #endregion
        /* 
         * 실구동용
         */
        #region
        public void Thread_Doing(object value)
        {
            try
            {
                if (cDefApp.GM_STAT_MAIN == false)
                {
                    throw new Exception("서비스 중지로 인한 쓰레드 종료");
                }

                // @.수동 절단 지시가 선 채로 (타이머 경합 등으로) 기동됐으면 바로 내려간다.
                if (m_bManualStop)
                {
                    throw new Exception("수동 절단 지시로 인한 쓰레드 종료");
                }

                MakeMsg_Imp("DB/Socket Connectting", m_nthNo);

                if (m_msQPlc.m_bSocCon == false && m_msQPlc.m_bDBOpen == false)
                {
                    // open된 포트개수 만큼 재연결 (FROM==TO 단일 포트여도 1회는 시도)
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

                            MakeMsg_Imp(string.Format("IP [{0}] PORT [{1}] 접속시도", m_strIp, m_nCurPort.ToString()), m_nthNo);
                            m_msQPlc.SetConfig(m_strIp, m_nCurPort, 2);

                            if (!m_msQPlc.Open(ref m_strRtnMsg))
                            {
                                SetErrorMsg("Comm" + m_nthNo + " :" + m_strRtnMsg);
                                MakeMsg_Error(m_strRtnMsg, m_nthNo);

                                //DB는 접속 되었는데 설비와 연결이 안되어 있는 경우 LOG남기기
                                if (m_msQPlc.m_bSocCon == false && m_msQPlc.m_bDBOpen == true)
                                {
                                    InsertWcsLogPgr("", "[Thread_Doing] 소켓 연결중 에러");
                                }

                                m_msQPlc.Close(ref m_strRtnMsg);

                                if (j == m_nPortCnt - 1)
                                {
                                    m_nCurPort = m_nCurPort + 1;
                                }
                                m_blConnectYn = false;

                                Thread.Sleep(500); //2000
                                continue;
                            }
                            else
                            {
                                // ini에 현재 설정된 포트값 쓰기
                                string strCOMM = "COMM" + m_nthNo;
                                cDefApi.WritePrivateProfileString(strCOMM, "CUR_PORT", Convert.ToString("" + m_nCurPort), cDefApp.GM_ENV_INI);

                                //접속 성공 로그 남기기 
                                InsertWcsLogPgr("", "[Thread_Doing] CV 그룹 번호 : " + m_strPlc_No + ", 연결포트 : " + m_nCurPort + " 접속 성공");

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
                }

                if (m_msQPlc.m_bSocCon == true && m_msQPlc.m_bDBOpen == true)
                {
                    IsOpen = true;
                    MakeMsg_Imp("DB login Ok! (21.07.07)", m_nthNo);

                    //DeviceMap XML 로드 (트랙 데이터 파싱 정의)
                    string strDevErr = "";
                    m_devMap = cDeviceMapRuntime.Load(m_strPlc_No, ref strDevErr);
                    if (m_devMap == null)
                    {
                        MakeMsg_Error("[DeviceMap] " + strDevErr, m_nthNo);
                        goto EXIT_LBL;
                    }
                    MakeMsg_Imp("[DeviceMap] DeviceMap" + m_strPlc_No + ".xml 로드 완료"
                                + " (트랙당 " + m_devMap.WordPerTrack + "워드, 필드 " + m_devMap.Fields.Count + "개"
                                + ", 상태영역 " + m_devMap.EtcAreas.Count + "개)", m_nthNo);
                    foreach (string strSkip in m_devMap.SkippedAreas)
                        MakeMsg_Imp("[DeviceMap] 상태영역 스킵: " + strSkip, m_nthNo);

                    //XML 에 정의된 dbcol 컬럼이 CV_DATA 에 없으면 생성
                    if (!EnsureCvDataColumns()) goto EXIT_LBL;

                    while (!m_bManualStop)
                    {
                        for (int Idx = 1; Idx <= m_nCnt; Idx++)
                        {
                            // @.수동 절단 지시는 한 트랙그룹 처리 안에서도 바로 반영한다.
                            if (m_bManualStop) goto EXIT_LBL;

                            this.m_msQPlc.IsAscii = m_frmMain.IsAscii;
                            this.m_msQPlc.IsHex = m_frmMain.IsHex;

                            if (!GetFirstAddress(Idx)) goto EXIT_LBL; //읽을 첫 주소를 가져온다.

                            if (!CvStatus(Idx)) goto EXIT_LBL; //컨베이어 정보를 READ한다.

                            if (!CvEtcStatus(Idx)) goto EXIT_LBL; //SeparatelyETC 상태영역(Auto 등)을 READ한다.

                            if (!CvChg_CMD_RQ_YN(Idx)) goto EXIT_LBL; //컨베이어 CMD 쓰기지시를 확인하고 DATA를 WRITE한다..

                            if (!CvChg_OD_RQ_YN(Idx)) goto EXIT_LBL; //컨베이어 OD 쓰기지시를 확인하고 DATA를 WRITE한다..


                            Thread.Sleep(200);
                        }
                    }
                }

            EXIT_LBL:
                {
                    if (m_bManualStop)
                    {
                        MakeMsg_Imp("수동 절단 지시로 통신을 해제합니다.", m_nthNo);
                        InsertWcsLogPgr("", "[Thread_Doing] 수동 절단 지시로 통신 해제 (PLC " + m_strPlc_No + ")");

                        // @.수동 절단도 EQP_MST 통신상태를 'N' 으로 내린다.
                        //   (정상 폴링은 CvStatus 가 매회 'Y' 로 유지하므로, 여기서 안 내리면
                        //    상위 시스템이 절단된 설비를 계속 접속중으로 오판한다)
                        Communication("N", m_strWh_typ, m_strEqmt_typ, m_strPlc_No);
                    }
                    SetErrorMsg("CoMM" + m_nthNo + " DB & Socket logoff!");
                    MakeMsg_Imp("DB & Socket logoff!", m_nthNo);
                }

            }
            catch (Exception ex)
            {
                MakeMsg_Error(ex.Message, m_nthNo);
            }
            IsOpen = false;
            m_msQPlc.Close(ref m_strRtnMsg);
            MakeMsg_Imp(m_strRtnMsg, m_nthNo);
            m_thThread = null;
        }
        #endregion
        #endregion Thread_Doing

        #region [GetFirstAddresss] :: 시작 주소 구하기
        /*
         * 시작위치 구함.
         */
        private bool GetFirstAddress(int Idx)
        {
            string strTitle = "[GetFirstAddress]";

            string strSql = "";
            string CRLF = "\r\n";
            int nSelCnt;
            DataTable dttest = new DataTable();

            try
            {
                strSql = "";
                strSql += CRLF + "SELECT TRACK_NO                ";
                strSql += CRLF + "      ,MC_NO                   ";
                strSql += CRLF + "FROM   CV_DATA                 ";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP        ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO        ";
                strSql += CRLF + "AND    MC_NO  = :MC_NO         ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR, 255).Value = m_nFrTrackNo.ToString("000");
                nSelCnt = m_msQPlc._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    MakeMsg_Error(strTitle + "첫 트랙의 DATA를 가져오는 중 ERROR. WH_TYP [" + m_strWh_typ + "]  PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + m_nFrTrackNo + "] ErroMsg [" + m_msQPlc._pBdb.ErrMsg + "]",m_nthNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    MakeMsg_Error(strTitle + "첫 트랙의 DATA를 찾지 못했습니다. WH_TYP [" + m_strWh_typ + "]  PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + m_nFrTrackNo + "]", m_nthNo);
                    return false;
                }

                //롯데 G동은 mc_no가 1(그룹번호)+01(설비번호)형식임.
                m_strAddress = m_msQPlc._pBdb.mDtMain.Rows[0]["MC_NO"].ToString().Substring(1,2);
                //m_strAddress = m_nFrTrackNo.ToString().Substring(2, 3);

                if (nSelCnt > 0)
                    m_nAddress = (Convert.ToInt32(0 + m_strAddress)) * m_devMap.WordPerTrack; //시작트랙 * 트랙당워드수 -> 시작 어드레스


                if (m_nAddress < 0)
                {
                    MakeMsg_Error(strTitle + "트랙 시작위치 주소가 이상합니다. WH_TYP [" + m_strWh_typ + "]  PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + m_nFrTrackNo + "]", m_nthNo);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + "Exception Error [" + ex.Message + "]", m_nthNo);
                return false;
            }
        }
        #endregion GetFirstAddress

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

                MakeMsg("PLC 통신 OK", m_nthNo);

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
                    MakeMsg_Error(strTitle + "PLC정보 변경중 ERROR. ErrorMsg [" + m_msQPlc._pBdb.ErrMsg + "] WH_TYP [" + WH_TYP + "] EQP_TYP [" + EQP_TYP + "]  PLC_NO [" + PLC_NO + "]", m_nthNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "PLC정보 변경중 Data가 없습니다.WH_TYP [" + WH_TYP + "] EQP_TYP [" + EQP_TYP + "] PLC_NO [" + PLC_NO + "] CONNECTED_YN [" + CONNECTED_YN + "]", m_nthNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error(strTitle + "Exception Error" + ex.Message, m_nthNo);
                return false;
            }
        }
        #endregion

        #region [CvStatus] :: CV READ 후 값이 변한게 있으면 DB UPDATE (DeviceMap XML 정의 기반 파싱)
        private bool CvStatus(int Idx)
        {
            string strTitle = "[CvStatus]";

            try
            {
                int nWPT = m_devMap.WordPerTrack;
                byte[] byRxBuff = new byte[80 * nWPT * 2 + 64];
                int nReadTrack;

                /*
                 * 80개의 트랙식 읽음.
                 */

                MakeMsg(strTitle + "80 TRACK PLC 통신", m_nthNo);
                nReadTrack = 80;

                for (int CvNo = m_nFrTrackNo; CvNo < m_nToTrackNo; )
                {
                    if ((CvNo + nReadTrack - 1) > m_nToTrackNo)
                    {
                        nReadTrack = m_nToTrackNo - CvNo + 1;
                    }

                    Array.Clear(byRxBuff, 0x00, byRxBuff.Length);
                    int nAddress = (CvNo - m_nFrTrackNo) * nWPT + m_nAddress;

                    if (m_msQPlc.READ((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                            (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                            nAddress,
                            nReadTrack * nWPT,
                            ref byRxBuff) == false)
                    {
                        // @.프로토콜이 남긴 실패 사유를 그대로 올린다. 인자 없이 던지면
                        //   로그에 "'System.Exception' 형식의 예외" 만 남아 원인을 못 본다.
                        throw new Exception("PLC 읽기 실패 (주소 " + nAddress
                                            + ", 워드 " + (nReadTrack * nWPT) + ") : "
                                            + m_msQPlc.GetErrorMsg());
                    }

                    MakeMsg("상태값 DB저장", m_nthNo);
                    for (int nIdx = 0; nIdx < nReadTrack; nIdx++)
                    {
                        int nCvNo = nIdx + CvNo;
                        int nArrayIdx = nIdx * nWPT * 2;

                        if (!CvDic.ContainsKey(nCvNo))
                        {
                            CvDic.Add(nCvNo, new CVData()); //Key를 추가한다.
                        }

                        //최초 실행 시 현재 DB값을 DIC에 넣기(상위에 상태보고 하는 값들)
                        if (CvDic[nCvNo].PrevVals.Count == 0)
                        {
                            if (!LoadPrevValsFromDb(nCvNo))
                            {
                                MakeMsg_Error(strTitle + "최초 트랙정보 읽는 중 에러(CV_DATA)", m_nthNo);
                                return false;
                            }
                        }

                        //Hexa string 값으로 가져온다.
                        string strCvHexVal = BytesToHexs(byRxBuff, nArrayIdx, nWPT * 2);

                        //Conveyor상태값이 다를 때만 Update.
                        if (CvDic[nCvNo].CVSTATHEXVAL != strCvHexVal)
                        {
                            CvDic[nCvNo].CVSTATHEXVAL = strCvHexVal; //Dictionary 값을 변경한다.

                            //DeviceMap XML 정의대로 트랙 버퍼를 필드 단위 파싱
                            Dictionary<string, string> dicVals = m_devMap.ParseTrack(byRxBuff, nArrayIdx);

                            //상위 보고 대상(hostRpt) 필드 값이 변했으면 보고 플래그 SET
                            foreach (cDeviceMapRuntime.CvField fld in m_devMap.Fields)
                            {
                                string strNew = dicVals[fld.DbCol];
                                string strOld;
                                CvDic[nCvNo].PrevVals.TryGetValue(fld.DbCol, out strOld);
                                if (strOld != strNew)
                                {
                                    CvDic[nCvNo].PrevVals[fld.DbCol] = strNew;
                                    if (fld.HostRpt)
                                        m_blHostSendYN = true;
                                }
                            }

                            //에러코드가 있고 전에 에러코드와 다를때만.
                            //처음에 에러코드가 0인거는 안탐.
                            int nErrorCode = 0;
                            string ERROR_CODE = "0000";
                            if (dicVals.ContainsKey("ERROR_CODE"))
                            {
                                ERROR_CODE = dicVals["ERROR_CODE"];
                                nErrorCode = Convert.ToInt32("0" + ERROR_CODE);
                            }

                            if (CvDic[nCvNo].CVERRCD != nErrorCode)
                            {
                                m_blHostErrSendYN = true;

                                string LUGG_NO_RD = dicVals.ContainsKey("LUGG_NO_RD") ? dicVals["LUGG_NO_RD"] : "";
                                if (!UpdateEQMT_ERR_LOG(m_strWh_typ, m_strEqmt_typ, nCvNo.ToString("000"), ERROR_CODE, LUGG_NO_RD))
                                {
                                    m_blHostErrSendYN = false;
                                    return false;
                                }

                                //에러코드를 SET한다.
                                CvDic[nCvNo].CVERRCD = nErrorCode;
                            }

                            //TRACK정보 UPDATE. (파싱된 전체 필드를 동적 반영)
                            if (!UpdateCvData(nCvNo.ToString("000"), dicVals))
                            {
                                m_blHostSendYN = false;
                                m_blHostErrSendYN = false;
                                return false;
                            }
                        }
                        m_blHostSendYN = false;
                        m_blHostErrSendYN = false;
                        m_strCvNo = Convert.ToString("" + nCvNo);
                    }

                    CvNo += nReadTrack;

                    if (m_nToTrackNo < CvNo) break;
                }
                //설비 통신상태 업데이트
                Communication("Y", m_strWh_typ, m_strEqmt_typ, m_strPlc_No);
            }
            catch (Exception ex)
            {
                m_blHostSendYN = false;
                m_blHostErrSendYN = false;
                SetErrorMsg("Comm" + m_nthNo + strTitle + "Exception Error" + ex.Message);
                Communication("N", m_strWh_typ, m_strEqmt_typ, m_strPlc_No);
                InsertWcsLogPgr(m_strCvNo, strTitle + " 트랙번호 : [" + m_strCvNo + "] 데이터 읽기 중 에러 : " + ex.Message);
                MakeMsg_Error(strTitle + m_strCvNo + "Exception Error" + ex.Message, m_nthNo);
                return false;
            }
            return true;
        }
        #endregion

        #region [LoadPrevValsFromDb] :: 최초 1회 CV_DATA 현재값을 PrevVals 에 적재
        private bool LoadPrevValsFromDb(int nCvNo)
        {
            strSql = "";
            strSql += cDefApp.CRLF + "SELECT CD.*                                          ";
            strSql += cDefApp.CRLF + "  FROM CV_DATA CD                                    ";
            strSql += cDefApp.CRLF + " WHERE CD.WH_TYP = :WH_TYP                           ";
            strSql += cDefApp.CRLF + "   AND CD.PLC_NO = :PLC_NO                           ";
            strSql += cDefApp.CRLF + "   AND CD.MC_NO  = :MC_NO                            ";

            m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
            m_msQPlc._pBdb.mComMain.Parameters.Clear();
            m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
            m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
            m_msQPlc._pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR, 255).Value = nCvNo.ToString("000");

            int nSelCount = m_msQPlc._pBdb.ExcuteQry(strSql);
            if (nSelCount < 0)
                return false;
            if (nSelCount == 0)
                return true;   //행이 없으면 최초 파싱값으로 채워짐

            foreach (string strCol in m_devMap.GetDbColumns())
            {
                if (m_msQPlc._pBdb.mDtMain.Columns.Contains(strCol))
                    CvDic[nCvNo].PrevVals[strCol] = m_msQPlc._pBdb.mDtMain.Rows[0][strCol].ToString();
            }

            if (m_msQPlc._pBdb.mDtMain.Columns.Contains("ERROR_CODE"))
                CvDic[nCvNo].CVERRCD = Convert.ToInt32("0" + m_msQPlc._pBdb.mDtMain.Rows[0]["ERROR_CODE"].ToString());

            return true;
        }
        #endregion

        #region [CvEtcStatus] :: SeparatelyETC 상태영역(Auto 등) READ 후 변경분 DB UPDATE
        private bool CvEtcStatus(int Idx)
        {
            string strTitle = "[CvEtcStatus]";

            if (m_devMap.EtcAreas.Count == 0)
                return true;

            try
            {
                byte[] byRxBuff = new byte[64];

                foreach (cDeviceMapRuntime.EtcArea area in m_devMap.EtcAreas)
                {
                    Array.Clear(byRxBuff, 0, byRxBuff.Length);
                    if (!m_msQPlc.READ((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                       (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                       area.Address,
                                       1,
                                       ref byRxBuff))
                    {
                        throw new Exception(area.Name + " 영역(D" + area.Address + ") 읽기 실패");
                    }

                    int nWord = (byRxBuff[1] << 8) + byRxBuff[0];

                    foreach (cDeviceMapRuntime.EtcBit bit in area.Bits)
                    {
                        string strVal = ((nWord >> bit.Pos) & 0x01).ToString();

                        foreach (int nCvNo in bit.Tracks)
                        {
                            if (!CvDic.ContainsKey(nCvNo))
                                CvDic.Add(nCvNo, new CVData());

                            string strOld;
                            CvDic[nCvNo].PrevVals.TryGetValue(area.DbCol, out strOld);
                            if (strOld == strVal) continue;

                            CvDic[nCvNo].PrevVals[area.DbCol] = strVal;
                            if (area.HostRpt)
                                m_blHostSendYN = true;

                            Dictionary<string, string> dicVals = new Dictionary<string, string>();
                            dicVals[area.DbCol] = strVal;
                            if (!UpdateCvData(nCvNo.ToString("000"), dicVals))
                            {
                                m_blHostSendYN = false;
                                return false;
                            }
                            m_blHostSendYN = false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                SetErrorMsg("Comm" + m_nthNo + strTitle + "Exception Error" + ex.Message);
                MakeMsg_Error(strTitle + "Exception Error" + ex.Message, m_nthNo);
                return false;
            }
        }
        #endregion

        #region [EnsureCvDataColumns] :: XML dbcol 컬럼이 CV_DATA 에 없으면 생성
        private bool EnsureCvDataColumns()
        {
#if POSTGRESQL
            try
            {
                foreach (string strCol in m_devMap.GetDbColumns())
                {
                    NpgsqlCommand cmdAdd = new NpgsqlCommand(
                        "ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS " + strCol + " VARCHAR(10)",
                        m_msQPlc._pConObj);
                    cmdAdd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error("[DeviceMap] CV_DATA 컬럼 생성 중 에러: " + ex.Message, m_nthNo);
                return false;
            }
#else
            return true;   //Oracle 은 수동 컬럼 관리
#endif
        }
        #endregion


        #region [CvChg_CMD_RQ_YN] :: CV_DATA에서 CMD_RQ_YN 여부에 따른 CV 지시
        private bool CvChg_CMD_RQ_YN(int Idx)
        {
            string strTitle = "[CvChg_CMD_RQ_YN]";

            try
            {
                string strSql = "";

                byte[] byTxBuff = new byte[1000];

                int nUpdCount = 0;
                /*
                 * 변경할 트랙 정보 읽음
                 */
                #region
                strSql = "";
                strSql += cDefApp.CRLF + "SELECT CD.*                                          ";
                strSql += cDefApp.CRLF + "  FROM CV_DATA CD                                    ";
                strSql += cDefApp.CRLF + " WHERE CD.WH_TYP = :WH_TYP                           ";
                strSql += cDefApp.CRLF + "   AND CD.PLC_NO = :PLC_NO                           ";
                strSql += cDefApp.CRLF + "   AND CD.MC_NO BETWEEN :FROM_TRACK AND :TO_TRACK    ";
                strSql += cDefApp.CRLF + "   AND CD.CMD_RQ_YN = 'Y'                            ";
                strSql += cDefApp.CRLF + "ORDER BY CD.WRITE_UPD_DT, CD.TRACK_NO                ";
                strSql += cDefApp.CRLF + "LIMIT 5;                                             ";


                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("FROM_TRACK", DbLang.VARCHAR, 255).Value = m_nFrTrackNo.ToString("000");
                m_msQPlc._pBdb.mComMain.Parameters.Add("TO_TRACK", DbLang.VARCHAR, 255).Value = m_nToTrackNo.ToString("000");

                nUpdCount = m_msQPlc._pBdb.ExcuteQry(strSql);

                if (nUpdCount < 0)
                {
                    MakeMsg_Error(strTitle + "트랙정보 읽는 중 에러(CV_DATA)", m_nthNo);
                    return false;
                }

                #endregion

                for (int nRows = 0; nRows < nUpdCount; nRows++)
                {
                    #region table정보 읽음

                    //LFC 사용
                    string TRACK_NO = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["MC_NO"].ToString();
                    string ADDR_NO = TRACK_NO.Substring(TRACK_NO.Length - 2, 2);
                    int nADDR_NO = (Convert.ToInt32(0 + ADDR_NO)) * m_devMap.WordPerTrack; //시작트랙 * 트랙당워드수 -> 해당 어드레스

                    string CMD_RQ_ID = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["CMD_RQ_ID"].ToString();

                    string CMD_RQ_PARM = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["CMD_RQ_PARM"].ToString();
                    int nCMD_RQ_PARM = (Convert.ToInt32(0 + CMD_RQ_PARM));

                    string PULP_SENSOR_OD = GfRow(nRows, "PULP_SENSOR_OD"); // 대기필요
                    int nPULP_SENSOR_OD = (Convert.ToInt32(0 + PULP_SENSOR_OD));

                    string WAIT_SC_RET_JOB_OD = GfRow(nRows, "WAIT_SC_RET_JOB_OD"); // 대기필요
                    int nWAIT_SC_RET_JOB_OD = (Convert.ToInt32(0 + WAIT_SC_RET_JOB_OD));

                    string WAIT_SC_RET_JOB_RD = GfRow(nRows, "WAIT_SC_RET_JOB_RD"); // 대기필요
                    int nWAIT_SC_RET_JOB_RD = (Convert.ToInt32(0 + WAIT_SC_RET_JOB_RD));

                    string TR_PAUSE_OD = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["TR_PAUSE_OD"].ToString();
                    int nTR_PAUSE_OD = (Convert.ToInt32(0 + TR_PAUSE_OD));

                    string TR_PAUSE_RD = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["TR_PAUSE_RD"].ToString();
                    int nTR_PAUSE_RD = (Convert.ToInt32(0 + TR_PAUSE_RD));

                    string JOB_TYP_RD = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["JOB_TYP_RD"].ToString();
                    int nJOB_TYP_RD = (Convert.ToInt32(0 + JOB_TYP_RD));
                    #endregion

                    /*
                     * 한 트랙에 전체 쓰기
                     */
                    #region
                    if (CMD_RQ_ID == "ROTATE")
                    {
                        if (9 >= m_devMap.WordPerTrack)
                        {
                            //현재 DeviceMap 트랙블록에 +9(모터) 영역이 없으면 지시 생략
                            MakeMsg_Error(strTitle + " TRACK [" + TRACK_NO + "] ROTATE 지시 생략 - DeviceMap 에 +9 영역 없음", m_nthNo);
                            if (!UpdateCvDataCmd(TRACK_NO)) return false;
                            continue;
                        }

                        Array.Clear(byTxBuff, 0, byTxBuff.Length);
                        byTxBuff[0] = (byte)(0 >> 0); //0
                        byTxBuff[1] = (byte)(0 >> 8); //128

                        int nWriteLen = 1;

                        if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                           (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                           nADDR_NO + 9,
                                                           nWriteLen,
                                                           byTxBuff) == false)
                        {
                            if (this.m_msQPlc.IsHex)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                            }
                            if (this.m_msQPlc.IsAscii)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                            }

                            m_strLogMsg = strTitle + "트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 실패";
                            if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                            {
                                return false;
                            }
                            return false;
                        }

                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }

                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 성공";
                        if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                        {
                            return false;
                        }

                        if (!UpdateCvDataCmd(TRACK_NO))
                        {
                            return false;
                        }
                    }
                    else if (CMD_RQ_ID == "WAIT")
                    {
                        if (4 >= m_devMap.WordPerTrack)
                        {
                            MakeMsg_Error(strTitle + " TRACK [" + TRACK_NO + "] WAIT 지시 생략 - DeviceMap 에 +4 영역 없음", m_nthNo);
                            if (!UpdateCvDataCmd(TRACK_NO)) return false;
                            continue;
                        }

                        Array.Clear(byTxBuff, 0, byTxBuff.Length);
                        byTxBuff[0] = (byte)(nCMD_RQ_PARM);
                        byTxBuff[1] = (byte)(0);

                        int nWriteLen = 1;

                        if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                           (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                           nADDR_NO + 4,
                                                           nWriteLen,
                                                           byTxBuff) == false)
                        {
                            if (this.m_msQPlc.IsHex)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                            }
                            if (this.m_msQPlc.IsAscii)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                            }

                            m_strLogMsg = strTitle + " TRACK_NO : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 실패";
                            if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                            {
                                return false;
                            }
                            return false;
                        }

                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }

                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 서공";
                        if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                        {
                            return false;
                        }

                        if (!UpdateCvDataCmd(TRACK_NO))
                        {
                            return false;
                        }
                    }
                    else if (CMD_RQ_ID == "NOREAD")
                    {
                        int nOffErr = m_devMap.GetWordOffsetByCol("ERROR_CODE");
                        if (nOffErr < 0)
                        {
                            MakeMsg_Error(strTitle + " TRACK [" + TRACK_NO + "] NOREAD 지시 생략 - DeviceMap 에 ERROR_CODE 정의 없음", m_nthNo);
                            if (!UpdateCvDataCmd(TRACK_NO)) return false;
                            continue;
                        }

                        Array.Clear(byTxBuff, 0, byTxBuff.Length);
                        int nNoread = 102;
                        byTxBuff[0] = (byte)(nNoread >> 0);
                        byTxBuff[1] = (byte)(nNoread >> 8);

                        int nWriteLen = 1;

                        if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                           (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                           nADDR_NO + nOffErr,
                                                           nWriteLen,
                                                           byTxBuff) == false)
                        {
                            if (this.m_msQPlc.IsHex)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                            }
                            if (this.m_msQPlc.IsAscii)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                            }

                            m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 실패";
                            if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                            {
                                return false;
                            }
                            return false;
                        }

                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }

                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 성공";
                        if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                        {
                            return false;
                        }

                        if (!UpdateCvDataCmd(TRACK_NO))
                        {
                            return false;
                        }
                    }
                    else if (CMD_RQ_ID == "RESET")  //에러 리셋
                    {
                        int nOffErr = m_devMap.GetWordOffsetByCol("ERROR_CODE");
                        if (nOffErr < 0)
                        {
                            MakeMsg_Error(strTitle + " TRACK [" + TRACK_NO + "] RESET 지시 생략 - DeviceMap 에 ERROR_CODE 정의 없음", m_nthNo);
                            if (!UpdateCvDataCmd(TRACK_NO)) return false;
                            continue;
                        }

                        Array.Clear(byTxBuff, 0, byTxBuff.Length);
                        byTxBuff[0] = (byte)(0);
                        byTxBuff[1] = (byte)(0);

                        int nWriteLen = 1;

                        if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                           (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                           nADDR_NO + nOffErr,
                                                           nWriteLen,
                                                           byTxBuff) == false)
                        {
                            if (this.m_msQPlc.IsHex)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                            }
                            if (this.m_msQPlc.IsAscii)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                            }

                            m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 실패";
                            if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                            {
                                return false;
                            }
                            return false;
                        }

                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }

                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 성공";
                        if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                        {
                            return false;
                        }

                        if (!UpdateCvDataCmd(TRACK_NO))
                        {
                            return false;
                        }
                    }
                    else if (CMD_RQ_ID == "1") // PULP_SENSOR
                    {
                        int nOffPulp = m_devMap.GetWordOffsetByCol("PULP_SENSOR_RD");
                        if (nOffPulp < 0)
                        {
                            MakeMsg_Error(strTitle + " TRACK [" + TRACK_NO + "] PULP_SENSOR 지시 생략 - DeviceMap 에 PULP_SENSOR_RD 정의 없음", m_nthNo);
                            if (!UpdateCvDataCmd(TRACK_NO)) return false;
                            continue;
                        }

                        //PULP 워드를 XML 정의 위치대로 구성 (같은 워드에 JOB_TYP 이 있으면 RD값 유지)
                        int[] anWords = new int[m_devMap.WordPerTrack];
                        bool[] abUsed = new bool[m_devMap.WordPerTrack];
                        m_devMap.SetWriteValue(anWords, abUsed, "JOB_TYP_RD", nJOB_TYP_RD);
                        m_devMap.SetWriteValue(anWords, abUsed, "PULP_SENSOR_RD", nPULP_SENSOR_OD);

                        Array.Clear(byTxBuff, 0, byTxBuff.Length);
                        byTxBuff[0] = (byte)(anWords[nOffPulp] & 0xFF);
                        byTxBuff[1] = (byte)((anWords[nOffPulp] >> 8) & 0xFF);

                        int nWriteLen = 1;

                        if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                           (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                           nADDR_NO + nOffPulp,
                                                           nWriteLen,
                                                           byTxBuff) == false)
                        {
                            if (this.m_msQPlc.IsHex)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                            }
                            if (this.m_msQPlc.IsAscii)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                            }

                            m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 실패";
                            if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                            {
                                return false;
                            }
                            return false;
                        }

                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }

                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 성공";
                        if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                        {
                            return false;
                        }

                        if (!UpdateCvDataCmd(TRACK_NO))
                        {
                            return false;
                        }
                    }
                    else if (CMD_RQ_ID == "2")//대기필요
                    {
                        int nOffPause = m_devMap.GetWordOffsetByCol("TR_PAUSE_RD");
                        if (nOffPause < 0)
                        {
                            MakeMsg_Error(strTitle + " TRACK [" + TRACK_NO + "] 대기필요 지시 생략 - DeviceMap 에 TR_PAUSE_RD 정의 없음", m_nthNo);
                            if (!UpdateCvDataCmd(TRACK_NO)) return false;
                            continue;
                        }

                        //대기필요 워드를 XML 정의 위치대로 구성 (TR_PAUSE는 기존에 가지고있던 rd값)
                        int[] anWords = new int[m_devMap.WordPerTrack];
                        bool[] abUsed = new bool[m_devMap.WordPerTrack];
                        m_devMap.SetWriteValue(anWords, abUsed, "TR_PAUSE_RD", nTR_PAUSE_RD);
                        m_devMap.SetWriteValue(anWords, abUsed, "WAIT_SC_RET_JOB_RD", nWAIT_SC_RET_JOB_OD);

                        Array.Clear(byTxBuff, 0, byTxBuff.Length);
                        byTxBuff[0] = (byte)(anWords[nOffPause] & 0xFF);
                        byTxBuff[1] = (byte)((anWords[nOffPause] >> 8) & 0xFF);

                        int nWriteLen = 1;

                        if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                           (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                           nADDR_NO + nOffPause,
                                                           nWriteLen,
                                                           byTxBuff) == false)
                        {
                            if (this.m_msQPlc.IsHex)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                            }
                            if (this.m_msQPlc.IsAscii)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                            }

                            m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 실패";
                            if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                            {
                                return false;
                            }
                            return false;
                        }

                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }

                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 성공";
                        if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                        {
                            return false;
                        }

                        if (!UpdateCvDataCmd(TRACK_NO))
                        {
                            return false;
                        }
                    }
                    else if (CMD_RQ_ID == "3") //트랙 대기
                    {
                        int nOffPause = m_devMap.GetWordOffsetByCol("TR_PAUSE_RD");
                        if (nOffPause < 0)
                        {
                            MakeMsg_Error(strTitle + " TRACK [" + TRACK_NO + "] 트랙대기 지시 생략 - DeviceMap 에 TR_PAUSE_RD 정의 없음", m_nthNo);
                            if (!UpdateCvDataCmd(TRACK_NO)) return false;
                            continue;
                        }

                        //트랙대기 워드를 XML 정의 위치대로 구성 (대기필요는 기존에 가지고 있던 rd값)
                        int[] anWords = new int[m_devMap.WordPerTrack];
                        bool[] abUsed = new bool[m_devMap.WordPerTrack];
                        m_devMap.SetWriteValue(anWords, abUsed, "TR_PAUSE_RD", nTR_PAUSE_OD);
                        m_devMap.SetWriteValue(anWords, abUsed, "WAIT_SC_RET_JOB_RD", nWAIT_SC_RET_JOB_RD);

                        Array.Clear(byTxBuff, 0, byTxBuff.Length);
                        byTxBuff[0] = (byte)(anWords[nOffPause] & 0xFF);
                        byTxBuff[1] = (byte)((anWords[nOffPause] >> 8) & 0xFF);

                        int nWriteLen = 1;

                        if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                           (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                           nADDR_NO + nOffPause,
                                                           nWriteLen,
                                                           byTxBuff) == false)
                        {
                            if (this.m_msQPlc.IsHex)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                            }
                            if (this.m_msQPlc.IsAscii)
                            {
                                MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                                MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                            }

                            m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 실패";
                            if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                            {
                                return false;
                            }
                            return false;
                        }

                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }

                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] CMD_RQ_ID : [" + CMD_RQ_ID + "] 커맨드 지시 성공";
                        if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                        {
                            return false;
                        }

                        if (!UpdateCvDataCmd(TRACK_NO))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!UpdateCvDataCmd(TRACK_NO))
                        {
                            return false;
                        }
                    }
                    #endregion
                }

                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                SetErrorMsg("Comm" + m_nthNo + ex.Message);
                MakeMsg_Error(ex.Message, m_nthNo);
                return false;
            }
        }
        #endregion

        /*
         * GfRow :: 조회 결과에서 컬럼 하나를 안전하게 꺼낸다.
         *
         *   현장마다 CV_DATA 의 컬럼 구성이 조금씩 다르다. 없는 컬럼을 DataRow 에서
         *   읽으면 예외가 나고, 그러면 통신 스레드가 통째로 빠져나가 재접속만 반복한다.
         *   (이 현장에는 PULP_SENSOR_OD / WAIT_SC_RET_JOB_OD / WAIT_SC_RET_JOB_RD 가 없다)
         */
        private string GfRow(int p_nRow, string p_strCol, string p_strDefault = "0")
        {
            try
            {
                if (m_msQPlc._pBdb.mDtMain == null) return p_strDefault;
                if (!m_msQPlc._pBdb.mDtMain.Columns.Contains(p_strCol)) return p_strDefault;
                if (p_nRow < 0 || p_nRow >= m_msQPlc._pBdb.mDtMain.Rows.Count) return p_strDefault;

                string strValue = m_msQPlc._pBdb.mDtMain.Rows[p_nRow][p_strCol].ToString().Trim();
                return (strValue.Length == 0) ? p_strDefault : strValue;
            }
            catch { return p_strDefault; }
        }

        #region [CvChg_OD_RQ_YN] :: CV_DATA에서 OD_RQ_YN 여부에 따른 CV 지시
        private bool CvChg_OD_RQ_YN(int Idx)
        {
            string strTitle = "[CvChg_OD_RQ_YN]";

            try
            {
                string strSql = "";

                byte[] byTxBuff = new byte[1000];

                int nUpdCount = 0;
                /*
                 * 변경할 트랙 정보 읽음
                 */
                #region
                strSql = "";
                strSql += cDefApp.CRLF + "SELECT CD.*                                          ";
                strSql += cDefApp.CRLF + "  FROM CV_DATA CD                                    ";
                strSql += cDefApp.CRLF + " WHERE CD.WH_TYP = :WH_TYP                           ";
                strSql += cDefApp.CRLF + "   AND CD.PLC_NO = :PLC_NO                           ";
                strSql += cDefApp.CRLF + "   AND CD.MC_NO BETWEEN :FROM_TRACK AND :TO_TRACK    ";
                strSql += cDefApp.CRLF + "   AND CD.OD_RQ_YN = 'Y'                             ";
                strSql += cDefApp.CRLF + "ORDER BY CD.WRITE_UPD_DT, CD.TRACK_NO                ";
                strSql += cDefApp.CRLF + "LIMIT 5;                                             ";


                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("FROM_TRACK", DbLang.VARCHAR, 255).Value = m_nFrTrackNo.ToString("000");
                m_msQPlc._pBdb.mComMain.Parameters.Add("TO_TRACK", DbLang.VARCHAR, 255).Value = m_nToTrackNo.ToString("000");

                nUpdCount = m_msQPlc._pBdb.ExcuteQry(strSql);

                if (nUpdCount < 0)
                {
                    MakeMsg_Error(strTitle + "트랙정보 읽는 중 에러(CV_DATA)", m_nthNo);
                    return false;
                }

                #endregion

                for (int nRows = 0; nRows < nUpdCount; nRows++)
                {
                    #region table정보 읽음

                    //LFC 사용
                    string TRACK_NO = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["MC_NO"].ToString();
                    string ADDR_NO = TRACK_NO.Substring(TRACK_NO.Length - 2, 2);
                    int nADDR_NO = (Convert.ToInt32(0 + ADDR_NO)) * m_devMap.WordPerTrack; //시작트랙 * 트랙당워드수 -> 해당 어드레스

                    string LUGG_NO_OD = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["LUGG_NO_OD"].ToString();
                    int nLUGG_NO_OD = (Convert.ToInt32(0 + LUGG_NO_OD));

                    string DEST_POS_OD = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["DEST_POS_OD"].ToString();
                    int nDEST_POS_OD = (Convert.ToInt32(0 + DEST_POS_OD));

                    string JOB_TYP_OD = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["JOB_TYP_OD"].ToString();
                    int nJOB_TYP_OD = (Convert.ToInt32(0 + JOB_TYP_OD));

                    string PULP_SENSOR_RD = GfRow(nRows, "PULP_SENSOR_RD");
                    int nPULP_SENSOR_RD = (Convert.ToInt32(0 + PULP_SENSOR_RD));

                    string PULP_SENSOR_OD = GfRow(nRows, "PULP_SENSOR_OD");
                    int nPULP_SENSOR_OD = (Convert.ToInt32(0 + PULP_SENSOR_OD));

                    string WAIT_SC_RET_JOB_RD = GfRow(nRows, "WAIT_SC_RET_JOB_RD");
                    int nWAIT_SC_RET_JOB_RD = (Convert.ToInt32(0 + WAIT_SC_RET_JOB_RD));

                    string WAIT_SC_RET_JOB_OD = GfRow(nRows, "WAIT_SC_RET_JOB_OD");
                    int nWAIT_SC_RET_JOB_OD = (Convert.ToInt32(0 + WAIT_SC_RET_JOB_OD));

                    #endregion

                    /*
                     * 한 트랙에 전체 쓰기
                     */
                    #region

                    Array.Clear(byTxBuff, 0, byTxBuff.Length);

                    MakeMsg_Imp("Track #" + TRACK_NO + " Writting"
                                                + ", 작업구분:" + JOB_TYP_OD
                                                + ", 작업번호:" + LUGG_NO_OD
                                                + ", 도착위치:" + DEST_POS_OD
                                                , m_nthNo);

                    //DeviceMap XML 정의 위치대로 쓰기 워드 구성 (맵에 없는 필드는 자동 제외)
                    int[] anWords = new int[m_devMap.WordPerTrack];
                    bool[] abUsed = new bool[m_devMap.WordPerTrack];
                    m_devMap.SetWriteValue(anWords, abUsed, "LUGG_NO_RD", nLUGG_NO_OD);
                    m_devMap.SetWriteValue(anWords, abUsed, "DEST_POS_RD", nDEST_POS_OD);
                    m_devMap.SetWriteValue(anWords, abUsed, "JOB_TYP_RD", nJOB_TYP_OD);
                    m_devMap.SetWriteValue(anWords, abUsed, "PULP_SENSOR_RD", nPULP_SENSOR_OD);

                    int nWriteLen = 0;
                    for (int nW = 0; nW < abUsed.Length; nW++)
                        if (abUsed[nW]) nWriteLen = nW + 1;

                    for (int nW = 0; nW < nWriteLen; nW++)
                    {
                        byTxBuff[nW * 2]     = (byte)(anWords[nW] & 0xFF);
                        byTxBuff[nW * 2 + 1] = (byte)((anWords[nW] >> 8) & 0xFF);
                    }

                    if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                       (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                       nADDR_NO,
                                                       nWriteLen,
                                                       byTxBuff) == false)
                    {
                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }
                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] 작업번호 : [" + LUGG_NO_OD + "] 도착지 : [" + DEST_POS_OD + "] 작업구분 : [" + JOB_TYP_OD + "] CV 지시 실패";
                        if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                        {
                            return false;
                        }

                        return false;
                    }
                    if (this.m_msQPlc.IsHex)
                    {
                        MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                        MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                    }
                    if (this.m_msQPlc.IsAscii)
                    {
                        MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                        MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                    }

                    m_strLogMsg = strTitle + " 트랙번호 : [" + TRACK_NO + "] 작업번호 : [" + LUGG_NO_OD + "] 도착지 : [" + DEST_POS_OD + "] 작업구분 : [" + JOB_TYP_OD + "] CV 지시 성공";
                    if (!InsertWcsLogPgr(TRACK_NO, m_strLogMsg))
                    {
                        return false;
                    }

                    if (!UpdateCvDataOD(TRACK_NO))
                    {
                        return false;
                    }

                    #endregion
                }

                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                SetErrorMsg("Comm" + m_nthNo + ex.Message);
                MakeMsg_Error(ex.Message, m_nthNo);
                return false;
            }
        }
        #endregion

        #region [UpdateCvData] :: 파싱된 DeviceMap XML 필드들을 CV_DATA 에 동적 UPDATE
        public bool UpdateCvData(string strTRACK_NO, Dictionary<string, string> dicVals)
        {
            string strTitle = "[UpdateCvData]";

            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += cDefApp.CRLF + " UPDATE CV_DATA                                                     ";
                bool bFirst = true;
                foreach (KeyValuePair<string, string> kv in dicVals)
                {
                    strSql += cDefApp.CRLF + (bFirst ? "    SET " : "       ,") + kv.Key + " = :" + kv.Key;
                    bFirst = false;
                }
                strSql += cDefApp.CRLF + "       ,READ_UPD_DT = " + DbLang.SYSDATE + "                        ";
                strSql += cDefApp.CRLF + "       ,OD_RQ_FLAG = 'N'                                            ";
                if (m_blHostSendYN == true)
                {
                    strSql += cDefApp.CRLF + "       ,HOST_SEND_YN = 'N'                                      ";
                }
                if (m_blHostErrSendYN == true)
                {
                    strSql += cDefApp.CRLF + "       ,HOST_ERR_SEND_YN = 'N'                                  ";
                }
                strSql += cDefApp.CRLF + "WHERE  WH_TYP   = :WH_TYP                                           ";
                strSql += cDefApp.CRLF + "AND    PLC_NO   = :PLC_NO                                           ";
                strSql += cDefApp.CRLF + "AND    MC_NO    = :MC_NO                                         ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                foreach (KeyValuePair<string, string> kv in dicVals)
                    m_msQPlc._pBdb.mComMain.Parameters.Add(kv.Key, DbLang.VARCHAR).Value = kv.Value;
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strTRACK_NO;

                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nthNo + " :" + strTitle + "트랙정보 변경 중 에러(CV_DATA)., PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + strTRACK_NO + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]");
                    MakeMsg_Error(strTitle + "트랙정보 변경 중 에러(CV_DATA)., PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + strTRACK_NO + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "트랙정보 변경 중 DATA가 없습니다., TRACK_NO [" + strTRACK_NO + "]", m_nthNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error(strTitle + "트랙정보 변경 중 에러(CV_DATA)., TRACK_NO [" + strTRACK_NO + "] MSG [" + ex.ToString() + "]", m_nthNo);
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
                strSql += cDefApp.CRLF + "INSERT INTO WCS_LOG_PGR (WH_TYP                ";
                strSql += cDefApp.CRLF + "						  ,INS_DT                ";
                strSql += cDefApp.CRLF + "						  ,LOG_SEQ               ";
                strSql += cDefApp.CRLF + "						  ,LUGG_NO               ";
                strSql += cDefApp.CRLF + "						  ,BCR_BOTTOM            ";
                strSql += cDefApp.CRLF + "						  ,BCR_TOP               ";
                strSql += cDefApp.CRLF + "						  ,PGR_NM                ";
                strSql += cDefApp.CRLF + "						  ,LOG_KOR               ";
                strSql += cDefApp.CRLF + "						  ,TRACK_FROM            ";
                strSql += cDefApp.CRLF + "						  ,TRACK_TO              ";
                strSql += cDefApp.CRLF + "						  ,JOB_STA               ";
                strSql += cDefApp.CRLF + "						  ,RQ_INS_ID             ";
                strSql += cDefApp.CRLF + "						  ,RQ_INS_DT             ";
                strSql += cDefApp.CRLF + "						  ,EQP_TYP )             ";
                strSql += cDefApp.CRLF + "				VALUES    (:WH_TYP               ";
                strSql += cDefApp.CRLF + "						  ," + DbLang.SYSDATE + "";
                strSql += cDefApp.CRLF + "						  ,NEXTVAL('LOG_SEQ')    ";
                strSql += cDefApp.CRLF + "						  ,NULL                  ";
                strSql += cDefApp.CRLF + "						  ,NULL                  ";
                strSql += cDefApp.CRLF + "						  ,NULL                  ";
                strSql += cDefApp.CRLF + "						  ,:PGR_NM               ";
                strSql += cDefApp.CRLF + "						  ,:LOG_KOR              ";
                strSql += cDefApp.CRLF + "						  ,NULL                  ";
                strSql += cDefApp.CRLF + "						  ,NULL                  ";
                strSql += cDefApp.CRLF + "						  ,:JOB_STA              ";
                strSql += cDefApp.CRLF + "						  ,:RQ_INS_ID            ";
                strSql += cDefApp.CRLF + "						  ," + DbLang.SYSDATE + "";
                strSql += cDefApp.CRLF + "						  ,:EQP_TYP )            ";


                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();

                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PGR_NM", DbLang.VARCHAR, 255).Value = m_strLogFileNm;
                m_msQPlc._pBdb.mComMain.Parameters.Add("LOG_KOR", DbLang.VARCHAR, 255).Value = strLOG_MSG;
                m_msQPlc._pBdb.mComMain.Parameters.Add("JOB_STA", DbLang.VARCHAR, 255).Value = "999";
                m_msQPlc._pBdb.mComMain.Parameters.Add("RQ_INS_ID", DbLang.VARCHAR, 255).Value = strTRACK_NO;
                m_msQPlc._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = m_strEqmt_typ;
                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nthNo + " :[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + strTRACK_NO + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]");
                    MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + strTRACK_NO + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nthNo + " :[InsertWcsLogPgr]쓰기지시 후 상태값 변경중 DATA가 없습니다., PLC_NO [" + m_strPlc_No + "] TRACK_NO [" + strTRACK_NO + "]");
                    MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 DATA가 없습니다.,PLC_NO [" + m_strPlc_No + "]  TRACK_NO [" + strTRACK_NO + "]", m_nthNo);
                    return false;

                }

                m_msQPlc._pBdb.Commit();
                return true;

            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                SetErrorMsg("Comm" + m_nthNo + " :[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., PLC_NO [" + m_strPlc_No + "] TRACK_NO  [" + strTRACK_NO + "] MSG [" + ex.ToString() + "]");
                MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., PLC_NO [" + strTRACK_NO + "] MSG [" + ex.ToString() + "]", m_nthNo);
                return false;
            }
        }
        #endregion

        #region [UpdateCvDataCmd] :: CV_DATA의 CMD_RQ_YN = 'N' 업데이트
        public bool UpdateCvDataCmd(string strTRACK_NO)
        {
            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "UPDATE CV_DATA						";
                strSql += CRLF + "   SET CMD_RQ_YN       = 'N'			";
                strSql += CRLF + "WHERE  WH_TYP          = :WH_TYP		";
                strSql += CRLF + "AND    PLC_NO          = :PLC_NO		";
                strSql += CRLF + "AND    MC_NO           = :MC_NO	    ";
                strSql += CRLF + "AND    CMD_RQ_YN       = 'Y'       	";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR, 255).Value = strTRACK_NO;

                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error("[UpdateCvDataCmd] 쓰기지시 후 상태값 변경중 ERROR., TRACK_NO [" + strTRACK_NO + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error("[UpdateCvDataCmd]쓰기지시 후 상태값 변경중 DATA가 없습니다., TRACK_NO [" + strTRACK_NO + "]", m_nthNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error("[UpdateCvDataCmd] 쓰기지시 후 상태값 변경중 ERROR., TRACK_NO [" + strTRACK_NO + "] MSG [" + ex.ToString() + "]", m_nthNo);
                return false;
            }
        }
        #endregion

        #region [UpdateCvDataOD] :: CV_DATA의 OD_RQ_YN = 'N' 업데이트
        public bool UpdateCvDataOD(string strTRACK_NO)
        {
            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "UPDATE CV_DATA						               ";
                strSql += CRLF + "   SET OD_RQ_YN        = 'N'			               ";
                strSql += CRLF + "      ,OD_RQ_FLAG      = 'Y'			               ";
                strSql += CRLF + "      ,WRITE_UPD_DT    = " + DbLang.SYSDATE + "      ";
                strSql += CRLF + "WHERE  WH_TYP          = :WH_TYP		               ";
                strSql += CRLF + "AND    PLC_NO          = :PLC_NO		               ";
                strSql += CRLF + "AND    MC_NO           = :MC_NO    	               ";
                strSql += CRLF + "AND    OD_RQ_YN        = 'Y'       	               ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR, 255).Value = strTRACK_NO;

                nSelCnt = m_msQPlc._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error("[UpdateCvDataOD] 쓰기지시 후 상태값 변경중 ERROR., TRACK_NO [" + strTRACK_NO + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    m_msQPlc._pBdb.Rollback();
                    MakeMsg_Error("[UpdateCvDataOD]쓰기지시 후 상태값 변경중 DATA가 없습니다., TRACK_NO [" + strTRACK_NO + "]", m_nthNo);
                    return false;

                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error("[UpdateCvDataOD] 쓰기지시 후 상태값 변경중 ERROR., TRACK_NO [" + strTRACK_NO + "] MSG [" + ex.ToString() + "]", m_nthNo);
                return false;
            }
        }
        #endregion

        #region [UpdateEQMT_ERR_LOG] :: CV 에러상태면 이력에 남기기
        public bool UpdateEQMT_ERR_LOG(string pWH_TYP,
                                       string pEQP_TYP,
                                       string pEQP_NO,
                                       string pEQP_ERR_CD,
                                       string pLUGG_NO)
        {
            try
            {
                m_msQPlc._pBdb.BeginTrans();

                strSql = "";
                strSql += cDefApp.CRLF + "INSERT INTO EQP_ERR_HIS (WH_TYP                ";
                strSql += cDefApp.CRLF + "                       , EQP_TYP               ";
                strSql += cDefApp.CRLF + "                       , EQP_NO                ";
                strSql += cDefApp.CRLF + "                       , ERROR_DT              ";
                strSql += cDefApp.CRLF + "                       , EQP_ERR_CD            ";
                strSql += cDefApp.CRLF + "                       , BCR_BOTTOM            ";
                strSql += cDefApp.CRLF + "                       , BCR_TOP               ";
                strSql += cDefApp.CRLF + "                       , LUGG_NO )             ";
                strSql += cDefApp.CRLF + "                VALUES  (:WH_TYP               ";
                strSql += cDefApp.CRLF + "                       , :EQP_TYP              ";
                strSql += cDefApp.CRLF + "                       , :EQP_NO               ";
                strSql += cDefApp.CRLF + "                       , " + DbLang.SYSDATE + "";
                strSql += cDefApp.CRLF + "                       , :EQP_ERR_CD           ";
                strSql += cDefApp.CRLF + "                       , null                  ";
                strSql += cDefApp.CRLF + "                       , null                  ";
                strSql += cDefApp.CRLF + "                       , :LUGG_NO );           ";



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
                    MakeMsg_Error("[UpdEQMT_ERR_LOG]:: Error:PLC설비 에러 로깅 실패 ", m_nthNo);
                    return false;
                }

                m_msQPlc._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error("[UpdEQMT_ERR_LOG]:: Error:PLC설비 에러 로깅 Exception 에러 실패 ", m_nthNo);
                return false;
            }
        }
        #endregion

        #region [UpdateCvFlowDef] :: CV_FLOW_DEF 업데이트. FLOW BIT SET (나중에)
        public bool UpdateCvFlowDef(string strTRACK_NO)
        {
            try
            {

                strSql = "";
                strSql += CRLF + " SELECT CFD.*, COALESCE(CD2.TRACK_NO, 'PASS') AS FRONT_GET_TRACK_NO, COALESCE(CD2.SENSOR0_DATA_RD, 'PASS') AS FRONT_SENSOR_VALUE  ";
                strSql += CRLF + "   FROM CV_FLOW_DEF CFD INNER JOIN CV_DATA CD                                                                                     ";
                strSql += CRLF + "                                ON CFD.WH_TYP = CD.WH_TYP                                                                         ";
                strSql += CRLF + "                               AND CFD.PLC_NO = CD.PLC_NO                                                                         ";
                strSql += CRLF + "                               AND CFD.WH_TYP = :WH_TYP                                                                           ";
                strSql += CRLF + "                               AND CFD.PLC_NO = :PLC_NO                                                                           ";
                strSql += CRLF + "                               AND CFD.TRACK_NO = CD.TRACK_NO                                                                     ";
                strSql += CRLF + "                               AND CFD.TRACK_NO <> CD.DEST_POS_RD                                                                 ";
                strSql += CRLF + "                               AND CFD.FLOW_YN = 'Y'                                                                              ";
                strSql += CRLF + "                               AND CD.AUTO_MODE_RD = '1'                                                                          ";
                strSql += CRLF + "                               AND CD.LUGG_NO_RD IS NOT NULL                                                                      ";
                strSql += CRLF + "                               AND CD.LUGG_NO_RD <> '0'                                                                           ";
                strSql += CRLF + "                               AND CD.SENSOR0_DATA_RD = '1'                                                                       ";
                strSql += CRLF + "                               AND CD.ERROR_CODE = '0'                                                                            ";
                strSql += CRLF + "                               AND CD.OD_RQ_YN = 'N'                                                                              ";
                strSql += CRLF + "                               AND CD.CMD_RQ_YN = 'N'                                                                             ";
                strSql += CRLF + "                               AND CD.READ_UPD_DT >= CD.WRITE_UPD_DT                                                              ";
                //strSql += CRLF + "                               AND ((now() - CD.READ_UPD_DT) * 24 * 60 * 60) > SET_TIME                                           ";
                //strSql += CRLF + "                               AND ((now() - CFD.OD_UPD_DT) * 24 * 60 * 60) > SET_TIME                                            ";
                strSql += CRLF + "                   LEFT OUTER JOIN CV_DATA CD2                                                                                    ";
                strSql += CRLF + "                                ON CD2.WH_TYP = CFD.WH_TYP                                                                        ";
                strSql += CRLF + "                               AND CD2.PLC_NO = CFD.PLC_NO                                                                        ";
                strSql += CRLF + "                               AND ( (CD2.TRACK_NO = CFD.FRONT_TRACK_NO                                                           ";
                strSql += CRLF + "                               AND CD2.AUTO_MODE_RD = '1'                                                                         ";
                strSql += CRLF + "                               AND CD2.ERROR_CODE = '0'                                                                           ";
                strSql += CRLF + "                               AND CD2.SENSOR0_DATA_RD = '0'                                                                      ";
                strSql += CRLF + "                                  ) OR (CFD.FRONT_TRACK_NO = '0' OR CFD.FRONT_TRACK_NO IS NULL) )                                 ";

                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;

                nSelCnt = m_msQPlc._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    MakeMsg_Error("[UpdateCvFlowDef] FLOW BIT 값 조회중 ERROR., PLC_NO [" + m_strPlc_No + "] MSG [" + m_msQPlc._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    MakeMsg_Error("[UpdateCvFlowDef] FLOW BIT 값 조회중 DATA가 없습니다., PLC_NO [" + m_strPlc_No + "]", m_nthNo);
                    return false;

                }

                for (int nRows = 0; nRows < nSelCnt; nRows++)
                {
                    string strFRONT_GET_TRACK_NO = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["FRONT_GET_TRACK_NO"].ToString();
                    string strFRONT_SENSOR_VALUE = "" + m_msQPlc._pBdb.mDtMain.Rows[nRows]["FRONT_SENSOR_VALUE"].ToString();

                    if (strFRONT_GET_TRACK_NO == "PASS")
                    {
                        continue;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error("[UpdateCvFlowDef] FLOW BIT 값 조회중 ERROR., PLC_NO [" + m_strPlc_No + "] MSG [" + ex.ToString() + "]", m_nthNo);
                return false;
            }
        }
        #endregion

        #region[chkSIM_MODE] :: 시뮬레이션 모드 확인
        private bool chkSIM_MODE(int Idx)
        {
            try
            {
                string strSql = "";
                string strTitle = "[sim_mode]";

                byte[] byTxBuff = new byte[1000];

                int nUpdCount = 0;

                strSql = "";
                strSql += cDefApp.CRLF + "SELECT SIM_MODE                     ";
                strSql += cDefApp.CRLF + "  FROM HOST_IF_LOG                  ";
                strSql += cDefApp.CRLF + " WHERE WH_TYP = :WH_TYP             ";
                strSql += cDefApp.CRLF + "ORDER BY INS_DT DESC                ";
                strSql += cDefApp.CRLF + "LIMIT 1                             ";


                m_msQPlc._pBdb.mComMain.CommandType = CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;

                nUpdCount = m_msQPlc._pBdb.ExcuteQry(strSql);

                if (nUpdCount == 0)
                {
                    MakeMsg_Error("HOST_IF_LOG에 데이터가 없어서 SIM_MODE는 건너뜁니다.", m_nthNo);
                    return true;
                }

                if (nUpdCount < 0)
                {
                    MakeMsg_Error(" SIM_MODE 읽는 중 에러", m_nthNo);
                    return false;
                }

                int nCvNo = 0;
                int nSIM_MODE = 0;
                string SIM_MODE = "" + m_msQPlc._pBdb.mDtMain.Rows[0]["SIM_MODE"].ToString();

                if (!CvDic.ContainsKey(nCvNo))
                {
                    CvDic.Add(nCvNo, new CVData()); //Key를 추가한다.
                }

                if (SIM_MODE == "0") //sim모드 아닐떄
                {
                    if (CvDic[nCvNo].SIM_MODE != SIM_MODE)
                    {
                        CvDic[nCvNo].SIM_MODE = SIM_MODE;
                        //D1에 0넣기
                        nSIM_MODE = 0;
                        m_blSimModeWrite = true;
                    }
                    else
                    {
                        m_blSimModeWrite = false;
                        return true;
                    }
                }
                else
                {
                    if (CvDic[nCvNo].SIM_MODE != SIM_MODE)
                    {
                        CvDic[nCvNo].SIM_MODE = SIM_MODE;
                        //D1에 1넣기
                        nSIM_MODE = 1;
                        m_blSimModeWrite = true;
                    }
                    else
                    {
                        m_blSimModeWrite = false;
                        return true;
                    }
                }

                if (m_blSimModeWrite == true)
                {
                    int nADDR_NO = 0;

                    Array.Clear(byTxBuff, 0, byTxBuff.Length);
                    byTxBuff[0] = (byte)(nSIM_MODE);
                    byTxBuff[1] = (byte)(0);

                    int nWriteLen = 1;

                    if (m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                                       (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_D,
                                                       nADDR_NO + 1,
                                                       nWriteLen,
                                                       byTxBuff) == false)
                    {
                        if (this.m_msQPlc.IsHex)
                        {
                            MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }
                        if (this.m_msQPlc.IsAscii)
                        {
                            MakeMsg_Error(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                            MakeMsg_Error(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                        }

                        m_strLogMsg = strTitle + " D1 = [" + nSIM_MODE + "] 쓰기 실패";
                        if (!InsertWcsLogPgr("000", m_strLogMsg))
                        {
                            return false;
                        }
                        return false;
                    }

                    if (this.m_msQPlc.IsHex)
                    {
                        MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndHexString + "]", m_nthNo);
                        MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                    }

                    if (this.m_msQPlc.IsAscii)
                    {
                        MakeMsg_Imp(strTitle + "트랙정보변경요청 SEND [" + m_msQPlc.SndAsciiString + "]", m_nthNo);
                        MakeMsg_Imp(strTitle + "트랙정보변경요청 RECEIVE [" + m_msQPlc.RcvAsciiString + "]", m_nthNo);
                    }

                    m_strLogMsg = strTitle + " D1 = [" + nSIM_MODE + "] 쓰기 성공";
                    if (!InsertWcsLogPgr("000", m_strLogMsg))
                    {
                        return false;
                    }
                }
            
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        #endregion


        /*
        // ─────────────────────────────────────────────────────────────────────
        //  헬퍼: M 비트 읽기/쓰기 (워드 단위 PLC 통신 기반)
        // ─────────────────────────────────────────────────────────────────────

        #region [M 비트 헬퍼]
        /// <summary>
        /// 2 워드(32비트) 버퍼에서 특정 비트 추출.
        /// bitIndex : 버퍼 시작(buf[0] 비트0)으로부터의 절대 비트 인덱스
        /// </summary>
        private bool GetMBitFromBuf(byte[] buf, int bitIndex)
        {
            int byteIdx = bitIndex / 8;
            int bitPos  = bitIndex % 8;
            if (byteIdx >= buf.Length) return false;
            return (buf[byteIdx] & (1 << bitPos)) != 0;
        }

        /// <summary>
        /// M 영역 특정 비트 한 개를 ON/OFF 쓰기 (Read-Modify-Write).
        /// mBitAddr : M 비트 절대 주소 (예: M0901 → 901)
        /// </summary>
        private bool WriteMBit(int mBitAddr, bool value)
        {
            int wordAddr = mBitAddr / 16;
            int bitPos   = mBitAddr % 16;

            // 현재 워드 읽기
            byte[] rxBuf = new byte[100];
            Array.Clear(rxBuf, 0, rxBuf.Length);
            if (!m_msQPlc.READ((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                               (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_M,
                               wordAddr, 1, ref rxBuf))
                return false;

            // 비트 변경
            int word = rxBuf[0] | (rxBuf[1] << 8);
            if (value)
                word |=  (1 << bitPos);
            else
                word &= ~(1 << bitPos);

            // 변경된 워드 쓰기
            byte[] txBuf = new byte[2];
            txBuf[0] = (byte)(word & 0xFF);
            txBuf[1] = (byte)((word >> 8) & 0xFF);
            return m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                  (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_M,
                                  wordAddr, 1, txBuf);
        }

        /// <summary>
        /// R 영역 트래킹 주소 계산.
        /// PPT: Tracking Start Address = (CV 기계번호 - 1) * 10 word
        ///      슬롯 s (0-based) → base + s*2 (JOB NO = 2 word)
        /// CV 기계번호는 m_strPlc_No 숫자 파싱 (예: "01" → 1, "11" → 11)
        /// </summary>
        private int GetRTrackingAddr(int trackNo)
        {
            int cvMachineNo = 0;
            string numStr = System.Text.RegularExpressions.Regex.Match(m_strPlc_No, @"\d+").Value;
            int.TryParse(numStr, out cvMachineNo);
            if (cvMachineNo < 1) cvMachineNo = 1;

            int rBase = (cvMachineNo - 1) * 10;          // CV 기계번호 기준 R 영역 시작
            int slot  = trackNo - m_nFrTrackNo;           // 이 CV 내 슬롯 인덱스 (0-based)
            return rBase + slot * 2;                      // JOB NO = 2 word
        }

        /// <summary>
        /// JOB NO BCD 인코딩.
        /// PPT: JOB NO 1234 → 메모리 2143 (십자리 쌍 교환: abcd → badc)
        /// 2 word(4 byte) 버퍼 offset 위치에 little-endian 기록.
        /// </summary>
        private void EncodeJobNoR(int jobNo, byte[] buf, int offset)
        {
            int a = (jobNo / 1000) % 10;
            int b = (jobNo / 100)  % 10;
            int c = (jobNo / 10)   % 10;
            int d =  jobNo         % 10;
            int encoded = b * 1000 + a * 100 + d * 10 + c; // badc 순

            buf[offset + 0] = (byte)( encoded        & 0xFF);
            buf[offset + 1] = (byte)((encoded >> 8)  & 0xFF);
            buf[offset + 2] = 0;
            buf[offset + 3] = 0;
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        //  PPT 시나리오 구현 메서드
        // ─────────────────────────────────────────────────────────────────────

        #region [CvEventCheck] :: M 비트 Load/Unload Complete 이벤트 감지 및 ACK 처리
        /// <summary>
        /// PPT Slide 5 / 8~15 시나리오:
        ///   PLC가 Load/Unload Complete M 비트 ON
        ///   → WCS ACK M 비트 ON
        ///   → PLC가 이벤트 비트 OFF
        ///   → WCS ACK 비트 OFF
        ///
        /// M 비트 맵 (CV 기계번호 N 기준):
        ///   이벤트 base = 160 + (N-1)*20
        ///   +1 : Unload Complete #1  (RGV측 화물 수령)
        ///   +2 : Load Complete #1    (RGV측 화물 적재)
        ///   +3 : Unload Complete #2  (작업자 반출)
        ///   +4 : Load Complete #2    (입고쪽 화물 적재)
        ///   +6 : W.O (작업지시 보고)
        ///   +10: 파렛트 존재 #1
        ///   +11: 파렛트 존재 #2
        ///
        ///   ACK base = 801 + (N-1)*10
        ///   +0 : 언로드 완료 #1 ACK
        ///   +1 : 로드 완료 #1 ACK
        ///   +2 : 언로드 완료 #2 ACK
        ///   +3 : 로드 완료 #2 ACK
        /// </summary>
        private bool CvEventCheck(int Idx)
        {
            string strTitle = "[CvEventCheck]";
            try
            {
                string numStr = System.Text.RegularExpressions.Regex.Match(m_strPlc_No, @"\d+").Value;
                int cvMachineNo = 0;
                int.TryParse(numStr, out cvMachineNo);
                if (cvMachineNo < 1) return true; // PLC_NO 파싱 불가 시 스킵

                int mBase   = 160 + (cvMachineNo - 1) * 20; // 이벤트 M 비트 base
                int ackBase = 801 + (cvMachineNo - 1) * 10; // ACK M 비트 base

                // 이벤트 M 영역 읽기: 2 워드 (base 워드 정렬)
                int mWordAddr  = mBase / 16;
                int mBitOffset = mBase % 16; // 버퍼 내 시작 비트 위치

                byte[] byRxBuff = new byte[100];
                Array.Clear(byRxBuff, 0, byRxBuff.Length);
                if (!m_msQPlc.READ((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                   (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_M,
                                   mWordAddr, 2, ref byRxBuff))
                {
                    MakeMsg_Error(strTitle + " M비트 읽기 실패", m_nthNo);
                    return false;
                }

                // 이벤트 비트 추출
                bool unloadComp1 = GetMBitFromBuf(byRxBuff, mBitOffset + 1);
                bool loadComp1   = GetMBitFromBuf(byRxBuff, mBitOffset + 2);
                bool unloadComp2 = GetMBitFromBuf(byRxBuff, mBitOffset + 3);
                bool loadComp2   = GetMBitFromBuf(byRxBuff, mBitOffset + 4);
                bool workOrder   = GetMBitFromBuf(byRxBuff, mBitOffset + 6);

                // 기준 CVData 키 (이벤트는 CV 단위 → m_nFrTrackNo 키 사용)
                int dicKey = m_nFrTrackNo;
                if (!CvDic.ContainsKey(dicKey))
                    CvDic.Add(dicKey, new CVData());

                CVData cv = CvDic[dicKey];

                // ─── Unload Complete #1 (RGV측) ─────────────────────────────
                if (unloadComp1 && !cv.UnloadComp1Acked)
                {
                    if (!WriteMBit(ackBase + 0, true)) return false;
                    cv.UnloadComp1Acked = true;
                    InsertWcsLogPgr(m_nFrTrackNo.ToString("000"),
                        strTitle + " Unload Complete #1 ACK ON → M" + (ackBase + 0));
                    MakeMsg_Imp(strTitle + " Unload Complete #1 감지, ACK M" + (ackBase + 0) + " ON", m_nthNo);
                }
                else if (!unloadComp1 && cv.UnloadComp1Acked)
                {
                    if (!WriteMBit(ackBase + 0, false)) return false;
                    cv.UnloadComp1Acked = false;
                    MakeMsg(strTitle + " Unload Complete #1 해제, ACK OFF", m_nthNo);
                }

                // ─── Load Complete #1 (RGV측) ───────────────────────────────
                if (loadComp1 && !cv.LoadComp1Acked)
                {
                    if (!WriteMBit(ackBase + 1, true)) return false;
                    cv.LoadComp1Acked = true;
                    InsertWcsLogPgr(m_nFrTrackNo.ToString("000"),
                        strTitle + " Load Complete #1 ACK ON → M" + (ackBase + 1));
                    MakeMsg_Imp(strTitle + " Load Complete #1 감지, ACK M" + (ackBase + 1) + " ON", m_nthNo);
                }
                else if (!loadComp1 && cv.LoadComp1Acked)
                {
                    if (!WriteMBit(ackBase + 1, false)) return false;
                    cv.LoadComp1Acked = false;
                    MakeMsg(strTitle + " Load Complete #1 해제, ACK OFF", m_nthNo);
                }

                // ─── Unload Complete #2 (작업자 반출) ───────────────────────
                if (unloadComp2 && !cv.UnloadComp2Acked)
                {
                    if (!WriteMBit(ackBase + 2, true)) return false;
                    cv.UnloadComp2Acked = true;
                    InsertWcsLogPgr(m_nFrTrackNo.ToString("000"),
                        strTitle + " Unload Complete #2 ACK ON → M" + (ackBase + 2));
                    MakeMsg_Imp(strTitle + " Unload Complete #2 감지, ACK M" + (ackBase + 2) + " ON", m_nthNo);
                }
                else if (!unloadComp2 && cv.UnloadComp2Acked)
                {
                    if (!WriteMBit(ackBase + 2, false)) return false;
                    cv.UnloadComp2Acked = false;
                    MakeMsg(strTitle + " Unload Complete #2 해제, ACK OFF", m_nthNo);
                }

                // ─── Load Complete #2 (입고쪽) ──────────────────────────────
                if (loadComp2 && !cv.LoadComp2Acked)
                {
                    if (!WriteMBit(ackBase + 3, true)) return false;
                    cv.LoadComp2Acked = true;
                    InsertWcsLogPgr(m_nFrTrackNo.ToString("000"),
                        strTitle + " Load Complete #2 ACK ON → M" + (ackBase + 3));
                    MakeMsg_Imp(strTitle + " Load Complete #2 감지, ACK M" + (ackBase + 3) + " ON", m_nthNo);
                }
                else if (!loadComp2 && cv.LoadComp2Acked)
                {
                    if (!WriteMBit(ackBase + 3, false)) return false;
                    cv.LoadComp2Acked = false;
                    MakeMsg(strTitle + " Load Complete #2 해제, ACK OFF", m_nthNo);
                }

                // ─── W.O 비트 DB 반영 (HOST_SEND_YN 트리거) ─────────────────
                if (workOrder)
                {
                    MakeMsg_Imp(strTitle + " W.O 비트 ON M" + (mBase + 6) + " - 상위 보고 대기", m_nthNo);
                }

                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + " Exception: " + ex.Message, m_nthNo);
                return false;
            }
        }
        #endregion

        #region [CvTrackingWrite] :: R 영역 트래킹 JOB 쓰기
        /// <summary>
        /// PPT Slide 8~15 시나리오 / Slide 26 Tracking Area:
        ///   DB CV_DATA에서 TRACKING_WRITE_YN = 'Y' 인 트랙 조회
        ///   → R 영역에 JOB NO BCD 인코딩 쓰기
        ///   → TRACKING_WRITE_YN = 'N' 업데이트
        ///
        ///   R 주소: (CV 기계번호-1)*10 + 슬롯*2
        ///   JOB NO: 2 word, BCD 인코딩 (1234 → 2143)
        /// </summary>
        private bool CvTrackingWrite(int Idx)
        {
            string strTitle = "[CvTrackingWrite]";
            try
            {
                // TRACKING_WRITE_YN = 'Y' 인 트랙 조회
                string sql = "";
                sql += cDefApp.CRLF + "SELECT CD.MC_NO, CD.LUGG_NO_OD, CD.DEST_POS_OD            ";
                sql += cDefApp.CRLF + "  FROM CV_DATA CD                                          ";
                sql += cDefApp.CRLF + " WHERE CD.WH_TYP = :WH_TYP                                ";
                sql += cDefApp.CRLF + "   AND CD.PLC_NO = :PLC_NO                                 ";
                sql += cDefApp.CRLF + "   AND CD.MC_NO BETWEEN :FROM_TRACK AND :TO_TRACK          ";
                sql += cDefApp.CRLF + "   AND CD.TRACKING_WRITE_YN = 'Y'                          ";
                sql += cDefApp.CRLF + " ORDER BY CD.MC_NO                                         ";
                sql += cDefApp.CRLF + " LIMIT 5;                                                  ";

                m_msQPlc._pBdb.mComMain.CommandType = System.Data.CommandType.Text;
                m_msQPlc._pBdb.mComMain.Parameters.Clear();
                m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP",      DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO",      DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_msQPlc._pBdb.mComMain.Parameters.Add("FROM_TRACK",  DbLang.VARCHAR, 255).Value = m_nFrTrackNo.ToString("000");
                m_msQPlc._pBdb.mComMain.Parameters.Add("TO_TRACK",    DbLang.VARCHAR, 255).Value = m_nToTrackNo.ToString("000");

                int cnt = m_msQPlc._pBdb.ExcuteQry(sql);
                if (cnt < 0)
                {
                    MakeMsg_Error(strTitle + " TRACKING_WRITE_YN 조회 오류", m_nthNo);
                    return false;
                }
                if (cnt == 0) return true;

                for (int i = 0; i < cnt; i++)
                {
                    string mcNo   = m_msQPlc._pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    int trackNo   = Convert.ToInt32("0" + mcNo);
                    int luggNo    = Convert.ToInt32("0" + m_msQPlc._pBdb.mDtMain.Rows[i]["LUGG_NO_OD"].ToString());
                    int destPos   = Convert.ToInt32("0" + m_msQPlc._pBdb.mDtMain.Rows[i]["DEST_POS_OD"].ToString());

                    int rAddr = GetRTrackingAddr(trackNo);

                    // JOB NO 2 word BCD 인코딩 후 R 영역 쓰기
                    byte[] txBuf = new byte[4];
                    EncodeJobNoR(luggNo, txBuf, 0);

                    if (!m_msQPlc.WRITE((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                        (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_R,
                                        rAddr, 2, txBuf))
                    {
                        if (m_msQPlc.IsHex)
                        {
                            MakeMsg_Error(strTitle + " R 쓰기 TX [" + m_msQPlc.SndHexString + "]", m_nthNo);
                            MakeMsg_Error(strTitle + " R 쓰기 RX [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                        }
                        InsertWcsLogPgr(mcNo, strTitle + " R" + rAddr + " JOB[" + luggNo + "] 쓰기 실패");
                        return false;
                    }

                    if (m_msQPlc.IsHex)
                    {
                        MakeMsg_Imp(strTitle + " R 쓰기 TX [" + m_msQPlc.SndHexString + "]", m_nthNo);
                        MakeMsg_Imp(strTitle + " R 쓰기 RX [" + m_msQPlc.RcvHexString + "]", m_nthNo);
                    }

                    MakeMsg_Imp(strTitle + " TRACK[" + mcNo + "] R" + rAddr
                                + " JOB[" + luggNo + "] DEST[" + destPos + "] 쓰기 성공", m_nthNo);
                    InsertWcsLogPgr(mcNo, strTitle + " R" + rAddr + " JOB[" + luggNo + "] 쓰기 성공");

                    // DB TRACKING_WRITE_YN 초기화
                    m_msQPlc._pBdb.BeginTrans();
                    string updSql = "";
                    updSql += CRLF + "UPDATE CV_DATA                              ";
                    updSql += CRLF + "   SET TRACKING_WRITE_YN = 'N'             ";
                    updSql += CRLF + "      ,WRITE_UPD_DT = " + DbLang.SYSDATE + "      ";
                    updSql += CRLF + " WHERE WH_TYP = :WH_TYP                    ";
                    updSql += CRLF + "   AND PLC_NO  = :PLC_NO                   ";
                    updSql += CRLF + "   AND MC_NO   = :MC_NO                    ";

                    m_msQPlc._pBdb.mComMain.CommandType = System.Data.CommandType.Text;
                    m_msQPlc._pBdb.mComMain.Parameters.Clear();
                    m_msQPlc._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                    m_msQPlc._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                    m_msQPlc._pBdb.mComMain.Parameters.Add("MC_NO",  DbLang.VARCHAR, 255).Value = mcNo;

                    int upd = m_msQPlc._pBdb.ExcuteNonQry(updSql);
                    if (upd < 0)
                    {
                        m_msQPlc._pBdb.Rollback();
                        MakeMsg_Error(strTitle + " TRACKING_WRITE_YN 업데이트 오류", m_nthNo);
                        return false;
                    }
                    m_msQPlc._pBdb.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                m_msQPlc._pBdb.Rollback();
                MakeMsg_Error(strTitle + " Exception: " + ex.Message, m_nthNo);
                return false;
            }
        }
        #endregion

        #region [CvAlarmCheck] :: 알람 보고 M 비트(M0492/M0493) 처리
        /// <summary>
        /// PPT Slide 7 시나리오 (Alarm Report):
        ///   M0492 ON → 알람 세트 보고 → ECS ACK M0963 ON
        ///   M0492 OFF → M0963 OFF
        ///   M0493 ON → 알람 리셋 보고 → ECS ACK M0964 ON
        ///   M0493 OFF → M0964 OFF
        /// </summary>
        private bool CvAlarmCheck(int Idx)
        {
            string strTitle = "[CvAlarmCheck]";
            const int M_ALARM_SET   = 492; // PLC → ECS: 알람 발생
            const int M_ALARM_RST   = 493; // PLC → ECS: 알람 해제
            const int M_ALARM_SET_ACK = 963; // ECS → PLC: 알람 발생 ACK
            const int M_ALARM_RST_ACK = 964; // ECS → PLC: 알람 해제 ACK

            try
            {
                // M0492, M0493 읽기 (같은 워드: 492/16=30, 493/16=30)
                byte[] byRxBuff = new byte[100];
                Array.Clear(byRxBuff, 0, byRxBuff.Length);
                if (!m_msQPlc.READ((byte)MelsecQ3E_UnitType.MELSECQ_CMD_WORD_UNIT,
                                   (byte)MelsecQ3E_UnitType_DEVICE.MELSECQ_DEVICE_CODE_M,
                                   M_ALARM_SET / 16, 1, ref byRxBuff))
                {
                    MakeMsg_Error(strTitle + " M알람 읽기 실패", m_nthNo);
                    return false;
                }

                int wordVal   = byRxBuff[0] | (byRxBuff[1] << 8);
                bool alarmSet = (wordVal & (1 << (M_ALARM_SET % 16))) != 0; // M0492
                bool alarmRst = (wordVal & (1 << (M_ALARM_RST % 16))) != 0; // M0493

                int dicKey = 0; // 알람은 CV 전체 공통 키 0 사용
                if (!CvDic.ContainsKey(dicKey))
                    CvDic.Add(dicKey, new CVData());
                CVData cvGlobal = CvDic[dicKey];

                // ─── 알람 세트 보고 ──────────────────────────────────────────
                if (alarmSet && !cvGlobal.AlarmSetAcked)
                {
                    if (!WriteMBit(M_ALARM_SET_ACK, true)) return false;
                    cvGlobal.AlarmSetAcked = true;
                    InsertWcsLogPgr("000", strTitle + " 알람 SET 보고 감지 → ACK M" + M_ALARM_SET_ACK + " ON");
                    MakeMsg_Imp(strTitle + " 알람 SET 감지 M" + M_ALARM_SET + ", ACK M" + M_ALARM_SET_ACK + " ON", m_nthNo);
                }
                else if (!alarmSet && cvGlobal.AlarmSetAcked)
                {
                    if (!WriteMBit(M_ALARM_SET_ACK, false)) return false;
                    cvGlobal.AlarmSetAcked = false;
                    MakeMsg(strTitle + " 알람 SET 해제, ACK M" + M_ALARM_SET_ACK + " OFF", m_nthNo);
                }

                // ─── 알람 리셋 보고 ──────────────────────────────────────────
                if (alarmRst && !cvGlobal.AlarmRstAcked)
                {
                    if (!WriteMBit(M_ALARM_RST_ACK, true)) return false;
                    cvGlobal.AlarmRstAcked = true;
                    InsertWcsLogPgr("000", strTitle + " 알람 RST 보고 감지 → ACK M" + M_ALARM_RST_ACK + " ON");
                    MakeMsg_Imp(strTitle + " 알람 RST 감지 M" + M_ALARM_RST + ", ACK M" + M_ALARM_RST_ACK + " ON", m_nthNo);
                }
                else if (!alarmRst && cvGlobal.AlarmRstAcked)
                {
                    if (!WriteMBit(M_ALARM_RST_ACK, false)) return false;
                    cvGlobal.AlarmRstAcked = false;
                    MakeMsg(strTitle + " 알람 RST 해제, ACK M" + M_ALARM_RST_ACK + " OFF", m_nthNo);
                }

                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + " Exception: " + ex.Message, m_nthNo);
                return false;
            }
        }
        #endregion
        //*/
    }
}
