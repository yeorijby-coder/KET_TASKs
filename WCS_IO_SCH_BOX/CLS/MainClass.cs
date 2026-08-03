using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using Samoh_Lib;
using Npgsql;
using NpgsqlTypes;
#if POSTGRESQL
using DbConn = Npgsql.NpgsqlConnection;
#elif SQL
using DbConn = System.Data.SqlClient.SqlConnection;
#endif

namespace TSK_COMM_IOSCH
{
    public class MainClass : BaseClass
    {
        /*
         * DBOpen
         */
        #region DBOpen
        public bool DBOpen()
        {
#if ORACLE
            IsRtnMsg = "DBOpen::";

            try
            {
                _pConObj = new OleDbConnection();
                _pConObj.ConnectionString = ConnectionString;
                _pConObj.Open();

                if (_pConObj.State != ConnectionState.Open)
                {
                    IsRtnMsg += "DB Open이 안됩니다.";
                    _pConObj.Dispose();
                    return false;
                }

                IsDBOpen = true;

                _pBdb = new cDbUse(ref _pConObj, false);

                check = check + 1;
                return true;
            }
#elif POSTGRESQL || SQL
            IsRtnMsg = "DBOpen::";

            try
            {
                _pConObj = new DbConn();
                _pConObj.ConnectionString = ConnectionString;
                _pConObj.Open();

                if (_pConObj.State != ConnectionState.Open)
                {
                    IsRtnMsg += "DB Open이 안됩니다.";
                    _pConObj.Dispose();
                    return false;
                }

                IsDBOpen = true;

                _pBdb = new cDbPostUse(_pConObj, false);

                check = check + 1;
                return true;
            }
#endif
            
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
                if (_pConObj != null) _pConObj.Dispose();
                return false;
            }
        }
        #endregion DBOpen
        /*
         * DBClose
         */
        #region DBClose
        public bool DBClose()
        {
            IsRtnMsg = "DBClose::";

            try
            {
                if (IsDBOpen)
                {
                    _pConObj.Dispose();
                    IsDBOpen = false;
                    _pBdb = null;
                }

                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
                return false;
            }
        }
        #endregion DBClose
        /*
         * BytesToHexs
         */
        #region
        public string BytesToHexs(byte[] value, int len)
        {
            IsRtnMsg = "BytesToHexs::";
            string cvtvalue = "";
            for (int ii = 0; ii < len; ii++)
            {
                cvtvalue = cvtvalue + string.Format("{0:X2}", value[ii]);
                if (((ii + 1) % 2) == 0) cvtvalue = cvtvalue + " ";
            }
            return cvtvalue;
        }
        #endregion

        /*
         * [Method]Collection 객체에서 컨트롤을 찾아 리턴한다.
         */
        #region
        public Control PfCtlFind(ref Panel pPnl, string pCtlNm)
        {
            IsRtnMsg = "PfCtlFind::";
            Control[] ctl;
            try
            {
                IsRtnMsg = "";

                ctl = pPnl.Controls.Find(pCtlNm, true);

                if (ctl.Length == 0)
                {
                    return null;
                }
                else
                {
                    IsRtnMsg += "Success::" + "(" + pCtlNm + ")";
                    return ctl[0];
                }
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            IsRtnMsg += "(" + pCtlNm + ")";
            return null;
        }

		public Control PfCtlFind1(SplitterPanel pPnl, string pCtlNm)
		{
			IsRtnMsg = "PfCtlFind::";
			Control[] ctl;
			try
			{
				IsRtnMsg = "";

				ctl = pPnl.Controls.Find(pCtlNm, true);

				if (ctl.Length == 0)
				{
					return null;
				}
				else
				{
					IsRtnMsg += "Success::" + "(" + pCtlNm + ")";
					return ctl[0];
				}
			}
			catch (Exception ex)
			{
				IsRtnMsg += ex.Message;
			}
			IsRtnMsg += "(" + pCtlNm + ")";
			return null;
		}
        #endregion


        /* 
         * 한 트랙정보를 읽어옴.
         * GetTrackNo(MC_STA_MST)
         */
        #region GetTrackNo
        public bool GetTrackNo(int TrackNo, ref int nSelCnt)
        {
            return GetTrackNo(ref this._pBdb, TrackNo, ref nSelCnt);
        }
#if ORACLE
        public bool GetTrackNo(ref cDbUse pBdb, int TrackNo, ref int nSelCnt)
        {
            return GetTrackNoImpl(pBdb, TrackNo, ref nSelCnt);
        }
#elif POSTGRESQL || SQL
        public bool GetTrackNo(ref cDbPostUse pBdb, int TrackNo, ref int nSelCnt)
        {
            return GetTrackNoImpl(pBdb, TrackNo, ref nSelCnt);
        }
#endif

        // DB 종류별 래퍼가 공통으로 사용하는 구현
        private bool GetTrackNoImpl(object pBdbObj, int TrackNo, ref int nSelCnt)
        {
            try
            {
                dynamic pBdb = pBdbObj;
                IsRtnMsg = "GetTrackNo::";

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
                strSql += CRLF + "          ,Y.BAGGING_START_WC_YON  ";
                strSql += CRLF + "          ,Y.BAGGING_START_BCR_YON ";
                strSql += CRLF + "          ,Y.BAGGING_EMG_LINE_YON         ";
                strSql += CRLF + "          ,Y.BAGGING_OP_GRADE_MIS_LINE_YON";
                strSql += CRLF + "   FROM  MC_STA_MST X              ";
                strSql += CRLF + "        ,TRACKINFO  Y              ";
                strSql += CRLF + "  WHERE  X.WH_TYP     = Y.WH_TYP   ";
                strSql += CRLF + "    AND  X.MC_TYP     = Y.MC_TYP   ";
                strSql += CRLF + "    AND  X.IF_CV_NO   = Y.TRACK_NO ";
                strSql += CRLF + "    AND  X.WH_TYP     = ?          ";
                strSql += CRLF + "    AND  X.MC_TYP     = ?          ";
                strSql += CRLF + "    AND  X.IF_CV_NO   = ?          ";

                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }


                CV_JOB_KIND         = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_JOB_KIND"].ToString());
                CV_LUGGNO           = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_LUGGNO"].ToString());
                CV_SOUR_SITE        = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_SOUR_SITE"].ToString());
                CV_DEST_SITE        = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_DEST_SITE"].ToString());
                CV_DATA_STA         = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_DATA_STA"].ToString());
                CV_PA_RD            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_PA_RD"].ToString());
                CV_PK_RD            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_PK_RD"].ToString());
                CV_HA_RD            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_HA_RD"].ToString());
                CV_HK_RD            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_HK_RD"].ToString());
                CV_LG_EX            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LG_EX"].ToString());
                CV_ERR_CODE         = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_ERR_CODE"].ToString());
                CV_BOX_TYP          = "" + pBdb.mDtMain.Rows[0]["CV_BOX_TYP"].ToString();
                CV_JEPUM_GRADE      = "" + pBdb.mDtMain.Rows[0]["CV_JEPUM_GRADE"].ToString();
                CV_BOX_START        = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_BOX_START"].ToString());
                CV_BOX_DEST         = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_BOX_DEST"].ToString());
                CV_LGV_START        = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LGV_START"].ToString());
                CV_LGV_DEST         = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LGV_DEST"].ToString());
                CV_WEIGHT_STATION   = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_WEIGHT_STATION"].ToString());
                CV_MERGE_STATION    = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_MERGE_STATION"].ToString());
                CV_REMAIN_STATION   = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_REMAIN_STATION"].ToString());
                CV_LIFTER_START     = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LIFTER_START"].ToString());
                CV_LIFTER_DEST      = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LIFTER_DEST"].ToString());
                CV_BCD_DATA         = "" + pBdb.mDtMain.Rows[0]["CV_BCD_DATA"].ToString();
                CV_BCD_DATA         = CV_BCD_DATA.Trim();


                CV_IO_TYP       = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_IO_TYP"].ToString());
                CV_IO_MODE      = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_IO_MODE"].ToString());
                MOUDLE_ID       = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["MOUDLE_ID"].ToString());
                DEFAULT_TRK_POS = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["DEFAULT_TRK_POS"].ToString());

