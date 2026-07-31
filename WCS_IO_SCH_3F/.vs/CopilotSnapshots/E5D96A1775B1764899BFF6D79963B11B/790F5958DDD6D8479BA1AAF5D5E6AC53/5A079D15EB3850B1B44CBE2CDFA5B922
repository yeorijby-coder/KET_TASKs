using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using Samoh_Lib;
using Npgsql;
#if POSTGRESQL
using DbConn = Npgsql.NpgsqlConnection;
#elif SQL
using DbConn = System.Data.SqlClient.SqlConnection;
#endif


namespace TSK_COMM_IOSCH
{
    public class BaseClass 
    {
        /*
         * Mcs Const
         */
        #region Mcs Const
        public const string _strUserID = "MCS";
        public const int _COMMAND_STO_ = 0;
        public const int _COMMAND_RET_ = 1;
        public const int _COMMAND_RTR_ = 2;
        public const int _COMMAND_STS_ = 3;
        public int nStep = _COMMAND_STO_;
        #endregion
        /*
         * variable define
         */
        #region variable define

#if ORACLE
        public OleDbConnection _pConObj;
        public cDbUse _pBdb;
#endif
#if POSTGRESQL || SQL
        public DbConn _pConObj;
        public cDbPostUse _pBdb;
#endif

        public int check = 0;

        string _strSql = "";
        private string _strConnectionString;
        private bool _IsDBOpen;
        string _CRLF = "\r\n";
        string _maindefineRtnMsg;
        private int _nSelCnt;

        public string strSql { get { return _strSql; } set { _strSql = value; } }
        public string CRLF { get { return _CRLF; } }
        public string crlf { get { return _CRLF; } }
        public string IsRtnMsg { get { return _maindefineRtnMsg; } set { _maindefineRtnMsg = value; } }
        public int SelCnt { get { return _nSelCnt; } set { _nSelCnt = value; } }
        public string ConnectionString { get { return _strConnectionString; } set { _strConnectionString = value; } }
        public bool IsDBOpen { get { return _IsDBOpen; } set { _IsDBOpen = value; } }

        #endregion variable define
        /*
         * Track No Define
         */
		//이전 작업을 체크한다.
        #region RobotPicker Define
		public const string RP_EMPTY = "empty";				//비어있다
		public const string RP_PICKUP = "pickup";			//Pickup작업
		public const string RP_UNLOAD = "unload";			//Unload작업
		public const string RP_MOVE = "move";				//Move작업
		public const string RP_HOME = "home";				//Home작업
		public const string RP_MSTACK = "measure stack";	//측정 작업
        #endregion Track No Define
        /*
         * Request Command Define
         */
        #region Request Command Define
        public const string CmdRequest = "RQ";
        public const string CmdResultError = "ER";
        public const string CmdResultOk = "OK";
        public const string CmdNoData = "OD";
        public const string CmdNoDefine = "  ";
        public const string CV_REQUEST      = "01";
        public const string CV_DOING        = "02";
        public const string SC_REQUEST      = "03";
        public const string SC_DOING        = "04";
        public const string SC_COMPLET      = "05";
        public const string BCR_REQUEST     = "11"; //SC_COMPLET후
        public const string BAGGING_START   = "B1";
        public const string BAGGING_WCT     = "B2";
        public const string BAGGING_EMG     = "B3";
        public const string BAGGING_LBL     = "B4";
        public const string BAGGING_END     = "B5";
  

        public const string CMD_IOGB_PA     = "01";
        public const string CMD_IOGB_PK     = "02";
        public const string CMD_IOGB_RTR    = "03";
        public const string CMD_IOGB_MV     = "04";
        public const string CMD_IOGB_BAGGING= "05";

