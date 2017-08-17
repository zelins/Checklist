using System;
using System.Globalization;
using System.Windows.Data;

namespace ChecklistApplication.Converters
{
    internal class IsAsyncWorkToAppState : IValueConverter
    {
        private const string processing = "Processing...";
        private const string ready = "Ready";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                var state = (bool)value;
                if (state)
                {
                    return processing;
                }
                else
                {
                    return ready;
                }
            }
            else
            {
                throw new ArgumentException(nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}