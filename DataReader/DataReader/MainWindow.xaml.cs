using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
//using Emgu.Util;
using System.Runtime.InteropServices;

using System.IO;

namespace DataReader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        String filepath = "C:\\Users\\Jonha\\Desktop\\Data\\";
        uint frameCount = 100;
        
        List<ImageSource> images;

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();          
        }
        private void InitializeData()
        {
            FrameController.Maximum = frameCount - 1;
        }
        private void ShowColorImage(int img_number)
        {
            string img_number_path = filepath + "Color\\KinectScreenshot_RGB" + img_number.ToString() + ".bmp";
            Image<Bgr, Byte> img = LoadImage(img_number_path);
            ColorImage.Source = BitmapSourceConvert.ToBitmapSource(img);     
        }
        private void ShowInfraredImage(int img_number)
        {
            string img_number_path = filepath + "Infrared\\KinectScreenshot_IR" + img_number.ToString() + ".bmp";
            Image<Bgr, Byte> img = LoadImage(img_number_path);
            InfraredImage.Source = BitmapSourceConvert.ToBitmapSource(img); 
        }
        private void ShowDepthImage(int img_number)
        {
            string file_number_path = filepath + "Depth\\Filedepth_" + img_number.ToString() + ".bin";

            Console.Text = " ";
            using (BinaryReader b = new BinaryReader(File.Open(file_number_path, FileMode.Open)))
            {
                int pos = 0;
                int length = (int)b.BaseStream.Length;

                byte[] depthPixelData = new byte[512 * 424];


                //binary파일이 하나의 픽셀 대응점마다 1byte가 아니라 2byte씩 할당함
                //따라서 이 파일을 읽어올 때에 1byte씩 읽지 말고 2byte씩 읽어야 제대로 된 값을 읽어 올 수 있음
                   
                int index = 0;
                while (pos < length)
                {
                    depthPixelData[index++] = (byte)b.ReadInt16();               
                    
                    pos += 2 * sizeof(byte);
                }
              
                Image<Gray, Byte> img = new Image<Gray, Byte>(512, 424);
                img.Bytes = depthPixelData;
                DepthImage.Source = BitmapSourceConvert.ToBitmapSource(img);
            }            
        }
        private Image<Bgr, Byte> LoadImage(String path)
        {            
            Image<Bgr, Byte> img = new Image<Bgr, Byte>(path);
            return img;
        }
        private void FrameController_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int img_number = (byte)FrameController.Value;
            ShowColorImage(img_number);
            ShowInfraredImage(img_number);
            ShowDepthImage(img_number);
                   
        }
        private void FrameInput_Click(object sender, RoutedEventArgs e)
        {
            frameCount = uint.Parse(FrameInputField.Text);
            FrameController.Maximum = frameCount - 1;
        }
    }

    
    /// <summary>
    /// 출처
    /// http://www.emgu.com/wiki/index.php/WPF_in_CSharp
    /// </summary>
    public static class BitmapSourceConvert
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }
    }
}
