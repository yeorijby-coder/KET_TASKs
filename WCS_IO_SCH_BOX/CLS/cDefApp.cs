using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Threading;
using Microsoft.VisualBasic;

namespace TSK_COMM_IOSCH
{
    public class cDefApp
    {
        public static OleDbConnection GM_DB_CN = null;          // @.DB연결 객체[OracleConnection]
        public const string GM_ENV_INI = "./ENV_IOSCH.INI";    // @.INI 파일경로

        // @@.W/C 접속정보
        public static string GM_DB_PROVIDER = "";
        public static string GM_DB_ALIAS = "";
        public static string GM_DB_USERID = "";
        public static string GM_DB_PASSWORD = "";

        //(post)접속정보
        public static string GM_PDB_IP = "";
        public static string GM_PDB_DATABASE = "";
        public static string GM_PDB_PORT = "";
        public static string GM_PDB_USER = "";
        public static string GM_PDB_USER_PW = "";

        //LOG파일 접속정보
        public static string GM_LOG_PATH = "";
        public static string GM_FILENAME = "";

        //LOG DEL 파일 접속정보
        public static string GM_LOG_DEL_YEAR = "0";
        public static string GM_LOG_DEL_MONTH = "0";
        public static string GM_LOG_DEL_DAY = "0";
        public static string GM_LOG_DEL_HOUR = "0";
        public static string GM_LOG_DEL_MINUTE = "0";
        public static string GM_LOG_DEL_SECOND = "0";
        public static string GM_LOG_DEL_ADDVALUE = "0";
        public static string GM_LOG_DEL_ADDTYPE = "MINUTE";

        // @@.응용프로그램 타임아웃 설정
        public static int GM_COMM_SND_TIME_OUT = 500;
        public static int GM_COMM_RCV_TIME_OUT = 500;

        // @@.통신포트별 통신 설정 정보
        public static string GM_COMM1_PLCNO = "001";
        public static string GM_COMM1_IP = "127.0.0.1";
        public static int GM_COMM1_PORT = 3001;
        public static int GM_COMM1_START_ADDRESS = 3001;
        public static int GM_COMM1_BARCODE_ADDRESS = 3001;
        public static int GM_COMM1_FROM_TRACK = 3001;
        public static int GM_COMM1_TO_TRACK = 3001;

        public static string GM_COMM2_PLCNO = "002";
        public static string GM_COMM2_IP = "127.0.0.1";
        public static int GM_COMM2_PORT = 3002;
        public static int GM_COMM2_START_ADDRESS = 3001;
        public static int GM_COMM2_BARCODE_ADDRESS = 3001;
        public static int GM_COMM2_FROM_TRACK = 3001;
        public static int GM_COMM2_TO_TRACK = 3001;

        public static string GM_COMM3_PLCNO = "003";
        public static string GM_COMM3_IP = "127.0.0.1";
        public static int GM_COMM3_PORT = 3003;
        public static int GM_COMM3_START_ADDRESS = 3001;
        public static int GM_COMM3_BARCODE_ADDRESS = 3001;
        public static int GM_COMM3_FROM_TRACK = 3001;
        public static int GM_COMM3_TO_TRACK = 3001;

        public static Queue<LogParam>[] m_LogQ = new Queue<LogParam>[200];


        // @@.Application 전역 상태 관리 변수 정의
        public static bool  GM_STAT_MAIN = false;  // @.전체 시스템 운전 상태[운전 시스템이 운전 되면 전체 운전!]
        public static bool GM_RE_START = false;

        public static bool GM_SND_MES = false;  // @.전체 시스템 운전 상태[운전 시스템이 운전 되면 전체 운전!]


        // @@.구분자
        public const string cSPA = ";";

        public static string mStrLogTypeCv = "CV";

        //로그 삭제
        public static string mStrLogTypeDel = "LOGDelete";

        //RP 재처리
        public static string mStrLogTypeRpOut = "RPOUT";

        // @@.통신처리작업정보체
        public struct stutComProc
        {
            public bool    bMakeTgmSnd;     // @.송신Tgm작성
            public bool    bSndTgm;         // @.Tgm송신
            public bool    bRcvTgm;         // @.Tgm수신
            public int     nChkTgm;         // @.수신Tgm체크
            public bool    bDBProc;         // @.DB처리
            public string  sProcMsg;        // @.처리메세지

