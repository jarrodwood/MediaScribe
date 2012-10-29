using System;
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

namespace JayDev.MediaScribe.ViewModel
{
    public class CourseUseViewModel : ViewModelBase
    {
        #region Private Local Variables, Constants

        private Course _currentCourse;
        private Track _currentTrack;
        private CourseRepository _repo;
        private MediaPlayer _player;
        private Dispatcher _uiDispatcher;

        private const bool IsKeepingBlankRowForEdit = true;

        #endregion 

        //TODO: MOVE THIS OUT OF VIEWMODEL!
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        #region Public Properties & Backing Fields

        #region CurrentPage

        public NavigateMessage CurrentPage
        {
            get
            {
                return NavigateMessage.WriteCourseNotes;
            }
        }

        #endregion

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

                    //hook up event handlers for the notes, so we can save any changes to them
                    foreach (Note note in _notes)
                    {
                        note.PropertyChanged += new PropertyChangedEventHandler(note_PropertyChanged);
                        note.ChangeCommitted += new Note.ObjectChangeCommittedEventHandler(note_ChangeCommitted);
                    }
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
                                                  context.Start = new TrackTime()
                                                  {
                                                      Time = CurrentTrackPlayPosition,
                                                      Track = _currentTrack,
                                                      ParentCourse = _currentCourse
                                                  };
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
                                              context.Start = new TrackTime()
                                              {
                                                  Track = _currentTrack,
                                                  Time = CurrentTrackPlayPosition,
                                                  ParentCourse = _currentCourse
                                              };

