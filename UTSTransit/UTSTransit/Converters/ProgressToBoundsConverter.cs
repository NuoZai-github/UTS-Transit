using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace UTSTransit.Converters
{
    public class ProgressToBoundsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                // X = progress (0..1), Y = 0.5 (Center), W = 40, H = 40
                return new Rect(progress, 0.5, 40, 40);
            }
            return new Rect(0, 0.5, 40, 40);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
