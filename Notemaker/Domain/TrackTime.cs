using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using JayDev.Notemaker.Common;
using Castle.ActiveRecord;

namespace JayDev.Notemaker
{
    [DataContract]
    [Serializable]
    [ActiveRecord("TrackTimes")]
    public class TrackTime : INotifyPropertyChanged, ICloneable
    {
        [PrimaryKey("TrackTimeID")]
        public int ID { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private Track track;
        [DataMember]
        [BelongsTo("TrackID", Cascade = CascadeEnum.SaveUpdate)]
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
        [Property("Time")]
        public TimeSpan Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged("Time");
            }
        }

        public string StringDisplayValue
        {
            get
            {
                StringBuilder resultBuilder = new StringBuilder();
                int indexInTrackList = -1;
                if (null != Track && null != Track.ParentCourse)
                {
                    //TODO: store this in DB
                    for (int i = 0; i < Track.ParentCourse.Tracks.Count; i++)
                    {
                        if (Track.ParentCourse.Tracks[i].FilePath == track.FilePath)
                        {
                            indexInTrackList = i;
                            break;
                        }
                    }
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
            return clone;
        }
    }
}
