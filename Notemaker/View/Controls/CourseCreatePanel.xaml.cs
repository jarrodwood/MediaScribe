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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;

namespace JayDev.Notemaker.View.Controls
{
    /// <summary>
    /// Interaction logic for CourseCreatePanel.xaml
    /// </summary>
    public partial class CourseCreatePanel : UserControl
    {


        public string CourseName
        {
            get { return (string)GetValue(CourseNameProperty); }
            set { SetValue(CourseNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CourseName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CourseNameProperty =
            DependencyProperty.Register("CourseName", typeof(string), typeof(CourseCreatePanel), new UIPropertyMetadata(null));



        public ObservableCollection<Track> CourseTracks
        {
            get { return (ObservableCollection<Track>)GetValue(CourseTracksProperty); }
            set { SetValue(CourseTracksProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CourseTracks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CourseTracksProperty =
            DependencyProperty.Register("CourseTracks", typeof(ObservableCollection<Track>), typeof(CourseCreatePanel), new UIPropertyMetadata(null, OnCourseTracksPropertyChanged));


        private static void OnCourseTracksPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //track
            //var old = e.OldValue as ObservableCollection<WorkItem>;

            //if (old != null)
            //    old.CollectionChanged -= me.OnWorkCollectionChanged;

            //var n = e.NewValue as ObservableCollection<WorkItem>;

            //if (n != null)
            //    n.CollectionChanged += me.OnWorkCollectionChanged;
            }




        public ICommand CreateCourseCommand
        {
            get { return (ICommand)GetValue(CreateCourseCommandProperty); }
            set { SetValue(CreateCourseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CreateCourseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CreateCourseCommandProperty =
            DependencyProperty.Register("CreateCourseCommand", typeof(ICommand), typeof(CourseCreatePanel));



        public ICommand DeleteCourseCommand
        {
            get { return (ICommand)GetValue(DeleteCourseCommandProperty); }
            set { SetValue(DeleteCourseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DeleteCourseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeleteCourseCommandProperty =
            DependencyProperty.Register("DeleteCourseCommand", typeof(ICommand), typeof(CourseCreatePanel));




        public ICommand AddTracksCommand
        {
            get { return (ICommand)GetValue(AddTracksCommandProperty); }
            set { SetValue(AddTracksCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddTracksCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddTracksCommandProperty =
            DependencyProperty.Register("AddTracksCommand", typeof(ICommand), typeof(CourseCreatePanel));
        
        


        public object DeleteTracksCommandParameter
        {
            get { return (object)GetValue(DeleteTracksDeleteTracksCommandParameterProperty); }
            set { SetValue(DeleteTracksDeleteTracksCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DeleteTracksDeleteTracksCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeleteTracksDeleteTracksCommandParameterProperty =
            DependencyProperty.Register("DeleteTracksDeleteTracksCommandParameter", typeof(object), typeof(CourseCreatePanel));

        public object AddTracksCommandParameter
        {
            get { return (object)GetValue(AddTracksAddTracksCommandParameterProperty); }
            set { SetValue(AddTracksAddTracksCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddTracksAddTracksCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddTracksAddTracksCommandParameterProperty =
            DependencyProperty.Register("AddTracksAddTracksCommandParameter", typeof(object), typeof(CourseCreatePanel));




        
        public CourseCreatePanel()
        {
            InitializeComponent();
        }
    }
}
