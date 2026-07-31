using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WCS_TASK_CV
{
    class cDefApi
    {
        // @@@.INI파일에서 정수형 데이터를 읽어옴.
        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileInt(string lpApplicationName, string lpKeyName, int nDefault, string lpFileName);

        // @@@.INI파일에서 문자형 데이터를 읽어옴.
        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        // @@@. INI파일에 쓰기.
        [DllImport("kernel32.dll")]
        public static extern uint WritePrivateProfileString(string section, string key, string val, string filePath);

        // @@@.GsGetInitPorFileDB
        public static void GsGetInitPorFileDB(ref string pHost,
                                              ref string pServiceName,
                                              ref string pUserID,
                                              ref string pPassword,
                                              ref string pPort,
                                              ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFileDB]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("DB", "HOST", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pHost = sb.ToString();

                GetPrivateProfileString("DB", "SERVICENAME", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pServiceName = sb.ToString();

                GetPrivateProfileString("DB", "USERID", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUserID = sb.ToString();

                GetPrivateProfileString("DB", "PASSWORD", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pPassword = sb.ToString();

                GetPrivateProfileString("DB", "PORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pPort = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileDB]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileDB]Error::" + pRtnMsg;
        }

        #region [DB_TYPE]::DB 종류 접속정보
        public static void GsGetInitPorFileDB_TYPE(ref string pTYPE,
                                                   ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFileDB_TYPE]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("DB_TYPE", "TYPE", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pTYPE = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileDB_TYPE]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileDB_TYPE]Error::" + pRtnMsg;
        }
        #endregion
        
        #region [DB_1]::Oracle 접속정보
        public static void GsGetInitPorFileDB_1(ref string pProvider,
                                              ref string pAlias,
                                              ref string pUserID,
                                              ref string pPassword,
                                              ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFileDB_1]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("DB_1", "PROVIDER", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pProvider = sb.ToString();

                GetPrivateProfileString("DB_1", "ALIAS", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pAlias = sb.ToString();

                GetPrivateProfileString("DB_1", "USERID", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUserID = sb.ToString();

                GetPrivateProfileString("DB_1", "PASSWORD", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pPassword = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileDB_1]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileDB]Error::" + pRtnMsg;
        }
        #endregion

        #region [DB_2]::PostgreSql 접속정보
        public static void GsGetInitPorFileDB_2(ref string pIP,
                                              ref string pDATABASE,
                                              ref string pPORT,
                                              ref string pUSER,
                                              ref string pUSER_PW,
                                              ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFileDB_2]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("DB_2", "IP", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pIP = sb.ToString();

                GetPrivateProfileString("DB_2", "DATABASE", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pDATABASE = sb.ToString();

                GetPrivateProfileString("DB_2", "PORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pPORT = sb.ToString();

                GetPrivateProfileString("DB_2", "USER", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUSER = sb.ToString();

                GetPrivateProfileString("DB_2", "USER_PW", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUSER_PW = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileDB_2]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileDB_2]Error::" + pRtnMsg;
        }
        #endregion

        #region [CNF]::접속정보
        public static void GsGetInitPorFileCNF(ref string pGRP,
                                              ref string pUSERID,
                                              ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFileCNF]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("CNF", "WH_TYP", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pGRP = sb.ToString();

                GetPrivateProfileString("CNF", "USERID", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUSERID = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileCNF]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileCNF]Error::" + pRtnMsg;
        }
        #endregion

        #region [COMM]::설비통신 접속정보
        public static bool GsReadInitProfileCom(string pAppNm,
                                            ref string pGrpNo,
                                            ref string pCommIP,
                                            ref    int pComCurPort,
                                            ref    int pComFromPort,
                                            ref    int pComToPort,
                                            ref    int pComPortCnt,
                                            ref    int pCnt,
                                            ref    int i,
                                            ref    int pFrTrack,
                                            ref    int pToTrack,
                                            ref string pLogPath,
                                            ref string pLogFileNm,
                                            ref string pEqmt,
                                            ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsReadInitProfileCom]::Not Found File";
                return false;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString(pAppNm, "EQMT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pEqmt = sb.ToString();

                GetPrivateProfileString(pAppNm, "PLC_NO", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pGrpNo = sb.ToString();

                GetPrivateProfileString(pAppNm, "IP", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pCommIP = sb.ToString();

                pComCurPort = GetPrivateProfileInt(pAppNm, "CUR_PORT", pComCurPort, cDefApp.GM_ENV_INI);

                pComFromPort = GetPrivateProfileInt(pAppNm, "FROM_PORT", pComFromPort, cDefApp.GM_ENV_INI);

                pComToPort = GetPrivateProfileInt(pAppNm, "TO_PORT", pComToPort, cDefApp.GM_ENV_INI);

                pCnt = GetPrivateProfileInt(pAppNm, "CNT", pCnt, cDefApp.GM_ENV_INI);

                pComPortCnt = GetPrivateProfileInt(pAppNm, "PORT_CNT", pCnt, cDefApp.GM_ENV_INI);

                pFrTrack = GetPrivateProfileInt(pAppNm, "FR_TRACK", 1, cDefApp.GM_ENV_INI);

                pToTrack = GetPrivateProfileInt(pAppNm, "TO_TRACK", 1, cDefApp.GM_ENV_INI);

                GetPrivateProfileString(pAppNm, "LOG_PATH", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pLogPath = sb.ToString();

                GetPrivateProfileString(pAppNm, "FILENAME", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pLogFileNm = sb.ToString();
                
                pRtnMsg = "[GsReadInitProfileCom]::Sucess";
                return true;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsReadInitProfileCom]Error::" + pRtnMsg;
            return false;
        }
        #endregion

        // @@@.GsReadInitProfileProcessCnt
        public static bool GsReadInitProfileProcessCnt(string pAppNm, ref int pProcessCnt, ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsReadInitProfileProcessCnt]::Not Found File";
                return false;
            }

            try
            {
                pRtnMsg = "";

                pProcessCnt = GetPrivateProfileInt(pAppNm, "CNT", 1, cDefApp.GM_ENV_INI);

                pRtnMsg = "[GsReadInitProfileProcessCnt]::Sucess";
                return true;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsReadInitProfileProcessCnt]Error::" + pRtnMsg;
            return false;
        }
        // @@@.GsReadInitProfileDelay
        public static void GsReadInitProfileDelay(string pDelayTyp, ref int pDelay, ref string pRtnMsg)
        {
            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsReadInitProfileDelay]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";
                pDelay = GetPrivateProfileInt("DELAY", pDelayTyp, pDelay, cDefApp.GM_ENV_INI);
                pRtnMsg = "[GsReadInitProfileDelay]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsReadInitProfileDelay]Error::" + pRtnMsg;
        }
        // @@@.GsReadInitProfileCom
        public static bool GsReadInitProfileTrack(string pAppNm,
                                            ref int pIdx,
                                            ref int pFrTrack,
                                            ref int pToTrack,
                                            ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsReadInitProfileTrack]::Not Found File";
                return false;
            }

            try
            {
                pRtnMsg = "";
                pFrTrack = GetPrivateProfileInt(pAppNm, "FR_TRACK_" + pIdx.ToString("000"), 1, cDefApp.GM_ENV_INI);
                pToTrack = GetPrivateProfileInt(pAppNm, "TO_TRACK_" + pIdx.ToString("000"), 1, cDefApp.GM_ENV_INI);

                pRtnMsg = "[GsReadInitProfileTrack]::Sucess";
                return true;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsReadInitProfileTrack]Error::" + pRtnMsg;
            return false;
        }
    }
}
