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
using System.Windows.Shapes;
using JayDev.MediaScribe.ViewModel;
using JayDev.MediaScribe.Model;
using System.Collections.ObjectModel;
using LibMPlayerCommon;
using GalaSoft.MvvmLight.Messaging;
using JayDev.MediaScribe.Common;
using System.Diagnostics;
using System.Windows.Interop;
using Microsoft.Windows.Shell;
using Microsoft.Practices.Unity;
using System.Runtime.InteropServices;
using AppLimit.NetSparkle;

namespace JayDev.MediaScribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string DEFAULT_TITLE = "MediaScribe";
        public string DefaultTitle
        {
            get
            {
                return DEFAULT_TITLE;
            }
        }

        private Sparkle _sparkle;

        public MainWindow()
        {
            InitializeComponent();

            _sparkle = new Sparkle("http://mediascribe.jarrod.co.nz/appcast.xml");
            _sparkle.StartLoop(true);

            UnityContainer unityContainer = new UnityContainer();
            unityContainer.RegisterType<IController, Controller>(new ContainerControlledLifetimeManager());

            //JDW NOTE: we need to set the data context BEFORE initializing the controller, since initialization
            //will perform a navigation that we need to catch in mainwindow's viewmodel... but AFTER creating the
            //the controller, since we need its reference inside the viewmodel.
            this.DataContext = new WindowHeaderViewModel(unityContainer);

            IController controller = unityContainer.Resolve<IController>();
            //We need to register the controller with Unity, before calling the initialize method. this is why the the logic isn't housed
            //in the controller constructor.
            controller.Initialize(this, tabControl1, unityContainer);

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }


        /// <summary>
        /// Give the main window the aero 'glass' effect, if possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtain the window handle for WPF application
                IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                
                LinearGradientBrush linear = new LinearGradientBrush();
                linear.StartPoint = new Point(0.06, 0);
                linear.EndPoint = new Point(0.91, 0.99);
                linear.SpreadMethod = GradientSpreadMethod.Pad;
                linear.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
                linear.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xDE, 0xDE, 0xEF), 0));
                linear.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xDE, 0xDE, 0xEF), 1));
                linear.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xDE, 0xDE, 0xEF), 0.81));
                linear.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xE6, 0xE6, 0xF3), 0.66));
                linear.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xCF, 0xCF, 0xE3), 0.46));
                linear.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xDE, 0xDE, 0xEF), 0.2));
                linear.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xCB, 0xCB, 0xE2), 0.4));

                mainWindowSrc.CompositionTarget.BackgroundColor = ColorHelper.FromString("#FFDEE6EF");

                // Get System Dpi
                System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                float DesktopDpiX = desktop.DpiX;
                float DesktopDpiY = desktop.DpiY;

                // Set Margins
                NonClientRegionAPI.MARGINS margins = new NonClientRegionAPI.MARGINS();

                // Extend glass frame into client area 
                // Note that the default desktop Dpi is 96dpi. The  margins are 
                // adjusted for the system Dpi.
                margins.cxLeftWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
                margins.cxRightWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
                margins.cyTopHeight = Convert.ToInt32(((int)tabControl1.ActualHeight + 5) * (DesktopDpiX / 96));
                margins.cyBottomHeight = Convert.ToInt32(5 * (DesktopDpiX / 96));

                int hr = NonClientRegionAPI.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                // 
                if (hr < 0)
                {
                    //DwmExtendFrameIntoClientArea Failed
                }
            }
            // If not Vista, paint background white. 
            catch (DllNotFoundException)
            {
                Application.Current.MainWindow.Background = new SolidColorBrush(ColorHelper.FromString("#FFDEE6EF"));
            }
        }

        class NonClientRegionAPI
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct MARGINS
            {
                public int cxLeftWidth;      // width of left border that retains its size 
                public int cxRightWidth;     // width of right border that retains its size 
                public int cyTopHeight;      // height of top border that retains its size 
                public int cyBottomHeight;   // height of bottom border that retains its size
            };


            [DllImport("DwmApi.dll")]
            public static extern int DwmExtendFrameIntoClientArea(
                IntPtr hwnd,
                ref MARGINS pMarInset);
        }

        private void _OnSystemCommandCloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow((Window)e.Parameter);
        }
    }
}
