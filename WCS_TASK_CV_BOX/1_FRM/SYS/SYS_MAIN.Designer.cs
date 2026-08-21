namespace WCS_TASK_CV
{
    partial class SYS_MAIN
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SYS_MAIN));
            this.lsvCOMM1 = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CH_FILE = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CH_FUNC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.txtTgm = new System.Windows.Forms.TextBox();
            this.splBodySkt = new System.Windows.Forms.SplitContainer();
            this.tab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.picCvDbCn0 = new System.Windows.Forms.PictureBox();
            this.picCvSkt0 = new System.Windows.Forms.PictureBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.btnDelLog = new System.Windows.Forms.Button();
            this.btnXmlSync = new System.Windows.Forms.Button();
            this.chkStopLog = new System.Windows.Forms.CheckBox();
            this.chkShow = new System.Windows.Forms.CheckBox();
            this.splBottom = new System.Windows.Forms.SplitContainer();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.imgLstStat = new System.Windows.Forms.ImageList(this.components);
            this.ImgLstBkgStat = new System.Windows.Forms.ImageList(this.components);
            this.Thread_Timer = new System.Windows.Forms.Timer(this.components);
            this.splBodySkt.Panel1.SuspendLayout();
            this.splBodySkt.Panel2.SuspendLayout();
            this.splBodySkt.SuspendLayout();
            this.tab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCvDbCn0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCvSkt0)).BeginInit();
            this.splBottom.Panel1.SuspendLayout();
            this.splBottom.Panel2.SuspendLayout();
            this.splBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // lsvCOMM1
            // 
            this.lsvCOMM1.AllowColumnReorder = true;
            this.lsvCOMM1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2,
            this.ColumnHeader3,
            this.CH_FILE,
            this.CH_FUNC,
            this.ColumnHeader4,
            this.ColumnHeader5});
            this.lsvCOMM1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsvCOMM1.FullRowSelect = true;
            this.lsvCOMM1.GridLines = true;
            this.lsvCOMM1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lsvCOMM1.Location = new System.Drawing.Point(3, 3);
            this.lsvCOMM1.MultiSelect = false;
            this.lsvCOMM1.Name = "lsvCOMM1";
            this.lsvCOMM1.Size = new System.Drawing.Size(1200, 347);
            this.lsvCOMM1.TabIndex = 790;
            this.lsvCOMM1.UseCompatibleStateImageBehavior = false;
            this.lsvCOMM1.View = System.Windows.Forms.View.Details;
            this.lsvCOMM1.Click += new System.EventHandler(this.lsvMsg_Click);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "Timestamp";
            this.ColumnHeader1.Width = 120;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "Thread No";
            // 
            // ColumnHeader3
            // 
            this.ColumnHeader3.Text = "Cmd";
            // 
            // ColumnHeader4
            // 
            // 
            // CH_FILE / CH_FUNC
            // 
            this.CH_FILE.Text = "FILE";
            this.CH_FILE.Width = 150;
            this.CH_FUNC.Text = "FUNCTION";
            this.CH_FUNC.Width = 200;
            this.ColumnHeader4.Text = "Message";
            this.ColumnHeader4.Width = 500;
            // 
            // ColumnHeader5
            // 
            this.ColumnHeader5.Text = "Telegram";
            this.ColumnHeader5.Width = 900;
            // 
            // txtMsg
            // 
            this.txtMsg.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.txtMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMsg.Location = new System.Drawing.Point(0, 0);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(716, 177);
            this.txtMsg.TabIndex = 0;
            // 
            // txtTgm
            // 
            this.txtTgm.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.txtTgm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTgm.Location = new System.Drawing.Point(0, 0);
            this.txtTgm.Multiline = true;
            this.txtTgm.Name = "txtTgm";
            this.txtTgm.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTgm.Size = new System.Drawing.Size(494, 177);
            this.txtTgm.TabIndex = 1;
            // 
            // splBodySkt
            // 
            this.splBodySkt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splBodySkt.Location = new System.Drawing.Point(0, 0);
            this.splBodySkt.Name = "splBodySkt";
            this.splBodySkt.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splBodySkt.Panel1
            // 
            this.splBodySkt.Panel1.Controls.Add(this.tab);
            this.splBodySkt.Panel1.Controls.Add(this.pnlTop);
            // 
            // splBodySkt.Panel2
            // 
            this.splBodySkt.Panel2.Controls.Add(this.splBottom);
            this.splBodySkt.Size = new System.Drawing.Size(1214, 615);
            this.splBodySkt.SplitterDistance = 434;
            this.splBodySkt.TabIndex = 794;
            // 
            // tab
            // 
            this.tab.Controls.Add(this.tabPage1);
            this.tab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab.Location = new System.Drawing.Point(0, 55);
            this.tab.Name = "tab";
            this.tab.SelectedIndex = 0;
            this.tab.Size = new System.Drawing.Size(1214, 379);
            this.tab.TabIndex = 790;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lsvCOMM1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1206, 353);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "COMM1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.picCvDbCn0);
            this.pnlTop.Controls.Add(this.picCvSkt0);
            this.pnlTop.Controls.Add(this.checkBox1);
            this.pnlTop.Controls.Add(this.checkBox2);
            this.pnlTop.Controls.Add(this.btnDelLog);
            this.pnlTop.Controls.Add(this.btnXmlSync);
            this.pnlTop.Controls.Add(this.chkStopLog);
            this.pnlTop.Controls.Add(this.chkShow);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1214, 55);
            this.pnlTop.TabIndex = 789;
            // 
            // picCvDbCn0
            // 
            this.picCvDbCn0.Location = new System.Drawing.Point(7, 6);
            this.picCvDbCn0.Name = "picCvDbCn0";
            this.picCvDbCn0.Size = new System.Drawing.Size(17, 17);
            this.picCvDbCn0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCvDbCn0.TabIndex = 843;
            this.picCvDbCn0.TabStop = false;
            this.picCvDbCn0.Tag = "S";
            this.ToolTip.SetToolTip(this.picCvDbCn0, "C/V #1 Database");
            this.picCvDbCn0.Visible = false;
            // 
            // picCvSkt0
            // 
            this.picCvSkt0.Location = new System.Drawing.Point(7, 29);
            this.picCvSkt0.Name = "picCvSkt0";
            this.picCvSkt0.Size = new System.Drawing.Size(17, 17);
            this.picCvSkt0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCvSkt0.TabIndex = 842;
            this.picCvSkt0.TabStop = false;
            this.picCvSkt0.Tag = "S";
            this.ToolTip.SetToolTip(this.picCvSkt0, "C/V#1 Status");
            this.picCvSkt0.Visible = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(485, 32);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(97, 16);
            this.checkBox1.TabIndex = 841;
            this.checkBox1.Text = "Ascii Display";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(288, 32);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(90, 16);
            this.checkBox2.TabIndex = 840;
            this.checkBox2.Text = "Hex Display";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // btnDelLog
            // 
            this.btnDelLog.Location = new System.Drawing.Point(593, 28);
            this.btnDelLog.Name = "btnDelLog";
            this.btnDelLog.Size = new System.Drawing.Size(97, 23);
            this.btnDelLog.TabIndex = 795;
            this.btnDelLog.Text = "Clear Log";
            this.btnDelLog.UseVisualStyleBackColor = true;
            this.btnDelLog.Click += new System.EventHandler(this.btnDelLog_Click);
            //
            // btnXmlSync
            //
            this.btnXmlSync.Location = new System.Drawing.Point(700, 28);
            this.btnXmlSync.Name = "btnXmlSync";
            this.btnXmlSync.Size = new System.Drawing.Size(120, 23);
            this.btnXmlSync.TabIndex = 798;
            this.btnXmlSync.Text = "XML 필드 동기화";
            this.btnXmlSync.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnXmlSync.ForeColor = System.Drawing.Color.White;
            this.btnXmlSync.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnXmlSync.UseVisualStyleBackColor = false;
            this.btnXmlSync.Click += new System.EventHandler(this.btnXmlSync_Click);
            //
            // chkStopLog
            // 
            this.chkStopLog.AutoSize = true;
            this.chkStopLog.Checked = true;
            this.chkStopLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStopLog.Location = new System.Drawing.Point(485, 6);
            this.chkStopLog.Name = "chkStopLog";
            this.chkStopLog.Size = new System.Drawing.Size(74, 16);
            this.chkStopLog.TabIndex = 794;
            this.chkStopLog.Text = "Stop Log";
            this.chkStopLog.UseVisualStyleBackColor = true;
            // 
            // chkShow
            // 
            this.chkShow.AutoSize = true;
            this.chkShow.Checked = true;
            this.chkShow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShow.Location = new System.Drawing.Point(288, 6);
            this.chkShow.Name = "chkShow";
            this.chkShow.Size = new System.Drawing.Size(189, 16);
            this.chkShow.TabIndex = 793;
            this.chkShow.Text = "See the latest information first";
            this.chkShow.UseVisualStyleBackColor = true;
            // 
            // splBottom
            // 
            this.splBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splBottom.Location = new System.Drawing.Point(0, 0);
            this.splBottom.Name = "splBottom";
            // 
            // splBottom.Panel1
            // 
            this.splBottom.Panel1.Controls.Add(this.txtMsg);
            // 
            // splBottom.Panel2
            // 
            this.splBottom.Panel2.Controls.Add(this.txtTgm);
            this.splBottom.Size = new System.Drawing.Size(1214, 177);
            this.splBottom.SplitterDistance = 716;
            this.splBottom.TabIndex = 791;
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 5000;
            this.ToolTip.InitialDelay = 1000;
            this.ToolTip.ReshowDelay = 500;
            // 
            // imgLstStat
            // 
            this.imgLstStat.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgLstStat.ImageStream")));
            this.imgLstStat.TransparentColor = System.Drawing.Color.Transparent;
            this.imgLstStat.Images.SetKeyName(0, "RecTangleButton_CN.bmp");
            this.imgLstStat.Images.SetKeyName(1, "RecTangleButton_CW.bmp");
            this.imgLstStat.Images.SetKeyName(2, "RecTangleButton_CE.bmp");
            this.imgLstStat.Images.SetKeyName(3, "RecTangleButton_TN.bmp");
            this.imgLstStat.Images.SetKeyName(4, "RecTangleButton_TW.bmp");
            this.imgLstStat.Images.SetKeyName(5, "RecTangleButton_TE.bmp");
            this.imgLstStat.Images.SetKeyName(6, "RecTangleButton_DN.bmp");
            this.imgLstStat.Images.SetKeyName(7, "RecTangleButton_DW.bmp");
            this.imgLstStat.Images.SetKeyName(8, "RecTangleButton_DE.bmp");
            // 
            // ImgLstBkgStat
            // 
            this.ImgLstBkgStat.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImgLstBkgStat.ImageStream")));
            this.ImgLstBkgStat.TransparentColor = System.Drawing.Color.Transparent;
            this.ImgLstBkgStat.Images.SetKeyName(0, "RecTangleButton_N.bmp");
            this.ImgLstBkgStat.Images.SetKeyName(1, "RecTangleButton_W.bmp");
            this.ImgLstBkgStat.Images.SetKeyName(2, "RecTangleButton_E.bmp");
            // 
            // Thread_Timer
            // 
            this.Thread_Timer.Interval = 5000;
            this.Thread_Timer.Tick += new System.EventHandler(this.Thread_Tick);
            // 
            // SYS_MAIN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1214, 615);
            this.Controls.Add(this.splBodySkt);
            this.Font = new System.Drawing.Font("돋움", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SYS_MAIN";
            this.Text = "WCS_TASK_CV_BOX (BOX CV 통신)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SYS_MAIN_FormClosing);
            this.Load += new System.EventHandler(this.SYS_MAIN_Load);
            this.splBodySkt.Panel1.ResumeLayout(false);
            this.splBodySkt.Panel2.ResumeLayout(false);
            this.splBodySkt.ResumeLayout(false);
            this.tab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCvDbCn0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCvSkt0)).EndInit();
            this.splBottom.Panel1.ResumeLayout(false);
            this.splBottom.Panel1.PerformLayout();
            this.splBottom.Panel2.ResumeLayout(false);
            this.splBottom.Panel2.PerformLayout();
            this.splBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ColumnHeader ColumnHeader1;
        internal System.Windows.Forms.ColumnHeader ColumnHeader2;
        internal System.Windows.Forms.ColumnHeader ColumnHeader3;
        internal System.Windows.Forms.ColumnHeader CH_FILE;
        internal System.Windows.Forms.ColumnHeader CH_FUNC;
        internal System.Windows.Forms.ColumnHeader ColumnHeader4;
        internal System.Windows.Forms.ColumnHeader ColumnHeader5;
        internal System.Windows.Forms.ToolTip ToolTip;
        internal System.Windows.Forms.TextBox txtMsg;
        internal System.Windows.Forms.TextBox txtTgm;
        internal System.Windows.Forms.SplitContainer splBodySkt;
        internal System.Windows.Forms.SplitContainer splBottom;
        internal System.Windows.Forms.ImageList imgLstStat;
        internal System.Windows.Forms.ImageList ImgLstBkgStat;
        private System.Windows.Forms.Timer Thread_Timer;
        internal System.Windows.Forms.Panel pnlTop;
        internal System.Windows.Forms.Button btnDelLog;
        internal System.Windows.Forms.Button btnXmlSync;
        internal System.Windows.Forms.CheckBox chkStopLog;
        internal System.Windows.Forms.CheckBox chkShow;
        internal System.Windows.Forms.CheckBox checkBox1;
        internal System.Windows.Forms.CheckBox checkBox2;
        internal System.Windows.Forms.ListView lsvCOMM1;
		private System.Windows.Forms.TabControl tab;
        private System.Windows.Forms.TabPage tabPage1;
		internal System.Windows.Forms.PictureBox picCvDbCn0;
        internal System.Windows.Forms.PictureBox picCvSkt0;
    }
}

