using System.Windows.Data;
using System.Windows.Media.Imaging;
using System;
using System.Globalization;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Collections.Generic;

namespace ChristmasClockController {
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var input = value as string;
            if (input?.StartsWith("BMP") ?? false) {
                var (header, data) = input.Split(':');
                var (width, height) = header.Substring(3).Split('x').Select(int.Parse).ToArray();
                var bytes = System.Convert.FromBase64String(data.Substring(0, data.Length - 4));
                var bmp = new Bitmap(width, height);
                var i = 0;
                for(int y = 0; y < height; y++){
                    for(int x = 0; x < width; x++, i+=4){
                        bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(bytes[i +2], bytes[i+3], bytes[i +1]));
                    }
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
            if (input?.StartsWith("7SEG") ?? false) {
                var (header, data) = input.Split(':');
                var bytes = System.Convert.FromBase64String(data.Substring(0, data.Length - 4));
                var bmp = new Bitmap(33, 13);
                var i = 0;
                foreach(var c in GetCoordinates()){
                    bmp.SetPixel(c.Item1, c.Item2, System.Drawing.Color.FromArgb(bytes[i +2], bytes[i+3], bytes[i +1]));
                    i += 4;
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

        private IEnumerable<Tuple<int, int>> GetCoordinates(){
            var oneSeg = new List<Tuple<int, int>>{ 
                new Tuple<int, int>(0, 5),
                new Tuple<int, int>(0, 4),
                new Tuple<int, int>(0, 3),
                new Tuple<int, int>(0, 2),
                new Tuple<int, int>(0, 1),
                new Tuple<int, int>(1, 0),
                new Tuple<int, int>(2, 0),
                new Tuple<int, int>(3, 0),
                new Tuple<int, int>(4, 0),
                new Tuple<int, int>(5, 0),
                new Tuple<int, int>(6, 1),
                new Tuple<int, int>(6, 2),
                new Tuple<int, int>(6, 3),
                new Tuple<int, int>(6, 4),
                new Tuple<int, int>(6, 5),
                new Tuple<int, int>(5, 6),
                new Tuple<int, int>(4, 6),
                new Tuple<int, int>(3, 6),
                new Tuple<int, int>(2, 6),
                new Tuple<int, int>(1, 6),
                new Tuple<int, int>(0, 7),
                new Tuple<int, int>(0, 8),
                new Tuple<int, int>(0, 9),
                new Tuple<int, int>(0, 10),
                new Tuple<int, int>(0, 11),
                new Tuple<int, int>(1, 12),
                new Tuple<int, int>(2, 12),
                new Tuple<int, int>(3, 12),
                new Tuple<int, int>(4, 12),
                new Tuple<int, int>(5, 12),
                new Tuple<int, int>(6, 11),
                new Tuple<int, int>(6, 10),
                new Tuple<int, int>(6, 9),
                new Tuple<int, int>(6, 8),
                new Tuple<int, int>(6, 7)
            };
            var offsets = new List<int>{ 0, 8, 18, 26 };

            foreach(var c in oneSeg.Select(x => new Tuple<int, int>(x.Item1 +offsets[0], x.Item2))){
                yield return c;
            }
            foreach(var c in oneSeg.Select(x => new Tuple<int, int>(x.Item1 +offsets[1], x.Item2))){
                yield return c;
            }
            yield return new Tuple<int, int>(16, 9);
            yield return new Tuple<int, int>(16, 6);
            foreach(var c in oneSeg.Select(x => new Tuple<int, int>(x.Item1 +offsets[2], x.Item2))){
                yield return c;
            }
            foreach(var c in oneSeg.Select(x => new Tuple<int, int>(x.Item1 +offsets[3], x.Item2))){
                yield return c;
            }
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
