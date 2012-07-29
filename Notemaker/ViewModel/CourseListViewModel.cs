using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.Notemaker.Model;
using GalaSoft.MvvmLight.Command;
using JayDev.Notemaker.Common;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using LibMPlayerCommon;
using System.Windows.Threading;

namespace JayDev.Notemaker.ViewModel
{
    public class CourseListViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Private Members

        private CourseRepository _repo;
        private Dispatcher _uiDispatcher;
        private Guid? _selectedCourseID;

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
                                              Messenger.Default.Send(new NavigateArgs(message), MessageType.Navigate);
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
                                              //Prepare bindable data for a new course
                                              SelectedCourseName = null;
                                              _selectedCourseID = null;
                                              SelectedCourseTracks = new ObservableCollection<Track>();
                                              MaintenanceMode = MaintenanceMode.Create;
                                          },
                                          () => true));
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
                                              Course courseToSave = new Course() {
                                                  Name = SelectedCourseName,
                                                  Tracks = new List<Track>(SelectedCourseTracks)
                                              };
                                              List<Course> list = _repo.GetCourseList();

                                              Course matchingCourse = list.FirstOrDefault(x => x.ID == _selectedCourseID);
                                              if (null != matchingCourse)
                                              {
                                                  int index = list.IndexOf(matchingCourse);
                                                  list[index] = courseToSave;
                                              }
                                              else
                                              {
                                                  list.Add(courseToSave);
                                              }

                                              SaveResult result = _repo.SaveCourseList(list);
                                              if (false == result.IsSaveSuccessful)
                                              {
                                                  Messenger.Default.Send<string>(result.ToString(), "errors");
                                              }
                                              else
                                              {
                                                  Courses = new ObservableCollection<Course>(_repo.GetCourseList());
                                                  //ensure that the row can be selected in the datagrid
                                                  SelectedCourse = Courses.First(x => x.Name == SelectedCourseName);
                                              }

                                              MaintenanceMode = Common.MaintenanceMode.View;
                                          },
                                          () => //can save if there is a name, and tracks.
                                              false == string.IsNullOrWhiteSpace(SelectedCourseName)
                                              && SelectedCourseTracks.Count > 0));
            }
        }

        #region DeleteCourseCommand

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
                                              MaintenanceMode = Common.MaintenanceMode.Edit;
                                          }));
            }
        }

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

                                          },
                                          () => AreCoursesExisting));
            }
        }

        #endregion

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
                                              filterBuilder.Append("Audio and Video Files|*.3GP;*.ASF;*.AVI;*.FLV;*.FLA;*.M4V;*.MKV;*.MOV;*.MPEG;*.MPG;*.OGV;*.RM;*.WMV;*.WAV;*.FLAC;*.M4A;*.WMA;*.MP2;*.MP3;*.WMA;*.AAC;*.M4A;*.RA;*.RM;*.SWA");
                                              filterBuilder.Append("|Video Files|*.3GP;*.ASF;*.AVI;*.FLV;*.FLA;*.M4V;*.MKV;*.MOV;*.MPEG;*.MPG;*.OGV;*.RM;*.WMV");
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
                                                          Discover discoverer = new Discover(file);
                                                          Track track = new Track()
                                                          {
                                                              FilePath = file,
                                                              Title = discoverer.Title,
                                                              Length = new TimeSpan(0, 0, discoverer.Length)
                                                          };
                                                          ThreadHelper.ExecuteAsyncUI(_uiDispatcher, delegate
                                                          {
                                                              SelectedCourseTracks.Add(track);
                                                          });
                                                      }

                                                      //at the end of executing the background thread, notify that we might be able to save now.
                                                      ThreadHelper.ExecuteSyncUI(_uiDispatcher, delegate
                                                      {
                                                          SaveCourseCommand.RaiseCanExecuteChanged();
                                                      });
                                                  });
                                              }
                                          }));
            }
        }


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
                                          () => {
                                              return _selectedCourseSelectedTracks != null && _selectedCourseSelectedTracks.Count() > 0;
                                          }
                                          ));
            }
        }

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
                                              Messenger.Default.Send(new NavigateArgs(NavigateMessage.WriteCourseNotes, SelectedCourse), MessageType.Navigate);
                                          }));
            }
        }

        #region Notified Properties

        private ObservableCollection<Course> _courses = new ObservableCollection<Course>();
        public ObservableCollection<Course> Courses { get { return _courses; } set { _courses = value; } }

        public const string AreCoursesExistingPropertyName = "AreCoursesExisting";
        public bool AreCoursesExisting { get { return Courses.Count > 0; } }


        public const string AreTracksExistingInSelectedCoursePropertyName = "AreTracksExistingInSelectedCourse";
        public bool AreTracksExistingInSelectedCourse { get { return null != SelectedCourseTracks && SelectedCourseTracks.Count > 0; } }

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

                SelectedCourseName = _selectedCourse.Name;
                SelectedCourseTracks = new ObservableCollection<Track>(_selectedCourse.Tracks);
                //when we've selected a course, we may be able to view it
                MaintenanceMode = Common.MaintenanceMode.View;
            }
        }

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
                if (MaintenanceMode == Common.MaintenanceMode.Edit)
                {
                    SaveCourseCommand.RaiseCanExecuteChanged();
                }
            }
        }


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

                _selectedCourseTracks = value;
                RaisePropertyChanged(SelectedCourseTracksPropertyName);
                _selectedCourseTracks.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_selectedCourseTracks_CollectionChanged);
            }
        }


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
                    _selectedCourseTracks = null;
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


        public CourseListViewModel(CourseRepository repo)
        {
            _repo = repo;
            _uiDispatcher = Dispatcher.CurrentDispatcher;

            _courses.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_courses_CollectionChanged);

            List<Course> courseList = _repo.GetCourseList();
            Courses = new ObservableCollection<Course>(courseList);
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
            //Courses.Add(new Course() { Name = "Test course 1" });
        }

        void _courses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(AreCoursesExistingPropertyName);
        }



        void _selectedCourseTracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(AreTracksExistingInSelectedCoursePropertyName);
        }
    }
}
