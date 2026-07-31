
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
//using System.Data.OleDb;
using System.Diagnostics;
using System.Windows.Forms;

#if ORACLE
using System.Data.OleDb;
#endif

#if SQL
using System.Data.SqlClient;
#endif

#if POSTGRESSQL
using Npgsql;
using NpgsqlTypes;
#endif

//*** DB 기초 클래스 **************************************************************************
//* DB 객체들과 실행 함수들을 정의
namespace TSK_HostCom
{
	public class CBaseDb
	{
#if ORACLE
		// 외부에서 할당해주는 Connection 개체 참조(Connection이 하나 일 경우)

		public OleDbConnection conMain;
		// 자체 생성되는 DB 객체들
		public OleDbTransaction trnMain;
		public OleDbCommand comMain = new OleDbCommand();
		//Private daMain As New OleDbDataAdapter
		public OleDbDataAdapter daMain = new OleDbDataAdapter();


#endif
#if SQL
	// 외부에서 할당해주는 Connection 개체 참조(Connection이 하나 일 경우)

	public SqlConnection conMain;
	// 자체 생성되는 DB 객체들
	public SqlTransaction trnMain;
	public SqlCommand comMain = new SqlCommand();
   	private SqlDataAdapter daMain = new SqlDataAdapter();
#endif
#if POSTGRESSQL
	// 외부에서 할당해주는 Connection 개체 참조(Connection이 하나 일 경우)

	public NpgsqlConnection conMain;

    //// 자체 생성되는 DB 객체들
    //public cDbPostUse _pBdb = null;

	public NpgsqlTransaction trnMain;
	public NpgsqlCommand comMain = new NpgsqlCommand();
   	private NpgsqlDataAdapter daMain = new NpgsqlDataAdapter();
#endif


        public DataTable dtMain = new DataTable("Default");
		//public DataTable dtSub = new DataTable("Default");

#if SERVER_PROGRAM
    Public DataTable dtLugDtl = new DataTable("Default");
	public DataTable dtMove = new DataTable("Default");
	public DataTable dtPkInf = new DataTable("Default");
    public DataTable dtSub = new DataTable("Default");
#endif

		// 바인딩 객체에 사용할지 여부
		// 바인딩 객체일 경우 Reset을 하면 안됨.(계속 연결된 상태, DataSource에 따라 작동)
		public bool blBindingType;
		// DB 에러
		public const int DB_ERR = -1;
		// DB 에러중 DB Lock
		public const int DB_LOCK = -2;
		// DB 에러중 중복 데이타

		public const int DB_DUP = -3;
		// DB Error Message
		public string strErrMsg = "";

		public bool blTran;
		public string ErrMsg
		{
			get
			{
				return strErrMsg;
			}
			set
			{
				strErrMsg = value;
			}
		}


		// DB Error 종류
		private int nErrKind = 0;
		public string ErrKind
		{
			get
			{
				return nErrKind.ToString();
			}
		}

		// 자체 Connection 객체 사용
		// 외부에서 New 생성 후 Connection 객체를 Open하고 Init을 호출한다.
		// 종료시 comMain.Close를 반드시 호출
		public CBaseDb(bool p_blBind = false)
		{
			blBindingType = p_blBind;
		}

		// 외부에서 Connection 객체 정의 (pc cliient 처럼 , 하나를 쓸 경우)
#if ORACLE
		public CBaseDb(ref OleDbConnection p_conObj, bool p_blBind = false)
#endif

#if SQL 
    public CBaseDb(ref SqlConnection p_conObj, bool p_blBind = false)
#endif

#if POSTGRESSQL 
    public CBaseDb(ref NpgsqlConnection p_conObj, bool p_blBind = false)
#endif
		{
			blBindingType = p_blBind;
			conMain = p_conObj;
			// DB init
			Init();
		}
		// DB init

		public void Init()
		{
			comMain.Connection = conMain;
			daMain.SelectCommand = comMain;

		}