        #endregion Track No Define
        /*
         * CV Read variable
         */
        #region CV Read define
        private int         _CV_JOB_KIND        ;
        private int         _CV_LUGGNO         ;
        private int         _CV_SOUR_SITE      ;
        private int         _CV_DEST_SITE       ;
        private int         _CV_DATA_STA        ;
        private bool        _CV_PA_RD;
        private bool        _CV_PK_RD;
        private bool        _CV_HA_RD;
        private bool        _CV_HK_RD;
        private bool        _CV_LG_EX;
        private int         _CV_ERR_CODE        ;
        private string      _CV_BOX_TYP;
        private string      _CV_JEPUM_GRADE     ;
        private bool        _CV_BOX_START;
        private bool        _CV_BOX_DEST;
        private bool        _CV_LGV_START;
        private bool        _CV_LGV_DEST;
        private bool        _CV_WEIGHT_STATION  ;
        private bool        _CV_MERGE_STATION;
        private bool        _CV_REMAIN_STATION;
        private bool        _CV_LIFTER_START;
        private bool        _CV_LIFTER_DEST;
        private string      _CV_BCD_DATA;
        private int         _CV_IO_TYP;
        private int         _CV_IO_MODE;
        private int         _CV_CONTROL1 ;
        private int         _CV_CONTROL2 ;
        private int         _CV_CONTROL3 ;
        //2010.01 조형준 추가-------------
        private int         _CV_CONTROL;
        //--------------------------------
        private int         _CV_TRK_PAUSE;
        private int         _CV_STATUS;
        private int         _CV_SENSOR;
        private int         _CV_MOTOR;
        private bool        _PA_HS_YON;
        private bool        _PK_HS_YON;
        private bool        _PA_MC_YON;
        private bool        _PK_MC_YON;
        private bool        _PS_MC_YON;
        private bool        _PP_MC_YON;
        private string      _LINE_ID;
        private bool        _BAGGING_WC_YON;
        private bool        _BAGGING_INSPECTION_YON;
        private bool        _BAGGING_LABEL_YON;
        private bool        _BAGGING_START_YON;
        private bool        _BREAKING_SCAN_YON;
        private bool        _BREAKING_CLEAN_YON;
        private bool        _BOX_AUTO_RET_YON;
        private bool        _BAGGING_START_WC_YON;
        private bool        _BAGGING_START_BCR_YON;
        private bool        _BAGGING_EMG_LINE_YON;
        private bool        _BAGGING_OP_GRADE_MIS_LINE_YON;

        private int         _BCR_NO             ;
        private string      _BCR_READ_STA       ;
        private string      _BCR_COM_STA        ;
        private string      _BCR_RCV_VAL1       ;
        private string      _BCR_RCV_VAL2;
        private string      _BCR_RCV_VAL3;
        private int         _BCR_ERR_CODE;
        private DateTime    _BCR_ERR_OC_DT;
        private DateTime    _BCR_RCV_DT;
        private int         _WGT_NO             ;
        private string      _WGT_READ_STA       ;
        private string      _WGT_COM_STA        ;
        private double      _WGT_RCV_VAL;
        private int         _WGT_ERR_CODE       ;
        private DateTime    _WGT_ERR_OC_DT;
        private DateTime    _WGT_RCV_DT;
        private string      _SITENO;
        private int         _MOUDLE_ID;
        private int         _DEFAULT_TRK_POS;
        private int         _TRACK_NO;
        private string      _OD_CMD_CV;
        private DateTime    _OD_CMD_DT;
        private string      _RT_CMD_CV;
        private DateTime    _RT_CMD_DT;

        private string      _WH_TYP;
        private string      _MC_TYP;

        public string   OD_CMD_CV           { set { _OD_CMD_CV         = value; } get { return _OD_CMD_CV; } }
        public DateTime OD_CMD_DT           { set { _OD_CMD_DT         = value; } get { return _OD_CMD_DT; } }
        public string   RT_CMD_CV           { set { _RT_CMD_CV         = value; } get { return _RT_CMD_CV; } }
        public DateTime RT_CMD_DT           { set { _RT_CMD_DT         = value; } get { return _RT_CMD_DT; } }
        

