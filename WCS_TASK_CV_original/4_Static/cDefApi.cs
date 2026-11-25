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

        /*
         * GsGetAutoModeStyle :: 자동/수동 표시를 어느 형식으로 읽을지.
         *
         *   WCS_DB.INI [DEVICEMAP] AUTO_MODE_STYLE
         *     NEW : 현재 형식. DeviceMap 의 Auto 영역을 그대로 쓴다. (8호기는 D207.0 한 비트)
         *     OLD : 구 ECS(C++ EcsSv) 형식. 8호기 자동/수동을 D203 의 OpBox 비트로 읽는다.
         *
         *   DeviceMap 의 <Area> 에 style 속성이 붙어 있으면 이 값과 같은 것만 읽는다.
         *   속성이 없는 영역은 형식과 무관하게 항상 읽는다.
         *   키가 없으면 NEW 다. (지금까지 돌던 방식)
         */
        /*
         * 작업번호 워드 인코딩 (WCS_DB.INI [CNF] LUGG_WORD_ENC)
         *
         *   DEC100 : 원본 ECS(EcsSv/Cv.cpp CCv::WriteTrackInfo) 와 같다.
         *            상위 바이트 = 작업번호 / 100, 하위 바이트 = 작업번호 % 100.
         *            1234 -> 0x0C22(3106). 현장 PLC 는 이 형식이다. 키가 없으면 이것이다.
         *   BIN    : 작업번호를 16비트 이진수 그대로. 시뮬레이터(CvSim)는 이 형식이다.
         */
        /// <summary>
        /// 1층 출고 루프 화물 수(D570) 읽기 사용 여부. WCS_DB.INI [CNF] RET_CNT_READ (Y/N). 키가 없으면 Y.
        /// PLC 02/05 에서만 쓴다. 현장에서 D570 읽기가 의심될 때 N 으로 끄고 비교한다. (2026-11-02)
        /// </summary>
        /// <summary>
        /// 설비 접속정보 출처. WCS_DB.INI [CNF] COMM_INFO = DB(기본) | INI.
        /// INI 면 10-19 빌드처럼 [COMMn] 의 IP / CUR_PORT / FROM_PORT / TO_PORT / PORT_CNT 를 쓰고 EQP_MST 는 보지 않는다. (2026-11-02)
        /// </summary>
        public static bool GsIsCommInfoIni()
        {
            try
            {
                StringBuilder sb = new StringBuilder(32);
                GetPrivateProfileString("CNF", "COMM_INFO", "DB", sb, sb.Capacity, cDefApp.GM_ENV_INI);
                return (sb.ToString().Trim().ToUpper() == "INI");
            }
            catch
            {
                return false;
            }
        }

        public static bool GsIsRetCntRead()
        {
            try
            {
                StringBuilder sb = new StringBuilder(32);
                GetPrivateProfileString("CNF", "RET_CNT_READ", "Y", sb, sb.Capacity, cDefApp.GM_ENV_INI);
                return (sb.ToString().Trim().ToUpper() != "N");
            }
            catch
            {
                return true;
            }
        }

        public static bool GsIsLuggWordDec100()
        {
            try
            {
                StringBuilder sb = new StringBuilder(32);
                GetPrivateProfileString("CNF", "LUGG_WORD_ENC", "DEC100", sb, sb.Capacity, cDefApp.GM_ENV_INI);
                return (sb.ToString().Trim().ToUpper() != "BIN");
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 가상 트랙 목록. WCS_DB.INI [COMMn] VIRTUAL_TRACK=2101:267,... ("mc_no:빌리는 트랙번호")
        /// 시뮬레이터용. 키가 없으면 빈 문자열.
        /// </summary>
        public static string GsGetVirtualTracks(string strSection)
        {
            try
            {
                StringBuilder sb = new StringBuilder(256);
                GetPrivateProfileString(strSection, "VIRTUAL_TRACK", "", sb, sb.Capacity, cDefApp.GM_ENV_INI);
                return sb.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }

        public static string GsGetAutoModeStyle()
        {
            try
            {
                StringBuilder sb = new StringBuilder(32);
                GetPrivateProfileString("DEVICEMAP", "AUTO_MODE_STYLE", "NEW", sb, sb.Capacity, cDefApp.GM_ENV_INI);

                string strStyle = sb.ToString().Trim().ToUpper();
                return (strStyle == "OLD") ? "OLD" : "NEW";
            }
            catch
            {
                return "NEW";
            }
        }

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

        /*
         * GsReadComWhTyp :: 그 COMM 이 맡은 창고.
         *
         *   [COMMn] 에 WH_TYP 가 있으면 그것을, 없으면 [CNF] 의 값을 쓴다.
         *   한 프로세스가 여러 설비를 맡는데 창고가 갈린 현장이 있다.
         *   이 현장은 P-BOX(크레인 11호기, CV PLC 04)만 WH_TYP 20 이다.
         */
        public static string GsReadComWhTyp(string pAppNm, string pDefault)
        {
            StringBuilder sb = new StringBuilder(32);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI)) return pDefault;

            GetPrivateProfileString(pAppNm, "WH_TYP", "", sb, sb.Capacity, cDefApp.GM_ENV_INI);
            string strVal = sb.ToString().Trim();

            return (strVal == "") ? pDefault : strVal;
        }
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


        #region [EQP_MST]::설비 접속정보를 DB 에서 읽고 쓴다 (2026-10-20)
        //  INI 의 IP / CUR_PORT / FROM_PORT / TO_PORT / PORT_CNT 는 더 이상 쓰지 않는다. (INI 에는 주석 처리)
        //  EQP_MST.PLC_IP, PLC_PORT(현재 접속 포트), PLC_PORT_FROM / PLC_PORT_TO(포트 범위) 를 쓴다.
        //  PLC_PORT 가 FROM~TO 범위 안이면 범위 안에서 포트를 바꿔 가며 접속하고, 아니면 PLC_PORT 하나만 쓴다.
        public static string GsComDbConnString()
        {
            string strConn = "host=" + cDefApp.GM_DB2_IP + ";username=" + cDefApp.GM_DB2_USER + ";password=" + cDefApp.GM_DB2_USER_PW
                           + ";database=" + cDefApp.GM_DB2_DATABASE + ";Timeout=5;CommandTimeout=10;";
            if (cDefApp.GM_DB2_PORT != null && cDefApp.GM_DB2_PORT != "")
                strConn += "port=" + cDefApp.GM_DB2_PORT + ";";
            return strConn;
        }

        public static bool GsReadComFromDb(string pWhTyp, string pEqType, string pPlcNo,
                                           ref string pIp, ref int pCur, ref int pFrom, ref int pTo, ref int pPortCnt,
                                           ref string pMsg)
        {
            try
            {
                using (Npgsql.NpgsqlConnection cn = new Npgsql.NpgsqlConnection(GsComDbConnString()))
                {
                    cn.Open();
                    using (Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand(
                        "SELECT PLC_IP, PLC_PORT, PLC_PORT_FROM, PLC_PORT_TO FROM EQP_MST " +
                        " WHERE WH_TYP = :w AND EQP_TYP = :t AND PLC_NO = :p", cn))
                    {
                        cmd.Parameters.AddWithValue("w", pWhTyp == null ? "" : pWhTyp.Trim());
                        cmd.Parameters.AddWithValue("t", pEqType == null ? "" : pEqType.Trim());
                        cmd.Parameters.AddWithValue("p", pPlcNo == null ? "" : pPlcNo.Trim());
                        using (Npgsql.NpgsqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read())
                            {
                                pMsg = "[GsReadComFromDb]EQP_MST 에 접속정보가 없습니다. WH_TYP[" + pWhTyp + "] EQP_TYP[" + pEqType + "] PLC_NO[" + pPlcNo + "]";
                                return false;
                            }
                            string strIp = ("" + rd[0]).Trim();
                            int nCur = 0, nFrom = 0, nTo = 0;
                            int.TryParse(("" + rd[1]).Trim(), out nCur);
                            int.TryParse(("" + rd[2]).Trim(), out nFrom);
                            int.TryParse(("" + rd[3]).Trim(), out nTo);
                            bool bRange = (nFrom > 0 && nTo >= nFrom);
                            if (strIp == "" || (nCur <= 0 && !bRange))
                            {
                                pMsg = "[GsReadComFromDb]EQP_MST 의 PLC_IP / PLC_PORT 가 비어 있습니다. WH_TYP[" + pWhTyp + "] EQP_TYP[" + pEqType + "] PLC_NO[" + pPlcNo + "]";
                                return false;
                            }
                            pIp = strIp;
                            // @.INI 와 같은 규칙으로 (2026-11-02) : 범위(FROM~TO)가 있으면 항상 범위를 쓴다. PLC_PORT 는 범위 안일 때만 시작 포트,
                            //   범위 밖이면 FROM 부터 시작한다. 범위가 없을 때만 PLC_PORT 하나를 쓴다.
                            //   전에는 PLC_PORT 가 범위 밖이면 PLC_PORT 하나만 썼다. 현장 EQP_MST 가 PLC_PORT 7000 / 범위 7001~7003 이라
                            //   DB 모드가 7000 으로 붙어 2호기 PLC 의 다른 접속 채널을 읽었다. (INI 모드 CUR_PORT 7001~7003 은 정상)
                            if (bRange)
                            {
                                pFrom = nFrom; pTo = nTo;
                                pCur = (nCur >= nFrom && nCur <= nTo) ? nCur : nFrom;
                                pPortCnt = 1;
                            }
                            else
                            {
                                pCur = nCur; pFrom = nCur; pTo = nCur; pPortCnt = 1;
                            }
                        }
                    }
                }
                pMsg = "[GsReadComFromDb]Sucess";
                return true;
            }
            catch (Exception ex)
            {
                pMsg = "[GsReadComFromDb]Error::" + ex.Message;
                return false;
            }
        }

        // 접속에 성공한 포트를 DB 에 기록한다. (예전에는 INI 의 CUR_PORT 에 썼다)
        public static void GsWriteCurPortToDb(string pWhTyp, string pEqType, string pPlcNo, int pPort)
        {
            try
            {
                using (Npgsql.NpgsqlConnection cn = new Npgsql.NpgsqlConnection(GsComDbConnString()))
                {
                    cn.Open();
                    using (Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand(
                        "UPDATE EQP_MST SET PLC_PORT = :port WHERE WH_TYP = :w AND EQP_TYP = :t AND PLC_NO = :p AND COALESCE(PLC_PORT, '') <> :port", cn))
                    {
                        cmd.Parameters.AddWithValue("port", pPort.ToString());
                        cmd.Parameters.AddWithValue("w", pWhTyp == null ? "" : pWhTyp.Trim());
                        cmd.Parameters.AddWithValue("t", pEqType == null ? "" : pEqType.Trim());
                        cmd.Parameters.AddWithValue("p", pPlcNo == null ? "" : pPlcNo.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
            }
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
        // @@@.GsGetCommInt : WCS_DB.INI [COMM<n>] 의 정수 값 (2026-11-15)
        public static int GsGetCommInt(int nThNo, string strKey, int nDefault)
        {
            return GetPrivateProfileInt("COMM" + nThNo, strKey, nDefault, cDefApp.GM_ENV_INI);
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
