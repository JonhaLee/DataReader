﻿using System;
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

namespace DataReader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        String filepath = "C:\\Users\\Jonha\\Desktop\\Data\\";


        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
            //Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(filepath + "Color\\KinectScreenshot_RGB0.bmp");
            //Color.Source = BitmapSourceConvert.ToBitmapSource(img1); 
        }
        private void InitializeData()
        {

        }
        private void FrameController_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            uint img_number = (byte)FrameController.Value;
            string img_number_path = filepath + "Color\\KinectScreenshot_RGB" + img_number.ToString() + ".bmp";
            Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(img_number_path);
            //Color.Source = BitmapSourceConvert.ToBitmapSource(img1); 
        }
        private void FrameInput_Click(object sender, RoutedEventArgs e)
        {
            uint frameCount = uint.Parse(FrameInputField.Text);

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                string img_number_path = filepath + "Color\\KinectScreenshot_RGB" + frameIndex.ToString() + ".bmp";
            }

                this.Title = "Clicked";
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
