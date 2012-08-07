using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace JayDev.MediaScribe.Converters
{
    /// <summary>
    /// Used to set the value of WPF grid column widths or row heights - ref http://stackoverflow.com/questions/147908/how-do-i-databind-a-columndefinitions-width-or-rowdefinitions-height
    /// </summary>
    public class GridLengthValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
                return null;
            double val = (double)value;
            GridLength gridLength = new GridLength(val);
            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
                return null;
            GridLength val = (GridLength)value;
            return val.Value;
        }
    }
}
