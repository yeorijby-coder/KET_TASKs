using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Npgsql;

namespace WCS_TASK_CV
{
    // @@@@.CvSim의 KeyWord.xml / DeviceMap??.xml 을 읽어
    //      CV_DATA 테이블 필드를 동기화하는 폼
    public partial class FRM_XML_FIELD_SYNC : Form
    {
        private string m_strConnectString = "";
        private cXmlFieldSync m_oSync = null;

        public FRM_XML_FIELD_SYNC(string strConnectString)
        {
            InitializeComponent();
            m_strConnectString = strConnectString;
            LoadDefaultXmlPaths();
        }

        #region@@@.로컬 7_DeviceMap 폴더의 XML 자동 세팅 (멜섹 전용)
        private void LoadDefaultXmlPaths()
        {
            try
            {
                string strDir = System.IO.Path.Combine(Application.StartupPath, "7_DeviceMap");
                if (!System.IO.Directory.Exists(strDir)) return;

                string strKeyWord = System.IO.Path.Combine(strDir, "KeyWord.xml");
                if (System.IO.File.Exists(strKeyWord))
                    txtKeyWord.Text = strKeyWord;

                string[] arrMaps = System.IO.Directory.GetFiles(strDir, "DeviceMap*.xml");
                Array.Sort(arrMaps);
                if (arrMaps.Length > 0)
                    txtDeviceMap.Text = string.Join(";", arrMaps);
            }
            catch { }
        }
        #endregion

        #region[Event]btnKeyWord_Click :: KeyWord.xml 선택
        private void btnKeyWord_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "KeyWord.xml|KeyWord.xml|XML 파일(*.xml)|*.xml";
            dlg.Title = "KeyWord.xml 선택";
            if (dlg.ShowDialog() == DialogResult.OK)
                txtKeyWord.Text = dlg.FileName;
        }
        #endregion

        #region[Event]btnDeviceMap_Click :: DeviceMap XML 다중 선택
        private void btnDeviceMap_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "DeviceMap XML|DeviceMap*.xml|XML 파일(*.xml)|*.xml";
            dlg.Title = "DeviceMap XML 선택 (복수 선택 가능)";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == DialogResult.OK)
                txtDeviceMap.Text = string.Join(";", dlg.FileNames);
        }
        #endregion

        #region[Event]btnParse_Click :: XML 파싱(미리보기)
        private void btnParse_Click(object sender, EventArgs e)
        {
            lsvLog.Items.Clear();
            if (!ParseXmls()) return;

            AddLog("[정보] 파싱된 필드 수: " + m_oSync.Fields.Count);
            foreach (cXmlFieldSync.XmlField f in m_oSync.Fields)
            {
                AddLog((f.Required ? "[필수] " : "[추가] ")
                       + f.Name + " → " + f.DbColumn + "  (" + f.Source + ")");
            }
            btnSync.Enabled = true;
        }
        #endregion

        #region[Event]btnSync_Click :: CV_DATA 필드 동기화 실행
        private void btnSync_Click(object sender, EventArgs e)
        {
            if (m_oSync == null || m_oSync.Fields.Count == 0)
            {
                MessageBox.Show("먼저 [XML 파싱]을 실행하세요.", "확인",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("CV_DATA 테이블 필드를 XML 정의대로 조정합니다.\r\n"
                                + "(데이터가 있으면 자동 백업 후 진행)\r\n\r\n계속하시겠습니까?",
                                "필드 동기화", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            NpgsqlConnection cn = null;
            try
            {
                cn = new NpgsqlConnection(m_strConnectString);
                cn.Open();

                bool bOk = false;
                List<string> lstLog = m_oSync.SyncToDb(cn, ref bOk);
                foreach (string strMsg in lstLog)
                    AddLog(strMsg);

                MessageBox.Show(bOk ? "필드 동기화가 완료되었습니다."
                                    : "동기화 중 오류/누락이 있습니다. 로그를 확인하세요.",
                                "필드 동기화",
                                MessageBoxButtons.OK,
                                bOk ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                AddLog("[오류] DB 접속 실패: " + ex.Message);
                MessageBox.Show("DB 접속 실패: " + ex.Message, "오류",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn != null) cn.Close();
            }
        }
        #endregion

        #region@@@.XML 파싱 공통
        private bool ParseXmls()
        {
            if (txtKeyWord.Text.Trim() == "")
            {
                MessageBox.Show("KeyWord.xml 파일을 선택하세요.", "확인",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            m_oSync = new cXmlFieldSync();

            // KeyWord.xml (필수 필드) 먼저 파싱
            if (!m_oSync.ParseKeyWordXml(txtKeyWord.Text.Trim()))
            {
                AddLog("[오류] " + m_oSync.ErrMsg);
                return false;
            }
            AddLog("[정보] KeyWord.xml 파싱 완료");

            // DeviceMap XML (추가 필드)
            if (txtDeviceMap.Text.Trim() != "")
            {
                string[] arrFiles = txtDeviceMap.Text.Split(';');
                foreach (string strFile in arrFiles)
                {
                    if (strFile.Trim() == "") continue;
                    if (!m_oSync.ParseDeviceMapXml(strFile.Trim()))
                    {
                        AddLog("[오류] " + m_oSync.ErrMsg);
                        return false;
                    }
                    AddLog("[정보] " + System.IO.Path.GetFileName(strFile) + " 파싱 완료");
                }
            }
            return true;
        }
        #endregion

        #region@@@.로그 출력
        private void AddLog(string strMsg)
        {
            lsvLog.Items.Add(DateTime.Now.ToString("HH:mm:ss") + "  " + strMsg);
            lsvLog.TopIndex = lsvLog.Items.Count - 1;
        }
        #endregion
    }
}
