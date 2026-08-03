using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualBasic;
using System.Data.OleDb;
using System.Threading;
using System.Collections;
using System.Data.SqlClient;
using Npgsql;


namespace TSK_HostCom
{
	class modDefApp
	{
		public static Boolean g_blListenThread, g_blSrvThread;
		//*********************************************************************************************   
	    // 전역 변수 정의
		public static UserInfo g_User;
		public static string g_strUserID;
		public static string g_strUserName;
		// 소켓 리슨 포트
		public static int g_iListenPort;
		// Remote IP
		public static string g_strRemoteIP;
		// 상대 포트
		public static int g_iRemotePort;
		// 시스템 운영모드
		public static OpSta g_SysMode;
        
        // 공파레트 요청 관련
        public static string g_strEmtpyPltKind;
        public static string g_strEmtpyPltStation;
        public static bool[] g_bEmtpyPltJob = new bool[10000];

		public static string g_strPcIp;
		public static string g_strPcNm;
		public static bool g_blSTOP_REQ;
        public static string g_strSYS_LANG = "LANG1";
        public static string COMPANY_CD = "F100";
        public static string AREA_CD = "F101";
        public static string SYS_GRP = "G1";
        public static string WH_CD = "AA06";
        // 창고 타입. EcsComA.ini 의 [DB] WH_TYP 로 덮어쓴다.(없으면 이 기본값)
        //   이것은 DB 안에서 창고를 구분하는 값이다. 전문에 실리는 값이 아니다.
        public static string WH_TYP = "10";

        /*
         * WH_DEF :: 전문의 창고구분 (WareHouse Define)
         *
         *   [09]Interface목록서[ECS-자동창고] 의 모든 전문에 들어가는 1바이트 항목.
         *     A : PalletRack 자동창고
         *     B : P-BoxRack  자동창고
         *   위의 WH_TYP(DB 창고타입)과는 전혀 다른 항목이니 섞지 말 것.
         *   EcsComA.ini 의 [DB] WH_DEF 로 덮어쓴다.
         */
        public static string WH_DEF = "A";

        /*
         * 전문의 장비분류 (Device Class) - 문서 IV.3 / IV.4
         */
        public const int DEV_CLASS_SC = 1;      // S/C
        public const int DEV_CLASS_CV = 2;      // C/V
        public const int DEV_CLASS_LGV = 3;     // LGV
        public const int DEV_CLASS_RGV = 4;     // RGV
        public const int DEV_CLASS_BCR = 6;     // BCR

        /*
         * 완료 차수 (Step Count) - 문서 IV.5
         *   1 : 1차완료,  2 : 최종완료
         */
        public const int STEP_FIRST = 1;
        public const int STEP_FINAL = 2;
        public const string PRS_ID = "TSK_ECSCOM";
		public static string g_strRE_DRCT_CNT;
		public static string[] g_strWH_CD;
        public static string[] g_strWH_TYP;
        public static string[] g_strCvNo;
        public static string[] g_strCvSta;
        public static bool g_blGetCvNo = false;
        public static string GM_RTN_MSG;

#if ORACLE
public const string II = "||";
public const string NVL = "NVL";
#endif
#if SQL
public const string II = "+";
public const string NVL = "ISNULL";
#endif
#if POSTGRESSQL
public const string II = "+";               // ????
public const string NVL = "COALESCE";
#endif
        //*********************************************************************************************

		//*********************************************************************************************
// 전역 객체 정의
#if ORACLE
public static OleDbConnection g_conDb;
#endif
#if SQL
	public static SqlConnection g_conDb;
#endif
#if POSTGRESSQL
    public static NpgsqlConnection g_conDb;
#endif

public static frmMain g_frmForm;
		// app 종료 여부
		public static bool g_blIsAppExit;

		// 서버소켓용 작업 Object
		public static CSrvWork g_SrvWork = new CSrvWork();
		// 클라이언트소켓용 작업 Object

