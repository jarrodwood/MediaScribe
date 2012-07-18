using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Windows;

namespace JayDev.Notemaker
{
    [DataContract]
    public class Course : SavedFile
    {
        [DataMember]
        public List<Note> Notes = new List<Note>();
        [DataMember]
        public List<Track> Tracks = new List<Track>();
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Nullable<double> EmbeddedVideoWidth { get; set; }
        [DataMember]
        public Nullable<double> EmbeddedVideoHeight { get; set; }

        [DataMember]
        public Track LastTrack { get; set; }
        [DataMember]
        public TimeSpan LastTrackPosition { get; set; }

        public TimeSpan CourseLength
        {
            get
            {
                var trackLengthTicks = Tracks.Sum(x => x.Length.Ticks);
                return new TimeSpan(trackLengthTicks);
            }
        }

        public string Type
        {
            get
            {
                return "todo";
            }
        }
    }
}
