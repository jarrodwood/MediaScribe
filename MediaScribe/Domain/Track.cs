using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;
using System.ComponentModel;

namespace JayDev.MediaScribe
{
    [DataContract]
    [Serializable]
    public class Track : ICloneable, INotifyPropertyChanged
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
        public virtual bool IsVideo { get; set; }
        [DataMember]
        public virtual float? AspectRatio { get; set; }

        public virtual Course ParentCourse { get; set; }

        public virtual int? TrackNumber { get; set; }

        public virtual long? FileSize { get; set; }

        public virtual event PropertyChangedEventHandler PropertyChanged;

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
            track.ID = this.ID;
            track.Title = this.Title;
            track.Length = this.Length;
            track.FilePath = this.FilePath;
            track.IsVideo = this.IsVideo;
            track.AspectRatio = this.AspectRatio;
            track.TrackNumber = this.TrackNumber;
        }

        public virtual object Clone()
        {
            Track clone = new Track();
            this.CopyTo(clone);
            return clone;
        }

        
        private bool _isPlaying;
        public virtual bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                _isPlaying = value;

                OnPropertyChanged("IsPlaying");
            }
        }



        // Create the OnPropertyChanged method to raise the event
        public virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
