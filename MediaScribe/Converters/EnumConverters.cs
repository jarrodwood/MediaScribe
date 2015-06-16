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
using System.Windows.Input;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.Converters
{
    public class ModifierKeysCollectionToStringConverter : IValueConverter
    {
        /// <summary>
        /// Given a 'ModiferKeys' flag enum, generate a string showing exactly which values are set in the flag.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is ModifierKeys)
            {
                throw new Exception("value must be a ModifierKeys");
            }
            if (targetType != typeof(string))
            {
                throw new Exception("targettype must be string");
            }
                
            ModifierKeys keys = (ModifierKeys)value;

            List<string> components = new List<string>();
            if ((keys & ModifierKeys.Windows) == ModifierKeys.Windows)
                components.Add("Win");
            if ((keys & ModifierKeys.Shift) == ModifierKeys.Shift)
                components.Add("Shift");
            if ((keys & ModifierKeys.Control) == ModifierKeys.Control)
                components.Add("Ctrl");
            if ((keys & ModifierKeys.Alt) == ModifierKeys.Alt)
                components.Add("Alt");
            string result = string.Join("+", components);
            Logging.Log(LoggingSource.Temp, "modifier keys: " + result);
            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Given a 'Key' enum (keyboard key), look up the key in the KeyNames resource file to get a human-readable version of it
    /// (falling back to the enum name if there isn't one.) This is used for nicer human-readable display in the UI.
    /// </summary>
    public class KeyEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is Key)
            {
                throw new Exception("value must be a a 'Key' enum");
            }
            if (targetType != typeof(string) && targetType != typeof(Object))
            {
                throw new Exception("targettype must be string or Object");
            }
            Key valueCasted = (Key)(value as Enum);
            string result = null;

            var keyName = KeyNames.ResourceManager.GetString(valueCasted.ToString());
            if (string.IsNullOrEmpty(keyName))
                keyName = valueCasted.ToString();

            return keyName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Generic converter that, given an enum value, returns the value's name as a string.
    /// </summary>
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
