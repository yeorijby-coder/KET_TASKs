
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Windows.Forms;
using NpgsqlTypes;
using Npgsql;
//**************************************************
//
// Stored Procedure/Function와 관련된 DB 작업 정의
//
//**************************************************
#if ORACLE
using System.Data.OleDb;
using System.Data.SqlClient;
#endif

#if SQL
using System.Data.SqlClient;
#endif

//*** StoredProcedure 사용 공통함수 정의 *********************************************
namespace TSK_HostCom
{
	static class modSpDb
	{

		public struct sp_Cell_Info
		{

			public string WH_TYP;
			public string AREA;
			public string SC_NO;
			public string BANK;
			public string BAY;
			public string LEV;

			public string CELL_TYP;
		}

		public struct sp_Cell_Dtl
		{
			public string QCT_NO;
			public string INV_STA;
			public string INV_TYP;
			public string PLT_QTY;
			public string SPARE01;
			public string SPARE02;
			public string SPARE03;
			public string SPARE04;
			public string SPARE05;
			public string IN_DATE;
			public string IN_TIME;
		}

		public struct spForm_Btn_D
		{

			public bool blSEL_BTN;
			public bool blINP_BTN;
			public bool blEDT_BTN;
			public bool blDEL_BTN;
			public bool blEXE_BTN;
			public bool blCLS_BTN;
			public bool blPRT_BTN;
			public bool blXLS_BTN;
		}

		public static spForm_Btn_D spForm_Btn;

		public struct spForm_Able_D
		{
			public bool blSEL_Able;
			public bool blINP_Able;
			public bool blEDT_Able;
			public bool blDEL_Able;
			public bool blEXE_Able;
			public bool blPRT_Able;
			public bool blXLS_Able;
		}

		public static spForm_Able_D spForm_Able;
		private const int SP_DB_ERROR = -1;
		private const int SP_FAILURE = 0;
		private const int SP_SUCCESS = 1;
		private static int SP_WmsCd;
		public static string SP_Err_Msg;

		public static string SP_Suc_Msg;
#if !SERVER_PROGRAM
		public static bool g_Log_Last_Dt_Msg = false;
		public static DateTime gLast_Job_Time;
		public static bool gLast_Job_Start = false;
		public static bool gTimeOver_Msg = false;
#endif


		public static void spForm_Btn_Able_False()
		{
			//var _with1 = spForm_Btn;
			//_with1.blSEL_BTN = false;
			//_with1.blINP_BTN = false;
			//_with1.blEDT_BTN = false;
			//_with1.blDEL_BTN = false;
			//_with1.blEXE_BTN = false;
			//_with1.blCLS_BTN = false;
			//_with1.blPRT_BTN = false;
			//_with1.blXLS_BTN = false;
			//var _with2 = spForm_Able;
			//_with2.blSEL_Able = false;
			//_with2.blINP_Able = false;
			//_with2.blEDT_Able = false;
			//_with2.blDEL_Able = false;
			//_with2.blEXE_Able = false;
			//_with2.blPRT_Able = false;
			//_with2.blXLS_Able = false;

			//var _with1 = spForm_Btn;
			spForm_Btn.blSEL_BTN = false;
			spForm_Btn.blINP_BTN = false;
			spForm_Btn.blEDT_BTN = false;
			spForm_Btn.blDEL_BTN = false;
			spForm_Btn.blEXE_BTN = false;
			spForm_Btn.blCLS_BTN = false;
			spForm_Btn.blPRT_BTN = false;
			spForm_Btn.blXLS_BTN = false;
			//var _with2 = spForm_Able;
			spForm_Able.blSEL_Able = false;
			spForm_Able.blINP_Able = false;
			spForm_Able.blEDT_Able = false;
			spForm_Able.blDEL_Able = false;
			spForm_Able.blEXE_Able = false;
			spForm_Able.blPRT_Able = false;
			spForm_Able.blXLS_Able = false;
		}


