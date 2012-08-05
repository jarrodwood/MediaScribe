using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace JayDev.Notemaker.Common
{
    [ValueConversion(typeof(TimeSpan), typeof(double))]
    public class SecondsToTimeSpanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(TimeSpan) && targetType != typeof(string))
                throw new InvalidOperationException("The target must be a TimeSpan or string (to be formatted as TimeSpan)");

            if (targetType == typeof(TimeSpan))
            {
                return new TimeSpan(0, 0, System.Convert.ToInt32(value));
            }
            else if (targetType == typeof(string))
            {
                return (new TimeSpan(0, 0, System.Convert.ToInt32(value))).ToString();
            }
            else
            {
                throw new Exception("error - should never reach this code");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(double) || targetType != typeof(int))
                throw new InvalidOperationException("The target must be a double");

            return ((TimeSpan)value).TotalSeconds;
        }

        #endregion
    }
}
