using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.Timers;

using JayDev.MediaScribe.Common;
using System.ComponentModel;
using GalaSoft.MvvmLight.Messaging;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// Interaction logic for VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl
    {
        public double? AspectRatio
        {
            get { return (double?)GetValue(AspectRatioProperty); }
            set { SetValue(AspectRatioProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AspectRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AspectRatioProperty =
            DependencyProperty.Register("AspectRatio", typeof(double?), typeof(VideoControl), new UIPropertyMetadata(null));

        




        public delegate void DoubleClickHandler();
        public event DoubleClickHandler OnDoubleClick;

        private const double defaultAspectRatio = 1.7777d;//16:9



        public IntPtr VideoPanelPointer
        {
            get { return (IntPtr)GetValue(VideoPanelPointerProperty); }
            set { SetValue(VideoPanelPointerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoPanelPointer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoPanelPointerProperty =
            DependencyProperty.Register("VideoPanelPointer", typeof(IntPtr), typeof(VideoControl), new UIPropertyMetadata(null));

        


        public bool IsShowingVideo
        {
            get { return (bool)GetValue(IsShowingVideoProperty); }
            set { SetValue(IsShowingVideoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShowingVideo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowingVideoProperty =
            DependencyProperty.Register("IsShowingVideo", typeof(bool), typeof(VideoControl), new UIPropertyMetadata(true));




        public bool EnableAutohideControls
        {
            get { return (bool)GetValue(EnableAutohideControlsProperty); }
            set { SetValue(EnableAutohideControlsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableAutohideControls.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableAutohideControlsProperty =
            DependencyProperty.Register("EnableAutohideControls", typeof(bool), typeof(VideoControl), new UIPropertyMetadata(null));


        #region PlayPauseCommand

        public static readonly DependencyProperty PlayPauseCommandqProperty =
            DependencyProperty.Register("PlayPauseCommand", typeof(ICommand), typeof(VideoControl));

        /// <summary>
        /// Gets the PlayCommand.
        /// </summary>
        public ICommand PlayPauseCommand
        {
            get { return (ICommand)GetValue(PlayPauseCommandqProperty); }
            set { SetValue(PlayPauseCommandqProperty, value); }
        }

        #endregion


        public VideoControl()
        {
            InitializeComponent();

            //since we only have one instance of mplayer running, we have to share the control around the rest of the application. this 
            //method yanks the video from whoever has it, and gives it to this control.
            AssociateVideoWithControl();

            //Ensure that when the video panel pointer property of the native win32 control is updated, we update our reference to it.
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(MediaPlayerWPFDisplayControl.VideoPanelHandlePropertyProperty, typeof(MediaPlayerWPFDisplayControl));
            if (dpd != null)
            {
                dpd.AddValueChanged(MediaPlayerWPFDisplayControl.Instance, delegate
                {
                    VideoPanelPointer = MediaPlayerWPFDisplayControl.Instance.VideoPanelHandleProperty;
                });
            }
        }

        /// <summary>
        /// since we only have one instance of mplayer running, we have to share the control around the rest of the application. this 
        /// method yanks the video from whoever has it, and gives it to this control.
        /// </summary>
        public void AssociateVideoWithControl()
        {
            var mPlayerWPFControl1 = MediaPlayerWPFDisplayControl.Instance;

            //since we re-use the video control (since we don't want to keep on re-running mplayer), we need to ensure we disconnect the
            //panel from its existing parent, before attaching it to us.
            if (null != mPlayerWPFControl1.Parent)
            {
                (mPlayerWPFControl1.Parent as ContentControl).Content = null;
            }

            this.videoPlaceholder.Content = mPlayerWPFControl1;

            //hook up the event handler so the user can double-click in the panel to toggle fullscreen
            MediaPlayerWPFDisplayControl.Instance.OnDoubleClick -= mPlayerWPFControl1_OnDoubleClick;
            MediaPlayerWPFDisplayControl.Instance.OnDoubleClick += new MediaPlayerWPFDisplayControl.DoubleClickHandler(mPlayerWPFControl1_OnDoubleClick);

            //ensure that the panel is the right size.
            SetPanelSizeForAspectRatio();

            if (EnableAutohideControls)
            {
                MediaPlayerWPFDisplayControl.Instance.EnableAutoHide();
            }
            else
            {
                MediaPlayerWPFDisplayControl.Instance.DisableAutoHide();
            }
        }


        void mPlayerWPFControl1_OnDoubleClick(MediaPlayerWPFDisplayControl.DoubleClickEventArgs args)
        {

            Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen, TabChangeSource.Application), MessageType.Navigate);
        }


        public enum VideoControlMode { Embedded, Collapsed, Fullscreen }

        public void SetMode(VideoControlMode mode)
        {
            if (mode == VideoControlMode.Embedded)
            {
                ControlPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else if (mode == VideoControlMode.Fullscreen)
            {
                ControlPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPanelSizeForAspectRatio(e.NewSize.Width, e.NewSize.Height);
        }

        public void SetPanelSizeForAspectRatio()
        {
            SetPanelSizeForAspectRatio(this.ActualWidth, this.ActualHeight);
        }
        private void SetPanelSizeForAspectRatio(double newWidth, double newHeight)
        {
                double aspectRatio = AspectRatio ?? defaultAspectRatio;
                //this.play.ForceAspectRatio(this.panelVideo.Width, this.panelVideo.Height);
                double width = newWidth;
                double height = newHeight - this.ControlPanel.ActualHeight;
                double correctedWidth = -1;
                double correctedHeight = -1;
                float panelAspectRatio = (float)width / (float)height;
                if (panelAspectRatio >= aspectRatio)
                {
                    correctedHeight = height;
                    correctedWidth = Convert.ToInt32((float)height * aspectRatio);
                    MediaPlayerWPFDisplayControl.Instance.Width = correctedWidth;
                    MediaPlayerWPFDisplayControl.Instance.Height = correctedHeight;
                }
                else
                {
                    correctedWidth = width;
                    correctedHeight = Convert.ToInt32((float)width / aspectRatio);
                    MediaPlayerWPFDisplayControl.Instance.Height = correctedHeight;
                    MediaPlayerWPFDisplayControl.Instance.Width = correctedWidth;
                }
        }



    }
}
