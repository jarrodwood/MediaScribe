using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.MediaScribe.Model;
using GalaSoft.MvvmLight.Command;
using JayDev.MediaScribe.Common;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using LibMPlayerCommon;
using System.Windows.Threading;
using JayDev.MediaScribe.Core;
using System.IO;
using JayDev.MediaScribe.View;
using Microsoft.Practices.Unity;

namespace JayDev.MediaScribe.ViewModel
{
    public class CourseListViewModel : ViewModelBase
    {
        #region Private Members

        private CourseRepository _repo;
        private Dispatcher _uiDispatcher;

        #endregion

        #region Commands

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
                                              //JDW: if we're navigating from the list view, we have no context course.
                                              Messenger.Default.Send(new NavigateArgs(message, TabChangeSource.Application), MessageType.PerformNavigation);
                                          }));
            }
        }

        #endregion

        #region CreateCourseCommand

        private RelayCommand _createCourseCommand;

        /// <summary>
        /// Gets the CreateCourseCommand.
        /// </summary>
        public RelayCommand CreateCourseCommand
        {
            get
            {
                return _createCourseCommand
                    ?? (_createCourseCommand = new RelayCommand(
                                          () =>
                                          {
                                              SelectedCourse = null;
                                              //Prepare bindable data for a new course
                                              SelectedCourseName = null;
                                              SelectedCourseTracks = new ObservableCollection<Track>();
                                              MaintenanceMode = MaintenanceMode.Create;
                                          },
                                          () => true));
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
                                              //update the OrderNumbers on the tracks before we save them
                                              for (int i = 0; i < SelectedCourseTracks.Count; i++)
                                              {
                                                  //JDW NOTE: set i+1 since it's user-visible, and people like lists starting at 1.
                                                  SelectedCourseTracks[i].TrackNumber = i + 1;
                                              }

                                              Course courseToSave;
                                              if (null != SelectedCourse && SelectedCourse.ID != null)
                                              {
                                                  courseToSave = _repo.GetCourse(SelectedCourse.ID.Value);
                                              }
                                              else
                                              {
                                                  courseToSave = new Course()
                                                  {
                                                      DateCreated = DateTime.Now,
                                                      DateViewed = DateTime.Now
                                                  };
                                              }
                                              courseToSave.Name = SelectedCourseName;
                                              courseToSave.Tracks.Clear();
                                              SelectedCourseTracks.ToList().ForEach(x => courseToSave.Tracks.Add(x));

                                              _repo.SaveCourseAndTracks(courseToSave);
                                              //if (false == result.IsSaveSuccessful)
                                              //{
                                              //    Messenger.Default.Send<string>(result.ToString(), "errors");
                                              //}
                                              //else
                                              //{
                                                  Courses = new ObservableCollection<Course>(_repo.GetCourseList());
                                                  //ensure that the row can be selected in the datagrid
                                                  SelectedCourse = Courses.First(x => x.ID == courseToSave.ID);
                                              //}

                                              MaintenanceMode = Common.MaintenanceMode.View;

                                              Courses = new ObservableCollection<Course>(_repo.GetCourseList());
                                              SelectedCourse = Courses.First(x => x.ID == courseToSave.ID);
                                              SelectedCourseTracks = new ObservableCollection<Track>(SelectedCourse.Tracks);


                                              _controller.UpdateCourseInMemory(SelectedCourse);
                                          },
                                          () => //can save if there is a name, and tracks.
                                              false == string.IsNullOrWhiteSpace(SelectedCourseName)
                                              && null != SelectedCourseTracks
                                              && SelectedCourseTracks.Count > 0));
            }
        }

        #endregion

        #region MaintenanceModeEditCommand

        private RelayCommand _maintenanceModeEditCommand;

        /// <summary>
        /// Gets the MaintenanceModeEditCommand.
        /// </summary>
        public RelayCommand MaintenanceModeEditCommand
        {
            get
            {
                return _maintenanceModeEditCommand
                    ?? (_maintenanceModeEditCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (SelectedCourse.Notes.Count > 0)
                                              {
                                                  var openResult = System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Are you sure you want to edit the tracks? Unless you're adding them on the end, this may mess up your note locations.", "Edit course track list confirmation", System.Windows.MessageBoxButton.YesNo);
                                                  if (openResult == System.Windows.MessageBoxResult.No)
                                                  {
                                                      return;
                                                  }
                                              }
                                              MaintenanceMode = Common.MaintenanceMode.Edit;
                                              SaveCourseCommand.RaiseCanExecuteChanged();
                                          }));
            }
        }

        #endregion

        #region DeleteCourseCommand

        private RelayCommand _deleteCourseCommand;

        /// <summary>
        /// Gets the CreateCourseCommand.
        /// </summary>
        public RelayCommand DeleteCourseCommand
        {
            get
            {
                return _deleteCourseCommand
                    ?? (_deleteCourseCommand = new RelayCommand(
                                          () =>
                                          {
                                              var openResult = System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Are you sure you want to delete the selected course?", "Delete course confirmation", System.Windows.MessageBoxButton.YesNo);
                                              if (openResult == System.Windows.MessageBoxResult.Yes)
                                              {
                                                  _controller.RemoveCourseFromMemory(SelectedCourse);
                                                  _repo.DeleteCourse(SelectedCourse);
                                                  Courses = new ObservableCollection<Course>(_repo.GetCourseList());
                                              }
                                          },
                                          () => AreCoursesExisting && SelectedCourse != null));
            }
        }

        #endregion

        #region AddTracksCommand

        private RelayCommand _addTracksCommand;

        /// <summary>
        /// Gets the AddTracksCommand.
        /// </summary>
        public RelayCommand AddTracksCommand
        {
            get
            {
                return _addTracksCommand
                    ?? (_addTracksCommand = new RelayCommand(
                                          () =>
                                          {
                                              //TODO: refactor so we don't use dialogs in viewmodels
                                              Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                                              dlg.Multiselect = true;
                                              StringBuilder filterBuilder = new StringBuilder();
                                              filterBuilder.Append("Audio and Video Files|*.3GP;*.ASF;*.AVI;*.FLV;*.FLA;*.M4V;*.MKV;*.MOV;*.MP4;*.MPEG;*.MPG;*.OGV;*.RM;*.WMV;*.WAV;*.FLAC;*.M4A;*.WMA;*.MP2;*.MP3;*.WMA;*.AAC;*.M4A;*.RA;*.RM;*.SWA");
                                              filterBuilder.Append("|Video Files|*.3GP;*.ASF;*.AVI;*.FLV;*.FLA;*.M4V;*.MKV;*.MOV;*.MP4;*.MPEG;*.MPG;*.OGV;*.RM;*.WMV");
                                              filterBuilder.Append("|Audio Files|*.WAV;*.FLAC;*.M4A;*.WMA;*.MP2;*.MP3;*.WMA;*.AAC;*.M4A;*.RA;*.RM;*.SWA");
                                              filterBuilder.Append("|All Files|*.*");
                                              dlg.Filter = filterBuilder.ToString(); // Filter files by extension    

                                              // Show open file dialog box 
                                              Nullable<bool> result = dlg.ShowDialog();

                                              if (result == true)
                                              {
                                                  ThreadHelper.ExecuteBackground(delegate
                                                  {
                                                      foreach (String file in dlg.FileNames)
                                                      {
                                                          FileInfo fi = new FileInfo(file);
                                                          Discover discoverer = new Discover(file);
                                                          Track track = new Track()
                                                          {
                                                              FilePath = file,
                                                              Title = discoverer.Title,
                                                              Length = new TimeSpan(0, 0, discoverer.Length),
                                                              IsVideo = discoverer.Video,
                                                              FileSize = fi.Length,
                                                              AspectRatio = discoverer.Video ? (float?)discoverer.AspectRatio : null
                                                          };
                                                          ThreadHelper.ExecuteAsyncUI(_uiDispatcher, delegate
                                                          {
                                                              SelectedCourseTracks.Add(track);
                                                              //notify that we might be able to save now.
                                                              SaveCourseCommand.RaiseCanExecuteChanged();
                                                          });
                                                      }
                                                  });
                                              }
                                          }));
            }
        }

        #endregion

        #region MoveItemsCommand

        private RelayCommand<MoveItemsCommandParameter> _moveItemsCommand;
        public RelayCommand<MoveItemsCommandParameter> MoveItemsCommand
        {
            get
            {
                return _moveItemsCommand
                    ?? (_moveItemsCommand = new RelayCommand<MoveItemsCommandParameter>(
                                          (MoveItemsCommandParameter args) =>
                                          {
                                              //remove the dragged items from the list
                                              args.ObjectsToInsert.ForEach(x => SelectedCourseTracks.Remove((Track)x));

                                              //insert dragged elements at hovered-over-rows location. we'll reverse the collection, so the inserts go in in the right order.
                                              args.ObjectsToInsert.Reverse();
                                              args.ObjectsToInsert.ForEach(x => SelectedCourseTracks.Insert(args.InsertToIndex, (Track)x));
                                          },
                                          (MoveItemsCommandParameter args) => true));
            }
        }

        #endregion

        #region DeleteTracksCommand

        private RelayCommand _deleteTracksCommand;

        /// <summary>
        /// Gets the DeleteTracksCommand.
        /// </summary>
        public RelayCommand DeleteTracksCommand
        {
            get
            {
                return _deleteTracksCommand
                    ?? (_deleteTracksCommand = new RelayCommand(
                                          () =>
                                          {
                                              foreach (Track track in _selectedCourseSelectedTracks)
                                              {
                                                  SelectedCourseTracks.Remove(track);
                                              }
                                          },
                                          () =>
                                          {
                                              return _selectedCourseSelectedTracks != null && _selectedCourseSelectedTracks.Count() > 0;
                                          }
                                          ));
            }
        }

        #endregion

        #region WriteNotesCommand

        private RelayCommand _writeNotesCommand;

        /// <summary>
        /// Gets the WriteNotesCommand.
        /// </summary>
        public RelayCommand WriteNotesCommand
        {
            get
            {
                return _writeNotesCommand
                    ?? (_writeNotesCommand = new RelayCommand(
                                          () =>
                                          {
                                              //NOTE: double-clicking on the headers for the grid also triggers this event, so check to
                                              //ensure that an item is selected first.
                                              if (null != SelectedCourse)
                                              {
                                                  Messenger.Default.Send(new NavigateArgs(NavigateMessage.WriteCourseNotes, SelectedCourse, TabChangeSource.Application), MessageType.PerformNavigation);
                                              }
                                          }));
            }
        }

        #endregion

        #region CancelCommand

        private RelayCommand _cancelCommand;

        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                                          () =>
                                          {
                                              //if (SelectedCourse == null)
                                              //{
                                              SelectedCourse = null;
                                                  MaintenanceMode = Common.MaintenanceMode.None;
                                              //}
                                              //else
                                              //{
                                              //    MaintenanceMode = Common.MaintenanceMode.View;
                                              //}
                                          }));
            }
        }

        #endregion

        #region UpdateCourseListCommand

        private RelayCommand _updateCourseListCommand;

        /// <summary>
        /// Gets the UpdateCourseListCommand.
        /// </summary>
        public RelayCommand UpdateCourseListCommand
        {
            get
            {
                return _updateCourseListCommand
                    ?? (_updateCourseListCommand = new RelayCommand(
                                          () =>
                                          {
                                              Courses = new ObservableCollection<Course>(_repo.GetCourseList());
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
                                              ExportCourseToExcel(SelectedCourse);
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
                                              saveFileDialog1.FileName = string.Format("Exported Notes for {0} - {1}.xlsx", SelectedCourse.Name, DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss"));
                                              saveFileDialog1.Title = "Save Exported Notes";

                                              if (saveFileDialog1.ShowDialog() == true)
                                              {
                                                  try
                                                  {
                                                      CsvExporter exporter = new CsvExporter();
                                                      string csvContents = exporter.CreateCsvText(SelectedCourse.Tracks.ToList(), SelectedCourse.Notes.ToList());
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

        #endregion

        #region Notified Properties

        #region Courses

        /// <summary>
        /// The <see cref="Courses" /> property's name.
        /// </summary>
        public const string CoursesPropertyName = "Courses";

        private ObservableCollection<Course> _courses = null;

        /// <summary>
        /// Sets and gets the Courses property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Course> Courses
        {
            get
            {
                return _courses;
            }

            set
            {
                if (_courses == value)
                {
                    return;
                }

                if (null != _courses)
                {
                    _courses.CollectionChanged -= _courses_CollectionChanged;
                }

                _courses = value;
                RaisePropertyChanged(CoursesPropertyName);

                if (null != _courses)
                {
                    _courses.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_courses_CollectionChanged);
                }
            }
        }

        #endregion

        #region AreCoursesExisting

        public const string AreCoursesExistingPropertyName = "AreCoursesExisting";
        public bool AreCoursesExisting { get { return Courses.Count > 0; } }

        #endregion

        #region AreTracksExistingInSelectedCourse

        /// <summary>
        /// The <see cref="AreTracksExistingInSelectedCourse" /> property's name.
        /// </summary>
        public const string AreTracksExistingInSelectedCoursePropertyName = "AreTracksExistingInSelectedCourse";

        /// <summary>
        /// Sets and gets the AreTracksExistingInSelectedCourse property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool AreTracksExistingInSelectedCourse
        {
            get
            {
                return null != SelectedCourseTracks && SelectedCourseTracks.Count > 0;
            }
        }

        #endregion

        #region SelectedCourse

        /// <summary>
        /// The <see cref="SelectedCourse" /> property's name.
        /// </summary>
        public const string SelectedCoursePropertyName = "SelectedCourse";

        private Course _selectedCourse = null;

        /// <summary>
        /// Sets and gets the SelectedCourse property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Course SelectedCourse
        {
            get
            {
                return _selectedCourse;
            }

            set
            {
                if (_selectedCourse == value)
                {
                    return;
                }


                _selectedCourse = value;
                RaisePropertyChanged(SelectedCoursePropertyName);

                if (null == SelectedCourse)
                {
                    SelectedCourseName = null;
                    SelectedCourseTracks = null;
                    MaintenanceMode = Common.MaintenanceMode.None;
                }
                else
                {
                    //check whether the tracks exist
                    _selectedCourse.Tracks.ForEach(x => x.CheckTrackMissing());

                    SelectedCourseName = _selectedCourse.Name;
                    SelectedCourseTracks = new ObservableCollection<Track>(_selectedCourse.Tracks);
                    //when we've selected a course, we may be able to view it
                    MaintenanceMode = Common.MaintenanceMode.View;
                }

                //flag that we either may or may not be able to trigger the delete course functionality now
                DeleteCourseCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region MaintenanceMode

        /// <summary>
        /// The <see cref="MaintenanceMode" /> property's name.
        /// </summary>
        public const string MaintenanceModePropertyName = "MaintenanceMode";

        private MaintenanceMode _maintenanceMode = MaintenanceMode.None;

        /// <summary>
        /// Sets and gets the MaintenanceMode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MaintenanceMode MaintenanceMode
        {
            get
            {
                return _maintenanceMode;
            }

            set
            {
                if (_maintenanceMode == value)
                {
                    return;
                }

                _maintenanceMode = value;
                RaisePropertyChanged(MaintenanceModePropertyName);
            }
        }

        #endregion MaintenanceMode

        #region SelectedCourseName

        /// <summary>
        /// The <see cref="SelectedCourseName" /> property's name.
        /// </summary>
        public const string SelectedCourseNamePropertyName = "SelectedCourseName";

        private string _selectedCourseName = null;

        /// <summary>
        /// Sets and gets the SelectedCourseName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedCourseName
        {
            get
            {
                return _selectedCourseName;
            }

            set
            {
                if (_selectedCourseName == value)
                {
                    return;
                }

                _selectedCourseName = value;
                RaisePropertyChanged(SelectedCourseNamePropertyName);

                //when the name changes, we may be able to save the course
                if (MaintenanceMode == Common.MaintenanceMode.Edit || MaintenanceMode == Common.MaintenanceMode.Create)
                {
                    SaveCourseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region SelectedCourseTracks

        /// <summary>
        /// The <see cref="SelectedCourseTracks" /> property's name.
        /// </summary>
        public const string SelectedCourseTracksPropertyName = "SelectedCourseTracks";

        private ObservableCollection<Track> _selectedCourseTracks = new ObservableCollection<Track>();

        /// <summary>
        /// Sets and gets the SelectedCourseTracks property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Track> SelectedCourseTracks
        {
            get
            {
                return _selectedCourseTracks;
            }

            set
            {
                if (_selectedCourseTracks == value)
                {
                    return;
                }

                if (null != _selectedCourseTracks)
                {
                    _selectedCourseTracks.CollectionChanged -= _selectedCourseTracks_CollectionChanged;
                }

                _selectedCourseTracks = value;
                RaisePropertyChanged(SelectedCourseTracksPropertyName);
                RaisePropertyChanged(AreTracksExistingInSelectedCoursePropertyName);
                RaisePropertyChanged(AreCoursesExistingPropertyName);

                if (null == _selectedCourseTracks)
                {
                }
                else
                {
                    _selectedCourseTracks.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_selectedCourseTracks_CollectionChanged);
                }
            }
        }

        #endregion

        #region SelectedCourseSelectedTracks

        /// <summary>
        /// The <see cref="SelectedCourseSelectedTracks" /> property's name.
        /// </summary>
        public const string SelectedCourseSelectedTracksPropertyName = "SelectedCourseSelectedTracks";

        private List<Track> _selectedCourseSelectedTracks = null;

        /// <summary>
        /// Sets and gets the SelectedCourseSelectedTracks property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<object> SelectedCourseSelectedTracks
        {
            //get
            //{
            //    return _selectedCourseSelectedTracks.Cast<object>().ToList();
            //}

            set
            {
                if (null == value)
                {
                    _selectedCourseSelectedTracks = null;
                }
                else
                {
                    _selectedCourseSelectedTracks = value.Cast<Track>().ToList();
                }
                RaisePropertyChanged(SelectedCourseSelectedTracksPropertyName);
                //since the selected tracks have changed, we may be able to delete tracks..
                DeleteTracksCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #endregion

        #region Constructor

        public CourseListViewModel(CourseRepository repo, UnityContainer unityContainer)
            : base(unityContainer)
        {
            _repo = repo;
            _uiDispatcher = Dispatcher.CurrentDispatcher;

            LoadCoursesBackground();
        }

        #endregion

        #region Event Handlers

        void _courses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(AreCoursesExistingPropertyName);
        }



        void _selectedCourseTracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(AreTracksExistingInSelectedCoursePropertyName);
        }

        #endregion

        public override void EnteringViewModel()
        {
            LoadCoursesBackground();
        }

        private void LoadCoursesBackground()
        {
            List<Course> courseList = _controller.AllCourses;
            Courses = new ObservableCollection<Course>(courseList);
        }
    }
}
