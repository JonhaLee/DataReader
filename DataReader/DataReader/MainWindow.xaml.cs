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
    struct bodyInfo_Structure
    {
        public float x;
        public float y;
        public float z;
        public int state;
        public int PlayerIndex;
    }
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        String filepath = "C:\\Users\\Jonha\\Desktop\\Data\\";
        //String filepath = "C:\\Saved_Data\\Data\\";
        uint frameCount = 100;
            
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
        private void ShowBodyOnDepthImage(int img_number)
        {
            string depthfile_number_path = filepath + "Depth\\Filedepth_" + img_number.ToString() + ".bin";
            string bodyfile_path = filepath + "Body\\Fileskeleton.bin";
                      
            Console.Text = " ";

            using (BinaryReader b = new BinaryReader(File.Open(depthfile_number_path, FileMode.Open)))
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


                //뎁스 이미지 위에 스켈레톤 정보 그리는 부분
                List<bodyInfo_Structure> body = new List<bodyInfo_Structure>();
                byte[] bodyInfo = File.ReadAllBytes(bodyfile_path);

                int rowSize = sizeof(float) + sizeof(float) + sizeof(float) + sizeof(int) + sizeof(int);
                int playerSize = rowSize * 25;
                int startPoint = img_number * 6 * playerSize;

                for (int playerIndex = 0; playerIndex < 6; playerIndex++)
                {
                    int offset = startPoint + (playerIndex * playerSize);
                    int check = BitConverter.ToInt16(bodyInfo, offset + 12);
                    if (check != 9999)
                    {
                        //한 사람마다 25개의 joint가 있으므로
                        for (int jointIndex = 0; jointIndex < 25; jointIndex++)
                        {
                            float x = BitConverter.ToSingle(bodyInfo, offset + 0);
                            float y = BitConverter.ToSingle(bodyInfo, offset + 4);
                            float z = BitConverter.ToSingle(bodyInfo, offset + 8);
                            int state = BitConverter.ToInt32(bodyInfo, offset + 12);
                            //int index = BitConverter.ToInt32(bodyInfo, offset + 16);
                            //Console.Text = x.ToString() + "  " + y.ToString() + "  " + z.ToString() + "  " + state.ToString() + "  " + index.ToString();

                            if (state == 2)
                            {
                                int posX = (int)(x + 0.5f);
                                int posY = (int)(y + 0.5f);

                                for (int off_y = -3; off_y <= 3; off_y++)
                                {
                                    for (int off_x = -3; off_x <= 3; off_x++)
                                    {
                                        if((off_x + posX >= 0 && off_x + posX < 512) &&
                                            (off_y + posY >= 0 && off_y + posY < 512)){

                                                depthPixelData[(off_y + posY) * 512 + (off_x + posX)] = 0;
                                            }
                                       
                                    }
                                }
                                    
                            }
                            offset += rowSize;
                        }
                    }
                }    


                Image<Gray, Byte> img = new Image<Gray, Byte>(512, 424);
                img.Bytes = depthPixelData;
                //최종 이미지 화면에 출력
                BodyOnDepthImage.Source = BitmapSourceConvert.ToBitmapSource(img);
            } 
        }
        private void ReadMapBinary(int img_number)
        {
            string file_number_path = filepath + "Mapp\\FileMapp_" + img_number.ToString() + ".bin";
            //byte[] mappData = new byte[1024 * 424];
            byte[] depthPixelData = new byte[512 * 424];
            /*
            using (BinaryReader b = new BinaryReader(File.Open(file_number_path, FileMode.Open)))
            {
                int pos = 0;
                int length = (int)b.BaseStream.Length;

                //binary파일이 하나의 픽셀 대응점마다 1byte가 아니라 2byte씩 할당함
                //따라서 이 파일을 읽어올 때에 1byte씩 읽지 말고 2byte씩 읽어야 제대로 된 값을 읽어 올 수 있음
                int index = 0;
                while (pos < length)
                {
                    mappData[index++] = (byte)b.ReadInt16();
                    pos +=  2 * sizeof(byte);
                }
            }*/
            Image<Gray, Byte> test = new Image<Gray, Byte>(1920, 1080);
            byte[] mappData = File.ReadAllBytes(file_number_path);    
              
                   
            
           
            string depthfile_number_path = filepath + "Depth\\Filedepth_" + img_number.ToString() + ".bin";

            using (BinaryReader b = new BinaryReader(File.Open(depthfile_number_path, FileMode.Open)))
            {
                int pos = 0;
                int length = (int)b.BaseStream.Length;

                //binary파일이 하나의 픽셀 대응점마다 1byte가 아니라 2byte씩 할당함
                //따라서 이 파일을 읽어올 때에 1byte씩 읽지 말고 2byte씩 읽어야 제대로 된 값을 읽어 올 수 있음
                int index = 0;
                while (pos < length)
                {                  
                    depthPixelData[index++] = (byte)b.ReadInt16();                    
                    pos += 2 * sizeof(byte);
                }
            }
            
            Image<Gray, Byte> gray_img = new Image<Gray, Byte>(512, 424);
            gray_img.Bytes = depthPixelData;
            Image<Bgr, Byte> result_img = gray_img.Convert<Bgr, Byte>();
            
            
            string img_number_path = filepath + "Color\\KinectScreenshot_RGB" + img_number.ToString() + ".bmp";
            Image<Bgr, Byte> color_img = LoadImage(img_number_path);


            /*
            for (int row = 0; row < 424; row++)
            {
                for (int col = 0; col < 512; col++)
                {
                    Gray val = new Gray();
                    if (Gray.Equals(gray_img[row, col], val) == false)
                    {
                        int x = mappData[(row * 1024 + (col * 2)) + 1];
                        int y = mappData[(row * 1024 + (col * 2))];
              
                        if ((x > 0 && x < 1920) && (y > 0 && y < 1080))
                        {                         
                            result_img[row, col] = color_img[y, x];                            
                        }
                    }
                }
            }
            RGBinDepth.Source = BitmapSourceConvert.ToBitmapSource(result_img);
            */
        

            int rowSize = sizeof(short) + sizeof(short);       
            int offset = 0;
            for (int row = 0; row < 424; row++)
            {
                for (int col = 0; col < 512; col++)
                {
                    Gray val = new Gray();
                    if (Gray.Equals(gray_img[row, col], val) == false)
                    {
                        int y = BitConverter.ToInt16(mappData, offset + 0);
                        int x = BitConverter.ToInt16(mappData, offset + 2);

                        if ((x > 0 && x < 1920) && (y > 0 && y < 1080))
                        {
                            result_img[row, col] = color_img[y, x];
                        }
                    }
                    offset += rowSize;
                }
            }
            RGBinDepth.Source = BitmapSourceConvert.ToBitmapSource(result_img);           
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
            ShowBodyOnDepthImage(img_number);
            ReadMapBinary(img_number);
                   
        }
        private void FrameInput_Click(object sender, RoutedEventArgs e)
        {
            frameCount = uint.Parse(FrameInputField.Text);
            FrameController.Maximum = frameCount - 1;
        }
        private void CSVToBinInput_Click(object sender, RoutedEventArgs e)
        {
            string bodyfile_path = filepath + "Body\\Fileskeleton.csv";
            string bodyfile_bin_path = filepath + "Body\\Fileskeleton.bin";

            List<bodyInfo_Structure> items = new List<bodyInfo_Structure>();
            foreach (var line in File.ReadAllLines(bodyfile_path))
            {
                var parts = line.Split(',');
                items.Add(new bodyInfo_Structure
                {
                    x = float.Parse(parts[0]),
                    y = float.Parse(parts[1]),
                    z = float.Parse(parts[2]),
                    state = int.Parse(parts[3]),
                    PlayerIndex = int.Parse(parts[4]),
                });
            }

            using (var fileStream = new FileStream(bodyfile_bin_path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(fileStream))
            {
                foreach (var item in items)
                {
                    writer.Write(item.x);
                    writer.Write(item.y);
                    writer.Write(item.z);
                    writer.Write(item.state);
                    writer.Write(item.PlayerIndex);
                }
            }
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
