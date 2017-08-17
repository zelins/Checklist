using ChecklistApplication.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ChecklistApplication.Converters
{
    internal sealed class StateToBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChecklistState)
            {
                var state = (ChecklistState)value;
                return state == ChecklistState.Completed;
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
