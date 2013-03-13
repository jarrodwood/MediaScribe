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
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace JayDev.MediaScribe.Core
{
    /// <summary>
    /// Helper class: contains code related to a 'preview' box when hovering the cursor over a WPF Slider. This is initially used in
    /// MediaScribe to view the current track's time at the cursor's position (e.g. hover at the start of the Slider, and the time will be
    /// at or near 0:00, at the end of the file the time will be the full track's duration, hover at the center and the time will show half
    /// of the track's duration)
    /// </summary>
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

        #region ThumbnailGenerator
        public static ThumbnailGenerator GetThumbnailGenerator(Slider slider)
        {
            return (ThumbnailGenerator)slider.GetValue(ThumbnailGeneratorProperty);
        }

        public static void SetThumbnailGenerator(
          Slider slider, ThumbnailGenerator value)
        {
            slider.SetValue(ThumbnailGeneratorProperty, value);
        }

        public static readonly DependencyProperty ThumbnailGeneratorProperty =
            DependencyProperty.RegisterAttached(
            "ThumbnailGenerator",
            typeof(ThumbnailGenerator),
            typeof(SliderPreviewHelper),
            new UIPropertyMetadata(null));
        #endregion


        static TrackbarPreview subControl = null;

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

        static void OnPreviewEnabledChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
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

        /// <summary>
        /// When the cursor leaves the control, hide the popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void item_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Slider slider = sender as Slider;
            ContentAdorner popup = GetPopupAdorner(slider);
            popup.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// When the cursor moves around the control, show it if it's not yet visible, set the appropriate time value in the popup, and move
        /// it to the cursor's x-axis location on the slider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            double displayValue = Math.Max(slider.Minimum, Math.Min(slider.Maximum, value));
            if (null == subControl)
                subControl = new TrackbarPreview();

            var generator = GetThumbnailGenerator(slider);
            TimeSpan hoverTime = new TimeSpan(0, 0, (int)displayValue + 1);
            subControl.CurrentPlayTime = hoverTime;

            if (generator.IsTrackVideo)
            {
                Thumbnail thumbnail = generator.GetThumbnailForTime(hoverTime);
                if (null != thumbnail)
                {
                    string fullPath = string.Format(@"{0}\{1}", generator.ImageDirectory, thumbnail.Filename);
                    subControl.Thumbnail.Source = new BitmapImage(new Uri(fullPath));
                    subControl.Thumbnail.Width = thumbnail.Width;
                    subControl.Thumbnail.Height = thumbnail.Height;
                    subControl.Throbber.Visibility = Visibility.Collapsed;
                    subControl.Thumbnail.Visibility = Visibility.Visible;
                }
                else
                {
                    subControl.Throbber.Visibility = Visibility.Visible;
                    subControl.Thumbnail.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                subControl.Throbber.Visibility = Visibility.Collapsed;
                subControl.Thumbnail.Visibility = Visibility.Collapsed;
            }
            subControl.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var height = subControl.ActualHeight + subControl.DesiredSize.Height;
            Debug.WriteLine(string.Format("actual height: {0}, desired: {1}, blah: {2}", subControl.ActualHeight, subControl.DesiredSize.Height, height));
            subControl.Margin = new Thickness(-1 * (subControl.ActualWidth / 2) - 5, -1 * (height), 0, 0);

            popup.Content = subControl;

            position = e.GetPosition(slider);
            position.Y = 0;// slider.ActualHeight / 2.0;
            popup.PlacementOffset = position;

        }
    }
}
