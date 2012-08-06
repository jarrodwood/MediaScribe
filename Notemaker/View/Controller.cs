using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using JayDev.Notemaker.Common;
using JayDev.Notemaker.ViewModel;
using JayDev.Notemaker.Model;
using GalaSoft.MvvmLight.Messaging;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;
//using log4net.Config;
using JayDev.Notemaker.Core;

namespace JayDev.Notemaker.View
{
    public class Controller
    {
        private static MainWindow _mainWindow;
        private static Microsoft.Practices.Unity.UnityContainer _container = new Microsoft.Practices.Unity.UnityContainer();
        private static CourseUseViewModel courseUseViewModel;
        private static CourseListViewModel courseListViewModel;
        private static SettingsViewModel settingsViewModel;
        private static UserControl currentView;

        private static Controller _instance;
        public static Controller Singleton
        {
            get
            {
                if (null == _instance)
                    _instance = new Controller();
                return _instance;
            }
        }

        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _mainWindow.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            //var logconfig = new System.IO.FileInfo("log4net.xml");
            //if (logconfig.Exists)
            //{
            //    log4net.Config.XmlConfigurator.ConfigureAndWatch(logconfig);
            //}  

            Console.SetOut(new DebugTextWriter());

            CourseRepository repo = new CourseRepository();
            courseUseViewModel = new CourseUseViewModel(repo);
            courseListViewModel = new CourseListViewModel(repo);
            settingsViewModel = new SettingsViewModel(new SettingRepository());
            var blah = repo.GetCourseList();

            var currentCourse = blah.First(x => x.Name == "Daygame");
            courseUseViewModel.SetCurrentCourse(currentCourse);
            CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
            _mainWindow.Content = courseUseView;
            currentView = courseUseView;
            Messenger.Default.Register<NavigateArgs>(Singleton, MessageType.Navigate, (message) => Navigate(message));
            Messenger.Default.Register<string>(Singleton, "errors", (error) => MessageBox.Show(error));

            _mainWindow.Closing += new System.ComponentModel.CancelEventHandler(_mainWindow_Closing);
        }

        void _mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                //save the course -- specifically intended for the embedded video height/width.
                courseUseViewModel.SaveCourseCommand.Execute(null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        static void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //TODO: handle lower-level (so can use keypad keys, regardless of numlock status). could try http://stackoverflow.com/a/5989521

            if (currentView is FullscreenCourseView)
            {
                (currentView as FullscreenCourseView).HandleKeypress(sender, e);
            }

            courseUseViewModel.HandleKeypress(sender, e);
        }


        static WindowStyle preFullscreenWindowStyle = WindowStyle.SingleBorderWindow;
        static WindowState preFullscreenWindowState = WindowState.Maximized;


        private static void Navigate(NavigateArgs args)
        {
            switch(args.Message) {
                case NavigateMessage.ToggleFullscreen:
                    if (currentView is CourseUseView)
                    {
                        preFullscreenWindowStyle = _mainWindow.WindowStyle;
                        preFullscreenWindowState = _mainWindow.WindowState;

                        FullscreenCourseView fscView = new FullscreenCourseView(courseUseViewModel);
                        _mainWindow.Content = fscView;
                        currentView = fscView;

                        if (_mainWindow.WindowState == System.Windows.WindowState.Maximized)
                        {
                            //JDW: have to set winbdowState to normal first, otherwise WPF will still show the windows taskbar
                            _mainWindow.WindowState = System.Windows.WindowState.Normal;
                        }
                        _mainWindow.WindowStyle = System.Windows.WindowStyle.None;
                        //this.Topmost = true;
                        _mainWindow.WindowState = System.Windows.WindowState.Maximized;
                    }
                    else
                    {
                        CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
                        _mainWindow.Content = courseUseView;
                        currentView = courseUseView;

                        _mainWindow.WindowStyle = preFullscreenWindowStyle;
                        _mainWindow.Topmost = false;
                        _mainWindow.WindowState = preFullscreenWindowState;

                        //ensure that the mouse cursor is visible. this is a bit of a hack, since interacting with the win32 control is a PITA... and if the cursor was hidden when we left fullscreen,
                        //it'll stay hidden until it moves back over the win32 control.
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case NavigateMessage.WriteCourseNotes:
                    {
                        CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
                        currentView = courseUseView;
                        _mainWindow.Content = currentView;
                        courseUseViewModel.SetCurrentCourse(args.Course);
                    }
                    break;
                case NavigateMessage.ListCourses:
                    CourseListView courseListView = new CourseListView(courseListViewModel);
                    currentView = courseListView;
                    _mainWindow.Content = currentView;
                    break;
                case NavigateMessage.Settings:
                    SettingsView settingsView = new SettingsView(settingsViewModel);
                    currentView = settingsView;
                    _mainWindow.Content = settingsView;
                    break;
            }

        }
    }
}
