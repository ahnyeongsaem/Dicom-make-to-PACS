﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DicomPACS_Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
           InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DicomCtrl.MakeDicom(@"C:\Users\elysium1\Source\Repos\Dicom-make-to-PACS\DicomPACS_Client\testimage.png", "path");
        }
    }
}
