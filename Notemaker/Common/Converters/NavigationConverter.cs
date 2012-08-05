using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace JayDev.Notemaker.Common
{
    public class NavigationCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Nullable<System.Boolean>))
                throw new InvalidOperationException("The target must be a bool");

            if(null == parameter)
                throw new InvalidOperationException("must pass in a parameter to match to");

            if (parameter.GetType() != typeof(NavigateMessage))
                throw new InvalidOperationException("The parameter must be a NavigateMessage");

            return ((NavigateMessage)value) == ((NavigateMessage)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class NavigationVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be Visibility");

            if (null == parameter)
                throw new InvalidOperationException("must pass in a parameter to match to");

            if (parameter.GetType() != typeof(NavigateMessage))
                throw new InvalidOperationException("The parameter must be a NavigateMessage");

            return ((NavigateMessage)value) == ((NavigateMessage)parameter) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
