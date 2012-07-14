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
using JayDev.Notemaker.ViewModel;
using JayDev.Notemaker.Model;
using System.Collections.ObjectModel;
using LibMPlayerCommon;
using GalaSoft.MvvmLight.Messaging;
using JayDev.Notemaker.Common;
using System.Diagnostics;

namespace JayDev.Notemaker.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Microsoft.Practices.Unity.UnityContainer _container = new Microsoft.Practices.Unity.UnityContainer();
        CourseUseViewModel courseUseViewModel;
        UserControl currentView;
        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);


            CourseRepository repo = new CourseRepository();
            courseUseViewModel = new CourseUseViewModel(repo);
            var blah = repo.GetCourseList();

            if (blah.Count == 0)
            {
                CreateDayGameCourse(repo);
                blah = repo.GetCourseList();
            }
            var currentCourse = blah.First(x => x.Name == "Daygame");
            courseUseViewModel.SetCurrentCourse(currentCourse);
            CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
            //_container.RegisterInstance<Course>
            this.Content = courseUseView;
            this.currentView = courseUseView;
            Messenger.Default.Register<NavigateMessage>(this, MessageType.Navigate, (message) => Navigate(message));
            Messenger.Default.Register<ReusableControlMessage>(this, MessageType.RegisterReusableControl, (message) => RegisterReusableControl(message));
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("KEY DOWN: " + e.Key);
            Messenger.Default.Send<KeyEventArgs>(e, 999);
        }

        List<ReusableControlMessage> _reusableControlMessages = new List<ReusableControlMessage>();
        private void RegisterReusableControl(ReusableControlMessage message)
        {
            _reusableControlMessages.Add(message);
        }

        WindowStyle preFullscreenWindowStyle = WindowStyle.SingleBorderWindow;
        WindowState preFullscreenWindowState = WindowState.Maximized;
        private void Navigate(NavigateMessage message)
        {
            if (currentView is CourseUseView)
            {
                FullscreenCourseView bleh = new FullscreenCourseView(courseUseViewModel);
                this.Content = bleh;
                currentView = bleh;

                preFullscreenWindowStyle = this.WindowStyle;
                preFullscreenWindowState = this.WindowState;
                if (this.WindowState == System.Windows.WindowState.Maximized)
                {
                    //JDW: have to set winbdowState to normal first, otherwise WPF will still show the windows taskbar
                    this.WindowState = System.Windows.WindowState.Normal;
                }
                this.WindowStyle = System.Windows.WindowStyle.None;
                this.Topmost = true;
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                CourseUseView courseUseView = new CourseUseView(courseUseViewModel);
                this.Content = courseUseView;
                currentView = courseUseView;

                this.WindowStyle = preFullscreenWindowStyle;
                this.Topmost = false;
                this.WindowState = preFullscreenWindowState;

                //ensure that the mouse cursor is visible. this is a bit of a hack, since interacting with the win32 control is a PITA... and if the cursor was hidden when we left fullscreen,
                //it'll stay hidden until it moves back over the win32 control.
                Mouse.OverrideCursor = null;
            }

        }

        private void CreateDayGameCourse(CourseRepository repo)
        {
            var result = repo.GetCourseList();
            var currentCourse = new Course();
            currentCourse.Name = "Daygame";
            var tracks = new List<string>();
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 1 - Capture\1. Daygame Foundations _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 1 - Capture\2. The Attention Snap _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 1 - Capture\3. The Prehistory _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 1 - Capture\4. The Observational Statement _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 1 - Capture\5. Assumption Stacking _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 1 - Capture\6. Approach Anxiety Annihilation _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 1 - Capture\7. End Of Day One Q&A _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 2 - Attraction\1. The Attractive Daygamer _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 2 - Attraction\2. Rules Of Daygame Attraction _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 2 - Attraction\3. Humour And Self Amusement _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 3 - Rapport\1. Authentic Communication _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 3 - Rapport\2. Deep Rapport _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 3 - Rapport\3. Number Closing & Instant Dates _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 3 - Rapport\4. Creating Deep Connections _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 3 - Rapport\5. End Of Day Two Q&A _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 3 - Rapport\6. Self-Esteem Supercharger _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 4 - Seduction\1. Leading & Logistics _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 4 - Seduction\2. Day Two Strategy _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 4 - Seduction\3. Inner Game Of Sexual Escalation _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 4 - Seduction\4. Outer Game Of Sexual Escalation _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 5 -Advanced\1. Indoor & Spontaneous Daygame _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 5 -Advanced\2. Same Day Lays _ Daygame Blueprint Members Area.mp4");
            tracks.Add(@"D:\Downloads\Daygame Blueprint- With Andy Yosha and Yad\Module 5 -Advanced\3. End Of Day Three Q&A _ Daygame Blueprint Members Area.mp4");

            foreach (string track in tracks)
            {
                var a = new Discover(track);
                Console.WriteLine("Length: {0}, is video: {1}, title: {2}", a.Length, a.Video, a.Title);
                currentCourse.Tracks.Add(new Track() { FilePath = track, Length = new TimeSpan(0, 0, a.Length), IsVideo = a.Video, AspectRatio = a.AspectRatio });
            }

            result.Add(currentCourse);
            repo.SaveCourseList(result);
        }
    }
}