                CV_SENSOR       = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_SENSOR"].ToString());

                CV_CONTROL1     = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_CONTROL1"].ToString());
                CV_CONTROL2     = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_CONTROL2"].ToString());
                CV_CONTROL3     = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_CONTROL3"].ToString());
                CV_TRK_PAUSE    = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_TRK_PAUSE"].ToString());

                BCR_NO          = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["BCR_NO"].ToString());
                BCR_READ_STA    = "" + pBdb.mDtMain.Rows[0]["BCR_READ_STA"].ToString();
                BCR_COM_STA     = "" + pBdb.mDtMain.Rows[0]["BCR_COM_STA"].ToString();
                BCR_RCV_VAL1    = "" +  pBdb.mDtMain.Rows[0]["BCR_RCV_VAL1"].ToString();
                BCR_RCV_VAL2    = "" + pBdb.mDtMain.Rows[0]["BCR_RCV_VAL2"].ToString();
                BCR_RCV_VAL3    = "" + pBdb.mDtMain.Rows[0]["BCR_RCV_VAL3"].ToString();
                BCR_ERR_CODE    = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["BCR_ERR_CODE"].ToString());

                BCR_ERR_OC_DT   = KjhToDataTime(pBdb.mDtMain.Rows[0]["BCR_ERR_OC_DT"].ToString());
                BCR_RCV_DT      = KjhToDataTime(pBdb.mDtMain.Rows[0]["BCR_RCV_DT"].ToString());

                WGT_NO          = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["WGT_NO"].ToString());
                WGT_READ_STA    = "" + pBdb.mDtMain.Rows[0]["WGT_READ_STA"].ToString();
                WGT_COM_STA     = "" +   pBdb.mDtMain.Rows[0]["WGT_COM_STA"].ToString();
                WGT_RCV_VAL     = Convert.ToDouble("0" + pBdb.mDtMain.Rows[0]["WGT_RCV_VAL"].ToString());
                WGT_ERR_CODE    = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["WGT_ERR_CODE"].ToString());


                OD_CMD_CV               = "" + pBdb.mDtMain.Rows[0]["OD_CMD_CV"].ToString();
                OD_CMD_DT               = KjhToDataTime(pBdb.mDtMain.Rows[0]["OD_CMD_DT"].ToString());
                RT_CMD_CV               = "" + pBdb.mDtMain.Rows[0]["RT_CMD_CV"].ToString();
                RT_CMD_DT               = KjhToDataTime(pBdb.mDtMain.Rows[0]["RT_CMD_DT"].ToString());

                
                WGT_ERR_OC_DT           = KjhToDataTime(pBdb.mDtMain.Rows[0]["WGT_ERR_OC_DT"].ToString());
                WGT_RCV_DT              = KjhToDataTime(pBdb.mDtMain.Rows[0]["WGT_RCV_DT"].ToString());
                WGT_ERR_OC_DT           = KjhToDataTime(pBdb.mDtMain.Rows[0]["WGT_ERR_OC_DT"].ToString());
                WGT_RCV_DT              = KjhToDataTime(pBdb.mDtMain.Rows[0]["WGT_RCV_DT"].ToString());
                SITENO                  = "" + pBdb.mDtMain.Rows[0]["SITENO"].ToString();
                TRACK_NO                = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString());
                BAGGING_WC_YON          = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_WC_YON"].ToString());
                BAGGING_INSPECTION_YON  = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_INSPECTION_YON"].ToString());
                LINE_ID                 = "" + pBdb.mDtMain.Rows[0]["LINE_ID"].ToString();
                BAGGING_LABEL_YON       = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_LABEL_YON"].ToString());
                BAGGING_START_YON       = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_START_YON"].ToString());
                BREAKING_SCAN_YON       = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BREAKING_SCAN_YON"].ToString());
                BREAKING_CLEAN_YON      = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BREAKING_CLEAN_YON"].ToString());
                PA_HS_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PA_HS_YON"].ToString());
                PK_HS_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PK_HS_YON"].ToString());
                PA_MC_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PA_MC_YON"].ToString());
                PK_MC_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PK_MC_YON"].ToString());
                PS_MC_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PS_MC_YON"].ToString());
                PP_MC_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PP_MC_YON"].ToString());
                BOX_AUTO_RET_YON        = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BOX_AUTO_RET_YON"].ToString());
                BAGGING_START_WC_YON    = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_START_WC_YON"].ToString());
                BAGGING_START_BCR_YON   = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_START_BCR_YON"].ToString());
                BAGGING_EMG_LINE_YON    = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_EMG_LINE_YON"].ToString());
                BAGGING_OP_GRADE_MIS_LINE_YON = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_OP_GRADE_MIS_LINE_YON"].ToString());

                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }

        public DataTable GetTrackNoToDataTable(int TrackNo, ref int nSelCnt)
        {
            try
            {
                IsRtnMsg = "GetTrackNo::";

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
                strSql += CRLF + "          ,Y.BAGGING_START_WC_YON  ";
                strSql += CRLF + "          ,Y.BAGGING_START_BCR_YON ";
                strSql += CRLF + "          ,Y.BAGGING_EMG_LINE_YON         ";
                strSql += CRLF + "          ,Y.BAGGING_OP_GRADE_MIS_LINE_YON";
                strSql += CRLF + "   FROM  MC_STA_MST X              ";
                strSql += CRLF + "        ,TRACKINFO  Y              ";
                strSql += CRLF + "  WHERE  X.WH_TYP     = Y.WH_TYP   ";
                strSql += CRLF + "    AND  X.MC_TYP     = Y.MC_TYP   ";
                strSql += CRLF + "    AND  X.IF_CV_NO   = Y.TRACK_NO ";
                strSql += CRLF + "    AND  X.WH_TYP     = ?          ";
                strSql += CRLF + "    AND  X.MC_TYP     = ?          ";
                strSql += CRLF + "    AND  X.IF_CV_NO   = ?          ";

                this._pBdb.mComMain.CommandType = CommandType.Text;
                this._pBdb.mComMain.Parameters.Clear();
                this._pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                this._pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                this._pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = this._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    IsRtnMsg += this._pBdb.ErrMsg;
                    return this._pBdb.mDtMain;
                }
                else if (nSelCnt == 0)
                {

                    IsRtnMsg = "요청자료 없음";
                    return this._pBdb.mDtMain;
                }
                else
                {
                    IsRtnMsg += "요청자료 가져오기 성공";
                    return this._pBdb.mDtMain;
                }

            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return this._pBdb.mDtMain;
        }

        public string GetTrackNo()
        {
            return TRACK_NO.ToString("000");
        }
        #endregion GetTrackNo

        /* 
         * 한 트랙정보를 읽어옴.
         * GetSITENO(MC_STA_MST)
         */
        #region GetSITENO
        public bool GetSITENO(string SITENO)
        {
            int nSelCnt = 0;
            return GetSITENO(ref this._pBdb, SITENO, ref nSelCnt);
        }
        public bool GetSITENO(string SITENO, ref int nSelCnt)
        {
            return GetSITENO(ref this._pBdb, SITENO, ref nSelCnt);
        }
