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
//using NHibernate;
//using NHibernate.Cfg;
using System.Reflection;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Practices.Unity;

namespace JayDev.MediaScribe.View
{
    public class Controller
    {
        private MainWindow _mainWindow;
        private CourseUseViewModel courseUseViewModel;
        private CourseListViewModel courseListViewModel;
        private SettingsViewModel settingsViewModel;

        private ViewModelBase currentViewModel = null;

        private Dispatcher _currentDispatcher;

        private Course _lastCourse;

        private List<Course> _allCourses;
        public List<Course> AllCourses
        {
            get
            {
                if (null == _allCourses)
                {
                    _allCourses = (new CourseRepository()).GetCourseList();
                }
                return _allCourses;
            }
        }

        const bool startAtLastCourse = true;

        private TabControl _tabControl;

        /// <summary>
        /// Initialize the constructor. This logic is not in the constructor, because the controller MUST be registered with Unity before
        /// calling this.
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="tabControl"></param>
        public void Initialize(MainWindow mainWindow, TabControl tabControl)
        {
            this._tabControl = tabControl;

            //Note the UI dispatcher
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            TryKillOldMPlayerProccesses();

            _mainWindow = mainWindow;
            _mainWindow.PreviewKeyDown += new KeyEventHandler(MainWindow_KeyDown);

            //allow all Debug messages to be sent to console (useful for debugging within VS)
            Console.SetOut(new DebugTextWriter());

            CourseRepository courseRepo = new CourseRepository();
            courseUseViewModel = new CourseUseViewModel(courseRepo);
            courseListViewModel = new CourseListViewModel(courseRepo);
            SettingRepository settingsRepo = new SettingRepository();
            settingsViewModel = new SettingsViewModel(settingsRepo);

            ((TabItem)_tabControl.Items[0]).Content = new CourseListView(courseListViewModel);
            ((TabItem)_tabControl.Items[1]).Content = new SettingsView(settingsViewModel);
            ((TabItem)_tabControl.Items[3]).Content = new CourseUseView(courseUseViewModel);



            Messenger.Default.Register<NavigateArgs>(this, MessageType.Navigate, (message) => Navigate(message));
            Messenger.Default.Register<string>(this, "errors", (error) => MessageBox.Show(error));

            _mainWindow.Closing += new System.ComponentModel.CancelEventHandler(_mainWindow_Closing);


            HotkeyManager.HandleHotkeyRegistration(new List<HotkeyBase>(settingsRepo.GetHotkeys()));

            bool loadedLastCourse = false;
            if (startAtLastCourse)
            {
                var currentCourse = AllCourses.OrderBy(x => x.DateViewed).LastOrDefault();
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
            if (currentViewModel is CourseUseViewModel)
            {
                courseUseViewModel.LeavingViewModel();
            }

            if (null != _fullscreenWindow)
            {
                _fullscreenWindow.Close();
            }
        }

        
        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //TODO: handle lower-level (so can use keypad keys, regardless of numlock status). could try http://stackoverflow.com/a/5989521
            currentViewModel.HandleWindowKeypress(sender, e);

            CourseUseView courseUseView = ((TabItem)_tabControl.Items[3]).Content as CourseUseView;
            courseUseView.HandleWindowKeypress(sender, e);
        }

        void FullscreenWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //TODO: handle lower-level (so can use keypad keys, regardless of numlock status). could try http://stackoverflow.com/a/5989521
            currentViewModel.HandleWindowKeypress(sender, e);

            FullscreenCourseView fullscreenView = _fullscreenWindow.Content as FullscreenCourseView;
            fullscreenView.HandleWindowKeypress(sender, e);
        }

        public void RefreshCourse(Course course)
        {
            for (int i = 0; i < AllCourses.Count; i++)
            {
                if (_allCourses[i].ID == course.ID)
                {
                    _allCourses[i] = course;
                    break;
                }
            }
        }

        Window _fullscreenWindow = null;

        public bool IsFullscreen { get; private set; }

