using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.ComponentModel;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is Enum)
            {
                throw new Exception("value must be an enum");
            }
            if (targetType != typeof(string))
            {
                throw new Exception("targettype must be string");
            }
            Enum valueCasted = value as Enum;
            return valueCasted.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class EnumInfoLabelToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
                return null;

            if (false == value is Enum)
            {
                return null;
            }
            Enum valueCasted = (Enum)value;
            var info = valueCasted.GetInformation();
            if (null == info)
            {
                return null;
            }

            return info.Label;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class EnumToTooltipTextBlockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            if (false == value is Enum)
            {
                return null;
            }
            Enum valueCasted = value as Enum;
            var info = valueCasted.GetInformation();
            if (null == info)
            {
                return null;
            }

            TextBlock toolTip = new TextBlock();

            toolTip.Inlines.Add(new Run("Description: ") { FontWeight = FontWeights.Bold });
            toolTip.Inlines.Add(new Run(info.Description) { FontWeight = FontWeights.Normal });

            toolTip.Inlines.Add(new LineBreak());

            toolTip.Inlines.Add(new Run("Applicable when: ") { FontWeight = FontWeights.Bold });
            toolTip.Inlines.Add(new Run(info.ApplicableWhen) { FontWeight = FontWeights.Normal });

            return toolTip;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
