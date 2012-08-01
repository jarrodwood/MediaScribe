using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace JayDev.Notemaker.Core
{

    public class EnumConverter<T> : IValueConverter
    {
        public EnumConverter(T visibleValue)
        {
            VisibleValue = visibleValue;
        }

        public T VisibleValue { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is T)
            {
                throw new ArgumentException("Value is not of correct type");
            }
            if (((T)value).Equals(VisibleValue))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