                                              ////NOTE: if we already have a 'start' tracktime, don't replace it - update it. this is to save
                                              ////us NHibernate pain
                                              //if (null == context.Start)
                                              //{
                                              //    context.Start = new TrackTime()
                                              //    {
                                              //        Track = _currentTrack,
                                              //        Time = CurrentTrackPlayPosition,
                                              //        ParentCourse = _currentCourse
                                              //    };
                                              //}
                                              //else
                                              //{
                                              //    context.Start.Track = _currentTrack;
                                              //    context.Start.Time = CurrentTrackPlayPosition;
                                              //    context.Start.ParentCourse = _currentCourse;
                                              //}
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
                                              context.End = new TrackTime()
                                              {
                                                  Track = _currentTrack,
                                                  Time = CurrentTrackPlayPosition,
                                                  ParentCourse = _currentCourse
                                              };
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
                                              if (null != context && null != context.Start)
                                              {
                                                  int trackIndex = Tracks.IndexOf(context.Start.Track);
                                                  PlayFile(Tracks.ToList(), trackIndex, context.Start.Time, false);
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
                                              Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen), MessageType.Navigate);
                                          },
                                          () => true
                    //{
                    //    return CurrentTrackName != null && PlayStatus != Common.PlayStatus.Playing;
                    //}
                                          ));
            }
        }

        #endregion

        #region NavigateCommand

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
                                _isBusy = true;
                                Notes = new ObservableCollection<Note>(_currentCourse.Notes);
                                _isBusy = false;
                                _isLoading = false;
                            }));
            }
        }

        #endregion

        #region ExportExcelCommand

        private RelayCommand _exportExcelCommand;

        /// <summary>
        /// Gets the ExportExcelCommand.
        /// </summary>
        public RelayCommand ExportExcelCommand
        {
            get
            {
                return _exportExcelCommand
                    ?? (_exportExcelCommand = new RelayCommand(
                                          () =>
                                          {
                                              //TODO: refactor so we don't use dialogs in viewmodels
                                              Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();
                                              saveFileDialog1.OverwritePrompt = true;
                                              saveFileDialog1.RestoreDirectory = true;
                                              saveFileDialog1.DefaultExt = "xslx";
                                              // Adds a extension if the user does not
                                              saveFileDialog1.AddExtension = true;
                                              saveFileDialog1.InitialDirectory = Convert.ToString(Environment.SpecialFolder.MyDocuments);
                                              saveFileDialog1.Filter = "Excel Spreadsheet|*.xlsx";
                                              saveFileDialog1.FileName = string.Format("Exported Notes for {0} - {1}.xlsx", _currentCourse.Name, DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss"));
                                              saveFileDialog1.Title = "Save Exported Notes";

                                              if (saveFileDialog1.ShowDialog() == true)
                                              {
                                                  try
                                                  {
                                                      using (System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile())
                                                      {
                                                          XslsExporter exporter = new XslsExporter();
                                                          exporter.CreateSpreadsheet(fs, Tracks.ToList(), Notes.ToList());

                                                          fs.Close();
                                                      }

                                                      var openResult = System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Export successful! Would you like to open the file?", "Open exported file confirmation", System.Windows.MessageBoxButton.YesNo);
                                                      if(openResult == System.Windows.MessageBoxResult.Yes) {
                                                          System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                                                          info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                                                          info.FileName = saveFileDialog1.FileName;
                                                          var process = System.Diagnostics.Process.Start(info);
                                                      }
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Error exporting :( - " + e.ToString());
                                                  }
                                              }
                                          }));
            }
        }

        #endregion

        #region ExportCsvCommand

        private RelayCommand _exportCsvCommand;

        /// <summary>
        /// Gets the ExportCsvCommand.
        /// </summary>
        public RelayCommand ExportCsvCommand
        {
            get
            {
                return _exportCsvCommand
                    ?? (_exportCsvCommand = new RelayCommand(
                                          () =>
                                          {
                                              //TODO: refactor so we don't use dialogs in viewmodels
                                              Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();
                                              saveFileDialog1.OverwritePrompt = true;
                                              saveFileDialog1.RestoreDirectory = true;
                                              saveFileDialog1.DefaultExt = "csv";
                                              // Adds a extension if the user does not
                                              saveFileDialog1.AddExtension = true;
                                              saveFileDialog1.InitialDirectory = Convert.ToString(Environment.SpecialFolder.MyDocuments);
                                              saveFileDialog1.Filter = "Csv File|*.csv";
                                              saveFileDialog1.FileName = string.Format("Exported Notes for {0} - {1}.xlsx", _currentCourse.Name, DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss"));
                                              saveFileDialog1.Title = "Save Exported Notes";

                                              if (saveFileDialog1.ShowDialog() == true)
                                              {
                                                  try
                                                  {
                                                      CsvExporter exporter = new CsvExporter();
                                                      string csvContents = exporter.CreateCsvText(Tracks.ToList(), Notes.ToList());
                                                      System.IO.File.WriteAllText(saveFileDialog1.FileName, csvContents);

                                                      System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Export successful!", "Export successful", System.Windows.MessageBoxButton.OK);
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Error exporting :( - " + e.ToString());
                                                  }
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

        #endregion

        #region Constructor

        public CourseUseViewModel(CourseRepository repo) : base()
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
            if (e.PropertyName == VideoPanelPointerPropertyName
                && (int)VideoPanelPointer != 0)
            {
                _player = new MediaPlayer(VideoPanelPointer);
                _player.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_player_PropertyChanged);
                //the volume will have been given an initial value before this event is raised
                _player.Volume(_volume);
                _player.TrackFinished += new MediaPlayer.TrackFinishedEventHandler(_player_TrackFinished);

                //auto-load the last track
                SetInitialTrack();
            }
        }

        void _player_TrackFinished(object sender, EventArgs e)
        {
            int trackIndex = Tracks.IndexOf(_currentTrack);
            if (trackIndex < Tracks.Count - 1)
            {
                PlayFile(Tracks.ToList(), trackIndex + 1, TimeSpan.Zero, true);
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
            var matches = HotkeyManager.CheckHotkey(e);

            if (null != matches && matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    switch (match.Function)
                    {
                        case HotkeyFunction.ToggleFullscreen:
                            Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen), MessageType.Navigate);
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
                    }
                }
            }
        }


        void _player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == MediaPlayer.CurrentPlayPositionPropertyName)
            {
                CurrentTrackPlayPosition = _player.CurrentPlayPosition;
            }
            if (e.PropertyName == MediaPlayer.PlayStatusPropertyName)
            {
                PlayStatus = _player.PlayStatus;
            }
            if (e.PropertyName == MediaPlayer.CurrentTrackPropertyName)
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
            if (null == Tracks)
            {
                _tracks = new ObservableCollection<Track>();
            }
            else
            {
                Tracks.Clear();
            }
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

            _currentTrack = track;
            IsCurrentTrackVideo = track.IsVideo;
            CurrentTrackPlayPosition = position;
            CurrentTrackName = _currentTrack.StringDisplayValue;
            //Propert
            CurrentTrackTotalLength = track.Length;

            track.IsPlaying = true;

            JayDev.MediaScribe.ViewModel.MediaPlayer.PlayAction action = maintainPlayStatus ? JayDev.MediaScribe.ViewModel.MediaPlayer.PlayAction.MaintainStatus : JayDev.MediaScribe.ViewModel.MediaPlayer.PlayAction.Play;
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
                _player.TogglePause();
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
            _currentCourse.LastPlayedTrack = _currentTrack;
            _currentCourse.LastPlayedTrackPosition = _currentTrackPlayPosition;
            _currentCourse.DateViewed = DateTime.Now;
            _repo.SaveCourseOnly(_currentCourse);

            _controller.RefreshCourse(_currentCourse);
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
            SaveCourse();
            Stop();
        }

        private void SetInitialTrack()
        {
            //NOTE: can only instruct mplayer to load a file, AFTER we've initialized the panel it will display to.
            if (MediaPlayerWPFDisplayControl.Instance.IsVideoPanelInitialized)
            {
                //auto-load the appropriate file
                if (null != _currentCourse.LastPlayedTrack)
                {
                    //JDW: track may have been removed
                    if (_currentCourse.Tracks.Any(x => x.FilePath == _currentCourse.LastPlayedTrack.FilePath))
                    {
                        int trackIndex = _currentCourse.Tracks.IndexOf(_currentCourse.LastPlayedTrack);
                        PlayFile(_currentCourse.Tracks, trackIndex, _currentCourse.LastPlayedTrackPosition, true);
                    }
                }
            }
        }
    }
}
