using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace КР_Мониторінг_ста
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void btnMon_Click(object sender, EventArgs e)
        {
            Monitoring monitoringForm = new Monitoring();
            monitoringForm.ShowDialog();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            Settings monitoringForm = new Settings();
            monitoringForm.ShowDialog();
        }
    }
}
