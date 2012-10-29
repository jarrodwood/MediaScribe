using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// Provides WPF content control functionality to an Adorner.
    /// </summary>
    public class ContentAdorner : Adorner
    {
        private ContentControl _contentControl;

        public ContentAdorner(UIElement adornedElem, Style contentStyle)
            : base(adornedElem)
        {
            _contentControl = new ContentControl(); 
            _contentControl.Style = contentStyle;
            AddLogicalChild(_contentControl);
            AddVisualChild(_contentControl);
        }
   
        public object Content
        {
            set
            {
                _contentControl.Content = value;
            }
        }
        
        public Point PlacementOffset
        {
            get { return (Point)GetValue(PlacementOffsetProperty); }
            set { SetValue(PlacementOffsetProperty, value); }
        }

        public static readonly DependencyProperty PlacementOffsetProperty =
            DependencyProperty.Register("PlacementOffset", typeof(Point), typeof(ContentAdorner),
            new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_contentControl != null)
            {
                _contentControl.Arrange(new Rect(PlacementOffset, _contentControl.DesiredSize));
            }
            return finalSize;
        }

        protected override int VisualChildrenCount { get { return 1; } }

        protected override Visual GetVisualChild(int index) { return _contentControl; }


    }

}
