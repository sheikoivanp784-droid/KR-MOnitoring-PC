using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace КР_Мониторінг_ста // проверь, чтобы namespace совпадал с твоим проектом
{
    public class Save
    {
        // Передаем форму (Form targetForm) как параметр
        public static void SaveFullSystemReport(Form targetForm)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Title = "Зберегти повний звіт про стан системи";
                saveDialog.Filter = "Текстовий документ (*.txt)|*.txt|Усі файли (*.*)|*.*";
                saveDialog.FileName = $"Повний_звіт_ПК_{DateTime.Now:yyyy-MM-dd_HH-mm}.txt";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("============================================================");
                    sb.AppendLine("           ПОВНИЙ ЗВІТ ПРО МОНІТОРИНГ СТАНУ ПК");
                    sb.AppendLine($"           Дата та час: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                    sb.AppendLine($"           Користувач / ПК: {Environment.UserName} / {Environment.MachineName}");
                    sb.AppendLine($"           ОС: {Environment.OSVersion}");
                    sb.AppendLine("============================================================");
                    sb.AppendLine();

                    if (targetForm != null)
                    {
                        // Шукаємо об'єкти моніторингу всередині переданої форми
                        FieldInfo[] formFields = targetForm.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (FieldInfo f in formFields)
                        {
                            string typeName = f.FieldType.Name;
                            if (typeName == "CpuMon" || typeName == "RamMon" || typeName == "GpuMon" || typeName == "DiskMon" || typeName == "WifiMon")
                            {
                                object monitorInstance = f.GetValue(targetForm);
                                AppendAllObjectData(sb, typeName.ToUpper(), monitorInstance);
                            }
                        }

                        // Збираємо всі Label з переданої форми
                        sb.AppendLine("[ ПОТОЧНІ ПОКАЗНИКИ З ЕКРАНУ ]");
                        foreach (Control ctrl in targetForm.Controls)
                        {
                            CollectLabels(ctrl, sb);
                        }
                    }

                    sb.AppendLine("============================================================");

                    try
                    {
                        File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Повний звіт успішно збережено!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка під час збереження файлу:\n{ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static void AppendAllObjectData(StringBuilder sb, string sectionTitle, object monitorObj)
        {
            sb.AppendLine($"[ {sectionTitle} ]");

            if (monitorObj == null)
            {
                sb.AppendLine("Дані відсутні (об'єкт не ініціалізовано)");
                sb.AppendLine();
                return;
            }

            Type type = monitorObj.GetType();

            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    object val = prop.GetValue(monitorObj);
                    FormatValue(sb, prop.Name, val);
                }
                catch { }
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.Name.Contains("k__BackingField")) continue;

                try
                {
                    object val = field.GetValue(monitorObj);
                    FormatValue(sb, field.Name, val);
                }
                catch { }
            }

            sb.AppendLine();
        }

        private static void FormatValue(StringBuilder sb, string name, object val)
        {
            if (val == null)
            {
                sb.AppendLine($"{name}: Немає даних");
            }
            else if (val is System.Collections.IEnumerable && !(val is string))
            {
                sb.AppendLine($"{name}:");
                int i = 1;
                foreach (var item in (System.Collections.IEnumerable)val)
                {
                    sb.AppendLine($"   {i}. {item}");
                    i++;
                }
            }
            else
            {
                sb.AppendLine($"{name}: {val}");
            }
        }

        private static void CollectLabels(Control parent, StringBuilder sb)
        {
            Label lbl = parent as Label;
            if (lbl != null && !string.IsNullOrWhiteSpace(lbl.Text))
            {
                sb.AppendLine($"{lbl.Name}: {lbl.Text}");
            }

            foreach (Control child in parent.Controls)
            {
                CollectLabels(child, sb);
            }
        }
    }
}