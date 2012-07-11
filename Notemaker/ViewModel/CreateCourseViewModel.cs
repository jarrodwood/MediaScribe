using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Runtime.Remoting.Messaging;
using LibMPlayerCommon;
using JayDev.Notemaker.Model;
using System.Collections.ObjectModel;
using System.Windows;

namespace JayDev.Notemaker.ViewModel
{
    class CreateCourseViewModel : ViewModelBase
    {
        private readonly Dispatcher currentDispatcher;
        public ObservableCollection<Track> Tracks { get; set; }
        public string CourseName { get; set; }



        public CreateCourseViewModel()
        {
            //Note the dispatcher of the UI thread, used to create this view model. We'll need this to update collections used by the UI
            this.currentDispatcher = Dispatcher.CurrentDispatcher;

            this.Tracks = new ObservableCollection<Track>();
        }

        public void SaveCourse()
        {
            CourseRepository service = new CourseRepository();
            Course course = new Course()
            {
                Name = CourseName,
                Tracks = new List<Track>(Tracks)
            };
            service.SaveCourse(course);
        }

        public void AddTracks()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;
            StringBuilder filterBuilder = new StringBuilder();
            filterBuilder.Append("Audio and Video Files|*.3GP;*.ASF;*.AVI;*.FLV;*.FLA;*.M4V;*.MKV;*.MOV;*.MPEG;*.MPG;*.OGV;*.RM;*.WMV;*.WAV;*.FLAC;*.M4A;*.WMA;*.MP2;*.MP3;*.WMA;*.AAC;*.M4A;*.RA;*.RM;*.SWA");
            filterBuilder.Append("|Video Files|*.3GP;*.ASF;*.AVI;*.FLV;*.FLA;*.M4V;*.MKV;*.MOV;*.MPEG;*.MPG;*.OGV;*.RM;*.WMV");
            filterBuilder.Append("|Audio Files|*.WAV;*.FLAC;*.M4A;*.WMA;*.MP2;*.MP3;*.WMA;*.AAC;*.M4A;*.RA;*.RM;*.SWA");
            filterBuilder.Append("|All Files|*.*");
            dlg.Filter = filterBuilder.ToString(); // Filter files by extension    

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Read the files
                foreach (String file in dlg.FileNames)
                {
                    AsyncMethodCaller caller = new AsyncMethodCaller(DiscoverTrack);
                    IAsyncResult asyncResult = caller.BeginInvoke(file, new AsyncCallback(DiscoverTrackCallback), Tracks);
                }
            }
        }

        private void DiscoverTrackCallback(IAsyncResult ar)
        {
            // Retrieve the delegate.
            AsyncResult result = (AsyncResult)ar;
            AsyncMethodCaller caller = (AsyncMethodCaller)result.AsyncDelegate;
            ObservableCollection<Track> course = (ObservableCollection<Track>)ar.AsyncState;
            Track returnValue = caller.EndInvoke(ar);

            Action dispatchAction = () => Tracks.Add(returnValue);
            currentDispatcher.BeginInvoke(dispatchAction);
        }

        private Track DiscoverTrack(string filePath)
        {
            Discover discoverer = new Discover(filePath);
            Track track = new Track()
            {
                FilePath = filePath,
                Title = discoverer.Title,
                Length = new TimeSpan(0, 0, discoverer.Length)
            };

            return track;
        }

        private delegate Track AsyncMethodCaller(string filePath);
    }
}
