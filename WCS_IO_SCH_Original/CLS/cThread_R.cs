//최초작성자	: 최원빈
//작성일		: 20190722
//화면개요	    : 재처리 작업처리
//수정이력	    : 
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
    public class cThread_R : IOSchDB
    {
        #region 변수 선언
        private int m_nId = 0;
        public PsMsgView callPsMsgView = null;
        public Thread m_Thread;
        public SYS_MAIN m_Main;
        private bool m_Open;
        public bool IsOpen { get { return m_Open; } set { m_Open = value; } }

        //Dictionary<string, IF_EQM_R_WCS_STAT_Data> IfDic = new Dictionary<string, IF_EQM_R_WCS_STAT_Data>();
        string strRTN_MSG = "";

        #endregion 변수 선언

        #region IOSCHThread
        public cThread_R(int Id)
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

        #region Thread_Doing(object value)
        public void Thread_Doing(object value)
        {
            while (!IsDBOpen)
            {
                try
                {
                    if (DBOpen()) { MakeMsg("[cThread_R] DB Open"); break; }
                    MakeMsg_Error("[cThread_R] DB Open 실패 - 5초 후 재시도");
                }
                catch (Exception ex) { MakeMsg_Error("[cThread_R] DB Open 오류: " + ex.Message); }
                Thread.Sleep(5000);
            }

            try
            {
                while (true)
                {
                    strRTN_MSG = "";
                    Thread.Sleep(100);

                    if (_pBdb == null) { try { IsDBOpen = false; DBOpen(); } catch { } continue; }

                    _pBdb.mDtMain.Clear();
                    _pBdb.mDtMain.Reset();
                    ////_pBdb.mDtMain.Dispose();

                    switch (m_nId)
                    {
                        case (int)cDefApp.eThGbn.R_GR01:
                            #region 오래된 로그 지우기

                            if (m_Main.chkLogDelTest.Checked)
                            {
                                #region ini에서 읽어오기 - TEST용
                                cDefApi.GsGetInitPorFileLogDel(ref cDefApp.GM_LOG_DEL_YEAR, ref cDefApp.GM_LOG_DEL_MONTH, ref cDefApp.GM_LOG_DEL_DAY,
                                                                ref cDefApp.GM_LOG_DEL_HOUR, ref cDefApp.GM_LOG_DEL_MINUTE, ref cDefApp.GM_LOG_DEL_MINUTE,
                                                                ref cDefApp.GM_LOG_DEL_ADDVALUE, ref cDefApp.GM_LOG_DEL_ADDTYPE, ref strRTN_MSG);

                                int nHour = Convert.ToInt16(cDefApp.GM_LOG_DEL_HOUR);
                                int nMinute = Convert.ToInt16(cDefApp.GM_LOG_DEL_MINUTE);
                                int nSecond = Convert.ToInt16(cDefApp.GM_LOG_DEL_MINUTE);
                                int nAddValue = Convert.ToInt16(cDefApp.GM_LOG_DEL_ADDVALUE);

                                DateTime dtCrrtDT = System.DateTime.Now;                                                                 // 현 재 시간 가져오기 
                                DateTime dtChkDT1 = new DateTime(dtCrrtDT.Year, dtCrrtDT.Month, dtCrrtDT.Day, nHour, nMinute, nSecond);  // Check 시간1 가져오기 
                                DateTime dtChkDT2 = new DateTime(dtCrrtDT.Year, dtCrrtDT.Month, dtCrrtDT.Day, nHour, nMinute, nSecond);

                                switch (cDefApp.GM_LOG_DEL_ADDTYPE)
                                {
                                    case "YEAR": dtChkDT2 = dtChkDT1.AddYears(nAddValue); break;                      // Check 시간2 가져오기        
                                    case "MONTH": dtChkDT2 = dtChkDT1.AddMonths(nAddValue); break;                      // Check 시간2 가져오기        
                                    case "DAY": dtChkDT2 = dtChkDT1.AddDays(nAddValue); break;                      // Check 시간2 가져오기        
                                    case "HOUR": dtChkDT2 = dtChkDT1.AddHours(nAddValue); break;                      // Check 시간2 가져오기        
                                    case "MINUTE": dtChkDT2 = dtChkDT1.AddMinutes(nAddValue); break;                      // Check 시간2 가져오기        
                                    case "SECOND": dtChkDT2 = dtChkDT1.AddSeconds(nAddValue); break;                      // Check 시간2 가져오기        
                                }

                                // 비교 순서(인자)를 확인요망!
                                int nResult1 = DateTime.Compare(dtChkDT1, dtCrrtDT);                                                    // 현재 시간과 Check할 시간의 비교 
                                int nResult2 = DateTime.Compare(dtCrrtDT, dtChkDT2);                                                    // 현재 시간과 Check할 시간의 30분 이후의 시간 비교하기

                                string strCheckDT1 = dtChkDT1.ToString("yyyy-MM-dd HH:mm:ss");
                                string strCheckDT2 = dtChkDT2.ToString("yyyy-MM-dd HH:mm:ss");
                                string strCurrentDT = dtCrrtDT.ToString("yyyy-MM-dd HH:mm:ss");

                                // dtChkDT < dtCrtDT && dtCrtDT < dtChkDT.AddMinutes(30) 인경우 
                                // 현재시간이 00:00:00 보다 크고 00:30:00 보다 작은 때인경우에만 실행됨!
                                if (nResult1 < 0 && nResult2 < 0)
                                {
                                    if (CHECK_DEL_HIS(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                        , ref strRTN_MSG) == false)
                                    {
                                        MakeMsg_Error_NoLog(strRTN_MSG);
                                        SetErrorMsg(strRTN_MSG);
                                    }
                                    Thread.Sleep(50);
                                }
                                #endregion
                            }

                            #endregion
                            #region 기존로직 - 주석처리
                            /*
                            //  BCR상태 재보고(3분)
                            if (CHECK_BCR_RETRY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);

                            // D/P 완료 재보고.
                            if (CHECK_DP_COMPLETE_RETRY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                        , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);

                            // M/G 완료 재보고.
                            if (CHECK_MG_COMPLETE_RETRY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                        , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);

                            // 완료작업 재보고
                            if (CHECK_SC_COMPLETE_RETRY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                        , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);

                            // 이중입고 및 공출고처리(공출고처리는 화재에 대해서만 자동으로 적용한다.)
                            // CELL_STA ERROR MARKING
                            if (CHECK_DUPLICATE_N_EMPTY_CELL(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                        , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);

                            //// 화재출고 완료작업보고
                            //if (CHECK_SC_FIRE_COMPLETE(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                            //                            , ref strRTN_MSG) == false)
                            //{
                            //    MakeMsg_Error_NoLog(strRTN_MSG);
                            //    SetErrorMsg(strRTN_MSG);
                            //}
                            //Thread.Sleep(50);


                            // D/P 도착 재보고.
                            if (CHECK_DP_ARRIVE_RETRY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                      , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);

                            // 오래된 미처리 작업삭제처리.
                            if (CHECK_OLD_JOB_DELETE(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                     , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);


                            // WAIT TRACK에 정지해있는 데이터를 조회하여, 미처리된 작업에 대한 스텝을 변경한다.
                            if (CHECK_WAIT_TRACK_JOBSTATUS_RETRY(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                                , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);

                            // 이중입고 및 공출고처리 (에러리셋, 작업삭제, 엑티브 지시)
                            if (PRO_DUPLICATE_N_EMPTY_CELL(((int)cDefApp.eWHTYP.SKI_WH01).ToString()
                                                        , ref strRTN_MSG) == false)
                            {
                                MakeMsg_Error_NoLog(strRTN_MSG);
                                SetErrorMsg(strRTN_MSG);
                            }
                            Thread.Sleep(50);
                            //*/
                            #endregion
                            break;
                        default:
                            break;
                    }

                    
                    _pBdb.mDtMain.Clear();
                    ////_pBdb.mDtMain.Dispose();

                    long mem = GC.GetTotalMemory(false);
                    Console.WriteLine("R-Current Memory : {0}", mem);
                    //GC.Collect(0, GCCollectionMode.Forced);

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                IsRtnMsg = ex.Message;
                MakeMsg_Error("[cThread_R] IOSCH Thread가 종료됩니다.(" + IsRtnMsg + ")");
                SetErrorMsg("[cThread_R] IOSCH Thread가 종료됩니다.(" + IsRtnMsg + ")");
                DBClose();

                m_Thread = null;
            }
        }
        #endregion

        public void SetErrorMsg(string strMsg)
        {
            Log.Error(strMsg);
        }

        // 기간이 지난 로그파일 지우기
        public bool CHECK_DEL_HIS(string strWH_TYP
                            , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strTABLE_NAME = "";
                string strCYCLE = "";
                string strUNIT = "";
                string strKEY_COLUMN = "";
                int nUpdateCnt = 0;
                int nJobCnt = 0;
                pRTN_MSG = "CHECK_DEL_HIS::";

                DataTable dtDEL_HIS = new DataTable();

                strSql = "";
                strSql += CRLF + " SELECT *                 ";
                strSql += CRLF + "   FROM DEL_HIS_SETTING   ";
                strSql += CRLF + "  WHERE WH_TYP = '" + strWH_TYP + "'";
                nSelCnt = _pBdb.ExcuteQry(dtDEL_HIS, strSql);
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
                bool bResult = false;
                string strLOG = "";
                for (int i = 0; i < nSelCnt; i++)
                {
                    strTABLE_NAME = dtDEL_HIS.Rows[i]["TABLE_NAME"].ToString() == "" ? "0" : dtDEL_HIS.Rows[i]["TABLE_NAME"].ToString();
                    strCYCLE = dtDEL_HIS.Rows[i]["CYCLE"].ToString() == "" ? "0" : dtDEL_HIS.Rows[i]["CYCLE"].ToString();
                    strUNIT = dtDEL_HIS.Rows[i]["UNIT"].ToString() == "" ? "0" : dtDEL_HIS.Rows[i]["UNIT"].ToString();
                    strKEY_COLUMN = dtDEL_HIS.Rows[i]["KEY_COLUMN"].ToString() == "" ? "0" : dtDEL_HIS.Rows[i]["KEY_COLUMN"].ToString();
                      
                    strSql = "";
                    strSql += CRLF + " DELETE FROM " + strTABLE_NAME;
                    strSql += CRLF + "  WHERE WH_TYP = '" + strWH_TYP + "'";
                    strSql += CRLF + "    AND " + strKEY_COLUMN + " <=  " + DbLang.SYSDATE + "::TIMESTAMP - '" + strCYCLE + " " + strUNIT + "'::INTERVAL ";
                    nJobCnt = _pBdb.ExcuteNonQry(strSql);
                    //if (nJobCnt < 1)
                    //    continue;
                    if (nJobCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }

                    if (nJobCnt == 0)
                    {
                        pRTN_MSG = "";
                        continue;
                    }

                    bResult = true;


                    strLOG = "[CHECK_DEL_HIS] 오래된(" + strCYCLE + " " + strUNIT + " 이상 경과된) 자료(HIS, LOG) " + nJobCnt .ToString() + "ROW를 삭제합니다. [Table:" + strTABLE_NAME + "]";
                    InsertLog(strWH_TYP, strLOG,"","","0","","","",false);      // transaction을 걸면 안됨
                    MakeMsg(strLOG);
                }


                if (bResult == true)
                {
                    _pBdb.Commit();

                    strLOG = "[CHECK_DEL_HIS] 오래된 자료(HIS, LOG) 삭제를 완료 하였습니다.";
                    InsertLog(strWH_TYP, strLOG);
                    MakeMsg(strLOG);
                }
                else
                {
                    _pBdb.Rollback();
                }

                return true;

            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
                return false;
            }
        }


        // BARCODE MST 재보고(3분)
        public bool CHECK_BCR_RETRY(string strWH_TYP
                              , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strTRACK_NO = "";
                int nUpdateCnt = 0;
                int nJobCnt = 0;
                pRTN_MSG = "CHECK_BCR_RETRY::";

                DataTable dtBCR_MST = new DataTable();

                strSql = "";
                strSql += CRLF + " SELECT BM.*, CD.*                                                                                    ";
                strSql += CRLF + "   FROM BCR_MST BM                                                                                    ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD                                                                               ";
                strSql += CRLF + "     ON BM.BCR_MC_NO          = CD.TRACK_NO                                                           ";
                strSql += CRLF + "    AND BM.WH_TYP             = CD.WH_TYP                                                             ";
                strSql += CRLF + "    AND CD.WH_TYP             =  :WH_TYP                                                              ";
                strSql += CRLF + "  WHERE CD.RET_READY_RD       = '1'                                                                   ";  // 출고대READY
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'                                                                   ";
                strSql += CRLF + "    AND CD.DEST_POS_RD       <> '00000'                                                               ";
                strSql += CRLF + "    AND BM.BCR_STA IN ('3')                                                                           ";
                strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + "-BM.UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                //strSql += CRLF + "    AND TO_CHAR(" + DbLang.SYSDATE + "-BM.UPD_DT, 'YYYYMMDDHH24MISS')::integer > 4  ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(dtBCR_MST, strSql);
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
                    strTRACK_NO = dtBCR_MST.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : dtBCR_MST.Rows[i]["TRACK_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " SELECT JM.LUGG_NO                            ";
                    strSql += CRLF + "   FROM JOB_MST JM                             ";
                    strSql += CRLF + "  WHERE JM.WH_TYP         =   :WH_TYP          ";
                    strSql += CRLF + "    AND JM.START_POS      =   :START_POS       ";
                    strSql += CRLF + "    AND JM.JOB_STATUS     =   '10'             ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strTRACK_NO;
                    nJobCnt = _pBdb.ExcuteQry(strSql);
                    if (nJobCnt != 0)
                        continue;

                    strSql = "";
                    strSql += CRLF + " UPDATE BCR_MST                           ";
                    strSql += CRLF + "    SET BCR_STA       = '1'               ";
                    strSql += CRLF + "      , NOREAD_CNT    = '0'               ";
                    strSql += CRLF + "      , UPD_DT        = " + DbLang.SYSDATE + "             ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP           ";
                    strSql += CRLF + "    AND BCR_MC_NO     = :BCR_MC_NO        ";
                    strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("BCR_MC_NO", DbLang.VARCHAR).Value = strTRACK_NO;
                    nUpdateCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nUpdateCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }
                    if (nUpdateCnt == 0)
                    {
                        pRTN_MSG += "변경할 BARCODE TRACK 정보가 존재하지 않습니다. TRACK NO : " + strTRACK_NO;
                        _pBdb.Rollback();
                        return false;
                    }
                }

                _pBdb.Commit();

                string strLOG = "[IO TASK] After 3min request  (CV : " + strTRACK_NO + ")";
                InsertLog(strWH_TYP, strLOG);
                return true;

            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
                return false;
            }
        }

        // 완료재보고
        public bool CHECK_SC_COMPLETE_RETRY(string strWH_TYP
                                      , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                int nJobCnt = 0;
                string strSql = "";

                string strLUGG_NO = "";

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
                    nJobCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nJobCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }
                    if (nJobCnt == 0)
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

        // 화재출고 완료보고
        public bool CHECK_SC_FIRE_COMPLETE(string strWH_TYP
                                     , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strSC_NO = "";
                string strLUGG_NO = "";
                string strSTART_LOCATION = "";

                pRTN_MSG = "CHECK_SC_FIRE_COMPLETE::";

                strSql = "";
                strSql += CRLF + " SELECT SD.*, JM.*                            ";
                strSql += CRLF + "   FROM SC_DATA SD                            ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                       ";
                strSql += CRLF + "     ON SD.WH_TYP         = JM.WH_TYP         ";
                strSql += CRLF + "    AND SD.SC_NO		    = JM.START_POS      ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK1   = JM.LUGG_NO        ";
                strSql += CRLF + "  INNER JOIN SC_HS_DEF SHD                    ";
                strSql += CRLF + "     ON SD.WH_TYP             = SHD.WH_TYP    ";
                strSql += CRLF + "    AND SD.SC_NO              = SHD.SC_NO     ";
                strSql += CRLF + "  WHERE SD.WH_TYP         = :WH_TYP           ";
                strSql += CRLF + "    AND SD.SC_FIRE_OD     = '57344'           ";
                strSql += CRLF + "    AND SHD.HS_MC_NO      = '57344'           ";
                strSql += CRLF + "    AND SD.COMPLETE_RD    <> '0'              ";
                strSql += CRLF + "    AND JM.JOB_TYP        = '8'               ";
                strSql += CRLF + "    AND JM.JOB_STATUS     = '22'              ";
                strSql += CRLF + "    AND SHD.HS_NO         IN ('04', '07')     ";
                strSql += CRLF + "    AND (SD.ITN_LUGG_FK1 IS NOT NULL OR SD.ITN_LUGG_FK1 <> '0')   ";
                strSql += CRLF + "    AND (SD.ITN_LUGG_FK2 IS NOT NULL OR SD.ITN_LUGG_FK2 <> '0')   ";
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

                strSC_NO = _pBdb.mDtMain.Rows[0]["SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["SC_NO"].ToString();
                strLUGG_NO = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                strSTART_LOCATION = _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_LOCATION"].ToString();

                // S/C 완료처리.
                if ((UPDATE_JOB_DATA("29", strLUGG_NO, strWH_TYP, "8", ref pRTN_MSG) == false))
                {
                    _pBdb.Rollback();
                    return false;
                }
                //qq12
                // 공CELL변경.
                if (UPDATE_CELL_MST_DTL_FIRE_RESET("0", "0", "0", strWH_TYP, strSC_NO, strSTART_LOCATION, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                // S/C 작업정보 초기화
                strSql = "";
                strSql += CRLF + "  UPDATE SC_DATA                          ";
                strSql += CRLF + "     SET ITN_LUGG_FK1        =  '0'       ";
                strSql += CRLF + "       , ITN_LUGG_FK2        =  '0'       ";   
                strSql += CRLF + "       , OD_USER_ID          =  'IOTASK'  ";
                strSql += CRLF + "       , OD_UPD_DT           =  " + DbLang.SYSDATE + "   ";
                strSql += CRLF + "   WHERE WH_TYP              =  :WH_TYP   ";
                strSql += CRLF + "     AND SC_NO               =  :SC_NO    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    _pBdb.Rollback();
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "변경할 SC DATA가 없습니다. SC_NO : " + strSC_NO;
                    _pBdb.Rollback();
                    return false;
                }

                _pBdb.Commit();

                string strLOG = "[IO TASK] 화재감지출고 완료보고 (LUGG NO : " + strLUGG_NO + ")";
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



        // CELL정보 UPDATE처리 및 재고정보 삭제 및 등록처리.
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

        // CELL정보 UPDATE처리 및 재고정보 삭제 및 등록처리.
        public bool UPDATE_CELL_MST_DTL_FIRE_RESET(string strCELL_STA,
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

                strRTN_MSG += "UPDATE_CELL_MST_DTL_FIRE_RESET::";

                strSql = "";
                strSql += CRLF + " UPDATE CELL_MST                     ";
                strSql += CRLF + "    SET CELL_STA    = :CELL_STA      ";
                strSql += CRLF + "      , BOTTOM_TRAY = :BOTTOM_TRAY   ";
                strSql += CRLF + "      , TOP_TRAY    = :TOP_TRAY      ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + "        ";
                strSql += CRLF + "      , FIRE_CMD_OD = '0' ";
                strSql += CRLF + "      , FIRE_OD_RQ_YN = 'N' ";
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
                                    string strPAIR_LUGG_NO = "0")
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                strRTN_MSG += "[SET_CV_DATA]::";

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




        // D/P 완료재보고
        public bool CHECK_DP_COMPLETE_RETRY(string strWH_TYP
                                      , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strTRACK_NO = "";
                int nUpdateCnt = 0;
                int nJobCnt = 0;

                pRTN_MSG = "CHECK_DP_COMPLETE_RETRY::";

                strSql = "";
                strSql += CRLF + " SELECT CD.*                                                              ";
                strSql += CRLF + "   FROM CV_DATA CD                                                        ";
                strSql += CRLF + "  WHERE CD.WH_TYP             = :WH_TYP                                   ";
                strSql += CRLF + "    AND CD.DP_HS_YN           = 'Y'                                       ";  // D/P H/S여부
                strSql += CRLF + "    AND CD.STO_READY_RD       = '1'                                       ";  // 입고대 READY ON
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'                                       ";  // 화물감지
                strSql += CRLF + "    AND CD.DEST_POS_RD       = '09998'                                    ";  // D/P 완료신호 ON
                strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - CD.READ_UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";// 4분경과시 재보고처리.
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
                DataTable dtJOB_MST = new DataTable();
                for (int i = 0; i < nSelCnt; i++)
                {
                    strTRACK_NO = _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " SELECT JM.LUGG_NO                            ";
                    strSql += CRLF + "   FROM JOB_MST JM                             ";
                    strSql += CRLF + "  WHERE JM.WH_TYP         =   :WH_TYP          ";
                    strSql += CRLF + "    AND JM.START_POS      =   :START_POS       ";
                    strSql += CRLF + "    AND JM.JOB_STATUS     =   '10'             ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strTRACK_NO;
                    nJobCnt = _pBdb.ExcuteQry(dtJOB_MST, strSql);
                    if (nJobCnt != 0)
                        continue;

                    strSql = "";
                    strSql += CRLF + " UPDATE CV_DATA                           ";
                    strSql += CRLF + "    SET COMP_DP       = 'N'               ";
                    strSql += CRLF + "      , READ_UPD_DT   = " + DbLang.SYSDATE + "           ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP           ";
                    strSql += CRLF + "    AND TRACK_NO      = :TRACK_NO         ";
                    strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - READ_UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strTRACK_NO;
                    nUpdateCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nUpdateCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }
                    if (nUpdateCnt == 0)
                    {
                        pRTN_MSG += "변경할 DP TRACK 정보가 존재하지 않습니다. TRACK NO : " + strTRACK_NO;
                        _pBdb.Rollback();
                        return false;
                    }
                }
                _pBdb.Commit();

                string strLOG = "[IO TASK] After 3min DP complete retry  (CV : " + strTRACK_NO + ")";
                InsertLog(strWH_TYP, strLOG);

                return true;
            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
                return false;
            }
        }


        // M/G 완료재보고
        public bool CHECK_MG_COMPLETE_RETRY(string strWH_TYP
                                      , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strTRACK_NO = "";
                int nUpdateCnt = 0;
                int nJobCnt = 0;
                pRTN_MSG = "CHECK_MG_COMPLETE_RETRY::";

                strSql = "";
                strSql += CRLF + " SELECT CD.*                                  ";
                strSql += CRLF + "   FROM CV_DATA CD                            ";
                strSql += CRLF + "  WHERE CD.WH_TYP             = :WH_TYP       ";  // 출고대READY
                strSql += CRLF + "    AND CD.MG_HS_YN           = 'Y'           ";
                strSql += CRLF + "    AND CD.STO_READY_RD       = '1'           ";  // 출고대READY
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'           ";
                strSql += CRLF + "    AND CD.DEST_POS_RD       = '09998'        ";
                strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - CD.READ_UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
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
                DataTable dtJOB_MST = new DataTable();
                for (int i = 0; i < nSelCnt; i++)
                {
                    strTRACK_NO = _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " SELECT JM.LUGG_NO                            ";
                    strSql += CRLF + "   FROM JOB_MST JM                             ";
                    strSql += CRLF + "  WHERE JM.WH_TYP         =   :WH_TYP          ";
                    strSql += CRLF + "    AND JM.START_POS      =   :START_POS       ";
                    strSql += CRLF + "    AND JM.JOB_STATUS     =   '10'             ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strTRACK_NO;
                    nJobCnt = _pBdb.ExcuteQry(dtJOB_MST, strSql);
                    if (nJobCnt != 0)
                        continue;

                    strSql = "";
                    strSql += CRLF + " UPDATE CV_DATA                           ";
                    strSql += CRLF + "    SET COMP_MG       = 'N'               ";
                    strSql += CRLF + "      , READ_UPD_DT   = " + DbLang.SYSDATE + "           ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP           ";
                    strSql += CRLF + "    AND TRACK_NO      = :TRACK_NO         ";
                    strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - READ_UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strTRACK_NO;
                    nUpdateCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nUpdateCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }
                    if (nUpdateCnt == 0)
                    {
                        pRTN_MSG += "변경할 DP TRACK 정보가 존재하지 않습니다. TRACK NO : " + strTRACK_NO;
                        _pBdb.Rollback();
                        return false;
                    }
                }

                _pBdb.Commit();

                string strLOG = "[IO TASK] After 3min MG complete retry  (CV : " + strTRACK_NO + ")";
                InsertLog(strWH_TYP, strLOG);

                return true;
            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
                return false;
            }
        }


        // D/P 도착 재보고
        public bool CHECK_DP_ARRIVE_RETRY(string strWH_TYP
                                    , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strTRACK_NO = "";
                string strLUGG_NO = "";
                int nUpdateCnt = 0;
                int nJobCnt = 0;
                pRTN_MSG = "CHECK_DP_ARRIVE_RETRY::";

                strSql = "";
                strSql += CRLF + " SELECT CD.*                                  ";
                strSql += CRLF + "   FROM CV_DATA CD                            ";
                strSql += CRLF + "  WHERE CD.WH_TYP             = :WH_TYP       "; 
                strSql += CRLF + "    AND CD.DP_HS_YN           = 'Y'           ";
                strSql += CRLF + "    AND CD.RET_READY_RD       = '1'           ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'           ";
                strSql += CRLF + "    AND CD.DEST_POS_RD        = CD.TRACK_NO   ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD         <> '0000'          ";
                strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - CD.READ_UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
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
                DataTable dtJOB_MST = new DataTable();
                for (int i = 0; i < nSelCnt; i++)
                {
                    strTRACK_NO = _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strLUGG_NO = _pBdb.mDtMain.Rows[i]["LUGG_NO_RD"].ToString() == "" ? "0000" : _pBdb.mDtMain.Rows[i]["LUGG_NO_RD"].ToString();

                    strSql = "";
                    strSql += CRLF + " SELECT JM.LUGG_NO                             ";
                    strSql += CRLF + "   FROM JOB_MST JM                             ";
                    strSql += CRLF + "  WHERE JM.WH_TYP         =   :WH_TYP          ";
                    strSql += CRLF + "    AND JM.DEST_POS       =   :TRACK_NO        ";
                    strSql += CRLF + "    AND JM.JOB_STATUS     =   '14'             ";
                    strSql += CRLF + "    AND JM.LUGG_NO        =   :LUGG_NO         ";
                    strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - JM.UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strTRACK_NO;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                    nJobCnt = _pBdb.ExcuteQry(dtJOB_MST, strSql);
                    if (nJobCnt == 0)
                        continue;


                    strSql = "";
                    strSql += CRLF + " UPDATE JOB_MST                           ";
                    strSql += CRLF + "    SET JOB_STATUS    = '13'              ";
                    strSql += CRLF + "      , UPD_DT        = " + DbLang.SYSDATE + "           ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP           ";
                    strSql += CRLF + "    AND DEST_POS      = :TRACK_NO         ";
                    strSql += CRLF + "    AND LUGG_NO       = :LUGG_NO          ";
                    strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strTRACK_NO;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                    nUpdateCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nUpdateCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }
                    if (nUpdateCnt == 0)
                    {
                        pRTN_MSG += "변경할 DP TRACK 정보가 존재하지 않습니다. TRACK NO : " + strTRACK_NO;
                        _pBdb.Rollback();
                        return false;
                    }
                }

                _pBdb.Commit();

                string strLOG = "[IO TASK] After 3min DP arrive retry  (CV : " + strTRACK_NO + ")";
                InsertLog(strWH_TYP, strLOG);

                return true;

            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
                return false;
            }
        }


        // 오래된작업 자동삭제처리.
        public bool CHECK_OLD_JOB_DELETE(string strWH_TYP
                                   , ref string pRTN_MSG)
        {
            try
            {
                string strLUGG_NO = "";
                int nUpdateCnt = 0;

                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_OLD_JOB_DELETE::";

                strSql = "";
                strSql += CRLF + " SELECT JM.*        ";
                strSql += CRLF + "   FROM JOB_MST JM  ";
                strSql += CRLF + "  WHERE JM.WH_TYP         = :WH_TYP                                      ";  
                strSql += CRLF + "    AND JM.JOB_STATUS     IN ('10', '11', '12', '13', '14', '15', '40')  ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM CV_DATA CD WHERE CD.LUGG_NO_RD = JM.LUGG_NO) ";  
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM RTV_DATA RD WHERE (RD.LUGG_NO_FK1_RD = JM.LUGG_NO OR RD.LUGG_NO_FK2_RD = JM.LUGG_NO)) ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM SC_DATA SD WHERE (SD.LUGG_NO_FK1_RD = JM.LUGG_NO OR SD.LUGG_NO_FK2_RD = JM.LUGG_NO))  ";
                //strSql += CRLF + "    AND ((" + DbLang.SYSDATE + " - JM.UPD_DT) * 24) > 12 ";  // 12시간 지난 작업에 대해서만 처리한다.
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

                return true; // C/V루프도는 구간이 최대3일까지 있을수있으니, JOB_MST를 지우면 안되므로 임시처리.

                for (int i = 0; i < nSelCnt; i++)
                {
                    _pBdb.BeginTrans();
                    strLUGG_NO = _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " DELETE JOB_MST JM                                                                                                        ";
                    strSql += CRLF + "  WHERE JM.WH_TYP             = :WH_TYP                                                                                   ";  // 출고대READY
                    strSql += CRLF + "    AND JM.LUGG_NO            = :LUGG_NO                                                                                  ";
                    strSql += CRLF + "    AND JM.JOB_STATUS         IN ('10', '11', '12', '13', '14', '15', '40')                                               ";
                    strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM CV_DATA CD WHERE CD.LUGG_NO_RD = JM.LUGG_NO)                                            ";  // 출고대READY
                    strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM RTV_DATA RD WHERE (RD.LUGG_NO_FK1_RD = JM.LUGG_NO OR RD.LUGG_NO_FK2_RD = JM.LUGG_NO))   ";
                    strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM SC_DATA SD WHERE (SD.LUGG_NO_FK1_RD = JM.LUGG_NO OR SD.LUGG_NO_FK2_RD = JM.LUGG_NO))    ";
                    strSql += CRLF + "    AND ((" + DbLang.SYSDATE + " - JM.UPD_DT) * 24) > 12                                                                  ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                    nUpdateCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nUpdateCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }
                    if (nUpdateCnt == 0)
                    {
                        pRTN_MSG += "변경할 old jobs 정보가 존재하지 않습니다. LUGG_NO : " + strLUGG_NO;
                        _pBdb.Rollback();
                        return false;
                    }

                    _pBdb.Commit();

                    string strLOG = "[IO TASK] delete old jobs (LUGG_NO : " + strLUGG_NO + ")";
                    InsertLog(strWH_TYP, strLOG);
                }



                return true;

            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
                return false;
            }
        }



        // 이중입고 및 공출고처리(공출고처리는 화재에 대해서만 자동으로 적용한다.)
        public bool CHECK_DUPLICATE_N_EMPTY_CELL(string strWH_TYP
                                           , ref string pRTN_MSG)
        {
            try
            {
                string strSql;
                int nSelCnt = 0;
                int nJOB_CNT = 0;

                pRTN_MSG = "CHECK_DUPLICATE_N_EMPTY_CELL::";

                // 1.미처리된 이중입고 또는 공출고 에러상태의 S/C 및 작업정보 가져오기.
                strSql = "";
                strSql += CRLF + " SELECT SD.*, JM.*, CM.*  ";
                strSql += CRLF + "   FROM SC_DATA SD        ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM   ";
                strSql += CRLF + "     ON SD.WH_TYP 		    = JM.WH_TYP     ";
                strSql += CRLF + "    AND (SD.SC_NO = JM.START_POS OR SD.SC_NO = JM.DEST_POS )";
                strSql += CRLF + "    AND SD.LUGG_NO_FK1_RD 	= JM.LUGG_NO    ";
                strSql += CRLF + "  INNER JOIN CELL_MST CM                      ";
                strSql += CRLF + "     ON SD.WH_TYP 		    = CM.WH_TYP     ";
                strSql += CRLF + "    AND SD.SC_NO		        = CM.CELL_SC_NO ";
                strSql += CRLF + "    AND (JM.START_LOCATION 	= CM.CELL_NO OR JM.DEST_LOCATION = CM.CELL_NO)  ";
                strSql += CRLF + "  WHERE JM.WH_TYP             = '" + strWH_TYP + "'       ";
                strSql += CRLF + "    AND CM.CELL_STA NOT IN('4', '5')                      ";
                strSql += CRLF + "    AND SD.ERR_CODE_RD IN ('58', '0058', '54', '0054')    ";
                strSql += CRLF + "    AND SD.SC_NO < '29700' "; // 충방이 아닌 데이터에 대해서만 처리한다.
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

                string strSC_NO = _pBdb.mDtMain.Rows[0]["SC_NO"].ToString();
                string strLUGG_NO = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                string strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                string strERROR_CODE = _pBdb.mDtMain.Rows[0]["ERR_CODE_RD"].ToString();
                string strCELL_NO = _pBdb.mDtMain.Rows[0]["CELL_NO"].ToString();
                string strCELL_STA = "";    // 이중입고 : 4, 공출고 : 5
                string strWRK_TYP = "";    
 
                // 54 : 이중입고, 58 : 공출고
                if (strERROR_CODE == "54" || strERROR_CODE == "0054")  
                {
                    if (strERROR_CODE == "0054") strERROR_CODE = "54";

                    // CELL에 이중입고 마킹
                    strCELL_STA = "4";
                }
                else if (strERROR_CODE == "58" || strERROR_CODE == "0058")
                {
                    if (strERROR_CODE == "0058") strERROR_CODE = "58";

                    // CELL에 공출고 마킹
                    strCELL_STA = "5";
                }
                else
                {
                    pRTN_MSG += "정의되지 않은 ERROR정보. ERROR CODE : " + strERROR_CODE;
                    _pBdb.Rollback();
                    return false;
                }
                

                // 2.이중입고 또는 공출고 에러발생 데이터 존재 시 CELL_MST에 ERROR MARKING 
                strSql = "";
                strSql += CRLF + " UPDATE CELL_MST  ";
                strSql += CRLF + "    SET CELL_STA    = '" + strCELL_STA + "' ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + "               ";
                strSql += CRLF + "  WHERE WH_TYP      = '" + strWH_TYP + "'   ";
                strSql += CRLF + "    AND CELL_SC_NO  = '" + strSC_NO + "'    ";
                strSql += CRLF + "    AND CELL_NO     = '" + strCELL_NO + "'  ";
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    _pBdb.Rollback();
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "변경할 CELL_MST 정보가 존재하지 않습니다. CELL_SC_NO : " + strSC_NO + " CELL NO : " + strCELL_NO;
                    _pBdb.Rollback();
                    return false;
                }

                // 3.화재출고 여부를 검색하여, 화재출고 및 공출고에러에 해당하면 자동 공출고처리 / 이중입고인 경우에는 무조건 사용자 조치.
                // 화재출고 발생 시에는 에러해제 후 작업삭제, ACTIVE 처리까지 자동으로 처리한다.
                if (strJOB_TYP == "8" && strERROR_CODE == "58")
                {
                    // 공통코드의 표현순서와 동일하게 작성.
                    for (int i = 0; i <= 3; i++)
                    {
                        // 해당데이터가 이미 존재하는지 여부를 확인 (미 존재 시 INSERT)
                        strSql = "";
                        strSql += CRLF + " SELECT ASW.*             ";
                        strSql += CRLF + "   FROM AUTO_SC_WORK ASW  ";
                        strSql += CRLF + "  WHERE ASW.WH_TYP        = '" + strWH_TYP + "' ";
                        strSql += CRLF + "    AND ASW.SC_NO         = '" + strSC_NO + "' ";
                        strSql += CRLF + "    AND ASW.LUGG_NO       = '" + strLUGG_NO + "' ";
                        strSql += CRLF + "    AND ASW.JOB_SEQ       = '" + i.ToString() + "' ";
                        nJOB_CNT = _pBdb.ExcuteQry(strSql);
                        if (nJOB_CNT > 0)
                            continue;

                        switch (i.ToString())
                        {
                            case "0":
                                strWRK_TYP = "RESET"; break;        // 에러리셋

                            case "1":
                                strWRK_TYP = "RESET"; break;        // 에러리셋

                            case "2":
                                strWRK_TYP = "DELFK12"; break;      // 작업삭제

                            case "3":
                                strWRK_TYP = "ACTIVE"; break;       // 엑티브
                        }

                        // JOB_SEQ를 FOR문의 순서대로 생성 (1 : 에러리셋, 2 : 작업삭제, 3 : ACTIVE)
                        strSql = "";
                        strSql += cDefApp.CRLF + " INSERT INTO AUTO_SC_WORK (             ";
                        strSql += cDefApp.CRLF + "                            WH_TYP      ";
                        strSql += cDefApp.CRLF + "                          , SC_NO       ";
                        strSql += cDefApp.CRLF + "                          , LUGG_NO     ";
                        strSql += cDefApp.CRLF + "                          , JOB_TYP     ";
                        strSql += cDefApp.CRLF + "                          , WRK_TYP     ";
                        strSql += cDefApp.CRLF + "                          , INS_DT      ";
                        strSql += cDefApp.CRLF + "                          , JOB_SEQ     ";
                        strSql += cDefApp.CRLF + "                          )             ";
                        strSql += cDefApp.CRLF + "                  VALUES (                    ";
                        strSql += cDefApp.CRLF + "                      '" + strWH_TYP + "'     ";
                        strSql += cDefApp.CRLF + "                    , '" + strSC_NO + "'      ";
                        strSql += cDefApp.CRLF + "                    , '" + strLUGG_NO + "'    ";
                        strSql += cDefApp.CRLF + "                    , '" + strJOB_TYP + "'    ";
                        strSql += cDefApp.CRLF + "                    , '" + strWRK_TYP + "'    ";
                        strSql += cDefApp.CRLF + "                    , " + DbLang.SYSDATE + "                 ";
                        strSql += cDefApp.CRLF + "                    , '" + i.ToString() + "'  ";
                        strSql += cDefApp.CRLF + "                          )                   ";
                        nSelCnt = _pBdb.ExcuteNonQry(strSql);
                        if (nSelCnt != 1)
                        {
                            strRTN_MSG += "AUTO_SC_WORK 등록 실패";
                            _pBdb.Rollback();
                            return false;
                        }
                    }
                }

                _pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
                return false;
            }
        }




        // 이중입고 및 공출고처리
        public bool PRO_DUPLICATE_N_EMPTY_CELL(string strWH_TYP
                                         , ref string pRTN_MSG)
        {
            try
            {
                string strSql;
                int nSelCnt = 0;

                pRTN_MSG = "PRO_DUPLICATE_N_EMPTY_CELL::";

                // 1.미처리된 자동 S/C처리예정 데이터 가져오기.
                strSql = "";
                strSql += CRLF + " SELECT ASW.*, JM.*                   ";
                strSql += CRLF + "   FROM AUTO_SC_WORK ASW              ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM               ";
                strSql += CRLF + "     ON ASW.WH_TYP  = JM.WH_TYP       ";
                strSql += CRLF + "    AND ASW.LUGG_NO = JM.LUGG_NO      ";
                strSql += CRLF + "  WHERE ASW.WH_TYP  = '" + strWH_TYP + "' ";
                strSql += CRLF + "  ORDER BY ASW.JOB_SEQ, ASW.INS_DT        ";
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
                string strSC_NO = _pBdb.mDtMain.Rows[0]["SC_NO"].ToString();
                string strLUGG_NO = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                string strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                string strWRK_TYP = _pBdb.mDtMain.Rows[0]["WRK_TYP"].ToString();
                string strJOB_SEQ = _pBdb.mDtMain.Rows[0]["JOB_SEQ"].ToString();
                string strMC_TYP = "SC";
                string strBCR_BOTTOM = _pBdb.mDtMain.Rows[0]["BCR_BOTTOM"].ToString();
                string strBCR_TOP = _pBdb.mDtMain.Rows[0]["BCR_TOP"].ToString();

                // 2.지시전 EVENT LOG 남기기.
                strSql = "";
                strSql += cDefApp.CRLF + " INSERT INTO EVENT_LOG (                  ";
                strSql += cDefApp.CRLF + "                        ISSUE_DATE        ";
                strSql += cDefApp.CRLF + "                      , EVENT_SEQ         ";
                strSql += cDefApp.CRLF + "                      , WH_TYP            ";
                strSql += cDefApp.CRLF + "                      , JOB_TYP           ";
                strSql += cDefApp.CRLF + "                      , MC_TYP            ";
                strSql += cDefApp.CRLF + "                      , MC_NO             ";
                strSql += cDefApp.CRLF + "                      , LUGG_NO           ";
                strSql += cDefApp.CRLF + "                      , BCR_BOTTOM        ";
                strSql += cDefApp.CRLF + "                      , BCR_TOP           ";
                strSql += cDefApp.CRLF + "                      , INS_DT            ";
                strSql += cDefApp.CRLF + "                      , INS_USER_ID       ";
                strSql += cDefApp.CRLF + "                      , REMARKS)          ";
                strSql += cDefApp.CRLF + "              VALUES (  " + DbLang.SYSDATE + "           ";         // ISSUE_DATE
                strSql += cDefApp.CRLF + "                      , '" + strJOB_SEQ + "'      "; // EVENT_SEQ
                strSql += cDefApp.CRLF + "                      , '" + strWH_TYP + "'       "; // WH_TYP
                strSql += cDefApp.CRLF + "                      , '" + strJOB_TYP + "'      "; // JOB_TYP
                strSql += cDefApp.CRLF + "                      , '" + strMC_TYP + "'       "; // MC_TYP
                strSql += cDefApp.CRLF + "                      , '" + strSC_NO + "'        "; // MC_NO
                strSql += cDefApp.CRLF + "                      , '" + strLUGG_NO + "'      "; // LUGG_NO
                strSql += cDefApp.CRLF + "                      , '" + strBCR_BOTTOM +  "'  "; // BCR_BOTTOM
                strSql += cDefApp.CRLF + "                      , '" + strBCR_TOP +     "'  "; // BCR_TOP
                strSql += cDefApp.CRLF + "                      , " + DbLang.SYSDATE + "                   "; // INS_DT
                strSql += cDefApp.CRLF + "                      , 'IO_SCH'                  "; // INS_USER_ID
                strSql += cDefApp.CRLF + "                      , '" + strWRK_TYP +     "'  "; // REMARKS
                strSql += cDefApp.CRLF + "                      )                           ";
                nSelCnt = _pBdb.ExcuteNonQry(strSql, false, true);
                if (nSelCnt != 1)
                {
                    strRTN_MSG += "EVENT_LOG 등록 실패";
                    _pBdb.Rollback();
                    return false;
                }

                // 3.에러리셋, 작업삭제, 엑티브 지시.
                if (strJOB_SEQ == "1" && strWRK_TYP == "RESET") Thread.Sleep(1500);


                strSql = "";
                strSql += CRLF + " UPDATE SC_DATA  ";
                strSql += CRLF + "    SET CMD_RQ_ID   = '" + strWRK_TYP + "'  ";
                strSql += CRLF + "      , CMD_RQ_YN   = 'Y'                   ";
                strSql += CRLF + "  WHERE WH_TYP      = '" + strWH_TYP + "'   ";
                strSql += CRLF + "    AND SC_NO       = '" + strSC_NO + "'    ";
                strSql += CRLF + "    AND CMD_RQ_YN   = 'N'                   ";
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    _pBdb.Rollback();
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "변경할 SC_DATA 정보가 존재하지 않습니다. SC_DATA : " + strSC_NO;
                    _pBdb.Rollback();
                    return false;
                }

                // 4.지시완료된 S/C정보삭제.
                strSql = "";
                strSql += CRLF + " DELETE FROM AUTO_SC_WORK              ";
                strSql += CRLF + "  WHERE WH_TYP  = '" + strWH_TYP + "'  ";
                strSql += CRLF + "    AND SC_NO   = '" + strSC_NO + "'   ";
                strSql += CRLF + "    AND LUGG_NO = '" + strLUGG_NO + "' ";
                strSql += CRLF + "    AND JOB_SEQ = '" + strJOB_SEQ + "' ";
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    _pBdb.Rollback();
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "변경할 SC_DATA 정보가 존재하지 않습니다. SC_DATA : " + strSC_NO;
                    _pBdb.Rollback();
                    return false;
                }

                _pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
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
                            , string strTRACK_TO = ""
                            //, string strEQP_TYP = ""
                              , bool bTrans = true)
        {
            string strTitle = "[InsertLog]";
            string strSql;

            string strPGR_NO = "IO_TASK";
            int nRtn = 0;

            try
            {
                if (bTrans == true)
                    _pBdb.BeginTrans();

                strSql = "";
                strSql = cDefApp.CRLF + " INSERT INTO WCS_LOG_PGR (WH_TYP               ";    // 1  
                strSql += cDefApp.CRLF + "                       , INS_DT               ";    // 2
                strSql += cDefApp.CRLF + "                       , LOG_SEQ              ";    // 3
                strSql += cDefApp.CRLF + "                       , LUGG_NO              ";    // 4
                strSql += cDefApp.CRLF + "                       , BCR_BOTTOM           ";    // 5
                strSql += cDefApp.CRLF + "                       , BCR_TOP              ";    // 6
                strSql += cDefApp.CRLF + "                       , PGR_NM               ";    // 7
                strSql += cDefApp.CRLF + "                       , LOG_KOR              ";    // 8
                strSql += cDefApp.CRLF + "                       , TRACK_FROM           ";    // 9
                strSql += cDefApp.CRLF + "                       , TRACK_TO             ";    // 10
                strSql += cDefApp.CRLF + "                       , JOB_STA              ";    // 11
                strSql += cDefApp.CRLF + "                       , RQ_INS_ID            ";    // 12
                strSql += cDefApp.CRLF + "                       , RQ_INS_DT            ";    // 13
                strSql += cDefApp.CRLF + "                       , EQP_TYP)             ";    // 14
                strSql += cDefApp.CRLF + "                VALUES ( :wh_typ              ";    // 1     // WH_TYP  
                strSql += cDefApp.CRLF + "                       , " + DbLang.SYSDATE + "                ";    // 2     // CELL_SC_NO 
                strSql += cDefApp.CRLF + "                       , " + DbLang.NEXTVAL("LOG_SEQ");          //LOG_SEQ.NEXTVAL   ";    // 3     // CELL_SC_NO 
                strSql += cDefApp.CRLF + "                       , :lugg_no             ";    // 4     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :bcr_bottom           ";    // 5     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :bcr_top             ";    // 6     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :pgr_nm              ";    // 7     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :log_kor             ";    // 8     // CELL_SEQ      
                strSql += cDefApp.CRLF + "                       , :track_from          ";    // 9     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :track_to            ";    // 10     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :job_sta             ";    // 11     // CELL_SEQ             
                strSql += cDefApp.CRLF + "                       , :rq_ins_id           ";    // 12     // AGING_START_DT  
                strSql += cDefApp.CRLF + "                       , " + DbLang.SYSDATE + "                ";    // 13    // REMARKS    
                strSql += cDefApp.CRLF + "                       , 'IO')                ";    // 14    // IO_TASK    

                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Add("wh_typ", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("lugg_no", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("bcr_bottom", DbLang.VARCHAR).Value = strBCR_BOTTOM;
                _pBdb.mComMain.Parameters.Add("bcr_top", DbLang.VARCHAR).Value = strBCR_TOP;
                _pBdb.mComMain.Parameters.Add("pgr_nm", DbLang.VARCHAR).Value = strPGR_NO;
                _pBdb.mComMain.Parameters.Add("log_kor", DbLang.VARCHAR).Value = strLOG_KOR;
                _pBdb.mComMain.Parameters.Add("track_from", DbLang.VARCHAR).Value = strTRACK_FROM;
                _pBdb.mComMain.Parameters.Add("track_to", DbLang.VARCHAR).Value = strTRACK_TO;
                _pBdb.mComMain.Parameters.Add("job_sta", DbLang.VARCHAR).Value = strJOB_STA;
                _pBdb.mComMain.Parameters.Add("rq_ins_id", DbLang.VARCHAR).Value = strPGR_NO;
                //_pBdb.mComMain.Parameters.Add("eqp_typ", DbLang.VARCHAR).Value = strEQP_TYP;
                nRtn = _pBdb.ExcuteNonQry(strSql);
                if (nRtn < 0)
                {
                    throw new Exception(strTitle + "LOG_PGR INSERT중 DATABASE 에러.. MESSAGE [" + _pBdb.ErrMsg + "]");
                }

                if (nRtn == 0)
                {
                    if (bTrans == true)
                        _pBdb.Rollback();
                    return;
                }

                if (bTrans == true)
                    _pBdb.Commit();
                return;
            }
            catch (Exception ex)
            {
                if (bTrans == true)
                    _pBdb.Rollback();
                MakeMsg_Error_NoLog(ex.Message);
                SetErrorMsg(ex.Message);
                return;
            }
        }


        // WAIT TRACK에 정지해있는 데이터를 조회하여, 미처리된 작업에 대한 스텝을 변경한다.
        public bool CHECK_WAIT_TRACK_JOBSTATUS_RETRY(string strWH_TYP
                                               , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                int nJobCnt = 0;
                string strSql = "";

                string strLUGG_NO = "";

                pRTN_MSG = "CHECK_WAIT_TRACK_JOBSTATUS_RETRY::";

                strSql = "";
                strSql += CRLF + " SELECT WT.WH_TYP, WT.WAIT_TRACK, JM.LUGG_NO, JM.JOB_STATUS, CD.LUGG_NO_RD    ";
                strSql += CRLF + "   FROM WAIT_TRACK WT         ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD       ";
                strSql += CRLF + "     ON WT.WH_TYP     = CD.WH_TYP     ";
                strSql += CRLF + "    AND WT.WAIT_TRACK = CD.TRACK_NO   ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM               ";
                strSql += CRLF + "     ON CD.WH_TYP     = JM.WH_TYP     ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD = JM.LUGG_NO    ";
                strSql += CRLF + "  WHERE WT.WH_TYP     = '" + strWH_TYP + "' ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'       ";
                strSql += CRLF + "    AND JM.JOB_STATUS         = '28'      ";
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
                    strLUGG_NO = _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " UPDATE JOB_MST                  ";
                    strSql += CRLF + "    SET JOB_STATUS    = '11'     ";
                    strSql += CRLF + "       ,UPD_DT        = " + DbLang.SYSDATE + "  ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP  ";
                    strSql += CRLF + "    AND LUGG_NO       = :LUGG_NO ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                    nJobCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nJobCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }
                    if (nJobCnt == 0)
                    {
                        pRTN_MSG += "변경할 작업정보가 존재하지 않습니다. LUGG NO : " + strLUGG_NO;
                        return false;
                    }
                }

                _pBdb.Commit();

                string strLOG = "[IO TASK] CHECK_WAIT_TRACK_JOBSTATUS_RETRY (LUGG NO : " + strLUGG_NO + ")";
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

    }
}