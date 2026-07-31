namespace TSK_HostCom
{
	partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.pnlTop = new System.Windows.Forms.Panel();
            this.chkSimMode = new System.Windows.Forms.CheckBox();
            this.StatusBar1 = new System.Windows.Forms.StatusBar();
            this.ver = new System.Windows.Forms.StatusBarPanel();
            this.Msg = new System.Windows.Forms.StatusBarPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pnlMsg = new System.Windows.Forms.Panel();
            this.chkScSta = new System.Windows.Forms.CheckBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.chkStatus = new System.Windows.Forms.CheckBox();
            this.picCliCom = new System.Windows.Forms.PictureBox();
            this.lsvMsgClient = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtDetail = new System.Windows.Forms.TextBox();
            this.chkShowClient = new System.Windows.Forms.CheckBox();
            this.btnDelLogClient = new System.Windows.Forms.Button();
            this.chkStopLogClient = new System.Windows.Forms.CheckBox();
            this.pnlMsg2 = new System.Windows.Forms.Panel();
            this.Label4 = new System.Windows.Forms.Label();
            this.picSrvCom = new System.Windows.Forms.PictureBox();
            this.lsvMsgServer = new System.Windows.Forms.ListView();
            this.ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imlLog = new System.Windows.Forms.ImageList(this.components);
            this.txtDetail2 = new System.Windows.Forms.TextBox();
            this.chkShowServer = new System.Windows.Forms.CheckBox();
            this.btnDelLogServer = new System.Windows.Forms.Button();
            this.chkStopLogServer = new System.Windows.Forms.CheckBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.imlConnect = new System.Windows.Forms.ImageList(this.components);
            this.tmrMain = new System.Windows.Forms.Timer(this.components);
            this.tmrSTOP_REQ = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ver)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Msg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlMsg.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCliCom)).BeginInit();
            this.pnlMsg2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSrvCom)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(784, 12);
            this.pnlTop.TabIndex = 11;
            // 
            // chkSimMode
            // 
            this.chkSimMode.BackColor = System.Drawing.Color.Transparent;
            this.chkSimMode.Location = new System.Drawing.Point(64, 32);
            this.chkSimMode.Name = "chkSimMode";
            this.chkSimMode.Size = new System.Drawing.Size(119, 16);
            this.chkSimMode.TabIndex = 24;
            this.chkSimMode.Text = "시뮬레이션 모드";
            this.chkSimMode.UseVisualStyleBackColor = false;
            this.chkSimMode.Visible = false;
            this.chkSimMode.CheckedChanged += new System.EventHandler(this.chkSimMode_CheckedChanged);
            // 
            // StatusBar1
            // 
            this.StatusBar1.Location = new System.Drawing.Point(0, 537);
            this.StatusBar1.Name = "StatusBar1";
            this.StatusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.ver,
            this.Msg});
            this.StatusBar1.ShowPanels = true;
            this.StatusBar1.Size = new System.Drawing.Size(784, 24);
            this.StatusBar1.TabIndex = 89;
            this.StatusBar1.Text = "StatusBar1";
            // 
            // ver
            // 
            this.ver.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.ver.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.Raised;
            this.ver.Name = "ver";
            this.ver.Text = "Ver.9999-99-99 10:00";
            this.ver.Width = 130;
            // 
            // Msg
            // 
            this.Msg.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.Msg.Name = "Msg";
            this.Msg.Width = 637;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pnlMsg);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pnlMsg2);
            this.splitContainer1.Size = new System.Drawing.Size(784, 525);
            this.splitContainer1.SplitterDistance = 384;
            this.splitContainer1.TabIndex = 90;
            // 
            // pnlMsg
            // 
            this.pnlMsg.BackColor = System.Drawing.Color.Transparent;
            this.pnlMsg.Controls.Add(this.chkSimMode);
            this.pnlMsg.Controls.Add(this.chkScSta);
            this.pnlMsg.Controls.Add(this.Label1);
            this.pnlMsg.Controls.Add(this.chkStatus);
            this.pnlMsg.Controls.Add(this.picCliCom);
            this.pnlMsg.Controls.Add(this.lsvMsgClient);
            this.pnlMsg.Controls.Add(this.txtDetail);
            this.pnlMsg.Controls.Add(this.chkShowClient);
            this.pnlMsg.Controls.Add(this.btnDelLogClient);
            this.pnlMsg.Controls.Add(this.chkStopLogClient);
            this.pnlMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMsg.Location = new System.Drawing.Point(0, 0);
            this.pnlMsg.Name = "pnlMsg";
            this.pnlMsg.Size = new System.Drawing.Size(384, 525);
            this.pnlMsg.TabIndex = 90;
            // 
            // chkScSta
            // 
            this.chkScSta.BackColor = System.Drawing.Color.Transparent;
            this.chkScSta.Checked = true;
            this.chkScSta.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScSta.Location = new System.Drawing.Point(296, 13);
            this.chkScSta.Name = "chkScSta";
            this.chkScSta.Size = new System.Drawing.Size(80, 15);
            this.chkScSta.TabIndex = 24;
            this.chkScSta.Text = "S/C상태";
            this.chkScSta.UseVisualStyleBackColor = false;
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.Color.Transparent;
            this.Label1.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Label1.Location = new System.Drawing.Point(52, 12);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(104, 16);
            this.Label1.TabIndex = 20;
            this.Label1.Text = "Client(Send)";
            // 
            // chkStatus
            // 
            this.chkStatus.BackColor = System.Drawing.Color.Transparent;
            this.chkStatus.Location = new System.Drawing.Point(203, 10);
            this.chkStatus.Name = "chkStatus";
            this.chkStatus.Size = new System.Drawing.Size(60, 24);
            this.chkStatus.TabIndex = 23;
            this.chkStatus.Text = "Status";
            this.chkStatus.UseVisualStyleBackColor = false;
            // 
            // picCliCom
            // 
            this.picCliCom.Image = ((System.Drawing.Image)(resources.GetObject("picCliCom.Image")));
            this.picCliCom.Location = new System.Drawing.Point(16, 4);
            this.picCliCom.Name = "picCliCom";
            this.picCliCom.Size = new System.Drawing.Size(32, 32);
            this.picCliCom.TabIndex = 19;
            this.picCliCom.TabStop = false;
            // 
            // lsvMsgClient
            // 
            this.lsvMsgClient.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvMsgClient.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2});
            this.lsvMsgClient.Font = new System.Drawing.Font("돋움체", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsvMsgClient.FullRowSelect = true;
            this.lsvMsgClient.Location = new System.Drawing.Point(8, 72);
            this.lsvMsgClient.Name = "lsvMsgClient";
            this.lsvMsgClient.Size = new System.Drawing.Size(368, 383);
            this.lsvMsgClient.TabIndex = 2;
            this.lsvMsgClient.UseCompatibleStateImageBehavior = false;
            this.lsvMsgClient.View = System.Windows.Forms.View.Details;
            this.lsvMsgClient.Click += new System.EventHandler(this.lsvMsgClient_Click);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "시간";
            this.ColumnHeader1.Width = 110;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "내용";
            this.ColumnHeader2.Width = 920;
            // 
            // txtDetail
            // 
            this.txtDetail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDetail.BackColor = System.Drawing.Color.Black;
            this.txtDetail.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtDetail.ForeColor = System.Drawing.Color.White;
            this.txtDetail.Location = new System.Drawing.Point(8, 461);
            this.txtDetail.Multiline = true;
            this.txtDetail.Name = "txtDetail";
            this.txtDetail.ReadOnly = true;
            this.txtDetail.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetail.Size = new System.Drawing.Size(368, 55);
            this.txtDetail.TabIndex = 1;
            // 
            // chkShowClient
            // 
            this.chkShowClient.BackColor = System.Drawing.Color.Transparent;
            this.chkShowClient.Checked = true;
            this.chkShowClient.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowClient.Location = new System.Drawing.Point(16, 48);
            this.chkShowClient.Name = "chkShowClient";
            this.chkShowClient.Size = new System.Drawing.Size(128, 24);
            this.chkShowClient.TabIndex = 16;
            this.chkShowClient.Text = "최신정보 먼저보기";
            this.chkShowClient.UseVisualStyleBackColor = false;
            // 
            // btnDelLogClient
            // 
            this.btnDelLogClient.Location = new System.Drawing.Point(240, 48);
            this.btnDelLogClient.Name = "btnDelLogClient";
            this.btnDelLogClient.Size = new System.Drawing.Size(72, 23);
            this.btnDelLogClient.TabIndex = 18;
            this.btnDelLogClient.Text = "로그 삭제";
            this.btnDelLogClient.Click += new System.EventHandler(this.btnDelLogClient_Click);
            // 
            // chkStopLogClient
            // 
            this.chkStopLogClient.BackColor = System.Drawing.Color.Transparent;
            this.chkStopLogClient.Location = new System.Drawing.Point(152, 48);
            this.chkStopLogClient.Name = "chkStopLogClient";
            this.chkStopLogClient.Size = new System.Drawing.Size(80, 24);
            this.chkStopLogClient.TabIndex = 17;
            this.chkStopLogClient.Text = "로그 중지";
            this.chkStopLogClient.UseVisualStyleBackColor = false;
            // 
            // pnlMsg2
            // 
            this.pnlMsg2.Controls.Add(this.Label4);
            this.pnlMsg2.Controls.Add(this.picSrvCom);
            this.pnlMsg2.Controls.Add(this.lsvMsgServer);
            this.pnlMsg2.Controls.Add(this.txtDetail2);
            this.pnlMsg2.Controls.Add(this.chkShowServer);
            this.pnlMsg2.Controls.Add(this.btnDelLogServer);
            this.pnlMsg2.Controls.Add(this.chkStopLogServer);
            this.pnlMsg2.Controls.Add(this.btnExit);
            this.pnlMsg2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMsg2.Location = new System.Drawing.Point(0, 0);
            this.pnlMsg2.Name = "pnlMsg2";
            this.pnlMsg2.Size = new System.Drawing.Size(396, 525);
            this.pnlMsg2.TabIndex = 92;
            // 
            // Label4
            // 
            this.Label4.BackColor = System.Drawing.Color.Transparent;
            this.Label4.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Label4.Location = new System.Drawing.Point(52, 12);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(112, 16);
            this.Label4.TabIndex = 20;
            this.Label4.Text = "Server(Recv)";
            // 
            // picSrvCom
            // 
            this.picSrvCom.BackColor = System.Drawing.Color.Transparent;
            this.picSrvCom.Image = ((System.Drawing.Image)(resources.GetObject("picSrvCom.Image")));
            this.picSrvCom.Location = new System.Drawing.Point(20, 4);
            this.picSrvCom.Name = "picSrvCom";
            this.picSrvCom.Size = new System.Drawing.Size(32, 32);
            this.picSrvCom.TabIndex = 19;
            this.picSrvCom.TabStop = false;
            // 
            // lsvMsgServer
            // 
            this.lsvMsgServer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvMsgServer.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader3,
            this.ColumnHeader4});
            this.lsvMsgServer.Font = new System.Drawing.Font("돋움체", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsvMsgServer.FullRowSelect = true;
            this.lsvMsgServer.Location = new System.Drawing.Point(8, 72);
            this.lsvMsgServer.Name = "lsvMsgServer";
            this.lsvMsgServer.Size = new System.Drawing.Size(380, 383);
            this.lsvMsgServer.SmallImageList = this.imlLog;
            this.lsvMsgServer.TabIndex = 14;
            this.lsvMsgServer.UseCompatibleStateImageBehavior = false;
            this.lsvMsgServer.View = System.Windows.Forms.View.Details;
            this.lsvMsgServer.Click += new System.EventHandler(this.lsvMsgServer_Click);
            // 
            // ColumnHeader3
            // 
            this.ColumnHeader3.Text = "시간";
            this.ColumnHeader3.Width = 110;
            // 
            // ColumnHeader4
            // 
            this.ColumnHeader4.Text = "내용";
            this.ColumnHeader4.Width = 920;
            // 
            // imlLog
            // 
            this.imlLog.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlLog.ImageStream")));
            this.imlLog.TransparentColor = System.Drawing.Color.Transparent;
            this.imlLog.Images.SetKeyName(0, "");
            // 
            // txtDetail2
            // 
            this.txtDetail2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDetail2.BackColor = System.Drawing.Color.Black;
            this.txtDetail2.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtDetail2.ForeColor = System.Drawing.Color.White;
            this.txtDetail2.Location = new System.Drawing.Point(8, 461);
            this.txtDetail2.Multiline = true;
            this.txtDetail2.Name = "txtDetail2";
            this.txtDetail2.ReadOnly = true;
            this.txtDetail2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetail2.Size = new System.Drawing.Size(380, 55);
            this.txtDetail2.TabIndex = 15;
            // 
            // chkShowServer
            // 
            this.chkShowServer.BackColor = System.Drawing.Color.Transparent;
            this.chkShowServer.Checked = true;
            this.chkShowServer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowServer.Location = new System.Drawing.Point(16, 48);
            this.chkShowServer.Name = "chkShowServer";
            this.chkShowServer.Size = new System.Drawing.Size(128, 24);
            this.chkShowServer.TabIndex = 16;
            this.chkShowServer.Text = "최신정보 먼저보기";
            this.chkShowServer.UseVisualStyleBackColor = false;
            // 
            // btnDelLogServer
            // 
            this.btnDelLogServer.Location = new System.Drawing.Point(240, 48);
            this.btnDelLogServer.Name = "btnDelLogServer";
            this.btnDelLogServer.Size = new System.Drawing.Size(72, 23);
            this.btnDelLogServer.TabIndex = 18;
            this.btnDelLogServer.Text = "로그 삭제";
            this.btnDelLogServer.Click += new System.EventHandler(this.btnDelLogServer_Click);
            // 
            // chkStopLogServer
            // 
            this.chkStopLogServer.BackColor = System.Drawing.Color.Transparent;
            this.chkStopLogServer.Location = new System.Drawing.Point(152, 48);
            this.chkStopLogServer.Name = "chkStopLogServer";
            this.chkStopLogServer.Size = new System.Drawing.Size(80, 24);
            this.chkStopLogServer.TabIndex = 17;
            this.chkStopLogServer.Text = "로그 중지";
            this.chkStopLogServer.UseVisualStyleBackColor = false;
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.ImageIndex = 3;
            this.btnExit.ImageList = this.imlConnect;
            this.btnExit.Location = new System.Drawing.Point(340, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(48, 44);
            this.btnExit.TabIndex = 7;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // imlConnect
            // 
            this.imlConnect.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlConnect.ImageStream")));
            this.imlConnect.TransparentColor = System.Drawing.Color.Transparent;
            this.imlConnect.Images.SetKeyName(0, "");
            this.imlConnect.Images.SetKeyName(1, "");
            this.imlConnect.Images.SetKeyName(2, "");
            this.imlConnect.Images.SetKeyName(3, "");
            // 
            // tmrMain
            // 
            this.tmrMain.Interval = 500;
            this.tmrMain.Tick += new System.EventHandler(this.tmrMain_Tick);
            // 
            // tmrSTOP_REQ
            // 
            this.tmrSTOP_REQ.Interval = 1000;
            this.tmrSTOP_REQ.Tick += new System.EventHandler(this.tmrSTOP_REQ_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.StatusBar1);
            this.Controls.Add(this.pnlTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "HOST 통신";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ver)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Msg)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlMsg.ResumeLayout(false);
            this.pnlMsg.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCliCom)).EndInit();
            this.pnlMsg2.ResumeLayout(false);
            this.pnlMsg2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSrvCom)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.Panel pnlTop;
		internal System.Windows.Forms.StatusBar StatusBar1;
		internal System.Windows.Forms.StatusBarPanel ver;
		internal System.Windows.Forms.StatusBarPanel Msg;
		private System.Windows.Forms.SplitContainer splitContainer1;
		internal System.Windows.Forms.Panel pnlMsg;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.PictureBox picCliCom;
		internal System.Windows.Forms.ListView lsvMsgClient;
		internal System.Windows.Forms.ColumnHeader ColumnHeader1;
		internal System.Windows.Forms.ColumnHeader ColumnHeader2;
		internal System.Windows.Forms.TextBox txtDetail;
		internal System.Windows.Forms.CheckBox chkShowClient;
		internal System.Windows.Forms.Button btnDelLogClient;
		internal System.Windows.Forms.CheckBox chkStopLogClient;
		internal System.Windows.Forms.Timer tmrMain;
		internal System.Windows.Forms.Timer tmrSTOP_REQ;
		internal System.Windows.Forms.ImageList imlConnect;
		internal System.Windows.Forms.Panel pnlMsg2;
		internal System.Windows.Forms.CheckBox chkScSta;
		internal System.Windows.Forms.CheckBox chkStatus;
		internal System.Windows.Forms.Label Label4;
		internal System.Windows.Forms.PictureBox picSrvCom;
		internal System.Windows.Forms.ListView lsvMsgServer;
		internal System.Windows.Forms.ColumnHeader ColumnHeader3;
		internal System.Windows.Forms.ColumnHeader ColumnHeader4;
		internal System.Windows.Forms.ImageList imlLog;
		internal System.Windows.Forms.TextBox txtDetail2;
		internal System.Windows.Forms.CheckBox chkShowServer;
		internal System.Windows.Forms.Button btnDelLogServer;
		internal System.Windows.Forms.CheckBox chkStopLogServer;
		internal System.Windows.Forms.Button btnExit;
        internal System.Windows.Forms.CheckBox chkSimMode;
	}
}

