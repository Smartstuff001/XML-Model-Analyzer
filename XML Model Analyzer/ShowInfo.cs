using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace XML_Model_Analyzer
{
    public partial class ShowInfo : Form
    {
        Form1 form1;
        public ShowInfo(Form1 f1)
        {
            InitializeComponent();
            form1 = f1;
            this.ActiveControl = button2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                form1.setHintNoShow();
            }
            Close();
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
