using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.Notemaker.Model;
using System.Collections.ObjectModel;
using JayDev.Notemaker.Common;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Threading;

namespace JayDev.Notemaker.ViewModel
{
    public class CourseUseViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private Course _currentCourse;
        private Track _currentTrack;
        private CourseRepository _repo;
        private MediaPlayer _player;
        private Dispatcher _uiDispatcher;

        private ObservableCollection<Note> _notes;
        public ObservableCollection<Note> Notes { get { return _notes; } set { _notes = value; }  }

        private ObservableCollection<Track> _tracks;
        public ObservableCollection<Track> Tracks { get { return _tracks; } }

        #region CourseName

        /// <summary>
        /// The <see cref="CourseName" /> property's name.
        /// </summary>
        public const string CourseNamePropertyName = "CourseName";

        private string _courseName;

        /// <summary>
        /// Sets and gets the CourseName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string CourseName
        {
            get
            {
                return _courseName;
            }

            set
            {
                if (_courseName == value)
                {
                    return;
                }

                _courseName = value;
                RaisePropertyChanged(CourseNamePropertyName);
            }
        }

        #endregion CourseName

        #region CurrentTrackName

        /// <summary>
        /// The <see cref="CurrentTrackName" /> property's name.
        /// </summary>
        public const string CurrentTrackNamePropertyName = "CurrentTrackName";

        private string _currentTrackName;

        /// <summary>
        /// Sets and gets the CurrentTrackName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string CurrentTrackName
        {
            get
            {
                return _currentTrackName;
            }

            set
            {
                if (_currentTrackName == value)
                {
                    return;
                }

                _currentTrackName = value;
                RaisePropertyChanged(CurrentTrackNamePropertyName);
            }
        }

        #endregion CurrentTrackName

        #region CurrentTrackPlayPosition

        /// <summary>
        /// The <see cref="CurrentTrackPlayPosition" /> property's name.
        /// </summary>
        public const string CurrentTrackPlayPositionPropertyName = "CurrentTrackPlayPosition";

        private TimeSpan _currentTrackPlayPosition;

        /// <summary>
        /// Sets and gets the CurrentTrackPlayPosition property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public TimeSpan CurrentTrackPlayPosition
        {
            get
            {
                return _currentTrackPlayPosition;
            }

            set
            {
                if (_currentTrackPlayPosition == value)
                {
                    return;
                }

                _currentTrackPlayPosition = value;
                RaisePropertyChanged(CurrentTrackPlayPositionPropertyName);
            }
        }

        #endregion CurrentTrackPlayPosition

        #region CurrentTrackTotalLength

        /// <summary>
        /// The <see cref="CurrentTrackTotalLength" /> property's name.
        /// </summary>
        public const string CurrentTrackTotalLengthPropertyName = "CurrentTrackTotalLength";

        private TimeSpan _currentTrackTotalLength;

        /// <summary>
        /// Sets and gets the CurrentTrackTotalLength property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public TimeSpan CurrentTrackTotalLength
        {
            get
            {
                return _currentTrackTotalLength;
            }

            set
            {
                if (_currentTrackTotalLength == value)
                {
                    return;
                }

                _currentTrackTotalLength = value;
                RaisePropertyChanged(CurrentTrackTotalLengthPropertyName);
            }
        }

        #endregion CurrentTrackTotalLength

        #region PlayStatus

        /// <summary>
        /// The <see cref="PlayStatus" /> property's name.
        /// </summary>
        public const string PlayStatusPropertyName = "PlayStatus";

        private PlayStatus _playStatus;

        /// <summary>
        /// Sets and gets the PlayStatus property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PlayStatus PlayStatus
        {
            get
            {
                return _playStatus;
            }

            set
            {
                if (_playStatus == value)
                {
                    return;
                }

                _playStatus = value;
                RaisePropertyChanged(PlayStatusPropertyName);
            }
        }

        #endregion PlayStatus

        #region LastEmbeddedVideoWidth

