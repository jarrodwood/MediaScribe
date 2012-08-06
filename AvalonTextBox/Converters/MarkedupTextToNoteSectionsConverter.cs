using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace AvalonTextBox.Converters
{
    public class MarkedupTextToNoteSectionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value is string)
            {
                throw new Exception("value must be of type string.");
            }
            if (targetType != typeof(List<Section>))
            {
                throw new Exception("targetType must be of type List<Section>.");
            }

            string markedupText = value as string;

            if (string.Empty == markedupText)
            {
                return new List<Section>(new[] { 
                    new Section() {
                        Colour = ColorHelper.ApplicationDefaultTextColour,
                        Text = string.Empty }});
            }

            int preparedUntilIndex = 0;
            List<Section> sections = new List<Section>();
            Section currentSection = new Section()
            {
                Colour = ColorHelper.ApplicationDefaultTextColour
            };
            for (int i = 0; i < markedupText.Length; i++)
            {
                switch (markedupText[i])
                {
                    case '<':
                        //if it's a non-empty section..
                        if (i - preparedUntilIndex > 0)
                        {
                            currentSection.Text = markedupText.Substring(preparedUntilIndex, i - preparedUntilIndex);
                            //add the section to the list (note: since section is a struct, this is a value-copy not a reference-copy.)
                            sections.Add(currentSection);
                            //we reset the section's text... but keep the formatting, until we know what this tag is supposed to be doing.
                            currentSection.Text = null;
                            preparedUntilIndex = i;
                        }
                        int offset = 1;
                        bool isEndingElement = markedupText[i + offset] == '/';
                        //if we're ending the element, the command comes next
                        if (isEndingElement)
                        {
                            offset++;
                        }
                        char command = markedupText[i + offset];
                        offset++;
                        switch (command)
                        {
                            case 'b':
                                if (isEndingElement)
                                {
                                    Assert(currentSection.Weight == NoteWeight.Normal, "ending a bold section, but the text isn't bold...");
                                    currentSection.Weight = NoteWeight.Normal;
                                }
                                else
                                {
                                    Assert(currentSection.Weight == NoteWeight.Bold, "starting a bold section, but the text is already bold...");
                                    currentSection.Weight = NoteWeight.Bold;
                                }
                                //skip past the end of the tag
                                Assert(markedupText[i + offset] != '>', "expecting end of tag, but didn't find it!");
                                offset++;
                                break;
                            case 'i':
                                if (isEndingElement)
                                {
                                    Assert(currentSection.Style == NoteStyle.Normal, "ending an italic section, but the text isn't italic...");
                                    currentSection.Style = NoteStyle.Normal;
                                }
                                else
                                {
                                    Assert(currentSection.Style == NoteStyle.Italic, "starting an italic section, but the text is already italic...");
                                    currentSection.Style = NoteStyle.Italic;
                                }
                                //skip past the end of the tag
                                Assert(markedupText[i + offset] != '>', "expecting end of tag, but didn't find it!");
                                offset++;
                                break;
                            case 'c':
                                if (isEndingElement)
                                {
                                    currentSection.Colour = ColorHelper.ApplicationDefaultTextColour;
                                    //skip past the end of the tag
                                    Assert(markedupText[i + offset] != '>', "expecting end of tag, but didn't find it!");
                                    offset++;
                                }
                                else
                                {
                                    if (markedupText[i + offset] != '=') throw new Exception("error in markup - expected \"<c=\", but no = found");
                                    int colourStartIndex = i + offset + 1;
                                    int colourEndIndex = markedupText.IndexOf('>', colourStartIndex);
                                    if (colourEndIndex == -1) throw new Exception("error in markup - no end tag for <>");
                                    string colourString = markedupText.Substring(colourStartIndex, colourEndIndex - colourStartIndex);
                                    Color colour = ColorHelper.FromString(colourString);
                                    currentSection.Colour = colour;
                                    //skip past the end of the tag
                                    offset += colourEndIndex - colourStartIndex + 2;
                                }
                                break;
                        }

                        preparedUntilIndex += offset;
                        break;
                }
            }

            //if we have SOME text chunk left that we haven't saved... save it.
            if (preparedUntilIndex < markedupText.Length)
            {
                currentSection.Text = markedupText.Substring(preparedUntilIndex);
                sections.Add(currentSection);
            }

            return sections;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        private void Assert(bool condition, string errorMessage)
        {
            if (condition)
                throw new Exception(errorMessage);
        }
    }
}
