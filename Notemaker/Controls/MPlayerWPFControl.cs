using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Diagnostics;
using System;

namespace JayDev.Notemaker.View.Controls
{
    /// <summary>
    /// Visualiztion control
    /// </summary>
    public class MPlayerWPFControl : HwndHost
    {
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

            Debug.WriteLine("shit.");
            return new HandleRef(null, _source.Handle);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...

            //if (msg != 32 && msg != 132 && msg != 512 && msg != 33)
            //{
            //    Debug.WriteLine(string.Format("Msg: {0}, wParam: {1}, lParam: {2}, handled? {3}", msg, wParam.ToInt32(), lParam.ToInt32(), handled));
            //}
            if (msg == (int)Constants.WM_Messages.WM_LBUTTONDBLCLK)
            {
                OnDoubleClick(new DoubleClickEventArgs());
                Debug.WriteLine("double-click! yay!");

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
