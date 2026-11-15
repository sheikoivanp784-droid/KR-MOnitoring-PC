using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace КР_Мониторінг_ста
{
    public class WifiMon
    {
        public string AdapterName { get; private set; } = "Wi-Fi";
        public float ChartLoadPercent { get; private set; } = 0f;
        public string SendSpeed { get; private set; } = "0 Кбіт/с";
        public string ReceiveSpeed { get; private set; } = "0 Кбіт/с";
        public string Ssid { get; private set; } = "Не підключено";
        public string ConnectionType { get; private set; } = "802.11ax (Wi-Fi 6)";
        public string LinkSpeed { get; private set; } = "Н/Д";
        public string Ipv4Address { get; private set; } = "Н/Д";
        public string Ipv6Address { get; private set; } = "Н/Д";
        public string MacAddress { get; private set; } = "Н/Д";

        private NetworkInterface wifiAdapter;
        private long lastBytesSent = 0;
        private long lastBytesReceived = 0;
        private DateTime lastCheckTime = DateTime.Now;

        public WifiMon()
        {
            FindWifiAdapter();
            LoadStaticInfo();
        }

        private void FindWifiAdapter()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            wifiAdapter = interfaces.FirstOrDefault(n =>
                n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                n.OperationalStatus == OperationalStatus.Up);

            if (wifiAdapter == null)
            {
                wifiAdapter = interfaces.FirstOrDefault(n =>
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    n.OperationalStatus == OperationalStatus.Up);
            }

            if (wifiAdapter != null)
            {
                var stats = wifiAdapter.GetIPv4Statistics();
                lastBytesSent = stats.BytesSent;
                lastBytesReceived = stats.BytesReceived;
                lastCheckTime = DateTime.Now;
            }
        }

        private void LoadStaticInfo()
        {
            if (wifiAdapter == null) return;

            AdapterName = wifiAdapter.Description;
            double speedMbps = wifiAdapter.Speed / 1_000_000.0;
            LinkSpeed = $"{Math.Round(speedMbps)} Мбіт/с";

            byte[] macBytes = wifiAdapter.GetPhysicalAddress().GetAddressBytes();
            if (macBytes.Length > 0)
                MacAddress = string.Join(":", macBytes.Select(b => b.ToString("X2")));

            var ipProps = wifiAdapter.GetIPProperties();
            foreach (var ip in ipProps.UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    Ipv4Address = ip.Address.ToString();
                else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6 && !ip.Address.IsIPv6LinkLocal)
                    Ipv6Address = ip.Address.ToString();
            }

            ReadSsidFromNetsh();
        }

        private void ReadSsidFromNetsh()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "wlan show interfaces",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(300);

                    foreach (string line in output.Split('\n'))
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("SSID") && !trimmed.StartsWith("BSSID"))
                        {
                            int idx = trimmed.IndexOf(':');
                            if (idx > 0) Ssid = trimmed.Substring(idx + 1).Trim();
                        }
                        else if (trimmed.StartsWith("Radio type") || trimmed.StartsWith("Тип радио"))
                        {
                            int idx = trimmed.IndexOf(':');
                            if (idx > 0) ConnectionType = trimmed.Substring(idx + 1).Trim();
                        }
                    }
                }
            }
            catch { }
        }

        private string FormatBitsSpeed(double bitsPerSec)
        {
            if (bitsPerSec >= 1_000_000)
                return $"{(bitsPerSec / 1_000_000.0):F1} Мбіт/с";
            return $"{Math.Round(bitsPerSec / 1000.0)} Кбіт/с";
        }

        public void Update()
        {
            if (wifiAdapter == null) return;

            DateTime now = DateTime.Now;
            double seconds = (now - lastCheckTime).TotalSeconds;
            if (seconds <= 0.1) return;

            var stats = wifiAdapter.GetIPv4Statistics();
            long sentDiff = stats.BytesSent - lastBytesSent;
            long recvDiff = stats.BytesReceived - lastBytesReceived;

            lastBytesSent = stats.BytesSent;
            lastBytesReceived = stats.BytesReceived;
            lastCheckTime = now;

            double sendBitsPerSec = (sentDiff * 8.0) / seconds;
            double recvBitsPerSec = (recvDiff * 8.0) / seconds;

            SendSpeed = FormatBitsSpeed(sendBitsPerSec);
            ReceiveSpeed = FormatBitsSpeed(recvBitsPerSec);

            // На графике показываем загрузку канала в % от скорости линка (масштабируем для наглядности)
            double maxLinkBits = Math.Max(1_000_000.0, wifiAdapter.Speed);
            double totalBits = sendBitsPerSec + recvBitsPerSec;
            ChartLoadPercent = (float)Math.Min(100.0, (totalBits / maxLinkBits) * 100.0);
        }
    }
}