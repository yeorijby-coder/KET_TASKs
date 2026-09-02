//최초작성자	: 최원빈
//작성일		: 20190722
//화면개요	    : C/V 작업처리
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
    public class IF_EQM_R_WCS_STAT_Data
    {
        private string IFStatHexVal;
        public string IFSTATHEXVAL
        {
            get { return IFStatHexVal; }
            set { IFStatHexVal = value; }
        }
        public IF_EQM_R_WCS_STAT_Data()
        {
            IFStatHexVal = "";
        }
    }

    public class cThread_CV : IOSchDB
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

        private string _strErrorMsg = "";
        public string m_strRtnMsg = "";
        public int m_nRetCd = 0;
        static int m_nCurSCNo = 0;
        string strRTN_MSG = "";
        #endregion 변수 선언

        #region DATATABLE
        DataTable dtBUFFER = new DataTable();
        #endregion

        #region IOSCHThread
        public cThread_CV(int Id)
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
                    if (DBOpen()) { MakeMsg("[cThread_CV] DB Open"); break; }
                    MakeMsg_Error("[cThread_CV] DB Open 실패 - 5초 후 재시도");
                }
                catch (Exception ex) { MakeMsg_Error("[cThread_CV] DB Open 오류: " + ex.Message); }
                Thread.Sleep(5000);
            }

            try
            {
                string strThreadNo = "";

                while (true)
                {
                    Thread.Sleep(100);

                    //  연결이 끊긴 것을 못 보면 "전송 연결에 데이터를 쓸 수 없습니다" 만 계속 나온다.
                    if (!IsDbAlive()) { if (!DBReopen()) Thread.Sleep(3000); continue; }

                    strRTN_MSG = "";

                    long mem = GC.GetTotalMemory(true);
                    Console.WriteLine("CV-Current Memory   ---- 1 : {0} ---- {1}", mem, m_nId);

                    switch (m_nId)
                    {
                        case (int)cDefApp.eThGbn.CV_GR01:
                            strThreadNo = "01";
                            break;
                        
                        default:
                            Thread.Sleep(20);
                            break;
                    }

                    #region 실제 스레드 구동 로직
                    if (strThreadNo != "")
                    {
                        // C/V 도착보고
                        if (!ARRIVE_CV(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }

                            //MakeMsg(strRTN_MSG + "출고대에서 완료보고 하였습니다.");
                        }
                        Thread.Sleep(10);

                        // C/V 입고 H/S 도착보고(ex) 01311.
                        if (!CHECK_CV_STOHS(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);
                        
                        // WC출고대 READY (이동, 입고)
                        if (!CHECK_CV_RET_WC_READY(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);

                        // 출고대 READY (이동, 입고)
                        if (!CHECK_CV_RET_READY(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);

                        // 출고 H/S에서 최종목적지 결정 C/V목적지쓰기
                        if (!CHECK_CV_RETHS(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);

                        // 작업지시된 신규작업을 CV 구동대기로 변경
                        if (!NEW_JOB_INVOKE_FOR_CV(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);

                        // 입고대에서 출발 하기 
                        if (!NEW_JOB_ORDER(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);

                        // MG/DP에서 입고 모드에 따라 공파레트 작업 요청 가능한지 체크  
                        if (!CHECK_REQUESTABLE_EMPTY_PLT(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, "149", ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);


                        // MG에서 입고 모드에 따라 공파레트 입고 요청 가능한지 체크  
                        if (!CHECK_REQUESTABLE_EMPTY_PLT_STORE(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, "108", ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);

                        // 148에서 완료처리후에  
                        if (!CHECK_CV_RET_READY_148(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        {
                            MakeMsg_Error_NoLog(strRTN_MSG);
                            SetErrorMsg(strRTN_MSG);
                        }
                        else
                        {
                            if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        }
                        Thread.Sleep(10);

                        //// 시뮬레이터 모드 일때 특정 상황에 맞게 PLC DATA 수정
                        //if (!CHECK_SIM_MODE(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        //{
                        //    MakeMsg_Error_NoLog(strRTN_MSG);
                        //    SetErrorMsg(strRTN_MSG);
                        //}
                        //else
                        //{
                        //    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        //}
                        //Thread.Sleep(10);

                        //// 대기대(SizeChecker, DeadLock대기대)에서 출발하기
                        //if (!CHECK_CV_WAIT_TRACK(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strThreadNo, ref strRTN_MSG))
                        //{
                        //    MakeMsg_Error_NoLog(strRTN_MSG);
                        //    SetErrorMsg(strRTN_MSG);
                        //}
                        //else
                        //{
                        //    if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
                        //}
                        //Thread.Sleep(10);

                    }
                    #endregion


                    _pBdb.mDtMain.Clear();
                    //_pBdb.mDtMain.Dispose();

                    mem = GC.GetTotalMemory(true);
                    Console.WriteLine("CV-Current Memory   ---- 2 : {0} ---- {1}", mem, m_nId);
                    //GC.Collect();
                    //GC.WaitForFullGCComplete();

                    Thread.Sleep(20);
                }
            }
            catch (Exception ex)
            {
                IsRtnMsg = ex.Message;
                MakeMsg_Error("[cThread_CV] IOSCH Thread가 종료됩니다.(" + IsRtnMsg + ")");
                SetErrorMsg("[cThread_CV] IOSCH Thread가 종료됩니다.(" + IsRtnMsg + ")");
                //DataBase를 Close한다.
                DBClose();

                //스레드 다시 시작하기위함.
                m_Thread = null;
            }
        }
        #endregion

        public void SetErrorMsg(string strMsg)
        {
            _strErrorMsg = strMsg;
            Log.Error(_strErrorMsg);
        }

        // MG 요청 신호 Check
        public bool CHECK_REQUESTABLE_EMPTY_PLT_STORE(  string strWH_TYP,
                                                        string strPLC_NO,
                                                        string strMC_NO,        // "149 , 108"이런식으로도 사용가능할거지만 그냥 149만 입력하도록 할것
                                                    ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_REQUESTABLE_EMPTY_PLT_STORE::";

                #region 공파레트 작업 요청할 상황인지 Check
                strSql = "";
                strSql += CRLF + " SELECT CD.*                                                                             ";
                strSql += CRLF + "   FROM CV_DATA CD                                                                       ";
                strSql += CRLF + "  WHERE CD.WH_TYP          = :WH_TYP                                                     ";
                strSql += CRLF + "    AND CD.PLC_NO          = :PLC_NO                                                     ";
                strSql += CRLF + "    AND CD.MC_NO           IN ('" + strMC_NO + "')                                       ";                 // 108도 추가될 수 있음!
                strSql += CRLF + "    AND CD.STO_READY_RD    = '1'                                                         "; //SENSOR0_DATA_RD
                strSql += CRLF + "    AND CD.LUGG_NO_RD    = '0000'                                                        ";
                strSql += CRLF + "    AND TO_CHAR(" + DbLang.SYSDATE + " - CD.READ_UPD_DT, 'YYYYMMDDHH24MISS')::INTEGER > 5                 ";


                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

                string strSTOCK_MODE = "1";                             // 1:공파레트 입고 요청, 2:공파레트 출고 요청

                DataTable dtHostEmptyPlt = new DataTable();

                _pBdb.BeginTrans();
                for (int i = 0; i < nSelCnt; i++)
                {
                    #region 작업번호가 0인 상위에 요청을 안한것이 있는지 확인

                    strSql = "";
                    strSql += CRLF + " SELECT *                                                                         ";
                    strSql += CRLF + "   FROM HOST_EMPTY_PLT                                                            ";
                    strSql += CRLF + "  WHERE WH_TYP  = :WH_TYP                                                         ";
                    strSql += CRLF + "    AND KIND    = '" + strSTOCK_MODE + "'                                         ";
                    strSql += CRLF + "    AND STN     = '" + strMC_NO + "'                                              ";
                    strSql += CRLF + "    AND STATUS  IN ('N', 'Q')                                                     ";
                    //strSql += CRLF + "    AND LUGG_NO = '0'                                                             "; //작업번호가 0인거 있으면 요청하면 안됨.(이미 요청한게 처리가 안됐음)
                    strSql += CRLF + " ORDER BY INS_DT DESC                                                             ";
                    strSql += CRLF + " LIMIT 1                                                                          ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    nSelCnt = _pBdb.ExcuteQry(dtHostEmptyPlt, strSql);

                    //조회 에러 return
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }

                    //조회건수가 0이면, N,Q인게 없을 때 CV 신호가 올라온거니까 공팔렛트 요청해야됨.
                    if (nSelCnt == 0)
                    {
                        //헷갈려서 그냥 넣어둠. 그냥 진행!
                    }

                    //N,Q인게 있으면 작업번호 유무를 확인해야 함.
                    if (nSelCnt > 0)
                    {
                        string strHOST_LUGG_NO = dtHostEmptyPlt.Rows[0]["LUGG_NO"].ToString();

                        //N,Q인 작업이 작업번호가 0이면 요청하면 안됨.
                        if (strHOST_LUGG_NO == "0")
                        {
                            pRTN_MSG = "";
                            _pBdb.Rollback();
                            return true;
                        }

                        //작업번호가 0이 아니면 해당 작업이 언제 생성 됐는지를 체크.

                        #region 조회된 작업이 생성된지 1분이 지났는지 확인
                        strSql = "";
                        strSql += CRLF + " SELECT *                                                                         ";
                        strSql += CRLF + "   FROM HOST_EMPTY_PLT                                                            ";
                        strSql += CRLF + "  WHERE WH_TYP  = :WH_TYP                                                         ";
                        strSql += CRLF + "    AND KIND    = '" + strSTOCK_MODE + "'                                         ";
                        strSql += CRLF + "    AND STN     = '" + strMC_NO + "'                                              ";
                        strSql += CRLF + "    AND STATUS  IN ('N', 'Q')                                                     ";
                        strSql += CRLF + "    AND LUGG_NO = '" + strHOST_LUGG_NO + "'                                       ";
                        strSql += CRLF + "    AND TO_CHAR(NOW() - INS_DT, 'YYYYMMDDHH24MISS')::INTEGER > 60                 ";

                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        nSelCnt = _pBdb.ExcuteQry(dtHostEmptyPlt, strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            _pBdb.Rollback();
                            return false;
                        }
                        if (nSelCnt == 0)
                        {
                            pRTN_MSG = "";
                            _pBdb.Rollback();
                            return true;
                        }
                        #endregion
                    }
                    #endregion

                    #region 요청정보를 Insert 한다.
                    strSql = "";
                    strSql += cDefApp.CRLF + " INSERT INTO                       ";
                    strSql += cDefApp.CRLF + "        HOST_EMPTY_PLT (           "; 
                    strSql += cDefApp.CRLF + "                  WH_TYP           ";
                    strSql += cDefApp.CRLF + "                , KIND             ";
                    strSql += cDefApp.CRLF + "                , STN              ";
                    strSql += cDefApp.CRLF + "                , STATUS           ";
                    strSql += cDefApp.CRLF + "                , LUGG_NO          ";
                    strSql += cDefApp.CRLF + "                , INS_ID           ";
                    strSql += cDefApp.CRLF + "                , INS_DT           ";
                    strSql += cDefApp.CRLF + "                )                  ";
                    strSql += cDefApp.CRLF + "        VALUES (                   ";
                    strSql += cDefApp.CRLF + "            '" + strWH_TYP + "'    ";
                    strSql += cDefApp.CRLF + "          , '" + strSTOCK_MODE + "'";
                    strSql += cDefApp.CRLF + "          , '" + strMC_NO + "'     ";
                    strSql += cDefApp.CRLF + "          , 'N'                    ";
                    strSql += cDefApp.CRLF + "          , '0'                    ";
                    strSql += cDefApp.CRLF + "          , 'IO_TASK'              ";
                    strSql += cDefApp.CRLF + "          , " + DbLang.SYSDATE + " ";
                    strSql += cDefApp.CRLF + "                )                  ";
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt != 1)
                    {
                        strRTN_MSG += "AUTO_SC_WORK 등록 실패";
                        _pBdb.Rollback();
                        return false;
                    }
                    #endregion

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


        // MG/DP 요청 신호 Check
        public bool CHECK_REQUESTABLE_EMPTY_PLT(string strWH_TYP,
                                                string strPLC_NO,
                                                string strMC_NO,        // "149 , 108"이런식으로도 사용가능할거지만 그냥 149만 입력하도록 할것
                                            ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_REQUESTABLE_EMPTY_PLT::";

                #region 공파레트 작업(입고 or 출고) 요청할 상황인지 Check
                strSql = "";
                strSql += CRLF + " SELECT CD.*                                           ";
                strSql += CRLF + "   FROM CV_DATA CD                                     ";
                strSql += CRLF + "  WHERE CD.WH_TYP             = :WH_TYP                ";
                strSql += CRLF + "    AND CD.PLC_NO             = :PLC_NO                ";
                strSql += CRLF + "    AND CD.MC_NO              IN ('" + strMC_NO + "')  ";                 // 108도 추가될 수 있음!
                strSql += CRLF + "    AND TO_CHAR(" + DbLang.SYSDATE + " - READ_UPD_DT,  'YYYYMMDDHH24MISS')::INTEGER > 5";
                //strSql += CRLF + "    AND TO_CHAR(" + DbLang.SYSDATE + " - WRITE_UPD_DT, 'YYYYMMDDHH24MISS')::INTEGER > 5";
                //strSql += CRLF + "    AND TO_CHAR(" + DbLang.SYSDATE + " - OD_UPD_DT,    'YYYYMMDDHH24MISS')::INTEGER > 5";
                strSql += CRLF + "    AND (((CD.STO_READY_RD    = '1' )  AND (CD.STOCK_MODE = '0')) OR ((CD.SENSOR0_DATA_RD  = '0')  AND (CD.STOCK_MODE = '1'))) ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD      = '0000'                                                    ";                                                                

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

                string strSTOCK_MODE = "";

                DataTable dtHostEmptyPlt = new DataTable();

                _pBdb.BeginTrans();
                for (int i = 0; i < nSelCnt; i++)
                {
                    strSTOCK_MODE = _pBdb.mDtMain.Rows[i]["STOCK_MODE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["STOCK_MODE"].ToString();

                    if (strSTOCK_MODE == "1")           // 입고 모드일때
                    {
                        strSTOCK_MODE = "2"; // 공파레트 출고요청

                        #region 이미 해당 작업대에 요청중인 작업이 있는지 Check

                        strSql = "";
                        strSql += CRLF + " SELECT *                                ";
                        strSql += CRLF + "   FROM HOST_EMPTY_PLT                   ";
                        strSql += CRLF + "  WHERE WH_TYP  = :WH_TYP                ";
                        strSql += CRLF + "    AND KIND    = '" + strSTOCK_MODE + "'";
                        strSql += CRLF + "    AND STN     = '" + strMC_NO + "'     ";
                        strSql += CRLF + "    AND STATUS  IN ('N', 'Q')            ";
                        //strSql += CRLF + "    AND LUGG_NO = '0'                    ";

                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        nSelCnt = _pBdb.ExcuteQry(dtHostEmptyPlt, strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            _pBdb.Rollback();
                            return false;
                        }
                        if (nSelCnt > 0)
                        {
                            pRTN_MSG = "";
                            _pBdb.Rollback();
                            return true;
                        }
                        #endregion
                    }
                    else //if (strSTOCK_MODE == "0")    // 출고 모드일때
                    {
                        strSTOCK_MODE = "1"; // 공파레트 입고요청

                        #region 작업번호가 0인 상위에 요청을 안한것이 있는지 확인

                        strSql = "";
                        strSql += CRLF + " SELECT *                                                                         ";
                        strSql += CRLF + "   FROM HOST_EMPTY_PLT                                                            ";
                        strSql += CRLF + "  WHERE WH_TYP  = :WH_TYP                                                         ";
                        strSql += CRLF + "    AND KIND    = '" + strSTOCK_MODE + "'                                         ";
                        strSql += CRLF + "    AND STN     = '" + strMC_NO + "'                                              ";
                        strSql += CRLF + "    AND STATUS  IN ('N', 'Q')                                                     ";
                        //strSql += CRLF + "    AND LUGG_NO = '0'                                                           "; //작업번호가 0인거 있으면 요청하면 안됨.(이미 요청한게 처리가 안됐음)
                        strSql += CRLF + " ORDER BY INS_DT DESC                                                             ";
                        strSql += CRLF + " LIMIT 1                                                                          ";

                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        nSelCnt = _pBdb.ExcuteQry(dtHostEmptyPlt, strSql);

                        //조회 에러 return
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            _pBdb.Rollback();
                            return false;
                        }

                        //조회건수가 0이면, N,Q인게 없을 때 CV 신호가 올라온거니까 공팔렛트 요청해야됨.
                        if (nSelCnt == 0)
                        {
                            //헷갈려서 그냥 넣어둠. 그냥 진행!
                        }

                        //N,Q인게 있으면 작업번호 유무를 확인해야 함.
                        if (nSelCnt > 0)
                        {
                            string strHOST_LUGG_NO = dtHostEmptyPlt.Rows[0]["LUGG_NO"].ToString();

                            //N,Q인 작업이 작업번호가 0이면 요청하면 안됨.
                            if (strHOST_LUGG_NO == "0")
                            {
                                pRTN_MSG = "";
                                _pBdb.Rollback();
                                return true;
                            }

                            //작업번호가 0이 아니면 해당 작업이 언제 생성 됐는지를 체크.

                            #region 조회된 작업이 생성된지 1분이 지났는지 확인
                            strSql = "";
                            strSql += CRLF + " SELECT *                                                                         ";
                            strSql += CRLF + "   FROM HOST_EMPTY_PLT                                                            ";
                            strSql += CRLF + "  WHERE WH_TYP  = :WH_TYP                                                         ";
                            strSql += CRLF + "    AND KIND    = '" + strSTOCK_MODE + "'                                         ";
                            strSql += CRLF + "    AND STN     = '" + strMC_NO + "'                                              ";
                            strSql += CRLF + "    AND STATUS  IN ('N', 'Q')                                                     ";
                            strSql += CRLF + "    AND LUGG_NO = '" + strHOST_LUGG_NO + "'                                       ";
                            strSql += CRLF + "    AND TO_CHAR(NOW() - INS_DT, 'YYYYMMDDHH24MISS')::INTEGER > 60                 ";

                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            nSelCnt = _pBdb.ExcuteQry(dtHostEmptyPlt, strSql);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                _pBdb.Rollback();
                                return false;
                            }
                            if (nSelCnt == 0)
                            {
                                pRTN_MSG = "";
                                _pBdb.Rollback();
                                return true;
                            }
                            #endregion
                        }
                        #endregion
                    }


                    #region 요청정보를 Insert 한다.
                    strSql = "";
                    strSql += cDefApp.CRLF + " INSERT INTO                       ";
                    strSql += cDefApp.CRLF + "        HOST_EMPTY_PLT (           ";
                    strSql += cDefApp.CRLF + "                  WH_TYP           ";
                    strSql += cDefApp.CRLF + "                , KIND             ";
                    strSql += cDefApp.CRLF + "                , STN              ";
                    strSql += cDefApp.CRLF + "                , STATUS           ";
                    strSql += cDefApp.CRLF + "                , LUGG_NO          ";
                    strSql += cDefApp.CRLF + "                , INS_ID           ";
                    strSql += cDefApp.CRLF + "                , INS_DT           ";
                    strSql += cDefApp.CRLF + "                )                  ";
                    strSql += cDefApp.CRLF + "        VALUES (                   ";
                    strSql += cDefApp.CRLF + "            '" + strWH_TYP + "'    ";
                    strSql += cDefApp.CRLF + "          , '" + strSTOCK_MODE + "'";
                    strSql += cDefApp.CRLF + "          , '" + strMC_NO + "'     ";
                    strSql += cDefApp.CRLF + "          , 'N'                    ";
                    strSql += cDefApp.CRLF + "          , '0'                    ";
                    strSql += cDefApp.CRLF + "          , 'IO_TASK'              ";
                    strSql += cDefApp.CRLF + "          , " + DbLang.SYSDATE + " ";
                    strSql += cDefApp.CRLF + "                )                  ";
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt != 1)
                    {
                        strRTN_MSG += "AUTO_SC_WORK 등록 실패";
                        _pBdb.Rollback();
                        return false;
                    }
                    #endregion
                    #region 요청정보를 UPDATE 한다. 
                    //strSql = "";
                    //strSql += cDefApp.CRLF + " INSERT INTO                       ";
                    //strSql += cDefApp.CRLF + "        HOST_EMPTY_PLT (           ";
                    //strSql += cDefApp.CRLF + "                  WH_TYP           ";
                    //strSql += cDefApp.CRLF + "                , KIND             ";
                    //strSql += cDefApp.CRLF + "                , STN              ";
                    //strSql += cDefApp.CRLF + "                , STATUS           ";
                    //strSql += cDefApp.CRLF + "                , LUGG_NO          ";
                    //strSql += cDefApp.CRLF + "                , INS_ID           ";
                    //strSql += cDefApp.CRLF + "                , INS_DT           ";
                    //strSql += cDefApp.CRLF + "                )                  ";
                    //strSql += cDefApp.CRLF + "        VALUES (                   ";
                    //strSql += cDefApp.CRLF + "            '" + strWH_TYP + "'    ";
                    //strSql += cDefApp.CRLF + "          , '" + strSTOCK_MODE + "'";
                    //strSql += cDefApp.CRLF + "          , '" + strMC_NO + "'     ";
                    //strSql += cDefApp.CRLF + "          , 'N'                    ";
                    //strSql += cDefApp.CRLF + "          , '0'                    ";
                    //strSql += cDefApp.CRLF + "          , 'IO_TASK'              ";
                    //strSql += cDefApp.CRLF + "          , " + DbLang.SYSDATE + " ";
                    //strSql += cDefApp.CRLF + "                )                  ";
                    //nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    //if (nSelCnt != 1)
                    //{
                    //    strRTN_MSG += "AUTO_SC_WORK 등록 실패";
                    //    _pBdb.Rollback();
                    //    return false;
                    //}
                    #endregion
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

        // DEST POS 크레인일 경우 크레인 번호(LINE 번호 구해오기)
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
                strSql += CRLF + "       , " + DbLang.NVL + "(COUNT(*),'0') as total     ";
                strSql += CRLF + "       , " + DbLang.NVL + "(SUM(CASE WHEN CELL_STA = '0' AND CELL_USE_DEF = '0' THEN 1 END), 0) AS EMPTY ";
                strSql += CRLF + "       , " + DbLang.NVL + "(SUM(CASE WHEN CELL_STA = '0' AND CELL_USE_DEF = '0' THEN 1 END), 0) / COUNT(*) * 100  AS EMPTY_USAGE ";
                strSql += CRLF + "    FROM CELL_MST CM                  ";
                strSql += CRLF + "   INNER JOIN SC_DATA SD              ";
                strSql += CRLF + "      ON CM.WH_TYP         = SD.WH_TYP ";
                strSql += CRLF + "     AND CM.CELL_SC_NO     = SD.SC_NO  ";
                strSql += CRLF + "   WHERE CM.WH_TYP         = ? ";
                strSql += CRLF + "     AND CM.AGING_TYP like '%' " + DbLang.II + " ? " + DbLang.II + " '%' ";
                strSql += CRLF + "     AND SD.ONLINE_MODE_RD = '1' ";
                strSql += CRLF + "     AND SD.AUTO_MODE_RD   = '1' ";
                strSql += CRLF + "     AND SD.ERR_CODE_RD    = '0' ";
                strSql += CRLF + "     AND SD.ACTIVE_MODE_RD = '1' ";
                strSql += CRLF + "     AND SD.SUSPEND        IN ('0', '2') ";
                strSql += CRLF + "   GROUP BY CM.AGING_TYP       ";
                strSql += CRLF + "   ORDER BY EMPTY_USAGE desc   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                nSelCnt = _pBdb.ExcuteQry(dtCELL_MST, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    dtCELL_MST.Dispose();
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


        // 새작업 있는지 체크해서 Invoke 한다.
        public bool NEW_JOB_INVOKE_FOR_CV(string strWH_TYP,
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

                IsRtnMsg = "NEW_JOB_INVOKE_FOR_CV::";

                strSql = "";
                strSql += cDefApp.CRLF + " SELECT *                                 ";
                strSql += cDefApp.CRLF + "   FROM JOB_MST                           ";
                strSql += cDefApp.CRLF + "  WHERE WH_TYP	    = :WH_TYP           ";
                strSql += cDefApp.CRLF + "    AND (JOB_TYP = '1' OR JOB_TYP = '6')  ";
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
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " UPDATE JOB_MST                                ";
                    strSql += CRLF + "    SET JOB_STATUS    = '10'                   ";
                    strSql += CRLF + "      , UPD_DT        = " + DbLang.SYSDATE + " ";
                    strSql += CRLF + "      , UPD_USER_ID   = 'IO_TASK'              ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP                ";
                    strSql += CRLF + "    AND LUGG_NO       = :LUGG_NO               ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
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
                        pRTN_MSG += "변경할 작업 정보가 존재하지 않습니다. [LUGG NO : " + strLUGG_NO + "]";
                        _pBdb.Rollback();
                        m_strRtnMsg = pRTN_MSG;
                        throw new Exception(m_strRtnMsg);
                    }
                }

                pRTN_MSG = "[신규 작업]을 [CV 구동대기]로 변경하였습니다. [작업번호:" + strLUGG_NO + "]";

                _pBdb.Commit();

                InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "10");
                return true;

            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                _pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }

        //Invoke Check 후 이동작업이면 다음 목적지로 향하고 입고작업이면 CELL을 구한다
        public bool NEW_JOB_ORDER(string strWH_TYP,
                                  string strPLC_NO,
                              ref string pRTN_MSG)
        {
            try
            {
                string strLUGG_NO = "";
                string strTRACK_NO = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";

                string strJOB_START_POS = "";
                string strJOB_DEST_POS = "";
                string strJOB_DEST_LOC = "";
                string strPRODUCT_SIZE = "";
                string strDestPos = "";
                string strDestLocation = "";

                string strSENSOR0 = "";         // 파레트감지
                string strSENSOR1 = "";         // 1단감지
                string strSENSOR2 = "";         // 2단감지

                int nJobType = 1;

                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg = "NEW_JOB_ORDER::";

                strSql = "";
                strSql += cDefApp.CRLF + " SELECT CD.*, JM.*                            ";
                strSql += cDefApp.CRLF + "   FROM CV_DATA CD                            ";
                strSql += cDefApp.CRLF + "  INNER JOIN JOB_MST JM                       ";
                strSql += cDefApp.CRLF + "     ON CD.MC_NO = JM.START_POS               ";
                strSql += cDefApp.CRLF + "    AND JM.JOB_STATUS = '10'                  ";
                strSql += cDefApp.CRLF + "  WHERE CD.PLC_NO	= :PLC_NO                   ";
                strSql += cDefApp.CRLF + "    AND CD.LUGG_NO_RD 	= '0000'            ";
                strSql += cDefApp.CRLF + "    AND CD.STO_READY_RD 	= '1'               ";
                strSql += cDefApp.CRLF + "    AND CD.SENSOR0_DATA_RD = '1'              ";
                strSql += cDefApp.CRLF + "    AND CD.AUTO_MODE_RD 	= '1'               ";
                strSql += cDefApp.CRLF + "    AND CD.ERROR_CODE		IN ('0','0000')     ";
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_YN		= 'N'               ";
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_FLAG		= 'N'               ";
                //strSql += cDefApp.CRLF + "    AND CD.ERR_RQ_RD	    = '0'               ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += cDefApp.CRLF + "    AND CD.WH_TYP		    = :WH_TYP           ";
                strSql += cDefApp.CRLF + "    AND 0 = (SELECT COUNT(*)                  ";
                strSql += cDefApp.CRLF + "               FROM JOB_MST                   ";
                strSql += cDefApp.CRLF + "              WHERE LUGG_NO = CD.LUGG_NO_RD)  ";
                strSql += cDefApp.CRLF + "  LIMIT 1                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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
                    DataTable dtDestPos = new DataTable();

                    strTRACK_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    //strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    //strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strPRODUCT_SIZE = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strJOB_DEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strJOB_DEST_LOC = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strJOB_START_POS = "" + _pBdb.mDtMain.Rows[i]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_POS"].ToString();

                    strSENSOR1 = "" + _pBdb.mDtMain.Rows[i]["SENSOR1_DATA_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SENSOR1_DATA_RD"].ToString();
                    strSENSOR2 = "" + _pBdb.mDtMain.Rows[i]["SENSOR2_DATA_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SENSOR2_DATA_RD"].ToString();

                    //string strSENSOR1 = "";
                    //string strSENSOR2 = "";

                    // 목적지 가져오기 
                    if (SELECT_DEST_POS(strWH_TYP, strTRACK_NO, strJOB_DEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                    {
                        m_strRtnMsg = pRTN_MSG;
                        _pBdb.Rollback();
                        throw new Exception(m_strRtnMsg);
                    }

                    bool bSizeCheck = false;
                    switch (strPRODUCT_SIZE)
                    {
                        case "1": bSizeCheck = (strSENSOR1 == "1" && strSENSOR2 == "1") ? false : true;    break;
                        case "2": bSizeCheck = true;                                                       break;
                        case "3": bSizeCheck = true;                                                       break;  // 현재는 알수 없음!! - 추후 Roll 입고시 판단요망!!
                    }

                    // 매거진일 경우는 싸이즈 체크 할 필요가 없음!
                    if (strTRACK_NO == "108" || strTRACK_NO == "149")
                        bSizeCheck = true;

                    if (bSizeCheck == true)
                    {
                        // 설비타스크에 작업지시
                        if (UPDATE_CV_DATA(nJobType.ToString(), strPRODUCT_SIZE, "0", strDestPos, "0", strLUGG_NO, strWH_TYP, strPLC_NO, strJOB_START_POS, "", ref pRTN_MSG) == false)
                        {
                            m_strRtnMsg = pRTN_MSG;
                            _pBdb.Rollback();
                            throw new Exception(m_strRtnMsg);
                        }

                        // 작업상태 변경 
                        if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, nJobType.ToString(), ref pRTN_MSG) == false)
                        {
                            m_strRtnMsg = pRTN_MSG;
                            _pBdb.Rollback();
                            throw new Exception(m_strRtnMsg);
                        }

                        pRTN_MSG = strTRACK_NO + "입고대에서 CV_TASK를 통해서 작업 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                        _pBdb.Commit();
                        InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "11", strJOB_START_POS, strDestPos);
                        return true;
                    }
                    else 
                    {
                        strSql = "";
                        strSql += CRLF + " UPDATE CV_DATA                       ";
                        strSql += CRLF + "    SET CMD_RQ_ID         =  '1'      ";
                        strSql += CRLF + "      , PULP_SENSOR_OD    =  '4'      ";
                        strSql += CRLF + "      , CMD_RQ_YN         =  'Y'      ";
                        strSql += CRLF + "  WHERE WH_TYP            =  :WH_TYP  ";
                        strSql += CRLF + "    AND MC_NO             =  :MC_NO   ";

                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strTRACK_NO;
                        nSelCnt = _pBdb.ExcuteNonQry(strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            _pBdb.Rollback();
                            return false;
                        }

                        if (nSelCnt == 0)
                        {
                            pRTN_MSG += "변경할 MC NO 정보가 존재하지 않습니다. MC_NO : " + strTRACK_NO;
                            _pBdb.Rollback();
                            return false;
                        }

                        pRTN_MSG = strTRACK_NO + "입고대에서 단수 에러를 지시하였습니다. [작업정보상의 단수:" + strPRODUCT_SIZE + "][실제센서(파레트/1단화물/2단화물):" + strSENSOR0 + "/" + strSENSOR1 + "/" + strSENSOR2 + "]";
                        _pBdb.Commit();
                        InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "10", strJOB_START_POS, strDestPos);
                    }
                }
                pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                _pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }


        // BCR 및 기타C/V도착 후 작업정보완료(삭제)처리. 
        public bool ARRIVE_CV(string strWH_TYP,
                              string strPLC_NO,
                          ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "ARRIVE_CV::";

                // BCR 도착.
                strSql = "";
                strSql += cDefApp.CRLF + " SELECT JM.*, CD.*                                ";
                strSql += cDefApp.CRLF + "   FROM CV_DATA CD                                ";
                strSql += cDefApp.CRLF + "  INNER JOIN JOB_MST JM                           ";
                strSql += cDefApp.CRLF + "     ON CD.WH_TYP             = JM.WH_TYP 	    ";
                strSql += cDefApp.CRLF + "    AND CD.MC_NO           = JM.DEST_POS 	    ";
                strSql += cDefApp.CRLF + "    AND CD.LUGG_NO_RD         = JM.LUGG_NO        ";
                strSql += cDefApp.CRLF + "  WHERE CD.WH_TYP		        = :pWH_TYP          ";
                strSql += cDefApp.CRLF + "    AND CD.PLC_NO	            = :pPLC_NO          ";
                strSql += cDefApp.CRLF + "    AND CD.RET_READY_RD 	    = '1'               ";   // 출고대 READY ON
                strSql += cDefApp.CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'               ";   // 화물감지
                strSql += cDefApp.CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'               ";   // 자동모드
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_YN		    = 'N'               ";
                //strSql += cDefApp.CRLF + "    AND CD.READ_UPD_DT        > CD.WRITE_UPD_DT   ";
                //strSql += cDefApp.CRLF + "    AND CD.ERR_RQ_RD	        = '0'               ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += cDefApp.CRLF + "    AND JM.JOB_STATUS 	    IN ('11', '92')     ";
                strSql += cDefApp.CRLF + "    AND JM.DEST_POS Is not null                   ";
                _pBdb.mComMain.CommandType = CommandType.Text  ;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("pPLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";
                string strMC_NO = "";

                for (int i = 0; i < nSelCnt; i++)
                {
                    _pBdb.BeginTrans();
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strMC_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    
                    // 도착시 C/V에 상태정보 UPDATE처리.
                    // BCR READ상태(대기 -> READ완료)로 변경처리 => BCR 상태처리는 하지않음(주석처리 되어있음)-CV 상태만 처리함!
                    if (UPDATE_BCR_DATA(strWH_TYP
                                      , strPLC_NO
                                      , strSTART_POS
                                      , ref pRTN_MSG
                                      , strBCR_BOTTOM
                                      , strBCR_TOP) == false)
                    {
                        _pBdb.Rollback();
                        //return false;
                        continue;
                    }


                    //새로운 작업이 이미 있을때 삭제함.  - 현재는 MES TASK에서 두개 생성안하기 때문에 상관 없음
                    //if (DELETE_PREV_JOB("10", strDEST_POS, strWH_TYP, ref pRTN_MSG)) ; // 2019.11.08 임시주석.

                    //// 완제품창고
                    //if (strWH_TYP == "40" || strWH_TYP == "20")
                    //{
                        // 목적지 이동완료 (도착보고시 기존작업삭제 후 MES에서 새작업을 생성하기에 JOB_STATUS = '19' 로 처리한다.
                        if (UPDATE_JOB_DATA("19"
                                            , strLUGG_NO
                                            , strWH_TYP
                                            , strJOB_TYP
                                            , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            //return false;
                            continue;
                        }
                    //}
                    //else
                    //{
                    //    // 화성동창고
                    //    // 이동완료삭제.
                    //    if (DELETE_JOB_DATA("19"
                    //                       , strLUGG_NO
                    //                       , strWH_TYP
                    //                       , ref pRTN_MSG) == false)
                    //    {
                    //        _pBdb.Rollback();
                    //        //return false;
                    //        continue;
                    //    }
                        //}     
                        pRTN_MSG = strMC_NO + "출고대에서 HOST_TASK를 통해서 완료보고 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                    _pBdb.Commit();

                    InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "19");      
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        public bool SELECT_DEST_POS(string strWH_TYP            // 창고타입
                                  , string strTRACK_NO          // 현재위치
                                  , string strDEST_POS          // 작업 정보 상의 목적지
                              , ref string strDEFINED_DEST_POS  // 정의 되어 있는 목적지
                              , ref string pRTN_MSG             // 반환 메세지
                              , ref DataTable pdtDestPos)       // DATA TABLE 객체    
        {
            try
            {
                int nJobCnt = 0;
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "SELECT_DEST_POS::";

                // 목적지에 도착한 상태정보
                strSql = "";
                strSql += CRLF + " SELECT *                                                 ";
                strSql += CRLF + "   FROM CV_DEF_INF                                        ";
                strSql += CRLF + "  WHERE WH_TYP		    = :WH_TYP                       ";
                strSql += CRLF + "    AND FR_CV             = :FR_CV                        ";
                strSql += CRLF + "    AND BUFFER_CV 	    = :BUFFER_CV                    ";
                strSql += CRLF + "    AND USE_YON		    = 'Y'                           ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("FR_CV", DbLang.VARCHAR).Value = strTRACK_NO;
                _pBdb.mComMain.Parameters.Add("BUFFER_CV", DbLang.VARCHAR).Value = strDEST_POS;

                nJobCnt = _pBdb.ExcuteQry(pdtDestPos, strSql);
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

                strDEFINED_DEST_POS = "" + pdtDestPos.Rows[0]["TO_CV"].ToString() == "" ? "" : pdtDestPos.Rows[0]["TO_CV"].ToString();

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // C/V에 이동 또는 입고작업을 쓰는처리. (출고대READY)
        // ex)BCR 도착 후 Reading 완료시작.
        public bool CHECK_CV_RET_WC_READY(string strWH_TYP
                                     , string strPLC_NO
                                     , ref string pRTN_MSG)
        {
            try
            {
                int nJobCnt = 0;
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_RET_WC_READY::";

                // 목적지에 도착한 상태정보
                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*                              ";
                strSql += CRLF + "   FROM CV_DATA CD                              ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                         ";
                strSql += CRLF + "     ON CD.WH_TYP	    = JM.WH_TYP               ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD = JM.LUGG_NO              ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP         ";
                strSql += CRLF + "    AND CD.PLC_NO	            = :PLC_NO         ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'             ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'             ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'             ";
                strSql += CRLF + "    AND CD.READ_UPD_DT        >= CD.WRITE_UPD_DT";
                strSql += CRLF + "    AND CD.ERROR_CODE	        = '0'             ";
                strSql += CRLF + "    AND CD.MC_NO	            = '104'           ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '16'            ";
                strSql += CRLF + "    AND JM.DEST_POS           IS NOT NULL       ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '1'             ";                  // 출고대  READY ON
                strSql += CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')             ";    // 트랙 일시정지가 아니어야 함!
                strSql += CRLF + "    AND CD.MC_NO              = CD.DEST_POS_RD  ";
                strSql += CRLF + "    AND CD.MC_NO              <> JM.DEST_POS    ";                  //조한성추가 - 해당 함수는 중간목적지에서 진행시키는 부분인데, 이부분이 없으면 최종목적지에서도 이함수가 진행돼서 완료처리를 못하                                                                                                              는 경우가 생겼었음. 확인부탁드립니다 !
                //strSql += CRLF + "    AND JM.JOB_STATUS         IS NOT NULL     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strDEST_LOCATION = "";
                string strDURATION_TIME = "000000000";
                string strAGING_TYP = "";
                string strLOOP_NM = "";

                string strBOTTOM_TRAY1 = "";
                string strTOP_TRAY1 = "";

                string strTRACK_NO = "";
                string strDestPos = "";
                DataTable dtDestPos = new DataTable();
                DataTable dtDeadLock = new DataTable();


                for (int i = 0; i < nJobCnt; i++)
                {
                    _pBdb.BeginTrans();
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    strLOOP_NM = "" + _pBdb.mDtMain.Rows[i]["LOOP_NM"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LOOP_NM"].ToString();
                    strDURATION_TIME = "" + _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString() == "" ? "000000000" : _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString();

                    strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();

                    strTRACK_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();


                    //이동 작업일때 
                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Move)
                    {
                        #region 이동 작업일때
                        // C/V에 이동작업쓰기.
                        if (UPDATE_CV_DATA(strJOB_TYP
                                            , strTRAY_TYP
                                            , strTRAY_LEV
                                            , strDEST_POS
                                            , strIS_TURN
                                            , strLUGG_NO
                                            , strWH_TYP
                                            , strPLC_NO
                                            , strSTART_POS
                                            , ""
                                            , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            //return false;
                            continue;
                        }

                        // 작업정보변경 (CV도착보고)
                        if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                        {
                            _pBdb.Rollback();
                            //return false;
                            continue;
                        }

                        _pBdb.Commit();
                        return true;
                        #endregion
                    }
                    else if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto)
                    {
                        #region 입고 작업일때

                        DataTable dtCV_DATA = new DataTable();
                        if (strDEST_LOCATION == "00-000-00" || strDEST_LOCATION.Trim() == "0")
                        {
                            #region CELL 관리를 하는 경우
                            // AGING별로 S/C GROUP 목적지에 대한 이동화물댓수검색. 
                            strSql = "";
                            strSql += CRLF + " SELECT CD.*       ";
                            strSql += CRLF + "   FROM CV_DATA CD ";
                            strSql += CRLF + "  WHERE CD.WH_TYP	 = :WH_TYP ";
                            strSql += CRLF + "    AND CD.DEST_POS_RD like  '%' " + DbLang.II + " :DEST_POS " + DbLang.II + " '%' ";
                            strSql += CRLF + "    AND CD.LOOP_NM = :LOOP_NM ";
                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                            _pBdb.mComMain.Parameters.Add("LOOP_NM", DbLang.VARCHAR).Value = strLOOP_NM;
                            nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                _pBdb.Rollback();
                                //return false;
                                continue;
                            }

                            if (nSelCnt == 0)
                            {
                                strAGING_TYP = "";  // L/F앞까지 이동(S/C LINE 번호를 구하기위함)

                                // 제일 적게 들어있는 S/C 호기구하기.
                                if (GET_LESS_AGING(strDEST_POS
                                                    , strWH_TYP
                                                    , ref strAGING_TYP
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // AGING_TYP (LIFTER앞까지의 목적지)
                                // 해당호기앞 L/F까지 이동작업쓰기.
                                if (UPDATE_CV_DATA(strJOB_TYP
                                                    , strTRAY_TYP
                                                    , strTRAY_LEV
                                                    , strAGING_TYP
                                                    , strIS_TURN
                                                    , strLUGG_NO
                                                    , strWH_TYP
                                                    , strPLC_NO
                                                    , strSTART_POS
                                                    , ""
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // 작업정보쓰기 (CV도착보고)
                                if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strAGING_TYP) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                _pBdb.Commit();
                                return true;
                            }
                            else if (nSelCnt > 0)
                            {
                                strAGING_TYP = ""; // L/F앞까지 이동(S/C LINE 번호를 구하기위함)

                                // AGING 호기 시퀀스 구해오기.
                                // 26일날 테스트 해볼때 확실히 확인할 것. CWB**

                                if (SET_NEXT_AGING_BY_LESS_BANK(strWH_TYP
                                                                    , strDEST_POS
                                                                    , strDURATION_TIME
                                                                    , strLOOP_NM
                                                                    , ref strAGING_TYP
                                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // AGING_TYP (LIFTER앞까지의 목적지)
                                // 해당호기앞 L/F까지 이동작업쓰기.
                                if (UPDATE_CV_DATA(strJOB_TYP
                                                    , strTRAY_TYP
                                                    , strTRAY_LEV
                                                    , strAGING_TYP
                                                    , strIS_TURN
                                                    , strLUGG_NO
                                                    , strWH_TYP
                                                    , strPLC_NO
                                                    , strSTART_POS
                                                    , ""
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // 작업정보쓰기 (CV도착보고)
                                if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strAGING_TYP) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }
                                _pBdb.Commit();
                                return true;
                            }

                            dtCV_DATA.Dispose();
                            #endregion
                        }
                        else
                        {
                            #region CELL 관리 안하는 경우

                            //public bool SELECT_DEST_POS(string strWH_TYP                  // 창고타입
                            //                          , string strTRACK_NO                // 현재위치
                            //                          , string strDEST_POS                // 작업 정보 상의 목적지
                            //                      , ref string strDEFINED_DEST_POS        // 정의 되어 있는 목적지
                            //                      , ref string pRTN_MSG                   // 반환 메세지
                            //                      , ref DataTable pdtDestPos)             // DATA TABLE 객체

                            // 목적지 테이블에서 목적지 가져오기
                            if (SELECT_DEST_POS(strWH_TYP, strTRACK_NO, strDEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                            {

                                _pBdb.Rollback();
                                continue;
                            }

                            // 데드락 체크 
                            if (cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strTRACK_NO, strDEST_POS, ref pRTN_MSG, ref dtDeadLock) == false)
                            {
                                _pBdb.Rollback();
                                continue;
                            }

                            //위에서 도착지를 구하지 못한경우 도착지가 빈칸으로 들어가서 빈칸이면 컨티뉴
                            if (strDestPos == "")
                            {
                                _pBdb.Rollback();
                                continue;
                            }

                            // AGING_TYP (LIFTER앞까지의 목적지)
                            // 해당호기앞 L/F까지 이동작업쓰기.
                            if (UPDATE_CV_DATA(strJOB_TYP
                                                , strTRAY_TYP
                                                , strTRAY_LEV
                                                , strDestPos                // strDEST_POS
                                                , strIS_TURN
                                                , strLUGG_NO
                                                , strWH_TYP
                                                , strPLC_NO
                                                , strSTART_POS
                                                , ""
                                                , ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                //return false ;
                                continue;
                            }

                            //string strJobStatus = "11";     // CV 구동중
                            //if (strDEST_POS == strDestPos)  
                            //    strJobStatus = "12";        // CV 도착 보고 

                            // C/V구동완료
                            if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                            {
                                _pBdb.Rollback();
                                //return false;
                                continue;
                            }

                            _pBdb.Commit();
                            return true;

                            #endregion

                        }
                        #endregion
                    }
                    else if(Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Ret ||
                            Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                    {
                        #region 출고 작업일때

                        DataTable dtCV_DATA = new DataTable();

                        //조한성 추가
                        // 목적지 테이블에서 목적지 가져오기
                        if (SELECT_DEST_POS(strWH_TYP, strTRACK_NO, strDEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        //위에서 도착지를 구하지 못한경우 도착지가 빈칸으로 들어가서 빈칸이면 컨티뉴
                        if (strDestPos == "")
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        // 데드락 체크 
                        if (cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strTRACK_NO, strDEST_POS, ref pRTN_MSG, ref dtDeadLock) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        if (UPDATE_CV_DATA(strJOB_TYP
                                                , strTRAY_TYP
                                                , strTRAY_LEV
                                                , strDestPos                // strDEST_POS
                                                , strIS_TURN
                                                , strLUGG_NO
                                                , strWH_TYP
                                                , strPLC_NO
                                                , strSTART_POS
                                                , ""
                                                , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            //return false ;
                            continue;
                        }

                        // C/V 구동중
                        if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                        {
                            _pBdb.Rollback();
                            //return false;
                            continue;
                        }


                        _pBdb.Commit();
                        return true;

                        #endregion
                    }

                    pRTN_MSG = strTRACK_NO + "출고대에서 CV_TASK를 통해서 목적지변경 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                    _pBdb.Commit();
                    InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "11", strSTART_POS, strDestPos);
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }



        // C/V에 이동 또는 입고작업을 쓰는처리. (출고대READY)
        // ex)BCR 도착 후 Reading 완료시작.
        public bool CHECK_CV_RET_READY(string strWH_TYP
                                     , string strPLC_NO
                                     , ref string pRTN_MSG)
        {
            try
            {
                int nJobCnt = 0;
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_RET_READY::";

                // 목적지에 도착한 상태정보
                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*                                            ";
                strSql += CRLF + "   FROM CV_DATA CD                                            ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                                       ";
                strSql += CRLF + "     ON CD.WH_TYP	    = JM.WH_TYP                             ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD = JM.LUGG_NO                            ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP                       ";
                strSql += CRLF + "    AND CD.PLC_NO	            = :PLC_NO                       ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'                           ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'                           ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'                           ";
                strSql += CRLF + "    AND CD.READ_UPD_DT        >= CD.WRITE_UPD_DT              ";
                strSql += CRLF + "    AND CD.ERROR_CODE	        = '0'                           ";
                strSql += CRLF + "    AND CD.MC_NO	            <> '104'                        ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '11'                          ";
                strSql += CRLF + "    AND JM.DEST_POS           IS NOT NULL                     ";
                strSql += CRLF + "    AND (CD.RET_READY_RD 	    = '1' OR CD.STO_READY_RD = '1') ";  // 출고대 또는 입고대 READY ON
                strSql += CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                           ";    // 트랙 일시정지가 아니어야 함!
                strSql += CRLF + "    AND CD.MC_NO              = CD.DEST_POS_RD                ";
                strSql += CRLF + "    AND CD.MC_NO              <> JM.DEST_POS                  "; //조한성추가 - 해당 함수는 중간목적지에서 진행시키는 부분인데, 이부분이 없으면 최종목적지에서도 이함수가 진행돼서 완료처리를 못하는 경우가 생겼었음. 확인부탁드립니다 !
                strSql += CRLF + "    AND JM.JOB_STATUS         IS NOT NULL                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strDEST_LOCATION = "";
                string strDURATION_TIME = "000000000";
                string strAGING_TYP = "";
                string strLOOP_NM = "";

                string strBOTTOM_TRAY1 = "";
                string strTOP_TRAY1 = "";

                string strTRACK_NO = "";
                string strDestPos = "";
                DataTable dtDestPos = new DataTable();
                DataTable dtDeadLock = new DataTable();


                for (int i = 0; i < nJobCnt; i++)
                {
                    _pBdb.BeginTrans();
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    strLOOP_NM = "" + _pBdb.mDtMain.Rows[i]["LOOP_NM"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LOOP_NM"].ToString();
                    strDURATION_TIME = "" + _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString() == "" ? "000000000" : _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString();

                    strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();

                    strTRACK_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();


                    //이동 작업일때 
                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Move)
                    {
                        #region 이동 작업일때
                        // C/V에 이동작업쓰기.
                        if (UPDATE_CV_DATA(strJOB_TYP
                                            , strTRAY_TYP
                                            , strTRAY_LEV
                                            , strDEST_POS
                                            , strIS_TURN
                                            , strLUGG_NO
                                            , strWH_TYP
                                            , strPLC_NO
                                            , strSTART_POS
                                            , ""
                                            , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            //return false;
                            continue;
                        }

                        // 작업정보변경 (CV도착보고)
                        if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                        {
                            _pBdb.Rollback();
                            //return false;
                            continue;
                        }

                        _pBdb.Commit();
                        return true;
                        #endregion
                    }
                    else if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto)
                    {
                        #region 입고 작업일때

                        DataTable dtCV_DATA = new DataTable();
                        if (strDEST_LOCATION == "00-000-00" || strDEST_LOCATION.Trim() == "0")
                        {
                            #region CELL 관리를 하는 경우
                            // AGING별로 S/C GROUP 목적지에 대한 이동화물댓수검색. 
                            strSql = "";
                            strSql += CRLF + " SELECT CD.*       ";
                            strSql += CRLF + "   FROM CV_DATA CD ";
                            strSql += CRLF + "  WHERE CD.WH_TYP	 = :WH_TYP ";
                            strSql += CRLF + "    AND CD.DEST_POS_RD like  '%' " + DbLang.II + " :DEST_POS " + DbLang.II + " '%' ";
                            strSql += CRLF + "    AND CD.LOOP_NM = :LOOP_NM ";
                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                            _pBdb.mComMain.Parameters.Add("LOOP_NM", DbLang.VARCHAR).Value = strLOOP_NM;
                            nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                _pBdb.Rollback();
                                //return false;
                                continue;
                            }

                            if (nSelCnt == 0)
                            {
                                strAGING_TYP = "";  // L/F앞까지 이동(S/C LINE 번호를 구하기위함)

                                // 제일 적게 들어있는 S/C 호기구하기.
                                if (GET_LESS_AGING(strDEST_POS
                                                    , strWH_TYP
                                                    , ref strAGING_TYP
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // AGING_TYP (LIFTER앞까지의 목적지)
                                // 해당호기앞 L/F까지 이동작업쓰기.
                                if (UPDATE_CV_DATA(strJOB_TYP
                                                    , strTRAY_TYP
                                                    , strTRAY_LEV
                                                    , strAGING_TYP
                                                    , strIS_TURN
                                                    , strLUGG_NO
                                                    , strWH_TYP
                                                    , strPLC_NO
                                                    , strSTART_POS
                                                    , ""
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // 작업정보쓰기 (CV도착보고)
                                if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strAGING_TYP) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                _pBdb.Commit();
                                return true;
                            }
                            else if (nSelCnt > 0)
                            {
                                strAGING_TYP = ""; // L/F앞까지 이동(S/C LINE 번호를 구하기위함)

                                // AGING 호기 시퀀스 구해오기.
                                // 26일날 테스트 해볼때 확실히 확인할 것. CWB**

                                if (SET_NEXT_AGING_BY_LESS_BANK(strWH_TYP
                                                                    , strDEST_POS
                                                                    , strDURATION_TIME
                                                                    , strLOOP_NM
                                                                    , ref strAGING_TYP
                                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // AGING_TYP (LIFTER앞까지의 목적지)
                                // 해당호기앞 L/F까지 이동작업쓰기.
                                if (UPDATE_CV_DATA(strJOB_TYP
                                                    , strTRAY_TYP
                                                    , strTRAY_LEV
                                                    , strAGING_TYP
                                                    , strIS_TURN
                                                    , strLUGG_NO
                                                    , strWH_TYP
                                                    , strPLC_NO
                                                    , strSTART_POS
                                                    , ""
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // 작업정보쓰기 (CV도착보고)
                                if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strAGING_TYP) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }
                                _pBdb.Commit();
                                return true;
                            }

                            dtCV_DATA.Dispose();
                        #endregion
                        }
                        else
                        {
                            #region CELL 관리 안하는 경우

        //public bool SELECT_DEST_POS(string strWH_TYP                  // 창고타입
        //                          , string strTRACK_NO                // 현재위치
        //                          , string strDEST_POS                // 작업 정보 상의 목적지
        //                      , ref string strDEFINED_DEST_POS        // 정의 되어 있는 목적지
        //                      , ref string pRTN_MSG                   // 반환 메세지
        //                      , ref DataTable pdtDestPos)             // DATA TABLE 객체

                            // 목적지 테이블에서 목적지 가져오기
                            if (SELECT_DEST_POS(strWH_TYP, strTRACK_NO, strDEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                            {

                                _pBdb.Rollback();
                                continue;
                            }

                            // 데드락 체크 
                            if (cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strTRACK_NO, strDEST_POS, ref pRTN_MSG, ref dtDeadLock) == false)
                            {
                                _pBdb.Rollback();
                                continue;
                            }  
							
	                        //위에서 도착지를 구하지 못한경우 도착지가 빈칸으로 들어가서 빈칸이면 컨티뉴
	                        if (strDestPos == "")
	                        {
	                            _pBdb.Rollback();
	                            continue;
	                        }

                            // AGING_TYP (LIFTER앞까지의 목적지)
                            // 해당호기앞 L/F까지 이동작업쓰기.
                            if (UPDATE_CV_DATA(strJOB_TYP
                                                , strTRAY_TYP
                                                , strTRAY_LEV
                                                , strDestPos                // strDEST_POS
                                                , strIS_TURN
                                                , strLUGG_NO
                                                , strWH_TYP
                                                , strPLC_NO
                                                , strSTART_POS
                                                , ""
                                                , ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                //return false ;
                                continue;
                            }

                            //string strJobStatus = "11";     // CV 구동중
                            //if (strDEST_POS == strDestPos)  
                            //    strJobStatus = "12";        // CV 도착 보고 

                            // C/V구동완료
                            if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                            {
                                _pBdb.Rollback();
                                //return false;
                                continue;
                            }

                            _pBdb.Commit();
                            return true;

                            #endregion

                        }
                        #endregion
                    }
                    else if(Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Ret ||
                            Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                    {
                        #region 출고 작업일때

                        DataTable dtCV_DATA = new DataTable();

                        //// AGING_TYP (LIFTER앞까지의 목적지)
                        //// 해당호기앞 L/F까지 이동작업쓰기.
                        //if (UPDATE_CV_DATA(strJOB_TYP
                        //                    , strTRAY_TYP
                        //                    , strTRAY_LEV
                        //                    , strDestPos                // strDEST_POS
                        //                    , strIS_TURN
                        //                    , strLUGG_NO
                        //                    , strWH_TYP
                        //                    , strPLC_NO
                        //                    , strSTART_POS
                        //                    , ref pRTN_MSG) == false)
                        //{
                        //    _pBdb.Rollback();
                        //    //return false ;
                        //    continue;
                        //}


                        // C/V구동완료
                        //if (UPDATE_JOB_DATA("19", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                        //{
                        //    _pBdb.Rollback();
                        //    //return false;
                        //    continue;
                        //}

                        //조한성 추가
                        // 목적지 테이블에서 목적지 가져오기
                        if (SELECT_DEST_POS(strWH_TYP, strTRACK_NO, strDEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        //위에서 도착지를 구하지 못한경우 도착지가 빈칸으로 들어가서 빈칸이면 컨티뉴
                        if (strDestPos == "")
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        // 데드락 체크 
                        if (cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strTRACK_NO, strDEST_POS, ref pRTN_MSG, ref dtDeadLock) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        if (UPDATE_CV_DATA(strJOB_TYP
                                                , strTRAY_TYP
                                                , strTRAY_LEV
                                                , strDestPos                // strDEST_POS
                                                , strIS_TURN
                                                , strLUGG_NO
                                                , strWH_TYP
                                                , strPLC_NO
                                                , strSTART_POS
                                                , ""
                                                , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            //return false ;
                            continue;
                        }

                        _pBdb.Commit();
                        return true;

                        #endregion
                    }

                    pRTN_MSG = strTRACK_NO + "출고대에서 CV_TASK를 통해서 목적지변경 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                    _pBdb.Commit();
                    InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "11", strSTART_POS, strDestPos);
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // C/V에 이동 또는 입고작업을 쓰는처리. (출고대READY)
        // ex)BCR 도착 후 Reading 완료시작.
        public bool CHECK_CV_RET_READY_148(string strWH_TYP
                                         , string strPLC_NO
                                     , ref string pRTN_MSG)
        {
            try
            {
                int nJobCnt = 0;
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_RET_READY_148::";

                // 목적지에 도착한 상태정보
                strSql = "";
                strSql += CRLF + " SELECT CD.*                                                                              ";
                strSql += CRLF + "   FROM CV_DATA CD                                                                        ";
                strSql += CRLF + "  WHERE CD.WH_TYP		     = :WH_TYP                                                      ";
                strSql += CRLF + "    AND CD.PLC_NO	         = :PLC_NO                                                      ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD = '1'                                                          ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	 = '1'                                                          ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		 = 'N'                                                          ";
                strSql += CRLF + "    AND CD.READ_UPD_DT     >= CD.WRITE_UPD_DT                                             ";
                strSql += CRLF + "    AND CD.ERROR_CODE	     = '0'                                                          ";
                strSql += CRLF + "    AND CD.MC_NO	         = '148'                                                        ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	 = '1'                                                          ";      // 출고대 또는 입고대 READY ON
                strSql += CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                                                          ";      // 트랙 일시정지가 아니어야 함!
                strSql += CRLF + "    AND CD.MC_NO           = CD.DEST_POS_RD                                               ";
                strSql += CRLF + "    and 0 = (CASE WHEN CD.LUGG_NO_RD = (SELECT LUGG_NO                                    ";
                strSql += CRLF + "                                          FROM JOB_MST                                    ";
                strSql += CRLF + "                                         WHERE LUGG_NO = CD.LUGG_NO_RD)THEN 1 ELSE 0 END) "; 
                strSql += CRLF + "                                    ";
                
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

        //// C/V에 목적지정보를 적는다.
        //public bool UPDATE_CV_DATA(string strJOB_TYP,
        //                           string strTRAY_TYP,
        //                           string strTRAY_LEV,
        //                           string strDEST_POS,
        //                           string strIS_TURN,
        //                           string strLUGG_NO,
        //                           string strWH_TYP,
        //                           string strPLC_NO,
        //                           string strSTART_POS,
        //                       ref string pRTN_MSG,
        //                           string strBCR_BOTTOM = "",
        //                           string strBCR_TOP = ""

                string strJOB_TYP = "" + _pBdb.mDtMain.Rows[0]["JOB_TYP_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP_RD"].ToString();
                string strTRAY_TYP = "" + _pBdb.mDtMain.Rows[0]["TRAY_TYP_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_TYP_RD"].ToString();
                string strTRAY_LEV = "" + _pBdb.mDtMain.Rows[0]["TRAY_LEV_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_LEV_RD"].ToString();
                string strDEST_POS = "151";
                string strIS_TURN = "" + _pBdb.mDtMain.Rows[0]["IS_TURN_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["IS_TURN_RD"].ToString();
                string strLUGG_NO = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO_RD"].ToString();
                string strSTART_POS = "" + _pBdb.mDtMain.Rows[0]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["MC_NO"].ToString();
                string strTRACK_NO = "" + _pBdb.mDtMain.Rows[0]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["MC_NO"].ToString();

                if (UPDATE_CV_DATA(strJOB_TYP
                                    , strTRAY_TYP
                                    , strTRAY_LEV
                                    , strDEST_POS
                                    , strIS_TURN
                                    , strLUGG_NO
                                    , strWH_TYP
                                    , strPLC_NO
                                    , strSTART_POS
                                    , ""
                                    , ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                #region 이거는 아무 의미 없음
                /*
                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strDEST_LOCATION = "";
                string strDURATION_TIME = "000000000";
                string strAGING_TYP = "";
                string strLOOP_NM = "";

                string strBOTTOM_TRAY1 = "";
                string strTOP_TRAY1 = "";

                string strDestPos = "";
                DataTable dtDestPos = new DataTable();
                DataTable dtDeadLock = new DataTable();


                for (int i = 0; i < nJobCnt; i++)
                {
                    _pBdb.BeginTrans();
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    strLOOP_NM = "" + _pBdb.mDtMain.Rows[i]["LOOP_NM"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LOOP_NM"].ToString();
                    strDURATION_TIME = "" + _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString() == "" ? "000000000" : _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString();

                    strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();

                    strTRACK_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();

                    #region 이거는 아무 의미 없음
                    
                    //이동 작업일때 
                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Move)
                    {
                        #region 이동 작업일때
                        // C/V에 이동작업쓰기.
                        if (UPDATE_CV_DATA(strJOB_TYP
                                            , strTRAY_TYP
                                            , strTRAY_LEV
                                            , strDEST_POS
                                            , strIS_TURN
                                            , strLUGG_NO
                                            , strWH_TYP
                                            , strPLC_NO
                                            , strSTART_POS
                                            , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            //return false;
                            continue;
                        }

                        // 작업정보변경 (CV도착보고)
                        if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                        {
                            _pBdb.Rollback();
                            //return false;
                            continue;
                        }

                        _pBdb.Commit();
                        return true;
                        #endregion
                    }
                    else if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto)
                    {
                        #region 입고 작업일때

                        DataTable dtCV_DATA = new DataTable();
                        if (strDEST_LOCATION == "00-000-00" || strDEST_LOCATION.Trim() == "0")
                        {
                            #region CELL 관리를 하는 경우
                            // AGING별로 S/C GROUP 목적지에 대한 이동화물댓수검색. 
                            strSql = "";
                            strSql += CRLF + " SELECT CD.*       ";
                            strSql += CRLF + "   FROM CV_DATA CD ";
                            strSql += CRLF + "  WHERE CD.WH_TYP	 = :WH_TYP ";
                            strSql += CRLF + "    AND CD.DEST_POS_RD like  '%' " + DbLang.II + " :DEST_POS " + DbLang.II + " '%' ";
                            strSql += CRLF + "    AND CD.LOOP_NM = :LOOP_NM ";
                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                            _pBdb.mComMain.Parameters.Add("LOOP_NM", DbLang.VARCHAR).Value = strLOOP_NM;
                            nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                _pBdb.Rollback();
                                //return false;
                                continue;
                            }

                            if (nSelCnt == 0)
                            {
                                strAGING_TYP = "";  // L/F앞까지 이동(S/C LINE 번호를 구하기위함)

                                // 제일 적게 들어있는 S/C 호기구하기.
                                if (GET_LESS_AGING(strDEST_POS
                                                    , strWH_TYP
                                                    , ref strAGING_TYP
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // AGING_TYP (LIFTER앞까지의 목적지)
                                // 해당호기앞 L/F까지 이동작업쓰기.
                                if (UPDATE_CV_DATA(strJOB_TYP
                                                    , strTRAY_TYP
                                                    , strTRAY_LEV
                                                    , strAGING_TYP
                                                    , strIS_TURN
                                                    , strLUGG_NO
                                                    , strWH_TYP
                                                    , strPLC_NO
                                                    , strSTART_POS
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // 작업정보쓰기 (CV도착보고)
                                if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strAGING_TYP) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                _pBdb.Commit();
                                return true;
                            }
                            else if (nSelCnt > 0)
                            {
                                strAGING_TYP = ""; // L/F앞까지 이동(S/C LINE 번호를 구하기위함)

                                // AGING 호기 시퀀스 구해오기.
                                // 26일날 테스트 해볼때 확실히 확인할 것. CWB**

                                if (SET_NEXT_AGING_BY_LESS_BANK(strWH_TYP
                                                                    , strDEST_POS
                                                                    , strDURATION_TIME
                                                                    , strLOOP_NM
                                                                    , ref strAGING_TYP
                                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // AGING_TYP (LIFTER앞까지의 목적지)
                                // 해당호기앞 L/F까지 이동작업쓰기.
                                if (UPDATE_CV_DATA(strJOB_TYP
                                                    , strTRAY_TYP
                                                    , strTRAY_LEV
                                                    , strAGING_TYP
                                                    , strIS_TURN
                                                    , strLUGG_NO
                                                    , strWH_TYP
                                                    , strPLC_NO
                                                    , strSTART_POS
                                                    , ref pRTN_MSG) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }

                                // 작업정보쓰기 (CV도착보고)
                                if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strAGING_TYP) == false)
                                {
                                    _pBdb.Rollback();
                                    //return false;
                                    continue;
                                }
                                _pBdb.Commit();
                                return true;
                            }

                            dtCV_DATA.Dispose();
                            #endregion
                        }
                        else
                        {
                            #region CELL 관리 안하는 경우

                            //public bool SELECT_DEST_POS(string strWH_TYP                  // 창고타입
                            //                          , string strTRACK_NO                // 현재위치
                            //                          , string strDEST_POS                // 작업 정보 상의 목적지
                            //                      , ref string strDEFINED_DEST_POS        // 정의 되어 있는 목적지
                            //                      , ref string pRTN_MSG                   // 반환 메세지
                            //                      , ref DataTable pdtDestPos)             // DATA TABLE 객체

                            // 목적지 테이블에서 목적지 가져오기
                            if (SELECT_DEST_POS(strWH_TYP, strTRACK_NO, strDEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                            {

                                _pBdb.Rollback();
                                continue;
                            }

                            // 데드락 체크 
                            if (cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strTRACK_NO, strDEST_POS, ref pRTN_MSG, ref dtDeadLock) == false)
                            {
                                _pBdb.Rollback();
                                continue;
                            }

                            //위에서 도착지를 구하지 못한경우 도착지가 빈칸으로 들어가서 빈칸이면 컨티뉴
                            if (strDestPos == "")
                            {
                                _pBdb.Rollback();
                                continue;
                            }

                            // AGING_TYP (LIFTER앞까지의 목적지)
                            // 해당호기앞 L/F까지 이동작업쓰기.
                            if (UPDATE_CV_DATA(strJOB_TYP
                                                , strTRAY_TYP
                                                , strTRAY_LEV
                                                , strDestPos                // strDEST_POS
                                                , strIS_TURN
                                                , strLUGG_NO
                                                , strWH_TYP
                                                , strPLC_NO
                                                , strSTART_POS
                                                , ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                //return false ;
                                continue;
                            }

                            //string strJobStatus = "11";     // CV 구동중
                            //if (strDEST_POS == strDestPos)  
                            //    strJobStatus = "12";        // CV 도착 보고 

                            // C/V구동완료
                            if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                            {
                                _pBdb.Rollback();
                                //return false;
                                continue;
                            }

                            _pBdb.Commit();
                            return true;

                            #endregion

                        }
                        #endregion
                    }
                    else if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Ret ||
                            Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                    {
                        #region 출고 작업일때

                        DataTable dtCV_DATA = new DataTable();

                        //// AGING_TYP (LIFTER앞까지의 목적지)
                        //// 해당호기앞 L/F까지 이동작업쓰기.
                        //if (UPDATE_CV_DATA(strJOB_TYP
                        //                    , strTRAY_TYP
                        //                    , strTRAY_LEV
                        //                    , strDestPos                // strDEST_POS
                        //                    , strIS_TURN
                        //                    , strLUGG_NO
                        //                    , strWH_TYP
                        //                    , strPLC_NO
                        //                    , strSTART_POS
                        //                    , ref pRTN_MSG) == false)
                        //{
                        //    _pBdb.Rollback();
                        //    //return false ;
                        //    continue;
                        //}


                        // C/V구동완료
                        //if (UPDATE_JOB_DATA("19", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strDEST_POS) == false)
                        //{
                        //    _pBdb.Rollback();
                        //    //return false;
                        //    continue;
                        //}

                        //조한성 추가
                        // 목적지 테이블에서 목적지 가져오기
                        if (SELECT_DEST_POS(strWH_TYP, strTRACK_NO, strDEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        //위에서 도착지를 구하지 못한경우 도착지가 빈칸으로 들어가서 빈칸이면 컨티뉴
                        if (strDestPos == "")
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        // 데드락 체크 
                        if (cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strTRACK_NO, strDEST_POS, ref pRTN_MSG, ref dtDeadLock) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        if (UPDATE_CV_DATA(strJOB_TYP
                                                , strTRAY_TYP
                                                , strTRAY_LEV
                                                , strDestPos                // strDEST_POS
                                                , strIS_TURN
                                                , strLUGG_NO
                                                , strWH_TYP
                                                , strPLC_NO
                                                , strSTART_POS
                                                , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            //return false ;
                            continue;
                        }

                        _pBdb.Commit();
                        return true;

                        #endregion
                    }
                
                    #endregion

                    pRTN_MSG = strTRACK_NO + "148 출고대에서 CV_TASK를 통해서 목적지변경 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                    _pBdb.Commit();
                    InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "11", strSTART_POS, strDestPos);
                }
                //*/
                #endregion

                pRTN_MSG = strTRACK_NO + "출고대에서 CV_TASK를 통해서 목적지변경 요청하였습니다. [작업번호:" + strLUGG_NO + "][목적지:" + strDEST_POS + "]";

                _pBdb.Commit();
                InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "11", strSTART_POS, strDEST_POS);

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // 2020.04.27 BYJUNG        
        // 입고 H/S 도착완료 상위 송신 부분
        public bool CHECK_CV_STOHS(string strWH_TYP,
                                   string strPLC_NO,
                               ref string pRTN_MSG)
        {
            try
            {

                int nMainSelCnt = 0;

                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_STOHS::";

                strSql = "";
                strSql += cDefApp.CRLF + " SELECT JM.*, CD.*, SHD.*                         ";
                strSql += cDefApp.CRLF + "   FROM CV_DATA CD                                ";
                strSql += cDefApp.CRLF + "  INNER JOIN JOB_MST JM                           ";
                strSql += cDefApp.CRLF + "     ON CD.WH_TYP             = JM.WH_TYP 	    ";
                strSql += cDefApp.CRLF + "    AND CD.LUGG_NO_RD         = JM.LUGG_NO        ";
                strSql += cDefApp.CRLF + "    AND CD.DEST_POS_RD        = JM.DEST_POS       ";
                strSql += cDefApp.CRLF + "  INNER JOIN SC_HS_DEF SHD                        ";
                strSql += cDefApp.CRLF + "     ON CD.WH_TYP             = SHD.WH_TYP 	    ";
                strSql += cDefApp.CRLF + "    AND CD.MC_NO              = SHD.HS_MC_NO      ";
                strSql += cDefApp.CRLF + "    AND CD.DEST_POS_RD        = SHD.SC_NO         ";
                strSql += cDefApp.CRLF + "  WHERE CD.WH_TYP		        = :pWH_TYP          ";
                strSql += cDefApp.CRLF + "    AND CD.PLC_NO	            = :pPLC_NO          ";
                strSql += cDefApp.CRLF + "    AND CD.STOHS_READY_RD     = '1'               ";   // 입고HS READY ON
                strSql += cDefApp.CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'               ";   // 화물감지
                strSql += cDefApp.CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'               ";   // 자동모드
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_YN		    = 'N'               ";
                strSql += cDefApp.CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')               ";    // 트랙 일시정지가 아니어야 함!
                strSql += cDefApp.CRLF + "    AND CD.READ_UPD_DT        > CD.WRITE_UPD_DT   ";
                strSql += cDefApp.CRLF + "    AND JM.JOB_STATUS 	    IN ('11', '92')     ";
                strSql += cDefApp.CRLF + "    AND JM.DEST_POS Is not null                   ";
                strSql += cDefApp.CRLF + "    AND SHD.HS_NO IN ('01','03')                  ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("pPLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                nMainSelCnt = _pBdb.ExcuteQry(strSql);
                if (nMainSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nMainSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";
                string strMC_NO = "";

                for (int i = 0; i < nMainSelCnt; i++)
                {
                    _pBdb.BeginTrans();
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strMC_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    
                    
                    // 도착시 C/V에 상태정보 UPDATE처리.
                    // BCR READ상태(대기 -> READ완료)로 변경처리 => BCR 상태처리는 하지않음(주석처리 되어있음)-CV 상태만 처리함!
                    if (UPDATE_BCR_DATA(strWH_TYP
                                      , strPLC_NO
                                      , strSTART_POS
                                      , ref pRTN_MSG
                                      , strBCR_BOTTOM
                                      , strBCR_TOP) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }

                    string strJOB_STATUS = "12";
                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle)
                    {
                        strJOB_STATUS = "18";
                    }
                    
                    if (UPDATE_JOB_DATA(strJOB_STATUS
                                        , strLUGG_NO
                                        , strWH_TYP
                                        , strJOB_TYP
                                        , ref pRTN_MSG
                                        , "0"
                                        , "0"
                                        , strMC_NO) == false)
                    {
                        _pBdb.Rollback();

                        continue;
                    }

                    pRTN_MSG = strMC_NO + "입고H/S에서 HOST_TASK로 도착보고 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                    _pBdb.Commit();

                    InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, strJOB_STATUS);
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // 2019.09.23 SMLEE (최대리전달사항으로 수정)
        // 출고H/S -> 메인물류 대기대(WAIT TRACK)
        public bool CHECK_CV_RETHS(string strWH_TYP,
                                   string strPLC_NO,
                               ref string pRTN_MSG)
        {
            try
            {
                int nMainSelCnt = 0;

                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_RETHS::";

                strSql = "";
                strSql += CRLF + " SELECT  JM.*, CD.*, SHD.*                                                       ";
                strSql += CRLF + "   FROM  JOB_MST JM                                                              ";
                strSql += CRLF + "  INNER  JOIN CV_DATA CD                                                         ";
                strSql += CRLF + "     ON  JM.WH_TYP             = CD.WH_TYP                                       ";
                strSql += CRLF + "    AND  JM.HS_TRACK_NO        = CD.MC_NO                                        ";
                strSql += CRLF + "   LEFT  OUTER       JOIN        SC_HS_DEF SHD                                   ";
                strSql += CRLF + "     ON  JM.WH_TYP             = SHD.WH_TYP                                      ";
                strSql += CRLF + "    AND (JM.HS_TRACK_NO        = SHD.HS_MC_NO OR JM.HS_TRACK_NO = SHD.PAIR_HS_NO)";
                strSql += CRLF + "    AND  JM.DEST_POS           = SHD.DEST_DEF_DAT                                ";
                strSql += CRLF + "  WHERE  CD.WH_TYP             = :WH_TYP                                         ";
                strSql += CRLF + "    AND  CD.PLC_NO             = :PLC_NO                                         ";
                strSql += CRLF + "    AND  CD.RET_HS_YN          = 'Y'                                             ";   // H/S여부
                //strSql += CRLF + "    AND  CD.TR_PAUSE_RD        = '0'                                           ";    // 트랙 일시정지가 아니어야 함! - 안보는게 나을듯!
                strSql += CRLF + "    AND  CD.SENSOR0_DATA_RD    = '1'                                             ";
                strSql += CRLF + "    AND  JM.JOB_STATUS 	     = '28'                                            ";   // 도착 보고 완료
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strSC_NO = "";
                string strSTART_LOCATION = "";
                string strWAIT_TRACK = "";
                string strROLL_USE_YN = "";
                DataTable dtDestPos = new DataTable();
                string strLOT_NO = "";

                for (int i = 0; i < nMainSelCnt; i++)
                {
                    _pBdb.BeginTrans();

                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    strSC_NO = "" + _pBdb.mDtMain.Rows[i]["SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SC_NO"].ToString();
                    strSTART_LOCATION = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString();
                    strWAIT_TRACK = "" + _pBdb.mDtMain.Rows[i]["WAIT_TRACK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WAIT_TRACK"].ToString();
                    strROLL_USE_YN = "" + _pBdb.mDtMain.Rows[i]["ROLL_USE_YN"].ToString() == "" ? "N" : _pBdb.mDtMain.Rows[i]["ROLL_USE_YN"].ToString();
                    strLOT_NO = "" + _pBdb.mDtMain.Rows[i]["LOT_NO"].ToString().Trim();

                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Move ||
                        Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Ret ||
                        Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.OtherRet ||
                        Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.FireRet ||
                        Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RackRet ||
                        Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle) //조한성 추가 - 호기간이동
                    {
                        //// CELL 초기화 (공CELL)
                        //if (Convert.ToInt32(strSC_NO) < 29700)
                        //{
                        //    //if (strJOB_TYP != cDefApp.eJOBTYP.FireRet.ToString())
                        //    if(Convert.ToInt16(strJOB_TYP) != (int)cDefApp.eJOBTYP.FireRet)
                        //    {
                        //        if (UPDATE_CELL_MST_DTL("0", "0", "0", strWH_TYP, strSC_NO, strSTART_LOCATION, ref pRTN_MSG) == false)
                        //        {
                        //            _pBdb.Rollback();
                        //            //return false;
                        //            continue;
                        //        }
                        //    }
                        //}

                        ////출고 HS에서 물류 대기대로 검색하여 이동.
                        //if (GET_WAIT_TRACK_NO(strWH_TYP
                        //                    , strSC_NO
                        //                    , strDEST_POS
                        //                    , ref strWAIT_TRACK
                        //                    , ref pRTN_MSG) == false)
                        //{
                        //    _pBdb.Rollback();
                        //    //return false;
                        //    continue;
                        //}

                        // WAIT TRACK (메인물류 이동전, 대기 TRACK)
                        if (strWAIT_TRACK == "0")
                        {
                            strWAIT_TRACK = strDEST_POS;
                        }

                        string strDestPos = "";

                        if (SELECT_DEST_POS(strWH_TYP, strSTART_POS, strDEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }

                        // C/V에 목적지정보쓰기.
                        if (UPDATE_CV_DATA(strJOB_TYP
                                         , strTRAY_TYP
                                         , strTRAY_LEV
                                         , strDestPos           // strWAIT_TRACK
                                         , strIS_TURN
                                         , strLUGG_NO
                                         , strWH_TYP
                                         , strPLC_NO
                                         , strSTART_POS
                                         , strLOT_NO    //파쇄기 라인 출고 품목 표시를 위한 수정 (조한성. 0302)
                                         , ref pRTN_MSG) == false)
                        {

                            //return false;
                            _pBdb.Rollback();
                            continue;
                        }

                        // 작업시작(구동중)
                        //if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                        //{
                        //    _pBdb.Rollback();
                        //    continue;
                        //}

                        if (strROLL_USE_YN == "H") //2021.01.30 JHS -> 파쇄기 라인 HS으로 설정되어 있는거는 CV에 데이터 쓰면서 작업은 바로 도착보고를 바로 함 !!
                        {
                            // 작업완료(완료보고)
                            if (UPDATE_JOB_DATA("19", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                continue;
                            }
                        }
                        else
                        {
                            // 작업시작(구동중)
                            if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                continue;
                            }
                        }

                        

                        // S/C 작업번호 초기화
                        strSql = "";
                        strSql += CRLF + "  UPDATE SC_DATA                          ";
                        strSql += CRLF + "     SET ITN_LUGG_FK1        =  '0'       ";
                        strSql += CRLF + "       , ITN_LUGG_FK2        =  '0'       ";  
                        strSql += CRLF + "       , OD_USER_ID          =  'IOTASK'  ";
                        strSql += CRLF + "       , OD_UPD_DT           =  " + DbLang.SYSDATE + "     ";
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
                            continue;
                        }

                        if (nSelCnt == 0)
                        {
                            continue;

                            //pRTN_MSG += "변경할 SC_DATA 정보가 존재하지 않습니다. SC_NO : " + strSC_NO;
                            //_pBdb.Rollback();
                            //return false;
                        }

                        pRTN_MSG = strSTART_POS + "출고 H/S에 CV_TASK를 통해서 DATA기록 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                        _pBdb.Commit();

                        InsertLog(((int)cDefApp.eWHTYP.SKI_WH01).ToString(), strRTN_MSG, "", "", strLUGG_NO, "11", strSTART_POS, strDestPos);
                        continue;

                    }
                    _pBdb.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }


        // SC 호기 검색 - 검색 조건과 비슷한 출고시간에서 열이 가장 적은 곳으로 입고
        public bool SET_NEXT_AGING_BY_LESS_BANK(string strWH_TYP,
                                                string strDEST_POS,
                                                string strDURATION_TIME,
                                                string strLOOP_NM,
                                            ref string strAGING_TYP,
                                            ref string pRTN_MSG)
        {
            int nAGING_CNT = 0;
            try
            {
                pRTN_MSG += "SET_NEXT_AGING_BY_LESS_BANK::";

                DataTable dtCV_DATA = new DataTable();
                DataTable dtSC_DATA = new DataTable();

                DataTable dtSYS_DATE = new DataTable();
                DataTable dtAGING = new DataTable();

                int nSelCnt = 0;
                string strSql = "";

                // 작업예정시간(DURATION_TIME [MES수신])
                strSql = "";
                strSql += cDefApp.CRLF + "SELECT " + DbLang.SYSDATE + " + (1/24/60/60)*((TO_NUMBER(SUBSTR(:DURATION_TIME1 ,1 ,2)) * 60 * 60 * 24)  ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME2 ,3 ,2)) * 60 * 60)       ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME3 ,5 ,2)) * 60)            ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME4 ,7 ,2)))) AS END_TIME    ";
                strSql += cDefApp.CRLF + " from DUAL ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("DURATION_TIME1", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME2", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME3", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME4", DbLang.VARCHAR).Value = strDURATION_TIME;
                nSelCnt = _pBdb.ExcuteQry(dtSYS_DATE, strSql);
                if (nSelCnt <= 0)
                {
                    dtAGING.Dispose();
                    dtSYS_DATE.Dispose();
                    dtCV_DATA.Dispose();
                    dtSC_DATA.Dispose();

                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt > 0)
                {

                    DateTime dtEND_TIME = new DateTime();
                    dtEND_TIME = Convert.ToDateTime(dtSYS_DATE.Rows[0]["END_TIME"].ToString());

                    strSql = "";
                    strSql += cDefApp.CRLF + "SELECT CM.AGING_TYP, 0 AS DURATION_TIME, 0 AS BANK_CNT                                        ";
                    strSql += cDefApp.CRLF + "      ,RANK() OVER (ORDER BY DBMS_RANDOM.VALUE) AS PRIORITY                                   ";
                    strSql += cDefApp.CRLF + "  FROM CELL_MST CM                                                                            ";
                    strSql += cDefApp.CRLF + " INNER JOIN SC_DATA SD            ";
                    strSql += cDefApp.CRLF + "    ON CM.WH_TYP = SD.WH_TYP      ";
                    strSql += cDefApp.CRLF + "   AND CM.CELL_SC_NO = SD.SC_NO   ";
                    strSql += cDefApp.CRLF + "   AND SD.ONLINE_MODE_RD = '1'    ";
                    strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD   = '1'    ";
                    strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD    = '0'    ";
                    strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD = '1'    ";
                    strSql += cDefApp.CRLF + "   AND SD.SUSPEND IN('0', '2')    ";
                    strSql += cDefApp.CRLF + " WHERE CM.WH_TYP = :WH_TYP1                                                                   ";
                    strSql += cDefApp.CRLF + "   AND 0 = ( SELECT COUNT(CD.CELL_SC_NO)                                                      ";
                    strSql += cDefApp.CRLF + " 	             FROM CELL_DTL CD                                                               ";
                    strSql += cDefApp.CRLF + "  	        WHERE CD.WH_TYP = CM.WH_TYP                                                     ";
                    strSql += cDefApp.CRLF + "  	          AND CONCAT(SUBSTR(CD.CELL_SC_NO, 0, 3),SUBSTR(CD.CELL_SC_NO, 5, 1))           ";
                    strSql += cDefApp.CRLF + "                  = CONCAT(SUBSTR(CM.CELL_SC_NO, 0, 3), SUBSTR(CM.CELL_SC_NO, 5, 1)))         ";
                    strSql += cDefApp.CRLF + "   AND SUBSTR(CM.CELL_SC_NO, 0, 3) = :DEST_POS1                                               ";

                    strSql += cDefApp.CRLF + " GROUP BY AGING_TYP                                                                           ";
                    strSql += cDefApp.CRLF + " UNION ALL                                                                                    ";
                    if (strDURATION_TIME != "000000000")
                    {
                        strSql += cDefApp.CRLF + " SELECT CM.AGING_TYP, ABS(MIN((:END_DT1 - CD.AGING_END_DT) * 24 * 60)) AS DURATION_TIME, COUNT(CM.BANK) AS COUNT   ";
                        strSql += cDefApp.CRLF + "  	 ,RANK() OVER (ORDER BY COUNT(CM.BANK)) AS PRIORITY                                                             ";
                        strSql += cDefApp.CRLF + "   FROM CELL_MST CM                                                                                                   ";
                        strSql += cDefApp.CRLF + "  INNER JOIN CELL_DTL CD                                                                                              ";
                        strSql += cDefApp.CRLF + "          ON CD.WH_TYP = CM.WH_TYP                                                                                    ";
                        strSql += cDefApp.CRLF + "         AND CD.CELL_NO = CM.CELL_NO                                                                                  ";
                        strSql += cDefApp.CRLF + "         AND SUBSTR(CD.CELL_SC_NO, 0, 3) = SUBSTR(CM.AGING_TYP, 0, 3)                                                 ";
                        strSql += cDefApp.CRLF + "        AND ABS(((:END_DT2 - CD.AGING_END_DT) * 24 * 60)) < 60                                                        ";
                        strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD            ";
                        strSql += cDefApp.CRLF + "     ON CM.WH_TYP = SD.WH_TYP      ";
                        strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO = SD.SC_NO   ";
                        strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD   = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD    = '0'    ";
                        strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.SUSPEND IN('0', '2')    ";
                        strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP = :WH_TYP2                                                                                          ";
                        strSql += cDefApp.CRLF + "    AND SUBSTR(CD.CELL_SC_NO, 0, 3) = :DEST_POS2                                                                      ";
                        strSql += cDefApp.CRLF + "  GROUP BY CM.AGING_TYP                                                                                               ";
                        strSql += cDefApp.CRLF + "  UNION ALL                                                                                                           ";
                        strSql += cDefApp.CRLF + " SELECT CM.AGING_TYP, ABS(MIN((:END_DT3 - CD.AGING_END_DT) * 24 * 60)) AS DURATION_TIME, COUNT(CM.BANK) AS COUNT   ";
                        strSql += cDefApp.CRLF + "  	 ,RANK() OVER (ORDER BY COUNT(CM.BANK)) AS PRIORITY                                                             ";
                        strSql += cDefApp.CRLF + "   FROM CELL_MST CM                                                                                                   ";
                        strSql += cDefApp.CRLF + "  INNER JOIN CELL_DTL CD                                                                                              ";
                        strSql += cDefApp.CRLF + "          ON CD.WH_TYP = CM.WH_TYP                                                                                    ";
                        strSql += cDefApp.CRLF + "         AND CD.CELL_NO = CM.CELL_NO                                                                                  ";
                        strSql += cDefApp.CRLF + "         AND SUBSTR(CD.CELL_SC_NO, 0, 3) = SUBSTR(CM.AGING_TYP, 0, 3)                                                 ";
                        strSql += cDefApp.CRLF + "        AND ABS(((:END_DT4 - CD.AGING_END_DT) * 24 * 60)) > 60                                                        ";
                        strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD            ";
                        strSql += cDefApp.CRLF + "     ON CM.WH_TYP = SD.WH_TYP      ";
                        strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO = SD.SC_NO   ";
                        strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD   = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD    = '0'    ";
                        strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.SUSPEND IN('0', '2')    ";
                        strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP = :WH_TYP3                                                                                          ";
                        strSql += cDefApp.CRLF + "    AND SUBSTR(CD.CELL_SC_NO, 0, 3) = :DEST_POS3                                                                      ";
                        strSql += cDefApp.CRLF + "  GROUP BY CM.AGING_TYP                                                                                               ";
                    }
                    else if (strDURATION_TIME == "000000000")
                    {
                        strSql += cDefApp.CRLF + " SELECT CM.AGING_TYP, 0 AS DURATION_TIME, COUNT(CM.BANK) AS COUNT   ";
                        strSql += cDefApp.CRLF + "  	 ,RANK() OVER (ORDER BY COUNT(CM.BANK)) AS PRIORITY                 ";
                        strSql += cDefApp.CRLF + "   FROM CELL_MST CM                                                       ";
                        strSql += cDefApp.CRLF + "  INNER JOIN CELL_DTL CD                                                  ";
                        strSql += cDefApp.CRLF + "          ON CD.WH_TYP = CM.WH_TYP                                        ";
                        strSql += cDefApp.CRLF + "         AND CD.CELL_NO = CM.CELL_NO                                      ";
                        strSql += cDefApp.CRLF + "         AND SUBSTR(CD.CELL_SC_NO, 0, 3) = SUBSTR(CM.AGING_TYP, 0, 3)     ";
                        strSql += cDefApp.CRLF + "  INNER JOIN SC_DATA SD            ";
                        strSql += cDefApp.CRLF + "     ON CM.WH_TYP = SD.WH_TYP      ";
                        strSql += cDefApp.CRLF + "    AND CM.CELL_SC_NO = SD.SC_NO   ";
                        strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD   = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD    = '0'    ";
                        strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD = '1'    ";
                        strSql += cDefApp.CRLF + "    AND SD.SUSPEND IN('0', '2')    ";
                        strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP = :WH_TYP4                                              ";
                        strSql += cDefApp.CRLF + "    AND SUBSTR(CD.CELL_SC_NO, 0, 3) = :DEST_POS4                          ";
                        strSql += cDefApp.CRLF + "  GROUP BY CM.AGING_TYP                                                   ";
                    }

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP1", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("DEST_POS1", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);

                    if (strDURATION_TIME != "000000000")
                    {
                        _pBdb.mComMain.Parameters.Add("END_DT1", DbLang.DATE).Value = dtEND_TIME;
                        _pBdb.mComMain.Parameters.Add("END_DT2", DbLang.DATE).Value = dtEND_TIME;
                        _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("DEST_POS2", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                        _pBdb.mComMain.Parameters.Add("END_DT3", DbLang.DATE).Value = dtEND_TIME;
                        _pBdb.mComMain.Parameters.Add("END_DT4", DbLang.DATE).Value = dtEND_TIME;
                        _pBdb.mComMain.Parameters.Add("WH_TYP3", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("DEST_POS3", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                    }
                    else if (strDURATION_TIME == "000000000")
                    {
                        _pBdb.mComMain.Parameters.Add("WH_TYP4", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("DEST_POS4", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                    }


                    nAGING_CNT = _pBdb.ExcuteQry(dtAGING, strSql);
                    if (nAGING_CNT < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nAGING_CNT == 0)
                    {
                        pRTN_MSG += "AGING 검색 실패";
                        return false;
                    }


                    for (int i = 0; i < nAGING_CNT; i++)
                    {

                        strAGING_TYP = "" + dtAGING.Rows[i]["AGING_TYP"].ToString();

                        strSql = "";
                        strSql += cDefApp.CRLF + "  SELECT CD.TRACK_NO                      ";
                        strSql += cDefApp.CRLF + "    FROM CV_DATA CD                       ";
                        strSql += cDefApp.CRLF + "   WHERE CD.DEST_POS_RD    = :DEST_POS_RD ";
                        strSql += cDefApp.CRLF + "     AND CD.WH_TYP         = :WH_TYP      ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("DEST_POS_RD", DbLang.VARCHAR).Value = strAGING_TYP;
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            return false;
                        }

                        // 박찬규가 바꾸랬음.
                        if (nSelCnt < 5)
                        {
                            //MSC가 3대이상 정지되어 있으면 다음 라인의 크레인을 검색한다
                            //ex : 29111, 29121, 29131 모두 정지이면 29112 검색 
                            strSql = "";
                            strSql += cDefApp.CRLF + "  SELECT SD.*                   ";
                            strSql += cDefApp.CRLF + "    FROM SC_DATA SD             ";
                            strSql += cDefApp.CRLF + "   WHERE SD.SC_NO like :SC_NO   ";
                            strSql += cDefApp.CRLF + "     AND SD.SUSPEND <> '0'      ";
                            strSql += cDefApp.CRLF + "     AND SD.WH_TYP  = :WH_TYP   ";
                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strAGING_TYP.Substring(0, 3) + "_" + strAGING_TYP.Substring(4, 1);
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            nSelCnt = _pBdb.ExcuteQry(dtSC_DATA, strSql);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                return false;
                            }

                            if (nSelCnt > 2)
                            {
                                continue;
                            }

                            dtAGING.Dispose();
                            dtSYS_DATE.Dispose();
                            dtCV_DATA.Dispose();
                            dtSC_DATA.Dispose();

                            return true;
                        }

                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }

        // BCR TOP, BOTTOM MAPPING 및 BCR READ상태변경.
        public bool UPDATE_BCR_DATA(string strWH_TYP,
                                    string strPLC_NO,
                                    string strSTART_POS,
                                ref string pRTN_MSG,
                                    string strBCR_BOTTOM = "",
                                    string strBCR_TOP = "")
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                pRTN_MSG += "[SET_CV_DATA]::";

                // BARCODE값 MAPPING.
                strSql = "";
                strSql += CRLF + "  UPDATE CV_DATA                      ";
                strSql += CRLF + "     SET BCR_BOTTOM   = :BCR_BOTTOM   ";
                strSql += CRLF + "       , BCR_TOP      = :BCR_TOP      ";
                strSql += CRLF + "       , COMP_MG      = 'N'           ";
                strSql += CRLF + "       , COMP_DP      = 'N'           ";
                strSql += CRLF + "       , OD_USER_ID   = 'IOTASK'      ";
                strSql += CRLF + "       , OD_UPD_DT    = " + DbLang.SYSDATE + "         ";
                strSql += CRLF + "   WHERE WH_TYP       = :WH_TYP       ";
                strSql += CRLF + "     AND PLC_NO       = :PLC_NO       ";
                strSql += CRLF + "     AND TRACK_NO     = :TRACK_NO     ";
                strSql += CRLF + "     AND OD_RQ_YN     = 'N'           ";
                strSql += CRLF + "     AND ERROR_CODE   = '0'           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
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
                    pRTN_MSG += "변경할 CV_DATA 정보가 존재하지 않습니다. TRACK NO : " + strSTART_POS;
                    return false;
                }

                // BCR STA (READING -> READ) 상태변경.
                //strSql = "";
                //strSql += CRLF + " UPDATE BCR_MST                   ";
                //strSql += CRLF + "    SET BCR_STA   = '1'           ";
                //strSql += CRLF + "  WHERE WH_TYP    = :WH_TYP       ";
                //strSql += CRLF + "    AND BCR_MC_NO = :BCR_MC_NO    ";
                //_pBdb.mComMain.CommandType = CommandType.Text;
                //_pBdb.mComMain.Parameters.Clear();
                //_pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                //_pBdb.mComMain.Parameters.Add("BCR_MC_NO", DbLang.VARCHAR).Value = strSTART_POS;
                //SelCnt = _pBdb.ExcuteNonQry(strSql);
                //if (SelCnt < 0)
                //{
                //    pRTN_MSG += _pBdb.ErrMsg;
                //    return false;
                //}

                //if (SelCnt == 0)
                //{
                //    return true;
                //}

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }

        // C/V에 목적지정보를 적는다.
        public bool UPDATE_CV_DATA(string strJOB_TYP,
                                   string strTRAY_TYP,
                                   string strTRAY_LEV,
                                   string strDEST_POS,
                                   string strIS_TURN,
                                   string strLUGG_NO,
                                   string strWH_TYP,
                                   string strPLC_NO,
                                   string strSTART_POS,
                                   string strJOB_LOT_NO,
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
                strSql += CRLF + " UPDATE CV_DATA                                 ";
                strSql += CRLF + "    SET JOB_TYP_OD         = :JOB_TYP_OD     ";
                strSql += CRLF + "      , PULP_SENSOR_OD     = :PULP_SENSOR_OD ";
                strSql += CRLF + "      , TRAY_LEV_OD        = :TRAY_LEV_OD    ";
                strSql += CRLF + "      , DEST_POS_OD        = :DEST_POS_OD    ";
                strSql += CRLF + "      , IS_TURN_OD         = :IS_TURN_OD     ";
                strSql += CRLF + "      , LUGG_NO_OD         = :LUGG_NO_OD     ";
                strSql += CRLF + "      , WAIT_SC_RET_JOB_OD = '0'             "; 
                strSql += CRLF + "      , COMP_DP            = 'N'             ";
                strSql += CRLF + "      , OD_RQ_YN           = 'Y'             ";
                strSql += CRLF + "      , OD_USER_ID         = 'IOTASK'        ";
                strSql += CRLF + "      , OD_UPD_DT          =  " + DbLang.SYSDATE + "          ";
                strSql += CRLF + "      , JOB_LOT_NO         = :JOB_LOT_NO     ";
                strSql += CRLF + "  WHERE WH_TYP             = :WH_TYP         ";
                strSql += CRLF + "    AND PLC_NO             = :PLC_NO         ";
                strSql += CRLF + "    AND MC_NO              = :TRACK_NO       ";
                strSql += CRLF + "    AND OD_RQ_YN           = 'N'             ";
                strSql += CRLF + "    AND (ERROR_CODE = '0' OR  ERROR_CODE = '0000')";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                _pBdb.mComMain.Parameters.Add("PULP_SENSOR_OD", DbLang.VARCHAR).Value = strTRAY_TYP;
                _pBdb.mComMain.Parameters.Add("TRAY_LEV_OD", DbLang.VARCHAR).Value = strTRAY_LEV;
                _pBdb.mComMain.Parameters.Add("DEST_POS_OD", DbLang.VARCHAR).Value = strDEST_POS;
                _pBdb.mComMain.Parameters.Add("IS_TURN_OD", DbLang.VARCHAR).Value = strIS_TURN;
                _pBdb.mComMain.Parameters.Add("LUGG_NO_OD", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("JOB_LOT_NO", DbLang.VARCHAR).Value = strJOB_LOT_NO;
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
                                    string strAGING_TYP = "0",
                                    string strSTART_POS = "0")
        {
            try
            {

                int nSelCnt = 0;
                string strSql = "";

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
                if (strAGING_TYP != "0")
                    strSql += CRLF + "    ,SC_NO            =  '" + strAGING_TYP + "'   ";
                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString() && strJOB_STATUS == "11")
                    strSql += CRLF + "    ,JOB_TYP          =  '6'      ";
                strSql += CRLF + "        ,UPD_DT           = " + DbLang.SYSDATE + "   ";
                strSql += CRLF + "        ,UPD_USER_ID      = 'IOTASK'  ";
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

        #region 사용하지 않는 함수들 선언 - 주석처리함!
        // 도착보고 후 기존DATA를 삭제 후 MES에서 새작업을 생성.
        // DELETE_JOB_DATA
        /*
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
        //*/


        // 도착보고 후 기존DATA를 삭제 후 MES에서 새작업을 생성.
        // DELETE_PREV_JOB
        /*
        public bool DELETE_PREV_JOB(string strJOB_STATUS,
                                    string strSTART_POS,
                                    string strWH_TYP,
                                ref string strRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg += "DELETE_PREV_JOB::";

                strSql = "";
                strSql += CRLF + " DELETE JOB_MST                 ";
                strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP    ";
                strSql += CRLF + "    AND START_POS  = :START_POS   ";
                strSql += CRLF + "    AND JOB_STATUS = '10'       ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strSTART_POS;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "";
                    return true;
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

        // UPDATE_CELL_MST_RESERVATION
        /*
        public bool UPDATE_CELL_MST_RESERVATION(string strJOB_STATUS,
                                 string strLUGG_NO,
                                 string strWH_TYP)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "UPDATE_CELL_MST_RESERVATION::";

                strSql = "";
                strSql += CRLF + " UPDATE  JOB_MST                                       ";
                strSql += CRLF + "    SET  JOB_STATUS       =  ?                         ";
                strSql += CRLF + "        ,UPD_DT  = " + DbLang.SYSDATE + "                             ";
                strSql += CRLF + "  WHERE  LUGG_NO          =  ?                         ";
                strSql += CRLF + "    AND  WH_TYP           =  ?                         ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strJOB_STATUS;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    //_pBdb.Commit();
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    //_pBdb.Rollback();
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                //_pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }
        //*/

        // LOCATION 가져오기.
        // 조건 1 - OUT이 채워져있으면서 현재 입고될 TRAY와 2분~10분 사이에 출고해야 될 경우 IN을 검색 
        // 조건 2 - 조건 1 없을 경우 OUT 채우기
        // 조건 3 - 조건 1, 2 없을 경우 IN 채우기
        // FETCH_LOCATION_BY_AGING_TYP
        /*
        public bool FETCH_LOCATION_BY_AGING_TYP(string strWH_TYP,
                                                string strDURATION_TIME,
                                                string strSC_NO_LEV,
                                            ref string strDEST_LOCATION,
                                            ref string pRTN_MSG)
        {
            int nSelCnt = 0;
            string strSql = "";

            IsRtnMsg = "FETCH_LOCATION_BY_AGING_TYP::";

            try
            {
                DataTable dtSYS_DATE = new DataTable();
                DataTable dtCELL_MST = new DataTable();

                IsRtnMsg = "UPDATE_CELL_MST_RESERVATION::";
                // 작업예정시간(DURATION_TIME [MES수신])
                strSql = "";
                strSql += cDefApp.CRLF + "SELECT " + DbLang.SYSDATE + " + (1/24/60/60)*((TO_NUMBER(SUBSTR(? ,1 ,3)) * 60 * 60 * 24)  ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(? ,4 ,2)) * 60 * 60)       ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(? ,6 ,2)) * 60)            ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(? ,8 ,2)))) AS END_TIME    ";
                strSql += cDefApp.CRLF + " from DUAL ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDURATION_TIME;
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
                        strSql += cDefApp.CRLF + "   AND SD.SUSPEND     IN('0', '2') ";
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
                    strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD      = '0'         ";
                    strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD     = '1'         "; // 자동모드
                    strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD   = '1'         "; // ACTIVE MODE
                    strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD   = '1'         "; // ONLINE MODE
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
                    strSql += cDefApp.CRLF + "    AND SD.ERR_CODE_RD        = '0'        ";
                    strSql += cDefApp.CRLF + "    AND SD.AUTO_MODE_RD       = '1'        "; // 자동모드
                    strSql += cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD     = '1'        "; // ACTIVE MODE
                    strSql += cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD     = '1'        "; // ONLINE MODE
                    strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP      = :WH_TYP_B    ";
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
                    strSql += cDefApp.CRLF + "   AND SD.ERR_CODE_RD         = '0'        ";
                    strSql += cDefApp.CRLF + "   AND SD.AUTO_MODE_RD        = '1'        "; // 자동모드
                    strSql += cDefApp.CRLF + "   AND SD.ACTIVE_MODE_RD      = '1'        "; // ACTIVE MODE
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

        // FETCH_LOCATION_BY_TEST
        /*
        public bool FETCH_LOCATION_BY_TEST()
        {
            try
            {
                DateTime dtT = new DateTime();
                DateTime dtT2 = new DateTime(2019, 04, 11, 16, 31, 10);

                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg = "FETCH_LOCATION_BY_TEST::";

                strSql = "";
                strSql += cDefApp.CRLF + "SELECT CD.* ";
                strSql += cDefApp.CRLF + "  FROM CELL_DTL CD ";
                strSql += cDefApp.CRLF + " WHERE ABS((CD.AGING_END_DT - ?)* 24 * 60) < 3  ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", OleDbType.DBTimeStamp).Value = dtT2;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    //_pBdb.Commit();
                    return true;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    //_pBdb.Rollback();
                    return false;
                } Convert.ToDateTime(_pBdb.mDtMain.Rows[0]["END_TIME"].ToString());

                dtT = Convert.ToDateTime(_pBdb.mDtMain.Rows[0]["AGING_END_DT"].ToString());

                return true;

            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                //_pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }
        //*/

        // CELL정보가져오기.
        // CHECK_LOCATION_BY_CELLNO
        /*
        public bool CHECK_LOCATION_BY_CELLNO(string strSC_NO,
                                             string strCELL_NO,
                                             string strWH_TYP,
                                             ref string strCELL_STA,
                                             ref string strBOTTOM_TRAY,
                                             ref string strTOP_TRAY,
                                             ref int nCOUNT)
        {
            try
            {
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
                    IsRtnMsg = "";
                    return false;
                }

                strCELL_STA = dtCELL_MST.Rows[0]["CELL_STA"].ToString();
                strBOTTOM_TRAY = dtCELL_MST.Rows[0]["BOTTOM_TRAY"].ToString();
                strTOP_TRAY = dtCELL_MST.Rows[0]["TOP_TRAY"].ToString();

                dtCELL_MST.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                return false;
            }
        }
        //*/ 

        // 해당 LOCATION이 시작작업에 해당되는지 체크한다.
        // CHECK_JOB_BY_CELLNO
        /*
        public bool CHECK_JOB_BY_CELLNO(string strCELL_NO,
                                        string strWH_TYP,
                                    ref int nCOUNT)
        {
            try
            {
                DataTable dtJOB_MST = new DataTable();

                IsRtnMsg = "CHECK_JOB_BY_CELLNO::";

                strSql = "";
                strSql += CRLF + " SELECT *       ";
                strSql += CRLF + "   FROM JOB_MST ";
                strSql += CRLF + "  WHERE WH_TYP         = :WH_TYP            ";
                strSql += CRLF + "    AND START_LOCATION = :START_LOCATION    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
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
        //*/


        // FETCH_LOCATION_BY_TRAYID
        /*
        public bool FETCH_LOCATION_BY_TRAYID(string strBOTTOM_TRAY,
                                            string strTOP_TRAY,
                                            string strCELL_TYP,
                                            string strAGING_TYP,
                                            string strWH_TYP)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "FETCH_LOCATION_BY_TRAYID::";

                strSql = "";
                strSql += CRLF + "  SELECT * ";
                strSql += CRLF + "    FROM CELL_MST ";
                strSql += CRLF + "   WHERE AGING_TYP like '%" + strAGING_TYP + "'   ";
                strSql += CRLF + "     AND BOTTOM_TRAY      = ?                     ";
                strSql += CRLF + "     AND TOP_TRAY         = ?                     ";
                strSql += CRLF + "     AND WH_TYP           = ?                     ";
                strSql += CRLF + "     AND CELL_TYP         = '1'                   ";
                strSql += CRLF + "     AND CELL_USE_DEF     = '0'                   ";
                strSql += CRLF + "     AND CELL_STA         = '2'                   ";
                strSql += CRLF + "     AND CELL_FIRE_YN     = N                     ";
                strSql += CRLF + "     AND FIRE_OD_RQ_YN    = N                     ";
                strSql += CRLF + "     AND CELL_USE_YN      = Y                     ";
                strSql += CRLF + "     AND ERR_CODE_RD      = '0'                   ";
                strSql += CRLF + "GROUP BY AGING_TYP ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strAGING_TYP;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strBOTTOM_TRAY;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strTOP_TRAY;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    //_pBdb.Commit();
                    return true;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    //_pBdb.Rollback();
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                //_pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }
        //*/

        // DEST POS 크레인일 경우 크레인 번호(LINE 번호 구해오기) - END TIME이 가장 먼 곳을 검색
        // GET_AGING_BY_DURATION_TIME
        /*
        public bool GET_AGING_BY_DURATION_TIME(string strDEST_POS,
                                               string strWH_TYP,
                                           ref string strAGING_TYP,
                                           ref string pRTN_MSG)
        {
            DataTable dtCELL_MST = new DataTable();

            int nSelCnt = 0;
            string strSql = "";

            try
            {
                pRTN_MSG += "GET_AGING_BY_DURATION_TIME::";

                strSql = "";
                strSql += CRLF + "  SELECT AGING_TYP                    ";
                strSql += CRLF + "       , " + DbLang.NVL + "(COUNT(*),0) as total     ";
                strSql += CRLF + "       , " + DbLang.NVL + "(SUM(CASE WHEN CELL_STA = '0' AND CELL_USE_DEF = '0' THEN 1 END), 0) AS EMPTY ";
                strSql += CRLF + "       , " + DbLang.NVL + "(SUM(CASE WHEN CELL_STA = '0' AND CELL_USE_DEF = '0' THEN 1 END), 0) / COUNT(*) * 100  AS EMPTY_USAGE ";
                strSql += CRLF + "    FROM CELL_MST CM                  ";
                strSql += CRLF + "   INNER JOIN SC_DATA SD              ";
                strSql += CRLF + "      ON CM.WH_TYP         = SD.WH_TYP ";
                strSql += CRLF + "     AND CM.CELL_SC_NO     = SD.SC_NO  ";
                strSql += CRLF + "   WHERE CM.WH_TYP         = ? ";
                strSql += CRLF + "     AND CM.AGING_TYP like '%' " + DbLang.II + " ? " + DbLang.II + " '%' ";
                strSql += CRLF + "     AND SD.ONLINE_MODE_RD = '1' ";
                strSql += CRLF + "     AND SD.AUTO_MODE_RD   = '1' ";
                strSql += CRLF + "     AND SD.ERR_CODE_RD    = '0' ";
                strSql += CRLF + "     AND SD.ACTIVE_MODE_RD = '1' ";
                strSql += CRLF + "     AND SD.SUSPEND       IN('0', '2')  ";
                strSql += CRLF + "   GROUP BY CM.AGING_TYP       ";
                strSql += CRLF + "   ORDER BY EMPTY_USAGE desc   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
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

        // DEST POS 크레인일 경우 크레인 번호 
        // GET_EMPTY_CELL
        /*
        public bool GET_EMPTY_CELL(string strJOB_TYP,
                               string strTRAY_TYP,
                               string strTRAY_LEV,
                               string strDEST_POS,
                               string strIS_TURN,
                               string strLUGG_NO,
                               string strWH_TYP,
                               string strPLC_NO,
                               string strSTART_POS,
                           ref string pRTN_MSG)
        {
            DataTable dtLugg_Mst = new DataTable();
            DataTable dtSC_DATA = new DataTable();

            int nCount = 0;
            string strAGING_TYP = "29" + strDEST_POS.Substring(2, 1) + "__";
            string strCOMMON_DEST = "";
            string strAGING_TYPE_TEMP = "";

            int nSelCnt = 0;
            string strSql = "";

            try
            {
                pRTN_MSG = "GET_EMPTY_CELL::";

                strSql = "";
                strSql += CRLF + "  SELECT AGING_TYP    ";
                strSql += CRLF + "       , " + DbLang.NVL + "(COUNT(*),0) as total     ";
                strSql += CRLF + "       , " + DbLang.NVL + "(SUM(CASE WHEN CELL_STA = '0' AND CELL_USE_DEF = '0' THEN 1 END), 0) AS EMPTY ";
                strSql += CRLF + "       , " + DbLang.NVL + "(SUM(CASE WHEN CELL_STA = '0' AND CELL_USE_DEF = '0' THEN 1 END), 0) / COUNT(*) * 100  AS EMPTY_USAGE ";
                strSql += CRLF + "       , AGING_TYP    ";
                strSql += CRLF + "    FROM CELL_MST     ";
                strSql += CRLF + "   WHERE AGING_TYP like '%" + strAGING_TYP + "'";
                strSql += CRLF + "     AND WH_TYP   = '" + strWH_TYP + "'";
                strSql += CRLF + "   GROUP BY AGING_TYP        ";
                strSql += CRLF + "   ORDER BY EMPTY_USAGE DESC ";
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

                for (int i = 0; i < nSelCnt; i++)
                {
                    strAGING_TYPE_TEMP = "" + _pBdb.mDtMain.Rows[i]["AGING_TYP"].ToString();
                    strCOMMON_DEST = strAGING_TYPE_TEMP.Substring(0, 3) + "0" + strAGING_TYPE_TEMP.Substring(4, 1);

                    // 셀 적재량이 가장 적은쪽으로 들어가고 있는 CV데이터 확인
                    strSql = "";
                    strSql += CRLF + "  SELECT *        ";
                    strSql += CRLF + "    FROM CV_DATA  ";
                    strSql += CRLF + "   WHERE WH_TYP	= :WH_TYP  ";
                    strSql += CRLF + "     AND PLC_NO	= :PLC_NO  ";
                    strSql += CRLF + "     AND DEST_POS_RD like '%" + strCOMMON_DEST + "'";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                    nCount = _pBdb.ExcuteQry(dtLugg_Mst, strSql);
                    if (nCount < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nCount == 0)
                    {
                        // 셀 적재량이 가장 적은쪽으로 들어가고 있는 화물이 없으면 해당 목적지로 진입
                        if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strCOMMON_DEST, strIS_TURN, strLUGG_NO, strWH_TYP, strPLC_NO, strSTART_POS, ref pRTN_MSG) == true)
                        {
                            if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == true)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // 셀 적재량이 가장 적은쪽에 화물이 들어가고 있으면 화물 순환
                        strSql = "";
                        strSql += CRLF + "  SELECT *        ";
                        strSql += CRLF + "    FROM SC_DATA  ";
                        strSql += CRLF + "   WHERE WH_TYP = :WH_TYP ";
                        strSql += CRLF + "     AND SC_NO like '%" + strAGING_TYP + "'";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        nCount = _pBdb.ExcuteQry(dtSC_DATA, strSql);
                        if (nCount < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            return false;
                        }
                        else if (nCount == 0)
                        {
                            pRTN_MSG += "변경할 SC_DATA 정보가 존재하지 않습니다";
                            return false;
                        }

                        m_nCurSCNo = (m_nCurSCNo == nCount) ? 0 : m_nCurSCNo;
                        for (; m_nCurSCNo < nCount; m_nCurSCNo++)
                        {
                            if (dtSC_DATA.Rows[m_nCurSCNo]["ONLINE_MODE_RD"].ToString() == "1" &&
                                dtSC_DATA.Rows[m_nCurSCNo]["AUTO_MODE_RD"].ToString() == "1" &&
                                dtSC_DATA.Rows[m_nCurSCNo]["ACTIVE_MODE_RD"].ToString() == "1")
                            {

                                int nSnt = 0;
                                strSql = "";
                                strSql += CRLF + "  SELECT *        ";
                                strSql += CRLF + "    FROM CV_DATA  ";
                                strSql += CRLF + "   WHERE WH_TYP	= ?          ";
                                strSql += CRLF + "     AND PLC_NO	= ?          ";
                                strSql += CRLF + "     AND DEST_POS_RD like '%" + strAGING_TYP + "'";
                                _pBdb.mComMain.CommandType = CommandType.Text;
                                _pBdb.mComMain.Parameters.Clear();
                                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strPLC_NO;
                                nSnt = _pBdb.ExcuteQry(dtLugg_Mst, strSql);
                                if (nSnt < 0)
                                {
                                    pRTN_MSG += _pBdb.ErrMsg;
                                    return false;
                                }
                                else if (nSnt < 2)
                                {
                                    strCOMMON_DEST = dtSC_DATA.Rows[m_nCurSCNo]["SC_NO"].ToString().Substring(0, 3) + "0" + dtSC_DATA.Rows[m_nCurSCNo]["SC_NO"].ToString().Substring(4, 1);

                                    if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strCOMMON_DEST, strIS_TURN, strLUGG_NO, strWH_TYP, strPLC_NO, strSTART_POS, ref pRTN_MSG) == true)
                                    {
                                        if (UPDATE_JOB_DATA("12", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == true)
                                        {
                                            m_nCurSCNo++;
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }

                                }

                            }
                            continue;
                        }

                    }
                }



                return true;

            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                _pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }
        //*/

        // SettingLocationInfo
        /*
        public void SettingLocationInfo(ref DataRow x, ref JobItem Job)
        {
            Job._strIOGB = "" + x["IOGB"].ToString();
            Job._strJobType = "" + x["JOBTYPE"].ToString();
            Job._strCommandID = "" + x["COMMANDID"].ToString();
            Job._nPriority = Convert.ToInt16("0" + x["PRIORITY"].ToString());
            Job._strCarrierID = "" + x["CARRIERID"].ToString();
            Job._strSOURCE = "" + x["SOURCE"].ToString();
            Job._strDEST = "" + x["DEST"].ToString();
            Job._strFinalDEST = "" + x["FINALDEST"].ToString();
            Job._strTransferSRC = "" + x["TRANSFERSRC"].ToString();
            Job._strTransferDST = "" + x["TRANSFERDST"].ToString();
            Job._tCommandTIME = KjhToDataTime(x["COMMANDTIME"].ToString());
            Job._tInstallTIME = KjhToDataTime(x["INSTALLTIME"].ToString());
            Job._strCarrierLOC = "" + x["CARRIERLOC"].ToString();
            Job._nSrcUnitType = Convert.ToInt16("0" + x["SRCUNITTYPE"].ToString());
            Job._nDstUnitType = Convert.ToInt16("0" + x["DSTUNITTYPE"].ToString());
            Job._strProcessStep = "" + x["PROCESSSTEP"].ToString();
            Job._bNULL = KjhToBoolean(x["NULLYON"].ToString());
            Job._strFrZone = "" + x["FRZONE"].ToString();
            Job._strToZone = "" + x["TOZONE"].ToString();
            Job._strWmsJob = "" + x["WMSJOB"].ToString();
            Job._strBoxIndex = "" + x["BOXINDEX"].ToString();
            Job._strBoxSeqID = "" + x["BOXSEQID"].ToString();
            Job._strBoxNo = "" + x["BOXNO"].ToString();
            Job._strBoxWeight = "" + x["BOXWEIGHT"].ToString();
            Job._strEmptyBoxWeight = "" + x["EMPTYBOXWEIGHT"].ToString();
            Job._strLotNo = "" + x["LOTNO"].ToString();
            Job._strProdGbn = "" + x["PRODGBN"].ToString();
            Job._nProdGrade = Convert.ToInt16("0" + x["NPRODGRADE"].ToString());
            Job._strCCID = "" + x["CCID"].ToString();
            Job._strCCCnt = "" + x["CCCNT"].ToString();
            Job._strURodNo = "" + x["URODNO"].ToString();
            Job._strChuckRemoveGbn = "" + x["CHUCKREMOVEGBN"].ToString();
            Job._strProdGrade = "" + x["PRODGRADE"].ToString();
            Job._strWeight = "" + x["WEIGHT"].ToString();
            Job._strProductGroup = "" + x["PRODUCTGROUP"].ToString();
            Job._strJisiDate = "" + x["JISIDATE"].ToString();
            Job._strProcessZone = "" + x["PROCESSZONE"].ToString();
            Job._strManualFlag = "" + x["MANUALFLAG"].ToString();
            Job._strLabelInfo = "" + x["LABELINFO"].ToString();
            Job._strJobNoDB = "" + x["JOBNODB"].ToString();
            Job._strJobTypeDB = "" + x["JOBTYPEDB"].ToString();
            Job._strScNoDB = "" + x["SCNODB"].ToString();
            Job._strJobStat = "" + x["JOBSTAT"].ToString();
            Job._nLuggNum = Convert.ToInt16("0" + x["LUGGNUM"].ToString());
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
                    //strSql += cDefApp.CRLF + " SELECT " + DbLang.NEXTVAL(strSEQ_NAME) + " AS AGING_TYP FROM DUAL              ";
                    if (nJobType == 1)
                    {
                        //strSql += cDefApp.CRLF + " SELECT STO_SEQ.NEXTVAL AS LUGG_NO FROM DUAL     ";
                        strSql += cDefApp.CRLF + " SELECT " + DbLang.NEXTVAL("STO_SEQ") + " AS LUGG_NO FROM DUAL     ";
                    }
                    else if (nJobType == 2 || nJobType == 4 || nJobType == 5 || nJobType == 7)
                    {
                        //strSql += cDefApp.CRLF + " SELECT RET_SEQ.NEXTVAL AS LUGG_NO FROM DUAL     ";
                        strSql += cDefApp.CRLF + " SELECT " + DbLang.NEXTVAL("RET_SEQ") + " AS LUGG_NO FROM DUAL     ";
                    }
                    else if (nJobType == 9)
                    {
                        //strSql += cDefApp.CRLF + " SELECT MANUAL_SEQ.NEXTVAL AS LUGG_NO FROM DUAL  ";
                        strSql += cDefApp.CRLF + " SELECT " + DbLang.NEXTVAL("MANUAL_SEQ") + " AS LUGG_NO FROM DUAL     ";
                    }
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    nSelCnt = _pBdb.ExcuteQry(dtLUGG_NO, strSql);
                    if (nSelCnt <= 0) { break; }
                    else
                    {
                        strLuggNo = dtLUGG_NO.Rows[0]["LUGG_NO"].ToString();

                        dtLUGG_NO.Dispose();

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
                            if (nJobType == 1)
                                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = "01";
                            else if (nJobType == 2 || nJobType == 4 || nJobType == 5 || nJobType == 7)
                                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = "04";
                            else if (nJobType == 9)
                                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = "04";
                            _pBdb.mComMain.Parameters.Add("JOB_PRIORITY", DbLang.VARCHAR).Value = "0";

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

                            dtJOB_MST.Dispose();
                            bSuccess = true;
                        }
                        else
                            continue;
                    }
                }

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


        // RTV 출고 Unload 완료
        // CHECK_CV_RTV_RETHS
        /*
        public bool CHECK_CV_RTV_RETHS(string strWH_TYP,
                                       string strPLC_NO,
                                   ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_RTV_RETHS::";

                // RTV UNLOAD 완료된 작업정보
                strSql = "";
                strSql += CRLF + " SELECT JM.*, CD.*                        ";
                strSql += CRLF + "   FROM JOB_MST JM                        ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD                   ";
                strSql += CRLF + "     ON JM.WH_TYP         = CD.WH_TYP     ";
                strSql += CRLF + "    AND JM.HS_TRACK_NO    = CD.TRACK_NO   ";
                strSql += CRLF + "  WHERE CD.WH_TYP         = :WH_TYP       ";
                strSql += CRLF + "    AND CD.PLC_NO         = :PLC_NO       ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	= '39'          ";  // RTV TASK에서 ('39' = RTV 구동완료) 변경완료
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strSC_NO = "";
                string strSTART_LOCATION = "";

                _pBdb.BeginTrans();
                for (int i = 0; i < nSelCnt; i++)
                {
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strSC_NO = "" + _pBdb.mDtMain.Rows[i]["SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SC_NO"].ToString();
                    strSTART_LOCATION = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString();

                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtvRet)
                    {
                        // C/V에 목적지정보 써준다. (RTV UNLOAD -> 입고 H/S)
                        if (UPDATE_CV_DATA(strJOB_TYP
                                         , strTRAY_TYP
                                         , strTRAY_LEV
                                         , strDEST_POS
                                         , strIS_TURN
                                         , strLUGG_NO
                                         , strWH_TYP
                                         , strPLC_NO
                                         , strSTART_POS
                                         , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        // C/V 구동중으로 변경
                        if(DELETE_JOB_DATA("19", strLUGG_NO, strWH_TYP, ref pRTN_MSG) == false)
                        {   
                            _pBdb.Rollback();
                            return false;
                        }
                //        if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                //        {
                //            _pBdb.Rollback();
                //            return false;
                //        }

                        _pBdb.Commit();
                        return true;
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

        // CWB-해당되는 MAIN트랙 이름을 가져와서 검색함(추가 함수)
        // CHECK_LOOP_USAGE
        /*
        public bool CHECK_LOOP_USAGE(string strWH_TYP,
                                     string strLOOP_NM,
                                 ref string pRTN_MSG)
        {
            try
            {
                string strLOOP_USE_USAGE = "";
                string strLOOP_USER_USAGE = "";
                string strLOOP_USE_CNT = "";
                string strLOOP_CNT = "";
                string strLOOP_NM_VIEW = "";
                int nSelCnt = 0;
                string strSql = "";

                DataTable dtLOOP_USAGE = new DataTable();

                pRTN_MSG += "CHECK_LOOP_USAGE::";

                //strSql = "";
                //strSql += CRLF + " SELECT LCU.*                             ";
                //strSql += CRLF + "   FROM LOOP_CV_USAGE LCU                 ";
                //strSql += CRLF + "  WHERE LCU.WH_TYP    = :WH_TYP           ";
                //strSql += CRLF + "    AND LCU.LOOP_NM   = :LOOP_NM          ";
                //_pBdb.mComMain.CommandType = CommandType.Text;
                //_pBdb.mComMain.Parameters.Clear();
                //_pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                //_pBdb.mComMain.Parameters.Add("LOOP_NM", DbLang.VARCHAR).Value = strLOOP_NM;
                strSql = "";
                strSql += CRLF + " SELECT *                                 ";
                strSql += CRLF + "   FROM WCS_VIEW_LOOP_CV_USAGE            ";
                nSelCnt = _pBdb.ExcuteQry(dtLOOP_USAGE, strSql);

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

                for (int i = 0; i < nSelCnt; i++)
                {
                    strLOOP_NM_VIEW = dtLOOP_USAGE.Rows[i]["LOOP_NM"].ToString() == "" ? "0" : dtLOOP_USAGE.Rows[i]["LOOP_NM"].ToString();
                    if (strLOOP_NM == strLOOP_NM_VIEW)
                    {

                        strLOOP_USE_USAGE = dtLOOP_USAGE.Rows[i]["LOOP_USE_USAGE"].ToString() == "" ? "0" : dtLOOP_USAGE.Rows[i]["LOOP_USE_USAGE"].ToString();
                        strLOOP_USER_USAGE = "" + dtLOOP_USAGE.Rows[i]["LOOP_USER_USAGE"].ToString() == "" ? "0" : dtLOOP_USAGE.Rows[i]["LOOP_USER_USAGE"].ToString();
                        strLOOP_USE_CNT = "" + dtLOOP_USAGE.Rows[i]["LOOP_USE_CNT"].ToString() == "" ? "0" : dtLOOP_USAGE.Rows[i]["LOOP_USE_CNT"].ToString();
                        strLOOP_CNT = "" + dtLOOP_USAGE.Rows[i]["LOOP_CNT"].ToString() == "" ? "0" : dtLOOP_USAGE.Rows[i]["LOOP_CNT"].ToString();

                        // Main 물류 TRAY가 사용자 설정량보다 많으면 RETURN FALSE
                        if (((int)Convert.ToDouble(strLOOP_USE_USAGE)) >= Convert.ToInt32(strLOOP_USER_USAGE))
                        {
                            dtLOOP_USAGE.Dispose();
                            return false;
                        }

                    }
                }

                dtLOOP_USAGE.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }
        //*/

        // CWB-출고 HS에서 WAIT 트랙을 결정. - 목적지와 SC번호에 따라서 WAIT트랙이 달라짐
        // GET_WAIT_TRACK_NO
        /*
        public bool GET_WAIT_TRACK_NO(string strWH_TYP,
                                      string strSC_NO,
                                      string strDEST_POS,
                                  ref string pWAIT_TRACK,
                                  ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                DataTable dtWAIT_TRACK = new DataTable();

                pRTN_MSG += "GET_WAIT_TRACK_NO::";

                strSql = "";
                strSql += CRLF + " SELECT WT.*                              ";
                strSql += CRLF + "   FROM WAIT_TRACK WT                     ";
                strSql += CRLF + "  WHERE WT.WH_TYP     = :WH_TYP           ";
                if (strSC_NO != "")
                    strSql += CRLF + "    AND WT.SC_NO      = '" + strSC_NO.Substring(0, 3) + "0" + strSC_NO.Substring(4, 1) + "'";
                strSql += CRLF + "    AND WT.DEST_POS   = :DEST_POS         ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDEST_POS;
                nSelCnt = _pBdb.ExcuteQry(dtWAIT_TRACK, strSql);
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
                else
                {
                    pWAIT_TRACK = dtWAIT_TRACK.Rows[0]["WAIT_TRACK"].ToString() == "" ? "0" : dtWAIT_TRACK.Rows[0]["WAIT_TRACK"].ToString();

                    dtWAIT_TRACK.Dispose();
                    return true;
                }

                dtWAIT_TRACK.Dispose();

                pWAIT_TRACK = "0";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }
        //*/

        // 설비, 크레인 출고 후 합류지점 트랙에서 대기한 후 가동률을 확인 후에 배출한다
        // 메인물류 대기대(WAIT TRACK) -> 메인물류
        // CHECK_CV_WAIT_TRACK
        /*
        public bool CHECK_CV_WAIT_TRACK(string strWH_TYP,
                                        string strPLC_NO,
                                    ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_RTV_RETHS::";

                strSql = "";
                strSql += CRLF + " SELECT JM.*, CD.*                    ";
                strSql += CRLF + "   FROM JOB_MST JM                    ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD               ";
                strSql += CRLF + "     ON JM.WH_TYP    = CD.WH_TYP      ";
                strSql += CRLF + "    AND JM.LUGG_NO   = CD.LUGG_NO_RD  ";
                strSql += CRLF + "  WHERE CD.WH_TYP    = :WH_TYP        ";
                strSql += CRLF + "    AND CD.PLC_NO    = :PLC_NO        ";
                strSql += CRLF + "    AND CD.TRACK_NO  = CD.DEST_POS_RD ";
                strSql += CRLF + "    AND JM.DEST_POS  <> CD.TRACK_NO   ";
                strSql += CRLF + "    AND CD.OD_RQ_YN   =  'N'          ";
                strSql += CRLF + "    AND CD.OD_RQ_FLAG =  'N'          ";
                strSql += CRLF + "    AND CD.WAIT_HS_YN =  'Y'          ";  // 확인후 주석제거예정 2019.10.13
                strSql += CRLF + "    AND CD.SUSPEND    = '0'           ";
                strSql += CRLF + "    AND JM.JOB_STATUS =  '11'         ";
                strSql += CRLF + "  ORDER BY CD.READ_UPD_DT ASC         ";  //빨리 출고 및 도착한 트랙이 먼저 검색해서 나올수 있도록                                     
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";

                string strLOOP_NM = "";

                _pBdb.BeginTrans();
                for (int i = 0; i < nSelCnt; i++)
                {
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strLOOP_NM = "" + _pBdb.mDtMain.Rows[i]["LOOP_NM"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LOOP_NM"].ToString();

                    if (strSTART_POS == "01024")
                    { 
                        // DeadLock Check할것!!
                    }

                    // C/V에 목적지정보 써준다. (S/C출고 및 이동완료 후 목적지로 배출)  
                    if (UPDATE_CV_DATA(strJOB_TYP
                                        , strTRAY_TYP
                                        , strTRAY_LEV
                                        , strDEST_POS
                                        , strIS_TURN
                                        , strLUGG_NO
                                        , strWH_TYP
                                        , strPLC_NO
                                        , strSTART_POS
                                        , ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;

                    }
                    // 작업정보 상태변경처리(C/V구동중).
                    if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    _pBdb.Commit();
                    return true;
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


        // BUFFER 매거진에서 처리하는 작업
        // CHECK_BUFFER_MG
        /*
        public bool CHECK_BUFFER_MG(string strWH_TYP,
                                    string strPLC_NO,
                                ref string pRTN_MSG)
        {
            try
            {

                string strTRAY_TYP = "";
                string strTRAY_TYP_RD = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strLUGG_NO = "";
                string strTRACK_NO = "";
                string strBUFFER_NO = "";
                string strWAIT_TIME = "";

                int nBUFFER_CNT = 0;
                int nJOB_CNT = 0;

                pRTN_MSG = "CHECK_BUFFER_MG::";

                strSql = "";
                strSql += CRLF + " SELECT CD.*    ";
                strSql += CRLF + "   FROM CV_DATA CD    ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP       ";
                strSql += CRLF + "    AND CD.PLC_NO	            = :PLC_NO       ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '0'           ";
                strSql += CRLF + "    AND CD.STO_READY_RD 	    = '0'           ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '0'           ";
                strSql += CRLF + "    AND (CD.SENSOR3_DATA_RD = '1' OR CD.SENSOR4_DATA_RD = '1') ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'           ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'           ";
                strSql += CRLF + "    AND CD.READ_UPD_DT >= CD.WRITE_UPD_DT     ";
                //strSql += CRLF + "    AND CD.ERR_RQ_RD	        = '0'           ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += CRLF + "    AND CD.BUFFER_NO	        IN ('1', '2')   ";
                strSql += CRLF + "    AND CD.MG_HS_YN	        = 'Y'           ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD 	    = '00000'       ";
                strSql += CRLF + "    AND CD.WAIT_TIME_RD 	    <> '0'          ";
                strSql += CRLF + "    AND CD.CMD_RQ_YN 	        = 'N'           ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                nJOB_CNT = _pBdb.ExcuteQry(strSql);
                if (nJOB_CNT < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nJOB_CNT == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }


                _pBdb.BeginTrans();
                for (int i = 0; i < nJOB_CNT; i++)
                {
                    strDEST_POS = _pBdb.mDtMain.Rows[i]["DEST_POS_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS_RD"].ToString();

                    strTRAY_TYP_RD = "" + _pBdb.mDtMain.Rows[i]["TRAY_TYP_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_TYP_RD"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV_RD"].ToString();

                    strTRACK_NO = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();

                    strBUFFER_NO = "" + _pBdb.mDtMain.Rows[i]["BUFFER_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BUFFER_NO"].ToString();

                    strWAIT_TIME = "" + _pBdb.mDtMain.Rows[i]["WAIT_TIME_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WAIT_TIME_RD"].ToString();

                    //자기 자신으로 오는 작업이 3개있을때 3개의 TRAY TYPE을 검색한다
                    strSql = "";
                    strSql += CRLF + " SELECT JM.*          ";
                    strSql += CRLF + "   FROM JOB_MST JM    ";
                    strSql += CRLF + "  WHERE JM.WH_TYP		        = :WH_TYP           ";
                    strSql += CRLF + "    AND JM.DEST_POS           = :DEST_POS        ";
                    //strSql += CRLF + "    AND JM.TRAY_TYP	        = :TRAY_TYP         ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strTRACK_NO;
                    SelCnt = _pBdb.ExcuteQry(dtBUFFER, strSql);
                    if (SelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }

                    //오고있는 화물이 3개 이상일때 같은게 1개 라도 있으면 기다리고 없으면 내린다.
                    if (SelCnt > 2)
                    {
                        for (int j = 0; j < SelCnt; j++)
                        {
                            strTRAY_TYP = "" + dtBUFFER.Rows[j]["TRAY_TYP"].ToString() == "" ? "0" : dtBUFFER.Rows[j]["TRAY_TYP"].ToString();
                            if (strTRAY_TYP_RD == strTRAY_TYP)   //현재 자신이 들고있는 트레이와 해당 트레이가 맞지 않는걸 카운트
                                nBUFFER_CNT++;

                        }
                        if (nBUFFER_CNT == 0)
                        {
                            // 임시주석 2019.10.21
                            //UPDATE_CV_WAIT_TIME(strWH_TYP, strPLC_NO, strTRACK_NO, "0", ref pRTN_MSG);

                        }
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

        // CHECK_BUFFER_START
        /*
        public bool CHECK_BUFFER_START(string strWH_TYP,
                                    string strPLC_NO,
                                ref string pRTN_MSG)
        {
            string strJOB_TYP = "";
            string strTRAY_TYP = "";
            string strTRAY_LEV = "";
            string strDEST_POS = "";
            string strLUGG_NO = "";
            string strSTART_POS = "";
            string strBCR_TOP = "";
            string strBCR_BOTTOM = "";
            string strBUFFER_NO = "";
            string strIS_TURN = "";
            int nBUFFER_CNT = 0;
            int nJOB_CNT = 0;
            int nDEST_CNT = 0;

            pRTN_MSG = "CHECK_BUFFER_START::";
            try
            {

                DataTable pDt = new DataTable();
                DataTable dtBUFFER = new DataTable();


                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*    ";
                strSql += CRLF + "   FROM CV_DATA CD    ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM ";
                strSql += CRLF + "     ON JM.WH_TYP = CD.WH_TYP ";
                strSql += CRLF + "    AND JM.LUGG_NO  = CD.LUGG_NO_RD    ";
                strSql += CRLF + "    AND JM.DEST_POS <> CD.DEST_POS_RD  ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP       ";
                strSql += CRLF + "    AND CD.PLC_NO	            = :PLC_NO       ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '1'           ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'           ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'           ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'           ";
                strSql += CRLF + "    AND CD.READ_UPD_DT >= CD.WRITE_UPD_DT     ";
                //strSql += CRLF + "    AND CD.ERR_RQ_RD	        = '0'           ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += CRLF + "    AND CD.BUFFER_NO	       <> '0'           ";
                strSql += CRLF + "    AND CD.MG_HS_YN	        = 'N'           ";
                strSql += CRLF + "    AND CD.DEST_POS_RD        <> '00000'      ";
                strSql += CRLF + "    AND CD.DEST_POS_RD = CD.TRACK_NO          ";
                strSql += CRLF + "    AND JM.JOB_STATUS         = '11'          ";
                strSql += CRLF + "  ORDER BY CD.READ_UPD_DT ASC                 ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                nJOB_CNT = _pBdb.ExcuteQry(pDt, strSql);
                if (nJOB_CNT < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nJOB_CNT == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }


                _pBdb.BeginTrans();
                for (int i = 0; i < nJOB_CNT; i++)
                {
                    strDEST_POS = pDt.Rows[i]["DEST_POS"].ToString() == "" ? "0" : pDt.Rows[i]["DEST_POS"].ToString();
                    strJOB_TYP = pDt.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : pDt.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = pDt.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : pDt.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = pDt.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : pDt.Rows[i]["TRAY_LEV"].ToString();

                    strLUGG_NO = pDt.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : pDt.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = pDt.Rows[i]["WH_TYP"].ToString() == "" ? "0" : pDt.Rows[i]["WH_TYP"].ToString();

                    strSTART_POS = pDt.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : pDt.Rows[i]["TRACK_NO"].ToString();

                    strBCR_TOP = pDt.Rows[i]["BCR_TOP"].ToString() == "" ? "0" : pDt.Rows[i]["BCR_TOP"].ToString();
                    strBCR_BOTTOM = pDt.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "0" : pDt.Rows[i]["BCR_BOTTOM"].ToString();

                    strIS_TURN = pDt.Rows[i]["IS_TURN_RD"].ToString() == "" ? "0" : pDt.Rows[i]["IS_TURN_RD"].ToString();
                    strBUFFER_NO = pDt.Rows[i]["BUFFER_NO"].ToString() == "" ? "0" : pDt.Rows[i]["BUFFER_NO"].ToString();
                    //자기 자신으로 오는 작업이 3개있을때 3개의 TRAY TYPE을 검색한다

                    strSql = "";
                    strSql += CRLF + "SELECT *                                ";
                    strSql += CRLF + "  FROM CV_DATA                          ";
                    strSql += CRLF + " WHERE WH_TYP = :WH_TYP                 ";
                    strSql += CRLF + "   AND DEST_POS_RD <> '00000'           ";
                    strSql += CRLF + "   AND BUFFER_NO = :BUFFER_NO           ";
                    strSql += CRLF + "   AND TRACK_NO <> :TRACK_NO            ";
                    strSql += CRLF + " ORDER BY TRACK_NO                      ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("BUFFER_NO", DbLang.VARCHAR).Value = strBUFFER_NO;
                    _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strSTART_POS;
                    nBUFFER_CNT = _pBdb.ExcuteQry(dtBUFFER, strSql);

                    if (nBUFFER_CNT < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }
                    else if (nBUFFER_CNT == 0)
                    {
                        if (UPDATE_CV_DATA(strJOB_TYP
                                            , strTRAY_TYP
                                            , strTRAY_LEV
                                            , strDEST_POS
                                            , strIS_TURN
                                            , strLUGG_NO
                                            , strWH_TYP
                                            , strPLC_NO
                                            , strSTART_POS
                                            , ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        continue;
                    }
                    else if (nBUFFER_CNT > 0)
                    {
                        for (int j = 0; j < nBUFFER_CNT; j++)
                        {
                            DateTime dtREAD_DT = new DateTime();
                            dtREAD_DT = Convert.ToDateTime(dtBUFFER.Rows[j]["READ_UPD_DT"].ToString());
                            DateTime dtOD_DT = new DateTime();
                            dtOD_DT = Convert.ToDateTime(dtBUFFER.Rows[j]["OD_UPD_DT"].ToString());

                            if (dtREAD_DT < dtOD_DT)
                            {
                                nDEST_CNT++;
                                pRTN_MSG = "";
                                break;
                            }
                            if (dtBUFFER.Rows[j]["DEST_POS_RD"].ToString() == strDEST_POS ||
                                dtBUFFER.Rows[j]["OD_RQ_YN"].ToString() == "Y")
                            {
                                nDEST_CNT++;
                                pRTN_MSG = "";
                                break;
                            }

                            int nMG_DP = 0;
                            nMG_DP = (int)cDefApp.eTRACK_NM.MGDP;
                            if (dtBUFFER.Rows[nBUFFER_CNT - 1]["DEST_POS_RD"].ToString() != (nMG_DP.ToString().PadLeft(5, '0')))
                            {
                                if (dtBUFFER.Rows[nBUFFER_CNT - 1]["SENSOR1_DATA_RD"].ToString() == "1" ||
                                    dtBUFFER.Rows[nBUFFER_CNT - 1]["SENSOR2_DATA_RD"].ToString() == "1" ||
                                    dtBUFFER.Rows[nBUFFER_CNT - 1]["SENSOR3_DATA_RD"].ToString() == "1" ||
                                    dtBUFFER.Rows[nBUFFER_CNT - 1]["SENSOR4_DATA_RD"].ToString() == "1")
                                {
                                    pRTN_MSG = "";
                                    continue;
                                }

                                if (dtBUFFER.Rows[nBUFFER_CNT - 1]["TRAY_TYP_RD"].ToString() == strTRAY_TYP)
                                {
                                    if (UPDATE_CV_DATA(strJOB_TYP
                                                        , strTRAY_TYP
                                                        , strTRAY_LEV
                                                        , strDEST_POS
                                                        , strIS_TURN
                                                        , strLUGG_NO
                                                        , strWH_TYP
                                                        , strPLC_NO
                                                        , strSTART_POS
                                                        , ref pRTN_MSG) == false)
                                    {
                                        _pBdb.Rollback();
                                        return false;
                                    }

                                    _pBdb.Commit();
                                    return true;
                                }

                            }
                        }

                        int nMGDP = 0;
                        nMGDP = (int)cDefApp.eTRACK_NM.MGDP;

                        if (nDEST_CNT == 0 &&
                            dtBUFFER.Rows[nBUFFER_CNT - 1]["DEST_POS_RD"].ToString() != nMGDP.ToString().PadLeft(5, '0') &&
                            dtBUFFER.Rows[nBUFFER_CNT - 1]["DEST_POS_RD"].ToString() != strDEST_POS)
                        {
                            if (UPDATE_CV_DATA(strJOB_TYP
                                                , strTRAY_TYP
                                                , strTRAY_LEV
                                                , strDEST_POS
                                                , strIS_TURN
                                                , strLUGG_NO
                                                , strWH_TYP
                                                , strPLC_NO
                                                , strSTART_POS
                                                , ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                return false;
                            }

                            _pBdb.Commit();
                            return true;

                        }

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

        // 롤링 후 BCR에서 다이렉트로 직진하는 로직 (필요하면 사용할 예정)
        // 롤링 BUFFER 전 BCR에서 BUFFER OR 직진 결정하는 함수. 
        // CHECK_BUFFER_BCR_RET_READY
        /*
        public bool CHECK_BUFFER_BCR_RET_READY(string strWH_TYP,
                                               string strPLC_NO,
                                           ref string pRTN_MSG)
        {
            try
            {

                pRTN_MSG = "CHECK_BUFFER_BCR_RET_READY::";

                // 목적지에 도착한 상태정보
                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*                                ";
                strSql += CRLF + "   FROM CV_DATA CD                                ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                           ";
                strSql += CRLF + "     ON CD.WH_TYP	  = JM.WH_TYP                   ";
                strSql += CRLF + "    AND CD.TRACK_NO = JM.START_POS                ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP           ";
                strSql += CRLF + "    AND CD.PLC_NO	            = :PLC_NO           ";
                strSql += CRLF + "    AND CD.BUFFER_NO	        IN ('1', '2')       ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'               ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'               ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'               ";
                strSql += CRLF + "    AND CD.READ_UPD_DT        >= CD.WRITE_UPD_DT  ";
                //strSql += CRLF + "    AND CD.ERR_RQ_RD	        = '0'               ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '40'              ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null                   ";
                strSql += CRLF + "    AND (CD.RET_READY_RD 	= '1' OR CD.STO_READY_RD 	= '1'  ) "; // 출고대 또는 입고대 READY ON
                strSql += CRLF + "    AND (CD.TRACK_NO      = CD.DEST_POS_RD        ";
                strSql += CRLF + "     OR  CD.DEST_POS_RD   = '00000'               ";
                strSql += CRLF + "     OR  CD.DEST_POS_RD   = '09999')              ";
                strSql += CRLF + "  ORDER BY JM.INS_DT ASC                             ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                SelCnt = _pBdb.ExcuteQry(strSql);
                if (SelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (SelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";
                string strBUFFER_NO = "";
                int nBUFFER_CNT = 0;
                int nBUFFER_TRAY_CNT = 0;

                string[] strTO_CV = new string[10];
                string[] strBUFFER_CV = new string[10];
                string[] strCNT = new string[10];
                _pBdb.BeginTrans();

                for (int i = 0; i < SelCnt; i++)
                {

                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();

                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();

                    strBUFFER_NO = "" + _pBdb.mDtMain.Rows[i]["BUFFER_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["BUFFER_NO"].ToString();


                    //갈수 있는 버퍼라인을 검색  ex)09013 -> 09015, 09017,09018, 09999  가고 있는 개수 COUNT
                    strSql = "";
                    strSql += CRLF + "SELECT CDI.TO_CV, CDI.BUFFER_CV, COUNT(CASE WHEN CDI.TO_CV = CD.DEST_POS_RD THEN 1 END) AS CNT   ";
                    strSql += CRLF + "  FROM CV_DEF_INF CDI, (SELECT *                                                  ";
                    strSql += CRLF + "                          FROM CV_DATA                                            ";
                    strSql += CRLF + "                         WHERE WH_TYP = :WH_TYP1                                  ";
                    strSql += CRLF + "                           AND BUFFER_NO = :BUFFER_NO) CD                         ";
                    strSql += CRLF + " WHERE CDI.WH_TYP =   :WH_TYP2                                                    ";
                    strSql += CRLF + "  AND CDI.USE_YON =   'Y'                                                         ";
                    strSql += CRLF + "  AND CDI.FR_CV   =   :START_POS                                                  ";
                    strSql += CRLF + " GROUP BY CDI.TO_CV, CDI.BUFFER_CV                                                               ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP1", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("BUFFER_NO", DbLang.VARCHAR).Value = strBUFFER_NO;
                    _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strSTART_POS;
                    nBUFFER_CNT = _pBdb.ExcuteQry(dtBUFFER, strSql);
                    if (nBUFFER_CNT < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        _pBdb.Rollback();
                        return false;
                    }


                    for (int j = 0; j < nBUFFER_CNT; j++)
                    {
                        strCNT[j] = "" + dtBUFFER.Rows[j]["CNT"].ToString() == "" ? "0" : dtBUFFER.Rows[j]["CNT"].ToString();
                        strTO_CV[j] = "" + dtBUFFER.Rows[j]["TO_CV"].ToString() == "" ? "0" : dtBUFFER.Rows[j]["TO_CV"].ToString();
                        strBUFFER_CV[j] = "" + dtBUFFER.Rows[j]["BUFFER_CV"].ToString() == "" ? "0" : dtBUFFER.Rows[j]["BUFFER_CV"].ToString();
                        if (strCNT[j] != "0")
                            nBUFFER_TRAY_CNT = nBUFFER_TRAY_CNT + Convert.ToInt32(strCNT[j]);
                    }

                    for (int j = 0; j < nBUFFER_CNT; j++)
                    {
                        if (nBUFFER_TRAY_CNT == 0)
                        {
                            if (UPDATE_CV_DATA(strJOB_TYP
                                             , strTRAY_TYP
                                             , strTRAY_LEV
                                             , strDEST_POS
                                             , strIS_TURN
                                             , strLUGG_NO
                                             , strWH_TYP
                                             , strPLC_NO
                                             , strSTART_POS
                                             , ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                return false;
                            }

                            // 작업정보 구동중으로 변경.
                            if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                return false;
                            }
                            break;

                        }
                        else if (nBUFFER_TRAY_CNT == 1)
                        {


                        }
                        else
                        {
                            if (strBUFFER_CV[j] == "Y")
                            {
                                if (strCNT[j] == "0")
                                {
                                    if (UPDATE_CV_DATA(strJOB_TYP
                                                     , strTRAY_TYP
                                                     , strTRAY_LEV
                                                     , strTO_CV[j]
                                                     , strIS_TURN
                                                     , strLUGG_NO
                                                     , strWH_TYP
                                                     , strPLC_NO
                                                     , strSTART_POS
                                                     , ref pRTN_MSG) == false)
                                    {
                                        _pBdb.Rollback();
                                        return false;
                                    }

                                    // 작업정보 구동중으로 변경.
                                    if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                                    {
                                        _pBdb.Rollback();
                                        return false;
                                    }
                                    break;

                                }

                            }

                        }
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

        // CHECK_VR_REPORT
        /*
        public bool CHECK_VR_REPORT(string strWH_TYP,
                                    string strPLC_NO,
                                ref string pRTN_MSG)
        {
            try
            {


                int nJOB_CNT = 0;

                pRTN_MSG = "CHECK_VR_REPORT::";

                strSql = "";
                strSql += CRLF + " SELECT JM.*, CD.*    ";
                strSql += CRLF + "   FROM CV_DATA CD    ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM   ";
                strSql += CRLF + "     ON JM.WH_TYP             = CD.WH_TYP     ";
                strSql += CRLF + "    AND JM.LUGG_NO            = CD.LUGG_NO_RD ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP       ";
                strSql += CRLF + "    AND CD.PLC_NO	            = :PLC_NO       ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '1'           ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'           ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'           ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'           ";
                strSql += CRLF + "    AND CD.READ_UPD_DT >= CD.WRITE_UPD_DT     ";
                strSql += CRLF + "    AND CD.ERROR_CODE	        = '0'           ";
                strSql += CRLF + "    AND CD.VR_HS_YN	        = 'Y'           ";
                strSql += CRLF + "    AND CD.SUSPEND		    = '0'           ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '11'          ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD <> '0000'                  ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                nJOB_CNT = _pBdb.ExcuteQry(strSql);
                if (nJOB_CNT < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nJOB_CNT == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strDEST_LOCATION = "";
                string strDURATION_TIME = "";

                string strDEST_POS_RD = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";

                _pBdb.BeginTrans();
                for (int i = 0; i < nJOB_CNT; i++)
                {
                    strDEST_POS_RD = _pBdb.mDtMain.Rows[i]["DEST_POS_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS_RD"].ToString();

                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strDURATION_TIME = "" + _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString();
                    strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();

                    if (Convert.ToInt16(strJOB_TYP) != (int)cDefApp.eJOBTYP.Sto)
                    {
                        if (UPDATE_BCR_DATA(strWH_TYP, strPLC_NO, strSTART_POS, ref pRTN_MSG, strBCR_BOTTOM, strBCR_TOP) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        //@@@@@@
                        //if (UPDATE_JOB_DATA("13", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
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

        // D/P 도착보고 (S6F11 CEID106)   
        // CHECK_DP_REPORT
        /*
        public bool CHECK_DP_REPORT(string strWH_TYP,
                                    string strPLC_NO,
                                ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";

                int nJOB_CNT = 0;

                pRTN_MSG = "CHECK_DP_REPORT::";

                strSql = "";
                strSql += CRLF + " SELECT JM.*, CD.*    ";
                strSql += CRLF + "   FROM CV_DATA CD    ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM   ";
                strSql += CRLF + "     ON JM.WH_TYP             = CD.WH_TYP     ";
                strSql += CRLF + "    AND JM.LUGG_NO            = CD.LUGG_NO_RD ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP       ";
                strSql += CRLF + "    AND CD.PLC_NO	            = :PLC_NO       ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '1'           ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'           ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'           ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'           ";
                strSql += CRLF + "    AND CD.READ_UPD_DT >= CD.WRITE_UPD_DT     ";
                strSql += CRLF + "    AND CD.ERROR_CODE	        = '0'           ";
                strSql += CRLF + "    AND CD.DP_HS_YN	        = 'Y'           ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '11'          ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD <> '0000'                  ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                nJOB_CNT = _pBdb.ExcuteQry(strSql);
                if (nJOB_CNT < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nJOB_CNT == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strDEST_LOCATION = "";
                string strDURATION_TIME = "";

                string strDEST_POS_RD = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";

                _pBdb.BeginTrans();
                for (int i = 0; i < nJOB_CNT; i++)
                {
                    strDEST_POS_RD = _pBdb.mDtMain.Rows[i]["DEST_POS_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS_RD"].ToString();

                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strDURATION_TIME = "" + _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString();
                    strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();

                    if (Convert.ToInt16(strJOB_TYP) != (int)cDefApp.eJOBTYP.Sto)
                    {
                        if (UPDATE_BCR_DATA(strWH_TYP, strPLC_NO, strSTART_POS, ref pRTN_MSG, strBCR_BOTTOM, strBCR_TOP) == false)      
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        if (UPDATE_JOB_DATA("13", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
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
        //*/

        // Invoke Check(RetStation ready)
        // LIFTER 앞에서 입고할 CELL 다시 결정 후 HS->이동 작업
        // CHECK_CV_STO_RESERVATION
        /*
        public bool CHECK_CV_STO_RESERVATION(string strWH_TYP,
                                             string strPLC_NO,
                                         ref string pRTN_MSG)
        {
            try
            {
                int nJOB_CNT = 0;

                pRTN_MSG = "CHECK_CV_STO_RESERVATION::";

                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*        ";
                strSql += CRLF + "   FROM CV_DATA CD        ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM   ";
                strSql += CRLF + "     ON JM.WH_TYP     = CD.WH_TYP       ";
                strSql += CRLF + "    AND JM.LUGG_NO    = CD.LUGG_NO_RD   ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP     ";
                strSql += CRLF + "    AND CD.PLC_NO	            = :PLC_NO     ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '1'         ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'         ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'         ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'         ";
                strSql += CRLF + "    AND CD.OD_RQ_FLAG		    = 'N'         ";
                //strSql += CRLF + "    AND CD.ERR_RQ_RD	        = '0'         ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += CRLF + "    AND CD.LIFTER_HS_YN	    = 'Y'         ";
                strSql += CRLF + "    AND CD.SUSPEND		    = '0'         ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '11'        ";
                strSql += CRLF + "    AND JM.DEST_LOCATION 	    = '00-000-00' ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD <> '0000'                ";
                //strSql += CRLF + "    AND B.JOB_TYP 	    = '3'          ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null            ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                nJOB_CNT = _pBdb.ExcuteQry(strSql);
                if (nJOB_CNT < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nJOB_CNT == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strDEST_LOCATION = "";
                string strDURATION_TIME = "";
                string strAGING_TYP = "";
                string strDEST_POS_RD = "";

                string strBOTTOM_TRAY1 = "";
                string strTOP_TRAY1 = "";

                _pBdb.BeginTrans();
                for (int i = 0; i < nJOB_CNT; i++)
                {
                    strDEST_POS_RD = _pBdb.mDtMain.Rows[i]["DEST_POS_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS_RD"].ToString();

                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strDURATION_TIME = "" + _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString() == "" ? "000000000" : _pBdb.mDtMain.Rows[i]["DURATION_TIME"].ToString();

                    strBOTTOM_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strTOP_TRAY1 = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();

                    // 입고
                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Sto)
                    {
                        if (strDEST_LOCATION == "00-000-00")
                        {
                            // LOCATION이 없을때 구해오는처리.
                            if (SET_NEXT_LEV(strWH_TYP
                                           , strDEST_POS_RD
                                           , strPLC_NO
                                           , strDURATION_TIME
                                           , ref strAGING_TYP
                                           , ref strDEST_LOCATION, ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                return false;
                            }

                            // C/V에서 목적지로 써주는처리.
                            if (UPDATE_CV_DATA(strJOB_TYP
                                             , strTRAY_TYP
                                             , strTRAY_LEV
                                             , strAGING_TYP
                                             , strIS_TURN
                                             , strLUGG_NO
                                             , strWH_TYP
                                             , strPLC_NO
                                             , strSTART_POS
                                             , ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                return false;
                            }

                            // CELL예약.
                            if (UPDATE_CELL_MST_DTL("1", strBOTTOM_TRAY1, strTOP_TRAY1, strWH_TYP, strAGING_TYP, strDEST_LOCATION, ref pRTN_MSG) == false)
                            {
                                _pBdb.Rollback();
                                return false;
                            }

                            // C/V 구동중으로 변경.
                            if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG, strAGING_TYP, strDEST_LOCATION, "0", strAGING_TYP) == false)
                            {
                                _pBdb.Rollback();
                                return false;
                            }
                            Thread.Sleep(100);
                            _pBdb.Commit();
                            return true;
                        }
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


        //RTV RET_HS READY(RetStation ready)
        //RTV RET_HS READY 후 이동작업이면 다음 목적지로 이동명령 Write
        /*
        public bool CHECK_CV_RTV_STOREADY(string strWH_TYP,
                                          string strPLC_NO,
                                      ref string pRTN_MSG)
        {
            try
            {
                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strDEST_LOCATION = "";

                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_CV_RETREADY::";

                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*                     ";
                strSql += CRLF + "   FROM CV_DATA CD, JOB_MST JM         ";
                strSql += CRLF + "  WHERE CD.TRACK_NO = JM.START_POS     ";
                strSql += CRLF + "    AND CD.STO_READY_RD 	= '1'        ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD = '1'       ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD      = '0000'       ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	= '1'        ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		= 'N'        ";
                strSql += CRLF + "    AND CD.READ_UPD_DT > CD.WRITE_UPD_DT      ";
                //strSql += CRLF + "    AND CD.ERR_RQ_RD	    = '0'        ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += CRLF + "    AND CD.PLC_NO	= :PLC_NO          ";
                strSql += CRLF + "    AND CD.WH_TYP		    = :WH_TYP          ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '10'   ";
                //strSql += CRLF + "    AND B.JOB_TYP 	    = '3'        ";
                strSql += CRLF + "    AND JM.JOB_TYP        = :JOB_TYP          ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null        ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("JOB_TYP", DbLang.VARCHAR).Value = ((int)cDefApp.eJOBTYP.RtvRet).ToString();
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
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strDEST_LOCATION = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();

                    if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RtvRet)
                    {
                        if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strDEST_POS, strIS_TURN, strLUGG_NO, strWH_TYP, strPLC_NO, strSTART_POS, ref pRTN_MSG) == true)
                        {
                            if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == true)
                            {
                                _pBdb.Commit();
                                return true;
                            }

                        }
                        else
                        {
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
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }
        //*/

        // BARCODE MST 재보고(3분)
        // CHECK_BCR_RETRY
        /*
        public bool CHECK_BCR_RETRY(string strWH_TYP
                              , ref string pRTN_MSG)
        {
            try
            {
                string strTRACK_NO = "";
                int nJobCnt = 0;
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_BCR_RETRY::";

                strSql = "";
                strSql += CRLF + " SELECT BM.*, CD.*                            ";
                strSql += CRLF + "   FROM BCR_MST BM                            ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD                       ";
                strSql += CRLF + "     ON BM.BCR_MC_NO          = CD.TRACK_NO   ";
                strSql += CRLF + "    AND BM.WH_TYP             = CD.WH_TYP     ";
                strSql += CRLF + "    AND CD.WH_TYP             =  :WH_TYP      ";
                strSql += CRLF + "  WHERE CD.RET_READY_RD       = '1'           ";  // 출고대READY
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD    = '1'           ";
                strSql += CRLF + "    AND CD.DEST_POS_RD       <> '00000'       ";
                strSql += CRLF + "    AND BM.BCR_STA IN ('3')                   ";
                strSql += CRLF + "    AND ((" + DbLang.SYSDATE + " - BM.UPD_DT) * 24 * 60) > 5  ";
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
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();

                for (int i = 0; i < nJobCnt; i++)
                {
                    strTRACK_NO = _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " UPDATE BCR_MST                                               ";
                    strSql += CRLF + "    SET BCR_STA       = '1'                                   ";
                    strSql += CRLF + "      , NOREAD_CNT    = '0'                                   ";
                    strSql += CRLF + "      , UPD_DT        = " + DbLang.SYSDATE + "                ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP                               ";
                    strSql += CRLF + "    AND BCR_MC_NO     = :BCR_MC_NO                            ";
                    strSql += CRLF + "    AND ((" + DbLang.SYSDATE + " - UPD_DT) * 24 * 60) > 5     ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("BCR_MC_NO", DbLang.VARCHAR).Value = strTRACK_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }
                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "변경할 BARCODE TRACK 정보가 존재하지 않습니다. TRACK NO : " + strTRACK_NO;
                        return false;
                    }
                }

                _pBdb.Commit();

                string strLOG = "[IO TASK] 3분 뒤 재보고  (CV : " + strTRACK_NO + ")";
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
        //*/

        // 완료재보고
        // CHECK_SC_COMPLETE_RETRY
        /*
        public bool CHECK_SC_COMPLETE_RETRY(string strWH_TYP
                                      , ref string pRTN_MSG)
        {
            try
            {
                string strLUGG_NO = "";
                int nJobCnt = 0;

                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = "CHECK_SC_COMPLETE_RETRY::";

                strSql = "";
                strSql += CRLF + " SELECT JM.*                                                                                            ";
                strSql += CRLF + "   FROM JOB_MST JM                                                                                      ";
                strSql += CRLF + "  WHERE JM.JOB_STATUS        = '28'                                                                     ";
                strSql += CRLF + "    AND JM.WH_TYP            = :WH_TYP                                                                  ";
                strSql += CRLF + "    AND JM.JOB_TYP           <> '8'                                                                     ";
                strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - JM.UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                //strSql += CRLF + "    AND TO_CHAR(" + DbLang.SYSDATE + " - JM.UPD_DT, 'YYYYMMDDHH24MISS')::integer > 4  ";

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
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();

                for (int i = 0; i < nJobCnt; i++)
                {
                    strLUGG_NO = _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();

                    strSql = "";
                    strSql += CRLF + " UPDATE JOB_MST                                 ";
                    strSql += CRLF + "    SET JOB_STATUS    = '29'                    ";
                    strSql += CRLF + "       ,UPD_DT        = " + DbLang.SYSDATE + "  ";
                    strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP                 ";
                    strSql += CRLF + "    AND LUGG_NO       = :LUGG_NO                ";
                    strSql += CRLF + "    AND " + DbLang.TO_NUMBER("TO_CHAR(" + DbLang.SYSDATE + " - UPD_DT, 'YYYYMMDDHH24MISS')") + " > 4 ";
                    //strSql += CRLF + "    AND TO_CHAR(" + DbLang.SYSDATE + " - UPD_DT, 'YYYYMMDDHH24MISS')::integer > 4  ";
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

        // AGING 호기 시퀀스 구해오기.
        // SET_NEXT_AGING
        /*
        public bool SET_NEXT_AGING(string strWH_TYP,
                                   string strDEST_POS,
                               ref string strAGING_TYP,
                               ref string pRTN_MSG)
        {
            string strSEQ_NAME = "";
            int nSEQ_CNT = 0;

            DataTable dtAGING = new DataTable();
            DataTable dtCV_DATA = new DataTable();
            DataTable dtSC_DATA = new DataTable();

            int nJobCnt = 0;

            int nSelCnt = 0;
            string strSql = "";

            try
            {
                pRTN_MSG += "SET_NEXT_AGING::";

                strSql = "";
                strSql += cDefApp.CRLF + "  SELECT US.SEQUENCE_NAME, (US.MAX_VALUE - US.MIN_VALUE + 1) AS MAX_LEN  ";
                strSql += cDefApp.CRLF + "    FROM USER_SEQUENCES US ";
                strSql += cDefApp.CRLF + "   WHERE (US.SEQUENCE_NAME = UPPER('AGING_1G_SEQ') ";
                strSql += cDefApp.CRLF + "      OR US.SEQUENCE_NAME = UPPER('AGING_2G_SEQ') ";
                strSql += cDefApp.CRLF + "      OR US.SEQUENCE_NAME = UPPER('AGING_3G_SEQ') ";
                strSql += cDefApp.CRLF + "      OR US.SEQUENCE_NAME = UPPER('AGING_4G_SEQ') ";
                strSql += cDefApp.CRLF + "      OR US.SEQUENCE_NAME = UPPER('AGING_5G_SEQ') ";
                strSql += cDefApp.CRLF + "      OR US.SEQUENCE_NAME = UPPER('AGING_6G_SEQ') ";
                strSql += cDefApp.CRLF + "      OR US.SEQUENCE_NAME = UPPER('AGING_7G_SEQ') ";
                strSql += cDefApp.CRLF + "      OR US.SEQUENCE_NAME = UPPER('AGING_8G_SEQ') ";
                strSql += cDefApp.CRLF + "      OR US.SEQUENCE_NAME = UPPER('AGING_9G_SEQ')) ";
                strSql += cDefApp.CRLF + "     AND US.LAST_NUMBER like '%' " + DbLang.II + "  ?  " + DbLang.II + " '%' ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                nSelCnt = _pBdb.ExcuteQry(dtAGING, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "시퀀스 구해오기 실패";
                    return false;
                }

                nSEQ_CNT = System.Convert.ToInt32(dtAGING.Rows[0]["MAX_LEN"].ToString());
                strSEQ_NAME = "" + dtAGING.Rows[0]["SEQUENCE_NAME"].ToString();



                for (int i = 0; i < nSEQ_CNT; i++)
                {
                    strSql = "";
                    //strSql += cDefApp.CRLF + " SELECT " + strSEQ_NAME + ".NEXTVAL AS AGING_TYP FROM DUAL              ";
                    strSql += cDefApp.CRLF + " SELECT " + DbLang.NEXTVAL(strSEQ_NAME) + " AS AGING_TYP FROM DUAL              ";
                    nSelCnt = _pBdb.ExcuteQry(dtAGING, strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        pRTN_MSG += "시퀀스 변경하기 실패";
                        return false;
                    }

                    strAGING_TYP = "" + dtAGING.Rows[0]["AGING_TYP"].ToString();

                    strSql = "";
                    strSql += cDefApp.CRLF + "  SELECT CD.TRACK_NO           ";
                    strSql += cDefApp.CRLF + "    FROM CV_DATA CD            ";
                    strSql += cDefApp.CRLF + "   WHERE CD.DEST_POS_RD    = ? ";
                    strSql += cDefApp.CRLF + "     AND CD.WH_TYP         = ? ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strAGING_TYP;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                    nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nSelCnt < 3)
                    {

                        string aa = strAGING_TYP.Substring(0, 3) + "_" + strAGING_TYP.Substring(4, 1);
                        strSql = "";
                        strSql += cDefApp.CRLF + "  SELECT *                 ";
                        strSql += cDefApp.CRLF + "    FROM SC_DATA SD        ";
                        strSql += cDefApp.CRLF + "   WHERE SD.SC_NO like :SC_NO   ";
                        strSql += cDefApp.CRLF + "     AND SD.SUSPEND <> '0' ";
                        strSql += cDefApp.CRLF + "     AND SD.WH_TYP  = :WH_TYP    ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strAGING_TYP.Substring(0, 3) + "_" + strAGING_TYP.Substring(4, 1);
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        nSelCnt = _pBdb.ExcuteQry(dtSC_DATA, strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            return false;
                        }

                        if (nSelCnt > 2)
                        {
                            continue;
                        }

                        dtAGING.Dispose();
                        dtCV_DATA.Dispose();
                        dtSC_DATA.Dispose();

                        return true;
                    }
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

        // SC 호기 검색 - 검색 조건과 출고 시간이 가장 먼 MSC 라인을 검색
        // SET_NEXT_AGING_BY_DURATION_TIME
        /*
        public bool SET_NEXT_AGING_BY_DURATION_TIME(string strWH_TYP,
                                                    string strDEST_POS,
                                                    string strDURATION_TIME,
                                                    string strLOOP_NM,
                                                ref string strAGING_TYP,
                                                ref string pRTN_MSG)
        {
            int nAGING_CNT = 0;
            try
            {
                pRTN_MSG += "SET_NEXT_AGING_BY_DURATION_TIME::";

                DataTable dtCV_DATA = new DataTable();
                DataTable dtSC_DATA = new DataTable();

                DataTable dtSYS_DATE = new DataTable();
                DataTable dtAGING = new DataTable();

                int nSelCnt = 0;
                string strSql = "";

                // 작업예정시간(DURATION_TIME [MES수신])
                strSql = "";
                strSql += cDefApp.CRLF + "SELECT " + DbLang.SYSDATE + " + (1/24/60/60)*((TO_NUMBER(SUBSTR(:DURATION_TIME1 ,1 ,3)) * 60 * 60 * 24)  ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME2 ,4 ,2)) * 60 * 60)       ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME3 ,6 ,2)) * 60)            ";
                strSql += cDefApp.CRLF + "                             + (TO_NUMBER(SUBSTR(:DURATION_TIME4 ,8 ,2)))) AS END_TIME    ";
                strSql += cDefApp.CRLF + " from DUAL ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("DURATION_TIME1", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME2", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME3", DbLang.VARCHAR).Value = strDURATION_TIME;
                _pBdb.mComMain.Parameters.Add("DURATION_TIME4", DbLang.VARCHAR).Value = strDURATION_TIME;
                nSelCnt = _pBdb.ExcuteQry(dtSYS_DATE, strSql);
                if (nSelCnt <= 0)
                {
                    dtAGING.Dispose();
                    dtSYS_DATE.Dispose();
                    dtCV_DATA.Dispose();
                    dtSC_DATA.Dispose();
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt > 0)
                {

                    DateTime dtEND_TIME = new DateTime();
                    dtEND_TIME = Convert.ToDateTime(dtSYS_DATE.Rows[0]["END_TIME"].ToString());

                    strSql = "";
                    strSql += cDefApp.CRLF + "SELECT CM.AGING_TYP, 0 AS DURATION_TIME           ";
                    strSql += cDefApp.CRLF + "      ,RANK() OVER (ORDER BY 0) AS PRIORITY       ";
                    strSql += cDefApp.CRLF + "  FROM CELL_MST CM                                ";
                    strSql += cDefApp.CRLF + " WHERE WH_TYP = :WH_TYP1                           ";
                    strSql += cDefApp.CRLF + "   AND 0 = ( SELECT COUNT(CD.CELL_SC_NO)          ";
                    strSql += cDefApp.CRLF + "               FROM CELL_DTL CD                   ";
                    strSql += cDefApp.CRLF + "              WHERE CD.WH_TYP = CM.WH_TYP         ";
                    strSql += cDefApp.CRLF + "  	          AND CONCAT(SUBSTR(CD.CELL_SC_NO, 0, 3),SUBSTR(CD.CELL_SC_NO, 5, 1))           ";
                    strSql += cDefApp.CRLF + "                  = CONCAT(SUBSTR(CM.CELL_SC_NO, 0, 3), SUBSTR(CM.CELL_SC_NO, 5, 1)))         ";
                    strSql += cDefApp.CRLF + "   AND SUBSTR(CM.CELL_SC_NO, 0, 3) = :DEST_POS1    ";
                    strSql += cDefApp.CRLF + " GROUP BY AGING_TYP                               ";
                    strSql += cDefApp.CRLF + " UNION ALL                                        ";
                    strSql += cDefApp.CRLF + " SELECT CM.AGING_TYP, MIN((:END_DT1 - CD.AGING_END_DT) * 24 * 60) AS DURATION_TIME               ";
                    strSql += cDefApp.CRLF + "  	 ,RANK() OVER (ORDER BY MIN((:END_DT2.AGING_END_DT) * 24 * 60) DESC) AS PRIORITY      ";
                    strSql += cDefApp.CRLF + "   FROM CELL_MST CM                                                     ";
                    strSql += cDefApp.CRLF + "  INNER JOIN CELL_DTL CD                                                ";
                    strSql += cDefApp.CRLF + "          ON CD.WH_TYP = CM.WH_TYP                                      ";
                    strSql += cDefApp.CRLF + "         AND CD.CELL_NO = CM.CELL_NO                                    ";
                    strSql += cDefApp.CRLF + "         AND SUBSTR(CD.CELL_SC_NO, 0, 3) = SUBSTR(CM.AGING_TYP, 0, 3)   ";
                    strSql += cDefApp.CRLF + "  WHERE CM.WH_TYP = :WH_TYP2                                                ";
                    strSql += cDefApp.CRLF + "    AND SUBSTR(CD.CELL_SC_NO, 0, 3) = :DEST_POS2                        ";
                    strSql += cDefApp.CRLF + "  GROUP BY CM.AGING_TYP                                                 ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP1", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("DEST_POS1", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                    _pBdb.mComMain.Parameters.Add("END_DT1", DbLang.DATE).Value = dtEND_TIME;
                    _pBdb.mComMain.Parameters.Add("END_DT2", DbLang.DATE).Value = dtEND_TIME;
                    _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("DEST_POS2", DbLang.VARCHAR).Value = strDEST_POS.Substring(0, 3);
                    nAGING_CNT = _pBdb.ExcuteQry(dtAGING, strSql);
                    if (nAGING_CNT < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nAGING_CNT == 0)
                    {
                        pRTN_MSG += "AGING 검색 실패";
                        return false;
                    }


                    for (int i = 0; i < nAGING_CNT; i++)
                    {

                        strAGING_TYP = "" + dtAGING.Rows[i]["AGING_TYP"].ToString();

                        strSql = "";
                        strSql += cDefApp.CRLF + "  SELECT CD.TRACK_NO                      ";
                        strSql += cDefApp.CRLF + "    FROM CV_DATA CD                       ";
                        strSql += cDefApp.CRLF + "   WHERE CD.DEST_POS_RD    = :DEST_POS_RD ";
                        strSql += cDefApp.CRLF + "     AND CD.WH_TYP         = :WH_TYP      ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("DEST_POS_RD", DbLang.VARCHAR).Value = strAGING_TYP;
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            return false;
                        }

                        if (nSelCnt < 3)
                        {
                            //MSC가 3대이상 정지되어 있으면 다음 라인의 크레인을 검색한다
                            //ex : 29111, 29121, 29131 모두 정지이면 29112 검색 
                            strSql = "";
                            strSql += cDefApp.CRLF + "  SELECT SC.*                   ";
                            strSql += cDefApp.CRLF + "    FROM SC_DATA SD             ";
                            strSql += cDefApp.CRLF + "   WHERE SD.SC_NO like :SC_NO   ";
                            strSql += cDefApp.CRLF + "     AND SD.SUSPEND <> '0'      ";
                            strSql += cDefApp.CRLF + "     AND SD.WH_TYP  = :WH_TYP   ";
                            _pBdb.mComMain.CommandType = CommandType.Text;
                            _pBdb.mComMain.Parameters.Clear();
                            _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strAGING_TYP.Substring(0, 3) + "_" + strAGING_TYP.Substring(4, 1);
                            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                            nSelCnt = _pBdb.ExcuteQry(dtSC_DATA, strSql);
                            if (nSelCnt < 0)
                            {
                                pRTN_MSG += _pBdb.ErrMsg;
                                return false;
                            }

                            if (nSelCnt > 2)
                            {
                                continue;
                            }

                            dtAGING.Dispose();
                            dtSYS_DATE.Dispose();
                            dtCV_DATA.Dispose();
                            dtSC_DATA.Dispose();
                            return true;
                        }

                    }
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

        // S/C에 작업쓰기.
        // UPDATE_SC_DATA
        /*
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
                                   string strSTART_HSPOS_FK2
                                 , ref string pRTN_MSG)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                // 작업전 초기화.
                if (UPDATE_SC_OD_RESET(strWH_TYP, strSC_NO, ref pRTN_MSG) == false)
                    return false;

                pRTN_MSG += "UPDATE_SC_DATA::";

                if (strJOB_TYP == "1")
                {
                    // 입고
                    // S/C에 작업정보쓰기.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                       ";
                    strSql += CRLF + "    SET JOB_TYP_OD          =  ?      ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD      =  ?      ";
                    strSql += CRLF + "      , DEST_BANK_FK1_OD    =  ?      ";
                    strSql += CRLF + "      , DEST_BAY_FK1_OD     =  ?      ";
                    strSql += CRLF + "      , DEST_LEVEL_FK1_OD   =  ?      ";
                    strSql += CRLF + "      , START_HSPOS_FK1_OD  =  ?      ";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD      =  ?      ";
                    strSql += CRLF + "      , DEST_BANK_FK2_OD    =  ?      ";
                    strSql += CRLF + "      , DEST_BAY_FK2_OD     =  ?      ";
                    strSql += CRLF + "      , DEST_LEVEL_FK2_OD   =  ?      ";
                    strSql += CRLF + "      , START_HSPOS_FK2_OD  =  ?      ";
                    strSql += CRLF + "      , USE_FK_OD           =  ?      ";
                    strSql += CRLF + "      , USER_COMMAND_OD     =  ?      ";            // D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD       =  ?      ";            // D191
                    strSql += CRLF + "      , OD_RQ_YN            =  'Y'    ";
                    strSql += CRLF + "      , OD_USER_ID          =  'IOTASK' ";
                    strSql += CRLF + "      , OD_UPD_DT           =  " + DbLang.SYSDATE + "  ";
                    strSql += CRLF + "  WHERE WH_TYP              = ?   ";
                    strSql += CRLF + "    AND SC_NO               = ?   ";
                    strSql += CRLF + "    AND OD_RQ_YN            = 'N' ";
                    strSql += CRLF + "    AND ERR_CODE_RD         = '0' ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSTART_HSPOS_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSTART_HSPOS_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);

                }

                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.Ret).ToString() ||
                    strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString() ||
                    strJOB_TYP == ((int)cDefApp.eJOBTYP.OtherRet).ToString())
                {
                    // 출고
                    // S/C에 작업정보쓰기.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                   ";
                    strSql += CRLF + "    SET JOB_TYP_OD         =  ?   ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD     =  ?   ";
                    strSql += CRLF + "      , START_BANK_FK1_OD  =  ?   ";
                    strSql += CRLF + "      , START_BAY_FK1_OD   =  ?   ";
                    strSql += CRLF + "      , START_LEVEL_FK1_OD =  ?   ";
                    strSql += CRLF + "      , DEST_HSPOS_FK1_OD  =  ?   ";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD     =  ?   ";
                    strSql += CRLF + "      , START_BANK_FK2_OD  =  ?   ";
                    strSql += CRLF + "      , START_BAY_FK2_OD   =  ?   ";
                    strSql += CRLF + "      , START_LEVEL_FK2_OD =  ?   ";
                    strSql += CRLF + "      , START_HSPOS_FK2_OD =  ?   ";
                    strSql += CRLF + "      , USE_FK_OD          =  ?   ";
                    strSql += CRLF + "      , USER_COMMAND_OD    =  ?   ";            //D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD      =  ?   ";            //D191
                    strSql += CRLF + "      , OD_RQ_YN           =  'Y' ";
                    strSql += CRLF + "      , OD_USER_ID         =  'IOTASK' ";
                    strSql += CRLF + "      , OD_UPD_DT          =  " + DbLang.SYSDATE + "  ";
                    strSql += CRLF + "  WHERE WH_TYP             = ?         ";
                    strSql += CRLF + "    AND SC_NO              = ?         ";
                    strSql += CRLF + "    AND OD_RQ_YN           = 'N'       ";
                    strSql += CRLF + "    AND ERR_CODE_RD        = '0'       ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_HSPOS_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_HSPOS_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSC_NO;
                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                }

                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.RtoR).ToString())
                {
                    // RACK TO RACK (RACK내)
                    // S/C에 작업정보쓰기.
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                    ";
                    strSql += CRLF + "    SET JOB_TYP_OD          =  ?   ";
                    strSql += CRLF + "      , LUGG_NO_FK1_OD      =  ?   ";
                    strSql += CRLF + "      , START_BANK_FK1_OD   =  ?   ";
                    strSql += CRLF + "      , START_BAY_FK1_OD    =  ?   ";
                    strSql += CRLF + "      , START_LEVEL_FK1_OD  =  ?   ";
                    strSql += CRLF + "      , DEST_BANK_FK1_OD    =  ?   ";
                    strSql += CRLF + "      , DEST_BAY_FK1_OD     =  ?   ";
                    strSql += CRLF + "      , DEST_LEVEL_FK1_OD   =  ?   ";
                    strSql += CRLF + "      , LUGG_NO_FK2_OD      =  ?   ";
                    strSql += CRLF + "      , START_BANK_FK2_OD   =  ?   ";
                    strSql += CRLF + "      , START_BAY_FK2_OD    =  ?   ";
                    strSql += CRLF + "      , START_LEVEL_FK2_OD  =  ?   ";
                    strSql += CRLF + "      , DEST_BANK_FK2_OD    =  ?   ";
                    strSql += CRLF + "      , DEST_BAY_FK2_OD     =  ?   ";
                    strSql += CRLF + "      , DEST_LEVEL_FK2_OD   =  ?   ";
                    strSql += CRLF + "      , USE_FK_OD           =  ?   ";
                    strSql += CRLF + "      , USER_COMMAND_OD     =  ?   ";            //D199 영역
                    strSql += CRLF + "      , WRITE_FLAG_OD       =  ?   ";            //D191
                    strSql += CRLF + "      , OD_RQ_YN            =  'Y' ";
                    strSql += CRLF + "      , OD_USER_ID          =  'IOTASK' ";
                    strSql += CRLF + "      , OD_UPD_DT           =  " + DbLang.SYSDATE + "  ";
                    strSql += CRLF + "  WHERE WH_TYP              = ?         ";
                    strSql += CRLF + "    AND SC_NO               = ?         ";
                    strSql += CRLF + "    AND OD_RQ_YN            = 'N'       ";
                    strSql += CRLF + "    AND ERR_CODE_RD         = '0'       ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strJOB_TYP;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strLUGG_NO_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_BANK_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_BAY_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_LEV_FK1;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strLUGG_NO_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSOUR_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_BANK_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_BAY_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strDEST_LEV_FK2;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strUSE_FK;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strUSER_COMMAND;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWRITE_FLAG;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSC_NO;
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
        //*/

        // S/C정보 초기화.
        // UPDATE_SC_OD_RESET
        /*
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
                                    ref string strRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
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
                        // 2019.10.16
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
                    strSql += cDefApp.CRLF + "                      " + DbLang.SYSDATE + " ,         ";
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

        // L/F에서 로케이션 구할때 가장 적게 들은 BANK로 입고함
        // SET_NEXT_LEV_BY_LESS_BANK
        /*
        public bool SET_NEXT_LEV_BY_LESS_BANK(string strWH_TYP,
                                              string strDEST_POS,
                                              string strPLC_NO,
                                              string strDURATION_TIME,
                                              ref string strAGING_TYP,
                                              ref string strDEST_LOCATION,
                                              ref string pRTN_MSG)
        {

            DataTable dtSC_LEV_DEF = new DataTable();
            DataTable dtCV_DATA = new DataTable();
            int nAgingCnt = 0;
            string strSC_NO_LEV = "";

            int nSelCnt = 0;
            string strSql = "";

            try
            {
                // 같은 AGING_TYP에서 가장 적게 입고된 SC를 구하는 로직. 
                strSql = "";
                strSql += CRLF + " SELECT CELL_SC_NO, COUNT(CM.BANK)        ";
                strSql += CRLF + "   FROM CELL_MST CM                       ";
                strSql += CRLF + "  WHERE CM.WH_TYP         = :WH_TYP       ";
                strSql += CRLF + "    AND CM.AGING_TYP 	    = :AGING_TYP    ";  // 에이징[호기별명칭]구분
                strSql += CRLF + "    AND CM.CELL_STA       = '0'           ";  // 사용가능한 BANK가 가장 많은 호기 구하기
                strSql += CRLF + "  GROUP BY CM.CELL_SC_NO                  ";
                strSql += CRLF + "  ORDER BY COUNT(CM.BANK) DESC            ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("AGING_TYP", DbLang.VARCHAR).Value = strDEST_POS;
                nAgingCnt = _pBdb.ExcuteQry(dtSC_LEV_DEF, strSql);
                if (nAgingCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nAgingCnt > 0)
                {
                    // S/C 호기별 순차적입고를 위한 로직.
                    for (int j = 0; j < nAgingCnt; j++)
                    {
                        strSC_NO_LEV = "" + dtSC_LEV_DEF.Rows[nAgingCnt]["CELL_SC_NO"].ToString();

                        // BUFFER수 확인.(3개미만이어야함)
                        strSql = "";
                        strSql += cDefApp.CRLF + "  SELECT CD.TRACK_NO                   ";
                        strSql += cDefApp.CRLF + "    FROM CV_DATA CD                    ";
                        strSql += cDefApp.CRLF + "   WHERE CD.WH_TYP      = :WH_TYP      ";
                        strSql += cDefApp.CRLF + "     AND CD.PLC_NO      = :PLC_NO      ";
                        strSql += cDefApp.CRLF + "     AND CD.DEST_POS_RD = :DEST_POS_RD ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                        _pBdb.mComMain.Parameters.Add("DEST_POS_RD", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            return false;
                        }

                        if (nSelCnt >= 2)
                            continue;

                        else if (nSelCnt < 3)
                        {
                            strAGING_TYP = strSC_NO_LEV;

                            // LOCATION 가져오기.
                            if (FETCH_LOCATION_BY_AGING_TYP(strWH_TYP
                                                          , strDURATION_TIME
                                                          , strSC_NO_LEV
                                                          , ref strDEST_LOCATION
                                                          , ref pRTN_MSG) == true)
                            {

                                dtSC_LEV_DEF.Dispose();
                                dtCV_DATA.Dispose();
                                return true;
                            }
                        }
                    }
                }

                dtSC_LEV_DEF.Dispose();
                dtCV_DATA.Dispose();
                return false;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
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

            int nSelCnt = 0;
            string strSql = "";

            DataTable dtSC_LEV_DEF = new DataTable();
            DataTable dtCV_DATA = new DataTable();

            try
            {
                // 마지막 입고한 LEVEL정보를 구해오는쿼리. 
                // SC_LEV_DEF에 SC_LEV_CNT = 1, POS = 1로 지정하면 됨 크레인번호
                strSql = "";
                strSql += CRLF + " SELECT SLD.*                             ";
                strSql += CRLF + "   FROM SC_LEV_DEF SLD                    ";
                strSql += CRLF + "  WHERE SLD.WH_TYP 	    = :WH_TYP       ";
                strSql += CRLF + "    AND SLD.AGING_TYP 	= :AGING_TYP    ";  // 에이징[호기별명칭]구분
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("AGING_TYP", DbLang.VARCHAR).Value = strDEST_POS;
                nSelCnt = _pBdb.ExcuteQry(dtSC_LEV_DEF, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt > 0)
                {
                    int nLEV_CNT = System.Convert.ToInt32("" + dtSC_LEV_DEF.Rows[0]["LEV_CNT"].ToString());
                    int nPos = System.Convert.ToInt32("" + dtSC_LEV_DEF.Rows[0]["POS"].ToString());
                    string strSC_NO_LEV = "";

                    // S/C 호기별 순차적입고를 위한 로직.
                    for (int j = 0; j < nLEV_CNT; j++)
                    {
                        nPos++;
                        if (nPos > nLEV_CNT)
                        {
                            nPos = 1;
                        }
                        string strColName = string.Format("SC_NO_LEV{0}", nPos);
                        strSC_NO_LEV = "" + dtSC_LEV_DEF.Rows[0][strColName].ToString();

                        //// 1.작업이 가능한 S/C인지 확인 한 후 불가능한 작업인경우
                        //strSql = "";
                        //strSql += cDefApp.CRLF + "  SELECT SD.*                 ";
                        //strSql += cDefApp.CRLF + "    FROM SC_DATA SD           ";
                        //strSql += cDefApp.CRLF + "   WHERE SD.WH_TYP = :WH_TYP  ";
                        //strSql += cDefApp.CRLF + "     AND SD.SC_NO  = :SC_NO   ";
                        //_pBdb.mComMain.CommandType = CommandType.Text;
                        //_pBdb.mComMain.Parameters.Clear();
                        //_pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        //_pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        //SelCnt = _pBdb.ExcuteQry(strSql);
                        //if (SelCnt <= 0)
                        //{
                        //    pRTN_MSG += _pBdb.ErrMsg;
                        //    return false;
                        //}

                        //// 1.자동 AUTO_MODE_RD, 2.ACTIVE ACTIVE_MODE_RD 확인, 3.ONLINE_MODE_RD = '1'(무조건 1일때만 처리가능)
                        //if (_pBdb.mDtMain.Rows[0]["AUTO_MODE_RD"].ToString() != "1")
                        //{
                        //    pRTN_MSG += "자동모드가 아닙니다.";
                        //    continue;
                        //}
                        //if (_pBdb.mDtMain.Rows[0]["ACTIVE_MODE_RD"].ToString() != "1")
                        //{
                        //    pRTN_MSG += "ACTIVE 모드가 아닙니다.";
                        //    continue;
                        //}
                        //if (_pBdb.mDtMain.Rows[0]["ONLINE_MODE_RD"].ToString() != "1")
                        //{
                        //    pRTN_MSG += "온라인모드가 아닙니다.";
                        //    continue;
                        //}
                        //// 2.SC_LEV_DEF의 POS를 ++ 후 CONTINUE.

                        // BUFFER수 확인.(3개미만이어야함)
                        strSql = "";
                        strSql += cDefApp.CRLF + "  SELECT CD.TRACK_NO                   ";
                        strSql += cDefApp.CRLF + "    FROM CV_DATA CD                    ";
                        strSql += cDefApp.CRLF + "   WHERE CD.WH_TYP      = :WH_TYP      ";
                        strSql += cDefApp.CRLF + "     AND CD.PLC_NO      = :PLC_NO      ";
                        strSql += cDefApp.CRLF + "     AND CD.DEST_POS_RD = :DEST_POS_RD ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                        _pBdb.mComMain.Parameters.Add("DEST_POS_RD", DbLang.VARCHAR).Value = strSC_NO_LEV;
                        nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                        if (nSelCnt < 0)
                        {
                            pRTN_MSG += _pBdb.ErrMsg;
                            return false;
                        }

                        if (nSelCnt >= 2)
                            continue;

                        else if (nSelCnt < 3)
                        {
                            strAGING_TYP = strSC_NO_LEV;

                            // LOCATION 가져오기.
                            if (FETCH_LOCATION_BY_AGING_TYP(strWH_TYP
                                                          , strDURATION_TIME
                                                          , strSC_NO_LEV
                                                          , ref strDEST_LOCATION
                                                          , ref pRTN_MSG) == true)
                            {
                                nPos = nPos + 1;
                                if (nPos > nLEV_CNT)
                                {
                                    nPos = 1;
                                }

                                // 구해온 LEVEL정보 UPDATE.
                                strSql = "";
                                strSql += CRLF + " UPDATE SC_LEV_DEF                ";
                                strSql += CRLF + "    SET POS         = :POS        ";
                                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP     ";
                                strSql += CRLF + "    AND AGING_TYP   = :AGING_TYP  ";
                                _pBdb.mComMain.CommandType = CommandType.Text;
                                _pBdb.mComMain.Parameters.Clear();
                                _pBdb.mComMain.Parameters.Add("POS", DbLang.VARCHAR).Value = nPos.ToString();
                                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                                _pBdb.mComMain.Parameters.Add("AGING_TYP", DbLang.VARCHAR).Value = strDEST_POS;
                                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                                if (nSelCnt < 0)
                                {
                                    pRTN_MSG += _pBdb.ErrMsg;
                                    return false;
                                }

                                if (nSelCnt == 0)
                                {
                                    pRTN_MSG += "변경할 SC_LEV_DEF 정보가 존재하지 않습니다. AGING_TYP : " + strDEST_POS;
                                    return false;
                                }

                                dtSC_LEV_DEF.Dispose();
                                dtCV_DATA.Dispose();
                                return true;
                            }
                        }
                    }
                }

                dtSC_LEV_DEF.Dispose();
                dtCV_DATA.Dispose();
                return false;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }
        //*/

        // GET_CV_NEW
        /*
        public bool GET_CV_NEW(string strGRP_TYP,
                               string strWH_TYP,
                               string strSTART_POS)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                _pBdb.BeginTrans();

                IsRtnMsg = "GET_CV_NEW::";

                strSql = "";
                strSql += CRLF + " SELECT  LUGG_NO,                       ";
                strSql += CRLF + "         START_POS,                     ";
                strSql += CRLF + "         DEST_POS,                      ";
                strSql += CRLF + "   FROM  JOB_DATA                       ";
                strSql += CRLF + "  WHERE  JOB_STATUS       = '10'        ";
                strSql += CRLF + "    AND  START_POS        = ?           ";
                strSql += CRLF + "    AND  WH_TYP           = ?           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSTART_POS;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    _pBdb.Commit();
                    return true;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    _pBdb.Rollback();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                _pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }
        //*/

        // 시뮬레이터 모드일때 특정 위치의 조건을 강제로 수정하는 기능 
        // CHECK_SIM_MODE
        /*
        public bool CHECK_SIM_MODE(string strWH_TYP,
                                  string strPLC_NO,
                              ref string pRTN_MSG)
        {
            try
            {
                string strLUGG_NO = "";
                string strTRACK_NO = "";
                //string strBCR_TOP = "";
                //string strBCR_BOTTOM = "";

                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg = "CHECK_SIM_MODE::";

                strSql = "";
                strSql += cDefApp.CRLF + " SELECT CD.*, JM.*                            ";
                strSql += cDefApp.CRLF + "   FROM CV_DATA CD                            ";
                strSql += cDefApp.CRLF + "  INNER JOIN JOB_MST JM                       ";
                strSql += cDefApp.CRLF + "     ON CD.MC_NO = JM.START_POS               ";
                strSql += cDefApp.CRLF + "    AND JM.JOB_STATUS = '10'                  ";
                strSql += cDefApp.CRLF + "  WHERE CD.PLC_NO	= :PLC_NO                   ";
                strSql += cDefApp.CRLF + "    AND CD.LUGG_NO_RD 	= '0000'            ";
                strSql += cDefApp.CRLF + "    AND CD.STO_READY_RD 	= '1'               ";
                strSql += cDefApp.CRLF + "    AND CD.SENSOR0_DATA_RD = '1'              ";
                strSql += cDefApp.CRLF + "    AND CD.AUTO_MODE_RD 	= '1'               ";
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_YN		= 'N'               ";
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_FLAG		= 'N'               ";
                //strSql += cDefApp.CRLF + "    AND CD.ERR_RQ_RD	    = '0'               ";      // 현재 에러 기록 요청은 사용하지 않는 것으로 결정 - 류팀장과 협의(2020.05.29)
                strSql += cDefApp.CRLF + "    AND CD.WH_TYP		    = :WH_TYP           ";
                strSql += cDefApp.CRLF + "    AND 0 = (SELECT COUNT(*)                  ";
                strSql += cDefApp.CRLF + "               FROM JOB_MST                   ";
                strSql += cDefApp.CRLF + "              WHERE LUGG_NO = CD.LUGG_NO_RD)  ";
                strSql += cDefApp.CRLF + "  LIMIT 1                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
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
                    string strJOB_START_POS = "";
                    string strJOB_DEST_POS = "";
                    string strJOB_DEST_LOC = "";
                    string strPRODUCT_SIZE = "";
                    string strDestPos = "";
                    string strDestLocation = "";
                    int nJobType = 1;
                    DataTable dtDestPos = new DataTable();

                    strTRACK_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    //strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    //strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strPRODUCT_SIZE = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strJOB_DEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strJOB_DEST_LOC = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strJOB_START_POS = "" + _pBdb.mDtMain.Rows[i]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_POS"].ToString();

                    // 목적지 가져오기 
                    if (SELECT_DEST_POS(strWH_TYP, strTRACK_NO, strJOB_DEST_POS, ref strDestPos, ref pRTN_MSG, ref dtDestPos) == false)
                    {
                        m_strRtnMsg = pRTN_MSG;
                        _pBdb.Rollback();
                        throw new Exception(m_strRtnMsg);
                    }

                    // 설비타스크에 작업지시
                    if (UPDATE_CV_DATA(nJobType.ToString(), strPRODUCT_SIZE, "0", strDestPos, "0", strLUGG_NO, strWH_TYP, strPLC_NO, strJOB_START_POS, ref pRTN_MSG) == false)
                    {
                        m_strRtnMsg = pRTN_MSG;
                        _pBdb.Rollback();
                        throw new Exception(m_strRtnMsg);
                    }

                    // 작업상태 변경 
                    if (UPDATE_JOB_DATA("11", strLUGG_NO, strWH_TYP, nJobType.ToString(), ref pRTN_MSG) == false)
                    {
                        m_strRtnMsg = pRTN_MSG;
                        _pBdb.Rollback();
                        throw new Exception(m_strRtnMsg);
                    }
                }
                pRTN_MSG = strTRACK_NO + "입고대에서 CV_TASK를 통해서 작업 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
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
        //*/
        #endregion
    }
}