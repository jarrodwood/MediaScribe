using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace JayDev.Notemaker
{
    [DataContract]
    public class Track
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
    }
}
