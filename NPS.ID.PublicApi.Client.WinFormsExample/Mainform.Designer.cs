namespace NPS.ID.PublicApi.Client.WinFormsExample
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonConnect = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonSendOrderEntry = new System.Windows.Forms.Button();
            this.buttonSendOrderModification = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonLogout = new System.Windows.Forms.Button();
            this.buttonSendTradeRecall = new System.Windows.Forms.Button();
            this.buttonTradeHistory = new System.Windows.Forms.Button();
            this.buttonOrderHistory = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(13, 13);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(12, 100);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(426, 380);
            this.textBoxLog.TabIndex = 2;
            // 
            // buttonSendOrderEntry
            // 
            this.buttonSendOrderEntry.Enabled = false;
            this.buttonSendOrderEntry.Location = new System.Drawing.Point(13, 42);
            this.buttonSendOrderEntry.Name = "buttonSendOrderEntry";
            this.buttonSendOrderEntry.Size = new System.Drawing.Size(137, 23);
            this.buttonSendOrderEntry.TabIndex = 4;
            this.buttonSendOrderEntry.Text = "Send Order Creation";
            this.buttonSendOrderEntry.UseVisualStyleBackColor = true;
            this.buttonSendOrderEntry.Click += new System.EventHandler(this.buttonSendOrderEntry_Click);
            // 
            // buttonSendOrderModification
            // 
            this.buttonSendOrderModification.Enabled = false;
            this.buttonSendOrderModification.Location = new System.Drawing.Point(156, 42);
            this.buttonSendOrderModification.Name = "buttonSendOrderModification";
            this.buttonSendOrderModification.Size = new System.Drawing.Size(137, 23);
            this.buttonSendOrderModification.TabIndex = 6;
            this.buttonSendOrderModification.Text = "Send Order Modification";
            this.buttonSendOrderModification.UseVisualStyleBackColor = true;
            this.buttonSendOrderModification.Click += new System.EventHandler(this.buttonSendOrderModification_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(363, 486);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 23);
            this.buttonClear.TabIndex = 7;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonLogout
            // 
            this.buttonLogout.Enabled = false;
            this.buttonLogout.Location = new System.Drawing.Point(94, 13);
            this.buttonLogout.Name = "buttonLogout";
            this.buttonLogout.Size = new System.Drawing.Size(132, 23);
            this.buttonLogout.TabIndex = 8;
            this.buttonLogout.Text = "Logout";
            this.buttonLogout.UseVisualStyleBackColor = true;
            this.buttonLogout.Click += new System.EventHandler(this.buttonLogout_Click);
            // 
            // buttonSendTradeRecall
            // 
            this.buttonSendTradeRecall.Enabled = false;
            this.buttonSendTradeRecall.Location = new System.Drawing.Point(299, 42);
            this.buttonSendTradeRecall.Name = "buttonSendTradeRecall";
            this.buttonSendTradeRecall.Size = new System.Drawing.Size(137, 23);
            this.buttonSendTradeRecall.TabIndex = 9;
            this.buttonSendTradeRecall.Text = "Send Trade Recall";
            this.buttonSendTradeRecall.UseVisualStyleBackColor = true;
            this.buttonSendTradeRecall.Click += new System.EventHandler(this.buttonSendTradeRecall_Click);
            // 
            // buttonTradeHistory
            // 
            this.buttonTradeHistory.Enabled = false;
            this.buttonTradeHistory.Location = new System.Drawing.Point(13, 71);
            this.buttonTradeHistory.Name = "buttonTradeHistory";
            this.buttonTradeHistory.Size = new System.Drawing.Size(137, 23);
            this.buttonTradeHistory.TabIndex = 10;
            this.buttonTradeHistory.Text = "REST: Trade history";
            this.buttonTradeHistory.UseVisualStyleBackColor = true;
            this.buttonTradeHistory.Click += new System.EventHandler(this.buttonTradeHistory_Click);
            // 
            // buttonOrderHistory
            // 
            this.buttonOrderHistory.Enabled = false;
            this.buttonOrderHistory.Location = new System.Drawing.Point(156, 71);
            this.buttonOrderHistory.Name = "buttonOrderHistory";
            this.buttonOrderHistory.Size = new System.Drawing.Size(137, 23);
            this.buttonOrderHistory.TabIndex = 11;
            this.buttonOrderHistory.Text = "REST: Order history";
            this.buttonOrderHistory.UseVisualStyleBackColor = true;
            this.buttonOrderHistory.Click += new System.EventHandler(this.buttonOrderHistory_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 518);
            this.Controls.Add(this.buttonOrderHistory);
            this.Controls.Add(this.buttonTradeHistory);
            this.Controls.Add(this.buttonSendTradeRecall);
            this.Controls.Add(this.buttonLogout);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonSendOrderModification);
            this.Controls.Add(this.buttonSendOrderEntry);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.buttonConnect);
            this.Name = "MainForm";
            this.Text = "Intraday 2.0 Api Example app";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button buttonSendOrderEntry;
        private System.Windows.Forms.Button buttonSendOrderModification;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonLogout;
        private System.Windows.Forms.Button buttonSendTradeRecall;
        private System.Windows.Forms.Button buttonTradeHistory;
        private System.Windows.Forms.Button buttonOrderHistory;
    }
}