#if ORACLE
        public bool GetSITENO(ref cDbUse pBdb, string strSITENO, ref int nSelCnt)
#elif POSTGRESQL || SQL
        public bool GetSITENO(ref cDbPostUse pBdb, string strSITENO, ref int nSelCnt)
#endif
        {
            try
            {
                IsRtnMsg = "GetTrackNo::";

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
                //2010.01 조형준 추가---------------------------------
                strSql += CRLF + "          ,X.CV_CONTROL            ";
                //----------------------------------------------------
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
                strSql += CRLF + "          ,Y.BAGGING_START_WC_YON  ";
                strSql += CRLF + "   FROM  MC_STA_MST X              ";
                strSql += CRLF + "        ,TRACKINFO  Y              ";
                strSql += CRLF + "  WHERE  X.WH_TYP     = Y.WH_TYP   ";
                strSql += CRLF + "    AND  X.MC_TYP     = Y.MC_TYP   ";
                strSql += CRLF + "    AND  X.IF_CV_NO   = Y.TRACK_NO ";
                strSql += CRLF + "    AND  X.WH_TYP     = ?          ";
                strSql += CRLF + "    AND  X.MC_TYP     = ?          ";
                strSql += CRLF + "    AND  X.SITENO     = ?          ";

                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strSITENO;
                nSelCnt = pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }


                CV_JOB_KIND         = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_JOB_KIND"].ToString());
                CV_LUGGNO           = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_LUGGNO"].ToString());
                CV_SOUR_SITE        = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_SOUR_SITE"].ToString());
                CV_DEST_SITE        = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_DEST_SITE"].ToString());
                CV_DATA_STA         = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_DATA_STA"].ToString());
                CV_PA_RD            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_PA_RD"].ToString());
                CV_PK_RD            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_PK_RD"].ToString());
                CV_HA_RD            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_HA_RD"].ToString());
                CV_HK_RD            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_HK_RD"].ToString());
                CV_LG_EX            = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LG_EX"].ToString());
                CV_ERR_CODE         = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_ERR_CODE"].ToString());
                CV_BOX_TYP          = "" + pBdb.mDtMain.Rows[0]["CV_BOX_TYP"].ToString();
                CV_JEPUM_GRADE      = "" + pBdb.mDtMain.Rows[0]["CV_JEPUM_GRADE"].ToString();
                CV_BOX_START        = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_BOX_START"].ToString());
                CV_BOX_DEST         = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_BOX_DEST"].ToString());
                CV_LGV_START        = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LGV_START"].ToString());
                CV_LGV_DEST         = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LGV_DEST"].ToString());
                CV_WEIGHT_STATION   = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_WEIGHT_STATION"].ToString());
                CV_MERGE_STATION    = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_MERGE_STATION"].ToString());
                CV_REMAIN_STATION   = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_REMAIN_STATION"].ToString());
                CV_LIFTER_START     = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LIFTER_START"].ToString());
                CV_LIFTER_DEST      = KjhToBoolean(pBdb.mDtMain.Rows[0]["CV_LIFTER_DEST"].ToString());
                CV_BCD_DATA         = "" + pBdb.mDtMain.Rows[0]["CV_BCD_DATA"].ToString();
                CV_BCD_DATA         = CV_BCD_DATA.Trim();


                CV_IO_TYP       = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_IO_TYP"].ToString());
                CV_IO_MODE      = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_IO_MODE"].ToString());
                MOUDLE_ID       = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["MOUDLE_ID"].ToString());
                DEFAULT_TRK_POS = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["DEFAULT_TRK_POS"].ToString());

                CV_SENSOR       = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_SENSOR"].ToString());

                CV_CONTROL1     = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_CONTROL1"].ToString());
                CV_CONTROL2     = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_CONTROL2"].ToString());
                CV_CONTROL3     = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_CONTROL3"].ToString());
                CV_TRK_PAUSE    = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_TRK_PAUSE"].ToString());
                CV_CONTROL      = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["CV_CONTROL"].ToString());

                BCR_NO          = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["BCR_NO"].ToString());
                BCR_READ_STA    = "" + pBdb.mDtMain.Rows[0]["BCR_READ_STA"].ToString();
                BCR_COM_STA     = "" + pBdb.mDtMain.Rows[0]["BCR_COM_STA"].ToString();
                BCR_RCV_VAL1    = "" +  pBdb.mDtMain.Rows[0]["BCR_RCV_VAL1"].ToString();
                BCR_RCV_VAL2    = "" + pBdb.mDtMain.Rows[0]["BCR_RCV_VAL2"].ToString();
                BCR_RCV_VAL3    = "" + pBdb.mDtMain.Rows[0]["BCR_RCV_VAL3"].ToString();
                BCR_ERR_CODE    = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["BCR_ERR_CODE"].ToString());

                BCR_ERR_OC_DT   = KjhToDataTime(pBdb.mDtMain.Rows[0]["BCR_ERR_OC_DT"].ToString());
                BCR_RCV_DT      = KjhToDataTime(pBdb.mDtMain.Rows[0]["BCR_RCV_DT"].ToString());

                WGT_NO          = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["WGT_NO"].ToString());
                WGT_READ_STA    = "" + pBdb.mDtMain.Rows[0]["WGT_READ_STA"].ToString();
                WGT_COM_STA     = "" +   pBdb.mDtMain.Rows[0]["WGT_COM_STA"].ToString();
                WGT_RCV_VAL     = Convert.ToDouble("0" + pBdb.mDtMain.Rows[0]["WGT_RCV_VAL"].ToString());
                WGT_ERR_CODE    = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["WGT_ERR_CODE"].ToString());


                OD_CMD_CV       = "" + pBdb.mDtMain.Rows[0]["OD_CMD_CV"].ToString();
                OD_CMD_DT       = KjhToDataTime(pBdb.mDtMain.Rows[0]["OD_CMD_DT"].ToString());
                RT_CMD_CV       = "" + pBdb.mDtMain.Rows[0]["RT_CMD_CV"].ToString();
                RT_CMD_DT       = KjhToDataTime(pBdb.mDtMain.Rows[0]["RT_CMD_DT"].ToString());


                WGT_ERR_OC_DT   = KjhToDataTime(pBdb.mDtMain.Rows[0]["WGT_ERR_OC_DT"].ToString());
                WGT_RCV_DT      = KjhToDataTime(pBdb.mDtMain.Rows[0]["WGT_RCV_DT"].ToString());
                WGT_ERR_OC_DT   = KjhToDataTime(pBdb.mDtMain.Rows[0]["WGT_ERR_OC_DT"].ToString());
                WGT_RCV_DT      = KjhToDataTime(pBdb.mDtMain.Rows[0]["WGT_RCV_DT"].ToString());
                SITENO          = "" + pBdb.mDtMain.Rows[0]["SITENO"].ToString();
                TRACK_NO        = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString());
                BAGGING_WC_YON          = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_WC_YON"].ToString());
                BAGGING_INSPECTION_YON  = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_INSPECTION_YON"].ToString());
                LINE_ID                 = "" + pBdb.mDtMain.Rows[0]["LINE_ID"].ToString();
                BAGGING_LABEL_YON       = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_LABEL_YON"].ToString());
                BAGGING_START_YON       = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_START_YON"].ToString());
                BREAKING_SCAN_YON       = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BREAKING_SCAN_YON"].ToString());
                BREAKING_CLEAN_YON      = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BREAKING_CLEAN_YON"].ToString());
                PA_HS_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PA_HS_YON"].ToString());
                PK_HS_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PK_HS_YON"].ToString());
                PA_MC_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PA_MC_YON"].ToString());
                PK_MC_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PK_MC_YON"].ToString());
                PS_MC_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PS_MC_YON"].ToString());
                PP_MC_YON               = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["PP_MC_YON"].ToString());
                BOX_AUTO_RET_YON        = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BOX_AUTO_RET_YON"].ToString());
                BAGGING_START_WC_YON = KjhToBooleanYN(pBdb.mDtMain.Rows[0]["BAGGING_START_WC_YON"].ToString());


                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion GetSITENO


        /* 
         * 설비상태        private string _PLC_COM_STA;
        private DateTime _PLC_COM_DT;
        private int _ERR_CODE;
         */
        #region GetPlcStat
        public bool GetPlcStat(string TrackNo)
        {
            return GetPlcStat(ref this._pBdb, TrackNo);
        }
