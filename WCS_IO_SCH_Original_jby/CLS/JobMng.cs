using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using Samoh_Lib;

namespace TSK_COMM_IOSCH
{
    static class JobMng 
    {
        private static ArrayList JobList = new ArrayList();

        public static string GetCodeBoxType(string _strWmsJob)
        {
            if (_strWmsJob.Length == 0) return "";

            string Hex = _strWmsJob.Substring(0, 1);
            if (Hex == "U" || Hex == "u") return _strWmsJob;

            return "";
        }
        public static string GetCodeBoxType2(string BoxSize, string _strWmsJob)
        {
            if (_strWmsJob.Length == 0) _strWmsJob = " ";
            string Hex = _strWmsJob.Substring(0, 1);
            if (Hex != "U" && Hex != "u")
            {
                Hex = "E";
            }

            if (BoxSize == "P2") Hex = Hex.ToUpper();
            if (BoxSize == "P3") Hex = Hex.ToLower();


            return Hex;
        }
        public static void JobAdd(JobItem pJob, ref ListView pLv)
        {
            ListViewItem lvItem = new ListViewItem(pJob._nLuggNum.ToString());
            lvItem.SubItems.Add(pJob._strJobType.ToString());
            lvItem.SubItems.Add(pJob._strCarrierID);
            lvItem.SubItems.Add(pJob._strBoxNo);
            lvItem.SubItems.Add(pJob._strLotNo);
            lvItem.SubItems.Add(pJob._strSOURCE);
            lvItem.SubItems.Add(pJob._strDEST);

            pLv.Items.Add(lvItem);
            JobList.Add(pJob);
        }

        public static void JobRemove(JobItem pJob, ref ListView pLv)
        {

            for (int i = 0; i <= pLv.Items.Count - 1; i++)
            {
                if (pLv.Items[i].SubItems[0].Text == pJob._nLuggNum.ToString())
                {
                    pLv.Items[i].Remove();
                }
            }

            for (int i = 0; i <= JobList.Count - 1; i++)
            {

                JobItem fJob;
                fJob = (JobItem)JobList[i];

                if (fJob._nLuggNum == pJob._nLuggNum)
                {
                    JobList.RemoveAt(i);
                }
            }
        }

        public static JobItem JobSerch(string pLuggNum)
        {

            for (int i = 0; i <= JobList.Count - 1; i++)
            {

                JobItem fJob;
                fJob = (JobItem)JobList[i];

                if (fJob._nLuggNum.ToString() == pLuggNum)
                {
                    return fJob;
                }
            }
            return null;

        }

        public static JobItem BoxSerch(string pBoxNo)
        {

            for (int i = 0; i <= JobList.Count - 1; i++)
            {

                JobItem fJob;
                fJob = (JobItem)JobList[i];

                if (fJob._strBoxNo.ToString() == pBoxNo)
                {
                    return fJob;
                }
            }
            return null;

        }


