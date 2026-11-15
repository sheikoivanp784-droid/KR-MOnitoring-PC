using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace КР_Мониторінг_ста
{
    public class CpuMon
    {
        public string Model { get; private set; } = "Завантаження...";
        public float Usage { get; private set; } = 0f;
        public string Temperature { get; private set; } = "Н/Д";
        public string Speed { get; private set; } = "0.00 ГГц";
        public string BaseSpeed { get; private set; } = "0.00 ГГц";
        public int Processes { get; private set; } = 0;
        public int Threads { get; private set; } = 0;
        public int Handles { get; private set; } = 0;
        public string UpTime { get; private set; } = "0:00:00:00";
        public int Sockets { get; private set; } = 1;
        public int Cores { get; private set; } = 0;
        public int LogicalProcessors { get; private set; } = Environment.ProcessorCount;
        public string Virtualization { get; private set; } = "Увімкнено";
        public string CacheL1 { get; private set; } = "Н/Д";
        public string CacheL2 { get; private set; } = "Н/Д";
        public string CacheL3 { get; private set; } = "Н/Д";

        private List<PerformanceCounter> coreCounters = new List<PerformanceCounter>();
        private PerformanceCounter perfCounter;
        private float baseClockMhz = 0f;
        private Queue<float> usageHistory = new Queue<float>();

        public CpuMon()
        {
            InitCounters();
            LoadStaticInfo();
        }

        private void InitCounters()
        {
            try
            {
                int coreCount = Environment.ProcessorCount;
                for (int i = 0; i < coreCount; i++)
                {
                    var counter = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
                    counter.NextValue();
                    coreCounters.Add(counter);
                }

                perfCounter = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total");
                perfCounter.NextValue();
            }
            catch { }
        }

        private void LoadStaticInfo()
        {
            try
            {
                int socketCount = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        socketCount++;
                        Model = obj["Name"]?.ToString().Trim() ?? "Невідомий ЦП";
                        Cores = Convert.ToInt32(obj["NumberOfCores"]);
                        LogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                        baseClockMhz = Convert.ToSingle(obj["MaxClockSpeed"]);
                        BaseSpeed = $"{(baseClockMhz / 1000f):F2} ГГц";

                        bool virt = Convert.ToBoolean(obj["VirtualizationFirmwareEnabled"]);
                        Virtualization = virt ? "Увімкнено" : "Вимкнено";

                        if (obj["L2CacheSize"] != null)
                            CacheL2 = $"{(Convert.ToDouble(obj["L2CacheSize"]) / 1024.0):F1} МБ";
                        if (obj["L3CacheSize"] != null)
                            CacheL3 = $"{(Convert.ToDouble(obj["L3CacheSize"]) / 1024.0):F1} МБ";
                    }
                }
                Sockets = socketCount > 0 ? socketCount : 1;

                long l1TotalKb = 0;
                using (var cacheSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_CacheMemory WHERE Level = 3"))
                {
                    foreach (ManagementObject obj in cacheSearcher.Get())
                    {
                        l1TotalKb += Convert.ToInt64(obj["MaxCacheSize"]);
                    }
                }
                if (l1TotalKb > 0) CacheL1 = $"{l1TotalKb} КБ";
            }
            catch { }
        }

        private string ReadCpuTemperature()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double kelvinTenths = Convert.ToDouble(obj["CurrentTemperature"]);
                        double celsius = (kelvinTenths / 10.0) - 273.15;
                        if (celsius > 10 && celsius < 120)
                            return $"{Math.Round(celsius)} °C";
                    }
                }
            }
            catch { }

            return "Н/Д";
        }

        public void Update()
        {
            if (coreCounters.Count > 0)
            {
                float sum = 0f;
                foreach (var c in coreCounters)
                {
                    sum += c.NextValue();
                }

                float avg = Math.Min(100f, sum / coreCounters.Count);

                usageHistory.Enqueue(avg);
                if (usageHistory.Count > 3)
                    usageHistory.Dequeue();

                float smooth = 0f;
                foreach (float val in usageHistory)
                    smooth += val;

                Usage = smooth / usageHistory.Count;
            }

            if (perfCounter != null && baseClockMhz > 0)
            {
                float perfPercent = perfCounter.NextValue();
                float currentGhz = (baseClockMhz / 1000f) * (perfPercent / 100f);
                Speed = $"{currentGhz:F2} ГГц";
            }

            Process[] processes = Process.GetProcesses();
            Processes = processes.Length;
            int threads = 0;
            int handles = 0;
            foreach (var p in processes)
            {
                try
                {
                    threads += p.Threads.Count;
                    handles += p.HandleCount;
                }
                catch { }
                finally { p.Dispose(); }
            }
            Threads = threads;
            Handles = handles;

            Temperature = ReadCpuTemperature();

            TimeSpan uptime = TimeSpan.FromMilliseconds((uint)Environment.TickCount);
            UpTime = $"{uptime.Days}:{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }
    }
}