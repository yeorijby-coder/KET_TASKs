namespace WCS_TASK_CV
{
    partial class FRM_XML_FIELD_SYNC
    {
        private System.ComponentModel.IContainer components = null;

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
            this.lblKeyWord = new System.Windows.Forms.Label();
            this.txtKeyWord = new System.Windows.Forms.TextBox();
            this.btnKeyWord = new System.Windows.Forms.Button();
            this.lblDeviceMap = new System.Windows.Forms.Label();
            this.txtDeviceMap = new System.Windows.Forms.TextBox();
            this.btnDeviceMap = new System.Windows.Forms.Button();
            this.btnParse = new System.Windows.Forms.Button();
            this.btnSync = new System.Windows.Forms.Button();
            this.lsvLog = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            //
            // lblKeyWord
            //
            this.lblKeyWord.AutoSize = true;
            this.lblKeyWord.Location = new System.Drawing.Point(12, 15);
            this.lblKeyWord.Name = "lblKeyWord";
            this.lblKeyWord.Size = new System.Drawing.Size(80, 12);
            this.lblKeyWord.Text = "KeyWord.xml";
            //
            // txtKeyWord
            //
            this.txtKeyWord.Location = new System.Drawing.Point(110, 12);
            this.txtKeyWord.Name = "txtKeyWord";
            this.txtKeyWord.Size = new System.Drawing.Size(480, 21);
            this.txtKeyWord.TabIndex = 0;
            //
            // btnKeyWord
            //
            this.btnKeyWord.Location = new System.Drawing.Point(596, 11);
            this.btnKeyWord.Name = "btnKeyWord";
            this.btnKeyWord.Size = new System.Drawing.Size(40, 23);
            this.btnKeyWord.TabIndex = 1;
            this.btnKeyWord.Text = "...";
            this.btnKeyWord.UseVisualStyleBackColor = true;
            this.btnKeyWord.Click += new System.EventHandler(this.btnKeyWord_Click);
            //
            // lblDeviceMap
            //
            this.lblDeviceMap.AutoSize = true;
            this.lblDeviceMap.Location = new System.Drawing.Point(12, 45);
            this.lblDeviceMap.Name = "lblDeviceMap";
            this.lblDeviceMap.Size = new System.Drawing.Size(90, 12);
            this.lblDeviceMap.Text = "DeviceMap.xml";
            //
            // txtDeviceMap
            //
            this.txtDeviceMap.Location = new System.Drawing.Point(110, 42);
            this.txtDeviceMap.Name = "txtDeviceMap";
            this.txtDeviceMap.Size = new System.Drawing.Size(480, 21);
            this.txtDeviceMap.TabIndex = 2;
            //
            // btnDeviceMap
            //
            this.btnDeviceMap.Location = new System.Drawing.Point(596, 41);
            this.btnDeviceMap.Name = "btnDeviceMap";
            this.btnDeviceMap.Size = new System.Drawing.Size(40, 23);
            this.btnDeviceMap.TabIndex = 3;
            this.btnDeviceMap.Text = "...";
            this.btnDeviceMap.UseVisualStyleBackColor = true;
            this.btnDeviceMap.Click += new System.EventHandler(this.btnDeviceMap_Click);
            //
            // btnParse
            //
            this.btnParse.Location = new System.Drawing.Point(110, 72);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(120, 28);
            this.btnParse.TabIndex = 4;
            this.btnParse.Text = "XML 파싱";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            //
            // btnSync
            //
            this.btnSync.Enabled = false;
            this.btnSync.Location = new System.Drawing.Point(240, 72);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(160, 28);
            this.btnSync.TabIndex = 5;
            this.btnSync.Text = "CV_DATA 필드 동기화";
            this.btnSync.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnSync.ForeColor = System.Drawing.Color.White;
            this.btnSync.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSync.UseVisualStyleBackColor = false;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            //
            // lsvLog
            //
            this.lsvLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvLog.FormattingEnabled = true;
            this.lsvLog.HorizontalScrollbar = true;
            this.lsvLog.ItemHeight = 12;
            this.lsvLog.Location = new System.Drawing.Point(12, 110);
            this.lsvLog.Name = "lsvLog";
            this.lsvLog.Size = new System.Drawing.Size(624, 340);
            this.lsvLog.TabIndex = 6;
            //
            // FRM_XML_FIELD_SYNC
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 466);
            this.Controls.Add(this.lblKeyWord);
            this.Controls.Add(this.txtKeyWord);
            this.Controls.Add(this.btnKeyWord);
            this.Controls.Add(this.lblDeviceMap);
            this.Controls.Add(this.txtDeviceMap);
            this.Controls.Add(this.btnDeviceMap);
            this.Controls.Add(this.btnParse);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.lsvLog);
            this.Name = "FRM_XML_FIELD_SYNC";
            this.Text = "XML 필드 동기화 (KeyWord / DeviceMap → CV_DATA)";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblKeyWord;
        private System.Windows.Forms.TextBox txtKeyWord;
        private System.Windows.Forms.Button btnKeyWord;
        private System.Windows.Forms.Label lblDeviceMap;
        private System.Windows.Forms.TextBox txtDeviceMap;
        private System.Windows.Forms.Button btnDeviceMap;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.ListBox lsvLog;
    }
}