        public  int     CV_JOB_KIND         {set { _CV_JOB_KIND         = value; } get { return _CV_JOB_KIND        ;} }        
        public  int     CV_LUGGNO           {set { _CV_LUGGNO           = value; } get { return _CV_LUGGNO          ;} } 
        public  int     CV_SOUR_SITE        {set { _CV_SOUR_SITE        = value; } get { return _CV_SOUR_SITE       ;} } 
        public  int     CV_DEST_SITE        {set { _CV_DEST_SITE        = value; } get { return _CV_DEST_SITE       ;} }
        public  int     CV_DATA_STA         {set { _CV_DATA_STA         = value; } get { return _CV_DATA_STA        ;} }
        public bool     CV_PA_RD            {set { _CV_PA_RD            = value; } get { return _CV_PA_RD           ;} }
        public bool     CV_PK_RD            {set { _CV_PK_RD            = value; } get { return _CV_PK_RD           ;} }
        public bool     CV_HA_RD            {set { _CV_HA_RD            = value; } get { return _CV_HA_RD           ;} }
        public bool     CV_HK_RD            {set { _CV_HK_RD            = value; } get { return _CV_HK_RD           ;} }
        public bool     CV_LG_EX            {set { _CV_LG_EX            = value; } get { return _CV_LG_EX           ;} }
        public  int     CV_ERR_CODE         {set { _CV_ERR_CODE         = value; } get { return _CV_ERR_CODE        ;} }
        public string CV_BOX_TYP { set { _CV_BOX_TYP          = value; } get { return _CV_BOX_TYP; } }
        public string CV_JEPUM_GRADE { set { _CV_JEPUM_GRADE      = value; } get { return _CV_JEPUM_GRADE; } }
        public bool     CV_BOX_START        {set { _CV_BOX_START        = value; } get { return _CV_BOX_START       ;} }
        public bool     CV_BOX_DEST         {set { _CV_BOX_DEST         = value; } get { return _CV_BOX_DEST        ;} }
        public bool     CV_LGV_START        {set { _CV_LGV_START        = value; } get { return _CV_LGV_START       ;} }
        public bool     CV_LGV_DEST         {set { _CV_LGV_DEST         = value; } get { return _CV_LGV_DEST        ;} }
        public bool     CV_WEIGHT_STATION   {set { _CV_WEIGHT_STATION   = value; } get { return _CV_WEIGHT_STATION  ;} }
        public bool     CV_MERGE_STATION    {set { _CV_MERGE_STATION    = value; } get { return _CV_MERGE_STATION   ;} }
        public bool     CV_REMAIN_STATION   {set { _CV_REMAIN_STATION   = value; } get { return _CV_REMAIN_STATION  ;} }
        public bool     CV_LIFTER_START     {set { _CV_LIFTER_START     = value; } get { return _CV_LIFTER_START    ;} }
        public bool     CV_LIFTER_DEST      {set { _CV_LIFTER_DEST      = value; } get { return _CV_LIFTER_DEST     ;} }
        public  string  CV_BCD_DATA         {set { _CV_BCD_DATA         = value; } get { return _CV_BCD_DATA        ;} }
        public  int     CV_IO_TYP           {set { _CV_IO_TYP           = value; } get { return _CV_IO_TYP          ;} }
        public  int     CV_IO_MODE          {set { _CV_IO_MODE          = value; } get { return _CV_IO_MODE         ;} }

        public int      CV_CONTROL1         {set { _CV_CONTROL1         = value; } get { return _CV_CONTROL1        ;} }
        public int      CV_CONTROL2         {set { _CV_CONTROL2         = value; } get { return _CV_CONTROL2        ;} }
        public int      CV_CONTROL3         {set { _CV_CONTROL3         = value; } get { return _CV_CONTROL3        ;} }
        public int      CV_CONTROL          {set { _CV_CONTROL          = value; } get { return _CV_CONTROL         ;} }
        public int      CV_TRK_PAUSE        {set { _CV_TRK_PAUSE        = value; } get { return _CV_TRK_PAUSE       ;} }  