		// 변환 필요
		// ORACLE  INTEGER  VARCHAR  Numeric Integer
		// MS-SQL  INT      VARCHAR  Decimal Int
#if ORACLE
		public const OleDbType DbTypeChar = OleDbType.VarChar;
		public const OleDbType DbTypeNum = OleDbType.Numeric;
		public const OleDbType DbTypeInt = OleDbType.Integer;
		public const string DbBindVal = ":";
#endif

#if SQL
	    public const SqlDbType DbTypeChar = SqlDbType.VarChar;
        public const SqlDbType DbTypeNVarChar = SqlDbType.NVarChar;
	    public const SqlDbType DbTypeNum = SqlDbType.Decimal;
        public const SqlDbType DbTypeInt = SqlDbType.Int;
        public const string DbBindVal = "@";
#endif

#if POSTGRESSQL
        public const NpgsqlDbType DbTypeChar = NpgsqlDbType.Varchar;
        public const NpgsqlDbType DbTypeNVarChar = NpgsqlDbType.Varchar;    // Post에서는 사용하지 않음 같이 쓰면됨!    출처:https://icodebroker.tistory.com/6429
        public const NpgsqlDbType DbTypeNum = NpgsqlDbType.Numeric;
        public const NpgsqlDbType DbTypeInt = NpgsqlDbType.Integer;
        public const string DbBindVal = "@";
#endif

        private static void Sp_Init_Name_Set(ref CUserDb p_Bdb, string p_strSp_Name)
		{
#if !SERVER_PROGRAM
			gLast_Job_Time = DateTime.Now;
#endif
			SP_Err_Msg = "";
			SP_Suc_Msg = "";

			p_Bdb.comMain.CommandType = CommandType.StoredProcedure;
			p_Bdb.comMain.CommandText = p_strSp_Name;
			p_Bdb.comMain.Parameters.Clear();
		}

		private static void Sp_Rtn_Parameters_Add(ref CUserDb p_Bdb)
		{
			p_Bdb.comMain.Parameters.Add("@OT_nWmsCd", DbTypeInt).Direction = ParameterDirection.Output;
			p_Bdb.comMain.Parameters.Add("@OT_vWmsMsg", DbTypeChar, 500).Direction = ParameterDirection.Output;
			p_Bdb.comMain.Parameters.Add("@OT_nSqlCd", DbTypeInt).Direction = ParameterDirection.Output;
			p_Bdb.comMain.Parameters.Add("@OT_vSqlMsg", DbTypeChar, 500).Direction = ParameterDirection.Output;

			Sp_Display(ref p_Bdb);
		}

