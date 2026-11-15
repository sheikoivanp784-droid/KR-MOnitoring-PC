using System;
using System.Diagnostics;
using System.Management;

namespace КР_Мониторінг_ста
{
    public class RamMon
    {
        public string Model { get; private set; } = "Оперативна пам'ять";
        public float UsagePercent { get; private set; } = 0f;
        public string InUse { get; private set; } = "0.0 ГБ";
        public string Available { get; private set; } = "0.0 ГБ";
        public string Committed { get; private set; } = "0.0 / 0.0 ГБ";
        public string Cached { get; private set; } = "0.0 ГБ";
        public string PagedPool { get; private set; } = "0 МБ";
        public string NonPagedPool { get; private set; } = "0 МБ";
        public string Speed { get; private set; } = "Н/Д";
        public string SlotsUsed { get; private set; } = "Н/Д";
        public string FormFactor { get; private set; } = "Н/Д";
        public string HardwareReserved { get; private set; } = "Н/Д";

        private float totalVisibleGb = 0f;
        private PerformanceCounter availCounter;
        private PerformanceCounter commitBytesCounter;
        private PerformanceCounter commitLimitCounter;
        private PerformanceCounter cacheBytesCounter;
        private PerformanceCounter pagedPoolCounter;
        private PerformanceCounter nonPagedPoolCounter;

        public RamMon()
        {
            InitCounters();
            LoadStaticInfo();
        }

        private void InitCounters()
        {
            try
            {
                availCounter = new PerformanceCounter("Memory", "Available MBytes");
                commitBytesCounter = new PerformanceCounter("Memory", "Committed Bytes");
                commitLimitCounter = new PerformanceCounter("Memory", "Commit Limit");
                cacheBytesCounter = new PerformanceCounter("Memory", "Cache Bytes");
                pagedPoolCounter = new PerformanceCounter("Memory", "Pool Paged Bytes");
                nonPagedPoolCounter = new PerformanceCounter("Memory", "Pool Nonpaged Bytes");
            }
            catch { }
        }

        private void LoadStaticInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double kb = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                        totalVisibleGb = (float)(kb / (1024.0 * 1024.0));
                        Model = $"{Math.Round(totalVisibleGb, 1)} ГБ DDR";
                    }
                }

                long installedBytes = 0;
                int usedSlots = 0;
                int speedMhz = 0;
                int formFactorCode = 0;

                using (var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, FormFactor FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        usedSlots++;
                        installedBytes += Convert.ToInt64(obj["Capacity"]);
                        if (obj["Speed"] != null) speedMhz = Convert.ToInt32(obj["Speed"]);
                        if (obj["FormFactor"] != null) formFactorCode = Convert.ToInt32(obj["FormFactor"]);
                    }
                }

                int totalSlots = 4;
                using (var arraySearcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray"))
                {
                    foreach (ManagementObject obj in arraySearcher.Get())
                    {
                        if (obj["MemoryDevices"] != null)
                            totalSlots = Convert.ToInt32(obj["MemoryDevices"]);
                    }
                }

                if (speedMhz > 0) Speed = $"{speedMhz} МТ/с";
                SlotsUsed = $"{usedSlots} з {totalSlots}";
                FormFactor = formFactorCode == 12 ? "SODIMM" : "DIMM";

                float installedGb = installedBytes / (1024f * 1024f * 1024f);
                if (installedGb > totalVisibleGb)
                {
                    float reservedMb = (installedGb - totalVisibleGb) * 1024f;
                    HardwareReserved = $"{Math.Round(reservedMb)} МБ";
                }
                else
                {
                    HardwareReserved = "0 МБ";
                }
            }
            catch { }
        }

        public void Update()
        {
            if (availCounter != null && totalVisibleGb > 0)
            {
                float availGb = availCounter.NextValue() / 1024f;
                float usedGb = Math.Max(0f, totalVisibleGb - availGb);

                UsagePercent = Math.Min(100f, (usedGb / totalVisibleGb) * 100f);
                InUse = $"{usedGb:F1} ГБ ({Math.Round(UsagePercent)}%)";
                Available = $"{availGb:F1} ГБ";
            }

            try
            {
                if (commitBytesCounter != null && commitLimitCounter != null)
                {
                    float comGb = commitBytesCounter.NextValue() / (1024f * 1024f * 1024f);
                    float limGb = commitLimitCounter.NextValue() / (1024f * 1024f * 1024f);
                    Committed = $"{comGb:F1} / {limGb:F1} ГБ";
                }

                if (cacheBytesCounter != null)
                {
                    float cacheGb = cacheBytesCounter.NextValue() / (1024f * 1024f * 1024f);
                    Cached = $"{cacheGb:F1} ГБ";
                }

                if (pagedPoolCounter != null)
                    PagedPool = $"{Math.Round(pagedPoolCounter.NextValue() / (1024f * 1024f))} МБ";

                if (nonPagedPoolCounter != null)
                    NonPagedPool = $"{Math.Round(nonPagedPoolCounter.NextValue() / (1024f * 1024f))} МБ";
            }
            catch { }
        }
    }
}