using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using JayDev.MediaScribe.Common;
using JayDev.MediaScribe.ViewModel;
using JayDev.MediaScribe.Model;
using GalaSoft.MvvmLight.Messaging;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;
//using log4net.Config;
using JayDev.MediaScribe.Core;
using MediaScribe.Common;
using NHibernate;
using NHibernate.Cfg;
using System.Reflection;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace JayDev.MediaScribe.View
{
    public class Controller
    {
        private static MainWindow _mainWindow;
        private static Microsoft.Practices.Unity.UnityContainer _container = new Microsoft.Practices.Unity.UnityContainer();
        private static CourseUseViewModel courseUseViewModel;
        private static CourseListViewModel courseListViewModel;
        private static SettingsViewModel settingsViewModel;

        private static UserControl currentView;
        private static ViewModelBase currentViewModel = new ViewModelBase();

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

        private static Course _lastCourse;

        const bool startAtLastCourse = true;

        public void Initialize(MainWindow mainWindow)
        {
            string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var oldChildMPlayerProcesses = (from procs in Process.GetProcesses()
                                    where procs.ProcessName == "mplayer"
                                    && Path.GetDirectoryName(procs.MainModule.FileName).Contains(currentAssemblyDirectoryName)
                                    select procs).ToList();

            try
            {
                oldChildMPlayerProcesses.ForEach(x => x.Kill());
            }
            catch { }

            _mainWindow = mainWindow;
            _mainWindow.PreviewKeyDown += new KeyEventHandler(MainWindow_KeyDown);

            //var logconfig = new System.IO.FileInfo("log4net.xml");
            //if (logconfig.Exists)
            //{
            //    log4net.Config.XmlConfigurator.ConfigureAndWatch(logconfig);
            //}  

            Console.SetOut(new DebugTextWriter());

            CourseRepository courseRepo = new CourseRepository();
            courseUseViewModel = new CourseUseViewModel(courseRepo);
            courseListViewModel = new CourseListViewModel(courseRepo);
            SettingRepository settingsRepo = new SettingRepository();
            settingsViewModel = new SettingsViewModel(settingsRepo);

            Messenger.Default.Register<NavigateArgs>(Singleton, MessageType.Navigate, (message) => Navigate(message));
            Messenger.Default.Register<string>(Singleton, "errors", (error) => MessageBox.Show(error));

            _mainWindow.Closing += new System.ComponentModel.CancelEventHandler(_mainWindow_Closing);


            HotkeyManager.HandleHotkeyRegistration(new List<HotkeyBase>(settingsRepo.GetHotkeys()));

            bool loadedLastCourse = false;
            if (startAtLastCourse)
            {
                var courses = courseRepo.GetCourseList();
                var currentCourse = courses.OrderBy(x => x.DateViewed).LastOrDefault();
                if (null != currentCourse)
                {
                    Navigate(new NavigateArgs(NavigateMessage.WriteCourseNotes, currentCourse));
                    loadedLastCourse = true;
                }
            }
            if (false == loadedLastCourse)
            {
                Navigate(new NavigateArgs(NavigateMessage.ListCourses));
            }

        }

        void _mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if we're closing the application, and we're on one of the course screens... we need to persist it's 'last' details to the db
            //(not valid if we're on list page, because we saved these details when we left the course screen)
            if (currentView is CourseUseView || currentView is FullscreenCourseView)
            {
                courseUseViewModel.LeavingViewModel();
            }
        }


        static void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //TODO: handle lower-level (so can use keypad keys, regardless of numlock status). could try http://stackoverflow.com/a/5989521
            currentViewModel.HandleWindowKeypress(sender, e);

            CourseUseView courseUseView = currentView as CourseUseView;
            if(null != courseUseView) {
                courseUseView.HandleWindowKeypress(sender, e);
            }
            FullscreenCourseView fullscreenView = currentView as FullscreenCourseView;
            if (null != fullscreenView)
            {
                fullscreenView.HandleWindowKeypress(sender, e);
            }
        }


        static WindowStyle preFullscreenWindowStyle = WindowStyle.SingleBorderWindow;
        static WindowState preFullscreenWindowState = WindowState.Maximized;


        private static void Navigate(NavigateArgs args)
        {
            switch (args.Message)
            {
                case NavigateMessage.ToggleFullscreen:
                    if (currentView is CourseUseView)
                    {
                        preFullscreenWindowStyle = _mainWindow.WindowStyle;
                        preFullscreenWindowState = _mainWindow.WindowState;

                        FullscreenCourseView fscView = new FullscreenCourseView(courseUseViewModel);
                        _mainWindow.Content = fscView;
                        currentView = fscView;
                        currentViewModel = courseUseViewModel;

                        if (_mainWindow.WindowState == System.Windows.WindowState.Maximized)
                        {
                            //JDW: have to set winbdowState to normal first, otherwise WPF will still show the windows taskbar
                            _mainWindow.WindowState = System.Windows.WindowState.Normal;
                        }
                        _mainWindow.WindowStyle = System.Windows.WindowStyle.None;
                        _mainWindow.WindowState = System.Windows.WindowState.Maximized;
                        
                    }
                    else
                    {
                        CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
                        _mainWindow.Content = courseUseView;
                        currentView = courseUseView;
                        currentViewModel = courseUseViewModel;

                        _mainWindow.WindowStyle = preFullscreenWindowStyle;
                        _mainWindow.Topmost = false;
                        _mainWindow.WindowState = preFullscreenWindowState;

                        //ensure that the mouse cursor is visible. this is a bit of a hack, since interacting with the win32 control is a
                        //PITA... and if the cursor was hidden when we left fullscreen, it'll stay hidden until it moves back over the win32
                        //control.
                        Mouse.OverrideCursor = null;
                    }
                    break;
                case NavigateMessage.WriteCourseNotes:
                    {
                        currentViewModel.LeavingViewModel();
                        CourseRepository courseRepo = new CourseRepository();
                        if (null == args.Course && null == _lastCourse)
                        {
                            var courses = courseRepo.GetCourseList();
                            if (null == courses || courses.Count == 0)
                            {  
                                MessageBox.Show(_mainWindow,"Please create a course, before going to the Write Notes section");
                                return;
                            }
                            _lastCourse = courses.First();
                        }


                        Course courseToLoad = null;
                        if (null != args.Course)
                        {
                            courseToLoad = args.Course;
                        }
                        else if (null != _lastCourse)
                        {
                            //the course is probably outdated... get a fresh copy.
                            courseToLoad = courseRepo.GetCourse(_lastCourse.ID.Value);
                        }
                        CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
                        currentView = courseUseView;
                        currentViewModel = courseUseViewModel;
                        _mainWindow.Content = currentView;
                        courseUseViewModel.SetCurrentCourse(courseToLoad);
                        
                        //note the last course we had viewed
                        _lastCourse = courseToLoad;

                        currentViewModel.EnteringViewModel();
                    }
                    break;
                case NavigateMessage.ListCourses:
                    currentViewModel.LeavingViewModel();
                    CourseListView courseListView = new CourseListView(courseListViewModel);
                    currentView = courseListView;
                    currentViewModel = courseListViewModel;
                    _mainWindow.Content = currentView;

                    currentViewModel.EnteringViewModel();
                    break;
                case NavigateMessage.Settings:
                    currentViewModel.LeavingViewModel();
                    SettingsView settingsView = new SettingsView(settingsViewModel);
                    currentView = settingsView;
                    currentViewModel = settingsViewModel;
                    _mainWindow.Content = settingsView;

                    currentViewModel.EnteringViewModel();
                    break;
            }
        }
    }
}
