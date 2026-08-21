using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TSK_COMM_IOSCH
{
    /*
     * 로그 리스트뷰의 열을 헤더에서 오른쪽 클릭해 켜고 끈다.
     *
     *   ListView 에는 열 숨김이 없어 폭을 0 으로 접는 방식을 쓴다.
     *   접어 둔 열은 마우스로 다시 벌릴 수 없게 ColumnWidthChanging 에서 막는다.
     *   (다시 켜는 것은 메뉴로만 한다. 그래야 접힌 열이 실수로 살아나지 않는다.)
     *
     *   헤더는 ListView 의 자식 창(SysHeader32)이라 ListView 의 마우스 이벤트가
     *   오지 않는다. LVM_GETHEADER 로 헤더 핸들을 얻어 직접 붙는다.
     */
    internal class ListViewColumnMenu : NativeWindow, IDisposable
    {
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETHEADER = LVM_FIRST + 31;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_CONTEXTMENU = 0x007B;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private readonly ListView m_lsv;
        private readonly ContextMenuStrip m_menu = new ContextMenuStrip();
        private readonly int[] m_nWidth;        // 접기 전 폭. 다시 켤 때 그대로 돌려준다.
        private bool m_bApplying = false;       // 우리가 폭을 바꾸는 중이면 막지 않는다.

        public ListViewColumnMenu(ListView lsv)
        {
            m_lsv = lsv;

            m_nWidth = new int[m_lsv.Columns.Count];
            for (int i = 0; i < m_lsv.Columns.Count; i++)
            {
                m_nWidth[i] = m_lsv.Columns[i].Width;
            }

            m_lsv.ColumnWidthChanging += OnColumnWidthChanging;
            m_lsv.HandleCreated += OnHandleCreated;
            m_lsv.HandleDestroyed += OnHandleDestroyed;

            if (m_lsv.IsHandleCreated) AttachHeader();
        }

        private void OnHandleCreated(object sender, EventArgs e)
        {
            AttachHeader();
        }

        private void OnHandleDestroyed(object sender, EventArgs e)
        {
            if (Handle != IntPtr.Zero) ReleaseHandle();
        }

        private void AttachHeader()
        {
            IntPtr hHeader = SendMessage(m_lsv.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            if (hHeader == IntPtr.Zero) return;

            if (Handle != IntPtr.Zero) ReleaseHandle();
            AssignHandle(hHeader);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_RBUTTONUP)
            {
                //  기본 처리를 먼저 태워 헤더가 잡고 있는 마우스 캡처를 풀게 한다.
                //  캡처가 살아 있는 채로 메뉴를 띄우면 바로 닫혀 버린다.
                base.WndProc(ref m);

                try
                {
                    m_lsv.BeginInvoke(new MethodInvoker(ShowMenu));
                }
                catch
                {
                }
                return;
            }

            if (m.Msg == WM_CONTEXTMENU)
            {
                //  헤더가 부모(ListView)로 넘기는 것을 막는다. 메뉴는 위에서 띄운다.
                return;
            }

            base.WndProc(ref m);
        }

        private void ShowMenu()
        {
            try
            {
                m_menu.Items.Clear();

                for (int i = 0; i < m_lsv.Columns.Count; i++)
                {
                    ColumnHeader col = m_lsv.Columns[i];

                    ToolStripMenuItem item = new ToolStripMenuItem(col.Text);
                    item.Checked = (col.Width > 0);
                    item.CheckOnClick = false;
                    item.Tag = i;
                    item.Click += OnMenuItemClick;

                    m_menu.Items.Add(item);
                }

                //  주인을 ListView 로 두어야 포커스/닫힘이 보통 컨텍스트 메뉴처럼 돈다.
                m_menu.Show(m_lsv, m_lsv.PointToClient(Cursor.Position));
            }
            catch
            {
            }
        }

        private void OnMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;

            int nIndex = (int)item.Tag;
            if (nIndex < 0 || nIndex >= m_lsv.Columns.Count) return;

            ColumnHeader col = m_lsv.Columns[nIndex];

            if (col.Width > 0)
            {
                // 마지막 한 열까지 접으면 아무것도 안 보인다.
                int nShown = 0;
                foreach (ColumnHeader c in m_lsv.Columns)
                {
                    if (c.Width > 0) nShown++;
                }
                if (nShown <= 1) return;

                m_nWidth[nIndex] = col.Width;
                SetWidth(col, 0);
            }
            else
            {
                SetWidth(col, m_nWidth[nIndex] > 0 ? m_nWidth[nIndex] : 100);
            }
        }

        private void SetWidth(ColumnHeader col, int nWidth)
        {
            m_bApplying = true;
            col.Width = nWidth;
            m_bApplying = false;
        }

        private void OnColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (m_bApplying) return;
            if (e.ColumnIndex < 0 || e.ColumnIndex >= m_lsv.Columns.Count) return;

            if (m_lsv.Columns[e.ColumnIndex].Width == 0)
            {
                // 접어 둔 열이다. 마우스로 벌리지 못하게 한다.
                e.NewWidth = 0;
                e.Cancel = true;
                return;
            }

            m_nWidth[e.ColumnIndex] = e.NewWidth;
        }

        public void Dispose()
        {
            try
            {
                m_lsv.ColumnWidthChanging -= OnColumnWidthChanging;
                m_lsv.HandleCreated -= OnHandleCreated;
                m_lsv.HandleDestroyed -= OnHandleDestroyed;
                if (Handle != IntPtr.Zero) ReleaseHandle();
                m_menu.Dispose();
            }
            catch
            {
            }
        }
    }
}