#if ORACLE
        public bool GetPlcStat(ref cDbUse pBdb, string TrackNo)
#elif POSTGRESQL || SQL
        public bool GetPlcStat(ref cDbPostUse pBdb, string TrackNo)
#endif
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "GetPlcStat::";

                strSql = "";
                strSql += CRLF + " SELECT  X.PLC_COM_STA,                           ";
                strSql += CRLF + "         X.PLC_COM_DT,                            ";
                strSql += CRLF + "         X.ERR_CODE,                              ";
                strSql += CRLF + "         (" + DbLang.SYSDATE + "-X.PLC_COM_DT)*24*60*60 seconds  ";
                strSql += CRLF + "   FROM  PLC_STA_MST X                ";
                strSql += CRLF + "        ,MC_STA_MST  Y                ";
                strSql += CRLF + "  WHERE  X.WH_TYP       = Y.WH_TYP    ";
                strSql += CRLF + "    AND  X.PLC_NO       = Y.PLC_NO    ";
                strSql += CRLF + "    AND  Y.WH_TYP       = ?           ";
                strSql += CRLF + "    AND  Y.IF_CV_NO     = ?           ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo;
                nSelCnt = pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }

                PLC_COM_STA         = "" + pBdb.mDtMain.Rows[0]["PLC_COM_STA"].ToString();
                PLC_COM_DT          = KjhToDataTime(pBdb.mDtMain.Rows[0]["PLC_COM_DT"].ToString());
                ERR_CODE            = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["ERR_CODE"].ToString());
                int Seconds         = Convert.ToInt16("0" + pBdb.mDtMain.Rows[0]["seconds"].ToString());

                if (Seconds>10) return false;

                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        public bool GetPlcStat()
        {
            if (PLC_COM_STA!=CmdResultOk) return false;

            TimeSpan x = System.DateTime.Now - PLC_COM_DT;
            if (x.Seconds>10) return false;


            return true;
        }
        #endregion GetTrackNo

        /* 
         * BCR 읽기요청.
         * RequestReadBCR(MC_STA_MST)
         */
        #region RequestReadBCR, SuccessReadBCR, SettingReadBCR
        public bool RequestReadBCR(int TrackNo)
        {
            return SettingReadBCR(ref this._pBdb, TrackNo, "RQ");
        }
#if ORACLE
        public bool RequestReadBCR(ref cDbUse pBdb, int TrackNo)
#elif POSTGRESQL || SQL
        public bool RequestReadBCR(ref cDbPostUse pBdb, int TrackNo)
#endif
        {
            return SettingReadBCR(ref pBdb, TrackNo, "RQ");
        }
        public bool SuccessReadBCR(int TrackNo)
        {
            return SettingReadBCR(ref this._pBdb, TrackNo, "OD");
        }
#if ORACLE
        public bool SuccessReadBCR(ref cDbUse pBdb, int TrackNo)
#elif POSTGRESQL || SQL
        public bool SuccessReadBCR(ref cDbPostUse pBdb, int TrackNo)
#endif
        {
            return SettingReadBCR(ref pBdb, TrackNo, "OD");
        }
        public bool SuccessReadBCR2(int TrackNo)
        {
            return SettingReadBCR2(ref this._pBdb, TrackNo, "OD", "", "", "");
        }
#if ORACLE
        public bool SuccessReadBCR2(ref cDbUse pBdb, int TrackNo)
#elif POSTGRESQL || SQL
        public bool SuccessReadBCR2(ref cDbPostUse pBdb, int TrackNo)
#endif
        {
            return SettingReadBCR2(ref pBdb, TrackNo, "OD", "", "", "");
        }

        public bool SuccessReadBCR3(int TrackNo, int BcrErrorCode)
        {
            return SettingReadBCR3(ref this._pBdb, TrackNo, "OD", "", "", "", BcrErrorCode);
        }

        public bool SettingReadBCR(int TrackNo, string Command)
        {
            return SettingReadBCR(ref this._pBdb, TrackNo, Command);
        }
#if ORACLE
        public bool SettingReadBCR(ref cDbUse pBdb, int TrackNo, string Command)
#elif POSTGRESQL || SQL
        public bool SettingReadBCR(ref cDbPostUse pBdb, int TrackNo, string Command)
#endif
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "SettingReadBCR::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST       ";
                strSql += CRLF + "    SET  BCR_READ_STA = ? ";
                strSql += CRLF + "  WHERE  WH_TYP       = ? ";
                strSql += CRLF + "    AND  MC_TYP       = ? ";
                strSql += CRLF + "    AND  IF_CV_NO     = ? ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }

                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
#if ORACLE
        public bool SettingReadBCR2(ref cDbUse pBdb, int TrackNo, string Command, string BCR_RCV_VAL1, string BCR_RCV_VAL2, string BCR_RCV_VAL3)
#elif POSTGRESQL || SQL
        public bool SettingReadBCR2(ref cDbPostUse pBdb, int TrackNo, string Command, string BCR_RCV_VAL1, string BCR_RCV_VAL2, string BCR_RCV_VAL3)
#endif
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "SettingReadBCR::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST                           ";
                strSql += CRLF + "    SET  BCR_READ_STA     = ?                 ";
                strSql += CRLF + "        ,BEF_BCR_RCV_VAL1 = BCR_RCV_VAL1      ";
                strSql += CRLF + "        ,BEF_BCR_RCV_VAL2 = BCR_RCV_VAL2      ";
                strSql += CRLF + "        ,BEF_BCR_RCV_VAL3 = BCR_RCV_VAL3      ";
                strSql += CRLF + "        ,BCR_RCV_VAL1     = ?                 ";
                strSql += CRLF + "        ,BCR_RCV_VAL2     = ?                 ";
                strSql += CRLF + "        ,BCR_RCV_VAL3     = ?                 ";
                strSql += CRLF + "  WHERE  WH_TYP           = ?                 ";
                strSql += CRLF + "    AND  MC_TYP           = ?                 ";
                strSql += CRLF + "    AND  IF_CV_NO         = ?                 ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BCR_RCV_VAL1;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BCR_RCV_VAL2;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BCR_RCV_VAL3;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }

                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
#if ORACLE
        public bool SettingReadBCR3(ref cDbUse pBdb, int TrackNo, string Command, string BCR_RCV_VAL1, string BCR_RCV_VAL2, string BCR_RCV_VAL3, int BcrErrorCode)
#elif POSTGRESQL || SQL
        public bool SettingReadBCR3(ref cDbPostUse pBdb, int TrackNo, string Command, string BCR_RCV_VAL1, string BCR_RCV_VAL2, string BCR_RCV_VAL3, int BcrErrorCode)
