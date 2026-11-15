using System;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace КР_Мониторінг_ста
{
    public class DiskMon
    {
        public string Model { get; private set; } = "Системний диск (C:)";
        public float ActiveTime { get; private set; } = 0f;
        public string ResponseTime { get; private set; } = "0.0 мс";
        public string ReadSpeed { get; private set; } = "0 КБ/с";
        public string WriteSpeed { get; private set; } = "0 КБ/с";
        public string Capacity { get; private set; } = "Н/Д";
        public string Formatted { get; private set; } = "Н/Д";
        public string FreeSpace { get; private set; } = "Н/Д";
        public string SystemDisk { get; private set; } = "Так";
        public string PageFile { get; private set; } = "Так";
        public string DriveType { get; private set; } = "SSD (NVMe)";

        private PerformanceCounter activeTimeCounter;
        private PerformanceCounter readBytesCounter;
        private PerformanceCounter writeBytesCounter;
        private PerformanceCounter avgSecPerTransferCounter;

        public DiskMon()
        {
            InitCounters();
            LoadStaticInfo();
        }

        private void InitCounters()
        {
            try
            {
                activeTimeCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
                activeTimeCounter.NextValue();

                readBytesCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
                readBytesCounter.NextValue();

                writeBytesCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
                writeBytesCounter.NextValue();

                avgSecPerTransferCounter = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Transfer", "_Total");
                avgSecPerTransferCounter.NextValue();
            }
            catch { }
        }

        private void LoadStaticInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Model, InterfaceType, Size FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        Model = obj["Model"]?.ToString().Trim() ?? "Накопичувач";
                        string iface = obj["InterfaceType"]?.ToString() ?? "";
                        DriveType = iface == "SCSI" || iface == "NVMe" ? "SSD (NVMe)" : iface;

                        if (obj["Size"] != null)
                        {
                            double sizeGb = Convert.ToDouble(obj["Size"]) / (1024.0 * 1024.0 * 1024.0);
                            Capacity = $"{Math.Round(sizeGb)} ГБ";
                        }
                        break;
                    }
                }

                DriveInfo cDrive = new DriveInfo("C");
                if (cDrive.IsReady)
                {
                    double totalGb = cDrive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    double freeGb = cDrive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    Formatted = $"{Math.Round(totalGb)} ГБ";
                    FreeSpace = $"{freeGb:F1} ГБ";
                }
            }
            catch { }
        }

        private string FormatSpeed(float bytesPerSec)
        {
            if (bytesPerSec >= 1024f * 1024f)
                return $"{(bytesPerSec / (1024f * 1024f)):F1} МБ/с";
            return $"{Math.Round(bytesPerSec / 1024f)} КБ/с";
        }

        public void Update()
        {
            try
            {
                if (activeTimeCounter != null)
                    ActiveTime = Math.Min(100f, activeTimeCounter.NextValue());

                if (readBytesCounter != null)
                    ReadSpeed = FormatSpeed(readBytesCounter.NextValue());

                if (writeBytesCounter != null)
                    WriteSpeed = FormatSpeed(writeBytesCounter.NextValue());

                if (avgSecPerTransferCounter != null)
                {
                    float ms = avgSecPerTransferCounter.NextValue() * 1000f;
                    ResponseTime = $"{ms:F1} мс";
                }
            }
            catch { }
        }
    }
}