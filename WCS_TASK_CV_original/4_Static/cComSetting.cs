using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WCS_TASK_CV
{
    /*
     * cComSetting
     *
     * 프로그램 기동 직전(Program.Main) 에 1회 실행되는 통신 설정 재구성 처리.
     *
     * INI [COM_SETTING] Setting 값에 따라:
     *   0 : 아무것도 하지 않는다.
     *   1 : [COMM0] 섹션만 남기고 나머지 [COMM?] 섹션을 모두 삭제한 뒤,
     *       [PROCESS] CNT 가 2 이상이면 [COMM0] 을 복사하여 CNT 개의
     *       [COMM0]~[COMM{CNT-1}] 섹션을 만든다.
     *       (복사본의 FILENAME 은 WCS_TASK_CV_COMM{번호} 로 변경)
     *       전부 성공하면 Setting=0 으로 되돌려 INI 에 저장한다.
     */
    public static class cComSetting
    {
        public static void Apply()
        {
            try
            {
                string strPath = cDefApp.GM_ENV_INI;
                if (!File.Exists(strPath)) return;

                string[] arLines = File.ReadAllLines(strPath, Encoding.Default);

                // [COM_SETTING] Setting : 1 이 아니면 아무것도 안 함
                if (GetIniValue(arLines, "COM_SETTING", "Setting") != "1")
                    return;

                // [PROCESS] CNT
                int nCnt = 0;
                int.TryParse(GetIniValue(arLines, "PROCESS", "CNT"), out nCnt);

                // [COMM0] 블록 추출
                List<string> lstComm0 = GetSectionBlock(arLines, "COMM0");
                if (lstComm0 == null)
                {
                    MessageBox.Show("[COM_SETTING] 처리 실패 : [COMM0] 섹션을 찾을 수 없습니다.\n통신 설정 재구성을 건너뜁니다.", "WCS_TASK_CV");
                    return;
                }

                int nMake = (nCnt >= 2) ? nCnt : 1;   // CNT 2 미만이면 COMM0 만 남김

                List<string> lstNew = new List<string>();
                string strCurSection = "";
                bool bInComm = false;
                bool bInserted = false;

                foreach (string strLine in arLines)
                {
                    string strTrim = strLine.Trim();

                    if (strTrim.StartsWith("["))
                    {
                        Match m = Regex.Match(strTrim, @"^\[([^\]]+)\]");
                        strCurSection = m.Success ? m.Groups[1].Value.Trim() : "";
                        bInComm = Regex.IsMatch(strCurSection, @"^COMM\d+$");
                    }

                    // 기존 [COMM?] 섹션 라인은 모두 버리고, 첫 위치에 새 블록들을 삽입
                    if (bInComm)
                    {
                        if (!bInserted)
                        {
                            bInserted = true;
                            for (int i = 0; i < nMake; i++)
                            {
                                foreach (string strSrc in lstComm0)
                                {
                                    string strOut = strSrc;
                                    if (i > 0)
                                    {
                                        if (Regex.IsMatch(strSrc.Trim(), @"^\[COMM0\]"))
                                            strOut = "[COMM" + i.ToString() + "]";
                                        else if (Regex.IsMatch(strSrc.Trim(), @"^FILENAME\s*=", RegexOptions.IgnoreCase))
                                            strOut = "FILENAME=WCS_TASK_CV_COMM" + i.ToString();
                                    }
                                    lstNew.Add(strOut);
                                }
                                lstNew.Add("");
                            }
                        }
                        continue;
                    }

                    // 성공 시 Setting=1 -> 0 으로 변경하여 저장
                    if (string.Compare(strCurSection, "COM_SETTING", true) == 0 &&
                        Regex.IsMatch(strTrim, @"^Setting\s*=", RegexOptions.IgnoreCase))
                    {
                        lstNew.Add("Setting=0");
                        continue;
                    }

                    lstNew.Add(strLine);
                }

                File.WriteAllLines(strPath, lstNew.ToArray(), Encoding.Default);
            }
            catch (Exception ex)
            {
                // 재구성 실패 시 Setting 은 1 로 남으므로 다음 기동에서 재시도된다.
                MessageBox.Show("[COM_SETTING] 처리 중 오류 : " + ex.Message, "WCS_TASK_CV");
            }
        }

        #region INI 텍스트 헬퍼
        // 섹션 내 항목 값 읽기 (없으면 "")
        private static string GetIniValue(string[] arLines, string strSection, string strKey)
        {
            bool bIn = false;
            foreach (string strLine in arLines)
            {
                string strTrim = strLine.Trim();
                if (strTrim.StartsWith("["))
                {
                    Match m = Regex.Match(strTrim, @"^\[([^\]]+)\]");
                    bIn = m.Success && string.Compare(m.Groups[1].Value.Trim(), strSection, true) == 0;
                    continue;
                }
                if (!bIn) continue;

                Match kv = Regex.Match(strTrim, @"^(?<k>[^=;\-][^=]*)=(?<v>.*)$");
                if (kv.Success && string.Compare(kv.Groups["k"].Value.Trim(), strKey, true) == 0)
                    return kv.Groups["v"].Value.Trim();
            }
            return "";
        }

        // 섹션 헤더부터 다음 섹션 직전까지 블록 추출 (끝의 빈 줄 제외, 없으면 null)
        private static List<string> GetSectionBlock(string[] arLines, string strSection)
        {
            List<string> lstBlock = null;
            foreach (string strLine in arLines)
            {
                string strTrim = strLine.Trim();
                if (strTrim.StartsWith("["))
                {
                    Match m = Regex.Match(strTrim, @"^\[([^\]]+)\]");
                    if (lstBlock != null) break;   // 다음 섹션 시작 -> 종료
                    if (m.Success && string.Compare(m.Groups[1].Value.Trim(), strSection, true) == 0)
                        lstBlock = new List<string>();
                }
                if (lstBlock != null)
                    lstBlock.Add(strLine);
            }
            if (lstBlock == null) return null;

            while (lstBlock.Count > 0 && lstBlock[lstBlock.Count - 1].Trim() == "")
                lstBlock.RemoveAt(lstBlock.Count - 1);
            return lstBlock;
        }
        #endregion
    }
}
