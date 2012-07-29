using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Windows;
using Castle.ActiveRecord;

namespace JayDev.Notemaker
{
    [DataContract]
    [ActiveRecord("Courses")]
    public class Course : ActiveRecordBase<Course>
    {
        [PrimaryKey("CourseID")]
        public int ID { get; set; }

        public IList<Note> _notes = new List<Note>();
        [DataMember]
        [HasMany(typeof(Note), Table = "Notes", ColumnKey = "CourseID", Cascade = ManyRelationCascadeEnum.None)]
        public IList<Note> Notes { get { return _notes; } set { _notes = value; } }

        public IList<Track> _tracks = new List<Track>();
        [DataMember]
        [HasMany(typeof(Track), Table = "Tracks", ColumnKey = "CourseID", Cascade = ManyRelationCascadeEnum.None)]
        public IList<Track> Tracks { get { return _tracks; } set { _tracks = value; } }
        [DataMember]
        [Property("Name")]
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
