﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

using Dicom;
using Dicom.Imaging;
using Dicom.IO.Buffer;
using Dicom.Network;
namespace DicomPACS_Client
{
    static class DicomCtrl
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }


        public static bool DcmPrefixValidate(FileStream Dcmfs) //dcm to image remove
        {
            Dcmfs.Seek(128, SeekOrigin.Begin); // Premble 128 bytes. and Prefix 'D','I','C','M' 4 bytes.
            return (Dcmfs.ReadByte() == (byte)'D' &&
                    Dcmfs.ReadByte() == (byte)'I' &&
                    Dcmfs.ReadByte() == (byte)'C' &&
                    Dcmfs.ReadByte() == (byte)'M');
        }

        public static string DcmStringCheck(string Dcmfile)
        {
            var dcm = DicomFile.Open(Dcmfile);
            string pID = dcm.Dataset.Get<string>(DicomTag.PatientID);
            string pName = dcm.Dataset.Get<string>(DicomTag.PatientName);
            return pName;
        }

        /// <summary>
        /// kernel32 import for ini file input/output
        /// </summary>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        

        /// <summary>
        /// TODO : 이미지 폴더를 통째로 dicom 파일로 만드는것(ini까지 포함해서)
        /// 를 만들어야함. //tomorrow commit need
        /// </summary>
        /// <param name="ImageFileFolder"></param>
        /// <param name="TargetPath"></param>
        /// <returns></returns>
        public static void MakeDicominFolder(string ImageFileFolder, string TargetPath)//TODO: need target path change
        {
            List<string> dirs = new List<string>(Directory.EnumerateDirectories(ImageFileFolder));
            //all dirs find

            StringBuilder PATIENT_ID = new StringBuilder();
            StringBuilder PATIENT_NAME = new StringBuilder();
            StringBuilder PATIENT_SEX = new StringBuilder();
            StringBuilder PATIENT_BOD = new StringBuilder();
            StringBuilder STUDY_DATE = new StringBuilder();
            StringBuilder STUDY_TIME = new StringBuilder();
            StringBuilder STUDY_DESC = new StringBuilder();
            StringBuilder ACCESSION_NO = new StringBuilder();
            StringBuilder ORDER_CODE = new StringBuilder();
            StringBuilder FILE_CNT = new StringBuilder();
            StringBuilder REQUEST = new StringBuilder();
            StringBuilder SEND_RESULT = new StringBuilder();

            foreach (string dir in dirs)
            {
                
                //Example : GetPrivateProfileString("WookoaSetting", "TopAlways", "", topAlways, topAlways.Capacity, "C:\\Setting.ini");
                //Example : WritePrivateProfileString("WookoaSetting", "ViewTray", "false", "C:\\Setting.ini");
                //not need dirs name
                GetPrivateProfileString("INFO", "PATIENT_ID", "", PATIENT_ID, PATIENT_ID.Capacity, dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "PATIENT_NAME", "", PATIENT_NAME, PATIENT_NAME.Capacity, dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "PATIENT_SEX", "", PATIENT_SEX, PATIENT_SEX.Capacity,  dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "PATIENT_BOD", "", PATIENT_BOD, PATIENT_BOD.Capacity,  dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "STUDY_DATE", "", STUDY_DATE, STUDY_DATE.Capacity, dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "STUDY_TIME", "", STUDY_TIME, STUDY_TIME.Capacity,  dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "STUDY_DESC", "", STUDY_DESC, STUDY_DESC.Capacity, dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "ACCESSION_NO", "", ACCESSION_NO, ACCESSION_NO.Capacity,  dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "ORDER_CODE", "", ORDER_CODE, ORDER_CODE.Capacity, dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "FILE_CNT", "", FILE_CNT, FILE_CNT.Capacity, dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "REQUEST", "", REQUEST, REQUEST.Capacity, dir + @"\Setting.ini");
                GetPrivateProfileString("INFO", "SEND_RESULT", "", SEND_RESULT, SEND_RESULT.Capacity, dir + @"\Setting.ini");

                Console.Out.WriteLine(dir);

                List<string> imgFiles = new List<string>(Directory.EnumerateFiles(dir));

                DicomDataset dataset = new DicomDataset();
                foreach (string imgfile in imgFiles)
                {
                    if(string.Compare(imgfile.Substring(imgfile.Length-3,3),"png")!=0)
                    {
                        continue;
                    }

                    Bitmap bitmap = new Bitmap(imgfile);
                    bitmap = GetValidImage(bitmap);

                    int rows, columns;
                    byte[] pixels = GetPixels(bitmap, out rows, out columns);

                    MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);

                    
                    FillDataset(dataset);

                    dataset.Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);
                    dataset.Add(DicomTag.Rows, (ushort)rows);
                    dataset.Add(DicomTag.Columns, (ushort)columns);

                    DicomPixelData pixelData = DicomPixelData.Create(dataset, true); //TODO : bug fix dicompixeldata create

                    pixelData.BitsStored = 8;
                    pixelData.SamplesPerPixel = 3; // 3 : red/green/blue  1 : CT/MR Single Grey Scale
                    pixelData.HighBit = 7;
                    pixelData.PixelRepresentation = 0;
                    pixelData.PlanarConfiguration = 0;

                    pixelData.AddFrame(buffer);
                    //TODO : Need to check if it is created dcm in directory
                    
                }
                
                DicomFile dicomfile = new DicomFile(dataset);

                //string TargetFile = Path.Combine(TargetPath, sopInstanceUID + ".dcm");
                string TargetFile = Path.Combine(TargetPath, "Test.dcm");

                dicomfile.Save(TargetFile); //todo : dicom file save error


            }

        }



        public static string MakeDicom(string ImageFile, string TargetPath)
        {
            Bitmap bitmap = new Bitmap(ImageFile);
            bitmap = GetValidImage(bitmap);

            int rows, columns;
            byte[] pixels = GetPixels(bitmap, out rows, out columns);

            MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);

            DicomDataset dataset = new DicomDataset();
            FillDataset(dataset);

            dataset.Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);
            dataset.Add(DicomTag.Rows, (ushort)rows);
            dataset.Add(DicomTag.Columns, (ushort)columns);

            DicomPixelData pixelData = DicomPixelData.Create(dataset, true); //TODO : bug fix dicompixeldata create

            pixelData.BitsStored = 8;
            pixelData.SamplesPerPixel = 3; // 3 : red/green/blue  1 : CT/MR Single Grey Scale
            pixelData.HighBit = 7;
            pixelData.PixelRepresentation = 0;
            pixelData.PlanarConfiguration = 0;

            pixelData.AddFrame(buffer);
            pixelData.AddFrame(buffer); //TODO : 두개가 들어가는지 테스트

            DicomFile dicomfile = new DicomFile(dataset);

            //string TargetFile = Path.Combine(TargetPath, sopInstanceUID + ".dcm");
            string TargetFile = Path.Combine(TargetPath, "Test.dcm");

            dicomfile.Save(TargetFile); //todo : dicom file save error

            return TargetFile;
        }

        private static void FillDataset(DicomDataset dataset)
        {
            dataset.Add(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
            dataset.Add(DicomTag.StudyInstanceUID, GenerateUid());  //스터디는 촬영 부위
            dataset.Add(DicomTag.SeriesInstanceUID, GenerateUid()); //예 : 이미지 10장을 묶는것 시리즈 상위 그룹이 있는듯?
            dataset.Add(DicomTag.SOPInstanceUID, GenerateUid());

            dataset.Add(DicomTag.BitsAllocated, "8");//add bit allocate but pixeldata delete

            dataset.Add(DicomTag.PatientID, "790830");

            dataset.Add(DicomTag.SpecificCharacterSet, "ISO 2022 IR 149");

            dataset.Add(DicomTag.PatientName, "안영샘"); 
            dataset.Add(DicomTag.PatientBirthDate, "1990726");
            dataset.Add(DicomTag.PatientSex, "M");
            /// A string of characters with one of the following formats
            /// -- nnnD, nnnW, nnnM, nnnY; where nnn shall contain the number of days for D, weeks for W, months for M, or years for Y.
            ///Example: "018M" would represent an age of 18 months.
            dataset.Add(DicomTag.PatientAge,"024Y"); 
            
            dataset.Add(DicomTag.StudyDate, DateTime.Now);
            dataset.Add(DicomTag.StudyTime, DateTime.Now);
            dataset.Add(DicomTag.AccessionNumber, string.Empty);
            dataset.Add(DicomTag.ReferringPhysicianName, string.Empty);
            dataset.Add(DicomTag.StudyID, "1");
            dataset.Add(DicomTag.SeriesNumber, "1");
            dataset.Add(DicomTag.ModalitiesInStudy, "CR");
            dataset.Add(DicomTag.Modality, "CR");
            dataset.Add(DicomTag.NumberOfStudyRelatedInstances, "1");
            dataset.Add(DicomTag.NumberOfStudyRelatedSeries, "1");
            dataset.Add(DicomTag.NumberOfSeriesRelatedInstances, "1");
            dataset.Add(DicomTag.PatientOrientation, @"F\A"); //Patient direction of the rows and columns of the image 
            dataset.Add(DicomTag.ImageLaterality, "U");

            dataset.Add(DicomTag.ContentDate, DateTime.Now); 
            dataset.Add(DicomTag.ContentTime, DateTime.Now); 
            dataset.Add(DicomTag.InstanceNumber, "1");
            dataset.Add(DicomTag.ConversionType, "WSD"); //Describes the kind of image conversion.
        }

        private static DicomUID GenerateUid()
        {
            StringBuilder uid = new StringBuilder();
            //uid.Append("1.08.1982.10121984.2.0.07").Append('.').Append(DateTime.UtcNow.Ticks); //original
            uid.Append("1.2.840.10008").Append('.').Append(DateTime.UtcNow.Ticks); //change


            return new DicomUID(uid.ToString(), "SOP Instance UID", DicomUidType.SOPInstance);
        }

        private static Bitmap GetValidImage(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {   
                Bitmap old = bitmap;
                using (old)
                {
                    bitmap = new Bitmap(old.Width, old.Height, PixelFormat.Format24bppRgb);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(old, 0, 0, old.Width, old.Height);
                    }
                }
            }
            return bitmap;
        }

        private static byte[] GetPixels(Bitmap image, out int rows, out int columns)
        {
            rows = image.Height;
            columns = image.Width;
            if (rows % 2 != 0 && columns % 2 != 0) --columns;

            BitmapData data = image.LockBits(new Rectangle(0, 0, columns, rows), ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr bmpData = data.Scan0;

            try
            {
                int stride = columns * 3;
                int size = rows * stride;
                byte[] pixelData = new byte[size];
                for (int i = 0; i < rows; ++i) Marshal.Copy(new IntPtr(bmpData.ToInt64() + i * data.Stride), pixelData, i * stride, stride);

                SwapRedBlue(pixelData);
                return pixelData;
            }
            finally
            {
                image.UnlockBits(data);
            }
        }
        private static void SwapRedBlue(byte[] pixel)
        {
            for (int i = 0; i < pixel.Length; i += 3)
            {
                byte temp = pixel[i];
                pixel[i] = pixel[i + 2];
                pixel[i + 2] = temp;
            }
        }

        //need button or code send to pacs
        public static void SendToPACS(string dcmfile, string sourceAET, string targetIP, int targetPort, string targetAET)
        {
            var m_pDicomFile = DicomFile.Open(dcmfile);

            DicomClient pClient = new DicomClient();

            pClient.NegotiateAsyncOps();
            pClient.AddRequest(new DicomCStoreRequest(m_pDicomFile, DicomPriority.Medium));
            pClient.Send(targetIP, targetPort, false, sourceAET, targetAET);
            

        }
       

    }





}


