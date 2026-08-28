namespace WCS_TASK_SC
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
            this.tabPage0 = new System.Windows.Forms.TabPage();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.btnDelLog = new System.Windows.Forms.Button();
            this.chkStopLog = new System.Windows.Forms.CheckBox();
            this.chkShow = new System.Windows.Forms.CheckBox();
            this.splBottom = new System.Windows.Forms.SplitContainer();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.imgLstStat = new System.Windows.Forms.ImageList(this.components);
            this.ImgLstBkgStat = new System.Windows.Forms.ImageList(this.components);
            this.Thread_Timer = new System.Windows.Forms.Timer(this.components);
            this.picScDbCn0 = new System.Windows.Forms.PictureBox();
            this.picScSkt0 = new System.Windows.Forms.PictureBox();
            this.splBodySkt.Panel1.SuspendLayout();
            this.splBodySkt.Panel2.SuspendLayout();
            this.splBodySkt.SuspendLayout();
            this.tab.SuspendLayout();
            this.tabPage0.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.splBottom.Panel1.SuspendLayout();
            this.splBottom.Panel2.SuspendLayout();
            this.splBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picScDbCn0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picScSkt0)).BeginInit();
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
            this.lsvCOMM1.HideSelection = false;
            this.lsvCOMM1.Location = new System.Drawing.Point(3, 3);
            this.lsvCOMM1.MultiSelect = false;
            this.lsvCOMM1.Name = "lsvCOMM1";
            this.lsvCOMM1.Size = new System.Drawing.Size(957, 333);
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
            this.txtMsg.Size = new System.Drawing.Size(573, 171);
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
            this.txtTgm.Size = new System.Drawing.Size(394, 171);
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
            this.splBodySkt.Size = new System.Drawing.Size(971, 595);
            this.splBodySkt.SplitterDistance = 420;
            this.splBodySkt.TabIndex = 794;
            // 
            // tab
            // 
            this.tab.Controls.Add(this.tabPage0);
            this.tab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab.Location = new System.Drawing.Point(0, 55);
            this.tab.Name = "tab";
            this.tab.SelectedIndex = 0;
            this.tab.Size = new System.Drawing.Size(971, 365);
            this.tab.TabIndex = 790;
            // 
            // tabPage0
            // 
            this.tabPage0.Controls.Add(this.lsvCOMM1);
            this.tabPage0.Location = new System.Drawing.Point(4, 22);
            this.tabPage0.Name = "tabPage0";
            this.tabPage0.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage0.Size = new System.Drawing.Size(963, 339);
            this.tabPage0.TabIndex = 0;
            this.tabPage0.Text = "COMM0";
            this.tabPage0.UseVisualStyleBackColor = true;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.checkBox1);
            this.pnlTop.Controls.Add(this.checkBox2);
            this.pnlTop.Controls.Add(this.picScSkt0);
            this.pnlTop.Controls.Add(this.picScDbCn0);
            this.pnlTop.Controls.Add(this.btnDelLog);
            this.pnlTop.Controls.Add(this.chkStopLog);
            this.pnlTop.Controls.Add(this.chkShow);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(971, 55);
            this.pnlTop.TabIndex = 789;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(755, 29);
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
            this.checkBox2.Location = new System.Drawing.Point(558, 29);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(90, 16);
            this.checkBox2.TabIndex = 840;
            this.checkBox2.Text = "Hex Display";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // btnDelLog
            // 
            this.btnDelLog.Location = new System.Drawing.Point(863, 25);
            this.btnDelLog.Name = "btnDelLog";
            this.btnDelLog.Size = new System.Drawing.Size(97, 23);
            this.btnDelLog.TabIndex = 795;
            this.btnDelLog.Text = "Clear Log";
            this.btnDelLog.UseVisualStyleBackColor = true;
            this.btnDelLog.Click += new System.EventHandler(this.btnDelLog_Click);
            // 
            // chkStopLog
            // 
            this.chkStopLog.AutoSize = true;
            this.chkStopLog.Checked = true;
            this.chkStopLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStopLog.Location = new System.Drawing.Point(755, 3);
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
            this.chkShow.Location = new System.Drawing.Point(558, 3);
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
            this.splBottom.Size = new System.Drawing.Size(971, 171);
            this.splBottom.SplitterDistance = 573;
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
            // picScDbCn0
            // 
            this.picScDbCn0.Location = new System.Drawing.Point(4, 12);
            this.picScDbCn0.Name = "picScDbCn0";
            this.picScDbCn0.Size = new System.Drawing.Size(17, 17);
            this.picScDbCn0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picScDbCn0.TabIndex = 838;
            this.picScDbCn0.TabStop = false;
            this.picScDbCn0.Tag = "S";
            this.ToolTip.SetToolTip(this.picScDbCn0, "C/V #3 Database");
            // 
            // picScSkt0
            // 
            this.picScSkt0.Location = new System.Drawing.Point(4, 29);
            this.picScSkt0.Name = "picScSkt0";
            this.picScSkt0.Size = new System.Drawing.Size(17, 17);
            this.picScSkt0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picScSkt0.TabIndex = 839;
            this.picScSkt0.TabStop = false;
            this.picScSkt0.Tag = "S";
            this.ToolTip.SetToolTip(this.picScSkt0, "C/V#3 Status");
            // 
            // SYS_MAIN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(971, 595);
            this.Controls.Add(this.splBodySkt);
            this.Font = new System.Drawing.Font("돋움", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SYS_MAIN";
            this.Text = "WCS_TASK_SC (SC 통신)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SYS_MAIN_FormClosing);
            this.Load += new System.EventHandler(this.SYS_MAIN_Load);
            this.splBodySkt.Panel1.ResumeLayout(false);
            this.splBodySkt.Panel2.ResumeLayout(false);
            this.splBodySkt.ResumeLayout(false);
            this.tab.ResumeLayout(false);
            this.tabPage0.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.splBottom.Panel1.ResumeLayout(false);
            this.splBottom.Panel1.PerformLayout();
            this.splBottom.Panel2.ResumeLayout(false);
            this.splBottom.Panel2.PerformLayout();
            this.splBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picScDbCn0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picScSkt0)).EndInit();
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
        internal System.Windows.Forms.CheckBox chkStopLog;
        internal System.Windows.Forms.CheckBox chkShow;
        internal System.Windows.Forms.CheckBox checkBox1;
        internal System.Windows.Forms.CheckBox checkBox2;
        internal System.Windows.Forms.ListView lsvCOMM1;
		private System.Windows.Forms.TabControl tab;
        private System.Windows.Forms.TabPage tabPage0;
        internal System.Windows.Forms.PictureBox picScSkt0;
        internal System.Windows.Forms.PictureBox picScDbCn0;
    }
}

