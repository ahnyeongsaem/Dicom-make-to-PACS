using System;
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

            DicomPixelData pixelData = DicomPixelData.Create(dataset, true);

            pixelData.BitsStored = 8;
            //pixelData.BitsAllocated = 8; //Todo : 이거 바꿔야할듯함.
            pixelData.SamplesPerPixel = 3; // 3 : red/green/blue  1 : CT/MR Single Grey Scale
            pixelData.HighBit = 7;
            pixelData.PixelRepresentation = 0;
            pixelData.PlanarConfiguration = 0;

            pixelData.AddFrame(buffer);

            DicomFile dicomfile = new DicomFile(dataset);

            //string TargetFile = Path.Combine(TargetPath, sopInstanceUID + ".dcm");
            string TargetFile = Path.Combine(TargetPath, "Test.dcm");

            dicomfile.Save(TargetFile);

            return TargetFile;
        }

        private static void FillDataset(DicomDataset dataset)
        {
            dataset.Add(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
            dataset.Add(DicomTag.StudyInstanceUID, GenerateUid());
            dataset.Add(DicomTag.SeriesInstanceUID, GenerateUid());
            dataset.Add(DicomTag.SOPInstanceUID, GenerateUid());

            dataset.Add(DicomTag.PatientID, "790830");
            dataset.Add(DicomTag.PatientName, "ParkSinHye");
            dataset.Add(DicomTag.PatientBirthDate, "19790830");
            dataset.Add(DicomTag.PatientSex, "F");
            dataset.Add(DicomTag.PatientAge, "21");
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
            dataset.Add(DicomTag.PatientOrientation, "F/A");
            dataset.Add(DicomTag.ImageLaterality, "U");

            dataset.Add(DicomTag.ContentDate, DateTime.Now);
            dataset.Add(DicomTag.ContentTime, DateTime.Now);
            dataset.Add(DicomTag.InstanceNumber, "1");
            dataset.Add(DicomTag.ConversionType, "a");
        }

        private static DicomUID GenerateUid()
        {
            StringBuilder uid = new StringBuilder();
            uid.Append("1.08.1982.10121984.2.0.07").Append('.').Append(DateTime.UtcNow.Ticks);
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


