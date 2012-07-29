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
using System.ComponentModel;
using System.Timers;

namespace JayDev.Notemaker.ViewModel
{
    public class CourseUseViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Private Local Variables, Constants

        private Course _currentCourse;
        private Track _currentTrack;
        private CourseRepository _repo;
        private MediaPlayer _player;
        private Dispatcher _uiDispatcher;

        private const bool IsKeepingBlankRowForEdit = true;

        #endregion 

        #region Public Properties & Backing Fields

        #region Notes

        private ObservableCollection<Note> _notes;
        public ObservableCollection<Note> Notes
        {
            get { return _notes; }
            set
            {
                if (null != _notes)
                {
                    _notes.CollectionChanged -= _notes_CollectionChanged;
                }
                _notes = value;
                if (null != value)
                {
                    this.RaisePropertyChanged("Notes");
                    _notes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_notes_CollectionChanged);
                }
            }
        }

        #endregion

        #region Tracks

        private ObservableCollection<Track> _tracks;
        public ObservableCollection<Track> Tracks { get { return _tracks; } }

        #endregion

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

        #region Volume

        /// <summary>
        /// The <see cref="Volume" /> property's name.
        /// </summary>
        public const string VolumePropertyName = "Volume";

        private double _volume = 0;

        /// <summary>
        /// Sets and gets the Volume property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                if (_volume == value)
                {
                    return;
                }

                _volume = value;
                RaisePropertyChanged(VolumePropertyName);
            }
        }

        #endregion Volume

        #endregion

        #region Commands

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
                                              SaveCourse();
                                          },
                                          () => false == _isBusy));
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
                                                      Track = _currentTrack
                                                      //,
                                                      //ParentCourse = _currentCourse
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
                                              throw new Exception("where do we call this from? code disabled in meantime..");
                                              //ThreadHelper.ExecuteBackground(delegate { SaveCourse(); });
                                              
                                          },
                                          (Note context) => true));
            }
        }


        private RelayCommand<Note> _noteEditCompletedCommand;
        public RelayCommand<Note> NoteEditCompletedCommand
        {
            get
            {
                return _noteEditCompletedCommand
                    ?? (_noteEditCompletedCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              if (false == string.IsNullOrWhiteSpace(context.Body))
                                              {
                                                  ThreadHelper.ExecuteBackground(delegate { _repo.SaveNote(_currentCourse, context); });
                                              }
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
                                              context.Start = new TrackTime()
                                              {
                                                  Track = _currentTrack,
                                                  Time = CurrentTrackPlayPosition
                                                  //,
                                                  //ParentCourse = _currentCourse
                                              };
                                          },
                                          (Note context) => true));
            }
        }

        private RelayCommand<Note> _setNoteEndTimeCommand;
        public RelayCommand<Note> SetNoteEndTimeCommand
        {
            get
            {
                return _setNoteEndTimeCommand
                    ?? (_setNoteEndTimeCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              context.End = new TrackTime()
                                              {
                                                  Track = _currentTrack,
                                                  Time = CurrentTrackPlayPosition
                                                  //,
                                                  //ParentCourse = _currentCourse
                                              };
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
                                              Stop();
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
                                              Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen), MessageType.Navigate);
                                          },
                                          () => true
                    //{
                    //    return CurrentTrackName != null && PlayStatus != Common.PlayStatus.Playing;
                    //}
                                          ));
            }
        }

        private RelayCommand<NavigateMessage> _navigateCommand;
        public RelayCommand<NavigateMessage> NavigateCommand
        {
            get
            {
                return _navigateCommand
                    ?? (_navigateCommand = new RelayCommand<NavigateMessage>(
                                          (NavigateMessage message) =>
                                          {
                                              Messenger.Default.Send(new NavigateArgs(message, _currentCourse), MessageType.Navigate);
                                          }));
            }
        }

        private RelayCommand _notesLoadedCommand;

        /// <summary>
        /// Gets the NotesLoadedCommand.
        /// </summary>
        public RelayCommand NotesLoadedCommand
        {
            get
            {
                return _notesLoadedCommand
                    ?? (_notesLoadedCommand = new RelayCommand(
                            () =>
                            {
                                _isBusy = true;
                                Notes = new ObservableCollection<Note>(_currentCourse.Notes);
                                _isBusy = false;
                                _isLoading = false;
                            }));
            }
        }

        #endregion

        #region Constructor

        public CourseUseViewModel(CourseRepository repo)
        {
            _repo = repo;
            //PlayCommand = new RelayCommand(Play, CanPlay);
            SelectTrackCommand = new RelayCommand<Track>(SelectTrack);

            _uiDispatcher = Dispatcher.CurrentDispatcher;

            _player = new MediaPlayer();
            _player.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_player_PropertyChanged);

            this.PropertyChanged += new PropertyChangedEventHandler(CourseUseViewModel_PropertyChanged);

            //set default volume to 100%
            _updateVolumeTimer = new Timer();
            _updateVolumeTimer.Interval = volUpdateGap;
            _updateVolumeTimer.AutoReset = false;
            _updateVolumeTimer.Elapsed += delegate
            {
                _updateVolumeTimer.Stop();
                _player.Volume(_volume);
            };
            Volume = 100;
        }

        #endregion



        int lastUpdatedAtMillisecond = 0;
        int volUpdateGap = 250;
        /// <summary>
        /// Since if we try to update the volume every value change of the slider (like 100 updates per second), mplayer will take a LOOONG time to process all the commands.
        /// So instead, we'll cap it at, say, 2 changes per second... and when the user stops dragging the slider, we'll have a timer who'll go and set the last value correctly.
        /// </summary>
        Timer _updateVolumeTimer;
        void CourseUseViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == VolumePropertyName)
            {
                if (Math.Abs(System.DateTime.Now.Millisecond - lastUpdatedAtMillisecond) > volUpdateGap)
                {
                    lastUpdatedAtMillisecond = System.DateTime.Now.Millisecond;
                    _player.Volume(_volume);
                }
                else
                {
                    _updateVolumeTimer.Stop();
                    _updateVolumeTimer.Start();
                }
            }
        }

        void _notes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            bool isSaveRequired = false;
            if (null != e.NewItems)
            {
                foreach (Note note in e.NewItems)
                {
                    note.PropertyChanged += new PropertyChangedEventHandler(note_PropertyChanged);
                    note.ChangeCommitted += new Note.ObjectChangeCommittedEventHandler(note_ChangeCommitted);
                    if (false == note.IsDirty && false == string.IsNullOrWhiteSpace(note.Body))
                        isSaveRequired = true;
                }
            }
            if (null != e.OldItems)
            {
                foreach (Note note in e.OldItems)
                {
                    note.PropertyChanged -= this.note_PropertyChanged;
                    note.ChangeCommitted -= note_ChangeCommitted;
                    isSaveRequired = true;
                }
            }

            if (false == _isLoading && false == _isBusy && isSaveRequired)
            {
                ThreadHelper.ExecuteBackground(delegate { SaveCourse(); });
            }
        }

        void note_ChangeCommitted(object sender, EventArgs e)
        {
            throw new Exception("need to save note instead of whole course");
            ThreadHelper.ExecuteBackground(delegate { SaveCourse(); });
        }

        void note_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int a = 5;
        }


        public void HandleKeypress(object sender, KeyEventArgs e)
        {
            switch(e.Key) {
                case Key.NumPad0:
                    _player.PlayPause();
                    e.Handled = true;
                    break;
                case Key.NumPad1:
                    _player.SeekRelative(-10);
                    e.Handled = true;
                    break;
                case Key.NumPad2:
                    _player.SeekRelative(-3);
                    e.Handled = true;
                    break;
                case Key.NumPad3:
                    _player.SeekRelative(3);
                    e.Handled = true;
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
            _isLoading = true;
            _currentCourse = course;
            Notes = new ExtendedObservableCollection<Note>();
            _tracks = new ObservableCollection<Track>(course.Tracks);
            _lastEmbeddedVideoHeight = course.EmbeddedVideoHeight;
            _lastEmbeddedVideoWidth = course.EmbeddedVideoWidth;

            if (null != course.LastTrack)
            {
                //JDW: track may have been removed
                if (course.Tracks.Any(x => x.FilePath == course.LastTrack.FilePath))
                {
                    SelectTrack(course.LastTrack);
                    CurrentTrackPlayPosition = course.LastTrackPosition;
                }
            }
        }


        private bool _isLoading = false;
        private bool _isBusy = false;

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
                    //If we have no play position, start from the beginning. otherwise, start at that spot
                    if (CurrentTrackPlayPosition == new TimeSpan())
                    {
                        PlayCurrentTrackFromBeginning();
                    }
                    else
                    {
                        PlayCurrentTrackFromBeginning();
                        SeekCurrentTrackToTime(CurrentTrackPlayPosition);
                    }
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
            }
            if (PlayStatus == Common.PlayStatus.Stopped || PlayStatus == Common.PlayStatus.Paused)
            {
                _player.Play(track.FilePath, VideoPanelPointer);
            }
            PlayStatus = Common.PlayStatus.Playing;
            _player.Seek(Convert.ToInt32(time.TotalSeconds));
        }
        private void SeekCurrentTrackToTime(TimeSpan time)
        {
            _player.Seek(Convert.ToInt32(time.TotalSeconds));
        }

        private void SaveCourse()
        {
            if (_isBusy)
            {
                throw new Exception("Error: shouldn't be trying to save when busy");
            }
            _currentCourse.Notes = new List<Note>(Notes);
            _currentCourse.EmbeddedVideoHeight = LastEmbeddedVideoHeight;
            _currentCourse.EmbeddedVideoWidth = LastEmbeddedVideoWidth;
            _currentCourse.LastTrack = _currentTrack;
            _currentCourse.LastTrackPosition = _currentTrackPlayPosition;
            _repo.SaveCourse(_currentCourse);
        }
        private void Stop()
        {
            PlayStatus = Common.PlayStatus.Stopped;
            CurrentTrackPlayPosition = new TimeSpan();
            _player.Stop();
        }
    }
}
