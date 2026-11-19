using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;

namespace КР_Мониторінг_ста
{
    public class Save
    {
        private readonly string connectionString;
        private readonly string dbFilePath;

        private readonly CpuMon cpu;
        private readonly GpuMon gpu;
        private readonly RamMon ram;
        private readonly DiskMon disk;
        private readonly WifiMon wifi;

        public Save(string dbPath, CpuMon cpu, GpuMon gpu, RamMon ram, DiskMon disk, WifiMon wifi)
        {
            this.dbFilePath = dbPath;
            this.connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";

            this.cpu = cpu;
            this.gpu = gpu;
            this.ram = ram;
            this.disk = disk;
            this.wifi = wifi;
        }

        public void SaveSnapshot()
        {
            if (!File.Exists(dbFilePath))
            {
                MessageBox.Show(
                    $"Файл бази данних не знайдено:\n{dbFilePath}",
                    "База данних не знайдена",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            cpu?.Update();
            gpu?.Update();
            ram?.Update();
            disk?.Update();
            wifi?.Update();

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    EnsureAllFiveTablesExist(conn);

                    DateTime now = DateTime.Now;
                    string dateStr = now.ToString("dd.MM.yyyy");
                    string timeStr = now.ToString("HH:mm:ss");

                    InsertCpu(conn, dateStr, timeStr);
                    InsertGpu(conn, dateStr, timeStr);
                    InsertRam(conn, dateStr, timeStr);
                    InsertDisk(conn, dateStr, timeStr);
                    InsertWifi(conn, dateStr, timeStr);
                }

                MessageBox.Show(
                    "Всі характеристики успішно записані",
                    "Сохранено в БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка при работі з базою данних Access:\n{ex.Message}",
                    "Помилка Access",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool TableExists(OleDbConnection conn, string tableName)
        {
            DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, tableName, "TABLE" });
            return schema != null && schema.Rows.Count > 0;
        }

        private void ExecuteSql(OleDbConnection conn, string sql)
        {
            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureAllFiveTablesExist(OleDbConnection conn)
        {
            // 1. Таблиця CPU
            if (!TableExists(conn, "CpuLogs"))
            {
                ExecuteSql(conn, @"
                    CREATE TABLE CpuLogs (
                        [ID] COUNTER PRIMARY KEY,
                        [RecordDate] TEXT(20),
                        [RecordTime] TEXT(20),
                        [Model] TEXT(100),
                        [UsagePercent] DOUBLE,
                        [Temperature] TEXT(30),
                        [Speed] TEXT(30),
                        [BaseSpeed] TEXT(30),
                        [UpTime] TEXT(30),
                        [Cores] INTEGER,
                        [LogicalProcessors] INTEGER,
                        [Sockets] INTEGER,
                        [Virtualization] TEXT(30),
                        [Processes] INTEGER,
                        [Threads] INTEGER,
                        [Handles] INTEGER,
                        [CacheL1] TEXT(30),
                        [CacheL2] TEXT(30),
                        [CacheL3] TEXT(30)
                    )");
            }

            // 2. Таблиця GPU
            if (!TableExists(conn, "GpuLogs"))
            {
                ExecuteSql(conn, @"
                    CREATE TABLE GpuLogs (
                        [ID] COUNTER PRIMARY KEY,
                        [RecordDate] TEXT(20),
                        [RecordTime] TEXT(20),
                        [Model] TEXT(100),
                        [UsagePercent] DOUBLE,
                        [Temperature] TEXT(30),
                        [Engine3D] TEXT(30),
                        [EngineCopy] TEXT(30),
                        [VideoEncode] TEXT(30),
                        [VideoDecode] TEXT(30),
                        [DedicatedMemory] TEXT(50),
                        [SharedMemory] TEXT(50),
                        [TotalMemory] TEXT(50),
                        [DirectX] TEXT(30),
                        [DriverVersion] TEXT(50),
                        [DriverDate] TEXT(30),
                        [Location] TEXT(100)
                    )");
            }

            // 3. Таблиця RAM
            if (!TableExists(conn, "RamLogs"))
            {
                ExecuteSql(conn, @"
                    CREATE TABLE RamLogs (
                        [ID] COUNTER PRIMARY KEY,
                        [RecordDate] TEXT(20),
                        [RecordTime] TEXT(20),
                        [Model] TEXT(100),
                        [UsagePercent] DOUBLE,
                        [InUse] TEXT(50),
                        [Available] TEXT(30),
                        [Committed] TEXT(50),
                        [Cached] TEXT(30),
                        [PagedPool] TEXT(30),
                        [NonPagedPool] TEXT(30),
                        [Speed] TEXT(30),
                        [SlotsUsed] TEXT(30),
                        [FormFactor] TEXT(30),
                        [HardwareReserved] TEXT(30)
                    )");
            }

            // 4. Таблиця DISK
            if (!TableExists(conn, "DiskLogs"))
            {
                ExecuteSql(conn, @"
                    CREATE TABLE DiskLogs (
                        [ID] COUNTER PRIMARY KEY,
                        [RecordDate] TEXT(20),
                        [RecordTime] TEXT(20),
                        [Model] TEXT(100),
                        [ActiveTimePercent] DOUBLE,
                        [ResponseTime] TEXT(30),
                        [ReadSpeed] TEXT(30),
                        [WriteSpeed] TEXT(30),
                        [Capacity] TEXT(30),
                        [Formatted] TEXT(30),
                        [SystemDisk] TEXT(20),
                        [PageFile] TEXT(20),
                        [DriveType] TEXT(30)
                    )");
            }

            // 5. Таблиця WI-FI
            if (!TableExists(conn, "WifiLogs"))
            {
                ExecuteSql(conn, @"
                    CREATE TABLE WifiLogs (
                        [ID] COUNTER PRIMARY KEY,
                        [RecordDate] TEXT(20),
                        [RecordTime] TEXT(20),
                        [AdapterName] TEXT(100),
                        [ReceiveSpeed] TEXT(30),
                        [SendSpeed] TEXT(30),
                        [Ssid] TEXT(60),
                        [ConnectionType] TEXT(40),
                        [Ipv4Address] TEXT(40),
                        [Ipv6Address] TEXT(60),
                        [SignalStrength] TEXT(30)
                    )");
            }
        }

        private string GetProp(object obj, params string[] propNames)
        {
            if (obj == null) return "Н/Д";
            Type t = obj.GetType();
            foreach (string name in propNames)
            {
                var prop = t.GetProperty(name);
                if (prop != null)
                {
                    object val = prop.GetValue(obj);
                    return val != null && !string.IsNullOrWhiteSpace(val.ToString()) ? val.ToString() : "Н/Д";
                }
            }
            return "Н/Д";
        }

        private void InsertCpu(OleDbConnection conn, string date, string time)
        {
            string sql = @"INSERT INTO CpuLogs (
                [RecordDate], [RecordTime], [Model], [UsagePercent], [Temperature], [Speed], [BaseSpeed], [UpTime],
                [Cores], [LogicalProcessors], [Sockets], [Virtualization], [Processes], [Threads], [Handles],
                [CacheL1], [CacheL2], [CacheL3]
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p1", date);
                cmd.Parameters.AddWithValue("@p2", time);
                cmd.Parameters.AddWithValue("@p3", cpu?.Model ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p4", Math.Round(cpu?.Usage ?? 0, 1));
                cmd.Parameters.AddWithValue("@p5", cpu?.Temperature ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p6", cpu?.Speed ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p7", cpu?.BaseSpeed ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p8", cpu?.UpTime ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p9", cpu?.Cores ?? 0);
                cmd.Parameters.AddWithValue("@p10", cpu?.LogicalProcessors ?? 0);
                cmd.Parameters.AddWithValue("@p11", cpu?.Sockets ?? 0);
                cmd.Parameters.AddWithValue("@p12", cpu?.Virtualization ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p13", cpu?.Processes ?? 0);
                cmd.Parameters.AddWithValue("@p14", cpu?.Threads ?? 0);
                cmd.Parameters.AddWithValue("@p15", cpu?.Handles ?? 0);
                cmd.Parameters.AddWithValue("@p16", cpu?.CacheL1 ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p17", cpu?.CacheL2 ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p18", cpu?.CacheL3 ?? "Н/Д");
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertGpu(OleDbConnection conn, string date, string time)
        {
            string sql = @"INSERT INTO GpuLogs (
                [RecordDate], [RecordTime], [Model], [UsagePercent], [Temperature], [Engine3D], [EngineCopy],
                [VideoEncode], [VideoDecode], [DedicatedMemory], [SharedMemory], [TotalMemory],
                [DirectX], [DriverVersion], [DriverDate], [Location]
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p1", date);
                cmd.Parameters.AddWithValue("@p2", time);
                cmd.Parameters.AddWithValue("@p3", gpu?.Model ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p4", Math.Round(gpu?.Usage ?? 0, 1));
                cmd.Parameters.AddWithValue("@p5", gpu?.Temperature ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p6", gpu?.Engine3D ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p7", gpu?.EngineCopy ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p8", gpu?.VideoEncode ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p9", gpu?.VideoDecode ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p10", gpu?.DedicatedMemory ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p11", gpu?.SharedMemory ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p12", gpu?.TotalMemory ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p13", gpu?.DirectX ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p14", gpu?.DriverVersion ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p15", gpu?.DriverDate ?? "Н/Д");
                cmd.Parameters.AddWithValue("@p16", gpu?.Location ?? "Н/Д");
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertRam(OleDbConnection conn, string date, string time)
        {
            string sql = @"INSERT INTO RamLogs (
                [RecordDate], [RecordTime], [Model], [UsagePercent], [InUse], [Available], [Committed],
                [Cached], [PagedPool], [NonPagedPool], [Speed], [SlotsUsed], [FormFactor], [HardwareReserved]
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p1", date);
                cmd.Parameters.AddWithValue("@p2", time);
                cmd.Parameters.AddWithValue("@p3", GetProp(ram, "Model"));
                cmd.Parameters.AddWithValue("@p4", Math.Round(ram?.UsagePercent ?? 0, 1));
                cmd.Parameters.AddWithValue("@p5", GetProp(ram, "InUse", "InUseCompressed"));
                cmd.Parameters.AddWithValue("@p6", GetProp(ram, "Available"));
                cmd.Parameters.AddWithValue("@p7", GetProp(ram, "Committed"));
                cmd.Parameters.AddWithValue("@p8", GetProp(ram, "Cached"));
                cmd.Parameters.AddWithValue("@p9", GetProp(ram, "PagedPool"));
                cmd.Parameters.AddWithValue("@p10", GetProp(ram, "NonPagedPool"));
                cmd.Parameters.AddWithValue("@p11", GetProp(ram, "Speed"));
                cmd.Parameters.AddWithValue("@p12", GetProp(ram, "SlotsUsed"));
                cmd.Parameters.AddWithValue("@p13", GetProp(ram, "FormFactor"));
                cmd.Parameters.AddWithValue("@p14", GetProp(ram, "HardwareReserved"));
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertDisk(OleDbConnection conn, string date, string time)
        {
            string sql = @"INSERT INTO DiskLogs (
                [RecordDate], [RecordTime], [Model], [ActiveTimePercent], [ResponseTime], [ReadSpeed],
                [WriteSpeed], [Capacity], [Formatted], [SystemDisk], [PageFile], [DriveType]
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p1", date);
                cmd.Parameters.AddWithValue("@p2", time);
                cmd.Parameters.AddWithValue("@p3", GetProp(disk, "Model"));
                cmd.Parameters.AddWithValue("@p4", Math.Round(disk?.ActiveTime ?? 0, 1));
                cmd.Parameters.AddWithValue("@p5", GetProp(disk, "ResponseTime"));
                cmd.Parameters.AddWithValue("@p6", GetProp(disk, "ReadSpeed"));
                cmd.Parameters.AddWithValue("@p7", GetProp(disk, "WriteSpeed"));
                cmd.Parameters.AddWithValue("@p8", GetProp(disk, "Capacity"));
                cmd.Parameters.AddWithValue("@p9", GetProp(disk, "Formatted"));
                cmd.Parameters.AddWithValue("@p10", GetProp(disk, "SystemDisk"));
                cmd.Parameters.AddWithValue("@p11", GetProp(disk, "PageFile"));
                cmd.Parameters.AddWithValue("@p12", GetProp(disk, "DriveType"));
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertWifi(OleDbConnection conn, string date, string time)
        {
            string sql = @"INSERT INTO WifiLogs (
                [RecordDate], [RecordTime], [AdapterName], [ReceiveSpeed], [SendSpeed], [Ssid],
                [ConnectionType], [Ipv4Address], [Ipv6Address], [SignalStrength]
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p1", date);
                cmd.Parameters.AddWithValue("@p2", time);
                cmd.Parameters.AddWithValue("@p3", GetProp(wifi, "AdapterName", "AdapterModel"));
                cmd.Parameters.AddWithValue("@p4", GetProp(wifi, "ReceiveSpeed"));
                cmd.Parameters.AddWithValue("@p5", GetProp(wifi, "SendSpeed"));
                cmd.Parameters.AddWithValue("@p6", GetProp(wifi, "Ssid"));
                cmd.Parameters.AddWithValue("@p7", GetProp(wifi, "ConnectionType"));
                cmd.Parameters.AddWithValue("@p8", GetProp(wifi, "Ipv4Address"));
                cmd.Parameters.AddWithValue("@p9", GetProp(wifi, "Ipv6Address"));
                cmd.Parameters.AddWithValue("@p10", GetProp(wifi, "SignalStrength"));
                cmd.ExecuteNonQuery();
            }
        }
    }
}