#endif
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "SettingReadBCR::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST                           ";
                strSql += CRLF + "    SET  BCR_READ_STA     = ?                 ";
                strSql += CRLF + "        ,BEF_BCR_RCV_VAL1 = BCR_RCV_VAL1      ";
                strSql += CRLF + "        ,BEF_BCR_RCV_VAL2 = BCR_RCV_VAL2      ";
                strSql += CRLF + "        ,BEF_BCR_RCV_VAL3 = BCR_RCV_VAL3      ";
                strSql += CRLF + "        ,BCR_RCV_VAL1     = ?                 ";
                strSql += CRLF + "        ,BCR_RCV_VAL2     = ?                 ";
                strSql += CRLF + "        ,BCR_RCV_VAL3     = ?                 ";
                strSql += CRLF + "        ,BCR_ERR_CODE     = ?                 ";
                strSql += CRLF + "  WHERE  WH_TYP           = ?                 ";
                strSql += CRLF + "    AND  MC_TYP           = ?                 ";
                strSql += CRLF + "    AND  IF_CV_NO         = ?                 ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BCR_RCV_VAL1;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BCR_RCV_VAL2;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BCR_RCV_VAL3;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BcrErrorCode.ToString("000");
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }

                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion RequestReadBCR

        #region[MC_STA_MST - Update CV_CONTROL1]
        public bool SettingCVControl(int TrackNo, string Command)
        {
            return SettingCVControl(ref this._pBdb, TrackNo, Command);
        }
#if ORACLE
        public bool SettingCVControl(ref cDbUse pBdb, int TrackNo, string Command)
#elif POSTGRESQL || SQL
        public bool SettingCVControl(ref cDbPostUse pBdb, int TrackNo, string Command)
#endif
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg = "SettingReadBCR::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST       ";
                strSql += CRLF + "    SET  CV_CONTROL1 = ? ";
                strSql += CRLF + "  WHERE  WH_TYP       = ? ";
                strSql += CRLF + "    AND  MC_TYP       = ? ";
                strSql += CRLF + "    AND  IF_CV_NO     = ? ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }

                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion

        #region[MC_STA_MST - Update CV_CONTROL1]
        public bool SettingCVFlag(int TrackNo, string Command)
        {
            return SettingCVFlag(ref this._pBdb, TrackNo, Command);
        }
#if ORACLE
        public bool SettingCVFlag(ref cDbUse pBdb, int TrackNo, string Command)
#elif POSTGRESQL || SQL
        public bool SettingCVFlag(ref cDbPostUse pBdb, int TrackNo, string Command)
#endif
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg = "SettingReadBCR::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST       ";
                strSql += CRLF + "    SET  CV_CONTROL3 = ? ";
                strSql += CRLF + "  WHERE  WH_TYP       = ? ";
                strSql += CRLF + "    AND  MC_TYP       = ? ";
                strSql += CRLF + "    AND  IF_CV_NO     = ? ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }

                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion

        /* 
         * Weighter 읽기요청.
         * RequestReadWC(MC_STA_MST)
         */
        #region RequestReadWC, SuccessReadWC, SettingReadWC
        public bool RequestReadWC(int TrackNo)
        {
            return SettingReadWC(ref this._pBdb, TrackNo, "RQ");
        }
#if ORACLE
        public bool RequestReadWC(ref cDbUse pBdb, int TrackNo)
#elif POSTGRESQL || SQL
        public bool RequestReadWC(ref cDbPostUse pBdb, int TrackNo)
#endif
        {
            return SettingReadWC(ref pBdb, TrackNo, "RQ");
        }
        public bool SuccessReadWC(int TrackNo)
        {
            return SettingReadWC(ref this._pBdb, TrackNo, "OD");
        }
        public bool SuccessReadWC2(int TrackNo)
        {
            return SettingReadWC2(ref this._pBdb, TrackNo, "OD", 0.0);
        }
#if ORACLE
        public bool SuccessReadWC(ref cDbUse pBdb, int TrackNo)
#elif POSTGRESQL || SQL
        public bool SuccessReadWC(ref cDbPostUse pBdb, int TrackNo)
#endif
        {
            return SettingReadWC(ref pBdb, TrackNo, "OD");
        }
        public bool SettingReadWC(int TrackNo, string Command)
        {
            return SettingReadWC(ref this._pBdb, TrackNo, Command);
        }
        public bool SettingReadWC2(int TrackNo, string Command)
        {
            return SettingReadWC2(ref this._pBdb, TrackNo, Command, 0.0);
        }
#if ORACLE
        public bool SettingReadWC(ref cDbUse pBdb, int TrackNo, string Command)
#elif POSTGRESQL || SQL
        public bool SettingReadWC(ref cDbPostUse pBdb, int TrackNo, string Command)
#endif
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg = "SettingReadWC::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST       ";
                strSql += CRLF + "    SET  WGT_READ_STA = ? ";
                strSql += CRLF + "  WHERE  WH_TYP       = ? ";
                strSql += CRLF + "    AND  MC_TYP       = ? ";
                strSql += CRLF + "    AND  IF_CV_NO     = ? ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }

                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
#if ORACLE
        public bool SettingReadWC2(ref cDbUse pBdb, int TrackNo, string Command, double WGT_RCV_VAL)
#elif POSTGRESQL || SQL
        public bool SettingReadWC2(ref cDbPostUse pBdb, int TrackNo, string Command, double WGT_RCV_VAL)
#endif
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg = "SettingReadWC::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST       ";
                strSql += CRLF + "    SET  WGT_READ_STA = ? ";
                strSql += CRLF + "        ,WGT_RCV_VAL  = ? ";
                strSql += CRLF + "  WHERE  WH_TYP       = ? ";
                strSql += CRLF + "    AND  MC_TYP       = ? ";
                strSql += CRLF + "    AND  IF_CV_NO     = ? ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WGT_RCV_VAL;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }

                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion GetTrackNo

        /* 
         * Cv Control비트 쓰기, Cv BoxId 쓰기  , Cv Data 쓰기
         * WriteControlToTrack, WriteBoxToTrack, WriteDataToTrack, WriteNormalToTrack, WriteToTrack
         */
        #region WriteControlToTrack, WriteBoxToTrack, WriteDataToTrack, WriteNormalToTrack, WriteToTrack
        public bool WriteControlToTrack(string TrackNo, int Control)
        {
            return WriteToTrack(ref this._pBdb, TrackNo,
                                          CV_CMD_WRITE_CONTROL,
                                          false, 0, 0, 0, "", "",
                                          true, Control,
                                          false, null);
        }
        public bool WriteControlToTrack(int TrackNo, int Control)
        {
            return WriteToTrack(TrackNo.ToString("000"),
                                CV_CMD_WRITE_CONTROL,
                                false, 0, 0, 0, "", "",
                                true, Control,
                                false, null);
        }
#if ORACLE
        public bool WriteControlToTrack(ref cDbUse pBdb, string TrackNo, int Control)
#elif POSTGRESQL || SQL
        public bool WriteControlToTrack(ref cDbPostUse pBdb, string TrackNo, int Control)
#endif
        {
            Control = 1 << Control;
            return WriteToTrack(ref pBdb, TrackNo,
                                          CV_CMD_WRITE_CONTROL,
                                          false, 0, 0, 0, "", "",
                                          true, Control,
                                          false, null);
        }


        public bool WriteBoxToTrack(string TrackNo, string BOXID)
        {
            return WriteToTrack(ref this._pBdb, TrackNo,
                                          CV_CMD_WRT_DATA_BOX,
                                          false, 0, 0, 0, "", "",
                                          false, 0,
                                          true, BOXID);
        }
#if ORACLE
        public bool WriteBoxToTrack(ref cDbUse pBdb, string TrackNo, string BOXID)
#elif POSTGRESQL || SQL
        public bool WriteBoxToTrack(ref cDbPostUse pBdb, string TrackNo, string BOXID)
