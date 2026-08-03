using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data;

namespace WCS_TASK_Display
{
    // 전광판 한 대의 메모리 상태(변경감지 + 색상 순환).
    // 레거시 CDisplayInfo(마지막으로 표시한 적재물을 캐시하던 클래스)에 해당한다.
    public class DispState
    {
        public string LAST_LUGG = "";   // @.마지막으로 전송한 적재물 번호
        public int COLOR_IDX = 2;       // @.0:빨강, 1:초록, 2:노랑  (레거시와 같이 노랑에서 시작)
    }

    // 전광판 컨트롤러 한 대를 담당하는 작업 스레드. 구조는 CvThread 와 동일하다.
    //   - DisplayProtocol(소켓 + DB) 객체를 하나 들고 있는다
    //   - 포트를 순환하며 전광판에 접속한다
    //   - 200ms 주기로 해당 컨트롤러의 전광판들을 폴링한다
    //   - AUTO   : DISPLAY_DATA 를 읽어 적재물이 바뀌면 표시내용을 전광판으로 보낸다
    //   - MANUAL : Client 가 기록한 CMD_RQ_YN='Y' 행을 읽어 그대로 전송한다
    //   - 접속상태를 EQP_MST 에 반영하고, 감사 로그를 WCS_LOG_PGR 에 남긴다
    public class DisplayThread : maindefine
    {
        #region 멤버
        private string m_strWh_typ;
        private string m_strEqmt_typ;   // @."DISPLAY"
        private string m_strPlc_No;     // @.컨트롤러 번호
        private string m_strIp;
        private int m_nCurPort;
        private int m_nFromPort;
        private int m_nToPort;
        private int m_nPortCnt;
        public int m_nCnt;              // @.이 컨트롤러에 달린 전광판 대수
        public int m_nFrTrackNo;
        public int m_nToTrackNo;
        public int m_nthNo;
        public string m_strRtnMsg;
        public string m_strLogFileNm;
        public bool m_blConnectYn = false;
        public bool m_bStopReq = false;     // @.[접속 끊기] 요청. 접속 시도/폴링 루프를 빠져나온다

        private string m_strConnectString;
        private DisplayProtocol m_disp;
        public Thread m_thThread;
        public SYS_MAIN m_frmMain;
        private bool m_bOpen;
        public bool IsOpen { get { return m_bOpen; } set { m_bOpen = value; } }

        // @.DISP_NO 를 키로 하는 전광판별 상태
        private Dictionary<int, DispState> DispDic = new Dictionary<int, DispState>();

        string strSql = "";
        string CRLF = "\r\n";
        int nSelCnt = 0;
        private string _strErrorMsg = "";
        #endregion

        #region 생성자
        public DisplayThread(int nThNo,
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
            m_nthNo = nThNo;
            m_strWh_typ = strWh_typ;
            m_strEqmt_typ = Eqmt_typ;
            m_strPlc_No = Plc_No;
            m_strIp = Ip;
            m_nCurPort = CurPort;
            m_nFromPort = FromPort;
            m_nToPort = ToPort;
            m_nPortCnt = PortCnt;
            m_nCnt = Cnt;
            m_nFrTrackNo = FrTrackNo;
            m_nToTrackNo = ToTrackNo;
            m_strConnectString = ConnectString;
            m_strLogFileNm = strLogFileNm;
            IsOpen = false;

            m_disp = new DisplayProtocol(m_strConnectString);
            m_disp.IsHex = true;
        }
        #endregion