		public static CCliWork g_CliWork = new CCliWork();
		// 디폴트 Read Timeout
		public const int TIME_OUT = 5;

		// 종료 이벤트
		public static AutoResetEvent g_areListenExitEvent = new AutoResetEvent(false);
		public static AutoResetEvent[] g_areLogExitEvent = new AutoResetEvent[2];

		// 리슨 소켓
		public static System.Net.Sockets.TcpListener g_tcplsn;
		// Client Socket 정보 리스트
		public static CliSocketInfo[] g_CliSockList;
		// Socket 동기화 객체
		public static object g_objSockSync = new object();
		// Log 메세지 리스트
		//Public gLogList As New ArrayList
		public static  ArrayList[] g_arrlstLogList = new ArrayList[2];

		//*********************************************************************************************

		//*********************************************************************************************
		// 전역 상수 정의
		public const string MAIN_INI = ".\\\\ECSCOMA.ini";

		public const string LOG_DIR = ".\\\\ECSCOMALog\\\\";
		// DB 에러
		public const int DB_ERR = -1;
		// DB 에러중 DB Lock
		public const int DB_LOCK = -2;
		// DB 에러중 중복 데이타

		public const int DB_DUP = -3;
		// Msg 종류
		// 보통
		public const string MSG_NOR = "NOR";
		// 중요
		public const string MSG_IMP = "IMP";
		// 에러
		public const string MSG_ERR = "ERR";

		// 표시 Icon 종류
		public const int ICON_NOR = 0;
		public const int ICON_ERR = 1;

		// 제어문자
		public const string CRLF = ControlChars.CrLf;
		public const int STX = 0x2;
		public const int ETX = 0x3;

		// SOCKET FRAME 제어 문자
		public const int TRANS_R = 0x52;	//R' - Request
		public const int TRANS_ACK = 0x41; 	//A' - 응답 Ack
		public const int TRANS_NAK = 0x4e;	//N' - 응답 Nak

		public const string MSG_ACK = "A";	//- 응답 Ack
		public const string MSG_NAK = "N";	//- 응답 Nak

		public const   int MSG_HEAD_CNT = 15;
		public const int MSG_ORDER_CNT = 61;
		public const int MSG_DUAL_STO_CNT = 31;
		public const int MSG_ARRI_BATCH_CNT = 7;

        //public const int MSG_EMPTY_PLT_CNT = 7;
        //public const int MSG_DUAL_STO_CNT = 31;
        //public const int MSG_ARRI_BATCH_CNT = 7;
		
		//  에러 메시지 코드
		public const int MSG_NO_ERROR = 0;
		public const int MSG_NO_STX = 1;
		public const int MSG_NO_ETX = 2;
		public const int MSG_LUGG_NO_DUPLICATED = 3;
		public const int MSG_NO_EXIST_JOB = 4;
		public const int MSG_ECS_BUFFER_FULL = 5;
		public const int MSG_INVALID_HEADER_LENG = 6;
		public const int MSG_INVALID_MSG_LENG = 7;
		public const int MSG_INVALID_LOC = 8;
		public const int MSG_INVALID_STN_NO = 9;
		public const int MSG_INVALID_LUGG_NO = 10;
		public const int MSG_UNKNOWN_JOB_TYPE = 11;
		public const int MSG_INHIBITED_LOC = 12;
		public const int MSG_INVALID_MAILBOX_NAME = 13;
		public const int MSG_ALREADY_INVOKED_JOB = 14;
		public const int MSG_INVALID_CONTENT = 15;
		public const int MSG_UNKNOWN_MSG_TYPE = 16;
		public const int MSG_IMPROPER_HANDSHAKE = 17;
		public const int MSG_NO_RESPONSE = 18;
		public const int MSG_SOCKET_ERROR = 19;
		public const int MSG_INTERNAL_ERROR = 99;

		// 조회조건 중 '전체'
		//Public Const ALL_QRY = "전체"