#endif
        {
            return WriteToTrack(ref pBdb, TrackNo,
                                          CV_CMD_WRT_DATA_BOX,
                                          false, 0, 0, 0, "", "",
                                          false, 0,
                                          true, BOXID);
        }


        public bool WriteDataToTrack(string TrackNo, int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade)
        {
            return WriteToTrack(ref this._pBdb, TrackNo,
                                          CV_CMD_WRITE_DATA,
                                          true, JobType, LugNo, DestPos, BoxType, ProdGrade,
                                          false, 0,
                                          false, null);
        }
        public bool WriteDataToTrack(int TrackNo, int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade)
        {
            return WriteToTrack(ref this._pBdb, TrackNo.ToString("000"),
                                          CV_CMD_WRITE_DATA,
                                          true, JobType, LugNo, DestPos, BoxType, ProdGrade,
                                          false, 0,
                                          false, null);
        }
#if ORACLE
        public bool WriteDataToTrack(ref cDbUse pBdb, string TrackNo,
                                                       int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade)
#elif POSTGRESQL || SQL
        public bool WriteDataToTrack(ref cDbPostUse pBdb, string TrackNo,
                                                       int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade)
#endif
        {
            return WriteToTrack(ref pBdb, TrackNo,
                                          CV_CMD_WRITE_DATA,
                                          true, JobType, LugNo, DestPos, BoxType, ProdGrade,
                                          false, 0,
                                          false, null);
        }


        public bool WriteNormalToTrack(int TrackNo,
                                       int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade,
                                       string BoxId)
        {
            return WriteToTrack(TrackNo.ToString("000"),
                                CV_CMD_WRT_DATA_BOX,
                                true, JobType, LugNo, DestPos, BoxType, ProdGrade,
                                false, 0,
                                true, BoxId);
        }
        public bool WriteNormalToTrack(string TrackNo,
                                       int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade,
                                       string BoxId)
        {
            return WriteToTrack(ref this._pBdb, TrackNo,
                                          CV_CMD_WRT_DATA_BOX,
                                          true, JobType, LugNo, DestPos, BoxType, ProdGrade,
                                          false, 0,
                                          true, BoxId);
        }
#if ORACLE
        public bool WriteNormalToTrack(ref cDbUse pBdb, string TrackNo,
                                                   int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade,
                                                   string BoxId)
#elif POSTGRESQL || SQL
        public bool WriteNormalToTrack(ref cDbPostUse pBdb, string TrackNo,
                                                   int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade,
                                                   string BoxId)
#endif
        {
            return WriteToTrack(ref pBdb, TrackNo,
                                          CV_CMD_WRT_DATA_BOX,
                                          true, JobType, LugNo, DestPos, BoxType, ProdGrade,
                                          false, 0,
                                          true, BoxId);
        }



        public bool WriteToTrack(string TrackNo,
                                                   string Command,
                                                   bool WriteJobYon,
                                                   int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade,
                                                   bool WriteControlYoN, int Control,
                                                   bool WriteBoxYoN, string strBoxID)
        {
            return WriteToTrack(ref this._pBdb, TrackNo,
                                                   Command,
                                                   WriteJobYon,
                                                   JobType, LugNo, DestPos, BoxType, ProdGrade,
                                                   WriteControlYoN, Control,
                                                   WriteBoxYoN, strBoxID);
        }
#if ORACLE
        public bool WriteToTrack(ref cDbUse pBdb, string TrackNo,
                                                   string Command,
                                                   bool WriteJobYon,
                                                   int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade,
                                                   bool WriteControlYoN, int Control,
                                                   bool WriteBoxYoN, string strBoxID)
#elif POSTGRESQL || SQL
        public bool WriteToTrack(ref cDbPostUse pBdb, string TrackNo,
                                                   string Command,
                                                   bool WriteJobYon,
                                                   int JobType, int LugNo, int DestPos, string BoxType, string ProdGrade,
                                                   bool WriteControlYoN, int Control,
                                                   bool WriteBoxYoN, string strBoxID)
#endif
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "WriteToTrack::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST               ";
                strSql += CRLF + "    SET                           ";
                strSql += CRLF + "         OD_CMD_CV      = ?       ";
                strSql += CRLF + "        ,OD_CMD_DT      = " + DbLang.SYSDATE + " ";
                if (WriteJobYon)
                {
                    strSql += CRLF + "        ,OD_JOB_KIND    = ? ";
                    strSql += CRLF + "        ,OD_LGNO_CV     = ? ";
                    strSql += CRLF + "        ,OD_DEST_SITE   = ? ";
                    strSql += CRLF + "        ,OD_BOX_TYP     = ? ";
                    strSql += CRLF + "        ,OD_JEPUM_GRADE = ? ";
                }
                if (WriteControlYoN)
                {
                    strSql += CRLF + "    ,OD_CONTROL_WRITE_YON  = ? ";
                    strSql += CRLF + "    ,OD_CONTROL            = ? ";
                }
                if (WriteBoxYoN)
                {
                    strSql += CRLF + "    ,OD_BCD_WRITE_YON      = ? ";
                    strSql += CRLF + "    ,OD_BCD_DATA           = ? ";
                }
                strSql += CRLF + "  WHERE  WH_TYP         = ? ";
                strSql += CRLF + "    AND  MC_TYP         = ? ";
                strSql += CRLF + "    AND  IF_CV_NO       = ? ";

                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                if (WriteJobYon)
                {
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = JobType.ToString();
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = LugNo.ToString();
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = DestPos.ToString();
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BoxType.ToString();
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = ProdGrade.ToString();
                }
                if (WriteControlYoN)
                {
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "Y";
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Control.ToString();
                }
                if (WriteBoxYoN)
                {
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "Y";
                    pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = strBoxID.PadRight(20, Convert.ToChar(0x20));
                }
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo;
                nSelCnt = pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }
                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion WriteControlToTrack, WriteBoxToTrack, WriteDataToTrack, WriteNormalToTrack, WriteToTrack
        #region CV_PK_RD update
#if ORACLE
        public bool UpdateCvPkRd(ref cDbUse pBdb, string TrackNo, string CvPkRd)
#elif POSTGRESQL || SQL
        public bool UpdateCvPkRd(ref cDbPostUse pBdb, string TrackNo, string CvPkRd)
#endif
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "UpdateCvPkRd::";

                strSql = "";
                strSql += CRLF + " UPDATE  MC_STA_MST               ";
                strSql += CRLF + "    SET  CV_PK_RD       = ?       ";
                strSql += CRLF + "  WHERE  WH_TYP         = ?       ";
                strSql += CRLF + "    AND  MC_TYP         = ?       ";
                strSql += CRLF + "    AND  IF_CV_NO       = ?       ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = CvPkRd;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = MC_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo;
                nSelCnt = pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }
                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }        
        #endregion

        /*
         * KjhToBoolean
         */
        #region KjhToBoolean
        public bool KjhToBoolean(string Kjh)
        {
            if (Kjh.ToUpper() == "TRUE")    return true;
            if (Kjh.ToUpper() == "FALSE")   return false;
            if (Kjh.ToUpper() == "Y")       return true;
            if (Kjh.ToUpper() == "N")       return false;
            if (Kjh.ToUpper() == "1")       return true;
            if (Kjh.ToUpper() == "0")       return false;
            return false;
        }
        #endregion
        /*
         * KjhToBoolean
         */
        #region KjhToBooleanYN
        public bool KjhToBooleanYN(string Kjh)
        {
            return Kjh=="Y" ? true : false;
        }
        #endregion
        /*
         * KjhToDataTime
         */
        #region KjhToDataTime
        public DateTime KjhToDataTime(string data)
        {
            return (data == "" ? DateTime.Now : Convert.ToDateTime("" + data));
        }
        #endregion

        /* 
         * BCR 읽기요청.
         * RequestWriteLbl(AL_STA_MST)
         */
        #region RequestWriteLBL, SuccessReadLBL, SettingReadLBL
        public bool RequestWriteLBL(int TrackNo, string Data)
        {
            return SettingWriteLBL(ref this._pBdb, TrackNo, "P", Data);
        }
