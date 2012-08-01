using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace JayDev.Notemaker.Common
{
    public class NumericRangeVisibilityConverter : IValueConverter
    {
        public NumericRangeVisibilityConverter()
        {
        }

        public int VisibleFrom { get; set; }
        public int VisibleTo { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false != value is double && false != value is int)
            {
                throw new ArgumentException("Value must be double or int");
            }

            int intValue = value is int ? (int)value : System.Convert.ToInt32(value);
            if (intValue >= VisibleFrom && intValue <= VisibleTo)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