		public const string ALL_QRY = "ALL";
		// PDA Client 수

		public const int MAX_CLI_CNT = 4;
		// 통신상태
		public enum ComSts : int
		{
			ComNor = 0,
			ComErr = 1
		}
		
		// RUN /STOP (wms운영상태)
		public enum OpSta : int
		{
			Unkonown = 0,
			RunSta = 1,
			StopSta = 2
		}
        #region 작업정보 처리 상수들
        public enum EN_JOB_TYPE : int
        {
            enJobTypeNone = 0,
            enJobTypeAutoSto = 1, enJobTypeAutoRet = 2, enJobTypeAutoPR = 3, enJobTypeAutoR2R = 4, enJobTypeAutoW2W = 5, enJobTypeAutoMove = 6,
            enJobTypeSemiSto = 11, enJobTypeSemiRet = 12, enJobTypeSemiPR = 13, enJobTypeSemiR2R = 14, enJobTypeSemiW2W = 15, enJobTypeSemiMove = 16,
            enJobTypeManual = 21
        };
        public enum EN_JOB_PATTERN : int
        {
            enJobPatternNone,
            enJobPatternSto, enJobPatternRet, enJobPatternPR, enJobPatternR2R, enJobPatternW2W, enJobPatternMove
        };

        public enum EN_JOB_STATUS : int
        {
            enJobStatusAll, enJobStatusNew,
            enJobStatusComplete, enJobStatusArrived, enJobStatusCancel,
            enJobStatusErrorEmptyRetrieve, enJobStatusErrorDualStore,
            enJobStatusErrorEmptyRetrieve2, enJobStatusErrorDualStore2,
            enJobStatusDualStoreRetry, enJobStatusEmptyRetrieveRetry,
            enJobStatusCvNew, enJobStatusCvInvoke,
            enJobStatusScRequest, enJobStatusScInvoke, enJobStatusScComplete,
            enJobStatusRtvInvoke, enJobStatusRtvComplete,
            enJobStatusLifterInvoke, enJobStatusLifterComplete, enJobStatusLifterCvInvoke,
            enJobStatusLgvInvoke, enJobStatusLgvComplete,
            enJobStatusResend,
            enJobStatusCompleteRequest, enJobStatusRefuseComplete,
            enJobStatusError, enJobStatusRefuseError, enJobStatusInvokeError,
            enJobStatusRefuseArrived,
            enJobStatusSize
        };
        public	enum EN_LENGTH : int 
        {
            enLengthStation = 3, 
            enLengthLocation = 7,
            enLengthControlID = 10
        };

        #region 작업대 번호 / 트랙 번호  (원본 Common/Include/Ecs/EcsDef.h 값 그대로)
        //
        //   작업대 번호  : WMS(HOST) 와 주고받는 전문에서 쓰는 번호. CV_DATA.HOST_STN_NO
        //   트랙 번호    : WCS 내부 설비 식별 번호.                  CV_DATA.TRACK_NO
        //   MC_NO        : 트랙번호(4자리)에서 가운데 0 을 뺀 3자리 표기
        //
        // 작업대 번호 (ECS_STN_POS_*)
        public const int ECS_STN_POS_3F_PLT_207 = 207;   // 3층 Pallet Magazine
        public const int ECS_STN_POS_3F_BOX_221 = 221;   // 자동 입고 대기 #1
        public const int ECS_STN_POS_3F_BOX_222 = 222;   // 자동 입고 대기 #2

        // 트랙 번호 (CNV_STN_POS_*)
        //   참고용으로만 둔다. 실제 조회는 CV_DATA.HOST_STN_NO 로 한다.
        //   CV_DATA 는 TRACK_NO(4자리) 와 MC_NO(3자리, 가운데 0 을 뺀 표기) 를 함께 갖는다.
        public const int CNV_STN_POS_3F_PLT_207 = 3001;   // MC_NO 301
        public const int CNV_STN_POS_3F_BOX_221 = 4019;   // MC_NO 419
        public const int CNV_STN_POS_3F_BOX_222 = 4020;   // MC_NO 420
        #endregion

