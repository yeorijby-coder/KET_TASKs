using System;
using System.Collections.Generic;
using System.Text;

namespace TSK_COMM_IOSCH
{
    //public enum EN_JOB_TYPE
    //{
    //    enJobTypeAutoSto = 1,
    //    enJobTypeAutoRet = 2,
    //    enJobTypeAutoR2R = 3,
    //    enJobTypeAutoMove = 4,
    //    enJobTypeManual = 21
    //};

    public class JobItem
    {
        public int _nLuggNum;
        public string _strJobType;
        public string _strCommandID;
        public int _nPriority;

        public string _strCarrierID;
        public string _strSOURCE;
        public string _strDEST;
        public string _strFinalDEST;

        public string _strTransferSRC;
        public string _strTransferDST;

        public DateTime _tCommandTIME;
        public DateTime _tInstallTIME;

        public string _strCarrierLOC;
        public int _nSrcUnitType;
        public int _nDstUnitType;

        //CUnit* m_pSourceUnit;
        //CUnit* m_pDestUnit;

        public string _strProcessStep;

        public bool _bNULL;

        public int _nProdGrade;

        public string _strFrZone;
        public string _strToZone;
        public string _strWmsJob;		//
        public string _strBoxIndex;      // 제품 등급-ProdGrade
        public string _strBoxSeqID;
        public string _strBoxNo;
        public string _strBoxWeight;
        public string _strEmptyBoxWeight;
        public string _strLotNo;
        public string _strProdGbn;
        public string _strCCID;
        public string _strCCCnt;
        public string _strURodNo;
        public string _strChuckRemoveGbn;
        public string _strProdGrade;
        public string _strWeight;
        public string _strProductGroup;
        public string _strJisiDate;

        public string _chipLuggNo;

        //---?? 공정정보 관련 이므로 추후 삭제 검토
        public string _strProcessZone;		// (1:Stocker Zone, 2:Breaking Zone, 3:Bagging Zone)
        public string _strManualFlag;
        public string _strLabelInfo;

        public string _strJobNoDB;
        public string _strJobTypeDB;
        public string _strScNoDB;
        public string _strJobStat;
        public string _strIOGB;
        public string _strUserID;
        public bool IsNull()
        {
            return _bNULL;
        }

        public string GetCodeBoxType()
        {
            return JobMng.GetCodeBoxType(_strWmsJob);
        }
        public int GetTransferSRC()
        {
            return Convert.ToInt16("0" + _strTransferSRC);
        }
        public int GetTransferDST()
        {
            return Convert.ToInt16("0" + _strTransferDST);
        }
        public string GetLugNo()
        {
            return _nLuggNum.ToString();
        }

        public void InitMember()
        {
	        _nLuggNum = 0;
            _strJobType = "";
	        _strCommandID = "";
	        _nPriority = 1;

	        _strCarrierID = "";
	        _strSOURCE = "";
	        _strDEST = "";
	        _strTransferSRC = "";
	        _strTransferDST = "";
	        _strFinalDEST = "";
	        _strCarrierLOC = "";

 
	        _nSrcUnitType = 0;
	        _nDstUnitType = 0;

            _nProdGrade = 0;

	        _strFrZone = "";
	        _strToZone	 = "";
	        _strWmsJob = "";
	        _strBoxIndex = "";
	        _strBoxSeqID = "";
	        _strBoxNo = "";
	        _strBoxWeight = "";
	        _strEmptyBoxWeight = "";
	        _strLotNo = "";
	        _strProdGbn = "";
	        _strCCID = "";
	        _strCCCnt = "";
	        _strURodNo = "";
	        _strChuckRemoveGbn = "";
	        _strProdGrade = "";
	        _strWeight = "";
	        _strJisiDate = "";
	        _strProductGroup = "";
            _strProcessZone = "";
	        _strManualFlag = "";
	        _strLabelInfo = "";

	        _strJobNoDB = "";
	        _strJobTypeDB = "";
	        _strScNoDB = "";
        }    
    }

    public struct REMOVELIST
    {
        public int nMaxCount;
        public bool bChoice;
        public string strOccurDate;
        public string strOccurNo;
        public string strFromLotID;
        public string strBoxID;
        public string strRemoveType;
        public string strToLotID;
        public string strBoxWeight;
        public string strCCIDs;
        public string strCCCnts;
        public string strRemoveWeight;
    }
}