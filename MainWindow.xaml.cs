using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
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
using Microsoft.Win32;
//Output.Text = Convert.ToString(count);
namespace Wpf
{
    public partial class MainWindow : Window{
        private string _filename;
        private string filename;
        private string _filename_1;
        private string filename_1 = "";
        private int threshold;
        private int click_num = 0;
        private double[] click_points = new double[8];
        private Bitmap img;
        private Bitmap img1;
        private Bitmap img_1;
        private Bitmap img_2;
        private Bitmap img_3;
        private Bitmap img_4;

        public MainWindow(){
            InitializeComponent();
        }
        private void Result(Bitmap img, short num){
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();

            MemoryStream tmp = new MemoryStream(bytes);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = tmp;
            bitmapImage.EndInit();

            switch (num){
                case 1:
                    Show_1.Source = bitmapImage;
                    break;
                case 2:
                    Show_2.Source = bitmapImage;
                    break;
                case 3:
                    Show_3.Source = bitmapImage;
                    break;
                case 4:
                    Show_4.Source = bitmapImage;
                    break;
            }
        }
        private void List_Click(object sender, RoutedEventArgs e){
            System.Windows.Forms.OpenFileDialog fdlg = new System.Windows.Forms.OpenFileDialog();
            fdlg.Title = "Open Bmp File";
            fdlg.InitialDirectory = "d:\\";
            fdlg.Filter = "All files (*.*)|*.*"; 
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;

            if (fdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                filename = fdlg.FileName;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filename);
                bitmap.EndInit();
                Pic.Source = bitmap;
                BitmapImage none = new BitmapImage();
                Show_1.Source = none;
                Show_2.Source = none;
                Show_3.Source = none;
                Show_4.Source = none;
                Pic1.Source = none;
                click_num = 0;
            }
        }
        private void Close_pic(){
            BitmapImage none = new BitmapImage();
            Show_1.Source = none;
            Show_2.Source = none;
            Show_3.Source = none;
            Show_4.Source = none;
        }
        private void Get_Extract_RGB(Bitmap img){
            for (int x = 0; x < img.Width; x++){
                for (int y = 0; y < img.Height; y++){
                    System.Drawing.Color pixel = img.GetPixel(x, y);
                    img_1.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel.A, pixel.R, pixel.R, pixel.R));
                    img_2.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel.A, pixel.G, pixel.G, pixel.G));
                    img_3.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel.A, pixel.B, pixel.B, pixel.B));
                }
            }
        }
        private void Extract_RGB_Gray(object sender, RoutedEventArgs e){
            Close_pic();
            _filename = filename;
            try{
                Bitmap _img = new Bitmap(_filename);
                img = _img;
            }
            catch{
                return;
            }
            img_1 = new Bitmap(img.Width, img.Height);
            img_2 = new Bitmap(img.Width, img.Height);
            img_3 = new Bitmap(img.Width, img.Height);

            // single R, G, B image
            Get_Extract_RGB(img);

            // gray image
            img_4 = new Bitmap(img.Width, img.Height);
            for (int x = 0; x < img.Width; x++){ 
                for (int y = 0; y < img.Height; y++){
                    System.Drawing.Color pixel = img.GetPixel(x, y);
                    int avg = (pixel.R + pixel.G + pixel.B) / 3;
                    img_4.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel.A, avg, avg, avg));
                }
            }
            img_1.Save("./R_channel.png", ImageFormat.Png);
            img_2.Save("./G_channel.png", ImageFormat.Png);
            img_3.Save("./B_channel.png", ImageFormat.Png);
            img_4.Save("./gray.png", ImageFormat.Png);
            Result(img_1, 1);
            Result(img_2, 2);
            Result(img_3, 3);
            Result(img_4, 4);
        }
        private Bitmap Convolution(Bitmap img, int[] filter, int filter_size, bool smooth = false){
            Bitmap _img = new Bitmap(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++) {
                for (int x = 0; x < img.Width; x++){
                    int offset_y_begin = y - (filter_size - 1) / 2;
                    offset_y_begin = offset_y_begin > 0 ? offset_y_begin : 0;
                    int offset_y_end = y + (filter_size - 1) / 2;
                    offset_y_end = offset_y_end < img.Height ? offset_y_end + 1 : img.Height;
                    offset_y_end -= 1;

                    int offset_x_begin = x - (filter_size - 1) / 2;
                    offset_x_begin = offset_x_begin > 0 ? offset_x_begin : 0;

                    int offset_x_end = x + (filter_size - 1) / 2;
                    offset_x_end = offset_x_end < img.Width ? offset_x_end + 1 : img.Width;
                    offset_x_end -= 1;

                    int count = 0;
                    int val_R = 0;
                    int val_G = 0;
                    int val_B = 0;

                    for (int offset_y = offset_y_begin; offset_y <= offset_y_end; offset_y++){
                        for (int offset_x = offset_x_begin; offset_x <= offset_x_end; offset_x++){
                            int ky = offset_y - y + (filter_size - 1) / 2;
                            int kx = offset_x - x + (filter_size - 1) / 2;
                            count += 1;
                            System.Drawing.Color pixel = img.GetPixel(offset_x, offset_y);
                            int filter_val = filter[(filter_size * filter_size) - ((ky * filter_size) + kx) - 1];
                            val_R += pixel.R * filter_val;
                            val_G += pixel.G * filter_val;
                            val_B += pixel.B * filter_val;
                        }
                    }
                    if (smooth){
                        val_R /= count;
                        val_G /= count;
                        val_B /= count;
                        _img.SetPixel(x, y, System.Drawing.Color.FromArgb(Math.Abs(val_R), Math.Abs(val_G), Math.Abs(val_B)));                      
                    }
                    else{
                        threshold = 40;
                        _img.SetPixel(x, y, System.Drawing.Color.FromArgb(Math.Abs(val_R) >= threshold ? 255 : 0, Math.Abs(val_G) >= threshold ? 255 : 0, Math.Abs(val_B) >= threshold ? 255 : 0));
                        //_img.SetPixel(x, y, System.Drawing.Color.FromArgb(Math.Abs(Convert.ToInt32(val_R*0.3)), Math.Abs(Convert.ToInt32(val_G * 0.3)), Math.Abs(Convert.ToInt32(val_B * 0.3))));
                        //_img.SetPixel(x, y, System.Drawing.Color.FromArgb(Math.Abs(val_R), Math.Abs(val_G), Math.Abs(val_B)));
                    }       
                }            
            }
            return _img;
        }

        private void Smooth_filter(object sender, RoutedEventArgs e){
            Close_pic();
            _filename = filename;
            try{
                Bitmap _img = new Bitmap(_filename);
                img = _img;
            }
            catch{
                return;
            }

            img_1 = new Bitmap(img.Width, img.Height);
            img_2 = new Bitmap(img.Width, img.Height);

            //mean
            int[] filter = new int[9];
            for (int i = 0; i < 9; i++){
                filter[i] = 1;
            }
            img_1 = Convolution(img, filter, 3,true);

            //median
            for (int y = 0; y < img.Height; y++){
                for (int x = 0; x < img.Width; x++){
                    int count = 0;
                    int[] R_arr = new int[9];
                    int[] G_arr = new int[9];
                    int[] B_arr = new int[9];

                    if (y - 1 >= 0){
                        if (x - 1 >= 0){
                            R_arr[count] = img.GetPixel(x - 1, y - 1).R;
                            G_arr[count] = img.GetPixel(x - 1, y - 1).G;
                            B_arr[count] = img.GetPixel(x - 1, y - 1).B;
                            count += 1;
                        }
                        R_arr[count] = img.GetPixel(x, y - 1).R;
                        G_arr[count] = img.GetPixel(x, y - 1).G;
                        B_arr[count] = img.GetPixel(x, y - 1).B;
                        count += 1;

                        if (x + 1 < img.Width){
                            R_arr[count] = img.GetPixel(x + 1, y - 1).R;
                            G_arr[count] = img.GetPixel(x + 1, y - 1).G;
                            B_arr[count] = img.GetPixel(x + 1, y - 1).B;
                            count += 1;
                        }
                    }
                    if (x - 1 >= 0){
                        R_arr[count] = img.GetPixel(x - 1, y).R;
                        G_arr[count] = img.GetPixel(x - 1, y).G;
                        B_arr[count] = img.GetPixel(x - 1, y).B;
                        count += 1;
                    }

                    R_arr[count] = img.GetPixel(x, y).R;
                    G_arr[count] = img.GetPixel(x, y).G;
                    B_arr[count] = img.GetPixel(x, y).B;
                    count += 1;

                    if (x + 1 < img.Width){
                        R_arr[count] = img.GetPixel(x + 1, y).R;
                        G_arr[count] = img.GetPixel(x + 1, y).G;
                        B_arr[count] = img.GetPixel(x + 1, y).B;
                        count += 1;
                    }
                    if (y + 1 < img.Height){
                        if (x - 1 >= 0){
                            R_arr[count] = img.GetPixel(x - 1, y + 1).R;
                            G_arr[count] = img.GetPixel(x - 1, y + 1).G;
                            B_arr[count] = img.GetPixel(x - 1, y + 1).B;
                            count += 1;
                        }
                        R_arr[count] = img.GetPixel(x, y + 1).R;
                        G_arr[count] = img.GetPixel(x, y + 1).G;
                        B_arr[count] = img.GetPixel(x, y + 1).B;
                        count += 1;

                        if (x + 1 < img.Width){
                            R_arr[count] = img.GetPixel(x + 1, y + 1).R;
                            G_arr[count] = img.GetPixel(x + 1, y + 1).G;
                            B_arr[count] = img.GetPixel(x + 1, y + 1).B;
                            count += 1;
                        }
                    }
                    Array.Sort(R_arr);
                    Array.Sort(G_arr);
                    Array.Sort(B_arr);           
                    img_2.SetPixel(x, y, System.Drawing.Color.FromArgb(R_arr[count / 2], G_arr[count / 2], B_arr[count / 2]));
                }
            }
            Result(img_1, 1);
            Result(img_2, 2);
            img_1.Save("./mean.png", ImageFormat.Png);
            img_2.Save("./median.png", ImageFormat.Png);
        }

        private void Histogram_equalization(object sender, RoutedEventArgs e){
            Close_pic();
            _filename = filename;
            try{
                Bitmap _img = new Bitmap(_filename);
                img = _img;
            }
            catch{
                return;
            }

            img_1 = new Bitmap(img.Width, img.Height);

            //historgram
            int[,] frequency = new int[3, 256];
            int[,] mapping = new int[3, 256];
            int all_num = img.Width * img.Height;
            int interval = all_num / 256;

            for (int y = 0; y < img.Height; y++){
                for (int x = 0; x < img.Width; x++){
                    frequency[0, img.GetPixel(x, y).R] += 1;
                    frequency[1, img.GetPixel(x, y).G] += 1;
                    frequency[2, img.GetPixel(x, y).B] += 1;
                }
            }
            //Plot_histogram();
            int accumulate_R = 0;
            int accumulate_G = 0;
            int accumulate_B = 0;
            int j_R = 0;
            int j_G = 0;
            int j_B = 0;
            for (int i = 0; i < 256; i++){
                mapping[0, i] = j_R;
                mapping[1, i] = j_G;
                mapping[2, i] = j_B;
                accumulate_R += frequency[0, i];
                accumulate_G += frequency[1, i];
                accumulate_B += frequency[2, i];
                while (accumulate_R > interval * (j_R + 1)){
                    j_R += 1;
                }
                while (accumulate_G > interval * (j_G + 1)){
                    j_G += 1;
                }
                while (accumulate_B > interval * (j_B + 1)){
                    j_B += 1;
                }
            }
            for (int y = 0; y < img.Height; y++){
                for (int x = 0; x < img.Width; x++){
                    img_1.SetPixel(x, y, System.Drawing.Color.FromArgb(mapping[0, img.GetPixel(x, y).R], mapping[1, img.GetPixel(x, y).G], mapping[2, img.GetPixel(x, y).B]));
                }
            }
            img_1.Save("./equalization.png", ImageFormat.Png);
            Result(img_1, 1);
        }

        private void Threshold(object sender, RoutedEventArgs e){
            Close_pic();
            _filename = filename;
            try{
                Bitmap _img = new Bitmap(_filename);
                img = _img;
            }
            catch{
                return;
            }

            img_1 = new Bitmap(img.Width, img.Height);
            // threshold
            for (int y = 0; y < img.Height; y++){
                for (int x = 0; x < img.Width; x++){
                    img_1.SetPixel(x, y, System.Drawing.Color.FromArgb((img.GetPixel(x, y).R >= threshold) ? 255 : 0, (img.GetPixel(x, y).G >= threshold) ? 255 : 0, (img.GetPixel(x, y).B >= threshold) ? 255 : 0));
                }      
            }
            img_1.Save("./threshold.png", ImageFormat.Png);
            Result(img_1, 1);
        }

        private void Input_Text(object sender, TextChangedEventArgs e){
            threshold = Int32.Parse(Input.Text);
        }

        private void Sobel_edge(object sender, RoutedEventArgs e){
            Close_pic();
            _filename = filename;
            try{
                Bitmap _img = new Bitmap(_filename);
                img = _img;
            }
            catch {
                return;
            }

            img_1 = new Bitmap(img.Width, img.Height);
            img_2 = new Bitmap(img.Width, img.Height);
            img_3 = new Bitmap(img.Width, img.Height);

            //vertical
            int[] filter_1 = new int[9] { 1, 0, -1, 1, 0, -1, 1, 0, -1 };
            img_1 = Convolution(img, filter_1, 3);
            //horizon
            int[] filter_2 = new int[9] { 1, 1, 1, 0, 0, 0, -1, -1, -1 };
            img_2 = Convolution(img, filter_2, 3);

            Result(img_1, 1);
            Result(img_2, 2);
            img_1.Save("./vertical.png", ImageFormat.Png);
            img_2.Save("./horizon.png", ImageFormat.Png);

            //combined
            for (int y = 0; y < img.Height; y++){
                for (int x = 0; x < img.Width; x++){
                    int R = Convert.ToInt32(Math.Pow((Math.Pow(img_1.GetPixel(x, y).R, 2) * 0.5 + Math.Pow(img_2.GetPixel(x, y).R, 2)), 0.5) * 0.5);
                    int G = Convert.ToInt32(Math.Pow((Math.Pow(img_1.GetPixel(x, y).G, 2) * 0.5 + Math.Pow(img_2.GetPixel(x, y).G, 2)), 0.5) * 0.5);
                    int B = Convert.ToInt32(Math.Pow((Math.Pow(img_1.GetPixel(x, y).B, 2) * 0.5 + Math.Pow(img_2.GetPixel(x, y).B, 2)), 0.5) * 0.5);
                    img_3.SetPixel(x, y, System.Drawing.Color.FromArgb(R, G, B));
                }
            }
            Result(img_3, 3);
            img_3.Save("./combined.png", ImageFormat.Png);
        }

        private void Overlap(object sender, RoutedEventArgs e){
            Close_pic();
            if (filename_1 == ""){
                Close_pic();
                System.Windows.Forms.OpenFileDialog fdlg = new System.Windows.Forms.OpenFileDialog();
                fdlg.Title = "Open Bmp File";
                fdlg.InitialDirectory = "d:\\";
                fdlg.Filter = "All files (*.*)|*.*";
                fdlg.FilterIndex = 2;
                fdlg.RestoreDirectory = true;

                if (fdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                    filename_1 = fdlg.FileName;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filename_1);
                    bitmap.EndInit();
                    Pic1.Source = bitmap;
                }
            }
            _filename = filename_1;
            try{
                Bitmap _img = new Bitmap(_filename);
                Bitmap _img1 = new Bitmap("./threshold.png");
                img = _img;
                img1 = _img1;
            }
            catch{
                return;
            }
            img_1 = new Bitmap(img.Width, img.Height);

            //overlap
            for (int y = 0; y < img.Height; y++){
                for (int x = 0; x < img.Width; x++){
                    if(img1.GetPixel(x, y).R != 0 || img1.GetPixel(x, y).G != 0 || img1.GetPixel(x, y).B != 0){
                        img_1.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 255, 0));
                    }
                    else{
                        img_1.SetPixel(x, y, System.Drawing.Color.FromArgb(img.GetPixel(x, y).R, img.GetPixel(x, y).G, img.GetPixel(x, y).B));
                    }
                }
            }
            Result(img1, 1);
            Result(img_1, 2);
            img_1.Save("./overlap.png", ImageFormat.Png);
            filename_1 = "";
        }
        private void recursion(int x, int y, int[,] img_flag,int R, int G, int B){
            img_1.SetPixel(x, y, System.Drawing.Color.FromArgb(R, G, B));
            img_flag[x, y] = 1;
            if (x + 1 < img.Width && img.GetPixel(x + 1, y).R != 255 && img.GetPixel(x + 1, y).G != 255 && img.GetPixel(x + 1, y).B != 255 && img_flag[x + 1, y] == 0){
                recursion(x + 1, y, img_flag, R, G, B);
            }
            if (x - 1 >= 0 && img.GetPixel(x - 1, y).R != 255 && img.GetPixel(x - 1, y).G != 255 && img.GetPixel(x - 1, y).B != 255 && img_flag[x - 1, y] == 0){
                recursion(x - 1, y, img_flag, R, G, B);
            }
            if (y + 1 < img.Height && img.GetPixel(x, y + 1).R != 255 && img.GetPixel(x, y + 1).G != 255 && img.GetPixel(x, y + 1).B != 255 && img_flag[x, y + 1] == 0){
                recursion(x, y + 1, img_flag, R, G, B);
            }
            if (y - 1 >= 0 && img.GetPixel(x, y - 1).R != 255 && img.GetPixel(x, y - 1).G != 255 && img.GetPixel(x, y - 1).B != 255 && img_flag[x, y - 1] == 0){
                recursion(x, y - 1, img_flag, R, G, B);
            }
            if (x + 1 < img.Width && y + 1 < img.Height && img.GetPixel(x + 1, y + 1).R != 255 && img.GetPixel(x + 1, y + 1).G != 255 && img.GetPixel(x + 1, y + 1).B != 255 && img_flag[x + 1, y + 1] == 0){
                recursion(x + 1 , y + 1, img_flag, R, G, B);
            }
            if (x + 1 < img.Width && y - 1 >= 0 && img.GetPixel(x + 1, y - 1).R != 255 && img.GetPixel(x + 1, y - 1).G != 255 && img.GetPixel(x + 1, y - 1).B != 255 && img_flag[x + 1, y - 1] == 0){
                recursion(x + 1, y - 1, img_flag, R, G, B);
            }
            if (x - 1 >= 0 && y + 1 < img.Height && img.GetPixel(x - 1, y + 1).R != 255 && img.GetPixel(x - 1, y + 1).G != 255 && img.GetPixel(x - 1, y + 1).B != 255 && img_flag[x - 1, y + 1] == 0){
                recursion(x - 1, y + 1, img_flag, R, G, B);
            }
            if (x - 1 >= 0 && y - 1 >= 0 && img.GetPixel(x - 1, y - 1).R != 255 && img.GetPixel(x - 1, y - 1).G != 255 && img.GetPixel(x - 1, y - 1).B != 255 && img_flag[x - 1, y - 1] == 0){
                recursion(x - 1, y - 1, img_flag, R, G, B);
            }
        }
        private void Connected(object sender, RoutedEventArgs e){
            Close_pic();
            _filename = filename;
            try{
                Bitmap _img = new Bitmap(_filename);
                img = _img;
            }
            catch{
                return;
            }
            img_1 = new Bitmap(img.Width, img.Height);
            int[,] img_flag = new int[img.Width, img.Height];

            //connected
            Random rnd = new Random();
            for (int y = 0; y < img.Height; y++){
                for (int x = 0; x < img.Width; x++){
                    if (img.GetPixel(x, y).R != 255 && img.GetPixel(x, y).G != 255 && img.GetPixel(x, y).B != 255  && img_flag[x, y] == 0){
                        int R = rnd.Next(0, 200);
                        int G = rnd.Next(0, 200);
                        int B = rnd.Next(0, 200);
                        recursion(x, y, img_flag, R, G, B);
                    }
                    else{
                        if(img_flag[x, y] == 0){
                            img_1.SetPixel(x, y, System.Drawing.Color.FromArgb(255, 255, 255));
                        }                      
                    }                    
                }
            }            
            Result(img_1, 1);
            img_1.Save("./connected.png", ImageFormat.Png);          
        }
        private double[] CalcVec(double[] click_points){
            double[] calcvec = new double[2];
            calcvec[0] = click_points[2] - click_points[0];
            calcvec[1] = click_points[3] - click_points[1];
            return calcvec;
        }
        private double[] CalcVec_matching(double[] click_points){
            double[] calcvec = new double[2];
            calcvec[0] = click_points[6] - click_points[4];
            calcvec[1] = click_points[7] - click_points[5];
            return calcvec;
        }
        private double[] Calc_Translate(double[] click_points){
            double[] calcvec = new double[2];
            calcvec[0] = click_points[4] - click_points[0];
            calcvec[1] = click_points[5] - click_points[1];
            return calcvec;
        }
        private double Calc_Scale(double[] vec){
            return Math.Pow(Math.Pow(vec[0], 2) + Math.Pow(vec[1], 2), 0.5);
        }
        private double[] Rotation_vector(double[] vec, double rad){
            double[] calcvec = new double[2];
            calcvec[0] = Math.Cos(rad) * vec[0] - Math.Sin(rad) * vec[1];
            calcvec[1] = Math.Sin(rad) * vec[0] + Math.Cos(rad) * vec[1];
            return calcvec;
        }
        private double Calc_Radian(double[] vec_1, double[] vec_2){
            double scale_1 = Calc_Scale(vec_1);
            double scale_2 = Calc_Scale(vec_2);
            double rad = Math.Acos((vec_1[0] * vec_2[0] + vec_1[1] * vec_2[1]) / (scale_1 * scale_2));
            double[] anticlockwise = Rotation_vector(vec_1, rad);
            double[] clockwise = Rotation_vector(vec_1, -rad);
            double anticlockwiseErr = Math.Pow(anticlockwise[0] - vec_2[0], 2) + Math.Pow(anticlockwise[1] - vec_2[1], 2);
            double clockwiseErr = Math.Pow(clockwise[0] - vec_2[0], 2) + Math.Pow(clockwise[1] - vec_2[1], 2);
            if(anticlockwiseErr > clockwiseErr){
                rad = -rad;
            }
            return rad;
        }
        private double intensity_difference(Bitmap img1, Bitmap img_1){
            double diff = 0;
            for (int y = 0; y < img1.Height; y++){
                for (int x = 0; x < img1.Width; x++){
                    diff += (Math.Abs(img1.GetPixel(x, y).R - img_1.GetPixel(x, y).R) + Math.Abs(img1.GetPixel(x, y).G - img_1.GetPixel(x, y).G) + Math.Abs(img1.GetPixel(x, y).B - img_1.GetPixel(x, y).B));
                }
            }
            return diff / (img1.Height * img1.Width * 3);
        }
        private void Registeration(object sender, RoutedEventArgs e){
            if (filename_1 == ""){
                Close_pic();
                System.Windows.Forms.OpenFileDialog fdlg = new System.Windows.Forms.OpenFileDialog();
                fdlg.Title = "Open Bmp File";
                fdlg.InitialDirectory = "d:\\";
                fdlg.Filter = "All files (*.*)|*.*";
                fdlg.FilterIndex = 2;
                fdlg.RestoreDirectory = true;
         
                if (fdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                    filename_1 = fdlg.FileName;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filename_1);
                    bitmap.EndInit();
                    Pic1.Source = bitmap;
                }
            }

            _filename = filename;
            _filename_1 = filename_1;
            try{
                Bitmap _img = new Bitmap(_filename);
                img = _img;
                Bitmap _img_1 = new Bitmap(_filename_1);
                img1 = _img_1;
            }
            catch{
                return;
            }
            if(click_num != 8){
                return;
            }

            img_1 = new Bitmap(img1.Width, img1.Height);

            //registration
            double[] vec_1 = CalcVec(click_points);
            double[] vec_2 = CalcVec_matching(click_points);
            double[] translate = Calc_Translate(click_points);
            double final_rad = Calc_Radian(vec_1, vec_2);
            double final_scale = Calc_Scale(vec_2) / Calc_Scale(vec_1);
            double[] offset = new double[2];
            offset[0] = click_points[0];
            offset[1] = click_points[1];
            for (int y = 0; y < img1.Height; y++){
                for (int x = 0; x < img1.Width; x++){
                    double[] tmp = new double[2];
                    tmp[0] = (x - translate[0] - offset[0]) / final_scale;
                    tmp[1] = (y - translate[1] - offset[1]) / final_scale;
                    double[] rotxy = Rotation_vector(tmp, -final_rad);
                    int _x = Convert.ToInt32(Math.Round(rotxy[0] + offset[0]));
                    int _y = Convert.ToInt32(Math.Round(rotxy[1] + offset[1]));
                    if(_x > img.Width-1 || _x < 0 || _y > img.Height - 1 || _y < 0){
                        img_1.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 0, 0));
                    }
                    else{
                        img_1.SetPixel(x, y, System.Drawing.Color.FromArgb(img.GetPixel(_x, _y).R, img.GetPixel(_x, _y).G, img.GetPixel(_x, _y).B));
                    }
                }
            }
            double diff = intensity_difference(img1, img_1);
            Output.Text = "translate: " + Convert.ToString(translate[0]) + ',' +Convert.ToString(translate[1]) + '\n' + "scale: " + Convert.ToString(final_scale)+ '\n' + "rad: " + Convert.ToString(final_rad * 57) + '\n' + "diff: " + Convert.ToString(diff);
            img_1.Save("./registration.png", ImageFormat.Png);
            Result(img_1, 1);
            click_num = 0;
            filename_1 = "";
        }
        //obey click rule: click first pic four times followed by second pic
        private void MouseLeftButton_Click(object sender, MouseButtonEventArgs e){   
            if(click_num != 4){
                System.Windows.Point p = e.GetPosition(Pic);
                int x = Convert.ToInt32(Pic.Source.Width * p.X / Pic.ActualWidth);
                int y = Convert.ToInt32(Pic.Source.Height * p.Y / Pic.ActualHeight);
                click_points[click_num] = x;
                click_points[click_num + 1] = y;
                click_num += 2;
            }
        }
        private void MouseLeftButton_Click_1(object sender, MouseButtonEventArgs e){
            if (click_num != 8){
                System.Windows.Point p = e.GetPosition(Pic1);
                int x = Convert.ToInt32(Pic1.Source.Width * p.X / Pic1.ActualWidth);
                int y = Convert.ToInt32(Pic1.Source.Height * p.Y / Pic1.ActualHeight);
                click_points[click_num] = x;
                click_points[click_num + 1] = y;
                click_num += 2;
            }
        }
    }
}
