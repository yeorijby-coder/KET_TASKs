//최초작성자	: 최원빈
//작성일		: 20190722
//화면개요	    : S/C 작업처리
//수정이력	    : 2019.10.28 이중입고 발생 시 JOB_STATUS = '44' 인 DATA를 찾아서 새로운 로케이션을 구해서 
//                다시 지시(CEID 201) 후 완료(CEID 203) 처리 / 재지시 및 완료 시 MES에서 새로운 로케이션을 받아줄수있는지 확인해야함.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.OleDb;
using Samoh_Lib;
using log4net;
using log4net.Config;
using NpgsqlTypes;

namespace TSK_COMM_IOSCH
{
    public class cThread_SC : IOSchDB
    {
        #region 변수 선언
        private int m_nId = 0;
        public PsMsgView callPsMsgView = null;
        public Thread m_Thread;
        public SYS_MAIN m_Main;
        private bool m_Open;
        public bool IsOpen { get { return m_Open; } set { m_Open = value; } }
        public string m_strLogName = "";

        //Dictionary<string, IF_EQM_R_WCS_STAT_Data> IfDic = new Dictionary<string, IF_EQM_R_WCS_STAT_Data>();
        public Dictionary<int, int> dicPrevJob = new Dictionary<int, int>();

        private string _strErrorMsg = "";
        public string m_strRtnMsg = "";
        public int m_nRetCd = 0;
        string strRTN_MSG = "";
        #endregion 변수 선언

        #region IOSCHThread
        public cThread_SC(int Id)
        {
            m_nId = Id;
        }
        #endregion IOSCHThread

        #region 화면 표시용.
        private void MakeMsg(string msg)
        {
            try
            {
                callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_NOR);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void MakeMsg_Error_NoLog(string msg)
        {
            try
            {
                callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_ERR);
            }
            catch (Exception ex)
            {
                return;
            }

        }

