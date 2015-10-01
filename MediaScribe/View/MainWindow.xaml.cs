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
using GalaSoft.MvvmLight.Messaging;
using JayDev.MediaScribe.Common;
using System.Diagnostics;
using System.Windows.Interop;
using Microsoft.Windows.Shell;
using Microsoft.Practices.Unity;
using System.Runtime.InteropServices;
using AppLimit.NetSparkle;
using MahApps.Metro.Controls;

namespace JayDev.MediaScribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Sparkle _sparkle;
        IController controller;

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

            controller = unityContainer.Resolve<IController>();
            //We need to register the controller with Unity, before calling the initialize method. this is why the the logic isn't housed
            //in the controller constructor.
            controller.Initialize(this, mainTabControl, unityContainer);


        }


        private void _OnSystemCommandCloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow((Window)e.Parameter);
        }

        void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if we're navigating programmatically, it'll cause the tab's selection to change. We want to ignore it, instead of triggering new navigation.
            if ((mainTabControl.Tag as string) == Constants.SUPPRESS_NAVIGATION_TAB_CONTROL)
                return;

            if (e.RemovedItems.Count == 1 && e.AddedItems.Count == 1 && e.AddedItems[0] is TabItem)
            {
                string selectedTab = (e.AddedItems[0] as TabItem).Header as string;
                WindowHeaderViewModel viewModel = this.DataContext as WindowHeaderViewModel;
                switch (selectedTab)
                {
                    case "Course List":
                        viewModel.NavigateCommand.Execute(NavigateMessage.ListCourses);
                        break;
                    case "Settings":
                        viewModel.NavigateCommand.Execute(NavigateMessage.Settings);
                        break;
                    case "Write Notes":
                        viewModel.NavigateCommand.Execute(NavigateMessage.WriteCourseNotes);
                        break;
                }
            }
        }
    }
}
