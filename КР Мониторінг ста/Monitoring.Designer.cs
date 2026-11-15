namespace КР_Мониторінг_ста
{
    partial class Monitoring
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.btnCpu = new System.Windows.Forms.Button();
            this.btnGpu = new System.Windows.Forms.Button();
            this.btnRam = new System.Windows.Forms.Button();
            this.btnDisk = new System.Windows.Forms.Button();
            this.btnWifi = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDeviceName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCpu
            // 
            this.btnCpu.Location = new System.Drawing.Point(12, 44);
            this.btnCpu.Name = "btnCpu";
            this.btnCpu.Size = new System.Drawing.Size(208, 85);
            this.btnCpu.TabIndex = 0;
            this.btnCpu.Text = "CPU";
            this.btnCpu.UseVisualStyleBackColor = true;
            this.btnCpu.Click += new System.EventHandler(this.btnCpu_Click);
            // 
            // btnGpu
            // 
            this.btnGpu.Location = new System.Drawing.Point(12, 135);
            this.btnGpu.Name = "btnGpu";
            this.btnGpu.Size = new System.Drawing.Size(208, 85);
            this.btnGpu.TabIndex = 1;
            this.btnGpu.Text = "GPU";
            this.btnGpu.UseVisualStyleBackColor = true;
            this.btnGpu.Click += new System.EventHandler(this.btnGpu_Click);
            // 
            // btnRam
            // 
            this.btnRam.Location = new System.Drawing.Point(12, 226);
            this.btnRam.Name = "btnRam";
            this.btnRam.Size = new System.Drawing.Size(208, 85);
            this.btnRam.TabIndex = 2;
            this.btnRam.Text = "RAM";
            this.btnRam.UseVisualStyleBackColor = true;
            this.btnRam.Click += new System.EventHandler(this.btnRam_Click);
            // 
            // btnDisk
            // 
            this.btnDisk.Location = new System.Drawing.Point(12, 317);
            this.btnDisk.Name = "btnDisk";
            this.btnDisk.Size = new System.Drawing.Size(208, 85);
            this.btnDisk.TabIndex = 3;
            this.btnDisk.Text = "DISK";
            this.btnDisk.UseVisualStyleBackColor = true;
            this.btnDisk.Click += new System.EventHandler(this.btnDisk_Click);
            // 
            // btnWifi
            // 
            this.btnWifi.Location = new System.Drawing.Point(12, 408);
            this.btnWifi.Name = "btnWifi";
            this.btnWifi.Size = new System.Drawing.Size(208, 85);
            this.btnWifi.TabIndex = 4;
            this.btnWifi.Text = "WI-FI";
            this.btnWifi.UseVisualStyleBackColor = true;
            this.btnWifi.Click += new System.EventHandler(this.btnWifi_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Location = new System.Drawing.Point(233, 522);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1182, 182);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // chart1
            // 
            chartArea3.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            this.chart1.Legends.Add(legend3);
            this.chart1.Location = new System.Drawing.Point(254, 59);
            this.chart1.Name = "chart1";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chart1.Series.Add(series3);
            this.chart1.Size = new System.Drawing.Size(1139, 418);
            this.chart1.TabIndex = 6;
            this.chart1.Text = "chart1";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(270, 40);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(37, 13);
            this.lblTitle.TabIndex = 7;
            this.lblTitle.Text = "lblTitle";
            // 
            // lblDeviceName
            // 
            this.lblDeviceName.AutoSize = true;
            this.lblDeviceName.Location = new System.Drawing.Point(254, 503);
            this.lblDeviceName.Name = "lblDeviceName";
            this.lblDeviceName.Size = new System.Drawing.Size(79, 13);
            this.lblDeviceName.TabIndex = 8;
            this.lblDeviceName.Text = "lblDeviceName";
            // 
            // Monitoring
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1427, 766);
            this.Controls.Add(this.lblDeviceName);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.btnWifi);
            this.Controls.Add(this.btnDisk);
            this.Controls.Add(this.btnRam);
            this.Controls.Add(this.btnGpu);
            this.Controls.Add(this.btnCpu);
            this.Name = "Monitoring";
            this.Text = "Monitoring";
            this.Load += new System.EventHandler(this.Monitoring_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCpu;
        private System.Windows.Forms.Button btnGpu;
        private System.Windows.Forms.Button btnRam;
        private System.Windows.Forms.Button btnDisk;
        private System.Windows.Forms.Button btnWifi;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDeviceName;
    }
}