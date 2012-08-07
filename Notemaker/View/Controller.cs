﻿using System;
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
using Notemaker.Common;

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
        private static ViewModelBase currentViewModel;

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

        bool startAtLastCourse = false;

        public void Initialize(MainWindow mainWindow)
        {
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
                var currentCourse = courses.OrderBy(x => x.DateViewed).FirstOrDefault();
                if (null != currentCourse)
                {
                    courseUseViewModel.SetCurrentCourse(currentCourse);
                    CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
                    _mainWindow.Content = courseUseView;
                    currentView = courseUseView;
                    currentViewModel = courseUseViewModel;
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
                        CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
                        currentView = courseUseView;
                        currentViewModel = courseUseViewModel;
                        _mainWindow.Content = currentView;
                        courseUseViewModel.SetCurrentCourse(args.Course);
                    }
                    break;
                case NavigateMessage.ListCourses:
                    CourseListView courseListView = new CourseListView(courseListViewModel);
                    currentView = courseListView;
                    currentViewModel = courseListViewModel;
                    _mainWindow.Content = currentView;
                    courseListViewModel.Init();

                    break;
                case NavigateMessage.Settings:
                    SettingsView settingsView = new SettingsView(settingsViewModel);
                    currentView = settingsView;
                    currentViewModel = settingsViewModel;
                    _mainWindow.Content = settingsView;
                    break;
            }
        }

    }
}
