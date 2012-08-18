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

        public MainWindow()
        {
            InitializeComponent();

            IUnityContainer myContainer = new UnityContainer();

            Controller controller = new Controller();
            myContainer.RegisterInstance<Controller>(controller);
            controller.Initialize(this);
        }
    }
}
