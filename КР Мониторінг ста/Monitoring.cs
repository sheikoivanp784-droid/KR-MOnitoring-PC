using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Text;
using System.Reflection;

namespace КР_Мониторінг_ста
{
    public partial class Monitoring : Form
    {
        private CpuMon cpuMon;
        private GpuMon gpuMon;
        private RamMon ramMon;
        private DiskMon diskMon;
        private WifiMon wifiMon;

        private string currentMode = "CPU";
        private Dictionary<string, Label> statLabels = new Dictionary<string, Label>();

        public Monitoring()
        {
            InitializeComponent();

            btnCpu.Click += btnCpu_Click;
            btnGpu.Click += btnGpu_Click;

            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }

        private void Monitoring_Load(object sender, EventArgs e)
        {
            cpuMon = new CpuMon();
            gpuMon = new GpuMon();
            ramMon = new RamMon();
            diskMon = new DiskMon();
            wifiMon = new WifiMon();

            SetupChart();
            SwitchToCpu();
        }

        private void SetupChart()
        {
            chart1.Series.Clear();
            Series series = new Series("Load");
            series.ChartType = SeriesChartType.Area;
            series.BorderWidth = 2;

            for (int i = 0; i < 60; i++)
                series.Points.AddY(0);

            chart1.Series.Add(series);

            var area = chart1.ChartAreas[0];
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 100;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisX.LabelStyle.Enabled = false;
            chart1.Legends.Clear();
        }

        private void ResetChart(Color lineColor)
        {
            var series = chart1.Series["Load"];
            series.BorderColor = lineColor;
            series.Color = Color.FromArgb(65, lineColor);

            series.Points.Clear();
            for (int i = 0; i < 60; i++)
                series.Points.AddY(0);
        }

        private void AddStatCard(string key, string title, string initialValue, int width = 165)
        {
            Panel card = new Panel();
            card.Size = new Size(width, 54);
            card.Margin = new Padding(4);
            card.BackColor = Color.FromArgb(245, 247, 250);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblTitle.ForeColor = Color.DimGray;
            lblTitle.Location = new Point(8, 6);
            lblTitle.AutoSize = true;

            Label lblValue = new Label();
            lblValue.Text = initialValue;
            lblValue.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            lblValue.ForeColor = Color.FromArgb(30, 30, 30);
            lblValue.Location = new Point(8, 24);
            lblValue.AutoSize = true;

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            flowLayoutPanel1.Controls.Add(card);

            statLabels[key] = lblValue;
        }

        private void SwitchToCpu()
        {
            currentMode = "CPU";
            lblTitle.Text = "Центральний процесор";
            lblDeviceName.Text = cpuMon.Model;
            ResetChart(Color.FromArgb(17, 125, 187));

            flowLayoutPanel1.Controls.Clear();
            statLabels.Clear();

            AddStatCard("Usage", "Завантаження ЦП", $"{Math.Round(cpuMon.Usage)}%");
            AddStatCard("Temp", "Температура ЦП", cpuMon.Temperature);
            AddStatCard("Speed", "Поточна частота", cpuMon.Speed);
            AddStatCard("BaseSpeed", "Базова частота", cpuMon.BaseSpeed);
            AddStatCard("UpTime", "Час роботи", cpuMon.UpTime);

            AddStatCard("Processes", "Активні процеси", cpuMon.Processes.ToString());
            AddStatCard("Threads", "Потоки", cpuMon.Threads.ToString());
            AddStatCard("Handles", "Дескриптори", cpuMon.Handles.ToString());
            AddStatCard("Virtualization", "Віртуалізація", cpuMon.Virtualization);

            AddStatCard("Cores", "Фізичні ядра", cpuMon.Cores.ToString());
            AddStatCard("Logical", "Логічні ядра", cpuMon.LogicalProcessors.ToString());
            AddStatCard("Sockets", "Сокети", cpuMon.Sockets.ToString());
            AddStatCard("CacheL1", "Кеш-пам'ять L1", cpuMon.CacheL1);
            AddStatCard("CacheL2", "Кеш-пам'ять L2", cpuMon.CacheL2);
            AddStatCard("CacheL3", "Кеш-пам'ять L3", cpuMon.CacheL3);
        }

