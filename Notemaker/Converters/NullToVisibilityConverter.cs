using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Windows;

namespace JayDev.Notemaker.Converters
{
    public class NullConverter<T> : IValueConverter
    {
        public NullConverter(T nullValue, T notNullValue)
        {
            NullValue = nullValue;
            NotNullValue = notNullValue;
        }

        public T NullValue { get; set; }
        public T NotNullValue { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is object && (null == value) ? NullValue : NotNullValue;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, NullValue);
        }
    }

    public sealed class NullToVisibilityConverter : NullConverter<Visibility>
    {
        public NullToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed) { }
    }
}
