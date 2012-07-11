using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace JayDev.Notemaker
{
    [DataContract]
    public class Note : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        private TrackTime start;

        public TrackTime Start
        {
            get { return start; }
            set
            {
                start = value;
                OnPropertyChanged("Start");
                if (null != start)
                {
                    (start as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(startPropertChanged);
                }
            }
        }

        void startPropertChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Start");
        }

        [DataMember]
        public TrackTime End { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Body { get; set; }
        [DataMember]
        public int Rating { get; set; }

        [DataMember]
        public List<Tag> Tags { get; set; }

        public string TitleBody
        {
            get
            {
                string result = string.Empty;
                if (false == string.IsNullOrEmpty(Title))
                {
                    result = Title += " - ";
                }
                if (false == string.IsNullOrEmpty(Body))
                {
                    result += Body;
                }
                return result;
            }
        }


        public Note()
        {
            Tags = new List<Tag>();
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
    }
}