        private void SwitchToGpu()
        {
            currentMode = "GPU";
            lblTitle.Text = "Графічний процесор";
            lblDeviceName.Text = gpuMon.Model;
            ResetChart(Color.FromArgb(155, 77, 202));

            flowLayoutPanel1.Controls.Clear();
            statLabels.Clear();

            AddStatCard("Usage", "Завантаження ГП", $"{Math.Round(gpuMon.Usage)}%");
            AddStatCard("Temp", "Температура ядра", gpuMon.Temperature);
            AddStatCard("Engine3D", "3D-рендеринг", gpuMon.Engine3D);
            AddStatCard("EngineCopy", "Copy (копіювання)", gpuMon.EngineCopy);
            AddStatCard("VideoEncode", "Video Encode", gpuMon.VideoEncode);
            AddStatCard("VideoDecode", "Video Decode", gpuMon.VideoDecode);

            AddStatCard("DedicatedMem", "Виділена відеопам'ять", gpuMon.DedicatedMemory, 185);
            AddStatCard("TotalMem", "Оперативна пам'ять ГП", gpuMon.TotalMemory, 185);
            AddStatCard("SharedMem", "Спільна пам'ять ГП", gpuMon.SharedMemory, 185);
            AddStatCard("DirectX", "Версія DirectX", gpuMon.DirectX);
            AddStatCard("DriverVer", "Версія драйвера", gpuMon.DriverVersion);
            AddStatCard("DriverDate", "Дата драйвера", gpuMon.DriverDate);
            AddStatCard("Location", "Шина підключення", gpuMon.Location, 185);
        }

        private void SwitchToRam()
        {
            currentMode = "RAM";
            lblTitle.Text = "Оперативна пам'ять";
            lblDeviceName.Text = ramMon.Model;
            ResetChart(Color.FromArgb(139, 92, 246));

            flowLayoutPanel1.Controls.Clear();
            statLabels.Clear();

            AddStatCard("InUse", "Використовується", ramMon.InUse, 185);
            AddStatCard("Available", "Доступно", ramMon.Available);
            AddStatCard("Committed", "Виділено", ramMon.Committed, 185);
            AddStatCard("Cached", "Кешовано", ramMon.Cached);

            AddStatCard("PagedPool", "Вивантажуваний пул", ramMon.PagedPool);
            AddStatCard("NonPagedPool", "Невивантажуваний пул", ramMon.NonPagedPool, 185);
            AddStatCard("Speed", "Швидкість", ramMon.Speed);
            AddStatCard("Slots", "Використано гнізд", ramMon.SlotsUsed);

            AddStatCard("FormFactor", "Форм-фактор", ramMon.FormFactor);
            AddStatCard("Reserved", "Зарезервовано апаратно", ramMon.HardwareReserved, 185);
        }

        private void SwitchToDisk()
        {
            currentMode = "DISK";
            lblTitle.Text = "Диск 0 (C:)";
            lblDeviceName.Text = diskMon.Model;
            ResetChart(Color.FromArgb(16, 185, 129));

            flowLayoutPanel1.Controls.Clear();
            statLabels.Clear();

            AddStatCard("ActiveTime", "Активний час", $"{Math.Round(diskMon.ActiveTime)}%");
            AddStatCard("ResponseTime", "Середній час відповіді", diskMon.ResponseTime, 185);
            AddStatCard("ReadSpeed", "Швидкість читання", diskMon.ReadSpeed);
            AddStatCard("WriteSpeed", "Швидкість запису", diskMon.WriteSpeed);

            AddStatCard("Capacity", "Ємність", diskMon.Capacity);
            AddStatCard("Formatted", "Форматовано", diskMon.Formatted);
            AddStatCard("FreeSpace", "Вільне місце", diskMon.FreeSpace);
            AddStatCard("DriveType", "Тип носія", diskMon.DriveType);

            AddStatCard("SystemDisk", "Системний диск", diskMon.SystemDisk);
            AddStatCard("PageFile", "Файл підкачки", diskMon.PageFile);
        }

