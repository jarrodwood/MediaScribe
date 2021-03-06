﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.MediaScribe.Model;
using System.Collections.ObjectModel;
using JayDev.MediaScribe.Common;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Threading;
using System.ComponentModel;
using System.Timers;
using System.Runtime.InteropServices;
using JayDev.MediaScribe.View.Controls;
using JayDev.MediaScribe.Core;
using MediaScribe.Common;
using JayDev.MediaScribe.View;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;

namespace JayDev.MediaScribe.ViewModel
{
    public class CourseUseViewModel : ViewModelBase
    {
        #region Private Local Variables, Constants

        private Course _currentCourse;
        private Track _currentTrack;
        private CourseRepository _repo;
        private Dispatcher _uiDispatcher;
        private MediaPlayerVLC _player;

        private const bool IsKeepingBlankRowForEdit = true;

        #endregion 

        //TODO: MOVE THIS OUT OF VIEWMODEL!
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        AsyncWorker findTextWorker = new AsyncWorker();
        object findTextMatchesLockToken = new object();
        List<Tuple<Note, HighlightMatch>> findTextMatches = new List<Tuple<Note, HighlightMatch>>();

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
                    foreach (Note note in _notes)
                    {
                        note.PropertyChanged -= new PropertyChangedEventHandler(note_PropertyChanged);
                        note.ChangeCommitted -= new Note.ObjectChangeCommittedEventHandler(note_ChangeCommitted);
                    }
                }
                _notes = value;
                if (null != value)
                {
                    _notes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_notes_CollectionChanged);

                    //hook up event handlers for the notes, so we can save any changes to them
                    foreach (Note note in _notes)
                    {
                        note.PropertyChanged += new PropertyChangedEventHandler(note_PropertyChanged);
                        note.ChangeCommitted += new Note.ObjectChangeCommittedEventHandler(note_ChangeCommitted);
                    }

