namespace КР_Мониторінг_ста
{
    partial class Main
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
            this.btnMon = new System.Windows.Forms.Button();
            this.btnSet = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnMon
            // 
            this.btnMon.Location = new System.Drawing.Point(408, 208);
            this.btnMon.Name = "btnMon";
            this.btnMon.Size = new System.Drawing.Size(217, 85);
            this.btnMon.TabIndex = 0;
            this.btnMon.Text = "Sistem Monitoring";
            this.btnMon.UseVisualStyleBackColor = true;
            this.btnMon.Click += new System.EventHandler(this.btnMon_Click);
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(408, 324);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(217, 96);
            this.btnSet.TabIndex = 1;
            this.btnSet.Text = "Settings";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(408, 455);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(217, 96);
            this.button3.TabIndex = 2;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 654);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.btnMon);
            this.Name = "Main";
            this.Text = "Main";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMon;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.Button button3;
    }
}