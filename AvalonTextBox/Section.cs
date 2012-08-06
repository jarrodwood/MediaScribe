using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AvalonTextBox
{
    public struct Section
    {
        public string Text { get; set; }
        public string Colour { get; set; }
        public NoteStyle Style { get; set; }
        public NoteWeight Weight { get; set; }
        public Section(string text)
            : this()
        {
            Text = text;
            Colour = "Default";
            Style = NoteStyle.Normal;
            Weight = NoteWeight.Normal;
        }
    }
}
