namespace WCS_TASK_Display
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
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblTopTitle = new System.Windows.Forms.Label();
            this.pnlManual = new System.Windows.Forms.Panel();
            this.lblCtrl = new System.Windows.Forms.Label();
            this.cmbController = new System.Windows.Forms.ComboBox();
            this.lblDisp = new System.Windows.Forms.Label();
            this.cmbDispNo = new System.Windows.Forms.ComboBox();
            this.lblColor = new System.Windows.Forms.Label();
            this.cmbColor = new System.Windows.Forms.ComboBox();
            this.lblData = new System.Windows.Forms.Label();
            this.txtData = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.chkHex = new System.Windows.Forms.CheckBox();
            this.chkAscii = new System.Windows.Forms.CheckBox();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.chkLogStop = new System.Windows.Forms.CheckBox();
            this.btnLogClear = new System.Windows.Forms.Button();
            this.chkLatestFirst = new System.Windows.Forms.CheckBox();
            this.chkAutoReconn = new System.Windows.Forms.CheckBox();
            this.lvMsg = new System.Windows.Forms.ListView();
            this.colTime = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colId = new System.Windows.Forms.ColumnHeader();
            this.colFile = new System.Windows.Forms.ColumnHeader();
            this.colFunc = new System.Windows.Forms.ColumnHeader();
            this.colMsg = new System.Windows.Forms.ColumnHeader();
            this.Thread_Timer = new System.Windows.Forms.Timer(this.components);
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.pnlManual.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTop
            //
            this.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(764, 90);
            this.pnlTop.TabIndex = 0;
            //
            // lblTopTitle
            //
            this.lblTopTitle.AutoSize = true;
            this.lblTopTitle.Location = new System.Drawing.Point(6, 4);
            this.lblTopTitle.Name = "lblTopTitle";
            this.lblTopTitle.Size = new System.Drawing.Size(120, 12);
            this.lblTopTitle.TabIndex = 0;
            this.lblTopTitle.Text = "DISPLAY CONTROLLERS";
            //
            // pnlManual
            //
            this.pnlManual.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlManual.Controls.Add(this.lblCtrl);
            this.pnlManual.Controls.Add(this.cmbController);
            this.pnlManual.Controls.Add(this.lblDisp);
            this.pnlManual.Controls.Add(this.cmbDispNo);
            this.pnlManual.Controls.Add(this.lblColor);
            this.pnlManual.Controls.Add(this.cmbColor);
            this.pnlManual.Controls.Add(this.lblData);
            this.pnlManual.Controls.Add(this.txtData);
            this.pnlManual.Controls.Add(this.btnSend);
            this.pnlManual.Controls.Add(this.btnClear);
            this.pnlManual.Controls.Add(this.chkHex);
            this.pnlManual.Controls.Add(this.chkAscii);
            this.pnlManual.Controls.Add(this.btnDisconnect);
            this.pnlManual.Controls.Add(this.chkLogStop);
            this.pnlManual.Controls.Add(this.btnLogClear);
            this.pnlManual.Controls.Add(this.chkLatestFirst);
            this.pnlManual.Controls.Add(this.chkAutoReconn);
            this.pnlManual.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlManual.Location = new System.Drawing.Point(0, 425);
            this.pnlManual.Name = "pnlManual";
            this.pnlManual.Size = new System.Drawing.Size(764, 96);
            this.pnlManual.TabIndex = 2;
            //
            // lblCtrl
            //
            this.lblCtrl.AutoSize = true;
            this.lblCtrl.Location = new System.Drawing.Point(8, 12);
            this.lblCtrl.Name = "lblCtrl";
            this.lblCtrl.Size = new System.Drawing.Size(63, 12);
            this.lblCtrl.TabIndex = 0;
            this.lblCtrl.Text = "Controller";
            //
            // cmbController
            //
            this.cmbController.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbController.Location = new System.Drawing.Point(75, 8);
            this.cmbController.Name = "cmbController";
            this.cmbController.Size = new System.Drawing.Size(70, 20);
            this.cmbController.TabIndex = 1;
            //
            // lblDisp
            //
            this.lblDisp.AutoSize = true;
            this.lblDisp.Location = new System.Drawing.Point(155, 12);
            this.lblDisp.Name = "lblDisp";
            this.lblDisp.Size = new System.Drawing.Size(45, 12);
            this.lblDisp.TabIndex = 2;
            this.lblDisp.Text = "DISP No";
            //
            // cmbDispNo
            //
            this.cmbDispNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDispNo.Location = new System.Drawing.Point(205, 8);
            this.cmbDispNo.Name = "cmbDispNo";
            this.cmbDispNo.Size = new System.Drawing.Size(55, 20);
            this.cmbDispNo.TabIndex = 3;
            //
            // lblColor
            //
            this.lblColor.AutoSize = true;
            this.lblColor.Location = new System.Drawing.Point(270, 12);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(35, 12);
            this.lblColor.TabIndex = 4;
            this.lblColor.Text = "Color";
            //
            // cmbColor
            //
            this.cmbColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColor.Location = new System.Drawing.Point(310, 8);
            this.cmbColor.Name = "cmbColor";
            this.cmbColor.Size = new System.Drawing.Size(80, 20);
            this.cmbColor.TabIndex = 5;
            //
            // lblData
            //
            this.lblData.AutoSize = true;
            this.lblData.Location = new System.Drawing.Point(8, 44);
            this.lblData.Name = "lblData";
            this.lblData.Size = new System.Drawing.Size(85, 12);
            this.lblData.TabIndex = 6;
            this.lblData.Text = "Data (max 8)";
            //
            // txtData
            //
            this.txtData.Location = new System.Drawing.Point(98, 40);
            this.txtData.MaxLength = 8;
            this.txtData.Name = "txtData";
            this.txtData.Size = new System.Drawing.Size(162, 21);
            this.txtData.TabIndex = 7;
            //
            // btnSend
            //
            this.btnSend.Location = new System.Drawing.Point(270, 39);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(58, 23);
            this.btnSend.TabIndex = 8;
            this.btnSend.Text = "DATA";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            //
            // btnClear
            //
            this.btnClear.Location = new System.Drawing.Point(332, 39);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(58, 23);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "CLEAR";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            //
            // chkHex
            //
            this.chkHex.AutoSize = true;
            this.chkHex.Checked = true;
            this.chkHex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHex.Location = new System.Drawing.Point(410, 10);
            this.chkHex.Name = "chkHex";
            this.chkHex.Size = new System.Drawing.Size(48, 16);
            this.chkHex.TabIndex = 10;
            this.chkHex.Text = "HEX";
            this.chkHex.UseVisualStyleBackColor = true;
            this.chkHex.CheckedChanged += new System.EventHandler(this.chkHex_CheckedChanged);
            //
            // chkAscii
            //
            this.chkAscii.AutoSize = true;
            this.chkAscii.Location = new System.Drawing.Point(410, 40);
            this.chkAscii.Name = "chkAscii";
            this.chkAscii.Size = new System.Drawing.Size(55, 16);
            this.chkAscii.TabIndex = 11;
            this.chkAscii.Text = "ASCII";
            this.chkAscii.UseVisualStyleBackColor = true;
            this.chkAscii.CheckedChanged += new System.EventHandler(this.chkAscii_CheckedChanged);
            //
            // btnDisconnect
            //
            this.btnDisconnect.Location = new System.Drawing.Point(8, 66);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(90, 24);
            this.btnDisconnect.TabIndex = 12;
            this.btnDisconnect.Text = "접속 끊기";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            //
            // chkAutoReconn
            //
            this.chkAutoReconn.AutoSize = true;
            this.chkAutoReconn.Checked = true;
            this.chkAutoReconn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoReconn.Location = new System.Drawing.Point(104, 71);
            this.chkAutoReconn.Name = "chkAutoReconn";
            this.chkAutoReconn.Size = new System.Drawing.Size(83, 16);
            this.chkAutoReconn.TabIndex = 16;
            this.chkAutoReconn.Text = "자동 재접속";
            this.chkAutoReconn.UseVisualStyleBackColor = true;
            this.chkAutoReconn.CheckedChanged += new System.EventHandler(this.chkAutoReconn_CheckedChanged);
            //
            // chkLogStop
            //
            this.chkLogStop.AutoSize = true;
            this.chkLogStop.Location = new System.Drawing.Point(205, 71);
            this.chkLogStop.Name = "chkLogStop";
            this.chkLogStop.Size = new System.Drawing.Size(71, 16);
            this.chkLogStop.TabIndex = 13;
            this.chkLogStop.Text = "로그 정지";
            this.chkLogStop.UseVisualStyleBackColor = true;
            //
            // btnLogClear
            //
            this.btnLogClear.Location = new System.Drawing.Point(288, 66);
            this.btnLogClear.Name = "btnLogClear";
            this.btnLogClear.Size = new System.Drawing.Size(90, 24);
            this.btnLogClear.TabIndex = 14;
            this.btnLogClear.Text = "로그 지우기";
            this.btnLogClear.UseVisualStyleBackColor = true;
            this.btnLogClear.Click += new System.EventHandler(this.btnLogClear_Click);
            //
            // chkLatestFirst
            //
            this.chkLatestFirst.AutoSize = true;
            this.chkLatestFirst.Location = new System.Drawing.Point(388, 71);
            this.chkLatestFirst.Name = "chkLatestFirst";
            this.chkLatestFirst.Size = new System.Drawing.Size(186, 16);
            this.chkLatestFirst.TabIndex = 15;
            this.chkLatestFirst.Text = "See the latest information first";
            this.chkLatestFirst.UseVisualStyleBackColor = true;
            this.chkLatestFirst.CheckedChanged += new System.EventHandler(this.chkLatestFirst_CheckedChanged);
            //
            // lvMsg
            //
            this.lvMsg.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTime,
            this.colType,
            this.colId,
            this.colFile,
            this.colFunc,
            this.colMsg});
            this.lvMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMsg.FullRowSelect = true;
            this.lvMsg.GridLines = true;
            this.lvMsg.Location = new System.Drawing.Point(0, 90);
            this.lvMsg.Name = "lvMsg";
            this.lvMsg.Size = new System.Drawing.Size(764, 361);
            this.lvMsg.TabIndex = 1;
            this.lvMsg.UseCompatibleStateImageBehavior = false;
            this.lvMsg.View = System.Windows.Forms.View.Details;
            //
            // colTime
            //
            this.colTime.Text = "Time";
            this.colTime.Width = 150;
            //
            // colType
            //
            this.colType.Text = "Type";
            this.colType.Width = 50;
            //
            // colId
            //
            this.colId.Text = "PLC";
            this.colId.Width = 60;
            //
            // colMsg
            //
            this.colFile.Text = "FILE";
            this.colFile.Width = 150;
            this.colFunc.Text = "FUNCTION";
            this.colFunc.Width = 200;
            this.colMsg.Text = "Message";
            this.colMsg.Width = 480;
            //
            // Thread_Timer
            //
            this.Thread_Timer.Interval = 1000;
            this.Thread_Timer.Tick += new System.EventHandler(this.Thread_Tick);
            //
            // SYS_MAIN
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 521);
            this.Controls.Add(this.lvMsg);
            this.Controls.Add(this.pnlManual);
            this.Controls.Add(this.pnlTop);
            this.Name = "SYS_MAIN";
            this.Text = "WCS_TASK_Display";
            this.Load += new System.EventHandler(this.SYS_MAIN_Load);
            this.pnlManual.ResumeLayout(false);
            this.pnlManual.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblTopTitle;
        private System.Windows.Forms.Panel pnlManual;
        private System.Windows.Forms.Label lblCtrl;
        private System.Windows.Forms.ComboBox cmbController;
        private System.Windows.Forms.Label lblDisp;
        private System.Windows.Forms.ComboBox cmbDispNo;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.ComboBox cmbColor;
        private System.Windows.Forms.Label lblData;
        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.CheckBox chkHex;
        private System.Windows.Forms.CheckBox chkAscii;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.CheckBox chkLogStop;
        private System.Windows.Forms.Button btnLogClear;
        private System.Windows.Forms.CheckBox chkLatestFirst;
        private System.Windows.Forms.CheckBox chkAutoReconn;
        private System.Windows.Forms.ListView lvMsg;
        private System.Windows.Forms.ColumnHeader colTime;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colId;
        private System.Windows.Forms.ColumnHeader colFile;
        private System.Windows.Forms.ColumnHeader colFunc;
        private System.Windows.Forms.ColumnHeader colMsg;
        private System.Windows.Forms.Timer Thread_Timer;
        private System.Windows.Forms.ToolTip ToolTip;
    }
}
