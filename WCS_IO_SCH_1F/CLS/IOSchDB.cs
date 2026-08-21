using NpgsqlTypes;
using Samoh_Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TSK_COMM_IOSCH
{
    public enum EN_JOB_TYPE
    {
        enJobTypeNone = 0,
        enJobTypeAutoSto = 1,
        enJobTypeAutoRet = 2,
        enJobTypeAutoPR = 3,
        enJobTypeAutoR2R = 4,
        enJobTypeAutoW2W = 5,
        enJobTypeAutoMove = 6,
        enJobTypeSemiSto = 11,
        enJobTypeSemiRet = 12,
        enJobTypeSemiPR = 13,
        enJobTypeSemiR2R = 14,
        enJobTypeSemiW2W = 15,
        enJobTypeSemiMove = 10,
        enJobTypeManual = 21
    }
    public enum EN_JOB_TYPE_STR
    {
        [Description("0")] enJobTypeNone,
        [Description("1")] enJobTypeAutoSto,
        [Description("2")] enJobTypeAutoRet,
        [Description("3")] enJobTypeAutoPR,
        [Description("4")] enJobTypeAutoR2R,
        [Description("5")] enJobTypeAutoW2W,
        [Description("6")] enJobTypeAutoMove,
        [Description("11")] enJobTypeSemiSto,
        [Description("12")] enJobTypeSemiRet,
        [Description("13")] enJobTypeSemiPR,
        [Description("14")] enJobTypeSemiR2R,
        [Description("15")] enJobTypeSemiW2W,
        [Description("10")] enJobTypeSemiMove,
        [Description("21")] enJobTypeManual
    };

    public enum EN_JOB_PATTERN
    {
        enJobPatternNone,
        enJobPatternSto,
        enJobPatternRet,
        enJobPatternPR,
        enJobPatternR2R,
        enJobPatternW2W,
        enJobPatternMove
    }; public class IOSchDB : MainClass
    {
#if ORACLE
        //RFID수신 후 작업처리.
        public bool B_SP_CHK_DIV1_JOB_CREATE(ref CUserDb argBdb, string strAREA_NO, string strSITENO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_DIV1_JOB_CREATE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_DIV2_JOB_CREATE(ref CUserDb argBdb, string strAREA_NO, string strSITENO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_DIV2_JOB_CREATE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_DIV3_JOB_CREATE(ref CUserDb argBdb, string strAREA_NO, string strSITENO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_DIV3_JOB_CREATE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_DIV4_JOB_CREATE(ref CUserDb argBdb, string strAREA_NO, string strSITENO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_DIV4_JOB_CREATE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        //화물 도착 후 이동처리(각지점)
        public bool ECS_SP_CHK_CV_MOVE_CREATE(ref CUserDb argBdb, string strWH_TYP, string strSITENO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "ECS_SP_CHK_CV_MOVE_CREATE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vWH_TYP", DbLang.VARCHAR, 255).Value = strWH_TYP;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        public bool ECS_SP_CHK_SC_JOB_CALL(ref CUserDb argBdb, string strWH_TYP, string strSC_NO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "ECS_SP_CHK_SC_JOB_CALL";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vWH_TYP", DbLang.VARCHAR, 255).Value = strWH_TYP;
            argBdb.comMain.Parameters.Add("IN_vSC_NO", DbLang.VARCHAR, 255).Value = strSC_NO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool ECS_SP_CHK_SC_INTER_LOCK(ref CUserDb argBdb, string strWH_TYP, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "ECS_SP_CHK_SC_INTER_LOCK";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vWH_TYP", DbLang.VARCHAR, 255).Value = strWH_TYP;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_RECHECK_JOB(ref CUserDb argBdb, string strAREA_NO, string strWH_NO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_RECHECK_JOB";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_RFID2_JOB_CRT(ref CUserDb argBdb, string strAREA_NO, string strSITENO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_RFID2_JOB_CRT";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_RFID3_JOB_CRT(ref CUserDb argBdb, string strAREA_NO, string strSITENO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_RFID3_JOB_CRT";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_RFID4_JOB_CRT(ref CUserDb argBdb, string strAREA_NO, string strSITENO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_RFID4_JOB_CRT";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        //입고 작업생성.
        public bool ECS_SP_CHK_COMP_ARR_CV(ref CUserDb argBdb, string strWH_TYP, string strSITENO1, string strSITENO2, string strSITENO3, string strSITENO4, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "ECS_SP_CHK_COMP_ARR_CV";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vWH_TYP", DbLang.VARCHAR, 255).Value = strWH_TYP;
            argBdb.comMain.Parameters.Add("IN_vSITENO1", DbLang.VARCHAR, 255).Value = strSITENO1;
            argBdb.comMain.Parameters.Add("IN_vSITENO2", DbLang.VARCHAR, 255).Value = strSITENO2;
            argBdb.comMain.Parameters.Add("IN_vSITENO3", DbLang.VARCHAR, 255).Value = strSITENO3;
            argBdb.comMain.Parameters.Add("IN_vSITENO4", DbLang.VARCHAR, 255).Value = strSITENO4;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_IN_31JOB_CREATE(ref CUserDb argBdb, string strAREA_NO, string strWH_NO, string strSITENO, string strRP_NO1, string strRP_NO2, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_IN_31JOB_CREATE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;
            argBdb.comMain.Parameters.Add("IN_vSITENO", DbLang.VARCHAR, 255).Value = strSITENO;
            argBdb.comMain.Parameters.Add("IN_vRP_NO1", DbLang.VARCHAR, 255).Value = strRP_NO1;
            argBdb.comMain.Parameters.Add("IN_vRP_NO2", DbLang.VARCHAR, 255).Value = strRP_NO2;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_MERGE(ref CUserDb argBdb, string strAREA_NO, string strWH_NO, string strRP_NO1, string strRP_NO2, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_MERGE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;
            argBdb.comMain.Parameters.Add("IN_vRP_NO1", DbLang.VARCHAR, 255).Value = strRP_NO1;
            argBdb.comMain.Parameters.Add("IN_vRP_NO2", DbLang.VARCHAR, 255).Value = strRP_NO2;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_COMP_RET(ref CUserDb argBdb, string strAREA_NO, string strWH_NO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_COMP_RET";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_EQUALITY(ref CUserDb argBdb, string strAREA_NO, string strWH_NO, string strRP_NO1, string strRP_NO2, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_EQUALITY";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;
            argBdb.comMain.Parameters.Add("IN_vRP_NO1", DbLang.VARCHAR, 255).Value = strRP_NO1;
            argBdb.comMain.Parameters.Add("IN_vRP_NO2", DbLang.VARCHAR, 255).Value = strRP_NO2;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool B_SP_CHK_EMG_TO_NOEMG(ref CUserDb argBdb, string strAREA_NO, string strWH_NO, string strRP_NO1, string strRP_NO2, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_EMG_TO_NOEMG";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;
            argBdb.comMain.Parameters.Add("IN_vRP_NO1", DbLang.VARCHAR, 255).Value = strRP_NO1;
            argBdb.comMain.Parameters.Add("IN_vRP_NO2", DbLang.VARCHAR, 255).Value = strRP_NO2;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool ECS_SP_CHK_COMP_ARR_SC(ref CUserDb argBdb, string strWH_TYP, string strSC_NO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "ECS_SP_CHK_COMP_ARR_SC";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vWH_TYP", DbLang.VARCHAR, 255).Value = strWH_TYP;
            argBdb.comMain.Parameters.Add("IN_vSC_NO", DbLang.VARCHAR, 255).Value = strSC_NO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool ECS_SP_DEL_HISTORY(ref CUserDb argBdb, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "ECS_SP_DEL_HISTORY";
            argBdb.comMain.Parameters.Clear();

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        
        public bool SP_CHK_ASRS_3F_CV_MV_DIV(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_3F_CV_MV_DIV";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        public bool SP_CHK_ASRS_3F_CV_MV_251(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_3F_CV_MV_251";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }


        public bool SP_CHK_ASRS_CV_MV_PADIV(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_CV_MV_PADIV";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_ALFT_CV_MOVE(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ALFT_CV_MOVE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        public bool SP_CHK_ASRS_3F_MG_CV_MOVE(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_3F_MG_CV_MOVE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_ASRS_CV_MV_MZ(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_CV_MV_MZ";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_ASRS_CV_MV_PA_PK(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_CV_MV_PA_PK";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_SC_RET_CV_MOVE(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_SC_RET_CV_MOVE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_SIDE_SC_JOB_CALL(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_SIDE_SC_JOB_CALL";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_ASRS_SC_JOB_CALL(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_SC_JOB_CALL";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_SC_ERR_JOB_DOING(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_SC_ERR_JOB_DOING";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_SC_WECS_ERROR(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_SC_WECS_ERROR";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        public bool SP_CHK_SC_COMP(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_SC_COMP";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }


        public bool SP_CHK_ASRS_COMP_CV(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_COMP_CV";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_MG_JOB_CREATE(ref CUserDb argBdb,
                                                  string strGRP_TYP,
                                                  string strSTRG_TYP,
                                                  string strEQMT_TYP,
                                                  string strGRP_NO,
                                                  string strMC_NO,
                                              ref string strRtnMsg,
                                                 ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_MG_JOB_CREATE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            argBdb.comMain.Parameters.Add("IN_vEQMT_TYP", DbLang.VARCHAR, 255).Value = strEQMT_TYP;
            argBdb.comMain.Parameters.Add("IN_vGRP_NO", DbLang.VARCHAR, 255).Value = strGRP_NO;
            argBdb.comMain.Parameters.Add("IN_vMC_NO", DbLang.VARCHAR, 255).Value = strMC_NO;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        public bool SP_CHK_SIDE_IN_HS_REQUEST(ref CUserDb argBdb, string strGRP_TYP, string strSTRG_TYP, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_SIDE_IN_HS_REQUEST";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        //입고 HS 도착시 작업 요청 프로시저 
        public bool SP_CHK_ASRS_IN_HS_REQUEST(ref CUserDb argBdb, string strGRP_TYP,string strSTRG_TYP, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_ASRS_IN_HS_REQUEST";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            argBdb.comMain.Parameters.Add("IN_vSTRG_TYP", DbLang.VARCHAR, 255).Value = strSTRG_TYP;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        //WCS -> WMS 작업 삭제 및 작업 완료 IF
        public bool SP_CHK_WMS_CMP_DEL_JOB_REPORT(ref CUserDb argBdb,string strGRP_TYP, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_WMS_CMP_DEL_JOB_REPORT";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        //WCS -> WMS 입고 작업 관련 IF
        public bool SP_CHK_WMS_IN_JOB_REPORT(ref CUserDb argBdb, string strGRP_TYP, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_WMS_IN_JOB_REPORT";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_IF_WCS_OTR_JOB_INFO(ref CUserDb argBdb, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_IF_WCS_OTR_JOB_INFO";
            argBdb.comMain.Parameters.Clear();

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }


        public bool SP_CHK_DEL_HISTORY(ref CUserDb argBdb, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_DEL_HISTORY";
            argBdb.comMain.Parameters.Clear();

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        public bool SP_CHK_IF_WCS_OUT_JOB_INFO(ref CUserDb argBdb, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_IF_WCS_OUT_JOB_INFO";
            argBdb.comMain.Parameters.Clear();

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        public bool SP_CHK_IF_WCS_IN_JOB_INFO(ref CUserDb argBdb, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_IF_WCS_IN_JOB_INFO";
            argBdb.comMain.Parameters.Clear();

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        
            //WCS -> WMS SC 작업 관련 IF
        public bool SP_CHK_INS_EQM_R_WCS_EQM_ERR(ref CUserDb argBdb, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_INS_EQM_R_WCS_EQM_ERR";
            argBdb.comMain.Parameters.Clear();

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        public bool SP_CHK_INS_IF_EQM_R_WCS_STAT(ref CUserDb argBdb, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_INS_IF_EQM_R_WCS_STAT";
            argBdb.comMain.Parameters.Clear();

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        //WCS -> WMS SC 작업 관련 IF
        public bool SP_CHK_WMS_SC_JOB_REPORT(ref CUserDb argBdb, string strGRP_TYP, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_WMS_SC_JOB_REPORT";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vGRP_TYP", DbLang.VARCHAR, 255).Value = strGRP_TYP;
            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
        //작업 완료 CALL.
        public bool B_SP_CHK_COMPLETE(ref CUserDb argBdb, string strAREA_NO, string strWH_NO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_COMPLETE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        //입고대 홈 이동.
        public bool B_SP_CHK_IN_HOME_CREATE(ref CUserDb argBdb, string strWH_NO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "B_SP_CHK_IN_HOME_CREATE";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }

        //입고대 홈 이동.
        public bool SP_CHK_RP_TEST(ref CUserDb argBdb, string strAREA_NO, string strWH_NO, string strRP_NO, ref string strRtnMsg, ref int nRetCd)
        {
            argBdb.comMain.CommandText = "SP_CHK_RP_TEST";
            argBdb.comMain.Parameters.Clear();

            argBdb.comMain.Parameters.Add("IN_vAREA_NO", DbLang.VARCHAR, 255).Value = strAREA_NO;
            argBdb.comMain.Parameters.Add("IN_vWH_NO", DbLang.VARCHAR, 255).Value = strWH_NO;
            argBdb.comMain.Parameters.Add("IN_vRP_NO", DbLang.VARCHAR, 255).Value = strRP_NO;

            //OUT
            argBdb.comMain.Parameters.Add("OT_nRETCD", DbLang.INTEGER).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vMSG", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;
            argBdb.comMain.Parameters.Add("OT_vDBERR", DbLang.VARCHAR, 255).Direction = ParameterDirection.Output;

            argBdb.comMain.CommandType = CommandType.StoredProcedure;
            argBdb.comMain.ExecuteScalar();

            if (System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value) != 0)
            {
                nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
                strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value) + " DB ERR MESSAGE [" + System.Convert.ToString(argBdb.comMain.Parameters["OT_vDBERR"].Value) + "]";
                return false;
            }

            nRetCd = System.Convert.ToInt32(argBdb.comMain.Parameters["OT_nRETCD"].Value);
            strRtnMsg = System.Convert.ToString(argBdb.comMain.Parameters["OT_vMSG"].Value);
            return true;
        }
#elif POSTGRESQL || SQL
        public PsMsgView callPsMsgView = null;
        public SYS_MAIN m_Main;
        public Thread m_Thread;
        public int m_nId = 0;
        public string _strErrorMsg = "";


        // JOB_MST.JOB_STATUS 라이프사이클 (대기 상태 없음 - '99' 또는 직전  완료가 대기 역할)
        //   CV : 99 → 지시 11 → 중 15 → 완료 19
        //   SC : 99 → 지시 21 → 중 25 → 완료 29
        //   입고(JOB_TYP 1)   : 99 → 11 → 15 → 19(CV완료=SC대기) → 21 → 25 → 29(최종)
        //   출고(JOB_TYP 2,3) : 99 → 21 → 25 → 29(SC완료=CV대기) → 11 → 15 → 19(최종)
        public const string ST_CV_CMD = "11"; // CV 구동지시
        public const string ST_CV_RUN = "15"; // CV 구동중
        public const string ST_CV_DONE = "19"; // CV 구동완료 (입고는 SC  대기)

        public const string ST_SC_CMD = "21"; // SC 구동지시
        public const string ST_SC_RUN = "25"; // SC 구동중
        public const string ST_SC_DONE = "29"; // SC 구동완료 (출고는 CV  대기)

        // HOST_TASK 신규 작업 (WCS_TASK_HOST frmMain.InsertJobMst 가 '99' 로 INSERT)
        public const string ST_NEW = "99";

        // JOB_MST.JOB_TYP (레거시 ECS EcsEnv.h JOB_TYPE_* 승계 - HOST 가 그대로 전달)
        //   1 = 입고(UNIT_STO), 2 = 출고(UNIT_RET), 3 = 픽킹출고(PICKING), 4 = 재배치(RackToRack), 5 = 호기간이동(AisleToAisle), 6 = 이동(MOVE)
        public const string JT_STO = "1";
        public const string JT_RET = "2";
        public const string JT_PICK = "3";
        public const string JT_R2R = "4";
        public const string JT_A2A = "5";
        public const string JT_MOVE = "6";
        // SC PLC 명령 JOB_TYP_OD (레거시 SC_JOB_TYPE_* : 1=Store, 2=Retrieve)
        public const string SC_CMD_STORE = "1";
        public const string SC_CMD_RETRIEVE = "2";

        // SC_HS_DEF.HS_NO : 1층 홈스탠드 랭크 (레거시 ECS RANK_1=입고HS, RANK_2=출고HS)
        public const string HS_NO_STORE = "01"; // 입고 HS (CV → SC 픽업)
        public const string HS_NO_RETRIEVE = "02"; // 출고 HS (SC → CV 언로드)


        #region 화면 표시용.
        // ※ callPsMsgView / m_LogQ null 가드 (2026-07-11) :
        //    화면 델리게이트 연결 전(또는 미연결 상태)에 호출되어도 예외가 발생하지 않도록 한다.
        //  strFile / strFunc 는 넘기지 않으면 컴파일러가 채운다.
        //  이 함수를 부른 소스 파일 경로와 함수 이름이 그대로 들어와,
        //  화면 로그의 FILE / FUNCTION 열에 찍힌다.
        public void MakeMsg(string msg,
                            [CallerFilePath] string strFile = "",
                            [CallerMemberName] string strFunc = "")
        {
            try
            {
                if (callPsMsgView == null) return;
                callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_NOR, strFile, strFunc);
            }
            catch (Exception ex)
            {
                return;
            }
        }
        public void MakeMsg_Error_NoLog(string msg,
                                        [CallerFilePath] string strFile = "",
                                        [CallerMemberName] string strFunc = "")
        {
            //  DB 연결이 끊겨서 난 오류면 표시를 세운다.
            //  다음 주기에 IsDbAlive() 가 false 가 되어 다시 붙는다.
            if (IsBrokenConnError(msg)) m_bDbBroken = true;

            try
            {
                if (callPsMsgView == null) return;
                callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_ERR, strFile, strFunc);
            }
            catch (Exception ex)
            {
                return;
            }

        }
        public void MakeMsg_Error(string msg,
                                  [CallerFilePath] string strFile = "",
                                  [CallerMemberName] string strFunc = "")
        {
            if (IsBrokenConnError(msg)) m_bDbBroken = true;

            try
            {
                if (callPsMsgView != null)
                    callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_ERR, strFile, strFunc);
                if (cDefApp.m_LogQ[m_nId] != null)
                    cDefApp.m_LogQ[m_nId].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }

        }
        public void MakeMsg_Imp(string msg,
                                [CallerFilePath] string strFile = "",
                                [CallerMemberName] string strFunc = "")
        {
            try
            {
                if (callPsMsgView != null)
                    callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_IMP, strFile, strFunc);
                if (cDefApp.m_LogQ[m_nId] != null)
                    cDefApp.m_LogQ[m_nId].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }

        }
        #endregion
        public void SetErrorMsg(string strMsg)
        {
            _strErrorMsg = strMsg;
            Log.Error(_strErrorMsg);
        }

        public void InsertLog(string strWH_TYP
                            , string strLOG_KOR
                            , string strBCR_BOTTOM = ""
                            , string strBCR_TOP = ""
                            , string strLUGG_NO = "0"
                            , string strJOB_STA = ""
                            , string strTRACK_FROM = ""
                            , string strTRACK_TO = ""
                            , bool bTrans = true)
        {
            string strTitle = "[InsertLog]";
            string strSql;

            string strPGR_NO = "IO_TASK";
            int nRtn = 0;

            try
            {
                if (bTrans == true)
                    _pBdb.BeginTrans();

                strSql = "";
                strSql = cDefApp.CRLF + " INSERT INTO WCS_LOG_PGR (WH_TYP               ";    // 1  
                strSql += cDefApp.CRLF + "                       , INS_DT               ";    // 2
                strSql += cDefApp.CRLF + "                       , LOG_SEQ              ";    // 3
                strSql += cDefApp.CRLF + "                       , LUGG_NO              ";    // 4
                strSql += cDefApp.CRLF + "                       , BCR_BOTTOM           ";    // 5
                strSql += cDefApp.CRLF + "                       , BCR_TOP              ";    // 6
                strSql += cDefApp.CRLF + "                       , PGR_NM               ";    // 7
                strSql += cDefApp.CRLF + "                       , LOG_KOR              ";    // 8
                strSql += cDefApp.CRLF + "                       , TRACK_FROM           ";    // 9
                strSql += cDefApp.CRLF + "                       , TRACK_TO             ";    // 10
                strSql += cDefApp.CRLF + "                       , JOB_STA              ";    // 11
                strSql += cDefApp.CRLF + "                       , RQ_INS_ID            ";    // 12
                strSql += cDefApp.CRLF + "                       , RQ_INS_DT            ";    // 13
                strSql += cDefApp.CRLF + "                       , EQP_TYP)             ";    // 14
                strSql += cDefApp.CRLF + "                VALUES ( :wh_typ              ";    // 1     // WH_TYP  
                strSql += cDefApp.CRLF + "                       , " + DbLang.SYSDATE + "                ";    // 2     // CELL_SC_NO 
                strSql += cDefApp.CRLF + "                       , " + DbLang.NEXTVAL("LOG_SEQ");          //LOG_SEQ.NEXTVAL   ";    // 3     // CELL_SC_NO 
                strSql += cDefApp.CRLF + "                       , :lugg_no             ";    // 4     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :bcr_bottom           ";    // 5     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :bcr_top             ";    // 6     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :pgr_nm              ";    // 7     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :log_kor             ";    // 8     // CELL_SEQ      
                strSql += cDefApp.CRLF + "                       , :track_from          ";    // 9     // CELL_NO
                strSql += cDefApp.CRLF + "                       , :track_to            ";    // 10     // CELL_NO 
                strSql += cDefApp.CRLF + "                       , :job_sta             ";    // 11     // CELL_SEQ             
                strSql += cDefApp.CRLF + "                       , :rq_ins_id           ";    // 12     // AGING_START_DT  
                strSql += cDefApp.CRLF + "                       , " + DbLang.SYSDATE + "                ";    // 13    // REMARKS    
                strSql += cDefApp.CRLF + "                       , 'IO')                ";    // 14    // IO_TASK    

                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Add("wh_typ", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("lugg_no", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("bcr_bottom", DbLang.VARCHAR).Value = strBCR_BOTTOM;
                _pBdb.mComMain.Parameters.Add("bcr_top", DbLang.VARCHAR).Value = strBCR_TOP;
                _pBdb.mComMain.Parameters.Add("pgr_nm", DbLang.VARCHAR).Value = strPGR_NO;
                _pBdb.mComMain.Parameters.Add("log_kor", DbLang.VARCHAR).Value = strLOG_KOR;
                _pBdb.mComMain.Parameters.Add("track_from", DbLang.VARCHAR).Value = strTRACK_FROM;
                _pBdb.mComMain.Parameters.Add("track_to", DbLang.VARCHAR).Value = strTRACK_TO;
                _pBdb.mComMain.Parameters.Add("job_sta", DbLang.VARCHAR).Value = strJOB_STA;
                _pBdb.mComMain.Parameters.Add("rq_ins_id", DbLang.VARCHAR).Value = strPGR_NO;
                //_pBdb.mComMain.Parameters.Add("eqp_typ", DbLang.VARCHAR).Value = strEQP_TYP;
                nRtn = _pBdb.ExcuteNonQry(strSql);
                if (nRtn < 0)
                {
                    throw new Exception(strTitle + "LOG_PGR INSERT중 DATABASE 에러.. MESSAGE [" + _pBdb.ErrMsg + "]");
                }

                if (nRtn == 0)
                {
                    if (bTrans == true)
                        _pBdb.Rollback();
                    return;
                }

                if (bTrans == true)
                    _pBdb.Commit();
                return;
            }
            catch (Exception ex)
            {
                if (bTrans == true)
                    _pBdb.Rollback();
                MakeMsg_Error_NoLog(ex.Message);
                SetErrorMsg(ex.Message);
                return;
            }
        }
        public bool UPDATE_JOB_DATA(string strJOB_STATUS,
                                    string strLUGG_NO,
                                    string strWH_TYP,
                                    string strJOB_TYP,
                                ref string strRTN_MSG,
                                    string strDEST_POS = "0",
                                    string strCELL_NO = "0",
                                    string strHS_MC_NO = "0",
                                    string strSC_NO = "0",
                                    string strSTART_POS = "0",
                                    string strPAIR_LUGG_NO = "0",
                                    string strCMP_STEP = "0")
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                string strMSG = "[UPDATE_JOB_DATA]";

                //먼저 JOB_MST_HIS에 추가하고 바꾸기
                strSql = "";
                strSql += CRLF + " INSERT INTO JOB_MST_HIS      ";
                strSql += CRLF + "           ( INS_DATE         ";
                strSql += CRLF + "           , INS_TIME         ";
                strSql += CRLF + "           , WH_TYP           ";
                strSql += CRLF + "           , LUGG_NO          ";
                strSql += CRLF + "           , START_POS        ";
                strSql += CRLF + "           , START_LOCATION   ";
                strSql += CRLF + "           , DEST_POS         ";
                strSql += CRLF + "           , DEST_LOCATION    ";
                strSql += CRLF + "           , TURN             ";
                strSql += CRLF + "           , PRODUCT_ID       ";
                strSql += CRLF + "           , PRODUCT_SIZE     ";
                strSql += CRLF + "           , JOB_TYP          ";
                strSql += CRLF + "           , BCR_TOP          ";
                strSql += CRLF + "           , BCR_BOTTOM       ";
                strSql += CRLF + "           , MES_WRITE_YN     ";
                strSql += CRLF + "           , CMD_STA          ";
                strSql += CRLF + "           , JOB_KIND         ";
                strSql += CRLF + "           , JOB_STATUS       ";
                strSql += CRLF + "           , JOB_PRIORITY     ";
                strSql += CRLF + "           , MES_ERROR_CD     ";
                strSql += CRLF + "           , OD_LAST_PAGE     ";
                strSql += CRLF + "           , OD_LAST_USER     ";
                strSql += CRLF + "           , JOB_START_DT     ";
                strSql += CRLF + "           , INS_DT           ";
                strSql += CRLF + "           , INS_USER_ID      ";
                strSql += CRLF + "           , REMARKS          ";
                strSql += CRLF + "           , TRAY_TYP         ";
                strSql += CRLF + "           , TRAY_LEV         ";
                strSql += CRLF + "           , HS_TRACK_NO      ";
                strSql += CRLF + "           , SC_NO            ";
                strSql += CRLF + "           , DURATION_TIME    ";
                strSql += CRLF + "           , SYSTEM_BYTE      ";
                strSql += CRLF + "           , S6F12_RESPONSE   ";
                strSql += CRLF + "           , S6F11_YON        ";
                strSql += CRLF + "           , CEID_NAME        ";
                strSql += CRLF + "           , FK_NO            ";
                strSql += CRLF + "           , AGING_TIME       ";
                strSql += CRLF + "           , PAIR_LUGG_NO )   ";
                strSql += CRLF + " SELECT TO_CHAR(" + DbLang.SYSDATE + ", 'YYYYMMDD'), TO_CHAR(" + DbLang.SYSDATE + ", 'HH24MISS')                            ";
                strSql += CRLF + "      , WH_TYP, LUGG_NO, START_POS, START_LOCATION, DEST_POS, DEST_LOCATION                   ";
                strSql += CRLF + "      , TURN, PRODUCT_ID, PRODUCT_SIZE, JOB_TYP, BCR_TOP, BCR_BOTTOM, MES_WRITE_YN            ";
                strSql += CRLF + "      , CMD_STA, JOB_KIND, JOB_STATUS, JOB_PRIORITY, MES_ERROR_CD, OD_LAST_PAGE, OD_LAST_USER ";
                strSql += CRLF + "      , JOB_START_DT, " + DbLang.SYSDATE + ", 'IO_TASK', REMARKS, TRAY_TYP, TRAY_LEV, HS_TRACK_NO, SC_NO    ";
                strSql += CRLF + "      , DURATION_TIME, SYSTEM_BYTE, S6F12_RESPONSE, S6F11_YON, CEID_NAME, FK_NO, AGING_TIME   ";
                strSql += CRLF + "      , PAIR_LUGG_NO                  ";
                strSql += CRLF + "   FROM JOB_MST                       ";
                strSql += CRLF + "  WHERE WH_TYP   = :WH_TYP       ";
                strSql += CRLF + "    AND LUGG_NO  = :LUGG_NO   ";

                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;

                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "작업 히스토리를 추가 할 수 없습니다. LUGG_NO : " + strLUGG_NO;
                    return false;
                }

                strSql = "";
                strSql += CRLF + " UPDATE  JOB_MST                                      ";
                strSql += CRLF + "    SET  JOB_STATUS       =  :JOB_STATUS              ";
                if (strSTART_POS != "0")
                    strSql += CRLF + "    ,START_POS        =  '" + strSTART_POS + "'  ";
                if (strDEST_POS != "0")
                    strSql += CRLF + "    ,DEST_POS         =  '" + strDEST_POS + "'    ";
                if (strCELL_NO != "0" &&
                   (strJOB_TYP == ((int)cDefApp.eJOBTYP.Ret).ToString() || strJOB_TYP == ((int)cDefApp.eJOBTYP.ManualRet).ToString() || strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString()))
                    strSql += CRLF + "    ,START_LOCATION   =  '" + strCELL_NO + "'";
                if (strCELL_NO != "0" &&
                   (strJOB_TYP == ((int)cDefApp.eJOBTYP.Sto).ToString() || strJOB_TYP == ((int)cDefApp.eJOBTYP.ManualSto).ToString() || strJOB_TYP == ((int)cDefApp.eJOBTYP.RtoR).ToString()))
                    strSql += CRLF + "    ,DEST_LOCATION    =  '" + strCELL_NO + "'";
                if (strHS_MC_NO != "0")
                    strSql += CRLF + "    ,HS_TRACK_NO      =  '" + strHS_MC_NO + "' ";
                if (strSC_NO != "0")
                    strSql += CRLF + "    ,SC_NO            =  '" + strSC_NO + "'   ";
                if (strJOB_TYP == ((int)cDefApp.eJOBTYP.RackRet).ToString() && strJOB_STATUS == "11")
                    strSql += CRLF + "    ,JOB_TYP          =  '6'      ";
                if (strPAIR_LUGG_NO != "0")
                    strSql += CRLF + "    ,PAIR_LUGG_NO     =  '" + strPAIR_LUGG_NO + "'";
                if (strCMP_STEP != "0")
                    strSql += CRLF + "     ,CMP_STEP         = '" + strCMP_STEP + "'   ";
                strSql += CRLF + "        ,UPD_DT           = " + DbLang.SYSDATE + "   ";
                strSql += CRLF + "  WHERE  WH_TYP           = :WH_TYP   ";
                strSql += CRLF + "    AND  LUGG_NO          = :LUGG_NO  ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = strJOB_STATUS;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG = strMSG + _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG = strMSG + "변경할 JOB_MST 정보가 존재하지 않습니다";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                strRTN_MSG += ex.ToString();
                return false;
            }
        }
        public bool UPDATE_CV_DATA_COMMING(string strCOMMING,
                                   string strWH_TYP,
                               ref string strRTN_MSG,
                                   string strMC_NO)
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                string strMSG = "[UPDATE_CV_DATA_COMMING]";

                strSql = "";
                strSql += CRLF + " UPDATE  CV_DATA                         ";
                strSql += CRLF + "    SET  COMMING_RD       = :COMMING_RD  ";
                strSql += CRLF + "        ,CMD_RQ_YN        = 'Y'  ";
                strSql += CRLF + "        ,CMD_RQ_ID        = 'COMMING'  ";
                strSql += CRLF + "  WHERE  WH_TYP           = :WH_TYP      ";
                strSql += CRLF + "    AND  MC_NO            = :MC_NO       ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("COMMING_RD", DbLang.VARCHAR).Value = strCOMMING;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG = strMSG + _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG = strMSG + "변경할 CV_DATA 정보가 존재하지 않습니다";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                strRTN_MSG += ex.ToString();
                return false;
            }
        }

        public bool UPDATE_RTV_OD_RQ_YN(string strWH_TYP
                                        , string strRTV_NO
                                    , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG += "[UPDATE_RTV_OD_RQ_YN]";

                strSql = "";
                strSql += CRLF + " UPDATE RTV_DATA                ";
                strSql += CRLF + "    SET OD_RQ_YN     =   'N'    ";
                strSql += CRLF + "  WHERE WH_TYP       =   :WH_TYP";
                strSql += CRLF + "    AND OD_RQ_YN     =   'Y'    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("RTV_NO", DbLang.VARCHAR).Value = strRTV_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = "RTV 초기화 실패 RTV NO : " + strRTV_NO;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }



        // S/C에 작업쓰기.
        public bool UPDATE_SC_DATA_MURATA(string strWH_TYP
                                         , string strSC_NO
                                         , string strJOB_TYP
                                         , string strLUGG_NO
                                         , string strSOUR_HSPOS_FK
                                         , string strSOUR_BANK_FK
                                         , string strSOUR_BAY_FK
                                         , string strSOUR_LEV_FK
                                         , string strDEST_HSPOS_FK
                                         , string strDEST_BANK_FK
                                         , string strDEST_BAY_FK
                                         , string strDEST_LEV_FK
                                     , ref string pRTN_MSG
                                         , bool bReLocation = false)
        {
            try
            {
                string strSql = "";
                int nSelCnt = 0;

                pRTN_MSG += "[UPDATE_SC_DATA_MURATA]";

                #region QUERY 문 작성 
                // DEEP CELL 구하기 위한 변수 선언 
                string strDEEP_CELL = "0";
                string strJobType = strJOB_TYP;
                int nDoubleSide = 0;
                int nBank_FK = 0;

                if (bReLocation == true)
                {
                    strJobType = "20";  // 이미 위에서 쓰였음!
                }
                // S/C에 작업정보쓰기.
                strSql = "";
                strSql += CRLF + " UPDATE SC_DATA_MURATA                             ";
                strSql += CRLF + "    SET JOB_TYP           =  '" + strJobType + "'";
                strSql += CRLF + "      , LUGG_NO           =  '" + strLUGG_NO + "'";
                strSql += CRLF + "      , ITN_LUGG          =  '" + strLUGG_NO + "'";
                strSql += CRLF + "      , ORDER_CHECK_RD    =  '1'";
                //                strSql += CRLF + "      , PROD_CHECK_RD     =  '0'";
                strSql += CRLF + "      , OD_RQ_YN          =  'Y'                       ";
                strSql += CRLF + "      , OD_USER_ID        =  'IOTASK'                  ";
                strSql += CRLF + "      , OD_UPD_DT         =  " + DbLang.SYSDATE + "    ";
                #region 작업구분별 달라지는 QUERY 문 작성

                nBank_FK = Convert.ToInt32(strDEST_BANK_FK);

                nDoubleSide = (int)((nBank_FK - 1) / 2) + 1;
                //랙투랙 도착지 안떠서 변경함 240927LJM
                //switch (strJobType)
                //{
                //    case "1":
                //    case "10":
                // 구하는 함수 만들기 귀찮아~~~~ 하드코딩으로 ...ㅋㅋㅋ
                if (strDEST_BANK_FK == "01" || strDEST_BANK_FK == "04" || strDEST_BANK_FK == "05" || strDEST_BANK_FK == "08")
                {
                    strDEEP_CELL = "1";
                }

                strSql += CRLF + "      , DEST_BANK         =  '" + nDoubleSide.ToString() + "'";
                strSql += CRLF + "      , DEST_BAY          =  '" + strDEST_BAY_FK + "'";
                strSql += CRLF + "      , DEST_LEVEL        =  '" + strDEST_LEV_FK + "'";
                strSql += CRLF + "      , DEST_DEEP_CELL    =  '" + strDEEP_CELL + "'";
                strSql += CRLF + "      , START_HSPOS       =  '" + strSOUR_HSPOS_FK + "'";
                //    break;
                //case "2":
                //case "3":
                //case "4":
                //case "5":
                nBank_FK = Convert.ToInt32(strSOUR_BANK_FK);

                nDoubleSide = (int)((nBank_FK - 1) / 2) + 1;
                strDEEP_CELL = "0";

                //    // 구하는 함수 만들기 귀찮아~~~~ 하드코딩으로 ...ㅋㅋㅋ
                if (strSOUR_BANK_FK == "01" || strSOUR_BANK_FK == "04" || strSOUR_BANK_FK == "05" || strSOUR_BANK_FK == "08")
                {
                    strDEEP_CELL = "1";
                }

                strSql += CRLF + "      , START_BANK        =  '" + nDoubleSide.ToString() + "'";
                strSql += CRLF + "      , START_BAY         =  '" + strSOUR_BAY_FK + "'";
                strSql += CRLF + "      , START_LEVEL       =  '" + strSOUR_LEV_FK + "'";
                strSql += CRLF + "      , START_DEEP_CELL   =  '" + strDEEP_CELL + "'";
                strSql += CRLF + "      , DEST_HSPOS        =  '" + strDEST_HSPOS_FK + "'";
                //        break;
                //}
                #endregion
                strSql += CRLF + "  WHERE WH_TYP              =  '" + strWH_TYP + "'";
                strSql += CRLF + "    AND SC_NO               =  '" + strSC_NO + "'";
                strSql += CRLF + "    AND OD_RQ_YN            = 'N'                 ";
                if (bReLocation == true)
                {
                    strSql += CRLF + "    AND ERROR_CODE_RD    IN('0060', '0061', '0062', '0063', '0064')";
                }
                else
                {
                    strSql += CRLF + "    AND ERROR_CODE_RD       = '0000'              ";
                }
                #endregion

                nSelCnt = _pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG += "작업처리실패.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }

        // strIF_STATUS => 처리 상태(N:미처리, Y:정상, E:에러)
        // strERRCODE => 에러코드(N:미처리, Y:정상, E:에러)
        // strMSG_TYP => 메세지 종류(N:공파레트 입고 요청, L:P-Box 입고 요청)
        // 
        public bool UPDATE_IF_REQ_MST(string strWH_TYP
                                    , string strMSG_TYP
                                    , string strJOB_TYP
                                    , string strSTN_NO
                                , ref string pRTN_MSG
                                    , string strLUGG_NO1 = "0"
                                    , string strLUGG_NO2 = "0"
                                    , string strIF_STATUS = "N")
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string pMSG = "[UPDATE_IF_REQ_MST]";
                string strArg1 = "";
                string strArg2 = "";

                if (strMSG_TYP != "L" && strMSG_TYP != "N")
                {
                    pRTN_MSG = pMSG + "정의 되지 않은 메세지 타입을 송신하려 하였습니다.  [MSG_TYP : " + strMSG_TYP + "]";
                    return false;
                }
                else
                {
                    if (strMSG_TYP == "L")
                    {
                        if (strLUGG_NO1 == "" || strLUGG_NO1 == "0" || strLUGG_NO1 == "00" || strLUGG_NO1 == "000" || strLUGG_NO1 == "0000")
                        {
                            pRTN_MSG = pMSG + "P-BOX 입고 요청시 작업번호1 없이 입고 요청 할수는 없습니다.  [LUGG_NO1 : " + strLUGG_NO1 + "]";
                            return false;
                        }
                        strArg1 = strLUGG_NO1;
                        strArg2 = strLUGG_NO2;
                    }
                    else if (strMSG_TYP == "N")
                    {
                        if (strSTN_NO == "" || strSTN_NO == "0" || strSTN_NO == "00" || strSTN_NO == "000")
                        {
                            pRTN_MSG = pMSG + "Pallet Magazine 입고 요청시 Station No 없이 입고 요청 할수는 없습니다.  [STN_NO : " + strSTN_NO + "]";
                            return false;
                        }
                        strArg1 = strSTN_NO;
                    }
                }


                strSql = "";
                strSql += CRLF + " INSERT INTO IF_REQ_MST_HIS                           ";
                strSql += CRLF + "           ( CRT_DATE                                 ";      // 이거 내용
                strSql += CRLF + "           , CRT_TIME                                 ";
                strSql += CRLF + "           , MSG_TYP                                  ";
                strSql += CRLF + "           , LUGG_NO1                                 ";
                strSql += CRLF + "           , LUGG_NO2                                 ";
                strSql += CRLF + "           , JOB_KIND                                 ";
                strSql += CRLF + "           , STN_NO                                   ";
                strSql += CRLF + "           , IF_STATUS                                ";
                strSql += CRLF + "           , UPD_DT                                   ";
                strSql += CRLF + "           , UPD_USER_ID                              ";
                strSql += CRLF + "           , WH_TYP )                                 ";
                strSql += CRLF + " SELECT TO_CHAR(" + DbLang.SYSDATE + ", 'YYYYMMDD')   ";
                strSql += CRLF + "      , TO_CHAR(" + DbLang.SYSDATE + ", 'HH24MISS')   ";
                strSql += CRLF + "      , MSG_TYP, LUGG_NO1, LUGG_NO2, JOB_KIND, STN_NO ";
                strSql += CRLF + "      , IF_STATUS, " + DbLang.SYSDATE + ", 'IO_TASK'  ";
                strSql += CRLF + "      , WH_TYP                                        ";
                strSql += CRLF + "   FROM IF_REQ_MST                                    ";
                strSql += CRLF + "  WHERE WH_TYP = :WH_TYP                              ";
                if (strMSG_TYP == "L")
                {
                    strSql += CRLF + "   AND LUGG_NO1    = '" + strLUGG_NO1 + "'        ";       
                    strSql += CRLF + "   AND LUGG_NO2    = '" + strLUGG_NO2 + "'        ";       
                }
                else if(strMSG_TYP == "N")
                {
                    strSql += CRLF + "   AND STN_NO      = '" + strSTN_NO + "'          ";
                }

                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG = pMSG + _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    if (strMSG_TYP == "N")
                    {
                        pRTN_MSG = pMSG + "인터페이스 히스토리를 추가 할 수 없습니다. [STN_NO : " + strSTN_NO + "]";
                    }
                    else if  (strMSG_TYP == "L")
                    {
                        pRTN_MSG = pMSG + "인터페이스 히스토리를 추가 할 수 없습니다. [LUGG_NO1 : " + strLUGG_NO1 + "][LUGG_NO2 : " + strLUGG_NO2 + "]";
                    }
                    else 
                    {
                        pRTN_MSG = pMSG + "인터페이스 히스토리를 추가 할 수 없습니다. [MSG_TYP : " + strMSG_TYP + "]";
                    }
                    return true;
                }

                // 상위 보고        
                strSql = "";
                strSql += CRLF + "  UPDATE IF_REQ_MST                           ";
                strSql += CRLF + "     SET IF_STATUS   = :IF_STATUS             ";      // IF_STATUS => 처리 상태 (N:미처리, Y:정상, E:에러)
                strSql += CRLF + "       , UPD_USER_ID = '3F_IO_TASK'           ";
                strSql += CRLF + "       , UPD_DT      = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "   WHERE WH_TYP = :WH_TYP                     ";
                if (strMSG_TYP == "L")
                {
                    strSql += CRLF + "   AND LUGG_NO1    = '" + strLUGG_NO1 + "'";
                    strSql += CRLF + "   AND LUGG_NO2    = '" + strLUGG_NO2 + "'";
                }
                else if (strMSG_TYP == "N")
                {
                    strSql += CRLF + "   AND STN_NO      = '" + strSTN_NO + "'  ";
                }
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;              //???
                _pBdb.mComMain.Parameters.Add("IF_STATUS", DbLang.VARCHAR).Value = strIF_STATUS;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG = pMSG + _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    //pRTN_MSG = pMSG + "변경할 IF_REQ_MST 정보가 존재하지 않습니다.[ LUGG_NO : " + strLUGG_NO + "][WORK_STA : " + strJOB_STATUS + "]";
                    if (strMSG_TYP == "N")
                    {
                        pRTN_MSG = pMSG + "변경할 IF_REQ_MST 정보가 존재하지 않습니다. [STN_NO : " + strSTN_NO + "]";
                    }
                    else if (strMSG_TYP == "L")
                    {
                        pRTN_MSG = pMSG + "변경할 IF_REQ_MST 정보가 존재하지 않습니다. [LUGG_NO1 : " + strLUGG_NO1 + "][LUGG_NO2 : " + strLUGG_NO2 + "]";
                    }
                    else
                    {
                        pRTN_MSG = pMSG + "변경할 IF_REQ_MST 정보가 존재하지 않습니다. [MSG_TYP : " + strMSG_TYP + "]";
                    }
                    return false;
                }

                //pRTN_MSG = pMSG + "정상적으로 IF_REQ_MST_HIS 에 백업하고, IF_REQ_MST 정보를 변경하였습니다. [LUGG_NO : " + strLUGG_NO + "][WORK_STA : " + strJOB_STATUS + "]";
                if (strMSG_TYP == "N")
                {
                    pRTN_MSG = pMSG + "정상적으로 IF_REQ_MST_HIS 에 백업하고, IF_REQ_MST 정보를 변경하였습니다.  [STN_NO : " + strSTN_NO + "]";
                }
                else if (strMSG_TYP == "L")
                {
                    pRTN_MSG = pMSG + "정상적으로 IF_REQ_MST_HIS 에 백업하고, IF_REQ_MST 정보를 변경하였습니다.  [LUGG_NO1 : " + strLUGG_NO1 + "][LUGG_NO2 : " + strLUGG_NO2 + "]";
                }
                else
                {
                    pRTN_MSG = pMSG + "정상적으로 IF_REQ_MST_HIS 에 백업하고, IF_REQ_MST 정보를 변경하였습니다.  [MSG_TYP : " + strMSG_TYP + "]";
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }
        public bool UPDATE_IF_LUGG_STA(string strWH_TYP
                                      , string strLUGG_NO
                                      , string strJOB_STATUS
                                  , ref string pRTN_MSG
                                      , string strIF_STATUS = "N"
                                      , string strERRCODE = "00"
                                      , string strPRIORITY = "100"
                                      , string strTO_AREA = "")
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string pMSG = "[UPDATE_IF_LUGG_STA]";

                strSql = "";
                strSql += CRLF + " INSERT INTO IF_LUGG_STA_HIS  ";
                strSql += CRLF + "           ( CRT_DATE         ";
                strSql += CRLF + "           , CRT_TIME         ";
                strSql += CRLF + "           , LUGGNO           ";
                strSql += CRLF + "           , JOB_KIND         ";
                strSql += CRLF + "           , LD_CTN_NO        ";
                strSql += CRLF + "           , FROM_CV_NO       ";
                strSql += CRLF + "           , TO_CV_NO         ";
                strSql += CRLF + "           , FROM_SC_NO       ";
                strSql += CRLF + "           , TO_SC_NO         ";
                strSql += CRLF + "           , FROM_AREA        ";
                strSql += CRLF + "           , TO_AREA          ";
                strSql += CRLF + "           , WORK_STA         ";
                strSql += CRLF + "           , ST_ISHIGH        ";
                strSql += CRLF + "           , ERRCODE          ";
                strSql += CRLF + "           , PRIORITY         ";
                strSql += CRLF + "           , IF_STATUS        ";
                strSql += CRLF + "           , UPD_DT           ";
                strSql += CRLF + "           , UPD_USER_ID      ";
                strSql += CRLF + "           , PRDCT_NM        ";
                strSql += CRLF + "           , WH_TYP)        ";
                strSql += CRLF + " SELECT TO_CHAR(" + DbLang.SYSDATE + ", 'YYYYMMDD')                       ";
                strSql += CRLF + "      , TO_CHAR(" + DbLang.SYSDATE + ", 'HH24MISS')                       ";
                strSql += CRLF + "      , LUGGNO, JOB_KIND, LD_CTN_NO                                       ";
                strSql += CRLF + "      , FROM_CV_NO, TO_CV_NO, FROM_SC_NO, TO_SC_NO, FROM_AREA, TO_AREA    ";
                strSql += CRLF + "      , WORK_STA, ST_ISHIGH, ERRCODE, PRIORITY, IF_STATUS                 ";
                strSql += CRLF + "      , " + DbLang.SYSDATE + ", 'IO_TASK', PRDCT_NM, WH_TYP               ";
                strSql += CRLF + "   FROM IF_LUGG_STA                                                       ";
                strSql += CRLF + "  WHERE LUGGNO      = '" + strLUGG_NO + "'                                ";

                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;

                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG = pMSG + _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = pMSG + "인터페이스 히스토리를 추가 할 수 없습니다. [LUGG_NO : " + strLUGG_NO + "]";
                    return true;
                }

                // 상위 보고        
                strSql = "";
                strSql += CRLF + "  UPDATE IF_LUGG_STA                          ";
                strSql += CRLF + "     SET IF_STATUS   = :IF_STATUS             ";      // IF_STATUS => 처리 상태 (N:미처리, Y:정상, E:에러)
                strSql += CRLF + "       , WORK_STA    = :JOB_STATUS            ";
                strSql += CRLF + "       , WH_TYP      = :WH_TYP                ";
                //if (strERRCODE != "00")
                strSql += CRLF + "   , ERRCODE     = :ERRCODE                   ";
                if (strPRIORITY != "100")
                    strSql += CRLF + "   , PRIORITY    = :PRIORITY              ";
                if (strTO_AREA != "")
                    strSql += CRLF + "   , TO_AREA     = :TO_AREA               ";
                strSql += CRLF + "       , UPD_USER_ID = 'WCS'                  ";
                strSql += CRLF + "       , UPD_DT      = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "   WHERE LUGGNO      = :LUGG_NO               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = strJOB_STATUS;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                if (strPRIORITY != "100")
                    _pBdb.mComMain.Parameters.Add("PRIORITY", DbLang.VARCHAR).Value = strPRIORITY;
                if (strTO_AREA != "")
                    _pBdb.mComMain.Parameters.Add("TO_AREA", DbLang.VARCHAR).Value = strTO_AREA;
                _pBdb.mComMain.Parameters.Add("IF_STATUS", DbLang.VARCHAR).Value = strIF_STATUS;
                _pBdb.mComMain.Parameters.Add("ERRCODE", DbLang.VARCHAR).Value = strERRCODE;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG = pMSG + _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = pMSG + "변경할 IF_LUGG_STA 정보가 존재하지 않습니다.[ LUGG_NO : " + strLUGG_NO + "][WORK_STA : " + strJOB_STATUS + "]";
                    return false;
                }

                //pRTN_MSG += "정상적으로 IF_LUGG_STA_HIS 에 백업하고, IF_LUGG_STA 정보를 변경하였습니다. [LUGG_NO : " + strLUGG_NO + "][WORK_STA : " + strJOB_STATUS + "]";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }
        public bool IsValidID(string strStationID)
        {
            if (strStationID.Length == 1 || strStationID.Length == 3)
                return true;

            return false;
        }

        public bool IsValidLocation(string strWH_TYP
                                    , string strLocation
                                , ref string pRTN_MSG
                                    , bool bIsStartStn = true)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string strSC_NO = "";
                string strBANK = strLocation.Substring(0, 2);
                string strBAY = strLocation.Substring(2, 3);
                string strLEVEL = strLocation.Substring(5, 2);
                string strCELL_NO = strBANK + "-" + strBAY + "-" + strLEVEL;

                if (strBANK == "01" || strBANK == "02" || strBANK == "03" || strBANK == "04")
                {
                    strSC_NO = "901";
                }
                else if (strBANK == "05" || strBANK == "06" || strBANK == "07" || strBANK == "08")
                {
                    strSC_NO = "902";
                }
                else
                {
                    pRTN_MSG += "잘못된 Location 입니다 - SC 번호를 구할수 없습니다. ";
                    return false;
                }

                strSql = "";
                strSql += CRLF + " SELECT *                            ";
                strSql += CRLF + "   FROM CELL_MST                     ";
                strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP      ";
                strSql += CRLF + "    AND CELL_SC_NO    = :SC_NO       ";
                strSql += CRLF + "    AND CELL_NO       = :CELL_NO     ";
                strSql += CRLF + "    AND BANK          = :BANK        ";
                strSql += CRLF + "    AND BAY           = :BAY         ";
                strSql += CRLF + "    AND LEV           = :LEVEL       ";
                strSql += CRLF + "    AND SC_NO         = :SC_NO       ";
                strSql += CRLF + "    AND CELL_USE_YN   = 'Y'          ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO", DbLang.VARCHAR).Value = strSC_NO;
                _pBdb.mComMain.Parameters.Add("CELL_NO", DbLang.VARCHAR).Value = strCELL_NO;
                _pBdb.mComMain.Parameters.Add("BANK", DbLang.VARCHAR).Value = strBANK;
                _pBdb.mComMain.Parameters.Add("BAY", DbLang.VARCHAR).Value = strBAY;
                _pBdb.mComMain.Parameters.Add("LEVEL", DbLang.VARCHAR).Value = strLEVEL;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }
        public string CheckLocation7To9(string strLocation)
        {
            if (strLocation.Length != 7)
                return "00-000-00";

            return strLocation.Substring(0, 2) + "-" + strLocation.Substring(2, 3) + "-" + strLocation.Substring(5, 2);
        }

        public string CheckLocation9To7(string strLocation)
        {
            if (strLocation.Length != 9)
                return "0000000";

            return strLocation.Substring(0, 2) + strLocation.Substring(3, 3) + strLocation.Substring(7, 2);
        }
        public EN_JOB_PATTERN ConvertJobTypeToPattern(string strJobTyp)
        {
            int nJobType = Convert.ToInt32(strJobTyp);
            switch (nJobType)
            {
                case (int)EN_JOB_TYPE.enJobTypeAutoSto:
                case (int)EN_JOB_TYPE.enJobTypeSemiSto: return EN_JOB_PATTERN.enJobPatternSto;
                case (int)EN_JOB_TYPE.enJobTypeAutoRet:
                case (int)EN_JOB_TYPE.enJobTypeSemiRet: return EN_JOB_PATTERN.enJobPatternRet;
                case (int)EN_JOB_TYPE.enJobTypeAutoPR:
                case (int)EN_JOB_TYPE.enJobTypeSemiPR: return EN_JOB_PATTERN.enJobPatternPR;
                case (int)EN_JOB_TYPE.enJobTypeAutoR2R:
                case (int)EN_JOB_TYPE.enJobTypeSemiR2R: return EN_JOB_PATTERN.enJobPatternR2R;
                case (int)EN_JOB_TYPE.enJobTypeAutoW2W:
                case (int)EN_JOB_TYPE.enJobTypeSemiW2W: return EN_JOB_PATTERN.enJobPatternW2W;
                case (int)EN_JOB_TYPE.enJobTypeAutoMove:
                case (int)EN_JOB_TYPE.enJobTypeSemiMove: return EN_JOB_PATTERN.enJobPatternMove;
            }

            return EN_JOB_PATTERN.enJobPatternNone;
        }
        public bool IsValidStartLocation(string strWH_TYP, string strLocation, string strJobTyp, ref string pRTN_MSG)
        {
            EN_JOB_PATTERN enPattern = ConvertJobTypeToPattern(strJobTyp);
            switch (enPattern)
            {
                case EN_JOB_PATTERN.enJobPatternSto:
                case EN_JOB_PATTERN.enJobPatternMove:
                    return true;
            }

            return IsValidLocation(strWH_TYP, strLocation, ref pRTN_MSG);
        }
        public bool IsValidDestLocation(string strWH_TYP, string strLocation, string strJobTyp, ref string pRTN_MSG)
        {
            EN_JOB_PATTERN enPattern = ConvertJobTypeToPattern(strJobTyp);
            switch (enPattern)
            {
                case EN_JOB_PATTERN.enJobPatternRet:
                case EN_JOB_PATTERN.enJobPatternPR:
                case EN_JOB_PATTERN.enJobPatternMove:
                    return true;
            }

            return IsValidLocation(strWH_TYP, strLocation, ref pRTN_MSG, false);
        }
        //IsValidStation
        //IsValidStartStation
        public bool IsValidStartStation(DataTable dtCV_DATA, int nSelCnt, EN_JOB_PATTERN enPattern, string strStationID, string strJobTyp, ref string pRTN_MSG)
        {
            string strSTN_KIND = "";
            string strMC_NO = "";

            bool bScStation = false;
            if (strStationID == "901" || strStationID == "902")
                bScStation = true;

            for (int i = 0; i < nSelCnt; i++)
            {
                strSTN_KIND = dtCV_DATA.Rows[i]["STN_KIND"].ToString() == "" ? "0" : dtCV_DATA.Rows[i]["STN_KIND"].ToString();
                strMC_NO = dtCV_DATA.Rows[i]["MC_NO"].ToString() == "" ? "0" : dtCV_DATA.Rows[i]["MC_NO"].ToString();

                //int nStnKind = Convert.ToInt32(strSTN_KIND);

                byte byteTemp = Convert.ToByte(Convert.ToInt16(strSTN_KIND));
                bool bStoStation = Convert.ToBoolean(byteTemp & 0x01);       // 입고대
                bool bRetStation = Convert.ToBoolean(byteTemp & 0x02);       // 출고대
                bool bArvStation = Convert.ToBoolean(byteTemp & 0x03);       // 도착대


                switch (enPattern)
                {
                    case EN_JOB_PATTERN.enJobPatternSto:
                    case EN_JOB_PATTERN.enJobPatternMove:
                        if (!bStoStation && !bArvStation)
                            continue;
                        break;

                    case EN_JOB_PATTERN.enJobPatternRet:
                    case EN_JOB_PATTERN.enJobPatternPR:
                    case EN_JOB_PATTERN.enJobPatternR2R:
                    case EN_JOB_PATTERN.enJobPatternW2W:
                        if (!bScStation)
                            continue;
                        break;
                    default:
                        pRTN_MSG += "잘못된 작업 패턴입니다.[작업대:" + strSTN_KIND + "][작업구분:" + strJobTyp + "][작업패턴:" + enPattern.ToString() + "]";
                        continue;
                }

                // 트랙번호와 Station 번호가 같기 때문에 이렇게 체크해도됨
                if (strMC_NO == strStationID)
                    return true;
            }

            if (strStationID == "901" || strStationID == "902")
                return true;

            return false;
        }
        public bool IsValidDestStation(DataTable dtCV_DATA, int nSelCnt, EN_JOB_PATTERN enPattern, string strStationID, string strJobTyp, ref string pRTN_MSG)
        {
            string strSTN_KIND = "";
            string strMC_NO = "";

            bool bScStation = false;
            if (strStationID == "901" || strStationID == "902")
                bScStation = true;

            for (int i = 0; i < nSelCnt; i++)
            {
                strSTN_KIND = dtCV_DATA.Rows[i]["STN_KIND"].ToString() == "" ? "0" : dtCV_DATA.Rows[i]["STN_KIND"].ToString();
                strMC_NO = dtCV_DATA.Rows[i]["MC_NO"].ToString() == "" ? "0" : dtCV_DATA.Rows[i]["MC_NO"].ToString();

                //int nStnKind = Convert.ToInt32(strSTN_KIND);

                byte byteTemp = Convert.ToByte(Convert.ToInt16(strSTN_KIND));
                bool bStoStation = Convert.ToBoolean(byteTemp & 0x01);       // 입고대
                bool bRetStation = Convert.ToBoolean(byteTemp & 0x02);       // 출고대
                bool bArvStation = Convert.ToBoolean(byteTemp & 0x03);       // 도착대


                switch (enPattern)
                {
                    case EN_JOB_PATTERN.enJobPatternSto:
                    case EN_JOB_PATTERN.enJobPatternR2R:
                    case EN_JOB_PATTERN.enJobPatternW2W:
                        if (!bScStation)
                            continue;
                        break;

                    case EN_JOB_PATTERN.enJobPatternRet:
                    case EN_JOB_PATTERN.enJobPatternPR:
                        if (!bRetStation)
                            continue;
                        break;
                    case EN_JOB_PATTERN.enJobPatternMove:
                        if (!bRetStation && !bArvStation)
                            continue;
                        break;
                    default:
                        pRTN_MSG += "잘못된 작업 패턴입니다.[작업대:" + strSTN_KIND + "][작업구분:" + strJobTyp + "][작업패턴:" + enPattern.ToString() + "]";
                        continue;
                }

                // 트랙번호와 Station 번호가 같기 때문에 이렇게 체크해도됨
                if (strMC_NO == strStationID)
                    return true;
            }

            if (strStationID == "901" || strStationID == "902")
                return true;

            return false;

        }
        public bool IsValidStation(string strWH_TYP, string strStationID, string strJobTyp, ref string pRTN_MSG, bool bStartStn = true)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                DataTable dtCV_DATA = new DataTable();

                strSql = "";
                strSql += CRLF + " SELECT STN_KIND, MC_NO              ";
                strSql += CRLF + "   FROM CV_DATA                      ";
                strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP      ";
                strSql += CRLF + "    AND STN_KIND      <> '0'         ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

                //nSelCnt = _pBdb.ExcuteQry(strSql);
                nSelCnt = _pBdb.ExcuteQry(dtCV_DATA, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    return false;
                }

                EN_JOB_PATTERN enPattern = ConvertJobTypeToPattern(strJobTyp);

                if (bStartStn)
                    return IsValidStartStation(dtCV_DATA, nSelCnt, enPattern, strStationID, strJobTyp, ref pRTN_MSG);
                else
                    return IsValidDestStation(dtCV_DATA, nSelCnt, enPattern, strStationID, strJobTyp, ref pRTN_MSG);
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }


        }

        public bool CheckDuplicatedLuggNum(string strWH_TYP
                                         , string strLUGG_NO
                                     , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                // RTV UNLOAD 완료된 작업정보
                strSql = "";
                strSql += CRLF + " SELECT *                            ";
                strSql += CRLF + "   FROM JOB_MST                      ";
                strSql += CRLF + "  WHERE WH_TYP        = :WH_TYP      ";
                strSql += CRLF + "    AND LUGG_NO       = :LUGG_NO       ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }

        // 작업번호가 다른 동일한 작업 존재 
        public bool CheckAlreadyJobContents(string strWH_TYP
                                          , string strJOB_TYP
                                          , string strBCR_BOTTOM
                                          , string strSTART_POS
                                          , string strSTART_LOCATION
                                          , string strDEST_POS
                                          , string strDEST_LOCATION
                                          , string strPRODUCT_ID
                                      , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                // RTV UNLOAD 완료된 작업정보
                strSql = "";
                strSql += CRLF + " SELECT *                                 ";
                strSql += CRLF + "   FROM JOB_MST                           ";
                strSql += CRLF + "  WHERE WH_TYP         = :WH_TYP          ";
                strSql += CRLF + "    AND JOB_TYP        = :JOB_TYP         ";
                strSql += CRLF + "    AND BCR_BOTTOM     = :BCR_BOTTOM      ";
                strSql += CRLF + "    AND START_POS      = :START_POS       ";
                strSql += CRLF + "    AND START_LOCATION = :START_LOCATION  ";
                strSql += CRLF + "    AND DEST_POS       = :DEST_POS        ";
                strSql += CRLF + "    AND DEST_LOCATION  = :DEST_LOCATION   ";
                strSql += CRLF + "    AND PRODUCT_ID     = :PRODUCT_ID      ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("JOB_TYP", DbLang.VARCHAR).Value = strJOB_TYP;
                _pBdb.mComMain.Parameters.Add("BCR_BOTTOM", DbLang.VARCHAR).Value = strBCR_BOTTOM;
                _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strSTART_POS;
                _pBdb.mComMain.Parameters.Add("START_LOCATION", DbLang.VARCHAR).Value = strSTART_LOCATION;
                _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDEST_POS;
                _pBdb.mComMain.Parameters.Add("DEST_LOCATION", DbLang.VARCHAR).Value = strDEST_LOCATION;
                _pBdb.mComMain.Parameters.Add("PRODUCT_ID", DbLang.VARCHAR).Value = strPRODUCT_ID;

                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }

        // 작업번호가 다른 동일한 작업 존재 
        public bool CheckReservedJobByStartStation(string strWH_TYP
                                                  , string strSTART_POS
                                              , ref string strOLD_LUGG
                                              , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                DataTable dtJOB_MST = new DataTable();

                // RTV UNLOAD 완료된 작업정보
                strSql = "";
                strSql += CRLF + " SELECT *                                 ";
                strSql += CRLF + "   FROM JOB_MST                           ";
                strSql += CRLF + "  WHERE WH_TYP         = :WH_TYP          ";
                strSql += CRLF + "    AND START_POS      = :START_POS       ";
                strSql += CRLF + "    AND JOB_TYP        IN ('1', '6')      ";
                strSql += CRLF + "    AND JOB_STATUS     IN ('99','10')     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strSTART_POS;
                nSelCnt = _pBdb.ExcuteQry(dtJOB_MST, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    //_pBdb.mDtMain.Dispose();
                    //pRTN_MSG = "";
                    return false;
                }

                strOLD_LUGG = dtJOB_MST.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : dtJOB_MST.Rows[0]["LUGG_NO"].ToString();

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                return false;
            }
        }


        public bool InsertJobMst(string strWH_TYP
                                , string strLuggNo
                                , string strStartPos
                                , string strStartLoc
                                , string strDestPos
                                , string strDestLoc
                                , string strJobDefine
                                , string strJobStatus
                            , ref string pRTN_MSG
                                , string strPalletNo = ""
                                , string strProdutID = ""
                                , string strHsTrackNo = ""
                                , string strPriority = "000")
        {
            string strSql = "";

            if (CheckDuplicatedLuggNum(strWH_TYP, strLuggNo, ref pRTN_MSG))
            {
                pRTN_MSG += "[CheckDuplicatedLuggNum] 이미 존재하는 작업이므로 작업을 추가할 수 없습니다. ";
                return false;
            }

            #region 작업 생성
            strSql += cDefApp.CRLF + "INSERT INTO JOB_MST   ";
            strSql += cDefApp.CRLF + "(  WH_TYP			    ";          //    01
            strSql += cDefApp.CRLF + " , LUGG_NO			";          //    02 
            strSql += cDefApp.CRLF + " , START_POS			";          //    03
            strSql += cDefApp.CRLF + " , START_LOCATION    	";          //    04
            strSql += cDefApp.CRLF + " , DEST_POS          	";          //    05
            strSql += cDefApp.CRLF + " , DEST_LOCATION     	";          //    06
            strSql += cDefApp.CRLF + " , JOB_TYP		    ";          //    07
            strSql += cDefApp.CRLF + " , JOB_STATUS        	";          //    08
            strSql += cDefApp.CRLF + " , JOB_PRIORITY     	";          //    09
            strSql += cDefApp.CRLF + " , BCR_BOTTOM	    	";          //    10
            strSql += cDefApp.CRLF + " , PRODUCT_ID     	";          //    11
            strSql += cDefApp.CRLF + " , HS_TRACK_NO     	";          //    11
            strSql += cDefApp.CRLF + " , INS_DT		    	";          //    12
            strSql += cDefApp.CRLF + " , INS_USER_ID       	";          //    13
            strSql += cDefApp.CRLF + " , REMARKS )         	";          //    14
            strSql += cDefApp.CRLF + "VALUES (:WH_TYP           ";      //    01
            strSql += cDefApp.CRLF + "     ,  :LUGG_NO		    ";	    //    02 
            strSql += cDefApp.CRLF + "     ,  :START_POS		";	    //    03
            strSql += cDefApp.CRLF + "     ,  :START_LOCATION   ";   	//    04
            strSql += cDefApp.CRLF + "     ,  :DEST_POS         ";   	//    05
            strSql += cDefApp.CRLF + "     ,  :DEST_LOCATION    ";   	//    06
            strSql += cDefApp.CRLF + "     ,  :JOB_TYP		    ";      //    07
            strSql += cDefApp.CRLF + "     ,  :JOB_STATUS       ";   	//    08
            strSql += cDefApp.CRLF + "     ,  :JOB_PRIORITY     ";  	//    09
            strSql += cDefApp.CRLF + "     ,  :BCR_BOTTOM	    ";	    //    10
            strSql += cDefApp.CRLF + "     ,  :PRODUCT_ID       ";	    //    11
            strSql += cDefApp.CRLF + "     ,  :HS_TRACK_NO       ";	    //    11
            strSql += cDefApp.CRLF + "     ,   " + DbLang.SYSDATE;      //    12
            strSql += cDefApp.CRLF + "     ,  'IO_TASK'";               //    13
            strSql += cDefApp.CRLF + "     ,  '');";                    //    14

            _pBdb.mComMain.Parameters.Clear();
            _pBdb.mComMain.CommandType = CommandType.Text;
            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
            _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLuggNo;
            _pBdb.mComMain.Parameters.Add("START_POS", DbLang.VARCHAR).Value = strStartPos;
            _pBdb.mComMain.Parameters.Add("START_LOCATION", DbLang.VARCHAR).Value = strStartLoc;
            _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDestPos;
            _pBdb.mComMain.Parameters.Add("DEST_LOCATION", DbLang.VARCHAR).Value = strDestLoc;
            _pBdb.mComMain.Parameters.Add("JOB_TYP", DbLang.VARCHAR).Value = strJobDefine;
            _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = strJobStatus;
            _pBdb.mComMain.Parameters.Add("JOB_PRIORITY", DbLang.VARCHAR).Value = strPriority;
            _pBdb.mComMain.Parameters.Add("BCR_BOTTOM", DbLang.VARCHAR).Value = strPalletNo;
            _pBdb.mComMain.Parameters.Add("PRODUCT_ID", DbLang.VARCHAR).Value = strProdutID;
            _pBdb.mComMain.Parameters.Add("HS_TRACK_NO", DbLang.VARCHAR).Value = strHsTrackNo;

            int iSelCnt = _pBdb.ExcuteNonQry(strSql);

            if (iSelCnt < 0)
            {
                pRTN_MSG += _pBdb.ErrMsg;
                return false;
            }
            if (iSelCnt == 0)
            {
                pRTN_MSG += string.Format("작업 생성에 실패하였습니다. [작업번호:{0}][출발지:{1}][출발LOC:{2}][도착지:{3}][도착LOC:{4}][PLT_NO:{5}][PRODUCTID:{6}]",
                    strLuggNo, strStartPos, strStartLoc, strDestPos, strDestLoc, strPalletNo, strProdutID);
                return false;
            }

            #endregion
            return true;
        }

        //LSJ 추가 포지션
        public bool DELETE_JOB_DATA(string strLUGG_NO,
                            string strWH_TYP,
                        ref string strRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                IsRtnMsg += "[DELETE_JOB_DATA]";

                #region DELETE 할때 JOB_MST_HIS에 추가하는부분 제거
                strSql = "";
                strSql += CRLF + " INSERT INTO JOB_MST_HIS      ";
                strSql += CRLF + "           ( INS_DATE         ";
                strSql += CRLF + "           , INS_TIME         ";
                strSql += CRLF + "           , WH_TYP           ";
                strSql += CRLF + "           , LUGG_NO          ";
                strSql += CRLF + "           , START_POS        ";
                strSql += CRLF + "           , START_LOCATION   ";
                strSql += CRLF + "           , DEST_POS         ";
                strSql += CRLF + "           , DEST_LOCATION    ";
                strSql += CRLF + "           , TURN             ";
                strSql += CRLF + "           , PRODUCT_ID       ";
                strSql += CRLF + "           , PRODUCT_SIZE     ";
                strSql += CRLF + "           , JOB_TYP          ";
                strSql += CRLF + "           , BCR_TOP          ";
                strSql += CRLF + "           , BCR_BOTTOM       ";
                strSql += CRLF + "           , MES_WRITE_YN     ";
                strSql += CRLF + "           , CMD_STA          ";
                strSql += CRLF + "           , JOB_KIND         ";
                strSql += CRLF + "           , JOB_STATUS       ";
                strSql += CRLF + "           , JOB_PRIORITY     ";
                strSql += CRLF + "           , MES_ERROR_CD     ";
                strSql += CRLF + "           , OD_LAST_PAGE     ";
                strSql += CRLF + "           , OD_LAST_USER     ";
                strSql += CRLF + "           , JOB_START_DT     ";
                strSql += CRLF + "           , INS_DT           ";
                strSql += CRLF + "           , INS_USER_ID      ";
                strSql += CRLF + "           , REMARKS          ";
                strSql += CRLF + "           , TRAY_TYP         ";
                strSql += CRLF + "           , TRAY_LEV         ";
                strSql += CRLF + "           , HS_TRACK_NO      ";
                strSql += CRLF + "           , SC_NO            ";
                strSql += CRLF + "           , DURATION_TIME    ";
                strSql += CRLF + "           , SYSTEM_BYTE      ";
                strSql += CRLF + "           , S6F12_RESPONSE   ";
                strSql += CRLF + "           , S6F11_YON        ";
                strSql += CRLF + "           , CEID_NAME        ";
                strSql += CRLF + "           , FK_NO            ";
                strSql += CRLF + "           , AGING_TIME       ";
                strSql += CRLF + "           , PAIR_LUGG_NO )   ";
                strSql += CRLF + " SELECT TO_CHAR(" + DbLang.SYSDATE + ", 'YYYYMMDD'), TO_CHAR(" + DbLang.SYSDATE + ", 'HH24MISS')                            ";
                strSql += CRLF + "      , WH_TYP, LUGG_NO, START_POS, START_LOCATION, DEST_POS, DEST_LOCATION                   ";
                strSql += CRLF + "      , TURN, PRODUCT_ID, PRODUCT_SIZE, JOB_TYP, BCR_TOP, BCR_BOTTOM, MES_WRITE_YN            ";
                strSql += CRLF + "      , CMD_STA, JOB_KIND, JOB_STATUS, JOB_PRIORITY, MES_ERROR_CD, OD_LAST_PAGE, OD_LAST_USER ";
                strSql += CRLF + "      , JOB_START_DT, " + DbLang.SYSDATE + ", 'IO_TASK', REMARKS, TRAY_TYP, TRAY_LEV, HS_TRACK_NO, SC_NO    ";
                strSql += CRLF + "      , DURATION_TIME, SYSTEM_BYTE, S6F12_RESPONSE, S6F11_YON, CEID_NAME, FK_NO, AGING_TIME   ";
                strSql += CRLF + "      , PAIR_LUGG_NO                  ";
                strSql += CRLF + "   FROM JOB_MST                       ";
                strSql += CRLF + "  WHERE WH_TYP   = :WH_TYP       ";
                strSql += CRLF + "    AND LUGG_NO  = :LUGG_NO   ";

                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;

                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "작업 히스토리를 추가 할 수 없습니다. LUGG_NO : " + strLUGG_NO;
                    return false;
                }
                #endregion

                strSql = "";
                strSql += CRLF + " DELETE FROM JOB_MST               ";
                strSql += CRLF + "  WHERE WH_TYP   = :WH_TYP    ";
                strSql += CRLF + "    AND LUGG_NO  = :LUGG_NO   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    strRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    strRTN_MSG += "삭제할 작업삭정보가 존재하지 않습니다. LUGG_NO : " + strLUGG_NO;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                strRTN_MSG += ex.ToString();
                return false;
            }
        }


        // 설비 상태 UPDATE 확인(LSJ 코딩 부분)
        public bool CHECK_IF_MC_STA(string strWH_TYP
                                    , string strMC_NO
                                    , ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                pRTN_MSG = "[CHECK_IF_MC_STA]";

                DataTable dtIF_MC_STA = new DataTable();

                strSql = "";
                strSql += CRLF + " SELECT * ";
                strSql += CRLF + "   FROM IF_MC_STA ";
                strSql += CRLF + "  WHERE MC_TYP    = :MC_TYP    ";
                strSql += CRLF + "    AND MC_NO      = :MC_NO     ";
                strSql += CRLF + "    AND IF_STATUS    = 'N'      ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_NO;
                nSelCnt = _pBdb.ExcuteQry(dtIF_MC_STA, strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = "설비 상태가 UPDATE 되지 않았습니다.";
                    return true;
                }


                string strLOG = pRTN_MSG + " 설비 상태 확인 (CV : " + strMC_NO + ")";
                InsertLog(strWH_TYP, strLOG);
                return true;

            }
            catch (Exception ex)
            {
                _pBdb.Rollback();
                pRTN_MSG += ex.ToString();
                return false;
            }
        }

        public bool UPDATE_IF_MC_STA(string strMC_TYP
                                      , string strMC_NO
                                  , ref string pRTN_MSG
                                      , string strCV_STA = ""
                                      , string strCV_IO_MODE = ""
                                      , string strMC_STA = ""
                                      , string strSC_STA = ""
                                      , string strMC_USE_DEF = ""
                                      , string strST_ISHIGH = ""
                                      , string strBCR_DATA = ""
                                      , string strPA_REQ_STA = ""
                                      , bool bTrans = true)
        {
            try
            {
                if (bTrans == true)
                    _pBdb.BeginTrans();

                int nSelCnt = 0;
                string strSql = "";
                pRTN_MSG += "[UPDATE_IF_MC_STA]";

                //BCR의 경우 IF_MC_STA_HIS에 남기기
                if (strMC_TYP == "BCR")
                {
                    strSql = "";
                    strSql += CRLF + " INSERT INTO IF_MC_STA_HIS     ";
                    strSql += CRLF + "           ( MC_TYP            ";
                    strSql += CRLF + "           , MC_NO             ";
                    strSql += CRLF + "           , CV_STA            ";
                    strSql += CRLF + "           , CV_IO_MODE        ";
                    strSql += CRLF + "           , MC_STA            ";
                    strSql += CRLF + "           , SC_STA            ";
                    strSql += CRLF + "           , MC_USE_DEF        ";
                    strSql += CRLF + "           , ST_ISHIGH         ";
                    strSql += CRLF + "           , BCR_DATA          ";
                    strSql += CRLF + "           , PA_REQ_STA        ";
                    strSql += CRLF + "           , IF_STATUS         ";
                    strSql += CRLF + "           , UPD_DT      )     ";
                    strSql += CRLF + " SELECT MC_TYP, MC_NO, CV_STA  ";
                    strSql += CRLF + "      , CV_IO_MODE, MC_STA     ";
                    strSql += CRLF + "      , SC_STA, MC_USE_DEF     ";
                    strSql += CRLF + "      , ST_ISHIGH, BCR_DATA    ";
                    strSql += CRLF + "      , PA_REQ_STA, IF_STATUS  ";
                    strSql += CRLF + "      , " + DbLang.SYSDATE + " ";
                    strSql += CRLF + "   FROM IF_MC_STA              ";
                    strSql += CRLF + "  WHERE MC_TYP    = :MC_TYP    ";
                    strSql += CRLF + "    AND MC_NO     = :MC_NO     ";

                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Add("MC_TYP", DbLang.VARCHAR).Value = strMC_TYP;
                    _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_NO;

                    nSelCnt = _pBdb.ExcuteNonQry(strSql);
                    if (nSelCnt < 0)
                    {
                        if (bTrans == true)
                            _pBdb.Rollback();

                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }

                    if (nSelCnt == 0)
                    {
                        if (bTrans == true)
                            _pBdb.Rollback();

                        pRTN_MSG += "설비 상태 인터페이스 히스토리를 추가 할 수 없습니다. [MC_TYP : " + strMC_TYP + "][MC_NO:" + strMC_NO + "]";
                        return false;
                    }
                }
                // 상위 보고 
                strSql = "";
                strSql += CRLF + "      UPDATE IF_MC_STA                         ";
                strSql += CRLF + "         SET IF_STATUS    = 'N'                ";      // 처리 상태 (N:미처리, Y:정상, E:에러)

                if (strCV_STA != "")
                    strSql += CRLF + "       , CV_STA       = :CV_STA            ";
                if (strCV_IO_MODE != "")
                    strSql += CRLF + "       , CV_IO_MODE   = :CV_IO_MODE        ";
                if (strMC_STA != "")
                    strSql += CRLF + "       , MC_STA       = :MC_STA            ";
                if (strSC_STA != "")
                    strSql += CRLF + "       , SC_STA       = :SC_STA            ";
                if (strMC_USE_DEF != "")
                    strSql += CRLF + "       , MC_USE_DEF   = :MC_USE_DEF        ";
                if (strST_ISHIGH != "")
                    strSql += CRLF + "       , ST_ISHIGH    = :ST_ISHIGH         ";
                if (strBCR_DATA != "")
                    strSql += CRLF + "       , BCR_DATA     = :BCR_DATA          ";
                if (strPA_REQ_STA != "")
                    strSql += CRLF + "       , PA_REQ_STA   = :PA_REQ_STA        ";

                strSql += CRLF + "       , UPD_DT      = " + DbLang.SYSDATE + "  ";
                strSql += CRLF + "  WHERE MC_TYP    = :MC_TYP    ";
                strSql += CRLF + "    AND MC_NO     = :MC_NO     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("MC_TYP", DbLang.VARCHAR).Value = strMC_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_NO;
                _pBdb.mComMain.Parameters.Add("CV_STA", DbLang.VARCHAR).Value = strCV_STA;
                _pBdb.mComMain.Parameters.Add("CV_IO_MODE", DbLang.VARCHAR).Value = strCV_IO_MODE;
                _pBdb.mComMain.Parameters.Add("MC_STA", DbLang.VARCHAR).Value = strMC_STA;
                _pBdb.mComMain.Parameters.Add("SC_STA", DbLang.VARCHAR).Value = strSC_STA;
                _pBdb.mComMain.Parameters.Add("MC_USE_DEF", DbLang.VARCHAR).Value = strMC_USE_DEF;
                _pBdb.mComMain.Parameters.Add("ST_ISHIGH", DbLang.VARCHAR).Value = strST_ISHIGH;
                _pBdb.mComMain.Parameters.Add("BCR_DATA", DbLang.VARCHAR).Value = strBCR_DATA;
                _pBdb.mComMain.Parameters.Add("PA_REQ_STA", DbLang.VARCHAR).Value = strPA_REQ_STA;

                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    if (bTrans == true)
                        _pBdb.Rollback();

                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    if (bTrans == true)
                        _pBdb.Rollback();

                    pRTN_MSG += "변경할 IF_MC_STA 정보가 존재하지 않습니다. [MC_TYP : " + strMC_TYP + "][MC_NO:" + strMC_NO + "]";
                    return false;
                }

                if (bTrans == true)
                    _pBdb.Commit();

                pRTN_MSG += "정상적으로 IF_MC_STA_HIS 에 백업하고, IF_MC_STA 정보를 변경하였습니다. [MC_TYP : " + strMC_TYP + "][MC_NO:" + strMC_NO + "]";

                return true;
            }
            catch (Exception ex)
            {
                if (bTrans == true)
                    _pBdb.Rollback();

                pRTN_MSG += ex.ToString();
                return false;
            }
        }
        public bool UPDATE_BCR_DATA(string strWH_TYP
                                  , string strBCR_NO
                              , ref string pRTN_MSG
                                  , string strBCR_STA = "99"
                                  , bool bTrans = true)
        {
            try
            {
                //if (bTrans == true)
                //    _pBdb.BeginTrans();

                int nSelCnt = 0;
                string strSql = "";
                pRTN_MSG += "[UPDATE_BCR_DATA]";

                // 상위 보고 
                strSql = "";
                strSql += CRLF + "      UPDATE BCR_DATA                              ";
                strSql += CRLF + "         SET UPD_USER_ID  = 'IO_TASK'              ";
                //if (strBCR_STA != "0")
                strSql += CRLF + "           , BCR_STA      = :BCR_STA               ";
                strSql += CRLF + "           , UPD_DT       = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "       WHERE WH_TYP       = :WH_TYP                ";
                strSql += CRLF + "         AND BCR_NO       = :BCR_NO                ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("BCR_STA", DbLang.VARCHAR).Value = strBCR_STA;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("BCR_NO", DbLang.VARCHAR).Value = strBCR_NO;

                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    if (bTrans == true)
                        _pBdb.Rollback();

                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    if (bTrans == true)
                        _pBdb.Rollback();

                    pRTN_MSG += "변경할 BCR_DATA 정보가 존재하지 않습니다. [BCR_NO:" + strBCR_NO + "][BCR_STA:" + strBCR_STA + "]";
                    return false;
                }

                if (bTrans == true)
                    _pBdb.Commit();


                pRTN_MSG += "정상적으로 BCR_DATA 정보를 변경하였습니다. [BCR_NO:" + strBCR_NO + "][BCR_STA:" + strBCR_STA + "]";

                return true;
            }
            catch (Exception ex)
            {
                if (bTrans == true)
                    _pBdb.Rollback();

                pRTN_MSG += ex.ToString();
                return false;
            }
#endif


        }

        public bool UPDATE_CV_DATA(string strJOB_TYP,
                                   string strTRAY_TYP,
                                   string strTRAY_LEV,
                                   string strDEST_POS,
                                   string strIS_TURN,
                                   string strLUGG_NO,
                                   string strWH_TYP,
                                   string strPLC_NO,
                                   string strSTART_POS,
                                   string strJOB_LOT_NO,
                               ref string pRTN_MSG,
                                   string strBCR_BOTTOM = "",
                                   string strBCR_TOP = ""
                                    )
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                pRTN_MSG = "[UPDATE_CV_DATA]";

                strSql = "";
                strSql += CRLF + " UPDATE CV_DATA                                 ";
                strSql += CRLF + "    SET JOB_TYP_OD         = :JOB_TYP_OD     ";
                strSql += CRLF + "      , DEST_POS_OD        = :DEST_POS_OD    ";
                strSql += CRLF + "      , LUGG_NO_OD         = :LUGG_NO_OD     ";
                strSql += CRLF + "      , OD_RQ_YN           = 'Y'             ";
                strSql += CRLF + "      , OD_USER_ID         = 'IOTASK'        ";
                strSql += CRLF + "      , OD_UPD_DT          =  " + DbLang.SYSDATE + "          ";
                strSql += CRLF + "  WHERE WH_TYP             = :WH_TYP         ";
                strSql += CRLF + "    AND PLC_NO             = :PLC_NO         ";
                strSql += CRLF + "    AND MC_NO              = :TRACK_NO       ";
                strSql += CRLF + "    AND OD_RQ_YN           = 'N'             ";
                strSql += CRLF + "    AND (ERROR_CODE = '0' OR  ERROR_CODE = '0000')";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_TYP_OD", DbLang.VARCHAR).Value = strJOB_TYP;
                _pBdb.mComMain.Parameters.Add("DEST_POS_OD", DbLang.VARCHAR).Value = strDEST_POS;
                _pBdb.mComMain.Parameters.Add("LUGG_NO_OD", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR).Value = strPLC_NO;
                _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strSTART_POS;
                nSelCnt = _pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    //                    pRTN_MSG += "변경할 CV_DATA 정보가 존재하지 않습니다. TRACK_NO : " + strSTART_POS;
                    return false;
                }
                pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                return false;
            }
        }
    }
}