        /// <summary>
        /// The <see cref="LastEmbeddedVideoWidth" /> property's name.
        /// </summary>
        public const string LastEmbeddedVideoWidthPropertyName = "LastEmbeddedVideoWidth";

        private double? _lastEmbeddedVideoWidth;

        /// <summary>
        /// Sets and gets the LastEmbeddedVideoWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double? LastEmbeddedVideoWidth
        {
            get
            {
                return _lastEmbeddedVideoWidth;
            }

            set
            {
                if (_lastEmbeddedVideoWidth == value)
                {
                    return;
                }

                _lastEmbeddedVideoWidth = value;
                RaisePropertyChanged(LastEmbeddedVideoWidthPropertyName);
            }
        }

        #endregion LastEmbeddedVideoWidth

        #region LastEmbeddedVideoHeight

        /// <summary>
        /// The <see cref="LastEmbeddedVideoHeight" /> property's name.
        /// </summary>
        public const string LastEmbeddedVideoHeightPropertyName = "LastEmbeddedVideoHeight";

        private double? _lastEmbeddedVideoHeight;

        /// <summary>
        /// Sets and gets the LastEmbeddedVideoHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double? LastEmbeddedVideoHeight
        {
            get
            {
                return _lastEmbeddedVideoHeight;
            }

            set
            {
                if (_lastEmbeddedVideoHeight == value)
                {
                    return;
                }

                _lastEmbeddedVideoHeight = value;
                RaisePropertyChanged(LastEmbeddedVideoHeightPropertyName);
            }
        }

        #endregion LastEmbeddedVideoHeight

        #region IsCurrentTrackVideo

        /// <summary>
        /// The <see cref="IsCurrentTrackVideo" /> property's name.
        /// </summary>
        public const string IsCurrentTrackVideoPropertyName = "IsCurrentTrackVideo";

        private bool _isCurrentTrackVideo = false;

        /// <summary>
        /// Sets and gets the IsCurrentTrackVideo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCurrentTrackVideo
        {
            get
            {
                return _isCurrentTrackVideo;
            }

            set
            {
                if (_isCurrentTrackVideo == value)
                {
                    return;
                }

                _isCurrentTrackVideo = value;
                RaisePropertyChanged(IsCurrentTrackVideoPropertyName);
            }
        }

        #endregion IsCurrentTrackVideo

        #region CurrentTrackVideoAspectRatio

        /// <summary>
        /// The <see cref="CurrentTrackVideoAspectRatio" /> property's name.
        /// </summary>
        public const string CurrentTrackVideoAspectRatioPropertyName = "CurrentTrackVideoAspectRatio";

        private double? _currentTrackVideoAspectRatio;

        /// <summary>
        /// Sets and gets the CurrentTrackVideoAspectRatio property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double? CurrentTrackVideoAspectRatio
        {
            get
            {
                return _currentTrackVideoAspectRatio;
            }

            set
            {
                if (_currentTrackVideoAspectRatio == value)
                {
                    return;
                }

                _currentTrackVideoAspectRatio = value;
                RaisePropertyChanged(CurrentTrackVideoAspectRatioPropertyName);
            }
        }

        #endregion CurrentTrackVideoAspectRatio

        #region VideoPanelPointerPropertyName

        /// <summary>
        /// The <see cref="VideoPanelPointer" /> property's name.
        /// </summary>
        public const string VideoPanelPointerPropertyName = "VideoPanelPointer";

        private IntPtr _videoPanelPointer;

        /// <summary>
        /// Sets and gets the VideoPanelPointer property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IntPtr VideoPanelPointer
        {
            get
            {
                return _videoPanelPointer;
            }

            set
            {
                if (_videoPanelPointer == value)
                {
                    return;
                }

                _videoPanelPointer = value;
                RaisePropertyChanged(VideoPanelPointerPropertyName);
            }
        }

        #endregion

        private RelayCommand _saveCourseCommand;

