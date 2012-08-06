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
using System.Windows.Input;
using System.Windows.Media;
using JayDev.Notemaker.View;
using JayDev.Notemaker.Core;

namespace JayDev.Notemaker.ViewModel
{
    public class SettingsViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Private Members

        private SettingRepository _repo;
        private Dispatcher _uiDispatcher;

        #endregion

        #region Commands

        private RelayCommand _restoreDefaultHotkeysCommand;

        /// <summary>
        /// Gets the RestoreDefaultHotkeysCommand.
        /// </summary>
        public RelayCommand RestoreDefaultHotkeysCommand
        {
            get
            {
                return _restoreDefaultHotkeysCommand
                    ?? (_restoreDefaultHotkeysCommand = new RelayCommand(
                                          () =>
                                          {
                                              //TODO: no messageboxes in viewmodel!
                                              var result = System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "Are you sure you want to replace all hotkeys with the default set?", "Replace hotkey confirmation", System.Windows.MessageBoxButton.YesNo);
                                              if (result == System.Windows.MessageBoxResult.Yes)
                                              {
                                                  _repo.PersistHotkeys(GetDefaultHotkeySet());
                                                  Hotkeys = new ObservableCollection<Hotkey>(_repo.GetHotkeys());

                                                  //let the controller know that the application's hotkeys have changed.
                                                  Controller.Singleton.RegisterHotkeys(Hotkeys.ToList());
                                              }
                                          }));
            }
        }

        #region HotkeysEditCommand

        private RelayCommand _hotkeysEditCommand;

        /// <summary>
        /// Gets the MaintenanceModeEditCommand.
        /// </summary>
        public RelayCommand HotkeysEditCommand
        {
            get
            {
                return _hotkeysEditCommand
                    ?? (_hotkeysEditCommand = new RelayCommand(
                                          () =>
                                          {
                                              IsEditingHotkeys = true;
                                          }));
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
                                              //JDW: if we're navigating from the list view, we have no context course.
                                              Messenger.Default.Send(new NavigateArgs(message), MessageType.Navigate);
                                          }));
            }
        }

        #endregion

        //#region AddHotkeyCommand

        //private RelayCommand _addHotkeyCommand;

        ///// <summary>
        ///// Gets the AddHotkeyCommand.
        ///// </summary>
        //public RelayCommand AddHotkeyCommand
        //{
        //    get
        //    {
        //        return _addHotkeyCommand
        //            ?? (_addHotkeyCommand = new RelayCommand(
        //                                  () =>
        //                                  {

        //                                  }));
        //    }
        //}

        //#endregion

        #region DeleteHotkeysCommand

        private RelayCommand _deleteHotkeysCommand;

        /// <summary>
        /// Gets the DeleteHotkeysCommand.
        /// </summary>
        public RelayCommand DeleteHotkeysCommand
        {
            get
            {
                return _deleteHotkeysCommand
                    ?? (_deleteHotkeysCommand = new RelayCommand(
                                          () =>
                                          {
                                              foreach (Hotkey hotkey in _selectedHotkeys)
                                              {
                                                  Hotkeys.Remove(hotkey);
                                              }
                                          },
                                          () =>
                                          {
                                              return _selectedHotkeys != null && _selectedHotkeys.Count() > 0;
                                          }
                                          ));
            }
        }

        #endregion

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
                                              IsEditingHotkeys = false;
                                              Hotkeys = new ObservableCollection<Hotkey>(_repo.GetHotkeys());
                                          }));
            }
        }

        #region SaveHotkeysCommand

        private RelayCommand _saveHotkeysCommand;

        /// <summary>
        /// Gets the SaveHotkeysCommand.
        /// </summary>
        public RelayCommand SaveHotkeysCommand
        {
            get
            {
                return _saveHotkeysCommand
                    ?? (_saveHotkeysCommand = new RelayCommand(
                                          () =>
                                          {
                                              _repo.PersistHotkeys(Hotkeys.ToList());
                                              Hotkeys = new ObservableCollection<Hotkey>(_repo.GetHotkeys());

                                              //let the controller know that the application's hotkeys have changed.
                                              Controller.Singleton.RegisterHotkeys(Hotkeys.ToList());
                                          }));
            }
        }

        #endregion

        #endregion

        #region Notified Properties

        /// <summary>
        /// The <see cref="IsEditingHotkeys" /> property's name.
        /// </summary>
        public const string IsEditingHotkeysPropertyName = "IsEditingHotkeys";

        private bool _isEditingHotkeys = false;

        /// <summary>
        /// Sets and gets the IsEditingHotkeys property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEditingHotkeys
        {
            get
            {
                return _isEditingHotkeys;
            }

            set
            {
                if (_isEditingHotkeys == value)
                {
                    return;
                }

                _isEditingHotkeys = value;
                RaisePropertyChanged(IsEditingHotkeysPropertyName);
            }
        }

        #region SelectedHotkeys

        /// <summary>
        /// The <see cref="SelectedHotkeys" /> property's name.
        /// </summary>
        public const string SelectedHotkeysPropertyName = "SelectedHotkeys";

        private List<Hotkey> _selectedHotkeys = null;

        /// <summary>
        /// Sets and gets the SelectedHotkeys property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<object> SelectedHotkeys
        {
            //get
            //{
            //    return _SelectedHotkeys.Cast<object>().ToList();
            //}

            set
            {
                if (null == value || value is string)
                {
                    _selectedHotkeys = null;
                }
                else
                {
                    _selectedHotkeys = value.Cast<Hotkey>().ToList();
                }
                RaisePropertyChanged(SelectedHotkeysPropertyName);
                //since the selected Hotkeys have changed, we may be able to delete Hotkeys..
                DeleteHotkeysCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        /// <summary>
        /// The <see cref="Hotkeys" /> property's name.
        /// </summary>
        public const string HotkeysPropertyName = "Hotkeys";

        private ObservableCollection<Hotkey> _hotkeys = null;

        /// <summary>
        /// Sets and gets the Hotkeys property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Hotkey> Hotkeys
        {
            get
            {
                return _hotkeys;
            }

            set
            {
                if (_hotkeys == value)
                {
                    return;
                }

                _hotkeys = value;
                RaisePropertyChanged(HotkeysPropertyName);
            }
        }
        


        #region AreCoursesExisting

        //todo: make dynamic.
        public bool AreSettingsExisting { get { return true; } }

        #endregion

        public NavigateMessage CurrentPage
        {
            get
            {
                return NavigateMessage.Settings;
            }
        }

        #endregion

        #region Constructor

        public SettingsViewModel(SettingRepository repo)
        {
            _repo = repo;
            _uiDispatcher = Dispatcher.CurrentDispatcher;

            var hotkeys = _repo.GetHotkeys();
            Hotkeys = new ObservableCollection<Hotkey>(hotkeys);
        }


        List<Hotkey> GetDefaultHotkeySet()
        {
            List<Hotkey> hotkeys = new List<Hotkey>();
            hotkeys.Add(new Hotkey(HotkeyFunction.ToggleFullscreen, ModifierKeys.Control, Key.F));
            hotkeys.Add(new Hotkey(HotkeyFunction.TogglePause, ModifierKeys.None, Key.NumPad0));
            hotkeys.Add(new Hotkey(HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad3) { SeekSeconds = 10, SeekDirection = Direction.Forward });
            hotkeys.Add(new Hotkey(HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad2) { SeekSeconds = 3, SeekDirection = Direction.Back });
            hotkeys.Add(new Hotkey(HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad2) { SeekSeconds = 10, SeekDirection = Direction.Back });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteBold, ModifierKeys.Control, Key.B));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteBold, ModifierKeys.None, Key.Subtract));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteItalic, ModifierKeys.Control, Key.I));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteItalic, ModifierKeys.None, Key.Add));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad5) { Colour = Color.FromArgb(255, 200, 50, 50) });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad6) { Colour = Color.FromArgb(255, 50, 50, 200) });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad4) { Colour = ColorHelper.ApplicationDefaultTextColour });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteSetStartTime, ModifierKeys.None, Key.Divide));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteEditBegin, ModifierKeys.None, Key.NumPad7));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteEditCommit, ModifierKeys.None, Key.NumPad8));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteEditCancel, ModifierKeys.None, Key.NumPad9));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D1) { Rating = 1 });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D2) { Rating = 2 });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D3) { Rating = 3 });

            return hotkeys;
        }

        #endregion
    }
}
