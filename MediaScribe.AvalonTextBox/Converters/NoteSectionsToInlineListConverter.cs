using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Documents;
using System.Windows.Media;
using JayDev.MediaScribe.Common;

namespace AvalonTextBox.Converters
{
    public class NoteSectionsToInlineListConverter : IValueConverter
    {
        static readonly Brush HighlightingBrush = Brushes.Yellow;
        static readonly Brush HighlightingCurrentBrush = Brushes.Orange;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture, null);
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture, List<HighlightMatch> highlightSections = null)
        {
            if (false == value is List<Section>)
            {
                throw new Exception("value must be of type List<Section>.");
            }
            if (targetType != typeof(List<Inline>))
            {
                throw new Exception("targetType must be of type List<Inline>.");
            }

            //take a COPY of the sections, so that we don't modify the originals
            List<Section> sections = new List<Section>(value as List<Section>);

            Section lastSection = new Section();
            
            HighlightMatch[] highlightSectionArray = null == highlightSections ? null : highlightSections.ToArray();
            
            int currentSectionIndex = 0;
            int progressedSectionLength = 0;
            List<Section> newSections = new List<Section>();
            if (null == highlightSections)
            {
                newSections = sections;
            }
            else
            {
                //parameters: start index, whether we highlight, and whether it's current match
                List<Tuple<int, bool, bool>> splitPoints = new List<Tuple<int, bool, bool>>();

                highlightSections.ForEach(x =>
                {
                    splitPoints.Add(new Tuple<int, bool, bool>(x.MatchStartIndex, true, x.CurrentMatch));
                    splitPoints.Add(new Tuple<int, bool, bool>(x.MatchStartIndex + x.MatchLength, false, false));
                });

                bool isHighlighting = false;
                Brush highlightBrush = null;
                foreach (var splitPoint in splitPoints)
                {
                    while (progressedSectionLength + sections[currentSectionIndex].Text.Length < splitPoint.Item1)
                    {
                        Section currentSection = sections[currentSectionIndex];
                        if (isHighlighting)
                            currentSection.Background = highlightBrush;
                        newSections.Add(currentSection);
                        currentSectionIndex++;
                        progressedSectionLength += currentSection.Text.Length;
                    }

                    int splitIntoSection = splitPoint.Item1 - progressedSectionLength;
                    Section section1 = sections[currentSectionIndex].Clone();
                    section1.Text = section1.Text.Substring(0, splitIntoSection);
                    section1.Background = isHighlighting ? highlightBrush : null;
                    newSections.Add(section1);

                    progressedSectionLength += section1.Text.Length;

                    Section section2 = sections[currentSectionIndex].Clone();
                    section2.Text = section2.Text.Substring(splitIntoSection);
                    isHighlighting = splitPoint.Item2;
                    highlightBrush = splitPoint.Item3 ? HighlightingCurrentBrush : HighlightingBrush;
                    section2.Background = isHighlighting ? highlightBrush : null;
                    //newSections.Add(section2);
                    sections[currentSectionIndex] = section2;
                }
                for (int i = currentSectionIndex; i < sections.Count; i++)
                    newSections.Add(sections[i]);
            }

            List<Inline> result = new List<Inline>();

            foreach (Section section in newSections)
            {
                Inline inline = new Run(section.Text);
                if (section.Colour != lastSection.Colour)
                {
                    inline.Foreground = new SolidColorBrush(section.Colour);
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
                if (section.Background != null)
                {
                    inline.Background = section.Background;
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