            public void init()
            {                               // @.정보체 멤버변수 초기화
                this.bMakeTgmSnd = false;   // @.송신Tgm작성
                this.bSndTgm = false;       // @.Tgm송신
                this.bRcvTgm = false;       // @.Tgm수신
                this.nChkTgm = 99;          // @.수신Tgm체크
                this.bDBProc = false;       // @.DB처리
                this.sProcMsg = "";         // @.처리메세지
            }
        }

        // @@.DB Err 정의
        public const int DB_ERR = -1;      // @.DB 오류
        public const int DB_LOCK = -2;     // @.DB 오류로 DB Lock
        public const int DB_DUP = -3;      // @.DB 오류로 중복 데이터

        // @@.enum 정의
        public enum eComSts { ComNor = 0, ComErr = 1 };                    // @.통신상태
        public enum eLogMsgType { MSG_NOR = 0, MSG_IMP = 1, MSG_ERR = 2 }; // @.eLogMsgType[0:일반, 1:중요, 2:오류]

        public enum eThGbn
        {
            R_GR01 = 0,
            CV_GR01 = 1, CV_GR02 = 2, CV_GR03 = 3, CV_GR04 = 4, CV_GR05 = 5,
            CV_GR06 = 6, CV_GR07 = 7, CV_GR08 = 8, CV_GR09 = 9, CV_GR10 = 10,
            CV_GR11 = 11, CV_GR12 = 12, CV_GR13 = 13, CV_GR14 = 14, CV_GR15 = 15,
            SC_GR22 = 22,
            SCH_GR01 = 50   // LG생명과학 자동 반송 Scheduler
        };  // @.eThGbn[0 ,1:CV, 2:RTV, 3:SC, 4:SCH]

        // LG생명과학 창고 자동 반송 설비 수량 (KET 현장은 RTV/RGV 미설치 - RGV 수량 제거)
        public const int LG_SC_COUNT  = 5;   // S/C #1~#5
        public const int LG_CV_COUNT  = 15;  // C/V #1~#15

        // 입고가능 (10 = 화학품, 20 = 완제품), 출고 (30 = 화학품, 40 = 완제품)
        public enum eWHTYP { KET_WH01 = 10, KET_WH02 = 20 };  // @.WH_TYP
        //public enum eJOBTYP { Sto = 1, Ret = 2, Move = 3, RtoR = 4, Aisle2Aisle = 5, RackSto = 6, OtherRet = 7, FireRet = 8, RackRet = 9, RtvRet = 12 };  // @.입고 구분
        public enum eJOBTYP
        {
            Sto = 1, Ret = 2, PRet = 3, RtoR = 4, Aisle2Aisle = 5, Move = 6, OtherRet = 7, FireRet = 8, RackRet = 9,// RackSto = 10, RtvRet = 12, //이정민 주석
            ManualMove = 10, ManualSto = 11, ManualRet = 12, ManualPickingRet = 13, ManualRtoR = 14, ManualRackMove = 15,
            DupicateSto = 20
        };  // @.입고 출고

        public enum eTRACK_NM { ASSEM01 = 1264, ASSEM02 = 1064, ASSEM0102 = 15051, MGDP = 9999, MGDPComp = 9998 };
        // 291 완제품:0, 상온1:1, 상온2:2, 상온3:3, 고온1:4, 고온2:5, 고온3:6, 냉동1:7, 냉동2:8, 냉동3:9
        public enum eAGINGTYP
        {
            AING_CNT = 1,
            AGING_1G = 9
        };

        // @@.Structure 정의
        public struct stutLogMsgInfo
        {
            public string Time;
            public string ID;
            public string MsgTyp;
            public string Com;
            public string File;         // 이 로그를 남긴 소스 파일 이름
            public string Func;         // 이 로그를 남긴 함수 이름
            public string Msg;
            public string Tgm;
            public void init()
            {
                this.Time   = "";
                this.ID     = "";
                this.MsgTyp = "";
                this.Com    = "";
                this.File   = "";
                this.Func   = "";
                this.Msg    = "";
                this.Tgm    = "";
            }
        }

        // @@.Structure 정의
        public struct stutAging
        {
            public string Time;
            public string ID;
            public string MsgTyp;
            public string Com;
            public string Msg;
            public string Tgm;
            public void init()
            {
                this.Time   = "";
                this.ID     = "";
                this.MsgTyp = "";
                this.Com    = "";
                this.Msg    = "";
                this.Tgm    = "";
            }
        }

        public const string CRLF = ControlChars.CrLf; // @.줄바꿈[vbCrLf]
        public const byte   STX  = 0x2;
        public const byte   ETX  = 0x3;
        public char GM_STR_STX = Convert.ToChar(2);
        public char GM_STR_ETX = Convert.ToChar(3);
    }
}