		private static void Sp_Rtn_Else_Message(ref CUserDb p_Bdb, int p_iSP_WmsCd, string p_strSp_Name = "", bool p_blMsgView = true)
		{
			string strSp_Name = null;

			if (string.IsNullOrEmpty(p_strSp_Name))
			{
				strSp_Name = p_Bdb.comMain.CommandText;
			}
			else
			{
				strSp_Name = p_strSp_Name;
			}
			switch (p_iSP_WmsCd)
			{
				case SP_FAILURE:
					SP_Err_Msg = Sp_Var_Str("" + p_Bdb.comMain.Parameters["@OT_vWmsMsg"].Value.ToString());
					strSp_Name = strSp_Name + "[Warning]";
					break;
				case SP_DB_ERROR:
					if (Convert.ToInt32(p_Bdb.comMain.Parameters["@OT_nSqlCd"].Value.ToString()) == 0)
					{
						SP_Err_Msg = Sp_Var_Str("" + p_Bdb.comMain.Parameters["@OT_vWmsMsg"].Value.ToString());
						strSp_Name = strSp_Name + "[Warning]";
					}
					else
					{
						if (Convert.ToInt32(p_Bdb.comMain.Parameters["@OT_nSqlCd"].Value) == 54 || Convert.ToInt32(p_Bdb.comMain.Parameters["@OT_nSqlCd"].Value) == 30006)
						{
							// No wait 를 사용할 경우
							SP_Err_Msg = "잠시 후 다시 사용하세요. (DB Lock)";
							strSp_Name = strSp_Name + "[Warning]";
						}
						else
						{
							SP_Err_Msg = "CODE : " + p_Bdb.comMain.Parameters["@OT_nSqlCd"].Value + Strings.Chr(13) + "MESG : " + Sp_Var_Str("" + p_Bdb.comMain.Parameters["@OT_vSqlMsg"].Value.ToString());
							strSp_Name = strSp_Name + "[DB Error]";
						}
					}
					break;
				default:
					SP_Err_Msg = "정의되지 않은 값이 리턴 되었습니다.[" + p_iSP_WmsCd + "]";
					strSp_Name = strSp_Name + "[기타 Error]";
					break;
			}
			if (p_blMsgView)
			{
				SP_ShowMsg(SP_Err_Msg, strSp_Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

		}

		public static void Sp_Display(ref CUserDb p_Bdb)
		{
//#if !SERVER_PROGRAM
//            if (modDefApp.wms_config["SQL_DISPLAY"] == "TRUE")
//            {
//                int iFor = 0;
//                string strPar = null;
//                MSG SqlDsp = new MSG();

//                SqlDsp.m_strTitle = "SP/SF DISPLAY";

//                SqlDsp.ChkTime.Visible = true;
//                SqlDsp.ChkTime.Checked = true;
//                SqlDsp.m_blReStart = false;

//                SqlDsp.PicErr.Visible = false;
//                SqlDsp.lblSql.Text = "SP/SF";
//                SqlDsp.lblSql.Visible = true;

//                strPar = p_Bdb.comMain.CommandText;

//                if (modDefApp.wms_config["SP_COL_DISPLAY"] == "TRUE")
//                {
//                    for (iFor = 0; iFor <= p_Bdb.comMain.Parameters.Count - 1; iFor++)
//                    {
//#if ORACLE
//                        strPar += modDef.CRLF + " COL Name:" + p_Bdb.comMain.Parameters[iFor].ParameterName.ToString() + modDef.CRLF + " COL Type:" + p_Bdb.comMain.Parameters[iFor].OleDbType.ToString() + modDef.CRLF + " COL I/O :" + p_Bdb.comMain.Parameters[iFor].Direction.ToString();
//#endif
//#if SQL
//                        strPar += modDef.CRLF + " COL Name:" + p_Bdb.comMain.Parameters[iFor].ParameterName.ToString() + modDef.CRLF + " COL Type:" + p_Bdb.comMain.Parameters[iFor].SqlDbType.ToString() + modDef.CRLF + " COL I/O :" + p_Bdb.comMain.Parameters[iFor].Direction.ToString();
//#endif
//                    }
//                }
//                SqlDsp.m_strMsg = strPar;
//                SqlDsp.ShowDialog();
//            }
//#endif
		}


		//------------------------------------------------------------------------------------------------
		// Stored Procedure 사용시
		//                   Provider=MSDAORA.1         로 Logon하면 String인경우 문제가 없으나
		//                                                                        Ex) "A0001" {string}
		//                   Provider=OraOLEDB.Oracle.1 로 Logon하면 String뒤에 Nothing이 같이 Return됨 
		//                                                                        Ex) "A0001  {string} 
		// 그래서 다음 함수를 모두 호출하도록 함
		//------------------------------------------------------------------------------------------------
		public static  string Sp_Var_Str(string p_strValue)
		{
			string strrtnValue = "";
			int iMyFor = 0;
			char cOneChar = '\0';

			if (p_strValue.Length < 1)
				return strrtnValue;

			cOneChar = Convert.ToChar(Strings.Mid(p_strValue, p_strValue.Length, 1));
			if (cOneChar != null)
			{
				return p_strValue;
			}

			for (iMyFor = 1; iMyFor <= p_strValue.Length; iMyFor++)
			{
				cOneChar = Convert.ToChar(Strings.Mid(p_strValue, iMyFor, 1));
				if (cOneChar == null)
				{
					break; // TODO: 원본은 Exit For 이었음. 동작이 다를 수 있으니 확인 필요
				}
				strrtnValue += Strings.Mid(p_strValue, iMyFor, 1);
			}
			return strrtnValue;

		}

		
		///******************************************************************************/
		///* Function명       : Get_Pa_Cell_Double                                      */  
		///*                    입고 CELL GET - 이중입고                                */
		///* Transaction 있음                                                           */
		///* ========================================================================== */
		///* Parameter        : parWH_TYP            창고구분                           */
		///*                    parAREA              저장위치                           */
		///*                    parLD_CTN_NO         적재용기                           */
		///* ========================================================================== */
		///* Return Value     : OT_vDEST_WH_TYP      DESTINATION 창고구분               */
		///*                    OT_vDEST_AREA        DESTINATION AREA                   */
		///*                    OT_vDEST_SITE        DESTINATION SITE                   */
		///*                    OT_vDEST_BANK        DESTINATION BANK                   */
		///*                    OT_vDEST_BAY         DESTIANTION BAY                    */
		///*                    OT_vDEST_LEV         DESTINATION LEVEL                  */
		///******************************************************************************/
		public static bool Get_Pa_Cell_Double(ref CUserDb p_Bdb, string p_strWH_TYP, string p_strAREA, string p_strLD_CTN_NO, ref sp_Cell_Info p_trnPa_Cell, bool p_blRollback = true, bool p_blMsgView = true)
		{
		
			try {
				Sp_Init_Name_Set(ref p_Bdb, "WMS_SP_GET_PA_CELL_DOUBLE");

                p_Bdb.comMain.Parameters.Add("@IN_vLANG", DbTypeChar).Value = "1";
                p_Bdb.comMain.Parameters.Add("@IN_vCOMPANY_CD", DbTypeChar).Value = modDefApp.COMPANY_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vAREA_CD", DbTypeChar).Value = modDefApp.AREA_CD;

                p_Bdb.comMain.Parameters.Add("@IN_vWH_CD", DbTypeChar).Value = p_strWH_TYP;
				p_Bdb.comMain.Parameters.Add("@IN_vAREA", DbTypeChar).Value = p_strAREA;
				p_Bdb.comMain.Parameters.Add("@IN_vLD_CTN_NO", DbTypeChar).Value = p_strLD_CTN_NO;
                p_Bdb.comMain.Parameters.Add("@IN_vCELL_TYP", DbTypeChar).Value = "";

                p_Bdb.comMain.Parameters.Add("@OT_vDEST_WH_CD", DbTypeChar, 7).Direction = ParameterDirection.Output;
				p_Bdb.comMain.Parameters.Add("@OT_vDEST_AREA", DbTypeChar, 9).Direction = ParameterDirection.Output;
				p_Bdb.comMain.Parameters.Add("@OT_vDEST_SITE", DbTypeChar, 3).Direction = ParameterDirection.Output;
				p_Bdb.comMain.Parameters.Add("@OT_vDEST_BANK", DbTypeChar, 2).Direction = ParameterDirection.Output;
				p_Bdb.comMain.Parameters.Add("@OT_vDEST_BAY", DbTypeChar, 3).Direction = ParameterDirection.Output;
				p_Bdb.comMain.Parameters.Add("@OT_vDEST_LEV", DbTypeChar, 2).Direction = ParameterDirection.Output;
				p_Bdb.comMain.Parameters.Add("@OT_vCELL_TYP", DbTypeChar, 7).Direction = ParameterDirection.Output;
		
				Sp_Rtn_Parameters_Add(ref p_Bdb);
		
				p_Bdb.comMain.ExecuteScalar();
		
				SP_WmsCd = Convert.ToInt32(p_Bdb.comMain.Parameters["@OT_nWmsCd"].Value.ToString());
				switch (SP_WmsCd) {
					case SP_SUCCESS:
						p_trnPa_Cell.WH_TYP = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vDEST_WH_CD"].Value.ToString());
						p_trnPa_Cell.AREA = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vDEST_AREA"].Value.ToString());
						p_trnPa_Cell.SC_NO = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vDEST_SITE"].Value.ToString());
						p_trnPa_Cell.BANK = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vDEST_BANK"].Value.ToString());
						p_trnPa_Cell.BAY = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vDEST_BAY"].Value.ToString());
						p_trnPa_Cell.LEV = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vDEST_LEV"].Value.ToString());
						p_trnPa_Cell.CELL_TYP = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vCELL_TYP"].Value.ToString());
						SP_Suc_Msg = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vWmsMsg"].Value.ToString());
						return true;
					default:
						if (p_blRollback)
							p_Bdb.trnMain.Rollback();
						Sp_Rtn_Else_Message(ref p_Bdb, SP_WmsCd, "입고 CELL GET - 이중입고", p_blMsgView);
						return false;
				}
#if ORACLE
			}
			catch (OleDbException DbErr)
			{
#endif
#if SQL
		   } catch (SqlException DbErr) {
#endif
#if POSTGRESSQL
            }
            catch (NpgsqlException DbErr)
            {
#endif
                SP_Err_Msg = DbErr.Message;
				if (p_blRollback)
					p_Bdb.trnMain.Rollback();
				if (p_blMsgView) {
					SP_ShowMsg(SP_Err_Msg, "Get_Pa_Cell_Double[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				return false;
			} catch (Exception AppErr) {
				SP_Err_Msg = AppErr.Message;
				if (p_blRollback)
					p_Bdb.trnMain.Rollback();
				if (p_blMsgView) {
					SP_ShowMsg(SP_Err_Msg, "Get_Pa_Cell_Double[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				return false;
			}
		
		}

				
		///******************************************************************************/
		///* Function명       : Pro_Mc_Elps_Log                                         */  
		///*                    설비 가동 LOGGING처리                                   */
		///* Transaction 있음                                                           */
		///* ========================================================================== */
		///* Parameter        : IN_vLOG_DTHH         LOG DATE/TIME(HH)                  */
		///*                    IN_vWH_TYP           창고구분                           */
		///*                    IN_vMC_TYP           MACHINE                            */
		///*                    IN_vMC_NO            MACHINE NO                         */
		///*                    IN_nLUG_CNT          작업수                             */
		///*                    IN_nRUN_TM           가동시간                           */
		///*                    IN_nERR_TM           경과시간                           */
		///*                    IN_nHLD_TM           대기시간                           */
		///* ========================================================================== */
		///* Return Value     :                                                         */
		///******************************************************************************/
		public static bool Pro_Mc_Elps_Log(ref CUserDb Bdb, string parLOG_DTHH, string parWH_TYP, string parMC_TYP, string parMC_NO, string parLUG_CNT, string parRUN_TM, string parERR_TM, string parHLD_TM, bool parRollback = true,
		bool parMsgView = true)
		{
		
			try {
				modSpDb.Sp_Init_Name_Set(ref Bdb, "wms_sp_Pro_Mc_Elps_Log");
		
				Bdb.comMain.Parameters.Add("@IN_vLOG_DTHH", DbTypeChar).Value = parLOG_DTHH;
				Bdb.comMain.Parameters.Add("@IN_vWH_TYP", DbTypeChar).Value = parWH_TYP;
				Bdb.comMain.Parameters.Add("@IN_vMC_TYP", DbTypeChar).Value = parMC_TYP;
				Bdb.comMain.Parameters.Add("@IN_vMC_NO", DbTypeChar).Value = parMC_NO;
				Bdb.comMain.Parameters.Add("@IN_nLUG_CNT", DbTypeChar).Value = Convert.ToDouble(parLUG_CNT);
				Bdb.comMain.Parameters.Add("@IN_nRUN_TM", DbTypeNum).Value = Convert.ToDouble(parRUN_TM);
				Bdb.comMain.Parameters.Add("@IN_nERR_TM", DbTypeChar).Value = Convert.ToDouble(parERR_TM);
				Bdb.comMain.Parameters.Add("@IN_nHLD_TM", DbTypeNum).Value = Convert.ToDouble(parHLD_TM);
		
				Sp_Rtn_Parameters_Add(ref Bdb);
		
				Bdb.comMain.ExecuteScalar();
		
				SP_WmsCd = int.Parse(Bdb.comMain.Parameters["@OT_nWmsCd"].Value.ToString());
				switch (SP_WmsCd) {
					case SP_SUCCESS:
						SP_Suc_Msg = Sp_Var_Str(Bdb.comMain.Parameters["@OT_vWmsMsg"].Value.ToString());
						return true;
					default:
						if (parRollback)
							Bdb.trnMain.Rollback();
						Sp_Rtn_Else_Message(ref Bdb, SP_WmsCd, "설비 가동 LOGGING처리", parMsgView);
						return false;
				}
#if ORACLE
				} catch (OleDbException DbErr) {
#endif
#if SQL
				} catch (SqlException DbErr) {
#endif	
#if POSTGRESSQL
            }
            catch (NpgsqlException DbErr)
            {
#endif
                SP_Err_Msg = DbErr.Message;
				if (parRollback)
					Bdb.trnMain.Rollback();
				if (parMsgView) {
					SP_ShowMsg(SP_Err_Msg, "Pro_Mc_Elps_Log[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				return false;
			} catch (Exception AppErr) {
				SP_Err_Msg = AppErr.Message;
				if (parRollback)
					Bdb.trnMain.Rollback();
				if (parMsgView) {
					SP_ShowMsg(SP_Err_Msg, "Pro_Mc_Elps_Log[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				return false;
			}
		
		}

		public static void SP_ShowMsg(string p_strMsg, string p_strTitle, MessageBoxButtons p_ButtonType = MessageBoxButtons.OK, MessageBoxIcon p_IconType = MessageBoxIcon.Information)
		{
//#if !SERVER_PROGRAM
//            if (p_strMsg.IndexOf("ORA-03113") != -1 || p_strMsg.IndexOf("ORA-03114") != -1 || p_strMsg.IndexOf("ORA-12560") != -1 || p_strMsg.IndexOf("ORA-12541") != -1)
//            {
//                modCmWork.ShowMsg("DB연결이 종료된 상태 입니다" + modDef.CRLF + "프로그램을 종료 후 다시 실행 하시오." + modDef.CRLF + "[" + p_strMsg + "]", p_strTitle, p_ButtonType, p_IconType);
//            }
//            else
//            {
//                modCmWork.ShowMsg(p_strMsg, p_strTitle, p_ButtonType, p_IconType);
//            }
//#endif
		}

        //******************************************************************************/
        //* Function명       : Pro_Pa_Empty_Ld_Ctn                                     */  
        //*                    공적재용기 입고작업생성                                  */
        //* Transaction 있음                                                           */
        //* ========================================================================== */
        //* Parameter        : parFormName          윈도우.Tetx                        */
        //*                    IN_vPA_CELL_GET_TYP  AUTO:CELL자동검색,                 */
        //*                                         USER:CELL지정입고                  */
        //*                                         NOT :CELL을 검색하지 않음          */
        //*                    IN_vLD_CTN_NO        적재용기번호                       */
        //*                    IN_vINV_STA          상태구분                           */
        //*                    IN_nJOB_QTY          작업량                             */
        //*                    IN_vSOUR_WH_CD      SOURCE 창고구분                    */
        //*                    IN_vSOUR_SITE        SOURCE SITE                        */
        //*                    IN_vDEST_WH_CD      DESTINATION 창고구분               */
        //*                    IN_vDEST_AREA        DESTINATION AREA                   */
        //*                    IN_vREMARKS          비고                               */
        //*                    IN_vVIA_SITE         경우 SITE                          */
        //* ========================================================================== */
        //* Return Value     :                                                         */
        //******************************************************************************/
        public static bool Pro_Pa_Empty_Ld_Ctn(ref CUserDb p_Bdb,
                                               string p_strFormName,
                                               string p_strPA_CELL_GET_TYP,
                                               string p_strLD_CTN_NO,
                                               string p_strINV_STA,
                                               int p_iJOB_QTY,
                                               string p_strSOUR_WH_CD,
                                               string p_strSOUR_SITE,
                                               string p_strDEST_WH_CD,
                                               string p_strDEST_AREA,
                                               string p_strUserID,
                                               string p_strUserName,
                                               string p_strPcNm,
                                               string p_strPcIp,
                                               string p_strREMARKS = "",
                                               string p_strVIA_SITE = "000",
                                               bool p_blRollback = true,
                                               bool p_blMsgView = true)
        {
            try
            {
                Sp_Init_Name_Set(ref p_Bdb, "wms_sp_Pro_Pa_Empty_Ld_Ctn");

                p_Bdb.comMain.Parameters.Add("@IN_vLANG", DbTypeNVarChar).Value = modDefApp.g_strSYS_LANG;
                p_Bdb.comMain.Parameters.Add("@IN_vCOMPANY_CD", DbTypeNVarChar).Value = modDefApp.COMPANY_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vAREA_CD", DbTypeNVarChar).Value = modDefApp.AREA_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vWIN_ID", DbTypeNVarChar).Value = p_strFormName;
                p_Bdb.comMain.Parameters.Add("@IN_vPA_CELL_GET_TYP", DbTypeNVarChar).Value = p_strPA_CELL_GET_TYP;
                p_Bdb.comMain.Parameters.Add("@IN_vLD_CTN_NO", DbTypeNVarChar).Value = p_strLD_CTN_NO;
                p_Bdb.comMain.Parameters.Add("@IN_vINV_STA", DbTypeNVarChar).Value = p_strINV_STA;
#if ORACLE
                p_Bdb.comMain.Parameters.Add("@IN_nJOB_QTY", DbTypeNum).Value = p_iJOB_QTY;
#endif
#if SQL 
                p_Bdb.comMain.Parameters.Add("@IN_nJOB_QTY", DbTypeNum).Value = p_iJOB_QTY;
#endif
#if POSTGRESSQL
                p_Bdb.comMain.Parameters.Add("@IN_nJOB_QTY", DbTypeNum).Value = p_iJOB_QTY;
#endif
                p_Bdb.comMain.Parameters.Add("@IN_vSOUR_WH_CD", DbTypeNVarChar).Value = p_strSOUR_WH_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vSOUR_SITE", DbTypeNVarChar).Value = p_strSOUR_SITE;
                p_Bdb.comMain.Parameters.Add("@IN_vDEST_WH_CD", DbTypeNVarChar).Value = p_strDEST_WH_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vDEST_AREA", DbTypeNVarChar).Value = p_strDEST_AREA;
                p_Bdb.comMain.Parameters.Add("@IN_vREMARKS", DbTypeNVarChar).Value = p_strREMARKS;
                p_Bdb.comMain.Parameters.Add("@IN_vVIA_SITE", DbTypeNVarChar).Value = p_strVIA_SITE;
                p_Bdb.comMain.Parameters.Add("@IN_vUSER_ID", DbTypeNVarChar).Value = p_strUserID;
                p_Bdb.comMain.Parameters.Add("@IN_vUSER_NM", DbTypeNVarChar).Value = p_strUserName;
                p_Bdb.comMain.Parameters.Add("@IN_vTERM_ID", DbTypeNVarChar).Value = p_strPcNm;
                p_Bdb.comMain.Parameters.Add("@IN_vTERM_IP", DbTypeNVarChar).Value = p_strPcIp;

                Sp_Rtn_Parameters_Add(ref p_Bdb);

                p_Bdb.comMain.ExecuteScalar();

                SP_WmsCd = Convert.ToInt32(p_Bdb.comMain.Parameters["@OT_nWmsCd"].Value.ToString());
                switch (SP_WmsCd)
                {
                    case SP_SUCCESS:
                        SP_Suc_Msg = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vWmsMsg"].Value.ToString());
                        return true;
                    default:
                        if (p_blRollback)
                            p_Bdb.trnMain.Rollback();
                        Sp_Rtn_Else_Message(ref p_Bdb, SP_WmsCd, "공적재용기 입고작업생성", p_blMsgView);
                        return false;
                }
#if ORACLE
            }
            catch (OleDbException DbErr)
            {
#endif
#if SQL 
            }
            catch (SqlException DbErr)
            {
#endif
#if POSTGRESSQL
            }
            catch (NpgsqlException DbErr)
            {
#endif

                SP_Err_Msg = DbErr.Message;
                if (p_blRollback)
                    p_Bdb.trnMain.Rollback();
                if (p_blMsgView)
                {
                    SP_ShowMsg(SP_Err_Msg, "Pro_Pa_Empty_Ld_Ctn[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception AppErr)
            {
                SP_Err_Msg = AppErr.Message;
                if (p_blRollback)
                    p_Bdb.trnMain.Rollback();
                if (p_blMsgView)
                {
                    SP_ShowMsg(SP_Err_Msg, "Pro_Pa_Empty_Ld_Ctn[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }

        public static bool PRO_PK_EMPTY_LD_CTN(ref CUserDb p_Bdb,
                                             string p_strFormName,
                                             string p_strLD_CTN_NO,
                                             string p_strSOUR_WH_CD,
                                             string p_strSOUR_AREA,
                                             string p_strDEST_WH_CD,
                                             string p_strDEST_AREA,
                                             string p_strPRIORITY,
                                             string p_strUserID,
                                             string p_strUserName,
                                             string p_strPcNm,
                                             string p_strPcIp,
                                             bool p_blRollback = true,
                                             bool p_blMsgView = true)
        {
            try
            {
                Sp_Init_Name_Set(ref p_Bdb, "WMS_SP_PRO_PK_EMPTY_LD_CTN");

                p_Bdb.comMain.Parameters.Add("@IN_vLANG", DbTypeNVarChar).Value = modDefApp.g_strSYS_LANG;
                p_Bdb.comMain.Parameters.Add("@IN_vCOMPANY_CD", DbTypeNVarChar).Value = modDefApp.COMPANY_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vAREA_CD", DbTypeNVarChar).Value = modDefApp.AREA_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vWIN_ID", DbTypeNVarChar).Value = p_strFormName;
                p_Bdb.comMain.Parameters.Add("@IN_vSOUR_WH_CD", DbTypeNVarChar).Value = p_strSOUR_WH_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vSOUR_AREA", DbTypeNVarChar).Value = p_strSOUR_AREA;
                p_Bdb.comMain.Parameters.Add("@IN_vDEST_WH_CD", DbTypeNVarChar).Value = p_strDEST_WH_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vDEST_CV_NO", DbTypeNVarChar).Value = p_strDEST_AREA;
                p_Bdb.comMain.Parameters.Add("@IN_vLD_CTN_NO", DbTypeNVarChar).Value = p_strLD_CTN_NO;
                p_Bdb.comMain.Parameters.Add("@IN_vPRIORITY", DbTypeNVarChar).Value = p_strPRIORITY;
                p_Bdb.comMain.Parameters.Add("@IN_vUSER_ID", DbTypeNVarChar).Value = p_strUserID;
                p_Bdb.comMain.Parameters.Add("@IN_vUSER_NM", DbTypeNVarChar).Value = p_strUserName;
                p_Bdb.comMain.Parameters.Add("@IN_vTERM_ID", DbTypeNVarChar).Value = p_strPcNm;
                p_Bdb.comMain.Parameters.Add("@IN_vTERM_IP", DbTypeNVarChar).Value = p_strPcIp;

                Sp_Rtn_Parameters_Add(ref p_Bdb);

                p_Bdb.comMain.ExecuteScalar();

                SP_WmsCd = Convert.ToInt32(p_Bdb.comMain.Parameters["@OT_nWmsCd"].Value.ToString());
                switch (SP_WmsCd)
                {
                    case SP_SUCCESS:
                        SP_Suc_Msg = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vWmsMsg"].Value.ToString());
                        return true;
                    default:
                        if (p_blRollback)
                            p_Bdb.trnMain.Rollback();
                        Sp_Rtn_Else_Message(ref p_Bdb, SP_WmsCd, "공적재용기 입고작업생성", p_blMsgView);
                        return false;
                }
#if ORACLE
            }
            catch (OleDbException DbErr)
            {
#endif
#if SQL 
            }
            catch (SqlException DbErr)
            {
#endif
#if POSTGRESSQL
            }
            catch (NpgsqlException DbErr)
            {
#endif

                SP_Err_Msg = DbErr.Message;
                if (p_blRollback)
                    p_Bdb.trnMain.Rollback();
                if (p_blMsgView)
                {
                    SP_ShowMsg(SP_Err_Msg, "PRO_PK_EMPTY_LD_CTN[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception AppErr)
            {
                SP_Err_Msg = AppErr.Message;
                if (p_blRollback)
                    p_Bdb.trnMain.Rollback();
                if (p_blMsgView)
                {
                    SP_ShowMsg(SP_Err_Msg, "PRO_PK_EMPTY_LD_CTN[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }


        public static bool Pro_Lugg_Logging(ref CUserDb p_Bdb
                              , string p_strSYS_GRP
                              , string p_strLUGG_NO
                              , string p_strDEL_YON
                              , bool p_blRollback = true
                              , bool p_blMsgView = true)
        {
            try
            {


                Sp_Init_Name_Set(ref p_Bdb, "wms_sp_Pro_Lugg_Logging");

                p_Bdb.comMain.Parameters.Add("@IN_vLANG", DbTypeNVarChar).Value = modDefApp.g_strSYS_LANG;
                p_Bdb.comMain.Parameters.Add("@IN_vCOMPANY_CD", DbTypeNVarChar).Value = modDefApp.COMPANY_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vAREA_CD", DbTypeNVarChar).Value = modDefApp.AREA_CD;
                p_Bdb.comMain.Parameters.Add("@IN_vSYS_GRP", DbTypeNVarChar, 255).Value = p_strSYS_GRP;
                p_Bdb.comMain.Parameters.Add("@IN_nLUGG_NO", DbTypeNVarChar, 255).Value = p_strLUGG_NO;
                p_Bdb.comMain.Parameters.Add("@IN_vDEL_YON", DbTypeNVarChar, 255).Value = p_strDEL_YON;

                Sp_Rtn_Parameters_Add(ref p_Bdb);

                p_Bdb.comMain.ExecuteScalar();

                SP_WmsCd = Convert.ToInt32(p_Bdb.comMain.Parameters["@OT_nWmsCd"].Value.ToString());

                switch (SP_WmsCd)
                {
                    case SP_SUCCESS:
                        SP_Suc_Msg = Sp_Var_Str(p_Bdb.comMain.Parameters["@OT_vWmsMsg"].Value.ToString());
                        return true;
                    default:
                        if (p_blRollback)
                            p_Bdb.trnMain.Rollback();
                        Sp_Rtn_Else_Message(ref p_Bdb, SP_WmsCd, "작업번호 Logging", p_blMsgView);
                        return false;
                }
            }
#if ORACLE
            catch (OleDbException DbErr)
#endif
#if SQL 
            catch (SqlException DbErr)
#endif
#if POSTGRESSQL    
            catch (NpgsqlException DbErr)
#endif
            {
                SP_Err_Msg = DbErr.Message;
                if (p_blRollback)
                    p_Bdb.trnMain.Rollback();
                if (p_blMsgView)
                {
                    SP_ShowMsg(SP_Err_Msg, "Pro_Lugg_Logging[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception AppErr)
            {
                SP_Err_Msg = AppErr.Message;
                if (p_blRollback)
                    p_Bdb.trnMain.Rollback();
                if (p_blMsgView)
                {
                    SP_ShowMsg(SP_Err_Msg, "Pro_Lugg_Logging[Error]", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }
    }
}