#if ORACLE
        public bool RequestWriteLBL(ref cDbUse pBdb, int TrackNo, string Data)
#elif POSTGRESQL || SQL
        public bool RequestWriteLBL(ref cDbPostUse pBdb, int TrackNo, string Data)
#endif
        {
            return SettingWriteLBL(ref pBdb, TrackNo, "P", Data);
        }
        public bool SuccessWriteLBL(int TrackNo, string Data)
        {
            return SettingWriteLBL(ref this._pBdb, TrackNo, "OD", Data);
        }
#if ORACLE
        public bool SuccessWriteLBL(ref cDbUse pBdb, int TrackNo, string Data)
#elif POSTGRESQL || SQL
        public bool SuccessWriteLBL(ref cDbPostUse pBdb, int TrackNo, string Data)
#endif
        {
            return SettingWriteLBL(ref pBdb, TrackNo, "OD", Data);
        }
        public bool SettingWriteLBL(int TrackNo, string Command, string Data)
        {
            return SettingWriteLBL(ref this._pBdb, TrackNo, Command, Data);
        }
#if ORACLE
        public bool SettingWriteLBL(ref cDbUse pBdb, int TrackNo, string Command, string Data)
#elif POSTGRESQL || SQL
        public bool SettingWriteLBL(ref cDbPostUse pBdb, int TrackNo, string Command, string Data)
