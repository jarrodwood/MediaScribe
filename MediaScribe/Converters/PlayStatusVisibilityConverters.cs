using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using JayDev.MediaScribe.Core;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.Converters
{

    public class PauseButtonVisibilityConverter : IValueConverter
    {
        public PauseButtonVisibilityConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is PlayStatus)
            {
                throw new ArgumentException("Value is not of correct type");
            }
            if (((PlayStatus)value) == PlayStatus.Playing)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    public class PlayButtonVisibilityConverter : IValueConverter
    {
        public PlayButtonVisibilityConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is PlayStatus)
            {
                throw new ArgumentException("Value is not of correct type");
            }
            if (((PlayStatus)value) != PlayStatus.Playing)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
