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

namespace JayDev.Notemaker.View
{
    public class Controller
    {
        private static MainWindow _mainWindow;
        private static Microsoft.Practices.Unity.UnityContainer _container = new Microsoft.Practices.Unity.UnityContainer();
        private static CourseUseViewModel courseUseViewModel;
        private static CourseMaintenanceViewModel courseMaintenanceViewModel;
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


            CourseRepository repo = new CourseRepository();
            courseUseViewModel = new CourseUseViewModel(repo);
            courseMaintenanceViewModel = new CourseMaintenanceViewModel(repo);
            var blah = repo.GetCourseList();

            if (blah.Count == 0)
            {
                throw new Exception("todo");
                //CreateDayGameCourse(repo);
                blah = repo.GetCourseList();
            }
            var currentCourse = blah.First(x => x.Name == "Daygame");
            courseUseViewModel.SetCurrentCourse(currentCourse);
            CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
            _mainWindow.Content = courseUseView;
            currentView = courseUseView;
            Messenger.Default.Register<NavigateMessage>(Singleton, MessageType.Navigate, (message) => Navigate(message));


            _mainWindow.Closing += new System.ComponentModel.CancelEventHandler(_mainWindow_Closing);
        }

        void _mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            courseUseViewModel.SaveCourseCommand.Execute(null);
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


        private static void Navigate(NavigateMessage message)
        {
            switch(message) {
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
                case NavigateMessage.UseCourse:
                    break;
                case NavigateMessage.MaintainCourse:
                    CourseMaintenanceView courseMaintenanceView = new CourseMaintenanceView();
                    currentView = courseMaintenanceView;
                    _mainWindow.Content = courseMaintenanceView;
                    break;
            }

        }
    }
}