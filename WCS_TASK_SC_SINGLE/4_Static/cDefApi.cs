using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WCS_TASK_SC
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

        // @@@.INI파일의 섹션 이름 목록을 읽어옴.
        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, string lpFileName);

        // @@@.INI파일의 섹션 전체(키=값 목록)를 읽어옴.
        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        // @@@.INI파일의 섹션 전체(키=값 목록)를 한번에 씀. (lpString이 null이면 섹션 삭제)
        [DllImport("kernel32.dll")]
        static extern int WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

        // @@@.[COM_SETTING] Setting=1 이면 [COMM0]을 기준으로 [COMM?] 섹션들을 재생성하고 Setting=0으로 저장한다.
        //     Setting=0 이거나 섹션/키가 없으면 아무 일도 하지 않는다.
        public static bool GsComSettingInit(ref string pRtnMsg)
        {
            string strTitle = "[GsComSettingInit] ";

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = strTitle + "Not Found File";
                return false;
            }

            try
            {
                //Setting 값 읽기 (키가 없으면 0 = 아무것도 하지 않음)
                int nSetting = GetPrivateProfileInt("COM_SETTING", "Setting", 0, cDefApp.GM_ENV_INI);

                //[COM_SETTING] 섹션이 없으면 Setting=0 으로 섹션을 만들어 둔다.
                StringBuilder sbChk = new StringBuilder(32);
                GetPrivateProfileString("COM_SETTING", "Setting", null, sbChk, sbChk.Capacity, cDefApp.GM_ENV_INI);
                if (sbChk.ToString() == "")
                {
                    WritePrivateProfileString("COM_SETTING", "Setting", "0", cDefApp.GM_ENV_INI);
                }

                if (nSetting != 1)
                {
                    pRtnMsg = strTitle + "Setting=0, Skip";
                    return true;
                }

                //[COMM0] 섹션 전체 읽기 (key=value 목록)
                byte[] bySec = new byte[32768];
                int nSecLen = GetPrivateProfileSection("COMM0", bySec, bySec.Length, cDefApp.GM_ENV_INI);
                if (nSecLen <= 0)
                {
                    pRtnMsg = strTitle + "[COMM0] 섹션이 없습니다.";
                    return false;
                }
                string strComm0 = Encoding.Default.GetString(bySec, 0, nSecLen);

                //전체 섹션 이름 읽기
                byte[] byNames = new byte[32768];
                int nNameLen = GetPrivateProfileSectionNames(byNames, byNames.Length, cDefApp.GM_ENV_INI);
                string[] strSections = Encoding.Default.GetString(byNames, 0, nNameLen).Split('\0');

                //[COMM0]을 제외한 [COMM숫자] 섹션 모두 삭제
                for (int ii = 0; ii < strSections.Length; ii++)
                {
                    string strSec = strSections[ii].Trim();
                    if (strSec == "" || strSec.ToUpper() == "COMM0") continue;
                    if (!strSec.ToUpper().StartsWith("COMM")) continue;

                    bool bNum = strSec.Length > 4;
                    for (int jj = 4; jj < strSec.Length; jj++)
                    {
                        if (!char.IsDigit(strSec[jj])) { bNum = false; break; }
                    }
                    if (!bNum) continue;

                    WritePrivateProfileString(strSec, null, null, cDefApp.GM_ENV_INI); //섹션 삭제
                }

                //[PROCESS] CNT 읽고 2 이상이면 [COMM0]을 복사하여 [COMM1] ~ [COMM(CNT-1)] 생성
                //(FILENAME은 섹션 번호에 맞게 변경. 예: WCS_TASK_SC_COMM0 -> WCS_TASK_SC_COMM1)
                int nCnt = GetPrivateProfileInt("PROCESS", "CNT", 1, cDefApp.GM_ENV_INI);
                if (nCnt >= 2)
                {
                    string[] strKeyVals = strComm0.TrimEnd('\0').Split('\0');

                    for (int ii = 1; ii < nCnt; ii++)
                    {
                        StringBuilder sbSec = new StringBuilder();
                        for (int jj = 0; jj < strKeyVals.Length; jj++)
                        {
                            string strLine = strKeyVals[jj];

                            if (strLine.ToUpper().StartsWith("FILENAME"))
                            {
                                int nEq = strLine.IndexOf('=');
                                if (nEq > 0)
                                {
                                    string strKey = strLine.Substring(0, nEq).Trim();
                                    string strVal = strLine.Substring(nEq + 1).Trim();

                                    int nPos = strVal.ToUpper().LastIndexOf("COMM0");
                                    if (nPos >= 0)
                                    {
                                        strVal = strVal.Substring(0, nPos) + "COMM" + ii.ToString();
                                    }
                                    else
                                    {
                                        strVal = strVal + "_COMM" + ii.ToString();
                                    }
                                    strLine = strKey + "=" + strVal;
                                }
                            }
                            sbSec.Append(strLine);
                            sbSec.Append('\0');
                        }

                        if (WritePrivateProfileSection("COMM" + ii.ToString(), sbSec.ToString(), cDefApp.GM_ENV_INI) == 0)
                        {
                            pRtnMsg = strTitle + "[COMM" + ii.ToString() + "] 섹션 생성 실패";
                            return false;
                        }
                    }
                }

                //모두 성공하면 Setting=0 으로 저장
                WritePrivateProfileString("COM_SETTING", "Setting", "0", cDefApp.GM_ENV_INI);

                pRtnMsg = strTitle + "Sucess (CNT=" + nCnt.ToString() + ")";
                return true;
            }
            catch (Exception ex)
            {
                pRtnMsg = strTitle + ex.Message;
                return false;
            }
        }

        // @@@.GsGetInitPorFileDB
        public static void GsGetStringInitPorFile(string strAppName,
                                                  string strKeyName,
                                                  ref string strValue,
                                                  ref string strRtnMsg)
        {
            string strTitle = "[GsGetStringInitPorFile] ";

            strValue = "";

            StringBuilder sb = new StringBuilder(1000);
            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                strRtnMsg = strTitle + "Not Found File";
                return;
            }

            try
            {
                strRtnMsg = "";
                GetPrivateProfileString(strAppName, strKeyName, null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                strValue = sb.ToString();
                strRtnMsg = strTitle + "Sucess";
                return;
            }
            catch (Exception ex)
            {
                strRtnMsg = strTitle + ex.Message;
                return;
            }
        }

        // @@@.GsGetInitPorFileDB
        public static void GsGetIntInitPorFile(string strAppName,
                                              string strKeyName,
                                              ref int nValue,
                                              ref string strRtnMsg)
        {
            string strTitle = "[GsGetIntInitPorFile] ";

            nValue = 0;
            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                strRtnMsg = strTitle + "Not Found File";
                return;
            }

            try
            {
                strRtnMsg = "";
                nValue = GetPrivateProfileInt(strAppName, strKeyName, nValue, cDefApp.GM_ENV_INI);
                strRtnMsg = strTitle + "Sucess";
                return;
            }
            catch (Exception ex)
            {
                strRtnMsg = strTitle + ex.Message;
                return;
            }
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

        #region [PROCESS]::CNT 가져오기
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
        #endregion

        #region [COMM]::설비통신 접속정보
        public static bool GsReadInitProfileCom(string pAppNm,
                                            ref string pGrpNo,
                                            ref string pCommIP,
                                            ref string pComCurPort,
                                            ref string pComFromPort,
                                            ref string pComToPort,
                                            ref    int i,
                                            ref string pLogPath,
                                            ref string pLogFileNm,
                                            ref string pEqmt,
                                            ref string pScNo,
                                            ref string pMcNo,
                                            ref string pScGrpNo,
                                            ref int    pPortCnt,
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

                GetPrivateProfileString(pAppNm, "IP", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pCommIP = sb.ToString();

                GetPrivateProfileString(pAppNm, "CUR_PORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pComCurPort = sb.ToString();

                GetPrivateProfileString(pAppNm, "FROM_PORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pComFromPort = sb.ToString();

                GetPrivateProfileString(pAppNm, "TO_PORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pComToPort = sb.ToString();

                GetPrivateProfileString(pAppNm, "PLC_NO", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pGrpNo = sb.ToString();

                GetPrivateProfileString(pAppNm, "SC_NO", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pScNo = sb.ToString();

                GetPrivateProfileString(pAppNm, "MC_NO", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pMcNo = sb.ToString();

                GetPrivateProfileString(pAppNm, "SC_GRP_NO", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pScGrpNo = sb.ToString();

                pPortCnt = GetPrivateProfileInt(pAppNm, "PORT_CNT", pPortCnt, cDefApp.GM_ENV_INI);

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

    }
}