        public int      BCR_NO              {set { _BCR_NO              = value; } get { return _BCR_NO             ;} }
        public string   BCR_READ_STA        {set { _BCR_READ_STA        = value; } get { return _BCR_READ_STA       ;} }
        public string   BCR_COM_STA         {set { _BCR_COM_STA         = value; } get { return _BCR_COM_STA        ;} }
        public string   BCR_RCV_VAL1        {set { _BCR_RCV_VAL1        = value; } get { return _BCR_RCV_VAL1       ;} }
        public string   BCR_RCV_VAL2        {set { _BCR_RCV_VAL2        = value; } get { return _BCR_RCV_VAL2       ;} }
        public string   BCR_RCV_VAL3        {set { _BCR_RCV_VAL3        = value; } get { return _BCR_RCV_VAL3       ;} }
        public int      BCR_ERR_CODE        {set { _BCR_ERR_CODE        = value; } get { return _BCR_ERR_CODE       ;} }
        public DateTime BCR_ERR_OC_DT       {set { _BCR_ERR_OC_DT       = value; } get { return _BCR_ERR_OC_DT      ;} }
        public DateTime BCR_RCV_DT          {set { _BCR_RCV_DT          = value; } get { return _BCR_RCV_DT         ;} }
        public int      WGT_NO              {set { _WGT_NO              = value; } get { return _WGT_NO             ;} }
        public string   WGT_READ_STA        {set { _WGT_READ_STA        = value; } get { return _WGT_READ_STA       ;} }
        public string   WGT_COM_STA         {set { _WGT_COM_STA         = value; } get { return _WGT_COM_STA        ;} }
        public double      WGT_RCV_VAL      {set { _WGT_RCV_VAL         = value; } get { return _WGT_RCV_VAL        ;} }
        public int      WGT_ERR_CODE        {set { _WGT_ERR_CODE        = value; } get { return _WGT_ERR_CODE       ;} }
        public DateTime WGT_ERR_OC_DT       {set { _WGT_ERR_OC_DT       = value; } get { return _WGT_ERR_OC_DT      ;} }
        public DateTime WGT_RCV_DT          {set { _WGT_RCV_DT          = value; } get { return _WGT_RCV_DT         ;} }
        public string   SITENO              {set { _SITENO              = value; } get { return _SITENO             ;} }

        public string   WH_TYP              {set { _WH_TYP              = value; } get { return _WH_TYP             ;} }
        public string   MC_TYP              {set { _MC_TYP              = value; } get { return _MC_TYP             ;} }

        public int      MOUDLE_ID           {set { _MOUDLE_ID           = value; } get { return _MOUDLE_ID          ;} }
        public int      DEFAULT_TRK_POS     {set { _DEFAULT_TRK_POS     = value; } get { return _DEFAULT_TRK_POS    ;} }
        public int      TRACK_NO            {set { _TRACK_NO            = value; } get { return _TRACK_NO           ;} }

        public int      CV_STATUS           {set { _CV_STATUS           = value; } get { return _CV_STATUS          ;} }
        public int      CV_MOTOR            {set { _CV_MOTOR            = value; } get { return _CV_MOTOR           ;} }
        public int      CV_SENSOR           {set { _CV_SENSOR           = value; } get { return _CV_SENSOR          ;} }
        public bool     BAGGING_WC_YON      {set { _BAGGING_WC_YON      = value; } get { return _BAGGING_WC_YON     ;} }
        public bool     BAGGING_INSPECTION_YON {set { _BAGGING_INSPECTION_YON   = value; } get { return _BAGGING_INSPECTION_YON; } }
        public string   LINE_ID             {set { _LINE_ID             = value; } get { return _LINE_ID            ;} }
        public bool     BAGGING_LABEL_YON   {set { _BAGGING_LABEL_YON   = value; } get { return _BAGGING_LABEL_YON  ;} }
        public bool     BAGGING_START_YON   {set { _BAGGING_START_YON   = value; } get { return _BAGGING_START_YON  ;} }

