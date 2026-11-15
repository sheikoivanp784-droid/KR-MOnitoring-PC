using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;

namespace КР_Мониторінг_ста
{
    public class GpuMon
    {
        public string Model { get; private set; } = "Завантаження...";
        public float Usage { get; private set; } = 0f;
        public string Engine3D { get; private set; } = "0%";
        public string EngineCopy { get; private set; } = "0%";
        public string VideoEncode { get; private set; } = "0%";
        public string VideoDecode { get; private set; } = "0%";
        public string Temperature { get; private set; } = "Н/Д";
        public string DedicatedMemory { get; private set; } = "0.0 / 12.0 ГБ";
        public string SharedMemory { get; private set; } = "0.0 / 0.0 ГБ";
        public string TotalMemory { get; private set; } = "0.0 / 0.0 ГБ";
        public string DriverVersion { get; private set; } = "Н/Д";
        public string DriverDate { get; private set; } = "Н/Д";
        public string DirectX { get; private set; } = "12 (FL 12.2)";
        public string Location { get; private set; } = "PCI-шина";

        private float totalDedicatedGb = 12f;
        private float totalSharedGb = 16f;
        private string nvidiaSmiPath = null;

        public GpuMon()
        {
            FindNvidiaSmi();
            LoadStaticInfo();
        }

        private void FindNvidiaSmi()
        {
            string sysPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "nvidia-smi.exe");
            string progFilesPath = @"C:\Program Files\NVIDIA Corporation\NVSMI\nvidia-smi.exe";

            if (File.Exists(sysPath))
                nvidiaSmiPath = sysPath;
            else if (File.Exists(progFilesPath))
                nvidiaSmiPath = progFilesPath;
            else
                nvidiaSmiPath = "nvidia-smi";
        }

        private void LoadStaticInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        Model = obj["Name"]?.ToString() ?? "Відеоадаптер";
                        DriverVersion = obj["DriverVersion"]?.ToString() ?? "Н/Д";

                        string rawDate = obj["DriverDate"]?.ToString();
                        if (!string.IsNullOrEmpty(rawDate) && rawDate.Length >= 8)
                        {
                            DriverDate = $"{rawDate.Substring(6, 2)}/{rawDate.Substring(4, 2)}/{rawDate.Substring(0, 4)}";
                        }

                        totalSharedGb = GetTotalSystemRamGb() / 2f;
                        Location = obj["PNPDeviceID"]?.ToString().Split('\\')[0] ?? "PCI-шина";
                        break;
                    }
                }
            }
            catch { }
        }

        private float GetTotalSystemRamGb()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double kb = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                        return (float)(kb / (1024.0 * 1024.0));
                    }
                }
            }
            catch { }
            return 32f;
        }

        private bool ReadNvidiaStats(out float gpuLoad, out string tempStr, out float usedVramGb, out float totalVramGb)
        {
            gpuLoad = 0f;
            tempStr = "Н/Д";
            usedVramGb = 0f;
            totalVramGb = totalDedicatedGb;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = nvidiaSmiPath,
                    Arguments = "--query-gpu=utilization.gpu,temperature.gpu,memory.used,memory.total --format=csv,noheader,nounits",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd().Trim();
                    proc.WaitForExit(400);

                    if (!string.IsNullOrEmpty(output))
                    {
                        string[] parts = output.Split(',');
                        if (parts.Length >= 4)
                        {
                            float.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out gpuLoad);
                            tempStr = $"{parts[1].Trim()} °C";

                            float usedMb = 0f, totalMb = 0f;
                            float.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out usedMb);
                            float.TryParse(parts[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out totalMb);

                            usedVramGb = usedMb / 1024f;
                            if (totalMb > 0)
                            {
                                totalVramGb = totalMb / 1024f;
                                totalDedicatedGb = totalVramGb;
                            }
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private void UpdateWindowsEngines(out float val3d, out float valCopy, out float valEncode, out float valDecode)
        {
            val3d = 0f;
            valCopy = 0f;
            valEncode = 0f;
            valDecode = 0f;

            try
            {
                var category = new PerformanceCounterCategory("GPU Engine");
                string[] instances = category.GetInstanceNames();

                foreach (string name in instances)
                {
                    if (name.EndsWith("engtype_3D") || name.Contains("Graphics"))
                    {
                        using (var c = new PerformanceCounter("GPU Engine", "Utilization Percentage", name, true))
                            val3d += c.NextValue();
                    }
                    else if (name.EndsWith("engtype_Copy"))
                    {
                        using (var c = new PerformanceCounter("GPU Engine", "Utilization Percentage", name, true))
                            valCopy += c.NextValue();
                    }
                    else if (name.EndsWith("engtype_VideoEncode"))
                    {
                        using (var c = new PerformanceCounter("GPU Engine", "Utilization Percentage", name, true))
                            valEncode += c.NextValue();
                    }
                    else if (name.EndsWith("engtype_VideoDecode"))
                    {
                        using (var c = new PerformanceCounter("GPU Engine", "Utilization Percentage", name, true))
                            valDecode += c.NextValue();
                    }
                }
            }
            catch { }

            val3d = Math.Min(100f, val3d);
            valCopy = Math.Min(100f, valCopy);
            valEncode = Math.Min(100f, valEncode);
            valDecode = Math.Min(100f, valDecode);
        }

        public void Update()
        {
            UpdateWindowsEngines(out float val3d, out float valCopy, out float valEncode, out float valDecode);

            bool hasNvidia = ReadNvidiaStats(out float nvLoad, out string nvTemp, out float usedDedicatedGb, out float totalVramGb);

            if (hasNvidia)
            {
                Usage = Math.Min(100f, Math.Max(nvLoad, val3d));
                Temperature = nvTemp;
                if (val3d < nvLoad) val3d = nvLoad;
            }
            else
            {
                Usage = Math.Max(Math.Max(val3d, valCopy), Math.Max(valEncode, valDecode));
            }

            Engine3D = $"{Math.Round(val3d)}%";
            EngineCopy = $"{Math.Round(valCopy)}%";
            VideoEncode = $"{Math.Round(valEncode)}%";
            VideoDecode = $"{Math.Round(valDecode)}%";

            float usedSharedGb = 0f;
            try
            {
                var memCategory = new PerformanceCounterCategory("GPU Adapter Memory");
                foreach (string inst in memCategory.GetInstanceNames())
                {
                    using (var shdCounter = new PerformanceCounter("GPU Adapter Memory", "Shared Usage", inst, true))
                        usedSharedGb += shdCounter.NextValue() / (1024f * 1024f * 1024f);

                    if (!hasNvidia)
                    {
                        using (var dedCounter = new PerformanceCounter("GPU Adapter Memory", "Dedicated Usage", inst, true))
                            usedDedicatedGb += dedCounter.NextValue() / (1024f * 1024f * 1024f);
                    }
                }
            }
            catch { }

            DedicatedMemory = $"{usedDedicatedGb:F1} / {totalDedicatedGb:F1} ГБ";
            SharedMemory = $"{usedSharedGb:F1} / {totalSharedGb:F1} ГБ";
            TotalMemory = $"{(usedDedicatedGb + usedSharedGb):F1} / {(totalDedicatedGb + totalSharedGb):F1} ГБ";
        }
    }
}