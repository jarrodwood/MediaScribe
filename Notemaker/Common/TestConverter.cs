using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace JayDev.Notemaker.Common
{
    class TestConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new TestConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