        public static bool GetSITENO_DETAIL(ref CUserDb pBdb, string SITENO, ref int nSelCnt, ref string Msg)
        {
            try
            {
                string strSql;
                string CRLF = Microsoft.VisualBasic.ControlChars.CrLf;
                Msg = "GetSITENO_DETAIL::";

                strSql = "";
                strSql += CRLF + " SELECT    X.CV_JOB_KIND           ";
                strSql += CRLF + "          ,X.CV_LUGGNO             ";
                strSql += CRLF + "          ,X.CV_SOUR_SITE          ";
                strSql += CRLF + "          ,X.CV_DEST_SITE          ";
                strSql += CRLF + "          ,X.CV_DATA_STA           ";
                strSql += CRLF + "          ,X.CV_PA_RD              ";
                strSql += CRLF + "          ,X.CV_PK_RD              ";
                strSql += CRLF + "          ,X.CV_HA_RD              ";
                strSql += CRLF + "          ,X.CV_HK_RD              ";
                strSql += CRLF + "          ,X.CV_LG_EX              ";
                strSql += CRLF + "          ,X.CV_ERR_CODE           ";
                strSql += CRLF + "          ,X.CV_ERR_OC_DT          ";
                strSql += CRLF + "          ,X.CV_BOX_TYP            ";
                strSql += CRLF + "          ,X.CV_JEPUM_GRADE        ";
                strSql += CRLF + "          ,X.CV_BOX_START          ";
                strSql += CRLF + "          ,X.CV_BOX_DEST           ";
                strSql += CRLF + "          ,X.CV_LGV_START          ";
                strSql += CRLF + "          ,X.CV_LGV_DEST           ";
                strSql += CRLF + "          ,X.CV_WEIGHT_STATION     ";
                strSql += CRLF + "          ,X.CV_MERGE_STATION      ";
                strSql += CRLF + "          ,X.CV_REMAIN_STATION     ";
                strSql += CRLF + "          ,X.CV_LIFTER_START       ";
                strSql += CRLF + "          ,X.CV_LIFTER_DEST        ";
                strSql += CRLF + "          ,X.CV_RCV_DT             ";
                strSql += CRLF + "          ,X.CV_BCD_DATA           ";
                strSql += CRLF + "          ,X.CV_IO_TYP             ";
                strSql += CRLF + "          ,X.CV_IO_MODE            ";
                strSql += CRLF + "          ,X.CV_CONTROL1           ";
                strSql += CRLF + "          ,X.CV_CONTROL2           ";
                strSql += CRLF + "          ,X.CV_CONTROL3           ";
                strSql += CRLF + "          ,X.CV_SENSOR             ";
                strSql += CRLF + "          ,X.CV_TRK_PAUSE          ";
                //2010.01 조형준 추가----------------------------------
                strSql += CRLF + "          ,X.CV_CONTROL            ";
                //-----------------------------------------------------
                strSql += CRLF + "          ,X.OD_CMD_CV             ";
                strSql += CRLF + "          ,X.OD_CMD_DT             ";
                strSql += CRLF + "          ,X.OD_JOB_KIND           ";
                strSql += CRLF + "          ,X.OD_LGNO_CV            ";
                strSql += CRLF + "          ,X.OD_SOUR_SITE          ";
                strSql += CRLF + "          ,X.OD_DEST_SITE          ";
                strSql += CRLF + "          ,X.OD_DATA_STA           ";
                strSql += CRLF + "          ,X.OD_UROD_TYP           ";
                strSql += CRLF + "          ,X.OD_BOX_TYP            ";
                strSql += CRLF + "          ,X.OD_JEPUM_GRADE        ";
                strSql += CRLF + "          ,X.OD_CONTROL            ";
                strSql += CRLF + "          ,X.OD_CONTROL_WRITE_YON  ";
                strSql += CRLF + "          ,X.OD_BCD_DATA           ";
                strSql += CRLF + "          ,X.RT_CMD_CV             ";
                strSql += CRLF + "          ,X.RT_CMD_DT             ";
                strSql += CRLF + "          ,X.BCR_NO                ";
                strSql += CRLF + "          ,X.BCR_READ_STA          ";
                strSql += CRLF + "          ,X.BCR_COM_STA           ";
                strSql += CRLF + "          ,X.BCR_RCV_VAL1          ";
                strSql += CRLF + "          ,X.BCR_RCV_VAL2          ";
                strSql += CRLF + "          ,X.BCR_RCV_VAL3          ";
                strSql += CRLF + "          ,X.BCR_ERR_CODE          ";
                strSql += CRLF + "          ,X.BCR_ERR_OC_DT         ";
                strSql += CRLF + "          ,X.BCR_RCV_DT            ";
                strSql += CRLF + "          ,X.WGT_NO                ";
                strSql += CRLF + "          ,X.WGT_READ_STA          ";
                strSql += CRLF + "          ,X.WGT_COM_STA           ";
                strSql += CRLF + "          ,X.WGT_RCV_VAL           ";
                strSql += CRLF + "          ,X.WGT_ERR_CODE          ";
                strSql += CRLF + "          ,X.WGT_ERR_OC_DT         ";
                strSql += CRLF + "          ,X.WGT_RCV_DT            ";
                strSql += CRLF + "          ,X.PA_HS_YON             ";
                strSql += CRLF + "          ,X.PK_HS_YON             ";
                strSql += CRLF + "          ,X.PA_MC_YON             ";
                strSql += CRLF + "          ,X.PK_MC_YON             ";
                strSql += CRLF + "          ,X.PS_MC_YON             ";
                strSql += CRLF + "          ,X.PP_MC_YON             ";
                strSql += CRLF + "          ,X.SITENO                ";
                strSql += CRLF + "          ,Y.MOUDLE_ID             ";
                strSql += CRLF + "          ,Y.DEFAULT_TRK_POS       ";
                strSql += CRLF + "          ,Y.TRACK_NO              ";
                strSql += CRLF + "          ,Y.BAGGING_WC_YON        ";
                strSql += CRLF + "          ,Y.BAGGING_INSPECTION_YON";
                strSql += CRLF + "          ,Y.LINE_ID               ";
                strSql += CRLF + "          ,Y.BAGGING_LABEL_YON     ";
                strSql += CRLF + "          ,Y.BAGGING_START_YON     ";
                strSql += CRLF + "          ,Y.BREAKING_SCAN_YON     ";
                strSql += CRLF + "          ,Y.BREAKING_CLEAN_YON    ";
                strSql += CRLF + "          ,Y.BOX_AUTO_RET_YON      ";
                strSql += CRLF + "          ,Y.SC_HS_NO              ";
                strSql += CRLF + "   FROM  MC_STA_MST X              ";
                strSql += CRLF + "        ,TRACKINFO  Y              ";
                strSql += CRLF + "  WHERE  X.WH_TYP     = Y.WH_TYP   ";
                strSql += CRLF + "    AND  X.MC_TYP     = Y.MC_TYP   ";
                strSql += CRLF + "    AND  X.IF_CV_NO   = Y.TRACK_NO ";
                strSql += CRLF + "    AND  X.WH_TYP     = ?          ";
                strSql += CRLF + "    AND  X.MC_TYP     = ?          ";
                strSql += CRLF + "    AND  X.SITENO     = ?          ";

                pBdb.comMain.CommandType = CommandType.Text;
                pBdb.comMain.Parameters.Clear();
                pBdb.comMain.Parameters.Add("?", DbLang.VARCHAR).Value = "P3";
                pBdb.comMain.Parameters.Add("?", DbLang.VARCHAR).Value = "CV";
                pBdb.comMain.Parameters.Add("?", DbLang.VARCHAR).Value = SITENO;
                nSelCnt = pBdb.ExcuteQry(strSql, false, true, true);

                if (nSelCnt < 0)
                {
                    Msg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    Msg = "요청자료 없음";
                    return false;
                }


                Msg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                Msg += ex.Message;
            }
            return false;
        }


