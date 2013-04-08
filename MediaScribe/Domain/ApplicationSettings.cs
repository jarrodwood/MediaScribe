using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace JayDev.MediaScribe
{

    [DataContract]
    public class ApplicationSettings : INotifyPropertyChanged
    {
        public virtual int? ID { get; set; }

        private bool? _generateThumbnails;
        [DataMember]
        public bool? GenerateThumbnails { get { return _generateThumbnails; }
            set
            {
                _generateThumbnails = value;
                OnPropertyChanged("GenerateThumbnails");
            }
        }

        private int? _fullscreenNotePanelWidth;
        [DataMember]
        public int? FullscreenNotePanelWidth
        {
            get { return _fullscreenNotePanelWidth; }
            set
            {
                _fullscreenNotePanelWidth = value;
                OnPropertyChanged("FullscreenNotePanelWidth");
            }
        }

        public string SerializedData { get; set; }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        public virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public static ApplicationSettings GetDefaultSettings()
        {
            ApplicationSettings settings = new ApplicationSettings();
            settings.GenerateThumbnails = true;
            settings.FullscreenNotePanelWidth = 600;
            return settings;
        }
    }
}
