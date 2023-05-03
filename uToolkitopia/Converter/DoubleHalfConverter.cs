using System;
using System.Globalization;
using System.Windows.Data;

namespace Kitopia.Converter
{
    public class DoubleHalfConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = (double)value[0] - (double)value[1];
            return doubleValue / 2;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
