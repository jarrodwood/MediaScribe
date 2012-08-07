/******
 * 
 * SOURCE: http://www.codeproject.com/Articles/22820/Adding-Glass-Effect-to-WPF-using-Attached-Properti
 * 
 ******/

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
using System.Runtime.InteropServices;
using System.Windows.Interop;


namespace JayDev.MediaScribe.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    };

    public class GlassEffect
    {
        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled",
                typeof(Boolean),
                typeof(GlassEffect),
                new FrameworkPropertyMetadata(OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, Boolean value)
        {
            element.SetValue(IsEnabledProperty, value);
        }
        public static Boolean GetIsEnabled(DependencyObject element)
        {
            return (Boolean)element.GetValue(IsEnabledProperty);
        }

        public static void OnIsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue == true)
            {
                Window wnd = (Window)obj;
                wnd.Loaded += new RoutedEventHandler(wnd_Loaded);
            }
        }

        static void wnd_Loaded(object sender, RoutedEventArgs e)
        {
            Window wnd = (Window)sender;
            Brush originalBackground = wnd.Background;
            wnd.Background = Brushes.Transparent;
            try
            {
                IntPtr mainWindowPtr = new WindowInteropHelper(wnd).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                //System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                //float DesktopDpiX = desktop.DpiX;
                //float DesktopDpiY = desktop.DpiY;

                MARGINS margins = new MARGINS();
                margins.cxLeftWidth = -1;
                margins.cxRightWidth = -1;
                margins.cyTopHeight = -1;
                margins.cyBottomHeight = -1;

                DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
            }
            catch (DllNotFoundException)
            {
                wnd.Background = originalBackground;
            }
        }





        [DllImport("dwmapi.dll")]
        extern static int DwmIsCompositionEnabled(ref int en);

        /// <summary>
        /// Extends the glass area into the client area of the window
        /// </summary>
        /// <param name="window"></param>
        /// <param name="top"></param>
        public static void ExtendGlass(Window window, Thickness thikness)
        {
            try
            {
                int isGlassEnabled = 0;
                DwmIsCompositionEnabled(ref isGlassEnabled);
                if (Environment.OSVersion.Version.Major > 5 && isGlassEnabled > 0)
                {
                    // Get the window handle
                    WindowInteropHelper helper = new WindowInteropHelper(window);
                    HwndSource mainWindowSrc = (HwndSource)HwndSource.
                        FromHwnd(helper.Handle);
                    mainWindowSrc.CompositionTarget.BackgroundColor =
                        Color.FromArgb(0,0,0,0);

                    // Get the dpi of the screen
                    System.Drawing.Graphics desktop =
                       System.Drawing.Graphics.FromHwnd(mainWindowSrc.Handle);
                    float dpiX = desktop.DpiX / 96;
                    float dpiY = desktop.DpiY / 96;

                    // Set Margins
                    MARGINS margins = new MARGINS();
                    margins.cxLeftWidth = (int)(0 * dpiX);
                    margins.cxRightWidth = (int)(0 * dpiX);
                    margins.cyBottomHeight = (int)(thikness.Top * dpiY*1.2 * 50);
                    margins.cyTopHeight = (int)(thikness.Top * dpiY);

                    window.Background = Brushes.Transparent;

                    int hr = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle,
                                ref margins);
                }
                else
                {
                    window.Background = SystemColors.WindowBrush;
                }
            }
            catch (DllNotFoundException)
            {

            }
        }

    }

}