        public enum EN_STN_KIND : int 
        { 
            enScStation, enStoStation, enRetStation, enArvStation 
        };

        #endregion

        #region 작업정보 처리 함수들
        public static int ConvertJobPattern(int nJobType)
        {
            switch (nJobType)
            {
                case (int)EN_JOB_TYPE.enJobTypeAutoSto:
                case (int)EN_JOB_TYPE.enJobTypeSemiSto:
                    return (int)EN_JOB_PATTERN.enJobPatternSto;

                case (int)EN_JOB_TYPE.enJobTypeAutoRet:
                case (int)EN_JOB_TYPE.enJobTypeSemiRet:
                    return (int)EN_JOB_PATTERN.enJobPatternRet;

                case (int)EN_JOB_TYPE.enJobTypeAutoR2R:
                case (int)EN_JOB_TYPE.enJobTypeSemiR2R:
                    return (int)EN_JOB_PATTERN.enJobPatternR2R;

                case (int)EN_JOB_TYPE.enJobTypeAutoMove:
                case (int)EN_JOB_TYPE.enJobTypeSemiMove:
                    return (int)EN_JOB_PATTERN.enJobPatternMove;

                case (int)EN_JOB_TYPE.enJobTypeAutoW2W:
                case (int)EN_JOB_TYPE.enJobTypeSemiW2W:
                    return (int)EN_JOB_PATTERN.enJobPatternW2W;
            }
            return (int)EN_JOB_PATTERN.enJobPatternNone;
        }
        public static bool IsOfflineJobType(int nJobType)
        {
	        switch (nJobType)
	        {
            case (int)EN_JOB_TYPE.enJobTypeSemiSto:
            case (int)EN_JOB_TYPE.enJobTypeSemiRet:
            case (int)EN_JOB_TYPE.enJobTypeSemiR2R:
            case (int)EN_JOB_TYPE.enJobTypeSemiPR:
            case (int)EN_JOB_TYPE.enJobTypeSemiMove:
            case (int)EN_JOB_TYPE.enJobTypeSemiW2W:	
		        return true;
	        }

	        return false;
        }
        public static bool IsOnlineJobType(int nJobType)
        {
            switch (nJobType)
            {
                case (int)EN_JOB_TYPE.enJobTypeAutoSto:
                case (int)EN_JOB_TYPE.enJobTypeAutoRet:
                case (int)EN_JOB_TYPE.enJobTypeAutoR2R:
                case (int)EN_JOB_TYPE.enJobTypeAutoPR:
                case (int)EN_JOB_TYPE.enJobTypeAutoMove:
                case (int)EN_JOB_TYPE.enJobTypeAutoW2W:	
                    return true;
            }

            return false;
        }
        public static bool IsOnlineLuggNum(int nLuggNum)
        {
            return ((nLuggNum > 0) && (nLuggNum < 9000)) ? true : false;
        }

