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
using JayDev.Notemaker.Model;
using LibMPlayerCommon;
using System.Data;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace JayDev.Notemaker.View
{
    /// <summary>
    /// Interaction logic for WriterWindow.xaml
    /// </summary>
    public partial class WriterWindow : Window
    {
        CourseRepository repo = new CourseRepository();
        Course currentCourse = null;
        Track currentTrack = null;
        private readonly Dispatcher currentDispatcher;

        private MPlayer play;
        //private string filePath;
        //private bool trackBarMousePushedDown = false;
        private int currentTime = 0;

        Timer timer = new Timer();

        private ObservableCollection<Note> observableNotes = null;

        public WriterWindow()
        {
            InitializeComponent();

            //ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            //tracks.Add(new Track() { Title = "Test File 1", Length = new TimeSpan(0, 35, 44) });
            //tracks.Add(new Track() { Title = "Test File 2", Length = new TimeSpan(1, 21, 52) });
            //tracks.Add(new Track() { Title = "Wally's World", Length = new TimeSpan(4, 02, 30) });

            //ObservableCollection<Note> notes = new ObservableCollection<Note>();
            //notes.Add(new Note() { Title = "My world rocks!", Body = "because i am just so awesome. Do i need more of a reason?", Rating = 4,
            //     Start = new TrackTime() { Track = tracks[0], Time = new TimeSpan(0, 20, 32) },
            //     End = new TrackTime() { Track = tracks[0], Time = new TimeSpan(0, 22, 47) }});

            //Course currentCourse = new Course() {
            //    Notes = new List<Note>(notes),
            //    Tracks = new List<Track>(tracks)
            //};

            var blah = repo.GetCourseList();

            if (blah.Count == 0)
            {
                CreateDayGameCourse();
                blah = repo.GetCourseList();
            }
            currentCourse = blah.First(x => x.Name == "Daygame");



            trackGrid.ItemsSource = currentCourse.Tracks;
            observableNotes = new ObservableCollection<Note>(currentCourse.Notes);
            //observableNotes.Add(new Note()
            //{
            //    Title = "My world rocks!",
            //    Body = "because i am just so awesome. Do i need more of a reason?",
            //    Rating = 4,
            //    Start = new TrackTime() { Track = currentCourse.Tracks[0], Time = new TimeSpan(0, 20, 32) },
            //    End = new TrackTime() { Track = currentCourse.Tracks[0], Time = new TimeSpan(0, 22, 47) }
            //});
            noteDataGrid.ItemsSource = observableNotes;

            ICollectionView dataView = CollectionViewSource.GetDefaultView(noteDataGrid.ItemsSource);
            //clear the existing sort order
            dataView.SortDescriptions.Clear();
            //create a new sort order for the sorting that is done lastly
            dataView.SortDescriptions.Add(new SortDescription("Start.SortValue", ListSortDirection.Ascending));
            //refresh the view which in turn refresh the grid
            dataView.Refresh();


            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            //Note the dispatcher of the UI thread, used to create this view model. We'll need this to update collections used by the UI
            this.currentDispatcher = Dispatcher.CurrentDispatcher;


            if (null != currentCourse.EmbeddedVideoWidth)
            {
                mainGrid.ColumnDefinitions[0].Width = new GridLength(currentCourse.EmbeddedVideoWidth.Value);
            }
            if (null != currentCourse.EmbeddedVideoHeight)
            {
                leftGridColumn.RowDefinitions[0].Height = new GridLength(currentCourse.EmbeddedVideoHeight.Value);
            }

            videoControl.OnDoubleClick += new Controls.VideoControl.DoubleClickHandler(videoControl_OnDoubleClick);
        }

        bool isFullscreen = false;
        WindowStyle preFullscreenWindowStyle = WindowStyle.SingleBorderWindow;
        WindowState preFullscreenWindowState = WindowState.Maximized;
        void videoControl_OnDoubleClick()
        {
            if (isFullscreen)
            {
                this.WindowStyle = preFullscreenWindowStyle;
                this.Topmost = false;
                this.WindowState = preFullscreenWindowState;
                isFullscreen = false;


                this.Content = entireWindow;
                videoControlHolder.Children.Add(this.videoControl);
                videoControl.SetMode(Controls.VideoControl.VideoControlMode.Embedded);
            }
            else {
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
                isFullscreen = true;

                videoControlHolder.Children.Remove(this.videoControl);
                this.Content = this.videoControl;
                videoControl.SetMode(Controls.VideoControl.VideoControlMode.Fullscreen);
            }
        }
        

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Adding 1 to make the row count start at 1 instead of 0
            // as pointed out by daub815
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void trackListDataGrid_MouseDoubleClick(object sender,
                                  System.Windows.Input.MouseButtonEventArgs e)
        {
            IInputElement element = e.MouseDevice.DirectlyOver;
            if (element != null && element is FrameworkElement)
            {
                if (((FrameworkElement)element).Parent is DataGridCell)
                {
                    var grid = sender as DataGrid;
                    if (grid != null && grid.SelectedItems != null
        && grid.SelectedItems.Count == 1)
                    {
                        var rowView = grid.SelectedItem as Track;
                        if (rowView != null)
                        {
                            PlayTrackTime(rowView);
                        }
                    }
                }
            }
        }

        private void PlayTrackTime(Track track)
        {
            PlayTrackTime(track, new TimeSpan());
        }

        private void PlayTrackTime(Track track, TimeSpan timeToPlayFrom)
        {
            currentTrack = track;
            currentTrackTextBlock.Text = currentTrack.StringDisplayValue;
            currentPlayTime.Text = Utility.GetTimeSpanAsShortString(new TimeSpan());
            totalTrackTime.Text = Utility.GetTimeSpanAsShortString(currentTrack.Length);
            PlayCurrentTrack();
            if (timeToPlayFrom != new TimeSpan())
            {
                Seek(timeToPlayFrom.TotalSeconds);
            }
        }



        private void CreateDayGameCourse()
        {

            var result = repo.GetCourseList();
            currentCourse = new Course();
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
                currentCourse.Tracks.Add(new Track() { FilePath = track, Length = new TimeSpan(0, 0, a.Length) });
            }

            result.Add(currentCourse);
            repo.SaveCourseList(result);
        }

        private void courseListButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            CourseListWindow window = new CourseListWindow();
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();
            this.IsEnabled = true;
        }


        int manualSeekCountdown = 0;
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.videoControl.PlayingStatus == Controls.VideoControl.VideoControlStatus.Playing)
            {
                TimeSpan currentPosition = videoControl.GetCurrentPlayTime();
                double playedSeconds = currentPosition.TotalSeconds;

                //Due to the fact we're using mplayer as the back-end, if the tries to seek, the video might take a second or two to find the appropriate place.
                //in this case, the best thing we can do is to ensure that the video has jumped more than [x] (in this case, 2) seconds before updating it again
                if (true == hasUserSeeked)
                {
                    double safetyNetSeconds = 2;
                    if (Math.Abs(playedSeconds - userSeekedFromTime) < safetyNetSeconds)
                    {
                        return;
                    }
                    else
                    {
                        hasUserSeeked = false;
                    }
                }

                //Action dispatchAction = () => this.timeSlider.Value = playedSeconds;
                Action workAction = delegate
                {
                    isSliderUpdatedForSecond = true;
                    this.timeSlider.Value = playedSeconds;
                    isSliderUpdatedForSecond = false;
                    currentPlayTime.Text = Utility.GetTimeSpanAsShortString(currentPosition);
                };
                currentDispatcher.BeginInvoke(workAction);
            }
        }

        private bool isSliderUpdatedForSecond = false;


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.videoControl.PlayingStatus == Controls.VideoControl.VideoControlStatus.Stopped)
            {
                PlayCurrentTrack();
            }
            else
            {
                PauseResume();
            }
        }

        private void PauseResume()
        {
            if (this.videoControl.PlayingStatus == Controls.VideoControl.VideoControlStatus.Playing)
            {
                timer.Stop();
            }
            else if(this.videoControl.PlayingStatus == Controls.VideoControl.VideoControlStatus.Paused)
            {
                timer.Start();
            }
            this.videoControl.PauseResume();
        }

        private void PlayCurrentTrack()
        {
            if (null != currentTrack && false == string.IsNullOrEmpty(currentTrack.FilePath))
            {
                Action dispatchAction = () => this.timeSlider.Minimum = 0;
                currentDispatcher.BeginInvoke(dispatchAction);
                dispatchAction = () => this.timeSlider.Maximum = currentTrack.Length.TotalSeconds;
                currentDispatcher.BeginInvoke(dispatchAction);

                if (videoControl.PlayingStatus != Controls.VideoControl.VideoControlStatus.Stopped)
                {
                    videoControl.Stop();
                }
                videoControl.Play(currentTrack.FilePath);
                timer.Start();
            }
        }

        private void Seek(double timeSeconds)
        {

            //Action dispatchAction = () => this.play.Seek((int)timeSeconds, LibMPlayerCommon.Seek.Absolute);
            //currentDispatcher.BeginInvoke(dispatchAction);
            videoControl.Seek((int)timeSeconds);
            TimeSpan currentPosition = new TimeSpan(0, 0, (int)timeSeconds);
            currentPlayTime.Text = Utility.GetTimeSpanAsShortString(currentPosition);
        }

        private bool hasUserSeeked = false;
        private double userSeekedFromTime = -1;
        private void Slider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (false == isSliderUpdatedForSecond)
            {
                hasUserSeeked = true;
                userSeekedFromTime = e.OldValue;
                Seek(e.NewValue);

            }
        }

        private void noteDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Note context = e.Row.DataContext as Note;
            if (null != context)
            {
                if (string.IsNullOrEmpty(context.Body))
                {
                    context.Start = null;
                    e.Cancel = true;
                }

                //operation to perform on row edit ended
                this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
                  {
                      SaveCourse();
                      return null;
                  }), DispatcherPriority.Background, new object[] { null });
            }
        }

        private void saveCourse_Click(object sender, RoutedEventArgs e)
        {
            SaveCourse();
        }

        private void SaveCourse()
        {
            if (null != observableNotes)
            {
                currentCourse.Notes = new List<Note>(observableNotes);
            }
            var allCourses = repo.GetCourseList();

            int indexOfDaygame = allCourses.IndexOf(allCourses.First(x => x.Name == "Daygame"));
            allCourses[indexOfDaygame] = currentCourse;
            repo.SaveCourseList(allCourses);
        }

        private void noteDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Note context = e.Row.DataContext as Note;
            if (null != context)
            {
                if (null == context.Start)
                {
                    if (null == currentTrack)
                    {
                        e.Cancel = true;
                        return;
                    }
                    TrackTime startTrackTime = new TrackTime();
                    startTrackTime.TracksCollection = currentCourse.Tracks;
                    startTrackTime.Track = this.currentTrack;
                    startTrackTime.Time = this.videoControl.GetCurrentPlayTime();
                    context.Start = startTrackTime;
                }
            }
        }

        private void setNoteStartTime_Click(object sender, RoutedEventArgs e)
        {
            Note currentNote = ((FrameworkElement)sender).DataContext as Note;
            currentNote.Start.Track = this.currentTrack;
            currentNote.Start.Time = this.videoControl.GetCurrentPlayTime();
        }

        private void playNoteButton_Click(object sender, RoutedEventArgs e)
        {
            Note currentNote = ((FrameworkElement)sender).DataContext as Note;
            PlayTrackTime(currentNote.Start.Track, currentNote.Start.Time);
        }

        private void deleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            Note currentNote = ((FrameworkElement)sender).DataContext as Note;
            observableNotes.Remove(currentNote);

            //operation to perform on row delete
            this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
            {
                SaveCourse();
                return null;
            }), DispatcherPriority.Background, new object[] { null });
        }

        /// <summary>
        /// the vertical splitter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            currentCourse.EmbeddedVideoWidth = mainGrid.ColumnDefinitions[0].Width.Value;
        }

        /// <summary>
        /// the horizontal splitter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSplitter_DragCompleted_1(object sender, DragCompletedEventArgs e)
        {
            currentCourse.EmbeddedVideoHeight = leftGridColumn.RowDefinitions[0].Height.Value;
        }



    }
}
