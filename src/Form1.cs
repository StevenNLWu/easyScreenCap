using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace easyScreenCap
{
    public partial class Form1 : Form
    {
        // DLL libraries used to manage hotkeys
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        // as a a reference for the hotkey 
        private const int MYACTION_HOTKEY_ID = 1;

        public Form1()
        {
            // Windows form
            InitializeComponent();

            // Register the hotkey 
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID, 2, (int)Keys.Space); // <- 2 = holding Ctrl

        }

        // make the Windows form always invisible
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false ? value : false);
        }

        // override the WndProc function, to handle operation system message
        protected override void WndProc(ref Message m)
        {
            try
            {
                // fire the global hotkey event - Ctrl + space 
                if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID)
                {
                    // get the screen resolution
                    Rectangle rectScreenResol = Screen.GetBounds(Point.Empty);
                    using (Bitmap bitmap = new Bitmap(rectScreenResol.Width, rectScreenResol.Height))
                    {
                        using (Graphics graphic = Graphics.FromImage(bitmap))
                        {
                            graphic.CopyFromScreen(new Point(rectScreenResol.Left, rectScreenResol.Top),
                                                  Point.Empty,
                                                  rectScreenResol.Size);
                        }

                        // determinate the file name
                        String strFolder = "output";
                        String strFileName = DateTime.Now.ToString("yyyy-MM-dd-hhmmss") + ".jpg";
                        String strFinalFileName = strFileName;
                        int intLoopName = 1;
                        while (System.IO.File.Exists(strFolder + "\\" + strFinalFileName))
                        {
                            strFinalFileName = strFileName.Replace(".jpg", String.Empty)
                                                + " "
                                                + "(" + intLoopName.ToString() + ")"
                                                + ".jpg";
                            intLoopName++;
                        }

                        // adject the image quality
                        ImageCodecInfo jgpEncoder = this.GetEncoder(ImageFormat.Jpeg);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                        myEncoderParameters.Param[0] = myEncoderParameter;

                        // save the screen capture
                        bitmap.Save(strFolder + "\\" + strFinalFileName, jgpEncoder, myEncoderParameters);
                        Console.WriteLine("Cap: " + strFinalFileName + "\n");
                    }

                }
                base.WndProc(ref m);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // Create an Encoder object based on the GUID
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