        public static bool IsOfflineLuggNum(int nLuggNum)
        {
            return ((nLuggNum > 9000) && (nLuggNum < 9999)) ? true : false;
        }
        public static bool IsValidID(string strStationID)
        {
            if (strStationID != "" || strStationID.Length == 1 || strStationID.Length == 3)
		        return true;

	        return false;
        }
        public static string GetJobTypeString(int nJobType)
        {
	        switch (nJobType)
	        {
	        case (int)EN_JOB_TYPE.enJobTypeNone:		return "없음";
	        case (int)EN_JOB_TYPE.enJobTypeAutoSto:		return "입고";
	        case (int)EN_JOB_TYPE.enJobTypeAutoRet:		return "출고";
	        case (int)EN_JOB_TYPE.enJobTypeAutoR2R:		return "랙이동";
	        case (int)EN_JOB_TYPE.enJobTypeAutoPR:		return "피킹출고";
	        case (int)EN_JOB_TYPE.enJobTypeAutoMove:	return "이동";
        //	case (int)EN_JOB_TYPE.enJobTypeAutoW2W:		return "창고간이동";
	        case (int)EN_JOB_TYPE.enJobTypeSemiSto:		return "반자동 입고";
	        case (int)EN_JOB_TYPE.enJobTypeSemiRet:		return "반자동 출고";
	        case (int)EN_JOB_TYPE.enJobTypeSemiR2R:		return "반자동 랙이동";
	        case (int)EN_JOB_TYPE.enJobTypeSemiPR:		return "반자동 피킹출고";
	        case (int)EN_JOB_TYPE.enJobTypeSemiMove:	return "반자동 이동";
        //	case (int)EN_JOB_TYPE.enJobTypeSemiW2W:		return "반자동 창고간이동";
	        case (int)EN_JOB_TYPE.enJobTypeManual:		return "수동작업";
	        }

	        return Convert.ToString(nJobType);
        }

        public static string GetJobStatusString(int nJobStatus)
        {
	        switch (nJobStatus)
	        {
	        case (int)EN_JOB_STATUS.enJobStatusAll:					return "전체";
	        case (int)EN_JOB_STATUS.enJobStatusNew:					return "신규";
	        case (int)EN_JOB_STATUS.enJobStatusComplete:			return "완료보고";
	        case (int)EN_JOB_STATUS.enJobStatusArrived:				return "도착보고";
	        case (int)EN_JOB_STATUS.enJobStatusCancel:				return "취소보고";
	        case (int)EN_JOB_STATUS.enJobStatusErrorEmptyRetrieve:	return "공 출 고	에  러";
	        case (int)EN_JOB_STATUS.enJobStatusErrorDualStore:		return "이중입고 에  러";
	        case (int)EN_JOB_STATUS.enJobStatusErrorEmptyRetrieve2:	return "공 출 고(FORK #2)";
	        case (int)EN_JOB_STATUS.enJobStatusErrorDualStore2:		return "이중입고(FORK #2)";
	        case (int)EN_JOB_STATUS.enJobStatusEmptyRetrieveRetry:	return "공 출 고	재지정";
	        case (int)EN_JOB_STATUS.enJobStatusDualStoreRetry:		return "이중입고 재지정";
	        case (int)EN_JOB_STATUS.enJobStatusCvNew:				return "C/V 구동 대기";
	        case (int)EN_JOB_STATUS.enJobStatusCvInvoke:			return "C/V 구동중";
	        case (int)EN_JOB_STATUS.enJobStatusScRequest:			return "S/C 구동 요구";
	        case (int)EN_JOB_STATUS.enJobStatusScInvoke:			return "S/C 구동중";
	        case (int)EN_JOB_STATUS.enJobStatusScComplete:			return "S/C 작업완료";
	        case (int)EN_JOB_STATUS.enJobStatusRtvInvoke:			return "RTV 구동중";
	        case (int)EN_JOB_STATUS.enJobStatusRtvComplete:			return "RTV 작업완료";
	        case (int)EN_JOB_STATUS.enJobStatusLifterInvoke:		return "Lifter 구동중";
	        case (int)EN_JOB_STATUS.enJobStatusLifterComplete:		return "Lifter 작업완료";
	        case (int)EN_JOB_STATUS.enJobStatusLifterCvInvoke:		return "Lifter C/V 구동";
	        case (int)EN_JOB_STATUS.enJobStatusLgvInvoke:			return "LGV 작업지시";
	        case (int)EN_JOB_STATUS.enJobStatusLgvComplete:			return "LGV 작업완료";
	        case (int)EN_JOB_STATUS.enJobStatusCompleteRequest:		return "완료 요청";
	        case (int)EN_JOB_STATUS.enJobStatusRefuseComplete:		return "완료 실패";
	        case (int)EN_JOB_STATUS.enJobStatusInvokeError:			return "지시실패";
	        case (int)EN_JOB_STATUS.enJobStatusError:				return "에러 보고";
	        case (int)EN_JOB_STATUS.enJobStatusRefuseError:			return "에러 보고 실패";
	        case (int)EN_JOB_STATUS.enJobStatusRefuseArrived:		return "도착 보고 실패";
	        case (int)EN_JOB_STATUS.enJobStatusResend:				return "재전송";
	        }

	        string strTemp;
            strTemp = string.Format("등록되지 않은 작업상태 [{0}]", nJobStatus);
	        return strTemp;
        }

