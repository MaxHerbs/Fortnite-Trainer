using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Windows.Forms;
using Tesseract;
using System.Threading;

namespace ShootMe
{
    public partial class form1 : Form
    {
        public form1()
        {
            InitializeComponent();
        }

        static readonly SerialPort arduino = new SerialPort("com6", 9600);
        static int x = 723;
        static int y = 941;
        static int frequencyOfCheck = 100; //Milliseconds
        static Bitmap final = new Bitmap(70, 35);
        static Bitmap otherFinal = new Bitmap(70, 35);
        static Bitmap grey = new Bitmap("GreyBackground.png");
        static int prevHealth;
        static int recentHealth;
        static int totalChanges = 0;
        bool inGame = false;
        static Color green = Color.FromArgb(77, 188, 53);

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Height = 110; //165, 110
            this.Width = 165;
            gbDebug.Hide();
            CheckForIllegalCrossThreadCalls = false;
            try
            {
                arduino.Open();
            }
            catch (Exception error)
            {
                MessageBox.Show("Arduino error: \n\n" + error, "Arduino Issue", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            checkScreen.Interval = frequencyOfCheck;
            checkScreen.Start();
            restart.Stop();
        }   

        private void CheckScreen_Tick(object sender, EventArgs e) //Checks the screen every however long
        {
            using (Bitmap bmp = new Bitmap(5, 5))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(706,
                                     938,
                                     0, 0,
                                     bmp.Size,
                                     CopyPixelOperation.SourceCopy);

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            Color now_color = bmp.GetPixel(j, i);
                            if (now_color == green)
                            {
                                inGame = true;
                            }
                            else
                            {
                                inGame = false;
                                Thread.Sleep(20);
                            }
                        }
                    }
                }
            }

            if (inGame && !spectating()) 
            {
                using (Bitmap bmp = new Bitmap(45, 26)) //45, 23
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(x,
                                         y,
                                         0, 0,
                                         bmp.Size,
                                         CopyPixelOperation.SourceCopy);

                        final = MakeGrayscale(bmp);
                        final = InvertColour(final);
                        final = OverlayImages(final);
                        pbRecent.Image = final;
                        string recentRead = GetText(final);
                        try
                        {
                            if (int.Parse(recentRead) <= 100 && int.Parse(recentRead) >= 0)
                            {

                            }
                            else
                            {
                                recentRead = "0";
                            }
                        }
                        catch
                        {
                            recentRead = "0";
                        }
           
                        Logic(recentRead);
                        txtResults.Text = recentRead;
                    }
                }
            }
            switch (inGame)
            {
                case true:
                    txtResults.BackColor = Color.Green;
                    break;
                case false:
                    txtResults.BackColor = Color.Red;
                    break;
                default:
                    txtResults.BackColor = Color.Purple;
                    break;
            }
        }

        private bool spectating()
        {
            using (Bitmap bmp = new Bitmap(260, 56))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(871,
                                     113,
                                     0, 0,
                                     bmp.Size,
                                     CopyPixelOperation.SourceCopy);

                    for (int i = 0; i < 56; i++)
                    {
                        for (int j = 0; j < 260; j++)
                        {
                            Color now_color = bmp.GetPixel(j, i);
                            if (now_color == Color.FromArgb(255, 230, 103))
                            {
                                return true;
                            }

                        }
                    }
                    return false;
                }
            }
        }

        #region Functions
        public Bitmap OverlayImages(Bitmap image) //Overlays the recent screen capture on the grey background to make it easier for the OCR to recognise
        {
            System.Drawing.Bitmap canvas = grey;
            Graphics gra = Graphics.FromImage(canvas);
            Bitmap smallImg = new Bitmap(image);
            gra.DrawImage(smallImg, new Point(200, 100));
            gra.Dispose();
            smallImg.Dispose();
            return canvas;
        }

        private string GetText(Bitmap imgsource) //Returns the text from the image
        {
            var ocrtext = string.Empty;
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using (var img = PixConverter.ToPix(imgsource))
                {
                    using (var page = engine.Process(img))
                    {
                        ocrtext = page.GetText();
                        ocrtext.TrimEnd();
                    }
                }
            }
            return ocrtext;
        }

        private Bitmap InvertColour(Bitmap source) //Inverts the colour to make the image easier to read
        {
            Bitmap newBitmap = new Bitmap(source.Width, source.Height);
            Graphics g = Graphics.FromImage(newBitmap);
            ColorMatrix colorMatrix = new ColorMatrix(
            new float[][]
            {
               new float[] {-1, 0, 0, 0, 0},
               new float[] {0, -1, 0, 0, 0},
               new float[] {0, 0, -1, 0, 0},
               new float[] {0, 0, 0, 1, 0},
               new float[] {1, 1, 1, 0, 1}
            });
            colorMatrix.Matrix00 = colorMatrix.Matrix11 = colorMatrix.Matrix22 = -1f;
            colorMatrix.Matrix33 = colorMatrix.Matrix44 = 1f;
            ImageAttributes attributes = new ImageAttributes();

            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            g.Dispose();

            return newBitmap;
        }

        private Bitmap MakeGrayscale(Bitmap original) //Greyscales image to make it easier to read
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            Graphics g = Graphics.FromImage(newBitmap);
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                   new float[] {.3f, .3f, .3f, 0, 0},
                   new float[] {.59f, .59f, .59f, 0, 0},
                   new float[] {.11f, .11f, .11f, 0, 0},
                   new float[] {0, 0, 0, 1, 0},
                   new float[] {0, 0, 0, 0, 1}
               });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            g.Dispose();
            return newBitmap;
        }

        private void Logic(string ocrRead) //Logic to workout if the arduino needs to fire and changes the text on the form
        {
            try
            {
                recentHealth = int.Parse(ocrRead);
                if (recentHealth < 101 && recentHealth >= 0)
                {
                    lblCurrent.Text = recentHealth.ToString();
                    lblRecent.Text = prevHealth.ToString();
                    if (recentHealth < prevHealth)
                    {
                        totalChanges++;
                        lblTotalChanges.Text = "Changes: " + totalChanges.ToString();
                        arduino.WriteLine("fire");
                        prevHealth = recentHealth;
                    }
                    if (recentHealth >= prevHealth)
                    {
                        prevHealth = recentHealth;
                    }
                }
                else
                {

                }
            }
            catch (Exception e)
            {

            }
        }
#endregion

        #region Just buttons
        private void cmdShoot_Click(object sender, EventArgs e)
        {
            try
            {
                arduino.Open();
            }
            catch
            {
            }
            try
            {
                arduino.WriteLine("fire");
            }
            catch (Exception)
            {
                MessageBox.Show("Operation Failed");
            }
        }

        private void restart_Tick(object sender, EventArgs e)
        {

        }

        private void cmdRestart_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("ShootMe Small.exe");
            Environment.Exit(0);
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            recentHealth = 100;
            prevHealth = 100;
        }

        private void form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            arduino.Close();
        }

        private void CmdDebug_Click(object sender, EventArgs e)
        {
            this.Height = 209; //614, 209
            this.Width = 614;
            gbDebug.Show();
        }

        private void CmdBack_Click(object sender, EventArgs e)
        {
            gbDebug.Hide();
            this.Width = 165; //165, 110   
            this.Height = 110;     
        }
        #endregion
    }
}
