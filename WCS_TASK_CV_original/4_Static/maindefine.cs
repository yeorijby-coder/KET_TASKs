using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WCS_TASK_CV
{
    public class maindefine
    {
        public string BytesToHexs(byte[] value, int len)
        {
            string cvtvalue = "";
            for (int ii = 0; ii < len; ii++)
            {
                cvtvalue = cvtvalue + string.Format("{0:X2}", value[ii]);
                if (((ii + 1) % 2) == 0) cvtvalue = cvtvalue + " ";
            }
            return cvtvalue;
        }

		/*
		 * 바이트 배열의 [nStartLen, nEndlen) 구간을 16진 문자열로 만든다.
		 *   ※ 세번째 인자는 "길이" 가 아니라 "끝 인덱스" 다.
		 *     길이를 넘기면 시작이 0 이 아닌 경우 빈 문자열이 돌아온다.
		 */
		public string BytesToHexs(byte[] value, int nStartLen, int nEndlen)
		{
			string cvtvalue = "";
			for (int ii = nStartLen; ii < nEndlen; ii++)
			{
				cvtvalue = cvtvalue + string.Format("{0:X2}", value[ii]);
				if (((ii + 1) % 2) == 0) cvtvalue = cvtvalue + "";
			}
			return cvtvalue;
		}



		/*
		 * [Method]Collection 객체에서 컨트롤을 찾아 리턴한다.
		 */
        #region
        public Control PfCtlFind(ref Panel pPnl, string pCtlNm, ref string pRtnMsg)
        {
            Control[] ctl;

            try
            {
                pRtnMsg = "";

                ctl = pPnl.Controls.Find(pCtlNm, true);

                if (ctl.Length == 0)
                {
                    return null;
                }
                else
                {
                    pRtnMsg = "[PfCtlFind]Success::" + "(" + pCtlNm + ")";
                    return ctl[0];
                }
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[PfCtlFind]Error::" + "(" + pCtlNm + ")" + pRtnMsg;
            return null;
        }

        public Control PfCtlFindTab(ref TabControl pPnl, string pCtlNm, ref string pRtnMsg)
        {
            Control[] ctl;

            try
            {
                pRtnMsg = "";

                ctl = pPnl.Controls.Find(pCtlNm, true);

                if (ctl.Length == 0)
                {
                    return null;
                }
                else
                {
                    pRtnMsg = "[PfCtlFind]Success::" + "(" + pCtlNm + ")";
                    return ctl[0];
                }
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[PfCtlFind]Error::" + "(" + pCtlNm + ")" + pRtnMsg;
            return null;
        }
        #endregion
    }
}
