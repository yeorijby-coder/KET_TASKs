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


        /*
         * JOB_MST.JOB_STATUS 라이프사이클
         *
         *   이동(6)      99 → 10 → 11 → 19
         *   입고(1)      99 → 10 → 11 → 21 → 29
         *   출고(2,3)    99 → 20 → 21 → 29 → 11 → 19
         *
         *   값은 이 현장 코드표(COMMON_CODE.JOB_STATUS)에 등록된 것을 그대로 쓴다.
         *   레거시 cThread_CV.cs 도 10 을 세우고 10/11 을 찾아 쓴다.
         *   통합 스케줄러를 만들며 잠깐 쓰던 15/25 는 코드표에 없는 값이라 없앴다.
         *   "지시했다"와 "구동중"을 따로 두지 않는다. 지시가 나가면 구동중이다.
         */
        public const string ST_CV_WAIT = "10"; // CV 구동대기 (접수 - CV 구간 진입 대기)
        public const string ST_CV_RUN = "11"; // CV 구동중 (지시가 나가 화물이 CV 위에 있다)
        public const string ST_CV_DONE = "19"; // CV 도착완료 보고

        public const string ST_SC_WAIT = "20"; // SC 구동요구 (접수 - 크레인 지시 대기)
        public const string ST_SC_RUN = "21"; // SC 구동중 (지시가 나가 크레인이 움직인다)
        public const string ST_SC_DONE = "29"; // SC 구동완료 보고
        public const string ST_SC_DONE_A2A_STEP1 = "28"; // 호기간 이동 1단계(출발 호기 출고) 완료 - 2단계(도착 호기 입고)가 남았다

        /*
         * 크레인 상태값 - 레거시 EcsEnv.h
         *   ONLINE_MODE_RD  0 오프라인 / 1 온라인 / 2 리모트      (지상반)
         *   AUTO_MODE_RD    1 자동 / 2 수동                       (기상반)
         *   ACTIVE_MODE_RD  0 정지 / 1 액티브
         *   UCSTATUS_RD     0 대기 / 1 아이들 / 2 이동중 / 4 에러
         */
        public const string SC_MODE_ONLINE = "1";
        public const string SC_MODE_AUTO   = "1";
        public const string SC_ACTIVE      = "1";
        public const string SC_STA_IDLE    = "0";
        public const string SC_STA_WAIT    = "1";

        /*
         * 크레인이 지시를 받을 수 있는 상태인가 - 레거시 CScInfo::IsReadyToWork (ScInfo.cpp:1620)
         *
         *   지상반 온라인 / 기상반 자동 / 액티브 / 상태 대기 라야 낸다.
         *   레거시는 이동중과 에러만 걸러 아이들(1)도 받았지만, 이 현장 크레인은
         *   아이들을 쓰지 않으므로 대기(0)일 때만 낸다.
         *
         *   여기에 "크레인에 작업이 없어야" 를 더한다.
         *     OD_RQ_YN='N'   앞 지시를 SC_TASK 가 이미 가져갔다
         *     ITN_LUGG_FK1   WCS 가 적어 둔 작업번호. 남아 있으면 앞 작업이 안 끝났다
         *                    (크레인이 읽어 준 LUGG_NO_FK1_RD 는 보지 않는다. 아래 참고)
         *     COMPLETE_RD    완료표시가 남아 있으면 ScCompleteCheck 가 아직 안 치웠다
         *
         *   에러코드는 '0000' 과 '0' 을 같이 본다. SC_TASK 는 네 자리로 쓰지만
         *   초기 시드 행에는 '0' 으로 들어 있는 것이 있어 '0000' 만 보면 영영 안 나간다.
         *
         *   ※ 조회에서 SC_DATA 의 별칭은 SD 여야 한다.
         */
        /*
         * 공백 관용 - 값이 비어 있는지 볼 때 쓰는 트림 문자 목록.
         *
         *   PLC 나 시드 데이터에 공백 한 칸이 들어 있는 경우가 있어
         *   IN ('','0','0000') 처럼 '' 만 봐서는 걸러지지 않는다.
         *   (실제로 SC_DATA.ITN_LUGG_FK1 이 ' ' 라 크레인이 영영 안 잡혔다)
         *
         *   BTRIM 으로 스페이스/탭/개행을 걷어내고 남은 것이 없으면 '0' 으로 접는다.
         *     COALESCE(NULLIF(BTRIM(컬럼, SQL_WS), ''), '0') IN ('0','0000')
         *   빈 값이 '0' 이 되므로 목록에서 '' 는 뺐다. 나머지 값의 판정은 이전과 같다.
         */
        public const string SQL_WS = "' ' || CHR(9) || CHR(10) || CHR(13)";

        /*
         * 우선순위 정렬식 - 값이 큰 것이 먼저다.
         *
         *   우선순위는 3자리로 정의되어 있다. 그런데 JOB_MST.JOB_PRIORITY 는
         *   varchar 라 그냥 ORDER BY 하면 문자열 순서고, 자릿수를 안 채워 넣은 값이
         *   섞이면 뒤집힌다. '90' 이 '150' 보다 위로 온다.
         *   실제로도 0 / 000 / 100 / 110 / 150 처럼 섞여 들어와 있다.
         *
         *   3자리로 0 을 채워 비교하면 문자열 순서가 곧 숫자 순서가 된다.
         *     0 -> 000     90 -> 090     100 -> 100     150 -> 150
         *   숫자로 캐스팅하지 않는 것은, 값에 숫자 아닌 것이 들어와도 질의가
         *   죽지 않게 하려는 것이다. 비어 있으면 '0' 으로 접어 맨 뒤로 보낸다.
         */
        public static string SQL_JOB_PRIORITY(string strAlias)
        {
            string strCol = ((strAlias == "") ? "" : strAlias + ".") + "JOB_PRIORITY";
            return "LPAD(COALESCE(NULLIF(BTRIM(" + strCol + ", " + SQL_WS + "), ''), '0'), 3, '0')";
        }

        public const string SQL_SC_READY =
              cDefApp.CRLF + "    AND SD.OD_RQ_YN         = 'N'                          "
            + cDefApp.CRLF + "    AND SD.ONLINE_MODE_RD   = '" + SC_MODE_ONLINE + "'      "
            + cDefApp.CRLF + "    AND SD.AUTO_MODE_RD     = '" + SC_MODE_AUTO + "'        "
            + cDefApp.CRLF + "    AND SD.ACTIVE_MODE_RD   = '" + SC_ACTIVE + "'           "
            //            + cDefApp.CRLF + "    AND SD.UCSTATUS_RD      = '" + SC_STA_WAIT + "'         "
            + cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.UCSTATUS_RD, " + SQL_WS + "), ''), '0')    IN ('" + SC_STA_IDLE + "','" + SC_STA_WAIT + "')"
            + cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.ERR_CODE_RD, " + SQL_WS + "), ''), '0')    IN ('0','0000')  "
            + cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.ITN_LUGG_FK1, " + SQL_WS + "), ''), '0')   IN ('0','0000')  ";
//            + cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.LUGG_NO_FK1_RD, " + SQL_WS + "), ''), '0') IN ('0','0000')  ";
        /*
         * 크레인이 읽어 준 작업번호(LUGG_NO_FK1_RD)는 보지 않는다.
         *
         *   이 현장 크레인은 작업을 마쳐도 제가 들고 있던 작업번호를 지우지
         *   않는다. 그 값을 "앞 작업이 안 끝났다" 로 읽으면 첫 작업 뒤로
         *   그 크레인에는 영영 지시가 나가지 않는다.
         *
         *   앞 작업이 끝났는지는 WCS 가 적어 둔 ITN_LUGG_FK1 로 본다. 그것은
         *   완료 처리(SC_COMP_CHK)에서 '0' 으로 되돌리므로 믿을 수 있다.
         *   크레인이 읽어 준 값은 완료 짝 맞추기(cThread_SC) 에서만 쓴다.
         */
