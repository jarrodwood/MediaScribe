using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe
{
    [DataContract]
    [Serializable]
    public class TrackTime : INotifyPropertyChanged, ICloneable
    {
        public virtual int? ID { get; set; }

        public virtual int? TrackNumber { get; set; }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual Course ParentCourse { get; set; }

        private Track track;
        [DataMember]
        public virtual Track Track
        {
            get
            {
                return track;
            }
            set
            {
                track = value;
                if (null == track)
                {
                    TrackNumber = null;
                }
                else
                {
                    TrackNumber = track.TrackNumber.Value;
                }
                OnPropertyChanged("Track");
            }
        }
        private TimeSpan time { get; set; }
        [DataMember]
        public virtual TimeSpan Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged("Time");
            }
        }

        public virtual int? IndexOfTrackInCourse
        {
            get
            {
                if (null != track)
                {
                    return track.TrackNumber + 1;
                }

                return null;
            }
        }

        public virtual string StringDisplayValue
        {
            get
            {
                StringBuilder resultBuilder = new StringBuilder();
                int? index = IndexOfTrackInCourse;
                if (null != index)
                {
                    resultBuilder.Append(index);
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

        public virtual string SortValue
        {
            get
            {
                StringBuilder resultBuilder = new StringBuilder();
                //TODO: store this in DB
                int indexInTrackList = -1;
                for (int i = 0; i < Track.ParentCourse.Tracks.Count; i++)
                {
                    if (Track.ParentCourse.Tracks[i].FilePath == track.FilePath)
                    {
                        indexInTrackList = i;
                        break;
                    }
                }
                if (indexInTrackList != -1)
                {
                    resultBuilder.Append((indexInTrackList + 1).ToString("0000"));
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
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public virtual object Clone()
        {
            TrackTime clone = new TrackTime();
            clone.time = this.time;
            clone.Track = (Track)this.Track.Clone();
            return clone;
        }
    }
}
