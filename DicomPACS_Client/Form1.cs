using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;

namespace DicomPACS_Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
           InitializeComponent();
        }
        public static TextBox tb1; //path
        public static TextBox tb2; //sourceAET
        public static TextBox tb3; //targetIP
        public static TextBox tb4; //targetPort
        public static TextBox tb5; //targetAET
        public static ListBox lb1; //log listbox

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        //TODO : add Timer Need

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.Items.Add("Form Loading Complete[" + DateTime.Now + "]");

            tb1 = textBox1;
            tb2 = textBox2;
            tb3 = textBox3;
            tb4 = textBox4;
            tb5 = textBox5;
            lb1 = listBox1;

            StringBuilder stmp = new StringBuilder();
            GetPrivateProfileString("LoadParameter", "PATH", "", stmp, stmp.MaxCapacity, ".\\LoadSetting.ini");
            tb1.Text = stmp.ToString();
            stmp.Clear();
            GetPrivateProfileString("LoadParameter", "SOURCE_AET", "", stmp, stmp.Capacity, ".\\LoadSetting.ini");
            tb2.Text = stmp.ToString();
            stmp.Clear();
            GetPrivateProfileString("LoadParameter", "TARGET_IP", "", stmp, stmp.Capacity, ".\\LoadSetting.ini");
            tb3.Text = stmp.ToString();
            stmp.Clear();
            GetPrivateProfileString("LoadParameter", "TARGET_PORT", "", stmp, stmp.Capacity, ".\\LoadSetting.ini");
            tb4.Text = stmp.ToString();
            stmp.Clear();
            GetPrivateProfileString("LoadParameter", "TARGET_AET", "", stmp, stmp.Capacity, ".\\LoadSetting.ini");
            tb5.Text = stmp.ToString();
            stmp.Clear();

            listBox1.Items.Add("User Setting Loading Complete[" + DateTime.Now+"]");
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {   
            WritePrivateProfileString("LoadParameter", "PATH", tb1.Text, ".\\LoadSetting.ini");
            WritePrivateProfileString("LoadParameter", "SOURCE_AET", tb2.Text, ".\\LoadSetting.ini");
            WritePrivateProfileString("LoadParameter", "TARGET_IP", tb3.Text, ".\\LoadSetting.ini");
            WritePrivateProfileString("LoadParameter", "TARGET_PORT", tb4.Text, ".\\LoadSetting.ini");
            WritePrivateProfileString("LoadParameter", "TARGET_AET", tb5.Text, ".\\LoadSetting.ini");

           
            //TODO : all textbox (text1,2,3,4,5,) add need;
            
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            /*
            DicomCtrl.MakeDicom(@"C:\Users\elysium1\Source\Repos\Dicom-make-to-PACS\DicomPACS_Client\testimage.png",
                @"C:\Users\elysium1\Source\Repos\Dicom-make-to-PACS");
            //add dicom path
            */
            DicomCtrl.MakeDicominFolder(textBox1.Text); //todo : target change
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //not determind listview
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Show();
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(checkBox1.Checked==true)
            {
                DicomCtrl.MakeDicominFolder(textBox1.Text);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {//QUIT MENU
            Application.Exit();
            
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DicomCtrl.MakeEachDicominFolder(textBox1.Text);
        }
    }
}
