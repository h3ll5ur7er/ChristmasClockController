using System.Windows.Data;
using System.Windows.Media.Imaging;
using System;
using System.Globalization;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Windows;

namespace ChristmasClockController {
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Trace.WriteLine("Converting " + value);

            var input = value?.ToString();
            if (input?.StartsWith("BMP") ?? false) {
                var (header, data) = input.Split(':');
                var (width, height) = header.Substring(3).Split('x').Select(int.Parse).ToArray();
                var bytes = System.Convert.FromBase64String(data.Substring(0, data.Length - 4));
                var bmp = new Bitmap(width, height);
                for (int i = 0; i < bytes.Length; i += 4) {
                    var x = i / 4 % width;
                    var y = i / 4 / width;
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(bytes[i+2], bytes[i + 1], bytes[i]));
                }
                var image = new BitmapImage();
                using (var mem = new System.IO.MemoryStream()) {
                    bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Bmp);
                    mem.Position = 0;
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                return image;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class ImageVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.GetType() == typeof(BitmapImage) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
