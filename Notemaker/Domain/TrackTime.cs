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
    [Serializable]
    public class TrackTime : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Track track;
        [DataMember]
        public Track Track
        {
            get { return track; }
            set
            {
                track = value;
                OnPropertyChanged("Track");
            }
        }
        private TimeSpan time { get; set; }
        [DataMember]
        public TimeSpan Time { get { return time; }
            set
            {
                time = value;
                OnPropertyChanged("Time");
            }
        }

        public Course ParentCourse { get; set; }

        public string StringDisplayValue
        {
            get
            {
                StringBuilder resultBuilder = new StringBuilder();
                int indexInTrackList = -1;
                if (null != ParentCourse)
                {
                    ParentCourse.Tracks.FindIndex(x => x.FilePath == track.FilePath);
                }
                if (indexInTrackList != -1)
                {
                    resultBuilder.Append(indexInTrackList + 1);
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
                int index = ParentCourse.Tracks.FindIndex(x => x.FilePath == track.FilePath);
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

        public object Clone()
        {
            TrackTime clone = new TrackTime();
            clone.time = this.time;
            clone.Track = (Track)this.Track.Clone();
            clone.ParentCourse = this.ParentCourse;
            return clone;
        }
    }
}