		//*** DB Error Message ***
		//* 프로젝트 별로 메세지를 표시하는 방법을 패생 클래스에서 오버라이드 해서 사용한다.
		public virtual void ShowErrMsg(bool p_blMsgBox, string p_strERR = "DB")
		{
			if (p_blMsgBox)
			{
				MessageBox.Show(ErrMsg, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public virtual void ShowErrMsg_Call(string p_strCALL, bool p_blMsgBox, string p_strERR = "DB")
		{
			if (p_blMsgBox)
			{
				MessageBox.Show(ErrMsg, "DB Error[" + p_strCALL + "]", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		public int ExcuteQry_Par(ref string strQry, bool bMsgBox = true, bool bReturnErr = false)
		{
			try
			{
				//01 20170118 RYU Parameter Query관련 변경 - 기능 추가
				string strQry_Run = strQry;
				string strQry_Display = strQry;

				ParamsReplace(ref strQry_Run, ref strQry_Display);
				//01 END 

				if (conMain.State != ConnectionState.Open)
				{
					conMain.Open();
				}
				strErrMsg = "";
				// 바인딩 객체일 경우 연결유지, DATA만 클리어
				if (blBindingType)
				{
					dtMain.Clear();
				}
				else
				{
                    dtMain.Reset();
				}

				Sql_Display(strQry);

				comMain.CommandType = CommandType.Text;
				comMain.CommandText = strQry_Run;
				strQry = strQry_Display;

				return daMain.Fill(dtMain);

#if ORACLE
			}
			catch (OleDbException DbErr)
			{
#endif

#if SQL
           } catch (SqlException DbErr) {
#endif

#if POSTGRESSQL
           } catch (NpgsqlException DbErr) {
#endif

				if ((bReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					// & CRLF & DbErr.StackTrace
					if (strErrMsg.IndexOf("ORA-00054") != -1 || strErrMsg.IndexOf("ORA-30006") != -1)
					{
						// No wait 를 사용할 경우
						nErrKind = DB_LOCK;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg(bMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((bReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg(bMsgBox, "APP");
				}
			}

			return DB_ERR;

		}

		public int ExcuteQry_Par(ref DataTable p_dtOther, ref string strQry, bool bMsgBox = true, bool bReturnErr = false)
		{
			try
			{
				//01 20170118 RYU Parameter Query관련 변경 - 기능 추가
				string strQry_Run = strQry;
				string strQry_Display = strQry;

				ParamsReplace(ref strQry_Run, ref strQry_Display);
				//01 END 

				strErrMsg = "";
				// 바인딩 객체일 경우 연결유지, DATA만 클리어
				if (blBindingType)
				{
					p_dtOther.Clear();
				}
				else
				{
					p_dtOther.Reset();
				}

				Sql_Display(strQry);

				//01 20170118 RYU Parameter Query관련 변경 - 기능 추가
				//comMain.CommandText = strQry;
				comMain.CommandText = strQry_Run;
				//comMain.Parameters.Clear()
				strQry = strQry_Display;
				//' 01 END

				return daMain.Fill(p_dtOther);

#if ORACLE
			}
			catch (OleDbException DbErr)
			{
#endif

#if SQL
          	} catch (SqlException DbErr) {
#endif

#if POSTGRESSQL
           } catch (NpgsqlException DbErr) {
#endif
				if ((bReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					// & CRLF & DbErr.StackTrace
					if (strErrMsg.IndexOf("ORA-00054") != -1 || strErrMsg.IndexOf("ORA-30006") != -1)
					{
						// No wait 를 사용할 경우
						nErrKind = DB_LOCK;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg(bMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((bReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg(bMsgBox, "APP");
				}
			}

			return DB_ERR;

		}

		public int ExcuteNonQry_Par(ref string p_strQry, bool p_blMsgBox = true, bool p_blReturnErr = false)
		{
			try
			{
				//01 20170118 RYU Parameter Query관련 변경 - 기능 추가
				string strQry_Run = p_strQry;
				string strQry_Display = p_strQry;

				ParamsReplace(ref strQry_Run, ref strQry_Display);
				//01 END 

				strErrMsg = "";
				Sql_Display(p_strQry);

				comMain.CommandType = CommandType.Text;
				//01 20170118 RYU Parameter Query관련 변경 - 기능 추가
				//comMain.CommandText = strQry;
				comMain.CommandText = strQry_Run;
				//comMain.Parameters.Clear()
				p_strQry = strQry_Display;
				//' 01 END

				return comMain.ExecuteNonQuery();

#if ORACLE
			}
			catch (OleDbException DbErr)
			{
#endif

#if SQL
            } catch (SqlException DbErr) {
#endif

#if POSTGRESSQL
           } catch (NpgsqlException DbErr) {
#endif
				if ((p_blReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					if (DbErr.Message.IndexOf("ORA-00001") != -1)
					{
						nErrKind = DB_DUP;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg(p_blMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((p_blReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg(p_blMsgBox, "APP");
				}
			}

			return DB_ERR;

		}

		// 01 20170118 RYU Parameter Query관련 변경 - 함수 신규 생성
		public void ParamsClear()
		{
			comMain.Parameters.Clear();
		}
#if ORACLE
		public string ClearAdd(Object p_Value, OleDbType p_DbType = modSpDb.DbTypeChar)
		{
#endif
#if SQL
        public string ClearAdd(Object p_Value, SqlDbType p_DbType = modSpDb.DbTypeChar)
        {
#endif

#if POSTGRESSQL
        public string ClearAdd(Object p_Value, NpgsqlDbType p_DbType = modSpDb.DbTypeChar)
        {
#endif
			ParamsClear();
			return ParamsAdd("0000", p_Value, p_DbType);
		}
#if ORACLE
		public string ClearAdd(string p_ParamName, Object p_Value, OleDbType p_DbType = modSpDb.DbTypeChar)
		{
#endif
#if SQL
        public string ClearAdd(string p_ParamName, Object p_Value, SqlDbType p_DbType = modSpDb.DbTypeChar)
        {
#endif
#if POSTGRESSQL
        public string ClearAdd(string p_ParamName, Object p_Value, NpgsqlDbType p_DbType = modSpDb.DbTypeChar)
        {
#endif

			ParamsClear();
			return ParamsAdd(p_ParamName, p_Value, p_DbType);
		}
#if ORACLE
		public string ParamsAdd(Object p_Value, OleDbType p_DbType = modSpDb.DbTypeChar)
		{
#endif
#if SQL
        public string ParamsAdd(Object p_Value, SqlDbType p_DbType = modSpDb.DbTypeChar)
        {
#endif
#if POSTGRESSQL
        public string ParamsAdd(Object p_Value, NpgsqlDbType p_DbType = modSpDb.DbTypeChar)
        {
#endif
			string p_ParamName = Strings.Format(comMain.Parameters.Count, "000#");
			comMain.Parameters.Add(p_ParamName, p_DbType).Value = p_Value;
			return modSpDb.DbBindVal + p_ParamName;
		}
#if ORACLE
		public string ParamsAdd(string p_ParamName, Object p_Value, OleDbType p_DbType = modSpDb.DbTypeChar)
		{
#endif
#if SQL
        public string ParamsAdd(string p_ParamName, Object p_Value, SqlDbType p_DbType = modSpDb.DbTypeChar)
        {
#endif
#if POSTGRESSQL
        public string ParamsAdd(string p_ParamName, Object p_Value, NpgsqlDbType p_DbType = modSpDb.DbTypeChar)
        {
#endif

			comMain.Parameters.Add(p_ParamName, p_DbType).Value = p_Value;
			return modSpDb.DbBindVal + p_ParamName;
		}

		public void ParamsReplace(ref string p_Qry_Run, ref string p_Qry_Display)
		{
			int iFor;
			for (iFor = 0; iFor <= comMain.Parameters.Count - 1; iFor++)
			{
				//p_Qry_Run = Microsoft.VisualBasic.Strings.Replace(p_Qry_Run, ":" + comMain.Parameters[iFor].ParameterName, "?", 1, 1);
#if ORACLE
				if (comMain.Parameters[iFor].OleDbType == modSpDb.DbTypeChar)
				{
					p_Qry_Display = Microsoft.VisualBasic.Strings.Replace(p_Qry_Display, modSpDb.DbBindVal + comMain.Parameters[iFor].ParameterName, "'" + comMain.Parameters[iFor].Value + "'", 1, 1);
				}
#endif
#if SQL
                if (comMain.Parameters[iFor].SqlDbType == modSpDb.DbTypeChar)
                {
                    p_Qry_Display = Microsoft.VisualBasic.Strings.Replace(p_Qry_Display, modSpDb.DbBindVal + comMain.Parameters[iFor].ParameterName, "'" + comMain.Parameters[iFor].Value + "'", 1, 1);
                }
                else if(comMain.Parameters[iFor].SqlDbType == modSpDb.DbTypeNVarChar) 
                {
                    p_Qry_Display = Microsoft.VisualBasic.Strings.Replace(p_Qry_Display, modSpDb.DbBindVal + comMain.Parameters[iFor].ParameterName, "N'" + comMain.Parameters[iFor].Value + "'", 1, 1);
                }
#endif
#if POSTGRESSQL
                if (comMain.Parameters[iFor].NpgsqlDbType == modSpDb.DbTypeChar)
				{
					p_Qry_Display = Microsoft.VisualBasic.Strings.Replace(p_Qry_Display, modSpDb.DbBindVal + comMain.Parameters[iFor].ParameterName, "'" + comMain.Parameters[iFor].Value + "'", 1, 1);
				}
#endif
				else
				{
					p_Qry_Display = Microsoft.VisualBasic.Strings.Replace(p_Qry_Display, modSpDb.DbBindVal + comMain.Parameters[iFor].ParameterName, comMain.Parameters[iFor].Value.ToString(), 1, 1);
				}
			}
		}

		//*********************************************************************************
		// 쿼리 실행 For Select..  
		// Parameter: bMsgBox (메세지 박스 표시 여부)
		//            bRerutnErr (에러 발생시, 에러를 Return할 지 여부)
		// Return: 성공 - 쿼리한 레코드 수 (양의 정수)
		//         실패 - DB_ERR(-1):  일반 DB Err
		//         실패 - DB_LOCK(-2): DB Lock
		//         실패 - DB_DUP(-3):  데이타 중복
		public int ExcuteQry(string p_strQry, bool p_blMsgBox = true, bool p_blbReturnErr = false, bool p_blTimeApy = true)
		{
			try
			{
				if (conMain.State != ConnectionState.Open)
				{
					conMain.Open();
				}
				strErrMsg = "";
				// 바인딩 객체일 경우 연결유지, DATA만 클리어
				if (blBindingType)
				{
					dtMain.Clear();
				}
				else
				{
                    dtMain.Reset();
				}

				Sql_Display(p_strQry, p_blTimeApy);

				comMain.CommandType = CommandType.Text;
				comMain.CommandText = p_strQry;
				comMain.Parameters.Clear();

				return daMain.Fill(dtMain);
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
                if ((p_blbReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;	// & CRLF & DbErr.StackTrace
					if (strErrMsg.IndexOf("ORA-00054") != -1 || strErrMsg.IndexOf("ORA-30006") != -1)
					{
						// No wait 를 사용할 경우
						nErrKind = DB_LOCK;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg(p_blMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((p_blbReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message.ToString() + modDefApp.CRLF + "----- 상세에러 -----" +  modDefApp.CRLF + AppErr.ToString();
					// & CRLF & DbErr.StackTrace
					nErrKind = 0;
					ShowErrMsg(p_blMsgBox, "APP");
				}

			}

			return DB_ERR;

		}

		public int ExcuteQry_Call(string p_strCALL, string p_strQry, bool p_blMsgBox = true, bool p_blReturnErr = false)
		{
			try
			{
				if (conMain.State != ConnectionState.Open)
				{
					conMain.Open();
				}
				strErrMsg = "";
				// 바인딩 객체일 경우 연결유지, DATA만 클리어
				if (blBindingType)
				{
					dtMain.Clear();
				}
				else
				{
                    dtMain.Reset();
				}

				Sql_Display_Call(p_strCALL, p_strQry);

				comMain.CommandType = CommandType.Text;
				comMain.CommandText = p_strQry;
				comMain.Parameters.Clear();

				return daMain.Fill(dtMain);

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

				if ((p_blReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					// & CRLF & DbErr.StackTrace
					if (strErrMsg.IndexOf("ORA-00054") != -1 || strErrMsg.IndexOf("ORA-30006") != -1)
					{
						// No wait 를 사용할 경우
						nErrKind = DB_LOCK;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg_Call(p_strCALL, p_blMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((p_blReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg_Call(p_strCALL, p_blMsgBox, "APP");
				}
			}

			return DB_ERR;

		}
        
		public int ExcuteQry(ref DataTable p_dtOther, string p_strQry, bool p_blMsgBox = true, bool p_blReturnErr = false)
		{
			try
			{
				if (conMain.State != ConnectionState.Open)
				{
					conMain.Open();
				}
				strErrMsg = "";
				// 바인딩 객체일 경우 연결유지, DATA만 클리어
				if (blBindingType)
				{
					p_dtOther.Clear();
				}
				else
				{
					p_dtOther.Reset();
				}

				Sql_Display(p_strQry);

				comMain.CommandType = CommandType.Text;
				comMain.CommandText = p_strQry;
				comMain.Parameters.Clear();

				return daMain.Fill(p_dtOther);

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

				if ((p_blReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					if (DbErr.Message.IndexOf("ORA-00054") != -1 || strErrMsg.IndexOf("ORA-30006") != -1)
					{
						// No wait 를 사용할 경우
						nErrKind = DB_LOCK;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg(p_blMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((p_blReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg(p_blMsgBox, "APP");
				}
			}

			return DB_ERR;

		}

		//*** DB 파라미터 클래스 **************************************************************************
		//*********************************************************************************
		// 쿼리 실행 For Select.., 2개 이상 쿼리를 할 경우 datatable을 별도로 바인딩 한다.  
		// Parameter: bRerutnErr (에러 발생시, 에러를 Return할 지 여부)
		// Return: 성공 - 쿼리한 레코드 수 (양의 정수)
		//         실패 - DB_ERR(-1):  일반 DB Err
		//         실패 - DB_LOCK(-2): DB Lock
		//         실패 - DB_DUP(-3):  데이타 중복
		public int ExcuteQry(ref DataTable p_dtOther, string p_strQry, bool p_blMsgBox, bool p_blReturnErr, params sqlPalam[] @params)
		{
			try
			{
				strErrMsg = "";
				// 바인딩 객체일 경우 연결유지, DATA만 클리어
				if (blBindingType)
				{
					p_dtOther.Clear();
				}
				else
				{
					p_dtOther.Reset();
				}

				Sql_Display(p_strQry);

				comMain.CommandType = CommandType.Text;
				comMain.CommandText = p_strQry;
				comMain.Parameters.Clear();

				// 다이나믹 쿼리를 사용할지 파라미터 쿼리를 사용할지에 대한 부분을 ExcuteQry메소드의 파라미터로 받는등으로 선택하게 하여도 되나 파라미터갯수가 늘어서 고민을 해야할 것 같습니다.

				//'====== 다이나믹쿼리 사용 예 ======
				//For Each item As sqlPalam In params
				//    p_strQry = p_strQry.Replace(item.palamName, item.value)
				//    ' p_strQry 에 해당 값이 박힘.
				//Next
				//'====== 다이나믹쿼리 사용 예 ======

				//====== 파라미터쿼리 사용 예 ======



				// 큐를 만들고 큐에서 파라미터를 받는 이유 :
				// 파라미터를 Add 하는 방식들중 OLEDB는 쿼리에 ? 를 주고 ADD를 각각의 변수별로 중복등을 가리지 않고 순차적으로 넣고
				// 데이터엑세스의 경우에도 ADD를 파라미터명 을 주고 중복을 가리지 않고 순차적으러 넣어야 한다고 들었습니다.
				// 그외에 어떤 커넥션(오라클클라이언트)은 맵핑만 해주면 각각의 맵핑될 파라미터만 넣으면 되는등 각각의 사용방법이 다르고
				// ?를 사용한다던지 명을 쓴다던지 하는 부분들을 신경쓰지 않고 동일한 방식으로 사용하고 싶어서 
				// 오라클클라이언트처럼 파라미터명에 벨류만 맵핑 시키는 방식과 비슷하게 모든 커넥션의 방식을 준용하기 위해 넣었습니다.
				// 주석으로 주(*) 해놓은 부분 앞에 전처리 지시문을 이용하여 커넥션 별로 #if 를 다른 타입으로 바꾸면 맵핑방식도 이용 가능합니다.

				// 해당부분의 설명 :
				// 쿼리중 파라미터를 문장의 처음부터 빠른 순으로 큐에 담고 그 순서로 PALAMETERS.ADD를 수행합니다.

				// 디버그를 위한 쿼리를 다이나믹쿼리와 동일하게 Replace를 하면 됨에도 따로 생성한 이유 :
				// 만에 하나 있을지 모르는 에러를 위해 실제 add된 부분과 동일하게 중복배제, 순차적으로 대입하여 보기 위함입니다.
				// 단 parameters.add 부분에는 object형 그대로 받고 (pValue)
				// 디버그쿼리에는 다이나믹쿼리와 동일하게 object형 의 타입별로 변형하여 (value) 프로퍼티를 받게 했습니다. 이는 바로 쿼리를 검증하기 위함입니다. 


				Queue<sqlPalam> queParam = new Queue<sqlPalam>();

				//주(*)   중복, 순서 지켜야 하는 커넥션 사용 시.
#if ORACLE
				makeQueue(p_strQry, @params, ref queParam);
#endif

				// 디버그용 쿼리문 
				string strQryDebug = "";

				sqlPalam tPalam = null;

				strQryDebug = p_strQry;

				//주(*)   중복, 순서 지켜야 하는 커넥션 사용 시.
#if ORACLE
				while ((queParam.Count > 0))
				{
					tPalam = queParam.Dequeue();
					comMain.Parameters.Add(tPalam.getParams).Value = tPalam.pValue;
					strQryDebug = Strings.Replace(strQryDebug, tPalam.palamName, tPalam.value, 1, 1);
				}
#endif


				//주(*) 상단부분 이외의 경우 :
#if 맵핑방식
			foreach (sqlPalam item in @params) {
				comMain.Parameters.Add(item.getParams).Value = item.pValue;
				strQryDebug = Strings.Replace(strQryDebug, item.palamName, tPalam.value, 1, 1);
			}
#endif

				//주(*) oledb 의 경우입니다. 
#if ORACLE
				foreach (sqlPalam item in @params)
				{
					p_strQry = p_strQry.Replace(item.palamName, "?");
				}
#endif


				//디버그
				//====== 파라미터쿼리 사용 예 ======
				return daMain.Fill(p_dtOther);

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

				if ((p_blReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					if (DbErr.Message.IndexOf("ORA-00054") != -1 || strErrMsg.IndexOf("ORA-30006") != -1)
					{
						// No wait 를 사용할 경우
						nErrKind = DB_LOCK;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg(p_blMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((p_blReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg(p_blMsgBox, "APP");
				}
			}

			return DB_ERR;

		}
		//*** DB 파라미터 클래스 **************************************************************************

		private void makeQueue(string strQry, sqlPalam[] sParams, ref Queue<sqlPalam> qParams)
		{
			int i = 0;
			sqlPalam sPalam = null;

			//가장 앞에 있는 파라미터 찾아옴
			foreach (sqlPalam item in sParams)
			{
				if (strQry.IndexOf(item.palamName) < 0)
				{
					continue;
				}
				if (i > strQry.IndexOf(item.palamName) | i == 0)
				{
					sPalam = item;
					i = strQry.IndexOf(item.palamName);
				}
			}

			if ((sPalam == null))
			{
				return;
			}

			//파라미터 인큐
			qParams.Enqueue(sPalam);

			//재탐색
			makeQueue(strQry.Substring(i + 1), sParams, ref qParams);

		}

		public int ExcuteQry_Call(string parCALL, ref DataTable dtOther, string strQry, bool bMsgBox = true, bool bReturnErr = false)
		{
			try
			{
				if (conMain.State != ConnectionState.Open)
				{
					conMain.Open();
				}
				strErrMsg = "";
				// 바인딩 객체일 경우 연결유지, DATA만 클리어
				if (blBindingType)
				{
					dtOther.Clear();
				}
				else
				{
					dtOther.Reset();
				}

				Sql_Display_Call(parCALL, strQry);

				comMain.CommandType = CommandType.Text;
				comMain.CommandText = strQry;
				comMain.Parameters.Clear();

				return daMain.Fill(dtOther);

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

				if ((bReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					if (DbErr.Message.IndexOf("ORA-00054") != -1 || strErrMsg.IndexOf("ORA-30006") != -1)
					{
						// No wait 를 사용할 경우
						nErrKind = DB_LOCK;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg_Call(parCALL, bMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((bReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg_Call(parCALL, bMsgBox, "APP");
				}
			}

			return DB_ERR;

		}

		//*********************************************************************************
		// None Query For insert, update, ...
		// Parameter: bRerutnErr (에러 발생시, 에러를 Return할 지 여부)
		// Return: 성공 - 반영된 레코드 수 (양의 정수)
		//         실패 - DB_ERR(-1):  일반 DB Err
		//         실패 - DB_LOCK(-2): DB Lock
		//         실패 - DB_DUP(-3):  데이타 중복
		public int ExcuteNonQry(string p_strQry, bool p_blMsgBox = true, bool p_blReturnErr = false)
		{
			try
			{
				strErrMsg = "";
				Sql_Display(p_strQry);

				comMain.CommandType = CommandType.Text;
				comMain.CommandText = p_strQry;
				comMain.Parameters.Clear();

				return comMain.ExecuteNonQuery();

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

				if ((p_blReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					if (DbErr.Message.IndexOf("ORA-00001") != -1)
					{
						nErrKind = DB_DUP;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg(p_blMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((p_blReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg(p_blMsgBox, "APP");
				}
			}

			return DB_ERR;

		}

		public int ExcuteNonQry_Call(string parCALL, string strQry, bool bMsgBox = true, bool bReturnErr = false)
		{
			try
			{
				if (conMain.State != ConnectionState.Open)
				{
					conMain.Open();
				}
				strErrMsg = "";
				Sql_Display_Call(parCALL, strQry);

				comMain.CommandType = CommandType.Text;
				comMain.CommandText = strQry;
				comMain.Parameters.Clear();

				return comMain.ExecuteNonQuery();

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

				if ((bReturnErr))
				{
					throw DbErr;
				}
				else
				{
					strErrMsg = DbErr.Message;
					if (DbErr.Message.IndexOf("ORA-00001") != -1)
					{
						nErrKind = DB_DUP;
					}
					else
					{
						nErrKind = 0;
					}
					ShowErrMsg_Call(parCALL, bMsgBox);
				}
			}
			catch (Exception AppErr)
			{
				if ((bReturnErr))
				{
					throw AppErr;
				}
				else
				{
					strErrMsg = AppErr.Message;
					nErrKind = 0;
					ShowErrMsg_Call(parCALL, bMsgBox, "APP");
				}
			}

			return DB_ERR;

		}

		public void BeginTrans(bool p_blDistributedTrans = false)
		{
			//*********************************************************************************
			// Transction 객체 할당
			try
			{
				if (conMain.State != ConnectionState.Open)
				{
					conMain.Open();
				}
				trnMain = conMain.BeginTransaction();
				comMain.Transaction = trnMain;
				blTran = true;

#if SQL
			//----------------------------------------------------------------------
			// 분산 트랜잭션을 위한 처리
			// ORACLE의 경우는 처리 할 필요가 없음
			// SQL SERVER의 경우 다른 서버를 같은 트랜잭션으로 처리시 필요
			//----------------------------------------------------------------------
			if (p_blDistributedTrans == true) {
				string strSql = null;
				int nSelCnt = 0;

				//----------------------------------------------------------------------
				// SET XACT_ABORT { ON | OFF }
				// Transact-SQL 문에서 런타임 오류가 발생할 경우 SQL Server 에서
				// 현재 트랜잭션을 자동으로 롤백할 것인지 여부를 지정합니다.
				//----------------------------------------------------------------------
				strSql = modDefApp.CRLF + " SET XACT_ABORT ON  ";
				nSelCnt = ExcuteNonQry(strSql);

				strSql = modDefApp.CRLF + " BEGIN DISTRIBUTED TRANSACTION ";
				nSelCnt = ExcuteNonQry(strSql);
			}
#endif
			}
			catch (Exception AppErr)
			{
				throw AppErr;
			}
		}

		public void RollbackTrans(bool p_blDistributedTrans = false)
		{
			try
			{
#if SQL
			//----------------------------------------------------------------------
			// 분산 트랜잭션을 위한 처리
			// ORACLE의 경우는 처리 할 필요가 없음
			// SQL SERVER의 경우 다른 서버를 같은 트랜잭션으로 처리시 필요
			//----------------------------------------------------------------------
			if (p_blDistributedTrans == true) {
				string strSql = null;
				int nSelCnt = 0;

				//strSql = CRLF & " ROLLBACK TRANSACTION  "
				//nSelCnt = ExcuteNonQry(strSql)

				strSql = modDefApp.CRLF + " SET XACT_ABORT OFF ";
				nSelCnt = ExcuteNonQry(strSql);
			}
#endif

				//If g_conDb.State <> ConnectionState.Closed Then
				//    g_conDb.Close()
				//    blTran = False
				//End If
				if (blTran == true)
					trnMain.Rollback();
				blTran = false;

				//trnMain.Rollback()
			}
			catch (Exception AppErr)
			{
				// Throw AppErr
				ShowErrMsg(true, AppErr.Message);
			}
		}

		public void CommitTrans(bool p_blDistributedTrans = false)
		{
			try
			{
#if SQL
	
			//----------------------------------------------------------------------
			// 분산 트랜잭션을 위한 처리
			// ORACLE의 경우는 처리 할 필요가 없음
			// SQL SERVER의 경우 다른 서버를 같은 트랜잭션으로 처리시 필요
			//----------------------------------------------------------------------
			if (p_blDistributedTrans == true) {
				string strSql = null;
				int nSelCnt = 0;

				strSql = modDefApp.CRLF + " COMMIT TRANSACTION  ";
				nSelCnt = ExcuteNonQry(strSql);

				strSql = modDefApp.CRLF + " SET XACT_ABORT OFF ";
				nSelCnt = ExcuteNonQry(strSql);
			}
#endif

                if (blTran == true)
					trnMain.Commit();
				blTran = false;

				//trnMain.Commit()
			}
			catch (Exception AppErr)
			{
				throw AppErr;
			}
		}

		//    Public Sub Sql_Display(ByVal strQry As String)

		//#If Not SERVER_PROGRAM Then
		//        gLast_Job_Time = Now
		//        gLast_Job_Start = True

		//        If wms_config("SQL_DISPLAY") = "TRUE" Then
		//            Dim SqlDsp As New MSG

		//            SqlDsp.strTitle = "SQL DISPLAY"

		//            SqlDsp.ChkTime.Visible = True
		//            SqlDsp.ChkTime.Checked = True
		//            SqlDsp.bReStart = False

		//            SqlDsp.PicErr.Visible = False
		//            SqlDsp.lblSql.Text = "SQL문"
		//            SqlDsp.lblSql.Visible = True

		//            SqlDsp.strMsg = strQry

		//            SqlDsp.ShowDialog()
		//        End If
		//#End If

		//    End Sub

		public void Sql_Display(string strQry, bool bTimeApy = true)
		{
#if !SERVER_PROGRAM
			//try
			//{
			//    if (bTimeApy == true)
			//    {
			//        modSpDb.gLast_Job_Time = System.DateTime.Now;
			//        modSpDb.gLast_Job_Start = true;
			//    }

			//    if (modDefApp.wms_config["SQL_DISPLAY"] == "TRUE")
			//    {

			//        MSG SqlDsp = new MSG();

			//        SqlDsp.m_strTitle = "SQL DISPLAY";

			//        SqlDsp.ChkTime.Visible = true;
			//        SqlDsp.ChkTime.Checked = true;
			//        SqlDsp.m_blReStart = false;

			//        SqlDsp.PicErr.Visible = false;
			//        SqlDsp.lblSql.Text = "SQL문";
			//        SqlDsp.lblSql.Visible = true;

			//        SqlDsp.m_strMsg = strQry;

			//        SqlDsp.ShowDialog();

			//    }

			//}
			//catch (Exception ex)
			//{
			//}
#endif

		}

		public void Sql_Display_Call(string parCALL, string strQry)
		{
#if !SERVER_PROGRAM
			//modSpDb.gLast_Job_Time = DateTime.Now;
			//modSpDb.gLast_Job_Start = true;
			//if (modDefApp.wms_config["SQL_DISPLAY"] == "TRUE")
			//{

			//    MSG SqlDsp = new MSG();

			//    SqlDsp.m_strTitle = "SQL DISPLAY";

			//    SqlDsp.ChkTime.Visible = true;
			//    SqlDsp.ChkTime.Checked = true;
			//    SqlDsp.m_blReStart = false;

			//    SqlDsp.PicErr.Visible = false;
			//    SqlDsp.lblSql.Text = "SQL문";
			//    SqlDsp.lblSql.Visible = true;

			//    SqlDsp.m_strMsg = strQry;

			//    SqlDsp.ShowDialog();

			//}
			////#elif
			////#if PROGRAM_NM = "ECSCOMA" Or PROGRAM_NM = "ECSCOMC"
			////if (gDebug_Sql == true) {
			////    if (parCALL.ToUpper == "CLIENT") {
			////        ShowMsgClient(strQry, MSG_DBG);
			////    } else {
			////        ShowMsgServer(strQry, MSG_DBG);
			////    }
			////}
			////#endif

#endif

		}

		//*** DB 파라미터 클래스 **************************************************************************
		/// <summary>
		/// 쿼리의 parameter 정의
		/// 주의사항 : length는 디폴트 0(0일시 입력 없음) 필요시 변경
		/// </summary>
		/// <remarks></remarks>

		//*** DB 파라미터 클래스 **************************************************************************

		//Public Sub WmsTrans()
		//    '*********************************************************************************
		//    ' Transction 객체 할당
		//    Try
		//        trnMain = conMain.BeginTransaction
		//        comMain.Transaction = trnMain
		//        bTran = True

		//    Catch AppErr As Exception
		//        Throw AppErr
		//    End Try
		//End Sub

		//Public Sub WmsCommit()
		//    '*********************************************************************************
		//    ' Transction 객체 할당
		//    Try
		//        If bTran Then trnMain.Commit()
		//        bTran = False
		//    Catch AppErr As Exception
		//        Throw AppErr
		//    End Try
		//End Sub

		//Public Sub WmsRollback()
		//    '*********************************************************************************
		//    ' Transction 객체 할당
		//    Try
		//        If bTran Then trnMain.Rollback()
		//        bTran = False
		//    Catch AppErr As Exception
		//        Throw AppErr
		//    End Try
		//End Sub
	}

	public class sqlPalam
	{
		private readonly string stPalamName;
		private readonly object ojValue;

		private readonly int iLength;
		public sqlPalam(string PalamName, object Value, int Length = 0)
		{
			stPalamName = PalamName;
			ojValue = Value;
			iLength = Length;
		}

		public string palamName
		{
			// : 는 제가 정했습니다. qry 에서 : 하고 명을 동일하게 쓰면 인식하게 하기 위함인데 @등 빈도순으로 잘 사용되지 않는 기호로 대체 가능합니다.
			get
			{
				return modSpDb.DbBindVal + stPalamName;
			}
		}

		public string value
		{
			get
			{
				if (ojValue.GetType() == typeof(System.String))
				{
					return "'" + ojValue.ToString().Replace("'", "''") + "'";
				}
				else if (ojValue.GetType() == typeof(System.DateTime))
				{
					return "to_date('" + ((DateTime)ojValue).ToString("yyyy-MM-dd-HH-mm-ss") + "', 'yyyy-mm-dd-hh24-mi-ss')";
				}
				else
				{
					return ojValue.ToString();
				}
			}
		}

		public object pValue
		{
			get
			{
				return ojValue;
			}
		}

#if ORACLE
		public OleDbType varType
		{
			get
			{
				//switch (ojValue.GetType())
				//{
				if (ojValue.GetType() == typeof(System.String))
				{
#if ORACLE
					return OleDbType.VarChar;
#endif
				}


				//Return OracleDbType.Varchar2
				if (ojValue.GetType() == typeof(System.Int32))
				{
					//Return OracleDbType.Decimal
#if ORACLE
					return OleDbType.Integer;
#endif
				}
				if (ojValue.GetType() == typeof(System.Int64))
				{
					//Return OracleDbType.Decimal
#if ORACLE
					return OleDbType.Integer;
#endif
				}
				if (ojValue.GetType() == typeof(System.DateTime))
				{
#if ORACLE
					return OleDbType.Date;
#endif
				}
				else
				{
					// 미처 예견하지 못한 데이터 형식의 경우 사용자에게 메세지박스 표시하여 개발자에게 전달 유도.
					MessageBox.Show("데이터 형이 선택되어지지 않았습니다. 개발자에게 문의하세요. 데이터 형 : " + ojValue.GetType().ToString());
					//Return OracleDbType.Varchar2
					return OleDbType.VarChar;
				}
				//}
			}
		}
#endif

#if SQL
        public SqlDbType varType
        {
            get
            {
                //switch (ojValue.GetType())
                //{
                if (ojValue.GetType() == typeof(System.String))
                {
                    return SqlDbType.VarChar;
                }


                //Return OracleDbType.Varchar2
                if (ojValue.GetType() == typeof(System.Int32))
                {
                    //Return OracleDbType.Decimal
                    return SqlDbType.Int;
                }
                if (ojValue.GetType() == typeof(System.Int64))
                {
                    //Return OracleDbType.Decimal
                    return SqlDbType.Int;
                }
                if (ojValue.GetType() == typeof(System.DateTime))
                {
                    return SqlDbType.Date;
                }
                else
                {
                    // 미처 예견하지 못한 데이터 형식의 경우 사용자에게 메세지박스 표시하여 개발자에게 전달 유도.
                    MessageBox.Show("데이터 형이 선택되어지지 않았습니다. 개발자에게 문의하세요. 데이터 형 : " + ojValue.GetType().ToString());
                    //Return OracleDbType.Varchar2
                    return SqlDbType.VarChar;
                }
                //}
            }
        }
#endif

#if ORACLE
		public OleDbParameter getParams
		{
			get
			{
				if (iLength == 0)
				{
					// 다른 타입 정의할때 OracleParameter 바꿔서 사용
					//Return New OracleParameter(palamName, varType)
#if ORACLE
					return new OleDbParameter("?", varType);
#endif
				}
				else
				{
					// 다른 타입 정의할때 OracleParameter 바꿔서 사용
					//Return New OracleParameter(palamName, varType)
#if ORACLE
					return new OleDbParameter("?", varType, iLength);
#endif

				}
			}
		}
#endif

#if SQL
        public SqlParameter getParams
        {
            get
            {
                if (iLength == 0)
                {
                    // 다른 타입 정의할때 OracleParameter 바꿔서 사용
                    //Return New OracleParameter(palamName, varType)
#if SQL
                    return new SqlParameter("?", varType);
#endif
                }
                else
                {
                    // 다른 타입 정의할때 OracleParameter 바꿔서 사용
                    //Return New OracleParameter(palamName, varType)
#if SQL
                    return new SqlParameter("?", varType, iLength);
#endif
                }
            }
        }
#endif

		public int llength
		{
			get
			{
				return iLength;
			}
		}
	}
}

//160222
//    Public Function ExcuteQry(ByVal p_strQry As String, _
//                              Optional ByVal p_blMsgBox As Boolean = True, _
//                              Optional ByVal p_blReturnErr As Boolean = False) As Integer
//        Try
//            strErrMsg = ""
//            ' 바인딩 객체일 경우 연결유지, DATA만 클리어
//            If bBindingType Then
//                dtMain.Clear()
//            Else
//                dtMain.Reset()
//            End If

//            Sql_Display(p_strQry)

//            comMain.CommandType = CommandType.Text
//            comMain.CommandText = p_strQry
//            comMain.Parameters.Clear()

//            Return daMain.Fill(dtMain)

//#if ORACLE Then
//        Catch DbErr As OleDbException
//#End If
//#if SQL Then
//        Catch DbErr As SqlException
//#End If
//            If (p_blReturnErr) Then
//                Throw DbErr
//            Else
//                strErrMsg = DbErr.Message ' & CRLF & DbErr.StackTrace
//                If strErrMsg.IndexOf("ORA-00054") <> -1 OrElse strErrMsg.IndexOf("ORA-30006") <> -1 Then
//                    ' No wait 를 사용할 경우
//                    nErrKind = DB_LOCK
//                Else
//                    nErrKind = 0
//                End If
//                ShowErrMsg(p_blMsgBox)
//            End If
//        Catch AppErr As Exception
//            If (p_blReturnErr) Then
//                Throw AppErr
//            Else
//                strErrMsg = AppErr.Message
//                nErrKind = 0
//                ShowErrMsg(p_blMsgBox, "APP")
//            End If
//        End Try

//        Return DB_ERR

//    End Function

