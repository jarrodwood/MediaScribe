using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Documents;

namespace AvalonTextBox.Converters
{
    class NoteSectionsToMarkedupTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is List<Section>)
            {
                throw new Exception("value must be of type List<Section>.");
            }
            if (targetType != typeof(string))
            {
                throw new Exception("targetType must be of type string.");
            }

            List<Section> sections = value as List<Section>;
            StringBuilder builder = new StringBuilder();
            Section lastSection = new Section() { Colour = ColorHelper.ApplicationDefaultTextColour };
            foreach (Section section in sections)
            {
                if (section.Colour != lastSection.Colour)
                {
                    if (section.Colour != ColorHelper.ApplicationDefaultTextColour)
                    {
                        builder.Append("<c=");
                        builder.Append(section.Colour);
                        builder.Append(">");
                    }
                    else
                    {
                        builder.Append("</c>");
                    }
                }
                if (section.Style != lastSection.Style)
                {
                    if (section.Style == NoteStyle.Italic)
                    {
                        builder.Append("<i>");
                    }
                    else
                    {
                        builder.Append("</i>");
                    }
                }
                if (section.Weight != lastSection.Weight)
                {
                    if (section.Weight == NoteWeight.Bold)
                    {
                        builder.Append("<b>");
                    }
                    else
                    {
                        builder.Append("</b>");
                    }
                }

                lastSection = section;
                builder.Append(section.Text);
            }

            return builder.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