        public static bool GetBoxSize(ref CUserDb argBdb, string BoxNo, ref string BoxSize, ref int nSelCnt, ref string Msg)
        {
            try
            {
                string strSql;
                string CRLF = Microsoft.VisualBasic.ControlChars.CrLf;

                Msg = "GetBoxSize::";

                strSql = "";
                strSql += CRLF + " select  cv_box_size ";
                strSql += CRLF + "   from  boxmast     ";
                strSql += CRLF + "  where  boxno  = ?  ";
                strSql += CRLF + "    and  rownum = 1  ";


                argBdb.comMain.CommandType = CommandType.Text;
                argBdb.comMain.Parameters.Clear();
                argBdb.comMain.Parameters.Add("pBOXNO", DbLang.VARCHAR).Value = BoxNo;

                nSelCnt = argBdb.ExcuteQry(strSql, false, true, false);

                if (nSelCnt != 1)
                {
                    Msg += argBdb.ErrMsg;
                    return false;
                }

                BoxSize = argBdb.dtMain.Rows[0]["cv_box_size"].ToString();

                Msg = "박스정보 읽기 성공";
                return true;
            }
            catch (Exception ex)
            {
                Msg += "[GetBoxSize]::박스정보 읽기 실패::" + ex.Message;
            }

            return false;
        }


        public static bool BaggingStartVal(ref CUserDb argBdb, int TrackNo, ref double WGT_RCV_VAL, ref string WGT_COM_STA, ref bool CV_MERGE_STATION,ref bool CV_CHIP_MODE, ref int nSelCnt, ref string Msg)
        {
            try
            {
                string strSql;
                string CRLF = Microsoft.VisualBasic.ControlChars.CrLf;

                Msg = "BaggingStartVal::";

                strSql = "";
                strSql += CRLF + " select  *  ";
                strSql += CRLF + "   from  mc_sta_mst    ";
                strSql += CRLF + "  where  if_cv_no  = ?  ";


                argBdb.comMain.CommandType = CommandType.Text;
                argBdb.comMain.Parameters.Clear();
                argBdb.comMain.Parameters.Add("SITENO", DbLang.VARCHAR).Value = TrackNo.ToString("000");

                nSelCnt = argBdb.ExcuteQry(strSql, false, true, false);

                if (nSelCnt != 1)
                {
                    Msg += argBdb.ErrMsg;
                    return false;
                }

                WGT_RCV_VAL = Convert.ToDouble("0"+argBdb.dtMain.Rows[0]["WGT_RCV_VAL"].ToString());
                WGT_COM_STA = "" + argBdb.dtMain.Rows[0]["WGT_READ_STA"].ToString();
                int Imsi = Convert.ToInt16("0" + argBdb.dtMain.Rows[0]["CV_MERGE_STATION"].ToString().Trim());
                int Imsc = Convert.ToInt16("0" + argBdb.dtMain.Rows[0]["CV_LGV_START"].ToString().Trim());

                CV_MERGE_STATION = Imsi == 1 ? true : false;
                CV_CHIP_MODE = Imsc == 1 ? true : false;


                //if (Imsi == 1)
                //{
                //    CV_MERGE_STATION = true;
                //}
                //else
                //{
                //    CV_MERGE_STATION = false;
                //}


                Msg = "박스정보 읽기 성공";
                return true;
            }
            catch (Exception ex)
            {
                Msg += "[GetBoxSize]::박스정보 읽기 실패::" + ex.Message;
            }

            return false;
        }


    }
}
