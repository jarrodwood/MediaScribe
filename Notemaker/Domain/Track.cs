using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace JayDev.Notemaker
{
    [DataContract]
    [Serializable]
    public class Track : ICloneable
    {
        public virtual int? ID { get; set; }

        public virtual int ParentCourseID { get; set; }

        [DataMember]
        public virtual string Title { get; set; }
        [DataMember]
        public virtual TimeSpan Length { get; set; }
        [DataMember]
        public virtual string FilePath { get; set; }
        [DataMember]
        public virtual int IsVideo { get; set; }
        [DataMember]
        public virtual float? AspectRatio { get; set; }

        public virtual Course ParentCourse { get; set; }

        public virtual int? OrderNumber { get; set; }

        public virtual string FileName
        {
            get
            {
                return FilePath.Substring(FilePath.LastIndexOf(@"\") + 1);
            }
        }

        /// <summary>
        /// If the track has a Title, display that. Otherwise, display the file name
        /// </summary>
        public virtual string StringDisplayValue
        {
            get
            {
                if (false == string.IsNullOrEmpty(Title))
                    return Title;
                else
                    return FileName;
            }
        }

        public virtual void CopyTo(Track track)
        {
            track.Title = this.Title;
            track.Length = this.Length;
            track.FilePath = this.FilePath;
            track.IsVideo = this.IsVideo;
            track.AspectRatio = this.AspectRatio;
            track.OrderNumber = this.OrderNumber;
        }

        public virtual object Clone()
        {
            Track clone = new Track();
            this.CopyTo(clone);
            return clone;
        }
    }
}
