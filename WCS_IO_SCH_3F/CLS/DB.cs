using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using Samoh_Lib;

namespace TSK_COMM_IOSCH
{
	public static class DB
	{
		public static cWork[] _MyWork = new cWork[30];
		public static string[] _MyWorkPcName = new string[30];

        //프로시저 선언
        // POSTGRESQL은 프로시저 X
#if ORACLE
        public static Boolean SP_TASK_QCOMMAND(ref CUserDb argBdb, int argLugNum, string argBoxID, ref JobItem argJob, ref string argMsg)
		{
			argBdb.comMain.CommandType = CommandType.StoredProcedure;
			argBdb.comMain.CommandText = "SP_TASK_QCOMMAND";

			argBdb.comMain.Parameters.Clear();
			argBdb.comMain.Parameters.Add("piProdID", DbLang.VARCHAR, 255).Value = "11";
			argBdb.comMain.Parameters.Add("piSiteNo", DbLang.VARCHAR, 255).Value = "P3BP_IP_AGV";
			argBdb.comMain.Parameters.Add("piLotID", DbLang.VARCHAR, 255).Value = "";
			argBdb.comMain.Parameters.Add("piBarcodeID", DbLang.VARCHAR, 255).Value = argBoxID;
			argBdb.comMain.Parameters.Add("piBarcodeSeq", DbLang.VARCHAR, 255).Value = "";
			argBdb.comMain.Parameters.Add("piAgvJobNo", DbLang.VARCHAR, 255).Value = String.Format("{0000}", argLugNum);
			argBdb.comMain.Parameters.Add("piProdGrade", DbLang.VARCHAR, 255).Value = "B";
			argBdb.comMain.Parameters.Add("piWeight", DbLang.VARCHAR, 255).Value = "";
			argBdb.comMain.Parameters.Add("piScNo", DbLang.VARCHAR, 255).Value = "";
			argBdb.comMain.Parameters.Add("piFrLoc", DbLang.VARCHAR, 255).Value = "";
			argBdb.comMain.Parameters.Add("piToLoc", DbLang.VARCHAR, 255).Value = "";
			argBdb.comMain.Parameters.Add("piDirectFlag", DbLang.VARCHAR, 255).Value = "";
			argBdb.comMain.Parameters.Add("piProductGroup", DbLang.VARCHAR, 255).Value = "B";
			argBdb.comMain.Parameters.Add("piUserID", DbLang.VARCHAR, 255).Value = "MCS";
			argBdb.comMain.Parameters.Add("poJobNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poIOGbn", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poScNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poSourID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDestID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poFinalDestID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poFrZone", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poToZone", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poWmsJob", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poBoxIndex", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poBoxSeqID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poBoxNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poBoxWeight", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poEmptyBoxWeight", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poLotNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poProdGbn", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poCCID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poCCCnt", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poURodNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poChuckRemoveGbn", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poProdGrade", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poWeight", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poProductGroup", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poRetCd", DbLang.INTEGER).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poMsg", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDBErr", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

			argBdb.comMain.ExecuteScalar();

			if (System.Convert.ToInt32(argBdb.comMain.Parameters["poRetCd"].Value) != 0)
			{
				argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);
				return false;
			}

			argJob._nLuggNum = System.Convert.ToInt16(argBdb.comMain.Parameters["poJobNo"].Value);
			argJob._strIOGB = System.Convert.ToString(argBdb.comMain.Parameters["poIOGbn"].Value);
			argJob._strScNoDB = System.Convert.ToString(argBdb.comMain.Parameters["poScNo"].Value);
			argJob._strSOURCE = System.Convert.ToString(argBdb.comMain.Parameters["poSourID"].Value);
			argJob._strDEST = System.Convert.ToString(argBdb.comMain.Parameters["poDestID"].Value);
			argJob._strFinalDEST = System.Convert.ToString(argBdb.comMain.Parameters["poFinalDestID"].Value);
			argJob._strFrZone = System.Convert.ToString(argBdb.comMain.Parameters["poFrZone"].Value);
			argJob._strToZone = System.Convert.ToString(argBdb.comMain.Parameters["poToZone"].Value);
			argJob._strWmsJob = System.Convert.ToString(argBdb.comMain.Parameters["poWmsJob"].Value);
			argJob._strBoxIndex = System.Convert.ToString(argBdb.comMain.Parameters["poBoxIndex"].Value);
			argJob._strBoxSeqID = System.Convert.ToString(argBdb.comMain.Parameters["poBoxSeqID"].Value);
			argJob._strBoxNo = System.Convert.ToString(argBdb.comMain.Parameters["poBoxNo"].Value);
			argJob._strBoxWeight = System.Convert.ToString(argBdb.comMain.Parameters["poBoxWeight"].Value);
			argJob._strEmptyBoxWeight = System.Convert.ToString(argBdb.comMain.Parameters["poEmptyBoxWeight"].Value);
			argJob._strLotNo = System.Convert.ToString(argBdb.comMain.Parameters["poLotNo"].Value);
			argJob._strProdGbn = System.Convert.ToString(argBdb.comMain.Parameters["poProdGbn"].Value);
			argJob._strCCID = System.Convert.ToString(argBdb.comMain.Parameters["poCCID"].Value);
			argJob._strCCCnt = System.Convert.ToString(argBdb.comMain.Parameters["poCCCnt"].Value);
			argJob._strURodNo = System.Convert.ToString(argBdb.comMain.Parameters["poURodNo"].Value);
			argJob._strChuckRemoveGbn = System.Convert.ToString(argBdb.comMain.Parameters["poChuckRemoveGbn"].Value);
			argJob._strProdGrade = System.Convert.ToString(argBdb.comMain.Parameters["poProdGrade"].Value);
			argJob._strWeight = System.Convert.ToString(argBdb.comMain.Parameters["poWeight"].Value);
			argJob._strProductGroup = System.Convert.ToString(argBdb.comMain.Parameters["poProductGroup"].Value);

			argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);
			return true;
		}
		public static Boolean SP_TASK_QCOMMAND1(ref CUserDb argBdb, List<string> agrStringList, ref JobItem argJob, ref string argMsg)
		{
			argBdb.comMain.CommandType = CommandType.StoredProcedure;
			argBdb.comMain.CommandText = "SP_TASK_QCOMMAND";

			argBdb.comMain.Parameters.Clear();
			argBdb.comMain.Parameters.Add("piProdID", DbLang.VARCHAR, 255).Value = "" + agrStringList[0];
			argBdb.comMain.Parameters.Add("piSiteNo", DbLang.VARCHAR, 255).Value = "" + agrStringList[1];
			argBdb.comMain.Parameters.Add("piLotID", DbLang.VARCHAR, 255).Value = "" + agrStringList[2];
			argBdb.comMain.Parameters.Add("piBarcodeID", DbLang.VARCHAR, 255).Value = "" + agrStringList[3];
			argBdb.comMain.Parameters.Add("piBarcodeSeq", DbLang.VARCHAR, 255).Value = "" + agrStringList[4];
			argBdb.comMain.Parameters.Add("piAgvJobNo", DbLang.VARCHAR, 255).Value = "" + agrStringList[5];
			argBdb.comMain.Parameters.Add("piProdGrade", DbLang.VARCHAR, 255).Value = "" + agrStringList[6];
			argBdb.comMain.Parameters.Add("piWeight", DbLang.VARCHAR, 255).Value = "" + agrStringList[7];
			argBdb.comMain.Parameters.Add("piScNo", DbLang.VARCHAR, 255).Value = "" + agrStringList[8];
			argBdb.comMain.Parameters.Add("piFrLoc", DbLang.VARCHAR, 255).Value = "" + agrStringList[9];
			argBdb.comMain.Parameters.Add("piToLoc", DbLang.VARCHAR, 255).Value = "" + agrStringList[10];
			argBdb.comMain.Parameters.Add("piDirectFlag", DbLang.VARCHAR, 255).Value = "" + agrStringList[11];
			argBdb.comMain.Parameters.Add("piProductGroup", DbLang.VARCHAR, 255).Value = "" + agrStringList[12];
			argBdb.comMain.Parameters.Add("piUserID", DbLang.VARCHAR, 255).Value = "" + agrStringList[13];
			argBdb.comMain.Parameters.Add("poJobNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poIOGbn", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poScNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poSourID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDestID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poFinalDestID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poFrZone", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poToZone", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poWmsJob", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poBoxIndex", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poBoxSeqID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poBoxNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poBoxWeight", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poEmptyBoxWeight", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poLotNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poProdGbn", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poCCID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poCCCnt", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poURodNo", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poChuckRemoveGbn", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poProdGrade", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poWeight", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poProductGroup", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poRetCd", DbLang.INTEGER).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poMsg", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDBErr", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

			argBdb.comMain.ExecuteScalar();


			if (System.Convert.ToInt32(argBdb.comMain.Parameters["poRetCd"].Value) != 0)
			{
				argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);
				return false;
			}


