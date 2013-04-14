using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace JayDev.MediaScribe
{
    [DataContract]
    public class Course
    {
        public int? ID { get; set; }

        public List<Note> _notes = new List<Note>();
        [DataMember]
        public List<Note> Notes { get { return _notes; } set { _notes = value; } }

        public List<Track> _tracks = new List<Track>();
        [DataMember]
        public List<Track> Tracks { get { return _tracks; } set { _tracks = value; } }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Nullable<double> EmbeddedVideoWidth { get; set; }
        [DataMember]
        public Nullable<double> EmbeddedVideoHeight { get; set; }

        public int? LastPlayedTrackID { get; set; }
        [DataMember]
        public TimeSpan LastPlayedTrackPosition { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateViewed { get; set; }

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
