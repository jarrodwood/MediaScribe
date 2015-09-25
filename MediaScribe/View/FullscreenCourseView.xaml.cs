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
using JayDev.MediaScribe.ViewModel;
using System.Timers;
using JayDev.MediaScribe.View.Controls;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using JayDev.MediaScribe.Common;
using JayDev.MediaScribe.Core;
using MediaScribe.Common;

namespace JayDev.MediaScribe.View
{
    /// <summary>
    /// Interaction logic for FullscreenCourseView.xaml
    /// </summary>
    public partial class FullscreenCourseView : UserControl
    {
        private CourseUseViewModel _viewModel;
        private readonly Dispatcher _currentDispatcher;

        public FullscreenCourseView(CourseUseViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            this.DataContext = _viewModel;
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            Messenger.Default.Register<ShowMessage>(this, MessageType.SetFullscreenMode, (message) => HandleShowMessage(message));

            mediaElement.OnMouseDoubleClick += mediaElement_OnMouseDoubleClick;
            mediaElement.MediaPlayer = LibVLC.NET.Presentation.MediaElement.SingletonPlayer;

            AutoHideInit();
        }

        public void Detach()
        {
            mediaElement.Detach();
        }

        public void Attach()
        {
            mediaElement.Attach();
        }

        void mediaElement_OnMouseDoubleClick(object sender, EventArgs e)
        {
            //mediaElement.Detach();
            Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen, TabChangeSource.Application), MessageType.PerformNavigation);
        }

        public void HandleWindowKeypress(object sender, KeyEventArgs e)
        {
            CourseUseView.HandleWindowKeypressForBothViews(sender, e, _currentDispatcher, notesGrid, _viewModel, this.IsVisible, SendShowMessage);
        }

        public void HideControls()
        {
        }

        public void ShowControls()
        {
        }

        void FullscreenCourseView_KeyDown(object sender, KeyEventArgs e)
        {
            Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen, TabChangeSource.Application), MessageType.PerformNavigation);
        }

        private void SendShowMessage(ShowMessage message)
        {
            Messenger.Default.Send(message, MessageType.SetFullscreenMode);
        }

        private void HandleShowMessage(ShowMessage message)
        {
            ReceiveHotkeyHideNotification(message);
            Debug.WriteLine("Handling message: " + message.Show.ToString() + ", source: " + message.Source.ToString());
            if (message.Show)
            {
                if (this.notesGrid.Visibility != Visibility.Visible)
                {
                        this.mediaControls.Visibility = System.Windows.Visibility.Visible;
                        this.notesGrid.Visibility = System.Windows.Visibility.Visible;
                        Mouse.OverrideCursor = null;
                        Debug.WriteLine(DateTime.Now.ToLongTimeString() + " override - show");
                }
            }
            else
            {
                if (this.notesGrid.Visibility != System.Windows.Visibility.Collapsed)
                {
                    //needs to run under UI thread
                    ThreadHelper.ExecuteSyncUI(_currentDispatcher, delegate
                    {
                        this.mediaControls.Visibility = System.Windows.Visibility.Collapsed;
                        this.notesGrid.Visibility = System.Windows.Visibility.Collapsed;
                        Mouse.OverrideCursor = Cursors.None;
                        Debug.WriteLine(DateTime.Now.ToLongTimeString() + " override - none");
                    });
                }
            }
        }

        #region auto-hide code
        /*********************
         * When the user hovers their cursor in the VIDEO, in full-screen mode (i.e. not over the controls, if they moved their mouse there
         * to type a note), we automatically hide the controls after half a second. Unfortunately, it's not SO simple... because when you
         * resize the panel, it creates a 'mouse move' event (presumably because the mouse moves relative to the panel)... and while we
         * receive notification that the mouse has moved OUT of the video, we don't receive notification that the mouse has moved INTO the
         * panel.
         * 
         * So... here's a little table of how this is going to work:
         * User hits create note hotkey:            We show the controls, and allow one 'buffer' mouse-move event before we begin the timer to hide the controls
         * User hits commit/cancel note hotkey:     We hide the controls, and allow one 'buffer' mouse-move event before we show the controls immediately
         * Go Fullscreen:                           Let nature take its course
         * Leave Fullscreen:                        Not our concern
         * Mouse move somewhere IN the video panel: Stop & re-start the auto-hide timer... so when the mouse stops moving for 500ms, the controls hide
         * mouse move OUT OF the video panel:       stop the auto-hide timer.
         * 
         * The 'buffer' is going to be handled with an int, called '_bufferCountdown'. When the above events occur that require a buffer,
         * the value of '_bufferCountdown' will be set to two... and every mouse-move event will reduce the value by 1. Once it reaches 0,
         * the auto-hide timer will begin.
         *********************/

        /// <summary>
        /// The default number to reset the '_bufferCountdown' variable to.
        /// </summary>
        const int COUNTDOWN_DEFAULT = 2;
        /// <summary>
        /// The 'buffer' used, to allow us to gracefully accept a single 'mouse-move' event when we show or hide the controls, without
        /// triggering any further actions.
        /// </summary>
        int _bufferCountdown = COUNTDOWN_DEFAULT;
        Timer hoverTimer;
        Point lastMoveLocation = new Point();

        /// <summary>
        /// The timer to auto-hide the controls has elapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void hoverTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Messenger.Default.Send(new ShowMessage() { Show = false, Source = ShowSource.MouseStoppedTimer }, MessageType.SetFullscreenMode);
            Logging.Log(LoggingSource.MPlayerWindow, "HIDING NOW");
            _bufferCountdown = COUNTDOWN_DEFAULT;
        }

        void AutoHideInit()
        {
            hoverTimer = new Timer();
            hoverTimer.AutoReset = false;
            hoverTimer.Elapsed += new ElapsedEventHandler(hoverTimer_Elapsed);
            hoverTimer.Interval = 500;

            this.mediaElement.MouseMove += mediaElement_MouseMove;
            this.mediaElement.MouseLeave += mediaElement_MouseLeave;
        }

        /// <summary>
        /// We've received notification from the FullScreen course view, that a hotkey combo has been pressed which will affect the showing
        /// of the controls
        /// </summary>
        /// <param name="show"></param>
        public void ReceiveHotkeyHideNotification(ShowMessage show)
        {
            if (show.Source == ShowSource.Hotkey)
            {
                _bufferCountdown = COUNTDOWN_DEFAULT;
            }
        }

        public void EnableAutoHide()
        {
            _autohide = true;
        }

        public void DisableAutoHide()
        {
            _autohide = false;
            if (null != hoverTimer)
            {
                hoverTimer.Stop();
            }
        }

        private bool _autohide = true;

        void mediaElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (_autohide)
            {
                //for a mouse-move event, the location of the cursor is stored as an int in the lParam parameter.
                Point currentMoveLocation = Mouse.GetPosition(mediaElement);
                if (currentMoveLocation != lastMoveLocation)
                {
                    //if the coundown falls to zero, we've used the buffer and the user is obviously using the mouse. So show the controls,
                    //and stop & start the auto-hide timer
                    if (_bufferCountdown <= 0)
                    {
                        Messenger.Default.Send(new ShowMessage() { Show = true, Source = ShowSource.MouseMove }, MessageType.SetFullscreenMode);
                        hoverTimer.Stop();
                        hoverTimer.Start();
                    }
                    else
                    {
                        _bufferCountdown--;
                    }

                    lastMoveLocation = Mouse.GetPosition(mediaElement);
                }
            }
        }

        void mediaElement_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_autohide)
            {
                hoverTimer.Stop();
            }
        }
        #endregion
    }
}
