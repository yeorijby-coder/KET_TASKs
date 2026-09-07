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
         *     ITN_LUGG_FK1   크레인이 들고 있는 작업번호. 남아 있으면 앞 작업이 안 끝났다
         *     LUGG_NO_FK1_RD 크레인이 읽어 준 포크1 작업번호
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
            + cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.ITN_LUGG_FK1, " + SQL_WS + "), ''), '0')   IN ('0','0000')  "
            + cDefApp.CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.LUGG_NO_FK1_RD, " + SQL_WS + "), ''), '0') IN ('0','0000')  ";
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
        protected static readonly string SCH_WH_TYP = ((int)cDefApp.eWHTYP.KET_WH01).ToString();

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

        protected void RunSchFunc(SchFunc fn)
        {
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

                    //// 후속 반출 목적지가 정의된 경우에만 도착 트랙에 이동 지시
                    //// (KET 현장은 출고대 도착이 최종 - 도착보고/작업삭제만 수행)
                    //if (strCOMMING_DEST_TR != "0" && strCOMMING_DEST_TR != "")
                    //{
                    //    if (UPDATE_CV_DATA(strJOB_TYP
                    //                     , strTRAY_TYP
                    //                     , strTRAY_LEV
                    //                     , strCOMMING_DEST_TR
                    //                     , strIS_TURN
                    //                     , strLUGG_NO
                    //                     , strWH_TYP
                    //                     , strPLC_NO
                    //                     , strMC_NO
                    //                     , ""
                    //                     , ref pRTN_MSG) == false)
                    //    {
                    //        _pBdb.Rollback();
                    //        return false;
                    //    }
                    //}
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
                strSql += CRLF + "    AND (   (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_STO) + " <> 0 AND CD.STO_READY_RD = '1'   ";
                strSql += CRLF + "             AND COALESCE(NULLIF(BTRIM(CD.LUGG_NO_RD, " + SQL_WS + "), ''), '0') IN ('0','0000'))                                                        ";
                strSql += CRLF + "         OR (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_ARV) + " <> 0 AND CD.RET_READY_RD = '1') )  ";
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD = '1'              ";
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
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }

                if (UPDATE_JOB_DATA(ST_CV_RUN, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + "TRACK " + strJOB_START_POS + "번[1층 입고대]에서 CV_TASK를 통해서 작업 지시하였습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_RUN, strJOB_START_POS, strJOB_DEST_POS);
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
        // 공통 코어 3 : 1층 도착보고 (ECS ArrivedCheck2/5 - ARRIVE_CV 의 PLC 한정판)
        //   목적지 트랙 도착(RET_READY + 작업번호 일치) → 상위 보고 + 작업 삭제(HIS 이관)
        // ─────────────────────────────────────────────────────────────────
        protected bool CV_ARRIVE_PLC(string strWH_TYP, string strCV_PLC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT JM.*, CD.*                                ";
                strSql += CRLF + "   FROM CV_DATA CD                                ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                           ";
                strSql += CRLF + "     ON CD.WH_TYP             = JM.WH_TYP 	    ";
                strSql += CRLF + "    AND CD.HOST_STN_NO       = JM.DEST_POS 	    ";
                strSql += CRLF + "    AND CD.LUGG_NO_RD         = JM.LUGG_NO        ";
                strSql += CRLF + "  WHERE CD.WH_TYP		        = :WH_TYP           ";
                strSql += CRLF + "    AND CD.PLC_NO             = :CV_PLC           ";   // 3층 해당 PLC 한정 (ECS m_nNum 게이트)
                strSql += CRLF + "    AND (" + DbLang.BITAND("CD.STN_KIND", cDefApp.STN_KIND_RET | cDefApp.STN_KIND_ARV) + " <> 0)  ";
                strSql += CRLF + "    AND CD.RET_READY_RD 	    = '1'               ";
                strSql += CRLF + "    AND CD.AUTO_MODE_RD 	    = '1'               ";
                strSql += CRLF + "    AND CD.OD_RQ_YN		    = 'N'               ";
                strSql += CRLF + "    AND JM.JOB_STATUS 	    = '11'              ";
                strSql += CRLF + "    AND JM.DEST_POS Is not null                   ";
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

                string strLUGG_NO = "";
                string strMC_NO = "";
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
                }

                pRTN_MSG = strTitle + "TRACK " + strMC_NO + "번[1층 출고대]에서 HOST_TASK를 통해서 완료보고 요청하였습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_CV_DONE);
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
        protected bool MOVING_TRACK_CHECK_PLC(string strWH_TYP, string strPlcDigit, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                // 게이트(대기) 트랙에 화물이 재하된 레인 정의 조회
                strSql = "";
                strSql += CRLF + " SELECT DISTINCT DLZ.CUR_POS, DLZ.CUR_DEST_POS,                 ";
                strSql += CRLF + "        CD.LUGG_NO_RD, CD.TR_PAUSE_RD                           ";
                strSql += CRLF + "   FROM DEAD_LOCK_ZONE_DEF DLZ                                  ";
                strSql += CRLF + "  INNER JOIN CV_DATA CD                                         ";
                strSql += CRLF + "     ON CD.WH_TYP = DLZ.WH_TYP AND CD.MC_NO = DLZ.CUR_POS       ";
                strSql += CRLF + "  WHERE DLZ.WH_TYP   = :WH_TYP                                  ";
                strSql += CRLF + "    AND DLZ.USE_YN   = 'Y'                                      ";
                strSql += CRLF + "    AND DLZ.CUR_POS LIKE :PFX                                   ";   // '3%' / '6%' (해당 층 라인)
                strSql += CRLF + "    AND CD.SENSOR0_DATA_RD = '1'                                ";   // 게이트에 화물 재하
                strSql += CRLF + "    AND CD.AUTO_MODE_RD    = '1'                                ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CD.ERROR_CODE, " + SQL_WS + "), ''), '0') IN ('0','00','000','0000')                       ";
                strSql += CRLF + "    AND COALESCE(CD.DEST_POS_RD,'') = DLZ.CUR_DEST_POS          ";   // 화물 목적지 = 레인 방향
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("PFX", DbLang.VARCHAR).Value = strPlcDigit + "%";
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

                DataTable dtGate = _pBdb.mDtMain.Copy();
                pRTN_MSG = "";

                for (int i = 0; i < dtGate.Rows.Count; i++)
                {
                    string strGATE = "" + dtGate.Rows[i]["CUR_POS"].ToString();
                    string strDEST = "" + dtGate.Rows[i]["CUR_DEST_POS"].ToString();
                    string strLUGG = "" + dtGate.Rows[i]["LUGG_NO_RD"].ToString();

                    // 레인 점유수 판정 (기존 공용 함수 - 초과 시 false)
                    string strChkMsg = "";
                    DataTable dtDeadLock = new DataTable();
                    bool bEnterOk = cDefApi.CHECK_ENTER_DEAD_LOCK_ZONE(_pBdb, strWH_TYP, strGATE, strDEST, ref strChkMsg, ref dtDeadLock);

                    // 진입허가('0') / 대기('1') - 값이 바뀔 때만 기록
                    string strPause = bEnterOk ? "0" : "1";
                    int nChg = UPDATE_CV_TR_PAUSE(strWH_TYP, strGATE, strPause, ref pRTN_MSG);
                    if (nChg < 0) return false;
                    if (nChg > 0)
                    {
                        pRTN_MSG = strTitle + "TRACK " + strGATE + "번[픽킹 레인 게이트] " +
                                   (bEnterOk ? "진입 허가" : "레인 만석 - 진입 대기") +
                                   " (목적지:" + strDEST + ", 작업번호:" + strLUGG + ")";
                        InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG, "", strGATE, strDEST, false);
                        return true;
                    }
                }

                pRTN_MSG = "";
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
        protected bool SC_COMP_CHK(string strWH_TYP, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";

                pRTN_MSG = strTitle;

                strSql = "";
                strSql += CRLF + " SELECT JM.LUGG_NO, JM.JOB_TYP, SD.SC_NO, SD.COMPLETE_RD ";
                strSql += CRLF + "   FROM JOB_MST JM                                       ";
                strSql += CRLF + "  INNER JOIN SC_DATA SD                                  ";
                strSql += CRLF + "     ON SD.WH_TYP           = JM.WH_TYP                  ";
                strSql += CRLF + "    AND SD.ITN_LUGG_FK1     = JM.LUGG_NO                 ";   // 이 작업을 들고 있는 크레인
                strSql += CRLF + "  WHERE JM.WH_TYP           = :WH_TYP                    ";
                strSql += CRLF + "    AND JM.JOB_STATUS       = '" + ST_SC_RUN + "'        ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(SD.COMPLETE_RD, " + SQL_WS + "), ''), '0') NOT IN ('0')     ";   // 작업완료표시
                strSql += CRLF + "    AND SD.READ_UPD_DT      > SD.WRITE_UPD_DT            ";   // 지시를 쓴 뒤에 읽은 값이어야 한다
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

                _pBdb.BeginTrans();

                if (UPDATE_JOB_DATA(ST_SC_DONE, strLUGG_NO, strWH_TYP, strJOB_TYP, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                if (ClearScFork1(strSC_NO, ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    return false;
                }

                pRTN_MSG = strTitle + strSC_NO + "호기가 작업을 완료했습니다. [작업번호:" + strLUGG_NO + "]";
                _pBdb.Commit();
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, ST_SC_DONE, "", strSC_NO);
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
        { return SC_COMP_CHK(strWH_TYP, "[ScCompleteCheck]", ref pRTN_MSG); }

        /*
         * ClearScFork1 :: 완료한 크레인의 포크#1 데이터를 지운다.
         *
         *   SC 태스크가 CMD_RQ_YN='Y' 를 보고 CMD_RQ_ID 별로 PLC 에 명령을 쓴다.
         *   DELFK1 은 D199=16(포크#1 데이터 삭제)이고, 기록 뒤 SC 태스크가
         *   ITN_LUGG_FK1 을 '0' 으로 되돌린다. 이 현장은 SINGLE 포크라 FK1 만 쓴다.
         */
        protected bool ClearScFork1(string strScNo, ref string strRtn)
        {
            try
            {
                string strSql = "";
                strSql += CRLF + " UPDATE SC_DATA                        ";
                strSql += CRLF + "    SET CMD_RQ_YN  = 'Y'               ";
                strSql += CRLF + "      , CMD_RQ_ID  = 'DELFK1'          ";
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
                strSql += CRLF + " SELECT CDF.LUGG_NO_RD, CDT.PLC_NO AS TO_PLC,                    ";
                strSql += CRLF + "        JM.JOB_TYP, JM.PRODUCT_SIZE, JM.TRAY_LEV, JM.TURN,       ";
                strSql += CRLF + "        JM.DEST_POS                                              ";
                strSql += CRLF + "   FROM CV_DATA CDF                                              ";
                strSql += CRLF + "  INNER JOIN CV_DATA CDT                                         ";
                strSql += CRLF + "     ON CDT.WH_TYP = CDF.WH_TYP AND CDT.MC_NO = :TO_MC           ";
                strSql += CRLF + "  INNER JOIN JOB_MST JM                                          ";
                strSql += CRLF + "     ON JM.WH_TYP = CDF.WH_TYP AND JM.LUGG_NO = CDF.LUGG_NO_RD   ";
                strSql += CRLF + "  WHERE CDF.WH_TYP = :WH_TYP                                     ";
                strSql += CRLF + "    AND CDF.MC_NO  = :FROM_MC                                    ";
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CDF.LUGG_NO_RD, " + SQL_WS + "), ''), '0') NOT IN ('0','0000')                    ";
                strSql += CRLF + "    AND CDF.SENSOR0_DATA_RD  = '0'                               ";   // FROM 센서 이탈 (이음새 통과 중)
                strSql += CRLF + "    AND COALESCE(NULLIF(BTRIM(CDT.LUGG_NO_RD, " + SQL_WS + "), ''), '0') IN ('0','0000')                        ";   // TO 데이터 비어있음
                strSql += CRLF + "    AND CDT.STO_READY_RD     = '1'                               ";
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

                // 동일 화물 중복 복사 방지
                if (m_dicSeamCopied.ContainsKey(strKey) && m_dicSeamCopied[strKey] == strLUGG_NO)
                {
                    pRTN_MSG = "";
                    return true;
                }

                _pBdb.BeginTrans();
                if (UPDATE_CV_DATA(strJOB_TYP, strTRAY_TYP, strTRAY_LEV, strDEST_POS, strIS_TURN,
                                   strLUGG_NO, strWH_TYP, strTO_PLC, strTO_MC, "", ref pRTN_MSG) == false)
                {
                    _pBdb.Rollback();
                    pRTN_MSG = "";
                    return true;    // 설비 미준비 - 다음 사이클 재시도
                }
                _pBdb.Commit();

                m_dicSeamCopied[strKey] = strLUGG_NO;
                pRTN_MSG = strTitle + "이음새 TRACK " + strFROM_MC + "→" + strTO_MC + " 작업 데이터 복사 지시. [작업번호:" + strLUGG_NO + "]";
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strFROM_MC, strTO_MC);
                return true;
            }
            catch (Exception ex)
            {
                pRTN_MSG = strTitle + ex.ToString();
                _pBdb.Rollback();
                return false;
            }
        }

        protected bool DELETE_TRACK_DATA(string strWH_TYP, string strFROM_MC, string strTO_MC, string strTitle, ref string pRTN_MSG)
        {
            try
            {
                int nSelCnt = 0;
                string strSql = "";
                string strKey = strFROM_MC + "_" + strTO_MC;

                pRTN_MSG = "";

                // 복사 이력이 없으면 통과
                if (m_dicSeamCopied.ContainsKey(strKey) == false)
                    return true;

                string strLUGG_NO = m_dicSeamCopied[strKey];

                // TO 트랙에 복사가 반영(PLC readback)되었는지 확인
                strSql = "";
                strSql += CRLF + " SELECT CDF.PLC_NO AS FROM_PLC                                   ";
                strSql += CRLF + "   FROM CV_DATA CDT                                              ";
                strSql += CRLF + "  INNER JOIN CV_DATA CDF                                         ";
                strSql += CRLF + "     ON CDF.WH_TYP = CDT.WH_TYP AND CDF.MC_NO = :FROM_MC         ";
                strSql += CRLF + "  WHERE CDT.WH_TYP     = :WH_TYP                                 ";
                strSql += CRLF + "    AND CDT.MC_NO      = :TO_MC                                  ";
                strSql += CRLF + "    AND CDT.LUGG_NO_RD = :LUGG_NO                                ";   // 반영 확인
                strSql += CRLF + "    AND CDF.LUGG_NO_RD = :LUGG_NO2                               ";   // FROM 에 아직 잔존
                strSql += CRLF + "    AND CDF.OD_RQ_YN   = 'N'                                     ";
                _pBdb.mComMain.CommandType = CommandType.Text;
                _pBdb.mComMain.Parameters.Clear();
                _pBdb.mComMain.Parameters.Add("FROM_MC", DbLang.VARCHAR).Value = strFROM_MC;
                _pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = strWH_TYP;
                _pBdb.mComMain.Parameters.Add("TO_MC", DbLang.VARCHAR).Value = strTO_MC;
                _pBdb.mComMain.Parameters.Add("LUGG_NO", DbLang.VARCHAR).Value = strLUGG_NO;
                _pBdb.mComMain.Parameters.Add("LUGG_NO2", DbLang.VARCHAR).Value = strLUGG_NO;
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

                m_dicSeamCopied.Remove(strKey);
                pRTN_MSG = strTitle + "이음새 TRACK " + strFROM_MC + " 작업 데이터 삭제 지시 (복사 완료 확인). [작업번호:" + strLUGG_NO + "]";
                InsertLog(SCH_WH_TYP, pRTN_MSG, "", "", strLUGG_NO, "", strFROM_MC, strTO_MC);
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
         *   호기 = (뱅크 + 1) / 2      뱅크 1,2 -> 1호기 / 3,4 -> 2호기 / ...
         *   WCS_TASK_HOST 의 modDefApp.GetStackerNum 과 같은 식이다.
         *   확인 : 입고 1002 의 랙 07-001-01 -> (7+1)/2 = 4 -> 904 호기
         */
        protected bool GetScNoByLocation(string strLoc, ref string strScNo)
        {
            string strBank = "", strBay = "", strLev = "";
            if (ParseLocation(strLoc, ref strBank, ref strBay, ref strLev) == false)
                return false;

            int nBank;
            if (int.TryParse(strBank, out nBank) == false) return false;
            if (nBank < 1) return false;

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
