﻿using System;
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
    public partial class Log : Form
    {
        public Log(string text)
        {
            InitializeComponent();
            textBox1.Text = text;
        }
    }
}
