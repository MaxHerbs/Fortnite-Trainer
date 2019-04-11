using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Windows.Forms;
using Tesseract;

namespace ShootMe
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static int x = 723;
        static int y = 941;
        static int frequencyOfCheck = 100; //Milliseconds
        static string comPort = "com5";

        static SerialPort arduino = new SerialPort(comPort, 115200);
        static Bitmap final = new Bitmap(70, 35);
        static Bitmap grey = new Bitmap("GreyBackground.png");
        static int prevHealth = 100;
        static int recentRead = 100;
        static int totalChanges = 0;


        


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                arduino.Open();
            }
            catch (Exception error)
            {
                MessageBox.Show("Arduino error: \n\n" + error);
            }
            checkScreen.Interval = frequencyOfCheck;
            checkScreen.Start();
        }

        private void checkScreen_Tick(object sender, EventArgs e) //Checks the screen every however long
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
                    final.Save("RecentCapture.png");

                    string Results = GetText(final);
                    Logic(Results);
                    txtResults.Text = Results;
                }
            }
        }

        public static Bitmap OverlayImages(Bitmap image) //Overlays the recent screen capture on the grey background to make it easier for the OCR to recognise
        {
            System.Drawing.Bitmap canvas = grey;
            Graphics gra = Graphics.FromImage(canvas);
            Bitmap smallImg = new Bitmap(image);
            gra.DrawImage(smallImg, new Point(200, 100));
            return canvas;
        }

        public static string GetText(Bitmap imgsource) //Returns the text from the image
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

        public Bitmap InvertColour(Bitmap source) //Inverts the colour to make the image easier to read
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

        public static Bitmap MakeGrayscale(Bitmap original) //Greyscales image to make it easier to read
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
                string test = ocrRead.Substring(0, 1);
                recentRead = int.Parse(ocrRead);
                if (recentRead < 101 && recentRead >= 0)
                {
                    if (recentRead != prevHealth && recentRead != 100)
                    {
                        totalChanges++;
                        lblTotalChanges.Text = "Total Changes: " + totalChanges.ToString();
                        prevHealth = recentRead;
                        try
                        {
                            arduino.WriteLine("fire");
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Arduino error:\n\n" + er);
                        }
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
    }
}
