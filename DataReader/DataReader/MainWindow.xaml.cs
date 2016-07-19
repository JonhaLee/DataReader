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
        int current_frame = 0;

        const ushort minDepth = 500;
        const short maxDepth = short.MaxValue;
        const int MapDepthToByte = 8000 / 256;

        bool isShowColorImage = true;
        bool isShowInfraredImage = true;
        bool isShowDepthImage = true;
        bool isShowDepthInColorImage = true;
        bool isShowBodyOnDepthImage = true;
        bool isShowColorInDepthImage = true;

        //color data, image
        Image<Bgr, Byte> colorImage_data;
        ImageSource colorImage_viewer;
        //infrared data, image
        Image<Bgr, Byte> infraredImage_data;
        ImageSource infraredImage_viewer;
        //depth data, image
        short[] depthData;   //(Original)
        Image<Gray, Byte> depthImage;
        ImageSource depthImage_viewer;        
        //body data, 
        List<bodyInfo_Structure> bodyData;
        //Body on Depth Image
        ImageSource bodyOnDepthImage_viewer;
        //mapp matrix
        byte[] mappData;
        //Depth in Color data, Image
        short[] HR_depthData;   //(High Resolution)
        short[] HR_depthData_bin;   //(High Resolution for save(.bin))
        //only read
        short[] HR_depthData_read;
        Image<Gray, Byte> HRdepthImage;
        ImageSource HRdepthImage_viewer;  

        ImageSource depthInColor_viewer;
        //Color in Depth image
        ImageSource colorInDepth_viewer;

        
            
        public MainWindow()
        {
            InitializeComponent();
            InitializeData();          
        }
        private void InitializeData()
        {
            FrameController.Maximum = frameCount - 1;

            depthData = new short[512 * 424];
            HR_depthData = new short[1920 * 1080];
            HR_depthData_bin = new short[3840 * 1080];
            HR_depthData_read = new short[3840 * 1080];
            bodyData = new List<bodyInfo_Structure>();    
            mappData = new byte[1024 * 424];            

            isShowColorImage = Check_ColorImageShow.IsChecked.Value;
            isShowInfraredImage = Check_InfraredImageShow.IsChecked.Value;
            isShowDepthImage = Check_DepthImageShow.IsChecked.Value;
            isShowDepthInColorImage = Check_DepthInColorImageShow.IsChecked.Value;
            isShowBodyOnDepthImage = Check_BodyOnDepthImageShow.IsChecked.Value;
            isShowColorInDepthImage = Check_ColorInDepthImageShow.IsChecked.Value;
            
        }
        private void ShowColorImage()
        {
            if (colorImage_viewer != null)
                ColorImageViewer.Source = colorImage_viewer;            
            else
                Console.Text += "Color Image is null\n";
        }
        private void ShowInfraredImage()
        {
            if (infraredImage_viewer != null)
                InfraredImageViewer.Source = infraredImage_viewer;
            else
                Console.Text += "Infrared Image is null\n"; 
        }
        private void ShowDepthImage()
        {
            if (depthImage_viewer != null)
                DepthImageViewer.Source = depthImage_viewer;
            else
                Console.Text += "depth Image is null\n";            
        }
        private void ShowDepthInColorImage()
        {
            //이미지 mapp 과정
            //DepthToHighResolution();
            //Mapp_DepthToColor();



            if (HRdepthImage_viewer != null)
                DepthInColorImageViewer.Source = HRdepthImage_viewer;
            else
                Console.Text += "depthInColor Image is null\n";  
        }
        private void ShowBodyOnDepthImage()
        {
            Mapp_BodyOnDepth();

            if (bodyOnDepthImage_viewer != null)
                BodyOnDepthImageViewer.Source = bodyOnDepthImage_viewer;
            else
                Console.Text += "Body on Depth Image is null\n";  
        }
        private void ShowColorInDepthImage()
        {
            //이미지 mapp 과정
            Mapp_ColorToDepth();

            if (colorInDepth_viewer != null)
                ColorinDepthImageViewer.Source = colorInDepth_viewer;
            else
                Console.Text += "rgbInDepth Image is null\n";   
        }
      
        private void DepthToHighResolution()
        {
            Array.Clear(HR_depthData, 0, HR_depthData.Length);
      
            int rowSize = sizeof(short) + sizeof(short);
            int offset = 0;
            for (int row = 0; row < 424; row++)
            {
                for (int col = 0; col < 512; col++)
                {
                    //Gray val = new Gray();
                    //if (Gray.Equals(depthImage[row, col], val) == false)
                    {
                        int y = BitConverter.ToInt16(mappData, offset + 0);
                        int x = BitConverter.ToInt16(mappData, offset + 2);

                        if ((x > 0 && x < 1920) && (y > 0 && y < 1080))
                        {
                            HR_depthData[y * 1920 + x] = depthData[row * 512 + col];
                            HR_depthData_bin[y * 3840 + (x * 2) + 1] = 1;
                        }
                    }
                    offset += rowSize;
                }
            }

            short[] tmp = new short[1920 * 1080];
            tmp = HR_depthData;
           // for (int i = 0; i < 3; i++)
            {
                Console.Text = "zz";
                const int masksize = 7;
                short[] arr = new short[masksize * masksize];

                for (int row = 0; row < 1080; row++)
                {
                    for (int col = 0; col < 1920; col++)
                    {
                        if (tmp[row * 1920 + col] == 0)
                        {
                            Array.Clear(arr, 0, arr.Length);

                            int index = 0;
                            for (int off_y = -masksize / 2; off_y < masksize / 2; off_y++)
                            {
                                for (int off_x = -masksize / 2; off_x < masksize / 2; off_x++)
                                {
                                    if ((off_y + row >= 0 && off_y + row < 1080) && (off_x + col >= 0 && off_x + col < 1920))
                                    {
                                        //if (HR_depthData[(row + off_y) * 1920 + col + off_x] != 0)
                                        arr[index] = tmp[(row + off_y) * 1920 + col + off_x];
                                    }
                                    index++;
                                }
                            }

                            HR_depthData[row * 1920 + col] = G_mask(arr, masksize);
                        }
                    }
                }
            }
            /*
            bool isStartPoint = false;
            bool isEndPoint = false;
            int iStartPoint = 0;
            int iEndPoint = 0;
            int repeat = 5;
            int th = 10;


            for (int count = 0; count < repeat; count++)
            {
                for (int row = 0; row < 1080; row++)
                {
                    for (int col = 0; col < 1920; col++)
                    {
                        if (HR_depthData[row * 1920 + col] > 0)
                        {
                            if (isStartPoint == false)
                            {
                                isStartPoint = true;
                                iStartPoint = row * 1920 + col;

                                for (int i = row * 1920 + col + 1; i < row * 1920 + 1920; i++)
                                {
                                    if (HR_depthData[i] > 0)
                                    {                                        
                                        isEndPoint = true;
                                        iEndPoint = i;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (isStartPoint == true && isEndPoint == true)
                            {
                                if (iEndPoint - iStartPoint < th)
                                {
                                    int index = row * 1920 + col;

                                    float total = iEndPoint - iStartPoint;

                                    float result = ((float)(iEndPoint - index) / total) * HR_depthData[iStartPoint] + ((float)(index - iStartPoint) / total) * HR_depthData[iEndPoint];
                                    HR_depthData[index] = (short)result;
                                }
                            }
                        }

                        if (iEndPoint == row * 1920 + col)
                        {
                            isStartPoint = false;
                            isEndPoint = false;
                            iStartPoint = 0;
                            iEndPoint = 0;
                        }
                    }
                    isStartPoint = false;
                    isEndPoint = false;
                    iStartPoint = 0;
                    iEndPoint = 0;
                }


                for (int col = 0; col < 1920; col++)
                {
                    for (int row = 0; row < 1080; row++)
                    {
                        if (HR_depthData[row * 1920 + col] > 0)
                        {
                            if (isStartPoint == false)
                            {
                                isStartPoint = true;
                                iStartPoint = row * 1920 + col;

                                for (int i = (row + 1) * 1920 + col; i < 1079 * 1920 + col; i += 1920)
                                {
                                    if (HR_depthData[i] > 0)
                                    {                                        
                                        isEndPoint = true;
                                        iEndPoint = i;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (isStartPoint == true && isEndPoint == true)
                            {
                                if ((iEndPoint - iStartPoint) / 1920 < th)
                                {
                                    int index = row * 1920 + col;

                                    float total = (iEndPoint - iStartPoint) / 1920;

                                    float result = ((float)((iEndPoint - index) / 1920) / total) * HR_depthData[iStartPoint] + ((float)((index - iStartPoint) / 1920) / total) * HR_depthData[iEndPoint];
                                    HR_depthData[index] = (short)result;
                                }
                            }
                        }

                        if (iEndPoint == row * 1920 + col)
                        {
                            isStartPoint = false;
                            isEndPoint = false;
                            iStartPoint = 0;
                            iEndPoint = 0;
                        }
                    }
                    isStartPoint = false;
                    isEndPoint = false;
                    iStartPoint = 0;
                    iEndPoint = 0;
                }
            }
            */

            for (int row = 0; row < 1080; row++)
            {
                for (int col = 0; col < 1920; col++)
                {
                    HR_depthData_bin[row * 3840 + (col * 2)] = HR_depthData[row * 1920 + col];
                }
            }
        }

        private short mask(short[] arr, int masksize)
        {
            int sum = 0;
            int cnt = 0;
            for (int y = 0; y < masksize; y++)
            {
                for (int x = 0; x < masksize; x++)
                {
                    if (arr[y * masksize + x] != 0)
                    {
                        sum += arr[y * masksize + x];
                        cnt++;
                    }
                }
            }

            if(cnt != 0)
                sum /= cnt;
            return (short)sum;
        }
        double[] g = {0, 0.000001, 0.000014, 0.000055, 0.000088, 0.000055, 0.000014, 0.000001, 0, 
                        0.000001, 0.000036, 0.000362, 0.001445, 0.002289, 0.001445, 0.000362, 0.000036, 0.000001, 
                        0.000014, 0.000362, 0.003672, 0.014648, 0.023205, 0.014648, 0.003672, 0.000362, 0.000014, 
                        0.000055, 0.001445, 0.014648, 0.058434, 0.092566, 0.058434, 0.014648, 0.001445, 0.000055, 
                        0.000088, 0.002289, 0.023205, 0.092566, 0.146634, 0.092566, 0.023205, 0.002289, 0.000088, 
                        0.000055, 0.001445, 0.014648, 0.058434, 0.092566, 0.058434, 0.014648, 0.001445, 0.000055, 
                        0.000014, 0.000362, 0.003672, 0.014648, 0.023205, 0.014648, 0.003672, 0.000362, 0.000014, 
                        0.000001, 0.000036, 0.000362, 0.001445, 0.002289, 0.001445, 0.000362, 0.000036, 0.000001, 
                        0, 0.000001, 0.000014, 0.000055, 0.000088, 0.000055, 0.000014, 0.000001, 0 };

        double[] g2 = {
                          0, 0.000001, 0.000002, 0.000005, 0.00001, 0.000018, 0.00003, 0.000043, 0.000057, 0.000068, 0.000071, 0.000068, 0.000057, 0.000043, 0.00003, 0.000018, 0.00001, 0.000005, 0.000002, 0.000001, 0 ,
                            0.000001, 0.000002, 0.000006, 0.000014, 0.000028, 0.000051, 0.000084, 0.000124, 0.000163, 0.000192, 0.000203, 0.000192, 0.000163, 0.000124, 0.000084, 0.000051, 0.000028, 0.000014, 0.000006, 0.000002, 0.000001, 
                            0.000002, 0.000006, 0.000015, 0.000035, 0.000071, 0.000131, 0.000215, 0.000315, 0.000415, 0.00049, 0.000518, 0.00049, 0.000415, 0.000315, 0.000215, 0.000131, 0.000071, 0.000035, 0.000015, 0.000006, 0.000002, 
                            0.000005, 0.000014, 0.000035, 0.00008, 0.000163, 0.000299, 0.00049, 0.00072, 0.000948, 0.001119, 0.001182, 0.001119, 0.000948, 0.00072, 0.00049, 0.000299, 0.000163, 0.00008, 0.000035, 0.000014, 0.000005, 
                            0.00001, 0.000028, 0.000071, 0.000163, 0.000333, 0.000611, 0.001002, 0.001473, 0.00194, 0.002288, 0.002417, 0.002288, 0.00194, 0.001473, 0.001002, 0.000611, 0.000333, 0.000163, 0.000071, 0.000028, 0.00001, 
                            0.000018, 0.000051, 0.000131, 0.000299, 0.000611, 0.001119, 0.001836, 0.002699, 0.003554, 0.004192, 0.004429, 0.004192, 0.003554, 0.002699, 0.001836, 0.001119, 0.000611, 0.000299, 0.000131, 0.000051, 0.000018, 
                            0.00003, 0.000084, 0.000215, 0.00049, 0.001002, 0.001836, 0.003013, 0.004429, 0.005833, 0.00688, 0.007269, 0.00688, 0.005833, 0.004429, 0.003013, 0.001836, 0.001002, 0.00049, 0.000215, 0.000084, 0.00003,
                            0.000043, 0.000124, 0.000315, 0.00072, 0.001473, 0.002699, 0.004429, 0.006511, 0.008574, 0.010114, 0.010686, 0.010114, 0.008574, 0.006511, 0.004429, 0.002699, 0.001473, 0.00072, 0.000315, 0.000124, 0.000043, 
                            0.000057, 0.000163, 0.000415, 0.000948, 0.00194, 0.003554, 0.005833, 0.008574, 0.011291, 0.013318, 0.014072, 0.013318, 0.011291, 0.008574, 0.005833, 0.003554, 0.00194, 0.000948, 0.000415, 0.000163, 0.000057, 
                            0.000068, 0.000192, 0.00049, 0.001119, 0.002288, 0.004192, 0.00688, 0.010114, 0.013318, 0.015709, 0.016598, 0.015709, 0.013318, 0.010114, 0.00688, 0.004192, 0.002288, 0.001119, 0.00049, 0.000192, 0.000068, 
                            0.000071, 0.000203, 0.000518, 0.001182, 0.002417, 0.004429, 0.007269, 0.010686, 0.014072, 0.016598, 0.017537, 0.016598, 0.014072, 0.010686, 0.007269, 0.004429, 0.002417, 0.001182, 0.000518, 0.000203, 0.000071, 
                            0.000068, 0.000192, 0.00049, 0.001119, 0.002288, 0.004192, 0.00688, 0.010114, 0.013318, 0.015709, 0.016598, 0.015709, 0.013318, 0.010114, 0.00688, 0.004192, 0.002288, 0.001119, 0.00049, 0.000192, 0.000068, 
                            0.000057, 0.000163, 0.000415, 0.000948, 0.00194, 0.003554, 0.005833, 0.008574, 0.011291, 0.013318, 0.014072, 0.013318, 0.011291, 0.008574, 0.005833, 0.003554, 0.00194, 0.000948, 0.000415, 0.000163, 0.000057, 
                            0.000043, 0.000124, 0.000315, 0.00072, 0.001473, 0.002699, 0.004429, 0.006511, 0.008574, 0.010114, 0.010686, 0.010114, 0.008574, 0.006511, 0.004429, 0.002699, 0.001473, 0.00072, 0.000315, 0.000124, 0.000043, 
                            0.00003, 0.000084, 0.000215, 0.00049, 0.001002, 0.001836, 0.003013, 0.004429, 0.005833, 0.00688, 0.007269, 0.00688, 0.005833, 0.004429, 0.003013, 0.001836, 0.001002, 0.00049, 0.000215, 0.000084, 0.00003, 
                            0.000018, 0.000051, 0.000131, 0.000299, 0.000611, 0.001119, 0.001836, 0.002699, 0.003554, 0.004192, 0.004429, 0.004192, 0.003554, 0.002699, 0.001836, 0.001119, 0.000611, 0.000299, 0.000131, 0.000051, 0.000018, 
                            0.00001, 0.000028, 0.000071, 0.000163, 0.000333, 0.000611, 0.001002, 0.001473, 0.00194, 0.002288, 0.002417, 0.002288, 0.00194, 0.001473, 0.001002, 0.000611, 0.000333, 0.000163, 0.000071, 0.000028, 0.00001, 
                            0.000005, 0.000014, 0.000035, 0.00008, 0.000163, 0.000299, 0.00049, 0.00072, 0.000948, 0.001119, 0.001182, 0.001119, 0.000948, 0.00072, 0.00049, 0.000299, 0.000163, 0.00008, 0.000035, 0.000014, 0.000005, 
                            0.000002, 0.000006, 0.000015, 0.000035, 0.000071, 0.000131, 0.000215, 0.000315, 0.000415, 0.00049, 0.000518, 0.00049, 0.000415, 0.000315, 0.000215, 0.000131, 0.000071, 0.000035, 0.000015, 0.000006, 0.000002,
                            0.000001, 0.000002, 0.000006, 0.000014, 0.000028, 0.000051, 0.000084, 0.000124, 0.000163, 0.000192, 0.000203, 0.000192, 0.000163, 0.000124, 0.000084, 0.000051, 0.000028, 0.000014, 0.000006, 0.000002, 0.000001, 
                            0, 0.000001, 0.000002, 0.000005, 0.00001, 0.000018, 0.00003, 0.000043, 0.000057, 0.000068, 0.000071, 0.000068, 0.000057, 0.000043, 0.00003, 0.000018, 0.00001, 0.000005, 0.000002, 0.000001, 0

                      };
        double[] g3 = {1, 1, 2, 2, 2, 1, 1,
                      1, 3, 4, 5, 4, 3, 1,
                      2, 4, 7, 8, 7, 4, 2,
                      2, 5, 8, 10, 8, 5, 2,
                      2, 4, 7, 8, 7, 4, 2,
                      1, 3, 4, 5, 4, 3, 1,
                      1, 1, 2, 2, 2, 1, 1};
        private short G_mask(short[] arr, int masksize)
        {
            double total =  0;
            double sum = 0;
            int cnt = 0;
            for (int y = 0; y < masksize; y++)
            {
                for (int x = 0; x < masksize; x++)
                {
                    if (arr[y * masksize + x] != 0)
                        total += g3[y * masksize + x];
                        //sum += (arr[y * masksize + x] * g2[y * masksize + x]) ;
                    //cnt++;                    
                }
            }

            for (int y = 0; y < masksize; y++)
            {
                for (int x = 0; x < masksize; x++)
                {
                    if (arr[y * masksize + x] != 0)
                        sum += 1.0 / total * (arr[y * masksize + x] * g3[y * masksize + x]) ;
                    //cnt++;                    
                }
            }

            //if (cnt != 0)
                //sum /= cnt;
            return (short)sum;
        }

        private void Mapp_DepthToColor()
        {
            Image<Gray, byte> depthInColor = new Image<Gray, byte>(new System.Drawing.Size(1920, 1080));
            byte[] depthImageTmp = new byte[1920 * 1080];
            //Array.Copy(HR_depthData, depthInColor.Bytes, HR_depthData.Length);
            for (int y = 0; y < 1080; y++)
                for (int x = 0; x < 1920; x++ )
                    //depthInColor[y, x] = new Gray(HR_depthData[y * 1920 + x]);// >= 0 ? (byte)HR_depthData[y * 1920 + x] : 255);
                    depthImageTmp[y * 1920 + x] = (byte)HR_depthData[y * 1920 + x];

            depthInColor.Bytes = depthImageTmp;
            depthInColor_viewer = BitmapSourceConvert.ToBitmapSource(depthInColor);  
        }
        private void Mapp_ColorToDepth()
        {            
            //grayscale to color space      
            Image<Bgr, Byte> Mapp_ColorToDepth_img = depthImage.Convert<Bgr, Byte>();

            int rowSize = sizeof(short) + sizeof(short);
            int offset = 0;
            for (int row = 0; row < 424; row++)
            {
                for (int col = 0; col < 512; col++)
                {
                    //Gray val = new Gray();                    
                    //if (Gray.Equals(depthImage[row, col], val) == false)
                    {
                        int y = BitConverter.ToInt16(mappData, offset + 0);
                        int x = BitConverter.ToInt16(mappData, offset + 2);

                        if ((x > 0 && x < 1920) && (y > 0 && y < 1080))
                        {
                            Mapp_ColorToDepth_img[row, col] = colorImage_data[y, x];
                        }
                    }
                    offset += rowSize;
                }
            }
            colorInDepth_viewer = BitmapSourceConvert.ToBitmapSource(Mapp_ColorToDepth_img);  
        }
        private void Mapp_BodyOnDepth()
        {
            byte[] background = depthImage.Bytes;
            if (bodyData.Count != 0)
            {
                for (int playerIndex = 0; playerIndex < 6; playerIndex++)
                {
                    for (int jointIndex = 0; jointIndex < 25; jointIndex++)
                    {
                        int state = bodyData[playerIndex * 25 + jointIndex].state;
                        if (state == 2)
                        {
                            int posX = (int)(bodyData[playerIndex * 25 + jointIndex].x + 0.5f);
                            int posY = (int)(bodyData[playerIndex * 25 + jointIndex].y + 0.5f);

                            //해당 joint좌표 기준 3x3의 크기로 그린다
                            for (int off_y = -3; off_y <= 3; off_y++)
                            {
                                for (int off_x = -3; off_x <= 3; off_x++)
                                {
                                    if ((off_x + posX >= 0 && off_x + posX < 512) &&
                                        (off_y + posY >= 0 && off_y + posY < 512))
                                    {

                                        background[(off_y + posY) * 512 + (off_x + posX)] = 0;
                                    }

                                }
                            }
                        }
                    }
                }
            }
            
            Image<Gray, Byte> img = new Image<Gray, Byte>(512, 424);
            img.Bytes = background;
            //최종 이미지 화면에 출력
            bodyOnDepthImage_viewer = BitmapSourceConvert.ToBitmapSource(img);
        }
      
        private Image<Bgr, Byte> LoadImage(String path)
        {           
            if (!File.Exists(path)) return null;

            Image<Bgr, Byte> img = new Image<Bgr, Byte>(path);
            return img;
        }
        private void LoadColorImage(int img_number)
        {
            String path = filepath + "Color\\KinectScreenshot_RGB" + img_number.ToString() + ".bmp";
            colorImage_data = LoadImage(path);

            if (colorImage_data == null)            
                Console.Text += "Color image Load False\n";

            colorImage_viewer = BitmapSourceConvert.ToBitmapSource(colorImage_data);  
        }
        private void LoadInfraredImage(int img_number)
        {
            String path = filepath + "Infrared\\KinectScreenshot_IR" + img_number.ToString() + ".bmp";
            infraredImage_data = LoadImage(path);

            if (infraredImage_data == null)
                Console.Text += "Infrared image Load False\n";

            infraredImage_viewer = BitmapSourceConvert.ToBitmapSource(infraredImage_data);
        }
        private void LoadDepthImage(int img_number)
        {
            string path = filepath + "Depth\\Filedepth_" + img_number.ToString() + ".bin";

            using (BinaryReader b = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                int pos = 0;
                int length = (int)b.BaseStream.Length;

                byte[] depthImageTmp = new byte[512 * 424];
                //binary파일이 하나의 픽셀 대응점마다 1byte가 아니라 2byte씩 할당함
                //따라서 이 파일을 읽어올 때에 1byte씩 읽지 말고 2byte씩 읽어야 제대로 된 값을 읽어 올 수 있음
                int index = 0;
                while (pos < length)
                {
                    depthData[index] = b.ReadInt16();                          
                    depthImageTmp[index] = (byte)(depthData[index] >= minDepth && depthData[index] <= maxDepth ? (depthData[index] / MapDepthToByte) : 0);

                    index++;
                    pos += 2 * sizeof(byte);
                }

                depthImage = new Image<Gray, Byte>(512, 424);
                depthImage.Bytes = depthImageTmp;
                depthImage_viewer = BitmapSourceConvert.ToBitmapSource(depthImage);
            }            
        }
        private void LoadHRDepthImage(int img_number)
        {
            string path = filepath + "HR_Depth\\FileHRdepth_" + 0.ToString() + ".bin";
            using (BinaryReader b = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                int pos = 0;
                int length = (int)b.BaseStream.Length;

                byte[] depthImageTmp = new byte[1920 * 1080];
                //binary파일이 하나의 픽셀 대응점마다 1byte가 아니라 2byte씩 할당함
                //따라서 이 파일을 읽어올 때에 1byte씩 읽지 말고 2byte씩 읽어야 제대로 된 값을 읽어 올 수 있음
                int index = 0;
                while (pos < length)
                {
                    HR_depthData_read[index] = b.ReadInt16();
                    if (index % 2 == 0)
                    {
                        //depthImageTmp[index / 2] = (byte)HR_depthData_read[index];
                        depthImageTmp[index / 2] = (byte)(HR_depthData_read[index] >= minDepth && HR_depthData_read[index] <= maxDepth ? (HR_depthData_read[index] / MapDepthToByte) : 0);
                    }

                    index++;
                    pos += 2 * sizeof(byte);
                }

                HRdepthImage = new Image<Gray, Byte>(1920, 1080);
                HRdepthImage.Bytes = depthImageTmp;
                HRdepthImage_viewer = BitmapSourceConvert.ToBitmapSource(HRdepthImage);
            }            
        }
        private void LoadMappMatrix(int img_number)
        {
            string path = filepath + "Mapp\\FileMapp_" + img_number.ToString() + ".bin";
            if (File.Exists(path))
                mappData = File.ReadAllBytes(path);
            else
                Console.Text += "MappMatrix can't loaded\n";
        }
        private void LoadBodyData(int img_number)
        {
            string path = filepath + "Body\\Fileskeleton.bin";
            byte[] bodyInfo;
            bodyData.Clear();
            if (File.Exists(path))
            {
                bodyInfo = File.ReadAllBytes(path);
               
                int rowSize = sizeof(float) + sizeof(float) + sizeof(float) + sizeof(int) + sizeof(int);
                int playerSize = rowSize * 25;
                int startPoint = img_number * 6 * playerSize;

                for (int playerIndex = 0; playerIndex < 6; playerIndex++)
                {
                    int offset = startPoint + (playerIndex * playerSize);                  
                    //한 사람마다 25개의 joint가 있으므로
                    for (int jointIndex = 0; jointIndex < 25; jointIndex++)
                    {
                        float _x = BitConverter.ToSingle(bodyInfo, offset + 0);
                        float _y = BitConverter.ToSingle(bodyInfo, offset + 4);
                        float _z = BitConverter.ToSingle(bodyInfo, offset + 8);
                        int _state = BitConverter.ToInt32(bodyInfo, offset + 12);
                        int _PlayerIndex = BitConverter.ToInt32(bodyInfo, offset + 16);


                        bodyData.Add(new bodyInfo_Structure
                        {
                            x = _x,
                            y = _y,
                            z = _z,
                            state = _state,
                            PlayerIndex = _PlayerIndex
                        });

                        offset += rowSize;
                    }                    
                }            
            }
            else
                Console.Text += "BodyData Can't loaded\n";
        }

        private void SaveHRDepthData()
        {
            for (int index = 0; index < 1; index++)
            {
                LoadDepthImage(index);
                LoadMappMatrix(index);
                DepthToHighResolution();

                string path = filepath + "HR_Depth\\FileHRdepth_" + index.ToString() + ".bin";

                FileStream fs = new FileStream(path, FileMode.Create);
                using (BinaryWriter bw = new BinaryWriter(fs, System.Text.Encoding.Default))
                {
                    foreach (short value in HR_depthData_bin)
                    {
                        bw.Write(value);
                    }
                }

                Console.Text = index.ToString() + ".bin complete\n";
            }

            Console.Text = "HRDepthData Save complete";
        }
        
        private void FrameController_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            current_frame = (byte)FrameController.Value;
            T_CurrentFrame.Text = (current_frame + 1).ToString() + "/" + frameCount.ToString();
            LoadColorImage(current_frame);
            LoadInfraredImage(current_frame);
            LoadDepthImage(current_frame);
            LoadHRDepthImage(current_frame);
            LoadMappMatrix(current_frame);
            LoadBodyData(current_frame);

            if (isShowColorImage)    ShowColorImage();
            if (isShowInfraredImage)   ShowInfraredImage();
            if (isShowDepthImage)   ShowDepthImage();
            if (isShowDepthInColorImage) ShowDepthInColorImage();
            if (isShowBodyOnDepthImage) ShowBodyOnDepthImage();
            if (isShowColorInDepthImage)   ShowColorInDepthImage();
            //ShowBodyOnDepthImage(img_number);
            //ReadMapBinary(img_number);
                   
        }
    
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }
        void Handle(CheckBox checkBox)
        {             
            // Use IsChecked.
            bool flag = checkBox.IsChecked.Value;

            if (checkBox.Name == "Check_ColorImageShow")            isShowColorImage = flag;
            if (checkBox.Name == "Check_InfraredImageShow")         isShowInfraredImage = flag;
            if (checkBox.Name == "Check_DepthImageShow")            isShowDepthImage = flag;
            if (checkBox.Name == "Check_DepthInColorImageShow")     isShowDepthInColorImage = flag;
            if (checkBox.Name == "Check_BodyOnDepthImageShow")      isShowBodyOnDepthImage = flag;
            if (checkBox.Name == "Check_ColorInDepthImageShow")     isShowColorInDepthImage = flag;           
        }
        private void HRDetphToBinInput_Click(object sender, RoutedEventArgs e)
        {
            SaveHRDepthData();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Handle(sender as Button);
        }
        void Handle(Button button)
        {
            if (button.Name == "PreviousFrameBtn")
            {
                if(current_frame > 0)
                    FrameController.Value = current_frame - 1;                
            }
            if (button.Name == "NextFrameBtn")
            {
                if(current_frame < frameCount)
                    FrameController.Value = current_frame + 1; 
            }
            if (button.Name == "ImageLoad")
            {
                frameCount = uint.Parse(FrameInputField.Text);
                FrameController.Maximum = frameCount - 1;
            }
           
        }

        /// <summary>
        /// 출처
        /// http://lacti.me/2014/07/09/csharp-csv-to-binary/
        /// </summary>
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
