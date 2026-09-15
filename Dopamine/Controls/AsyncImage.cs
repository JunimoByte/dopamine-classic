using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Dopamine.Controls
{
    public static class AsyncImage
    {
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.RegisterAttached("Path", typeof(string), typeof(AsyncImage), new PropertyMetadata(null, OnPathOrSizeChanged));

        public static readonly DependencyProperty DecodeSizeProperty =
            DependencyProperty.RegisterAttached("DecodeSize", typeof(double), typeof(AsyncImage), new PropertyMetadata(0.0, OnPathOrSizeChanged));

        public static string GetPath(DependencyObject obj) => (string)obj.GetValue(PathProperty);
        public static void SetPath(DependencyObject obj, string value) => obj.SetValue(PathProperty, value);

        public static double GetDecodeSize(DependencyObject obj) => (double)obj.GetValue(DecodeSizeProperty);
        public static void SetDecodeSize(DependencyObject obj, double value) => obj.SetValue(DecodeSizeProperty, value);

        private static async void OnPathOrSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Image image)
            {
                string path = GetPath(image);
                double size = GetDecodeSize(image);

                if (string.IsNullOrEmpty(path) || size <= 0)
                {
                    image.Source = null;
                    return;
                }

                try
                {
                    var bmp = await Task.Run(() =>
                    {
                        var fi = new FileInfo(path);
                        if (fi.Exists && fi.Length > 0)
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.IgnoreImageCache;
                            bitmap.DecodePixelWidth = (int)size;
                            bitmap.UriSource = new Uri(fi.FullName, UriKind.Absolute);
                            bitmap.EndInit();
                            bitmap.Freeze();
                            
                            long approxBytes = (long)bitmap.PixelWidth * bitmap.PixelHeight * 4;
                            GC.AddMemoryPressure(approxBytes);
                            
                            return bitmap;
                        }
                        return null;
                    });

                    // Ensure the property hasn't changed while we were decoding (UI virtualization recycled the image)
                    if (GetPath(image) == path)
                    {
                        image.Source = bmp;
                    }
                }
                catch
                {
                    if (GetPath(image) == path)
                    {
                        image.Source = null;
                    }
                }
            }
        }
    }
}

