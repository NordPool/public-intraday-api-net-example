namespace NPS.ID.PublicApi.Client.WinFormsExample
{
    partial class Form1
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
            this.btnConnect = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonGenerateSchemas = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.buttonGenerateCS = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(13, 13);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(12, 42);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(1059, 366);
            this.textBoxLog.TabIndex = 2;
            // 
            // buttonGenerateSchemas
            // 
            this.buttonGenerateSchemas.Location = new System.Drawing.Point(841, 13);
            this.buttonGenerateSchemas.Name = "buttonGenerateSchemas";
            this.buttonGenerateSchemas.Size = new System.Drawing.Size(112, 23);
            this.buttonGenerateSchemas.TabIndex = 3;
            this.buttonGenerateSchemas.Text = "Generate schemas";
            this.buttonGenerateSchemas.UseVisualStyleBackColor = true;
            this.buttonGenerateSchemas.Click += new System.EventHandler(this.buttonGenerateSchemas_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(94, 13);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(114, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "SendIncorrectOrderRequest";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // buttonGenerateCS
            // 
            this.buttonGenerateCS.Location = new System.Drawing.Point(959, 13);
            this.buttonGenerateCS.Name = "buttonGenerateCS";
            this.buttonGenerateCS.Size = new System.Drawing.Size(112, 23);
            this.buttonGenerateCS.TabIndex = 5;
            this.buttonGenerateCS.Text = "Generate C#";
            this.buttonGenerateCS.UseVisualStyleBackColor = true;
            this.buttonGenerateCS.Click += new System.EventHandler(this.buttonGenerateCS_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 420);
            this.Controls.Add(this.buttonGenerateCS);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.buttonGenerateSchemas);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.btnConnect);
            this.Name = "Form1";
            this.Text = "Intraday 2.0 Api Sample app [NOTSUPPORTED]";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button buttonGenerateSchemas;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button buttonGenerateCS;
    }
}

