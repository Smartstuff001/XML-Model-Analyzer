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
    public partial class Sort : Form
    {
        Form1 form1;
        public Sort(Form1 f1)
        {
            InitializeComponent();
            checkedListBox1.SetItemChecked(0, true);
            form1 = f1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int nrchecked = 0;
            for (int ix = 0; ix < checkedListBox1.Items.Count; ++ix)
            {
                if (checkedListBox1.GetItemChecked(ix))
                {
                    nrchecked = ix;
                }
            }

            if (nrchecked != 1)
            {
                form1.sortElementsByAttribute(comboBox2.Text, comboBox1.Text, nrchecked);
            }
            // sort attribute within elements
            if (nrchecked == 1)
            {
                form1.sortAttributesWithinElements();
            }

            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            if (e.NewValue == CheckState.Checked)
            {
                for (int ix = 0; ix < checkedListBox1.Items.Count; ++ix)
                {
                    if (ix != e.Index) checkedListBox1.SetItemChecked(ix, false);
                }
                if (e.Index == 0)
                {
                    comboBox1.Enabled = true;
                }
                if (e.Index == 3)
                {
                    comboBox2.Enabled = true;
                }
            }
        }
    }
}
