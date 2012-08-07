using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using JayDev.MediaScribe.Common;

namespace AvalonTextBox
{
    public struct Section
    {
        public string Text { get; set; }
        public Color Colour { get; set; }
        public NoteStyle Style { get; set; }
        public NoteWeight Weight { get; set; }
        public Section(string text)
            : this()
        {
            Text = text;
            Colour = ColorHelper.ApplicationDefaultTextColour;
            Style = NoteStyle.Normal;
            Weight = NoteWeight.Normal;
        }
    }
}
