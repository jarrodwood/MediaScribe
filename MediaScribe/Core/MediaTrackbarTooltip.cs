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
using JayDev.MediaScribe.Common;
using System.Windows.Threading;

namespace JayDev.MediaScribe.Core
{
    /// <summary>
    /// Helper class: contains code related to a 'preview' box when hovering the cursor over a WPF Slider. This is initially used in
    /// MediaScribe to view the current track's time at the cursor's position (e.g. hover at the start of the Slider, and the time will be
    /// at or near 0:00, at the end of the file the time will be the full track's duration, hover at the center and the time will show half
    /// of the track's duration)
    /// </summary>
    public static class MediaTrackbarTooltip
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
            typeof(MediaTrackbarTooltip),
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
            typeof(MediaTrackbarTooltip),
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
            typeof(MediaTrackbarTooltip),
            new UIPropertyMetadata(null));
        #endregion


        static TrackbarPreview subControl = null;

        ////Internal Dependancy property
        //#region PopupAdorner
        //private static ContentAdorner GetPopupAdorner(Slider slider)
        //{
        //    return (ContentAdorner)slider.GetValue(PopupAdornerProperty);
        //}

        //private static readonly DependencyProperty PopupAdornerProperty = DependencyProperty.RegisterAttached(
        //    "PopupAdorner",
        //    typeof(ContentAdorner),
        //    typeof(MediaTrackbarTooltip),
        //    new UIPropertyMetadata(null));
        //#endregion

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

        static ToolTip tooltip = null;

        /// <summary>
        /// When the cursor leaves the control, hide the popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void item_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Slider slider = sender as Slider;
            //ContentAdorner popup = GetPopupAdorner(slider);
            //popup.Visibility = Visibility.Collapsed;
            if (null != tooltip)
            {
                tooltip.IsOpen = false;
                //ensure that we clear the thumbnail source, to release the file lock so it can be deleted.
                subControl.Thumbnail.Source = null;
            }
        }

        static TimeSpan GetHoverTime(Slider slider, Point position, System.Windows.Controls.Primitives.Track _track)
        {
            double value = _track.ValueFromPoint(position);

            if (slider.SmallChange != 0.0)
            {
                double diff = value % slider.SmallChange;
                value -= diff;
            }

            double displayValue = Math.Max(slider.Minimum, Math.Min(slider.Maximum, value));
            TimeSpan hoverTime = new TimeSpan(0, 0, (int)displayValue + 1);
            return hoverTime;
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
            var generator = GetThumbnailGenerator(slider);
            //before we do anything, clear any existing request to be notified when a certain thumbnail is generated.
            generator.NotifyWhenThumbnailReady = null;


            System.Windows.Controls.Primitives.Track _track = slider.Template.FindName("PART_Track", slider) as System.Windows.Controls.Primitives.Track;
            Point position = e.MouseDevice.GetPosition(_track);

            TimeSpan hoverTime = GetHoverTime(slider, position, _track);


            if (null == subControl)
                subControl = new TrackbarPreview();


            subControl.CurrentPlayTime = hoverTime;

            if (generator.IsTrackVideo)
            {
                Thumbnail thumbnail = generator.GetThumbnailForTime(hoverTime);
                if (null != thumbnail)
                {
                    string fullPath = string.Format(@"{0}\{1}", thumbnail.FileDirectory, thumbnail.Filename);
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

                    var uiDispatcher = Dispatcher.CurrentDispatcher;
                    generator.NotifyWhenThumbnailReady = new Tuple<TimeSpan, Action<Thumbnail>>
                        (hoverTime, (notifyThumb) =>
                    {
                        ThreadHelper.ExecuteSyncUI(uiDispatcher, delegate
                        {
                            subControl.Throbber.Visibility = Visibility.Collapsed;
                            subControl.Thumbnail.Visibility = Visibility.Visible;
                            string fullPath = string.Format(@"{0}\{1}", notifyThumb.FileDirectory, notifyThumb.Filename);
                            subControl.Thumbnail.Source = new BitmapImage(new Uri(fullPath));
                            subControl.Thumbnail.Width = notifyThumb.Width;
                            subControl.Thumbnail.Height = notifyThumb.Height;

                            SetControlSize(slider, position, true);
                        });
                    });
                }
            }
            else
            {
                subControl.Throbber.Visibility = Visibility.Collapsed;
                subControl.Thumbnail.Visibility = Visibility.Collapsed;
            }

            SetControlSize(slider, position, false);
        }

        private static void SetControlSize(Slider slider, Point position, bool isFromNotification)
        {
            //TODO: fix this hack. The dropshadow is set in the TrackbarPreview, we should just grab the value from there.
            int marginForTooltipDropshadow = 10;

            subControl.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var height = subControl.DesiredSize.Height + marginForTooltipDropshadow;
            Logging.Log(LoggingSource.ThumbnailGeneration, string.Format("actual height: {0}, desired: {1}, blah: {2}", subControl.ActualHeight, subControl.DesiredSize.Height, height));
            //subControl.Margin = new Thickness(-1 * (subControl.ActualWidth / 2) - 5, -1 * (height), 0, 0);

            Logging.Log(LoggingSource.ThumbnailGeneration, string.Format("actual width: {0}, desired: {1}, thumb: {2}", subControl.ActualWidth, subControl.DesiredSize.Width, subControl.Thumbnail.Width));


            if (null == tooltip)
            {
                tooltip = new ToolTip
                {
                    Content = subControl,
                    OverridesDefaultStyle = true,
                    Height = height + marginForTooltipDropshadow,
                    Width = subControl.DesiredSize.Width,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0)
                };
            }
            ToolTipService.SetToolTip(slider, tooltip);
            ToolTipService.SetInitialShowDelay(slider, 0);

            slider.ToolTip = tooltip;

            position.Y = 0;

            var width = subControl.DesiredSize.Width + marginForTooltipDropshadow;
            //TODO: remove this hack, figure out why when we use the notification, the control's desiredwidth
            //      is often smaller than the thumbnail it contains.
            if (subControl.Thumbnail.Visibility == Visibility.Visible)
                width = subControl.Thumbnail.Width + marginForTooltipDropshadow;

            tooltip.Width = width;
            tooltip.Height = height;
            tooltip.PlacementTarget = slider;

            tooltip.Placement = PlacementMode.Top;
            tooltip.HorizontalOffset = position.X - (tooltip.Width / 2) + (marginForTooltipDropshadow / 2);
            tooltip.IsOpen = true;
        }
    }
}
