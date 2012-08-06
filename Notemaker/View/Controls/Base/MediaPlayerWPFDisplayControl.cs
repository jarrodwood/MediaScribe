using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Diagnostics;
using System;
using JayDev.Notemaker.Common;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Timers;
using JayDev.Notemaker.Core;

namespace JayDev.Notemaker.View.Controls
{
    /// <summary>
    /// Visualiztion control
    /// </summary>
    public class MediaPlayerWPFDisplayControl : HwndHost
    {
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
            return new HandleRef(null, _source.Handle);
        }

        void hoverTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Messenger.Default.Send("hide", 12345);
            Debug.WriteLine("HIDING NOW");
            countDown = 10;
        }

        int countDown = 0;
        Timer hoverTimer;
        int lastMoveLocation = 0;
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...

            if(msg != 132 && msg != 70)
            //if (msg != 32 && msg != 132 && msg != 512 && msg != 33)
            {
                Debug.WriteLine(string.Format("Msg: {0}, wParam: {1}, lParam: {2}, handled? {3}", msg, wParam.ToInt32(), lParam.ToInt32(), handled));
            }
            if (msg == (int)WM_Messages.WM_MOUSEMOVE)
            {
                if (lParam.ToInt32() != lastMoveLocation)
                {
                    if (countDown <= 0)
                    {
                        Debug.WriteLine("SHOW - from timer");
                        Messenger.Default.Send("show", 12345);
                        hoverTimer.Stop();
                        hoverTimer.Start();
                    }
                    else
                    {
                        countDown--;
                    }
                    lastMoveLocation = lParam.ToInt32();
                }
            }
            if (msg == (int)WM_Messages.WM_MOUSELEAVE)
            {
                countDown = 10;
                hoverTimer.Stop();
            }
            else if (msg == (int)WM_Messages.WM_LBUTTONDBLCLK)
            {
                //OnDoubleClick(new DoubleClickEventArgs());
                Debug.WriteLine("double-click! yay!");
                Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen), MessageType.Navigate);

                //JDW: have to set it to handled, otherwise it fires the event twice.
                handled = true;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        //// Destroy handle
        /// </summary>
        /// <param name="hwnd">Parent Handle</param>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            _source.Dispose();
        }
    }
}