                    this.RaisePropertyChanged("Notes");
                }
            }
        }

        #endregion

        #region Tracks

        private ObservableCollection<Track> _tracks = new ObservableCollection<Track>();
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

        
        #region FindTextInput

        /// <summary>
        /// The <see cref="FindTextInput" /> property's name.
        /// </summary>
        public const string FindTextInputPropertyName = "FindTextInput";

        private string _findTextInput;

        /// <summary>
        /// Sets and gets the FindTextInput property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string FindTextInput
        {
            get
            {
                return _findTextInput;
            }

            set
            {
                if (_findTextInput == value)
                {
                    return;
                }

                _findTextInput = value;
                RaisePropertyChanged(FindTextInputPropertyName);
            }
        }

        #endregion FindTextInput


        
        #region FindTextMatchCount

        /// <summary>
        /// The <see cref="FindTextMatchCount" /> property's name.
        /// </summary>
        public const string FindTextMatchCountPropertyName = "FindTextMatchCount";

        private int _findTextMatchCount;

        /// <summary>
        /// Sets and gets the FindTextMatchCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int FindTextMatchCount
        {
            get
            {
                return _findTextMatchCount;
            }

            set
            {
                if (_findTextMatchCount == value)
                {
                    return;
                }

                _findTextMatchCount = value;
                RaisePropertyChanged(FindTextMatchCountPropertyName);
            }
        }

        #endregion FindTextMatchCount

        
        #region FindTextMatchIndex

        /// <summary>
        /// The <see cref="FindTextMatchIndex" /> property's name.
        /// </summary>
        public const string FindTextMatchIndexPropertyName = "FindTextMatchIndex";

        private int _findTextMatchIndex;

        /// <summary>
        /// Sets and gets the FindTextMatchIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int FindTextMatchIndex
        {
            get
            {
                return _findTextMatchIndex;
            }

            set
            {
                if (_findTextMatchIndex == value)
                {
                    return;
                }

                _findTextMatchIndex = value;
                RaisePropertyChanged(FindTextMatchIndexPropertyName);
            }
        }

        #endregion FindTextMatchIndex

        
        #region FindTextMatchNote

        /// <summary>
        /// The <see cref="FindTextMatchNote" /> property's name.
        /// </summary>
        public const string FindTextMatchNotePropertyName = "FindTextMatchNote";

        private Note _findTextMatchNote;

        /// <summary>
        /// Sets and gets the FindTextMatchNote property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Note FindTextMatchNote
        {
            get
            {
                return _findTextMatchNote;
            }

            set
            {
                if (_findTextMatchNote == value)
                {
                    return;
                }

                _findTextMatchNote = value;
                RaisePropertyChanged(FindTextMatchNotePropertyName);
            }
        }

        #endregion FindTextMatchNote
						
				

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

        #region VirtualEmbeddedVideoHeight

        /// <summary>
        /// The <see cref="VirtualEmbeddedVideoHeight" /> property's name.
        /// </summary>
        public const string VirtualEmbeddedVideoHeightPropertyName = "VirtualEmbeddedVideoHeight";

        /// <summary>
        /// Sets and gets the VirtualEmbeddedVideoHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double VirtualEmbeddedVideoHeight
        {
            get
            {
                if (IsCurrentTrackVideo)
                {
                    return LastEmbeddedVideoHeight ?? 300;
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (IsCurrentTrackVideo)
                {
                    LastEmbeddedVideoHeight = value;
                }
                else
                {
                }
            }
        }

        #endregion

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

                //when we change tracks, we show or hide the video control based upon whether the track /has/ video or not.
                //as such, we need to inform the UI that the height of the video is changed (since we can't literally collapse grid rows in
                //WPF, we have to set the height to 0)
                RaisePropertyChanged(VirtualEmbeddedVideoHeightPropertyName);
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

        #region Speed

        /// <summary>
        /// The <see cref="Speed" /> property's name.
        /// </summary>
        public const string SpeedPropertyName = "Speed";

        private double _speed = 1f;

        /// <summary>
        /// Sets and gets the Speed property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Speed
        {
            get
            {
                return _speed;
            }

            set
            {
                if (_speed == value)
                {
                    return;
                }

                _speed = value;
                RaisePropertyChanged(SpeedPropertyName);
            }
        }

        #endregion Speed

        #region IsMuted

        /// <summary>
        /// The <see cref="IsMuted" /> property's name.
        /// </summary>
        public const string IsMutedPropertyName = "IsMuted";

        private bool _isMuted = false;

        /// <summary>
        /// Sets and gets the IsMuted property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMuted
        {
            get
            {
                return _isMuted;
            }

            set
            {
                if (_isMuted == value)
                {
                    return;
                }

                _isMuted = value;
                RaisePropertyChanged(IsMutedPropertyName);
            }
        }

        #endregion

        #region ThumbnailGenerator

        /// <summary>
        /// The <see cref="ThumbnailGenerator" /> property's name.
        /// </summary>
        public const string ThumbnailGeneratorPropertyName = "ThumbnailGenerator";

        private ThumbnailGenerator _thumbnailGenerator = null;

        /// <summary>
        /// Sets and gets the ThumbnailGenerator property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ThumbnailGenerator ThumbnailGenerator
        {
            get
            {
                return _thumbnailGenerator;
            }

            set
            {
                if (_thumbnailGenerator == value)
                {
                    return;
                }

                _thumbnailGenerator = value;
                RaisePropertyChanged(ThumbnailGeneratorPropertyName);
            }
        }

        #endregion


        #region FullscreenWindowSize

        /// <summary>
        /// The <see cref="FullscreenWindowSize" /> property's name.
        /// </summary>
        public const string FullscreenWindowSizePropertyName = "FullscreenWindowSize";

        private System.Windows.Size _fullscreenWindowSize = System.Windows.Size.Empty;

        /// <summary>
        /// Sets and gets the FullscreenWindowSize property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public System.Windows.Size FullscreenWindowSize
        {
            get
            {
                return _fullscreenWindowSize;
            }

            set
            {
                if (_fullscreenWindowSize == value)
                {
                    return;
                }

                _fullscreenWindowSize = value;
                RaisePropertyChanged(FullscreenWindowSizePropertyName);
                //when the fullscreen window size property changes, we need to notify that the controlled fullscreen notes width property has changed too.
                //this is because it's derived from the fullscreen window size property.
                RaisePropertyChanged(ControlledFullscreenNotePanelWidthPropertyName);
            }
        }

        #endregion

        #region ControlledFullscreenNotesWidth

        /// <summary>
        /// The <see cref="ControlledFullscreenNotePanelWidth" /> property's name.
        /// </summary>
        public const string ControlledFullscreenNotePanelWidthPropertyName = "ControlledFullscreenNotePanelWidth";

        /// <summary>
        /// Sets and gets the ControlledFullscreenNotePanelWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ControlledFullscreenNotePanelWidth
        {
            get
            {
                return Math.Min(FullscreenWindowSize.Width - Constants.MINIMUM_FULLSCREN_VIDEO_WIDTH, (double)SettingManager.ApplicationSettings.FullscreenNotePanelWidth);
            }
        }

        #endregion

        #endregion

        #region Commands

        #region NextTrackCommand
        private RelayCommand _nextTrackCommand;

        /// <summary>
        /// Gets the NextTrackCommand.
        /// </summary>
        public RelayCommand NextTrackCommand
        {
            get
            {
                return _nextTrackCommand
                    ?? (_nextTrackCommand = new RelayCommand(
                                          () =>
                                          {
                                              int currentTrackIndex = Tracks.IndexOf(_currentTrack);
                                              int nextTrackIndex = currentTrackIndex == Tracks.Count - 1 ? 0 : currentTrackIndex + 1;
                                              PlayFile(Tracks.ToList(), nextTrackIndex, TimeSpan.Zero, true);
                                          }));
            }
        }

        #endregion

        #region PrevTrackCommand

        private RelayCommand _prevTrackCommand;

        /// <summary>
        /// Gets the PrevTrackCommand.
        /// </summary>
        public RelayCommand PrevTrackCommand
        {
            get
            {
                return _prevTrackCommand
                    ?? (_prevTrackCommand = new RelayCommand(
                                          () =>
                                          {
                                              int currentTrackIndex = Tracks.IndexOf(_currentTrack);
                                              int prevTrackIndex = currentTrackIndex == 0 ? Tracks.Count - 1 : currentTrackIndex - 1;
                                              PlayFile(Tracks.ToList(), prevTrackIndex, TimeSpan.Zero, true);
                                          }));
            }
        }

        #endregion

        #region SaveCourseCommand

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

        #endregion

        #region PlayPauseCommand

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

        #endregion

        #region PrepareNoteForEditCommand

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
                                                  context.StartTime = CurrentTrackPlayPosition;
                                                  //if the user's configured it, let's push back the start time by a configured number of settings.
                                                  if (null != SettingManager.ApplicationSettings.NewNoteRewindSeconds && SettingManager.ApplicationSettings.NewNoteRewindSeconds > 0)
                                                  {
                                                      context.StartTime = context.StartTime.Value.Subtract(new TimeSpan(0, 0, SettingManager.ApplicationSettings.NewNoteRewindSeconds.Value));
                                                      //ensure that it's never below zero.
                                                      //TODO: perhaps this should should jump back to the end of the previous track instad?
                                                      if (context.StartTime.Value.TotalMilliseconds < 0)
                                                          context.StartTime = new TimeSpan();
                                                  }
                                                  context.StartTrackNumber = _currentTrack.TrackNumber;
                                              }
                                          },
                                          (Note context) => true));
            }
        }

        #endregion

        #region NoteSavedCommand

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

        #endregion

        #region NoteEditCompletedCommand

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
                                                  //ThreadHelper.ExecuteBackground(delegate { _repo.SaveNote(_currentCourse, context); });
                                                  //_repo.SaveNote(_currentCourse, context);
                                              }
                                          },
                                          (Note context) => true));
            }
        }

        #endregion

        #region SetNoteStartTimeCommand

        private RelayCommand<Note> _setNoteStartTimeCommand;
        public RelayCommand<Note> SetNoteStartTimeCommand
        {
            get
            {
                return _setNoteStartTimeCommand
                    ?? (_setNoteStartTimeCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              context.StartTrackNumber = _currentTrack.TrackNumber;
                                              context.StartTime = CurrentTrackPlayPosition;
                                          },
                                          (Note context) => true));
            }
        }

        #endregion

        #region SetNoteEndTimeCommand

        private RelayCommand<Note> _setNoteEndTimeCommand;
        public RelayCommand<Note> SetNoteEndTimeCommand
        {
            get
            {
                return _setNoteEndTimeCommand
                    ?? (_setNoteEndTimeCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              context.EndTrackNumber = _currentTrack.TrackNumber;
                                              context.EndTime = CurrentTrackPlayPosition;
                                          },
                                          (Note context) => true));
            }
        }

        #endregion

        #region PlayNoteCommand

        private RelayCommand<Note> _playNoteCommand;
        public RelayCommand<Note> PlayNoteCommand
        {
            get
            {
                return _playNoteCommand
                    ?? (_playNoteCommand = new RelayCommand<Note>(
                                          (Note context) =>
                                          {
                                              if (null != context && null != context.StartTime)
                                              {
                                                  int trackIndex = Tracks.IndexOf(Tracks.First(x => x.TrackNumber == context.StartTrackNumber));
                                                  PlayFile(Tracks.ToList(), trackIndex, context.StartTime.Value, false);
                                              }
                                          },
                                          (Note context) => true));
            }
        }

        #endregion

        #region StopCommand

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

        #endregion

        #region SeekToCommand

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
                                              if (null != _currentTrack)
                                              {
                                                  int currentTrackIndex = Tracks.IndexOf(_currentTrack);
                                                  PlayFile(Tracks.ToList(), currentTrackIndex, time, false);
                                              }
                                          },
                                          (TimeSpan time) => true
                                          ));
            }
        }

        #endregion

        #region SelectTrackCommand

        private RelayCommand<Track> _selectTrackCommand;

        /// <summary>
        /// Gets the SelectTrackCommand.
        /// </summary>
        public RelayCommand<Track> SelectTrackCommand
        {
            get
            {
                return _selectTrackCommand
                    ?? (_selectTrackCommand = new RelayCommand<Track>(
                                          (Track track) =>
                                          {
                                              if (null == track)
                                                  return;

                                              int trackIndex = Tracks.IndexOf(track);
                                              PlayFile(Tracks.ToList(), trackIndex, TimeSpan.Zero, false);
                                          }));
            }
        }

        #endregion

        #region ToggleFullscreenCommand

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
                                              Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen, TabChangeSource.Application), MessageType.PerformNavigation);
                                          },
                                          () => true
                    //{
                    //    return CurrentTrackName != null && PlayStatus != Common.PlayStatus.Playing;
                    //}
                                          ));
            }
        }

        #endregion

        #region NotesLoadedCommand

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
                                if (null != _currentCourse)
                                {
                                    _isBusy = true;
                                    Notes = new ObservableCollection<Note>(_currentCourse.Notes);
                                    _isBusy = false;
                                    _isLoading = false;
                                }
                            }));
            }
        }

        #endregion

        #region ToggleMuteCommand

        private RelayCommand _toggleMuteCommand;

        /// <summary>
        /// Gets the ToggleMuteCommand.
        /// </summary>
        public RelayCommand ToggleMuteCommand
        {
            get
            {
                return _toggleMuteCommand
                    ?? (_toggleMuteCommand = new RelayCommand(
                                          () =>
                                          {
                                              IsMuted = !IsMuted;
                                              _player.ToggleMute();
                                          }));
            }
        }

        #endregion


        #region FindTextNextMatchCommand

        private RelayCommand _findTextNextMatchCommand;

        /// <summary>
        /// Gets the FindTextNextMatchCommand.
        /// </summary>
        public RelayCommand FindTextNextMatchCommand
        {
            get
            {
                return _findTextNextMatchCommand
                    ?? (_findTextNextMatchCommand = new RelayCommand(
                                          () =>
                                          {
                                              HandleFindTextMatchNextAndPrevious(moveNext: true);
                                          }));
            }
        }

        #endregion
        

        #region FindTextPreviousMatchCommand

        private RelayCommand _findTextPreviousMatchCommand;

        /// <summary>
        /// Gets the FindTextPreviousMatchCommand.
        /// </summary>
        public RelayCommand FindTextPreviousMatchCommand
        {
            get
            {
                return _findTextPreviousMatchCommand
                    ?? (_findTextPreviousMatchCommand = new RelayCommand(
                                          () =>
                                          {
                                              HandleFindTextMatchNextAndPrevious(moveNext: false);
                                          }));
            }
        }

        #endregion

        #region FindTextCloseCommand

        private RelayCommand _findTextCloseCommand;

        /// <summary>
        /// Gets the FindTextCloseCommand.
        /// </summary>
        public RelayCommand FindTextCloseCommand
        {
            get
            {
                return _findTextCloseCommand
                    ?? (_findTextCloseCommand = new RelayCommand(
                                          () =>
                                          {
                                              FindTextInput = null;
                                              _lastFindTextInput = null;
                                              findTextMatches.Clear();
                                          }));
            }
        }

        #endregion

        /// <summary>
        /// When we're finding text, it's useful to know what the last input WAS so we can determine if this will filter down the last result
        /// set, or effectively add more results into it. (e.g. typing "abc" then changed to "abcd" means we only have to search in the results
        /// that matched "abc", whereas if it goes from "abc" to "ac" it means we need to re-search all notes.
        /// </summary>
        private string _lastFindTextInput;
        
        #endregion

        #region Constructor

        public CourseUseViewModel(CourseRepository repo, UnityContainer unityContainer)
            : base(unityContainer)
        {
            _repo = repo;

            _uiDispatcher = Dispatcher.CurrentDispatcher;

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

            Volume = Constants.DEFAULT_VOLUME;

            if (null == ThumbnailGenerator)
                ThumbnailGenerator = new Core.ThumbnailGenerator();


            //when the application settings change, ensure we're notified about it so that any derived
            //properties can be updated too.
            Messenger.Default.Register<ApplicationSettings>(this, MessageType.ApplicationSettingsChanged, (message) =>
            {
                RaisePropertyChanged(ControlledFullscreenNotePanelWidthPropertyName);
            });

            Messenger.Default.Register<string>(this, MessageType.SaveCourseAndExportToExcel, (message) => {
                SaveCourse();
                ExportCourseToExcel(_currentCourse);
            });

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
                if (null != _player)
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

            //we only want to execute this once, when the video panel pointer is initially set... not afterwrds.
            if (null == _player)
            {
                _player = new MediaPlayerVLC();
                _player.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_player_PropertyChanged);
                //the volume will have been given an initial value before this event is raised
                _player.Volume(_volume);
                _player.TrackFinished += new MediaPlayerVLC.TrackFinishedEventHandler(_player_TrackFinished);
            }

            if (e.PropertyName == FindTextInputPropertyName)
            {
                findTextWorker.AbortAllWork();
                if (string.IsNullOrEmpty(FindTextInput))
                {
                    FindTextMatchCount = 0;
                    findTextMatches.Select(x => x.Item1).Distinct().ToList().ForEach(x => x.HighlightSections = new List<HighlightMatch>());
                }
                else
                {
                    findTextWorker.QueueWorkItem((DoWorkEventArgs args) =>
                    {
                        string findText = FindTextInput.ToLowerInvariant();
                        bool cancelled = false;
                        int matchCount = 0;
                        List<Tuple<Note, HighlightMatch>> matches = new List<Tuple<Note, HighlightMatch>>();
                        object findTextLockToken = new object();

                        //for efficiency... if we're starting a new search, search all notes... if we're typing and effectively filtering down a search, use the list of notes that matche
                        bool filterExistingResults = !string.IsNullOrEmpty(_lastFindTextInput) && (findText.StartsWith(_lastFindTextInput) || findText.EndsWith(_lastFindTextInput));
                        List<Note> notesToSearch = filterExistingResults ? findTextMatches.Select(x => x.Item1).Distinct().ToList() : new List<Note>(Notes);
                        //save /this/ search for next time the user changes it.
                        _lastFindTextInput = findText;

                        Parallel.ForEach(notesToSearch, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, (note, loopstate) =>
                        {
                            if (args.Cancel)
                            {
                                cancelled = true;
                                loopstate.Stop();
                            }

                            var noteMatches = note.BodyStripped.AllIndexesOf(findText);
                            lock (findTextLockToken)
                            {
                                matchCount += noteMatches.Count();
                                List<HighlightMatch> matchesForNote = new List<HighlightMatch>();
                                foreach (var noteMatch in noteMatches)
                                {
                                    HighlightMatch highlightMatch = new HighlightMatch(noteMatch, findText.Length);
                                    matches.Add(new Tuple<Note, HighlightMatch>(note, highlightMatch));
                                    matchesForNote.Add(highlightMatch);
                                 }

                                note.HighlightSections = matchesForNote;
                            }
                        });

                        matches = matches.OrderBy(x => x.Item1.StartTrackNumber)
                            .ThenBy(x => x.Item1.StartTime)
                            .ThenBy(x => x.Item2.MatchStartIndex).ToList();

                        if (false == cancelled)
                        {
                            FindTextMatchCount = matchCount;
                            lock(findTextMatchesLockToken) {
                                findTextMatches.Clear();
                                findTextMatches.AddRange(matches);
                            }

                            FindTextMatchIndex = matchCount > 0 ? 1 : 0;

                            //if there is at least one match, set the first as the current match.
                            if (matches.Count > 0)
                            {
                                matches[0].Item2.CurrentMatch = true;
                                FindTextMatchNote = matches[0].Item1; //set this so that the notes datagrid jumps to it
                            }
                        }
                    });
                }
            }
        }

        void _player_TrackFinished(object sender, EventArgs e)
        {
            int trackIndex = Tracks.IndexOf(_currentTrack);
            if (trackIndex < Tracks.Count - 1)
            {
                PlayFile(Tracks.ToList(), trackIndex + 1, TimeSpan.Zero, maintainPlayStatus:false);
            }
            else
            {
                Stop();
            }
        }

        void _notes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //if we're still loading, we don't care about any changes going on.
            if (_isLoading)
                return;

            if (null != e.NewItems)
            {
                foreach (Note note in e.NewItems)
                {
                    note.PropertyChanged += new PropertyChangedEventHandler(note_PropertyChanged);
                    note.ChangeCommitted += new Note.ObjectChangeCommittedEventHandler(note_ChangeCommitted);
                    if (false == note.IsDirty && false == string.IsNullOrWhiteSpace(note.Body))
                    {
                        _repo.SaveNote(_currentCourse, note);
                    }
                }
            }
            if (null != e.OldItems)
            {
                foreach (Note note in e.OldItems)
                {
                    ThreadHelper.ExecuteBackground(delegate
                    {
                        note.PropertyChanged -= this.note_PropertyChanged;
                        note.ChangeCommitted -= note_ChangeCommitted;

                        if (null != note.ID)
                        {
                            _repo.DeleteNote(note);
                        }
                    });
                }
            }
        }

        void note_ChangeCommitted(object sender, EventArgs e)
        {
            Note note = sender as Note;
            if (false == string.IsNullOrWhiteSpace(note.Body))
            {
                bool isNewNote = null == note.ID;

                _repo.SaveNote(_currentCourse, note);
            }
            else
            {
                Notes.Remove(note);
            }
        }

        void note_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }


        public override void HandleWindowKeypress(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var matches = SettingManager.CheckHotkey(e);

            if (null != matches && matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    switch (match.Function)
                    {
                        case HotkeyFunction.ToggleFullscreen:
                            Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen, TabChangeSource.Application), MessageType.PerformNavigation);
                            e.Handled = true;
                            break;
                        case HotkeyFunction.TogglePause:
                            _player.TogglePause();
                            e.Handled = true;
                            break;
                        case HotkeyFunction.Seek:
                            int seconds = match.SeekDirection == Direction.Forward ? match.SeekSeconds : match.SeekSeconds * -1;
                            _player.SeekRelative(seconds);
                            e.Handled = true;
                            break;
                        case HotkeyFunction.SpeedIncrease:
                            Speed += (double)match.SpeedModifierPercent / 100f;
                            _player.SetSpeed(Speed);
                            break;
                        case HotkeyFunction.SpeedDecrease:
                            Speed -= (double)match.SpeedModifierPercent / 100f;
                            _player.SetSpeed(Speed);
                            break;
                        case HotkeyFunction.SpeedReset:
                            Speed = 1f;
                            _player.SetSpeed(Speed);
                            break;
                    }
                }
            }
        }


        void _player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == MediaPlayerVLC.CurrentPlayPositionPropertyName)
            {
                CurrentTrackPlayPosition = _player.CurrentPlayPosition;
            }
            if (e.PropertyName == MediaPlayerVLC.PlayStatusPropertyName)
            {
                PlayStatus = _player.PlayStatus;
            }
            if (e.PropertyName == MediaPlayerVLC.CurrentTrackPropertyName)
            {
                _currentTrack = _player.CurrentTrack;

            }
        }

        public void SetCurrentCourse(Course course)
        {
            _isLoading = true;
            _currentCourse = course;
            CourseName = course.Name;
            Notes = new ExtendedObservableCollection<Note>();

            //flag missing track files
            course.Tracks.ForEach(x => x.CheckTrackMissing());

            Tracks.Clear();
            Tracks.AddRange(new ObservableCollection<Track>(course.Tracks));
            _lastEmbeddedVideoHeight = course.EmbeddedVideoHeight;
            _lastEmbeddedVideoWidth = course.EmbeddedVideoWidth;

            SetInitialTrack();
        }


        private bool _isLoading = false;
        private bool _isBusy = false;

        private void PlayFile(List<Track> tracks, int trackIndex, TimeSpan position, bool maintainPlayStatus)
        {
            Track track = tracks[trackIndex];
            if (null == track)
                return;

            if (null != _currentTrack)
            {
                _currentTrack.IsPlaying = false;
            }

            if (_currentTrack != track
                && true == SettingManager.ApplicationSettings.GenerateThumbnails)
            {
                ThumbnailGenerator.Generate(track);
            }

            _currentTrack = track;
            IsCurrentTrackVideo = track.IsVideo;
            CurrentTrackPlayPosition = position;
            CurrentTrackName = _currentTrack.StringDisplayValue;
            CurrentTrackVideoAspectRatio = _currentTrack.AspectRatio;
            CurrentTrackTotalLength = track.Length;


            //check if the track is missing. if so, skip to the next one.
            //JDW NOTE: this has to be done /after/ setting the track as the current track, so we know which track the 'next' track is.
            tracks[trackIndex].CheckTrackMissing();
            if (tracks[trackIndex].IsMissing)
            {
                //if the track is missing, play the next one... unless this is the last track, then stop. Otherwise if /all/ tracks are missing, it leads to an infinite loop.
                if (trackIndex < tracks.Count - 1) { 
                    NextTrackCommand.Execute(null);
                }
                else
                {
                    Stop();
                }
                return;
            }

            //notify the main window that we want to change title
            Messenger.Default.Send<SetWindowTitleMessage>(new SetWindowTitleMessage()
            {
                Mode = SetWindowTitleMode.ReplaceTitle,
                Text = _currentTrackName
            });


            track.IsPlaying = true;

            //actually play the file
            JayDev.MediaScribe.ViewModel.MediaPlayerVLC.PlayAction action = maintainPlayStatus ? JayDev.MediaScribe.ViewModel.MediaPlayerVLC.PlayAction.MaintainStatus : JayDev.MediaScribe.ViewModel.MediaPlayerVLC.PlayAction.Play;
            _player.PlayFile(tracks, trackIndex, position, action);

        }

        private void PlayPause()
        {
            //if we haven't selected a track, play the first one.
            if (null == _currentTrack)
            {
                if (null == Tracks || Tracks.Count == 0)
                {
                    //do nothing
                }
                else
                {
                    PlayFile(Tracks.ToList(), 0, TimeSpan.Zero, false);
                }
            }
            else
            {
                if (false == _currentTrack.IsMissing)
                {
                    _player.TogglePause();
                }
            }
        }

        private void SaveCourse()
        {
            if (_isBusy)
            {
                throw new Exception("Error: shouldn't be trying to save when busy");
            }
            _currentCourse.EmbeddedVideoHeight = LastEmbeddedVideoHeight;
            _currentCourse.EmbeddedVideoWidth = LastEmbeddedVideoWidth;
            _currentCourse.LastPlayedTrackID = null == _currentTrack ? null : _currentTrack.ID;
            _currentCourse.LastPlayedTrackPosition = _currentTrackPlayPosition;
            _currentCourse.DateViewed = DateTime.Now;
            _currentCourse.Notes = Notes.ToList();
            _repo.SaveCourse(_currentCourse);

            _controller.UpdateCourseInMemory(_currentCourse);
        }
        private void Stop()
        {
            if (null != _player)
            {
                _player.Stop();
            }
        }


        public override void LeavingViewModel()
        {
            //notify the main window that we want to change title
            Messenger.Default.Send<SetWindowTitleMessage>(new SetWindowTitleMessage()
            {
                Mode = SetWindowTitleMode.ResetToDefaultTitle
            });

            SaveCourse();
            Stop();
        }

        private void SetInitialTrack()
        {
            //auto-load the appropriate file
            if (null != _currentCourse.LastPlayedTrackID)
            {
                //JDW: track may have been removed
                int trackIndex = _currentCourse.Tracks.FindIndex(x => x.ID == _currentCourse.LastPlayedTrackID);

                //since we haven't found the track, there's clearly an issue. log it. set the initial track
                //to be the first... but if there are no tracks, simply exit out of there.
                if (trackIndex == -1)
                {
                    Logging.Log(LoggingSource.Errors, "SetInitialTrack - track not found??");

                    if (_currentCourse.Tracks.Count == 0)
                    {
                        return;
                    }

                    trackIndex = 0;
                }

                PlayFile(_currentCourse.Tracks, trackIndex, _currentCourse.LastPlayedTrackPosition, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moveNext">true if we move next, false if we move back</param>
        private void HandleFindTextMatchNextAndPrevious(bool moveNext)
        {

            var currentMatch = findTextMatches[FindTextMatchIndex - 1]; //we subtract 1 because the index is 1-based.
            //set the current matche's "current" field to false. 
            currentMatch.Item2.CurrentMatch = false;
            //let the note know we've updated the match status
            currentMatch.Item1.HighlightSections = new List<HighlightMatch>(currentMatch.Item1.HighlightSections);

            //move to the next index
            if (moveNext)
            {
                FindTextMatchIndex = FindTextMatchIndex < FindTextMatchCount ? FindTextMatchIndex + 1 : 1;
            }
            else
            {
                FindTextMatchIndex = FindTextMatchIndex > 1 ? FindTextMatchIndex - 1 : FindTextMatchCount;
            }

            currentMatch = findTextMatches[FindTextMatchIndex - 1]; //we subtract 1 because the index is 1-based.
            //set the current matche's "current" field to false. 
            currentMatch.Item2.CurrentMatch = true;
            //let the note know we've updated the match status
            currentMatch.Item1.HighlightSections = new List<HighlightMatch>(currentMatch.Item1.HighlightSections);

            //set the DP for the note, so that the notes grid can scroll to it.
            FindTextMatchNote = currentMatch.Item1;
        }
    }
}