        public bool     BREAKING_SCAN_YON   {set { _BREAKING_SCAN_YON   = value; } get { return _BREAKING_SCAN_YON  ;} }
        public bool     BREAKING_CLEAN_YON  {set { _BREAKING_CLEAN_YON  = value; } get { return _BREAKING_CLEAN_YON ;} }
        public bool     BAGGING_START_WC_YON    { set { _BAGGING_START_WC_YON = value; } get { return _BAGGING_START_WC_YON; } }
        public bool     BAGGING_START_BCR_YON { set { _BAGGING_START_BCR_YON = value; } get { return _BAGGING_START_BCR_YON; } }
        public bool     BAGGING_EMG_LINE_YON { set { _BAGGING_EMG_LINE_YON = value; } get { return _BAGGING_EMG_LINE_YON; } }
        public bool BAGGING_OP_GRADE_MIS_LINE_YON { set { _BAGGING_OP_GRADE_MIS_LINE_YON = value; } get { return _BAGGING_OP_GRADE_MIS_LINE_YON; } }
        

        public bool     PA_HS_YON           {set { _PA_HS_YON           = value; } get { return _PA_HS_YON          ;} }
        public bool     PK_HS_YON           {set { _PK_HS_YON           = value; } get { return _PK_HS_YON          ;} }
        public bool     PA_MC_YON           {set { _PA_MC_YON           = value; } get { return _PA_MC_YON          ;} }
        public bool     PK_MC_YON           {set { _PK_MC_YON           = value; } get { return _PK_MC_YON          ;} }
        public bool     PS_MC_YON           {set { _PS_MC_YON           = value; } get { return _PS_MC_YON          ;} }
        public bool     PP_MC_YON           {set { _PP_MC_YON           = value; } get { return _PP_MC_YON          ;} }
        public bool     BOX_AUTO_RET_YON    { set { _BOX_AUTO_RET_YON   = value; } get { return _BOX_AUTO_RET_YON   ;} }

        #endregion CV Read define
        /*
         * 설비통신상태-PLC_STA_MST(variable)
         */
        #region Plc Status define
        private string      _PLC_COM_STA;
        private DateTime    _PLC_COM_DT;
        private int         _ERR_CODE;
        public string    PLC_COM_STA    { set { _PLC_COM_STA            = value; } get { return _PLC_COM_STA; } }
        public DateTime PLC_COM_DT      { set { _PLC_COM_DT             = value; } get { return _PLC_COM_DT; } }
        public int      ERR_CODE        { set { _ERR_CODE               = value; } get { return _ERR_CODE; } }
        #endregion


        /*
         * Request Order ID
         */
        #region Request Order ID
        public const string QC_ID_STORE_UROD			="11";
        public const string QC_ID_STORE_CCE_NOREMOVE	="12";
        public const string QC_ID_STORE_CCE_REMOVE		="13";
        public const string QC_ID_STORE_BOX				="14";
        public const string QC_ID_STORE_CCE_VACANTBOX	="15";
        public const string QC_ID_STORE_P2TOP3			="16";
        public const string QC_ID_RETRIEVE_BATCH		="21";
        public const string QC_ID_RETRIEVE_BOX			="22";
        public const string QC_ID_RETRIEVE_P2TOP1		="26";
        public const string QC_ID_RETRIEVE_FILAMENT		="27";
        public const string QC_ID_RETRIEVE_VACANT		="29";
        public const string QC_ID_STOCK_RTR				="31";
        public const string QC_ID_STORE_FILAMENT		="41";
        public const string QC_ID_MOVE_BREAKING			="46";
        public const string QC_ID_MOVE_BAGGING          = "47";
        public const string QC_ID_MOVE_CHIP             = "48";
        #endregion

        /*
         * JobType define
         */
        #region JobType define
        public const int JOB_TYPE_BAGGING			= 1;
        public const int JOB_TYPE_MERGE				= 2;
        public const int JOB_TYPE_WEIGHT_ERROR		= 3;
        public const int JOB_TYPE_GRADE_ERROR       = 4;
        #endregion

        /*
         * Cv Order Command
         */
        #region Cv Order Command
        public const string CV_CMD_IDLE             = "00";
        public const string CV_CMD_WRT_DATA_BOX     = "01"; //데이터+박스
        public const string CV_CMD_DEL_DATA_BOX     = "02"; //데이터+박스
        public const string CV_CMD_WRITE_CONTROL    = "03"; //Control
        public const string CV_CMD_WRITE_DATA       = "04"; //박스제외
        #endregion

        /*
         * Control Ascii
         */
        #region Control Ascii   
        #endregion


    }
}
