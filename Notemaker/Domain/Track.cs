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
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public TimeSpan Length { get; set; }
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public bool IsVideo { get; set; }
        [DataMember]
        public double? AspectRatio { get; set; }

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
