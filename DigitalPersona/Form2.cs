using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;

namespace DigitalPersona
{


    public partial class Form2 : Form, DPFP.Capture.EventHandler
    {
        Funciones f = new Funciones();
        public int PROBABILITY_ONE = 0x7FFFFFFF;
        bool registrationInProgress = false;
        System.Drawing.Graphics graphics;
        System.Drawing.Font font;
        DPFP.Capture.ReadersCollection readers;
        DPFP.Capture.ReaderDescription readerDescription;
        DPFP.Capture.Capture capturer;
        DPFP.Template template;
        DPFP.FeatureSet[] regFeatures;
        DPFP.FeatureSet verFeatures;
        DPFP.Processing.Enrollment createRegTemplate;
        DPFP.Verification.Verification verify;
        DPFP.Capture.SampleConversion converter;
        public Form2()
        {
            InitializeComponent();
            graphics = this.CreateGraphics();
            font = new Font("Times New Roman", 12, FontStyle.Bold,GraphicsUnit.Pixel);
            DPFP.Capture.ReadersCollection coll = new
            DPFP.Capture.ReadersCollection();
            regFeatures = new DPFP.FeatureSet[4];
            for (int i = 0; i < 4; i++)
                regFeatures[i] = new DPFP.FeatureSet();
            verFeatures = new DPFP.FeatureSet();
            createRegTemplate = new DPFP.Processing.Enrollment();
            readers = new DPFP.Capture.ReadersCollection();
            for (int i = 0; i < readers.Count; i++)
            {
                readerDescription = readers[i];
                if ((readerDescription.Vendor == "Digital Persona, Inc.") ||
               (readerDescription.Vendor == "DigitalPersona, Inc."))
                {
                    try
                    {
                        capturer = new
                       DPFP.Capture.Capture(readerDescription.SerialNumber,
                       DPFP.Capture.Priority.Normal);//CREAMOS UNA OPERACION DE CAPTURAS.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    capturer.EventHandler = this; //AQUI CAPTURAMOS LOS EVENTOS.
                    converter = new DPFP.Capture.SampleConversion();
                    try
                    {
                        verify = new DPFP.Verification.Verification();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ex: " + ex.ToString());
                    }
                    break;
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            registrationInProgress = true;
            createRegTemplate.Clear();
            if (capturer != null)
            {
                try
                {
                    capturer.StartCapture();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        public void OnComplete(object obj, string info, DPFP.Sample sample)
        {
            this.Invoke(new Functionz(delegate ()
            {
                textBox1.Text = "Captura Completa";
            }));
            this.Invoke(new Functionz(delegate ()
            {
                Bitmap tempRef = null;
                converter.ConvertToPicture(sample, ref tempRef);
                System.Drawing.Image img = tempRef;
        
                 Bitmap bmp = new Bitmap(converter.ConvertToPicture(sample, ref
                tempRef), pbImage.Size);
                String pxFormat = bmp.PixelFormat.ToString();
                Point txtLoc = new Point(pbImage.Width / 2 - 20, 0);
                graphics = Graphics.FromImage(bmp);

            if (registrationInProgress)
                {
                    try
                    {

                        regFeatures[0] = ExtractFeatures(sample,
                        DPFP.Processing.DataPurpose.Enrollment);
                        regFeatures[1] = ExtractFeatures(sample,
                       DPFP.Processing.DataPurpose.Enrollment);
                        regFeatures[2] = ExtractFeatures(sample,
                       DPFP.Processing.DataPurpose.Enrollment);
                        regFeatures[3] = ExtractFeatures(sample,
                       DPFP.Processing.DataPurpose.Enrollment);
                        if (regFeatures[0] != null)
                        {
                            string b64 =
                           Convert.ToBase64String(regFeatures[0].Bytes);

                            regFeatures[0].DeSerialize(Convert.FromBase64String(b64));
                            if (regFeatures[0] == null)
                            {
                                txtLoc.X = pbImage.Width / 2 - 26;
                                graphics.DrawString("Error", font, Brushes.Cyan,
                               txtLoc);
                                return;
                            }
                            createRegTemplate.AddFeatures(regFeatures[0]);
                            createRegTemplate.AddFeatures(regFeatures[1]);
                            createRegTemplate.AddFeatures(regFeatures[2]);
                            createRegTemplate.AddFeatures(regFeatures[3]);
                            graphics = Graphics.FromImage(bmp);
                            graphics.DrawString("" + 0 + " De 4", font,
                            Brushes.Black, txtLoc);
                            if (createRegTemplate.TemplateStatus ==
                           DPFP.Processing.Enrollment.Status.Failed)
                            {
                                capturer.StopCapture();
                                MessageBox.Show("Error en la captura");
                            }
                            else
                            if (createRegTemplate.TemplateStatus ==
                           DPFP.Processing.Enrollment.Status.Ready)
                            {
                                string mensaje = "";
                                MemoryStream x = new MemoryStream();
                                MemoryStream mem = new MemoryStream();
                                template = createRegTemplate.Template;
                                template.Serialize(mem);
                                verFeatures = ExtractFeatures(sample,
                               DPFP.Processing.DataPurpose.Verification);
                                mensaje = comparar(verFeatures);
                                if (mensaje == "Huella registrada")
                                {
                                    MessageBox.Show(mensaje, "Información",
                                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                                else
                                {
                                    textBox1.Text = "";
                                    MessageBox.Show("Huella no registrada",
                                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                    catch (DPFP.Error.SDKException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                pbImage.Image = bmp;
            }));
        }
        private string comparar(DPFP.FeatureSet features)
        {
            string mensaje = "";
            try
            {
                f.conexion.Open();
                SqlCommand y = new SqlCommand("select Huella, Nombres from Huellas where huella is not null", f.conexion);
               
                SqlDataReader select;
                select = y.ExecuteReader();
                byte[] tx = null;
                int cont = 0;
                DPFP.Verification.Verification.Result resulta = new
               DPFP.Verification.Verification.Result();
                while (select.Read())
                {
                    tx = (byte[])select.GetValue(0);
                    DPFP.Template templates = new DPFP.Template();
                    templates.DeSerialize((byte[])tx);
                    verify.Verify(features, templates, ref resulta);
                    if (resulta.Verified)
                    {
                        mensaje = "Huella registrada";
                        textBox1.Text = select.GetValue(1).ToString();
                        cont++;
                        break;
                    }
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message + "...");
            }
            f.conexion.Close();
            return mensaje;
        }
        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            this.Invoke(new Functionz(delegate ()
            {
                textBox1.Text = "Huella leída";
            }));
        }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            this.Invoke(new Functionz(delegate ()
            {
                textBox1.Text = "Leyendo huella";
            }));
        }
        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            this.Invoke(new Functionz(delegate ()
            {
                textBox1.Text = "Lector Conectado";
            }));
        }
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            this.Invoke(new Functionz(delegate ()
            {
                textBox1.Text = "Lector Desconectado"; MessageBox.Show("readercount: " + readers.Count);
            }));
        }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber,
       DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            MessageBox.Show("Calidad de la muestra!!!! " +
           CaptureFeedback.ToString());
        }
        protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample,
       DPFP.Processing.DataPurpose Purpose)
        {
            DPFP.Processing.FeatureExtraction Extractor = new
           DPFP.Processing.FeatureExtraction(); // Create a feature extractor
            DPFP.Capture.CaptureFeedback feedback =
           DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            try
            {
                Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref
               features);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            capturer.StopCapture();
        }

    }
}
