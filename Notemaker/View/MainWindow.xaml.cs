﻿using System;
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

            Controller.Singleton.Initialize(this);

            this.SourceInitialized += new EventHandler(MainWindow_SourceInitialized);
        }

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
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
            //repo.SaveCourseList(result);
        }





        private void _OnStandardChromeClicked(object sender, RoutedEventArgs e)
        {
            this.Style = null;
        }

        private void _OnGradientChromeClicked(object sender, RoutedEventArgs e)
        {
            var style = (Style)Resources["GradientStyle"];
            this.Style = style;
        }

        private void _OnGlassyChromeClicked(object sender, RoutedEventArgs e)
        {
            var style = (Style)Resources["GlassStyle"];
            this.Style = style;
        }

        private void _OnSystemCommandCloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow((Window)e.Parameter);
        }


    }
}
