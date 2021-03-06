﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using System.Windows;
using ICSharpCode.AvalonEdit.Document;

namespace AvalonTextBox
{
    class MarkupColorizer : DocumentColorizingTransformer
    {
        List<Section> _sections;
        public List<Section> Sections { set { _sections = value; } }
        Brush _defaultTextColour;
        TextEditor _avalonEditControl;
        public MarkupColorizer(List<Section> sections, Brush defaultTextColour, TextEditor avalonEditControl)
        {
            this._sections = sections;
            this._defaultTextColour = defaultTextColour;
            this._avalonEditControl = avalonEditControl;
        }

        protected override void Colorize(ITextRunConstructionContext context)
        {
            base.Colorize(context);
        }
        protected override void ColorizeLine(DocumentLine line)
        {
            int position = 0;
            for (int i = 0; i < _sections.Count; i++)
            {
                Section currentSection = _sections[i];
                //if this section has already been parsed...
                if (position + currentSection.Text.Length < line.Offset)
                {
                    position += currentSection.Text.Length;
                    continue;
                }
                //otherwise, the section is relevant to this line...
                else
                {
                    int startPosition = Math.Max(position, line.Offset); //if this is the start of a section, start from the section. otherwise if this is a new line (within a section chunk) start from where the new line is...
                    int endPosition = Math.Min(line.EndOffset, position + currentSection.Text.Length);
                    base.ChangeLinePart(
                        startPosition, // startOffset
                        endPosition, // endOffset
                        (VisualLineElement element) =>
                        {
                            //element.TextRunProperties.
                            // This lambda gets called once for every VisualLineElement
                            // between the specified offsets.
                            Typeface tf = element.TextRunProperties.Typeface;
                            // Replace the typeface with a modified version of
                            // the same typeface
                            element.TextRunProperties.SetTypeface(new Typeface(
                                tf.FontFamily,
                                currentSection.Style == NoteStyle.Italic ? FontStyles.Italic : FontStyles.Normal,
                                //HACK: the text by default is narrow, e.g.:
                                //currentSection.Weight == NoteWeight.Bold ? FontWeights.Bold : FontWeights.Normal,
                                currentSection.Weight == NoteWeight.Bold ? FontWeights.Bold : FontWeights.Normal,
                                tf.Stretch
                            ));
                            //TODO: highlighting doesn't remove colour yet...
                            if (element.TextRunProperties.ForegroundBrush != _avalonEditControl.TextArea.SelectionForeground)
                            {
                                element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(currentSection.Colour));
                            }
                        });
                    //if this takes us to the end of the line, break out.
                    if (position + currentSection.Text.Length >= line.EndOffset)
                    {
                        break;
                    }
                    position += currentSection.Text.Length;
                }
            }
        }
    }
}
