using System;
using System.Globalization;
using System.Windows.Data;

namespace Kitopia.Converter
{
    public class DoubleQuarterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = (double)value;
            return doubleValue / 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
