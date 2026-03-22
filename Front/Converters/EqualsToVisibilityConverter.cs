using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Front.Converters
{
    public class EqualsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
                return Visibility.Collapsed;

            var paramString = parameter.ToString();
            if (string.Equals(value.ToString(), paramString, StringComparison.OrdinalIgnoreCase))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
