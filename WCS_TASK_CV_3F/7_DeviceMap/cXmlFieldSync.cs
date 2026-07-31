using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Npgsql;

namespace WCS_TASK_CV
{
    // @@@@.CvSim의 KeyWord.xml / DeviceMap??.xml 을 파싱하여
    //      CV_DATA 테이블 필드를 XML 정의에 맞게 동기화하는 클래스
    public class cXmlFieldSync
    {
        #region@@@.파싱 결과 항목
        public class XmlField
        {
            public string Name;        // XML 상의 이름 (예: LuggNum)
            public string Parent;      // 부모 (예: D5n+0, StatusData)
            public string DbColumn;    // 매핑된 CV_DATA 컬럼명
            public bool   Required;    // KeyWord.xml 유래 여부(필수)
            public string Source;      // 출처 파일명
        }
        #endregion

        #region@@@.KeyWord 이름 → CV_DATA 컬럼 매핑 (CvSim Dictionary 의미 기준)
        // KeyWord.xml의 이름들은 CV_TASK가 이미 사용중인 CV_DATA 컬럼과 1:1 매핑됨
        private static readonly Dictionary<string, string> KNOWN_MAP
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "LuggNum",       "LUGG_NO_RD"            },
            { "DestPos",       "DEST_POS_RD"           },
            { "JobType",       "JOB_TYP_RD"            },
            { "SensorData",    "SENSOR0_DATA_RD"       },
            { "ErrorCode",     "ERROR_CODE"            },
            { "ProductSensor", "PULP_SENSOR_RD"        },
            { "Auto",          "AUTO_MODE_RD"          },
            { "StoStation",    "STO_READY_RD"          },
            { "RetStation",    "RET_READY_RD"          },
            { "ScStoHS",       "STOHS_READY_RD"        },
            { "ScRetHS",       "RETHS_READY_RD"        },
            { "RtvDepartHS",   "RTV_DEPARTHS_READY_RD" },
            { "RtvArriveHS",   "RTV_ARRIVEHS_READY_RD" },
            { "LgvDepartHS",   "LGV_DEPARTHS_READY_RD" },
            { "LgvArriveHS",   "LGV_ARRIVEHS_READY_RD" },
            { "Emergency",     "EMERGENCY_RD"          },
            { "CvCheckPos",    "CV_CHECK_POS_RD"       },
            { "OverTime",      "OVER_TIME_RD"          },
            { "OverLoad",      "OVER_LOAD_RD"          },
            { "WaitTime",      "WAIT_TIME_RD"          },
            { "TrPause",       "TR_PAUSE_RD"           },
        };
        #endregion

        public List<XmlField> Fields = new List<XmlField>();
        public string ErrMsg = "";

        #region@@@.이름 → DB 컬럼명 변환
        // 알려진 매핑이 없으면 CamelCase → UPPER_SNAKE + "_RD"
        public static string ToDbColumn(string strName)
        {
            if (KNOWN_MAP.ContainsKey(strName)) return KNOWN_MAP[strName];

            // 주소형 이름(D5n+4 등)은 컬럼화: D5N_4_RD
            string s = Regex.Replace(strName, @"[^A-Za-z0-9]+", "_");
            s = Regex.Replace(s, "([a-z0-9])([A-Z])", "$1_$2").ToUpper();
            s = s.Trim('_');
            if (s.Length == 0) return "";
            if (char.IsDigit(s[0])) s = "F_" + s;
            if (!s.EndsWith("_RD")) s += "_RD";
            return s;
        }
        #endregion

        #region@@@.KeyWord.xml 파싱 [필수 필드]
        public bool ParseKeyWordXml(string strPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(strPath);

                XmlNodeList nodes = doc.SelectNodes("/KeyWords/KeyWord");
                if (nodes == null || nodes.Count == 0)
                {
                    ErrMsg = "KeyWord.xml 에서 KeyWord 노드를 찾을 수 없습니다.";
                    return false;
                }

                string strFile = System.IO.Path.GetFileName(strPath);
                foreach (XmlNode node in nodes)
                {
                    string strName = GetAttr(node, "name");
                    if (strName == "") continue;

                    AddField(strName, GetAttr(node, "parent"), true, strFile);
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrMsg = "KeyWord.xml 파싱 오류: " + ex.Message;
                return false;
            }
        }
        #endregion

        #region@@@.DeviceMap??.xml 파싱 [추가 필드]
        public bool ParseDeviceMapXml(string strPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(strPath);

                string strFile = System.IO.Path.GetFileName(strPath);

                // 1) TrackingArea 하위 KeyWord
                XmlNodeList nodes = doc.SelectNodes("/DeviceMap/TrackingArea/KeyWord");
                if (nodes != null)
                    foreach (XmlNode node in nodes)
                        AddField(node.InnerText.Trim(), "TrackingArea", false, strFile);

                // 2) TrackDatas 하위 Word 및 그 자식 (ByteL/ByteH/Nibble/Bit)
                nodes = doc.SelectNodes("/DeviceMap/TrackDatas/Word");
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        string strName = GetAttr(node, "name");
                        string strAddr = GetAttr(node, "addr");
                        AddField(strName, strAddr, false, strFile);

                        foreach (XmlNode child in node.ChildNodes)
                        {
                            if (child.NodeType != XmlNodeType.Element) continue;
                            string strSub = GetAttr(child, "name");
                            AddField(strSub, strName, false, strFile);
                        }
                    }
                }

                // 3) SeparatelyETC 하위 Area
                nodes = doc.SelectNodes("/DeviceMap/SeparatelyETC/Area");
                if (nodes != null)
                    foreach (XmlNode node in nodes)
                        AddField(GetAttr(node, "name"), "SeparatelyETC", false, strFile);

                return true;
            }
            catch (Exception ex)
            {
                ErrMsg = strPath + " 파싱 오류: " + ex.Message;
                return false;
            }
        }
        #endregion

        #region@@@.필드 추가 (중복 제거, KeyWord 우선)
        private void AddField(string strName, string strParent, bool bRequired, string strSource)
        {
            if (strName == null) return;
            strName = strName.Trim();
            if (strName == "") return;

            string strCol = ToDbColumn(strName);
            if (strCol == "") return;

            foreach (XmlField f in Fields)
            {
                if (string.Compare(f.DbColumn, strCol, true) == 0)
                {
                    if (bRequired) f.Required = true;   // KeyWord 유래로 승격
                    return;
                }
            }

            XmlField fld = new XmlField();
            fld.Name     = strName;
            fld.Parent   = strParent;
            fld.DbColumn = strCol;
            fld.Required = bRequired;
            fld.Source   = strSource;
            Fields.Add(fld);
        }

        private static string GetAttr(XmlNode node, string strAttr)
        {
            if (node.Attributes == null) return "";
            XmlAttribute att = node.Attributes[strAttr];
            return (att == null) ? "" : att.Value.Trim();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────
        //  DB 동기화
        // ─────────────────────────────────────────────────────────────────

        #region@@@.CV_DATA 현재 컬럼 목록 조회
        public static List<string> GetCvDataColumns(NpgsqlConnection cn, ref string strErr)
        {
            List<string> lstCols = new List<string>();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(
                    "SELECT column_name FROM information_schema.columns " +
                    "WHERE lower(table_name) = 'cv_data' ORDER BY ordinal_position", cn);
                NpgsqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                    lstCols.Add(rd.GetString(0).ToUpper());
                rd.Close();
            }
            catch (Exception ex)
            {
                strErr = "컬럼 조회 오류: " + ex.Message;
            }
            return lstCols;
        }
        #endregion

        #region@@@.동기화 실행 [백업 → 필드 추가]
        // 반환: 처리 로그 문자열 목록. bOk = 성공 여부
        public List<string> SyncToDb(NpgsqlConnection cn, ref bool bOk)
        {
            List<string> lstLog = new List<string>();
            bOk = false;

            string strErr = "";
            List<string> lstCols = GetCvDataColumns(cn, ref strErr);
            if (lstCols.Count == 0)
            {
                lstLog.Add("[오류] CV_DATA 테이블을 찾을 수 없거나 컬럼 조회 실패. " + strErr);
                return lstLog;
            }
            lstLog.Add("[정보] CV_DATA 현재 컬럼 수: " + lstCols.Count);

            // 1) 데이터가 있으면 내부 백업 테이블 생성
            try
            {
                NpgsqlCommand cmdCnt = new NpgsqlCommand("SELECT COUNT(*) FROM CV_DATA", cn);
                long nRows = Convert.ToInt64(cmdCnt.ExecuteScalar());
                if (nRows > 0)
                {
                    string strBak = "CV_DATA_BAK_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    NpgsqlCommand cmdBak = new NpgsqlCommand(
                        "CREATE TABLE " + strBak + " AS SELECT * FROM CV_DATA", cn);
                    cmdBak.ExecuteNonQuery();
                    lstLog.Add("[백업] " + nRows + "건 → " + strBak + " 생성 완료");
                }
                else
                {
                    lstLog.Add("[백업] CV_DATA 데이터 없음 - 백업 생략");
                }
            }
            catch (Exception ex)
            {
                lstLog.Add("[오류] 백업 실패 - 동기화 중단: " + ex.Message);
                return lstLog;
            }

            // 2) 필드 조정: 누락된 컬럼 추가 (기존 데이터는 ALTER ADD 이므로 그대로 유지됨)
            int nAdded = 0, nExist = 0, nFail = 0;
            foreach (XmlField f in Fields)
            {
                if (lstCols.Contains(f.DbColumn.ToUpper()))
                {
                    nExist++;
                    continue;
                }

                try
                {
                    NpgsqlCommand cmdAdd = new NpgsqlCommand(
                        "ALTER TABLE CV_DATA ADD COLUMN " + f.DbColumn + " VARCHAR(255) DEFAULT ''", cn);
                    cmdAdd.ExecuteNonQuery();
                    nAdded++;
                    lstLog.Add((f.Required ? "[필수추가] " : "[추가] ")
                               + f.DbColumn + "  (XML: " + f.Name + ", 출처: " + f.Source + ")");
                }
                catch (Exception ex)
                {
                    nFail++;
                    lstLog.Add("[실패] " + f.DbColumn + " 추가 오류: " + ex.Message);
                }
            }

            // 3) 필수(KeyWord.xml) 필드 최종 검증
            List<string> lstColsAfter = GetCvDataColumns(cn, ref strErr);
            bool bRequiredOk = true;
            foreach (XmlField f in Fields)
            {
                if (!f.Required) continue;
                if (!lstColsAfter.Contains(f.DbColumn.ToUpper()))
                {
                    bRequiredOk = false;
                    lstLog.Add("[오류] 필수 필드 누락: " + f.DbColumn + " (KeyWord: " + f.Name + ")");
                }
            }

            lstLog.Add("─────────────────────────────");
            lstLog.Add("[결과] 기존 " + nExist + "개 / 추가 " + nAdded + "개 / 실패 " + nFail + "개");
            lstLog.Add(bRequiredOk ? "[결과] KeyWord.xml 필수 필드 모두 존재함"
                                   : "[결과] 필수 필드 누락 있음 - 확인 필요!");

            bOk = (nFail == 0 && bRequiredOk);
            return lstLog;
        }
        #endregion
    }
}
