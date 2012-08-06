using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Documents;
using System.Windows.Media;

namespace AvalonTextBox.Converters
{
    class NoteSectionsToInlineListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is List<Section>)
            {
                throw new Exception("value must be of type List<Section>.");
            }
            if (targetType != typeof(List<Inline>))
            {
                throw new Exception("targetType must be of type List<Inline>.");
            }

            List<Section> sections = value as List<Section>;
            List<Inline> result = new List<Inline>();

            Section lastSection = new Section();
            foreach (Section section in sections)
            {
                Inline inline = new Run(section.Text);
                if (section.Colour != lastSection.Colour)
                {
                    if (section.Colour != "Default")
                    {
                        //TODO: handle all colours
                        switch (section.Colour)
                        {
                            case "Red":
                                inline.Foreground = Brushes.Red;
                                break;
                            case "Grey":
                                inline.Foreground = Brushes.Gray;
                                break;
                            case "Blue":
                                inline.Foreground = Brushes.Blue;
                                break;
                        }
                    }
                }
                if (section.Style != lastSection.Style)
                {
                    if (section.Style == NoteStyle.Italic)
                    {
                        inline = new Italic(inline);
                    }
                }
                if (section.Weight != lastSection.Weight)
                {
                    if (section.Weight == NoteWeight.Bold)
                    {
                        inline = new Bold(inline);
                    }
                }

                result.Add(inline);
            }

            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
