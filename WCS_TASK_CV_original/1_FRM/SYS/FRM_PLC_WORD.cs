using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WCS_TASK_CV
{
    // @@@@.PLC 워드 읽기/쓰기 도구
    //      선택한 COMM 의 PLC 에서 D 디바이스의 특정 워드를 읽어 보고, 특정 값을 쓸 수 있다.
    //      게이트 허가(D558) 같은 신호를 현장에서 확인하거나 강제로 켜 볼 때 쓴다.
    //
    //      PLC 소켓은 해당 COMM 의 통신 스레드가 쓰고 있으므로 이 폼이 직접 보내지 않는다.
    //      요청을 그 스레드의 큐에 넣고, 스레드가 다음 주기에 처리한 결과를 받아 보여 준다.
    //      (같은 소켓에 두 곳이 동시에 보내 전문이 섞이는 것을 막는다)
    //      쓰기는 확인창을 거치고, 쓴 내용은 WCS_LOG_PGR 에 남는다.
    public class FRM_PLC_WORD : Form
    {
        private SYS_MAIN m_frmMain;
        private ComboBox cmbComm;
        private TextBox txtAddr, txtCount, txtVal, txtBit, txtOut;
        private CheckBox chkHex;
        private Button btnRead, btnWrite, btnBitOn, btnBitOff, btnWatch;
        // @.변화 감시 : 지정한 범위를 짧은 주기로 읽어 바뀐 워드만 시각과 함께 적는다.
        //   기존 ECS 와 WCS 를 번갈아 돌리며 어느 워드를 누가 바꾸는지, PLC 가 언제 반응하는지 볼 때 쓴다.
        private System.Windows.Forms.Timer m_tmrWatch;
        private PlcWordReq m_watchReq;
        private int[] m_watchPrev;
        private int m_watchAddr, m_watchCount, m_watchComm;
        private DateTime m_watchSent;
        private int m_watchFail;
        private Label lblState;

        public FRM_PLC_WORD(SYS_MAIN frmMain)
        {
            m_frmMain = frmMain;
            BuildUi();
            FillComm();
        }

        #region UI
        private void BuildUi()
        {
            this.Text = "PLC 워드 읽기/쓰기";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(640, 470);
            this.MinimumSize = new Size(560, 420);
            this.Font = new Font("맑은 고딕", 9F);

            int x = 12, y = 12;
            AddLabel("통신(COMM)", x, y + 4);
            cmbComm = new ComboBox { Left = x + 90, Top = y, Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbComm);

            y += 36;
            AddLabel("D 주소(10진)", x, y + 4);
            txtAddr = new TextBox { Left = x + 90, Top = y, Width = 80, Text = "558" };
            this.Controls.Add(txtAddr);
            AddLabel("개수", x + 190, y + 4, 34);       // 폭을 줄인다. 88 이면 뒤에 오는 개수 입력칸을 덮어 가린다
            txtCount = new TextBox { Left = x + 230, Top = y, Width = 50, Text = "1" };
            this.Controls.Add(txtCount);
            chkHex = new CheckBox { Left = x + 300, Top = y + 2, Width = 220, Text = "값을 16진수로 입력/표시", Checked = true };
            this.Controls.Add(chkHex);

            y += 36;
            AddLabel("쓸 값", x, y + 4);
            txtVal = new TextBox { Left = x + 90, Top = y, Width = 360 };
            this.Controls.Add(txtVal);

            y += 36;
            AddLabel("비트(0~15)", x, y + 4);
            txtBit = new TextBox { Left = x + 90, Top = y, Width = 50, Text = "4" };
            this.Controls.Add(txtBit);

            y += 40;
            btnRead = new Button { Left = x, Top = y, Width = 110, Height = 28, Text = "읽기" };
            btnWrite = new Button { Left = x + 120, Top = y, Width = 150, Height = 28, Text = "워드 쓰기(값 덮어쓰기)" };
            btnBitOn = new Button { Left = x + 280, Top = y, Width = 150, Height = 28, Text = "비트 켜기(읽고 OR)" };
            btnBitOff = new Button { Left = x + 440, Top = y, Width = 150, Height = 28, Text = "비트 끄기(읽고 AND)" };
            this.Controls.Add(btnRead); this.Controls.Add(btnWrite); this.Controls.Add(btnBitOn); this.Controls.Add(btnBitOff);
            btnRead.Click += delegate { Run(0); };
            btnWrite.Click += delegate { Run(1); };
            btnBitOn.Click += delegate { Run(2); };
            btnBitOff.Click += delegate { Run(3); };

            y += 34;
            btnWatch = new Button { Left = x, Top = y, Width = 150, Height = 28, Text = "변화 감시 시작" };
            this.Controls.Add(btnWatch);
            btnWatch.Click += delegate { ToggleWatch(); };
            m_tmrWatch = new System.Windows.Forms.Timer { Interval = 200 };
            m_tmrWatch.Tick += delegate { WatchTick(); };
            this.FormClosing += delegate { m_tmrWatch.Stop(); };

            y += 36;
            lblState = new Label { Left = x, Top = y, Width = 610, Height = 32, ForeColor = Color.DimGray,
                Text = "여러 워드의 값은 쉼표로 구분합니다. 16진은 0010 또는 0x10, 10진은 16 처럼 적습니다.\r\n쓰기는 확인창을 거치고 WCS_LOG_PGR 에 기록됩니다." };
            this.Controls.Add(lblState);

            y += 40;
            txtOut = new TextBox { Left = x, Top = y, Width = 616, Height = this.ClientSize.Height - y - 12,
                Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true,
                Font = new Font("Consolas", 9.5F), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            this.Controls.Add(txtOut);
        }

        private void AddLabel(string strText, int x, int y, int nWidth = 88)
        {
            Label l = new Label { Left = x, Top = y, Width = nWidth, Text = strText, AutoSize = false };
            this.Controls.Add(l);
        }

        private void FillComm()
        {
            cmbComm.Items.Clear();
            for (int ii = 0; ii < m_frmMain.ProcessCount; ii++)
            {
                if (m_frmMain.m_thCvThread[ii] == null) continue;
                cmbComm.Items.Add(new CommItem(ii, "COMM" + ii + "  PLC " + m_frmMain.m_strPLC_NO[ii]
                                  + "  " + m_frmMain.m_strCOMM_IP[ii] + ":" + m_frmMain.m_nCOMM_CUR_PORT[ii]));
            }
            if (cmbComm.Items.Count > 0) cmbComm.SelectedIndex = 0;
        }

        private class CommItem
        {
            public int Idx; public string Text;
            public CommItem(int i, string t) { Idx = i; Text = t; }
            public override string ToString() { return Text; }
        }
        #endregion

        #region 실행
        private void Out(string strLine)
        {
            txtOut.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + strLine + "\r\n");
        }

        private bool ParseInt(string strText, bool bHexDefault, out int nVal)
        {
            nVal = 0;
            string t = (strText ?? "").Trim();
            if (t == "") return false;
            bool bHex = bHexDefault;
            if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) { bHex = true; t = t.Substring(2); }
            try
            {
                nVal = bHex ? Convert.ToInt32(t, 16) : Convert.ToInt32(t, 10);
                return true;
            }
            catch { return false; }
        }

        private static string Bin16(int n)
        {
            string s = Convert.ToString(n & 0xFFFF, 2).PadLeft(16, '0');
            return s.Substring(0, 4) + " " + s.Substring(4, 4) + " " + s.Substring(8, 4) + " " + s.Substring(12, 4);
        }

        // nOp : 0 읽기, 1 워드 쓰기, 2 비트 켜기, 3 비트 끄기
        private void Run(int nOp)
        {
            CommItem ci = cmbComm.SelectedItem as CommItem;
            if (ci == null) { MessageBox.Show("통신(COMM)을 고르십시오."); return; }

            int nAddr, nCount = 1, nBit = 0;
            if (!ParseInt(txtAddr.Text, false, out nAddr) || nAddr < 0) { MessageBox.Show("D 주소를 10진수로 적으십시오."); return; }
            if (!ParseInt(txtCount.Text, false, out nCount) || nCount < 1 || nCount > 100) { MessageBox.Show("개수는 1~100 입니다."); return; }

            PlcWordReq r = new PlcWordReq();
            r.Op = nOp; r.Addr = nAddr; r.Count = nCount;

            string strDesc = "COMM" + ci.Idx + " D" + nAddr;
            if (nOp == 1)
            {
                string[] arr = txtVal.Text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 0) { MessageBox.Show("쓸 값을 적으십시오."); return; }
                r.Values = new int[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    int v;
                    if (!ParseInt(arr[i], chkHex.Checked, out v) || v < 0 || v > 0xFFFF) { MessageBox.Show("값이 잘못되었습니다: " + arr[i] + " (0~65535 / 0000~FFFF)"); return; }
                    r.Values[i] = v;
                }
                r.Count = arr.Length;
                strDesc += " 에 " + arr.Length + "워드 쓰기 [" + txtVal.Text.Trim() + "]";
            }
            else if (nOp == 2 || nOp == 3)
            {
                if (!ParseInt(txtBit.Text, false, out nBit) || nBit < 0 || nBit > 15) { MessageBox.Show("비트는 0~15 입니다."); return; }
                r.Bit = nBit; r.Count = 1;
                strDesc += "." + nBit + (nOp == 2 ? " 비트 켜기" : " 비트 끄기");
            }
            else
            {
                strDesc += " 에서 " + nCount + "워드 읽기";
            }

            if (nOp != 0)
            {
                if (MessageBox.Show(strDesc + "\r\n\r\nPLC 에 실제로 씁니다. 계속하시겠습니까?", "PLC 쓰기 확인",
                                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                    return;
            }

            CvThread th = m_frmMain.m_thCvThread[ci.Idx];
            if (th == null || !th.IsOpen) { Out("실패: " + strDesc + " - 이 COMM 은 PLC 와 연결되어 있지 않습니다."); return; }

            SetBusy(true);
            Out("요청: " + strDesc);
            th.EnqueueWordReq(r);

            // 통신 스레드가 처리할 때까지 기다린다. (화면이 멈추지 않게 짧게 쉬며 이벤트를 처리)
            DateTime tEnd = DateTime.Now.AddSeconds(10);
            while (!r.Done && DateTime.Now < tEnd)
            {
                Application.DoEvents();
                Thread.Sleep(30);
            }
            SetBusy(false);

            if (!r.Done) { Out("실패: 10초 안에 처리되지 않았습니다. (통신 스레드가 멈췄거나 연결이 끊김)"); return; }
            if (!r.Ok) { Out("실패: " + r.Msg); return; }

            if (r.Result != null && r.Result.Length > 0)
            {
                Out("결과: " + r.Msg);
                for (int i = 0; i < r.Result.Length; i++)
                    Out(string.Format("  D{0,-5} = {1:X4}h  ({2,5})  {3}", r.Addr + i, r.Result[i], r.Result[i], Bin16(r.Result[i])));
            }
            else
            {
                Out("완료: " + r.Msg);
            }
        }

        #region 변화 감시
        private void ToggleWatch()
        {
            if (m_tmrWatch.Enabled)
            {
                m_tmrWatch.Stop(); m_watchReq = null; m_watchPrev = null;
                btnWatch.Text = "변화 감시 시작";
                Out("감시를 멈췄습니다.");
                return;
            }
            CommItem ci = cmbComm.SelectedItem as CommItem;
            if (ci == null) { MessageBox.Show("통신(COMM)을 고르십시오."); return; }
            int nAddr, nCount;
            if (!ParseInt(txtAddr.Text, false, out nAddr) || nAddr < 0) { MessageBox.Show("D 주소를 10진수로 적으십시오."); return; }
            if (!ParseInt(txtCount.Text, false, out nCount) || nCount < 1 || nCount > 100) { MessageBox.Show("개수는 1~100 입니다."); return; }
            CvThread th = m_frmMain.m_thCvThread[ci.Idx];
            if (th == null || !th.IsOpen) { Out("실패: 이 COMM 은 PLC 와 연결되어 있지 않습니다."); return; }
            m_watchComm = ci.Idx; m_watchAddr = nAddr; m_watchCount = nCount; m_watchPrev = null; m_watchReq = null; m_watchFail = 0;
            Out("감시 시작: COMM" + ci.Idx + " D" + nAddr + " 부터 " + nCount + "워드 (바뀐 것만 기록)");
            btnWatch.Text = "변화 감시 중지";
            m_tmrWatch.Start();
        }

        private void WatchLine(string strLine)
        {
            txtOut.AppendText(strLine + Environment.NewLine);
        }

        private void WatchTick()
        {
            CvThread th = m_frmMain.m_thCvThread[m_watchComm];
            if (th == null || !th.IsOpen) { Out("감시 중단: PLC 연결이 끊겼습니다."); ToggleWatch(); return; }

            if (m_watchReq != null)
            {
                if (!m_watchReq.Done)
                {
                    if ((DateTime.Now - m_watchSent).TotalSeconds > 8) { Out("감시 중단: 8초 안에 응답이 없습니다."); ToggleWatch(); }
                    return;
                }
                PlcWordReq r = m_watchReq; m_watchReq = null;
                if (!r.Ok)
                {
                    // 일시적인 읽기 실패(시간 초과 등)는 건너뛰고 계속 본다. 연속 10번 실패하면 멈춘다.
                    m_watchFail++;
                    if (m_watchFail == 1) WatchLine(DateTime.Now.ToString("HH:mm:ss.fff") + "  읽기 실패(다시 시도): " + r.Msg);
                    if (m_watchFail >= 10) { Out("감시 중단: 연속 10번 읽지 못했습니다. (" + r.Msg + ")"); ToggleWatch(); return; }
                }
                else
                {
                    if (m_watchFail > 0) WatchLine(DateTime.Now.ToString("HH:mm:ss.fff") + "  읽기 복구 (실패 " + m_watchFail + "번)");
                    m_watchFail = 0;
                    WatchProcess(r);
                }
            }

            PlcWordReq q = new PlcWordReq();
            q.Op = 0; q.Addr = m_watchAddr; q.Count = m_watchCount;
            m_watchReq = q; m_watchSent = DateTime.Now;
            th.EnqueueWordReq(q);
        }

        private void WatchProcess(PlcWordReq r)
        {
            {
                string strTime = DateTime.Now.ToString("HH:mm:ss.fff");
                if (m_watchPrev == null)
                {
                    m_watchPrev = r.Result;
                    WatchLine(strTime + "  기준값 D" + m_watchAddr + "~D" + (m_watchAddr + m_watchCount - 1));
                    for (int i = 0; i < r.Result.Length; i++)
                        if (r.Result[i] != 0)
                            WatchLine(string.Format("            D{0,-5} = {1:X4}h  {2}", m_watchAddr + i, r.Result[i], Bin16(r.Result[i])));
                    WatchLine("            (0 인 워드는 생략)");
                }
                else
                {
                    for (int i = 0; i < r.Result.Length && i < m_watchPrev.Length; i++)
                    {
                        if (r.Result[i] == m_watchPrev[i]) continue;
                        WatchLine(string.Format("{0}  D{1,-5} {2:X4}h -> {3:X4}h   {4}", strTime, m_watchAddr + i, m_watchPrev[i], r.Result[i], Bin16(r.Result[i])));
                    }
                    m_watchPrev = r.Result;
                }
                if (txtOut.TextLength > 60000) txtOut.Text = txtOut.Text.Substring(txtOut.TextLength - 30000);
            }
        }
        #endregion

        private void SetBusy(bool b)
        {
            btnRead.Enabled = btnWrite.Enabled = btnBitOn.Enabled = btnBitOff.Enabled = !b;   // 감시 버튼은 감시 중에도 눌러 멈출 수 있게 둔다
            this.Cursor = b ? Cursors.WaitCursor : Cursors.Default;
        }
        #endregion
    }
}
