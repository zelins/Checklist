using ChecklistApplication.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChecklistApplication.Converters
{
    /// <summary>
    /// This class isn't used in this project
    /// </summary>
    class StateToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChecklistState)
            {
                var state = (ChecklistState)value;
                Brush result;
                switch (state)
                {
                    case ChecklistState.Empty:
                        result = Brushes.White;
                        break;
                    case ChecklistState.Opened:
                        result = Brushes.LightCyan;
                        break;
                    case ChecklistState.InProgress:
                        result = Brushes.Orange;
                        break;
                    case ChecklistState.Completed:
                        result = Brushes.ForestGreen;
                        break;
                    default:
                        result = Brushes.White;
                        break;
                }
                return result;
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
