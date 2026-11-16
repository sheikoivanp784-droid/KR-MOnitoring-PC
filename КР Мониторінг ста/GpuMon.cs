using System;
using System.Collections.Generic;
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

        private Process nvidiaProcess = null;
        private volatile float nvLatestLoad = -1f;
        private volatile float nvLatestUsedMb = 0f;
        private volatile float nvLatestTotalMb = 0f;
        private string nvLatestTemp = "Н/Д";

        private Dictionary<string, PerformanceCounter> engineCounters = new Dictionary<string, PerformanceCounter>();
        private DateTime lastRefreshTime = DateTime.MinValue;

        public GpuMon()
        {
            FindNvidiaSmi();
            LoadStaticInfo();
            StartNvidiaBackgroundStream();
            RefreshEngineCounters();
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

        private void StartNvidiaBackgroundStream()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = nvidiaSmiPath,
                    Arguments = "--query-gpu=utilization.gpu,temperature.gpu,memory.used,memory.total --format=csv,noheader,nounits -l 1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                nvidiaProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
                nvidiaProcess.OutputDataReceived += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;
                    string[] parts = e.Data.Split(',');
                    if (parts.Length >= 4)
                    {
                        if (float.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float load))
                            nvLatestLoad = load;

                        nvLatestTemp = $"{parts[1].Trim()} °C";

                        if (float.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float usedMb))
                            nvLatestUsedMb = usedMb;

                        if (float.TryParse(parts[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float totalMb) && totalMb > 0)
                            nvLatestTotalMb = totalMb;
                    }
                };

                nvidiaProcess.Start();
                nvidiaProcess.BeginOutputReadLine();
            }
            catch { }
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
                            DriverDate = $"{rawDate.Substring(6, 2)}/{rawDate.Substring(4, 2)}/{rawDate.Substring(0, 4)}";

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

        private void RefreshEngineCounters()
        {
            try
            {
                var category = new PerformanceCounterCategory("GPU Engine");
                var currentNames = new HashSet<string>(category.GetInstanceNames());

                var toRemove = new List<string>();
                foreach (var kvp in engineCounters)
                {
                    if (!currentNames.Contains(kvp.Key))
                    {
                        kvp.Value.Dispose();
                        toRemove.Add(kvp.Key);
                    }
                }
                foreach (string key in toRemove)
                    engineCounters.Remove(key);

                foreach (string name in currentNames)
                {
                    if (engineCounters.ContainsKey(name)) continue;

                    if (name.EndsWith("engtype_3D") ||
                        name.Contains("Graphics") ||
                        name.EndsWith("engtype_Copy") ||
                        name.EndsWith("engtype_VideoEncode") ||
                        name.EndsWith("engtype_VideoDecode"))
                    {
                        var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", name, true);
                        counter.NextValue();
                        engineCounters[name] = counter;
                    }
                }

                lastRefreshTime = DateTime.Now;
            }
            catch { }
        }

        private void ReadWindowsEngines(out float val3d, out float valCopy, out float valEncode, out float valDecode)
        {
            val3d = 0f;
            valCopy = 0f;
            valEncode = 0f;
            valDecode = 0f;

            if ((DateTime.Now - lastRefreshTime).TotalSeconds >= 3.0)
                RefreshEngineCounters();

            var deadKeys = new List<string>();
            foreach (var kvp in engineCounters)
            {
                try
                {
                    float val = kvp.Value.NextValue();
                    string name = kvp.Key;

                    if (name.EndsWith("engtype_3D") || name.Contains("Graphics"))
                        val3d += val;
                    else if (name.EndsWith("engtype_Copy"))
                        valCopy += val;
                    else if (name.EndsWith("engtype_VideoEncode"))
                        valEncode += val;
                    else if (name.EndsWith("engtype_VideoDecode"))
                        valDecode += val;
                }
                catch
                {
                    deadKeys.Add(kvp.Key);
                }
            }

            foreach (string key in deadKeys)
            {
                engineCounters[key].Dispose();
                engineCounters.Remove(key);
            }

            val3d = Math.Min(100f, val3d);
            valCopy = Math.Min(100f, valCopy);
            valEncode = Math.Min(100f, valEncode);
            valDecode = Math.Min(100f, valDecode);
        }

        public void Update()
        {
            ReadWindowsEngines(out float val3d, out float valCopy, out float valEncode, out float valDecode);

            float usedDedicatedGb = 0f;
            bool hasNvidiaStream = nvLatestLoad >= 0f;

            if (hasNvidiaStream)
            {
                Usage = Math.Min(100f, Math.Max(nvLatestLoad, val3d));
                Temperature = nvLatestTemp;
                if (val3d < nvLatestLoad) val3d = nvLatestLoad;

                usedDedicatedGb = nvLatestUsedMb / 1024f;
                if (nvLatestTotalMb > 0)
                    totalDedicatedGb = nvLatestTotalMb / 1024f;
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

                    if (!hasNvidiaStream)
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