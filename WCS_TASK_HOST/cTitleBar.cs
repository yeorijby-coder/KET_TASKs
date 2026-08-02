using System;
using System.Drawing;
using System.Windows.Forms;

namespace TSK_HostCom
{
    /*
     * 폼 타이틀바 표시 클래스
     *   - 시스템 구성정보(DB / DB TABLE / COMM)를 타이틀에 표시한다.
     *   - 표시할 내용이 현재 창 폭보다 길면 왼쪽으로 흘려보낸다.(전광판 방식)
     */
    public class cTitleBar
    {
        private const string GM_GAP = "        ";    // @.순환 표시시 끝과 처음 사이 간격
        private const int GM_INTERVAL = 250;         // @.흘러가는 속도(ms)

        private readonly Form m_frmOwner;
        private readonly Timer m_tmrScroll = new Timer();
        private string m_strFull = "";
        private int m_nOffset = 0;

        public cTitleBar(Form frmOwner)
        {
            m_frmOwner = frmOwner;

            m_tmrScroll.Interval = GM_INTERVAL;
            m_tmrScroll.Tick += new EventHandler(PsScroll_Tick);

            m_frmOwner.Resize += new EventHandler(PsOwner_Resize);
            m_frmOwner.FormClosed += new FormClosedEventHandler(PsOwner_FormClosed);
        }

        // @@.표시할 전체 타이틀 설정
        public void SetTitle(string pTitle)
        {
            m_strFull = (pTitle == null) ? "" : pTitle;
            m_nOffset = 0;
            PsRefresh();
        }

        // @@.현재 설정된 전체 타이틀(잘리지 않은 원문)
        public string FullText
        {
            get { return m_strFull; }
        }

        #region[Event]
        private void PsOwner_Resize(object sender, EventArgs e)
        {
            m_nOffset = 0;
            PsRefresh();
        }

        private void PsOwner_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_tmrScroll.Stop();
            m_tmrScroll.Dispose();
        }

        private void PsScroll_Tick(object sender, EventArgs e)
        {
            string strLoop = m_strFull + GM_GAP;

            m_nOffset = (m_nOffset + 1) % strLoop.Length;
            m_frmOwner.Text = GfClip(strLoop.Substring(m_nOffset) + strLoop.Substring(0, m_nOffset));
        }
        #endregion

        #region[Method]
        // @@.현재 창 폭에 맞춰 다시 표시한다.
        private void PsRefresh()
        {
            if (m_strFull.Length == 0)
            {
                m_tmrScroll.Stop();
                return;
            }

            if (GfTextWidth(m_strFull) <= GfCaptionWidth())
            {
                // @.창 폭 안에 다 들어가면 흘리지 않는다.
                m_tmrScroll.Stop();
                m_frmOwner.Text = m_strFull;
            }
            else
            {
                m_frmOwner.Text = GfClip(m_strFull);
                m_tmrScroll.Start();
            }
        }

        // @@.타이틀바에서 글자를 그릴 수 있는 폭(픽셀). 아이콘/시스템버튼 영역은 제외.
        private int GfCaptionWidth()
        {
            int nBtn = SystemInformation.CaptionButtonSize.Width * 3;   // @.최소화/최대화/닫기
            int nIcon = SystemInformation.SmallIconSize.Width + 8;      // @.폼 아이콘
            int nWidth = m_frmOwner.Width - nBtn - nIcon - 16;

            return (nWidth < 40) ? 40 : nWidth;
        }

        // @@.문자열 픽셀 폭
        private static int GfTextWidth(string pText)
        {
            return TextRenderer.MeasureText(pText,
                                            SystemFonts.CaptionFont,
                                            new Size(int.MaxValue, int.MaxValue),
                                            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
        }

        // @@.창 폭에 맞게 앞에서부터 잘라낸다.
        private string GfClip(string pText)
        {
            int nMax = GfCaptionWidth();

            if (GfTextWidth(pText) <= nMax) return pText;

            int nLo = 0;
            int nHi = pText.Length;

            while (nLo < nHi)
            {
                int nMid = (nLo + nHi + 1) / 2;

                if (GfTextWidth(pText.Substring(0, nMid)) <= nMax) nLo = nMid;
                else nHi = nMid - 1;
            }

            return pText.Substring(0, nLo);
        }
        #endregion

        #region[Static] DB 접속정보 표시문자열
        // @@.[DB(DB종류) : 데이터베이스명@계정/IP:PORT]
        public static string GfDbInfo()
        {
#if ORACLE
            return "[DB(Oracle) : " + modDefApp.g_User.g_strDbAlias + "@" + modDefApp.g_User.g_strUserID + "]";
#elif SQL
            return "[DB(MS-SQL) : " + modDefApp.g_User.g_strDatabase + "@" + modDefApp.g_User.g_strUserID
                                    + "/" + modDefApp.g_User.g_strDbIP + ":" + modDefApp.g_User.g_strDbPort + "]";
#else
            // @.POSTGRESSQL(철자 주의 : 이 프로젝트의 정의 심볼) 및 기본
            return "[DB(PostgreSQL) : " + modDefApp.g_User.g_strDatabase + "@" + modDefApp.g_User.g_strUserID
                                        + "/" + modDefApp.g_User.g_strDbIP + ":" + modDefApp.g_User.g_strDbPort + "]";
#endif
        }
        #endregion
    }
}
