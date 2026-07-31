namespace TSK_COMM_IOSCH
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
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.picDbCn0 = new System.Windows.Forms.PictureBox();
            this.imgLstStat = new System.Windows.Forms.ImageList(this.components);
            this.ImgLstBkgStat = new System.Windows.Forms.ImageList(this.components);
            this.Thread_Timer = new System.Windows.Forms.Timer(this.components);
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.btnDelLog = new System.Windows.Forms.Button();
            this.chkShow = new System.Windows.Forms.CheckBox();
            this.chkStopLog = new System.Windows.Forms.CheckBox();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.chkLogDelTest = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.TABLOG = new System.Windows.Forms.TabControl();
            this.tbR = new System.Windows.Forms.TabPage();
            this.lsvR = new System.Windows.Forms.ListView();
            this.CH01 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CH02 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CH03 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CH04 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.picDbCn0)).BeginInit();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.TABLOG.SuspendLayout();
            this.tbR.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 5000;
            this.ToolTip.InitialDelay = 1000;
            this.ToolTip.ReshowDelay = 500;
            // 
            // picDbCn0
            // 
            this.picDbCn0.Location = new System.Drawing.Point(12, 18);
            this.picDbCn0.Name = "picDbCn0";
            this.picDbCn0.Size = new System.Drawing.Size(18, 24);
            this.picDbCn0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDbCn0.TabIndex = 813;
            this.picDbCn0.TabStop = false;
            this.picDbCn0.Tag = "S";
            this.ToolTip.SetToolTip(this.picDbCn0, "Scheduler Database");
            this.picDbCn0.Visible = false;
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
            this.Thread_Timer.Interval = 1000;
            this.Thread_Timer.Tick += new System.EventHandler(this.Thread_Tick);
            // 
            // txtMsg
            // 
            this.txtMsg.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.txtMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMsg.Location = new System.Drawing.Point(0, 0);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(1012, 179);
            this.txtMsg.TabIndex = 0;
            // 
            // btnDelLog
            // 
            this.btnDelLog.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnDelLog.Location = new System.Drawing.Point(905, 33);
            this.btnDelLog.Name = "btnDelLog";
            this.btnDelLog.Size = new System.Drawing.Size(97, 23);
            this.btnDelLog.TabIndex = 795;
            this.btnDelLog.Text = "Clear Log";
            this.btnDelLog.UseVisualStyleBackColor = true;
            this.btnDelLog.Click += new System.EventHandler(this.btnDelLog_Click);
            // 
            // chkShow
            // 
            this.chkShow.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.chkShow.AutoSize = true;
            this.chkShow.Checked = true;
            this.chkShow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShow.Location = new System.Drawing.Point(710, 8);
            this.chkShow.Name = "chkShow";
            this.chkShow.Size = new System.Drawing.Size(189, 16);
            this.chkShow.TabIndex = 818;
            this.chkShow.Text = "See the latest information first";
            this.chkShow.UseVisualStyleBackColor = true;
            // 
            // chkStopLog
            // 
            this.chkStopLog.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.chkStopLog.AutoSize = true;
            this.chkStopLog.Location = new System.Drawing.Point(921, 7);
            this.chkStopLog.Name = "chkStopLog";
            this.chkStopLog.Size = new System.Drawing.Size(74, 16);
            this.chkStopLog.TabIndex = 794;
            this.chkStopLog.Text = "Stop Log";
            this.chkStopLog.UseVisualStyleBackColor = true;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.chkLogDelTest);
            this.pnlTop.Controls.Add(this.chkStopLog);
            this.pnlTop.Controls.Add(this.picDbCn0);
            this.pnlTop.Controls.Add(this.btnDelLog);
            this.pnlTop.Controls.Add(this.chkShow);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1012, 60);
            this.pnlTop.TabIndex = 795;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(837, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 12);
            this.label1.TabIndex = 836;
            this.label1.Text = "2021.01.30";
            // 
            // chkLogDelTest
            // 
            this.chkLogDelTest.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.chkLogDelTest.AutoSize = true;
            this.chkLogDelTest.Location = new System.Drawing.Point(476, 8);
            this.chkLogDelTest.Name = "chkLogDelTest";
            this.chkLogDelTest.Size = new System.Drawing.Size(217, 16);
            this.chkLogDelTest.TabIndex = 794;
            this.chkLogDelTest.Text = "Log Delete Test(Always Read ini) ";
            this.chkLogDelTest.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 60);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.TABLOG);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtMsg);
            this.splitContainer1.Size = new System.Drawing.Size(1012, 673);
            this.splitContainer1.SplitterDistance = 490;
            this.splitContainer1.TabIndex = 796;
            // 
            // TABLOG
            // 
            this.TABLOG.Controls.Add(this.tbR);
            this.TABLOG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TABLOG.Location = new System.Drawing.Point(0, 0);
            this.TABLOG.Name = "TABLOG";
            this.TABLOG.SelectedIndex = 0;
            this.TABLOG.Size = new System.Drawing.Size(1012, 490);
            this.TABLOG.TabIndex = 0;
            // 
            // tbR
            // 
            this.tbR.Controls.Add(this.lsvR);
            this.tbR.Location = new System.Drawing.Point(4, 22);
            this.tbR.Name = "tbR";
            this.tbR.Size = new System.Drawing.Size(1004, 464);
            this.tbR.TabIndex = 18;
            this.tbR.Text = "Scheduler";
            this.tbR.UseVisualStyleBackColor = true;
            // 
            // lsvR
            // 
            this.lsvR.AllowColumnReorder = true;
            this.lsvR.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.CH01,
            this.CH02,
            this.CH03,
            this.CH04});
            this.lsvR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsvR.FullRowSelect = true;
            this.lsvR.GridLines = true;
            this.lsvR.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lsvR.HideSelection = false;
            this.lsvR.Location = new System.Drawing.Point(0, 0);
            this.lsvR.Margin = new System.Windows.Forms.Padding(0);
            this.lsvR.MultiSelect = false;
            this.lsvR.Name = "lsvR";
            this.lsvR.Size = new System.Drawing.Size(1004, 464);
            this.lsvR.TabIndex = 791;
            this.lsvR.UseCompatibleStateImageBehavior = false;
            this.lsvR.View = System.Windows.Forms.View.Details;
            this.lsvR.Click += new System.EventHandler(this.lsvMsg_Click);
            // 
            // CH01
            // 
            this.CH01.Text = "TIMESTAMP";
            this.CH01.Width = 220;
            // 
            // CH02
            // 
            this.CH02.Text = "THREAD NO";
            this.CH02.Width = 95;
            // 
            // CH03
            // 
            this.CH03.Text = "Command";
            this.CH03.Width = 80;
            // 
            // CH04
            // 
            this.CH04.Text = "Message";
            this.CH04.Width = 2000;
            // 
            // SYS_MAIN
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1012, 733);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pnlTop);
            this.Font = new System.Drawing.Font("돋움", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SYS_MAIN";
            this.Text = "IO_TASK_SEMI_FINISH (스케쥴러)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SYS_MAIN_FormClosing);
            this.Load += new System.EventHandler(this.SYS_MAIN_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picDbCn0)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.TABLOG.ResumeLayout(false);
            this.tbR.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ToolTip ToolTip;
        internal System.Windows.Forms.ImageList imgLstStat;
        internal System.Windows.Forms.ImageList ImgLstBkgStat;
		private System.Windows.Forms.Timer Thread_Timer;
		internal System.Windows.Forms.TextBox txtMsg;
		internal System.Windows.Forms.Button btnDelLog;
		internal System.Windows.Forms.CheckBox chkShow;
		internal System.Windows.Forms.CheckBox chkStopLog;
        internal System.Windows.Forms.PictureBox picDbCn0;
		private System.Windows.Forms.Panel pnlTop;
		private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl TABLOG;
        private System.Windows.Forms.TabPage tbR;
        internal System.Windows.Forms.ListView lsvR;
        internal System.Windows.Forms.ColumnHeader CH01;
        internal System.Windows.Forms.ColumnHeader CH02;
        internal System.Windows.Forms.ColumnHeader CH03;
        internal System.Windows.Forms.ColumnHeader CH04;
        internal System.Windows.Forms.CheckBox chkLogDelTest;
        private System.Windows.Forms.Label label1;
    }
}
