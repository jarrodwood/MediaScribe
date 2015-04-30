using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace JayDev.MediaScribe.Converters
{
    public class NumericRangeVisibilityConverter : IValueConverter
    {
        public NumericRangeVisibilityConverter()
        {
        }

        public double VisibleFrom { get; set; }
        public double VisibleTo { get; set; }
        public bool Inverted { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false != value is double && false != value is int)
            {
                throw new ArgumentException("Value must be double or int");
            }

            double intValue = value is double ? (double)value : System.Convert.ToDouble(value);
            Visibility result = Visibility.Visible;
            if (intValue >= VisibleFrom && intValue <= VisibleTo)
            {
                result = Visibility.Visible;
            }
            else
            {
                result = Visibility.Collapsed;
            }

            if (Inverted)
            {
                return result == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return result;
            }
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