        /// <summary>
        /// Gets the SaveCourseCommand.
        /// </summary>
        public RelayCommand SaveCourseCommand
        {
            get
            {
                return _saveCourseCommand
                    ?? (_saveCourseCommand = new RelayCommand(
                                          () =>
                                          {
                                              _currentCourse.Notes = new List<Note>(Notes);
                                              _currentCourse.EmbeddedVideoHeight = LastEmbeddedVideoHeight;
                                              _currentCourse.EmbeddedVideoWidth = LastEmbeddedVideoWidth;
                                              _repo.SaveCourse(_currentCourse);
                                          }));
            }
        }

        private RelayCommand _playPauseCommand;

        /// <summary>
        /// Gets the PlayCommand.
        /// </summary>
        public RelayCommand PlayPauseCommand
        {
            get
            {
                return _playPauseCommand
                    ?? (_playPauseCommand = new RelayCommand(
                                          () =>
                                          {
                                              PlayPause();
                                          },
                                          () => true
                                          //{
                                          //    return CurrentTrackName != null && PlayStatus != Common.PlayStatus.Playing;
                                          //}
                                          ));
            }
        }


        private RelayCommand<Note> _prepareNoteForEditCommand;
        public RelayCommand<Note> PrepareNoteForEditCommand
        {
            get
            {
                return _prepareNoteForEditCommand
                    ?? (_prepareNoteForEditCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              if(_currentTrack != null && string.IsNullOrEmpty(context.Body)) {
                                                  context.Start = new TrackTime()
                                                  {
                                                      Time = CurrentTrackPlayPosition,
                                                      Track = _currentTrack,
                                                      TracksCollection = new List<Track>(Tracks)
                                                  };
                                              }
                                          },
                                          (Note context) => true));
            }
        }


        private RelayCommand<Note> _noteSavedCommand;
        public RelayCommand<Note> NoteSavedCommand
        {
            get
            {
                return _noteSavedCommand
                    ?? (_noteSavedCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              //when a note is saved, we save the course so that changed should never be lost...
                                              _currentCourse.Notes = new List<Note>(Notes);
                                              _repo.SaveCourse(_currentCourse);
                                          },
                                          (Note context) => true));
            }
        }

        private RelayCommand<Note> _setNoteStartTimeCommand;
        public RelayCommand<Note> SetNoteStartTimeCommand
        {
            get
            {
                return _setNoteStartTimeCommand
                    ?? (_setNoteStartTimeCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              context.Start.Track = _currentTrack;
                                              context.Start.Time = CurrentTrackPlayPosition;
                                          },
                                          (Note context) => true));
            }
        }

        private RelayCommand<Note> _playNoteCommand;
        public RelayCommand<Note> PlayNoteCommand
        {
            get
            {
                return _playNoteCommand
                    ?? (_playNoteCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              PlayGivenTrackFromTime(context.Start.Track, context.Start.Time);
                                          },
                                          (Note context) => true));
            }
        }

        

        private RelayCommand _stopCommand;

        /// <summary>
        /// Gets the StopCommand.
        /// </summary>
        public RelayCommand StopCommand
        {
            get
            {
                return _stopCommand
                    ?? (_stopCommand = new RelayCommand(
                                          () =>
                                          {
                                              PlayStatus = Common.PlayStatus.Stopped;
                                              _player.Stop();
                                          },
                                          () => true
                                          ));
            }
        }


        private RelayCommand<TimeSpan> _seekToCommand;

        /// <summary>
        /// Gets the SeekToCommand.
        /// </summary>
        public RelayCommand<TimeSpan> SeekToCommand
        {
            get
            {
                return _seekToCommand
                    ?? (_seekToCommand = new RelayCommand<TimeSpan>(
                                          (TimeSpan time) =>
                                          {
                                              SeekCurrentTrackToTime(time);
                                          },
                                          (TimeSpan time) => true
                                          ));
            }
        }


        //public RelayCommand PlayCommand { get; private set; }
        public RelayCommand<Track> SelectTrackCommand { get; private set; }


        private RelayCommand _toggleFullscreenCommand;

        /// <summary>
        /// Gets the ToggleFullscreenCommand.
        /// </summary>
        public RelayCommand ToggleFullscreenCommand
        {
            get
            {
                return _toggleFullscreenCommand
                    ?? (_toggleFullscreenCommand = new RelayCommand(
                                          () =>
                                          {
                                              Messenger.Default.Send(NavigateMessage.ToggleFullscreen, MessageType.Navigate);
                                          },
                                          () => true
                    //{
                    //    return CurrentTrackName != null && PlayStatus != Common.PlayStatus.Playing;
                    //}
                                          ));
            }
        }


        public CourseUseViewModel(CourseRepository repo)
        {
            _repo = repo;
            //PlayCommand = new RelayCommand(Play, CanPlay);
            SelectTrackCommand = new RelayCommand<Track>(SelectTrack);

            _uiDispatcher = Dispatcher.CurrentDispatcher;

            _player = new MediaPlayer();
            _player.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_player_PropertyChanged);
            Messenger.Default.Register<KeyEventArgs>(this, 999, (message) => HandleKeyPress(message));
        }


        private void HandleKeyPress(KeyEventArgs e)
        {
            switch(e.Key) {
                case Key.NumPad0:
                _player.PlayPause();
                    break;
                case Key.NumPad1:
                    _player.SeekRelative(-10);
                    break;
                case Key.NumPad2:
                    _player.SeekRelative(-3);
                    break;
                case Key.NumPad3:
                    _player.SeekRelative(3);
                    break;
            }
        }


        void _player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == MediaPlayer.CurrentPlayPositionPropertyName)
            {
                CurrentTrackPlayPosition = _player.CurrentPlayPosition;
            }
        }

        public void SetCurrentCourse(Course course)
        {
            _currentCourse = course;
            _notes = new ObservableCollection<Note>(course.Notes);
            _notes.Add(new Note());
            _tracks = new ObservableCollection<Track>(course.Tracks);
            //Notes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Notes_CollectionChanged);
        }

        private void SelectTrack(Track track)
        {
            if (PlayStatus != Common.PlayStatus.Stopped)
            {
                _player.Stop();
            }
            _currentTrack = track;
            CurrentTrackPlayPosition = new TimeSpan();
            CurrentTrackName = _currentTrack.StringDisplayValue;
            //Propert
            CurrentTrackTotalLength = track.Length;
        }


        private void PlayPause()
        {
            if (null == _currentTrack)
            {
                if (null == Tracks || Tracks.Count == 0)
                {
                    throw new Exception("what to play?");
                }
                else
                {
                    Track firstTrack = Tracks.First();
                    SelectTrack(firstTrack);
                    PlayCurrentTrackFromBeginning();
                }
            }
            else
            {
                if (_player.PlayingStatus == Common.PlayStatus.Stopped)
                {
                    PlayCurrentTrackFromBeginning();
                }
                else
                {
                    if (_player.PlayingStatus == Common.PlayStatus.Paused)
                    {
                        PlayStatus = Common.PlayStatus.Playing;
                    }
                    else if (_player.PlayingStatus == Common.PlayStatus.Playing)
                    {
                        PlayStatus = Common.PlayStatus.Paused;
                    }
                    _player.PlayPause();
                }
            }
        }
        private void PlayCurrentTrackFromBeginning()
        {
            _player.Play(_currentTrack.FilePath, VideoPanelPointer);
            PlayStatus = Common.PlayStatus.Playing;
        }
        private void PlayGivenTrackFromTime(Track track, TimeSpan time)
        {
            if (null == _currentTrack || track.FilePath != _currentTrack.FilePath)
            {
                _player.Stop();
                SelectTrack(track);
                _player.Play(track.FilePath, VideoPanelPointer);
            }
            _player.Seek(Convert.ToInt32(time.TotalSeconds));
        }
        private void SeekCurrentTrackToTime(TimeSpan time)
        {
            _player.Seek(Convert.ToInt32(time.TotalSeconds));
        }
    }
}
