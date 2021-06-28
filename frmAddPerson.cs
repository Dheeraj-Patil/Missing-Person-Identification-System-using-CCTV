using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Data;

using AForge;

using AForge.Math.Geometry;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Threading;
using AForge.Imaging.Filters;

using System.IO;
using System.Net;
using MySql.Data.MySqlClient;
namespace MultiFaceRec
{
    public partial class frmAddPerson : Form
    {

        MySqlConnection con;
        private string server;
        private string database;
        private string uid;
        private string password;


        Image OrigionalImage = null;
        Image tempimg = null;
        int circlecnt = 0;
        int cnt = 0;
        Image<Bgr, Byte> currentFrame;
        HaarCascade face;
        HaarCascade eye;
        Image<Gray, byte> result;

        private VideoCaptureDevice device; //Current chosen device(camera) 
        private Dictionary<string, string> cameraDict = new Dictionary<string, string>();
        private const int CameraWidth = 320;  // constant Width
        private const int CameraHeight = 240; // constant Height
        private FilterInfoCollection cameras; //Collection of Cameras that connected to PC
        private int frameCounter = 0;
        int camWidth = 320;
        int camHeight = 240;
        int fcnt = 0;
        AForge.Video.DirectShow.FileVideoSource f;
        Bitmap srcimg = new Bitmap(256, 256);
        Image<Gray, byte>  TrainedFace = null;
     
        public frmAddPerson()
        {
            InitializeComponent();
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            eye = new HaarCascade("haarcascade_eye.xml");

            server = "198.71.225.62";
            database = "punedb";
            uid = "punedb";
            password = "123456";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            con = new MySqlConnection(connectionString);
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "jpg|*.jpg|bmp|*.bmp|png|*.png";

             DialogResult dr= ofd.ShowDialog();
             if (dr == System.Windows.Forms.DialogResult.OK)
             {
                 srcimg = new Bitmap(ofd.FileName);
                 pictureBox1.Image = Bitmap.FromFile(ofd.FileName);

             }
        }

        private void frmAddPerson_Load(object sender, EventArgs e)
        {
            this.cameras = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
            int i = 1;

            foreach (AForge.Video.DirectShow.FilterInfo camera in this.cameras)
            {
                if (!this.cameraDict.ContainsKey(camera.Name))
                    this.cameraDict.Add(camera.Name, camera.MonikerString);
                else
                {
                    this.cameraDict.Add(camera.Name + "-" + i.ToString(), camera.MonikerString);
                    i++;
                }
            }

            this.cbCamera.DataSource = new List<string>(cameraDict.Keys); //Bind camera names to combobox


        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap temp = new Bitmap(srcimg);
            currentFrame = new Image<Bgr, byte>(temp);
          
            Image<Gray, byte> gray = currentFrame.Convert<Gray, Byte>();
            //gray = currentFrame.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC).Convert<Gray, byte>();
              
            //Face Detector
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
          face,
          1.2,
          10,
          Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
          new Size(20, 20));

            int flg = 0;
            //Action for each element detected
            foreach (MCvAvgComp f in facesDetected[0])
            {
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //draw the face detected in the 0th (gray) channel with blue color
                //currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);
             //   bt = bt.Clone(f.rect, bt.PixelFormat);
             //   bt = currentFrame.Copy(f.rect).Convert<Gray, byte>().Bitmap;
                TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Bitmap bt = new Bitmap(TrainedFace.Bitmap);
          
                pictureBox2.Image = bt;

                flg = 1;
                // fcnt++;
                //bt.Save(label1.Text+"\\"+fcnt+".jpg");
            }
            
           

        }

        private void StartCapture()
        {
            try
            {
                // _capture = new Capture();
                this.device = new VideoCaptureDevice(this.cameraDict[cbCamera.SelectedItem.ToString()]);
                this.device.NewFrame += new NewFrameEventHandler(videoNewFrame);
                this.device.DesiredFrameSize = new Size(CameraWidth, CameraHeight);

                device.Start();
                //    ApplyCamSettings();
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
       
        void videoNewFrame(object sender, NewFrameEventArgs args)
        {
           
                    Bitmap temp = args.Frame.Clone() as Bitmap;
                    srcimg = new Bitmap(temp);
                 
                    currentFrame = new Image<Bgr, byte>(temp);
                    //pictureBox1.Image = temp;
                  

                    Image<Gray, byte> gray = currentFrame.Convert<Gray, Byte>();

                    //Face Detector
                    MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                  face,
                  1.2,
                  10,
                  Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                  new Size(20, 20));

                    //Action for each element detected
                    foreach (MCvAvgComp f in facesDetected[0])
                    {
                        result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        //draw the face detected in the 0th (gray) channel with blue color
                        currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);
                        Bitmap bt = new Bitmap(currentFrame.Bitmap);
                        bt = bt.Clone(f.rect, bt.PixelFormat);


                        fcnt++;
                        //bt.Save(label1.Text+"\\"+fcnt+".jpg");
                    }

                    //Image<Bgr, byte> src = new Image<Bgr, byte>(bt);


                    pictureBox1.Image = currentFrame.Bitmap;

             
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                StartCapture();
            }
            else
            {
                try
                {
                    if (device.IsRunning)
                    {
                        device.SignalToStop();
                        device.Stop();
                    }
                }
                catch { }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
                 Cursor.Current = Cursors.WaitCursor;
                 try
                 {
                     con.Open();
                     String q = "insert into tblperson values('" + txtname.Text + "',' ')";
                     MySqlCommand cmd = new MySqlCommand(q, con);
                     cmd.ExecuteNonQuery();
                     con.Close();
                    

                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show("error: " + ex);
                 }
                 finally
                 {
                     con.Close();
                 }

                Bitmap bt = new Bitmap(pictureBox2.Image);
                String fname = Application.StartupPath + "\\trainfacelocal\\" + txtname.Text + ".bmp";
                bt.Save(fname);

                FtpWebRequest requestFTPUploader = (FtpWebRequest)WebRequest.Create("ftp://ftp.emergingtech.in/" + Path.GetFileName(fname));
                requestFTPUploader.Credentials = new NetworkCredential("faces", "myface@2021");
                requestFTPUploader.Method = WebRequestMethods.Ftp.UploadFile;

                FileInfo fileInfo = new FileInfo(fname);
                FileStream  fileStream = fileInfo.OpenRead();
                int bufferLength =File.ReadAllBytes(fname).Length;
                byte[] buffer = new byte[bufferLength];

                Stream uploadStream = requestFTPUploader.GetRequestStream();
                int contentLength = fileStream.Read(buffer, 0, bufferLength);

                while (contentLength != 0)
                {
                    uploadStream.Write(buffer, 0, contentLength);
                    contentLength = fileStream.Read(buffer, 0, bufferLength);
                }

                uploadStream.Close();
                fileStream.Close();

                Cursor.Current = Cursors.Default;
               
                MessageBox.Show("Person Added");
            
        }
    }
}