			argJob._nLuggNum = System.Convert.ToInt16(argBdb.comMain.Parameters["poJobNo"].Value);
			argJob._strIOGB = System.Convert.ToString(argBdb.comMain.Parameters["poIOGbn"].Value);
			argJob._strScNoDB = System.Convert.ToString(argBdb.comMain.Parameters["poScNo"].Value);
			argJob._strSOURCE = System.Convert.ToString(argBdb.comMain.Parameters["poSourID"].Value);
			argJob._strDEST = System.Convert.ToString(argBdb.comMain.Parameters["poDestID"].Value);
			argJob._strFinalDEST = System.Convert.ToString(argBdb.comMain.Parameters["poFinalDestID"].Value);
			argJob._strFrZone = System.Convert.ToString(argBdb.comMain.Parameters["poFrZone"].Value);
			argJob._strToZone = System.Convert.ToString(argBdb.comMain.Parameters["poToZone"].Value);
			argJob._strWmsJob = System.Convert.ToString(argBdb.comMain.Parameters["poWmsJob"].Value);
			argJob._strBoxIndex = System.Convert.ToString(argBdb.comMain.Parameters["poBoxIndex"].Value);
			argJob._strBoxSeqID = System.Convert.ToString(argBdb.comMain.Parameters["poBoxSeqID"].Value);
			argJob._strBoxNo = System.Convert.ToString(argBdb.comMain.Parameters["poBoxNo"].Value);
			argJob._strBoxWeight = System.Convert.ToString(argBdb.comMain.Parameters["poBoxWeight"].Value);
			argJob._strEmptyBoxWeight = System.Convert.ToString(argBdb.comMain.Parameters["poEmptyBoxWeight"].Value);
			argJob._strLotNo = System.Convert.ToString(argBdb.comMain.Parameters["poLotNo"].Value);
			argJob._strProdGbn = System.Convert.ToString(argBdb.comMain.Parameters["poProdGbn"].Value);
			argJob._strCCID = System.Convert.ToString(argBdb.comMain.Parameters["poCCID"].Value);
			argJob._strCCCnt = System.Convert.ToString(argBdb.comMain.Parameters["poCCCnt"].Value);
			argJob._strURodNo = System.Convert.ToString(argBdb.comMain.Parameters["poURodNo"].Value);
			argJob._strChuckRemoveGbn = System.Convert.ToString(argBdb.comMain.Parameters["poChuckRemoveGbn"].Value);
			argJob._strProdGrade = System.Convert.ToString(argBdb.comMain.Parameters["poProdGrade"].Value);
			argJob._strWeight = System.Convert.ToString(argBdb.comMain.Parameters["poWeight"].Value);
			argJob._strProductGroup = System.Convert.ToString(argBdb.comMain.Parameters["poProductGroup"].Value);

			argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);
			return true;
		}

		public static Boolean WMS_SP_CREATE_LUGG_MST(ref CUserDb argBdb, List<string> agrStringList, ref string strOtLuggNo, ref string strOT_WmsCd, ref string strvWmsMsg, ref string OT_nSqlCd, ref string OT_vSqlMsg)
		{
			argBdb.comMain.CommandType = CommandType.StoredProcedure;
			argBdb.comMain.CommandText = "WMS_SP_CREATE_LUGG_MST@DL_OCICHIP";

			argBdb.comMain.Parameters.Clear();
			argBdb.comMain.Parameters.Add("IN_vTRAY_LOC", DbLang.VARCHAR, 255).Value = "" + agrStringList[0];
			argBdb.comMain.Parameters.Add("IN_vTRAY_NO", DbLang.VARCHAR, 255).Value = "" + agrStringList[1];
			argBdb.comMain.Parameters.Add("IN_nJOB_WGT", DbLang.DECIMAL, 255).Value = Convert.ToDecimal("0" + agrStringList[2]);
			argBdb.comMain.Parameters.Add("IN_vLOT_ID", DbLang.VARCHAR, 255).Value = "" + agrStringList[3];
			argBdb.comMain.Parameters.Add("IN_vST_CV_NO", DbLang.VARCHAR, 255).Value = "" + agrStringList[4];
			argBdb.comMain.Parameters.Add("IN_vEND_CV_NO", DbLang.VARCHAR, 255).Value = "" + agrStringList[5];
			argBdb.comMain.Parameters.Add("IN_vITM_CLASS", DbLang.VARCHAR, 255).Value = "" + agrStringList[6];
			argBdb.comMain.Parameters.Add("OT_nLUGG_NO", DbLang.INTEGER, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("OT_nWmsCd", DbLang.INTEGER, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("OT_vWmsMsg", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("OT_nSqlCd", DbLang.INTEGER, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("OT_vSqlMsg", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

			argBdb.comMain.ExecuteScalar();

			if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nWmsCd"].Value) <= 0)
			{
				strvWmsMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vWmsMsg"].Value);
				return false;
			}

			strOtLuggNo = System.Convert.ToString(argBdb.comMain.Parameters["OT_nLUGG_NO"].Value);
			strOT_WmsCd = System.Convert.ToString(argBdb.comMain.Parameters["OT_nWmsCd"].Value);
			strvWmsMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vWmsMsg"].Value);
			OT_nSqlCd = System.Convert.ToString(argBdb.comMain.Parameters["OT_nSqlCd"].Value);
			OT_vSqlMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vSqlMsg"].Value);

			//argJob._nLuggNum = System.Convert.ToInt16(argBdb.comMain.Parameters["poJobNo"].Value);
			//argJob._strIOGB = System.Convert.ToString(argBdb.comMain.Parameters["poIOGbn"].Value);
			//argJob._strScNoDB = System.Convert.ToString(argBdb.comMain.Parameters["poScNo"].Value);
			//argJob._strSOURCE = System.Convert.ToString(argBdb.comMain.Parameters["poSourID"].Value);
			//argJob._strDEST = System.Convert.ToString(argBdb.comMain.Parameters["poDestID"].Value);
			//argJob._strFinalDEST = System.Convert.ToString(argBdb.comMain.Parameters["poFinalDestID"].Value);
			//argJob._strFrZone = System.Convert.ToString(argBdb.comMain.Parameters["poFrZone"].Value);
			//argJob._strToZone = System.Convert.ToString(argBdb.comMain.Parameters["poToZone"].Value);
			//argJob._strWmsJob = System.Convert.ToString(argBdb.comMain.Parameters["poWmsJob"].Value);
			//argJob._strBoxIndex = System.Convert.ToString(argBdb.comMain.Parameters["poBoxIndex"].Value);
			//argJob._strBoxSeqID = System.Convert.ToString(argBdb.comMain.Parameters["poBoxSeqID"].Value);
			//argJob._strBoxNo = System.Convert.ToString(argBdb.comMain.Parameters["poBoxNo"].Value);
			//argJob._strBoxWeight = System.Convert.ToString(argBdb.comMain.Parameters["poBoxWeight"].Value);
			//argJob._strEmptyBoxWeight = System.Convert.ToString(argBdb.comMain.Parameters["poEmptyBoxWeight"].Value);
			//argJob._strLotNo = System.Convert.ToString(argBdb.comMain.Parameters["poLotNo"].Value);
			//argJob._strProdGbn = System.Convert.ToString(argBdb.comMain.Parameters["poProdGbn"].Value);
			//argJob._strCCID = System.Convert.ToString(argBdb.comMain.Parameters["poCCID"].Value);
			//argJob._strCCCnt = System.Convert.ToString(argBdb.comMain.Parameters["poCCCnt"].Value);
			//argJob._strURodNo = System.Convert.ToString(argBdb.comMain.Parameters["poURodNo"].Value);
			//argJob._strChuckRemoveGbn = System.Convert.ToString(argBdb.comMain.Parameters["poChuckRemoveGbn"].Value);
			//argJob._strProdGrade = System.Convert.ToString(argBdb.comMain.Parameters["poProdGrade"].Value);
			//argJob._strWeight = System.Convert.ToString(argBdb.comMain.Parameters["poWeight"].Value);
			//argJob._strProductGroup = System.Convert.ToString(argBdb.comMain.Parameters["poProductGroup"].Value);

			//argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);

			return true;
		}
		public static Boolean SP_TASK_FCOMMAND(ref CUserDb argBdb, ref JobItem argJob, ref string strBaggingOutFlag, ref string strBaggingDestID, ref string argMsg)
		{
			argBdb.comMain.CommandType = CommandType.StoredProcedure;
			argBdb.comMain.CommandText = "Sp_Task_FCommand";


			argBdb.comMain.Parameters.Clear();
			argBdb.comMain.Parameters.Add("piJobNo", DbLang.VARCHAR, 255).Value = argJob._nLuggNum;
			argBdb.comMain.Parameters.Add("piIOGbn", DbLang.VARCHAR, 255).Value = "" + argJob._strIOGB;
			argBdb.comMain.Parameters.Add("piSourID", DbLang.VARCHAR, 255).Value = "" + argJob._strSOURCE;
			argBdb.comMain.Parameters.Add("piDestID", DbLang.VARCHAR, 255).Value = "" + argJob._strDEST;
			argBdb.comMain.Parameters.Add("piFrZone", DbLang.VARCHAR, 255).Value = "" + argJob._strFrZone;
			argBdb.comMain.Parameters.Add("piToZone", DbLang.VARCHAR, 255).Value = "" + argJob._strToZone;
			argBdb.comMain.Parameters.Add("piWmsJob", DbLang.VARCHAR, 255).Value = "" + argJob._strWmsJob;
			argBdb.comMain.Parameters.Add("piBoxIndex", DbLang.VARCHAR, 255).Value = "" + argJob._strBoxIndex;
			argBdb.comMain.Parameters.Add("piBoxSeqID", DbLang.VARCHAR, 255).Value = argJob._strBoxSeqID;
			argBdb.comMain.Parameters.Add("piBoxNo", DbLang.VARCHAR, 255).Value = "" + argJob._strBoxNo;
			argBdb.comMain.Parameters.Add("piBoxWeight", DbLang.VARCHAR, 255).Value = "" + argJob._strBoxWeight;
			argBdb.comMain.Parameters.Add("piEmptyBoxWeight", DbLang.VARCHAR, 255).Value = "" + argJob._strEmptyBoxWeight;
			argBdb.comMain.Parameters.Add("piLotNo", DbLang.VARCHAR, 255).Value = "" + argJob._strLotNo;
			argBdb.comMain.Parameters.Add("piProdGrade", DbLang.VARCHAR, 255).Value = "" + argJob._strProdGrade;
			argBdb.comMain.Parameters.Add("piProdGbn", DbLang.VARCHAR, 255).Value = "" + argJob._strProdGbn;
			argBdb.comMain.Parameters.Add("piCCID", DbLang.VARCHAR, 255).Value = "" + argJob._strCCID;
			argBdb.comMain.Parameters.Add("piCCCnt", DbLang.VARCHAR, 255).Value = "" + argJob._strCCCnt;
			argBdb.comMain.Parameters.Add("piURodNo", DbLang.VARCHAR, 255).Value = "" + argJob._strURodNo;
			argBdb.comMain.Parameters.Add("piChuckRemoveGbn", DbLang.VARCHAR, 255).Value = "" + argJob._strChuckRemoveGbn;
			argBdb.comMain.Parameters.Add("piJisiDate", DbLang.VARCHAR, 255).Value = "" + argJob._strJisiDate;
			argBdb.comMain.Parameters.Add("piProductGroup", DbLang.VARCHAR, 255).Value = "" + argJob._strProductGroup;
			argBdb.comMain.Parameters.Add("piUserID", DbLang.VARCHAR, 255).Value = "MCS";

			argBdb.comMain.Parameters.Add("poBaggingOutFlag", OleDbType.VarChar, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poOutSiteNo", OleDbType.VarChar, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poRetCd", OleDbType.Integer).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poMsg", OleDbType.VarChar, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDBErr", OleDbType.VarChar, 255).Direction = ParameterDirection.Output;

			argBdb.comMain.ExecuteScalar();


			if (System.Convert.ToInt32(argBdb.comMain.Parameters["poRetCd"].Value) != 0)
			{
				argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value) + "          " +
						 System.Convert.ToString(argBdb.comMain.Parameters["poDBErr"].Value);
				return false;
			}

			argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);

			strBaggingOutFlag = System.Convert.ToString(argBdb.comMain.Parameters["poBaggingOutFlag"].Value);
			strBaggingDestID = System.Convert.ToString(argBdb.comMain.Parameters["poOutSiteNo"].Value);
			return true;
		}
		public static Boolean SP_TASK_ACOMMAND(ref CUserDb argBdb, ref JobItem argJob, ref string argMsg)
		{
			argBdb.comMain.CommandType = CommandType.StoredProcedure;
			argBdb.comMain.CommandText = "Sp_Task_ACommand";

			argBdb.comMain.Parameters.Clear();
			argBdb.comMain.Parameters.Add("piJobNo", DbLang.VARCHAR, 255).Value = argJob._nLuggNum;
			argBdb.comMain.Parameters.Add("piIOGbn", DbLang.VARCHAR, 255).Value = "" + argJob._strIOGB;
			argBdb.comMain.Parameters.Add("piSourID", DbLang.VARCHAR, 255).Value = "" + argJob._strSOURCE;
			argBdb.comMain.Parameters.Add("piDestID", DbLang.VARCHAR, 255).Value = "" + argJob._strDEST;
			argBdb.comMain.Parameters.Add("piFrZone", DbLang.VARCHAR, 255).Value = "" + argJob._strFrZone;
			argBdb.comMain.Parameters.Add("piToZone", DbLang.VARCHAR, 255).Value = "" + argJob._strToZone;
			argBdb.comMain.Parameters.Add("piWmsJob", DbLang.VARCHAR, 255).Value = "" + argJob._strWmsJob;
			argBdb.comMain.Parameters.Add("piBoxIndex", DbLang.VARCHAR, 255).Value = "" + argJob._strBoxIndex;
			argBdb.comMain.Parameters.Add("piBoxSeqID", DbLang.VARCHAR, 255).Value = argJob._strBoxSeqID;
			argBdb.comMain.Parameters.Add("piBoxNo", DbLang.VARCHAR, 255).Value = "" + argJob._strBoxNo;
			argBdb.comMain.Parameters.Add("piBoxWeight", DbLang.VARCHAR, 255).Value = "" + argJob._strBoxWeight;
			argBdb.comMain.Parameters.Add("piEmptyBoxWeight", DbLang.VARCHAR, 255).Value = "" + argJob._strEmptyBoxWeight;
			argBdb.comMain.Parameters.Add("piLotNo", DbLang.VARCHAR, 255).Value = "" + argJob._strLotNo;
			argBdb.comMain.Parameters.Add("piProdGrade", DbLang.VARCHAR, 255).Value = "" + argJob._strProdGrade;
			argBdb.comMain.Parameters.Add("piProdGbn", DbLang.VARCHAR, 255).Value = "" + argJob._strProdGbn;
			argBdb.comMain.Parameters.Add("piCCID", DbLang.VARCHAR, 255).Value = "" + argJob._strCCID;
			argBdb.comMain.Parameters.Add("piCCCnt", DbLang.VARCHAR, 255).Value = "" + argJob._strCCCnt;
			argBdb.comMain.Parameters.Add("piURodNo", DbLang.VARCHAR, 255).Value = "" + argJob._strURodNo;
			argBdb.comMain.Parameters.Add("piChuckRemoveGbn", DbLang.VARCHAR, 255).Value = "" + argJob._strChuckRemoveGbn;
			argBdb.comMain.Parameters.Add("piJisiDate", DbLang.VARCHAR, 255).Value = "" + argJob._strJisiDate;
			argBdb.comMain.Parameters.Add("piProductGroup", DbLang.VARCHAR, 255).Value = "" + argJob._strProductGroup;
			argBdb.comMain.Parameters.Add("piUserID", DbLang.VARCHAR, 255).Value = "MCS";

			argBdb.comMain.Parameters.Add("poRetCd", DbLang.INTEGER).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poMsg", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDBErr", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

			argBdb.comMain.ExecuteScalar();


			if (System.Convert.ToInt32(argBdb.comMain.Parameters["poRetCd"].Value) != 0)
			{
				argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value) + "          " +
						 System.Convert.ToString(argBdb.comMain.Parameters["poDBErr"].Value);
				return false;
			}

			argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);
			return true;
		}

		public static Boolean ExecuteMcsUIProcS_LOT(ref CUserDb argBdb, String strReactorID, String strLotStat, String strUserID, ref String strLotID, ref string argMsg)
		{
			argBdb.comMain.CommandType = CommandType.StoredProcedure;
			argBdb.comMain.CommandText = "Sp_Task_SCommand_LOT";

			argBdb.comMain.Parameters.Clear();
			argBdb.comMain.Parameters.Add("piReactorID", DbLang.VARCHAR, 255).Value = strReactorID;
			argBdb.comMain.Parameters.Add("piLotStat", DbLang.VARCHAR, 255).Value = strLotStat;
			argBdb.comMain.Parameters.Add("piUserID", DbLang.VARCHAR, 255).Value = strUserID;

			argBdb.comMain.Parameters.Add("poLotID", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poRetCd", DbLang.INTEGER).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poMsg", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDBErr", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

			argBdb.comMain.ExecuteScalar();

			if (System.Convert.ToInt32(argBdb.comMain.Parameters["poRetCd"].Value) != 0)
			{
				argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value) + "          " +
						 System.Convert.ToString(argBdb.comMain.Parameters["poDBErr"].Value);
				return false;
			}
			strLotID = System.Convert.ToString(argBdb.comMain.Parameters["poLotID"].Value);
			argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);
			return true;
		}
		public static Boolean ExecuteSECS_S2F41(ref CUserDb argBdb,
													String strRCMD, String strFrLotID, String strToLotID,
													String strFrWeight, String strToWeight, String strWeight,
													String strProductID, String strBoxNo, String MergeLotIDs, ref string argMsg)
		{
			argBdb.comMain.CommandType = CommandType.StoredProcedure;
			argBdb.comMain.CommandText = "Sp_SECS_S2F41";

			argBdb.comMain.Parameters.Clear();
			argBdb.comMain.Parameters.Add("piRCMD", DbLang.VARCHAR, 255).Value = strRCMD;
			argBdb.comMain.Parameters.Add("piFrLotID", DbLang.VARCHAR, 255).Value = strFrLotID;
			argBdb.comMain.Parameters.Add("piToLotID", DbLang.VARCHAR, 255).Value = strToLotID;
			argBdb.comMain.Parameters.Add("piFrWeight", DbLang.VARCHAR, 255).Value = strFrWeight;
			argBdb.comMain.Parameters.Add("piToWeight", DbLang.VARCHAR, 255).Value = strToWeight;
			argBdb.comMain.Parameters.Add("piWeight", DbLang.VARCHAR, 255).Value = strWeight;
			argBdb.comMain.Parameters.Add("piProductID", DbLang.VARCHAR, 255).Value = strProductID;
			argBdb.comMain.Parameters.Add("piBoxNo", DbLang.VARCHAR, 255).Value = strBoxNo;
			argBdb.comMain.Parameters.Add("piMergeLotIDs", DbLang.VARCHAR, 255).Value = MergeLotIDs;

			argBdb.comMain.Parameters.Add("poRetCd", DbLang.INTEGER).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poMsg", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDBErr", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

			argBdb.comMain.ExecuteScalar();

			if (System.Convert.ToInt32(argBdb.comMain.Parameters["poRetCd"].Value) != 0)
			{
				argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value) + "          " +
						 System.Convert.ToString(argBdb.comMain.Parameters["poDBErr"].Value);
				return false;
			}

			argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);
			return true;

		}
		public static Boolean ExecuteSECS_S2F41_Remain(ref CUserDb argBdb,
														   String strRCMD, String strFrLotID, String strModuleID,
														   String strLocation, String strToLotID, ref string argMsg)
		{
			argBdb.comMain.CommandType = CommandType.StoredProcedure;
			argBdb.comMain.CommandText = "Sp_SECS_S2F41_Remain";

			argBdb.comMain.Parameters.Clear();
			argBdb.comMain.Parameters.Add("piRCMD", DbLang.VARCHAR, 255).Value = strRCMD;
			argBdb.comMain.Parameters.Add("piFrLotID", DbLang.VARCHAR, 255).Value = strFrLotID;
			argBdb.comMain.Parameters.Add("piToLotID", DbLang.VARCHAR, 255).Value = strToLotID;
			argBdb.comMain.Parameters.Add("piTsNo", DbLang.VARCHAR, 255).Value = strModuleID;
			argBdb.comMain.Parameters.Add("piCellID", DbLang.VARCHAR, 255).Value = strLocation;
			argBdb.comMain.Parameters.Add("piUserID", DbLang.VARCHAR, 255).Value = "MES";

			argBdb.comMain.Parameters.Add("poRetCd", DbLang.INTEGER).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poMsg", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
			argBdb.comMain.Parameters.Add("poDBErr", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

			argBdb.comMain.ExecuteScalar();

			if (System.Convert.ToInt32(argBdb.comMain.Parameters["poRetCd"].Value) != 0)
			{
				argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value) + "          " +
						 System.Convert.ToString(argBdb.comMain.Parameters["poDBErr"].Value);
				return false;
			}

			argMsg = System.Convert.ToString(argBdb.comMain.Parameters["poMsg"].Value);
			return true;

		}
#elif POSTGRESQL || SQL
#endif

    }
}
