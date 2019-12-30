using System;
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
        public static TextBox tb1;
        public static TextBox tb2;
        public static TextBox tb3;
        public static TextBox tb4;
        public static TextBox tb5;


        private void Form1_Load(object sender, EventArgs e)
        {
            tb1 = textBox1;
            tb2 = textBox2;
            tb3 = textBox3;
            tb4 = textBox4;
            tb5 = textBox5;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            //TODO : all textbox (text1,2,3,4,5,) add need;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            /*
            DicomCtrl.MakeDicom(@"C:\Users\elysium1\Source\Repos\Dicom-make-to-PACS\DicomPACS_Client\testimage.png",
                @"C:\Users\elysium1\Source\Repos\Dicom-make-to-PACS");
            //add dicom path
            */
            DicomCtrl.MakeDicominFolder(textBox1.Text); //todo : target change
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DicomCtrl.SendToPACS(@"C:\Users\elysium1\Source\Repos\Dicom-make-to-PACS\Test.dcm", "OT", "192.168.0.226", 103, "VIEWREX");
            //TODO : modify ip arguments
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //not determind listview
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //directory browser
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop; // TODO : init save/load
            fbd.Description = "+++ Select Sender Folder +++";
            fbd.ShowNewFolderButton = false;
            if(fbd.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text = fbd.SelectedPath;
            }
        }
    }
}
