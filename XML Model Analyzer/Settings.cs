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
    public partial class Settings : Form
    {
        Form1 form1;
        public Settings(Form1 f1)
        {
            InitializeComponent();
            form1 = f1;
            try
            {
                label2.Text = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (Exception ex)
            {
                label2.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            this.ActiveControl = button2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Accepted
            form1.accepted = true;
            form1.setKey(checkBox1.Checked);
            Close();
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
