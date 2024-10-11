using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XML_Model_Analyzer
{
    public partial class Dictionary : Form
    {
        Form1 form1 = null;
        public Dictionary(Form1 f1)
        {
            form1 = f1;
            InitializeComponent();
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                form1.gotoIdentifier(listBox1.SelectedItem.ToString());
            }
            if (e.Button == MouseButtons.Right)
            {
                Clipboard.SetText(listBox1.SelectedItem.ToString());
            }
        }
    }
}