        #region 화면 메시지 출력 (폼으로 위임, CvThread 와 동일 규약)
        private void MakeMsg(string msg, int nThGbn)
        {
            try { m_frmMain.PsMsgView(msg, m_strPlc_No.ToString(), nThGbn); }
            catch { return; }
        }
        private void MakeMsg_Error(string msg, int nThGbn)
        {
            try
            {
                m_frmMain.PsMsgView_Error(msg, m_strPlc_No.ToString(), nThGbn);
                cDefApp.m_LogQ[m_nthNo].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch { return; }
        }
        private void MakeMsg_Imp(string msg, int nThGbn)
        {
            try
            {
                m_frmMain.PsMsgView_IMP(msg, m_strPlc_No.ToString(), nThGbn);
                cDefApp.m_LogQ[m_nthNo].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch { return; }
        }
        public void SetErrorMsg(string strMsg) { _strErrorMsg = strMsg; Log.Error(_strErrorMsg); }
        #endregion

        #region Thread_Doing
        public void Thread_Doing(object value)
        {
            try
            {
                if (cDefApp.GM_STAT_MAIN == false)
                    throw new Exception("Main system not running");

                MakeMsg_Imp("DB/Socket Connecting", m_nthNo);

                // ---- 포트를 순환하며 접속 (CvThread 와 동일한 방식) ----
                if (m_disp.m_bSocCon == false && m_disp.m_bDBOpen == false)
                {
                    for (int i = 0; i < m_nToPort - m_nFromPort; i++)
                    {
                        if (m_bStopReq) goto EXIT_LBL;   // @.접속 시도 중에도 중단 요청을 받는다
                        if (m_nCurPort > m_nToPort) m_nCurPort = m_nFromPort;

                        for (int j = 0; j < m_nPortCnt; j++)
                        {
                            MakeMsg_Imp(string.Format("IP [{0}] PORT [{1}] connect try", m_strIp, m_nCurPort), m_nthNo);
                            m_disp.SetConfig(m_strIp, m_nCurPort, 2);

                            if (!m_disp.Open(ref m_strRtnMsg))
                            {
                                SetErrorMsg("Comm" + m_nthNo + " :" + m_strRtnMsg);
                                MakeMsg_Error(m_strRtnMsg, m_nthNo);

                                if (m_disp.m_bSocCon == false && m_disp.m_bDBOpen == true)
                                    InsertWcsLogPgr("", "[Thread_Doing] socket connect fail");

                                m_disp.Close(ref m_strRtnMsg);

                                if (j == m_nPortCnt - 1) m_nCurPort = m_nCurPort + 1;
                                m_blConnectYn = false;
                                Thread.Sleep(500);
                                continue;
                            }
                            else
                            {
                                string strCOMM = "COMM" + m_nthNo;
                                cDefApi.WritePrivateProfileString(strCOMM, "CUR_PORT", Convert.ToString("" + m_nCurPort), cDefApp.GM_ENV_INI);
                                InsertWcsLogPgr("", "[Thread_Doing] DISPLAY PLC_NO : " + m_strPlc_No + ", PORT : " + m_nCurPort + " connected");
                                m_blConnectYn = true;
                                break;
                            }
                        }
                        if (m_blConnectYn == true) break;
                    }
                }

                // ---- 메인 폴링 루프 ----
                if (m_disp.m_bSocCon == true && m_disp.m_bDBOpen == true)
                {
                    IsOpen = true;
                    MakeMsg_Imp("DB login Ok!", m_nthNo);
                    Communication("Y", m_strWh_typ, m_strEqmt_typ, m_strPlc_No);

                    while (true)
                    {
                        if (cDefApp.GM_STAT_MAIN == false) goto EXIT_LBL;
                        if (m_bStopReq) goto EXIT_LBL;                  // @.[접속 끊기] 요청

                        m_disp.IsAscii = m_frmMain.IsAscii;
                        m_disp.IsHex = m_frmMain.IsHex;

                        if (!DispManual()) goto EXIT_LBL; // @.Client 수동지령을 먼저 처리
                        if (!DispAuto()) goto EXIT_LBL;   // @.트랙 변경에 따른 자동 표시

                        Thread.Sleep(200);
                    }
                }

            EXIT_LBL:
                SetErrorMsg("Comm" + m_nthNo + " DB & Socket logoff!");
                MakeMsg_Imp("DB & Socket logoff!", m_nthNo);
            }
            catch (Exception ex)
            {
                MakeMsg_Error(ex.Message, m_nthNo);
            }

            IsOpen = false;
            Communication("N", m_strWh_typ, m_strEqmt_typ, m_strPlc_No);
            m_disp.Close(ref m_strRtnMsg);
            MakeMsg_Imp(m_strRtnMsg, m_nthNo);
            m_thThread = null;
        }
        #endregion

        #region 색상 순환
        private byte NextColor(DispState st)
        {
            st.COLOR_IDX = (st.COLOR_IDX + 1) % 3;          // @.빨강 -> 초록 -> 노랑 -> ...
            return (byte)(DisplayProtocol.COLOR_RED + st.COLOR_IDX);
        }
        private DispState GetState(int nDispNo)
        {
            if (!DispDic.ContainsKey(nDispNo)) DispDic.Add(nDispNo, new DispState());
            return DispDic[nDispNo];
        }
        #endregion

        #region DispAuto :: DISPLAY_DATA 를 읽어 적재물이 바뀌면 전송
        private bool DispAuto()
        {
            string strTitle = "[DispAuto]";
            try
            {
                strSql = "";
                strSql += CRLF + "SELECT DISP_NO                          ";
                strSql += CRLF + "      ,COALESCE(DISP_DATA,'') AS DISP_DATA ";
                strSql += CRLF + "      ,COALESCE(LUGG_NO,'')   AS LUGG_NO   ";
                strSql += CRLF + "      ,COALESCE(COLOR,0)      AS COLOR     ";
                strSql += CRLF + "FROM   DISPLAY_DATA                    ";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP                ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO                ";
                strSql += CRLF + "AND    COALESCE(CMD_RQ_YN,'N') <> 'Y'  ";
                strSql += CRLF + "ORDER  BY DISP_NO                       ";

                m_disp._pBdb.mComMain.CommandType = CommandType.Text;
                m_disp._pBdb.mComMain.Parameters.Clear();
                m_disp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_disp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                nSelCnt = m_disp._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    MakeMsg_Error(strTitle + " DISPLAY_DATA select error [" + m_disp._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }
                if (nSelCnt == 0) return true; // @.이 컨트롤러에 등록된 전광판이 없음

                DataTable dt = m_disp._pBdb.mDtMain;
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    int nDispNo = Convert.ToInt32("0" + dt.Rows[r]["DISP_NO"].ToString());
                    string strData = dt.Rows[r]["DISP_DATA"].ToString();
                    string strLugg = dt.Rows[r]["LUGG_NO"].ToString();
                    int nColor = Convert.ToInt32("0" + dt.Rows[r]["COLOR"].ToString());

                    DispState st = GetState(nDispNo);
                    if (st.LAST_LUGG == strLugg) continue; // @.변경 없으면 건너뜀

                    byte byColor;
                    string strSend;
                    if (strLugg == "" || strLugg == "0")
                    {
                        strSend = "";                 // @.빈 트랙 -> 공백 8자리
                        byColor = DisplayProtocol.COLOR_YELLOW;
                    }
                    else
                    {
                        strSend = strData;
                        byColor = (nColor >= DisplayProtocol.COLOR_RED && nColor <= DisplayProtocol.COLOR_YELLOW)
                                  ? (byte)nColor : NextColor(st);
                    }

                    string msg = "";
                    if (m_disp.SendDisplay(nDispNo - 1, byColor, strSend, ref msg))
                    {
                        st.LAST_LUGG = strLugg;
                        UpdateSentStatus(nDispNo, DisplayProtocol.FitProduct(strSend), strLugg);
                        MakeMsg_Imp(string.Format("DISP[{0}] LUGG[{1}] DATA[{2}] sent", nDispNo, strLugg, strSend), m_nthNo);
                    }
                    else
                    {
                        MakeMsg_Error(string.Format("DISP[{0}] send fail [{1}]", nDispNo, msg), m_nthNo);
                        return false; // @.통신 끊김 -> 재접속
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + " Exception [" + ex.Message + "]", m_nthNo);
                return false;
            }
        }
        #endregion

        #region DispManual :: DISPLAY_DATA 의 CMD_RQ_YN='Y' 행 처리 (Client 수동지령)
        private bool DispManual()
        {
            string strTitle = "[DispManual]";
            try
            {
                strSql = "";
                strSql += CRLF + "SELECT DISP_NO                              ";
                strSql += CRLF + "      ,COALESCE(CMD_RQ_ID,'DATA') AS CMD_RQ_ID ";
                strSql += CRLF + "      ,COALESCE(CMD_DATA,'')      AS CMD_DATA  ";
                strSql += CRLF + "      ,COALESCE(CMD_COLOR,6)      AS CMD_COLOR ";
                strSql += CRLF + "FROM   DISPLAY_DATA                        ";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP                    ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO                    ";
                strSql += CRLF + "AND    CMD_RQ_YN = 'Y'                     ";
                strSql += CRLF + "ORDER  BY DISP_NO                           ";

                m_disp._pBdb.mComMain.CommandType = CommandType.Text;
                m_disp._pBdb.mComMain.Parameters.Clear();
                m_disp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_disp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                nSelCnt = m_disp._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    MakeMsg_Error(strTitle + " DISPLAY_DATA cmd select error [" + m_disp._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }
                if (nSelCnt == 0) return true;

                DataTable dt = m_disp._pBdb.mDtMain;
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    int nDispNo = Convert.ToInt32("0" + dt.Rows[r]["DISP_NO"].ToString());
                    string strCmd = dt.Rows[r]["CMD_RQ_ID"].ToString().ToUpper();
                    string strData = dt.Rows[r]["CMD_DATA"].ToString();
                    int nColor = Convert.ToInt32("0" + dt.Rows[r]["CMD_COLOR"].ToString());

                    if (strCmd == "CLEAR") strData = "";
                    byte byColor = (nColor >= DisplayProtocol.COLOR_RED && nColor <= DisplayProtocol.COLOR_YELLOW)
                                   ? (byte)nColor : DisplayProtocol.COLOR_YELLOW;

                    string msg = "";
                    if (m_disp.SendDisplay(nDispNo - 1, byColor, strData, ref msg))
                    {
                        ClearManualCmd(nDispNo, DisplayProtocol.FitProduct(strData), byColor);
                        GetState(nDispNo); // @.상태 객체가 없으면 만들어 둔다
                        MakeMsg_Imp(string.Format("DISP[{0}] MANUAL[{1}] DATA[{2}] sent", nDispNo, strCmd, strData), m_nthNo);
                    }
                    else
                    {
                        MakeMsg_Error(string.Format("DISP[{0}] manual send fail [{1}]", nDispNo, msg), m_nthNo);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + " Exception [" + ex.Message + "]", m_nthNo);
                return false;
            }
        }
        #endregion

        #region DB 갱신
        private bool UpdateSentStatus(int nDispNo, string strSentData, string strLugg)
        {
            try
            {
                m_disp._pBdb.BeginTrans();
                strSql = "";
                strSql += CRLF + "UPDATE DISPLAY_DATA                          ";
                strSql += CRLF + "   SET SEND_YN        = 'Y'                   ";
                strSql += CRLF + "      ,LAST_SENT_DATA = :LAST_SENT_DATA       ";
                strSql += CRLF + "      ,LAST_SENT_LUGG = :LAST_SENT_LUGG       ";
                strSql += CRLF + "      ,SEND_DT        = " + DbLang.SYSDATE + "";
                strSql += CRLF + "      ,UPD_DT         = " + DbLang.SYSDATE + "";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP                       ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO                       ";
                strSql += CRLF + "AND    DISP_NO = :DISP_NO                       ";

                m_disp._pBdb.mComMain.CommandType = CommandType.Text;
                m_disp._pBdb.mComMain.Parameters.Clear();
                m_disp._pBdb.mComMain.Parameters.Add("LAST_SENT_DATA", DbLang.VARCHAR, 255).Value = strSentData;
                m_disp._pBdb.mComMain.Parameters.Add("LAST_SENT_LUGG", DbLang.VARCHAR, 255).Value = strLugg;
                m_disp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_disp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_disp._pBdb.mComMain.Parameters.Add("DISP_NO", DbLang.VARCHAR, 255).Value = nDispNo.ToString();
                nSelCnt = m_disp._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0) { m_disp._pBdb.Rollback(); return false; }
                m_disp._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_disp._pBdb.Rollback();
                SetErrorMsg("[UpdateSentStatus] " + ex.Message);
                return false;
            }
        }

        private bool ClearManualCmd(int nDispNo, string strData, int nColor)
        {
            try
            {
                m_disp._pBdb.BeginTrans();
                strSql = "";
                strSql += CRLF + "UPDATE DISPLAY_DATA                          ";
                strSql += CRLF + "   SET CMD_RQ_YN      = 'N'                   ";
                strSql += CRLF + "      ,DISP_DATA      = :DISP_DATA            ";
                strSql += CRLF + "      ,COLOR          = :COLOR                ";
                strSql += CRLF + "      ,SEND_YN        = 'Y'                   ";
                strSql += CRLF + "      ,LAST_SENT_DATA = :DISP_DATA            ";
                strSql += CRLF + "      ,SEND_DT        = " + DbLang.SYSDATE + "";
                strSql += CRLF + "      ,UPD_DT         = " + DbLang.SYSDATE + "";
                strSql += CRLF + "WHERE  WH_TYP = :WH_TYP                       ";
                strSql += CRLF + "AND    PLC_NO = :PLC_NO                       ";
                strSql += CRLF + "AND    DISP_NO = :DISP_NO                       ";

                m_disp._pBdb.mComMain.CommandType = CommandType.Text;
                m_disp._pBdb.mComMain.Parameters.Clear();
                m_disp._pBdb.mComMain.Parameters.Add("DISP_DATA", DbLang.VARCHAR, 255).Value = strData;
                // @.COLOR 는 INTEGER 컬럼이다. Varchar 파라미터로 넘기면 거부당하므로(42804) INT 로 바인딩한다.
                m_disp._pBdb.mComMain.Parameters.Add("COLOR", DbLang.INT).Value = nColor;
                m_disp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_disp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlc_No;
                m_disp._pBdb.mComMain.Parameters.Add("DISP_NO", DbLang.VARCHAR, 255).Value = nDispNo.ToString();
                nSelCnt = m_disp._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0) { m_disp._pBdb.Rollback(); return false; }
                m_disp._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                m_disp._pBdb.Rollback();
                SetErrorMsg("[ClearManualCmd] " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Communication :: EQP_MST 접속상태 기록  (CvThread 와 동일)
        public bool Communication(string CONNECTED_YN, string WH_TYP, string EQP_TYP, string PLC_NO)
        {
            string strTitle = "[Communication]";
            try
            {
                m_disp._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "UPDATE EQP_MST                                    ";
                strSql += CRLF + "   SET CONNECTED_YN      = :CONNECTED_YN          ";
                strSql += CRLF + "      ,UPD_DT            = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      ,PLC_PORT          = :PLC_PORT              ";
                strSql += CRLF + "WHERE  WH_TYP            = :WH_TYP                ";
                strSql += CRLF + "AND    EQP_TYP           = :EQP_TYP               ";
                strSql += CRLF + "AND    PLC_NO            = :PLC_NO                ";

                m_disp._pBdb.mComMain.CommandType = CommandType.Text;
                m_disp._pBdb.mComMain.Parameters.Clear();
                m_disp._pBdb.mComMain.Parameters.Add("CONNECTED_YN", DbLang.VARCHAR).Value = CONNECTED_YN;
                m_disp._pBdb.mComMain.Parameters.Add("PLC_PORT", DbLang.VARCHAR, 255).Value = Convert.ToString("" + m_nCurPort);
                m_disp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = WH_TYP;
                m_disp._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = EQP_TYP;
                m_disp._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = PLC_NO;
                nSelCnt = m_disp._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    m_disp._pBdb.Rollback();
                    MakeMsg_Error(strTitle + " EQP_MST update error [" + m_disp._pBdb.ErrMsg + "]", m_nthNo);
                    return false;
                }
                m_disp._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                try { m_disp._pBdb.Rollback(); }
                catch { }
                SetErrorMsg(strTitle + " " + ex.Message);
                return false;
            }
        }
        #endregion

        #region InsertWcsLogPgr :: WCS_LOG_PGR 감사로그  (CvThread 와 동일 컬럼)
        public bool InsertWcsLogPgr(string strTRACK_NO, string strLOG_MSG)
        {
            try
            {
                m_disp._pBdb.BeginTrans();

                strSql = "";
                strSql += cDefApp.CRLF + "INSERT INTO WCS_LOG_PGR (WH_TYP    ";
                strSql += cDefApp.CRLF + "                        ,INS_DT    ";
                strSql += cDefApp.CRLF + "                        ,LOG_SEQ   ";
                strSql += cDefApp.CRLF + "                        ,LUGG_NO   ";
                strSql += cDefApp.CRLF + "                        ,BCR_BOTTOM";
                strSql += cDefApp.CRLF + "                        ,BCR_TOP   ";
                strSql += cDefApp.CRLF + "                        ,PGR_NM    ";
                strSql += cDefApp.CRLF + "                        ,LOG_KOR   ";
                strSql += cDefApp.CRLF + "                        ,TRACK_FROM";
                strSql += cDefApp.CRLF + "                        ,TRACK_TO  ";
                strSql += cDefApp.CRLF + "                        ,JOB_STA   ";
                strSql += cDefApp.CRLF + "                        ,RQ_INS_ID ";
                strSql += cDefApp.CRLF + "                        ,RQ_INS_DT ";
                strSql += cDefApp.CRLF + "                        ,EQP_TYP ) ";
                strSql += cDefApp.CRLF + "    VALUES (:WH_TYP                ";
                strSql += cDefApp.CRLF + "           ," + DbLang.SYSDATE + " ";
                strSql += cDefApp.CRLF + "           ,NEXTVAL('LOG_SEQ')     ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,:PGR_NM                ";
                strSql += cDefApp.CRLF + "           ,:LOG_KOR               ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,NULL                   ";
                strSql += cDefApp.CRLF + "           ,:JOB_STA               ";
                strSql += cDefApp.CRLF + "           ,:RQ_INS_ID             ";
                strSql += cDefApp.CRLF + "           ," + DbLang.SYSDATE + " ";
                strSql += cDefApp.CRLF + "           ,:EQP_TYP )             ";

                m_disp._pBdb.mComMain.CommandType = CommandType.Text;
                m_disp._pBdb.mComMain.Parameters.Clear();
                m_disp._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWh_typ;
                m_disp._pBdb.mComMain.Parameters.Add("PGR_NM", DbLang.VARCHAR, 255).Value = m_strLogFileNm;
                m_disp._pBdb.mComMain.Parameters.Add("LOG_KOR", DbLang.VARCHAR, 255).Value = strLOG_MSG;
                m_disp._pBdb.mComMain.Parameters.Add("JOB_STA", DbLang.VARCHAR, 255).Value = "999";
                m_disp._pBdb.mComMain.Parameters.Add("RQ_INS_ID", DbLang.VARCHAR, 255).Value = strTRACK_NO;
                m_disp._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = m_strEqmt_typ;
                nSelCnt = m_disp._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    m_disp._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nthNo + " :[InsertWcsLogPgr] error [" + m_disp._pBdb.ErrMsg + "]");
                    return false;
                }
                m_disp._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                try { m_disp._pBdb.Rollback(); }
                catch { }
                SetErrorMsg("Comm" + m_nthNo + " :[InsertWcsLogPgr] " + ex.ToString());
                return false;
            }
        }
        #endregion
    }
}
