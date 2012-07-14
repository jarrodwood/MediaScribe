using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using JayDev.Notemaker.Common;

namespace JayDev.Notemaker
{
    [DataContract]
    public class TrackTime : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        private Track track;
        public Track Track
        {
            get { return track; }
            set
            {
                track = value;
                OnPropertyChanged("Track");
            }
        }
        [DataMember]
        private TimeSpan time { get; set; }
        public TimeSpan Time { get { return time; }
            set
            {
                time = value;
                OnPropertyChanged("Time");
            }
        }

        private List<Track> tracksCollection = new List<Track>();
        public List<Track> TracksCollection { get { return tracksCollection; } set { tracksCollection = value; } }

        public string StringDisplayValue
        {
            get
            {
                StringBuilder resultBuilder = new StringBuilder();
                int index = tracksCollection.FindIndex(x => x.FilePath == track.FilePath);
                if (index != -1)
                {
                    resultBuilder.Append(index + 1);
                }
                else
                {
                    resultBuilder.Append(Track.StringDisplayValue);
                }
                resultBuilder.Append(" - ");
                resultBuilder.Append(Utility.GetTimeSpanAsLongString(Time));

                return resultBuilder.ToString();
            }
        }

        public string SortValue
        {
            get
            {
                StringBuilder resultBuilder = new StringBuilder();
                int index = tracksCollection.FindIndex(x => x.FilePath == track.FilePath);
                if (index != -1)
                {
                    resultBuilder.Append((index + 1).ToString("0000"));
                }
                else
                {
                    resultBuilder.Append(Track.StringDisplayValue);
                }
                resultBuilder.Append(" - ");
                resultBuilder.Append(Utility.GetTimeSpanAsLongString(Time));

                return resultBuilder.ToString();
            }
        }

        public override string ToString()
        {
            return StringDisplayValue;
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
