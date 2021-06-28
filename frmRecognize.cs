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
using System.IO;
using System.IO.Ports;
using System.Data.SqlClient;
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
    public partial class frmRecognize : Form
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

        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null;
        int flag = 0;
        String s1 = "";

        public frmRecognize()
        {
            InitializeComponent();
            //Load haarcascades for face detection
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
        public void Gui(String msg)
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new MethodInvoker(delegate { label1.Text = msg; }));
            }
        }

        private void frmRecognize_Load(object sender, EventArgs e)
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
            try
            {
                //Load of previus trainned faces and labels for each image
                String[] files = Directory.GetFiles(Application.StartupPath + "\\trainfacelocal");
                NumLabels = files.Length;
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 0; tf < NumLabels; tf++)
                {
                    LoadFaces = files[tf];
                    trainingImages.Add(new Image<Gray, byte>(LoadFaces));
                    labels.Add(Path.GetFileNameWithoutExtension(LoadFaces));
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(e.ToString());
                MessageBox.Show("Nothing in binary database, please add at least a face", "Triained faces load", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            try
            {
                try
                {
                    NamePersons.Clear();
                }
                catch { }

                Bitmap temp = args.Frame.Clone() as Bitmap;
                currentFrame = new Image<Bgr, byte>(temp);
               
                // Bitmap bt = new Bitmap(temp.Clone() as Bitmap);
                pictureBox1.Image = temp;

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
                    t = t + 1;
                    result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    //draw the face detected in the 0th (gray) channel with blue color
                    currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);
                   // Image<Bgr, byte> currentFrame1 = new Image<Bgr, byte>(currentFrame.Bitmap);
               
                    pictureBox3.Image = currentFrame.Bitmap;
                    if (trainingImages.ToArray().Length != 0)
                    {
                        //TermCriteria for face recognition with numbers of trained images like maxIteration
                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                        //Eigen face recognizer
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                           trainingImages.ToArray(),
                           labels.ToArray(),
                           1800,
                           ref termCrit);

                        name = recognizer.Recognize(result);
                        if (name != "" && flag == 0)
                        {
                            flag = 1;

                        }
                        if (name != "")
                        {
                            NamePersons.Add(name);
                            //Draw the label for each face detected and recognized
                            currentFrame.Draw(name, ref font, new System.Drawing.Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                            pictureBox3.Image = currentFrame.Bitmap;
                        }
                    }

                    
                    //Set the number of faces detected on the scene
                   // label3.Text = facesDetected[0].Length.ToString();


                }
                t = 0;

                //Names concatenation of persons recognized
                names = "";
                for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                {
                    names = names + NamePersons[nnn] + ", ";
                    
                }
                if (NamePersons.Count > 0)
                {
                    Gui( names + " detected at location Nashik");
                }
                //Show the faces procesed and recognized
                pictureBox3.Image = currentFrame.Bitmap;
              //  label4.Text = names;
                names = "";
                //Clear the list(vector) of names
                NamePersons.Clear();

                //Image<Bgr, byte> src = new Image<Bgr, byte>(bt);


              
            }
            catch(Exception ex) { 
            
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartCapture();
        }

        
        private void frmRecognize_FormClosing(object sender, FormClosingEventArgs e)
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

        private void button5_Click(object sender, EventArgs e)
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

        private void button6_Click(object sender, EventArgs e)
        {
          
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            List<string> files = new List<string>();
                con.Open();
                String q = "select * from tblperson";
                DataTable dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(q, con);
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    String name = dt.Rows[i]["personname"].ToString();

                    files.Add(name+".bmp");
                }
                con.Close();


                for (int i = 0; i < files.Count; i++)
                {
                    string fname = files[i];

                    FtpWebRequest requestFTPUploader = (FtpWebRequest)WebRequest.Create("ftp://ftp.emergingtech.in/" + Path.GetFileName(fname));
                    requestFTPUploader.Credentials = new NetworkCredential("faces", "myface@2021");
                    String ls = WebRequestMethods.Ftp.ListDirectory;
                    requestFTPUploader.Method = WebRequestMethods.Ftp.DownloadFile;
                    FtpWebResponse response = (FtpWebResponse)requestFTPUploader.GetResponse();

                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);
                    string path = Application.StartupPath + "\\trainfacelocal\\" + fname;

                    using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        responseStream.CopyTo(fileStream);
                    }
                }
            MessageBox.Show("Files sucessfully downloaded");
            try
            {
                //Load of previus trainned faces and labels for each image
                String[] files1 = Directory.GetFiles(Application.StartupPath + "\\trainfacelocal");
                NumLabels = files1.Length;
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 0; tf < NumLabels; tf++)
                {
                    LoadFaces = files1[tf];
                    trainingImages.Add(new Image<Gray, byte>(LoadFaces));
                    labels.Add(Path.GetFileNameWithoutExtension(LoadFaces));
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(e.ToString());
                MessageBox.Show("Nothing in binary database, please add at least a face", "Triained faces load", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }


            Cursor.Current = Cursors.Default;
        }

    }
}
