using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using JayDev.MediaScribe.View.Controls;

namespace JayDev.MediaScribe.Core
{
   public static class SliderPreviewHelper
    {
        #region PreviewEnabled

        public static bool GetPreviewEnabled(Slider slider)
        {
            return (bool)slider.GetValue(PreviewEnabledProperty);
        }

        public static void SetPreviewEnabled(
          Slider slider, bool value)
        {
            slider.SetValue(PreviewEnabledProperty, value);
        }

        public static readonly DependencyProperty PreviewEnabledProperty =
            DependencyProperty.RegisterAttached(
            "PreviewEnabled",
            typeof(bool),
            typeof(SliderPreviewHelper),
            new UIPropertyMetadata(false, OnPreviewEnabledChanged));
       
        #endregion // PreviewEnabled

        #region ContentStyle
        public static Style GetContentStyle(Slider slider)
        {
            return (Style)slider.GetValue(ContentStyleProperty);
        }

        public static void SetContentStyle(
          Slider slider, Style value)
        {
            slider.SetValue(ContentStyleProperty, value);
        }

        public static readonly DependencyProperty ContentStyleProperty =
            DependencyProperty.RegisterAttached(
            "ContentStyle",
            typeof(Style),
            typeof(SliderPreviewHelper),
            new UIPropertyMetadata(null));
        #endregion 

        //Internal Dependancy property
        #region PopupAdorner
        private static ContentAdorner GetPopupAdorner(Slider slider)
        {
            return (ContentAdorner)slider.GetValue(PopupAdornerProperty);
        }

        private static readonly DependencyProperty PopupAdornerProperty = DependencyProperty.RegisterAttached(
            "PopupAdorner",
            typeof(ContentAdorner),
            typeof(SliderPreviewHelper),
            new UIPropertyMetadata(null));
        #endregion 

        static void OnPreviewEnabledChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            Slider item = depObj as Slider;
            if (item == null)
                return;

            if ((bool)e.NewValue)
            {
                item.PreviewMouseMove += new System.Windows.Input.MouseEventHandler(item_PreviewMouseMove);
                item.MouseLeave += new System.Windows.Input.MouseEventHandler(item_MouseLeave);
            }
            else
            {
                item.PreviewMouseMove -= new System.Windows.Input.MouseEventHandler(item_PreviewMouseMove);
                item.MouseLeave -= new System.Windows.Input.MouseEventHandler(item_MouseLeave);
            }
        }

        static void item_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Slider slider = sender as Slider;
            ContentAdorner popup = GetPopupAdorner(slider);
            popup.Visibility = Visibility.Collapsed;
        }

        static void item_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Slider slider = sender as Slider;

            ContentAdorner popup = GetPopupAdorner(slider);
            if (popup == null)
            {
                Style style = GetContentStyle(slider);
                popup = new ContentAdorner(slider, style);
                slider.SetValue(PopupAdornerProperty, popup);
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(slider);
                layer.Add(popup);
            }

            popup.Visibility = Visibility.Visible;

            System.Windows.Controls.Primitives.Track _track = slider.Template.FindName("PART_Track", slider) as System.Windows.Controls.Primitives.Track;

            Point position = e.MouseDevice.GetPosition(_track);

            double value = _track.ValueFromPoint(position);

            if (slider.SmallChange != 0.0)
            {
                double diff = value % slider.SmallChange;
                value -= diff;
            }
            popup.Content = Math.Max(slider.Minimum, Math.Min(slider.Maximum, value));
            position = e.GetPosition(slider);
            position.Y = slider.ActualHeight / 2.0;
            popup.PlacementOffset = position;
        }


    }
}
