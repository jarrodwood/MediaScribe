using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Diagnostics;
using System;
using JayDev.MediaScribe.Common;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Timers;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// Visualiztion control
    /// </summary>
    public class MediaPlayerWPFDisplayControl : HwndHost
    {
        public bool IsVideoPanelInitialized { get; private set; }

        private static MediaPlayerWPFDisplayControl instance;

        public static MediaPlayerWPFDisplayControl Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MediaPlayerWPFDisplayControl();
                }
                return instance;
            }
        }

        /// <summary>
        /// Visible constant
        /// </summary>
        const int WS_VISIBLE = 0x10000000;

        /// <summary>
        /// Child constant
        /// </summary>
        const int WS_CHILD =   0x40000000;

        /// <summary>
        /// Double-CLick Window Class Style Constant - ref: http://msdn.microsoft.com/en-us/library/windows/desktop/ff729176(v=vs.85).aspx
        /// </summary>
        const int CS_DBLCLKS = 0x0008;

        private HwndSource _source;

        public class DoubleClickEventArgs {
        }
        public delegate void DoubleClickHandler(DoubleClickEventArgs args);

        public event DoubleClickHandler OnDoubleClick;


        private MediaPlayerWPFDisplayControl()
        {
        }


        public IntPtr VideoPanelHandleProperty
        {
            get { return (IntPtr)GetValue(VideoPanelHandlePropertyProperty); }
            set { SetValue(VideoPanelHandlePropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoPanelProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoPanelHandlePropertyProperty =
            DependencyProperty.Register("VideoPanelHandleProperty", typeof(IntPtr), typeof(MediaPlayerWPFDisplayControl), new UIPropertyMetadata(null));


        /// <summary>
        /// Create handle
        /// </summary>
        /// <param name="hwndParent">Parent handle</param>
        /// <returns>Window handle</returns>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var parameters = new HwndSourceParameters
            {
                ParentWindow = hwndParent.Handle,
                WindowStyle = WS_VISIBLE | WS_CHILD,
                WindowClassStyle = CS_DBLCLKS //We have to register this window to receive double-click events >< ref: http://msdn.microsoft.com/en-us/library/windows/desktop/ff729176(v=vs.85).aspx
            };
            parameters.SetSize(100, 100);

            _source = new HwndSource(parameters);
            //var border = new Border();
            Grid panel = new Grid();
            _source.AddHook(WndProc);
            //border.Child = panel;
            _source.RootVisual = panel;


            hoverTimer = new Timer();
            hoverTimer.AutoReset = false;
            hoverTimer.Elapsed += new ElapsedEventHandler(hoverTimer_Elapsed);
            hoverTimer.Interval = 500;

            SetValue(VideoPanelHandlePropertyProperty, _source.Handle);

            IsVideoPanelInitialized = true;
            return new HandleRef(null, _source.Handle);
        }

        /// <summary>
        //// Destroy handle
        /// </summary>
        /// <param name="hwnd">Parent Handle</param>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            _source.Dispose();
        }

        #region Handle hide-controls-after-half-second-of-cursor-in-video code
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
        int lastMoveLocation = 0;

        /// <summary>
        /// The timer to auto-hide the controls has elapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void hoverTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Messenger.Default.Send(new ShowMessage() { Show = false, Source = ShowSource.MouseStoppedTimer }, 12345);
            Debug.WriteLine("HIDING NOW");
            _bufferCountdown = COUNTDOWN_DEFAULT;
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

        /// <summary>
        /// WinAPI event handler, receiving messages for mouse-related events in the video panel
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(msg != 132 && msg != 70)
            {
                Debug.WriteLine(string.Format("Msg: {0}, wParam: {1}, lParam: {2}, handled? {3}", msg, wParam.ToInt32(), lParam.ToInt32(), handled));
            }
            if (msg == (int)WM_Messages.WM_MOUSEMOVE)
            {
                //for a mouse-move event, the location of the cursor is stored as an int in the lParam parameter.
                int currentMoveLocation = lParam.ToInt32();
                if (currentMoveLocation != lastMoveLocation)
                {
                    //if the coundown falls to zero, we've used the buffer and the user is obviously using the mouse. So show the controls,
                    //and stop & start the auto-hide timer
                    if (_bufferCountdown <= 0)
                    {
                        Messenger.Default.Send(new ShowMessage() { Show = true, Source = ShowSource.MouseMove }, 12345);
                        hoverTimer.Stop();
                        hoverTimer.Start();
                    }
                    else
                    {
                        _bufferCountdown--;
                    }

                    lastMoveLocation = currentMoveLocation;
                }
            }

            //if the cursor's left the video panel, stop the auto-hide timer.
            if (msg == (int)WM_Messages.WM_MOUSELEAVE)
            {
                hoverTimer.Stop();
            }

            //if the user double-clicks in the video panel, toggle fullscreen
            else if (msg == (int)WM_Messages.WM_LBUTTONDBLCLK)
            {
                Debug.WriteLine("double-click in video panel");
                Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen), MessageType.Navigate);

                //JDW: have to set it to handled, otherwise it fires the event twice.
                handled = true;
            }
            return IntPtr.Zero;
        }

        #endregion
    }
}
