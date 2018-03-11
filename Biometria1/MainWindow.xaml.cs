using Microsoft.Win32;
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
using System.Drawing;
using System.IO;

namespace Biometria1
{
    public partial class MainWindow : Window
    {
        private System.Windows.Point start;
        private System.Windows.Point origin;
        public WriteableBitmap EditableImage { get; set; }
        public string stream { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|" +
                "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
            if (op.ShowDialog() == true)
            {
                stream = op.FileName;
                EditableImage = new WriteableBitmap(new BitmapImage(new Uri(stream)));
            }
            imgPhoto.Source = EditableImage;
        }

        private void MenuFileSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = "YourName";
            dlg.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|" +
                "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
            if (dlg.ShowDialog() == true)
            {
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(EditableImage));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }
            }

        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)imgPhoto.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

            double zoom = e.Delta > 0 ? .2 : -.2;

            if (transform.ScaleX - 0.21 > 0 && transform.ScaleY - 0.21 > 0)
            {
                transform.ScaleX += zoom;
                transform.ScaleY += zoom;
            } else if (zoom > 0)
            {
                transform.ScaleX += zoom;
                transform.ScaleY += zoom;
            }
            
        }

        private void imgZoomIn(object sender, RoutedEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)imgPhoto.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

            if (transform.ScaleX > 0 && transform.ScaleY > 0)
            {
                transform.ScaleX += 0.2;
                transform.ScaleY += 0.2;
            }
        }

        private void imgZoomOut(object sender, RoutedEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)imgPhoto.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

            if (transform.ScaleX - 0.21 > 0 && transform.ScaleY - 0.21 > 0)
            {
                transform.ScaleX -= 0.2;
                transform.ScaleY -= 0.2;
            }
        }

        private void imgMouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(((IInputElement)e.Source));

            if ((p.X >= 0) && (p.X < EditableImage.Width) && (p.Y >= 0) && (p.Y < EditableImage.Height))
            {
                System.Drawing.Color color = BitmapFromWriteableBitmap(EditableImage).GetPixel((int)p.X, (int)p.Y);
                int r = color.R;
                int g = color.G;
                int b = color.B;
                float hue = color.GetHue();
                float saturation = color.GetSaturation();
                float brightness = color.GetBrightness();

                RGB.Text = String.Format("R: {0} G: {1} B: {2}", r.ToString(), g.ToString(), b.ToString());
            }
        }

        private void imgChangePixel(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(((IInputElement)e.Source));

            if ((p.X >= 0) && (p.X < EditableImage.Width) && (p.Y >= 0) && (p.Y < EditableImage.Height))
            {
                var bitmap = BitmapFromWriteableBitmap(EditableImage);
                System.Drawing.Color color = bitmap.GetPixel((int)p.X, (int)p.Y);

                var rr = -1;
                var gg = -1;
                var bb = -1;
                int r = 0;
                int g = 0;
                int b = 0;

                if (int.TryParse(R.Text, out rr) && rr >= 0 && rr < 256)
                    r = rr;
                else
                    MessageBox.Show("Wrong Data");

                if (int.TryParse(R.Text, out gg) && gg >= 0 && gg < 256)
                    g = gg;
                else
                    MessageBox.Show("Wrong Data");

                if (int.TryParse(R.Text, out bb) && bb >= 0 && bb < 256)
                    b = bb;
                else
                    MessageBox.Show("Wrong Data");

                System.Drawing.Color colorToSet = System.Drawing.Color.FromArgb(r, g, b);
                float hue = color.GetHue();
                float saturation = color.GetSaturation();
                float brightness = color.GetBrightness();


                bitmap = CreateNonIndexedImage(bitmap);

                bitmap.SetPixel(Convert.ToInt32(p.X), Convert.ToInt32(p.Y), colorToSet);

                EditableImage = WriteableBitmapBitmapFromBitmap(bitmap);

                imgPhoto.Source = EditableImage;
            }
        }


        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        private WriteableBitmap WriteableBitmapBitmapFromBitmap(Bitmap writeBmp)
        {
            BitmapSource bitmapSource =
                 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(writeBmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                 System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            WriteableBitmap writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(bitmapSource);
            return writeableBitmap;
        }

        public Bitmap CreateNonIndexedImage(Bitmap src)
        {
            Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0);
            }

            return newBmp;
        }

        private void About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Author: Kamil Oleksy");
        }

        private void HowToUse(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("To zoom in and out picture use mouse wheel.");
        }



        //private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    TransformGroup transformGroup = (TransformGroup)imgPhoto.RenderTransform;
        //    ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

        //    double zoom = e.NewValue;
        //    transform.ScaleX = zoom;
        //    transform.ScaleY = zoom;
        //}

        //private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    imgPhoto.CaptureMouse();
        //    var tt = (TranslateTransform)((TransformGroup)imgPhoto.RenderTransform)
        //        .Children.First(tr => tr is TranslateTransform);
        //    start = e.GetPosition((IInputElement)e.Source);
        //    origin = new System.Windows.Point(tt.X, tt.Y);
        //}

        //private void image_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (imgPhoto.IsMouseCaptured)
        //    {
        //        var tt = (TranslateTransform)((TransformGroup)imgPhoto.RenderTransform)
        //            .Children.First(tr => tr is TranslateTransform);
        //        Vector v = start - e.GetPosition((IInputElement)e.Source);
        //        tt.X = origin.X - v.X;
        //        tt.Y = origin.Y - v.Y;
        //    }
        //}

        //private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    imgPhoto.ReleaseMouseCapture();
        //}

    }
}