#endif
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "SettingWriteLBL::";

                strSql = "";
                strSql += CRLF + " UPDATE  AL_STA_MST               ";
                strSql += CRLF + "    SET  AL_SND_STA   = ?         ";
                strSql += CRLF + "        ,AL_SND_VAL   = ?         ";
                strSql += CRLF + "        ,AL_SND_DT    = " + DbLang.SYSDATE + "   ";
                strSql += CRLF + "  WHERE  WH_TYP       = ?         ";
                strSql += CRLF + "    AND  CV_NO        = ?         ";
                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Command;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = Data;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = WH_TYP;
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = TrackNo.ToString("000");
                nSelCnt = pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg += "변경자료 없음";
                    return false;
                }
                IsRtnMsg += "자료 변경 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion RequestReadBCR


        /*
         * GetJobInfo
         * SC완료작업 구함(GetRetScComplete)
         */
        #region GetJobInfo, GetRetScComplete, SettingJobInfo, SetProcessStep, DeleteJob
        public bool GetJobInfo(int LugNo, ref JobItem Job)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "GetJobInfo::";

                strSql = "";
                strSql += CRLF + "SELECT   LUGGNUM          ";
                strSql += CRLF + "        ,IOGB             ";
                strSql += CRLF + "        ,JOBTYPE          ";
                strSql += CRLF + "        ,COMMANDID        ";
                strSql += CRLF + "        ,PRIORITY         ";
                strSql += CRLF + "        ,CARRIERID        ";
                strSql += CRLF + "        ,SOURCE           ";
                strSql += CRLF + "        ,DEST             ";
                strSql += CRLF + "        ,FINALDEST        ";
                strSql += CRLF + "        ,TRANSFERSRC      ";
                strSql += CRLF + "        ,TRANSFERDST      ";
                strSql += CRLF + "        ,COMMANDTIME      ";
                strSql += CRLF + "        ,INSTALLTIME      ";
                strSql += CRLF + "        ,CARRIERLOC       ";
                strSql += CRLF + "        ,SRCUNITTYPE      ";
                strSql += CRLF + "        ,DSTUNITTYPE      ";
                strSql += CRLF + "        ,PROCESSSTEP      ";
                strSql += CRLF + "        ,NULLYON          ";
                strSql += CRLF + "        ,FRZONE           ";
                strSql += CRLF + "        ,TOZONE           ";
                strSql += CRLF + "        ,WMSJOB           ";
                strSql += CRLF + "        ,BOXINDEX         ";
                strSql += CRLF + "        ,BOXSEQID         ";
                strSql += CRLF + "        ,BOXNO            ";
                strSql += CRLF + "        ,BOXWEIGHT        ";
                strSql += CRLF + "        ,EMPTYBOXWEIGHT   ";
                strSql += CRLF + "        ,LOTNO            ";
                strSql += CRLF + "        ,PRODGBN          ";
                strSql += CRLF + "        ,NPRODGRADE       ";
                strSql += CRLF + "        ,CCID             ";
                strSql += CRLF + "        ,CCCNT            ";
                strSql += CRLF + "        ,URODNO           ";
                strSql += CRLF + "        ,CHUCKREMOVEGBN   ";
                strSql += CRLF + "        ,PRODGRADE        ";
                strSql += CRLF + "        ,WEIGHT           ";
                strSql += CRLF + "        ,PRODUCTGROUP     ";
                strSql += CRLF + "        ,JISIDATE         ";
                strSql += CRLF + "        ,PROCESSZONE      ";
                strSql += CRLF + "        ,MANUALFLAG       ";
                strSql += CRLF + "        ,LABELINFO        ";
                strSql += CRLF + "        ,JOBNODB          ";
                strSql += CRLF + "        ,JOBTYPEDB        ";
                strSql += CRLF + "        ,SCNODB           ";
                strSql += CRLF + "        ,JOBSTAT          ";
                strSql += CRLF + " FROM   JOB_ITEM          ";
                strSql += CRLF + "WHERE   LUGGNUM   =  ?    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = LugNo.ToString("00000");
                nSelCnt = _pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }


                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }

                DataRow x = _pBdb.mDtMain.Rows[0];
                SettingJobInfo(ref x, ref Job);


                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        public bool GetRetScComplete(string SiteNo, ref JobItem Job)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "GetRetScComplete::";

                strSql = "";
                strSql += crlf + "select  *                            ";
                strSql += crlf + " from   job_item                     ";
                strSql += crlf + "where   IOGB          in (?, ?, ?)   ";
                strSql += crlf + "  and   jobstat       =  ?           ";
                strSql += crlf + "  and   dest          =  ?           ";
                strSql += crlf + "  and   rownum        =  1           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "02";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "04";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "06";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = SC_COMPLET;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = SiteNo;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }
                DataRow x = _pBdb.mDtMain.Rows[0];
                SettingJobInfo(ref x, ref Job);
                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        public bool GetRetCvDoing(string SiteNo, ref JobItem Job)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "GetRetScComplete::";

                strSql = "";
                strSql += crlf + "select  *                            ";
                strSql += crlf + " from   job_item                     ";
                strSql += crlf + "where   IOGB          in (?, ?, ?)   ";
                strSql += crlf + "  and   jobstat       =  ?           ";
                strSql += crlf + "  and   dest          =  ?           ";
                strSql += crlf + "  and   rownum        =  1           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "02";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "04";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "06";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = CV_DOING;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = SiteNo;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }
                DataRow x = _pBdb.mDtMain.Rows[0];
                SettingJobInfo(ref x, ref Job);
                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        public bool GetRetBcrJobInfo(string SiteNo, ref JobItem Job)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "GetRetBcrJobInfo::";

                strSql = "";
                strSql += crlf + "select  *                             ";
                strSql += crlf + " from   job_item                      ";
                strSql += crlf + "where   IOGB          in (?, ? , ?)   ";
                strSql += crlf + "  and   jobstat       =  ?            ";
                strSql += crlf + "  and   dest          =  ?            ";
                strSql += crlf + "  and   rownum        =  1            ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "02";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "04";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = "06";
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = BCR_REQUEST;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = SiteNo;
                nSelCnt = _pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }
                DataRow x = _pBdb.mDtMain.Rows[0];
                SettingJobInfo(ref x, ref Job);
                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        public void SettingJobInfo(ref DataRow x, ref JobItem Job)
        {
            Job._strIOGB            = "" + x["IOGB"].ToString();
            Job._strJobType         = "" + x["JOBTYPE"].ToString();
            Job._strCommandID       = "" + x["COMMANDID"].ToString();
            Job._nPriority          = Convert.ToInt16("0" + x["PRIORITY"].ToString());
            Job._strCarrierID       = "" + x["CARRIERID"].ToString();
            Job._strSOURCE          = "" + x["SOURCE"].ToString();
            Job._strDEST            = "" + x["DEST"].ToString();
            Job._strFinalDEST       = "" + x["FINALDEST"].ToString();
            Job._strTransferSRC     = "" + x["TRANSFERSRC"].ToString();
            Job._strTransferDST     = "" + x["TRANSFERDST"].ToString();
            Job._tCommandTIME       = KjhToDataTime(x["COMMANDTIME"].ToString());
            Job._tInstallTIME       = KjhToDataTime(x["INSTALLTIME"].ToString());
            Job._strCarrierLOC      = "" + x["CARRIERLOC"].ToString();
            Job._nSrcUnitType       = Convert.ToInt16("0" +  x["SRCUNITTYPE"].ToString());
            Job._nDstUnitType       = Convert.ToInt16("0" +  x["DSTUNITTYPE"].ToString());
            Job._strProcessStep     = "" +  x["PROCESSSTEP"].ToString();
            Job._bNULL              = KjhToBoolean(x["NULLYON"].ToString());
            Job._strFrZone          = "" + x["FRZONE"].ToString();
            Job._strToZone          = "" + x["TOZONE"].ToString();
            Job._strWmsJob          = "" + x["WMSJOB"].ToString();
            Job._strBoxIndex        = "" + x["BOXINDEX"].ToString();
            Job._strBoxSeqID        = "" + x["BOXSEQID"].ToString();
            Job._strBoxNo           = "" + x["BOXNO"].ToString();
            Job._strBoxWeight       = "" + x["BOXWEIGHT"].ToString();
            Job._strEmptyBoxWeight  = "" + x["EMPTYBOXWEIGHT"].ToString();
            Job._strLotNo           = "" + x["LOTNO"].ToString();
            Job._strProdGbn         = "" + x["PRODGBN"].ToString();
            Job._nProdGrade         = Convert.ToInt16("0" +  x["NPRODGRADE"].ToString());
            Job._strCCID            = "" + x["CCID"].ToString();
            Job._strCCCnt           = "" + x["CCCNT"].ToString();
            Job._strURodNo          = "" + x["URODNO"].ToString();
            Job._strChuckRemoveGbn  = "" + x["CHUCKREMOVEGBN"].ToString();
            Job._strProdGrade       = "" + x["PRODGRADE"].ToString();
            Job._strWeight          = "" + x["WEIGHT"].ToString();
            Job._strProductGroup    = "" + x["PRODUCTGROUP"].ToString();
            Job._strJisiDate        = "" + x["JISIDATE"].ToString();
            Job._strProcessZone     = "" + x["PROCESSZONE"].ToString();
            Job._strManualFlag      = "" + x["MANUALFLAG"].ToString();
            Job._strLabelInfo       = "" + x["LABELINFO"].ToString();
            Job._strJobNoDB         = "" + x["JOBNODB"].ToString();
            Job._strJobTypeDB       = "" + x["JOBTYPEDB"].ToString();
            Job._strScNoDB          = "" + x["SCNODB"].ToString();
            Job._strJobStat         = "" +  x["JOBSTAT"].ToString();
            Job._nLuggNum           =  Convert.ToInt16("0" + x["LUGGNUM"].ToString());


        }
        public bool DeleteJob(int LugNo)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "DeleteJob::";

                strSql = "";
                strSql += CRLF + "delete                    ";
                strSql += CRLF + " FROM   JOB_ITEM          ";
                strSql += CRLF + "WHERE   LUGGNUM   =  ?    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = LugNo.ToString("00000");
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }


                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }


                IsRtnMsg += "요청자료륵 삭제하는데 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }        /*
         * SetProcessStep
         */
        #region c++ SetProcessStep
        //void CJob::SetProcessStep(CJobItem &JobItem)
        //{
        //    CSingleLock SingleLock(&m_csSyncJobList);
        //    S       ingleLock.Lock();

        //    CJobItem* pJobItem = FindLuggNum(JobItem.m_nLuggNum);
        //    if(pJobItem == NULL)
        //        return;

        //    pJobItem->SetProcessStep(JobItem.m_nProcessStep, true);
        //    Backup();
        //}
        #endregion
        #region c++ SetProcessStep
        public bool SetProcessStep(int luggnum, string jobstat)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "SetProcessStep::";

                strSql = "";
                strSql += crlf + "update  Job_Item        ";
                strSql += crlf + "   set  jobstat = ?     ";
                strSql += crlf + " where  luggnum = ?     ";   
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = jobstat;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = luggnum;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }


                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }

                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        public bool SetProcessStep(int luggnum, string jobstat, string jobstep)
        {
            int nSelCnt = 0;
            string strSql = "";

            try
            {
                IsRtnMsg = "SetProcessStep::";

                strSql = "";
                strSql += crlf + "update  Job_Item        ";
                strSql += crlf + "   set  jobstat = ?     ";
                strSql += crlf + "       ,jobstep = ?     ";
                strSql += crlf + " where  luggnum = ?     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = jobstat;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = jobstep;
                _pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = luggnum;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    IsRtnMsg += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }

                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion
        #endregion

        /* 
         * 한 트랙정보를 읽어옴.
         * GetFindLugNo
         */
        #region GetFindLugNo
        public bool GetFindLugNo(int LugNo, ref int nSelCnt)
        {
            return GetFindLugNo(ref this._pBdb, LugNo.ToString("00000"), ref nSelCnt);
        }
        public bool GetFindLugNo(string LugNo, ref int nSelCnt)
        {
            return GetFindLugNo(ref this._pBdb, LugNo, ref nSelCnt);
        }
#if ORACLE
        public bool GetFindLugNo(ref cDbUse pBdb, string LugNo, ref int nSelCnt)
#elif POSTGRESQL || SQL
        public bool GetFindLugNo(ref cDbPostUse pBdb, string LugNo, ref int nSelCnt)
#endif
        {
            try
            {
                IsRtnMsg = "GetFindLugNo::";

                strSql = "";
                strSql += CRLF + " SELECT  *                         ";
                strSql += CRLF + "   FROM  JOB_ITEM   X              ";
                strSql += CRLF + "  WHERE  X.LUGGNUM  = ?            ";

                pBdb.mComMain.CommandType = CommandType.Text;
                pBdb.mComMain.Parameters.Clear();
                pBdb.mComMain.Parameters.Add("?", DbLang.VARCHAR).Value = LugNo;
                nSelCnt = pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    IsRtnMsg += pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    IsRtnMsg = "요청자료 없음";
                    return false;
                }


                IsRtnMsg += "요청자료 가져오기 성공";
                return true;
            }
            catch (Exception ex)
            {
                IsRtnMsg += ex.Message;
            }
            return false;
        }
        #endregion GetTrackNo
    }
}