        private void Navigate(NavigateArgs args)
        {
            switch (args.Message)
            {
                case NavigateMessage.ToggleFullscreen:
                    if (_mainWindow.Visibility == Visibility.Visible)
                    {
                        currentViewModel = courseUseViewModel;

                        if (null == _fullscreenWindow)
                        {
                            _fullscreenWindow = new BlankWindow();
                            _fullscreenWindow.Content = new FullscreenCourseView(courseUseViewModel);
                            _fullscreenWindow.PreviewKeyDown += new KeyEventHandler(FullscreenWindow_KeyDown);
                            ((FullscreenCourseView)_fullscreenWindow.Content).videoControl.AssociateVideoWithControl();
                        }
                        else
                        {
                            ((FullscreenCourseView)_fullscreenWindow.Content).videoControl.AssociateVideoWithControl();
                        }

                        _fullscreenWindow.Visibility = Visibility.Visible;
                        _mainWindow.Visibility = Visibility.Collapsed;

                        if (_fullscreenWindow.WindowState == System.Windows.WindowState.Maximized)
                        {
                            //JDW: have to set winbdowState to normal first, otherwise WPF will still show the windows taskbar
                            _fullscreenWindow.WindowState = System.Windows.WindowState.Normal;
                        }
                        _fullscreenWindow.WindowStyle = System.Windows.WindowStyle.None;
                        _fullscreenWindow.WindowState = System.Windows.WindowState.Maximized;
                        _fullscreenWindow.BringIntoView();
                        _fullscreenWindow.Focus();


                        IsFullscreen = true;
                    }
                    else
                    {
                        _mainWindow.Visibility = Visibility.Visible;
                        _fullscreenWindow.Visibility = Visibility.Collapsed;

                        CourseUseView courseUseView = ((TabItem)_tabControl.Items[3]).Content as CourseUseView;
                        courseUseView.videoControl.AssociateVideoWithControl();

                        currentViewModel = courseUseViewModel;

                        //ensure that the mouse cursor is visible. this is a bit of a hack, since interacting with the win32 control is a
                        //PITA... and if the cursor was hidden when we left fullscreen, it'll stay hidden until it moves back over the win32
                        //control.
                        Mouse.OverrideCursor = null;

                        IsFullscreen = false;
                    }
                    break;
                case NavigateMessage.WriteCourseNotes:
                    {
                        if (null != currentViewModel)
                        {
                            currentViewModel.LeavingViewModel();
                        }
                        CourseRepository courseRepo = new CourseRepository();
                        if (null == args.Course && null == _lastCourse)
                        {
                            if (null == AllCourses || AllCourses.Count == 0)
                            {  
                                MessageBox.Show(_mainWindow,"Please create a course, before going to the Write Notes section");
                                return;
                            }
                            _lastCourse = AllCourses.First();
                        }


                        Course courseToLoad = null;
                        if (null != args.Course)
                        {
                            courseToLoad = args.Course;
                        }
                        else if (null != _lastCourse)
                        {
                            //the course is probably outdated... get a fresh copy.
                            courseToLoad = _allCourses.First(x => x.ID == _lastCourse.ID);
                        }

                        currentViewModel = courseUseViewModel;
                        courseUseViewModel.SetCurrentCourse(courseToLoad);
                        
                        //note the last course we had viewed
                        _lastCourse = courseToLoad;

                        currentViewModel.EnteringViewModel();

                        ThreadHelper.ExecuteAsyncUI(_currentDispatcher, delegate
                        {
                            _tabControl.SelectedIndex = 3;
                        });
                    }
                    break;
                case NavigateMessage.ListCourses:
                    if (null != currentViewModel)
                    {
                        currentViewModel.LeavingViewModel();
                    }
                    currentViewModel = courseListViewModel;

                    currentViewModel.EnteringViewModel();
                    break;
                case NavigateMessage.Settings:
                    if (null != currentViewModel)
                    {
                        currentViewModel.LeavingViewModel();
                    }
                    currentViewModel = settingsViewModel;

                    currentViewModel.EnteringViewModel();
                    break;
            }
        }

        /// <summary>
        /// When MediaScribe is killed in Visual Studio, it often leaves the child mplayer processes running. This will find all old mplayer
        /// instances started by MediaScribe, and kill them.
        /// </summary>
        private void TryKillOldMPlayerProccesses()
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
        }
    }
}
