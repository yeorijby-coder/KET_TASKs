using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TSK_HostCom
{
    /*
     * cIniOpener :: 이 프로그램이 맨 처음 참조하는 INI 파일을 메모장으로 열어 주는 버튼.
     *
     *   현장에서 설정을 확인하거나 고칠 때 탐색기로 실행 폴더를 찾아 들어가야 했다.
     *   화면에 버튼 하나를 두어 바로 열 수 있게 한다.
     *
     *   버튼 자리는 디자이너에 박지 않고 폼이 뜬 뒤에 잡는다. 프로그램마다 폼 크기와
     *   컨트롤 배치가 달라서 좌표를 하나로 정할 수 없고, 잘못 박으면 기존 컨트롤을
     *   가리기 때문이다. 오른쪽 아래에서부터 훑어 비어 있는 자리에 놓는다.
     *
     *   경로는 GetPrivateProfileString 이 쓰는 것과 같은 상대경로를 그대로 받아
     *   Path.GetFullPath 로 푼다. 프로그램이 실제로 읽는 그 파일이 열린다.
     */
    public static class cIniOpener
    {
        private const int BTN_W = 96;       // 버튼 크기
        private const int BTN_H = 25;
        private const int MARGIN = 8;       // 폼 가장자리에서 띄울 간격
        private const int GAP = 6;          // 다른 컨트롤과 띄울 간격
        private const int STEP = 8;         // 빈 자리를 찾을 때 옮겨 볼 간격

        /*
         * Attach :: 폼에 INI 열기 버튼을 붙인다.
         *   frm         버튼을 붙일 폼
         *   strIniPath  프로그램이 쓰는 INI 경로 (상대경로 그대로 넘기면 된다)
         */
        public static void Attach(Form frm, string strIniPath)
        {
            if (frm == null)
                return;

            // 두 번 부르더라도 버튼이 겹쳐 쌓이지 않게 한다
            if (frm.Controls.ContainsKey("btnOpenIni"))
                return;

            // 자리를 잡는 일은 Shown 까지 미룬다.
            //   Load 안에서는 아직 컨트롤이 다 보이지 않거나 크기가 잡히지 않은 것이 있어,
            //   그 시점에 빈 자리를 찾으면 나중에 나타난 컨트롤과 겹칠 수 있다.
            if (!frm.Visible)
            {
                Form frmTarget = frm;
                string strPath = strIniPath;
                EventHandler onShown = null;
                onShown = delegate(object sender, EventArgs e)
                {
                    frmTarget.Shown -= onShown;
                    Place(frmTarget, strPath);
                };
                frm.Shown += onShown;
                return;
            }

            Place(frm, strIniPath);
        }

        private static void Place(Form frm, string strIniPath)
        {
            if (frm.Controls.ContainsKey("btnOpenIni"))
                return;

            Button btn = new Button();
            btn.Name = "btnOpenIni";
            btn.Text = "INI 열기";
            btn.Size = new Size(BTN_W, BTN_H);
            btn.TabStop = false;
            btn.UseVisualStyleBackColor = true;
            btn.Tag = strIniPath;
            btn.Click += new EventHandler(OnClickOpenIni);

            // 빽빽한 화면도 있어서 큰 크기로 안 되면 조금씩 줄여 가며 자리를 찾는다.
            Size[] sizes = new Size[] { new Size(BTN_W, BTN_H), new Size(78, 22), new Size(64, 20) };
            Point pt = Point.Empty;
            bool bFound = false;
            for (int i = 0; i < sizes.Length && !bFound; i++)
            {
                if (FindFreeSpot(frm, sizes[i], out pt))
                {
                    btn.Size = sizes[i];
                    bFound = true;
                }
            }

            if (bFound)
            {
                // 놓인 쪽 모서리를 따라가게 한다. 창 크기가 바뀌어도 자리를 지킨다.
                btn.Location = pt;
                AnchorStyles anchor = AnchorStyles.None;
                anchor |= (pt.X > frm.ClientSize.Width / 2) ? AnchorStyles.Right : AnchorStyles.Left;
                anchor |= (pt.Y > frm.ClientSize.Height / 2) ? AnchorStyles.Bottom : AnchorStyles.Top;
                btn.Anchor = anchor;

                frm.Controls.Add(btn);
                btn.BringToFront();
                return;
            }

            // 그래도 자리가 없으면 폼을 키워 아래에 전용 띠를 만든다.
            PlaceInNewStrip(frm, btn);
        }

        private static void PlaceInNewStrip(Form frm, Button btn)
        {
            Panel pnl = new Panel();
            pnl.Name = "pnlOpenIni";
            pnl.Height = BTN_H + MARGIN;
            pnl.Dock = DockStyle.Bottom;

            if (frm.WindowState == FormWindowState.Normal)
                frm.Height += pnl.Height;       // 기존 영역이 줄지 않게 폼을 키운다

            frm.Controls.Add(pnl);
            pnl.BringToFront();

            btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn.Location = new Point(pnl.ClientSize.Width - BTN_W - MARGIN, 2);
            pnl.Controls.Add(btn);
        }

        /*
         * FindFreeSpot :: 실제 컨트롤과 겹치지 않는 자리를 찾는다.
         *
         *   폼이 커다란 Panel 하나로 덮여 있는 경우가 많다. 그 Panel 을 피하려 들면
         *   놓을 자리가 없다. 컨테이너의 빈 곳에는 놓아도 되므로, 자식이 있는
         *   컨테이너는 통과해 들어가고 실제로 뭔가 그려지는 잎 컨트롤만 피한다.
         *
         *   다만 탭은 통째로 피한다. 탭을 바꾸면 그 자리에 다른 내용이 올라와
         *   버튼이 그 위를 덮게 되기 때문이다. 메뉴/상태바/툴바도 같은 이유로 피한다.
         */
        private static bool FindFreeSpot(Form frm, Size sz, out Point result)
        {
            result = Point.Empty;
            Rectangle client = frm.ClientRectangle;

            List<Rectangle> used = new List<Rectangle>();
            CollectBlockers(frm, Point.Empty, used);

            for (int y = client.Bottom - sz.Height - MARGIN; y >= client.Top + MARGIN; y -= STEP)
            {
                for (int x = client.Right - sz.Width - MARGIN; x >= client.Left + MARGIN; x -= STEP)
                {
                    Rectangle probe = new Rectangle(x - GAP, y - GAP,
                                                    sz.Width + GAP * 2, sz.Height + GAP * 2);
                    bool bHit = false;
                    for (int i = 0; i < used.Count; i++)
                    {
                        if (used[i].IntersectsWith(probe))
                        {
                            bHit = true;
                            break;
                        }
                    }
                    if (!bHit)
                    {
                        result = new Point(x, y);
                        return true;
                    }
                }
            }

            return false;       // 놓을 자리가 없다
        }

        /*
         * CollectBlockers :: 피해야 할 컨트롤들의 자리를 폼 기준 좌표로 모은다.
         *   pt 는 parent 의 클라이언트 원점이 폼 기준 어디인지를 나타낸다.
         */
        private static void CollectBlockers(Control parent, Point pt, List<Rectangle> list)
        {
            foreach (Control c in parent.Controls)
            {
                if (c == null || !c.Visible)
                    continue;

                Rectangle abs = new Rectangle(pt.X + c.Left, pt.Y + c.Top, c.Width, c.Height);

                if (IsAlwaysAvoid(c))
                {
                    list.Add(abs);
                    continue;
                }

                if (c.Controls.Count > 0)
                {
                    // 컨테이너는 통과한다. 빈 곳에는 놓아도 되기 때문이다.
                    Point inner = new Point(abs.X + c.ClientRectangle.X, abs.Y + c.ClientRectangle.Y);
                    CollectBlockers(c, inner, list);
                }
                else
                {
                    list.Add(abs);
                }
            }
        }

        private static bool IsAlwaysAvoid(Control c)
        {
            // 탭은 내용이 바뀌므로 통째로 피한다. 메뉴/상태바/툴바도 마찬가지다.
            if (c is TabControl) return true;
            if (c is MenuStrip) return true;
            if (c is StatusStrip) return true;
            if (c is ToolStrip) return true;
            return false;
        }

        private static void OnClickOpenIni(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;

            string strPath = btn.Tag as string;
            if (string.IsNullOrEmpty(strPath))
                return;

            try
            {
                // 프로그램이 INI 를 읽을 때와 같은 기준(현재 작업 폴더)으로 푼다
                string strFull = Path.GetFullPath(strPath);

                if (!File.Exists(strFull))
                {
                    MessageBox.Show("INI 파일이 없습니다." + Environment.NewLine + strFull,
                                    "INI 열기", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Process.Start("notepad.exe", "\"" + strFull + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("INI 파일을 열지 못했습니다." + Environment.NewLine + ex.Message,
                                "INI 열기", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
