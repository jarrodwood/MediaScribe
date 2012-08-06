using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Documents;

namespace AvalonTextBox.Converters
{
    public class MarkedupTextToInlineListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (targetType != typeof(List<Inline>))
            {
                throw new Exception("targetType must be of type List<Inline>.");
            }
            if (false == value is string)
            {
                throw new Exception("value must be of type string.");
            }

            List<Section> sections = new MarkedupTextToNoteSectionsConverter().Convert(value, typeof(List<Section>), null, null) as List<Section>;
            List<Inline> result = new NoteSectionsToInlineListConverter().Convert(sections, typeof(List<Inline>), null, null) as List<Inline>;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
