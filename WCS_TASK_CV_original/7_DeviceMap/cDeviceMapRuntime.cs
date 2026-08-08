using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace WCS_TASK_CV
{
    /*
     * cDeviceMapRuntime
     *
     * DeviceMap??.xml 의 정의를 로드하여 PLC 데이터 버퍼를 필드 단위로
     * 파싱하는 런타임 파서. (CvThread 에서 사용)
     *
     * [TrackDatas - 트랙 블록 정의]
     *   <TrackDatas>
     *     <Word type="W" addr="D5n+0" name="LuggNum" />
     *     <Word type="B" addr="D5n+1" name="D5n+1">
     *       <ByteL name="DestPos"/>
     *       <ByteH name="JobType"/>
     *     </Word>
     *     <Word type="b" addr="D5n+2" name="SensorData">
     *       <Bit name="ProductSensor" desc="D5n+2.0"/>
     *     </Word>
     *   </TrackDatas>
     *
     *   addr="D5n+3" : 트랙당 워드수 5, 트랙 내 워드 오프셋 3
     *   type         : W(워드), N(니블), b(비트), B(바이트: ByteL/ByteH)
     *   비트/니블 위치 : pos 속성 우선, 없으면 desc 끝의 ".0"~".F" 사용
     *   dbcol 속성이 있으면 그대로 CV_DATA 컬럼으로 쓰고,
     *   없으면 이름 매핑 테이블(LuggNum->LUGG_NO_RD 등)로 컬럼을 정한다.
     *   이름이 주소형(D5n+4 등)이거나 매핑이 없으면 저장 대상에서 제외.
     *
     * [SeparatelyETC - 트랙 블록 외 상태 영역]
     *   <Area type="b" seq="1" name="Auto" address="357">
     *     <Bit desc="D357.0" Use="1"><TrList>101</TrList>...</Bit>
     *   </Area>
     *   Bit(Use=1) 하나가 TrList 의 트랙들 상태를 대표한다.
     *   TrList 가 없는 Area 는 트랙 매핑 정보가 없어 스킵된다. (SkippedAreas)
     */
    public class cDeviceMapRuntime
    {
        public class CvField
        {
            public string Name;        // XML 상의 이름
            public string DbCol;       // CV_DATA 컬럼명
            public char Kind;          // 'W','N','b','B'
            public int WordOffset;     // 트랙 내 워드 오프셋
            public int Pos;            // 니블(0~3)/비트(0~15)/바이트(0~1) 위치
            public string Fmt;         // zero-padding 형식
            public bool HostRpt;       // 변경 시 상위 보고 여부
        }

        public class EtcBit
        {
            public int Pos;                              // 워드 내 비트 위치
            public List<int> Tracks = new List<int>();   // 이 비트가 대표하는 트랙(MC_NO)들
        }

        public class EtcArea
        {
            public string Name;
            public string DbCol;
            public bool HostRpt;
            public int Address;                          // D 디바이스 절대 주소
            public List<EtcBit> Bits = new List<EtcBit>();
        }

        public int WordPerTrack = 0;
        public List<CvField> Fields = new List<CvField>();
        public List<EtcArea> EtcAreas = new List<EtcArea>();
        public List<string> SkippedAreas = new List<string>();   // 트랙 매핑 정보가 없어 스킵된 Area

        #region 이름 -> (컬럼, 포맷, 상위보고) 기본 매핑
        private class ColDef
        {
            public string DbCol; public string Fmt; public bool HostRpt;
            public ColDef(string strCol, string strFmt, bool bRpt) { DbCol = strCol; Fmt = strFmt; HostRpt = bRpt; }
        }

        private static readonly Dictionary<string, ColDef> NAME_MAP
            = new Dictionary<string, ColDef>(StringComparer.OrdinalIgnoreCase)
        {
            { "LuggNum",       new ColDef("LUGG_NO_RD",            "0000", false) },
            { "DestPos",       new ColDef("DEST_POS_RD",           "000",  false) },
            { "JobType",       new ColDef("JOB_TYP_RD",            "",     false) },
            { "ErrorCode",     new ColDef("ERROR_CODE",            "0000", false) },
            { "SensorData",    new ColDef("SENSOR0_DATA_RD",       "",     true)  },
            { "ProductSensor", new ColDef("SENSOR0_DATA_RD",       "",     true)  },
            { "PulpSensor",    new ColDef("PULP_SENSOR_RD",        "",     false) },
            { "TrPause",       new ColDef("TR_PAUSE_RD",           "",     false) },
            { "WaitScRetJob",  new ColDef("WAIT_SC_RET_JOB_RD",    "",     false) },
            { "Auto",          new ColDef("AUTO_MODE_RD",          "",     true)  },
            { "StoStation",    new ColDef("STO_READY_RD",          "",     true)  },
            { "RetStation",    new ColDef("RET_READY_RD",          "",     true)  },
            { "ScStoHS",       new ColDef("STOHS_READY_RD",        "",     true)  },
            { "ScRetHS",       new ColDef("RETHS_READY_RD",        "",     true)  },
            { "RtvDepartHS",   new ColDef("RTV_DEPARTHS_READY_RD", "",     true)  },
            { "RtvArriveHS",   new ColDef("RTV_ARRIVEHS_READY_RD", "",     true)  },
            { "Emergency",     new ColDef("EMERGENCY_RD",          "",     true)  },
        };
        #endregion

        #region Load() - DeviceMap{PLC_NO}.xml 로드
        public static cDeviceMapRuntime Load(string strPlcNo, ref string strErr)
        {
            string strPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "7_DeviceMap\\DeviceMap" + strPlcNo + ".xml");
            if (!System.IO.File.Exists(strPath))
                strPath = "7_DeviceMap\\DeviceMap" + strPlcNo + ".xml";
            if (!System.IO.File.Exists(strPath))
            {
                strErr = "DeviceMap" + strPlcNo + ".xml 파일을 찾을 수 없습니다.";
                return null;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(strPath);

                cDeviceMapRuntime map = new cDeviceMapRuntime();

                // 1) TrackDatas : 트랙 블록 필드
                XmlNodeList nodes = doc.SelectNodes("/DeviceMap/TrackDatas/Word");
                if (nodes == null || nodes.Count == 0)
                {
                    strErr = strPath + " 에서 TrackDatas/Word 노드를 찾을 수 없습니다.";
                    return null;
                }

                foreach (XmlNode node in nodes)
                {
                    string strAddr = GetAttr(node, "addr");
                    Match m = Regex.Match(strAddr, @"^D(\d+)n\+(\d+)$");
                    if (!m.Success)
                    {
                        strErr = "addr 형식 오류: [" + strAddr + "] (예: D5n+3)";
                        return null;
                    }

                    int nWpt = Convert.ToInt32(m.Groups[1].Value);
                    int nOffset = Convert.ToInt32(m.Groups[2].Value);

                    if (map.WordPerTrack == 0)
                        map.WordPerTrack = nWpt;
                    else if (map.WordPerTrack != nWpt)
                    {
                        strErr = "트랙당 워드수가 일치하지 않습니다: D" + map.WordPerTrack + "n vs " + strAddr;
                        return null;
                    }

                    string strType = GetAttr(node, "type");
                    char cKind = (strType == "") ? 'W' : strType[0];

                    if (cKind == 'W')
                    {
                        AddField(map, node, cKind, nOffset, 0);
                    }
                    else
                    {
                        // N/b/B : 하위 엘리먼트(Nibble/Bit/ByteL/ByteH)들이 각각 필드
                        int nSeq = 0;
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            if (child.NodeType != XmlNodeType.Element) continue;

                            int nPos;
                            if (child.Name == "ByteL")      nPos = 0;
                            else if (child.Name == "ByteH") nPos = 1;
                            else                            nPos = GetPos(child, nSeq);

                            AddField(map, child, cKind, nOffset, nPos);
                            nSeq++;
                        }
                    }
                }

                if (map.Fields.Count == 0)
                {
                    strErr = strPath + " 에 CV_DATA 컬럼으로 매핑되는 필드가 없습니다.";
                    return null;
                }

                // 2) SeparatelyETC : 트랙 블록 외 상태 영역 (Bit+TrList 가 있는 Area 만)
                XmlNode etcRoot = doc.SelectSingleNode("/DeviceMap/SeparatelyETC");
                if (etcRoot != null && GetAttr(etcRoot, "use") == "1")
                {
                    foreach (XmlNode area in etcRoot.ChildNodes)
                    {
                        if (area.NodeType != XmlNodeType.Element || area.Name != "Area") continue;
                        LoadEtcArea(map, area);
                    }
                }

                return map;
            }
            catch (Exception ex)
            {
                strErr = strPath + " 파싱 오류: " + ex.Message;
                return null;
            }
        }

        private static void LoadEtcArea(cDeviceMapRuntime map, XmlNode area)
        {
            string strName = GetAttr(area, "name");

            EtcArea etc = new EtcArea();
            etc.Name = strName;
            etc.Address = Convert.ToInt32("0" + GetAttr(area, "address"));

            // 컬럼 매핑 (dbcol 속성 우선, 없으면 이름 매핑)
            string strDbCol = GetAttr(area, "dbcol");
            if (strDbCol == "" && NAME_MAP.ContainsKey(strName))
            {
                strDbCol = NAME_MAP[strName].DbCol;
                etc.HostRpt = NAME_MAP[strName].HostRpt;
            }
            if (GetAttr(area, "hostRpt") == "1") etc.HostRpt = true;
            if (strDbCol == "")
            {
                map.SkippedAreas.Add(strName + "(컬럼 매핑 없음)");
                return;
            }
            etc.DbCol = strDbCol.ToUpper();

            // Bit + TrList : 비트별 담당 트랙
            int nSeq = 0;
            foreach (XmlNode bit in area.ChildNodes)
            {
                if (bit.NodeType != XmlNodeType.Element || bit.Name != "Bit") continue;

                int nPos = GetPos(bit, nSeq);
                nSeq++;

                if (GetAttr(bit, "Use") != "1") continue;   // 사용안함 비트

                EtcBit eb = new EtcBit();
                eb.Pos = nPos;
                foreach (XmlNode tr in bit.ChildNodes)
                {
                    if (tr.NodeType != XmlNodeType.Element || tr.Name != "TrList") continue;
                    if (GetAttr(tr, "desc").IndexOf("사용안함") >= 0) continue;
                    eb.Tracks.Add(Convert.ToInt32("0" + tr.InnerText.Trim()));
                }
                if (eb.Tracks.Count > 0)
                    etc.Bits.Add(eb);
            }

            if (etc.Bits.Count > 0)
                map.EtcAreas.Add(etc);
            else
                map.SkippedAreas.Add(strName + "(트랙 매핑 정보 없음)");
        }

        private static void AddField(cDeviceMapRuntime map, XmlNode node, char cKind, int nOffset, int nPos)
        {
            string strName = GetAttr(node, "name");

            // 주소형 이름(D5n+4 등)은 스페어/컨테이너 -> 저장 대상 아님
            if (strName == "" || Regex.IsMatch(strName, @"^D\d+n"))
                return;

            string strDbCol = GetAttr(node, "dbcol");
            string strFmt = GetAttr(node, "fmt");
            bool bHostRpt = (GetAttr(node, "hostRpt") == "1");

            if (strDbCol == "" && NAME_MAP.ContainsKey(strName))
            {
                ColDef def = NAME_MAP[strName];
                strDbCol = def.DbCol;
                if (strFmt == "") strFmt = def.Fmt;
                bHostRpt = bHostRpt || def.HostRpt;
            }
            if (strDbCol == "")
                strDbCol = cXmlFieldSync.ToDbColumn(strName);   // 매핑 없는 이름은 자동 규칙
            if (strDbCol == "")
                return;

            CvField f = new CvField();
            f.Name       = strName;
            f.DbCol      = strDbCol.ToUpper();
            f.Kind       = cKind;
            f.WordOffset = nOffset;
            f.Pos        = nPos;
            f.Fmt        = strFmt;
            f.HostRpt    = bHostRpt;
            map.Fields.Add(f);
        }

        // 비트/니블 위치: pos 속성 우선, 없으면 desc 끝의 ".0"~".F", 그것도 없으면 등장 순서
        private static int GetPos(XmlNode node, int nSeq)
        {
            string strPos = GetAttr(node, "pos");
            if (strPos != "")
                return Convert.ToInt32(strPos);

            string strDesc = GetAttr(node, "desc");
            Match m = Regex.Match(strDesc, @"\.([0-9A-Fa-f])$");
            if (m.Success)
                return Convert.ToInt32(m.Groups[1].Value, 16);

            return nSeq;
        }

        private static string GetAttr(XmlNode node, string strAttr)
        {
            if (node.Attributes == null) return "";
            XmlAttribute att = node.Attributes[strAttr];
            return (att == null) ? "" : att.Value.Trim();
        }
        #endregion

        #region ParseTrack() - 트랙 버퍼 파싱 (컬럼명 -> 값)
        /// <summary>
        /// PLC 수신 버퍼에서 한 트랙 분량을 XML 정의대로 파싱한다.
        /// nBaseByte : 해당 트랙의 시작 바이트 인덱스 (트랙인덱스 * WordPerTrack * 2)
        /// </summary>
        public Dictionary<string, string> ParseTrack(byte[] byBuf, int nBaseByte)
        {
            Dictionary<string, string> dicVals = new Dictionary<string, string>();
            foreach (CvField f in Fields)
                dicVals[f.DbCol] = GetValue(f, byBuf, nBaseByte);
            return dicVals;
        }

        public string GetValue(CvField f, byte[] byBuf, int nBaseByte)
        {
            int nByteIdx = nBaseByte + f.WordOffset * 2;
            int nWord = (byBuf[nByteIdx + 1] << 8) + byBuf[nByteIdx];   // little-endian 워드

            int nVal;
            switch (f.Kind)
            {
                case 'N': nVal = (nWord >> (4 * f.Pos)) & 0x0F; break;
                case 'b': nVal = (nWord >> f.Pos) & 0x01;       break;
                case 'B': nVal = (nWord >> (8 * f.Pos)) & 0xFF; break;
                default:  nVal = nWord;                          break;
            }
            return (f.Fmt == "") ? nVal.ToString() : nVal.ToString(f.Fmt);
        }
        #endregion

        #region 쓰기 지원 - 컬럼 기준 워드 오프셋/값 구성
        /// <summary>컬럼이 트랙 블록의 몇 번째 워드에 있는지 반환 (-1: 정의 없음)</summary>
        public int GetWordOffsetByCol(string strDbCol)
        {
            foreach (CvField f in Fields)
                if (string.Compare(f.DbCol, strDbCol, true) == 0)
                    return f.WordOffset;
            return -1;
        }

        /// <summary>
        /// 트랙 블록 쓰기용 워드 배열(anWords)에 컬럼 값을 XML 정의 위치대로 채운다.
        /// 반환 false: 해당 컬럼이 맵에 정의되지 않음 (쓰기 생략 대상)
        /// </summary>
        public bool SetWriteValue(int[] anWords, bool[] abUsed, string strDbCol, int nVal)
        {
            foreach (CvField f in Fields)
            {
                if (string.Compare(f.DbCol, strDbCol, true) != 0) continue;

                switch (f.Kind)
                {
                    case 'W': anWords[f.WordOffset] = nVal & 0xFFFF; break;
                    case 'N': anWords[f.WordOffset] |= (nVal & 0x0F) << (4 * f.Pos); break;
                    case 'b': anWords[f.WordOffset] |= (nVal & 0x01) << f.Pos; break;
                    case 'B': anWords[f.WordOffset] |= (nVal & 0xFF) << (8 * f.Pos); break;
                }
                abUsed[f.WordOffset] = true;
                return true;
            }
            return false;
        }
        #endregion

        #region GetDbColumns() - 저장 대상 컬럼 목록
        public List<string> GetDbColumns()
        {
            List<string> lstCols = new List<string>();
            foreach (CvField f in Fields)
                if (!lstCols.Contains(f.DbCol))
                    lstCols.Add(f.DbCol);
            foreach (EtcArea a in EtcAreas)
                if (!lstCols.Contains(a.DbCol))
                    lstCols.Add(a.DbCol);
            return lstCols;
        }
        #endregion
    }
}
