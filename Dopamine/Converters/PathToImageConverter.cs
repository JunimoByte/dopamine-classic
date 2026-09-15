using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dopamine.Converters
{
    /// <summary>
    /// Stateless image converter. 
    /// Relies entirely on WPF's native IsAsync=True to offload work to a background thread.
    /// By not caching images in a static dictionary, we guarantee that the moment an image scrolls 
    /// off-screen, WPF's UI Virtualization drops the only reference to it, allowing the GC 
    /// to instantly reclaim the RAM.
    /// </summary>
    public class PathToImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is string path && !string.IsNullOrEmpty(path) && values[1] != null)
                {
                    int size = System.Convert.ToInt32(values[1]);
                    var fi = new FileInfo(path);

                    if (fi.Exists && fi.Length > 0)
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        // IgnoreImageCache absolutely prevents WPF from secretly holding onto the image in its global dictionary
                        bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.IgnoreImageCache;
                        bmp.DecodePixelWidth = size;
                        bmp.UriSource = new Uri(fi.FullName, UriKind.Absolute);
                        bmp.EndInit();
                        bmp.Freeze();

                        // Notify GC of the unmanaged WIC memory allocation so it isn't lazy about cleaning up
                        long approxBytes = (long)bmp.PixelWidth * bmp.PixelHeight * 4;
                        GC.AddMemoryPressure(approxBytes);

                        return bmp;
                    }
                }
            }
            catch
            {
                // File might be corrupted or locked, gracefully return null so UI handles fallback
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