//            + cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.COMPLETE_RD, " + SQL_WS + "), ''), '0')    IN ('0')         ";      // 이건 보지 말자
        /*
         * 입고/출고 정지 - 레거시 SC_INFO->m_bStoreSuspend / m_bRetrieveSuspend
         *
         *   SC_DATA.SUSPEND 하나에 두 비트를 담는다.
         *     0 정지 없음 / 1 입고 정지 / 2 출고 정지 / 3 둘 다 정지
         *   (WCS Client 의 ScSkinDlg 가 이 값으로 입고/출고 체크를 그린다)
         */
        public const string SQL_SC_STO_NOT_SUSPEND =
            cDefApp.CRLF + "    AND COALESCE(SD.SUSPEND,'0') IN ('0','2')                 ";
        public const string SQL_SC_RET_NOT_SUSPEND =
            cDefApp.CRLF + "    AND COALESCE(SD.SUSPEND,'0') IN ('0','1')                 ";

        /*
         * H/S 트랙이 멈춰 있지 않은가.
         *
         *   TR_PAUSE_RD 는 PLC 가 알려 준 정지, TR_PAUSE_OD 는 우리가 내린 정지다.
         *   둘 중 하나라도 서 있으면 그 H/S 로는 화물을 넣지도 빼지도 않는다.
         *   USE_YN='N' 은 아예 쓰지 않는 자리다.
         *
         *   ※ 조회에서 CV_DATA 의 별칭은 CD 여야 한다.
         */
        public const string SQL_HS_NOT_PAUSED =
              cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.TR_PAUSE_RD, " + SQL_WS + "), ''), '0') IN ('0')            "
            + cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.TR_PAUSE_OD, " + SQL_WS + "), ''), '0') IN ('0')            "
            //+ cDefApp.CRLF + "    AND COALESCE(CD.USE_YN,'Y')      = 'Y'                  "
            ;

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

        /*
         * 작업구분은 정상과 반자동이 짝을 이룬다.
         *
         *     입고   1 / 11      출고       2 / 12      픽킹출고 3 / 13
         *     랙투랙 4 / 14      호기간이동 5 / 15      이동     6 / 10
         *
         * 그런데 곳곳에서 정상 번호 하나만 비교해 반자동 작업이 통째로 걸러졌다.
         * 반자동 입고(11)는 크레인 지시 질의가 JOB_TYP = '1' 이라 아예 안 잡혀,
         * CV 까지는 와도 크레인이 작업을 받지 않았다.
         *
         * 비교는 번호 하나가 아니라 짝으로 한다.
         *   SQL  : JOB_TYP IN (JT_IN_STO) 처럼 목록을 쓴다
         *   코드 : IsStoJobType / IsMoveJobType 처럼 패턴으로 본다
         */
        public const string JT_IN_STO  = "'1','11'";
        public const string JT_IN_RET  = "'2','12'";
        public const string JT_IN_PICK = "'3','13'";
        public const string JT_IN_R2R  = "'4','14'";
        public const string JT_IN_A2A  = "'5','15'";
        public const string JT_IN_MOVE = "'6','10'";

        protected bool IsStoJobType(string strJOB_TYP)  { return ConvertJobTypeToPattern(strJOB_TYP) == EN_JOB_PATTERN.enJobPatternSto; }
        protected bool IsMoveJobType(string strJOB_TYP) { return ConvertJobTypeToPattern(strJOB_TYP) == EN_JOB_PATTERN.enJobPatternMove; }
        protected bool IsPickJobType(string strJOB_TYP) { return ConvertJobTypeToPattern(strJOB_TYP) == EN_JOB_PATTERN.enJobPatternPR; }
        protected bool IsA2AJobType(string strJOB_TYP)  { return ConvertJobTypeToPattern(strJOB_TYP) == EN_JOB_PATTERN.enJobPatternW2W; }
        protected bool IsR2RJobType(string strJOB_TYP)  { return ConvertJobTypeToPattern(strJOB_TYP) == EN_JOB_PATTERN.enJobPatternR2R; }
        // SC PLC 명령 JOB_TYP_OD (레거시 SC_JOB_TYPE_* : 1=Store, 2=Retrieve)
        public const string SC_CMD_STORE = "1";
        public const string SC_CMD_RETRIEVE = "2";

        // SC_HS_DEF.HS_NO : 1층 홈스탠드 랭크 (레거시 ECS RANK_1=입고HS, RANK_2=출고HS)
        public const string HS_NO_STORE = "01"; // 입고 HS (CV → SC 픽업)
        public const string HS_NO_RETRIEVE = "02"; // 출고 HS (SC → CV 언로드)


        #region 화면 표시용.
        // 전용 스레드(지시 / 도착 검사)가 내는 화면 로그에는 말머리를 붙여 층 스레드 본체의 로그와 구분한다.
        //   층 스레드 본체 = 말머리 없음,  지시 전용 = "[지시] ",  도착 검사 전용 = "[도착] "
        //   같은 층 탭에 섞여 나오므로 한눈에 어느 스레드가 낸 것인지 보이게 하려는 것이다.
        public string m_strWorkerTag = "";
        // ※ callPsMsgView / m_LogQ null 가드 (2026-07-11) :
        //    화면 델리게이트 연결 전(또는 미연결 상태)에 호출되어도 예외가 발생하지 않도록 한다.
        //  strFile / strFunc 는 넘기지 않으면 컴파일러가 채운다.
        //  이 함수를 부른 소스 파일 경로와 함수 이름이 그대로 들어와,
        //  화면 로그의 FILE / FUNCTION 열에 찍힌다.
        public void MakeMsg(string msg,
                            [CallerFilePath] string strFile = "",
                            [CallerMemberName] string strFunc = "")
        {
            if (m_strWorkerTag != "") msg = m_strWorkerTag + msg;
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
            if (m_strWorkerTag != "") msg = m_strWorkerTag + msg;
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
            if (m_strWorkerTag != "") msg = m_strWorkerTag + msg;
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
            if (m_strWorkerTag != "") msg = m_strWorkerTag + msg;
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
        public void MakeMsg_Blue(string msg,
                                [CallerFilePath] string strFile = "",
                                [CallerMemberName] string strFunc = "")
        {
            if (m_strWorkerTag != "") msg = m_strWorkerTag + msg;
            try
            {
                if (callPsMsgView != null)
                    callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_BLUE, strFile, strFunc);
                if (cDefApp.m_LogQ[m_nId] != null)
                    cDefApp.m_LogQ[m_nId].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /// <summary>구↔신 라인 이음새(1F 264→531, 556→250 / 3F 352→631, 654→355) 작업 흐름을 "이음새" 탭에 남긴다.
        /// 이음새 탭을 쓰지 않으면(SPLIT_TABS=N 또는 USE_SEAM=N) false 를 돌려주니, 부른 쪽이 각 층 탭에 그대로 남기면 된다.
        /// 색 : 일반(복사/삭제), 주황(대기 - 왜 안 넘어가는지), 붉은색(오류).</summary>
        public bool MakeSeamMsg(string msg, cDefApp.eLogMsgType typ = cDefApp.eLogMsgType.MSG_NOR,
                                [CallerFilePath] string strFile = "",
                                [CallerMemberName] string strFunc = "")
        {
            try
            {
                if (!cDefApp.GM_SPLIT_TABS || !cDefApp.GM_USE_SEAM) return false;
                int nSeam = (int)cDefApp.eThGbn.SCH_GR07;
                if (callPsMsgView != null)
                    callPsMsgView(msg, nSeam.ToString(), "", "", nSeam, typ, strFile, strFunc);
                if (cDefApp.m_LogQ[nSeam] != null)
                    cDefApp.m_LogQ[nSeam].Enqueue(new LogParam(DateTime.Now, msg));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        // @.이음새 트랙번호 -> 층. 1F 는 2xx / 5xx, 3F 는 3xx / 6xx (264, 556, 531, 250 / 352, 654, 355, 631)
        protected static string SeamFloor(string strMcNo)
        {
            if (strMcNo != null && strMcNo.Length > 0 && (strMcNo[0] == '3' || strMcNo[0] == '6')) return "3F";
            return "1F";
        }
        /// <summary>팔레트 매거진(207) 관련 작업 흐름을 "매거진(3F)" 탭에 남긴다. (어느 스레드에서 불러도 그 탭으로 간다)</summary>
        /// <para>색 : 매거진에서 랙으로 입고(공파렛트)하는 작업은 초록 바탕 흰 글자(MSG_IMP), 상위로 보내는 공파렛트 입고 요구(N)와 그 응답은
        /// 하늘색 바탕 검은 글자(MSG_BLUE), 그 밖(매거진으로 이동의 접수·구동·도착)은 보통(MSG_NOR). 글의 내용으로 가른다.</para>
        public void MakeMagMsg(string msg, cDefApp.eLogMsgType typ = cDefApp.eLogMsgType.MSG_NOR,
                               [CallerFilePath] string strFile = "",
                               [CallerMemberName] string strFunc = "")
        {
            try
            {
                if (!cDefApp.GM_SPLIT_TABS) return;     // 분리하지 않으면 기존대로(각 층 탭의 기존 로그만)
                if (typ == cDefApp.eLogMsgType.MSG_NOR)
                {
                    if (msg.Contains("공파렛트 입고 요구(N)")) typ = cDefApp.eLogMsgType.MSG_BLUE;
                    else if (msg.Contains("매거진에서 입고")) typ = cDefApp.eLogMsgType.MSG_IMP;
                }
                int nMag = (int)cDefApp.eThGbn.SCH_GR06;
                if (callPsMsgView != null)
                    callPsMsgView(msg, nMag.ToString(), "", "", nMag, typ, strFile, strFunc);
                if (cDefApp.m_LogQ[nMag] != null)
                    cDefApp.m_LogQ[nMag].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /// <summary>호기간 이동은 3층(3F) 탭, 랙투랙은 공통(랙투랙) 탭에 단계 로그를 남긴다. (어느 스레드에서 불러도 그 탭으로 간다)</summary>
        /// <para>호기간 이동은 출발 호기(3층 출고) -> 컨베이어 -> 도착 호기(3층 입고)로 3F 스레드와 1F 스레드가 번갈아 처리하므로,
        /// 흐름이 한 탭에서 이어 읽히도록 모아 둔다. 색 : 지시는 초록(MSG_IMP), 완료는 하늘색(MSG_BLUE), 접수/기록은 보통.</para>
        public void MakeRackMsg(string msg, cDefApp.eLogMsgType typ = cDefApp.eLogMsgType.MSG_NOR,
                                [CallerFilePath] string strFile = "",
                                [CallerMemberName] string strFunc = "")
        {
            try
            {
                int nTab = msg.StartsWith("[호기간") ? (int)cDefApp.eThGbn.SCH_GR02 : (int)cDefApp.eThGbn.SCH_GR04;   // 호기간 이동은 3층(3F) 탭, 랙투랙은 공통(LOG삭제/접수/랙투랙) 탭
                if (callPsMsgView != null)
                    callPsMsgView(msg, nTab.ToString(), "", "", nTab, typ, strFile, strFunc);
                if (cDefApp.m_LogQ[nTab] != null)
                    cDefApp.m_LogQ[nTab].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception) { }
        }
        // @.팔레트 매거진 스테이션 번호
        protected const string MAG_STN = "207";
        protected static string MagJobName(string strJobTyp, string strStart, string strDest)
        {
            if (strStart == MAG_STN) return "매거진에서 입고(공파렛트를 랙으로)";
            if (strDest == MAG_STN) return "매거진으로 이동";
            return "작업구분 " + strJobTyp;
        }
        public void MakeMsg_Orange(string msg,
                                [CallerFilePath] string strFile = "",
                                [CallerMemberName] string strFunc = "")
        {
            try
            {
                if (callPsMsgView != null)
                    callPsMsgView(msg, m_nId.ToString(), "", "", m_nId, cDefApp.eLogMsgType.MSG_ORANGE, strFile, strFunc);
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
                strSql += CRLF + "           , PAIR_LUGG_NO     ";
                strSql += CRLF + "           , LOT_NO           ";
                strSql += CRLF + "           , ITEM_INFO        ";
                strSql += CRLF + "           , ITEM_INFO2 )     ";
                strSql += CRLF + " SELECT TO_CHAR(" + DbLang.SYSDATE + ", 'YYYYMMDD'), TO_CHAR(" + DbLang.SYSDATE + ", 'HH24MISS')                            ";
                strSql += CRLF + "      , WH_TYP, LUGG_NO, START_POS, START_LOCATION, DEST_POS, DEST_LOCATION                   ";
                strSql += CRLF + "      , TURN, PRODUCT_ID, PRODUCT_SIZE, JOB_TYP, BCR_TOP, BCR_BOTTOM, MES_WRITE_YN            ";
                strSql += CRLF + "      , CMD_STA, JOB_KIND, JOB_STATUS, JOB_PRIORITY, MES_ERROR_CD, OD_LAST_PAGE, OD_LAST_USER ";
                strSql += CRLF + "      , JOB_START_DT, " + DbLang.SYSDATE + ", 'IO_TASK', REMARKS, TRAY_TYP, TRAY_LEV, HS_TRACK_NO, SC_NO    ";
                strSql += CRLF + "      , DURATION_TIME, SYSTEM_BYTE, S6F12_RESPONSE, S6F11_YON, CEID_NAME, FK_NO, AGING_TIME   ";
                strSql += CRLF + "      , PAIR_LUGG_NO, LOT_NO, ITEM_INFO, ITEM_INFO2   ";
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
                    strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(ERROR_CODE_RD, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')              ";
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

                /*
                 * 뱅크에서 호기를 구한다. 창고마다 셈이 다르다.
                 *   (기존 ECS : EcsSv/Lib.cpp CLib::GetStackerNum, CLib::IsValidLocation)
                 *
                 *   파레트 창고(10) : 뱅크 01~20. 두 줄에 한 대씩 901~910.
                 *   P-BOX 창고(20) : 뱅크 01~02. 한 대뿐이라 늘 911.
                 *
                 * Location 은 창고 안에서만 뜻이 있는 번호다. 뱅크 01 이 어느
                 * 창고의 01 인지는 WH_TYP 이 가린다. 그래서 창고를 함께 본다.
                 * 전에는 01~08 만 알아 901/902 로 갈랐고(개발용 자료 그대로),
                 * 09 이상과 P-BOX 는 모두 "잘못된 Location" 으로 떨어졌다.
                 */
                int nBank = 0;
                int.TryParse(strBANK, out nBank);

                /*
                 * 뱅크는 창고 안에서만 뜻이 있는 번호다.
                 *   파레트 창고 01~20  -> 두 줄에 한 호기 : 901~910
                 *   P-BOX  창고 01~02  -> 호기는 하나 : 911
                 *
                 * 그래서 뱅크만으로는 호기를 못 구한다. 창고를 함께 봐야 한다.
                 * 뱅크 01 은 파레트면 901, P-BOX 면 911 이다.
                 *
                 * 크레인에 나가는 값은 어느 창고든 1/2 다.
                 *   (ScThread.cs GfSideOfBank, 기존 ECS CSc::GetSide)
                 *
                 * ※ 현장 자료에 P-BOX 를 21/22 로 적어 둔 작업이 있다.
                 *   (2026-10-12 JOB_MST_HIS 의 21-056-01 등)
                 *   그것은 시운전 중 급히 만든 것이고 올바른 값이 아니다.
                 *   받아 주지 않는다. 틀린 자료는 걸러서 드러나는 쪽이 맞다.
                 */
                bool bBox = (strWH_TYP != null && strWH_TYP.Trim() == ((int)cDefApp.eWHTYP.KET_WH02).ToString());

                int nBankMax = bBox ? 2 : 20;
                bool bBankOk = (nBank >= 1) && (nBank <= nBankMax);

                if (bBankOk == false)
                {
                    pRTN_MSG += "잘못된 Location 입니다 - SC 번호를 구할수 없습니다."
                              + " [창고:" + strWH_TYP + "][BANK:" + strBANK + "]"
                              + " 뱅크는 01 ~ " + nBankMax.ToString("00") + " 까지다. ";
                    return false;
                }

                // P-BOX 는 호기가 하나다. 파레트는 뱅크 두 줄에 한 대씩이다.
                if (bBox)
                    strSC_NO = "911";
                else
                    strSC_NO = "9" + ((nBank - 1) / 2 + 1).ToString("00");

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
            // @.숫자가 아니거나 비어 있어도 죽지 않게 한다. (예전에는 Convert.ToInt32 라 던졌다)
            int nJobType;
            if (int.TryParse((strJobTyp == null ? "" : strJobTyp).Trim(), out nJobType) == false)
                return EN_JOB_PATTERN.enJobPatternNone;

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
                bool bStoStation = Convert.ToBoolean(byteTemp & cDefApp.STN_KIND_STO);   // 입고대
                bool bRetStation = Convert.ToBoolean(byteTemp & cDefApp.STN_KIND_RET);   // 출고대

                // @.예전에는 (byteTemp & 0x03) 을 도착대라고 읽었다. 그건 "입고대이거나
                //   출고대" 라는 뜻이지 도착대라는 뜻이 아니다. 도착대는 EcsDefine.xml 에
                //   ArvStation 으로 따로 정의된 자리다(예: 1F Size Checker).
                bool bArvStation = Convert.ToBoolean(byteTemp & cDefApp.STN_KIND_ARV);   // 도착대


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
                bool bStoStation = Convert.ToBoolean(byteTemp & cDefApp.STN_KIND_STO);   // 입고대
                bool bRetStation = Convert.ToBoolean(byteTemp & cDefApp.STN_KIND_RET);   // 출고대

                // @.예전에는 (byteTemp & 0x03) 을 도착대라고 읽었다. 그건 "입고대이거나
                //   출고대" 라는 뜻이지 도착대라는 뜻이 아니다. 도착대는 EcsDefine.xml 에
                //   ArvStation 으로 따로 정의된 자리다(예: 1F Size Checker).
                bool bArvStation = Convert.ToBoolean(byteTemp & cDefApp.STN_KIND_ARV);   // 도착대


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
                strSql += CRLF + "           , PAIR_LUGG_NO     ";
                strSql += CRLF + "           , LOT_NO           ";
                strSql += CRLF + "           , ITEM_INFO        ";
                strSql += CRLF + "           , ITEM_INFO2 )     ";
                strSql += CRLF + " SELECT TO_CHAR(" + DbLang.SYSDATE + ", 'YYYYMMDD'), TO_CHAR(" + DbLang.SYSDATE + ", 'HH24MISS')                            ";
                strSql += CRLF + "      , WH_TYP, LUGG_NO, START_POS, START_LOCATION, DEST_POS, DEST_LOCATION                   ";
                strSql += CRLF + "      , TURN, PRODUCT_ID, PRODUCT_SIZE, JOB_TYP, BCR_TOP, BCR_BOTTOM, MES_WRITE_YN            ";
                strSql += CRLF + "      , CMD_STA, JOB_KIND, JOB_STATUS, JOB_PRIORITY, MES_ERROR_CD, OD_LAST_PAGE, OD_LAST_USER ";
                strSql += CRLF + "      , JOB_START_DT, " + DbLang.SYSDATE + ", 'IO_TASK', REMARKS, TRAY_TYP, TRAY_LEV, HS_TRACK_NO, SC_NO    ";
                strSql += CRLF + "      , DURATION_TIME, SYSTEM_BYTE, S6F12_RESPONSE, S6F11_YON, CEID_NAME, FK_NO, AGING_TIME   ";
                strSql += CRLF + "      , PAIR_LUGG_NO, LOT_NO, ITEM_INFO, ITEM_INFO2   ";
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
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')";

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

        // ================================================================
        //  층 공통 (1F / 3F / BOX 스케줄러 스레드가 같이 쓴다)
        //
        //    층별 클래스(cThread_SCH_*)에 같은 코드가 세 벌 있던 것을 여기로 올렸다.
        //    층마다 다른 것 — Thread_Doing, CV_RETHS_PLC, ScStoreRoutine,
        //    ScRetrieveRoutine, UpdateScData, CHECK_RET_LANE_READY, RetCmdCheck 과
        //    층 전용 상수 / 얇은 래퍼 — 는 층별 클래스에 그대로 남겨 두었다.
        // ================================================================
        #region 층 공통 - 멤버

        // 창고 구분 : cDefApp.eWHTYP.KET_WH01 = 10
        /*
         * 이 스케줄러 스레드가 맡은 창고.
         *
         *   한 프로세스 안에서 1F / 3F / BOX 세 스레드가 돈다. P-BOX 는 WMS 가
         *   따로 관리해 WH_TYP 20(KET_WH02) 으로 떼어 놓았으므로, 창고를 모든
         *   스레드가 나눠 쓰는 static 으로 두면 BOX 스레드가 제 설비를 못 찾는다.
         *   인스턴스마다 두고 BOX 스레드 생성자에서 20 으로 바꾼다.
         */
        protected string SCH_WH_TYP = ((int)cDefApp.eWHTYP.KET_WH01).ToString();

        // 명령 지시 주체 표기
        protected const string OD_USER = "IOTASK";

        // 공용 리턴 메세지
        public string strRTN_MSG = "";
        public string m_strRtnMsg = "";

        // 이음새 복사 상태 (레거시 m_bCopied 대응 : key = FROM_TO, value = 복사한 작업번호)
        protected readonly Dictionary<string, string> m_dicSeamCopied = new Dictionary<string, string>();

        // Thread_Doing 공용 호출 헬퍼가 받는 스케줄 함수 꼴
        protected delegate bool SchFunc(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG);

        #endregion

        #region 층 공통 - 함수

        /*
         * ReportOnChange :: 같은 말을 200ms 마다 쏟지 않게, 바뀔 때만 알린다.
         *   스케줄러는 한 주기에 같은 자리를 계속 지나므로, 막힌 사유는 상태가
         *   바뀌는 순간에만 한 줄 나오면 된다. 풀리면 "풀렸다" 도 한 줄 낸다.
         */
        private readonly Dictionary<string, string> m_dicLastReport = new Dictionary<string, string>();

        protected void ReportOnChange(string strKey, string strMsg)
        {
            string strPrev;
            if (m_dicLastReport.TryGetValue(strKey, out strPrev) && strPrev == strMsg)
                return;

            m_dicLastReport[strKey] = strMsg;
            if (strMsg != "") MakeMsg_Error_NoLog(strMsg);
        }

        protected void ClearReport(string strKey, string strMsg)
        {
            string strPrev;
            if (m_dicLastReport.TryGetValue(strKey, out strPrev) == false || strPrev == "")
                return;

            m_dicLastReport[strKey] = "";
            MakeMsg(strMsg);
        }

        /*
         * REPORT_CV_START_BLOCKED :: CV 출발 지시가 왜 안 나가는지 한 줄로 알린다.
         *
         *   CV_STO_START_PLC 의 조회는 설비 조건을 전부 AND 로 건다. 하나만 어긋나도
         *   0 건이 되어 조용히 다음 주기로 넘어가고, 무엇이 어긋났는지 볼 자리가 없었다.
         *   "지시는 받았는데 작업대에 쓰지를 못한다" 가 여기서 났다.
         *
         *   대기 중인 작업(10)의 출발 작업대를 그대로 읽어 어느 칸이 막고 있는지 적는다.
         *   화물이 아직 안 왔을 뿐인 정상 대기도 여기 걸리므로, 바뀔 때만 남긴다.
         */
        private readonly Dictionary<string, int[]> m_dicUnknownBlock = new Dictionary<string, int[]>();
        private const int UNKNOWN_BLOCK_HOLD_MS = 2000;   // 원인 불명 상태가 이만큼 이어져야 알린다
        private const int UNKNOWN_BLOCK_GAP_MS  = 1500;   // 이보다 오래 안 보이면 이어진 것이 아니라 새로 센다

        protected void REPORT_CV_START_BLOCKED(string strWH_TYP, string strCV_PLC, string strTitle)
        {
            string strKey = "CVSTART_" + strCV_PLC;

            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT JM.LUGG_NO, JM.JOB_TYP, JM.START_POS, CD.MC_NO, CD.STN_KIND ";
                strSql += CRLF + "      , CD.STO_READY_RD, CD.RET_READY_RD, CD.SENSOR0_DATA_RD        ";
                strSql += CRLF + "      , CD.AUTO_MODE_RD, CD.OD_RQ_YN, CD.OD_RQ_FLAG                 ";
                strSql += CRLF + "      , CD.TR_PAUSE_RD, CD.ERROR_CODE, CD.LUGG_NO_RD                ";
                strSql += CRLF + "   FROM JOB_MST JM                                                  ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD                                             ";
                strSql += CRLF + "     ON CD.WH_TYP = JM.WH_TYP AND CD.HOST_STN_NO = JM.START_POS     ";
                strSql += CRLF + "  WHERE JM.WH_TYP     = :WH_TYP                                     ";
                strSql += CRLF + "    AND JM.JOB_STATUS = '" + ST_CV_WAIT + "'                        ";
                strSql += CRLF + "    AND CD.PLC_NO     = :CV_PLC                                     ";
                strSql += CRLF + "  ORDER BY JM.LUGG_NO                                               ";
                strSql += CRLF + "  LIMIT 1                                                           ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;

                int nCnt = _pBdb.ExcuteQry(strSql);
                if (nCnt <= 0)
                {
                    // 기다리는 작업이 없다. 막힌 것이 아니라 할 일이 없는 것이다.
                    m_dicUnknownBlock.Remove(strKey);
                    ClearReport(strKey, strTitle + "출발 대기가 풀렸다.");
                    return;
                }

                DataRow r = _pBdb.mDtMain.Rows[0];
                string strLugg   = GetVal(r, "lugg_no");
                string strStn    = GetVal(r, "start_pos");
                string strMc     = GetVal(r, "mc_no");
                string strLuggRd = GetVal(r, "lugg_no_rd").Trim();

                int nKind = 0;
                int.TryParse(GetVal(r, "stn_kind"), out nKind);

                bool bSto = (nKind & cDefApp.STN_KIND_STO) != 0;
                bool bArv = (nKind & cDefApp.STN_KIND_ARV) != 0;

                List<string> lstWhy = new List<string>();

                // 출발 자격 : 입고대(트래킹이 비어 있어야 한다) 이거나 도착대(출고 준비)
                if (bSto == false && bArv == false && IsStartNoReadyStation(strStn) == false)
                {
                    lstWhy.Add("STN_KIND=" + nKind + " 에 입고대(0x01)도 도착대(0x40)도 없다");
                }
                else
                {
                    bool bLuggEmpty = (strLuggRd == "" || strLuggRd == "0" || strLuggRd == "0000");
                    bool bStoOk = bSto && GetVal(r, "sto_ready_rd") == "1" && bLuggEmpty;
                    bool bArvOk = bArv && GetVal(r, "ret_ready_rd") == "1";
                    bool bNoReadyOk = IsStartNoReadyStation(strStn) && bLuggEmpty;      // START_NO_READY_STN

                    if (bStoOk == false && bArvOk == false && bNoReadyOk == false)
                    {
                        if (bSto && GetVal(r, "sto_ready_rd") != "1")
                            lstWhy.Add("입고대인데 STO_READY_RD=" + GetVal(r, "sto_ready_rd"));
                        if (bSto && GetVal(r, "sto_ready_rd") == "1" && bLuggEmpty == false)
                            lstWhy.Add("입고대인데 앞 작업번호가 남아 있다 LUGG_NO_RD=" + strLuggRd);
                        if (bArv)
                            lstWhy.Add("도착대인데 RET_READY_RD=" + GetVal(r, "ret_ready_rd"));
                    }
                }

                if (GetVal(r, "sensor0_data_rd") != "1") lstWhy.Add("화물이 없다 SENSOR0_DATA_RD=" + GetVal(r, "sensor0_data_rd"));
                if (GetVal(r, "auto_mode_rd")    != "1") lstWhy.Add("자동모드가 아니다 AUTO_MODE_RD=" + GetVal(r, "auto_mode_rd"));
                if (GetVal(r, "od_rq_yn")        != "N") lstWhy.Add("앞 명령을 아직 안 가져갔다 OD_RQ_YN=" + GetVal(r, "od_rq_yn"));
                if (GetVal(r, "od_rq_flag")      != "N") lstWhy.Add("OD_RQ_FLAG=" + GetVal(r, "od_rq_flag"));

                string strPause = GetVal(r, "tr_pause_rd");
                if (strPause != "" && strPause != "0") lstWhy.Add("진입 대기 TR_PAUSE_RD=" + strPause);

                string strErr = GetVal(r, "error_code").Trim();
                if (strErr != "" && strErr.TrimStart('0') != "") lstWhy.Add("설비 오류 ERROR_CODE=" + strErr);

                if (lstWhy.Count == 0)
                {
                    // 막은 칸을 하나도 못 찾았다. 지시 조회와 이 진단 조회는 서로 다른 순간에 도는 별도 질의라,
                    // 그 사이에 PLC 값(피킹완료 버튼 등)이 바뀌면 이렇게 된다. 한 번 스치는 것은 알리지 않고 이 상태가 이어질 때만 알린다.
                    int nNow = Environment.TickCount;
                    int[] arrUnk;
                    if (m_dicUnknownBlock.TryGetValue(strKey, out arrUnk) == false || unchecked(nNow - arrUnk[1]) > UNKNOWN_BLOCK_GAP_MS)
                    {
                        arrUnk = new int[] { nNow, nNow };      // [0]=처음 본 시각, [1]=마지막으로 본 시각
                        m_dicUnknownBlock[strKey] = arrUnk;
                    }
                    arrUnk[1] = nNow;
                    if (unchecked(nNow - arrUnk[0]) < UNKNOWN_BLOCK_HOLD_MS)
                        return;

                    string strWhy = "막은 항목을 찾지 못했다 (설비 값은 모두 정상). 같은 작업번호 중복 또는 값이 바뀌는 중일 수 있다";
                    if (strLuggRd != "" && strLuggRd != "0" && strLuggRd != "0000")
                        strWhy += " - 트랙의 작업번호 LUGG_NO_RD(" + strLuggRd + ") 와 같은 번호의 작업이 작업목록에 남아 있는지 확인";
                    lstWhy.Add(strWhy);
                }
                else
                {
                    m_dicUnknownBlock.Remove(strKey);
                }

                ReportOnChange(strKey,
                               strTitle + "출발 지시를 못 낸다. [작업번호:" + strLugg + "][출발:" + strStn
                             + "][TRACK:" + strMc + "] - " + string.Join(" / ", lstWhy.ToArray()));
            }
            catch (Exception ex)
            {
                ReportOnChange(strKey, strTitle + "출발 막힌 사유를 읽지 못했다. " + ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 호기별 입고/출고 교대 (구 ECS CSc::InvokeCheck / StoreRoutine / RetrieveRoutine) (2026-11-22)
        //   구 ECS : 호기 스레드 하나가 직렬로
        //     직전 지시가 입고(m_nPrevRtn=STORE) 이면  출고 -> 입고 -> 랙투랙,  출고였으면 입고 -> 출고 -> 랙투랙 순으로 시도한다.
        //     입고는 직전 랭크가 1(1층) 이면 3(3층) 먼저, 3 이면 1 먼저.  출고는 직전 랭크가 2(1층) 이면 4(3층) 먼저, 4 이면 2 먼저.
        //     처음 값 : 직전=입고, 입고 랭크=1, 출고 랭크=2.   (가중치는 INI 기본 0 이라 꺼져 있어 옮기지 않았다)
        //   신규 : 층 스레드(1F/3F)가 따로 돌아 이 순서가 없었다. 호기마다 같은 상태를 두고, 차례가 아닌 쪽은
        //     순번 p(0=차례, 1=같은 종류의 다른 랭크, 2=다른 종류의 차례 랭크, 3=나머지)가 더 앞선 후보에 "지금 지시할 일감이 실제로 있을 때만" 양보한다.
        //     일감이 없거나 그 H/S 가 못 받으면 기다리지 않고 바로 지시한다. 시간을 정해 기다리지 않는다. (막힘 방지로 한 후보는 최대 TURN_DEFER_MAX_MS 만 양보)
        // ─────────────────────────────────────────────────────────────────
        private sealed class CraneTurnState
        {
            public int Prev = 1;        // 직전 지시 : 1 = 입고, 2 = 출고
            public int StoPrev = 1;     // 입고 직전 랭크 (1 / 3)
            public int RetPrev = 2;     // 출고 직전 랭크 (2 / 4)
            public int DeferStart = 0;  // 양보를 시작한 시각 (TickCount). 0 = 양보 중 아님
            // 지금 지시할 일감이 "실제로 있는" 후보 표시 시각. 0=InvokeStoRank1, 1=STO 3, 2=RET 2, 3=RET 4 (TickCount, 0=없음)
            public int[] Pending = new int[4];
        }
        private const int TURN_PENDING_MS = 4000;      // 후보 표시가 살아 있는 시간 (층 스레드 한 바퀴보다 길어야 한다)
        private const int TURN_DEFER_MAX_MS = 10000;   // 한 후보를 이만큼 기다려도 안 나가면 더 기다리지 않는다 (막힘 방지)
        private static int TurnIdx(bool bStore, int nRank) { return bStore ? ((nRank == 1) ? 0 : 1) : ((nRank == 2) ? 2 : 3); }
        private static readonly System.Collections.Generic.Dictionary<string, CraneTurnState> s_craneTurn
            = new System.Collections.Generic.Dictionary<string, CraneTurnState>();

        // @.차례 판정 기록 (IoTurn.log, 실행 폴더). INI [SC] DISPATCH_LOG=Y 일 때만. 기본 N.
        private static void TurnLog(string strMsg)
        {
            try
            {
                if (!cDefApp.GM_SC_ALT_LOG) return;
                System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IoTurn.log"),
                    DateTime.Now.ToString("HH:mm:ss.fff") + " " + strMsg + "\r\n");
            }
            catch { }
        }

        // 순번 p : 0 = 차례인 종류의 차례 랭크, 1 = 같은 종류의 다른 랭크, 2 = 다른 종류의 차례 랭크, 3 = 나머지 (구 ECS 시도 순서)
        private static int TurnRankP(CraneTurnState s, bool bStore, int nRank)
        {
            bool bPrefStore = (s.Prev == 2);                                           // 직전이 출고였으면 입고가 먼저
            int  nPrefRank  = bStore ? ((s.StoPrev == 1) ? 3 : 1) : ((s.RetPrev == 2) ? 4 : 2);
            return ((bStore == bPrefStore) ? 0 : 2) + ((nRank == nPrefRank) ? 0 : 1);
        }

        private static CraneTurnState TurnState(string strScNo)
        {
            CraneTurnState s;
            if (!s_craneTurn.TryGetValue(strScNo, out s)) { s = new CraneTurnState(); s_craneTurn[strScNo] = s; }
            return s;
        }

        /// <summary>그 호기에 이 후보(입고/출고, 랭크)의 일감이 지금 없다고 확인되면 표시를 지운다.</summary>
        protected static void TurnUnmark(string strScNo, bool bStore, int nRank)
        {
            if (!cDefApp.GM_SC_ALTERNATE) return;
            lock (s_craneTurn) { TurnState(strScNo).Pending[TurnIdx(bStore, nRank)] = 0; }
        }

        /// <summary>이 후보(입고/출고, 랭크)를 모든 호기에서 지운다. (그 점검이 호기 전체를 다시 훑기 직전에)</summary>
        protected static void TurnUnmarkAll(bool bStore, int nRank)
        {
            if (!cDefApp.GM_SC_ALTERNATE) return;
            lock (s_craneTurn)
                foreach (CraneTurnState s in s_craneTurn.Values) s.Pending[TurnIdx(bStore, nRank)] = 0;
        }

        /// <summary>
        /// 이 후보를 지금 지시해도 되는가. 호출한 쪽은 "이 호기가 비어 있고 이 후보의 일감이 실제로 있다" 는 것을 이미 확인한 상태다.
        ///   구 ECS 시도 순서(직전 종류의 반대 종류 먼저, 그 안에서 직전과 다른 랭크 먼저) 중
        ///   더 앞선 후보에 "지금 지시할 일감이 실제로 있으면" 그쪽에 양보하고, 없으면(일감이 없거나 그 H/S 가 못 받으면) 바로 지시한다.
        ///   시간을 정해 기다리지 않는다.
        /// </summary>
        protected static bool TurnAllowed(string strScNo, bool bStore, int nRank)
        {
            if (!cDefApp.GM_SC_ALTERNATE) return true;
            lock (s_craneTurn)
            {
                CraneTurnState s = TurnState(strScNo);
                int now = Environment.TickCount; if (now == 0) now = 1;
                int me = TurnIdx(bStore, nRank);
                s.Pending[me] = now;                       // 나도 일감이 있다는 표시

                int p = TurnRankP(s, bStore, nRank);
                string strBlocker = "";
                for (int i = 0; i < 4 && strBlocker == ""; i++)
                {
                    if (i == me || s.Pending[i] == 0) continue;
                    if (unchecked(now - s.Pending[i]) > TURN_PENDING_MS) { s.Pending[i] = 0; continue; }   // 오래된 표시
                    bool bSto = (i < 2);
                    int  nRk  = (i == 0) ? 1 : (i == 1) ? 3 : (i == 2) ? 2 : 4;
                    if (TurnRankP(s, bSto, nRk) < p) strBlocker = (bSto ? "STO" : "RET") + nRk;
                }

                if (strBlocker == "") { s.DeferStart = 0; return true; }

                if (s.DeferStart == 0)
                {
                    s.DeferStart = now;
                    TurnLog(strScNo + " DEFER " + (bStore ? "STO" : "RET") + nRank + " p=" + p + " (먼저 지시할 일감 있음: " + strBlocker + ", 직전 " + (s.Prev == 1 ? "STO" : "RET") + ")");
                }
                if (unchecked(now - s.DeferStart) > TURN_DEFER_MAX_MS)
                {
                    TurnLog(strScNo + " GO_TIMEOUT " + (bStore ? "STO" : "RET") + nRank + " (" + strBlocker + " 가 " + TURN_DEFER_MAX_MS + "ms 넘게 안 나가 먼저 지시)");
                    s.DeferStart = 0;
                    return true;
                }
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 차례 확인 - 지시 직전에 상대 쪽 일감을 DB 로 직접 본다 (2026-11-25)
        //
        //   전에는 입고 / 출고 점검이 각자 "일감 있음" 표시(Pending)를 남기고, 지시하려는 쪽이 그 표시를 보고 양보했다.
        //   그런데 표시는 크레인이 비어 있을 때만 남았다. 크레인이 일을 끝낸 순간에는 어느 쪽도 표시가 없어
        //   먼저 점검한 쪽이 차례와 상관없이 가져갔다. 입고가 계속 밀려오면 입고 점검이 매번 먼저 잡아,
        //   출고 작업이 있고 출고 H/S 가 비어 있어도 입고만 연속으로 나갔다.
        //   (2026-11-25 현장 906호기 14:11 ~ 14:25 : 1층 출고 6건이 대기 중인데 입고만 9회 연속)
        //
        //   이제 표시를 쓰지 않는다. 지시하려는 순간에, 구 ECS 시도 순서로 나보다 앞선 후보가 "지금 이 호기에 실제로
        //   지시할 수 있는 일감" 을 갖고 있는지 DB 에서 바로 확인한다. 있으면 양보하고 없으면 바로 지시한다.
        //   확인에 쓰는 조회는 실제 지시 루틴이 쓰는 것과 같은 함수(SqlRetJobs / SqlRetScHsReady / SqlStoCandidates)다.
        //   그래서 "지시할 수 있다" 는 판단이 양쪽에서 어긋나지 않는다.
        //   앞선 후보가 일감이 있는데도 TURN_DEFER_MAX_MS 넘게 안 나가면 더 기다리지 않는다. (막힘 방지)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>출고 대기(20) 작업 목록. 1층은 도착이 STN_1F_PLT, 3층은 STN_3F_PLT + 호기간 이동. 인자 : WH_TYP</summary>
        protected string SqlRetJobs(bool b3F)
        {
            string strSql = "";
            strSql += CRLF + " SELECT LUGG_NO, JOB_TYP, START_POS, START_LOCATION, DEST_POS ";
            strSql += CRLF + "   FROM JOB_MST                                            ";
            strSql += CRLF + "  WHERE WH_TYP           = :WH_TYP                         ";
            strSql += CRLF + "    AND JOB_STATUS       = '" + ST_SC_WAIT + "'            ";   // 20 = SC 구동요구
            if (b3F)
            {
                strSql += CRLF + "    AND ( DEST_POS IN (" + SqlInList(STN_3F_PLT) + ")      ";
                strSql += CRLF + "       OR JOB_TYP  IN (" + JT_IN_A2A + ") )                  ";   // 호기간 이동은 3층으로 (반자동 15 포함)
            }
            else
                strSql += CRLF + "    AND DEST_POS IN (" + SqlInList(STN_1F_PLT) + ")            ";   // 1층으로 나갈 것만
            strSql += CRLF + "  ORDER BY " + SQL_JOB_PRIORITY("") + " DESC, LUGG_NO      ";
            return strSql;
        }

        /// <summary>그 호기와 출고 H/S 가 출고 지시를 받을 수 있는 상태인가. 인자 : HS_NO, WH_TYP, SC_NO</summary>
        protected string SqlRetScHsReady()
        {
            string strSql = "";
            strSql += CRLF + " SELECT SHD.HS_MC_NO                                      ";
            strSql += CRLF + "   FROM SC_DATA SD                                        ";
            strSql += CRLF + "  INNER JOIN SC_HS_DEF SHD                                ";
            strSql += CRLF + "     ON SHD.WH_TYP          = SD.WH_TYP                   ";
            strSql += CRLF + "    AND SHD.SC_NO           = SD.SC_NO                    ";
            strSql += CRLF + "    AND SHD.HS_NO           = :HS_NO                      ";   // 3층 출고 H/S
            strSql += CRLF + "    AND SHD.HS_USE_YN       = 'Y'                         ";
            strSql += CRLF + "  INNER JOIN CV_DATA CD                                   ";
            strSql += CRLF + "     ON CD.WH_TYP           = SD.WH_TYP                   ";
            strSql += CRLF + "    AND CD.MC_NO            = SHD.HS_MC_NO                ";
            strSql += CRLF + "  WHERE SD.WH_TYP           = :WH_TYP                     ";
            strSql += CRLF + "    AND SD.SC_NO            = :SC_NO                      ";
            strSql += SQL_SC_READY;                 // 크레인이 지시를 받을 수 있는 상태인가
            strSql += SQL_SC_RET_NOT_SUSPEND;       // 출고 정지가 아닌가
            // @.메인 폼 "출고 H/S 조건 제외" 가 켜져 있으면 아래 H/S(컨베이어) 조건은 보지 않는다. (크레인 조건만 본다)
            /*
             * @.출고 H/S 조건을 기존 ECS(CSc::RetrieveRoutine(int nRank))와 같게 한다. (2026-11-25)
             *
             *   기존 ECS 가 보는 것은 둘이다.
             *     1) 출고 H/S 준비 신호 (pCvTrackInfo->IsRetHomeStandReady() = PLC 가 올리는 비트 = CV_DATA.RETHS_READY_RD)
             *     2) 앞 화물이 H/S 에서 못 떠나고 있지 않은가 (IsPrevPalletOnRetHS : 그 H/S 로 내린 출고 / 호기간 이동 작업 중
             *        상태가 "CV 구동 요구" 인 것 = 크레인은 끝냈는데 컨베이어 출발 지시가 아직 안 나간 것. 여기서는 29 / 28)
             *   전에는 H/S 의 화물 센서가 꺼지고 작업번호가 지워질 때까지 기다렸다. 그래서 같은 층 출고를 연달아 할 때마다
             *   방금 내려놓은 화물이 빠질 때까지 10 ~ 19 초씩 지시가 늦었다. (2026-11-25 현장 : 1층->1층 265건, 3층->3층 76건)
             *
             *   ENV_IOSCH.INI [SC] RET_HS_LEGACY=N 으로 두면 전의 조건(센서 / 작업번호 / 자동 / 에러)으로 돌아간다. 기본 Y.
             */
            if (cDefApp.GM_RET_SKIP_HS == false && cDefApp.GM_RET_HS_LEGACY == true)
            {
                strSql += CRLF + "    AND CD.RETHS_READY_RD   = '1'                         ";   // 출고 H/S 준비 (PLC)
                strSql += CRLF + "    AND NOT EXISTS (SELECT 1 FROM JOB_MST JP                  ";   // 앞 화물이 H/S 에서 못 떠나고 있지 않다
                strSql += CRLF + "                     WHERE JP.WH_TYP      = SD.WH_TYP        ";
                strSql += CRLF + "                       AND JP.HS_TRACK_NO = SHD.HS_MC_NO     ";
                strSql += CRLF + "                       AND JP.JOB_STATUS IN ('" + ST_SC_DONE + "','" + ST_SC_DONE_A2A_STEP1 + "')";
                strSql += CRLF + "                       AND JP.JOB_TYP    IN (" + JT_IN_RET + "," + JT_IN_PICK + "," + JT_IN_A2A + "))";
            }
            else if (cDefApp.GM_RET_SKIP_HS == false)
            {
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                         ";   // 출고 H/S 가 비어 있다
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0')       IN ('0','0000')            ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                         ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')               ";
            }
            // @.출고 H/S 일시정지는 "출고 H/S 조건 제외" 와 상관없이 항상 본다. (멈춘 H/S 에는 화물을 내려놓지 않는다)
            strSql += SQL_HS_NOT_PAUSED;
            return strSql;
        }

        /// <summary>입고 H/S 에 올라와 크레인이 받을 수 있는 입고 화물. strPlcCond : CV PLC 조건, strScCond : 호기 조건(없으면 "")</summary>
        protected string SqlStoCandidates(string strPlcCond, string strScCond)
        {
            string strSql = "";
            strSql += CRLF + " SELECT JM.LUGG_NO, JM.JOB_TYP, JM.DEST_POS, JM.DEST_LOCATION, CD.MC_NO  ";
            strSql += CRLF + "   FROM JOB_MST JM                                        ";
            strSql += CRLF + "  INNER JOIN CV_DATA CD                                   ";
            strSql += CRLF + "     ON CD.WH_TYP           = JM.WH_TYP                   ";
            strSql += CRLF + "    AND CD.LUGG_NO_RD       = JM.LUGG_NO                  ";
            strSql += CRLF + "  INNER JOIN SC_DATA SD                                   ";
            strSql += CRLF + "     ON SD.WH_TYP           = JM.WH_TYP                   ";
            strSql += CRLF + "    AND SD.SC_NO            = JM.DEST_POS                 ";
            strSql += CRLF + "  WHERE JM.WH_TYP           = :WH_TYP                     ";
            strSql += CRLF + "    AND JM.JOB_TYP          IN (" + JT_IN_STO + "," + JT_IN_A2A + ")  ";   // @.반자동 입고(11)도, 호기간 이동(5/15)의 2단계(도착 호기 입고)도 크레인이 받는다
            strSql += CRLF + "    AND JM.JOB_STATUS       = '" + ST_CV_RUN + "'         ";   // CV 구동중
            strSql += strPlcCond;
            strSql += CRLF + "    AND CD.STOHS_READY_RD   = '1'                         ";   // 입고 H/S 준비
            strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '1'                         ";   // 재하
            strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                         ";
            strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')               ";
            strSql += SQL_HS_NOT_PAUSED;            // 입고 H/S 가 멈춰 있지 않은가
            strSql += SQL_SC_READY;                 // 크레인이 지시를 받을 수 있는 상태인가
            strSql += SQL_SC_STO_NOT_SUSPEND;       // 입고 정지가 아닌가
            // @.트윈이면 두 H/S 를 다 봐야 하므로 한 줄로 끊지 않는다.
            strSql += strScCond;
            strSql += CRLF + "  ORDER BY CD.MC_NO                                       ";
            return strSql;
        }

        /// <summary>이 호기에 지금 지시할 수 있는 입고가 있는가. nRank : 1 = 1층, 3 = 3층</summary>
        protected bool StoHasWork(string strWH_TYP, string strScNo, int nRank)
        {
            // 1층 입고 H/S 는 CV PLC 02 / 05, 3층은 03 / 06 에 있다. (StoHsCheck2 / 5 / 3 / 6 이 보는 PLC)
            string strPlc = (nRank == 1) ? "'02','05'" : "'03','06'";
            string strSql = SqlStoCandidates(CRLF + "    AND CD.PLC_NO           IN (" + strPlc + ")              ",
                                             CRLF + "    AND SD.SC_NO            = :SC_NO                      ");
            _pBdb.mComMain.CommandType = CommandType.Text;
            _pBdb.mComMain.Parameters.Clear();
            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
            _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strScNo;
            return _pBdb.ExcuteQry(strSql) > 0;
        }

        /// <summary>이 호기에 지금 지시할 수 있는 출고가 있는가. nRank : 2 = 1층, 4 = 3층</summary>
        protected bool RetHasWork(string strWH_TYP, string strScNo, int nRank)
        {
            bool b3F = (nRank == 4);

            // 호기와 그 층 출고 H/S 가 받을 수 있는가 (ScRetrieveRoutine 4단계와 같은 조회)
            _pBdb.mComMain.CommandType = CommandType.Text;
            _pBdb.mComMain.Parameters.Clear();
            _pBdb.mComMain.Parameters.Add("HS_NO",  DbLang.VARCHAR).Value = b3F ? HS_NO_RET_3F : HS_NO_RETRIEVE;
            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
            _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strScNo;
            if (_pBdb.ExcuteQry(SqlRetScHsReady()) <= 0)
                return false;

            // 이 호기에서 그 층으로 나갈 출고 작업이 있는가 (ScRetrieveRoutine 1 ~ 3단계와 같은 기준)
            _pBdb.mComMain.CommandType = CommandType.Text;
            _pBdb.mComMain.Parameters.Clear();
            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
            if (_pBdb.ExcuteQry(SqlRetJobs(b3F)) <= 0)
                return false;
            DataTable dtJob = _pBdb.mDtMain.Copy();

            int nCnt202 = 0, nCntEtc = 0;
            string strMsg = "";
            bool bLimitOk = b3F && Count202Picking(strWH_TYP, ref nCnt202, ref nCntEtc, ref strMsg);
            int nLimit202 = cDefApi.GsGetLimitStn202Picking();

            for (int i = 0; i < dtJob.Rows.Count; i++)
            {
                if (IsRetJobType(dtJob.Rows[i]["JOB_TYP"].ToString()) == false)
                    continue;
                if (bLimitOk && (dtJob.Rows[i]["DEST_POS"].ToString() == STN_202) && (nCntEtc > 0) && (nCnt202 >= nLimit202))
                    continue;
                string strSc = dtJob.Rows[i]["START_POS"].ToString();
                if (IsScNo(strSc) == false)
                {
                    if (GetScNoByLocation(dtJob.Rows[i]["START_LOCATION"].ToString(), ref strSc) == false)
                        continue;
                }
                if (strSc.Trim() == strScNo.Trim())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 이 후보(입고 / 출고, 랭크)를 지금 지시해도 되는가. 호출한 쪽은 "이 호기가 비어 있고 이 후보의 일감이 실제로 있다" 는 것을 이미 확인한 상태다.
        ///   구 ECS 시도 순서로 나보다 앞선 후보에 지금 지시할 수 있는 일감이 있으면 양보한다. (DB 로 직접 확인)
        /// </summary>
        protected bool TurnAllowedDb(string strWH_TYP, string strScNo, bool bStore, int nRank)
        {
            if (!cDefApp.GM_SC_ALTERNATE) return true;

            int me = TurnIdx(bStore, nRank);
            int p;
            int[] aP = new int[4];
            string strPrev;
            lock (s_craneTurn)
            {
                CraneTurnState st = TurnState(strScNo);
                p = TurnRankP(st, bStore, nRank);
                for (int i = 0; i < 4; i++)
                    aP[i] = TurnRankP(st, (i < 2), (i == 0) ? 1 : (i == 1) ? 3 : (i == 2) ? 2 : 4);
                strPrev = (st.Prev == 1) ? "STO" : "RET";
            }

            // 나보다 앞선 후보를 앞선 순서대로 확인한다
            string strBlocker = "";
            try
            {
                for (int nOrder = 0; nOrder < p && strBlocker == ""; nOrder++)
                {
                    for (int i = 0; i < 4 && strBlocker == ""; i++)
                    {
                        if (i == me || aP[i] != nOrder) continue;
                        bool bSto = (i < 2);
                        int  nRk  = (i == 0) ? 1 : (i == 1) ? 3 : (i == 2) ? 2 : 4;
                        bool bHas = bSto ? StoHasWork(strWH_TYP, strScNo, nRk) : RetHasWork(strWH_TYP, strScNo, nRk);
                        if (bHas) strBlocker = (bSto ? "STO" : "RET") + nRk;
                    }
                }
            }
            catch (Exception ex)
            {
                // 확인을 못 했으면 막지 않는다
                TurnLog(strScNo + " CHECK_ERR " + ex.Message);
                strBlocker = "";
            }

            lock (s_craneTurn)
            {
                CraneTurnState st = TurnState(strScNo);
                int now = Environment.TickCount; if (now == 0) now = 1;
                if (strBlocker == "") { st.DeferStart = 0; return true; }

                if (st.DeferStart == 0)
                {
                    st.DeferStart = now;
                    TurnLog(strScNo + " DEFER " + (bStore ? "STO" : "RET") + nRank + " p=" + p + " (먼저 지시할 일감 있음: " + strBlocker + ", 직전 " + strPrev + ")");
                }
                if (unchecked(now - st.DeferStart) > TURN_DEFER_MAX_MS)
                {
                    TurnLog(strScNo + " GO_TIMEOUT " + (bStore ? "STO" : "RET") + nRank + " (" + strBlocker + " 가 " + TURN_DEFER_MAX_MS + "ms 넘게 안 나가 먼저 지시)");
                    st.DeferStart = 0;
                    return true;
                }
                return false;
            }
        }

        protected static void TurnDone(string strScNo, bool bStore, int nRank)
        {
            lock (s_craneTurn)
            {
                CraneTurnState s;
                if (!s_craneTurn.TryGetValue(strScNo, out s)) { s = new CraneTurnState(); s_craneTurn[strScNo] = s; }
                if (bStore) { s.Prev = 1; s.StoPrev = nRank; } else { s.Prev = 2; s.RetPrev = nRank; }
                s.DeferStart = 0;
                for (int i = 0; i < 4; i++) s.Pending[i] = 0;   // 이 호기는 이제 일을 받았다 - 다음 점검에서 다시 표시한다
                TurnLog(strScNo + " DONE " + (bStore ? "STO" : "RET") + nRank);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 1F / 3F 공통 출고 지시에 쓰는 층별 값 (예전에는 각 층 스레드 클래스가 따로 가졌다)
        // ─────────────────────────────────────────────────────────────────
        protected enum EN_RET_FLOOR { F1, F3, BOX }   // BOX 는 CV_RETHS_PLC 만 쓴다 (BOX 출고 지시는 cThread_SCH_BOX 의 자체 루틴)

        // SC_HS_DEF.HS_NO : 3층 출고 H/S (1층은 HS_NO_RETRIEVE '02', 3층은 '04')
        protected const string HS_NO_RET_3F = "04";

        // SC_DATA 에 실을 H/S 자리 번호 = 레거시 RANK 값 그대로
        protected const string HSPOS_STO_1F = "1";   // RANK_1 (D176 입고 출발 H/S)
        protected const string HSPOS_RET_1F = "2";   // RANK_2 (D180 출고 도착 H/S)
        protected const string HSPOS_STO_3F = "3";   // RANK_3 (D176 입고 출발 H/S)
        protected const string HSPOS_RET_3F = "4";   // RANK_4 (D180 출고 도착 H/S)

        /*
         * 1층 출고 작업대 - 레거시 CLib::GetRank (Lib.cpp:2343) 가 RANK_2 를 돌려주는 자리.
         *   출고 작업이 1층으로 나갈지 3층으로 나갈지는 도착 작업대 하나로 정해진다.
         *   3층 PLT 와 BOX 는 각각 3F / BOX 스레드가 맡는다. 호기간 이동은 목적지를 보지 않고 3층으로 뺀다 (레거시 2011.02.06 수정).
         */
        protected static readonly string[] STN_1F_PLT = { "103", "104", "105" };

        // 3층 PLT 출고 작업대 - 레거시 CLib::GetRank 가 RANK_4 를 돌려주는 자리
        //   200~209 (구라인) + 212~215 (신라인). 211/221/222/231/241/242/251 은 BOX 라인이라 여기서 잡지 않는다.
        protected static readonly string[] STN_3F_PLT =
            { "200", "201", "202", "203", "204", "205", "206", "207",
              "208", "209", "212", "213", "214", "215" };

        // 202 피킹대 쏠림 제한에 쓰는 작업대
        protected const string STN_202 = "202";
        protected const string STN_201 = "201";
        protected const string STN_203 = "203";

        // ─────────────────────────────────────────────────────────────────
        // 출고 지시 (20 -> 21) - 레거시 CSc::RetrieveRoutine(1층 RANK_2 / 3층 RANK_4) + Retrieve   [1F / 3F 공통]
        //   층마다 다른 것은 eFloor 로 가른다. (BOX 는 트윈 포크 짝 맞추기가 있어 따로 둔다 - cThread_SCH_BOX)
        //     1층 : 도착이 STN_1F_PLT 인 작업, 출고 H/S HS_NO_RETRIEVE, 크레인 자리 HSPOS_*_1F, 차례 랭크 2
        //     3층 : 도착이 STN_3F_PLT 인 작업 + 호기간 이동, 출고 H/S HS_NO_RET_3F, 자리 HSPOS_*_3F, 차례 랭크 4,
        //           202 피킹대 쏠림 제한, 호기간 이동 로그
        //   상위가 넣은 출고 작업 중 3층으로 나갈 것만 골라 크레인에 넘긴다.
        //     꺼낼 곳     : JM.START_LOCATION (랙 위치)
        //     내려놓을 곳 : 그 호기의 3층 출고 H/S (SC_HS_DEF.HS_NO='04')
        //     크레인      : JM.START_POS (출고 작업은 출발지가 호기다)
        //   출고 H/S 가 비어 있어야 크레인이 내려놓을 수 있다.
        //   이어받는 곳은 CV_RETHS_PLC 다. 그쪽이 JM.HS_TRACK_NO 로 트랙을 찾으므로
        //   여기서 그 값을 채워 준다.
        // ─────────────────────────────────────────────────────────────────
        protected bool ScRetrieveRoutine(string strWH_TYP, string strHS_NO, EN_RET_FLOOR eFloor, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                bool   b3F         = (eFloor == EN_RET_FLOOR.F3);
                int    nTurnRank   = b3F ? 4 : 2;                                  // 구 ECS 차례 랭크 (출고 1층 2 / 3층 4)
                string strHsposSto = b3F ? HSPOS_STO_3F : HSPOS_STO_1F;
                string strHsposRet = b3F ? HSPOS_RET_3F : HSPOS_RET_1F;
                string strFlr      = b3F ? "3층 " : "";                             // 메시지용
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                // ── 1) 3층으로 나갈 출고 작업만 고른다 (호기는 아직 모른다)
                //      레거시 CLib::GetRank 가 RANK_4 를 주는 작업대 + 호기간 이동
                strSql = SqlRetJobs(b3F);

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                DataTable dtJob = _pBdb.mDtMain.Copy();

                // ── 2) 202 피킹대 쏠림 제한에 쓸 개수를 미리 센다
                //      (레거시 CJob::FetchScRetJobByScNumberNPriority 의 앞부분)
                //
                // @.쏠림 제한은 부수 판정이다. 개수를 못 셌다고 3층 출고 지시를
                //   통째로 접지 않는다. 전에는 여기서 바로 return false 라, 이 현장에서
                //   발동하지도 않는 제한(피킹 작업 0건)의 DB 오류 하나가 그 주기의
                //   3층 출고를 통째로 멈췄다. 사유만 남기고 제한 없이 간다.
                int nCnt202 = 0, nCntEtc = 0;
                bool bLimitOk = b3F && Count202Picking(strWH_TYP, ref nCnt202, ref nCntEtc, ref pRTN_MSG);   // 1층은 제한 없음
                if (b3F && bLimitOk == false)
                {
                    MakeMsg_Error_NoLog(pRTN_MSG + " - 202 제한을 세지 못했다. 이번 주기는 제한 없이 간다");
                    pRTN_MSG = strTitle;
                }
                int nLimit202 = cDefApi.GsGetLimitStn202Picking();

                string strLUGG_NO = "", strJOB_TYP = "", strSC_NO = "", strSTART_LOC = "";
                string strDEST_POS = "", strHS_MC_NO = "";
                bool bFound = false;
                // @.같은 호기는 한 번만 본다. (1층과 같다) 대기 작업이 수천 건일 때 건마다 같은 호기 조회를 되풀이해 한 바퀴가 1~2초 걸렸다.
                //   호기/H/S 준비 상태는 호기에만 달려 있으므로 결과가 같다.
                System.Collections.Generic.HashSet<string> setNotReady = new System.Collections.Generic.HashSet<string>();

                for (int i = 0; i < dtJob.Rows.Count; i++)
                {
                    strLUGG_NO   = dtJob.Rows[i]["LUGG_NO"].ToString();
                    strJOB_TYP   = dtJob.Rows[i]["JOB_TYP"].ToString();
                    strSTART_LOC = dtJob.Rows[i]["START_LOCATION"].ToString();
                    strDEST_POS  = dtJob.Rows[i]["DEST_POS"].ToString();

                    if (IsRetJobType(strJOB_TYP) == false)
                        continue;

                    // 202 로 갈 작업인데 201/203 에도 대기가 있고 진행 중인 202 가 한도 이상이면 건너뛴다
                    //   (개수를 못 센 주기에는 걸지 않는다. 한도 기본값이 0 이라
                    //    0 으로 두고 넘어가면 202 출고가 오히려 전부 막힌다)
                    if (bLimitOk && (strDEST_POS == STN_202) && (nCntEtc > 0) && (nCnt202 >= nLimit202))
                        continue;

                    // ── 3) 호기를 정한다. 상위가 호기를 줬으면 그대로, 아니면 랙 뱅크에서 구한다.
                    strSC_NO = dtJob.Rows[i]["START_POS"].ToString();
                    if (IsScNo(strSC_NO) == false)
                    {
                        if (GetScNoByLocation(strSTART_LOC, ref strSC_NO) == false)
                            continue;
                    }

                    if (setNotReady.Contains(strSC_NO))
                        continue;

                    // ── 4) 그 호기와 3층 출고 H/S 가 지시를 받을 수 있는 상태인지 본다
                    strSql = SqlRetScHsReady();

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("HS_NO",  DbLang.VARCHAR).Value = strHS_NO;
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strSC_NO;

                    nSelCnt = _pBdb.ExcuteQry(strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }
                    if (nSelCnt == 0)
                    {
                        setNotReady.Add(strSC_NO);
                        TurnUnmark(strSC_NO, false, nTurnRank);
                        continue;   // 아직 받을 수 없다
                    }

                    // 차례 확인이 DB 를 다시 읽으므로 H/S 트랙을 먼저 꺼내 둔다
                    strHS_MC_NO = _pBdb.mDtMain.Rows[0]["HS_MC_NO"].ToString();

                    // @.구 ECS 처럼 호기마다 입고/출고, 1층/3층 차례를 지킨다. (차례가 아니면 이 호기는 이번 주기에 건너뛴다)
                    if (!TurnAllowedDb(strWH_TYP, strSC_NO, false, nTurnRank))
                        continue;

                    bFound = true;
                    break;
                }

                if (bFound == false)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();

                if (UpdateScData(strSC_NO, JT_RET, strLUGG_NO, strSTART_LOC, "",
                                 strHsposSto, strHsposRet, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 크레인 미준비 - 다음 사이클 재시도
                }

                // 이어받을 CV_RETHS_PLC 가 HS_TRACK_NO 로 3층 출고 H/S 트랙을 찾는다
                if (UPDATE_JOB_DATA(ST_SC_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG,
                                    "0", "0", strHS_MC_NO) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + strSC_NO + "호기에 " + strFlr + "출고를 지시하였습니다. [작업번호:" + strLUGG_NO
                         + "][출발LOC:" + strSTART_LOC + "][출고H/S:" + strHS_MC_NO + "][도착지:" + strDEST_POS + "]";
                _pBdb.Commit();
                TurnDone(strSC_NO, false, nTurnRank);
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_SC_RUN, strHS_MC_NO, strSC_NO);
                if (b3F && IsA2AJobType(strJOB_TYP))
                    MakeRackMsg("[호기간 이동 2/6] " + strSC_NO + "호기에 호기간 이동(출고)을 지시하였습니다. [작업번호:" + strLUGG_NO + "][출발LOC:" + strSTART_LOC + "][3층 출고 H/S TRACK " + strHS_MC_NO + "][도착 호기:" + strDEST_POS + "]", cDefApp.eLogMsgType.MSG_IMP);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }


        /*
         * Count202Picking :: 202 피킹대 쏠림 제한에 쓰는 두 개수를 센다.
         *
         *   레거시 CJob::FetchScRetJobByScNumberNPriority (Job.cpp:1002) 앞부분 그대로다.
         *     nCount202 : 도착지가 202 인 피킹 작업 중 "이미 진행 중인" 것
         *                 (레거시는 상태가 JOB_STA_SC_OPER_REQUEST 가 아닌 것 = 여기서는 20 이 아닌 것)
         *                 + 202 트랙에 작업목록에 없는 피킹 화물이 얹혀 있으면 하나 더
         *     nCountEtc : 도착지가 201 또는 203 인 피킹 작업 수
         *   둘 다 피킹(JOB_TYP='3') 작업만 센다.
         */
        protected bool Count202Picking(string strWH_TYP, ref int nCnt202, ref int nCntEtc, ref string pRTN_MSG)
        {
            nCnt202 = 0;
            nCntEtc = 0;

            string strSql = "";
            strSql += CRLF + " SELECT                                                        ";
            strSql += CRLF + "   ( SELECT COUNT(*) FROM JOB_MST J                            ";
            strSql += CRLF + "      WHERE J.WH_TYP     = :WH_TYP                             ";
            strSql += CRLF + "        AND J.JOB_TYP    IN (" + JT_IN_PICK + ")                 ";
            strSql += CRLF + "        AND J.DEST_POS   = '" + STN_202 + "'                   ";
            strSql += CRLF + "        AND J.JOB_STATUS <> '" + ST_SC_WAIT + "' )             ";
            strSql += CRLF + " + ( SELECT COUNT(*) FROM CV_DATA CD                           ";
            strSql += CRLF + "      WHERE CD.WH_TYP    = :WH_TYP                             ";
            strSql += CRLF + "        AND CD.MC_NO     = '" + STN_202 + "'                   ";
            strSql += CRLF + "        AND CD.JOB_TYP_RD IN (" + JT_IN_PICK + ")                ";
            strSql += CRLF + "        AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0') NOT IN ('0','0000') ";
            strSql += CRLF + "        AND NOT EXISTS ( SELECT 1 FROM JOB_MST J2              ";
            strSql += CRLF + "                          WHERE J2.WH_TYP  = CD.WH_TYP         ";
            strSql += CRLF + "                            AND J2.LUGG_NO = CD.LUGG_NO_RD ) ) AS CNT202 ";
            strSql += CRLF + " , ( SELECT COUNT(*) FROM JOB_MST J                            ";
            strSql += CRLF + "      WHERE J.WH_TYP     = :WH_TYP                             ";
            strSql += CRLF + "        AND J.JOB_TYP    IN (" + JT_IN_PICK + ")                 ";
            strSql += CRLF + "        AND J.DEST_POS   IN ('" + STN_201 + "','" + STN_203 + "') ) AS CNTETC ";

            _pBdb.mComMain.CommandType = CommandType.Text;
            _pBdb.mComMain.Parameters.Clear();
            _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

            int nSelCnt = _pBdb.ExcuteQry(strSql);
            if (nSelCnt < 0)
            {
                pRTN_MSG += _pBdb.ErrMsg;
                return false;
            }
            if (nSelCnt == 0) return true;

            int.TryParse(GetVal(_pBdb.mDtMain.Rows[0], "cnt202"), out nCnt202);
            int.TryParse(GetVal(_pBdb.mDtMain.Rows[0], "cntetc"), out nCntEtc);
            return true;
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 5 : 입고 H/S 도착 -> 크레인 입고 지시 (레거시 ECS CSc::Store)
        //   화물이 크레인 입고 H/S 에 올라오면 그 크레인에 입고를 지시한다.
        //     지시 대상 크레인 : JOB_MST.DEST_POS (901~911)
        //       상위는 전문에 호기를 1~11 로 보내지만, HOST_TASK 가 받으면서
        //       9xx 로 바꿔 넣는다(CSrvWork.cs "Convert S/C No"). 그래서 여기서
        //       SC_DATA.SC_NO 와 그대로 짝지어진다.
        //     넣을 랙 위치     : JOB_MST.DEST_LOCATION (상위가 준 값)
        //   입고 H/S 인지는 CV_DATA.STOHS_READY_RD 로 안다. 그 값은 CV 태스크가
        //   DeviceMap 의 ScStoHS 영역에서 읽어 채운다.
        //
        //   층마다 다른 것은 CV PLC 와 H/S 자리 번호(레거시 RANK) 둘뿐이라
        //   그것만 인자로 받는다. 전에는 1F / 3F / BOX 클래스에 거의 같은 본체가
        //   세 벌 있어, 한 쪽만 고치면 조용히 어긋났다.
        // ─────────────────────────────────────────────────────────────────
        /*
         * strStoHsFk1 / strStoHsFk2 : 이 랭크의 입고 H/S 트랙 둘.
         *
         *   트윈 포크 크레인은 같은 랭크에 H/S 가 둘이다. 포크#1 것과 포크#2 것이
         *   따로 있다. 전문에는 랭크만 실리고 어느 쪽인지는 USE_FK 가 가린다.
         *   그래서 화물이 어느 쪽에 올라왔는지를 보고 포크를 정해야 한다.
         *     (기존 ECS : CLib::GetScHSTrackPerRank - 같은 랭크에 Single/Twin 두 트랙)
         *     P-BOX 는 입고 420(포크#1) / 419(포크#2) 다.
         *
         *   비워 두면 예전처럼 포크#1 하나로만 간다. 싱글 포크 호기(1층/3층 PLT)는
         *   비워서 부른다.
         */
        protected bool ScStoreRoutine(string strWH_TYP, string strCV_PLC, string strStoHsPos, string strRetHsPos, string strTitle, ref string pRTN_MSG,
                                      string strStoHsFk1 = "", string strStoHsFk2 = "")
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = SqlStoCandidates(CRLF + "    AND CD.PLC_NO           = :CV_PLC                     ", "");

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;

                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                // @.이 점검이 호기 전체를 다시 훑으므로 이 랭크의 "일감 있음" 표시를 먼저 지우고 지금 있는 것만 다시 표시한다.
                if (strStoHsPos == "1") TurnUnmarkAll(true, 1); else if (strStoHsPos == "3") TurnUnmarkAll(true, 3);
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                DataTable dtSto = _pBdb.mDtMain.Copy();

                // @.구 ECS 처럼 호기마다 차례를 지킨다. (1층 입고 랭크 1 / 3층 입고 랭크 3. 그 밖(BOX 등)은 건드리지 않는다)
                int nTurnRank = (strStoHsPos == "1") ? 1 : ((strStoHsPos == "3") ? 3 : 0);
                if (nTurnRank > 0 && cDefApp.GM_SC_ALTERNATE)
                {
                    DataTable dtTurn = dtSto.Clone();
                    for (int r = 0; r < dtSto.Rows.Count; r++)
                        if (TurnAllowedDb(strWH_TYP, dtSto.Rows[r]["DEST_POS"].ToString().Trim(), true, nTurnRank)) dtTurn.ImportRow(dtSto.Rows[r]);
                    if (dtTurn.Rows.Count == 0) { pRTN_MSG = ""; return true; }   // 다른 쪽 차례다
                    dtSto = dtTurn;
                }

                /*
                 * 어느 H/S 에 올라와 있는지로 포크를 가린다.
                 *
                 *   포크#1 H/S(420) 것만        -> 포크#1 단독 (USE_FK 0)
                 *   양쪽(420,419)에 다 있으면   -> 트윈       (USE_FK 1)
                 *
                 * 포크#2 단독은 없다. 419 는 420 이 찬 뒤에 쓰이는 뒷자리이므로,
                 * 419 에만 화물이 있는 것은 정상 상태가 아니다. 그때는 지시하지
                 * 않고 사유를 남긴다. 포크#1 로 지시해 버리면 크레인이 420 으로
                 * 가서 빈 자리를 집으려 한다.
                 */
                int nRowFk1 = -1, nRowFk2 = -1;

                bool bTwinHs = (strStoHsFk1 != "") && (strStoHsFk2 != "");

                for (int i = 0; i < dtSto.Rows.Count; i++)
                {
                    string strMc = dtSto.Rows[i]["MC_NO"].ToString().Trim();

                    if (bTwinHs && (strMc == strStoHsFk2))
                    {
                        if (nRowFk2 < 0) nRowFk2 = i;
                    }
                    else if (bTwinHs && (strMc == strStoHsFk1))
                    {
                        if (nRowFk1 < 0) nRowFk1 = i;
                    }
                    else if (!bTwinHs)
                    {
                        if (nRowFk1 < 0) nRowFk1 = i;      // 싱글 포크 호기 - 예전 그대로
                    }
                }

                // 아무 쪽도 못 가렸으면 맨 앞을 포크#1 로 본다.
                if ((nRowFk1 < 0) && (nRowFk2 < 0))
                    nRowFk1 = 0;

                /*
                 * 포크#2 쪽에만 있으면 지시하지 않는다.
                 *   사유는 한 번만 적는다. 다음 주기에 420 이 차면 트윈으로 나간다.
                 */
                if (nRowFk1 < 0)
                {
                    pRTN_MSG = strTitle + "포크#2 쪽 입고 H/S(TRACK " + strStoHsFk2 + ")에만 화물이 있습니다."
                             + " 포크#2 단독 작업은 없으므로 포크#1 쪽(TRACK " + strStoHsFk1 + ")이 찰 때까지 기다립니다.";
                    return true;
                }

                /*
                 * 포크#1(420) 은 준비됐는데 포크#2(419) 자리에도 화물이 있고 그 작업이
                 * 곧 준비될 참이면(작업이 CV 구동대기/구동중이거나 지시가 쓰이는 중) 기다린다.
                 *   420 의 입고 H/S 는 목적지가 쓰이면 바로 서므로, 419 에 아직 안 쓰인
                 *   그 틈에 단독으로 내보내면 트윈이 영영 안 묶인다. (원본 CSc::StoreRoutine 과 같다)
                 *   419 화물에 작업이 아예 없으면 기다리지 않고 단독으로 간다.
                 */
                if (bTwinHs && (nRowFk1 >= 0) && (nRowFk2 < 0))
                {
                    string strSqlWait = "";
                    strSqlWait += CRLF + " SELECT COUNT(1) AS CNT                                            ";
                    strSqlWait += CRLF + "   FROM CV_DATA CD                                                ";
                    strSqlWait += CRLF + "   LEFT OUTER JOIN JOB_MST JM                                     ";
                    strSqlWait += CRLF + "     ON JM.WH_TYP  = CD.WH_TYP                                    ";
                    strSqlWait += CRLF + "    AND JM.LUGG_NO = CD.LUGG_NO_RD                                ";
                    strSqlWait += CRLF + "  WHERE CD.WH_TYP          = :WH_TYP                              ";
                    strSqlWait += CRLF + "    AND CD.PLC_NO          = :CV_PLC                              ";
                    strSqlWait += CRLF + "    AND CD.MC_NO           = :MC_NO2                              ";
                    strSqlWait += CRLF + "    AND CD.SENSOR0_DATA_RD = '1'                                 ";
                    // @.419 화물에 아직 작업번호가 안 쓰였어도, 그 자리(HOST_STN_NO)를 출발지로 둔
                    //   입고 작업이 CV 구동대기/구동중이면 곧 쓰일 것이므로 같이 기다린다.
                    strSqlWait += CRLF + "    AND (   JM.JOB_STATUS IN ('" + ST_CV_WAIT + "','" + ST_CV_RUN + "')";
                    strSqlWait += CRLF + "         OR CD.OD_RQ_YN = 'Y'";
                    strSqlWait += CRLF + "         OR EXISTS (SELECT 1 FROM JOB_MST J2";
                    strSqlWait += CRLF + "                     WHERE J2.WH_TYP = CD.WH_TYP AND J2.START_POS = CD.HOST_STN_NO";
                    strSqlWait += CRLF + "                       AND J2.JOB_STATUS IN ('" + ST_CV_WAIT + "','" + ST_CV_RUN + "'))";
                    // @.두 화물이 같이 도착해 완료보고가 같이 나가면, HOST 가 입고 요구에 답하는 데 2~3초가
                    //   걸려 420 쪽 입고 작업이 먼저 쓰인다. 그 사이 419 화물에는 아직 작업이 없다.
                    //   (현장 11:23:59 둘 다 도착 -> 11:24:01 420 입고 접수 -> 11:24:02 단독 지시 -> 11:24:03 419 입고 접수)
                    //   그래서 419 화물의 이동 작업이 아직 JOB_MST 에 남아 있거나(완료보고 처리 중),
                    //   420 쪽 입고 작업이 접수된 지 10초가 안 됐으면 419 작업을 조금 더 기다린다.
                    //   10초가 지나도 안 오면 단독으로 간다. (영영 기다리지 않는다)
                    strSqlWait += CRLF + "         OR (JM.LUGG_NO IS NOT NULL AND JM.JOB_TYP = '6')";
                    strSqlWait += CRLF + "         OR EXISTS (SELECT 1 FROM JOB_MST J3";
                    strSqlWait += CRLF + "                     WHERE J3.WH_TYP = CD.WH_TYP AND J3.LUGG_NO = :LUGG1";
                    strSqlWait += CRLF + "                       AND J3.INS_DT > " + DbLang.SYSDATE + " - INTERVAL '10 second'))";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;
                    _pBdb.mComMain.Parameters.Add("MC_NO2", DbLang.VARCHAR).Value = strStoHsFk2;
                    _pBdb.mComMain.Parameters.Add("LUGG1", DbLang.VARCHAR).Value = dtSto.Rows[nRowFk1]["LUGG_NO"].ToString();
                    int nWaitCnt = _pBdb.ExcuteQry(strSqlWait);
                    if (nWaitCnt > 0 && Convert.ToInt32(_pBdb.mDtMain.Rows[0]["CNT"]) > 0)
                    {
                        pRTN_MSG = strTitle + "포크#2 쪽 입고 H/S(TRACK " + strStoHsFk2 + ")의 화물도 곧 준비됩니다."
                                 + " 트윈으로 묶으려고 이번 주기는 지시하지 않습니다.";
                        return true;
                    }
                }

                bool bStoTwin = (nRowFk1 >= 0) && (nRowFk2 >= 0);

                int nRowMain = nRowFk1;

                string strLUGG_NO  = dtSto.Rows[nRowMain]["LUGG_NO"].ToString();
                string strSC_NO    = dtSto.Rows[nRowMain]["DEST_POS"].ToString();
                string strDEST_LOC = dtSto.Rows[nRowMain]["DEST_LOCATION"].ToString();
                string strMC_NO    = dtSto.Rows[nRowMain]["MC_NO"].ToString();

                string strLUGG_NO2 = "0";
                string strDEST_LOC2 = "";
                string strMC_NO2 = "";

                if (bStoTwin)
                {
                    // 두 작업이 같은 호기여야 한 번에 간다.
                    if (dtSto.Rows[nRowFk2]["DEST_POS"].ToString() == strSC_NO)
                    {
                        strLUGG_NO2  = dtSto.Rows[nRowFk2]["LUGG_NO"].ToString();
                        strDEST_LOC2 = dtSto.Rows[nRowFk2]["DEST_LOCATION"].ToString();
                        strMC_NO2    = dtSto.Rows[nRowFk2]["MC_NO"].ToString();

                        // 같은 칸이면 짝이 될 수 없다.
                        if (strDEST_LOC2 == strDEST_LOC)
                        {
                            strLUGG_NO2 = "0"; strDEST_LOC2 = ""; strMC_NO2 = "";
                            bStoTwin = false;
                        }
                    }
                    else
                    {
                        bStoTwin = false;
                    }
                }

                _pBdb.BeginTrans();

                if (UpdateScData(strSC_NO, JT_STO, strLUGG_NO, "", strDEST_LOC,
                                 strStoHsPos, strRetHsPos, ref pRTN_MSG,
                                 strLUGG_NO2, "", strDEST_LOC2) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 크레인 미준비 - 다음 사이클 재시도
                }

                if (UPDATE_JOB_DATA(ST_SC_RUN, strLUGG_NO, strWH_TYP, JT_STO, ref pRTN_MSG,
                                    "0", "0", "0", "0", "0", strLUGG_NO2) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                // 짝도 같이 돌린다. 한쪽만 돌리면 다음 주기에 또 지시된다.
                if (strLUGG_NO2 != "0")
                {
                    if (UPDATE_JOB_DATA(ST_SC_RUN, strLUGG_NO2, strWH_TYP, JT_STO, ref pRTN_MSG,
                                        "0", "0", "0", "0", "0", strLUGG_NO) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                }

                if (strLUGG_NO2 != "0")
                {
                    pRTN_MSG = strTitle + strSC_NO + "호기에 입고를 트윈(포크#1,#2)으로 지시하였습니다."
                             + " [포크#1 TRACK " + strMC_NO  + " 작업번호:" + strLUGG_NO  + " 도착LOC:" + strDEST_LOC  + "]"
                             + " [포크#2 TRACK " + strMC_NO2 + " 작업번호:" + strLUGG_NO2 + " 도착LOC:" + strDEST_LOC2 + "]";
                }
                else
                {
                    pRTN_MSG = strTitle + "TRACK " + strMC_NO + "번[입고 H/S]에서 SC_TASK를 통해서 "
                             + strSC_NO + "호기에 입고 지시하였습니다. [작업번호:" + strLUGG_NO + "][도착LOC:" + strDEST_LOC + "][포크#1]";
                }
                _pBdb.Commit();
                if (nTurnRank > 0) TurnDone(strSC_NO, true, nTurnRank);
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_SC_RUN, strMC_NO, strSC_NO);
                if (IsA2AJobType(dtSto.Rows[nRowMain]["JOB_TYP"].ToString().Trim()))
                    MakeRackMsg("[호기간 이동 5/6] CV TRACK " + strMC_NO + "[3층 입고 H/S]에서 " + strSC_NO + "호기에 호기간 이동(입고)을 지시하였습니다. [작업번호:" + strLUGG_NO + "][도착LOC:" + strDEST_LOC + "]", cDefApp.eLogMsgType.MSG_IMP);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }








        /// <summary>
        /// SC_DATA 이송 명령 지시 (레거시 ECS CSc::Store / CSc::Retrieve 의 명령 데이터 포팅)
        ///   레거시 Melsec 명령 D171~D192 : 명령(1=입고/2=출고), 화물번호, BANK/BAY/LEVEL, HS(=랭크)
        ///   - 입고(Store)    : 출발 = 입고 H/S(strStoHsPos), 도착 = 랙 위치(DEST_LOCATION 분해)
        ///   - 출고(Retrieve) : 출발 = 랙 위치(START_LOCATION 분해), 도착 = 출고 H/S(strRetHsPos)
        ///   위치 문자열 = BANK(2) + BAY(3) + LEVEL(2)  (레거시 LOCATION_LEN=7)
        ///   H/S 자리 번호는 곧 랭크다. 1층이면 1/2, 3층이면 3/4 를 넣는다.
        ///   (레거시 CSc::GetScSelfStoHS / GetScSelfRetHS 가 랭크를 그대로 돌려준다)
        ///   FK1 단일포크 사용, WRITE_FLAG_OD='1' = 명령 기록 완료 플래그.
        /// </summary>
        protected bool UpdateScData(
                    string strScNo
            ,       string strJobTyp
            ,       string strLuggNo
            ,       string strStartLoc
            ,       string strDestLoc
            ,       string strStoHsPos
            ,       string strRetHsPos
            , ref   string strRtn
            /*
             * 포크#2. 트윈 포크 크레인이 한 번에 두 개를 집을 때만 쓴다.
             *   strLuggNo2 가 "0" 이거나 비면 예전처럼 포크#1 하나로 간다.
             *   (기존 ECS : EcsSv/Sc.cpp CSc::Retrieve 의 nLuggNumForkTwo)
             */
            ,       string strLuggNo2   = "0"
            ,       string strStartLoc2 = ""
            ,       string strDestLoc2  = ""
            )
        {
            try
            {
                bool bStore = (strJobTyp == JT_STO);
                bool bR2R   = (strJobTyp == JT_R2R);     // 랙투랙 : 같은 호기 안에서 랙 -> 랙 (D171=4, 출발/도착 모두 랙)

                // 랙 위치 분해 (입고는 도착, 출고는 출발이 랙)
                string strBank = "0", strBay = "0", strLev = "0";
                string strLoc = bStore ? strDestLoc : strStartLoc;
                if (!ParseLocation(strLoc, ref strBank, ref strBay, ref strLev))
                {
                    strRtn += "랙 위치 형식 오류(LOC:" + strLoc + ")";
                    return false;
                }

                // 랙투랙은 도착도 랙이다. (입고는 도착, 출고는 출발만 랙)
                string strDBank = "0", strDBay = "0", strDLev = "0";
                if (bR2R && !ParseLocation(strDestLoc, ref strDBank, ref strDBay, ref strDLev))
                {
                    strRtn += "랙투랙 도착 랙 위치 형식 오류(LOC:" + strDestLoc + ")";
                    return false;
                }

                /*
                 * 포크#2 가 있는가.
                 *
                 *   있으면 USE_FK_OD 를 '1'(양포크)로 주고 FK2 자리를 함께 적는다.
                 *   없으면 예전과 똑같이 '0'(포크1) 으로 가고 FK2 는 손대지 않는다.
                 */
                bool bTwin = false;
                string strBank2 = "0", strBay2 = "0", strLev2 = "0";

                /*
                 * 쓸 포크를 정한다. USE_FK_OD(D181) 의 뜻은 SC 태스크 주석과 같다.
                 *     0 = 포크#1 만 / 1 = 포크#1,#2 동시 / 2 = 포크#2 만
                 *
                 * 포크#2 단독은 없다. 짝이 있으면 1, 없으면 0 이다.
                 *   입고 : 포크#1 단독, 또는 포크#1,#2 트윈
                 *   출고 : 단독도 트윈도 포크#1 에서 실행한다
                 * 그래서 2 는 쓰지 않는다.
                 */
                string strUseFk = "0";

                if (!string.IsNullOrEmpty(strLuggNo2) && strLuggNo2.Trim() != "0" && strLuggNo2.Trim() != "")
                {
                    string strLoc2 = bStore ? strDestLoc2 : strStartLoc2;
                    if (!ParseLocation(strLoc2, ref strBank2, ref strBay2, ref strLev2))
                    {
                        strRtn += "포크#2 랙 위치 형식 오류(LOC:" + strLoc2 + ")";
                        return false;
                    }

                    bTwin = true;
                    strUseFk = "1";
                }


                string strSql = "";
                strSql += CRLF + " UPDATE SC_DATA                                        ";
                strSql += CRLF + "    SET JOB_TYP_OD          = :JOB_TYP_OD              ";
                strSql += CRLF + "      , LUGG_NO_FK1_OD      = :LUGG_NO_FK1_OD          ";
                strSql += CRLF + "      , START_BANK_FK1_OD   = :START_BANK              ";
                strSql += CRLF + "      , START_BAY_FK1_OD    = :START_BAY               ";
                strSql += CRLF + "      , START_LEVEL_FK1_OD  = :START_LEVEL             ";
                strSql += CRLF + "      , START_HSPOS_FK1_OD  = :START_HSPOS             ";
                strSql += CRLF + "      , DEST_BANK_FK1_OD    = :DEST_BANK               ";
                strSql += CRLF + "      , DEST_BAY_FK1_OD     = :DEST_BAY                ";
                strSql += CRLF + "      , DEST_LEVEL_FK1_OD   = :DEST_LEVEL              ";
                strSql += CRLF + "      , DEST_HSPOS_FK1_OD   = :DEST_HSPOS              ";

                // FK2 칸은 늘 적는다. 쓰지 않을 때 0 으로 지워 두지 않으면 지난
                // 트윈 지시의 값이 남아 크레인이 엉뚱한 곳을 집는다.
                strSql += CRLF + "      , LUGG_NO_FK2_OD      = :LUGG_NO_FK2_OD      ";
                strSql += CRLF + "      , START_BANK_FK2_OD   = :START_BANK2         ";
                strSql += CRLF + "      , START_BAY_FK2_OD    = :START_BAY2          ";
                strSql += CRLF + "      , START_LEVEL_FK2_OD  = :START_LEVEL2        ";
                strSql += CRLF + "      , START_HSPOS_FK2_OD  = :START_HSPOS2        ";
                strSql += CRLF + "      , DEST_BANK_FK2_OD    = :DEST_BANK2          ";
                strSql += CRLF + "      , DEST_BAY_FK2_OD     = :DEST_BAY2           ";
                strSql += CRLF + "      , DEST_LEVEL_FK2_OD   = :DEST_LEVEL2         ";
                strSql += CRLF + "      , DEST_HSPOS_FK2_OD   = :DEST_HSPOS2         ";
                //   사용 포크 : 0 = 포크1 / 1 = 포크1,2(양쪽) / 2 = 포크2
                //   이 현장 크레인은 전부 SINGLE 로 둔다(BOX 는 레거시상 Twin 이나 포크1만 쓴다).
                //   1 을 주면 크레인이 양포크 작업으로 돌아 한 건을 끝내도
                //   적재 표시가 다 안 내려가고, 다음 작업이 집기부터 시작하지 못한다.
                //   0 = 포크1 / 1 = 포크1,2(양쪽) / 2 = 포크2
                //   짝이 없을 때 1 을 주면 크레인이 양포크 작업으로 돌아 한 건을 끝내도
                //   적재 표시가 다 안 내려가고 다음 작업이 집기부터 시작하지 못한다.
                //   그래서 FK2 를 실제로 적을 때만 1 로 준다.
                strSql += CRLF + "      , USE_FK_OD           = '" + strUseFk + "'          ";
                strSql += CRLF + "      , WRITE_FLAG_OD       = '1'                      ";   // 명령 기록 완료 (레거시 D191=1)
                strSql += CRLF + "      , OD_RQ_YN            = 'Y'                      ";
                strSql += CRLF + "      , OD_USER_ID          = '" + OD_USER + "'        ";
                strSql += CRLF + "      , OD_UPD_DT           = " + DbLang.SYSDATE + "   ";
                strSql += CRLF + "  WHERE WH_TYP              = :WH_TYP                  ";
                strSql += CRLF + "    AND SC_NO               = :SC_NO                   ";
                strSql += CRLF + "    AND OD_RQ_YN            = 'N'                      ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(ERR_CODE_RD, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                   ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_TYP_OD",     DbLang.VARCHAR).Value = bR2R ? "4" : (bStore ? SC_CMD_STORE : SC_CMD_RETRIEVE);
                _pBdb.mComMain.Parameters.Add("LUGG_NO_FK1_OD", DbLang.VARCHAR).Value = strLuggNo;
                _pBdb.mComMain.Parameters.Add("START_BANK",     DbLang.VARCHAR).Value = bStore ? "0" : strBank;
                _pBdb.mComMain.Parameters.Add("START_BAY",      DbLang.VARCHAR).Value = bStore ? "0" : strBay;
                _pBdb.mComMain.Parameters.Add("START_LEVEL",    DbLang.VARCHAR).Value = bStore ? "0" : strLev;
                _pBdb.mComMain.Parameters.Add("START_HSPOS",    DbLang.VARCHAR).Value = bStore ? strStoHsPos : "0";
                _pBdb.mComMain.Parameters.Add("DEST_BANK",      DbLang.VARCHAR).Value = bR2R ? strDBank : (bStore ? strBank : "0");
                _pBdb.mComMain.Parameters.Add("DEST_BAY",       DbLang.VARCHAR).Value = bR2R ? strDBay : (bStore ? strBay  : "0");
                _pBdb.mComMain.Parameters.Add("DEST_LEVEL",     DbLang.VARCHAR).Value = bR2R ? strDLev : (bStore ? strLev  : "0");
                _pBdb.mComMain.Parameters.Add("DEST_HSPOS",     DbLang.VARCHAR).Value = (bStore || bR2R) ? "0" : strRetHsPos;
                /*
                 * FK2 칸에 넣을 값.
                 *   USE_FK 1 : 짝의 자리를 넣는다.
                 *   USE_FK 0 : 0 으로 지운다.
                 */
                string strFk2Lugg  = bTwin ? strLuggNo2 : "0";
                string strFk2Bank  = bTwin ? strBank2   : "0";
                string strFk2Bay   = bTwin ? strBay2    : "0";
                string strFk2Lev   = bTwin ? strLev2    : "0";
                bool   bFk2Used    = bTwin;

                _pBdb.mComMain.Parameters.Add("LUGG_NO_FK2_OD", DbLang.VARCHAR).Value = strFk2Lugg;
                _pBdb.mComMain.Parameters.Add("START_BANK2",    DbLang.VARCHAR).Value = (bFk2Used && !bStore) ? strFk2Bank : "0";
                _pBdb.mComMain.Parameters.Add("START_BAY2",     DbLang.VARCHAR).Value = (bFk2Used && !bStore) ? strFk2Bay  : "0";
                _pBdb.mComMain.Parameters.Add("START_LEVEL2",   DbLang.VARCHAR).Value = (bFk2Used && !bStore) ? strFk2Lev  : "0";
                _pBdb.mComMain.Parameters.Add("START_HSPOS2",   DbLang.VARCHAR).Value = (bFk2Used &&  bStore) ? strStoHsPos : "0";
                _pBdb.mComMain.Parameters.Add("DEST_BANK2",     DbLang.VARCHAR).Value = (bFk2Used &&  bStore) ? strFk2Bank : "0";
                _pBdb.mComMain.Parameters.Add("DEST_BAY2",      DbLang.VARCHAR).Value = (bFk2Used &&  bStore) ? strFk2Bay  : "0";
                _pBdb.mComMain.Parameters.Add("DEST_LEVEL2",    DbLang.VARCHAR).Value = (bFk2Used &&  bStore) ? strFk2Lev  : "0";
                _pBdb.mComMain.Parameters.Add("DEST_HSPOS2",    DbLang.VARCHAR).Value = (bFk2Used && !bStore) ? strRetHsPos : "0";

                _pBdb.mComMain.Parameters.Add("WH_TYP",         DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO",          DbLang.VARCHAR).Value = strScNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "SC_DATA 명령 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "지시할 SC_DATA 가 없음(SC_NO:" + strScNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        // ─────────────────────────────────────────────────────────────────
        // 랙투랙 지시 (20 -> 21) - 레거시 CSc::RackToRackRoutine + RackToRack
        //   같은 호기 안에서 랙 위치만 바꾼다. 출발 랙에서 집어 도착 랙에 내려놓는다. (D171=4)
        //     크레인  : JM.START_POS (랙투랙은 출발지가 호기, 상위가 0 으로 줬어도 HOST 가 랙 뱅크로 채운다)
        //     출발    : JM.START_LOCATION      도착 : JM.DEST_LOCATION
        //   H/S 를 쓰지 않으므로 H/S 조건은 없다. 크레인이 지시를 받을 수 있는 상태(SQL_SC_READY)이고
        //   입고/출고 정지가 아니어야 한다. 완료는 SC_COMP_CHK 가 29 로 닫고 HOST 가 완료보고한다.
        //   층과 무관하므로 어느 층 스레드가 불러도 같은 잠금으로 한 곳에서만 돈다.
        // ─────────────────────────────────────────────────────────────────
        private static readonly object m_objR2RLock = new object();

        public bool RackToRackCheck(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return RACK_TO_RACK_ROUTINE(strWH_TYP, "[RackToRackCheck]", ref pRTN_MSG); }

        protected bool RACK_TO_RACK_ROUTINE(string strWH_TYP, string strTitle, ref string pRTN_MSG, bool bCommon = false)
        {
            if (!bCommon && CommonAlive()) { pRTN_MSG = ""; return true; }   // 공통 스레드가 맡는다
            if (!Monitor.TryEnter(m_objR2RLock)) { pRTN_MSG = ""; return true; }
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                pRTN_MSG = strTitle;

                strSql += CRLF + " SELECT LUGG_NO, JOB_TYP, START_POS, START_LOCATION, DEST_LOCATION ";
                strSql += CRLF + "   FROM JOB_MST                                            ";
                strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP                               ";
                strSql += CRLF + "    AND JOB_TYP    IN (" + JT_IN_R2R + ")                  ";   // 랙투랙 4 / 14
                strSql += CRLF + "    AND JOB_STATUS = '" + ST_SC_WAIT + "'                  ";   // 20 = SC 구동요구
                strSql += CRLF + "  ORDER BY " + SQL_JOB_PRIORITY("") + " DESC, LUGG_NO      ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0) { pRTN_MSG += _pBdb.ErrMsg; return false; }
                if (nSelCnt == 0) { pRTN_MSG = ""; return true; }

                DataTable dtJob = _pBdb.mDtMain.Copy();
                for (int i = 0; i < dtJob.Rows.Count; i++)
                {
                    string strLUGG_NO  = dtJob.Rows[i]["LUGG_NO"].ToString().Trim();
                    string strJOB_TYP  = dtJob.Rows[i]["JOB_TYP"].ToString().Trim();
                    string strSC_NO    = dtJob.Rows[i]["START_POS"].ToString().Trim();
                    string strSTART_LOC = dtJob.Rows[i]["START_LOCATION"].ToString().Trim();
                    string strDEST_LOC  = dtJob.Rows[i]["DEST_LOCATION"].ToString().Trim();

                    if (IsScNo(strSC_NO) == false)
                    {
                        if (GetScNoByLocation(strSTART_LOC, ref strSC_NO) == false) continue;
                    }

                    // 그 크레인이 지시를 받을 수 있는가 (H/S 는 보지 않는다)
                    strSql = "";
                    strSql += CRLF + " SELECT SD.SC_NO FROM SC_DATA SD                      ";
                    strSql += CRLF + "  WHERE SD.WH_TYP = :WH_TYP AND SD.SC_NO = :SC_NO     ";
                    strSql += SQL_SC_READY;
                    strSql += SQL_SC_RET_NOT_SUSPEND;
                    strSql += SQL_SC_STO_NOT_SUSPEND;
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strSC_NO;
                    nSelCnt = _pBdb.ExcuteQry(strSql);
                    if (nSelCnt < 0) { pRTN_MSG += _pBdb.ErrMsg; return false; }
                    if (nSelCnt == 0) continue;     // 아직 받을 수 없다 - 다음 작업/다음 주기

                    _pBdb.BeginTrans();
                    if (UpdateScData(strSC_NO, JT_R2R, strLUGG_NO, strSTART_LOC, strDEST_LOC, "0", "0", ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        pRTN_MSG = "";
                        return true;    // 크레인 미준비 - 다음 사이클 재시도
                    }
                    if (UPDATE_JOB_DATA(ST_SC_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG,
                                        "0", "0", "0", strSC_NO) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                    pRTN_MSG = strTitle + strSC_NO + "호기에 랙투랙을 지시하였습니다. [작업번호:" + strLUGG_NO
                             + "][출발LOC:" + strSTART_LOC + "][도착LOC:" + strDEST_LOC + "]";
                    _pBdb.Commit();
                    InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_SC_RUN, "", strSC_NO);
                    MakeRackMsg("[랙투랙 2/3] " + strSC_NO + "호기에 랙투랙을 지시하였습니다. [작업번호:" + strLUGG_NO + "][출발LOC:" + strSTART_LOC + " → 도착LOC:" + strDEST_LOC + "]", cDefApp.eLogMsgType.MSG_IMP);
                    return true;    // 사이클당 1건
                }
                pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
            finally
            {
                Monitor.Exit(m_objR2RLock);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 전용 스레드 공통 틀 (지시 전용 / 도착 검사 전용)  -  1F/3F/BOX 가 같이 쓴다.
        //   층마다 다른 것은 두 가지뿐이다.
        //     CreateWorker() : 자기와 같은 층 클래스의 새 인스턴스 (전용 스레드는 자기 DB 연결을 쓴다)
        //     CmdFuncs() / ArvFuncs() : 그 스레드가 0.05초 주기로 도는 함수 목록
        //   나머지(시작/중지/DB 재연결/오류 처리/주기)는 여기서 한 번만 정의한다.
        // ─────────────────────────────────────────────────────────────────
        protected volatile bool m_bCmdStop = false;
        protected volatile bool m_bArvStop = false;
        private Thread m_thCmd = null, m_thArv = null;
        private IOSchDB m_cmdWorker = null, m_arvWorker = null;

        protected virtual IOSchDB CreateWorker() { return null; }
        protected virtual SchFunc[] CmdFuncs() { return new SchFunc[0]; }
        protected virtual SchFunc[] ArvFuncs() { return new SchFunc[0]; }

        private IOSchDB NewWorker(string strTag)
        {
            IOSchDB w = CreateWorker();
            w.m_strWorkerTag = strTag;
            w.ConnectionString = this.ConnectionString;
            w.callPsMsgView = this.callPsMsgView;
            w.m_Main = this.m_Main;
            return w;
        }

        protected void StartCmdWorker()
        {
            try
            {
                StopCmdWorker();
                m_cmdWorker = NewWorker("[지시] ");
                m_thCmd = new Thread(m_cmdWorker.CmdLoop);
                m_thCmd.IsBackground = true;
                m_thCmd.Name = "SCH_CMD_" + m_nId;
                m_thCmd.Start();
            }
            catch (Exception ex)
            {
                MakeMsg_Error("[SCH] 지시 전용 스레드 시작 오류: " + ex.Message);
            }
        }

        protected void StopCmdWorker()
        {
            try
            {
                if (m_cmdWorker != null) m_cmdWorker.m_bCmdStop = true;
                if (m_thCmd != null && m_thCmd.IsAlive) m_thCmd.Join(3000);
            }
            catch { }
            m_cmdWorker = null;
            m_thCmd = null;
        }

        protected void StartArvWorker()
        {
            try
            {
                StopArvWorker();
                m_arvWorker = NewWorker("[도착] ");
                m_thArv = new Thread(m_arvWorker.ArvLoop);
                m_thArv.IsBackground = true;
                m_thArv.Name = "SCH_ARV_" + m_nId;
                m_thArv.Start();
            }
            catch (Exception ex)
            {
                MakeMsg_Error("[SCH] 도착 검사 전용 스레드 시작 오류: " + ex.Message);
            }
        }

        protected void StopArvWorker()
        {
            try
            {
                if (m_arvWorker != null) m_arvWorker.m_bArvStop = true;
                if (m_thArv != null && m_thArv.IsAlive) m_thArv.Join(3000);
            }
            catch { }
            m_arvWorker = null;
            m_thArv = null;
        }

        public void CmdLoop() { WorkerLoop("지시", CmdFuncs(), () => m_bCmdStop); }
        public void ArvLoop() { WorkerLoop("도착 검사", ArvFuncs(), () => m_bArvStop); }

        private void WorkerLoop(string strKind, SchFunc[] funcs, Func<bool> isStop)
        {
            while (!isStop() && !IsDBOpen)
            {
                try { if (DBOpen()) break; } catch { }
                Thread.Sleep(2000);
            }
            if (isStop()) return;
            MakeMsg("[SCH] " + strKind + " 전용 스레드 시작 (0.05초 주기)");   // 말머리가 앞에 붙는다: "[지시] [SCH] 지시 전용 스레드 시작"

            while (!isStop())
            {
                try
                {
                    if (!IsDbAlive())
                    {
                        if (!DBReopen()) Thread.Sleep(3000);
                        continue;
                    }
                    foreach (SchFunc fn in funcs) RunSchFunc(fn);
                }
                catch (Exception ex)
                {
                    MakeMsg_Error("[SCH] " + strKind + " 전용 스레드 오류: " + ex.Message);
                    Thread.Sleep(1000);
                }
                Thread.Sleep(50);
            }
            MakeMsg("[SCH] " + strKind + " 전용 스레드를 종료했습니다.");
        }

        /// <summary>
        /// 출고 H/S 에 내려진 화물(상태 29)을 CV 로 내보낸다. (1F / 3F / BOX 공통)  상태 29 -> 21(CV 구동)
        ///   층별로 달라지는 것은 eFloor 로 가른다.
        ///     F1  : 출고 H/S HS_NO_RETRIEVE, 그룹 출고대(105)를 H/S 의 대기대(SC_HS_DEF.WAIT_TRACK)로 바꾼다
        ///     F3  : 출고 H/S HS_NO_RET_3F, 상태 28(호기간 이동 1단계 끝)도 받고 호기간 이동은 호기 번호로 CV 에 싣고 2단계로 넘긴다
        ///     BOX : 출고 H/S '02', 트윈 포크의 짝 H/S(PAIR_HS_NO)에 놓인 것도 본다
        /// </summary>
        protected bool CV_RETHS_PLC(string strWH_TYP, string strCV_PLC, EN_RET_FLOOR eFloor, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                bool bF1  = (eFloor == EN_RET_FLOOR.F1);
                bool b3F  = (eFloor == EN_RET_FLOOR.F3);
                bool bBox = (eFloor == EN_RET_FLOOR.BOX);
                string strHsNo  = bF1 ? HS_NO_RETRIEVE : (b3F ? HS_NO_RET_3F : "02");
                string strFlrNm = bF1 ? "1층" : "3층";

                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT  JM.*, CD.*" + (bF1 ? ", SHD.WAIT_TRACK" : "") + "                    ";
                strSql += CRLF + "   FROM  JOB_MST JM                                  ";
                strSql += CRLF + "  INNER  JOIN CV_DATA CD                             ";
                strSql += CRLF + "     ON  JM.WH_TYP             = CD.WH_TYP           ";
                strSql += CRLF + "    AND  JM.HS_TRACK_NO        = CD.MC_NO            ";
                strSql += CRLF + "  INNER  JOIN SC_HS_DEF SHD                          ";
                strSql += CRLF + "     ON  JM.WH_TYP             = SHD.WH_TYP          ";
                if (bBox)
                {
                    // 트윈으로 내보내면 포크#1 은 HS_MC_NO 에, 포크#2 는 PAIR_HS_NO 에 내려놓는다. HS_MC_NO 만 보면 짝 자리의 화물을 아무도 집어 가지 않는다.
                    strSql += CRLF + "    AND ( JM.HS_TRACK_NO       = SHD.HS_MC_NO         ";
                    strSql += CRLF + "       OR JM.HS_TRACK_NO       = SHD.PAIR_HS_NO        )";
                }
                else
                    strSql += CRLF + "    AND  JM.HS_TRACK_NO        = SHD.HS_MC_NO        ";
                strSql += CRLF + "    AND  SHD.HS_NO             = '" + strHsNo + "'   ";   // 출고 HS (1F: 02 / 3F 레거시 RANK_4: 04 / BOX 레거시 RANK_2: 02)
                strSql += CRLF + "    AND  SHD.HS_USE_YN         = 'Y'                 ";
                strSql += CRLF + "  WHERE  CD.WH_TYP             = :WH_TYP             ";
                strSql += CRLF + "    AND  CD.PLC_NO             = :CV_PLC             ";   // 3층 해당 PLC 한정 (ECS m_nNum 게이트)
                strSql += CRLF + "    AND  COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                 ";
                strSql += CRLF + "    AND  CD.SENSOR0_DATA_RD    = '1'                 ";   // 재하 (ECS IsOnSensorIO(0))
                strSql += CRLF + "    AND  CD.OD_RQ_YN           = 'N'                 ";
                if (b3F)
                {
                    strSql += CRLF + "    AND  (JM.JOB_STATUS 	      = '" + ST_SC_DONE + "' OR (JM.JOB_STATUS      = '" + ST_SC_DONE_A2A_STEP1 + "' AND     ";
                    strSql += CRLF + "                                                           JM.WC_STEP    = '1'))   ";   // S/C 반출 완료 (CV 요구). 28 = 호기간 이동 1단계 완료
                }
                else
                    strSql += CRLF + "    AND  JM.JOB_STATUS 	     = '29'                ";   // S/C 반출 완료 (CV 요구)
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                DataTable dtHs = _pBdb.mDtMain.Copy();

                for (int i = 0; i < dtHs.Rows.Count; i++)
                {
                    string strJOB_TYP = dtHs.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : dtHs.Rows[i]["JOB_TYP"].ToString();
                    string strTRAY_TYP = "" + dtHs.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : dtHs.Rows[i]["PRODUCT_SIZE"].ToString();
                    string strTRAY_LEV = "" + dtHs.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : dtHs.Rows[i]["TRAY_LEV"].ToString();
                    string strDEST_POS = "" + dtHs.Rows[i]["DEST_POS"].ToString() == "" ? "0" : dtHs.Rows[i]["DEST_POS"].ToString();
                    string strIS_TURN = "" + dtHs.Rows[i]["TURN"].ToString() == "" ? "0" : dtHs.Rows[i]["TURN"].ToString();
                    string strLUGG_NO = "" + dtHs.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : dtHs.Rows[i]["LUGG_NO"].ToString();
                    string strHS_MC = "" + dtHs.Rows[i]["MC_NO"].ToString() == "" ? "0" : dtHs.Rows[i]["MC_NO"].ToString();

                    // 출고 계열 작업만 대상 (CHECK_CV_RETHS 와 동일 기준)
                    if (IsRetJobType(strJOB_TYP) == false)
                        continue;

                    /*
                     * 그룹 출고대(105)는 가상 자리다 - 실제 자리를 여기서 정한다.
                     *
                     *   상위는 "1층으로 빼라"만 말하고(DEST_POS=105, DEST_POS_DEF 상 MC 2101),
                     *   어느 크레인의 출고 대기대로 갈지는 WCS 가 정해야 한다. 대기대는
                     *   화물을 내려놓은 그 출고 H/S 의 짝이므로 SC_HS_DEF.WAIT_TRACK 에 있다.
                     *   (이 질의는 이미 SHD 를 HS_MC_NO 로 조인해 두었다)
                     *
                     *   전에는 JOB_MST.START_POS 를 호기로 보고 150 을 더했다. 두 군데가 틀렸다.
                     *     - START_POS 는 출고작업에서 000 으로 들어오는 일이 있다 (랙 위치는
                     *       START_LOCATION 에 있다). 그러면 0+150 = 150 이 나온다.
                     *     - 호기 표기는 9NN(901~911)이라, 값이 제대로 와도 904+150 = 1054 다.
                     *       150+n 은 호기가 1~10 일 때만 맞는 식이었다.
                     *   대기대 자리는 이미 데이터(SC_HS_DEF.WAIT_TRACK)에 있으니 계산하지 않는다.
                     *
                     *   WAIT_TRACK 은 실트랙(220)이고 PLC 에 쓸 목적지는 스테이션(154)이라
                     *   DEST_POS_DEF 로 되돌린다.
                     */
                    // (2026-11-24) 105 뿐 아니라 103 / 104 로 내려온 작업도 중간 도착대 번호를 쓴다.
                    if (bF1 && Array.IndexOf(STN_1F_PLT, strDEST_POS.Trim()) >= 0)
                    {
                        string strWAIT_MC = dtHs.Rows[i]["WAIT_TRACK"].ToString().Trim();
                        if (strWAIT_MC == "")
                        {
                            pRTN_MSG = strTitle + "출고 H/S(" + strHS_MC + ")에 대기대(SC_HS_DEF.WAIT_TRACK)가 없습니다. [작업번호:" + strLUGG_NO + "]";
                            InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strHS_MC, "");
                            continue;
                        }

                        if (GET_DEST_POS_STATION(strWH_TYP, strWAIT_MC, ref strDEST_POS, ref pRTN_MSG) == false)
                        {
                            pRTN_MSG = strTitle + pRTN_MSG + " [작업번호:" + strLUGG_NO + "]";
                            InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strHS_MC, strWAIT_MC);
                            continue;
                        }
                    }

                    _pBdb.BeginTrans();

                    // 호기간 이동(1단계 끝): 도착은 다른 호기의 3층 입고 H/S 다. 목적지가 크레인 번호(9NN)이므로 CV 에는 호기 번호로 실어 보낸다.
                    bool bA2A = b3F && IsA2AJobType(strJOB_TYP);

                    // C/V에 목적지정보쓰기 (ECS: JobItem.m_nDestPos 그대로 - 재매핑 없음)
                    if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, bA2A ? GfCvDestPos(strDEST_POS) : strDEST_POS, strIS_TURN,
                                       strLUGG_NO, strWH_TYP, strCV_PLC, strHS_MC, "", ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    // 호기간 이동은 이때 2단계(CMP_STEP='2')로 넘어간다. 도착 호기 3층 입고 H/S 에 닿으면 StoHsCheck 가 입고로 받는다.
                    if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG,
                                        "0", "0", "0", "0", "0", "0", bA2A ? "2" : "0") == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    pRTN_MSG = strTitle + "TRACK " + strHS_MC + "번[" + strFlrNm + " 출고 H/S]에서 "
                             + (bA2A ? "도착 호기(" + strDEST_POS + ")의 3층 입고 H/S" : "픽킹대(" + strDEST_POS + ")") + "로 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                    _pBdb.Commit();
                    InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strHS_MC, strDEST_POS);
                    if (bA2A)
                        MakeRackMsg("[호기간 이동 4/6] CV TRACK " + strHS_MC + "[3층 출고 H/S]에 데이터를 기록했습니다. 도착 호기 " + strDEST_POS + "의 3층 입고 H/S 로 보냅니다. [작업번호:" + strLUGG_NO + "]", cDefApp.eLogMsgType.MSG_NOR);
                    return true;
                }

                pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        /// <summary>
        /// 출고 레인 가용 확인 (1F/3F/BOX 공통)
        ///   strCheckMC : 비었는지 볼 트랙 (무재하/AUTO/무에러)
        ///   strDestMC  : 그쪽으로 가는 중인 화물(DEST_POS_OD, OD_RQ_YN='Y')이 없어야 하는 목적지
        ///   1F 는 레인 진입 자리와 작업대 번호가 다르고, 3F/BOX 는 같은 트랙이다.
        /// </summary>
        protected bool CHECK_LANE_FREE(string strWH_TYP, string strCheckMC, string strDestMC)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT COUNT(*) AS READY_CNT                                        ";
                strSql += CRLF + "   FROM CV_DATA CD                                                   ";
                strSql += CRLF + "  WHERE CD.WH_TYP           = :WH_TYP                                ";
                strSql += CRLF + "    AND CD.MC_NO            = :ENTRY_MC                              ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD  = '0'                                    ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD     = '1'                                    ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                           ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*) FROM CV_DATA CD2                        ";
                strSql += CRLF + "              WHERE CD2.WH_TYP      = :WH_TYP2                       ";
                strSql += CRLF + "                AND CD2.DEST_POS_OD = :MC_NO2                        ";
                strSql += CRLF + "                AND CD2.OD_RQ_YN    = 'Y')                           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("ENTRY_MC", DbLang.VARCHAR).Value = strCheckMC;
                _pBdb.mComMain.Parameters.Add("WH_TYP2", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO2", DbLang.VARCHAR).Value = strDestMC;
                int nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt <= 0) return false;
                return Convert.ToInt32(_pBdb.mDtMain.Rows[0]["READY_CNT"]) > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>출고대 가용 확인 (3F/BOX) : 그 트랙이 비었고 그 트랙으로 가는 화물이 없음</summary>
        protected bool CHECK_RET_LANE_READY(string strWH_TYP, string strMC_NO)
        {
            return CHECK_LANE_FREE(strWH_TYP, strMC_NO, strMC_NO);
        }

        protected void RunSchFunc(SchFunc fn)
        {
            int nT0 = Environment.TickCount;
            if (!fn(SCH_WH_TYP, m_nId.ToString(), ref strRTN_MSG))
            {
                if (strRTN_MSG != "")
                {
                    MakeMsg_Error_NoLog(strRTN_MSG);
                    SetErrorMsg(strRTN_MSG);
                }
            }
            else
            {
                if (strRTN_MSG != "") { MakeMsg(strRTN_MSG); }
            }
            Thread.Sleep(10);
            int nMs = unchecked(Environment.TickCount - nT0);
            if (nMs >= 500)
            {
                MakeMsg("[SCH] 느린 루틴 " + fn.Method.Name + " " + nMs + " ms");
                try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IoSlow.log"), DateTime.Now.ToString("HH:mm:ss") + " 느린 루틴 " + fn.Method.Name + " " + nMs + " ms" + Environment.NewLine); } catch { }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 0 : 신규 작업 접수 (99 → 10 / 20)
        //   상위가 넣은 신규('99')를 어느 설비 구간에서 시작할지 정한다.
        //     이동(6)/입고(1) : CV 에서 시작한다  → 10(CV 구동대기)
        //     그 밖(출고 등)  : 크레인에서 시작한다 → 20(SC 구동요구)
        //
        //   층이 아니라 작업 단위의 일이라 한 주기에 한 스레드만 돌리면 된다.
        //   전에는 1F 스레드의 private 함수라 거기서만 돌았고, ENV_IOSCH.INI 의
        //   USE_1F 를 'N' 으로 두면 3층·BOX 는 작업이 '99' 에 그대로 남아 통째로
        //   멈췄다. 1층을 끄고 3층만 돌리는 구성이 그래서 안 됐다.
        //   여기(공통)로 올리고, 세 스레드가 같은 행을 동시에 집지 않도록
        //   잠금을 잡은 스레드만 이번 주기의 접수를 맡게 한다.
        // ─────────────────────────────────────────────────────────────────
        // @.접수를 이번 주기에 누가 맡는지 가리는 잠금. 층별 스레드가 한 프로세스
        //   안에 같이 뜨므로 static 하나면 된다.
        private static readonly object m_objJobAcceptLock = new object();

        /*
         * 공통 스레드(화면의 "공통" 탭 = cThread_LOG)의 맥박.
         *
         *   신규 작업 접수(JOB_ACCEPT)와 랙투랙 지시는 층과 무관한 일이라 공통 스레드 한 곳이 맡는다.
         *   공통 스레드가 한 바퀴 돌 때마다 맥박을 찍고, 최근 3초 안에 찍혔으면 층(1F/3F/BOX) 스레드는 이 두 일을 건너뛴다.
         *   공통 스레드가 꺼져 있거나(USE_LOG=N / 수동 중지) 멈추면 맥박이 끊겨 층 스레드가 예전처럼 대신 한다.
         *   (그래서 어떤 경우에도 접수가 멈추지 않는다)
         */
        private static volatile int s_nCommonBeat = 0;
        public static void CommonBeat() { int n = Environment.TickCount; s_nCommonBeat = (n == 0) ? 1 : n; }

        /*
         * 크레인 자동 복구 - 메인 폼 "크레인 자동 에러 해제" / "크레인 자동 ACTIVE"
         *
         *   공통 스레드가 5초마다 한 번 부른다. SC_TASK 는 SC_DATA.CMD_RQ_YN='Y' 를 보고 CMD_RQ_ID 별로
         *   PLC D199 에 명령을 쓴다(RESET=8, ACTIVE=2). 쓴 뒤 CMD_RQ_YN 을 'N' 으로 되돌린다.
         *
         *   에러 해제 : 오류코드(ERR_CODE_RD)가 0 이 아닌 크레인에만 RESET 을 낸다. 오류가 아니면 내지 않는다.
         *   ACTIVE   : 기동 상태(ACTIVE_MODE_RD)가 1(ACTIVE)이 아닌 크레인에만 ACTIVE 를 낸다. ACTIVE 면 내지 않는다.
         *   앞 명령(CMD_RQ_YN='Y')을 SC_TASK 가 아직 안 가져갔으면 이번에는 건너뛴다(겹쳐 쓰지 않는다).
         *   오류가 풀리거나 ACTIVE 가 되면 조건에서 빠지므로 저절로 그친다.
         *   한 크레인이 둘 다 해당하면 에러 해제를 먼저 내고, ACTIVE 는 다음 5초에 낸다.
         */
        public bool ScAutoRecover(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        {
            pRTN_MSG = "";
            if (cDefApp.GM_SC_AUTO_RESET == false && cDefApp.GM_SC_AUTO_ACTIVE == false) return true;
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT SC_NO, ERR_CODE_RD, ACTIVE_MODE_RD                                    ";
                strSql += CRLF + "   FROM SC_DATA SD                                                             ";
                strSql += CRLF + "  WHERE SD.WH_TYP = :WH_TYP                                                    ";
                strSql += CRLF + "    AND COALESCE(SD.CMD_RQ_YN,'N') = 'N'                                       ";   // 앞 명령을 SC_TASK 가 가져갔다
                strSql += CRLF + "    AND ( COALESCE(NULLIF(BTRIM(SD.ERR_CODE_RD, " + SQL_WS + "), ''), '0') NOT IN ('0','0000')   ";
                strSql += CRLF + "       OR COALESCE(NULLIF(BTRIM(SD.ACTIVE_MODE_RD, " + SQL_WS + "), ''), '0') <> '" + SC_ACTIVE + "' )";
                strSql += CRLF + "  ORDER BY SC_NO                                                               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                int nCnt = _pBdb.ExcuteQry(strSql);
                if (nCnt < 0) { pRTN_MSG = "[ScAutoRecover]" + _pBdb.ErrMsg; return false; }
                if (nCnt == 0) return true;

                DataTable dt = _pBdb.mDtMain.Copy();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string strSC  = dt.Rows[i]["SC_NO"].ToString().Trim();
                    string strErr = dt.Rows[i]["ERR_CODE_RD"].ToString().Trim();
                    string strAct = dt.Rows[i]["ACTIVE_MODE_RD"].ToString().Trim();
                    bool bErr = (strErr != "" && strErr.TrimStart('0') != "");
                    bool bStop = (strAct != SC_ACTIVE);

                    // @.이중입고(54/55) / 공출고(58/59) 는 자동으로 풀지 않는다 (2026-11-05).
                    //   이 에러는 상위(WMS)가 재지정을 내려 줄 때까지 크레인이 서 있어야 한다. 여기서 RESET 을 내면
                    //   상위가 판단하기도 전에 에러가 사라진다. 처리는 ScDualStoreRetry 가 한다.
                    string strErrN = strErr.TrimStart('0');
                    if (strErrN == "54" || strErrN == "55" || strErrN == "58" || strErrN == "59") continue;

                    string strCmd = "", strWhat = "";
                    if (bErr && cDefApp.GM_SC_AUTO_RESET)        { strCmd = "RESET";  strWhat = "에러 해제(오류코드 " + strErr + ")"; }
                    else if (bStop && cDefApp.GM_SC_AUTO_ACTIVE) { strCmd = "ACTIVE"; strWhat = "ACTIVE(기동 상태 " + (strAct == "" ? "-" : strAct) + " = STOP)"; }
                    if (strCmd == "") continue;

                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                        ";
                    strSql += CRLF + "    SET CMD_RQ_YN  = 'Y'               ";
                    strSql += CRLF + "      , CMD_RQ_ID  = '" + strCmd + "'  ";
                    strSql += CRLF + "      , OD_USER_ID = '" + OD_USER + "' ";
                    strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP           ";
                    strSql += CRLF + "    AND SC_NO      = :SC_NO            ";
                    strSql += CRLF + "    AND COALESCE(CMD_RQ_YN,'N') = 'N'  ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strSC;
                    int n = _pBdb.ExcuteNonQry(strSql);
                    if (n < 0) { pRTN_MSG = "[ScAutoRecover]" + _pBdb.ErrMsg; return false; }
                    if (n == 0) continue;

                    string strMsg = "[ScAutoRecover]" + strSC + "호기에 " + strWhat + " 명령을 자동 지시하였습니다. (5초마다, 풀리면 그침)";
                    InsertLog(strWH_TYP, strMsg, "", "", "", "", "", strSC);
                    MakeMsg(strMsg);
                }
                return true;
            }
            catch (Exception ex) { pRTN_MSG = "[ScAutoRecover]" + ex.Message; return false; }
        }
        /*
         * 이중입고 / 공출고 재지정 뒤처리 (2026-11-05) - 기존 ECS CSc::ErrorRoutine 을 옮긴 것
         *
         *   공통 스레드가 1초마다 부른다.
         *
         *   [이중입고 54(포크#1) / 55(포크#2)]
         *     크레인이 랙에 내리려다 이미 화물이 있어 선 상태다. 화물은 포크에 그대로 있다.
         *     HOST 태스크가 에러 보고(E, 종류 1)를 올리고, 상위가 재지정(R, 종류 1)을 내리면 HOST 태스크가
         *     JOB_MST.DEST_LOCATION 을 새 위치로 바꾼다. (작업 상태는 SC 구동중 21 그대로다)
         *     여기서는 "크레인이 이중입고 에러인데 작업의 도착 위치가 마지막으로 지시한 위치(SC_DATA.DEST_*_OD)와 다르다"
         *     를 재지정된 것으로 보고, 마지막 지시에서 도착 위치만 바꿔 다시 쓴다. (기존 ECS 도 에러인 채로 새 Store 를 썼다)
         *     다시 쓰고 나면 OD 값이 새 위치와 같아지므로 한 번만 나간다.
         *     포크#2 에러(55)는 포크 사용 구분을 2(포크#2 만)로 바꿔 포크#2 만 다시 내리게 한다.
         *
         *   [공출고 58 / 59]
         *     크레인이 랙에서 뜨려는데 화물이 없어 선 상태다. 포크는 비어 있다.
         *     상위가 재지정(R, 종류 2)을 내리면 HOST 태스크가 START_LOCATION 을 바꾸고 작업을 SC 구동요구(20)로 되돌린다.
         *     작업이 20 으로 돌아갔거나(재지정) 없어졌으면(취소) 크레인에 포크 데이터 삭제(DELFK)를 내고, 지워지면 RESET 을 낸다.
         *     그 뒤는 평소 출고 지시가 새 출발 위치로 다시 낸다.
         *     상위가 아무것도 내리지 않으면 크레인은 에러로 서 있는다. (자동 에러 해제도 이 코드는 건드리지 않는다)
         */
        public bool ScDualStoreRetry(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        {
            pRTN_MSG = "";
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT SD.SC_NO, SD.ERR_CODE_RD, SD.JOB_TYP_OD, SD.USE_FK_OD, SD.ITN_LUGG_FK1, SD.ITN_LUGG_FK2 ";
                strSql += CRLF + "      , SD.LUGG_NO_FK1_OD, SD.DEST_BANK_FK1_OD, SD.DEST_BAY_FK1_OD, SD.DEST_LEVEL_FK1_OD ";
                strSql += CRLF + "      , SD.LUGG_NO_FK2_OD, SD.DEST_BANK_FK2_OD, SD.DEST_BAY_FK2_OD, SD.DEST_LEVEL_FK2_OD ";
                strSql += CRLF + "      , COALESCE(SD.HOST_ERR_SEND_YN,'N') AS HOST_ERR_SEND_YN                          ";
                strSql += CRLF + "   FROM SC_DATA SD                                                                     ";
                strSql += CRLF + "  WHERE SD.WH_TYP = :WH_TYP                                                            ";
                strSql += CRLF + "    AND COALESCE(SD.OD_RQ_YN,'N')  = 'N'                                               ";
                strSql += CRLF + "    AND COALESCE(SD.CMD_RQ_YN,'N') = 'N'                                               ";
                strSql += CRLF + "    AND LTRIM(BTRIM(COALESCE(SD.ERR_CODE_RD,'')), '0') IN ('54','55','58','59')        ";
                strSql += CRLF + "  ORDER BY SD.SC_NO                                                                    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                int nCnt = _pBdb.ExcuteQry(strSql);
                if (nCnt < 0) { pRTN_MSG = "[ScDualStoreRetry]" + _pBdb.ErrMsg; return false; }
                if (nCnt == 0) return true;

                DataTable dt = _pBdb.mDtMain.Copy();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string strSC  = dt.Rows[i]["SC_NO"].ToString().Trim();
                    string strErr = dt.Rows[i]["ERR_CODE_RD"].ToString().Trim().TrimStart('0');
                    bool bFk2 = (strErr == "55" || strErr == "59");
                    string strFk = bFk2 ? "FK2" : "FK1";
                    int nLugg = 0; int.TryParse(dt.Rows[i]["LUGG_NO_" + strFk + "_OD"].ToString().Trim(), out nLugg);
                    if (nLugg <= 0) continue;

                    // 그 작업
                    strSql = "";
                    strSql += CRLF + " SELECT LUGG_NO, JOB_TYP, JOB_STATUS, START_LOCATION, DEST_LOCATION ";
                    strSql += CRLF + "   FROM JOB_MST                                                    ";
                    strSql += CRLF + "  WHERE WH_TYP = :WH_TYP                                           ";
                    strSql += CRLF + "    AND LTRIM(BTRIM(LUGG_NO), '0') = :LUGG_NO                       ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP",  DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = nLugg.ToString();
                    int nJob = _pBdb.ExcuteQry(strSql);
                    if (nJob < 0) { pRTN_MSG = "[ScDualStoreRetry]" + _pBdb.ErrMsg; return false; }

                    string strJobStatus = "", strDestLoc = "", strJobLuggDb = "";
                    if (nJob > 0)
                    {
                        strJobLuggDb = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                        strJobStatus = _pBdb.mDtMain.Rows[0]["JOB_STATUS"].ToString().Trim();
                        strDestLoc   = _pBdb.mDtMain.Rows[0]["DEST_LOCATION"].ToString().Trim();
                    }

                    if (strErr == "54" || strErr == "55")
                    {
                        #region 이중입고 : 도착 위치가 바뀌었으면 다시 지시한다
                        if (nJob == 0 || strJobStatus != ST_SC_RUN) continue;
                        if (dt.Rows[i]["JOB_TYP_OD"].ToString().Trim() != SC_CMD_STORE) continue;

                        string strBank = "0", strBay = "0", strLev = "0";
                        if (!ParseLocation(strDestLoc, ref strBank, ref strBay, ref strLev)) continue;

                        int nOdBank = 0, nOdBay = 0, nOdLev = 0;
                        int.TryParse(dt.Rows[i]["DEST_BANK_"  + strFk + "_OD"].ToString().Trim(), out nOdBank);
                        int.TryParse(dt.Rows[i]["DEST_BAY_"   + strFk + "_OD"].ToString().Trim(), out nOdBay);
                        int.TryParse(dt.Rows[i]["DEST_LEVEL_" + strFk + "_OD"].ToString().Trim(), out nOdLev);
                        if (int.Parse(strBank) == nOdBank && int.Parse(strBay) == nOdBay && int.Parse(strLev) == nOdLev)
                            continue;   // 아직 재지정이 안 왔다

                        strSql = "";
                        strSql += CRLF + " UPDATE SC_DATA                                       ";
                        strSql += CRLF + "    SET DEST_BANK_"  + strFk + "_OD = :DEST_BANK       ";
                        strSql += CRLF + "      , DEST_BAY_"   + strFk + "_OD = :DEST_BAY        ";
                        strSql += CRLF + "      , DEST_LEVEL_" + strFk + "_OD = :DEST_LEVEL      ";
                        if (bFk2)
                            strSql += CRLF + "      , USE_FK_OD        = '2'                    ";   // 포크#2 만 다시 내린다
                        strSql += CRLF + "      , WRITE_FLAG_OD    = '1'                        ";
                        strSql += CRLF + "      , OD_RQ_YN         = 'Y'                        ";
                        strSql += CRLF + "      , OD_USER_ID       = '" + OD_USER + "'          ";
                        strSql += CRLF + "      , OD_UPD_DT        = " + DbLang.SYSDATE + "     ";
                        strSql += CRLF + "  WHERE WH_TYP           = :WH_TYP                    ";
                        strSql += CRLF + "    AND SC_NO            = :SC_NO                     ";
                        strSql += CRLF + "    AND COALESCE(OD_RQ_YN,'N') = 'N'                  ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("DEST_BANK",  DbLang.VARCHAR).Value = strBank;
                        _pBdb.mComMain.Parameters.Add("DEST_BAY",   DbLang.VARCHAR).Value = strBay;
                        _pBdb.mComMain.Parameters.Add("DEST_LEVEL", DbLang.VARCHAR).Value = strLev;
                        _pBdb.mComMain.Parameters.Add("WH_TYP",     DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("SC_NO",      DbLang.VARCHAR).Value = strSC;
                        int n = _pBdb.ExcuteNonQry(strSql);
                        if (n < 0) { pRTN_MSG = "[ScDualStoreRetry]" + _pBdb.ErrMsg; return false; }
                        if (n == 0) continue;

                        string strMsg = "[이중입고 재지정]" + strSC + "호기(오류코드 " + strErr + ", 포크#" + (bFk2 ? "2" : "1") + ")에 입고를 다시 지시하였습니다."
                                      + " [작업번호:" + nLugg + "][이전 LOC:" + nOdBank.ToString("00") + "-" + nOdBay.ToString("000") + "-" + nOdLev.ToString("00") + "]"
                                      + "[재지정 LOC:" + strDestLoc + "]";
                        InsertLog(strWH_TYP, strMsg, "", "", nLugg.ToString(), ST_SC_RUN, "", strSC);
                        MakeMsg(strMsg);
                        #endregion
                    }
                    else
                    {
                        #region 공출고 : 재지정(20)됐거나 작업이 없어졌으면 크레인 에러를 푼다
                        if (nJob > 0 && strJobStatus != ST_SC_WAIT)
                        {
                            /*
                             * 공출고 자동 삭제 (2026-11-23)
                             *
                             *   클라이언트 리본 "로그 삭제 설정" 의 공출고 자동 삭제(DEL_HIS_SETTING 'empty_ret_auto_del')가 Y 이면
                             *   공출고로 선 작업을 여기서 지운다. 지우고 나면 다음 주기에 아래 "작업이 없어졌으면" 처리가
                             *   포크 데이터 삭제(DELFK)와 에러 해제(RESET)를 낸다.
                             *
                             *   상위 보고는 그대로 나간다. HOST 태스크가 에러 보고(E)를 집어 간 뒤(HOST_ERR_SEND_YN = 'Y')에만 지운다.
                             *   HOST 는 보고 전문의 위치를 JOB_MST 에서 읽으므로, 먼저 지우면 위치 없이 보고된다.
                             *   입고 지시 중의 공출고는 HOST 가 보고하지 않는 경우(SC 로직체크)라 지우지 않는다.
                             *   설정이 N 이면 전과 같이 상위의 재지정 / 취소를 기다린다.
                             */
                            if (strJobStatus != ST_SC_RUN) continue;
                            if (dt.Rows[i]["JOB_TYP_OD"].ToString().Trim() == SC_CMD_STORE) continue;
                            if (dt.Rows[i]["HOST_ERR_SEND_YN"].ToString().Trim() != "Y") continue;
                            if (IsEmptyRetAutoDel(strWH_TYP) == false) continue;

                            string strDelMsg = "";
                            _pBdb.BeginTrans();
                            if (DELETE_JOB_DATA(strJobLuggDb, strWH_TYP, ref strDelMsg) == false)
                            {
                                _pBdb.Rollback();
                                pRTN_MSG = "[공출고 자동 삭제]" + strSC + "호기 작업 " + nLugg + " 삭제 실패 : " + strDelMsg;
                                continue;
                            }
                            _pBdb.Commit();

                            string strAutoMsg = "[공출고 자동 삭제]" + strSC + "호기(오류코드 " + strErr + ")의 작업 " + nLugg
                                              + " 을 삭제하였습니다. (로그 삭제 설정 : 공출고 자동 삭제 = Y, 상위 에러 보고 뒤)";
                            InsertLog(strWH_TYP, strAutoMsg, "", "", nLugg.ToString(), strJobStatus, "", strSC);
                            MakeMsg(strAutoMsg);
                            continue;       // 크레인 풀기(DELFK -> RESET)는 다음 주기에 한다
                        }

                        // 먼저 포크 데이터를 지운다(DELFK). 크레인이 준비됐는지는 ITN_LUGG 가 0 인가로 보는데, 공출고로 선 작업의 번호가
                        // 거기 남아 있으면 에러를 풀어도 다음 지시가 영영 안 나간다. SC 태스크가 지운 뒤 다음 주기에 RESET 을 낸다.
                        int nItn = 0; int.TryParse(dt.Rows[i]["ITN_LUGG_" + strFk].ToString().Trim(), out nItn);
                        string strCmd = (nItn > 0) ? (bFk2 ? "DELFK12" : "DELFK1") : "RESET";

                        strSql = "";
                        strSql += CRLF + " UPDATE SC_DATA                        ";
                        strSql += CRLF + "    SET CMD_RQ_YN  = 'Y'               ";
                        strSql += CRLF + "      , CMD_RQ_ID  = '" + strCmd + "'    ";
                        strSql += CRLF + "      , OD_USER_ID = '" + OD_USER + "' ";
                        strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP           ";
                        strSql += CRLF + "    AND SC_NO      = :SC_NO            ";
                        strSql += CRLF + "    AND COALESCE(CMD_RQ_YN,'N') = 'N'  ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strSC;
                        int n = _pBdb.ExcuteNonQry(strSql);
                        if (n < 0) { pRTN_MSG = "[ScDualStoreRetry]" + _pBdb.ErrMsg; return false; }
                        if (n == 0) continue;

                        string strMsg = "[공출고 재지정]" + strSC + "호기(오류코드 " + strErr + ")의 작업 " + nLugg
                                      + (nJob > 0 ? " 이 재지정되어(SC 구동요구)" : " 이 없어져(취소)")
                                      + " 크레인에 " + (strCmd == "RESET" ? "에러 해제(RESET)" : "포크 데이터 삭제(" + strCmd + ")") + "를 지시하였습니다.";
                        InsertLog(strWH_TYP, strMsg, "", "", nLugg.ToString(), strJobStatus, "", strSC);
                        MakeMsg(strMsg);
                        #endregion
                    }
                }
                return true;
            }
            catch (Exception ex) { pRTN_MSG = "[ScDualStoreRetry]" + ex.Message; return false; }
        }

        // 공출고 자동 삭제 설정 (DEL_HIS_SETTING 의 TABLE_NAME 'empty_ret_auto_del', CYCLE 'Y' / 'N'). 못 읽으면 꺼진 것으로 본다. (2026-11-23)
        protected bool IsEmptyRetAutoDel(string strWH_TYP)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT COUNT(*) AS CNT FROM DEL_HIS_SETTING              ";
                strSql += CRLF + "  WHERE WH_TYP = :WH_TYP                                  ";
                strSql += CRLF + "    AND LOWER(BTRIM(TABLE_NAME)) = 'empty_ret_auto_del'   ";
                strSql += CRLF + "    AND UPPER(BTRIM(COALESCE(CYCLE,''))) = 'Y'            ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                if (_pBdb.ExcuteQry(strSql) <= 0) return false;
                return Convert.ToInt32(_pBdb.mDtMain.Rows[0]["CNT"]) > 0;
            }
            catch
            {
                return false;
            }
        }

        protected static bool CommonAlive()
        {
            int nBeat = s_nCommonBeat;
            return nBeat != 0 && unchecked(Environment.TickCount - nBeat) < 3000;
        }

        protected bool JOB_ACCEPT(string strWH_TYP, string strTitle, ref string pRTN_MSG, bool bCommon = false)
        {
            // @.공통 스레드가 살아 있으면 층 스레드는 접수하지 않는다. (공통 스레드 자신은 bCommon=true 로 부른다)
            if (!bCommon && CommonAlive()) { pRTN_MSG = ""; return true; }

            // @.다른 층 스레드가 이미 접수 중이면 이번 주기는 건너뛴다.
            //   (기다리지 않는다. 어차피 200ms 뒤에 다시 온다)
            if (!Monitor.TryEnter(m_objJobAcceptLock))
            {
                pRTN_MSG = "";
                return true;
            }

            try
            {
                try
                {
                    int nSelCnt = 0;
                    string strSql = "";

                    pRTN_MSG = strTitle;

                    strSql = "";
                    strSql += CRLF + " SELECT LUGG_NO, JOB_TYP, START_POS, DEST_POS, START_LOCATION, DEST_LOCATION ";
                    strSql += CRLF + "   FROM JOB_MST                 ";
                    strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP    ";
                    strSql += CRLF + "    AND JOB_STATUS = '" + ST_NEW + "' ";
                    strSql += CRLF + "  ORDER BY LUGG_NO              ";

                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

                    nSelCnt = _pBdb.ExcuteQry(strSql);
                    if (nSelCnt < 0)
                    {
                        pRTN_MSG += _pBdb.ErrMsg;
                        return false;
                    }
                    if (nSelCnt == 0)
                    {
                        pRTN_MSG = "";
                        return true;
                    }

                    string strMsg = "";
                    string strMagLogs = "";     // @.매거진 작업 접수 기록(Commit 뒤에 탭으로 보낸다)
                    string strRackLogs = "";    // @.호기간 이동/랙투랙 접수 기록(Commit 뒤에 탭으로 보낸다)
                    _pBdb.BeginTrans();

                    for (int i = 0; i < nSelCnt; i++)
                    {
                        string strLUGG_NO = _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                        string strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                        string strACC_START = _pBdb.mDtMain.Rows[i]["START_POS"].ToString().Trim();
                        string strACC_DEST = _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString().Trim();

                        // CV 에서 시작하는 작업인가
                        // @.입고 / 이동은 CV 에서 시작한다. 반자동(11 / 10)도 같다.
                        bool bCvFirst = IsStoJobType(strJOB_TYP) || IsMoveJobType(strJOB_TYP);
                        string strNext = bCvFirst ? ST_CV_WAIT : ST_SC_WAIT;

                        if (UPDATE_JOB_DATA(strNext, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            return false;
                        }

                        strMsg += (strMsg == "" ? "" : ", ") + strLUGG_NO + "→" + strNext;
                        if (strACC_START == MAG_STN || strACC_DEST == MAG_STN)
                            strMagLogs += "|[매거진] 작업 " + strLUGG_NO + " 접수 : CV 구동대기로 준비 (99→" + strNext + ") - " + MagJobName(strJOB_TYP, strACC_START, strACC_DEST) + " [출발:" + strACC_START + " 도착:" + strACC_DEST + "]";
                        if (IsA2AJobType(strJOB_TYP))
                            strRackLogs += "|[호기간 이동 1/6] 신규 작업 접수 (99→" + strNext + ") [작업번호:" + strLUGG_NO + "][출발:" + strACC_START + " " + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString().Trim() + "][도착:" + strACC_DEST + " " + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString().Trim() + "]";
                        else if (IsR2RJobType(strJOB_TYP))
                            strRackLogs += "|[랙투랙 1/3] 신규 작업 접수 (99→" + strNext + ") [작업번호:" + strLUGG_NO + "][호기:" + strACC_START + "][출발LOC:" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString().Trim() + " → 도착LOC:" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString().Trim() + "]";
                        InsertLog(SCH_WH_TYP, strTitle + "작업 " + strLUGG_NO + " 접수 (상태 " + strNext + ")",
                                  "", "", strLUGG_NO, strNext, "", "", false);
                    }

                    pRTN_MSG = strTitle + "신규 작업을 접수했습니다. [" + strMsg + "]";
                    _pBdb.Commit();
                    foreach (string strMag in strMagLogs.Split('|'))
                        if (strMag != "") MakeMagMsg(strMag);
                    foreach (string strRack in strRackLogs.Split('|'))
                        if (strRack != "") MakeRackMsg(strRack);
                    return true;
                }
                catch (Exception ex)
                {
                    pRTN_MSG = strTitle + ex.ToString();
                    _pBdb.Rollback();
                    return false;
                }
            }
            finally
            {
                Monitor.Exit(m_objJobAcceptLock);
            }
        }

        public bool NEW_JOB_ORDER(string strWH_TYP,
                                 string strPLC_NO,
                             ref string pRTN_MSG)
        {
            try
            {
                string strLUGG_NO = "";
                string strTRACK_NO = "";
                string strJOB_START_POS = "";
                string strJOB_DEST_POS = "";
                string strJOB_DEST_LOC = "";
                string strPRODUCT_SIZE = "";
                string strDestPos = "";
                string strSENSOR1 = "";         // 1단감지
                string strSENSOR2 = "";         // 2단감지
                int nJobType = 1;
                int nSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[NEW_JOB_ORDER]";

                strSql = "";
                strSql += cDefApp.CRLF + " SELECT CD.*, JM.*                            ";
                strSql += cDefApp.CRLF + "   FROM CV_DATA CD                            ";
                strSql += cDefApp.CRLF + "  INNER JOIN JOB_MST JM                       ";
                strSql += cDefApp.CRLF + "     ON CD.HOST_STN_NO = JM.START_POS         ";
                strSql += cDefApp.CRLF + "    AND JM.JOB_STATUS = '" + ST_CV_WAIT + "'      ";   // 10 = CV 구동대기. 신규('99')는 JOB_ACCEPT 가 10/20 으로 나눈다
                strSql += cDefApp.CRLF + "  WHERE COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0')    IN ('0','0000')   ";
                strSql += cDefApp.CRLF + "    AND CD.STO_READY_RD 	= '1'               ";
                strSql += cDefApp.CRLF + "    AND CD.SENSOR0_DATA_RD = '1'              ";
                strSql += cDefApp.CRLF + "    AND CD.AUTO_MODE_RD 	= '1'               ";
                strSql += cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')     ";
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_YN		= 'N'               ";
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_FLAG		= 'N'               ";
                strSql += cDefApp.CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')               ";
                strSql += cDefApp.CRLF + "    AND CD.WH_TYP		    = :WH_TYP           ";
                strSql += cDefApp.CRLF + "    AND 0 = (SELECT COUNT(*)                  ";
                strSql += cDefApp.CRLF + "               FROM JOB_MST                   ";
                strSql += cDefApp.CRLF + "              WHERE LUGG_NO = CD.LUGG_NO_RD)  ";
                strSql += cDefApp.CRLF + "  LIMIT 1                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();
                for (int i = 0; i < nSelCnt; i++)
                {
                    DataTable dtDestPos = new DataTable();

                    strTRACK_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    strPRODUCT_SIZE = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strJOB_DEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strJOB_DEST_LOC = "" + _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_LOCATION"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strJOB_START_POS = "" + _pBdb.mDtMain.Rows[i]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_POS"].ToString();

                    strSENSOR1 = "" + _pBdb.mDtMain.Rows[i]["SENSOR1_DATA_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SENSOR1_DATA_RD"].ToString();
                    strSENSOR2 = "" + _pBdb.mDtMain.Rows[i]["SENSOR2_DATA_RD"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SENSOR2_DATA_RD"].ToString();

                    // 명령 대상 PLC 는 조회 행(CV_DATA)에서 읽어 사용 (스레드 ID 아님)
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();

                    // 설비타스크에 작업지시
                    if (UPDATE_CV_DATA(nJobType.ToString(), strPRODUCT_SIZE, "0", strJOB_DEST_POS, "0", strLUGG_NO, strWH_TYP, strPLC_NO, strTRACK_NO, "", ref pRTN_MSG) == false)
                    {
                        m_strRtnMsg = pRTN_MSG;
                        _pBdb.Rollback();
                        throw new Exception(m_strRtnMsg);
                    }

                    // 작업상태 변경 
                    if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, nJobType.ToString(), ref pRTN_MSG) == false)
                    {
                        m_strRtnMsg = pRTN_MSG;
                        _pBdb.Rollback();
                        throw new Exception(m_strRtnMsg);
                    }

                    pRTN_MSG = strFunction + "TRACK " + strTRACK_NO + "번[입고대]에서 CV_TASK를 통해서 작업 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                    _pBdb.Commit();
                    InsertLog(SCH_WH_TYP, strRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strJOB_START_POS, strDestPos);
                    return true;
                }
                pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                m_strRtnMsg = ex.ToString();
                _pBdb.Rollback();
                throw new Exception(m_strRtnMsg);
            }
        }

        public bool ARRIVE_CV(string strWH_TYP,
                              string strPLC_NO,
                          ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[ARRIVE_CV]";

                // BCR 도착.
                strSql = "";
                strSql += cDefApp.CRLF + " SELECT JM.*, CD.*                                ";
                strSql += cDefApp.CRLF + "   FROM CV_DATA CD                                ";
                strSql += cDefApp.CRLF + "  INNER JOIN JOB_MST JM                           ";
                strSql += cDefApp.CRLF + "     ON CD.WH_TYP             = JM.WH_TYP 	    ";
                strSql += cDefApp.CRLF + "    AND CD.HOST_STN_NO       = JM.DEST_POS 	    ";
                strSql += cDefApp.CRLF + "    AND CD.LUGG_NO_RD         = JM.LUGG_NO        ";
                // ※ PLC_NO 필터 제거 (2026-07-11) : 스케줄러는 전체 PLC 를 관장한다.
                strSql += cDefApp.CRLF + "  WHERE CD.WH_TYP		        = :pWH_TYP          ";
                strSql += cDefApp.CRLF + "    AND CD.RET_READY_RD 	    = '1'               ";   // 출고대 READY ON
                strSql += cDefApp.CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'               ";   // 자동모드
                strSql += cDefApp.CRLF + "    AND CD.OD_RQ_YN		    = 'N'               ";
                strSql += cDefApp.CRLF + "    AND JM.JOB_STATUS 	    = '11'             ";   // 11 = CV 구동중
                strSql += cDefApp.CRLF + "    AND JM.DEST_POS Is not null                   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("pWH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strBCR_TOP = "";
                string strBCR_BOTTOM = "";
                string strMC_NO = "";
                string strCOMMING_DEST_TR = "";
                _pBdb.BeginTrans();

                for (int i = 0; i < nSelCnt; i++)
                {
                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRACK_NO"].ToString();
                    strBCR_TOP = "" + _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_TOP"].ToString();
                    strBCR_BOTTOM = "" + _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["BCR_BOTTOM"].ToString();
                    strMC_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    // ※ COMMING_DEST_TR(도착 후 후속 반출 목적지)은 세미피니시 스키마 전용 컬럼 -
                    //    KET 현장 CV_DATA 에는 없으므로 존재할 때만 읽는다 (없으면 '0' = 후속 지시 없음)
                    strCOMMING_DEST_TR = _pBdb.mDtMain.Columns.Contains("COMMING_DEST_TR") == false ? "0"
                        : ("" + _pBdb.mDtMain.Rows[i]["COMMING_DEST_TR"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["COMMING_DEST_TR"].ToString());

                    //// 상위 TASK에 도착 보고 - 추후확인이 필요하다...
                    if (UPDATE_IF_LUGG_STA(strWH_TYP,
                                            strLUGG_NO,
                                            "90",           //  strJOB_STATUS,      <= 90: 정상 완료(IF_LUGG_STA의 WRK_STA 값임)
                                        ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    // 도착보고가 성공하면 - 작업 삭제 
                    if (DELETE_JOB_DATA(strLUGG_NO,
                                        strWH_TYP,
                                    ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                }

                pRTN_MSG = strFunction + "TRACK " + strMC_NO + "번[출고대]에서 HOST_TASK를 통해서 완료보고 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                _pBdb.Commit();

                InsertLog(SCH_WH_TYP, strRTN_MSG, "", "", strLUGG_NO, "19");

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        public bool CHECK_CV_RETHS(string strWH_TYP,
                                   string strPLC_NO,
                               ref string pRTN_MSG)
        {
            try
            {
                int nMainSelCnt = 0;
                string strSql = "";

                string strFunction = pRTN_MSG = "[CHECK_CV_RETHS]";

                strSql = "";
                strSql += CRLF + " SELECT  JM.*, CD.*, SHD.*                          ";
                strSql += CRLF + "   FROM  JOB_MST JM                                 ";
                strSql += CRLF + "  INNER  JOIN CV_DATA CD                            ";
                strSql += CRLF + "     ON  JM.WH_TYP             = CD.WH_TYP          ";
                strSql += CRLF + "    AND  JM.HS_TRACK_NO        = CD.MC_NO           ";
                strSql += CRLF + "   LEFT  OUTER       JOIN        SC_HS_DEF SHD      ";
                strSql += CRLF + "     ON  JM.WH_TYP             = SHD.WH_TYP         ";
                strSql += CRLF + "    AND  JM.HS_TRACK_NO        = SHD.HS_MC_NO       ";
                // ※ LIKE 방향 수정 (2026-07-11) : 서비스 가능 출고대 목록(DEST_DEF_DAT='103, 104, 105')
                //    안에 작업 목적지(DEST_POS)가 포함되는지 판정
                strSql += CRLF + "    AND  SHD.DEST_DEF_DAT   like '%' " + DbLang.II + " JM.DEST_POS " + DbLang.II + " '%'";
                // ※ PLC_NO 필터 제거 (2026-07-11) : 스케줄러는 전체 PLC 를 관장한다.
                //    (기존에는 스레드 ID(SCH_GR01=50)가 :PLC_NO 로 전달되어 무동작이었음)
                strSql += CRLF + "  WHERE  CD.WH_TYP             = :WH_TYP            ";
                strSql += CRLF + "    AND  COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')                ";    // 트랙 일시정지가 아니어야 함! - 안보는게 나을듯!
                strSql += CRLF + "    AND  CD.SENSOR0_DATA_RD    = '1'                ";
                strSql += CRLF + "    AND  JM.JOB_STATUS 	     = '29'               ";    // 도착 보고 완료
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nMainSelCnt = _pBdb.ExcuteQry(strSql);
                if (nMainSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }

                if (nMainSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strJOB_TYP = "";
                string strTRAY_TYP = "";
                string strTRAY_LEV = "";
                string strDEST_POS = "";
                string strIS_TURN = "";
                string strLUGG_NO = "";
                string strSTART_POS = "";
                string strSC_NO = "";
                string strSTART_LOCATION = "";
                string strWAIT_TRACK = "";
                string strLOT_NO = "";
                DataTable dtDestPos = new DataTable();

                for (int i = 0; i < nMainSelCnt; i++)
                {
                    _pBdb.BeginTrans();

                    strJOB_TYP = _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();
                    strTRAY_TYP = "" + _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PRODUCT_SIZE"].ToString();
                    strTRAY_LEV = "" + _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TRAY_LEV"].ToString();
                    strDEST_POS = "" + _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["DEST_POS"].ToString();
                    strIS_TURN = "" + _pBdb.mDtMain.Rows[i]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["TURN"].ToString();
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strWH_TYP = "" + _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WH_TYP"].ToString();
                    strPLC_NO = "" + _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["PLC_NO"].ToString();
                    strSTART_POS = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    strSC_NO = "" + _pBdb.mDtMain.Rows[i]["SC_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["SC_NO"].ToString();
                    strSTART_LOCATION = "" + _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["START_LOCATION"].ToString();
                    strWAIT_TRACK = "" + _pBdb.mDtMain.Rows[i]["WAIT_TRACK"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["WAIT_TRACK"].ToString();

                    //if (Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Move ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Ret ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.PRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.OtherRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.FireRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.RackRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.Aisle2Aisle ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.ManualRet ||
                    //    Convert.ToInt16(strJOB_TYP) == (int)cDefApp.eJOBTYP.ManualPickingRet) //이정민 추가
                    //{
                    if (Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeAutoRet ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeAutoPR ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeAutoW2W ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeAutoMove ||    // KET 현장 구조상 이렇게 되지는 않음
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeSemiRet ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeSemiPR ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeSemiW2W ||
                        Convert.ToInt16(strJOB_TYP) == (int)EN_JOB_TYPE.enJobTypeSemiMove ) 
                    {
                        // 대기 트랙의 정보가 있다면...
                        //   ※ '0' 체크 추가 (2026-07-12) : WAIT_TRACK 미정의(NULL)시 기본값 '0' 이
                        //     목적지를 덮어쓰는 버그 수정 - 3층 출고 HS(hs04)는 대기 트랙이 없음
                        if (strWAIT_TRACK != "" && strWAIT_TRACK != null && strWAIT_TRACK != "0")
                            strDEST_POS = strWAIT_TRACK;

                        // C/V에 목적지정보쓰기.
                        if (UPDATE_CV_DATA(strJOB_TYP
                                         , strTRAY_TYP
                                         , strTRAY_LEV
                                         , strDEST_POS          // strWAIT_TRACK        
                                         , strIS_TURN
                                         , strLUGG_NO
                                         , strWH_TYP
                                         , strPLC_NO
                                         , strSTART_POS
                                         , strLOT_NO    //파쇄기 라인 출고 품목 표시를 위한 수정 (조한성. 0302)
                                         , ref pRTN_MSG) == false)
                        {
                            //return false;
                            _pBdb.Rollback();
                            continue;
                        }
                        // 작업시작(구동중)
                        if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                        {
                            _pBdb.Rollback();
                            continue;
                        }
                        pRTN_MSG = strFunction + "TRACK " + strSTART_POS + "번[출고 H/S]에 CV_TASK를 통해 DATA 기록 요청하였습니다. [작업번호:" + strLUGG_NO + "]";

                        _pBdb.Commit();

                        InsertLog(SCH_WH_TYP, strRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strSTART_POS, strDEST_POS);
                        continue;

                    }
                    _pBdb.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        protected bool CV_STO_START_PLC(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT CD.*, JM.*                            ";
                strSql += CRLF + "   FROM CV_DATA CD                            ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                       ";
                strSql += CRLF + "     ON CD.HOST_STN_NO = JM.START_POS         ";
                strSql += CRLF + "    AND JM.JOB_STATUS = '" + ST_CV_WAIT + "'      ";   // 10 = CV 구동대기. 신규('99')는 JOB_ACCEPT 가 10/20 으로 나눈다
                strSql += CRLF + "  WHERE CD.PLC_NO         = :CV_PLC           ";   // 3층 해당 PLC 한정 (ECS m_nNum 게이트)
                /*
                 * @.출발 + 도착을 같이 하는 작업대 (피킹대 / 사이즈체크)  원본 ECS 의 CLib::IsStartNDestStation
                 *   원본은 이 작업대에서 "입고대 준비 신호(StoStation, PLC 푸시버튼 = 피킹완료)" 가 켜졌을 때만 출발 지시를 쓴다.
                 *   도착대 준비 신호(RetStation) 는 도착 완료 판정에만 쓴다.
                 *   예전에는 이 작업대에서도 도착대 준비 신호로 출발 지시를 써서, 도착하자마자(푸시버튼 전에) PLC 에 작업이 기록됐다.
                 *   그래서 이 작업대는 푸시버튼(STO_READY_RD)만으로 출발시키고, STN_KIND 비트와 작업번호 유무는 보지 않는다.
                 */
                string strStartNDest = cDefApp.START_N_DEST_STN_SQL;
                strSql += CRLF + "    AND (   (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_STO) + " <> 0 AND CD.STO_READY_RD = '1'   ";
                strSql += CRLF + "             AND CD.HOST_STN_NO NOT IN (" + strStartNDest + ")                                           ";
                strSql += CRLF + "             AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0') IN ('0','0000'))                                                        ";
                strSql += CRLF + "         OR (CD.HOST_STN_NO IN (" + strStartNDest + ") AND CD.STO_READY_RD = '1')  ";
                /*
                 * @.3층 팔레트 매거진(207) : 원본 StartInvokeCheck3 은 입고대 준비 신호(만재 / 출발 가능)와 작업번호 0 만 보고 출발시킨다.
                 *   현장 DB 는 207 의 STN_KIND 가 64(도착대)뿐이라 위의 입고대 비트 조건을 못 탄다. 그래서 스테이션 번호로 잡는다.
                 *   도착대 준비 신호(RetStation)는 도착 판정에만 쓰므로 출발 조건에서는 뺀다.
                 */
                strSql += CRLF + "         OR (CD.HOST_STN_NO IN (" + cDefApp.START_PM_STN_SQL + ") AND CD.STO_READY_RD = '1'  ";
                strSql += CRLF + "             AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0') IN ('0','0000'))  ";
                strSql += CRLF + "         OR (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_ARV) + " <> 0 AND CD.RET_READY_RD = '1'  ";
                strSql += CRLF + "             AND CD.HOST_STN_NO NOT IN (" + strStartNDest + "," + cDefApp.START_PM_STN_SQL + "))  ";
                // @.START_NO_READY_STN : 준비 신호 없이 센서 + 작업번호 0 만으로 출발시키는 작업대 (아래 센서/자동/에러 조건은 그대로 AND)
                string strNoReadyIn = GetStartNoReadyInList();
                if (strNoReadyIn != "")
                    strSql += CRLF + "         OR (CD.HOST_STN_NO IN (" + strNoReadyIn + ") AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0') IN ('0','0000')) ";
                strSql += CRLF + "       )  ";
                strSql += CRLF + "    AND (CD.SENSOR0_DATA_RD = '1' OR CD.HOST_STN_NO IN (" + cDefApp.START_PM_STN_SQL + "))   ";   // 매거진은 화물 센서를 보지 않는다(원본)
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	= '1'               ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')     ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		= 'N'               ";
                strSql += CRLF + "    AND CD.OD_RQ_FLAG		= 'N'               ";
                strSql += CRLF + "    AND COALESCE(CD.TR_PAUSE_RD,'0') IN ('0','')               ";
                strSql += CRLF + "    AND CD.WH_TYP		    = :WH_TYP           ";
                strSql += CRLF + "    AND 0 = (SELECT COUNT(*)                  ";
                strSql += CRLF + "               FROM JOB_MST                   ";
                strSql += CRLF + "              WHERE LUGG_NO = CD.LUGG_NO_RD)  ";
                strSql += CRLF + "  LIMIT 1                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    // @.설비 조건이 전부 AND 라 하나만 어긋나도 0건이 된다.
                    //   기다리는 작업이 있는데도 0건이면 어느 칸이 막는지 알린다.
                    REPORT_CV_START_BLOCKED(strWH_TYP, strCV_PLC, strTitle);
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                string strJOB_TYP = "" + _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "1" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                string strPRODUCT_SIZE = "" + _pBdb.mDtMain.Rows[0]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PRODUCT_SIZE"].ToString();
                string strTRAY_LEV = "" + _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString();
                string strJOB_DEST_POS = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();
                string strJOB_START_POS = "" + _pBdb.mDtMain.Rows[0]["START_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["START_POS"].ToString();

                // @.CV 에 지시할 때는 물리 트랙번호(CV_DATA.MC_NO)를 써야 한다.
                //   START_POS 는 HOST 가 쓰는 스테이션 번호(101)이고, 설비를 움직이는
                //   번호는 MC_NO(217)다. 조회를 HOST_STN_NO 로 맞춰 뒀으므로 여기서
                //   같은 행의 MC_NO 를 꺼내 쓴다. (예전에는 101 로 UPDATE 해서 0건이
                //   갱신되고 "설비 미준비" 로 조용히 재시도만 반복했다)
                string strCV_MC_NO = _pBdb.mDtMain.Rows[0]["MC_NO"].ToString();
                if (strCV_MC_NO == "") strCV_MC_NO = strJOB_START_POS;
                string strIS_TURN = "" + _pBdb.mDtMain.Rows[0]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TURN"].ToString();

                _pBdb.BeginTrans();

                if (UPDATE_CV_DATA(strJOB_TYP, strPRODUCT_SIZE, strTRAY_LEV, GfCvDestPos(strJOB_DEST_POS), strIS_TURN,
                                   strLUGG_NO, strWH_TYP, strCV_PLC, strCV_MC_NO, "", ref pRTN_MSG) == false)
                {
                    // @.골라 놓고 쓰지를 못했다. 고르는 조건과 쓰는 조건이 달라
                    //   그 사이에 설비가 바뀌었거나, MC_NO 가 어긋나거나, 오류가 서 있다.
                    //   전에는 사유 없이 재시도만 반복했다.
                    _pBdb.Rollback();
                    ReportOnChange("CVSTART_UPD_" + strCV_PLC,
                                   strTitle + "TRACK " + strCV_MC_NO + " 에 지시를 쓰지 못했다(0건 갱신). "
                                 + "[작업번호:" + strLUGG_NO + "][출발:" + strJOB_START_POS + "] "
                                 + "OD_RQ_YN / ERROR_CODE / MC_NO 를 보십시오.");
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }

                if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + "TRACK " + strJOB_START_POS + "번[출발 작업대]에서 CV_TASK를 통해서 작업 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strJOB_START_POS, strJOB_DEST_POS);
                if (strJOB_START_POS == MAG_STN || strJOB_DEST_POS == MAG_STN)
                    MakeMagMsg("[매거진] 작업 " + strLUGG_NO + " 구동 지시 : CV_TASK 가 PLC 에 기록하도록 DB 에 썼습니다 (10→" + ST_CV_RUN + ") - " + MagJobName(strJOB_TYP, strJOB_START_POS, strJOB_DEST_POS) + " [출발:" + strJOB_START_POS + " 도착:" + strJOB_DEST_POS + "] TRACK " + strCV_MC_NO);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 3 : 도착보고 (ECS ArrivedCheck2/5 - ARRIVE_CV 의 PLC 한정판)
        //   1층 / 3층 / BOX 가 같이 쓴다. 로그에 층을 박지 않는다 - 앞머리의
        //   [ArrivedCheck3] / [ArrivedCheck6] 가 어느 라인인지를 이미 말해 준다.
        //   (전에는 "1층 출고대" 로 박혀 있어 3층 도착을 1층이라고 찍어 햷갈릴 수 있었다)
        //   목적지 트랙 도착(RET_READY + 작업번호 일치) → 상위 보고 + 작업 삭제(HIS 이관)
        // ─────────────────────────────────────────────────────────────────
        /// <summary>도착하면 트랙 작업번호를 지우는 작업대인지 (ENV_IOSCH.INI [SCH] DELETE_DATA_STN, 기본 207,251)</summary>
        /// <summary>준비 신호 없이 출발시키는 작업대 SQL IN 목록 ('251','...'). 없으면 빈 문자열. (숫자만 허용)</summary>
        protected string GetStartNoReadyInList()
        {
            string strOut = "";
            foreach (string s in cDefApi.GsGetStartNoReadyStn().Split(','))
            {
                string t = s.Trim();
                if (t == "") continue;
                bool bDigit = true;
                foreach (char c in t) if (c < '0' || c > '9') { bDigit = false; break; }
                if (!bDigit) continue;
                strOut += (strOut == "" ? "" : ",") + "'" + t + "'";
            }
            return strOut;
        }

        protected bool IsStartNoReadyStation(string strHostStn)
        {
            if (strHostStn == null || strHostStn.Trim() == "") return false;
            foreach (string s in cDefApi.GsGetStartNoReadyStn().Split(','))
                if (s.Trim() == strHostStn.Trim()) return true;
            return false;
        }

        protected bool IsDeleteDataStation(string strHostStn)
        {
            if (strHostStn == null || strHostStn.Trim() == "") return false;
            string strList = cDefApi.GsGetDeleteDataStn();
            foreach (string s in strList.Split(','))
                if (s.Trim() == strHostStn.Trim()) return true;
            return false;
        }

        /*
         * 보조 청소 (원본 EcsSv/Cv.cpp:1075 "작업 없이 DATA 만 남음" 분기)
         *
         *   삭제 작업대에 화물이 서 있고 트랙에 작업번호가 남았는데 그 작업이 JOB_MST 에
         *   없으면(완료보고까지 끝나 지워짐) 작업번호를 0 으로 지운다.
         *   도착 처리 때 못 지웠거나, 태스크 재기동 등으로 남은 것을 치운다.
         *   화물 센서 ON 이고 출고대 준비가 켜진 자리만 본다. 빈 트랙에는 쓰지 않는다. (EHS)
         */
        protected bool CV_DELETE_DATA_STN(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                pRTN_MSG = "";
                string strList = cDefApi.GsGetDeleteDataStn();
                if (strList == "") return true;
                string strIn = "";
                foreach (string s in strList.Split(','))
                    if (s.Trim() != "") strIn += (strIn == "" ? "" : ",") + "'" + s.Trim().Replace("'", "") + "'";
                if (strIn == "") return true;

                string strSql = "";
                strSql += CRLF + " SELECT CD.MC_NO, CD.HOST_STN_NO, CD.LUGG_NO_RD, CD.JOB_TYP_RD           ";
                strSql += CRLF + "   FROM CV_DATA CD                                                ";
                strSql += CRLF + "  WHERE CD.WH_TYP          = :WH_TYP                              ";
                strSql += CRLF + "    AND CD.PLC_NO          = :CV_PLC                              ";
                strSql += CRLF + "    AND CD.HOST_STN_NO    IN (" + strIn + ")                      ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD = '1'                                  ";
                strSql += CRLF + "    AND CD.RET_READY_RD    = '1'                                  ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD    = '1'                                  ";
                strSql += CRLF + "    AND CD.OD_RQ_YN        = 'N'                                  ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0') NOT IN ('0','0000')";
                strSql += CRLF + "    AND NOT EXISTS (SELECT 1 FROM JOB_MST JM                      ";
                strSql += CRLF + "                     WHERE JM.WH_TYP = CD.WH_TYP                  ";
                strSql += CRLF + "                       AND JM.LUGG_NO = CD.LUGG_NO_RD)            ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;
                int nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0) { pRTN_MSG = strTitle + _pBdb.ErrMsg; return false; }
                if (nSelCnt == 0) return true;

                DataTable dt = _pBdb.mDtMain.Copy();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string strMc  = dt.Rows[i]["MC_NO"].ToString().Trim();
                    string strStn = dt.Rows[i]["HOST_STN_NO"].ToString().Trim();
                    string strLug = dt.Rows[i]["LUGG_NO_RD"].ToString().Trim();
                    string strTyp = dt.Rows[i]["JOB_TYP_RD"].ToString().Trim();
                    if (strTyp == "") strTyp = "0";
                    string strMsg = "";
                    _pBdb.BeginTrans();
                    if (UPDATE_CV_DATA(strTyp, "", "", strStn, "", "0", strWH_TYP, strCV_PLC, strMc, "", ref strMsg) == false)
                    {
                        _pBdb.Rollback();
                        continue;
                    }
                    _pBdb.Commit();
                    pRTN_MSG = strTitle + "TRACK " + strMc + "번[" + strStn + "] 에 작업 없는 작업번호(" + strLug + ")가 남아 0 으로 지우도록 CV_TASK 에 요청했습니다.";
                    InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLug, "", strMc);
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.Message;
                _pBdb.Rollback();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 사이즈 체커 감시 (2026-11-09)
        //   사이즈 체커 작업대(HOST 205, 206, 209, 214, 215) 트랙의 READY / AUTO / ERROR / OD_RQ / 작업번호가 바뀔 때마다
        //   SizeChk_yyyyMMdd.log 에 시각과 함께 적는다. 에러 해제 뒤 도착 조건이 언제 맞는지 보려는 로그이다.
        //   또 에러가 보인 트랙은 30초 동안 "에러 해제 직후"로 기억해, 도착 완료의 1초 안정 대기를 건너뛴다.
        // ─────────────────────────────────────────────────────────────────
        private static readonly object g_szLock = new object();
        private static readonly Dictionary<string, int> g_szErrTick = new Dictionary<string, int>();
        private static readonly Dictionary<string, string> g_szLast = new Dictionary<string, string>();
        private const string SZ_STN_LIST = "'205','206','209','214','215'";

        private static void SzLog(string strMsg)
        {
            try
            {
                lock (g_szLock)
                {
                    System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "SizeChk_" + DateTime.Now.ToString("yyyyMMdd") + ".log"),
                        DateTime.Now.ToString("HH:mm:ss.fff") + " " + strMsg + Environment.NewLine);
                }
            }
            catch { }
        }

        // 에러가 보인 뒤 30초 안인 사이즈 체커 트랙 번호 목록 ('312','304' 꼴, 없으면 "")
        private static string SzBypassList()
        {
            string strRet = "";
            lock (g_szLock)
            {
                foreach (KeyValuePair<string, int> kv in g_szErrTick)
                {
                    if (unchecked(Environment.TickCount - kv.Value) < 30000)
                        strRet += (strRet == "" ? "" : ",") + "'" + kv.Key.Replace("'", "") + "'";
                }
            }
            return strRet;
        }

        public bool SizeChkWatch(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        {
            pRTN_MSG = "";
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT CD.MC_NO, CD.HOST_STN_NO, CD.RET_READY_RD, CD.AUTO_MODE_RD, CD.ERROR_CODE,   ";
                strSql += CRLF + "        CD.OD_RQ_YN, CD.LUGG_NO_RD, CD.READ_UPD_DT                                    ";
                strSql += CRLF + "   FROM CV_DATA CD                                                                    ";
                strSql += CRLF + "  WHERE CD.WH_TYP       = :WH_TYP                                                     ";
                strSql += CRLF + "    AND CD.HOST_STN_NO IN (" + SZ_STN_LIST + ")                                       ";
                strSql += CRLF + "  ORDER BY CD.MC_NO                                                                   ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                int nCnt = _pBdb.ExcuteQry(strSql);
                if (nCnt < 0) { pRTN_MSG = "[SizeChkWatch]" + _pBdb.ErrMsg; return false; }

                for (int i = 0; i < nCnt; i++)
                {
                    DataRow r = _pBdb.mDtMain.Rows[i];
                    string strMc    = r["MC_NO"].ToString().Trim();
                    string strStn   = r["HOST_STN_NO"].ToString().Trim();
                    string strErr   = r["ERROR_CODE"].ToString().Trim();
                    string strCur   = "READY " + r["RET_READY_RD"].ToString().Trim()
                                    + " AUTO " + r["AUTO_MODE_RD"].ToString().Trim()
                                    + " ERR " + (strErr == "" ? "-" : strErr)
                                    + " OD_RQ " + r["OD_RQ_YN"].ToString().Trim()
                                    + " 작업 " + r["LUGG_NO_RD"].ToString().Trim().TrimStart('0');
                    string strUpd = r["READ_UPD_DT"] == DBNull.Value ? "-" : Convert.ToDateTime(r["READ_UPD_DT"]).ToString("HH:mm:ss.fff");

                    bool bErr = !(strErr == "" || strErr.TrimStart('0') == "");
                    string strPrev;
                    lock (g_szLock)
                    {
                        if (bErr) g_szErrTick[strMc] = Environment.TickCount;
                        if (!g_szLast.TryGetValue(strMc, out strPrev)) strPrev = null;
                        g_szLast[strMc] = strCur;
                    }
                    if (strPrev == null)
                        SzLog("트랙 " + strMc + "(작업대 " + strStn + ") 감시 시작 : " + strCur + " | PLC값 시각 " + strUpd);
                    else if (strPrev != strCur)
                        SzLog("트랙 " + strMc + "(작업대 " + strStn + ") [" + strPrev + "] -> [" + strCur + "] | PLC값 시각 " + strUpd);
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = "[SizeChkWatch]" + ex.Message;
                return false;
            }
        }

        protected bool CV_ARRIVE_PLC(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nA0 = Environment.TickCount;
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                /*
                 * 도착 자리 맞추기 (원본 ECS CLib::IsArrivedToRetStation)
                 *
                 *   P-BOX 자동입고 대기대 221(트랙 419, 바깥) 과 222(트랙 420, 안쪽) 은
                 *   한 크레인의 트윈 자리라 원본이 둘을 한 자리로 본다.
                 *
                 *   상위는 이동 작업의 목적지를 늘 221 로 준다. 그런데 뒤에 오는
                 *   화물이 없으면 PLC 가 화물을 안쪽(222, 트랙 420)으로 밀어 넣는다.
                 *   예전에는 CD.HOST_STN_NO = JM.DEST_POS 로만 맞춰, 화물이 420 으로
                 *   밀려 가면 222 != 221 이 되어 도착이 영영 안 잡혔다.
                 */
                strSql += CRLF + " SELECT JM.*, CD.*                                ";
                strSql += CRLF + "   FROM CV_DATA CD                                ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                           ";
                strSql += CRLF + "     ON CD.WH_TYP             = JM.WH_TYP 	    ";
                strSql += CRLF + "    AND (CD.HOST_STN_NO       = JM.DEST_POS       ";
                strSql += CRLF + "         OR (JM.DEST_POS     IN ('221','222')     ";
                strSql += CRLF + "             AND CD.HOST_STN_NO IN ('221','222')))";
                strSql += CRLF + "    AND CD.LUGG_NO_RD         = JM.LUGG_NO        ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP           ";
                strSql += CRLF + "    AND CD.PLC_NO             = :CV_PLC           ";   // 3층 해당 PLC 한정 (ECS m_nNum 게이트)
                strSql += CRLF + "    AND (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_RET | cDefApp.STN_KIND_ARV) + " <> 0)  ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '1'               ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'               ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'               ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '11'              ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null                   ";
                // @.에러 중인 트랙에서는 도착 완료를 내지 않는다 (2026-11-08).
                //   3층 피킹대 -> 사이즈 체커 이동에서 PLC 가 도착 READY(D551)를 먼저 올리고 한두 스캔 뒤
                //   사이즈 에러(D5n+3)를 올리면, CV 태스크 -> DB -> IO 로 이어지는 지연 때문에 READY 만 본 채
                //   이동을 완료해 버리고, 상위가 입고를 내려 그 입고에서 에러가 난 것처럼 보였다.
                //   원본(CCv::ArrivedCheck)도 에러를 보지는 않았지만 한 프로세스라 그 틈이 거의 없었다.
                strSql += CRLF + "    AND COALESCE(CD.ERROR_CODE,'0') IN ('0','0000')      ";
                // @.사이즈 체커(HOST 작업대 205, 206, 209, 214, 215) 도착의 "트랙 값이 일정 시간 안 바뀜" 대기는 없앴다. (2026-11-11)
                //   전에는 1초(11-09 에 0.5초)를 기다렸다. 이제 READY / AUTO 가 맞고 ERROR_CODE 가 0 이면 바로 완료한다.
                //   에러 중에는 위의 ERROR_CODE 조건이 여전히 도착 완료를 막는다.
                //   READY 가 에러보다 먼저 서는 틈이 다시 보이면 SizeChk 로그로 간격을 보고 대기를 되돌린다.
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("CV_PLC", DbLang.VARCHAR).Value = strCV_PLC;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                int nA1 = Environment.TickCount;
                // @.사이즈 체커 도착 조건이 맞아 완료 처리에 들어간 시각을 남긴다. (SizeChk 로그와 맞춰 보려고)
                for (int z = 0; z < nSelCnt; z++)
                {
                    string strZStn = _pBdb.mDtMain.Rows[z]["HOST_STN_NO"].ToString().Trim();
                    if (strZStn == "205" || strZStn == "206" || strZStn == "209" || strZStn == "214" || strZStn == "215")
                        SzLog("트랙 " + _pBdb.mDtMain.Rows[z]["MC_NO"].ToString().Trim() + "(작업대 " + strZStn + ") 도착 조건 충족 -> 완료 처리 시작 [작업번호:"
                              + _pBdb.mDtMain.Rows[z]["LUGG_NO"].ToString().Trim().TrimStart('0') + "]");
                }
                /*
                 * 트윈 대기대 보정 (원본 ECS CCv::ArrivedCheck4 의 "작업정보 변경")
                 *
                 *   적힌 목적지와 화물이 실제로 선 자리가 다르면 — P-BOX 221/222
                 *   에서만 생긴다 — 먼저 그것부터 바로잡고 이번 주기는 끝낸다.
                 *   다음 주기에는 두 값이 같아져 아래 보통 길로 도착 처리된다.
                 *
                 *   아래 도착 처리 트랜잭션 안에서 같이 하지 않는 까닭이 있다.
                 *   그 안에서 부르는 UPDATE_IF_LUGG_STA 가 트랜잭션을 또 열어
                 *   "Nested/Concurrent transactions aren't supported" 로 걸리고,
                 *   그 처리 과정에서 그때까지의 갱신이 되돌려진다. 뒤이은 문장은
                 *   트랜잭션 밖에서 돌아 남으므로 상태만 19 로 바뀌고 목적지는
                 *   옛 값으로 남는 어긋난 상태가 된다.
                 */
                for (int k = 0; k < nSelCnt; k++)
                {
                    string strArv = _pBdb.mDtMain.Rows[k]["HOST_STN_NO"].ToString().Trim();
                    string strDst = _pBdb.mDtMain.Rows[k]["DEST_POS"].ToString().Trim();
                    if (strArv == "" || strDst == "" || strArv == strDst) continue;

                    string strFixLugg = _pBdb.mDtMain.Rows[k]["LUGG_NO"].ToString().Trim();
                    string strFixMc   = _pBdb.mDtMain.Rows[k]["MC_NO"].ToString().Trim();
                    string strFixTyp  = _pBdb.mDtMain.Rows[k]["JOB_TYP"].ToString().Trim();
                    if (strFixTyp == "") strFixTyp = "0";

                    // @.트랙 데이터의 목적지를 제자리로 쓴다. 이게 없으면 P-BOX 입고
                    //   요구가 "아직 221 로 가는 중" 으로 읽어 요구를 내지 않는다.
                    _pBdb.BeginTrans();
                    if (UPDATE_CV_DATA(strFixTyp, "", "", strArv, "",
                                       strFixLugg, strWH_TYP, strCV_PLC, strFixMc, "",
                                       ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        pRTN_MSG = strTitle + "TRACK " + strFixMc + "번 목적지를 [" + strArv
                                 + "]로 다시 쓰지 못해 다음 주기에 다시 합니다. [작업번호:" + strFixLugg + "]";
                        return true;
                    }

                    if (UPDATE_JOB_DATA(ST_CV_RUN, strFixLugg, strWH_TYP, strFixTyp,
                                        ref pRTN_MSG, strArv) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                    _pBdb.Commit();

                    pRTN_MSG = strTitle + "[" + strDst + "]로의 작업이 [" + strArv
                             + "]에 도착하여 작업정보와 트랙 목적지를 바꿨습니다. [작업번호:" + strFixLugg + "]";
                    InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strFixLugg, ST_CV_RUN, strFixMc);
                    return true;        // 도착 처리는 다음 주기에
                }

                int nA2 = Environment.TickCount;
                string strLUGG_NO = "";
                string strMC_NO = "";
                string strMagArr = "";      // @.매거진(207) 도착 기록(Commit 뒤에 탭으로 보낸다)
                string strDelLogs = "";     // @.삭제 작업대 0 쓰기 결과. InsertLog 가 트랜잭션을 또 열므로 Commit 뒤에 적는다.
                _pBdb.BeginTrans();

                for (int i = 0; i < nSelCnt; i++)
                {
                    strLUGG_NO = "" + _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[i]["LUGG_NO"].ToString();
                    strMC_NO = "" + _pBdb.mDtMain.Rows[i]["MC_NO"].ToString() == "" ? "" : _pBdb.mDtMain.Rows[i]["MC_NO"].ToString();
                    string strJOB_TYP = "" + _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString() == "" ? "1" : _pBdb.mDtMain.Rows[i]["JOB_TYP"].ToString();

                    // 상위 TASK에 도착 보고 (LFC 인터페이스 테이블)
                    if (UPDATE_IF_LUGG_STA(strWH_TYP, strLUGG_NO, "90", ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    /*
                     * 여기서 작업을 지우면 안 된다.
                     *
                     *   HOST 태스크는 JOB_MST 의 상태로 보고 대상을 고른다.
                     *     GetJobCompleteReport(19)  출고작업 완료 보고(CV 완료)
                     *     GetJobCompleteReport(29)  입고작업 완료 보고(SC 완료)
                     *   행을 지워 버리면 보고할 것이 없어져 완료보고(F)가 나가지 않는다.
                     *   상위는 그 작업이 끝난 줄 모르니 다음 작업도 만들지 않아,
                     *   이동 -> 입고 -> 출고 순환이 첫 단계에서 멈췄다.
                     *
                     *   원본 참고 구현(CLS/cThread_CV.cs)도 같은 자리에 이렇게 적어 두었다.
                     *     "목적지 이동완료 (도착보고시 기존작업삭제 후 MES에서 새작업을
                     *      생성하기에 JOB_STATUS = '19' 로 처리한다."
                     *   거기서도 DELETE_JOB_DATA 는 주석 처리돼 있다.
                     *   실제 삭제는 HOST 태스크가 완료보고를 보낸 뒤에 한다.
                     */
                    if (UPDATE_JOB_DATA(ST_CV_DONE, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }

                    /*
                     * 도착 시 트랙 작업번호 삭제 (원본 ECS 유형 D : IsDeleteDataStation = 207, 251)
                     *
                     *   원본 CCv::ArrivedCheck(EcsSv/Cv.cpp:1197~1205) 는 이 자리에 화물이 닿으면
                     *   WriteTrackInfo(트랙, 0, 작업구분, 목적지) 로 작업번호 자리만 0 으로 쓰고
                     *   JOB->Arrived() 를 불렀다. 신규는 이것을 빼먹어 251(트랙 415)에 작업번호가
                     *   화물이 나간 뒤에도 남았다. PLC 가 251 쪽을 "데이터 있음" 으로 보면
                     *   414 디버터 스토퍼를 올리지 않아 다음 출고 화물이 409 로 빠진다.
                     *
                     *   0 쓰기는 CV_DATA 의 _OD 를 통해 CV_TASK 가 한다. 화물 센서 ON 뒤의 도착
                     *   처리 안에서만 부르므로 빈 트랙에 미리 쓰는 일은 없다. (EHS)
                     */
                    string strArvStn = "" + _pBdb.mDtMain.Rows[i]["HOST_STN_NO"].ToString().Trim();
                    if (strArvStn == MAG_STN)
                        strMagArr += "|[매거진] 작업 " + strLUGG_NO + " 매거진(" + MAG_STN + ") 도착 - 완료보고 요청 (11→" + ST_CV_DONE + ") TRACK " + strMC_NO;
                    if (IsDeleteDataStation(strArvStn))
                    {
                        string strDelMsg = "";
                        if (UPDATE_CV_DATA(strJOB_TYP, "", "", strArvStn, "", "0",
                                           strWH_TYP, strCV_PLC, strMC_NO, "", ref strDelMsg) == false)
                        {
                            strDelLogs += "|" + strTitle + "TRACK " + strMC_NO + "번[" + strArvStn + "] 도착 후 작업번호 0 쓰기를 이번 주기에 못 냈습니다(지시 자리가 비지 않음). 보조 청소가 다시 봅니다. " + strDelMsg;
                        }
                        else
                        {
                            strDelLogs += "|" + strTitle + "TRACK " + strMC_NO + "번[" + strArvStn + "] 도착 - 트랙 작업번호를 0 으로 지우도록 CV_TASK 에 요청했습니다(작업구분/목적지 유지). [작업번호:" + strLUGG_NO + "]";
                        }
                    }
                }

                pRTN_MSG = strTitle + "TRACK " + strMC_NO + "번[도착 작업대]에서 HOST_TASK를 통해서 완료보고 요청하였습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_DONE);
                foreach (string strMag in strMagArr.Split('|'))
                    if (strMag != "") MakeMagMsg(strMag);
                foreach (string strOne in strDelLogs.Split('|'))
                    if (strOne != "") InsertLog(SCH_WH_TYP, strOne, "", "", strLUGG_NO, ST_CV_DONE, strMC_NO);
                int nA3 = Environment.TickCount;
                if (unchecked(nA3 - nA0) >= 300)
                {
                    try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IoSlow.log"), DateTime.Now.ToString("HH:mm:ss") + " 도착처리 " + strCV_PLC + " 건수 " + nSelCnt + " 조회 " + unchecked(nA1 - nA0) + " ms / 보정 " + unchecked(nA2 - nA1) + " ms / 처리 " + unchecked(nA3 - nA2) + " ms" + Environment.NewLine); } catch { }
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 4 : 픽킹 레인 진입 제한 (ECS MovingTrackCheckPlc3/6)
        //   레거시 : 레인 점유수 < 제한(6)이면 진입허가 비트를 PLC 워드 558 에 기록.
        //   To-Be  : DEAD_LOCK_ZONE_DEF(게이트=CUR_POS, 레인=BUFFERS, 제한=COUNT) 기반으로
        //            기존 cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE 판정 → 게이트 트랙의
        //            TR_PAUSE_OD 를 '0'(진입허가)/'1'(대기) 로 제어한다.
        //   ★현장확인 : CV 통신 Task 가 TR_PAUSE_OD 를 PLC 진입허가(레거시 워드 558)로
        //               반영하는지 확인 필요. (변화가 있을 때만 기록하여 부하 최소화)
        // 이거는 좀더 확인이 필요함! => 일단 갯수 카운트를 하고 있는지가 확인이 필요함!
        // ─────────────────────────────────────────────────────────────────
        // @.진입 허가 명령을 다시 내기까지 최소 간격(초). PLC 가 비트를 되돌려 주기 전에 같은 명령을 쌓지 않으려는 것이다.
        private static System.Collections.Generic.Dictionary<string, DateTime> m_dicEntryOrderAt
            = new System.Collections.Generic.Dictionary<string, DateTime>();

        /*
         * 3F 픽킹 레인 진입 허가 (원본 EcsSv/Cv.cpp CCv::MovingTrackCheckPlc3/6)
         *
         *   레인 하나(DEAD_LOCK_ZONE_DEF 에서 BUFFERS 가 같은 게이트들)마다
         *     1) 레인 점유 수 = 작업 트랙(BUFFERS)에 작업번호가 있는 수
         *                     + 게이트 중 허가 비트(CV_CHECK_POS_RD)가 이미 켜져 있고 작업번호가 있는 수
         *     2) 점유 수 >= 제한(COUNT, 6)이면 이번 주기에는 허가하지 않는다.
         *     3) 아니면 게이트 중 작업번호가 있고 허가 비트가 아직 꺼진 것에 진입 허가(CMD_RQ_ID='ENTRY')를 낸다.
         *   CV 태스크가 PLC 워드 558 의 그 게이트 비트를 켠다. PLC 가 화물을 들이면 비트를 내린다.
         *
         *   목적지는 보지 않는다. (원본도 작업번호만 본다. 예전 신규 코드는 게이트 화물의 목적지가
         *   정의의 CUR_DEST_POS(트랙번호)와 같을 때만 보았는데 목적지는 작업대번호로 적혀 거의 맞지 않았다.)
         *   TR_PAUSE_OD 는 쓰지 않는다. (CV 태스크가 PLC 에 반영하지 않는 값이다)
         */
        protected bool MOVING_TRACK_CHECK_PLC(string strWH_TYP, string strPlcDigit, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                pRTN_MSG = strTitle;
                string strSql = "";

                // 1) 레인 정의
                strSql += CRLF + " SELECT DISTINCT DLZ.CUR_POS, DLZ.BUFFERS, DLZ.COUNT               ";
                strSql += CRLF + "   FROM DEAD_LOCK_ZONE_DEF DLZ                                       ";
                strSql += CRLF + "  WHERE DLZ.WH_TYP   = :WH_TYP                                       ";
                strSql += CRLF + "    AND DLZ.USE_YN   = 'Y'                                           ";
                strSql += CRLF + "    AND DLZ.CUR_POS LIKE :PFX                                        ";
                strSql += CRLF + "    AND COALESCE(DLZ.EQP_STA_CHK_TR, '') = ''                        ";
                strSql += CRLF + "  ORDER BY DLZ.CUR_POS                                               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PFX", DbLang.VARCHAR).Value = strPlcDigit + "%";
                int nDef = _pBdb.ExcuteQry(strSql);
                if (nDef < 0) { pRTN_MSG += _pBdb.ErrMsg; return false; }
                if (nDef == 0) { pRTN_MSG = ""; return true; }
                DataTable dtDef = _pBdb.mDtMain.Copy();

                // 레인 = BUFFERS 가 같은 정의들
                System.Collections.Generic.List<string> lstBuffers = new System.Collections.Generic.List<string>();
                System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> dicGates
                    = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
                System.Collections.Generic.Dictionary<string, int> dicLimit = new System.Collections.Generic.Dictionary<string, int>();
                for (int i = 0; i < dtDef.Rows.Count; i++)
                {
                    string strBuf = ("" + dtDef.Rows[i]["BUFFERS"]).Trim();
                    string strGate = ("" + dtDef.Rows[i]["CUR_POS"]).Trim();
                    int nLimit = 6;
                    int.TryParse(("" + dtDef.Rows[i]["COUNT"]).Trim(), out nLimit);
                    if (strBuf == "" || strGate == "") continue;
                    if (!dicGates.ContainsKey(strBuf)) { dicGates[strBuf] = new System.Collections.Generic.List<string>(); lstBuffers.Add(strBuf); dicLimit[strBuf] = nLimit; }
                    if (!dicGates[strBuf].Contains(strGate)) dicGates[strBuf].Add(strGate);
                }

                string strDone = "";
                foreach (string strBuf in lstBuffers)
                {
                    // 2) 작업 트랙의 작업 수
                    strSql = "";
                    strSql += CRLF + " SELECT COUNT(*) AS CNT FROM CV_DATA                                   ";
                    strSql += CRLF + "  WHERE WH_TYP = :WH_TYP                                               ";
                    strSql += CRLF + "    AND MC_NO IN (" + strBuf + ")                                      ";
                    strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(LUGG_NO_RD, " + SQL_WS + "), ''), '0') NOT IN ('0','0000') ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    if (_pBdb.ExcuteQry(strSql) < 0) { pRTN_MSG += _pBdb.ErrMsg; return false; }
                    int nCount = 0;
                    int.TryParse(("" + _pBdb.mDtMain.Rows[0]["CNT"]).Trim(), out nCount);

                    // 3) 게이트 상태
                    string strGateIn = "";
                    foreach (string g in dicGates[strBuf]) strGateIn += (strGateIn == "" ? "" : ",") + "'" + g + "'";
                    strSql = "";
                    strSql += CRLF + " SELECT MC_NO, COALESCE(NULLIF(BTRIM(LUGG_NO_RD, " + SQL_WS + "), ''), '0') AS LUGG_NO_RD ";
                    strSql += CRLF + "      , COALESCE(CV_CHECK_POS_RD, '0') AS CV_CHECK_POS_RD, COALESCE(CMD_RQ_YN, 'N') AS CMD_RQ_YN ";
                    strSql += CRLF + "   FROM CV_DATA                                                        ";
                    strSql += CRLF + "  WHERE WH_TYP = :WH_TYP                                               ";
                    strSql += CRLF + "    AND MC_NO IN (" + strGateIn + ")                                   ";
                    strSql += CRLF + "  ORDER BY MC_NO                                                       ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                    int nGateCnt = _pBdb.ExcuteQry(strSql);
                    if (nGateCnt < 0) { pRTN_MSG += _pBdb.ErrMsg; return false; }
                    DataTable dtGate = _pBdb.mDtMain.Copy();

                    // 이미 허가가 나가 있는 게이트 화물도 레인 안으로 센다 (원본 IsReadyStartToMove)
                    for (int i = 0; i < dtGate.Rows.Count; i++)
                    {
                        bool bLugg = ("" + dtGate.Rows[i]["LUGG_NO_RD"]) != "0" && ("" + dtGate.Rows[i]["LUGG_NO_RD"]) != "0000";
                        if (bLugg && ("" + dtGate.Rows[i]["CV_CHECK_POS_RD"]).Trim() == "1") nCount++;
                    }

                    // 제한 이상이면 이번 주기에는 허가하지 않는다
                    if (nCount >= dicLimit[strBuf]) continue;

                    // 작업번호가 있고 허가 비트가 아직 꺼진 게이트에 허가
                    for (int i = 0; i < dtGate.Rows.Count; i++)
                    {
                        string strGate = ("" + dtGate.Rows[i]["MC_NO"]).Trim();
                        string strLugg = ("" + dtGate.Rows[i]["LUGG_NO_RD"]).Trim();
                        if (strLugg == "0" || strLugg == "0000") continue;
                        if (("" + dtGate.Rows[i]["CV_CHECK_POS_RD"]).Trim() == "1") continue;     // 이미 허가됨
                        if (("" + dtGate.Rows[i]["CMD_RQ_YN"]).Trim() == "Y") continue;           // 앞 명령을 CV 태스크가 아직 안 가져감

                        string strKey = strWH_TYP + "/" + strGate;
                        DateTime dtLast;
                        if (m_dicEntryOrderAt.TryGetValue(strKey, out dtLast) && (DateTime.Now - dtLast).TotalSeconds < 5) continue;

                        strSql = "";
                        strSql += CRLF + " UPDATE CV_DATA                        ";
                        strSql += CRLF + "    SET CMD_RQ_ID  = 'ENTRY'           ";
                        strSql += CRLF + "      , CMD_RQ_YN  = 'Y'               ";
                        strSql += CRLF + "      , WRITE_UPD_DT = " + DbLang.SYSDATE + " ";
                        strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP           ";
                        strSql += CRLF + "    AND MC_NO      = :MC_NO            ";
                        strSql += CRLF + "    AND COALESCE(CMD_RQ_YN, 'N') = 'N' ";
                        _pBdb.mComMain.CommandType = CommandType.Text;
                        _pBdb.mComMain.Parameters.Clear();
                        _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                        _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strGate;
                        int nUpd = _pBdb.ExcuteNonQry(strSql);
                        if (nUpd < 0) { pRTN_MSG += _pBdb.ErrMsg; return false; }
                        if (nUpd == 0) continue;

                        m_dicEntryOrderAt[strKey] = DateTime.Now;
                        nCount++;
                        strDone = strTitle + "TRACK " + strGate + "번[픽킹 레인 게이트] 진입 허가를 CV_TASK 에 지시했습니다. [작업번호:" + strLugg
                                + "][레인 작업 수:" + nCount + "/" + dicLimit[strBuf] + "]";
                        InsertLog(SCH_WH_TYP, strDone, "", "", strLugg, "", strGate, "", false);
                    }
                }
                pRTN_MSG = strDone;
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 6 : 크레인 작업 완료 -> 작업 29(SC 구동완료 보고)
        //   D110(COMPLETE_RD)이 0 이 아니면 완료다. (1=포크1, 2=포크2, 3=전체)
        //   어느 크레인인지는 ITN_LUGG_FK1 로 안다. 작업대 번호로 찾으면 안 된다 -
        //   출고 작업은 출발지가 '000' 이고 도착지는 출고대라 크레인이 아니다.
        //   입고는 29 가 최종이라 HOST_TASK 가 완료보고(F)를 보내고 작업을 지운다.
        //   출고는 29 이후 CV 구간(11→15→19)이 남는다.
        //   완료를 확인했으면 포크 데이터를 지워 크레인을 다음 작업에 쓸 수 있게 한다.
        // ─────────────────────────────────────────────────────────────────
        // strFloorSel : "3" = 3층 지시(H/S 입고 3 / 출고 4)만, "!3" = 3층이 아닌 것(1층 / BOX)만, "" = 전부 (2026-11-11)
        //   예전에는 1층 스레드와 3층 스레드가 둘 다 모든 층의 완료를 처리해, 3층 대물량의 완료가 1층 탭에 찍혔다.
        //   크레인에 지시를 쓸 때 SC_DATA 의 *_HSPOS_FK*_OD 에 층의 H/S 자리 번호(1층/BOX 1,2 / 3층 3,4)를 적어 두므로 그것으로 가른다.
        protected bool SC_COMP_CHK(string strWH_TYP, string strTitle, ref string pRTN_MSG, string strFloorSel = "")
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                //   ITN_LUGG_FK2 : 포크#2 가 같이 들고 나간 작업번호. 트윈일 때만 들어 있다.
                strSql += CRLF + " SELECT JM.LUGG_NO, JM.JOB_TYP, SD.SC_NO, SD.COMPLETE_RD, COALESCE(NULLIF(BTRIM(JM.CMP_STEP, " + SQL_WS + "), ''), '0') AS CMP_STEP ";
                strSql += CRLF + "      , COALESCE(NULLIF(BTRIM(SD.ITN_LUGG_FK2, " + SQL_WS + "), ''), '0') AS ITN_LUGG_FK2 ";
                /*
                 * JM_PAIR : 지시할 때 우리가 적어 둔 짝 작업번호.
                 *
                 *   크레인이 돌려주는 ITN_LUGG_FK2 만 보면 안 된다. 작업이 끝나면
                 *   SC 태스크가 ITN_LUGG_FK1/FK2 를 모두 '0' 으로 지운다
                 *   (ScThread.cs 1712~1721). 완료표시를 읽는 시점과 지우는 시점이
                 *   겹치면 짝 번호가 이미 0 이 되어 있어 짝을 못 닫는다.
                 *   실제로 포크#1 작업만 닫히고 포크#2 작업이 21 에 영원히
                 *   멈춰 있었다. (7101 완료 / 7102 21 고정)
                 *
                 *   PAIR_LUGG_NO 는 우리가 지시할 때 두 작업에 서로를 가리키게
                 *   적어 둔 값이라 지워지지 않는다. 이쪽을 먼저 본다.
                 */
                strSql += CRLF + "      , COALESCE(NULLIF(BTRIM(JM.PAIR_LUGG_NO, " + SQL_WS + "), ''), '0') AS JM_PAIR ";
                strSql += CRLF + "   FROM JOB_MST JM                                       ";
                strSql += CRLF + "  INNER JOIN SC_DATA SD                                  ";
                strSql += CRLF + "     ON SD.WH_TYP           = JM.WH_TYP                  ";
                /*
                 * 이 작업을 들고 있는 크레인.
                 *
                 *   포크#2 단독 지시(USE_FK 2)면 크레인의 내부 작업번호가 FK2 자리에
                 *   들어가고 FK1 은 0 이다. FK1 만 보면 그 작업을 못 찾아 구동중(21)에
                 *   영원히 멈춘다. 두 자리를 다 본다.
                 */
                strSql += CRLF + "    AND (   COALESCE(NULLIF(BTRIM(SD.ITN_LUGG_FK1, " + SQL_WS + "), ''), '0') = JM.LUGG_NO ";
                strSql += CRLF + "         OR COALESCE(NULLIF(BTRIM(SD.ITN_LUGG_FK2, " + SQL_WS + "), ''), '0') = JM.LUGG_NO ) ";
                strSql += CRLF + "  WHERE JM.WH_TYP           = :WH_TYP                    ";
                strSql += CRLF + "    AND JM.JOB_STATUS       = '" + ST_SC_RUN + "'        ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.COMPLETE_RD, " + SQL_WS + "), ''), '0') NOT IN ('0')     ";   // 작업완료표시
                strSql += CRLF + "    AND SD.READ_UPD_DT      > SD.WRITE_UPD_DT + INTERVAL '2 seconds' ";   // 지시를 쓴 뒤에 읽은 값이어야 한다
                /*
                 * 기존 ECS 의 완료 조건(CScInfo::IsFinishTheWork, ScInfo.cpp)에 맞춘다.
                 *
                 *   온라인 / 자동 / 액티브
                 *   상태 대기(0) 또는 아이들(1)           -> 움직이는 중(2)이면 끝난 게 아니다
                 *   포크 화물 없음(SENSOR_FK_RD '0')      -> 화물을 들고 있으면 끝난 게 아니다
                 *   작업완료표시(COMPLETE_RD)             -> 위에서 본다
                 *   크레인이 읽어 준 작업번호 == 우리가 지시한 작업번호
                 *       (레거시: m_nLuggNum == m_nInternalLuggNum)
                 *
                 *   마지막 조건이 없으면 앞 작업의 완료표시가 아직 남아 있을 때 다음 작업을 지시하는 순간
                 *   그것을 새 작업의 완료로 읽는다. (2677: 앞 작업 4169 완료 0.24초 뒤에 지시, 0.46초 만에 완료 처리
                 *   -> 크레인은 아직 입고 중인데 DELFK1 이 나가고 다음 작업 4112 가 덮어써 0144 OVERWRITE 에러)
                 *   크레인이 새 작업을 받으면 읽어 주는 작업번호가 새 번호로 바뀌므로 그때부터 완료를 볼 수 있다.
                 *   포크#2 단독 지시일 수 있어 FK1/FK2 둘 다 본다. 앞의 0 은 접어서 비교한다.
                 */
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.ONLINE_MODE_RD, " + SQL_WS + "), ''), '0') = '" + SC_MODE_ONLINE + "' ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.AUTO_MODE_RD, "   + SQL_WS + "), ''), '0') = '" + SC_MODE_AUTO + "' ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.ACTIVE_MODE_RD, " + SQL_WS + "), ''), '0') = '" + SC_ACTIVE + "' ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.UCSTATUS_RD, "    + SQL_WS + "), ''), '0') IN ('" + SC_STA_IDLE + "','" + SC_STA_WAIT + "') ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.SENSOR_FK_RD, "   + SQL_WS + "), ''), '0') = '0' ";
                strSql += CRLF + "    AND (   COALESCE(NULLIF(LTRIM(BTRIM(SD.LUGG_NO_FK1_RD, " + SQL_WS + "), '0'), ''), '0') = COALESCE(NULLIF(LTRIM(BTRIM(JM.LUGG_NO, " + SQL_WS + "), '0'), ''), '0') ";
                strSql += CRLF + "         OR COALESCE(NULLIF(LTRIM(BTRIM(SD.LUGG_NO_FK2_RD, " + SQL_WS + "), '0'), ''), '0') = COALESCE(NULLIF(LTRIM(BTRIM(JM.LUGG_NO, " + SQL_WS + "), '0'), ''), '0') ";
                strSql += CRLF + "         OR (    COALESCE(NULLIF(LTRIM(BTRIM(SD.LUGG_NO_FK1_RD, " + SQL_WS + "), '0'), ''), '0') = '0' ";
                strSql += CRLF + "             AND COALESCE(NULLIF(LTRIM(BTRIM(SD.LUGG_NO_FK2_RD, " + SQL_WS + "), '0'), ''), '0') = '0' ) ) ";   // 작업 끝에 번호를 지우는 크레인(ScSim)도 받는다
                if (strFloorSel != "")
                {
                    string strIs3F = "( COALESCE(BTRIM(SD.START_HSPOS_FK1_OD, " + SQL_WS + "), '') = '3' OR COALESCE(BTRIM(SD.DEST_HSPOS_FK1_OD, " + SQL_WS + "), '') = '4' "
                                   + "  OR COALESCE(BTRIM(SD.START_HSPOS_FK2_OD, " + SQL_WS + "), '') = '3' OR COALESCE(BTRIM(SD.DEST_HSPOS_FK2_OD, " + SQL_WS + "), '') = '4' )";
                    strSql += CRLF + "    AND " + (strFloorSel == "3" ? "" : "NOT ") + strIs3F + " ";
                }
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.ERR_CODE_RD, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                     ";
                strSql += CRLF + "  ORDER BY JM.LUGG_NO                                    ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;

                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO = _pBdb.mDtMain.Rows[0]["LUGG_NO"].ToString();
                string strJOB_TYP = _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                string strSC_NO   = _pBdb.mDtMain.Rows[0]["SC_NO"].ToString();
                string strLUGG_FK2 = _pBdb.mDtMain.Rows[0]["ITN_LUGG_FK2"].ToString().Trim();
                string strJmPair   = _pBdb.mDtMain.Rows[0]["JM_PAIR"].ToString().Trim();
                string strCmpStep  = _pBdb.mDtMain.Rows[0]["CMP_STEP"].ToString().Trim();

                // 호기간 이동(5/15)은 두 단계다. 1단계(출발 호기가 3층 출고 H/S 로 내림)가 끝나면 아직 작업이 끝난 게 아니다.
                //   상태 29 는 HOST 가 완료보고하는 상태라 28 로 둔다. 28 은 CV_RETHS_PLC 가 이어받아 도착 호기로 보낸다.
                //   2단계(도착 호기 입고)가 끝날 때(CMP_STEP='2')만 29 로 닫는다.
                string strDoneStatus = (IsA2AJobType(strJOB_TYP) && strCmpStep != "2") ? ST_SC_DONE_A2A_STEP1 : ST_SC_DONE;

                // 우리가 적어 둔 것을 먼저 쓴다. 없으면 크레인이 돌려준 것을 쓴다.
                if ((strJmPair != "") && (strJmPair != "0") && (strJmPair != "0000"))
                    strLUGG_FK2 = strJmPair;

                _pBdb.BeginTrans();

                if (UPDATE_JOB_DATA(strDoneStatus, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                /*
                 * 포크#2 작업도 함께 닫는다.
                 *
                 *   트윈은 크레인이 한 번 들어가서 둘을 집어 오는 한 동작이다.
                 *   작업완료표시(D198)는 포크별로 오지 않고 그 동작 하나에 대해
                 *   한 번 온다. 우리 ScSim 도 포크 수와 무관하게 3 을 쓴다.
                 *   그래서 완료를 받으면 두 작업을 같이 닫는다.
                 *
                 *   한쪽만 닫으면 남은 작업이 지시 대기(20)가 아니라 구동중(21)에
                 *   머물러, 출고 H/S 에 화물을 올려 둔 채 아무도 집어 가지 않는다.
                 *   (실제로 포크#2 작업이 411 위에 그대로 멈춰 있었다)
                 */
                /*
                 * 짝이 자기 자신이면 짝이 아니다.
                 *
                 *   포크#2 단독 지시(USE_FK 2)면 ITN_LUGG_FK2 에 그 작업 자신이
                 *   들어 있다. 그걸 짝으로 보고 한 번 더 닫으려 하면, 이미 29 로
                 *   옮긴 뒤라 UPDATE_JOB_DATA 가 "고칠 줄이 없다"로 실패하고
                 *   트랜잭션을 통째로 되돌린다. 그래서 그 작업이 21 에 그대로
                 *   남았다. (8601 이 그랬다)
                 */
                bool bTwinDone = (strLUGG_FK2 != "") && (strLUGG_FK2 != "0") && (strLUGG_FK2 != "0000")
                              && (strLUGG_FK2 != strLUGG_NO);

                /*
                 * @.짝이 JOB_MST 에 구동중(21)으로 실제로 있어야 같이 닫는다.
                 *   상위가 적어 준 PAIR_LUGG_NO 가 가리키는 작업이 이미 따로(단독) 끝나 없으면
                 *   UPDATE_JOB_DATA 가 "히스토리 추가 불가" 로 실패하고 롤백돼, 이 작업이
                 *   21 에 영원히 남고 크레인 완료표시도 안 풀려 뒤가 다 멈췄다. (1056 이 그랬다)
                 */
                if (bTwinDone)
                {
                    string strSqlPair = CRLF + " SELECT COUNT(1) AS CNT FROM JOB_MST WHERE WH_TYP = :WH_TYP AND LUGG_NO = :LUGG_NO AND JOB_STATUS = '" + ST_SC_RUN + "' ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP",  DbLang.VARCHAR).Value = strWH_TYP;
                    _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_FK2;
                    int nPairCnt = _pBdb.ExcuteQry(strSqlPair);
                    if (nPairCnt <= 0 || Convert.ToInt32(_pBdb.mDtMain.Rows[0]["CNT"]) == 0)
                    {
                        ReportOnChange("SCCOMP_PAIR_" + strSC_NO, strTitle + "짝 작업번호 " + strLUGG_FK2 + " 가 구동중으로 없어 작업번호 " + strLUGG_NO + " 만 닫습니다.");
                        bTwinDone = false;
                    }
                }

                if (bTwinDone)
                {
                    if (UPDATE_JOB_DATA(ST_SC_DONE, strLUGG_FK2, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                    {
                        _pBdb.Rollback();
                        return false;
                    }
                }

                if (ClearScFork(strSC_NO, bTwinDone, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                if (bTwinDone)
                    pRTN_MSG = strTitle + strSC_NO + "호기가 트윈 작업을 완료했습니다. [포크#1 작업번호:" + strLUGG_NO + "][포크#2 작업번호:" + strLUGG_FK2 + "]";
                else
                    pRTN_MSG = strTitle + strSC_NO + "호기가 작업을 완료했습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_SC_DONE, "", strSC_NO);
                if (IsA2AJobType(strJOB_TYP))
                {
                    if (strCmpStep != "2")
                        MakeRackMsg("[호기간 이동 3/6] " + strSC_NO + "호기 호기간 이동(출고) 완료 - 3층 출고 H/S 에 내렸습니다. [작업번호:" + strLUGG_NO + "]", cDefApp.eLogMsgType.MSG_BLUE);
                    else
                        MakeRackMsg("[호기간 이동 6/6] " + strSC_NO + "호기 호기간 이동(입고) 완료 - 호기간 이동이 끝났습니다. HOST 완료보고 대기 [작업번호:" + strLUGG_NO + "]", cDefApp.eLogMsgType.MSG_BLUE);
                }
                else if (IsR2RJobType(strJOB_TYP))
                    MakeRackMsg("[랙투랙 3/3] " + strSC_NO + "호기 랙투랙 완료. HOST 완료보고 대기 [작업번호:" + strLUGG_NO + "]", cDefApp.eLogMsgType.MSG_BLUE);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        public bool ScCompleteCheck(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return SC_COMP_CHK(strWH_TYP, "[ScCompleteCheck]", ref pRTN_MSG, "!3"); }       // 1층 (3층 지시는 3층 스레드가 한다)
        public bool ScCompleteCheck3(string strWH_TYP, string strPLC_NO, ref string pRTN_MSG)
        { return SC_COMP_CHK(strWH_TYP, "[ScCompleteCheck3]", ref pRTN_MSG, "3"); }       // 3층

        /*
         * ClearScFork1 :: 완료한 크레인의 포크#1 데이터를 지운다.
         *
         *   SC 태스크가 CMD_RQ_YN='Y' 를 보고 CMD_RQ_ID 별로 PLC 에 명령을 쓴다.
         *   DELFK1 은 D199=16(포크#1 데이터 삭제)이고, 기록 뒤 SC 태스크가
         *   ITN_LUGG_FK1 을 '0' 으로 되돌린다. 이 현장은 SINGLE 포크라 FK1 만 쓴다.
         */
        /*
         * ClearScFork :: 완료한 크레인의 포크 데이터를 지운다.
         *
         *   bTwin 이면 포크#2 까지 지운다. SC 태스크가 CMD_RQ_ID 별로 PLC 에
         *   명령을 쓰고, 기록 뒤 ITN_LUGG_FK1 / FK2 를 '0' 으로 되돌린다.
         *     DELFK1  D199=16  포크#1 만
         *     DELFK12          포크#1,#2 둘 다
         *   포크#2 를 지우지 않으면 ITN_LUGG_FK2 가 남아 다음 완료에서 이미
         *   닫은 작업을 또 닫으려 든다.
         */
        protected bool ClearScFork(string strScNo, bool bTwin, ref string strRtn)
        {
            try
            {
                string strSql = "";
                // @.INI [SC] DELFK_ON_COMPLETE (기본 N) : 완료 때 크레인에 포크 데이터 삭제(DELFK)를 내릴지.
                //   N(기본) = 구 ECS 와 같다. PLC 의 포크 데이터(작업번호)는 그대로 두고 다음 지시가 덮어쓴다. 내부 작업번호(ITN_LUGG_FK1/2)만 0 으로 되돌려
                //            "다음 지시 가능"(SQL_SC_READY 의 ITN_LUGG_FK1=0)과 "ECS작업유무" 를 바로 푼다. WMS 에는 PLC 의 마지막 작업번호가 그대로 보인다.
                //   Y       = 예전 방식. DELFK 를 적재하고 SC 태스크가 PLC 에 써서 지운 뒤 ITN 을 0 으로 되돌린다.
                if (!cDefApi.GsGetIniYN("SC", "DELFK_ON_COMPLETE", false))
                {
                    strSql = "";
                    strSql += CRLF + " UPDATE SC_DATA                        ";
                    strSql += CRLF + "    SET ITN_LUGG_FK1 = '0'             ";
                    if (bTwin)
                        strSql += CRLF + "      , ITN_LUGG_FK2 = '0'         ";
                    strSql += CRLF + "      , OD_USER_ID = '" + OD_USER + "' ";
                    strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP           ";
                    strSql += CRLF + "    AND SC_NO      = :SC_NO            ";
                    _pBdb.mComMain.CommandType = CommandType.Text;
                    _pBdb.mComMain.Parameters.Clear();
                    _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = SCH_WH_TYP;
                    _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strScNo;
                    int nIt = _pBdb.ExcuteNonQry(strSql);
                    if (nIt < 0) { strRtn += "SC_DATA 내부 작업번호 정리 오류:" + _pBdb.ErrMsg; return false; }
                    return true;
                }

                strSql += CRLF + " UPDATE SC_DATA                        ";
                strSql += CRLF + "    SET CMD_RQ_YN  = 'Y'               ";
                strSql += CRLF + "      , CMD_RQ_ID  = '" + (bTwin ? "DELFK12" : "DELFK1") + "'      ";
                strSql += CRLF + "      , OD_USER_ID = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP     = :WH_TYP           ";
                strSql += CRLF + "    AND SC_NO      = :SC_NO            ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("SC_NO",  DbLang.VARCHAR).Value = strScNo;

                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "SC_DATA 포크 삭제 지시 오류:" + _pBdb.ErrMsg; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        // ─────────────────────────────────────────────────────────────────
        // 공통 코어 5/6 : 구↔신 라인 이음새 데이터 복사/삭제 (ECS Copy/DeleteTrackData2/5)
        //   레거시 : FROM 트랙의 작업데이터(화물번호/작업구분/목적지)를 TO 트랙에 복사한 뒤,
        //            반영이 확인되면 FROM 트랙 데이터를 삭제 (화물번호가 두 PLC 에 동시에
        //            존재하지 않도록 하는 핸드오프)
        // ─────────────────────────────────────────────────────────────────
        protected bool COPY_TRACK_DATA(string strWH_TYP, string strFROM_MC, string strTO_MC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string strKey = strFROM_MC + "_" + strTO_MC;

                pRTN_MSG = strTitle;

                // FROM 재하 작업 + TO 빈 트랙(물리 도착) 확인
                strSql = "";
                // @.FROM 트랙의 PLC 데이터(JOB_TYP_RD / DEST_POS_RD)를 그대로 TO 트랙에 복사한다. (2026-11-02, 구 ECS CopyTrackData 와 같다)
                //   전에는 JOB_MST 의 JOB_TYP / DEST_POS 를 썼다. 7~10호기 출고 화물은 1층에서 목적지 171(출고위치 결정대)로 받아 오는데
                //   이음새에서 작업정보의 목적지(105 등)로 바꿔 쓰니 PLC 가 모르는 번호라 화물이 서 있었다.
                //   FROM 값이 비어 있을 때만 JOB_MST 값으로 메운다. 작업이 JOB_MST 에 없어도(LEFT JOIN) 트랙 데이터는 넘긴다.
                strSql += CRLF + " SELECT CDF.LUGG_NO_RD, CDT.PLC_NO AS TO_PLC,                    ";
                strSql += CRLF + "        CDF.JOB_TYP_RD, CDF.DEST_POS_RD,                         ";
                strSql += CRLF + "        JM.JOB_TYP, JM.PRODUCT_SIZE, JM.TRAY_LEV, JM.TURN,       ";
                strSql += CRLF + "        JM.DEST_POS                                              ";
                strSql += CRLF + "   FROM CV_DATA CDF                                              ";
                strSql += CRLF + "  INNER JOIN CV_DATA CDT                                         ";
                strSql += CRLF + "     ON CDT.WH_TYP = CDF.WH_TYP AND CDT.MC_NO = :TO_MC           ";
                strSql += CRLF + "   LEFT JOIN JOB_MST JM                                          ";
                strSql += CRLF + "     ON JM.WH_TYP = CDF.WH_TYP AND JM.LUGG_NO = CDF.LUGG_NO_RD   ";
                strSql += CRLF + "  WHERE CDF.WH_TYP = :WH_TYP                                     ";
                strSql += CRLF + "    AND CDF.MC_NO  = :FROM_MC                                    ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CDF.LUGG_NO_RD, " + SQL_WS + "), ''), '0') NOT IN ('0','0000')                    ";
                strSql += CRLF + "    AND CDF.SENSOR0_DATA_RD  = '0'                               ";   // FROM 센서 이탈 (이음새 통과 중)
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CDT.LUGG_NO_RD, " + SQL_WS + "), ''), '0') IN ('0','0000')                        ";   // TO 데이터 비어있음
                // @.TO 트랙의 입고대 준비(STO_READY_RD) = 구 ECS 의 "Move Track Data" 비트(상태영역 word 19 = D569.0).
                //   구 ECS(EcsSv Cv.cpp CopyData)는 FROM 작업번호 有·센서 OFF, TO 작업번호 無·StoStationReady·센서 ON 일 때 복사한다.
                //   받는 트랙(355/631/250/531)은 보통 트랙이라 D550 입고대 비트가 없고, CV_TASK DeviceMap 에 D569 영역이 빠져 있어
                //   이 값이 늘 비어 있었다. DeviceMap0x.xml 에 D569.0 -> 그 트랙의 STO_READY_RD 를 넣어 채운다. (2026-10-26)
                //   ENV_IOSCH.INI [SCH] SEAM_NEED_STO_READY=N 이면 이 비트를 보지 않고(도착 + 데이터 없음만) 복사한다.
                //   (현장 PLC 가 D569.0 을 올리지 않을 때 시험용. 기본 Y = 구 ECS 와 같다)
                if (cDefApi.GsGetIniYN("SCH", "SEAM_NEED_STO_READY", true))
                    strSql += CRLF + "    AND CDT.STO_READY_RD     = '1'                               ";   // 데이터 이동 준비 (D569.0)
                strSql += CRLF + "    AND CDT.SENSOR0_DATA_RD  = '1'                               ";   // TO 에 물리 도착
                strSql += CRLF + "    AND CDT.OD_RQ_YN         = 'N'                               ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("TO_MC", DbLang.VARCHAR).Value = strTO_MC;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("FROM_MC", DbLang.VARCHAR).Value = strFROM_MC;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG += _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    // @.왜 안 넘어가는지 보이게 : FROM 에 작업번호가 있고 센서가 꺼졌는데 TO 가 준비가 안 됐으면 작업번호마다 한 번 적는다.
                    if (!cDefApp.GM_USE_SEAM) SeamWaitDiag(strWH_TYP, strFROM_MC, strTO_MC, strTitle);   // 이음새 탭을 쓰면 그 스레드가 본다
                    pRTN_MSG = "";
                    return true;
                }

                string strLUGG_NO = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO_RD"].ToString();
                string strTO_PLC = "" + _pBdb.mDtMain.Rows[0]["TO_PLC"].ToString();
                string strJOB_TYP = "" + _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["JOB_TYP"].ToString();
                string strTRAY_TYP = "" + _pBdb.mDtMain.Rows[0]["PRODUCT_SIZE"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["PRODUCT_SIZE"].ToString();
                string strTRAY_LEV = "" + _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TRAY_LEV"].ToString();
                string strIS_TURN = "" + _pBdb.mDtMain.Rows[0]["TURN"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["TURN"].ToString();
                string strDEST_POS = "" + _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString() == "" ? "0" : _pBdb.mDtMain.Rows[0]["DEST_POS"].ToString();

                // @.FROM 트랙 PLC 값이 있으면 그것을 그대로 쓴다. (JOB_MST 값은 비었을 때의 대비)
                string strFROM_JOB_TYP = ("" + _pBdb.mDtMain.Rows[0]["JOB_TYP_RD"]).Trim();
                string strFROM_DEST = ("" + _pBdb.mDtMain.Rows[0]["DEST_POS_RD"]).Trim();
                bool bFromJob = strFROM_JOB_TYP != "" && Convert.ToInt32("0" + strFROM_JOB_TYP) != 0;
                bool bFromDest = strFROM_DEST != "" && Convert.ToInt32("0" + strFROM_DEST) != 0;
                string strWR_JOB_TYP = bFromJob ? Convert.ToInt32("0" + strFROM_JOB_TYP).ToString() : strJOB_TYP;
                string strWR_DEST = bFromDest ? Convert.ToInt32("0" + strFROM_DEST).ToString() : GfCvDestPos(strDEST_POS);

                // 동일 화물 중복 복사 방지
                if (m_dicSeamCopied.ContainsKey(strKey) && m_dicSeamCopied[strKey] == strLUGG_NO)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();
                // @.JOB_MST.DEST_POS 가 크레인이면 9NN(907)이다. 그대로 쓰면 한 바이트로 잘려(907&0xFF=139) 화물이 엉뚱한 곳으로 돈다. 호기 번호(7)로 바꿔 쓴다.
                //   (FROM 트랙 값이 있으면 위에서 그 값을 썼으므로 여기 변환은 FROM 이 비었을 때만 쓰인다)
                if (UPDATE_CV_DATA(strWR_JOB_TYP, strTRAY_TYP, strTRAY_LEV, strWR_DEST, strIS_TURN,
                                   strLUGG_NO, strWH_TYP, strTO_PLC, strTO_MC, "", ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }
                _pBdb.Commit();

                m_dicSeamCopied[strKey] = strLUGG_NO;
                pRTN_MSG = strTitle + "이음새 TRACK " + strFROM_MC + "→" + strTO_MC + " 작업 데이터 복사 지시. [작업번호:" + strLUGG_NO + " 작업구분:" + strWR_JOB_TYP + " 목적지:" + strWR_DEST + (bFromDest ? "(FROM 트랙 값)" : "(JOB_MST 값)") + "]";
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strFROM_MC, strTO_MC);
                // @.이음새 탭을 쓰면 거기에만 남기고 층 탭에는 남기지 않는다.
                if (MakeSeamMsg("[" + SeamFloor(strFROM_MC) + " 이음새] " + strFROM_MC + "→" + strTO_MC + " 복사 지시 - 작업번호 " + strLUGG_NO + " 작업구분 " + strWR_JOB_TYP + " 목적지 " + strWR_DEST + (bFromDest ? "(FROM 트랙 값)" : "(JOB_MST 값)") + " (" + strTitle.Trim('[', ']') + ")"))
                    pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        /// <summary>이음새 복사가 안 걸릴 때 그 이유를 탭에 한 번 적는다. (작업번호마다 한 번)</summary>
        protected readonly Dictionary<string, string> m_dicSeamDiag = new Dictionary<string, string>();
        protected void SeamWaitDiag(string strWH_TYP, string strFROM_MC, string strTO_MC, string strTitle)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT CDF.LUGG_NO_RD, CDF.SENSOR0_DATA_RD AS F_SEN, CDT.LUGG_NO_RD AS T_LUGG,       ";
                strSql += CRLF + "        CDT.SENSOR0_DATA_RD AS T_SEN, CDT.STO_READY_RD AS T_RDY, CDT.OD_RQ_YN AS T_OD ";
                strSql += CRLF + "   FROM CV_DATA CDF                                                                  ";
                strSql += CRLF + "  INNER JOIN CV_DATA CDT ON CDT.WH_TYP = CDF.WH_TYP AND CDT.MC_NO = :TO_MC           ";
                strSql += CRLF + "  WHERE CDF.WH_TYP = :WH_TYP AND CDF.MC_NO = :FROM_MC                                ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CDF.LUGG_NO_RD, " + SQL_WS + "), ''), '0') NOT IN ('0','0000')  ";
                strSql += CRLF + "    AND CDF.SENSOR0_DATA_RD = '0'                                                    ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("TO_MC", DbLang.VARCHAR).Value = strTO_MC;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("FROM_MC", DbLang.VARCHAR).Value = strFROM_MC;
                if (_pBdb.ExcuteQry(strSql) <= 0) return;
                DataRow r = _pBdb.mDtMain.Rows[0];
                string strLugg = ("" + r["LUGG_NO_RD"]).Trim();
                string strState = "TO " + strTO_MC + " 센서=" + r["T_SEN"] + " 작업번호=" + ("" + r["T_LUGG"]).Trim() + " 이동준비(D569.0)=" + r["T_RDY"] + " OD_RQ_YN=" + r["T_OD"];
                string strKey = strFROM_MC + "_" + strTO_MC;
                string strLast;
                if (m_dicSeamDiag.TryGetValue(strKey, out strLast) && strLast == strLugg + "|" + strState) return;
                m_dicSeamDiag[strKey] = strLugg + "|" + strState;
                string strWait = "[" + SeamFloor(strFROM_MC) + " 이음새] " + strFROM_MC + "→" + strTO_MC + " 대기 : FROM 작업번호 " + strLugg + " 떠남(센서 0) - " + strState;
                if (!MakeSeamMsg(strWait, cDefApp.eLogMsgType.MSG_ORANGE))
                    MakeMsg(strTitle + "이음새 대기 : FROM " + strFROM_MC + " 작업번호 " + strLugg + " 떠남(센서 0) - " + strState);
            }
            catch { }
        }

        protected bool DELETE_TRACK_DATA(string strWH_TYP, string strFROM_MC, string strTO_MC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string strKey = strFROM_MC + "_" + strTO_MC;

                pRTN_MSG = "";

                // @.복사 이력(m_dicSeamCopied)이 있든 없든 TO 트랙에 FROM 과 같은 작업번호가 반영되어 있으면 FROM 을 지운다.
                //   전에는 이력이 있어야만 지워, IO 를 다시 띄우면(이력 소실) FROM 에 작업번호가 영영 남았다.
                strSql = "";
                strSql += CRLF + " SELECT CDF.PLC_NO AS FROM_PLC, CDF.LUGG_NO_RD                   ";
                strSql += CRLF + "   FROM CV_DATA CDT                                              ";
                strSql += CRLF + "  INNER JOIN CV_DATA CDF                                         ";
                strSql += CRLF + "     ON CDF.WH_TYP = CDT.WH_TYP AND CDF.MC_NO = :FROM_MC         ";
                strSql += CRLF + "  WHERE CDT.WH_TYP     = :WH_TYP                                 ";
                strSql += CRLF + "    AND CDT.MC_NO      = :TO_MC                                  ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CDF.LUGG_NO_RD, " + SQL_WS + "), ''), '0') NOT IN ('0','0000')                    ";   // FROM 에 아직 잔존
                strSql += CRLF + "    AND BTRIM(CDT.LUGG_NO_RD, " + SQL_WS + ") = BTRIM(CDF.LUGG_NO_RD, " + SQL_WS + ")                             ";   // TO 에 반영 확인
                strSql += CRLF + "    AND CDF.SENSOR0_DATA_RD = '0'                               ";   // FROM 에서 화물은 떠났다
                strSql += CRLF + "    AND CDF.OD_RQ_YN   = 'N'                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("FROM_MC", DbLang.VARCHAR).Value = strFROM_MC;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("TO_MC", DbLang.VARCHAR).Value = strTO_MC;
                nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt < 0)
                {
                    pRTN_MSG = strTitle + _pBdb.ErrMsg;
                    return false;
                }
                if (nSelCnt == 0)
                {
                    // 반영 전이거나 FROM 이 이미 비워짐 - FROM 이 비워졌으면 이력 정리
                    return true;
                }

                string strFROM_PLC = "" + _pBdb.mDtMain.Rows[0]["FROM_PLC"].ToString();
                string strLUGG_NO = "" + _pBdb.mDtMain.Rows[0]["LUGG_NO_RD"].ToString().Trim();

                // FROM 트랙 데이터 삭제 지시 (작업데이터 0 클리어 - 레거시 WriteTrackInfo(...,0,0,0))
                _pBdb.BeginTrans();
                if (UPDATE_CV_DATA("0", "0", "0", "0", "0", "0",
                                   strWH_TYP, strFROM_PLC, strFROM_MC, "", ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }
                _pBdb.Commit();

                if (m_dicSeamCopied.ContainsKey(strKey)) m_dicSeamCopied.Remove(strKey);
                pRTN_MSG = strTitle + "이음새 TRACK " + strFROM_MC + " 작업 데이터 삭제 지시 (복사 완료 확인). [작업번호:" + strLUGG_NO + "]";
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strFROM_MC, strTO_MC);
                if (MakeSeamMsg("[" + SeamFloor(strFROM_MC) + " 이음새] " + strFROM_MC + " 데이터 삭제 지시 (" + strTO_MC + " 반영 확인) - 작업번호 " + strLUGG_NO + " (" + strTitle.Trim('[', ']') + ")"))
                    pRTN_MSG = "";
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        /// <summary>
        /// 게이트 트랙의 TR_PAUSE_OD 기록 (값이 다를 때만) - 반환 : 변경 행수(0=변화없음, -1=오류)
        /// ★현장확인 : CV Task 가 TR_PAUSE_OD 를 PLC 에 반영해야 한다 (레거시 진입허가 워드 558 대응)
        /// </summary>
        protected int UPDATE_CV_TR_PAUSE(string strWH_TYP, string strMC_NO, string strPause, ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE CV_DATA                                     ";
                strSql += CRLF + "    SET TR_PAUSE_OD  = :TR_PAUSE                    ";
                strSql += CRLF + "      , OD_USER_ID   = 'IOTASK'                     ";
                strSql += CRLF + "      , OD_UPD_DT    = " + DbLang.SYSDATE + "        ";
                strSql += CRLF + "  WHERE WH_TYP       = :WH_TYP                      ";
                strSql += CRLF + "    AND MC_NO        = :MC_NO                       ";
                strSql += CRLF + "    AND TR_PAUSE_OD IS DISTINCT FROM :TR_PAUSE2     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("TR_PAUSE", DbLang.VARCHAR).Value = strPause;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO", DbLang.VARCHAR).Value = strMC_NO;
                _pBdb.mComMain.Parameters.Add("TR_PAUSE2", DbLang.VARCHAR).Value = strPause;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { pRTN_MSG += "TR_PAUSE_OD 기록 오류:" + _pBdb.ErrMsg; return -1; }
                return n;
            }
            catch (Exception ex) { pRTN_MSG += ex.Message; return -1; }
        }

        /// <summary>
        /// 스테이션 번호 → 실트랙(MC_NO) 변환 (DEST_POS_DEF.TRACK_NO → MC_NO)
        /// </summary>
        protected bool GET_DEST_POS_MC(string strWH_TYP, string strSTATION, ref string strMC, ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT DPD.MC_NO                          ";
                strSql += CRLF + "   FROM DEST_POS_DEF DPD                   ";
                strSql += CRLF + "  WHERE DPD.WH_TYP   = :WH_TYP             ";
                strSql += CRLF + "    AND DPD.TRACK_NO = :TRACK_NO           ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("TRACK_NO", DbLang.VARCHAR).Value = strSTATION;
                int nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt <= 0)
                {
                    pRTN_MSG += "DEST_POS_DEF 에 스테이션(" + strSTATION + ") 정의가 없습니다.";
                    return false;
                }
                strMC = _pBdb.mDtMain.Rows[0]["MC_NO"].ToString();
                if (strMC == "")
                {
                    pRTN_MSG += "DEST_POS_DEF 스테이션(" + strSTATION + ")의 MC_NO 가 비어 있습니다.";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 실트랙(MC_NO) → 스테이션 번호 변환 (DEST_POS_DEF.MC_NO → TRACK_NO)
        ///
        ///   GET_DEST_POS_MC 의 반대 방향이다. PLC 에 쓰는 목적지(DEST_POS_OD)는
        ///   실트랙이 아니라 스테이션 번호라, 토폴로지 표(SC_HS_DEF.WAIT_TRACK 등)가
        ///   실트랙으로 들고 있는 자리를 지시로 바꿀 때 이 변환이 필요하다.
        /// </summary>
        protected bool GET_DEST_POS_STATION(string strWH_TYP, string strMC, ref string strSTATION, ref string pRTN_MSG)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " SELECT DPD.TRACK_NO                       ";
                strSql += CRLF + "   FROM DEST_POS_DEF DPD                   ";
                strSql += CRLF + "  WHERE DPD.WH_TYP   = :WH_TYP             ";
                strSql += CRLF + "    AND DPD.MC_NO    = :MC_NO              ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO",  DbLang.VARCHAR).Value = strMC;
                int nSelCnt = _pBdb.ExcuteQry(strSql);
                if (nSelCnt <= 0)
                {
                    pRTN_MSG += "DEST_POS_DEF 에 실트랙(" + strMC + ") 정의가 없습니다.";
                    return false;
                }
                strSTATION = _pBdb.mDtMain.Rows[0]["TRACK_NO"].ToString();
                if (strSTATION == "")
                {
                    pRTN_MSG += "DEST_POS_DEF 실트랙(" + strMC + ")의 TRACK_NO 가 비어 있습니다.";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG += ex.Message;
                return false;
            }
        }

        /*
         * GfCvDestPos :: CV 에 실을 목적지 번호
         *
         *   크레인이 목적지일 때 JOB_MST.DEST_POS 는 WCS 표기인 9NN(901~911)이다.
         *   CV 레지스터의 목적지 자리는 한 바이트라 904 를 넣으면 136(904 & 0xFF)으로
         *   잘린다. 설비가 쓰는 번호는 호기 번호 1~11 이므로 그것으로 바꿔 넘긴다.
         *   (상위는 1~11 로 주고, HOST 태스크가 9NN 으로 저장한다.
         *    WCS_TASK_HOST/CSrvWork.cs 의 Convert S/C No)
         */
        protected string GfCvDestPos(string strDestPos)
        {
            int nDest = 0;
            Int32.TryParse((strDestPos == null) ? "" : strDestPos.Trim(), out nDest);

            if (nDest > 900 && nDest < 1000)
                return (nDest - 900).ToString("000");

            return strDestPos;
        }

        /// <summary>
        /// 출고 계열 작업 여부 (CHECK_CV_RETHS 의 작업구분 목록과 동일)
        /// </summary>
        protected bool IsRetJobType(string strJOB_TYP)
        {
            int nTyp;
            if (int.TryParse(strJOB_TYP, out nTyp) == false) return false;
            return (nTyp == (int)EN_JOB_TYPE.enJobTypeAutoRet ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeAutoPR ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeAutoW2W ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeAutoMove ||    // KET 현장 구조상 이렇게 되지는 않음
                    nTyp == (int)EN_JOB_TYPE.enJobTypeSemiRet ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeSemiPR ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeSemiW2W ||
                    nTyp == (int)EN_JOB_TYPE.enJobTypeSemiMove);
        }

        /*
         * SqlInList :: 문자열 배열을 IN 절 목록으로 만든다. ('103','104',...)
         *   값이 전부 코드표에서 온 숫자 문자열이라 따옴표만 붙인다.
         */
        protected static string SqlInList(string[] arr)
        {
            string s = "";
            for (int i = 0; i < arr.Length; i++)
                s += (i == 0 ? "'" : ",'") + arr[i] + "'";
            return s;
        }

        /// <summary>
        /// JOB_MST.JOB_STATUS 상태 변경 (기존 cThread_*.UPDATE_JOB_DATA 의 핵심부)
        /// </summary>
        protected bool UpdateJobStatus(string strStatus, string strLuggNo, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE JOB_MST                       ";
                strSql += CRLF + "    SET JOB_STATUS  = :JOB_STATUS     ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_USER_ID = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP         ";
                strSql += CRLF + "    AND LUGG_NO     = :LUGG_NO        ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = strStatus;
                _pBdb.mComMain.Parameters.Add("WH_TYP",     DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO",    DbLang.VARCHAR).Value = strLuggNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "JOB_MST 상태변경 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "변경할 JOB_MST 작업이 없음(LUGG_NO:" + strLuggNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /// <summary>
        /// 신규 작업 상태 변경 : 상태 + 출발/도착 + S/C/HS 일괄 기록 (PrepareNewJobs 전용)
        ///   JOB_STATUS='99' 인 행만 갱신하여 중복 접수를 방지한다.
        /// </summary>
        protected bool UpdateJobInvoke(string strStatus, string strLuggNo,
                                     string strStartPos, string strDestPos,
                                     string strScNo, string strHsTrack, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE JOB_MST                        ";
                strSql += CRLF + "    SET JOB_STATUS   = :JOB_STATUS     ";
                strSql += CRLF + "      , START_POS    = :START_POS      ";
                strSql += CRLF + "      , DEST_POS     = :DEST_POS       ";
                strSql += CRLF + "      , SC_NO        = :SC_NO          ";
                strSql += CRLF + "      , HS_TRACK_NO  = :HS_TRACK_NO    ";
                strSql += CRLF + "      , JOB_START_DT = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_DT       = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_USER_ID  = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP       = :WH_TYP         ";
                strSql += CRLF + "    AND LUGG_NO      = :LUGG_NO        ";
                strSql += CRLF + "    AND JOB_STATUS   = '" + ST_CV_WAIT + "' ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_STATUS",  DbLang.VARCHAR).Value = strStatus;
                _pBdb.mComMain.Parameters.Add("START_POS",   DbLang.VARCHAR).Value = strStartPos;
                _pBdb.mComMain.Parameters.Add("DEST_POS",    DbLang.VARCHAR).Value = strDestPos;
                _pBdb.mComMain.Parameters.Add("SC_NO",       DbLang.VARCHAR).Value = strScNo;
                _pBdb.mComMain.Parameters.Add("HS_TRACK_NO", DbLang.VARCHAR).Value = strHsTrack;
                _pBdb.mComMain.Parameters.Add("WH_TYP",      DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO",     DbLang.VARCHAR).Value = strLuggNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "JOB_MST 상태 변경 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "접수할 신규 작업이 없음(LUGG_NO:" + strLuggNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /// <summary>
        ///  핸드오프 : 상태 변경 + 출발지 치환 (CompleteCV→SC  / CompleteSC→CV )
        /// </summary>
        protected bool UpdateJobLeg(string strStatus, string strNewStartPos, string strLuggNo, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE JOB_MST                       ";
                strSql += CRLF + "    SET JOB_STATUS  = :JOB_STATUS     ";
                strSql += CRLF + "      , START_POS   = :START_POS      ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_USER_ID = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP         ";
                strSql += CRLF + "    AND LUGG_NO     = :LUGG_NO        ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_STATUS", DbLang.VARCHAR).Value = strStatus;
                _pBdb.mComMain.Parameters.Add("START_POS",  DbLang.VARCHAR).Value = strNewStartPos;
                _pBdb.mComMain.Parameters.Add("WH_TYP",     DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO",    DbLang.VARCHAR).Value = strLuggNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "JOB_MST 변경 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "변경할 JOB_MST 작업이 없음(LUGG_NO:" + strLuggNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /// <summary>
        /// 목적지 갱신 : 공용 출고대가 물리 레인으로 분배된 경우 작업에 반영
        ///   (레거시 ECS 도 job 의 m_nDestPos 를 분배된 물리 레인으로 재기록)
        /// </summary>
        protected bool UpdateJobDest(string strDestPos, string strLuggNo, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE JOB_MST                       ";
                strSql += CRLF + "    SET DEST_POS    = :DEST_POS       ";
                strSql += CRLF + "      , UPD_DT      = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      , UPD_USER_ID = '" + OD_USER + "' ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP         ";
                strSql += CRLF + "    AND LUGG_NO     = :LUGG_NO        ";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("DEST_POS", DbLang.VARCHAR).Value = strDestPos;
                _pBdb.mComMain.Parameters.Add("WH_TYP",   DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("LUGG_NO",  DbLang.VARCHAR).Value = strLuggNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "JOB_MST 목적지갱신 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "갱신할 JOB_MST 작업이 없음(LUGG_NO:" + strLuggNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /// <summary>
        /// CV_DATA 명령 지시 (기존 cThread_CV.UPDATE_CV_DATA)
        ///   _OD 컬럼 기록 + OD_RQ_YN='Y'. 유휴(OD_RQ_YN='N') + 무에러 행만 대상.
        /// </summary>
        protected bool UpdateCvData(
                    string strJobTyp
            ,       string strDestPos
            ,       string strLuggNo
            ,       string strPlcNo
            ,       string strTrackNo
            , ref   string strRtn
            )
        {
            try
            {
                // 실제 CV_DATA 명령(_OD) 컬럼만 사용: JOB_TYP_OD, DEST_POS_OD, LUGG_NO_OD, OD_RQ_YN
                //   방향(입고0/출고1, D0310)은 통신 Task가 JOB_TYP 기반으로 PLC에 기록.
                string strSql = "";
                strSql += CRLF + " UPDATE CV_DATA                                 ";
                strSql += CRLF + "    SET JOB_TYP_OD  = :JOB_TYP_OD               ";
                strSql += CRLF + "      , DEST_POS_OD = :DEST_POS_OD              ";
                strSql += CRLF + "      , LUGG_NO_OD  = :LUGG_NO_OD               ";
                strSql += CRLF + "      , OD_RQ_YN    = 'Y'                       ";
                strSql += CRLF + "      , OD_USER_ID  = '" + OD_USER + "'         ";
                strSql += CRLF + "      , OD_UPD_DT   = " + DbLang.SYSDATE + "     ";
                strSql += CRLF + "  WHERE WH_TYP      = :WH_TYP                   ";
                strSql += CRLF + "    AND PLC_NO      = :PLC_NO                   ";
                strSql += CRLF + "    AND MC_NO       = :TRACK_NO                 ";
                strSql += CRLF + "    AND OD_RQ_YN    = 'N'                       ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')";

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("JOB_TYP_OD",  DbLang.VARCHAR).Value = strJobTyp;
                _pBdb.mComMain.Parameters.Add("DEST_POS_OD", DbLang.VARCHAR).Value = strDestPos;
                _pBdb.mComMain.Parameters.Add("LUGG_NO_OD",  DbLang.VARCHAR).Value = strLuggNo;
                _pBdb.mComMain.Parameters.Add("WH_TYP",      DbLang.VARCHAR).Value = SCH_WH_TYP;
                _pBdb.mComMain.Parameters.Add("PLC_NO",      DbLang.VARCHAR).Value = strPlcNo;
                _pBdb.mComMain.Parameters.Add("TRACK_NO",    DbLang.VARCHAR).Value = strTrackNo;
                int n = _pBdb.ExcuteNonQry(strSql);
                if (n < 0) { strRtn += "CV_DATA 명령 오류:" + _pBdb.ErrMsg; return false; }
                if (n == 0) { strRtn += "지시할 CV_DATA 가 없음(TRACK_NO:" + strTrackNo + ")"; return false; }
                return true;
            }
            catch (Exception ex) { strRtn += ex.Message; return false; }
        }

        /*
         * IsScNo :: 크레인 번호(901~911) 형태인가.
         *   상위가 출발지에 호기를 주는 현장도 있고 '000' 을 주는 현장도 있다.
         */
        protected bool IsScNo(string strPos)
        {
            int nPos;
            if (int.TryParse(strPos, out nPos) == false) return false;
            return ((nPos > 900) && (nPos < 1000));
        }

        /*
         * GetScNoByLocation :: 랙 위치에서 담당 호기를 구한다.
         *
         *   파레트 창고 : 호기 = (뱅크 + 1) / 2   뱅크 1,2 -> 901 / 3,4 -> 902 / ...
         *                 WCS_TASK_HOST 의 modDefApp.GetStackerNum 과 같은 식이다.
         *                 확인 : 랙 07-001-01 -> (7+1)/2 = 4 -> 904 호기
         *
         *   P-BOX 창고  : 호기가 하나다. 늘 911 이다.
         *                 뱅크는 창고 안에서만 뜻이 있는 번호(01/02)라 뱅크만
         *                 보면 901 이 나와 버린다. 그래서 창고를 함께 받는다.
         *                 비워서 부르면 예전처럼 뱅크만 본다(파레트 호기들).
         */
        protected bool GetScNoByLocation(string strLoc, ref string strScNo, string strWhTyp = null)
        {
            string strBank = "", strBay = "", strLev = "";
            if (ParseLocation(strLoc, ref strBank, ref strBay, ref strLev) == false)
                return false;

            int nBank;
            if (int.TryParse(strBank, out nBank) == false) return false;
            if (nBank < 1) return false;

            if ((strWhTyp != null) && (strWhTyp.Trim() == ((int)cDefApp.eWHTYP.KET_WH02).ToString()))
            {
                strScNo = "911";
                return true;
            }

            strScNo = (900 + ((nBank + 1) / 2)).ToString();
            return true;
        }

        /*
         * ParseLocation :: 랙 위치 문자열을 뱅크/베이/단으로 나눈다.
         *
         *   상위가 주는 형식이 현장마다 다르다. "07-001-01" 처럼 구분자를 넣기도 하고
         *   "0700101" 처럼 붙여 쓰기도 한다. 숫자만 뽑아 2/3/2 로 자른다.
         *   (레거시 LOCATION_LEN=7 : BANK 2 + BAY 3 + LEVEL 2)
         */
        /*
         * IsRetHsTrackReady :: 그 출고 H/S 트랙이 지금 받을 수 있는가.
         *
         *   트윈 포크로 둘을 보낼 때 포크#2 가 내려놓을 짝 H/S(411)도 비어 있어야
         *   한다. 포크#1 쪽은 부르는 곳에서 이미 보고 왔다.
         *
         *   보는 것은 포크#1 쪽과 같다.
         *     화물이 없고(SENSOR0, LUGG_NO), 자동이고, 에러가 없고, 멈춰 있지 않다.
         *   (기존 ECS : CCvTrackInfo::IsRetHomeStandReady)
         */
        protected bool IsRetHsTrackReady(string strWH_TYP, string strHsTrack)
        {
            try
            {
                if (string.IsNullOrEmpty(strHsTrack)) return false;
                if (strHsTrack.Trim() == "") return false;

                string strSql = "";
                strSql += CRLF + " SELECT CD.MC_NO                                        ";
                strSql += CRLF + "   FROM CV_DATA CD                                       ";
                strSql += CRLF + "  WHERE CD.WH_TYP          = :WH_TYP                     ";
                strSql += CRLF + "    AND CD.MC_NO           = :MC_NO                      ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD = '0'                         ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0') IN ('0','0000') ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD    = '1'                         ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000') ";
                strSql += SQL_HS_NOT_PAUSED;

                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("MC_NO",  DbLang.VARCHAR).Value = strHsTrack;

                return (_pBdb.ExcuteQry(strSql) > 0);
            }
            catch
            {
                return false;
            }
        }

        /*
         * ArrangeTwinByBay :: 두 출고 작업을 포크 순서에 맞게 놓는다.
         *
         *   기존 ECS 와 같은 두 가지를 본다.
         *     (EcsSv/Job.cpp 1220~1226, EcsSv/Sc.cpp 756~781)
         *
         *   ㄱ. 같은 뱅크·같은 단이면 베이가 작은 쪽이 포크#1 이다.
         *       뱅크나 단이 다르면 건드리지 않는다. 기존 ECS 도 그 셋이 다 같을
         *       때만 맞바꾼다.
         *   ㄴ. 포크#2 가 될 쪽의 베이는 BAY_CNT_W_BOX(100) 보다 작아야 한다.
         *       포크#2 쪽이 100 이면 둘을 맞바꿔 본다. 둘 다 100 이면 묶을 수
         *       없으므로 false 를 돌려준다. 부르는 곳이 하나만 지시한다.
         *
         *   돌려주는 값 : true 면 짝으로 묶을 수 있다. 인자는 포크 순서대로 놓인다.
         */
        protected const int BAY_CNT_W_BOX = 100;

        protected bool ArrangeTwinByBay(ref string strLuggFk1, ref string strLocFk1,
                                        ref string strLuggFk2, ref string strLocFk2)
        {
            string strBank1 = "0", strBay1 = "0", strLev1 = "0";
            string strBank2 = "0", strBay2 = "0", strLev2 = "0";

            if (!ParseLocation(strLocFk1, ref strBank1, ref strBay1, ref strLev1)) return false;
            if (!ParseLocation(strLocFk2, ref strBank2, ref strBay2, ref strLev2)) return false;

            int nBay1 = 0, nBay2 = 0;
            int.TryParse(strBay1, out nBay1);
            int.TryParse(strBay2, out nBay2);

            // ㄱ. 같은 뱅크·같은 단이면 베이가 작은 쪽을 포크#1 로
            if ((strBank1 == strBank2) && (strLev1 == strLev2) && (nBay2 < nBay1))
            {
                SwapTwin(ref strLuggFk1, ref strLocFk1, ref strLuggFk2, ref strLocFk2);
                int nTmp = nBay1; nBay1 = nBay2; nBay2 = nTmp;
            }

            // ㄴ. 포크#2 의 베이는 100 보다 작아야 한다
            if (nBay2 >= BAY_CNT_W_BOX)
            {
                if (nBay1 >= BAY_CNT_W_BOX)
                    return false;       // 둘 다 안 된다

                SwapTwin(ref strLuggFk1, ref strLocFk1, ref strLuggFk2, ref strLocFk2);
            }

            return true;
        }

        private static void SwapTwin(ref string strLuggFk1, ref string strLocFk1,
                                     ref string strLuggFk2, ref string strLocFk2)
        {
            string strTmp;
            strTmp = strLuggFk1; strLuggFk1 = strLuggFk2; strLuggFk2 = strTmp;
            strTmp = strLocFk1;  strLocFk1  = strLocFk2;  strLocFk2  = strTmp;
        }

        protected bool ParseLocation(string strLoc, ref string strBank, ref string strBay, ref string strLev)
        {
            if (strLoc == null)
                return false;

            string strDigit = "";
            foreach (char ch in strLoc)
            {
                if ((ch >= '0') && (ch <= '9'))
                    strDigit += ch;
            }

            if (strDigit.Length != 7)
                return false;

            strBank = strDigit.Substring(0, 2);
            strBay  = strDigit.Substring(2, 3);
            strLev  = strDigit.Substring(5, 2);
            return true;
        }

        /// <summary>DataRow 값 추출 (null/공백 안전, Trim)</summary>
        protected string GetVal(DataRow row, string col)
        {
            if (row[col] == null || row[col] == DBNull.Value) return "";
            return row[col].ToString().Trim();
        }

        #endregion

    }
}
