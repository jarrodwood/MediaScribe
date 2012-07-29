using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace JayDev.Notemaker.Common
{

    class TextListAdorner : Adorner
    {
        string _text;
        Point _position;

        public TextListAdorner(UIElement adornedElement, IEnumerable<string> selectedEntries)
            : base(adornedElement)
        {
            _text = CreateText(selectedEntries);
            adornedElement.DragOver += new DragEventHandler(adornedElement_DragOver);
        }


        void adornedElement_DragOver(object sender, DragEventArgs e)
        {
            _position = e.GetPosition(AdornedElement) + new Vector(10, 10);
            InvalidateVisual();
            Debug.WriteLine("drag over");
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (_position == null ||
                _position.X == 0 ||
                _position.Y == 0)
                return;


            var text = new FormattedText(_text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Verdanda"),
                    13, new SolidColorBrush(Color.FromArgb(230, 25, 25, 25)));

            drawingContext.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(200, 150, 150, 230)), new Pen(Brushes.Black, 0.4d),
                new Rect(_position, _position + new Vector(text.Width + 24, text.Height + 24)), 3, 3);
            drawingContext.DrawText(text, _position + new Vector(12, 12));
            base.OnRender(drawingContext);

            Debug.WriteLine("render");
        }

        private string CreateText(IEnumerable<string> selectedEntries)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string entry in selectedEntries)
            {
                if (builder.Length > 0)
                {
                    builder.Append("\r\n");
                }
                builder.Append(entry);
            }
            return builder.ToString();
        }
    }
}