        private void SwitchToWifi()
        {
            currentMode = "WIFI";
            lblTitle.Text = "Wi-Fi";
            lblDeviceName.Text = wifiMon.AdapterName;
            ResetChart(Color.FromArgb(236, 72, 153));

            flowLayoutPanel1.Controls.Clear();
            statLabels.Clear();

            AddStatCard("Send", "Надсилання", wifiMon.SendSpeed);
            AddStatCard("Receive", "Отримання", wifiMon.ReceiveSpeed);
            AddStatCard("Ssid", "Ім'я мережі (SSID)", wifiMon.Ssid, 185);
            AddStatCard("ConnType", "Тип підключення", wifiMon.ConnectionType, 185);

            AddStatCard("LinkSpeed", "Швидкість з'єднання", wifiMon.LinkSpeed, 185);
            AddStatCard("Ipv4", "Адреса IPv4", wifiMon.Ipv4Address);
            AddStatCard("Mac", "MAC-адреса", wifiMon.MacAddress, 185);
            AddStatCard("Ipv6", "Адреса IPv6", wifiMon.Ipv6Address, 220);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            float chartValue = 0f;

            if (currentMode == "CPU")
            {
                cpuMon.Update();
                chartValue = cpuMon.Usage;

                statLabels["Usage"].Text = $"{Math.Round(cpuMon.Usage)}%";
                statLabels["Temp"].Text = cpuMon.Temperature;
                statLabels["Speed"].Text = cpuMon.Speed;
                statLabels["Processes"].Text = cpuMon.Processes.ToString();
                statLabels["Threads"].Text = cpuMon.Threads.ToString();
                statLabels["Handles"].Text = cpuMon.Handles.ToString();
                statLabels["UpTime"].Text = cpuMon.UpTime;
            }
            else if (currentMode == "GPU")
            {
                gpuMon.Update();
                chartValue = gpuMon.Usage;

                statLabels["Usage"].Text = $"{Math.Round(gpuMon.Usage)}%";
                statLabels["Temp"].Text = gpuMon.Temperature;
                statLabels["Engine3D"].Text = gpuMon.Engine3D;
                statLabels["EngineCopy"].Text = gpuMon.EngineCopy;
                statLabels["VideoEncode"].Text = gpuMon.VideoEncode;
                statLabels["VideoDecode"].Text = gpuMon.VideoDecode;
                statLabels["DedicatedMem"].Text = gpuMon.DedicatedMemory;
                statLabels["TotalMem"].Text = gpuMon.TotalMemory;
                statLabels["SharedMem"].Text = gpuMon.SharedMemory;
            }
            else if (currentMode == "RAM")
            {
                ramMon.Update();
                chartValue = ramMon.UsagePercent;

                statLabels["InUse"].Text = ramMon.InUse;
                statLabels["Available"].Text = ramMon.Available;
                statLabels["Committed"].Text = ramMon.Committed;
                statLabels["Cached"].Text = ramMon.Cached;
                statLabels["PagedPool"].Text = ramMon.PagedPool;
                statLabels["NonPagedPool"].Text = ramMon.NonPagedPool;
            }
            else if (currentMode == "DISK")
            {
                diskMon.Update();
                chartValue = diskMon.ActiveTime;

                statLabels["ActiveTime"].Text = $"{Math.Round(diskMon.ActiveTime)}%";
                statLabels["ResponseTime"].Text = diskMon.ResponseTime;
                statLabels["ReadSpeed"].Text = diskMon.ReadSpeed;
                statLabels["WriteSpeed"].Text = diskMon.WriteSpeed;
            }
            else if (currentMode == "WIFI")
            {
                wifiMon.Update();
                chartValue = wifiMon.ChartLoadPercent;

                statLabels["Send"].Text = wifiMon.SendSpeed;
                statLabels["Receive"].Text = wifiMon.ReceiveSpeed;
            }

            var points = chart1.Series["Load"].Points;
            points.RemoveAt(0);
            points.AddY(chartValue);
        }

        private void btnCpu_Click(object sender, EventArgs e)
        {
            SwitchToCpu();
        }

        private void btnGpu_Click(object sender, EventArgs e)
        {
            SwitchToGpu();
        }

        private void btnRam_Click(object sender, EventArgs e)
        {
            SwitchToRam();
        }

        private void btnDisk_Click(object sender, EventArgs e)
        {
            SwitchToDisk();
        }

        private void btnWifi_Click(object sender, EventArgs e)
        {
            SwitchToWifi();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save.SaveFullSystemReport(this);
        }

    }
}