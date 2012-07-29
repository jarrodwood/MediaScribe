using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Castle.ActiveRecord;

namespace JayDev.Notemaker
{
    [DataContract]
    [Serializable]
    [ActiveRecord("Tracks")]
    public class Track : ActiveRecordBase<Track>, ICloneable
    {
        [PrimaryKey("TrackID")]
        public int ID { get; set; }

        [DataMember]
        [Property("Title")]
        public string Title { get; set; }
        [DataMember]
        [Property("Length")]
        public TimeSpan Length { get; set; }
        [DataMember]
        [Property("FilePath")]
        public string FilePath { get; set; }
        [DataMember]
        [Property("IsVideo")]
        public bool IsVideo { get; set; }
        [DataMember]
        public double? AspectRatio { get; set; }

        [BelongsTo("CourseID")]
        public Course ParentCourse { get; set; }

        public string FileName
        {
            get
            {
                return FilePath.Substring(FilePath.LastIndexOf(@"\") + 1);
            }
        }

        /// <summary>
        /// If the track has a Title, display that. Otherwise, display the file name
        /// </summary>
        public string StringDisplayValue
        {
            get
            {
                if (false == string.IsNullOrEmpty(Title))
                    return Title;
                else
                    return FileName;
            }
        }

        public object Clone()
        {
            Track clone = new Track();
            clone.Title = this.Title;
            clone.Length = this.Length;
            clone.FilePath = this.FilePath;
            return clone;
        }
    }
}
