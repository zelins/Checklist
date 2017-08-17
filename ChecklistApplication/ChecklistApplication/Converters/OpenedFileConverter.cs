using System;
using System.Globalization;
using System.Windows.Data;

namespace ChecklistApplication.Converters
{
    internal class OpenedFileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && parameter is string)
            {
                if (string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return parameter.ToString();
                }
                else
                {
                    return value.ToString();
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}