        public static int GetSide(string strLocation)
        {
            if (strLocation.Length != (int)EN_LENGTH.enLengthLocation)
                return 0;

            return ((Convert.ToInt32(strLocation.Substring(0, 2)) + 1) % 2) + 1;
        }
        public static int GetBank(string strLocation)
        {
	        if (strLocation.Length != (int)EN_LENGTH.enLengthLocation)
		        return 0;

            return Convert.ToInt32(strLocation.Substring(0, 2));
        }

        public static int GetBay(string strLocation)
        {
	        if (strLocation.Length != (int)EN_LENGTH.enLengthLocation)
		        return 0;

            return Convert.ToInt32(strLocation.Substring(2, 3));
        }

        public static int GetLevel(string strLocation)
        {
	        if (strLocation.Length != (int)EN_LENGTH.enLengthLocation)
		        return 0;

            return Convert.ToInt32(strLocation.Substring(5, 2));
        }
        public static int GetStackerNum(int nWarehouse, string strLocation)
        {
            if (strLocation.Length != (int)EN_LENGTH.enLengthLocation)
                return 0;

            int nScNum = (GetBank(strLocation) + 1) / 2;

            return nScNum;
        }



        #endregion 
      	
        public const int	SC_STATUS_NORMAL_WAIT			= 0;
        public const int	SC_STATUS_STORING				= 1;
        public const int	SC_STATUS_RETRIEVING			= 2;
        public const int	SC_STATUS_RACK_TO_RACK			= 3;
        public const int	SC_STATUS_NO_ONLINE				= 4;
        public const int	SC_STATUS_ERROR					= 5;
        public const int	SC_STATUS_SUSPEND_STO			= 6;
        public const int	SC_STATUS_SUSPEND_RET			= 7;
        public const int	SC_STATUS_SUSPEND_ALL			= 8;

        // SC 수
		public const int SC_CNT = 2;
		// Bank수'
		public const int BANK_1_CNT = 4;
		// Bay수'필
		public const int BAY_1_CNT = 25;
		// Level수'필
		public const int LEVEL_1_CNT = 9;
		//*********************************************************************************************

		//**************************************************************************************
	    //Structure 정의

		// DB 사용자 정보
		public struct UserInfo
		{
			public string g_strUserID;
			public string g_strUserName;
			public string g_strUserPassword;
			public string g_strUserLevel;
			public string g_strDbAlias;
			//mssql
			public string g_strDatabase;

            // PostgresSql
            public string g_strDbIP;
            public string g_strDbPort;
		}

		public struct CliSocketInfo
		{
			// List내의 순서, ID
			public int g_iMyID;
			// Client IP
			public string g_strClientIP;
			// 접속한 Client ID
			public string g_strClientID;
			// 접속여부
			public bool g_blConnected;
			// 활당된 쓰레드 객체 주소
			public System.Threading.Thread g_thrThread;
			// 생성된 소켓 객체 주소
			public System.Net.Sockets.Socket g_sktSocket;

			// 소켓 쓰레드 종료 (강제종료)
			public void ManualClose()
			{
				modCmWork.CloseSocket(ref g_sktSocket);
				g_thrThread.Join();
			}

		}
		//**************************************************************************************
	}
}
