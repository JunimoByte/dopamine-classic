using Digimezzo.Foundation.Core.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dopamine.Converters
{
    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return DependencyProperty.UnsetValue;

            if (value is byte[] bytes && bytes.Length > 0)
            {
                try
                {
                    int maxDecodeSize = 500;
                    if (parameter != null && int.TryParse(parameter.ToString(), out int paramSize) && paramSize > 0)
                    {
                        maxDecodeSize = paramSize;
                    }

                    BitmapImage bmp = ImageUtils.ByteToBitmapImage(bytes, maxDecodeSize, maxDecodeSize, 0);
                    if (bmp != null)
                    {
                        return bmp;
                    }
                }
                catch
                {
                    // Fall back to original value if decoding fails
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