        private void MakeMsg_Error(string msg)
        {
            try
            {
                callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_ERR);
                cDefApp.m_LogQ[m_nId].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }

        }
        private void MakeMsg_Imp(string msg)
        {
            try
            {
                callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_IMP);
                cDefApp.m_LogQ[m_nId].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }

        }

        #endregion

        public void Thread_Doing(object value)
        {
            while (!IsDBOpen)
            {
                try
                {
                    if (DBOpen()) { MakeMsg("[cThread_SC] DB Open"); break; }
                    MakeMsg_Error("[cThread_SC] DB Open 실패 - 5초 후 재시도");
                }
                catch (Exception ex) { MakeMsg_Error("[cThread_SC] DB Open 오류: " + ex.Message); }
                Thread.Sleep(5000);
            }

            try
            {
                while (true)
                {

                    strRTN_MSG = "";
                    int nPrevJob = 0;

                    Thread.Sleep(100);

                    if (_pBdb == null) { try { IsDBOpen = false; DBOpen(); } catch { } continue; }

                    _pBdb.mDtMain.Clear();
                    _pBdb.mDtMain.Reset();
                    //_pBdb.mDtMain.Dispose();

                    switch (m_nId)
                    {
                        case (int)cDefApp.eThGbn.SC_GR22:
                            // 이전 작업을 판단하여 크레인 입-출-랙투랙  로테이션
                            dicPrevJob.TryGetValue((int)cDefApp.eAGINGTYP.AGING_1G, out nPrevJob);
                            switch (nPrevJob)
                            {
                                case (int)cDefApp.eJOBTYP.Sto:
                                    {
                                        IsOnlineSC(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString()
                                                , ref m_strRtnMsg);

                                        PriorityRet(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                  , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString());

                                    }
                                    break;
                                case (int)cDefApp.eJOBTYP.Ret:
                                    {
                                        PriorityRackToRack(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                         , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString());

                                    }
                                    break;
                                case (int)cDefApp.eJOBTYP.RtoR:
                                    {
                                        PrioritySto(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                  , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString());

                                    }
                                    break;
                                case (int)cDefApp.eJOBTYP.Aisle2Aisle:
                                    {
                                        PriorityAisleToAisle(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                           , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString());

                                    }
                                    break;

                            }

                            // 입고완료 (S/C완료처리)
                            if (!CHECK_SC_COMPLETE(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                 , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString()
                                                 , "0"
                                                 , ref strRTN_MSG))
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            else
                            {
                                if (strRTN_MSG != "") 
                                { 
                                    MakeMsg(strRTN_MSG); 
                                }
                            }
                            Thread.Sleep(50);

                            // 작업지시된 신규작업을 SC 구동요구로 변경
                            if (!NEW_JOB_INVOKE_FOR_SC(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), ref strRTN_MSG))
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            else
                            {
                                if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                            }
                            Thread.Sleep(10);

                            break;
                        default:
                            break;
                    }
                    _pBdb.mDtMain.Clear();
                    //_pBdb.mDtMain.Dispose();
                    long mem = GC.GetTotalMemory(false);
                    Console.WriteLine("SC-Current Memory : {0}", mem);

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                IsRtnMsg = ex.Message;
                MakeMsg_Error("[cThread_SC] IOSCH Thread가 종료됩니다.(" + IsRtnMsg + ")");
                SetErrorMsg("[cThread_SC] IOSCH Thread가 종료됩니다.(" + IsRtnMsg + ")");
                DBClose();

                //스레드 다시 시작하기위함.
                m_Thread = null;
            }
        }

        public void SetErrorMsg(string strMsg)
        {
            _strErrorMsg = strMsg;
            Log.Error(_strErrorMsg);
        }

        // SC COMPLETE(RetHS ready)
        // 입고완료 (S/C완료처리)
        public bool CHECK_SC_COMPLETE(string strWH_TYP,
                                      string strAGING_TYP,
                                      string strPLC_NO,
                                  ref string pRTN_MSG)
        {
            try
            {
                //JobItem Job = null;
                string strSql;
                int nSelCnt = 0;

                pRTN_MSG = "CHECK_SC_COMPLETE::";

                // 입고H/S에서 입고지시한 DATA검색
                strSql = "";
                strSql += cDefApp.CRLF + "SELECT SD.*, JM.*                        ";
                strSql += cDefApp.CRLF + "  FROM JOB_MST JM                        ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD                   ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP = SD.WH_TYP             ";
                strSql += cDefApp.CRLF + "   AND (JM.DEST_POS       =  SD.SC_NO    ";
                strSql += cDefApp.CRLF + "    OR  JM.SC_NO          =  SD.SC_NO    ";
                strSql += cDefApp.CRLF + "    OR  JM.START_POS      =  SD.SC_NO)   ";
                strSql += cDefApp.CRLF + " WHERE SD.WH_TYP          =  :pWH_TYP    ";
                strSql += cDefApp.CRLF + "   AND SD.SC_NO like '%' " + DbLang.II + " :pSC_NO " + DbLang.II + " '%'       ";
                strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  =  '1'         ";
                strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    =  '1'         ";
                strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD     =  '0000'      ";
                //strSql += cDefApp.CRLF + "   AND (SD.ITN_LUGG_FK1   <> '0'         ";  // S/C완료 ITN_LUGG_FK1, 2에 작업번호 올라오면 완료된거임.
                //strSql += cDefApp.CRLF + "    OR  SD.ITN_LUGG_FK2    <>  '0')       ";
                //strSql += cDefApp.CRLF + "   AND (SD.ITN_LUGG_FK1   =  JM.LUGG_NO  ";
                //strSql += cDefApp.CRLF + "    OR  SD.ITN_LUGG_FK2    =  JM.LUGG_NO) ";
                //strSql += cDefApp.CRLF + "   AND (SD.LUGG_NO_FK1_RD <> '0'         ";
                //strSql += cDefApp.CRLF + "    OR  SD.LUGG_NO_FK2_RD <> '0')        ";
                //strSql += cDefApp.CRLF + "   AND (SD.LUGG_NO_FK1_RD =  JM.LUGG_NO  ";
                //strSql += cDefApp.CRLF + "    OR  SD.LUGG_NO_FK2_RD =  JM.LUGG_NO) ";

                strSql += cDefApp.CRLF + "   AND (     (SD.CMD_RQ_ID       =  'FCMP'      AND  SD.CMD_RQ_YN       =  'Y')       ";       
                strSql += cDefApp.CRLF + "     OR (    (SD.ITN_LUGG_FK1    <> '0'         OR   SD.ITN_LUGG_FK2    <> '0')       ";
                strSql += cDefApp.CRLF + "         AND (SD.LUGG_NO_FK1_RD  <> '0'         OR   SD.LUGG_NO_FK2_RD  <> '0')       ";
                strSql += cDefApp.CRLF + "         AND (SD.ITN_LUGG_FK1    =  JM.LUGG_NO  OR   SD.ITN_LUGG_FK2    =  JM.LUGG_NO)";
                strSql += cDefApp.CRLF + "         AND (SD.LUGG_NO_FK1_RD  =  JM.LUGG_NO  OR   SD.LUGG_NO_FK2_RD  =  JM.LUGG_NO)";
                strSql += cDefApp.CRLF + "         AND  SD.ACTIVE_MODE_RD  =  '1'                                               ";
                strSql += cDefApp.CRLF + "         AND  SD.UCSTATUS_RD     =  '1'                                               ";           // 강제완료를 위해서 ????
                strSql += cDefApp.CRLF + "         AND  SD.COMPLETE_RD     <> '0'                                                  ))";

                strSql += cDefApp.CRLF + "   AND SD.FORKPOS_FK1_RD  =  '0'         ";
                strSql += cDefApp.CRLF + "   AND SD.SENSOR_FK_RD    =  '0'         ";
                strSql += cDefApp.CRLF + "   AND SD.OD_RQ_YN        =  'N'         ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_STATUS      =  '21'        ";  // S/C구동중
                strSql += cDefApp.CRLF + "   AND  " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - SD.WRITE_UPD_DT, 'YYYYMMDDHH24MISS')") + " > " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - SD.READ_UPD_DT , 'YYYYMMDDHH24MISS')");
                strSql += cDefApp.CRLF + "   AND  " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - SD.WRITE_UPD_DT, 'YYYYMMDDHH24MISS')") + " > 5";
                strSql += cDefApp.CRLF + " ORDER BY SD.SC_NO                         ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("pSC_NO", DbLang.VARCHAR).Value = strAGING_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false ;
                }

                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true ;
                }

                string strJOB_TYP = "0";
                string strBOTTOM_TRAY1 = "0";
                string strTOP_TRAY1 = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";
                string strBOTTOM_TRAY2 = "0";
                string strTOP_TRAY2 = "0";
                string strDEST_POS2 = "0";
                //string strLUGG_NO2 = "0";
                // Fetch Location정보
                string strDEST_LOCATION1 = "0";
                string strDEST_LOCATION2 = "0";
                string strSTART_LOCATION1 = "0";
                string strSTART_LOCATION2 = "0";
                string strUSE_FK = "0";

                string strSC_TYP = "0";
                string strCELL_SC_NO = "0";
                string strDURATION_TIME1 = "000000000";
                string strDURATION_TIME2 = "000000000";
                string strMC_NO = "000";
                string strCMP_STEP = "0";   // 호기간 이동시에 사용    -----    1: 출고 완료, 2: 입고완료        -- JOB_MST에 CMP_STEP라는 필드 생성 요망 

                string strJobType = "";
                string strJOB_STATUS = "";

                string strFCMP = "0";

                // DateTime 
                strSC_TYP = _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString();
                strMC_NO = "" + _pBdb.mDtMain.Rows[0]["HS_TRACK_NO"].ToString() == "" ? "000" : _pBdb.mDtMain.Rows[0]["HS_TRACK_NO"].ToString();
                _pBdb.BeginTrans();
                for (int i = 0; i < nSelCnt; i++)
                {


                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strLUGG_NO1 = _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    strDEST_POS1 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                    strDEST_LOCATION1 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strSTART_LOCATION1 = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[i]["SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SC_NO"].ToString();
                    strDURATION_TIME1 = "" + _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString() == "" ? "000000000" : _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString();
                    strCMP_STEP = "" + _pBdb.mDtMain.Rows[i]["CMP_STEP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["CMP_STEP"].ToString();
                    strFCMP = "" + _pBdb.mDtMain.Rows[i]["CMD_RQ_YN"].ToString() == "" ? "N" : _pBdb.mDtMain.Rows[i]["CMD_RQ_YN"].ToString();

                     
                    strUSE_FK = "0";
                    if (i > 0 && strSC_TYP == "TWIN")
                    {
                        if (strCELL_SC_NO != _pBdb.mDtMain.Rows[i]["SC_NO"].ToString()) { break; }

                        strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                        strBOTTOM_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                        strTOP_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                        strDEST_POS2 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                        //strLUGG_NO2 = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                        strDEST_LOCATION2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                        strSTART_LOCATION2 = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString();
                        strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                        strDURATION_TIME2 = "" + _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString() == "" ? "000000000" : _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString();
                        strUSE_FK = "1";

                        break;
                    }

                }

                if ((Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto) || 
                    (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtoR) ||
                    (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RackSto) ||
                    (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle && strCMP_STEP == "2"))
                {
                    // H/S 정보 가져오기 


                    // S/C 완료처리.
                    if ((UPDATE_JOB_DATA("29", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false))
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // S/C 상태 초기화작업.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                  ";
                    strSql += CRLF + "    SET ITN_LUGG_FK1 =  '0'      ";
                    strSql += CRLF + "      , OD_USER_ID   =  'IOTASK' ";
                    if (strFCMP == "Y")
                    {
                        strSql += CRLF + "    , CMD_RQ_ID    =  'DELFK1' ";
                        strSql += CRLF + "    , CMD_RQ_YN    =  'Y'      ";
                    }
                    strSql += CRLF + "  WHERE WH_TYP       =  :WH_TYP  ";
                    strSql += CRLF + "    AND SC_NO        =  :SC_NO   ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strCELL_SC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "변경할 SC NO 정보가 존재하지 않습니다. SC_NO : " + strCELL_SC_NO;
                        _pBdb.Rollback();
                        return false;
                    }

                    if (strUSE_FK == "1")   // 2번 FORK
                    {
                        if ((UPDATE_JOB_DATA("29", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false))
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        /*
                        if (Convert.ToInt16(strCELL_SC_NO) < 29700)
                        {
                            // CELL상태 출고가능으로 변경 및 CELL DTL에 재고생성.
                            if (UPDATE_CELL_MST_DTL("2", strBOTTOM_TRAY2, strTOP_TRAY2, strWH_TYP, strCELL_SC_NO, strDEST_LOCATION2, ref pRTN_MSG, dtEND_TIME2) == false)
                            {
                                _pBdb.Rollback();
                                return false;
                            }
                        }
                         * //*/

                        // S/C 상태 초기화작업.
                        strSql = "";
                        strSql += CRLF + " UPDATE SC_DATA                  ";
                        strSql += CRLF + "    SET ITN_LUGG_FK2 =  '0'      ";
                        strSql += CRLF + "       , OD_USER_ID  =  'IOTASK' ";
                        strSql += CRLF + "       , OD_UPD_DT   =  " + DbLang.SYSDATE + "    ";
                        strSql += CRLF + "  WHERE WH_TYP       =  :WH_TYP  ";
                        strSql += CRLF + "    AND SC_NO        =  :SC_NO   ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strCELL_SC_NO;
                        nSelCnt = _pBdb.ExcuteNonQry(strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            _pBdb.Rollback();
                            return false;
                        }

                        if (nSelCnt == 0)
                        {
                            pRTN_MSG += "변경할 SC NO 정보가 존재하지 않습니다. SC_NO : " + strCELL_SC_NO;
                            _pBdb.Rollback();
                            return false;
                        }

                        if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                            strJobType = "호기간 이동 작업 중 입고 완료";
                        else if(Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtoR)
                            strJobType = "랙투랙 완료 ";
                        else //if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto)
                            strJobType = "입고 완료 ";

                        pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strJobType + "하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";


                        _pBdb.Commit();

                        InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, "21", strCELL_SC_NO, strDEST_LOCATION2);
                        return true;
                    }
                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                        strJobType = "호기간 이동 작업 중 입고 완료";
                    else if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtoR)
                        strJobType = "랙투랙 완료 ";
                    else //if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto)
                        strJobType = "입고 완료 ";

                    pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strJobType + "하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";

                    _pBdb.Commit();
                    InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO1, "21", strCELL_SC_NO, strDEST_LOCATION1);
                    return true;
                }
                else if((Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Ret) ||
                        (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RackRet) ||
                        (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.OtherRet) ||
                        (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle && strCMP_STEP == "1"))
                {
                    strJOB_STATUS = "22";
                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                    {
                        strJOB_STATUS = "28";
                    }


                    if (UPDATE_JOB_DATA(strJOB_STATUS, strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG, "0", "0", strMC_NO) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // S/C 상태 초기화작업.
                    if (strFCMP == "Y")
                    {
                        strSql = "";
                        strSql += CRLF + " UPDATE SC_DATA                  ";
                        strSql += CRLF + "    SET CMD_RQ_ID    =  'DELFK1' ";
                        strSql += CRLF + "      , CMD_RQ_YN    =  'Y'      ";
                        strSql += CRLF + "  WHERE WH_TYP       =  :WH_TYP  ";
                        strSql += CRLF + "    AND SC_NO        =  :SC_NO   ";

                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strCELL_SC_NO;
                        nSelCnt = _pBdb.ExcuteNonQry(strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            _pBdb.Rollback();
                            return false;
                        }

                        if (nSelCnt == 0)
                        {
                            pRTN_MSG += "변경할 SC NO 정보가 존재하지 않습니다. SC_NO : " + strCELL_SC_NO;
                            _pBdb.Rollback();
                            return false;
                        }
                    }

                    if (strUSE_FK == "1")
                    {
                        if (UPDATE_JOB_DATA(strJOB_STATUS, strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                        if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                            strJobType = "호기간 이동 작업 중 출고 완료";
                        //else if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtoR)
                        //    strJobType = "랙투랙 완료 ";
                        else //if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto)
                            strJobType = "출고 완료 ";

                        pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strJobType + "하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";

                        _pBdb.Commit();

                        InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, strJOB_STATUS, strCELL_SC_NO, strMC_NO);
                        return true;
                    }
                }
                if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                    strJobType = "호기간 이동 작업 중 출고 완료";
                //else if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtoR)
                //    strJobType = "랙투랙 완료 ";
                else //if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto)
                    strJobType = "출고 완료 ";

                pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strJobType + "하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";

                _pBdb.Commit();

                InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, strJOB_STATUS, strCELL_SC_NO, strMC_NO);

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }



        // RACK TO RACK
        public bool CHECK_RACK_TO_RACK(string strWH_TYP,
                                       string strAGING_TYP,
                                       string strPLC_NO,
                                   ref string pRTN_MSG)
        {
            string strSql = "";
            int nSelCnt = 0;

            try
            {
                pRTN_MSG = "CHECK_RACK_TO_RACK::";

                strSql = "";                                                    
                strSql += cDefApp.CRLF + "SELECT JM.*, SCM.*, SD.SC_TYP                             ";
                strSql += cDefApp.CRLF + "  FROM JOB_MST JM                                         ";
                strSql += cDefApp.CRLF + " INNER JOIN CELL_MST SCM                                  ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP          = SCM.WH_TYP                    ";
                strSql += cDefApp.CRLF + "   AND JM.START_POS       = SCM.CELL_SC_NO                ";
                strSql += cDefApp.CRLF + "   AND JM.START_LOCATION  = SCM.CELL_NO                   ";
                strSql += cDefApp.CRLF + " INNER JOIN CELL_MST DCM                                  ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP      = DCM.WH_TYP                        ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS   = DCM.CELL_SC_NO                     ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_LOCATION   = DCM.CELL_NO                   ";
                strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD                                   ";
                strSql += cDefApp.CRLF + "    ON DCM.WH_TYP      = SD.WH_TYP                        ";
                strSql += cDefApp.CRLF + "   AND DCM.CELL_SC_NO  = SD.SC_NO                         ";
                strSql += cDefApp.CRLF + " WHERE SCM.WH_TYP      = :pWH_TYP                         ";
                strSql += cDefApp.CRLF + "   AND SCM.AGING_TYP like '%' " + DbLang.II + " :pAGING_TYP " + DbLang.II + " '%' ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_STATUS  = '20'                              ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_TYP     = '4'                               ";  // RACK TO RACK
                strSql += cDefApp.CRLF + "   AND JM.START_POS   = JM.DEST_POS                       ";  // 같은 SC 번호
                strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'                           ";
                strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'                           ";
                strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD     = '0000'                        ";
                strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD  = '1'                           ";
                strSql += cDefApp.CRLF + "   AND SD.UCSTATUS_RD     = '1'                           ";
                strSql += cDefApp.CRLF + "   AND (SD.ITN_LUGG_FK1   = '0' AND SD.ITN_LUGG_FK2 = '0')";
                strSql += cDefApp.CRLF + "   AND SD.OD_RQ_YN        = 'N'                           ";
                strSql += cDefApp.CRLF + "   AND SCM.CELL_USE_YN    = 'Y'                           ";
                strSql += cDefApp.CRLF + "   AND SD.SUSPEND        IN ('0', '1') ";
                strSql += cDefApp.CRLF + "   AND DCM.CELL_USE_YN    = 'Y'                           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("pAGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false ;
                }

                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "0";
                string strBOTTOM_TRAY1 = "0";
                string strTOP_TRAY1 = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";

                string strBOTTOM_TRAY2 = "0";
                string strTOP_TRAY2 = "0";
                string strDEST_POS2 = "0";
                string strLUGG_NO2 = "0";

                string strHS_MC_NO1 = "0";
                string strHS_MC_NO2 = "0";

                string strSOUR_BANK_FK1 = "0";
                string strSOUR_BAY_FK1 = "0";
                string strSOUR_LEV_FK1 = "0";
                string strDEST_HSPOS_FK1 = "0";
                string strCELL_NO1 = "0";
                string strCELL_NO1_PAIR = "0";

                string strSOUR_BANK_FK2 = "0";
                string strSOUR_BAY_FK2 = "0";
                string strSOUR_LEV_FK2 = "0";
                string strDEST_HSPOS_FK2 = "0";
                string strCELL_NO2 = "0";
                string strUSE_FK = "0";
                string strUSER_COMMAND = "0";
                string strWRITE_FLAG = "0";
                string strSC_TYP = "0";
                string strCELL_SC_NO = "0";

                string strDEST_LOCATION1 = "0";
                string strDEST_LOCATION2 = "0";

                string strDEST_BANK_FK1 = "0";
                string strDEST_BAY_FK1 = "0";
                string strDEST_LEV_FK1 = "0";
                string strSOUR_HSPOS_FK1 = "0";
                string strDEST_BANK_FK2 = "0";
                string strDEST_BAY_FK2 = "0";
                string strDEST_LEV_FK2 = "0";
                string strSOUR_HSPOS_FK2 = "0";

                string strCELL_INOUT = "0";
                string strSC_PLT_JOB_TYP = "0";
                string strCELL_BANK = "";
                DateTime dtAGING_END_DT = new DateTime();
                DataTable dtCELL_MST = new DataTable();

                bool bSuccess = false;

                _pBdb.BeginTrans();

                strSC_TYP = _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString();

                if (strSC_TYP == "TWIN")
                {
                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                        strCELL_NO1 = "" + _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString();
                        strCELL_NO1_PAIR = "" + _pBdb.mDtMain.Rows[0]["PAIR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PAIR"].ToString();
                        strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BANK"].ToString();
                        strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BAY"].ToString();
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LEV"].ToString();
                        strDEST_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(0, 2);
                        strDEST_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(3, 3);
                        strDEST_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(7, 2);
                        strDEST_LOCATION1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();
                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString();
                        strHS_MC_NO1 = "" + _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString();
                        strUSE_FK = "0";
                        strCELL_INOUT = "" + _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString();

                        dtAGING_END_DT = Convert.ToDateTime(_pBdb.mDtMain.Rows[0]["AGING_END_DT"].ToString());

                        bSuccess = true;
                        if (i > 0)
                        {
                            if (strCELL_SC_NO != _pBdb.mDtMain.Rows[i]["CELL_SC_NO"].ToString() ||
                                strWH_TYP != _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString())
                                continue;

                            strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                            strBOTTOM_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BOTTOM_TRAY"].ToString();
                            strTOP_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TOP_TRAY"].ToString();
                            strDEST_POS2 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                            strLUGG_NO2 = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                            strCELL_NO2 = "" + _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString();
                            strSOUR_BANK_FK2 = "" + _pBdb.mDtMain.Rows[i]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BANK"].ToString();
                            strSOUR_BAY_FK2 = "" + _pBdb.mDtMain.Rows[i]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BAY"].ToString();
                            strSOUR_LEV_FK2 = "" + _pBdb.mDtMain.Rows[i]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LEV"].ToString();

                            strDEST_BANK_FK2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString().Substring(0, 2);
                            strDEST_BAY_FK2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString().Substring(3, 3);
                            strDEST_LEV_FK2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString().Substring(7, 2);
                            strDEST_LOCATION2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();

                            strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                            strUSE_FK = "1";

                            bSuccess = true;

                            break;
                        }
                    }
                }
                else
                {

                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                        strCELL_NO1 = "" + _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString();
                        strCELL_NO1_PAIR = "" + _pBdb.mDtMain.Rows[0]["PAIR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PAIR"].ToString();
                        strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BANK"].ToString();
                        strCELL_BANK = strSOUR_BANK_FK1;
                        strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BAY"].ToString();
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LEV"].ToString();

                        strDEST_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(0, 2);
                        strDEST_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(3, 3);
                        strDEST_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(7, 2);

                        //Convert.ToString("" + ((Convert.ToInt32(0 + strDEST_LOCATION.Substring(0, 2)) + 1) % 2) +1) ;
                        strSOUR_BANK_FK1 = Convert.ToString("" + (((Convert.ToInt32(0 + strSOUR_BANK_FK1) + 1) % 2) + 1));
                        strDEST_BANK_FK1 = Convert.ToString("" + (((Convert.ToInt32(0 + strDEST_BANK_FK1) + 1) % 2) + 1));

                        strDEST_LOCATION1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();
                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();

                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString();
                        strUSE_FK = "0";

                        strCELL_INOUT = "" + _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString();
                        bSuccess = true;

                        #region OUT 위치일 시 IN 위치 화물이 잇고 에이징 END Time 검색 후 3분 이내면(사용x) - 주석 처리 
                        /*
                        if (strCELL_INOUT == "OUT")
                        {
                            int nCOUNT = 0;
                            string strPAIR_CELL_STA = "";
                            string strPAIR_BOTTOM_TRAY = "";
                            string strPAIR_TOP_TRAY = "";
                            string strPAIR_DEST_LOCATION = "";
                            //OUT일때 IN 작업이 있을때 RACK TO RACK을 할지 아니면 출고를 하던지
                            CHECK_LOCATION_BY_CELLNO(strCELL_SC_NO
                                                   , strCELL_NO1_PAIR
                                                   , strWH_TYP
                                                   , ref strPAIR_CELL_STA
                                                   , ref strPAIR_BOTTOM_TRAY
                                                   , ref strPAIR_TOP_TRAY
                                                   , ref dtAGING_END_DT
                                                   , ref nCOUNT);

                            // IN에 화물존재.
                            if (nCOUNT != 0)
                            {
                                if (strPAIR_CELL_STA == "1" || strPAIR_CELL_STA == "3")
                                {
                                    bSuccess = false;
                                    continue;
                                }
                                else if (strPAIR_CELL_STA == "2")
                                {

                                    // 미존재해야한다.
                                    CHECK_JOB_BY_CELLNO(strCELL_SC_NO, strCELL_NO1_PAIR, strWH_TYP, ref nCOUNT);
                                    if (nCOUNT == 0)
                                    {
                                        if (FETCH_LOCATION_BY_AGING_TYP(strWH_TYP
                                                                      , "000000000"
                                                                      , strCELL_SC_NO
                                                                      , dtAGING_END_DT
                                                                      , ref strPAIR_DEST_LOCATION
                                                                      , ref pRTN_MSG) == false)
                                        {
                                            _pBdb.Rollback();
                                            return false;
                                        }
                                        int iResult;

                                        // RACK TO RACK 작업생성.
                                        iResult = InsertJobMst(strWH_TYP
                                                              , 4
                                                              , strCELL_SC_NO
                                                              , strCELL_NO1_PAIR
                                                              , strCELL_SC_NO
                                                              , strPAIR_DEST_LOCATION
                                                              , strCELL_SC_NO
                                                              , strPAIR_TOP_TRAY
                                                              , strPAIR_BOTTOM_TRAY
                                                              , dtAGING_END_DT
                                                              , "0"
                                                              , ref pRTN_MSG);
                                        if (iResult > 0)
                                        {
                                            bSuccess = false;
                                            continue;
                                        }
                                        else
                                        {
                                            _pBdb.Rollback();
                                            return false;
                                        }
                                    }

                                    // 존재 시 작업을 하지않아야한다..
                                    bSuccess = false;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            bSuccess = true;
                            break;
                        }
                        //*/
                        #endregion
                    }
                }

                //출발 로케이션에 따라서 SC PLT 작업 정보 (Roll 작업 : 8열 1단 ~ 8열 7단)
                strSql = "";
                strSql += CRLF + " SELECT CM.*                      ";
                strSql += CRLF + "   FROM CELL_MST CM               ";
                strSql += CRLF + "  WHERE CM.WH_TYP   = :WH_TYP     ";
                strSql += CRLF + "    AND CM.BANK = :BANK           ";
                strSql += CRLF + "    AND CM.BAY = :BAY             ";
                strSql += CRLF + "    AND CM.LEV = :LEV             ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strCELL_BANK;
                _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                nSelCnt = _pBdb.ExcuteQry(dtCELL_MST, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    _pBdb.Rollback();
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG += "조회할 CELL_MST 정보가 존재하지 않습니다. CELL_MST : " + strCELL_BANK + "-" + strSOUR_BAY_FK1 + "-" + strSOUR_LEV_FK1;
                    _pBdb.Rollback();
                    return false;
                }

                strSC_PLT_JOB_TYP = dtCELL_MST.Rows[0]["SC_PLT_JOB_TYP"].ToString();

                if (bSuccess == true)
                {
                    // S/C 작업지시.
                    if (UPDATE_SC_DATA(strJOB_TYP,
                                       strLUGG_NO1,
                                       strSOUR_BANK_FK1,
                                       strSOUR_BAY_FK1,
                                       strSOUR_LEV_FK1,
                                       strDEST_HSPOS_FK1,
                                       strLUGG_NO2,
                                       strSOUR_BANK_FK2,
                                       strSOUR_BAY_FK2,
                                       strSOUR_LEV_FK2,
                                       strDEST_HSPOS_FK2,
                                       strUSE_FK,
                                       strUSER_COMMAND,
                                       strWRITE_FLAG,
                                       strWH_TYP,
                                       strCELL_SC_NO,
                                       strDEST_BANK_FK1,
                                       strDEST_BAY_FK1,
                                       strDEST_LEV_FK1,
                                       strSOUR_HSPOS_FK1,
                                       strDEST_BANK_FK2,
                                       strDEST_BAY_FK2,
                                       strDEST_LEV_FK2,
                                       strSOUR_HSPOS_FK2,strSC_PLT_JOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }


                    if (UPDATE_JOB_DATA("21", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS1, strCELL_NO1, strHS_MC_NO1, strCELL_SC_NO, "0", strLUGG_NO2) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                    /*
                    if (UPDATE_CELL_MST_DTL("3", strBOTTOM_TRAY1, strTOP_TRAY1, strWH_TYP, strCELL_SC_NO, strCELL_NO1, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // 도착지 LOCATION 입고예약처리 2019.10.13
                    if (UPDATE_CELL_MST_DTL("1", strBOTTOM_TRAY1, strTOP_TRAY1, strWH_TYP, strCELL_SC_NO, strDEST_LOCATION1, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                    //*/
                    if (strUSE_FK == "1")
                    {
                        if (UPDATE_JOB_DATA("21", strLUGG_NO2, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS2, strCELL_NO2, strHS_MC_NO2, strCELL_SC_NO, "0", strLUGG_NO1) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                        /*
                        if (UPDATE_CELL_MST_DTL("3", strBOTTOM_TRAY2, strTOP_TRAY2, strWH_TYP, strCELL_SC_NO, strCELL_NO2, ref pRTN_MSG) == false )
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        // 도착지 LOCATION 입고예약처리 2019.10.13
                        if (UPDATE_CELL_MST_DTL("1", strBOTTOM_TRAY2, strTOP_TRAY2, strWH_TYP, strCELL_SC_NO, strDEST_LOCATION2, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                        //*/
                        pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strCELL_NO2 + "위치의 화물을 " + strDEST_LOCATION2 + " 랙투랙하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";
                        _pBdb.Commit();
                        InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, "21", strCELL_NO2, strDEST_LOCATION2);
                        return true;
                    }
                }
                pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strCELL_NO1 + "위치의 화물을 " + strDEST_LOCATION1 + "위치로 랙투랙하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";

                _pBdb.Commit();
                InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, "21", strCELL_NO1, strDEST_LOCATION1);
                return true;

            }
            catch (Exception ex)
            {
                 pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }


        // RACK TO RACK
        public bool CHECK_AISLE_TO_AISLE(string strWH_TYP,
                                         string strAGING_TYP,
                                         string strPLC_NO,
                                     ref string pRTN_MSG)
        {
            string strSql = "";
            int nSelCnt = 0;

            try
            {
                pRTN_MSG = "CHECK_AISLE_TO_AISLE::";

                strSql = "";
                strSql += cDefApp.CRLF + "SELECT JM.*, SCM.*, SD.SC_TYP, SHD.HS_NO, SHD.HS_MC_NO                               ";
                strSql += cDefApp.CRLF + "  FROM JOB_MST JM                                                                    ";
                strSql += cDefApp.CRLF + " INNER JOIN CELL_MST SCM                                                             ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP              = SCM.WH_TYP                                           ";
                strSql += cDefApp.CRLF + "   AND JM.START_POS           = SCM.CELL_SC_NO                                       ";
                strSql += cDefApp.CRLF + "   AND JM.START_LOCATION      = SCM.CELL_NO                                          ";
                strSql += cDefApp.CRLF + " INNER JOIN CELL_MST DCM                                                             ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP              = DCM.WH_TYP                                           ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS            = DCM.CELL_SC_NO                                       ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_LOCATION       = DCM.CELL_NO                                          ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_HS_DEF SHD                                                            ";
                strSql += cDefApp.CRLF + "    ON SCM.WH_TYP             = SHD.WH_TYP                                           ";
                strSql += cDefApp.CRLF + "   AND SCM.CELL_SC_NO         = SHD.SC_NO                                            ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD                                                               ";
                strSql += cDefApp.CRLF + "    ON SCM.WH_TYP             = SD.WH_TYP                                            ";
                strSql += cDefApp.CRLF + "   AND SCM.CELL_SC_NO         = SD.SC_NO                                             ";
                strSql += cDefApp.CRLF + " INNER JOIN CV_DATA CD                                                               ";
                strSql += cDefApp.CRLF + "    ON SHD.WH_TYP             = CD.WH_TYP                                            ";
                strSql += cDefApp.CRLF + "   AND SHD.HS_MC_NO           = CD.MC_NO                                             ";
                strSql += cDefApp.CRLF + " WHERE SCM.WH_TYP             = :pWH_TYP                                             ";
                strSql += cDefApp.CRLF + "   AND SCM.AGING_TYP like '%' " + DbLang.II + " :pAGING_TYP "                        + DbLang.II + " '%' ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_STATUS          = '20'                                                 ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_TYP             = '5'                                                  ";  // AISLE TO AISLE
                strSql += cDefApp.CRLF + "   AND JM.START_POS           <> JM.DEST_POS                                         ";  // 다른 SC 번호
                strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD      = '1'                                                  ";
                strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD        = '1'                                                  ";
                strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD         = '0000'                                               ";
                strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD      = '1'                                                  ";
                strSql += cDefApp.CRLF + "   AND SD.UCSTATUS_RD         = '1'                                                  ";
                strSql += cDefApp.CRLF + "   AND CD.RETHS_READY_RD      = '1'                                                  ";  // DATA 없을때 출고H/S READY ON
                strSql += cDefApp.CRLF + "   AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                                                  ";  // 트랙 일시정지가 아니어야 함!
                strSql += cDefApp.CRLF + "   AND (SD.ITN_LUGG_FK1       = '0' AND SD.ITN_LUGG_FK2 = '0')                       ";
                strSql += cDefApp.CRLF + "   AND SD.OD_RQ_YN            = 'N'                                                  ";
                strSql += cDefApp.CRLF + "   AND SCM.CELL_USE_YN        = 'Y'                                                  ";
                strSql += cDefApp.CRLF + "   AND DCM.CELL_USE_YN        = 'Y'                                                  ";
                strSql += cDefApp.CRLF + "   AND SD.SUSPEND             IN ('0', '1')                                          "; //조한성 추가 - 정상이거나 입고금지일 때
                strSql += cDefApp.CRLF + "   AND SHD.DEST_DEF_DAT like '%' " + DbLang.II + " JM.DEST_POS " + DbLang.II + " '%' "; //조한성 추가
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("pAGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "0";
                string strBOTTOM_TRAY1 = "0";
                string strTOP_TRAY1 = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";

                string strBOTTOM_TRAY2 = "0";
                string strTOP_TRAY2 = "0";
                string strDEST_POS2 = "0";
                string strLUGG_NO2 = "0";

                string strHS_MC_NO1 = "0";
                string strHS_MC_NO2 = "0";

                string strSOUR_BANK_FK1 = "0";
                string strSOUR_BAY_FK1 = "0";
                string strSOUR_LEV_FK1 = "0";
                string strDEST_HSPOS_FK1 = "0";
                string strCELL_NO1 = "0";
                string strCELL_NO1_PAIR = "0";

                string strSOUR_BANK_FK2 = "0";
                string strSOUR_BAY_FK2 = "0";
                string strSOUR_LEV_FK2 = "0";
                string strDEST_HSPOS_FK2 = "0";
                string strCELL_NO2 = "0";
                string strUSE_FK = "0";
                string strUSER_COMMAND = "0";
                string strWRITE_FLAG = "0";
                string strSC_TYP = "0";
                string strCELL_SC_NO = "0";

                string strDEST_LOCATION1 = "0";
                string strDEST_LOCATION2 = "0";

                string strDEST_BANK_FK1 = "0";
                string strDEST_BAY_FK1 = "0";
                string strDEST_LEV_FK1 = "0";
                string strSOUR_HSPOS_FK1 = "0";
                string strDEST_BANK_FK2 = "0";
                string strDEST_BAY_FK2 = "0";
                string strDEST_LEV_FK2 = "0";
                string strSOUR_HSPOS_FK2 = "0";

                string strCELL_INOUT = "0";
                string strSC_PLT_JOB_TYP = "0";
                string strCELL_BANK = "";
                DateTime dtAGING_END_DT = new DateTime();
                DateTime dtRetLimit = new DateTime();
                DataTable dtCELL_MST = new DataTable();

                bool bSuccess = false;

                _pBdb.BeginTrans();

                strSC_TYP = _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString();

                if (strSC_TYP == "TWIN")
                {
                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                        strCELL_NO1 = "" + _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString();
                        strCELL_NO1_PAIR = "" + _pBdb.mDtMain.Rows[0]["PAIR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PAIR"].ToString();
                        strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BANK"].ToString();
                        strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BAY"].ToString();
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LEV"].ToString();
                        strDEST_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(0, 2);
                        strDEST_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(3, 3);
                        strDEST_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(7, 2);
                        strDEST_LOCATION1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();
                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString();
                        strHS_MC_NO1 = "" + _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString();
                        strUSE_FK = "0";
                        strCELL_INOUT = "" + _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString();

                        dtAGING_END_DT = Convert.ToDateTime(_pBdb.mDtMain.Rows[0]["AGING_END_DT"].ToString());

                        bSuccess = true;
                        if (i > 0)
                        {
                            if (strCELL_SC_NO != _pBdb.mDtMain.Rows[i]["CELL_SC_NO"].ToString() ||
                                strWH_TYP != _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString())
                                continue;

                            strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                            strBOTTOM_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BOTTOM_TRAY"].ToString();
                            strTOP_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TOP_TRAY"].ToString();
                            strDEST_POS2 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                            strLUGG_NO2 = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                            strCELL_NO2 = "" + _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString();
                            strSOUR_BANK_FK2 = "" + _pBdb.mDtMain.Rows[i]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BANK"].ToString();
                            strSOUR_BAY_FK2 = "" + _pBdb.mDtMain.Rows[i]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BAY"].ToString();
                            strSOUR_LEV_FK2 = "" + _pBdb.mDtMain.Rows[i]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LEV"].ToString();

                            strDEST_BANK_FK2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString().Substring(0, 2);
                            strDEST_BAY_FK2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString().Substring(3, 3);
                            strDEST_LEV_FK2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString().Substring(7, 2);
                            strDEST_LOCATION2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();

                            strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                            strUSE_FK = "1";

                            bSuccess = true;

                            break;
                        }
                    }
                }
                else
                {

                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                        strCELL_NO1 = "" + _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString();
                        strCELL_NO1_PAIR = "" + _pBdb.mDtMain.Rows[0]["PAIR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PAIR"].ToString();
                        strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BANK"].ToString();
                        strCELL_BANK = strSOUR_BANK_FK1;
                        strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BAY"].ToString();
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LEV"].ToString();

                        strDEST_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(0, 2);
                        strDEST_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(3, 3);
                        strDEST_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Substring(7, 2);
                        strDEST_HSPOS_FK1 = "" + _pBdb.mDtMain.Rows[i]["HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["HS_NO"].ToString();
                        strDEST_LOCATION1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();

                        strSOUR_BANK_FK1 = Convert.ToString("" + (((Convert.ToInt32(0 + strSOUR_BANK_FK1) + 1) % 2) + 1));
                        strDEST_BANK_FK1 = Convert.ToString("" + (((Convert.ToInt32(0 + strDEST_BANK_FK1) + 1) % 2) + 1));

                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                        strHS_MC_NO1 = "" + _pBdb.mDtMain.Rows[i]["HS_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["HS_MC_NO"].ToString();
                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString();
                        strUSE_FK = "0";

                        strCELL_INOUT = "" + _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString();
                        bSuccess = true;

                        #region OUT 위치일 시 IN 위치 화물이 잇고 에이징 END Time 검색 후 3분 이내면(사용x) - 주석처리
                        /*
                        if (strCELL_INOUT == "OUT")
                        {
                            int nCOUNT = 0;
                            string strPAIR_CELL_STA = "";
                            string strPAIR_BOTTOM_TRAY = "";
                            string strPAIR_TOP_TRAY = "";
                            string strPAIR_DEST_LOCATION = "";
                            //OUT일때 IN 작업이 있을때 RACK TO RACK을 할지 아니면 출고를 하던지
                            CHECK_LOCATION_BY_CELLNO(strCELL_SC_NO
                                                   , strCELL_NO1_PAIR
                                                   , strWH_TYP
                                                   , ref strPAIR_CELL_STA
                                                   , ref strPAIR_BOTTOM_TRAY
                                                   , ref strPAIR_TOP_TRAY
                                                   , ref dtAGING_END_DT
                                                   , ref nCOUNT);

                            // IN에 화물존재.
                            if (nCOUNT != 0)
                            {
                                if (strPAIR_CELL_STA == "1" || strPAIR_CELL_STA == "3")
                                {
                                    bSuccess = false;
                                    continue;
                                }
                                else if (strPAIR_CELL_STA == "2")
                                {

                                    // 미존재해야한다.
                                    CHECK_JOB_BY_CELLNO(strCELL_SC_NO, strCELL_NO1_PAIR, strWH_TYP, ref nCOUNT);
                                    if (nCOUNT == 0)
                                    {
                                        if (FETCH_LOCATION_BY_AGING_TYP(strWH_TYP
                                                                      , "000000000"
                                                                      , strCELL_SC_NO
                                                                      , dtAGING_END_DT
                                                                      , ref strPAIR_DEST_LOCATION
                                                                      , ref pRTN_MSG) == false)
                                        {
                                            _pBdb.Rollback();
                                            return false;
                                        }
                                        int iResult;

                                        // RACK TO RACK 작업생성.
                                        iResult = InsertJobMst(strWH_TYP
                                                              , 4
                                                              , strCELL_SC_NO
                                                              , strCELL_NO1_PAIR
                                                              , strCELL_SC_NO
                                                              , strPAIR_DEST_LOCATION
                                                              , strCELL_SC_NO
                                                              , strPAIR_TOP_TRAY
                                                              , strPAIR_BOTTOM_TRAY
                                                              , dtAGING_END_DT
                                                              , "0"
                                                              , ref pRTN_MSG);
                                        if (iResult > 0)
                                        {
                                            bSuccess = false;
                                            continue;
                                        }
                                        else
                                        {
                                            _pBdb.Rollback();
                                            return false;
                                        }
                                    }

                                    // 존재 시 작업을 하지않아야한다..
                                    bSuccess = false;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            bSuccess = true;
                            break;
                        }
                        //*/
                        #endregion
                    }
                }

                //출발 로케이션에 따라서 SC PLT 작업 정보 (Roll 작업 : 8열 1단 ~ 8열 7단)
                strSql = "";
                strSql += CRLF + " SELECT CM.*                      ";
                strSql += CRLF + "   FROM CELL_MST CM               ";
                strSql += CRLF + "  WHERE CM.WH_TYP   = :WH_TYP     ";
                strSql += CRLF + "    AND CM.BANK = :BANK           ";
                strSql += CRLF + "    AND CM.BAY = :BAY             ";
                strSql += CRLF + "    AND CM.LEV = :LEV             ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strCELL_BANK;
                _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                nSelCnt = _pBdb.ExcuteQry(dtCELL_MST, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    _pBdb.Rollback();
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG += "조회할 CELL_MST 정보가 존재하지 않습니다. CELL_MST : " + strCELL_BANK + "-" + strSOUR_BAY_FK1 + "-" + strSOUR_LEV_FK1;
                    _pBdb.Rollback();
                    return false;
                }

                strSC_PLT_JOB_TYP = dtCELL_MST.Rows[0]["SC_PLT_JOB_TYP"].ToString();

                if (bSuccess == true)
                {
                    // S/C 작업지시.
                    if (UPDATE_SC_DATA("2",                     // 출고로 지시가 되어야함!
                                       strLUGG_NO1,
                                       strSOUR_BANK_FK1,
                                       strSOUR_BAY_FK1,
                                       strSOUR_LEV_FK1,
                                       strDEST_HSPOS_FK1,
                                       strLUGG_NO2,
                                       strSOUR_BANK_FK2,
                                       strSOUR_BAY_FK2,
                                       strSOUR_LEV_FK2,
                                       strDEST_HSPOS_FK2,
                                       strUSE_FK,
                                       strUSER_COMMAND,
                                       strWRITE_FLAG,
                                       strWH_TYP,
                                       strCELL_SC_NO,
                                       strDEST_BANK_FK1,
                                       strDEST_BAY_FK1,
                                       strDEST_LEV_FK1,
                                       strSOUR_HSPOS_FK1,
                                       strDEST_BANK_FK2,
                                       strDEST_BAY_FK2,
                                       strDEST_LEV_FK2,
                                       strSOUR_HSPOS_FK2, strSC_PLT_JOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // 출고 H/S에 출고중 Flag Setting (조한성 추가)
                    if (UPDATE_CV_DATA_FOR_RET_ING(strLUGG_NO1, strWH_TYP, strCELL_SC_NO, strHS_MC_NO1, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }


                    if (UPDATE_JOB_DATA("21", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS1, strCELL_NO1, strHS_MC_NO1, strCELL_SC_NO, "0", strLUGG_NO2, "1") == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    /*
                    if (UPDATE_CELL_MST_DTL("3", strBOTTOM_TRAY1, strTOP_TRAY1, strWH_TYP, strCELL_SC_NO, strCELL_NO1, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // 도착지 LOCATION 입고예약처리 2019.10.13
                    if (UPDATE_CELL_MST_DTL("1", strBOTTOM_TRAY1, strTOP_TRAY1, strWH_TYP, strCELL_SC_NO, strDEST_LOCATION1, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                    //*/
                    if (strUSE_FK == "1")
                    {
                        if (UPDATE_JOB_DATA("21", strLUGG_NO2, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS2, strCELL_NO2, strHS_MC_NO2, strCELL_SC_NO, "0", strLUGG_NO1, "1") == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                        /*
                        if (UPDATE_CELL_MST_DTL("3", strBOTTOM_TRAY2, strTOP_TRAY2, strWH_TYP, strCELL_SC_NO, strCELL_NO2, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        // 도착지 LOCATION 입고예약처리 2019.10.13
                        if (UPDATE_CELL_MST_DTL("1", strBOTTOM_TRAY2, strTOP_TRAY2, strWH_TYP, strCELL_SC_NO, strDEST_LOCATION2, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                        //*/
                        pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strHS_MC_NO2 + " 출고 H/S로 호기간이동 시작하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]"; _pBdb.Commit();
                        _pBdb.Commit();
                        InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, "21", strCELL_SC_NO, strHS_MC_NO2);
                        return true;
                    }
                }
                pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strHS_MC_NO1 + " 출고 H/S로 호기간이동 시작하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";
                _pBdb.Commit();
                InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, "21", strCELL_SC_NO, strHS_MC_NO1);
                return true;

            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // 출고 갯수 제한
        public bool CHECK_RET_LIMIT_COUNT(string strWH_TYP,
                                          string strSTATION,
                                       DataTable dtRetLimit,
                                      ref string pRTN_MSG)
        {
            string strSql = "";
            int nSelCnt = 0;
            int nCvCnt = 0;
            int nScCnt = 0;
            int nRetLimit = 0;

            try
            {
                pRTN_MSG = "CHECK_RET_LIMIT_COUNT::";

                strSql = "";
                strSql += cDefApp.CRLF + "SELECT RET_CNT                                               ";
                strSql += cDefApp.CRLF + "  FROM DEST_POS_DEF                                    ";
                strSql += cDefApp.CRLF + " WHERE WH_TYP              = :pWH_TYP                  ";
                strSql += cDefApp.CRLF + "   AND MC_NO               IN ('" + strSTATION + "')   ";  // 149 로 출고되는 경우
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(dtRetLimit, strSql);

                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG += "해당 도착지에 설정된 값이 없습니다.";
                    return false;
                }

                string strCNT = "" + dtRetLimit.Rows[0]["RET_CNT"].ToString();

                nRetLimit = Convert.ToInt32(strCNT);

                strSql = "";
                strSql += cDefApp.CRLF + "SELECT JM.*, CD.*                                         ";
                strSql += cDefApp.CRLF + "  FROM JOB_MST JM                                         ";
                strSql += cDefApp.CRLF + " INNER JOIN CV_DATA CD                                    ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP              = CD.WH_TYP                 ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS            = CD.DEST_POS_RD            ";
                strSql += cDefApp.CRLF + " WHERE JM.WH_TYP              = :pWH_TYP                  ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_STATUS      NOT IN ('20', '99', '10')       ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_TYP             IN ('2', '3')               ";  // 출고, 피킹출고
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS            IN ('" + strSTATION + "')   ";  // 149 로 출고되는 경우
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(dtRetLimit, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                nCvCnt = nSelCnt;

                strSql = "";
                strSql += cDefApp.CRLF + "SELECT JM.*, SD.*                                         ";
                strSql += cDefApp.CRLF + "  FROM JOB_MST JM                                         ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD                                    ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP              = SD.WH_TYP                 ";
                strSql += cDefApp.CRLF + "   AND JM.LUGG_NO             = SD.ITN_LUGG_FK1            ";
                strSql += cDefApp.CRLF + " WHERE JM.WH_TYP              = :pWH_TYP                  ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_STATUS      NOT IN ('20', '99', '10')       ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_TYP             IN ('2', '3')               ";  // 출고, 피킹출고
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS            IN ('" + strSTATION + "')   ";  // 149 로 출고되는 경우
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(dtRetLimit, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                nScCnt = nSelCnt;

                int nTotalCnt = nCvCnt + nScCnt;

                if (nTotalCnt >= nRetLimit)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "출고제한 정책에 따라 출고할수 없습니다.[제한갯수 : " + nRetLimit .ToString()+ "개]";
                    return false;
                }

                pRTN_MSG = "";
                // string a = "" + dtRetLimit.Rows[0][""].ToString();


                //pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strHS_MC_NO1 + " 출고 H/S로 호기간이동 시작하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";
                //_pBdb.Commit();
                //InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, "21", strCELL_SC_NO, strHS_MC_NO1);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }            
        }

        // RACK TO RACK PCH, OCH, FCH 
        public bool CHECK_RACK_TO_RACK_PCH(string strWH_TYP,
                                           string strAGING_TYP,
                                           string strPLC_NO,
                                       ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";


                pRTN_MSG = "CHECK_RACK_TO_RACK_PCH::";

                strSql = "";
                strSql += cDefApp.CRLF + "SELECT JM.*, SD.*   ";
                strSql += cDefApp.CRLF + "  FROM JOB_MST JM                      ";          
                strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD  ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP    = SD.WH_TYP ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS  = SD.SC_NO  ";
                strSql += cDefApp.CRLF + " WHERE JM.WH_TYP      = ? ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS like '%' " + DbLang.II + " ? " + DbLang.II + " '%' ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_STATUS  = '20'  ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_TYP     = '4'   ";  // RACK TO RACK
                strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'          ";
                strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'          ";
                strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD     = '0'          ";
                strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD  = '1'          ";
                strSql += cDefApp.CRLF + "   AND SD.UCSTATUS_RD     = '1'          ";
                strSql += cDefApp.CRLF + "   AND (SD.ITN_LUGG_FK1   = '0' AND SD.ITN_LUGG_FK2 = '0')  ";
                strSql += cDefApp.CRLF + "   AND SD.OD_RQ_YN        = 'N'                             ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strAGING_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }


                string strJOB_TYP = "0";
                string strBOTTOM_TRAY1 = "0";
                string strTOP_TRAY1 = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";

                string strBOTTOM_TRAY2 = "0";
                string strTOP_TRAY2 = "0";
                string strDEST_POS2 = "0";
                string strLUGG_NO2 = "0";

                string strSTART_LOCATION1 = "0";
                string strDEST_LOCATION1 = "0";
                string strSTART_LOCATION2 = "0";
                string strDEST_LOCATION2 = "0";
                string strHS_MC_NO1 = "0";
                string strHS_MC_NO2 = "0";

                string strSOUR_BANK_FK1 = "0";
                string strSOUR_BAY_FK1 = "0";
                string strSOUR_LEV_FK1 = "0";
                string strDEST_HSPOS_FK1 = "0";
                string strCELL_NO1 = "0";


                string strSOUR_BANK_FK2 = "0";
                string strSOUR_BAY_FK2 = "0";
                string strSOUR_LEV_FK2 = "0";
                string strDEST_HSPOS_FK2 = "0";
                string strCELL_NO2 = "0";
                string strUSE_FK = "0";
                string strUSER_COMMAND = "0";
                string strWRITE_FLAG = "0";
                string strSC_TYP = "0";
                string strCELL_SC_NO = "0";

                string strDEST_BANK_FK1 = "0";
                string strDEST_BAY_FK1 = "0";
                string strDEST_LEV_FK1 = "0";
                string strSOUR_HSPOS_FK1 = "0";
                string strDEST_BANK_FK2 = "0";
                string strDEST_BAY_FK2 = "0";
                string strDEST_LEV_FK2 = "0";
                string strSOUR_HSPOS_FK2 = "0";
                string strSC_PLT_JOB_TYP = "0";


                bool bSuccess = false;

                _pBdb.BeginTrans();

                strSC_TYP = _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString();

                if (strSC_TYP == "TWIN")
                {
                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();

                        strSTART_LOCATION1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString();
                        if (strSTART_LOCATION1.Length == 9)
                        {
                            strSOUR_BANK_FK1 = strSTART_LOCATION1.Substring(0, 2);
                            strSOUR_BAY_FK1 = strSTART_LOCATION1.Substring(3, 3);
                            strSOUR_LEV_FK1 = strSTART_LOCATION1.Substring(7, 2);
                        }

                        strDEST_LOCATION1 =  "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();
                        if (strDEST_LOCATION1.Length == 9)
                        {
                            strDEST_BANK_FK1 = strDEST_LOCATION1.Substring(0, 2);
                            strDEST_BAY_FK1 = strDEST_LOCATION1.Substring(3, 3);
                            strDEST_LEV_FK1 = strDEST_LOCATION1.Substring(7, 2);
                        }

                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
   
                        strUSE_FK = "0";
                       
                        bSuccess = true;
                        if (i > 0)
                        {
                            if (strDEST_POS1 != _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() ||
                                strWH_TYP != _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString())
                                continue;

                            strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                            strBOTTOM_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                            strTOP_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                            strDEST_POS2 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                            strLUGG_NO2 = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
           
                            strSTART_LOCATION2 = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString();
                            if (strSTART_LOCATION2.Length == 9)
                            {
                                strSOUR_BANK_FK2 = strSTART_LOCATION2.Substring(0, 2);
                                strSOUR_BAY_FK2 = strSTART_LOCATION2.Substring(3, 3);
                                strSOUR_LEV_FK2 = strSTART_LOCATION2.Substring(7, 2);
                            }

                            strDEST_LOCATION2 =  "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                            if (strDEST_LOCATION2.Length == 9)
                            {
                                strDEST_BANK_FK2 = strDEST_LOCATION2.Substring(0, 2);
                                strDEST_BAY_FK2 = strDEST_LOCATION2.Substring(3, 3);
                                strDEST_LEV_FK2 = strDEST_LOCATION2.Substring(7, 2);
                            }                       

                            strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                            strUSE_FK = "1";

                            bSuccess = true;
                            break;
                        }
                    }
                }
                else
                {

                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();

                        strSTART_LOCATION1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString();
                        if (strSTART_LOCATION1.Length == 9)
                        {
                            strSOUR_BANK_FK1 = strSTART_LOCATION1.Substring(0, 2);
                            strSOUR_BAY_FK1 = strSTART_LOCATION1.Substring(3, 3);
                            strSOUR_LEV_FK1 = strSTART_LOCATION1.Substring(7, 2);
                        }

                        strDEST_LOCATION1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();
                        if (strDEST_LOCATION1.Length == 9)
                        {
                            strDEST_BANK_FK1 = strDEST_LOCATION1.Substring(0, 2);
                            strDEST_BAY_FK1 = strDEST_LOCATION1.Substring(3, 3);
                            strDEST_LEV_FK1 = strDEST_LOCATION1.Substring(7, 2);
                        }

                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();

                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                        strUSE_FK = "0";
                
                        bSuccess = true;
                        break;
                    }
                }

                if (bSuccess == true)
                {
                    // S/C 작업지시.
                    if (UPDATE_SC_DATA(strJOB_TYP,
                                       strLUGG_NO1,
                                       strSOUR_BANK_FK1,
                                       strSOUR_BAY_FK1,
                                       strSOUR_LEV_FK1,
                                       strDEST_HSPOS_FK1,
                                       strLUGG_NO2,
                                       strSOUR_BANK_FK2,
                                       strSOUR_BAY_FK2,
                                       strSOUR_LEV_FK2,
                                       strDEST_HSPOS_FK2,
                                       strUSE_FK,
                                       strUSER_COMMAND,
                                       strWRITE_FLAG,
                                       strWH_TYP,
                                       strCELL_SC_NO,
                                       strDEST_BANK_FK1,
                                       strDEST_BAY_FK1,
                                       strDEST_LEV_FK1,
                                       strSOUR_HSPOS_FK1,
                                       strDEST_BANK_FK2,
                                       strDEST_BAY_FK2,
                                       strDEST_LEV_FK2,
                                       strSOUR_HSPOS_FK2, strSC_PLT_JOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }


                    if (UPDATE_JOB_DATA("21", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS1, strCELL_NO1, strHS_MC_NO1, strCELL_SC_NO) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    if (strUSE_FK == "1")
                    {
                        if (UPDATE_JOB_DATA("21", strLUGG_NO2, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS2, strCELL_NO2, strHS_MC_NO2, strCELL_SC_NO) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        _pBdb.Commit();
                        return true;
                    }
                }

                _pBdb.Commit();
                return true;

            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }
        //Retrieve Check(RetHS ready)
        //RetHS Ready 신호가 올라오면서 출고작업이 있을 경우
        // S/C에서 출고H/S로 출고작업
        public bool CHECK_CV_RET_HS_READY(string strWH_TYP,
                                          string strAGING_TYP,
                                          string strPLC_NO, 
                                      ref string pRTN_MSG)
        {

            string strSql = "";
            int nSelCnt = 0;

            try
            {
                pRTN_MSG = "CHECK_CV_RETHS_READY::";

                #region SQL
                strSql = cDefApp.CRLF + "SELECT JM.*, CM.*, SD.SC_TYP, SHD.HS_NO, SHD.HS_MC_NO  ";
                strSql += cDefApp.CRLF + "  FROM JOB_MST JM                                     ";
                strSql += cDefApp.CRLF + " INNER JOIN CELL_MST CM                               ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP      = CM.WH_TYP                      ";
                strSql += cDefApp.CRLF + "   AND JM.START_POS   = CM.CELL_SC_NO                      ";
                strSql += cDefApp.CRLF + "   AND JM.START_LOCATION  = CM.CELL_NO                ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_HS_DEF SHD             ";
                strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = SHD.WH_TYP     ";
                strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = SHD.SC_NO      ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD ";
                strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = SD.WH_TYP   ";
                strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = SD.SC_NO    ";
                strSql += cDefApp.CRLF + " INNER JOIN CV_DATA CD ";          
                strSql += cDefApp.CRLF + "    ON SHD.WH_TYP     = CD.WH_TYP   ";
                strSql += cDefApp.CRLF + "   AND SHD.HS_MC_NO   = CD.MC_NO ";
                strSql += cDefApp.CRLF + " WHERE CM.WH_TYP      = :WH_TYP           ";
                strSql += cDefApp.CRLF + "   AND CM.AGING_TYP   like '%' " + DbLang.II + " :AGING_TYP " + DbLang.II + " '%'    ";
                strSql += cDefApp.CRLF + "   AND SHD.DEST_DEF_DAT like '%' " + DbLang.II + " JM.DEST_POS " + DbLang.II + " '%' ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_STATUS      = '20'  ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_TYP         = '2'   "; //조한성추가 - 출고작업
                strSql += cDefApp.CRLF + "   AND CD.RETHS_READY_RD  = '1'   ";  // DATA 없을때 출고H/S READY ON
                strSql += cDefApp.CRLF + "   AND CD.SENSOR0_DATA_RD = '0'   ";  // A
                strSql += cDefApp.CRLF + "   AND CD.OD_RQ_YN        = 'N'   ";
                strSql += cDefApp.CRLF + "   AND SD.OD_RQ_YN        = 'N'   ";
                strSql += cDefApp.CRLF + "   AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')   ";  // 트랙 일시정지가 아니어야 함!
                strSql += cDefApp.CRLF + "   AND CD.RET_HS_YN       = 'Y'   ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS        <> '0'  ";
                strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'   ";
                strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'   ";
                strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD     = '0000'";
                strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD  = '1'   ";
                strSql += cDefApp.CRLF + "   AND SD.UCSTATUS_RD     = '1'   ";
                strSql += cDefApp.CRLF + "   AND SD.SENSOR_FK_RD    = '0'   ";   // 화물없음.
                strSql += cDefApp.CRLF + "   AND (SD.ITN_LUGG_FK1   = '0' AND SD.ITN_LUGG_FK2 = '0')    ";
                strSql += cDefApp.CRLF + "   AND SD.SUSPEND        IN ('0', '1') ";
                strSql += cDefApp.CRLF + "   AND SHD.HS_NO         IN ('02', '04', '05', '06', '07', '08', '09', '10', '11')";  
                strSql += cDefApp.CRLF + " ORDER BY JM.JOB_PRIORITY DESC, JM.INS_DT ASC, CM.CELL_INOUT ";   // 우선순위가 높은숫자순으로 처리.
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("AGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }
                #endregion
                
                #region 각종변수 선언 및 초기화

                string strJOB_TYP = "0";
                string strBOTTOM_TRAY1 = "0";
                string strTOP_TRAY1 = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";

                string strBOTTOM_TRAY2 = "0";
                string strTOP_TRAY2 = "0";
                string strDEST_POS2 = "0";
                string strLUGG_NO2 = "0";

                string strHS_MC_NO1 = "0";
                string strHS_MC_NO2 = "0";

                string strSOUR_BANK_FK1 = "0";
                string strSOUR_BAY_FK1 = "0";
                string strSOUR_LEV_FK1 = "0";
                string strDEST_HSPOS_FK1 = "0";
                string strCELL_NO1 = "0";
                string strCELL_NO1_PAIR = "0";
                string strSOUR_BANK_FK2 = "0";
                string strSOUR_BAY_FK2 = "0";
                string strSOUR_LEV_FK2 = "0";
                string strDEST_HSPOS_FK2 = "0";
                string strDEST_LOCATION1 = "00-000-00";
                string strDEST_LOCATION2 = "00-000-00";
                string strCELL_NO2 = "0";
                string strUSE_FK = "0";
                string strUSER_COMMAND = "0";
                string strWRITE_FLAG = "0";
                string strSC_TYP = "0";
                string strCELL_SC_NO = "0";

                string strDEST_BANK_FK1 = "0";
                string strDEST_BAY_FK1 = "0";
                string strDEST_LEV_FK1 = "0";
                string strSOUR_HSPOS_FK1 = "0";
                string strDEST_BANK_FK2 = "0";
                string strDEST_BAY_FK2 = "0";
                string strDEST_LEV_FK2 = "0";
                string strSOUR_HSPOS_FK2 = "0";

                string strJOB_PRIORITY = "0";

                string strCELL_INOUT = "0";
                string strSC_PLT_JOB_TYP = "0";
                string strCELL_BANK = "";
                DateTime dtAGING_END_DT = new DateTime();
                bool bSuccess = false;
                DataTable dtDeadLock = new DataTable();
                DataTable dtRetLimit = new DataTable();
                DataTable dtCELL_MST = new DataTable();

                
                #endregion

                #region 각종변수 DATA입력
                _pBdb.BeginTrans();
                strSC_TYP = _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString();

                if (strSC_TYP == "TWIN")
                {
                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                        strCELL_NO1 = "" + _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString();
                        strCELL_NO1_PAIR = "" + _pBdb.mDtMain.Rows[0]["PAIR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PAIR"].ToString();
                        strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BANK"].ToString();
                        strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BAY"].ToString();
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LEV"].ToString();
                        strDEST_HSPOS_FK1 = "" + _pBdb.mDtMain.Rows[0]["HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["HS_NO"].ToString();
                        strDEST_LOCATION1 = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();
                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LEV"].ToString();
                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString();
                        strHS_MC_NO1 = "" + _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString();
                        strUSE_FK = "0";

                        strJOB_PRIORITY = "" + _pBdb.mDtMain.Rows[0]["JOB_PRIORITY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_PRIORITY"].ToString();
                        strCELL_INOUT = "" + _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString();
                        dtAGING_END_DT = Convert.ToDateTime(_pBdb.mDtMain.Rows[0]["AGING_END_DT"].ToString());


                        bSuccess = true;
                        if (i > 0)
                        {
                            if (strCELL_SC_NO       != _pBdb.mDtMain.Rows[i]["CELL_SC_NO"].ToString() ||
                                strDEST_HSPOS_FK1   != _pBdb.mDtMain.Rows[i]["HS_NO"].ToString()      ||
                                strWH_TYP           != _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() )
                                continue;

                            strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                            strBOTTOM_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BOTTOM_TRAY"].ToString();
                            strTOP_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TOP_TRAY"].ToString();
                            strDEST_POS2 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                            strLUGG_NO2 = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                            strCELL_NO2 = "" + _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString();
                            strSOUR_BANK_FK2 = "" + _pBdb.mDtMain.Rows[i]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BANK"].ToString();
                            strSOUR_BAY_FK2 = "" + _pBdb.mDtMain.Rows[i]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BAY"].ToString();
                            strSOUR_LEV_FK2 = "" + _pBdb.mDtMain.Rows[i]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LEV"].ToString();
                            strDEST_HSPOS_FK2 = "" + _pBdb.mDtMain.Rows[i]["HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["HS_NO"].ToString();
                            strDEST_LOCATION2 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                            strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                            strHS_MC_NO2 = "" + _pBdb.mDtMain.Rows[i]["PAIR_HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["HS_MC_NO"].ToString();
                            strUSE_FK = "1";
                        }
                    }
                }
                else
                {
                    // SINGLE
                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                        //strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BOTTOM_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BOTTOM_TRAY"].ToString();
                        //strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["TOP_TRAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TOP_TRAY"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                        strCELL_NO1 = "" + _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString();
                        //strCELL_NO1_PAIR = "" + _pBdb.mDtMain.Rows[i]["PAIR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PAIR"].ToString();
                        strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[i]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BANK"].ToString();
                        strCELL_BANK = strSOUR_BANK_FK1;
                        strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[i]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BAY"].ToString();
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[i]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LEV"].ToString();
                        strDEST_HSPOS_FK1 = "" + _pBdb.mDtMain.Rows[i]["HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["HS_NO"].ToString();
                        strDEST_LOCATION1 = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                        strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[i]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LEV"].ToString();
                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[i]["CELL_SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["CELL_SC_NO"].ToString();
                        strHS_MC_NO1 = "" + _pBdb.mDtMain.Rows[i]["HS_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["HS_MC_NO"].ToString();
                        strUSE_FK = "0";

                        strSOUR_BANK_FK1 = Convert.ToString("" + (((Convert.ToInt32(0 + strSOUR_BANK_FK1) + 1) % 2) + 1));

                        strJOB_PRIORITY = "" + _pBdb.mDtMain.Rows[i]["JOB_PRIORITY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_PRIORITY"].ToString();
                        strCELL_INOUT = "" + _pBdb.mDtMain.Rows[i]["CELL_INOUT"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["CELL_INOUT"].ToString();
                        bSuccess = true;

                        #region MSC 일때의 로직 - 주석 처리됨
                        /*
                        // OUT 위치일 시 IN 위치 화물이 잇고 에이징 END Time 검색 후 3분 이내면 
                        // 출고작업 위치가 OUT CELL일때.
                        if (strCELL_INOUT == "OUT")
                        {
                            int nCOUNT = 0;
                            string strPAIR_CELL_STA = "";
                            string strPAIR_BOTTOM_TRAY = "";
                            string strPAIR_TOP_TRAY = "";
                            string strPAIR_DEST_LOCATION = "";

                            // OUT일때 IN 작업이 있을때 RACK TO RACK을 할지 아니면 출고를 하던지
                            // CELL정보가져오기(CELL_STA, BOTTOM, TOP TRAY정보)
                            // strCELL_NO1_PAIR = "IN"
                            // IN을 검색.
                            CHECK_LOCATION_BY_CELLNO(strCELL_SC_NO
                                                   , strCELL_NO1_PAIR
                                                   , strWH_TYP
                                                   , ref strPAIR_CELL_STA
                                                   , ref strPAIR_BOTTOM_TRAY
                                                   , ref strPAIR_TOP_TRAY
                                                   , ref dtAGING_END_DT
                                                   , ref nCOUNT);

                            // OUT위치의 재고를 출고시켜야 하는데 IN에 재고가 존재할경우.
                            if (nCOUNT != 0)
                            {
                                // IN재고가 입고예약 및 출고예약 중에 있다면 다음작업 진행.
                                if (strPAIR_CELL_STA == "1" || strPAIR_CELL_STA == "3")
                                {
                                    bSuccess = false;
                                    continue;
                                }
                                else if (strPAIR_CELL_STA == "2")
                                {
                                    // 해당 LOCATION이 시작작업에 해당되는지 체크한다.
                                    CHECK_JOB_BY_CELLNO(strCELL_SC_NO, strCELL_NO1_PAIR, strWH_TYP, ref nCOUNT);

                                    if (nCOUNT == 0)
                                    {
                                        // LOCATION 가져오기.
                                        if (FETCH_LOCATION_BY_AGING_TYP(strWH_TYP
                                                                      , "000000000"
                                                                      , strCELL_SC_NO
                                                                      , dtAGING_END_DT
                                                                      , ref strPAIR_DEST_LOCATION
                                                                      , ref pRTN_MSG) == false)
                                        {
                                            _pBdb.Rollback();
                                            return false;
                                        }

                                        int iResult;

                                        // RACK TO RACK 작업생성.
                                        iResult = InsertJobMst(strWH_TYP
                                                             , 4
                                                             , strCELL_SC_NO
                                                             , strCELL_NO1_PAIR
                                                             , strCELL_SC_NO
                                                             , strPAIR_DEST_LOCATION
                                                             , strCELL_SC_NO
                                                             , strPAIR_TOP_TRAY
                                                             , strPAIR_BOTTOM_TRAY
                                                             , dtAGING_END_DT
                                                             , "0"
                                                             , ref pRTN_MSG);

                                        if (iResult > 0)
                                        {
                                            bSuccess = false;
                                            continue;
                                        }
                                        else
                                        {
                                            _pBdb.Rollback();
                                            return false;
                                        }
                                    }
                                    bSuccess = false;
                                    continue;
                                }
                            }
                            else
                            {
                                bSuccess = true;
                                break;
                            }
                        }
                        else
                        {
                            bSuccess = true;
                            break;
                        }
                        //*/
                        #endregion
                    }
                }
                #endregion

                //출발 로케이션에 따라서 SC PLT 작업 정보 (Roll 작업 : 8열 1단 ~ 8열 7단)
                #region 출발 로케이션에 따라서 SC PLT 작업 정보(Roll 작업 : 8열 1단 ~ 8열 7단)
                strSql = "";
                strSql += CRLF + " SELECT CM.*                      ";
                strSql += CRLF + "   FROM CELL_MST CM               ";
                strSql += CRLF + "  WHERE CM.WH_TYP   = :WH_TYP     ";
                strSql += CRLF + "    AND CM.BANK = :BANK           ";
                strSql += CRLF + "    AND CM.BAY = :BAY             ";
                strSql += CRLF + "    AND CM.LEV = :LEV             ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strCELL_BANK;
                _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                nSelCnt = _pBdb.ExcuteQry(dtCELL_MST, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    _pBdb.Rollback();
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG += "조회할 CELL_MST 정보가 존재하지 않습니다. CELL_MST : " + strCELL_BANK + "-" + strSOUR_BAY_FK1 + "-" + strSOUR_LEV_FK1;
                    _pBdb.Rollback();
                    return false;
                }

                strSC_PLT_JOB_TYP = dtCELL_MST.Rows[0]["SC_PLT_JOB_TYP"].ToString();
                #endregion

                #region 실제 처리
                if (bSuccess == true)
                {
                    #region 데드락 체크
                    if (cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strCELL_SC_NO, strDEST_POS1, ref pRTN_MSG, ref dtDeadLock) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                    #endregion
                    #region S/C에 출고명령
                    if (UPDATE_SC_DATA("2", 
                                       strLUGG_NO1, 
                                       strSOUR_BANK_FK1, 
                                       strSOUR_BAY_FK1, 
                                       strSOUR_LEV_FK1, 
                                       strDEST_HSPOS_FK1, 
                                       strLUGG_NO2,
                                       strSOUR_BANK_FK2, 
                                       strSOUR_BAY_FK2, 
                                       strSOUR_LEV_FK2, 
                                       strDEST_HSPOS_FK2, 
                                       strUSE_FK, 
                                       strUSER_COMMAND, 
                                       strWRITE_FLAG,
                                       strWH_TYP, 
                                       strCELL_SC_NO,
                                       strDEST_BANK_FK1,
                                       strDEST_BAY_FK1,
                                       strDEST_LEV_FK1, 
                                       strSOUR_HSPOS_FK1, 
                                       strDEST_BANK_FK2,
                                       strDEST_BAY_FK2,
                                       strDEST_LEV_FK2,
                                       strSOUR_HSPOS_FK2, strSC_PLT_JOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                    #endregion
                    #region 출고 H/S에 출고중 Flag Setting
                    if (UPDATE_CV_DATA_FOR_RET_ING(strLUGG_NO1, strWH_TYP, strCELL_SC_NO, strHS_MC_NO1, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    #endregion

                    #region 출고 제한 걸기
                    string strLimitDestPos = "149";
                    if (strDEST_POS1 == strLimitDestPos)
                    {
                        if (CHECK_RET_LIMIT_COUNT(strWH_TYP, strLimitDestPos, dtRetLimit, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                    }

                    #endregion
                    #region 화재감지출고인경우               - 주석처리(SKI 에서 사용)
                    //if (strJOB_PRIORITY == "119")
                    //{
                    //    if (UPDATE_SC_FIRE(strWH_TYP, strCELL_SC_NO, strCELL_NO1, ref pRTN_MSG) == false)
                    //    {
                    //        _pBdb.Rollback();
                    //        return false;
                    //    }
                    //}
                    #endregion
                    #region 작업구동중으로 변경
                    if (UPDATE_JOB_DATA("21"
                                       , strLUGG_NO1
                                       , strWH_TYP
                                       , strJOB_TYP
                                       , ref pRTN_MSG
                                       , strDEST_POS1
                                       , strCELL_NO1
                                       , strHS_MC_NO1
                                       , strCELL_SC_NO) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                    #endregion
                    #region 출고예약                         - 주석처리(재고관리시 사용)
                    //if (UPDATE_CELL_MST_DTL("3"
                    //                      , strBOTTOM_TRAY1
                    //                      , strTOP_TRAY1
                    //                      , strWH_TYP
                    //                      , strCELL_SC_NO
                    //                      , strCELL_NO1
                    //                      , ref pRTN_MSG) == false )
                    //{
                    //    _pBdb.Rollback();
                    //    return false;
                    //}

                    //if (strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString())
                    //{
                    //    if (UPDATE_CELL_MST_DTL("1"
                    //                          , strBOTTOM_TRAY1
                    //                          , strTOP_TRAY1
                    //                          , strWH_TYP
                    //                          , strDEST_POS1
                    //                          , strDEST_LOCATION1
                    //                          , ref pRTN_MSG) == false)
                    //    {
                    //        _pBdb.Rollback();
                    //        return false;
                    //    }
                    //}
                    #endregion
                    #region 2번포크                          - 주석처리(Twin, Double일때 사용)
                    //if (strUSE_FK == "1")
                    //{
                    //    if (UPDATE_JOB_DATA("21"
                    //                      , strLUGG_NO2
                    //                      , strWH_TYP
                    //                      , strJOB_TYP
                    //                      , ref pRTN_MSG
                    //                      , strDEST_POS2
                    //                      , strCELL_NO2
                    //                      , strHS_MC_NO2
                    //                      , strCELL_SC_NO) == false )
                    //    {
                    //        _pBdb.Rollback();
                    //        return false;
                    //    }

                    //    // 출고예약
                    //    if (UPDATE_CELL_MST_DTL("3", strBOTTOM_TRAY2, strTOP_TRAY2, strWH_TYP, strCELL_SC_NO, strCELL_NO2, ref pRTN_MSG) == false)
                    //    {
                    //        _pBdb.Rollback();
                    //        return false;
                    //    }

                    //    _pBdb.Commit();
                    //    return true;
                    //}
                    #endregion
                }
                #endregion

                pRTN_MSG += "SC번호 : " + strCELL_SC_NO + "에서 " + strHS_MC_NO1 + " 출고 H/S로 출고하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";

                 //+ "출고 작업지시 성공"
                _pBdb.Commit();
                InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, "21", strCELL_SC_NO, strHS_MC_NO1);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }


        // 입고H/S도착 후 입고시작 상태로 변경.
        public bool IsOnlineSC(string strWH_TYP,
                               string strAGING_TYP,
                           ref string pRTN_MSG)
        {
            //DataTable dtSC_HS_DEF = new DataTable();
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                pRTN_MSG = "IsOnlineSC::";

                strSql = "";
                strSql += CRLF + " SELECT SD.* ";
                strSql += CRLF + "   FROM SC_DATA SD       ";
                strSql += CRLF + "  INNER JOIN EQP_MST EM  ";
                strSql += CRLF + "     ON EM.WH_TYP         = SD.WH_TYP  ";
                strSql += CRLF + "    AND EM.PLC_NO         = SD.PLC_NO ";
                strSql += CRLF + "  WHERE SD.WH_TYP		    = :WH_TYP   ";
                strSql += CRLF + "    AND SD.OD_RQ_YN           = 'N' ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK1       = '0' ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK2       = '0' ";
                strSql += CRLF + "    AND SD.ONLINE_MODE_RD     = '1' ";
                strSql += CRLF + "    AND SD.AUTO_MODE_RD       = '1' ";
                strSql += CRLF + "    AND SD.ERR_CODE_RD        = '0000' ";
                strSql += CRLF + "    AND SD.ACTIVE_MODE_RD     = '1' ";
                strSql += CRLF + "    AND SD.UCSTATUS_RD        = '1' ";  
                strSql += CRLF + "    AND SD.SC_GRP_NO          =  :AGING_TYP  ";  
                strSql += CRLF + "    AND EM.CONNECTED_YN       = 'Y' ";  
                strSql += CRLF + "    AND EM.USE_YN             = 'Y' ";  
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("AGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return false;

                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }

        // 작업순서를 출고 -> 랙투랙 -> 호기간 이동 -> 입고 순으로 진행
        public bool PrioritySto(string strWH_TYP,
                                string strAGING_TYP)
        {


            try
            {

                string strRTN_MSG = "";

                strRTN_MSG = "PrioritySto::";


                // 입고 HS도착 후 크레인 입고 지시 내릴지 체크
                if (!CHECK_CV_STO_HS_READY(strWH_TYP
                                         , strAGING_TYP
                                         , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Sto);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // S/C에서 출고H/S로 출고작업처리
                if (!CHECK_CV_RET_HS_READY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                        , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString()
                                        , "0"
                                        , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Ret);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                if (!CHECK_RACK_TO_RACK(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                      , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString()
                                      , "0"
                                      , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.RtoR);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // 크레인 호기간 이동 
                if (!CHECK_AISLE_TO_AISLE(strWH_TYP
                                        , strAGING_TYP
                                        , "0"
                                        , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Aisle2Aisle);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                } 
                Thread.Sleep(50);


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // 작업순서를 출고 -> 랙투랙 -> 호기간 이동 -> 입고 순으로 진행
        public bool PriorityRet(string strWH_TYP,
                                string strAGING_TYP)
        {


            try
            {
                string strRTN_MSG = "";
                strRTN_MSG = "PriorityRet::";

                //// 화재감지 출고작업생성
                //if (!CHECK_SC_FIRE_STATUS(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                //                        , strAGING_TYP
                //                        , "0"
                //                        , ref strRTN_MSG))
                //{
                //    MakeMsg_Error_NoLog(strRTN_MSG);
                //    SetErrorMsg(strRTN_MSG);
                //}
                //else
                //{
                //    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                //    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Ret);
                //    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                //}
                //Thread.Sleep(50);


                // S/C에서 출고H/S로 출고작업처리
                if (!CHECK_CV_RET_HS_READY(strWH_TYP
                                        ,  strAGING_TYP
                                        ,  "0"
                                        , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Ret);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // 크레인 랙투랙 지시
                if (!CHECK_RACK_TO_RACK(strWH_TYP
                                      , strAGING_TYP
                                      , "0"
                                      , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.RtoR);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // 크레인 호기간 이동 
                if (!CHECK_AISLE_TO_AISLE(strWH_TYP
                                        , strAGING_TYP
                                        , "0"
                                        , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Aisle2Aisle);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // 이중입고처리
                if (!CHECK_SC_DUPLICATE_STORAGE(strWH_TYP
                                              , strAGING_TYP
                                              , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Sto);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);


                // 입고 HS도착 후 크레인 입고 지시 내릴지 체크
                if (!CHECK_CV_STO_HS_READY(strWH_TYP
                                         , strAGING_TYP
                                         , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Sto);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);



                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }



        // 작업순서를 출고 -> 랙투랙 -> 호기간 이동 -> 입고 순으로 진행
        public bool PriorityRackToRack(string strWH_TYP,
                                       string strAGING_TYP)
        {


            try
            {

                string strRTN_MSG = "";

                strRTN_MSG = "PriorityRackToRack::";

                // 크레인 랙투랙 지시
                if (!CHECK_RACK_TO_RACK(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                      , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString()
                                      , "0"
                                      , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.RtoR);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);


                // 크레인 호기간 이동 
                if (!CHECK_AISLE_TO_AISLE(strWH_TYP
                                        , strAGING_TYP
                                        , "0"
                                        , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Aisle2Aisle);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // 입고 HS도착 후 크레인 입고 지시 내릴지 체크
                if (!CHECK_CV_STO_HS_READY(strWH_TYP
                                         , strAGING_TYP
                                         , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Sto);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // S/C에서 출고H/S로 출고작업처리
                if (!CHECK_CV_RET_HS_READY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                        , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString()
                                        , "0"
                                        , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Ret);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        // 작업순서를 출고 -> 랙투랙 -> 호기간 이동 -> 입고 순으로 진행
        public bool PriorityAisleToAisle(string strWH_TYP,
                                         string strAGING_TYP)
        {


            try
            {

                string strRTN_MSG = "";

                strRTN_MSG = "PriorityAisleToAisle::";


                // 크레인 호기간 이동 
                if (!CHECK_AISLE_TO_AISLE(strWH_TYP
                                        , strAGING_TYP
                                        , "0"
                                        , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Aisle2Aisle);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // 입고 HS도착 후 크레인 입고 지시 내릴지 체크
                if (!CHECK_CV_STO_HS_READY(strWH_TYP
                                         , strAGING_TYP
                                         , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Sto);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                // S/C에서 출고H/S로 출고작업처리
                if (!CHECK_CV_RET_HS_READY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                        , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString()
                                        , "0"
                                        , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.Ret);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);


                // 크레인 랙투랙 지시
                if (!CHECK_RACK_TO_RACK(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                      , ((int)cDefApp.eAGINGTYP.AGING_1G).ToString()
                                      , "0"
                                      , ref strRTN_MSG))
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
                else
                {
                    dicPrevJob.Remove(Convert.ToInt32(strAGING_TYP));
                    dicPrevJob.Add(Convert.ToInt32(strAGING_TYP), (int)cDefApp.eJOBTYP.RtoR);
                    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                }
                Thread.Sleep(50);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        // 입고H/S도착 후 입고시작 상태로 변경.
        public bool CHECK_CV_STO_HS_READY(string strWH_TYP,
                                          string strAGING_TYP,
                                      ref string pRTN_MSG)
        {
            string strPLC_NO = "";
            
 
            try
            {
                string strSql;
                int nSelCnt = 0;
                int nJobCnt = 0;

                pRTN_MSG = "CHECK_CV_STO_HS_READY::";

                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*, SD.*                                          ";
                strSql += CRLF + "      , CD.MC_NO AS CV_MC_NO, SD.MC_NO AS SC_MC_NO                ";
                strSql += CRLF + "   FROM CV_DATA CD                                                ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                                           ";
                strSql += CRLF + "     ON CD.WH_TYP        = JM.WH_TYP                              ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD    = JM.LUGG_NO                             ";
                strSql += CRLF + "  LEFT OUTER JOIN SC_DATA SD                                      ";
                strSql += CRLF + "     ON JM.WH_TYP        = SD.WH_TYP                              ";
                strSql += CRLF + "    AND JM.DEST_POS      = SD.SC_NO                               ";
                strSql += CRLF + "  WHERE CD.WH_TYP		    = :pWH_TYP                              ";
                strSql += CRLF + "    AND CD.STOHS_READY_RD = '1'                                   ";  // 입고 H/S READY ON
                strSql += CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                                      ";  // 트랙 일시정지가 아니어야 함!
                strSql += CRLF + "    AND CD.DEST_POS_RD like '%' " + DbLang.II + " :pAGING_TYP " + DbLang.II + " '%'             ";
                strSql += CRLF + "    AND JM.DEST_POS    like '%' " + DbLang.II + " :pAGING_TYP " + DbLang.II + " '%'             ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'                               ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'                               ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'                               ";
                strSql += CRLF + "    AND SD.OD_RQ_YN           = 'N'                               ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK1       = '0'                               ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK2       = '0'                               ";
                strSql += CRLF + "    AND (SD.ONLINE_MODE_RD    = '1' OR SD.ONLINE_MODE_RD = '2')   ";
                strSql += CRLF + "    AND SD.AUTO_MODE_RD       = '1'                               ";
                strSql += CRLF + "    AND SD.ERR_CODE_RD        = '0000'                            ";
                strSql += CRLF + "    AND SD.ACTIVE_MODE_RD     = '1'                               ";
                strSql += CRLF + "    AND SD.UCSTATUS_RD        = '1'                               ";  // [대기중/동작중]
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '18'                              ";  // 도착보고 완료
                strSql += CRLF + "    AND (JM.JOB_TYP 	        = '1' OR JM.JOB_TYP 	    = '5')  ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null                                   ";
                strSql += CRLF + "    AND SD.SUSPEND          IN ('0', '2')                         "; //입고금지, 작업금지가 아닐때만
                strSql += CRLF + "  ORDER BY JM.INS_DT, CD.TRACK_NO                                 ";
                _pBdb.mComMain.CommandType = CommandType.Text; 
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("pAGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                _pBdb.mComMain.Parameters.Add("pAGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                nJobCnt = _pBdb.ExcuteQry(strSql);
                if (nJobCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nJobCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }


                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strIS_TURN = "";
                string strSTART_POS = "";
                string strSTART_TRACK = "";
                string strJOB_TYP = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";
                string strLUGG_NO2 = "0";
                string strHS_MC_NO2 = "0";

                string strDEST_LOCATION = "00-000-00";
                string strSOUR_BANK_FK1 = "0";
                string strSOUR_BAY_FK1 = "0";
                string strSOUR_LEV_FK1 = "0";
                string strDEST_HSPOS_FK1 = "0";
                string strSOUR_BANK_FK2 = "0";
                string strSOUR_BAY_FK2 = "0";
                string strSOUR_LEV_FK2 = "0";
                string strDEST_HSPOS_FK2 = "0";
                string strCELL_NO1 = "0";
                string strCELL_NO2 = "0";
                string strUSE_FK = "0";
                string strUSER_COMMAND = "0";
                string strWRITE_FLAG = "0";
                string strSC_TYP = "0";

                string strDEST_BANK_FK1 = "0";
                
                string strDEST_BAY_FK1 = "0";
                string strDEST_LEV_FK1 = "0";
                string strSOUR_HSPOS_FK1 = "0";
                string strDEST_BANK_FK2 = "0";
                string strDEST_BAY_FK2 = "0";
                string strDEST_LEV_FK2 = "0";
                string strSOUR_HSPOS_FK2 = "0";
                string strSC_PLT_JOB_TYP = "0";

                string strJobType = "";

                _pBdb.BeginTrans();

                DataTable dtSC_HS_DEF = new DataTable();
                DataTable dtCELL_MST = new DataTable();
                for (int i = 0; i < nJobCnt; i++)
                {
                    strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[0]["TRAY_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_TYP"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString();
                    strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[0]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TURN"].ToString();
                    strLUGG_NO1 = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[0]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[0]["CV_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CV_MC_NO"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString();
                    strSC_TYP = "" + _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString();
                    strSTART_TRACK = "" + _pBdb.mDtMain.Rows[0]["CV_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CV_MC_NO"].ToString();
                    if (strDEST_LOCATION.Length == 9)
                    {
                        strDEST_BANK_FK1 = Convert.ToString("" + (((Convert.ToInt32(0 + strDEST_LOCATION.Substring(0, 2)) + 1) % 2) +1)) ;
                        strDEST_BAY_FK1 = strDEST_LOCATION.Substring(3, 3);
                        strDEST_LEV_FK1 = strDEST_LOCATION.Substring(7, 2);
                    }

                    //도착 로케이션에 따라서 SC PLT 사이즈 지시
                    strSql = "";
                    strSql += CRLF + " SELECT CM.*                      ";
                    strSql += CRLF + "   FROM CELL_MST CM               ";
                    strSql += CRLF + "  WHERE CM.WH_TYP   = :WH_TYP     ";
                    strSql += CRLF + "    AND CM.BANK = :BANK           ";
                    strSql += CRLF + "    AND CM.BAY = :BAY             ";
                    strSql += CRLF + "    AND CM.LEV = :LEV             "; 
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strDEST_LOCATION.Substring(0, 2);
                    _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                    nSelCnt = _pBdb.ExcuteQry(dtCELL_MST, strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }
                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "조회할 CELL_MST 정보가 존재하지 않습니다. CELL_MST : " + strDEST_LOCATION;
                        _pBdb.Rollback();
                        return false;
                    }

                    strSC_PLT_JOB_TYP = dtCELL_MST.Rows[0]["SC_PLT_JOB_TYP"].ToString();

                    // 입고 H/S정보가져오기.
                    strSql = "";
                    strSql += CRLF + " SELECT SHD.*                      ";
                    strSql += CRLF + "   FROM SC_HS_DEF SHD              ";
                    strSql += CRLF + "  WHERE SHD.WH_TYP   = :WH_TYP     ";
                    strSql += CRLF + "    AND SHD.HS_MC_NO = :HS_MC_NO   ";
                    strSql += CRLF + "    AND SHD.HS_NO IN ('01','03')   ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("HS_MC_NO", DbLang.VARCHAR).Value = strSTART_POS;
                    nSelCnt = _pBdb.ExcuteQry(dtSC_HS_DEF, strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }
                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "조회할 H/S TRACK 정보가 존재하지 않습니다. HS_MC_NO : " + strSTART_POS;
                        _pBdb.Rollback();
                        return false;
                    }

                    strSOUR_HSPOS_FK1 = dtSC_HS_DEF.Rows[0]["HS_NO"].ToString() == "" ? "0" : dtSC_HS_DEF.Rows[0]["HS_NO"].ToString();

                    if (i > 0 && strSC_TYP == "TWIN")
                    {
                        if (_pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() != _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString())
                            continue;

                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();        
                        strLUGG_NO2 = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                      
                        strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();

                        strDEST_BANK_FK2 = strDEST_LOCATION.Substring(0, 2);
                        strDEST_BAY_FK2 = strDEST_LOCATION.Substring(3, 3);
                        strDEST_LEV_FK2 = strDEST_LOCATION.Substring(7, 2);

                        strSOUR_HSPOS_FK2 = strSOUR_HSPOS_FK1;
                        strUSE_FK = "1";
                        break;
                    }   
                }

                //@@ 크레인 번호가 다름. 확인해봐야 될 사항.
                // strDEST_LEV_FK1 = (Convert.ToInt32(strDEST_LEV_FK1) % 4).ToString();
                // if (strDEST_LEV_FK1 == "0")
                // strDEST_LEV_FK1 = "4";
                if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto ||
                    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RackSto ||
                    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                {
                    // S/C에 작업정보쓰기 UPDATE
                    if (UPDATE_SC_DATA("1",
                                       strLUGG_NO1,
                                       strSOUR_BANK_FK1,
                                       strSOUR_BAY_FK1,
                                       strSOUR_LEV_FK1,
                                       strDEST_HSPOS_FK1,
                                       strLUGG_NO2,
                                       strSOUR_BANK_FK2,
                                       strSOUR_BAY_FK2,
                                       strSOUR_LEV_FK2,
                                       strDEST_HSPOS_FK2,
                                       strUSE_FK,
                                       strUSER_COMMAND,
                                       strWRITE_FLAG,
                                       strWH_TYP,
                                       strDEST_POS1,
                                       strDEST_BANK_FK1,
                                       strDEST_BAY_FK1,
                                       strDEST_LEV_FK1,
                                       strSOUR_HSPOS_FK1,
                                       strDEST_BANK_FK2,
                                       strDEST_BAY_FK2,
                                       strDEST_LEV_FK2,
                                       strSOUR_HSPOS_FK2, 
                                       strSC_PLT_JOB_TYP,
                                       ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // 작업정보를 입고시작 상태로 변경
                    if (UPDATE_JOB_DATA("21", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG, "0", strCELL_NO1, strSTART_TRACK, strDEST_POS1, "0", strLUGG_NO2, "2") == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    if (strUSE_FK == "1")   // USE_FK = 0 (1번포크) USE_FK = 1 (2번포크) 
                    {
                        if (UPDATE_JOB_DATA("21", strLUGG_NO2, strWH_TYP, strJOB_TYP, ref pRTN_MSG, "0", strCELL_NO2, strHS_MC_NO2, strDEST_POS1, "0", strLUGG_NO1, "2") == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                        
                        if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                            strJobType = "호기간 이동 작업 중 ";

                        pRTN_MSG += "SC번호 : " + strDEST_POS1 + "에서 " + strSOUR_HSPOS_FK2 + " 입고 H/S에서 " + strJobType + "입고하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";
                        _pBdb.Commit();
                        return true;
                    }
                }

                if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                    strJobType = "호기간 이동 작업 중 ";

                pRTN_MSG += "SC번호 : " + strDEST_POS1 + "에서 " + strSOUR_HSPOS_FK1 + " 입고 H/S에서 " + strJobType + "입고하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";

                dtSC_HS_DEF.Dispose();
                _pBdb.Commit();
                InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), pRTN_MSG, "", "", strLUGG_NO1, "21", strSOUR_HSPOS_FK1, strDEST_POS1);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }


        // 이중입고 발생 시 새로운 로케이션 구한 후 S/C에 재지시
        // JOB_STATUS = '44' -> '21'
        public bool CHECK_SC_DUPLICATE_STORAGE(string strWH_TYP,
                                               string strAGING_TYP,
                                           ref string pRTN_MSG)
        {
            string strPLC_NO = "";

            try
            {
                string strSql;
                int nJobCnt = 0;

                pRTN_MSG = "CHECK_SC_DUPLICATE_STORAGE::";

                strSql = "";
                strSql += CRLF + " SELECT JM.*, SD.*                                          ";
                strSql += CRLF + "   FROM JOB_MST JM                                          ";
                strSql += CRLF + "  INNER JOIN SC_DATA SD                                     ";
                strSql += CRLF + "     ON JM.WH_TYP         = SD.WH_TYP                       ";
                strSql += CRLF + "    AND JM.DEST_POS       = SD.SC_NO                        ";
                strSql += CRLF + "    AND JM.LUGG_NO        = SD.LUGG_NO_FK1_RD               ";
                strSql += CRLF + "  WHERE JM.WH_TYP		    = :WH_TYP                         ";
                strSql += CRLF + "    AND JM.DEST_POS like '%' " + DbLang.II + " :AGING_TYP " + DbLang.II + " '%'           ";
                strSql += CRLF + "    AND SD.OD_RQ_YN       = 'N'                             ";
                strSql += CRLF + "    AND (SD.ITN_LUGG_FK1 <> '0' OR SD.ITN_LUGG_FK2 <> '0')  ";
                strSql += CRLF + "    AND SD.ONLINE_MODE_RD = '1'                             ";
                strSql += CRLF + "    AND SD.AUTO_MODE_RD   = '1'                             ";
                strSql += CRLF + "    AND (SD.ERR_CODE_RD   = '54' OR SD.ERR_CODE_RD = '0054')";
                strSql += CRLF + "    AND SD.ACTIVE_MODE_RD <> '1'                            ";   // DE ACTIVE
                strSql += CRLF + "    AND SD.UCSTATUS_RD    = '4'                             ";
                strSql += CRLF + "    AND SD.SENSOR_FK_RD   = '1'                             ";   // 화물있음.
                strSql += CRLF + "    AND JM.JOB_STATUS 	= '9'                             ";   // 이중입고 ERROR
                strSql += CRLF + "    AND JM.JOB_TYP 	    in ('1', '4', '5')                ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null                             ";
                strSql += CRLF + "  ORDER BY JM.INS_DT, SD.SC_NO                              ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("AGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                nJobCnt = _pBdb.ExcuteQry(strSql);
                if (nJobCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nJobCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strIS_TURN = "";
                string strSTART_POS = "";
                string strJOB_TYP = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";

                string strDEST_LOCATION = "00-000-00";
                string strSOUR_BANK_FK1 = "0";
                string strSOUR_BAY_FK1 = "0";
                string strSOUR_LEV_FK1 = "0";
                string strDEST_HSPOS_FK1 = "0";
                string strSOUR_BANK_FK2 = "0";
                string strSOUR_BAY_FK2 = "0";
                string strSOUR_LEV_FK2 = "0";
                string strDEST_HSPOS_FK2 = "0";
                string strUSE_FK = "0";
                string strUSER_COMMAND = "0";
                string strWRITE_FLAG = "0";
                string strSC_TYP = "0";

                string strDEST_BANK_FK1 = "0";
                string strDEST_BAY_FK1 = "0";
                string strDEST_LEV_FK1 = "0";
                string strSOUR_HSPOS_FK1 = "0";
                string strDEST_BANK_FK2 = "0";
                string strDEST_BAY_FK2 = "0";
                string strDEST_LEV_FK2 = "0";
                string strSOUR_HSPOS_FK2 = "0";

                string strDURATION_TIME = "";

                string strBOTTOM_TRAY1 = "";
                string strTOP_TRAY1 = "";

                _pBdb.BeginTrans();
                for (int i = 0; i < nJobCnt; i++)
                {
                    strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[0]["TRAY_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_TYP"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[0]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_POS"].ToString();
                    strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[0]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TURN"].ToString();
                    strLUGG_NO1 = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[0]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PLC_NO"].ToString();
                    strDURATION_TIME = "" + _pBdb.mDtMain.Rows[0]["DURATION_TIME"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DURATION_TIME"].ToString();

                    strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString();
                    strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString();

                    strSC_TYP = "" + _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString(); ;
                    //// 새로운 LOCATION 구해오기.
                    //if (SET_NEXT_LEV(strWH_TYP
                    //               , strDEST_POS1
                    //               , strPLC_NO
                    //               , strDURATION_TIME
                    //               , ref strAGING_TYP
                    //               , ref strDEST_LOCATION
                    //               , ref pRTN_MSG) == false)
                    //{
                    //    _pBdb.Rollback();
                    //    return false;
                    //}

                    if (strDEST_LOCATION.Length == 9)
                    {
                        //strDEST_BANK_FK1 = strDEST_LOCATION.Substring(0, 2);
                        strDEST_BANK_FK1 = Convert.ToString("" + (((Convert.ToInt32(0 + strDEST_LOCATION.Substring(0, 2)) + 1) % 2) + 1));
                        strDEST_BAY_FK1 = strDEST_LOCATION.Substring(3, 3);
                        strDEST_LEV_FK1 = strDEST_LOCATION.Substring(7, 2);
                    }

                    strSOUR_HSPOS_FK1 = _pBdb.mDtMain.Rows[0]["START_HSPOS_FK1_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_HSPOS_FK1_RD"].ToString();
                }

                // JOB_MST UPDATE
                // CELL_STA = '1' 
                if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto ||
                    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RackSto ||
                    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtoR)
                {
                    //// CELL예약.
                    //if (UPDATE_CELL_MST_DTL("1", strBOTTOM_TRAY1, strTOP_TRAY1, strWH_TYP, strDEST_POS1, strDEST_LOCATION, ref pRTN_MSG) == false)
                    //{
                    //    _pBdb.Rollback();
                    //    return false;
                    //}

                    // 작업정보를 입고시작 상태로 변경
                    if (UPDATE_JOB_DATA("21", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG, "0", strDEST_LOCATION, "0", strDEST_POS1, "0", "0") == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtoR)
                        strSOUR_HSPOS_FK1 = "1";

                    // S/C에 작업정보쓰기 UPDATE
                    // 이중입고 전용
                    if (UPDATE_SC_DATA_DUPLICATE("1",
                                                  strLUGG_NO1,
                                                  strSOUR_BANK_FK1,
                                                  strSOUR_BAY_FK1,
                                                  strSOUR_LEV_FK1,
                                                  strDEST_HSPOS_FK1,
                                                  "0",
                                                  strSOUR_BANK_FK2,
                                                  strSOUR_BAY_FK2,
                                                  strSOUR_LEV_FK2,
                                                  strDEST_HSPOS_FK2,
                                                  strUSE_FK,
                                                  strUSER_COMMAND,
                                                  strWRITE_FLAG,
                                                  strWH_TYP,
                                                  strDEST_POS1,
                                                  strDEST_BANK_FK1,
                                                  strDEST_BAY_FK1,
                                                  strDEST_LEV_FK1,
                                                  strSOUR_HSPOS_FK1,
                                                  strDEST_BANK_FK2,
                                                  strDEST_BAY_FK2,
                                                  strDEST_LEV_FK2,
                                                  strSOUR_HSPOS_FK2,
                                              ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                }

                pRTN_MSG += "SC번호 : " + strPLC_NO + "에서 이중 입고 재지정 처리하도록 SC_TASK를 통해서 SC에 지시 요청하였습니다. [작업번호:" + strLUGG_NO1 + "]";

                _pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }


        // C/V에 목적지정보를 적는다.
        public bool UPDATE_CV_DATA_FOR_RET_ING(string strLUGG_NO,
                                               string strWH_TYP,
                                               string strPLC_NO,
                                               string strRET_HS_TR_NO,
                                           ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "UPDATE_CV_DATA_FOR_RET_ING::";

                strSql = "";
                strSql += CRLF + " UPDATE CV_DATA                                       ";
                strSql += CRLF + "    SET WAIT_SC_RET_JOB_OD    = '1'                   ";
                strSql += CRLF + "      , CMD_RQ_YN             = 'Y'                   ";
                strSql += CRLF + "      , CMD_RQ_ID             = '2'                   ";  // 1:PULP 단수, 2:출고중(대기필요), 3:Track Pause
                strSql += CRLF + "      , OD_USER_ID            = 'IOTASK'              ";
                strSql += CRLF + "      , OD_UPD_DT             = " + DbLang.SYSDATE;
                strSql += CRLF + "  WHERE WH_TYP                = :WH_TYP               ";
                strSql += CRLF + "    AND MC_NO                 = :MC_NO                ";
                strSql += CRLF + "    AND CMD_RQ_YN             = 'N'                   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strRET_HS_TR_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "변경할 CV_DATA 정보가 존재하지 않습니다. TRACK_NO : " + strRET_HS_TR_NO;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }
        // S/C에 작업쓰기.
        public bool UPDATE_SC_DATA(string strJOB_TYP,
                                   string strLUGG_NO_FK1,
                                   string strSOUR_BANK_FK1,
                                   string strSOUR_BAY_FK1,
                                   string strSOUR_LEV_FK1,
                                   string strDEST_HSPOS_FK1,
                                   string strLUGG_NO_FK2,
                                   string strSOUR_BANK_FK2,
                                   string strSOUR_BAY_FK2,
                                   string strSOUR_LEV_FK2,
                                   string strDEST_HSPOS_FK2,
                                   string strUSE_FK,
                                   string strUSER_COMMAND,
                                   string strWRITE_FLAG,
                                   string strWH_TYP,
                                   string strSC_NO,
                                   string strDEST_BANK_FK1,
                                   string strDEST_BAY_FK1,
                                   string strDEST_LEV_FK1,
                                   string strSTART_HSPOS_FK1,        
                                   string strDEST_BANK_FK2,
                                   string strDEST_BAY_FK2,
                                   string strDEST_LEV_FK2,
                                   string strSTART_HSPOS_FK2,
                                   string strSC_PLT_JOB_TYP
                                 , ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                pRTN_MSG += "UPDATE_SC_DATA::";

                // 작업전 초기화.
                if (UPDATE_SC_OD_RESET(strWH_TYP, strSC_NO, ref pRTN_MSG) == false)
                    return false;


                // ----- 상온 2 -----
                #region CELL_MST 사용 안함!
                /*
                // 충방전여부를 판단해서 충방전이 아니면, CELL정보를 검색 20191002 SMLEE (주석처리)
                // S/C LEVEL 구하기.
                // 입고 및 RACK TO RACK
                if (Convert.ToInt32(strSC_NO) < 29700)
                {
                    if (strJOB_TYP == "1" || strJOB_TYP == "4")
                    {
                        // S/C에 작업정보 쓰기전, LEVEL별 LOCATION 쓰기 [ex) 01-001-05 -> 29121 01-001-01] / CELL_MST SC_LEV 추가.
                        strSql = "";
                        strSql += CRLF + " SELECT *                         ";
                        strSql += CRLF + "   FROM CELL_MST                  ";
                        strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP      ";
                        strSql += CRLF + "    AND CELL_SC_NO = :CELL_SC_NO  ";
                        strSql += CRLF + "    AND BANK       = :BANK        ";
                        strSql += CRLF + "    AND BAY        = :BAY         ";
                        strSql += CRLF + "    AND LEV        = :LEV         ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                        _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strDEST_BANK_FK1;
                        _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                        _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                        nSelCnt = _pBdb.ExcuteQry(strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            return false;
                        }
                        if (nSelCnt == 0)
                        {
                            pRTN_MSG += "조회할 CELL MST 정보가 존재하지 않습니다. CELL_NO : " + strDEST_BANK_FK1 + strDEST_BAY_FK1 + strDEST_LEV_FK1;
                            return false;
                        }
                        strDEST_LEV_FK1 = _pBdb.mDtMain.Rows[0]["SC_LEV"].ToString();

                        // 2번포크에 작업존재시.
                        if (strLUGG_NO_FK2 != "" && strLUGG_NO_FK2 != "0")
                        {
                            // S/C에 작업정보 쓰기전, LEVEL별 LOCATION 쓰기 [ex) 01-001-05 -> 29121 01-001-01] / CELL_MST SC_LEV 추가.
                            strSql = "";
                            strSql += CRLF + " SELECT *                         ";
                            strSql += CRLF + "   FROM CELL_MST                  ";
                            strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP      ";
                            strSql += CRLF + "    AND CELL_SC_NO = :CELL_SC_NO  ";
                            strSql += CRLF + "    AND BANK       = :BANK        ";
                            strSql += CRLF + "    AND BAY        = :BAY         ";
                            strSql += CRLF + "    AND LEV        = :LEV         ";
                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                            _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strDEST_BANK_FK2;
                            _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strDEST_BAY_FK2;
                            _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strDEST_LEV_FK2;
                            nSelCnt = _pBdb.ExcuteQry(strSql);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                return false;
                            }
                            if (nSelCnt == 0)
                            {
                                pRTN_MSG += "조회할 CELL MST 정보가 존재하지 않습니다. CELL_NO : " + strDEST_BANK_FK2 + strDEST_BAY_FK2 + strDEST_LEV_FK2;
                                return false;
                            }
                            strDEST_LEV_FK2 = _pBdb.mDtMain.Rows[0]["SC_LEV"].ToString();
                        }
                    }

                    // 출고 및 RACK TO RACK 등
                    if (strJOB_TYP == "2" || strJOB_TYP == "5" || strJOB_TYP == "7" || strJOB_TYP == "4")
                    {
                        // S/C에 작업정보 쓰기전, LEVEL별 LOCATION 쓰기 [ex) 01-001-05 -> 29121 01-001-01] / CELL_MST SC_LEV 추가.
                        strSql = "";
                        strSql += CRLF + " SELECT *                         ";
                        strSql += CRLF + "   FROM CELL_MST                  ";
                        strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP      ";
                        strSql += CRLF + "    AND CELL_SC_NO = :CELL_SC_NO  ";
                        strSql += CRLF + "    AND BANK       = :BANK        ";
                        strSql += CRLF + "    AND BAY        = :BAY         ";
                        strSql += CRLF + "    AND LEV        = :LEV         ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                        _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strSOUR_BANK_FK1;
                        _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                        _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                        nSelCnt = _pBdb.ExcuteQry(strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            return false;
                        }
                        if (nSelCnt == 0)
                        {
                            pRTN_MSG += "조회할 CELL MST 정보가 존재하지 않습니다. CELL_NO : " + strSOUR_BANK_FK1 + strSOUR_BAY_FK1 + strSOUR_LEV_FK1;
                            return false;
                        }
                        strSOUR_LEV_FK1 = _pBdb.mDtMain.Rows[0]["SC_LEV"].ToString();

                        // 2번포크에 작업존재시.
                        if (strLUGG_NO_FK2 != "" && strLUGG_NO_FK2 != "0")
                        {
                            // S/C에 작업정보 쓰기전, LEVEL별 LOCATION 쓰기 [ex) 01-001-05 -> 29121 01-001-01] / CELL_MST SC_LEV 추가.
                            strSql = "";
                            strSql += CRLF + " SELECT *                         ";
                            strSql += CRLF + "   FROM CELL_MST                  ";
                            strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP      ";
                            strSql += CRLF + "    AND CELL_SC_NO = :CELL_SC_NO  ";
                            strSql += CRLF + "    AND BANK       = :BANK        ";
                            strSql += CRLF + "    AND BAY        = :BAY         ";
                            strSql += CRLF + "    AND LEV        = :LEV         ";
                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                            _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strSOUR_BANK_FK2;
                            _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strSOUR_BAY_FK2;
                            _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strSOUR_LEV_FK2;
                            nSelCnt = _pBdb.ExcuteQry(strSql);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                return false;
                            }
                            if (nSelCnt == 0)
                            {
                                pRTN_MSG += "조회할 CELL MST 정보가 존재하지 않습니다. CELL_NO : " + strSOUR_BANK_FK2 + strSOUR_BAY_FK2 + strSOUR_LEV_FK2;
                                return false;
                            }
                            strSOUR_LEV_FK2 = _pBdb.mDtMain.Rows[0]["SC_LEV"].ToString();
                        }
                    }
                }
                //*/
                #endregion


                //strSOUR_BANK_FK1 = cDefApi.GetBankWH(cDefApp.eWHTYP.SKI_WH01, strSOUR_BANK_FK1);
                //strSOUR_BANK_FK2 = cDefApi.GetBankWH(cDefApp.eWHTYP.SKI_WH01, strSOUR_BANK_FK2);
                //strDEST_BANK_FK1 = cDefApi.GetBankWH(cDefApp.eWHTYP.SKI_WH01, strDEST_BANK_FK1);
                //strDEST_BANK_FK2 = cDefApi.GetBankWH(cDefApp.eWHTYP.SKI_WH01, strDEST_BANK_FK2);

                // 입고
                if (strJOB_TYP == "1")
                {
                    // S/C에 작업정보쓰기.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                                   ";
                    strSql += CRLF + "    SET JOB_TYP_OD          =  :JOB_TYP_OD        ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD      =  :LUGG_NO_FK1_OD    ";
                    strSql += CRLF + "      , DEST_BANK_FK1_OD    =  :DEST_BANK_FK1_OD  ";
                    strSql += CRLF + "      , DEST_BAY_FK1_OD     =  :DEST_BAY_FK1_OD   ";
                    strSql += CRLF + "      , DEST_LEVEL_FK1_OD   =  :DEST_LEVEL_FK1_OD ";
                    strSql += CRLF + "      , START_HSPOS_FK1_OD  =  :START_HSPOS_FK1_OD";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD      =  :LUGG_NO_FK2_OD    ";
                    strSql += CRLF + "      , DEST_BANK_FK2_OD    =  :DEST_BANK_FK2_OD  ";
                    strSql += CRLF + "      , DEST_BAY_FK2_OD     =  :DEST_BAY_FK2_OD   ";
                    strSql += CRLF + "      , DEST_LEVEL_FK2_OD   =  :DEST_LEVEL_FK2_OD ";
                    strSql += CRLF + "      , START_HSPOS_FK2_OD  =  :START_HSPOS_FK2_OD";
                    strSql += CRLF + "      , USE_FK_OD           =  :USE_FK_OD         ";
                    strSql += CRLF + "      , USER_COMMAND_OD     =  :USER_COMMAND_OD   ";            //D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD       =  :WRITE_FLAG_OD     ";            //D191
                    strSql += CRLF + "      , SC_PLT_JOB_TYP_OD   =  :SC_PLT_JOB_TYP_OD ";            //D96
                    strSql += CRLF + "      , OD_RQ_YN            =  'Y'                ";
                    strSql += CRLF + "      , OD_USER_ID          =  'IOTASK'           ";
                    strSql += CRLF + "      , OD_UPD_DT           =  " + DbLang.SYSDATE + "              ";
                    strSql += CRLF + "  WHERE WH_TYP              = :WH_TYP             ";
                    strSql += CRLF + "    AND SC_NO               = :SC_NO              ";
                    strSql += CRLF + "    AND OD_RQ_YN            = 'N'                 ";
                    strSql += CRLF + "    AND ERR_CODE_RD         = '0000'              ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_BANK_FK1_OD", DbLang.VARCHAR).Value = strDEST_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_BAY_FK1_OD", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK1_OD", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("START_HSPOS_FK1_OD", DbLang.VARCHAR).Value = strSTART_HSPOS_FK1;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK2_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_BANK_FK2_OD", DbLang.VARCHAR).Value = strDEST_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_BAY_FK2_OD", DbLang.VARCHAR).Value = strDEST_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK2_OD", DbLang.VARCHAR).Value = strDEST_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("START_HSPOS_FK2_OD", DbLang.VARCHAR).Value = strSTART_HSPOS_FK2;
                    _pBdb.mComMain.Parameters.Add("USE_FK_OD", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("USER_COMMAND_OD", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("WRITE_FLAG_OD", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("SC_PLT_JOB_TYP_OD", DbLang.VARCHAR).Value = strSC_PLT_JOB_TYP;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                    
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);

                }

                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.Ret).ToString()     ||
                    strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString() ||
                    strJOB_TYP == ((int)cDefApp.eJOBTYP.OtherRet).ToString() ||
                    strJOB_TYP == ((int)cDefApp.eJOBTYP.Aisle2Aisle).ToString()) //조한성 추가 - 호기간이동
                {
                    // 출고,호기간이동
                    // S/C에 작업정보쓰기.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                                       ";
                    strSql += CRLF + "    SET JOB_TYP_OD         =  :JOB_TYP_OD             ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD     =  :LUGG_NO_FK1_OD         ";
                    strSql += CRLF + "      , START_BANK_FK1_OD  =  :START_BANK_FK1_OD      ";
                    strSql += CRLF + "      , START_BAY_FK1_OD   =  :START_BAY_FK1_OD       ";
                    strSql += CRLF + "      , START_LEVEL_FK1_OD =  :START_LEVEL_FK1_OD     ";
                    strSql += CRLF + "      , DEST_HSPOS_FK1_OD  =  :DEST_HSPOS_FK1_OD      ";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD     =  :LUGG_NO_FK2_OD         ";
                    strSql += CRLF + "      , START_BANK_FK2_OD  =  :START_BANK_FK2_OD      ";
                    strSql += CRLF + "      , START_BAY_FK2_OD   =  :START_BAY_FK2_OD       ";
                    strSql += CRLF + "      , START_LEVEL_FK2_OD =  :START_LEVEL_FK2_OD     ";
                    strSql += CRLF + "      , DEST_HSPOS_FK2_OD  =  :DEST_HSPOS_FK2_OD      ";
                    strSql += CRLF + "      , USE_FK_OD          =  :USE_FK_OD              ";
                    strSql += CRLF + "      , USER_COMMAND_OD    =  :USER_COMMAND_OD        ";            //D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD      =  :WRITE_FLAG_OD          ";            //D191
                    strSql += CRLF + "      , SC_PLT_JOB_TYP_OD   =  :SC_PLT_JOB_TYP_OD     ";            //D96
                    strSql += CRLF + "      , OD_RQ_YN           =  'Y'                     ";
                    strSql += CRLF + "      , OD_USER_ID         =  'IOTASK'                ";
                    strSql += CRLF + "      , OD_UPD_DT          =  " + DbLang.SYSDATE + "  ";
                    strSql += CRLF + "  WHERE WH_TYP             = :WH_TYP                  ";
                    strSql += CRLF + "    AND SC_NO              = :SC_NO                   ";
                    strSql += CRLF + "    AND OD_RQ_YN           = 'N'                      ";
                    strSql += CRLF + "    AND ERR_CODE_RD        = '0000'                   ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("START_BANK_FK1_OD", DbLang.VARCHAR).Value = strSOUR_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("START_BAY_FK1_OD", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("START_LEVEL_FK1_OD", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_HSPOS_FK1_OD", DbLang.VARCHAR).Value = strDEST_HSPOS_FK1;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK2_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("START_BANK_FK2_OD", DbLang.VARCHAR).Value = strSOUR_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("START_BAY_FK2_OD", DbLang.VARCHAR).Value = strSOUR_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("START_LEVEL_FK2_OD", DbLang.VARCHAR).Value = strSOUR_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_HSPOS_FK2_OD", DbLang.VARCHAR).Value = strDEST_HSPOS_FK2;
                    _pBdb.mComMain.Parameters.Add("USE_FK_OD", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("USER_COMMAND_OD", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("WRITE_FLAG_OD", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("SC_PLT_JOB_TYP_OD", DbLang.VARCHAR).Value = strSC_PLT_JOB_TYP;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                }

                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.RtoR).ToString())
                {
                    // RACK TO RACK (RACK내)
                    // S/C에 작업정보쓰기.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                                       ";
                    strSql += CRLF + "    SET JOB_TYP_OD          =  :JOB_TYP_OD            ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD      =  :LUGG_NO_FK1_OD        ";
                    strSql += CRLF + "      , START_BANK_FK1_OD   =  :START_BANK_FK1_OD     ";
                    strSql += CRLF + "      , START_BAY_FK1_OD    =  :START_BAY_FK1_OD      ";
                    strSql += CRLF + "      , START_LEVEL_FK1_OD  =  :START_LEVEL_FK1_OD    ";
                    strSql += CRLF + "      , DEST_BANK_FK1_OD    =  :DEST_BANK_FK1_OD      ";
                    strSql += CRLF + "      , DEST_BAY_FK1_OD     =  :DEST_BAY_FK1_OD       ";
                    strSql += CRLF + "      , DEST_LEVEL_FK1_OD   =  :DEST_LEVEL_FK1_OD     ";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD      =  :LUGG_NO_FK2_OD        ";
                    strSql += CRLF + "      , START_BANK_FK2_OD   =  :START_BANK_FK2_OD     ";
                    strSql += CRLF + "      , START_BAY_FK2_OD    =  :START_BAY_FK2_OD      ";
                    strSql += CRLF + "      , START_LEVEL_FK2_OD  =  :START_LEVEL_FK2_OD    ";
                    strSql += CRLF + "      , DEST_BANK_FK2_OD    =  :DEST_BANK_FK2_OD      ";
                    strSql += CRLF + "      , DEST_BAY_FK2_OD     =  :DEST_BAY_FK2_OD       ";
                    strSql += CRLF + "      , DEST_LEVEL_FK2_OD   =  :DEST_LEVEL_FK2_OD     ";
                    strSql += CRLF + "      , USE_FK_OD           =  :USE_FK_OD             ";
                    strSql += CRLF + "      , USER_COMMAND_OD     =  :USER_COMMAND_OD       ";            //D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD       =  :WRITE_FLAG_OD         ";            //D191
                    strSql += CRLF + "      , SC_PLT_JOB_TYP_OD   =  :SC_PLT_JOB_TYP_OD     ";            //D96
                    strSql += CRLF + "      , OD_RQ_YN            =  'Y'                    ";
                    strSql += CRLF + "      , OD_USER_ID          =  'IOTASK'               ";
                    strSql += CRLF + "      , OD_UPD_DT           =  " + DbLang.SYSDATE + "                  ";
                    strSql += CRLF + "  WHERE WH_TYP              = :WH_TYP                 ";
                    strSql += CRLF + "    AND SC_NO               = :SC_NO                  ";
                    strSql += CRLF + "    AND OD_RQ_YN            = 'N'                     ";
                    strSql += CRLF + "    AND ERR_CODE_RD         = '0000'                  ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("START_BANK_FK1_OD", DbLang.VARCHAR).Value = strSOUR_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("START_BAY_FK1_OD", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("START_LEVEL_FK1_OD", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_BANK_FK1_OD", DbLang.VARCHAR).Value = strDEST_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_BAY_FK1_OD", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK1_OD", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK2_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("START_BANK_FK2_OD", DbLang.VARCHAR).Value = strSOUR_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("START_BAY_FK2_OD", DbLang.VARCHAR).Value = strSOUR_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("START_LEVEL_FK2_OD", DbLang.VARCHAR).Value = strSOUR_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_BANK_FK2_OD", DbLang.VARCHAR).Value = strDEST_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_BAY_FK2_OD", DbLang.VARCHAR).Value = strDEST_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK2_OD", DbLang.VARCHAR).Value = strDEST_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("USE_FK_OD", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("USER_COMMAND_OD", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("WRITE_FLAG_OD", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("SC_PLT_JOB_TYP_OD", DbLang.VARCHAR).Value = strSC_PLT_JOB_TYP;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                }

                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "작업처리실패.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }




        // S/C에 작업쓰기.
        // 이중입고 전용
        public bool UPDATE_SC_DATA_DUPLICATE(string strJOB_TYP,
                                             string strLUGG_NO_FK1,
                                             string strSOUR_BANK_FK1,
                                             string strSOUR_BAY_FK1,
                                             string strSOUR_LEV_FK1,
                                             string strDEST_HSPOS_FK1,
                                             string strLUGG_NO_FK2,
                                             string strSOUR_BANK_FK2,
                                             string strSOUR_BAY_FK2,
                                             string strSOUR_LEV_FK2,
                                             string strDEST_HSPOS_FK2,
                                             string strUSE_FK,
                                             string strUSER_COMMAND,
                                             string strWRITE_FLAG,
                                             string strWH_TYP,
                                             string strSC_NO,
                                             string strDEST_BANK_FK1,
                                             string strDEST_BAY_FK1,
                                             string strDEST_LEV_FK1,
                                             string strSTART_HSPOS_FK1,
                                             string strDEST_BANK_FK2,
                                             string strDEST_BAY_FK2,
                                             string strDEST_LEV_FK2,
                                             string strSTART_HSPOS_FK2,
                                         ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                pRTN_MSG += "UPDATE_SC_DATA_DUPLICATE::";

                // 작업전 초기화.
                if (UPDATE_SC_OD_RESET(strWH_TYP, strSC_NO, ref pRTN_MSG) == false)
                    return false;

                //// 충방전여부를 판단해서 충방전이 아니면, CELL정보를 검색 20191002 SMLEE (주석처리)
                //// S/C LEVEL 구하기.
                //// 입고 및 RACK TO RACK
                //if (Convert.ToInt32(strSC_NO) < 29700)
                //{
                //    if (strJOB_TYP == "1" || strJOB_TYP == "4")
                //    {
                //        // S/C에 작업정보 쓰기전, LEVEL별 LOCATION 쓰기 [ex) 01-001-05 -> 29121 01-001-01] / CELL_MST SC_LEV 추가.
                //        strSql = "";
                //        strSql += CRLF + " SELECT *                         ";
                //        strSql += CRLF + "   FROM CELL_MST                  ";
                //        strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP      ";
                //        strSql += CRLF + "    AND CELL_SC_NO = :CELL_SC_NO  ";
                //        strSql += CRLF + "    AND BANK       = :BANK        ";
                //        strSql += CRLF + "    AND BAY        = :BAY         ";
                //        strSql += CRLF + "    AND LEV        = :LEV         ";
                //        _pBdb.mComMain.CommandType = CommandType.Text;
                //        _pBdb.mComMain.Parameters.Clear();
                //        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                //        _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                //        _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strDEST_BANK_FK1;
                //        _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                //        _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                //        nSelCnt = _pBdb.ExcuteQry(strSql);
                //        if (nSelCnt < 0)
                //        {
                //            pRTN_MSG += _pBdb.ErrMsg;
                //            return false;
                //        }
                //        if (nSelCnt == 0)
                //        {
                //            pRTN_MSG += "조회할 CELL MST 정보가 존재하지 않습니다. CELL_NO : " + strDEST_BANK_FK1 + strDEST_BAY_FK1 + strDEST_LEV_FK1;
                //            return false;
                //        }
                //        strDEST_LEV_FK1 = _pBdb.mDtMain.Rows[0]["SC_LEV"].ToString();

                //        // 2번포크에 작업존재시.
                //        if (strLUGG_NO_FK2 != "" && strLUGG_NO_FK2 != "0")
                //        {
                //            // S/C에 작업정보 쓰기전, LEVEL별 LOCATION 쓰기 [ex) 01-001-05 -> 29121 01-001-01] / CELL_MST SC_LEV 추가.
                //            strSql = "";
                //            strSql += CRLF + " SELECT *                         ";
                //            strSql += CRLF + "   FROM CELL_MST                  ";
                //            strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP      ";
                //            strSql += CRLF + "    AND CELL_SC_NO = :CELL_SC_NO  ";
                //            strSql += CRLF + "    AND BANK       = :BANK        ";
                //            strSql += CRLF + "    AND BAY        = :BAY         ";
                //            strSql += CRLF + "    AND LEV        = :LEV         ";
                //            _pBdb.mComMain.CommandType = CommandType.Text;
                //            _pBdb.mComMain.Parameters.Clear();
                //            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                //            _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                //            _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strDEST_BANK_FK2;
                //            _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strDEST_BAY_FK2;
                //            _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strDEST_LEV_FK2;
                //            nSelCnt = _pBdb.ExcuteQry(strSql);
                //            if (nSelCnt < 0)
                //            {
                //                pRTN_MSG += _pBdb.ErrMsg;
                //                return false;
                //            }
                //            if (nSelCnt == 0)
                //            {
                //                pRTN_MSG += "조회할 CELL MST 정보가 존재하지 않습니다. CELL_NO : " + strDEST_BANK_FK2 + strDEST_BAY_FK2 + strDEST_LEV_FK2;
                //                return false;
                //            }
                //            strDEST_LEV_FK2 = _pBdb.mDtMain.Rows[0]["SC_LEV"].ToString();
                //        }
                //    }

                //    // 출고 및 RACK TO RACK 등
                //    if (strJOB_TYP == "2" || strJOB_TYP == "5" || strJOB_TYP == "7" || strJOB_TYP == "4")
                //    {
                //        // S/C에 작업정보 쓰기전, LEVEL별 LOCATION 쓰기 [ex) 01-001-05 -> 29121 01-001-01] / CELL_MST SC_LEV 추가.
                //        strSql = "";
                //        strSql += CRLF + " SELECT *                         ";
                //        strSql += CRLF + "   FROM CELL_MST                  ";
                //        strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP      ";
                //        strSql += CRLF + "    AND CELL_SC_NO = :CELL_SC_NO  ";
                //        strSql += CRLF + "    AND BANK       = :BANK        ";
                //        strSql += CRLF + "    AND BAY        = :BAY         ";
                //        strSql += CRLF + "    AND LEV        = :LEV         ";
                //        _pBdb.mComMain.CommandType = CommandType.Text;
                //        _pBdb.mComMain.Parameters.Clear();
                //        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                //        _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                //        _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strSOUR_BANK_FK1;
                //        _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                //        _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                //        nSelCnt = _pBdb.ExcuteQry(strSql);
                //        if (nSelCnt < 0)
                //        {
                //            pRTN_MSG += _pBdb.ErrMsg;
                //            return false;
                //        }
                //        if (nSelCnt == 0)
                //        {
                //            pRTN_MSG += "조회할 CELL MST 정보가 존재하지 않습니다. CELL_NO : " + strSOUR_BANK_FK1 + strSOUR_BAY_FK1 + strSOUR_LEV_FK1;
                //            return false;
                //        }
                //        strSOUR_LEV_FK1 = _pBdb.mDtMain.Rows[0]["SC_LEV"].ToString();

                //        // 2번포크에 작업존재시.
                //        if (strLUGG_NO_FK2 != "" && strLUGG_NO_FK2 != "0")
                //        {
                //            // S/C에 작업정보 쓰기전, LEVEL별 LOCATION 쓰기 [ex) 01-001-05 -> 29121 01-001-01] / CELL_MST SC_LEV 추가.
                //            strSql = "";
                //            strSql += CRLF + " SELECT *                         ";
                //            strSql += CRLF + "   FROM CELL_MST                  ";
                //            strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP      ";
                //            strSql += CRLF + "    AND CELL_SC_NO = :CELL_SC_NO  ";
                //            strSql += CRLF + "    AND BANK       = :BANK        ";
                //            strSql += CRLF + "    AND BAY        = :BAY         ";
                //            strSql += CRLF + "    AND LEV        = :LEV         ";
                //            _pBdb.mComMain.CommandType = CommandType.Text;
                //            _pBdb.mComMain.Parameters.Clear();
                //            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                //            _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                //            _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strSOUR_BANK_FK2;
                //            _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strSOUR_BAY_FK2;
                //            _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strSOUR_LEV_FK2;
                //            nSelCnt = _pBdb.ExcuteQry(strSql);
                //            if (nSelCnt < 0)
                //            {
                //                pRTN_MSG += _pBdb.ErrMsg;
                //                return false;
                //            }
                //            if (nSelCnt == 0)
                //            {
                //                pRTN_MSG += "조회할 CELL MST 정보가 존재하지 않습니다. CELL_NO : " + strSOUR_BANK_FK2 + strSOUR_BAY_FK2 + strSOUR_LEV_FK2;
                //                return false;
                //            }
                //            strSOUR_LEV_FK2 = _pBdb.mDtMain.Rows[0]["SC_LEV"].ToString();
                //        }
                //    }
                //}

                strSOUR_BANK_FK1 = cDefApi.GetBankWH(cDefApp.eWHTYP.SKI_WH01, strSOUR_BANK_FK1);
                strSOUR_BANK_FK2 = cDefApi.GetBankWH(cDefApp.eWHTYP.SKI_WH01, strSOUR_BANK_FK2);
                strDEST_BANK_FK1 = cDefApi.GetBankWH(cDefApp.eWHTYP.SKI_WH01, strDEST_BANK_FK1);
                strDEST_BANK_FK2 = cDefApi.GetBankWH(cDefApp.eWHTYP.SKI_WH01, strDEST_BANK_FK2);

                // 입고
                if (strJOB_TYP == "1")
                {
                    // S/C에 작업정보쓰기.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                                   ";
                    strSql += CRLF + "    SET JOB_TYP_OD          =  :JOB_TYP_OD        ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD      =  :LUGG_NO_FK1_OD    ";
                    strSql += CRLF + "      , DEST_BANK_FK1_OD    =  :DEST_BANK_FK1_OD  ";
                    strSql += CRLF + "      , DEST_BAY_FK1_OD     =  :DEST_BAY_FK1_OD   ";
                    strSql += CRLF + "      , DEST_LEVEL_FK1_OD   =  :DEST_LEVEL_FK1_OD ";
                    strSql += CRLF + "      , START_HSPOS_FK1_OD  =  :START_HSPOS_FK1_OD";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD      =  :LUGG_NO_FK2_OD    ";
                    strSql += CRLF + "      , DEST_BANK_FK2_OD    =  :DEST_BANK_FK2_OD  ";
                    strSql += CRLF + "      , DEST_BAY_FK2_OD     =  :DEST_BAY_FK2_OD   ";
                    strSql += CRLF + "      , DEST_LEVEL_FK2_OD   =  :DEST_LEVEL_FK2_OD ";
                    strSql += CRLF + "      , START_HSPOS_FK2_OD  =  :START_HSPOS_FK2_OD";
                    strSql += CRLF + "      , USE_FK_OD           =  :USE_FK_OD         ";
                    strSql += CRLF + "      , USER_COMMAND_OD     =  :USER_COMMAND_OD   "; //D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD       =  :WRITE_FLAG_OD     "; //D191
                    strSql += CRLF + "      , OD_RQ_YN            =  'Y'                ";
                    strSql += CRLF + "      , OD_USER_ID          =  'IOTASK'           ";
                    strSql += CRLF + "      , OD_UPD_DT           =  " + DbLang.SYSDATE + "              ";
                    strSql += CRLF + "  WHERE WH_TYP              =  :WH_TYP            ";
                    strSql += CRLF + "    AND SC_NO               =  :SC_NO             ";
                    strSql += CRLF + "    AND OD_RQ_YN            =  'N'                ";
                    strSql += CRLF + "    AND ERR_CODE_RD         <> '0'                ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_BANK_FK1_OD", DbLang.VARCHAR).Value = strDEST_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_BAY_FK1_OD", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK1_OD", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("START_HSPOS_FK1_OD", DbLang.VARCHAR).Value = strSTART_HSPOS_FK1;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK2_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_BANK_FK2_OD", DbLang.VARCHAR).Value = strDEST_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_BAY_FK2_OD", DbLang.VARCHAR).Value = strDEST_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK2_OD", DbLang.VARCHAR).Value = strDEST_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("START_HSPOS_FK2_OD", DbLang.VARCHAR).Value = strSTART_HSPOS_FK2;
                    _pBdb.mComMain.Parameters.Add("USE_FK_OD", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("USER_COMMAND_OD", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("WRITE_FLAG_OD", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);

                }

                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.Ret).ToString() ||
                    strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString() ||
                    strJOB_TYP == ((int)cDefApp.eJOBTYP.OtherRet).ToString())
                {
                    // 출고
                    // S/C에 작업정보쓰기.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                                  ";
                    strSql += CRLF + "    SET JOB_TYP_OD         =  :JOB_TYP_OD        ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD     =  :LUGG_NO_FK1_OD    ";
                    strSql += CRLF + "      , START_BANK_FK1_OD  =  :START_BANK_FK1_OD ";
                    strSql += CRLF + "      , START_BAY_FK1_OD   =  :START_BAY_FK1_OD  ";
                    strSql += CRLF + "      , START_LEVEL_FK1_OD =  :START_LEVEL_FK1_OD";
                    strSql += CRLF + "      , DEST_HSPOS_FK1_OD  =  :DEST_HSPOS_FK1_OD ";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD     =  :LUGG_NO_FK2_OD    ";
                    strSql += CRLF + "      , START_BANK_FK2_OD  =  :START_BANK_FK2_OD ";
                    strSql += CRLF + "      , START_BAY_FK2_OD   =  :START_BAY_FK2_OD  ";
                    strSql += CRLF + "      , START_LEVEL_FK2_OD =  :START_LEVEL_FK2_OD";
                    strSql += CRLF + "      , DEST_HSPOS_FK2_OD  =  :DEST_HSPOS_FK2_OD ";
                    strSql += CRLF + "      , USE_FK_OD          =  :USE_FK_OD         ";
                    strSql += CRLF + "      , USER_COMMAND_OD    =  :USER_COMMAND_OD   ";            //D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD      =  :WRITE_FLAG_OD     ";            //D191
                    strSql += CRLF + "      , OD_RQ_YN           =  'Y'                ";
                    strSql += CRLF + "      , OD_USER_ID         =  'IOTASK'           ";
                    strSql += CRLF + "      , OD_UPD_DT          =  " + DbLang.SYSDATE + "              ";
                    strSql += CRLF + "  WHERE WH_TYP             =  :WH_TYP            ";
                    strSql += CRLF + "    AND SC_NO              =  :SC_NO             ";
                    strSql += CRLF + "    AND OD_RQ_YN           =  'N'                ";
                    strSql += CRLF + "    AND ERR_CODE_RD        <> '0'                ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("START_BANK_FK1_OD", DbLang.VARCHAR).Value = strSOUR_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("START_BAY_FK1_OD", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("START_LEVEL_FK1_OD", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_HSPOS_FK1_OD", DbLang.VARCHAR).Value = strDEST_HSPOS_FK1;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK2_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("START_BANK_FK2_OD", DbLang.VARCHAR).Value = strSOUR_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("START_BAY_FK2_OD", DbLang.VARCHAR).Value = strSOUR_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("START_LEVEL_FK2_OD", DbLang.VARCHAR).Value = strSOUR_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_HSPOS_FK2_OD", DbLang.VARCHAR).Value = strDEST_HSPOS_FK2;
                    _pBdb.mComMain.Parameters.Add("USE_FK_OD", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("USER_COMMAND_OD", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("WRITE_FLAG_OD", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                }

                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.RtoR).ToString())
                {
                    // RACK TO RACK (RACK내)
                    // S/C에 작업정보쓰기.
                    strSql = "";    
                    strSql += CRLF + " UPDATE SC_DATA                                   ";
                    strSql += CRLF + "    SET JOB_TYP_OD          =  :JOB_TYP_OD        ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD      =  :LUGG_NO_FK1_OD    ";
                    strSql += CRLF + "      , START_BANK_FK1_OD   =  :START_BANK_FK1_OD ";
                    strSql += CRLF + "      , START_BAY_FK1_OD    =  :START_BAY_FK1_OD  ";
                    strSql += CRLF + "      , START_LEVEL_FK1_OD  =  :START_LEVEL_FK1_OD";
                    strSql += CRLF + "      , DEST_BANK_FK1_OD    =  :DEST_BANK_FK1_OD  ";
                    strSql += CRLF + "      , DEST_BAY_FK1_OD     =  :DEST_BAY_FK1_OD   ";
                    strSql += CRLF + "      , DEST_LEVEL_FK1_OD   =  :DEST_LEVEL_FK1_OD ";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD      =  :LUGG_NO_FK2_OD    ";
                    strSql += CRLF + "      , START_BANK_FK2_OD   =  :START_BANK_FK2_OD ";
                    strSql += CRLF + "      , START_BAY_FK2_OD    =  :START_BAY_FK2_OD  ";
                    strSql += CRLF + "      , START_LEVEL_FK2_OD  =  :START_LEVEL_FK2_OD";
                    strSql += CRLF + "      , DEST_BANK_FK2_OD    =  :DEST_BANK_FK2_OD  ";
                    strSql += CRLF + "      , DEST_BAY_FK2_OD     =  :DEST_BAY_FK2_OD   ";
                    strSql += CRLF + "      , DEST_LEVEL_FK2_OD   =  :DEST_LEVEL_FK2_OD ";
                    strSql += CRLF + "      , USE_FK_OD           =  :USE_FK_OD         ";
                    strSql += CRLF + "      , USER_COMMAND_OD     =  :USER_COMMAND_OD   ";            //D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD       =  :WRITE_FLAG_OD     ";            //D191
                    strSql += CRLF + "      , OD_RQ_YN            =  'Y'                ";
                    strSql += CRLF + "      , OD_USER_ID          =  'IOTASK'           ";
                    strSql += CRLF + "      , OD_UPD_DT           =  " + DbLang.SYSDATE + "              ";
                    strSql += CRLF + "  WHERE WH_TYP              =  :WH_TYP            ";
                    strSql += CRLF + "    AND SC_NO               =  :SC_NO             ";
                    strSql += CRLF + "    AND OD_RQ_YN            =  'N'                ";
                    strSql += CRLF + "    AND ERR_CODE_RD         <> '0'                ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("START_BANK_FK1_OD", DbLang.VARCHAR).Value = strSOUR_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("START_BAY_FK1_OD", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("START_LEVEL_FK1_OD", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_BANK_FK1_OD", DbLang.VARCHAR).Value = strDEST_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_BAY_FK1_OD", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK1_OD ", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO_FK2_OD", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("START_BANK_FK2_OD", DbLang.VARCHAR).Value = strSOUR_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("START_BAY_FK2_OD", DbLang.VARCHAR).Value = strSOUR_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("START_LEVEL_FK2_OD", DbLang.VARCHAR).Value = strSOUR_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_BANK_FK2_OD", DbLang.VARCHAR).Value = strDEST_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_BAY_FK2_OD", DbLang.VARCHAR).Value = strDEST_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("DEST_LEVEL_FK2_OD", DbLang.VARCHAR).Value = strDEST_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("USE_FK_OD", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("USER_COMMAND_OD", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("WRITE_FLAG_OD", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                }

                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "작업처리실패.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }



        // S/C정보 초기화.
        public bool UPDATE_SC_OD_RESET(string strWH_TYP
                                     , string strSC_NO
                                     , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG += "UPDATE_SC_OD_RESET::";

                strSql = "";
                strSql += CRLF + " UPDATE SC_DATA                    ";
                strSql += CRLF + "    SET USER_COMMAND_OD     =   '0'";
                strSql += CRLF + "      , WRITE_FLAG_OD       =   '0'";
                strSql += CRLF + "      , WRITE_CONTINUE_OD   =   '0'";
                strSql += CRLF + "      , DEST_HSPOS_FK2_OD   =   '0'";
                strSql += CRLF + "      , DEST_BANK_FK2_OD    =   '0'";
                strSql += CRLF + "      , START_HSPOS_FK2_OD  =   '0'";
                strSql += CRLF + "      , START_BANK_FK2_OD   =   '0'";
                strSql += CRLF + "      , LUGG_NO_FK2_OD      =   '0'";
                strSql += CRLF + "      , USE_FK_OD           =   '0'";
                strSql += CRLF + "      , DEST_HSPOS_FK1_OD   =   '0'";
                strSql += CRLF + "      , DEST_BANK_FK1_OD    =   '0'";
                strSql += CRLF + "      , START_HSPOS_FK1_OD  =   '0'";
                strSql += CRLF + "      , START_BANK_FK1_OD   =   '0'";
                strSql += CRLF + "      , LUGG_NO_FK1_OD      =   '0'";
                strSql += CRLF + "      , JOB_TYP_OD          =   '0'";
                strSql += CRLF + "      , DEST_LEVEL_FK2_OD   =   '0'";
                strSql += CRLF + "      , DEST_BAY_FK2_OD     =   '0'";
                strSql += CRLF + "      , DEST_LEVEL_FK1_OD   =   '0'";
                strSql += CRLF + "      , DEST_BAY_FK1_OD     =   '0'";
                strSql += CRLF + "      , START_LEVEL_FK2_OD  =   '0'";
                strSql += CRLF + "      , START_BAY_FK2_OD    =   '0'";
                strSql += CRLF + "      , START_LEVEL_FK1_OD  =   '0'";
                strSql += CRLF + "      , START_BAY_FK1_OD    =   '0'";
                strSql += CRLF + "      , SC_FIRE_OD    =   '0'";
                strSql += CRLF + "  WHERE WH_TYP              =   :WH_TYP  ";
                strSql += CRLF + "    AND SC_NO               =   :SC_NO   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = "S/C 초기화 실패 S/C NO : " + strSC_NO;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }

        // 작업정보 상태변경처리.
        public bool UPDATE_JOB_DATA(string strJOB_STATUS,
                                    string strLUGG_NO,
                                    string strWH_TYP,
                                    string strJOB_TYP,
                                ref string strRTN_MSG,
                                    string strDEST_POS = "0",
                                    string strCELL_NO = "0",
                                    string strHS_MC_NO = "0",
                                    string strSC_NO = "0",
                                    string strSTART_POS = "0",
                                    string strPAIR_LUGG_NO = "0",
                                    string strCMP_STEP = "0")       // 호기간 이동을 위해서... 
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                strRTN_MSG += "UPDATE_JOB_DATA";

                strSql = "";
                strSql += CRLF + " UPDATE  JOB_MST                                      ";
                strSql += CRLF + "    SET  JOB_STATUS       =  :JOB_STATUS              ";
                if (strSTART_POS != "0")
                    strSql += CRLF + "    ,START_POS        =  '" + strSTART_POS + "'  ";
                if (strDEST_POS != "0")
                    strSql += CRLF + "    ,DEST_POS         =  '" + strDEST_POS + "'    ";
                if (strCELL_NO != "0" &&
                   (strJOB_TYP == ((int)cDefApp.eJOBTYP.Ret).ToString() || strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString()))
                    strSql += CRLF + "    ,START_LOCATION   =  '" + strCELL_NO + "'";
                if (strCELL_NO != "0" && 
                    strJOB_TYP == ((int)cDefApp.eJOBTYP.Sto).ToString())
                    strSql += CRLF + "    ,DEST_LOCATION    =  '" + strCELL_NO + "'";
                if (strHS_MC_NO != "0")
                    strSql += CRLF + "    ,HS_TRACK_NO      =  '" + strHS_MC_NO + "' ";
                if (strSC_NO != "0")
                    strSql += CRLF + "    ,SC_NO            =  '" + strSC_NO + "'   ";
                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString() && strJOB_STATUS == "11")
                    strSql += CRLF + "    ,JOB_TYP          =  '6'      ";
                if (strPAIR_LUGG_NO != "0")
                    strSql += CRLF + "    ,PAIR_LUGG_NO     =  '" + strPAIR_LUGG_NO + "'";
                if (strCMP_STEP != "0")
                   strSql += CRLF + "     ,CMP_STEP         = '" + strCMP_STEP + "'   ";
                strSql += CRLF + "        ,UPD_DT           = " + DbLang.SYSDATE + "   ";
                strSql += CRLF + "  WHERE  WH_TYP           = :WH_TYP   ";
                strSql += CRLF + "    AND  LUGG_NO          = :LUGG_NO  ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = strJOB_STATUS;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "변경할 JOB_MST 정보가 존재하지 않습니다";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                strRTN_MSG += ex.ToString();
                return false;
            }
        }



        // 새작업 있는지 체크해서 Invoke 한다.
        public bool NEW_JOB_INVOKE_FOR_SC(string strWH_TYP,
                              ref string pRTN_MSG)
        {
            try
            {
                string strLUGG_NO = "";
                string strTRACK_NO = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";

                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg = "NEW_JOB_INVOKE_FOR_SC::";

                strSql = "";
                strSql += cDefApp.CRLF + " SELECT *                                 ";
                strSql += cDefApp.CRLF + "   FROM JOB_MST                           ";
                strSql += cDefApp.CRLF + "  WHERE WH_TYP	    = :WH_TYP           ";
                strSql += cDefApp.CRLF + "    AND JOB_TYP      IN ('2', '4', '5')   ";
                strSql += cDefApp.CRLF + "    AND JOB_STATUS 	= '99'              ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();
                for (int i = 0; i < nSelCnt; i++)
                {
                    string strJOB_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " UPDATE JOB_MST                         ";
                    strSql += CRLF + "    SET JOB_STATUS    = '20'            ";
                    strSql += CRLF + "      , UPD_DT        = " + DbLang.SYSDATE + "           ";
                    strSql += CRLF + "      , UPD_USER_ID   = 'IO_TASK'       ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP         ";
                    strSql += CRLF + "    AND LUGG_NO       = :LUGG_NO        ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strJOB_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        m_strRtnMsg = pRTN_MSG;
                        throw new Exception(m_strRtnMsg);
                    }
                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "변경할 작업 정보가 존재하지 않습니다. [LUGG NO : " + strJOB_NO + "]";
                        _pBdb.Rollback();
                        m_strRtnMsg = pRTN_MSG;
                        throw new Exception(m_strRtnMsg);
                    }
                }

                _pBdb.Commit();
                return true;

            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                _pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }

        // 도착보고 후 기존DATA를 삭제 후 MES에서 새작업을 생성.
        public bool DELETE_JOB_DATA(string strJOB_STATUS,
                                    string strLUGG_NO,
                                    string strWH_TYP, 
                                ref string strRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg += "DELETE_JOB_DATA::";

                strSql = "";
                strSql += CRLF + " INSERT INTO JOB_MST_HIS      ";
                strSql += CRLF + "           ( INS_DATE         ";
                strSql += CRLF + "           , INS_TIME         ";
                strSql += CRLF + "           , WH_TYP           ";
                strSql += CRLF + "           , LUGG_NO          ";
                strSql += CRLF + "           , START_POS        ";
                strSql += CRLF + "           , START_LOCATION   ";
                strSql += CRLF + "           , DEST_POS         ";
                strSql += CRLF + "           , DEST_LOCATION    ";
                strSql += CRLF + "           , TURN             ";
                strSql += CRLF + "           , PRODUCT_ID       ";
                strSql += CRLF + "           , PRODUCT_SIZE     ";
                strSql += CRLF + "           , JOB_TYP          ";
                strSql += CRLF + "           , BCR_TOP          ";
                strSql += CRLF + "           , BCR_BOTTOM       ";
                strSql += CRLF + "           , MES_WRITE_YN     ";
                strSql += CRLF + "           , CMD_STA          ";
                strSql += CRLF + "           , JOB_KIND         ";
                strSql += CRLF + "           , JOB_STATUS       ";
                strSql += CRLF + "           , JOB_PRIORITY     ";
                strSql += CRLF + "           , MES_ERROR_CD     ";
                strSql += CRLF + "           , OD_LAST_PAGE     ";
                strSql += CRLF + "           , OD_LAST_USER     ";
                strSql += CRLF + "           , JOB_START_DT     ";
                strSql += CRLF + "           , INS_DT           ";
                strSql += CRLF + "           , INS_USER_ID      ";
                strSql += CRLF + "           , REMARKS          ";
                strSql += CRLF + "           , TRAY_TYP         ";
                strSql += CRLF + "           , TRAY_LEV         ";
                strSql += CRLF + "           , HS_TRACK_NO      ";
                strSql += CRLF + "           , SC_NO            ";
                strSql += CRLF + "           , DURATION_TIME    ";
                strSql += CRLF + "           , SYSTEM_BYTE      ";
                strSql += CRLF + "           , S6F12_RESPONSE   ";
                strSql += CRLF + "           , S6F11_YON        ";
                strSql += CRLF + "           , CEID_NAME        ";
                strSql += CRLF + "           , FK_NO            ";
                strSql += CRLF + "           , AGING_TIME       ";
                strSql += CRLF + "           , PAIR_LUGG_NO )   ";
                strSql += CRLF + " SELECT TO_CHAR(" + DbLang.SYSDATE + ", 'YYYYMMDD'), TO_CHAR(" + DbLang.SYSDATE + ", 'HH24MISS')                            ";
                strSql += CRLF + "      , WH_TYP, LUGG_NO, START_POS, START_LOCATION, DEST_POS, DEST_LOCATION                   ";
                strSql += CRLF + "      , TURN, PRODUCT_ID, PRODUCT_SIZE, JOB_TYP, BCR_TOP, BCR_BOTTOM, MES_WRITE_YN            ";
                strSql += CRLF + "      , CMD_STA, JOB_KIND, JOB_STATUS, JOB_PRIORITY, MES_ERROR_CD, OD_LAST_PAGE, OD_LAST_USER ";
                strSql += CRLF + "      , JOB_START_DT, " + DbLang.SYSDATE + ", 'IO_TASK', REMARKS, TRAY_TYP, TRAY_LEV, HS_TRACK_NO, SC_NO    ";
                strSql += CRLF + "      , DURATION_TIME, SYSTEM_BYTE, S6F12_RESPONSE, S6F11_YON, CEID_NAME, FK_NO, AGING_TIME   ";
                strSql += CRLF + "      , PAIR_LUGG_NO                  ";
                strSql += CRLF + "   FROM JOB_MST                       ";
                strSql += CRLF + "  WHERE WH_TYP   = :WH_TYP       ";
                strSql += CRLF + "    AND LUGG_NO  = :LUGG_NO   ";

                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;

                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "작업 히스토리를 추가 할 수 없습니다. LUGG_NO : " + strLUGG_NO;
                    return false;
                }

                strSql = "";
                strSql += CRLF + " DELETE JOB_MST               ";
                strSql += CRLF + "  WHERE WH_TYP   = :WH_TYP    ";
                strSql += CRLF + "    AND LUGG_NO  = :LUGG_NO   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "삭제할 작업삭정보가 존재하지 않습니다. LUGG_NO : " + strLUGG_NO;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                strRTN_MSG += ex.ToString();
                return false;
            }
        }



        // LOCATION 가져오기.
        public bool FETCH_LOCATION_BY_AGING_TYP(string strWH_TYP, 
                                                string strDURATION_TIME,
                                                string strSC_NO_LEV,
                                                DateTime dtAGING_END_DT,
                                            ref string strDEST_LOCATION,
                                            ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                DataTable dtSYS_DATE = new DataTable();
                DataTable dtCELL_MST = new DataTable();

                // 작업예정시간(DURATION_TIME [MES수신])
                strSql = "";
                strSql += cDefApp.CRLF + "SELECT " + DbLang.SYSDATE + " + (1/24/60/60) * ((TO_NUMBER(SUBSTR(:DURATION_TIME ,1 ,3)) * 60 * 60 * 24)  ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME ,4 ,2)) * 60 * 60)       ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME ,6 ,2)) * 60)            ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME ,8 ,2)))) AS END_TIME    ";
                strSql += cDefApp.CRLF + " from DUAL ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("DURATION_TIME", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME", DbLang.VARCHAR).Value = strDURATION_TIME;
                nSelCnt = _pBdb.ExcuteQry(dtSYS_DATE, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt > 0)
                {
                    DateTime dtEND_TIME = new DateTime();
                    dtEND_TIME = Convert.ToDateTime(dtSYS_DATE.Rows[0]["END_TIME"].ToString());
                    dtSYS_DATE.Dispose();

                    strSql = "";

                    // MES수신시간이 존재하면,
                    if (strDURATION_TIME != "000000000")
                    {
                        // LOCATION 구한다.
                        // ● / ●    
                        // (OUT LOCATION - IN LOCATION 시간[기존에 적재되어있는 OUT과 현재 입고중인 화물에 대한 출고예정시간을 비교한다.]이 3분이내)
                        strSql += cDefApp.CRLF + "SELECT 1 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                        strSql += cDefApp.CRLF + "      , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                        strSql += cDefApp.CRLF + "  FROM CELL_MST CM        ";
                        strSql += cDefApp.CRLF + " INNER JOIN CELL_MST CM2  ";
                        strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = CM2.WH_TYP ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_SC_NO = :SC_NO_LEV1 ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO     = CM2.PAIR   ";
                        strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD             ";
                        strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = SD.WH_TYP  ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = SD.SC_NO   ";
                        strSql += cDefApp.CRLF + "   AND SD.SUSPEND    IN('0', '2')  ";
                        strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD = '0'        ";
                        strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'    "; // 자동모드
                        strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD  = '1'    "; // ACTIVE MODE
                        strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'    "; // ONLINE MODE
                        strSql += cDefApp.CRLF + "  LEFT OUTER JOIN CELL_DTL CD      ";
                        strSql += cDefApp.CRLF + "    ON CM2.WH_TYP     = CD.WH_TYP      ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_SC_NO = CD.CELL_SC_NO  ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_NO    = CD.CELL_NO     ";
                        strSql += cDefApp.CRLF + " WHERE CM.WH_TYP      = :WH_TYP1    ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = :SC_NO_LEV2 ";   
                        strSql += cDefApp.CRLF + "   AND CM.CELL_INOUT  = 'IN'       ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_STA IN ('0')        ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_USE_DEF IN ('0', '2')";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_STA IN ('2')       ";
                        strSql += cDefApp.CRLF + "   AND (((CD.AGING_END_DT - :END_TIME1) * 24 * 60) > 2 AND  "; // 2분초과 및 10분미만.
                        strSql += cDefApp.CRLF + "        ((CD.AGING_END_DT - :END_TIME2) * 24 * 60) < 10)    ";
                        // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST    ";
                        strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                        strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO      ";
                        strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO)  ";
                        strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))             ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                        strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                        strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                        strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                        strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))             ";
                        strSql += cDefApp.CRLF + " UNION ALL                                                     ";
                    }
                    else
                    {
                        strSql += cDefApp.CRLF + "SELECT 1 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                        strSql += cDefApp.CRLF + "      , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                        strSql += cDefApp.CRLF + "  FROM CELL_MST CM        ";
                        strSql += cDefApp.CRLF + " INNER JOIN CELL_MST CM2  ";
                        strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = CM2.WH_TYP ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_SC_NO = :SC_NO_LEV1 ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO     = CM2.PAIR   ";
                        strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD             ";
                        strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = SD.WH_TYP  ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = SD.SC_NO   ";
                        strSql += cDefApp.CRLF + "   AND SD.SUSPEND  IN    ('0', '2')";
                        strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD = '0'        ";
                        strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'    "; // 자동모드
                        strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD  = '1'    "; // ACTIVE MODE
                        strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'    "; // ONLINE MODE
                        strSql += cDefApp.CRLF + "  LEFT OUTER JOIN CELL_DTL CD      ";
                        strSql += cDefApp.CRLF + "    ON CM2.WH_TYP     = CD.WH_TYP      ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_SC_NO = CD.CELL_SC_NO  ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_NO    = CD.CELL_NO     ";
                        strSql += cDefApp.CRLF + " WHERE CM.WH_TYP      = :WH_TYP1    ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = :SC_NO_LEV2 ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_INOUT  = 'IN'       ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_STA IN ('0')        ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_USE_DEF IN ('0', '2')";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_STA IN ('2')       ";
                        strSql += cDefApp.CRLF + "   AND (((CD.AGING_END_DT - :END_TIME1) * 24 * 60) > 2 AND  "; // 2분초과 및 10분미만.
                        strSql += cDefApp.CRLF + "        ((CD.AGING_END_DT - :END_TIME2) * 24 * 60) < 10)    ";
                        // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST   ";
                        strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP         ";
                        strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO     ";
                        strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO) ";
                        strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))            ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                        strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                        strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                        strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                        strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))        ";
                        strSql += cDefApp.CRLF + " UNION ALL      ";
                    }
                    // OUT LOCATION 검색.
                    strSql += cDefApp.CRLF + " SELECT 2 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                    strSql += cDefApp.CRLF + "       , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                    strSql += cDefApp.CRLF + "   FROM CELL_MST CM        ";
                    strSql += cDefApp.CRLF + "  INNER JOIN CELL_MST CM2  ";
                    strSql += cDefApp.CRLF + "     ON CM.WH_TYP           = CM2.WH_TYP  ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_SC_NO      = :SC_NO_LEV_A  ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_NO          = CM2.PAIR    ";
                    strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD                   ";
                    strSql += cDefApp.CRLF + "     ON CM.WH_TYP           = SD.WH_TYP   ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO       = SD.SC_NO    ";
                    strSql += cDefApp.CRLF + "    AND SD.SUSPEND          IN('0', '2')  ";
                    strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD      = '0'         ";
                    strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD    = '1'    "; // 자동모드
                    strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD  = '1'    "; // ACTIVE MODE
                    strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD  = '1'    "; // ONLINE MODE
                    strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP           = :WH_TYP_A     ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO       = :SC_NO_LEV_A2  ";   
                    strSql += cDefApp.CRLF + "    AND CM.CELL_INOUT       = 'OUT'       ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_USE_DEF IN ('0', '2')     ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_STA IN ('0')              ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_STA IN ('0')             ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_USE_DEF IN ('0', '2')    ";
                    // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP         ";
                    strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))            ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))        ";
                    strSql += cDefApp.CRLF + "  UNION ALL                               ";

                    // RACK TO RACK을 해도상관없는 비어있는 IN LOCATION검색.
                    strSql += cDefApp.CRLF + " SELECT 3 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                    strSql += cDefApp.CRLF + "       , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                    strSql += cDefApp.CRLF + "   FROM CELL_MST CM        ";
                    strSql += cDefApp.CRLF + "  INNER JOIN CELL_MST CM2  ";
                    strSql += cDefApp.CRLF + "     ON CM.WH_TYP      = CM2.WH_TYP ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_SC_NO = :SC_NO_LEV_B ";  
                    strSql += cDefApp.CRLF + "    AND CM.CELL_NO     = CM2.PAIR   ";
                    strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD             ";
                    strSql += cDefApp.CRLF + "     ON CM.WH_TYP      = SD.WH_TYP  ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO  = SD.SC_NO   ";
                    strSql += cDefApp.CRLF + "    AND SD.SUSPEND    IN('0', '2')  ";
                    strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD = '0'        ";
                    strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD    = '1'    "; // 자동모드
                    strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD  = '1'    "; // ACTIVE MODE
                    strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD  = '1'    "; // ONLINE MODE
                    strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP      = :WH_TYP_B    ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO  = :SC_NO_LEV_B2 ";   
                    strSql += cDefApp.CRLF + "    AND CM.CELL_INOUT  = 'IN'       ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_STA IN ('0')        ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_USE_DEF IN ('0', '2')";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_STA IN ('2')       ";
                    // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP         ";
                    strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))            ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))        ";
                    strSql += cDefApp.CRLF + "  UNION ALL ";
                    strSql += cDefApp.CRLF + "SELECT 4 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                    strSql += cDefApp.CRLF + "      , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                    strSql += cDefApp.CRLF + "  FROM CELL_MST CM        ";
                    strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD    ";
                    strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = SD.WH_TYP  ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = SD.SC_NO   ";
                    strSql += cDefApp.CRLF + "   AND SD.SUSPEND    IN('0', '2')  ";
                    strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD = '0'        ";
                    strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'    "; // 자동모드
                    strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD  = '1'    "; // ACTIVE MODE
                    strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'    "; // ONLINE MODE
                    strSql += cDefApp.CRLF + " WHERE CM.WH_TYP      = :WH_TYP_C    ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = :SC_NO_LEV_C ";   
                    strSql += cDefApp.CRLF + "   AND CM.CELL_STA IN ('0')        ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_USE_DEF IN ('0', '2')";
                    // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP         ";
                    strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))            ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))        ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    if (strDURATION_TIME != "000000000")
                    {
                        _pBdb.mComMain.Parameters.Add("SC_NO_LEV1", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        _pBdb.mComMain.Parameters.Add("WH_TYP1", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("SC_NO_LEV2", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        _pBdb.mComMain.Parameters.Add("END_TIME1", DbLang.DATE).Value = dtEND_TIME;
                        _pBdb.mComMain.Parameters.Add("END_TIME2", DbLang.DATE).Value = dtEND_TIME;
                    }
                    else
                    {
                        _pBdb.mComMain.Parameters.Add("SC_NO_LEV1", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        _pBdb.mComMain.Parameters.Add("WH_TYP1", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("SC_NO_LEV2", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        _pBdb.mComMain.Parameters.Add("END_TIME1", DbLang.DATE).Value = dtAGING_END_DT;
                        _pBdb.mComMain.Parameters.Add("END_TIME2", DbLang.DATE).Value = dtAGING_END_DT;
                    }
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_A", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    _pBdb.mComMain.Parameters.Add("WH_TYP_A", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_A2", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_B", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    _pBdb.mComMain.Parameters.Add("WH_TYP_B", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_B2", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    _pBdb.mComMain.Parameters.Add("WH_TYP_C", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_C", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    nSelCnt = _pBdb.ExcuteQry(dtCELL_MST, strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "입고가능한 CELL이 없습니다.";
                        return false;
                    }
                    if (nSelCnt > 0)
                    {
                        strDEST_LOCATION = "" + dtCELL_MST.Rows[0]["CELL_NO"].ToString();

                        

                        dtCELL_MST.Dispose();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }

        // CELL정보가져오기.
        public bool CHECK_LOCATION_BY_CELLNO(string strSC_NO, 
                                             string strCELL_NO,
                                             string strWH_TYP,
                                             ref string strCELL_STA,
                                             ref string strBOTTOM_TRAY,
                                             ref string strTOP_TRAY,
                                             ref DateTime dtAGING_END_DT,
                                             ref int nCOUNT)
        {
            try
            {
                string strSql = "";

                DataTable dtCELL_MST = new DataTable();
                IsRtnMsg = "CHECK_LOCATION_BY_CELLNO::";

                strSql = "";
                strSql += CRLF + " SELECT CM.*, CD.*             ";
                strSql += CRLF + "   FROM CELL_MST CM            ";
                strSql += CRLF + "  INNER JOIN CELL_DTL CD       ";
                strSql += CRLF + "     ON CM.WH_TYP         = CD.WH_TYP  ";
                strSql += CRLF + "    AND CM.CELL_NO        = CD.CELL_NO";
                strSql += CRLF + "  WHERE CM.WH_TYP         =   :WH_TYP   ";
                strSql += CRLF + "    AND CM.CELL_NO        =   :CELL_NO  ";
                strSql += CRLF + "    AND CM.CELL_SC_NO     =   :SC_NO  ";
                strSql += CRLF + "    AND CM.CELL_STA <> '0' ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CELL_NO", DbLang.VARCHAR).Value = strCELL_NO;
                _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                nCOUNT = _pBdb.ExcuteQry(dtCELL_MST, strSql);

                if (nCOUNT < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }

                if (nCOUNT == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    IsRtnMsg = "";
                    return false;
                }

                strCELL_STA = dtCELL_MST.Rows[0]["CELL_STA"].ToString();
                strBOTTOM_TRAY = dtCELL_MST.Rows[0]["BOTTOM_TRAY"].ToString();
                strTOP_TRAY = dtCELL_MST.Rows[0]["TOP_TRAY"].ToString();
                dtAGING_END_DT = Convert.ToDateTime(dtCELL_MST.Rows[0]["AGING_END_DT"].ToString());

                dtCELL_MST.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                return false;
            }
        }

        // 해당 LOCATION이 시작작업에 해당되는지 체크한다.
        public bool CHECK_JOB_BY_CELLNO(string strCELL_SC_NO, 
                                        string strCELL_NO,
                                        string strWH_TYP,
                                    ref int nCOUNT)
        {
            try
            {
                string strSql = "";

                DataTable dtJOB_MST = new DataTable();
                IsRtnMsg = "CHECK_JOB_BY_CELLNO::";

                strSql = "";                      
                strSql += CRLF + " SELECT *                                 ";
                strSql += CRLF + "   FROM JOB_MST                           ";
                strSql += CRLF + "  WHERE WH_TYP         = :WH_TYP          ";
                strSql += CRLF + "    AND START_POS      = :START_POS       ";
                strSql += CRLF + "    AND START_LOCATION = :START_LOCATION  ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strCELL_SC_NO;
                _pBdb.mComMain.Parameters.Add("START_LOCATION", DbLang.VARCHAR).Value = strCELL_NO;
                nCOUNT = _pBdb.ExcuteQry(dtJOB_MST, strSql);
                if (nCOUNT < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }

                if (nCOUNT == 0)
                {
                    IsRtnMsg = "";
                    return false;
                }

                dtJOB_MST.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                return false;
            }
        }
        


        // LOG 남기기.
        public void InsertLog(string strWH_TYP
                            , string strLOG_KOR
                            , string strBCR_BOTTOM = ""
                            , string strBCR_TOP = ""
                            , string strLUGG_NO = "0"
                            , string strJOB_STA = ""
                            , string strTRACK_FROM = ""
                            , string strTRACK_TO = "")
        {
            string strTitle = "[InsertLog]";
            string strSql;

            string strPGR_NO = "IO_TASK";
            int nRtn = 0;

            try
            {
                _pBdb.BeginTrans();

                strSql = "";
                strSql = cDefApp.CRLF + " INSERT INTO WCS_LOG_PGR (WH_TYP         ";    // 1  
                strSql += cDefApp.CRLF + "                       , INS_DT            ";    // 9
                strSql += cDefApp.CRLF + "                       , LOG_SEQ           ";    // 2
                strSql += cDefApp.CRLF + "                       , LUGG_NO           ";    // 3
                strSql += cDefApp.CRLF + "                       , BCR_BOTTOM        ";    // 3
                strSql += cDefApp.CRLF + "                       , BCR_TOP           ";    // 3
                strSql += cDefApp.CRLF + "                       , PGR_NM            ";    // 3
                strSql += cDefApp.CRLF + "                       , LOG_KOR           ";    // 4
                strSql += cDefApp.CRLF + "                       , TRACK_FROM        ";    // 4
                strSql += cDefApp.CRLF + "                       , TRACK_TO          ";    // 4
                strSql += cDefApp.CRLF + "                       , JOB_STA           ";    // 4
                strSql += cDefApp.CRLF + "                       , RQ_INS_ID         ";    // 8
                strSql += cDefApp.CRLF + "                       , RQ_INS_DT         ";    // 9
                strSql += cDefApp.CRLF + "                       , EQP_TYP)          ";    // 9
                strSql += cDefApp.CRLF + "                VALUES ( :WH_TYP           ";    // 1     // WH_TYP  
                strSql += cDefApp.CRLF + "                       , " + DbLang.SYSDATE + "           ";    // 3     // CELL_SC_NO 
                strSql += cDefApp.CRLF + "                       , " + DbLang.NEXTVAL("LOG_SEQ");          //LOG_SEQ.NEXTVAL   ";    // 3     // CELL_SC_NO 
                strSql += cDefApp.CRLF + "                       , :LUGG_NO          ";    // 2     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :BCR_BOTTOM       ";    // 2     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :BCR_TOP          ";    // 2     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :PGR_NM           ";    // 2     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :LOG_KOR          ";    // 4     // CELL_SEQ      
                strSql += cDefApp.CRLF + "                       , :TRACK_FROM       ";    // 2     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :TRACK_TO         ";    // 2     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :JOB_STA          ";    // 4     // CELL_SEQ             
                strSql += cDefApp.CRLF + "                       , :RQ_INS_ID        ";    // 8     // AGING_START_DT  
                strSql += cDefApp.CRLF + "                       , " + DbLang.SYSDATE + "           ";    // 11    // REMARKS    
                strSql += cDefApp.CRLF + "                       , 'IO')             ";    // 11    // IO_TASK    
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("BCR_BOTTOM", DbLang.VARCHAR).Value = strBCR_BOTTOM;
                _pBdb.mComMain.Parameters.Add("BCR_TOP", DbLang.VARCHAR).Value = strBCR_TOP;
                _pBdb.mComMain.Parameters.Add("PGR_NM", DbLang.VARCHAR).Value = strPGR_NO;
                _pBdb.mComMain.Parameters.Add("LOG_KOR", DbLang.VARCHAR).Value = strLOG_KOR;
                _pBdb.mComMain.Parameters.Add("TRACK_FROM", DbLang.VARCHAR).Value = strTRACK_FROM;
                _pBdb.mComMain.Parameters.Add("TRACK_TO", DbLang.VARCHAR).Value = strTRACK_TO;
                _pBdb.mComMain.Parameters.Add("JOB_STA", DbLang.VARCHAR).Value = strJOB_STA;
                _pBdb.mComMain.Parameters.Add("RQ_INS_ID", DbLang.VARCHAR).Value = strPGR_NO;
                //_pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR).Value = strEQP_TYP;
                nRtn = _pBdb.ExcuteNonQry(strSql);
                if (nRtn < 0)
                {
                    throw new Exception(strTitle + "LOG_PGR INSERT중 DATABASE 에러.. MESSAGE [" + _pBdb.ErrMsg + "]");
                }

                if (nRtn == 0)
                {
                    _pBdb.Rollback();
                    return;
                }

                _pBdb.Commit();
                return;
            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                MakeMsg_Error_NoLog(ex.Message);
                SetErrorMsg(ex.Message);
                return;
            }
        }

        #region 사용하지 않는 함수들 선언 - 주석처리함!
        // 2019.11.02 SMLEE (화재출고에 대한 완료로직)
        // CHECK_CV_FIRE_COMPLETE
        /*
        public bool CHECK_CV_FIRE_COMPLETE(string strWH_TYP,
                                           string strAGING_TYP,
                                       ref string pRTN_MSG)
        {
            try
            {
                int nMainSelCnt = 0;

                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_FIRE_COMPLETE::";

                // 1.S/C완료된 화재출고의 CELL을 찾는다.
                strSql = "";
                strSql += CRLF + " SELECT  JM.*, CM.*, CDL.*, CD.* ";
                strSql += CRLF + "   FROM JOB_MST JM               ";
                strSql += CRLF + "  INNER JOIN CELL_MST CM         ";
                strSql += CRLF + "     ON JM.START_POS 		= CM.CELL_SC_NO     ";
                strSql += CRLF + "    AND JM.START_LOCATION = CM.CELL_NO        ";
                strSql += CRLF + "    AND JM.BCR_BOTTOM 	= CM.BOTTOM_TRAY    ";
                strSql += CRLF + "  INNER JOIN CELL_DTL CDL                     ";
                strSql += CRLF + "     ON CM.CELL_SC_NO		= CDL.CELL_SC_NO    ";
                strSql += CRLF + "    AND CM.CELL_NO		= CDL.CELL_NO       ";
                strSql += CRLF + "    AND CM.BOTTOM_TRAY 	= CDL.BOTTOM_TRAY   ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD                       ";
                strSql += CRLF + "     ON JM.WH_TYP         = CD.WH_TYP         ";
                strSql += CRLF + "    AND JM.DEST_POS       = CD.TRACK_NO       ";
                strSql += CRLF + "  WHERE CD.WH_TYP         = :WH_TYP           ";
                strSql += CRLF + "    AND JM.START_POS like '%' " + DbLang.II + " " + strAGING_TYP + " " + DbLang.II + " '%'       ";
                strSql += CRLF + "    AND JM.JOB_STATUS     = '28'              ";
                strSql += CRLF + "    AND JM.JOB_TYP        = '8'               ";
                strSql += CRLF + "    AND CM.CELL_STA	    = '0'               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nMainSelCnt = _pBdb.ExcuteQry(strSql);
                if (nMainSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nMainSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                // 2. 화재출고가 생성되고, S/C는 완료되면 데이터에 대해서 재고삭제 및 작업삭제처리.
                string strJOB_TYP = "";
                string strDEST_POS = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strSC_NO = "";
                string strSTART_LOCATION = "";

                string strBCR_BOTTOM = "";
                string strBCR_TOP = "";

                _pBdb.BeginTrans();
                strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                strDEST_POS = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                strLUGG_NO = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                strSTART_POS = "" + _pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString();
                strSC_NO = "" + _pBdb.mDtMain.Rows[0]["SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_NO"].ToString();
                strSTART_LOCATION = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString();
                strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString();

                if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.FireRet)
                {
                    // 작업삭제전 JOB_MST_HIS 생성.
                    // 삭제 전 작업정보이력생성.
                    strSql = "";
                    strSql += CRLF + " INSERT INTO JOB_MST_HIS      ";
                    strSql += CRLF + "           ( INS_DATE         ";
                    strSql += CRLF + "           , INS_TIME         ";
                    strSql += CRLF + "           , WH_TYP           ";
                    strSql += CRLF + "           , LUGG_NO          ";
                    strSql += CRLF + "           , START_POS        ";
                    strSql += CRLF + "           , START_LOCATION   ";
                    strSql += CRLF + "           , DEST_POS         ";
                    strSql += CRLF + "           , DEST_LOCATION    ";
                    strSql += CRLF + "           , TURN             ";
                    strSql += CRLF + "           , PRODUCT_ID       ";
                    strSql += CRLF + "           , PRODUCT_SIZE     ";
                    strSql += CRLF + "           , JOB_TYP          ";
                    strSql += CRLF + "           , BCR_TOP          ";
                    strSql += CRLF + "           , BCR_BOTTOM       ";
                    strSql += CRLF + "           , MES_WRITE_YN     ";
                    strSql += CRLF + "           , CMD_STA          ";
                    strSql += CRLF + "           , JOB_KIND         ";
                    strSql += CRLF + "           , JOB_STATUS       ";
                    strSql += CRLF + "           , JOB_PRIORITY     ";
                    strSql += CRLF + "           , MES_ERROR_CD     ";
                    strSql += CRLF + "           , OD_LAST_PAGE     ";
                    strSql += CRLF + "           , OD_LAST_USER     ";
                    strSql += CRLF + "           , JOB_START_DT     ";
                    strSql += CRLF + "           , INS_DT           ";
                    strSql += CRLF + "           , INS_USER_ID      ";
                    strSql += CRLF + "           , REMARKS          ";
                    strSql += CRLF + "           , TRAY_TYP         ";
                    strSql += CRLF + "           , TRAY_LEV         ";
                    strSql += CRLF + "           , HS_TRACK_NO      ";
                    strSql += CRLF + "           , SC_NO            ";
                    strSql += CRLF + "           , DURATION_TIME    ";
                    strSql += CRLF + "           , SYSTEM_BYTE      ";
                    strSql += CRLF + "           , S6F12_RESPONSE   ";
                    strSql += CRLF + "           , S6F11_YON        ";
                    strSql += CRLF + "           , CEID_NAME        ";
                    strSql += CRLF + "           , FK_NO            ";
                    strSql += CRLF + "           , AGING_TIME       ";
                    strSql += CRLF + "           , PAIR_LUGG_NO )   ";
                    strSql += CRLF + " SELECT TO_CHAR(" + DbLang.SYSDATE + ", 'YYYYMMDD'), TO_CHAR(" + DbLang.SYSDATE + ", 'HH24MISS')                            ";
                    strSql += CRLF + "      , WH_TYP, LUGG_NO, START_POS, START_LOCATION, DEST_POS, DEST_LOCATION                   ";
                    strSql += CRLF + "      , TURN, PRODUCT_ID, PRODUCT_SIZE, JOB_TYP, BCR_TOP, BCR_BOTTOM, MES_WRITE_YN            ";
                    strSql += CRLF + "      , CMD_STA, JOB_KIND, JOB_STATUS, JOB_PRIORITY, MES_ERROR_CD, OD_LAST_PAGE, OD_LAST_USER ";
                    strSql += CRLF + "      , JOB_START_DT, " + DbLang.SYSDATE + ", 'MES TASK', REMARKS, TRAY_TYP, TRAY_LEV, HS_TRACK_NO, SC_NO    ";
                    strSql += CRLF + "      , DURATION_TIME, SYSTEM_BYTE, S6F12_RESPONSE, S6F11_YON, CEID_NAME, FK_NO, AGING_TIME   ";
                    strSql += CRLF + "      , PAIR_LUGG_NO ";
                    strSql += CRLF + "   FROM JOB_MST      ";
                    strSql += CRLF + "  WHERE WH_TYP                = :WH_TYP ";
                    if (strBCR_TOP != "")
                    {
                        strSql += cDefApp.CRLF + "    AND BCR_TOP   = :BCR_TOP ";
                    }
                    strSql += cDefApp.CRLF + "    AND BCR_BOTTOM    = :BCR_BOTTOM   ";
                    strSql += cDefApp.CRLF + "    AND LUGG_NO       = :LUGG_NO      ";
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    if (strBCR_TOP != "")
                    {
                        _pBdb.mComMain.Parameters.Add("BCR_TOP", DbLang.VARCHAR).Value = strBCR_TOP;
                    }
                    _pBdb.mComMain.Parameters.Add("BCR_BOTTOM", DbLang.VARCHAR).Value = strBCR_BOTTOM;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql, false, true);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "생성할 JOB_MST 정보가 존재하지 않습니다. LUGG_NO : " + strLUGG_NO;
                        _pBdb.Rollback();
                        return false;
                    }

                    // 작업삭제
                    if (DELETE_JOB_DATA("28", strLUGG_NO, strWH_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                }

                _pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }
        //*/

        // 화재감지 출고작업생성. 
        // CHECK_SC_FIRE_STATUS
        /*
        public bool CHECK_SC_FIRE_STATUS(string strWH_TYP,
                                         string strAGING_TYP,
                                         string strPLC_NO,
                                     ref string pRTN_MSG)
        {

            string strSql = "";
            int nSelCnt = 0;

            try
            {
                pRTN_MSG = "CHECK_SC_FIRE_STATUS::";

                // 1.FIRE_CMD_OD = 119   FIRE_OD_RQ_YN = Y 를 조회한다.
                strSql += cDefApp.CRLF + "SELECT CM.*, CDL.*, SD.*  ";
                strSql += cDefApp.CRLF + "  FROM CELL_MST CM        ";
                strSql += cDefApp.CRLF + " INNER JOIN CELL_DTL CDL  ";
                strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = CDL.WH_TYP        ";
                strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = CDL.CELL_SC_NO    ";
                strSql += cDefApp.CRLF + "   AND CM.CELL_NO     = CDL.CELL_NO       ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD                    ";
                strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = SD.WH_TYP         ";
                strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = SD.SC_NO          ";
                strSql += cDefApp.CRLF + " WHERE CM.WH_TYP      = :pWH_TYP                 ";
                strSql += cDefApp.CRLF + "   AND CM.AGING_TYP   like '%' " + DbLang.II + " :pAGING_TYP " + DbLang.II + " '%'    ";
                strSql += cDefApp.CRLF + "   AND CM.CELL_STA        = '2'   ";
                strSql += cDefApp.CRLF + "   AND CM.CELL_USE_DEF IN ('0', '1')";
                strSql += cDefApp.CRLF + "   AND SD.OD_RQ_YN        = 'N'   ";
                strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'   ";
                strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'   ";
                strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD     = '0'   ";
                strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD  = '1'   ";
                strSql += cDefApp.CRLF + "   AND SD.UCSTATUS_RD     = '1'   ";
                strSql += cDefApp.CRLF + "   AND (SD.ITN_LUGG_FK1   = '0' AND SD.ITN_LUGG_FK2 = '0')    ";
                strSql += cDefApp.CRLF + "   AND SD.SUSPEND IN ('0', '1')       ";
                strSql += cDefApp.CRLF + "   AND CM.FIRE_CMD_OD     = '119'     ";  // 화재발생
                strSql += cDefApp.CRLF + "   AND CM.FIRE_OD_RQ_YN   = 'Y'       ";  // 화재출고요청
                strSql += cDefApp.CRLF + "   AND 0 = (SELECT COUNT(*)           ";  // 이미 지시내린 화재출고를 또 내리지 않기위함.
                strSql += cDefApp.CRLF + "              FROM JOB_MST            ";
                strSql += cDefApp.CRLF + "             WHERE WH_TYP = CM.WH_TYP ";
                strSql += cDefApp.CRLF + "               AND START_POS = CM.CELL_SC_NO      ";
                strSql += cDefApp.CRLF + "               AND START_LOCATION = CM.CELL_NO)   "; 
                strSql += cDefApp.CRLF + " ORDER BY CM.BAY DESC, CM.CELL_INOUT              ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("pAGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strCELL_SC_NO = "0";
                string strCELL_NO1 = "0";
                string strBOTTOM_TRAY1 = "0";
                string strTOP_TRAY1 = "0";
                string strDEST_POS1 = "0";
                string strJOB_PRIORITY = "119";
                DateTime dtAGING_END_DT = new DateTime();

                _pBdb.BeginTrans();
                strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString();
                strCELL_NO1 = "" + _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString();
                strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BOTTOM_TRAY"].ToString();
                strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["TOP_TRAY"].ToString();
                dtAGING_END_DT = Convert.ToDateTime(_pBdb.mDtMain.Rows[0]["AGING_END_DT"].ToString());

                switch (strAGING_TYP)
                {
                    case "291":
                        strDEST_POS1 = "03126"; // 상온1
                        break;
                    case "292":
                        if (strCELL_SC_NO.Substring(3, 1).ToString() == "3")
                        {
                            strDEST_POS1 = "04256"; // 상온2 - 3단
                        }
                        else
                        {
                            strDEST_POS1 = "07443"; // 상온2 - 1/2단
                        }
                        break;
                    case "294":
                        strDEST_POS1 = "07443"; // 고온1
                        break;
                    case "295":
                        strDEST_POS1 = "03126"; // 고온2
                        break;
                    default:
                        strDEST_POS1 = "07443";
                        break;
                }

                // 2.화재감지 출고작업을 생성한다. JOB_TYP = '8'
                if (InsertJobMst(strWH_TYP
                               , 8
                               , strCELL_SC_NO
                               , strCELL_NO1
                               , strDEST_POS1   // 수동출고대. (S/C에 따라 달라진다.)
                               , ""
                               , strCELL_SC_NO
                               , strTOP_TRAY1
                               , strBOTTOM_TRAY1
                               , dtAGING_END_DT
                               , strJOB_PRIORITY    // 119
                               , ref pRTN_MSG) < 1)
                {
                    _pBdb.Rollback();
                    return false;
                }

                _pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }
        //*/

        // Retrieve Check(RetHS ready)
        // RetHS Ready 신호가 올라오면서 출고작업이 있을 경우
        // CELL에서 Ret
        // 충방전기
        // CHECK_CV_RET_HS_READY_PCH
        /*
        public bool CHECK_CV_RET_HS_READY_PCH(string strWH_TYP,
                                              string strAGING_TYP,
                                              string strPLC_NO,
                                          ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_RETHS_READY_PCH::";

                strSql = "";
                strSql += cDefApp.CRLF + "SELECT SD.SC_NO, SD.SC_TYP, JM.*, SHD.HS_NO, SHD.HS_MC_NO, SHD.PAIR_HS_NO ";
                strSql += cDefApp.CRLF + "  FROM JOB_MST JM                  ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_HS_DEF SHD          ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP      = SHD.WH_TYP ";
                strSql += cDefApp.CRLF + "   AND JM.START_POS   = SHD.SC_NO  ";
                strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD             ";
                strSql += cDefApp.CRLF + "    ON JM.WH_TYP      = SD.WH_TYP  ";
                strSql += cDefApp.CRLF + "   AND JM.START_POS   = SD.SC_NO   ";
                strSql += cDefApp.CRLF + " INNER JOIN CV_DATA CD             ";
                strSql += cDefApp.CRLF + "    ON SHD.WH_TYP   = CD.WH_TYP    ";
                strSql += cDefApp.CRLF + "   AND SHD.HS_MC_NO = CD.TRACK_NO  ";
                strSql += cDefApp.CRLF + " WHERE JM.WH_TYP = ?                                     ";
                strSql += cDefApp.CRLF + "   AND JM.START_POS like '%' " + DbLang.II + " ? " + DbLang.II + " '%'                 ";
                strSql += cDefApp.CRLF + "   AND JM.JOB_STATUS      = '20'                               ";
                strSql += cDefApp.CRLF + "   AND SHD.DEST_DEF_DAT like '%' " + DbLang.II + " JM.DEST_POS " + DbLang.II + " '%'    ";
                strSql += cDefApp.CRLF + "   AND JM.DEST_POS        <> '0' ";
                strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'  ";
                strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'  ";
                strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD     = '0'  ";
                strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD  = '1'  ";
                strSql += cDefApp.CRLF + "   AND SD.UCSTATUS_RD     = '1'  ";
                strSql += cDefApp.CRLF + "   AND SD.SUSPEND        IN ('0', '1') ";
                strSql += cDefApp.CRLF + "   AND (SD.ITN_LUGG_FK1 = '0' AND SD.ITN_LUGG_FK2 = '0')  ";
                strSql += cDefApp.CRLF + "   AND SD.OD_RQ_YN        = 'N' ";
                strSql += cDefApp.CRLF + "   AND CD.RETHS_READY_RD  = '1' ";    // Ãâ°íH/S READY ON (È­¹° ¹× µ¥ÀÌÅÍ ¹ÌÁ¸Àç)
                strSql += cDefApp.CRLF + "   AND CD.OD_RQ_YN        = 'N' ";
                strSql += cDefApp.CRLF + "   AND CD.RET_HS_YN       = 'Y' ";
                strSql += cDefApp.CRLF + " ORDER BY JM.JOB_PRIORITY  DESC, JM.INS_DT ASC    ";
 
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strAGING_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "0";
                string strBOTTOM_TRAY1 = "0";
                string strTOP_TRAY1 = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";

                string strBOTTOM_TRAY2 = "0";
                string strTOP_TRAY2 = "0";
                string strDEST_POS2 = "0";
                string strLUGG_NO2 = "0";

                string strHS_MC_NO1 = "0";
                string strHS_MC_NO2 = "0";

                string strSOUR_BANK_FK1 = "0";
                string strSOUR_BAY_FK1 = "0";
                string strSOUR_LEV_FK1 = "0";
                string strDEST_HSPOS_FK1 = "0";
                string strCELL_NO1 = "0";

                string strSOUR_BANK_FK2 = "0";
                string strSOUR_BAY_FK2 = "0";
                string strSOUR_LEV_FK2 = "0";
                string strDEST_HSPOS_FK2 = "0";
                string strCELL_NO2 = "0";
                string strUSE_FK = "0";
                string strUSER_COMMAND = "0";
                string strWRITE_FLAG = "0";
                string strSC_TYP = "0";
                string strCELL_SC_NO = "0";

                string strDEST_BANK_FK1 = "0";
                string strDEST_BAY_FK1 = "0";
                string strDEST_LEV_FK1 = "0";
                string strSOUR_HSPOS_FK1 = "0";
                string strDEST_BANK_FK2 = "0";
                string strDEST_BAY_FK2 = "0";
                string strDEST_LEV_FK2 = "0";
                string strSOUR_HSPOS_FK2 = "0";
                string strSC_PLT_JOB_TYP = "0";

                bool bSuccess = false;

                _pBdb.BeginTrans();
                strSC_TYP = _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_TYP"].ToString();

                if (strSC_TYP == "TWIN")
                {
                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                        strCELL_NO1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString();
                        strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(0, 2) == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(0, 2);
                        strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(3, 3) == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(3, 3);
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(7, 2) == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(7, 2);
                        strDEST_HSPOS_FK1 = "" + _pBdb.mDtMain.Rows[0]["HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["HS_NO"].ToString();
                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_POS"].ToString();
                        strHS_MC_NO1 = "" + _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString();
                        strUSE_FK = "0";           

                        bSuccess = true;
                        if (i > 0)
                        {
                            if (strCELL_SC_NO != _pBdb.mDtMain.Rows[i]["SC_NO"].ToString() ||
                                strDEST_HSPOS_FK1 != _pBdb.mDtMain.Rows[i]["HS_NO"].ToString() ||
                                strWH_TYP != _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString())
                                continue;

                            strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                            strBOTTOM_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                            strTOP_TRAY2 = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                            strDEST_POS2 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                            strLUGG_NO2 = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                            //strCELL_NO2 = "" + _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["CELL_NO"].ToString();
                            strSOUR_BANK_FK2 = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString().Substring(0, 2) == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString().Substring(0, 2);
                            strSOUR_BAY_FK2 = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString().Substring(3, 3) == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString().Substring(3, 3);
                            strSOUR_LEV_FK2 = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString().Substring(7, 2) == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString().Substring(7, 2);
                            strDEST_HSPOS_FK2 = "" + _pBdb.mDtMain.Rows[i]["HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["HS_NO"].ToString();
                            strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                            strHS_MC_NO2 = "" + _pBdb.mDtMain.Rows[i]["PAIR_HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PAIR_HS_NO"].ToString();
                            strUSE_FK = "1";

                            break;
                        }
                    }

                }
                else
                {

                    for (int i = 0; i < nSelCnt; i++)
                    {
                        strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                        strLUGG_NO1 = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString();
                        strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString();
                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                        strCELL_NO1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString();
                        //strCELL_NO1_PAIR = "" + _pBdb.mDtMain.Rows[0]["PAIR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PAIR"].ToString();
                        //strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["BANK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BANK"].ToString();
                        //strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["BAY"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["BAY"].ToString();
                        //strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LEV"].ToString();
                        strSOUR_BANK_FK1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(0, 2) == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(0, 2);
                        strSOUR_BAY_FK1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(3, 3) == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(3, 3);
                        strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(7, 2) == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString().Substring(7, 2);

                        strDEST_HSPOS_FK1 = "" + _pBdb.mDtMain.Rows[0]["HS_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["HS_NO"].ToString();
                        strWH_TYP = "" + _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["WH_TYP"].ToString();
                        //strSOUR_LEV_FK1 = "" + _pBdb.mDtMain.Rows[0]["LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LEV"].ToString();
                        strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_POS"].ToString();
                        //strCELL_SC_NO = "" + _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_SC_NO"].ToString();
                        strHS_MC_NO1 = "" + _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString();
                        strUSE_FK = "0";

                        //strCELL_INOUT = "" + _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["CELL_INOUT"].ToString();
                        bSuccess = true;
                    }
                }

                if (bSuccess == true)
                {
                    // S/C에 지시처리.
                    if (UPDATE_SC_DATA(strJOB_TYP,
                                       strLUGG_NO1,
                                       strSOUR_BANK_FK1,
                                       strSOUR_BAY_FK1,
                                       strSOUR_LEV_FK1,
                                       strDEST_HSPOS_FK1,
                                       strLUGG_NO2,
                                       strSOUR_BANK_FK2,
                                       strSOUR_BAY_FK2,
                                       strSOUR_LEV_FK2,
                                       strDEST_HSPOS_FK2,
                                       strUSE_FK,
                                       strUSER_COMMAND,
                                       strWRITE_FLAG,
                                       strWH_TYP,
                                       strCELL_SC_NO,
                                       strDEST_BANK_FK1,
                                       strDEST_BAY_FK1,
                                       strDEST_LEV_FK1,
                                       strSOUR_HSPOS_FK1,
                                       strDEST_BANK_FK2,
                                       strDEST_BAY_FK2,
                                       strDEST_LEV_FK2,
                                       strSOUR_HSPOS_FK2,
                                       strSC_PLT_JOB_TYP,
                                       ref pRTN_MSG) == false )
                    {
                        return false;
                    }

                    // SC 구동시작 보고 
                    if (UPDATE_JOB_DATA("21", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS1, strCELL_NO1, strHS_MC_NO1, strCELL_SC_NO, "0", strLUGG_NO2) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // 출고예약처리.
                    if (Convert.ToInt32(strCELL_SC_NO) < 29700)
                    {
                        if (UPDATE_CELL_MST_DTL("3", strBOTTOM_TRAY1, strTOP_TRAY1, strWH_TYP, strCELL_SC_NO, strCELL_NO1, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                    }



                    // 2번포크
                    if (strUSE_FK == "1")
                    {
                        // SC 구동시작 보고 
                        if (UPDATE_JOB_DATA("21", strLUGG_NO2, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS2, strCELL_NO2, strHS_MC_NO2, strCELL_SC_NO, "0", strLUGG_NO1) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        // 출고예약처리.
                        //추후 상온 고온이 트윈이 되면 고려해 봐야될 상황 - 충방에서는 재고 관리가 필요없음
                        //if (UPDATE_CELL_MST_DTL("3", strBOTTOM_TRAY2, strTOP_TRAY2, strWH_TYP, strCELL_SC_NO, strCELL_NO2, ref pRTN_MSG) == false)
                        //{
                        //    _pBdb.Rollback();
                        //    return false;
                        //}
                        _pBdb.Commit();
                        return true;
                    }
                }

                _pBdb.Commit();
                return true;

            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }
        //*/

        // 입고H/S도착 후 타출고(직출고) 시작 로직.
        // CHECK_CV_HS_READY_OTHER_RET
        /*
        public bool CHECK_CV_HS_READY_OTHER_RET(string strWH_TYP,
                                          string strAGING_TYP,
                                      ref string pRTN_MSG)
        {


            try
            {
                int nJobCnt = 0;

                int nSelCnt = 0;
                string strSql = "";

                string strSTART_POS = "";
                string strSTART_TRACK = "";
                string strJOB_TYP = "0";
                string strDEST_POS1 = "0";
                string strLUGG_NO1 = "0";
                string strLUGG_NO2 = "0";
                string strHS_MC_NO2 = "0";

                string strDEST_LOCATION = "00-000-00";
                string strSOUR_BANK_FK1 = "0";
                string strSOUR_BAY_FK1 = "0";
                string strSOUR_LEV_FK1 = "0";
                string strDEST_HSPOS_FK1 = "0";
                string strSOUR_BANK_FK2 = "0";
                string strSOUR_BAY_FK2 = "0";
                string strSOUR_LEV_FK2 = "0";
                string strDEST_HSPOS_FK2 = "0";
                string strCELL_NO2 = "0";
                string strUSE_FK = "0";
                string strUSER_COMMAND = "0";
                string strWRITE_FLAG = "0";
                string strSC_TYP = "0";

                string strDEST_BANK_FK1 = "0";
                string strDEST_BAY_FK1 = "0";
                string strDEST_LEV_FK1 = "0";
                string strSOUR_HSPOS_FK1 = "0";
                string strDEST_BANK_FK2 = "0";
                string strDEST_BAY_FK2 = "0";
                string strDEST_LEV_FK2 = "0";
                string strSOUR_HSPOS_FK2 = "0";
                string strSC_PLT_JOB_TYP = "0";

                string strSC_NO = "0";
                pRTN_MSG = "CHECK_CV_HS_READY_OTHER_RET::";

                strSql = "";
                strSql += CRLF + " SELECT JM.JOB_TYP, JM.DEST_POS, JM.LUGG_NO, CD.*, SD.*";
                strSql += CRLF + "   FROM CV_DATA CD       ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM  ";
                strSql += CRLF + "     ON CD.WH_TYP        = JM.WH_TYP  ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD    = JM.LUGG_NO ";
                strSql += CRLF + "  INNER JOIN SC_HS_DEF SHD             ";
                strSql += CRLF + "     ON CD.WH_TYP      = SHD.WH_TYP     ";
                strSql += CRLF + "    AND CD.TRACK_NO    = SHD.HS_MC_NO   ";
                strSql += CRLF + "  INNER JOIN SC_DATA SD  ";
                strSql += CRLF + "     ON JM.WH_TYP        = SD.WH_TYP ";
                strSql += CRLF + "    AND SHD.SC_NO        = SD.SC_NO  ";
                strSql += CRLF + "  WHERE CD.WH_TYP		    = :WH_TYP   ";
                strSql += CRLF + "    AND CD.STOHS_READY_RD = '1' ";  // 입고 H/S READY ON
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1' ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1' ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N' ";
                //strSql += CRLF + "    AND CD.ERR_RQ_RD	        = '0' ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += CRLF + "    AND SD.OD_RQ_YN           = 'N' ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK1       = '0' ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK2       = '0' ";
                strSql += CRLF + "    AND SD.ONLINE_MODE_RD     = '1' ";
                strSql += CRLF + "    AND SD.AUTO_MODE_RD       = '1' ";
                strSql += CRLF + "    AND SD.ERR_CODE_RD        = '0' ";
                strSql += CRLF + "    AND SD.ACTIVE_MODE_RD     = '1' ";
                strSql += CRLF + "    AND SD.UCSTATUS_RD        = '1' ";  // [대기중/동작중]
                strSql += CRLF + "    AND SD.SC_NO like      '%' " + DbLang.II + " :AGING_TYP " + DbLang.II + " '%' ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '11' ";
                strSql += CRLF + "    AND JM.JOB_TYP 	        = '7' ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null     ";
                strSql += CRLF + "  ORDER BY JM.INS_DT, CD.TRACK_NO   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("AGING_TYP", DbLang.VARCHAR).Value = strAGING_TYP;
                nJobCnt = _pBdb.ExcuteQry(strSql);
                if (nJobCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nJobCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";

                    return true;
                }

                _pBdb.BeginTrans();

                DataTable dtSC_HS_DEF = new DataTable();
                DataTable dtCV_RET_HS_DEF = new DataTable();

                for (int i = 0; i < nJobCnt; i++)
                {
                    strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();                                 
                    strDEST_POS1 = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();       
                    strLUGG_NO1 = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();                           
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString();
                    strSC_NO = "" + _pBdb.mDtMain.Rows[0]["SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_NO"].ToString();
                    strSTART_TRACK = "" + _pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString();

                    // 입고 H/S정보가져오기.
                    strSql = "";
                    strSql += CRLF + " SELECT SHD.*                      ";
                    strSql += CRLF + "   FROM SC_HS_DEF SHD              ";
                    strSql += CRLF + "  WHERE SHD.WH_TYP   = :WH_TYP     ";
                    strSql += CRLF + "    AND SHD.HS_MC_NO = :HS_MC_NO   ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("HS_MC_NO", DbLang.VARCHAR).Value = strSTART_POS;
                    nSelCnt = _pBdb.ExcuteQry(dtSC_HS_DEF, strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }
                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "조회할 H/S TRACK 정보가 존재하지 않습니다. HS_MC_NO : " + strSTART_POS;
                        _pBdb.Rollback();
                        return false;
                    }

                    strSOUR_HSPOS_FK1 = dtSC_HS_DEF.Rows[0]["HS_NO"].ToString() == "" ? "0" : dtSC_HS_DEF.Rows[0]["HS_NO"].ToString();

                    strSql = "";
                    strSql += cDefApp.CRLF + "SELECT JM.*, SHD.HS_NO, SHD.HS_MC_NO  ";
                    strSql += cDefApp.CRLF + "  FROM JOB_MST JM         ";          
                    strSql += cDefApp.CRLF + " INNER JOIN SC_HS_DEF SHD             ";
                    strSql += cDefApp.CRLF + "    ON JM.WH_TYP      = SHD.WH_TYP     ";
                    strSql += cDefApp.CRLF + "   AND :SC_NO         = SHD.SC_NO      ";
                    strSql += cDefApp.CRLF + " INNER JOIN CV_DATA CD ";
                    strSql += cDefApp.CRLF + "    ON SHD.WH_TYP     = CD.WH_TYP   ";
                    strSql += cDefApp.CRLF + "   AND SHD.HS_MC_NO   = CD.TRACK_NO ";
                    strSql += cDefApp.CRLF + " WHERE JM.WH_TYP      = :WH_TYP        ";
                    strSql += cDefApp.CRLF + "   AND SHD.DEST_DEF_DAT like '%' " + DbLang.II + " JM.DEST_POS " + DbLang.II + " '%' ";
                    strSql += cDefApp.CRLF + "   AND CD.RETHS_READY_RD  = '1'   ";  // DATA 없을때 출고H/S READY ON
                    strSql += cDefApp.CRLF + "   AND CD.OD_RQ_YN        = 'N'   ";
                    strSql += cDefApp.CRLF + "   AND CD.RET_HS_YN       = 'Y'   ";
                    strSql += cDefApp.CRLF + "   AND JM.DEST_POS        <> '0'  ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    nSelCnt = _pBdb.ExcuteQry(dtCV_RET_HS_DEF, strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        pRTN_MSG = "";
                        return true;
                    }

                    strDEST_HSPOS_FK1 = dtCV_RET_HS_DEF.Rows[0]["HS_NO"].ToString() == "" ? "0" : dtCV_RET_HS_DEF.Rows[0]["HS_NO"].ToString();

                    if (i > 0 && strSC_TYP == "TWIN")
                    {
                        if (_pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() != _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString())
                            continue;

                        strDEST_POS1 = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                        strLUGG_NO2 = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();

                        strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();

                        strSOUR_HSPOS_FK2 = strSOUR_HSPOS_FK1;
                        strDEST_HSPOS_FK2 = strDEST_HSPOS_FK1;

                        break;
                    }
                }

                if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.OtherRet)
                {
                    // S/C에 작업정보쓰기 UPDATE
                    if (UPDATE_SC_DATA("3",
                                       strLUGG_NO1,
                                       strSOUR_BANK_FK1,
                                       strSOUR_BAY_FK1,
                                       strSOUR_LEV_FK1,
                                       strDEST_HSPOS_FK1,
                                       strLUGG_NO2,
                                       strSOUR_BANK_FK2,
                                       strSOUR_BAY_FK2,
                                       strSOUR_LEV_FK2,
                                       strDEST_HSPOS_FK2,
                                       strUSE_FK,
                                       strUSER_COMMAND,
                                       strWRITE_FLAG,
                                       strWH_TYP,
                                       strSC_NO,
                                       strDEST_BANK_FK1,
                                       strDEST_BAY_FK1,
                                       strDEST_LEV_FK1,
                                       strSOUR_HSPOS_FK1,
                                       strDEST_BANK_FK2,
                                       strDEST_BAY_FK2,
                                       strDEST_LEV_FK2,
                                       strSOUR_HSPOS_FK2, strSC_PLT_JOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // 작업정보를 입고시작 상태로 변경
                    if (UPDATE_JOB_DATA("21", strLUGG_NO1, strWH_TYP, strJOB_TYP, ref pRTN_MSG, "0", strDEST_LOCATION, strSTART_TRACK, strSC_NO, "0", strLUGG_NO2) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    if (strUSE_FK == "1")   // USE_FK = 0 (1번포크) USE_FK = 1 (2번포크) 
                    {
                        if (UPDATE_JOB_DATA("21", strLUGG_NO2, strWH_TYP, strJOB_TYP, ref pRTN_MSG, "0", strCELL_NO2, strHS_MC_NO2, strSC_NO, "0", strLUGG_NO1) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }
                        _pBdb.Commit();
                        return true;
                    }
                }

                dtSC_HS_DEF.Dispose();
                dtCV_RET_HS_DEF.Dispose(); 
                _pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        //*/

        // 완료재보고
        // CHECK_SC_COMPLETE_RETRY
        /*
        public bool CHECK_SC_COMPLETE_RETRY(string strWH_TYP
                                      , ref string pRTN_MSG)
        {
            try
            {
                int nJobCnt = 0;

                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_SC_COMPLETE_RETRY::";

                strSql = "";
                strSql += CRLF + " SELECT JM.*                                  ";
                strSql += CRLF + "   FROM JOB_MST JM                            ";
                strSql += CRLF + "  WHERE JM.JOB_STATUS        = '28'           ";
                strSql += CRLF + "    AND JM.WH_TYP            = :WH_TYP        ";
                strSql += CRLF + "    AND JM.JOB_TYP           <> '8'           ";
                strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - JM.UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nJobCnt = _pBdb.ExcuteQry(strSql);
                if (nJobCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nJobCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO = "";

                _pBdb.BeginTrans();

                for (int i = 0; i < nJobCnt; i++)
                {
                    strLUGG_NO = _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " UPDATE JOB_MST                  ";
                    strSql += CRLF + "    SET JOB_STATUS    = '29'     ";
                    strSql += CRLF + "       ,UPD_DT        = " + DbLang.SYSDATE + "  ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP  ";
                    strSql += CRLF + "    AND LUGG_NO       = :LUGG_NO ";
                    strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }
                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "변경할 작업정보가 존재하지 않습니다. LUGG NO : " + strLUGG_NO;
                        return false;
                    }
                }

                _pBdb.Commit();

                string strLOG = "[IO TASK] 3분 뒤 완료보고 재보고  (LUGG NO : " + strLUGG_NO + ")";
                InsertLog(strWH_TYP, strLOG);
                return true;

            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }
        //*/

        // C/V에 목적지정보를 적는다.
        // UPDATE_CV_DATA
        /*
        public bool UPDATE_CV_DATA(string strJOB_TYP,
                                   string strTRAY_TYP,
                                   string strTRAY_LEV,
                                   string strDEST_POS,
                                   string strIS_TURN,
                                   string strLUGG_NO,
                                   string strWH_TYP,
                                   string strPLC_NO,
                                   string strSTART_POS,
                               ref string pRTN_MSG,
                                   string strBCR_BOTTOM = "",
                                   string strBCR_TOP = ""
                                    )
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "UPDATE_CV_DATA::";

                strSql = "";
                strSql += CRLF + " UPDATE CV_DATA                       ";
                strSql += CRLF + "    SET JOB_TYP_OD    = :JOB_TYP_OD   ";
                strSql += CRLF + "      , TRAY_TYP_OD   = :TRAY_TYP_OD  ";
                strSql += CRLF + "      , TRAY_LEV_OD   = :TRAY_LEV_OD  ";
                strSql += CRLF + "      , DEST_POS_OD   = :DEST_POS_OD  ";
                strSql += CRLF + "      , IS_TURN_OD    = :IS_TURN_OD   ";
                strSql += CRLF + "      , LUGG_NO_OD    = :LUGG_NO_OD   ";
                strSql += CRLF + "      , BCR_BOTTOM    = :BCR_BOTTOM   ";
                strSql += CRLF + "      , BCR_TOP       = :BCR_TOP      ";
                strSql += CRLF + "      , OD_RQ_YN      = 'Y'           ";
                strSql += CRLF + "      , OD_USER_ID    = 'IOTASK'      ";
                strSql += CRLF + "      , OD_UPD_DT     = " + DbLang.SYSDATE + "       ";
                strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP       ";
                strSql += CRLF + "    AND PLC_NO        = :PLC_NO       ";
                strSql += CRLF + "    AND TRACK_NO      = :TRACK_NO     ";
                strSql += CRLF + "    AND OD_RQ_YN      = 'N'           ";
                strSql += CRLF + "    AND ERROR_CODE    = '0'           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                _pBdb.mComMain.Parameters.Add("TRAY_TYP_OD", DbLang.VARCHAR).Value = strTRAY_TYP;
                _pBdb.mComMain.Parameters.Add("TRAY_LEV_OD", DbLang.VARCHAR).Value = strTRAY_LEV;
                _pBdb.mComMain.Parameters.Add("DEST_POS_OD", DbLang.VARCHAR).Value = strDEST_POS;
                _pBdb.mComMain.Parameters.Add("IS_TURN_OD", DbLang.VARCHAR).Value = strIS_TURN;
                _pBdb.mComMain.Parameters.Add("LUGG_NO_OD", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("BCR_BOTTOM", DbLang.VARCHAR).Value = strBCR_BOTTOM;
                _pBdb.mComMain.Parameters.Add("BCR_TOP", DbLang.VARCHAR).Value = strBCR_TOP;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strSTART_POS;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "변경할 CV_DATA 정보가 존재하지 않습니다. TRACK_NO : " + strSTART_POS;
                    return false;
                }

                strSql = "";
                strSql += CRLF + " UPDATE BCR_MST                    ";
                strSql += CRLF + "    SET BCR_STA    =  '2'          ";
                strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP       ";
                strSql += CRLF + "    AND BCR_MC_NO  = :BCR_MC_NO    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("BCR_MC_NO", DbLang.VARCHAR).Value = strSTART_POS;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "변경할 BCR_MST 정보가 존재하지 않습니다. BCR_MC_NO : " + strSTART_POS;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }
        //*/

        // Fire 비트 기록
        // UPDATE_SC_FIRE
        /*
        public bool UPDATE_SC_FIRE(string strWH_TYP
                                 , string strSC_NO
                                 , string strCELL_NO
                                 , ref string pRTN_MSG)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                pRTN_MSG += "UPDATE_SC_FIRE::";

                strSql = "";
                strSql += CRLF + " UPDATE SC_DATA                 ";
                strSql += CRLF + "    SET SC_FIRE_OD =  '57344'   ";
                strSql += CRLF + "      , OD_USER_ID =  'IO TASK' ";
                strSql += CRLF + "  WHERE WH_TYP     =  :WH_TYP   ";
                strSql += CRLF + "    AND SC_NO      =  :SC_NO    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = "S/C 초기화 실패 S/C NO : " + strSC_NO;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }
        //*/

        // CELL정보 UPDATE처리 및 재고정보 삭제 및 등록처리.
        // UPDATE_CELL_MST_DTL
        /*
        public bool UPDATE_CELL_MST_DTL(string strCELL_STA,
                                        string BOTTOM_TRAY,
                                        string TOP_TRAY,
                                        string strWH_TYP,
                                        string strCELL_SC_NO,
                                        string strCELL_NO,
                                    ref string strRTN_MSG,
                                        DateTime dtEND_TIME = new DateTime())
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                strRTN_MSG += "UPDATE_CELL_MST_DTL::";

                strSql = "";
                strSql += CRLF + " UPDATE CELL_MST                     ";
                strSql += CRLF + "    SET CELL_STA    = :CELL_STA      ";
                strSql += CRLF + "      , BOTTOM_TRAY = :BOTTOM_TRAY   ";
                strSql += CRLF + "      , TOP_TRAY    = :TOP_TRAY      ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + "        ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP        ";
                strSql += CRLF + "    AND CELL_NO     = :CELL_NO       ";
                strSql += CRLF + "    AND CELL_SC_NO  = :CELL_SC_NO    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("CELL_STA", DbLang.VARCHAR).Value = strCELL_STA;
                _pBdb.mComMain.Parameters.Add("BOTTOM_TRAY", DbLang.VARCHAR).Value = BOTTOM_TRAY;
                _pBdb.mComMain.Parameters.Add("TOP_TRAY", DbLang.VARCHAR).Value = TOP_TRAY;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CELL_NO", DbLang.VARCHAR).Value = strCELL_NO;
                _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strCELL_SC_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "변경할 CELL_MST 정보가 존재하지 않습니다. CELL NO : " + strCELL_NO;
                    return false;
                }

                if (strCELL_STA == "0") // 공CELL
                {
                    // 출고완료
                    strRTN_MSG += "UPDATE_CELL_MST::";

                    strSql = "";
                    strSql += CRLF + " DELETE CELL_DTL                             ";
                    strSql += CRLF + "  WHERE WH_TYP       =  :WH_TYP              ";
                    strSql += CRLF + "    AND CELL_NO      =  :CELL_NO             ";
                    strSql += CRLF + "    AND CELL_SC_NO   =  :CELL_SC_NO          ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("CELL_NO", DbLang.VARCHAR).Value = strCELL_NO;
                    _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strCELL_SC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt < 0)
                    {
                        strRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        strRTN_MSG += "삭제할 CELL_DTL 정보가 존재하지 않습니다. CELL NO : " + strCELL_NO;
                        return false;
                    }
                }
                else if (strCELL_STA == "2")
                {
                    // 출고완료
                    strRTN_MSG += "UPDATE_CELL_MST::";

                    strSql = "";
                    strSql += cDefApp.CRLF + " INSERT INTO CELL_DTL (                 ";
                    strSql += cDefApp.CRLF + "                      WH_TYP,           ";
                    strSql += cDefApp.CRLF + "                      CELL_NO,          ";
                    strSql += cDefApp.CRLF + "                      CELL_SC_NO,       ";
                    strSql += cDefApp.CRLF + "                      CELL_SEQ,         ";
                    strSql += cDefApp.CRLF + "                      BANK,             ";
                    strSql += cDefApp.CRLF + "                      BAY,              ";
                    strSql += cDefApp.CRLF + "                      LEV,              ";
                    strSql += cDefApp.CRLF + "                      AGING_START_DT,   ";
                    strSql += cDefApp.CRLF + "                      AGING_END_DT,     ";
                    strSql += cDefApp.CRLF + "                      USER_REMARKS,     ";
                    strSql += cDefApp.CRLF + "                      REMARKS,          ";
                    strSql += cDefApp.CRLF + "                      INS_DT,           ";
                    strSql += cDefApp.CRLF + "                      INS_USER_ID,      ";
                    strSql += cDefApp.CRLF + "                      UPD_DT,           ";
                    strSql += cDefApp.CRLF + "                      UPD_USER_ID,      ";
                    strSql += cDefApp.CRLF + "                      BOTTOM_TRAY,      ";
                    strSql += cDefApp.CRLF + "                      TOP_TRAY          ";
                    strSql += cDefApp.CRLF + "                      )                 ";  
                    strSql += cDefApp.CRLF + "              VALUES (                  ";
                    strSql += cDefApp.CRLF + "                      :WH_TYP ,         ";
                    strSql += cDefApp.CRLF + "                      :CELL_NO ,        ";
                    strSql += cDefApp.CRLF + "                      :CELL_SC_NO ,     ";
                    strSql += cDefApp.CRLF + "                      '1',              ";
                    strSql += cDefApp.CRLF + "                      :BANK ,           ";
                    strSql += cDefApp.CRLF + "                      :BAY ,            ";
                    strSql += cDefApp.CRLF + "                      :LEV ,            ";
                    strSql += cDefApp.CRLF + "                      " + DbLang.SYSDATE + " ,         ";
                    strSql += cDefApp.CRLF + "                      :AGING_END_DT ,   ";
                    strSql += cDefApp.CRLF + "                      '' ,              ";
                    strSql += cDefApp.CRLF + "                      '' ,              ";
                    strSql += cDefApp.CRLF + "                      " + DbLang.SYSDATE + " ,         ";
                    strSql += cDefApp.CRLF + "                      'IOTASK',         ";
                    strSql += cDefApp.CRLF + "                      " + DbLang.SYSDATE + " ,         ";
                    strSql += cDefApp.CRLF + "                      'IOTASK',         ";
                    strSql += cDefApp.CRLF + "                      :BOTTOM_TRAY ,    ";
                    strSql += cDefApp.CRLF + "                      :TOP_TRAY         ";
                    strSql += cDefApp.CRLF + "                      )                 ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("CELL_NO", DbLang.VARCHAR).Value = strCELL_NO;
                    _pBdb.mComMain.Parameters.Add("CELL_SC_NO", DbLang.VARCHAR).Value = strCELL_SC_NO;
                    _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strCELL_NO.Substring(0, 2);
                    _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strCELL_NO.Substring(3, 3);
                    _pBdb.mComMain.Parameters.Add("LEV", DbLang.VARCHAR).Value = strCELL_NO.Substring(7, 2);
                    _pBdb.mComMain.Parameters.Add("AGING_END_DT", DbLang.DATE).Value = dtEND_TIME;
                    _pBdb.mComMain.Parameters.Add("BOTTOM_TRAY", DbLang.VARCHAR).Value = BOTTOM_TRAY;
                    _pBdb.mComMain.Parameters.Add("TOP_TRAY", DbLang.VARCHAR).Value = TOP_TRAY;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt < 0)
                    {
                        strRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        strRTN_MSG += "등록할 CELL_DTL 정보가 존재하지 않습니다. CELL NO : " + strCELL_NO;
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                strRTN_MSG += ex.ToString();
                return false;
            }
        }
        //*/

        // LOCATION 구하기.
        // SET_NEXT_LEV
        /*
        public bool SET_NEXT_LEV(string strWH_TYP,
                                 string strDEST_POS,
                                 string strPLC_NO,
                                 string strDURATION_TIME,
                                 ref string strAGING_TYP,
                                 ref string strDEST_LOCATION,
                                 ref string pRTN_MSG)
        {

            try
            {
                // 이중입고 발생 시 새로운 LOCATION 가져오기.
                if (FETCH_LOCATION_BY_AGING_TYP_DUPLICATE(strWH_TYP
                                                        , strDURATION_TIME
                                                        , strDEST_POS
                                                        , ref strDEST_LOCATION
                                                        , ref pRTN_MSG) == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }
        //*/

        // 이중입고 전용으로 새로운 LOCATION 가져오기.
        // FETCH_LOCATION_BY_AGING_TYP_DUPLICATE
        /*
        public bool FETCH_LOCATION_BY_AGING_TYP_DUPLICATE(string strWH_TYP,
                                                string strDURATION_TIME,
                                                string strSC_NO_LEV,
                                            ref string strDEST_LOCATION,
                                            ref string pRTN_MSG)
        {
            int nSelCnt = 0;
            string strSql = "";

            IsRtnMsg = "FETCH_LOCATION_BY_AGING_TYP_DUPLICATE::";

            try
            {
                DataTable dtSYS_DATE = new DataTable();
                DataTable dtCELL_MST = new DataTable();

                IsRtnMsg = "UPDATE_CELL_MST_RESERVATION::";
                // 작업예정시간(DURATION_TIME [MES수신])
                strSql = "";
                strSql += cDefApp.CRLF + "SELECT " + DbLang.SYSDATE + " + (1/24/60/60)*((TO_NUMBER(SUBSTR(:DURATION_TIME ,1 ,3)) * 60 * 60 * 24)    ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME ,4 ,2)) * 60 * 60)       ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME ,6 ,2)) * 60)            ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME ,8 ,2)))) AS END_TIME    ";
                strSql += cDefApp.CRLF + " from DUAL ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("DURATION_TIME", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME", DbLang.VARCHAR).Value = strDURATION_TIME;
                nSelCnt = _pBdb.ExcuteQry(dtSYS_DATE, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt > 0)
                {
                    DateTime dtEND_TIME = new DateTime();
                    dtEND_TIME = Convert.ToDateTime(dtSYS_DATE.Rows[0]["END_TIME"].ToString());

                    strSql = "";

                    // MES수신시간이 존재하면,
                    if (strDURATION_TIME != "000000000")
                    {
                        // LOCATION 구한다.
                        // ● / ●    
                        // (OUT LOCATION - IN LOCATION 시간[기존에 적재되어있는 OUT과 현재 입고중인 화물에 대한 출고예정시간을 비교한다.]이 3분이내)
                        strSql += cDefApp.CRLF + "SELECT 1 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                        strSql += cDefApp.CRLF + "      , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                        strSql += cDefApp.CRLF + "  FROM CELL_MST CM        ";
                        strSql += cDefApp.CRLF + " INNER JOIN CELL_MST CM2  ";
                        strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = CM2.WH_TYP ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_SC_NO = :SC_NO_LEV1 ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO     = CM2.PAIR   ";
                        strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD             ";
                        strSql += cDefApp.CRLF + "    ON CM.WH_TYP      = SD.WH_TYP  ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = SD.SC_NO   ";
                        strSql += cDefApp.CRLF + "   AND SD.SUSPEND     IN('0', '2')        ";
                        strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD <> '0'              "; // ERROR CODE가 존재해야한다.
                        strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD    = '1'           "; // 자동모드
                        strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD <> '1'           "; // DE ACTIVE
                        strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD  = '1'           "; // ONLINE MODE
                        strSql += cDefApp.CRLF + "  LEFT OUTER JOIN CELL_DTL CD             ";
                        strSql += cDefApp.CRLF + "    ON CM2.WH_TYP     = CD.WH_TYP      ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_SC_NO = CD.CELL_SC_NO  ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_NO    = CD.CELL_NO     ";
                        strSql += cDefApp.CRLF + " WHERE CM.WH_TYP      = :WH_TYP1    ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = :SC_NO_LEV2 ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_INOUT  = 'IN'       ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_STA IN ('0')        ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_USE_DEF IN ('0', '2')    ";
                        strSql += cDefApp.CRLF + "   AND CM2.CELL_STA IN ('2')       ";
                        strSql += cDefApp.CRLF + "   AND (((CD.AGING_END_DT - :END_TIME1) * 24 * 60) > 2 AND  "; // 2분초과 및 10분미만.
                        strSql += cDefApp.CRLF + "        ((CD.AGING_END_DT - :END_TIME2) * 24 * 60) < 1000)    ";
                        // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST    ";
                        strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                        strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO      ";
                        strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO)  ";
                        strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))             ";
                        strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                        strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                        strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                        strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                        strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))             ";
                        strSql += cDefApp.CRLF + " UNION ALL                                                 ";
                    }
                    // OUT LOCATION 검색.
                    strSql += cDefApp.CRLF + " SELECT 2 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                    strSql += cDefApp.CRLF + "       , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                    strSql += cDefApp.CRLF + "   FROM CELL_MST CM        ";
                    strSql += cDefApp.CRLF + "  INNER JOIN CELL_MST CM2  ";
                    strSql += cDefApp.CRLF + "     ON CM.WH_TYP           = CM2.WH_TYP  ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_SC_NO      = :SC_NO_LEV_A  ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_NO          = CM2.PAIR    ";
                    strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD                   ";
                    strSql += cDefApp.CRLF + "     ON CM.WH_TYP           = SD.WH_TYP   ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO       = SD.SC_NO    ";
                    strSql += cDefApp.CRLF + "    AND SD.SUSPEND          IN('0', '2')  ";
                    strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD      <> '0'        ";  // ERROR CODE가 존재해야한다.
                    strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD     = '1'         ";  // 자동모드
                    strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD   <> '1'        ";  // DE ACTIVE
                    strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD   = '1'         ";  // ONLINE MODE
                    strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP           = :WH_TYP_A     ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO       = :SC_NO_LEV_A2  ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_INOUT       = 'OUT'       ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_USE_DEF IN ('0', '2')    ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_STA IN ('0')              ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_STA IN ('0')             ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_USE_DEF IN ('0', '2')    ";
                    // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST    ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO      ";
                    strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO)  ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))             ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))             ";
                    strSql += cDefApp.CRLF + "  UNION ALL                               ";

                    // RACK TO RACK을 해도상관없는 비어있는 IN LOCATION검색.
                    strSql += cDefApp.CRLF + " SELECT 3 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                    strSql += cDefApp.CRLF + "       , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                    strSql += cDefApp.CRLF + "   FROM CELL_MST CM        ";
                    strSql += cDefApp.CRLF + "  INNER JOIN CELL_MST CM2  ";
                    strSql += cDefApp.CRLF + "     ON CM.WH_TYP      = CM2.WH_TYP ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_SC_NO = :SC_NO_LEV_B ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_NO     = CM2.PAIR   ";
                    strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD             ";
                    strSql += cDefApp.CRLF + "     ON CM.WH_TYP             = SD.WH_TYP  ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO         = SD.SC_NO   ";
                    strSql += cDefApp.CRLF + "    AND SD.SUSPEND           IN('0', '2')  ";
                    strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD     <> '0'       ";   // ERROR CODE가 존재해야한다.
                    strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD    = '1'        "; // 자동모드
                    strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD  <> '1'       ";
                    strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD  = '1'        "; // ONLINE MODE
                    strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP      = :WH_TYP_B      ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO  = :SC_NO_LEV_B2 ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_INOUT  = 'IN'       ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_STA IN ('0')        ";
                    strSql += cDefApp.CRLF + "    AND CM.CELL_USE_DEF IN ('0', '2') ";
                    strSql += cDefApp.CRLF + "    AND CM2.CELL_STA IN ('2')       ";
                    // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST    ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO      ";
                    strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO)  ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))             ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))             ";
                    strSql += cDefApp.CRLF + "  UNION ALL ";
                    strSql += cDefApp.CRLF + "SELECT 4 AS SEQ, CM.CELL_NO, CM.BANK, CM.BAY, CM.LEV, CM.CELL_SC_NO           ";
                    strSql += cDefApp.CRLF + "      , RANK() OVER (ORDER BY CM.BAY, CM.LEV, CM.BANK) AS RANK_ORDER_BY       ";
                    strSql += cDefApp.CRLF + "  FROM CELL_MST CM                         ";
                    strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD                     ";
                    strSql += cDefApp.CRLF + "    ON CM.WH_TYP              = SD.WH_TYP  ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO          = SD.SC_NO   ";
                    strSql += cDefApp.CRLF + "   AND SD.SUSPEND            IN('0', '2')  ";
                    strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD         <> '0'       ";
                    strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD        = '1'        "; // 자동모드
                    strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD      <> '1'       ";
                    strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD      = '1'        "; // ONLINE MODE
                    strSql += cDefApp.CRLF + " WHERE CM.WH_TYP      = :WH_TYP_C    ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO  = :SC_NO_LEV_C ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_STA IN ('0')        ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_USE_DEF IN ('0', '2')    ";
                    // 2019.10.23 09:00 RACK TO RACK 또는 출고작업시 목적지, 출발지에 해당하는 LOCATION과 일치하지 않아야한다.
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT DEST_LOCATION FROM JOB_MST    ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND DEST_POS = CM.CELL_SC_NO      ";
                    strSql += cDefApp.CRLF + "                             AND (DEST_LOCATION = CM.CELL_NO)  ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('4'))             ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_NO NOT IN (SELECT START_LOCATION FROM JOB_MST   ";
                    strSql += cDefApp.CRLF + "                           WHERE WH_TYP   = CM.WH_TYP          ";
                    strSql += cDefApp.CRLF + "                             AND START_POS = CM.CELL_SC_NO     ";
                    strSql += cDefApp.CRLF + "                             AND (START_LOCATION = CM.CELL_NO) ";
                    strSql += cDefApp.CRLF + "                             AND JOB_TYP IN ('2'))             ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    if (strDURATION_TIME != "000000000")
                    {
                        _pBdb.mComMain.Parameters.Add("SC_NO_LEV1", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        _pBdb.mComMain.Parameters.Add("WH_TYP1", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("SC_NO_LEV2", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        _pBdb.mComMain.Parameters.Add("END_TIME1", DbLang.DATE).Value = dtEND_TIME;
                        _pBdb.mComMain.Parameters.Add("END_TIME2", DbLang.DATE).Value = dtEND_TIME;
                    }
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_A", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    _pBdb.mComMain.Parameters.Add("WH_TYP_A", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_A2", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_B", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    _pBdb.mComMain.Parameters.Add("WH_TYP_B", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_B2", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    _pBdb.mComMain.Parameters.Add("WH_TYP_C", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO_LEV_C", DbLang.VARCHAR).Value = strSC_NO_LEV;
                    nSelCnt = _pBdb.ExcuteQry(dtCELL_MST, strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        dtSYS_DATE.Dispose();
                        dtCELL_MST.Dispose();

                        pRTN_MSG += "입고가능한 CELL이 없습니다.";
                        return false;
                    }
                    if (nSelCnt > 0)
                    {
                        strDEST_LOCATION = "" + dtCELL_MST.Rows[0]["CELL_NO"].ToString();

                        dtSYS_DATE.Dispose();
                        dtCELL_MST.Dispose();
                        return true;
                    }
                }

                dtSYS_DATE.Dispose();
                dtCELL_MST.Dispose();

                return false;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }
        //*/

        // DEST POS 크레인일 경우 크레인 번호(LINE 번호 구해오기)
        // GET_LESS_AGING
        /*
        public bool GET_LESS_AGING(string strDEST_POS,
                                   string strWH_TYP, 
                               ref string strAGING_TYP,
                               ref string pRTN_MSG)
        {
            DataTable dtCELL_MST = new DataTable();
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                pRTN_MSG += "GET_EMPTY_CELL::";

                strSql = "";
                strSql += CRLF + "  SELECT AGING_TYP                    ";
                strSql += CRLF + "       , " + DbLang.NVL + "(COUNT(*),0) as total     ";
                strSql += CRLF + "       , " + DbLang.NVL + "(SUM(CASE WHEN CELL_STA = '0' AND CELL_USE_DEF = '0' THEN 1 END), 0) AS EMPTY ";
                strSql += CRLF + "       , " + DbLang.NVL + "(SUM(CASE WHEN CELL_STA = '0' AND CELL_USE_DEF = '0' THEN 1 END), 0) / COUNT(*) * 100  AS EMPTY_USAGE ";
                strSql += CRLF + "    FROM CELL_MST CM                  ";
                strSql += CRLF + "   INNER JOIN SC_DATA SD              ";
                strSql += CRLF + "      ON CM.WH_TYP         = SD.WH_TYP ";
                strSql += CRLF + "     AND CM.CELL_SC_NO     = SD.SC_NO  ";
                strSql += CRLF + "   WHERE CM.WH_TYP         = :WH_TYP ";
                strSql += CRLF + "     AND CM.AGING_TYP like '%' " + DbLang.II + " :AGING_TYP " + DbLang.II + " '%' ";
                strSql += CRLF + "     AND SD.ONLINE_MODE_RD = '1' ";
                strSql += CRLF + "     AND SD.AUTO_MODE_RD   = '1' ";
                strSql += CRLF + "     AND SD.ERR_CODE_RD    = '0' ";
                strSql += CRLF + "     AND SD.ACTIVE_MODE_RD = '1' ";
                strSql += CRLF + "     AND SD.SUSPEND        IN ('0', '2') ";
                strSql += CRLF + "   GROUP BY CM.AGING_TYP       ";
                strSql += CRLF + "   ORDER BY EMPTY_USAGE desc   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("AGING_TYP", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                nSelCnt = _pBdb.ExcuteQry(dtCELL_MST, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "변경할 AGING_TYP 정보가 존재하지 않습니다. AGING_TYP : " + strDEST_POS.Substring(0, 3);
                    return false;
                }

                strAGING_TYP = dtCELL_MST.Rows[0]["AGING_TYP"].ToString();

                dtCELL_MST.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }
        //*/

        // 작업정보생성.
        // InsertJobMst
        /*
        public int InsertJobMst(string strWH_TYP
                              , int nJobType
                              , string strStartPos
                              , string strStartLocation
                              , string strDestPos
                              , string strDestLocation
                              , string strSC_NO
                              , string strTopBcr
                              , string strBottomBcr
                              , DateTime dtAGING_END_DT
                              , string strJOB_PRIORITY
                              , ref string pRTN_MSG)
        {
            string strSql;
            string strLuggNo = "0";
            int nSelCnt = 0;

            bool bSuccess = false;
            DataTable dtLUGG_NO = new DataTable();
            DataTable dtJOB_MST = new DataTable();

            try
            {
                while (bSuccess == false)
                {
                    strSql = "";
                    if (nJobType == 1)
                    {
                        //strSql += cDefApp.CRLF + " SELECT STO_SEQ.NEXTVAL AS LUGG_NO FROM DUAL              ";
                        strSql += cDefApp.CRLF + " SELECT " + DbLang.NEXTVAL("STO_SEQ") + " AS LUGG_NO FROM DUAL     ";
                    }
                    else if (nJobType == 2 || nJobType == 4 || nJobType == 5 || nJobType == 7 || nJobType == 8)
                    {
                        //strSql += cDefApp.CRLF + " SELECT RET_SEQ.NEXTVAL AS LUGG_NO FROM DUAL              ";
                        strSql += cDefApp.CRLF + " SELECT " + DbLang.NEXTVAL("RET_SEQ") + " AS LUGG_NO FROM DUAL     ";
                    }
                    else if (nJobType == 9)
                    {
                        //strSql += cDefApp.CRLF + " SELECT MANUAL_SEQ.NEXTVAL AS LUGG_NO FROM DUAL           ";
                        strSql += cDefApp.CRLF + " SELECT " + DbLang.NEXTVAL("MANUAL_SEQ") + " AS LUGG_NO FROM DUAL     ";
                    }
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    nSelCnt = _pBdb.ExcuteQry(dtLUGG_NO, strSql);
                    if (nSelCnt <= 0) { break; }
                    else
                    {
                        strLuggNo = dtLUGG_NO.Rows[0]["LUGG_NO"].ToString();

                        strSql = "";
                        strSql += cDefApp.CRLF + " SELECT *                  ";
                        strSql += cDefApp.CRLF + "   FROM JOB_MST            ";
                        strSql += cDefApp.CRLF + "  WHERE WH_TYP  = :WH_TYP  ";
                        strSql += cDefApp.CRLF + "    AND LUGG_NO = :LUGG_NO ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLuggNo;
                        nSelCnt = _pBdb.ExcuteQry(dtJOB_MST, strSql);

                        if (nSelCnt < 0)
                            break;
                        else if (nSelCnt == 0)
                        {
                            strSql = "";
                            strSql += cDefApp.CRLF + " INSERT INTO JOB_MST (WH_TYP          ";
                            strSql += cDefApp.CRLF + "                    , JOB_TYP         ";
                            strSql += cDefApp.CRLF + "                    , LUGG_NO         ";
                            strSql += cDefApp.CRLF + "                    , START_POS       ";
                            strSql += cDefApp.CRLF + "                    , START_LOCATION  ";
                            strSql += cDefApp.CRLF + "                    , DEST_POS        ";
                            strSql += cDefApp.CRLF + "                    , DEST_LOCATION   ";
                            strSql += cDefApp.CRLF + "                    , TURN            ";
                            strSql += cDefApp.CRLF + "                    , PRODUCT_ID      ";
                            strSql += cDefApp.CRLF + "                    , PRODUCT_SIZE    ";
                            strSql += cDefApp.CRLF + "                    , BCR_TOP         ";
                            strSql += cDefApp.CRLF + "                    , BCR_BOTTOM      ";
                            strSql += cDefApp.CRLF + "                    , AGING_TIME      ";
                            strSql += cDefApp.CRLF + "                    , JOB_STATUS      ";
                            strSql += cDefApp.CRLF + "                    , JOB_PRIORITY    ";
                            strSql += cDefApp.CRLF + "                    , JOB_START_DT)   ";
                            strSql += cDefApp.CRLF + "           VALUES ( :WH_TYP           ";
                            strSql += cDefApp.CRLF + "                  , :JOB_TYP          ";
                            strSql += cDefApp.CRLF + "                  , :LUGG_NO          ";
                            strSql += cDefApp.CRLF + "                  , :START_POS        ";
                            strSql += cDefApp.CRLF + "                  , :START_LOCATION   ";
                            strSql += cDefApp.CRLF + "                  , :DEST_POS         ";
                            strSql += cDefApp.CRLF + "                  , :DEST_LOCATION    ";
                            strSql += cDefApp.CRLF + "                  , :TURN             ";
                            strSql += cDefApp.CRLF + "                  , :PRODUCT_ID       ";
                            strSql += cDefApp.CRLF + "                  , :PRODUCT_SIZE     ";
                            strSql += cDefApp.CRLF + "                  , :BCR_TOP          ";
                            strSql += cDefApp.CRLF + "                  , :BCR_BOTTOM       ";
                            strSql += cDefApp.CRLF + "                  , :AGING_TIME       ";
                            strSql += cDefApp.CRLF + "                  , :JOB_STATUS       ";
                            strSql += cDefApp.CRLF + "                  , :JOB_PRIORITY     ";
                            strSql += cDefApp.CRLF + "                  , " + DbLang.SYSDATE + ")          ";
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            _pBdb.mComMain.Parameters.Add("JOB_TYP", DbLang.VARCHAR).Value = nJobType.ToString();
                            _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLuggNo;
                            _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strStartPos;
                            _pBdb.mComMain.Parameters.Add("START_LOCATION", DbLang.VARCHAR).Value = strStartLocation;
                            _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDestPos;
                            _pBdb.mComMain.Parameters.Add("DEST_LOCATION", DbLang.VARCHAR).Value = strDestLocation;
                            _pBdb.mComMain.Parameters.Add("TURN", DbLang.VARCHAR).Value = "";
                            _pBdb.mComMain.Parameters.Add("PRODUCT_ID", DbLang.VARCHAR).Value = "";
                            _pBdb.mComMain.Parameters.Add("PRODUCT_SIZE", DbLang.VARCHAR).Value = ""; 
                            _pBdb.mComMain.Parameters.Add("BCR_TOP", DbLang.VARCHAR).Value = strTopBcr; 
                            _pBdb.mComMain.Parameters.Add("BCR_BOTTOM", DbLang.VARCHAR).Value = strBottomBcr;
                            _pBdb.mComMain.Parameters.Add("AGING_TIME", DbLang.DATE).Value = dtAGING_END_DT; 
                            if (nJobType == 1)
                                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = "01";
                            else if (nJobType == 2 || nJobType == 4 || nJobType == 5 || nJobType == 7 || nJobType == 8)
                                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = "20"; 
                            else if (nJobType == 9)
                                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = "04";
                            _pBdb.mComMain.Parameters.Add("JOB_PRIORITY", DbLang.VARCHAR).Value = strJOB_PRIORITY;

                            nSelCnt = _pBdb.ExcuteNonQry(strSql, false, true);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                return -1;
                            }

                            if (nSelCnt == 0)
                            {
                                pRTN_MSG += "작업정보 등록실패.";
                                return -1;
                            }

                            bSuccess = true;
                        }
                        else
                            continue;
                    }
                }

                dtLUGG_NO.Dispose();
                dtJOB_MST.Dispose();
                if (bSuccess == true)
                    return 1;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return -1;
            }
        } 
        //*/
        #endregion
    }
}