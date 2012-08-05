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

namespace JayDev.Notemaker.ViewModel
{
    public class SettingsViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Private Members

        private SettingRepository _repo;
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
                                              Messenger.Default.Send(new NavigateArgs(message), MessageType.Navigate);
                                          }));
            }
        }

        #endregion

        #endregion

        #region Notified Properties

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


        void SaveDefaultHotkeySet()
        {
            List<Hotkey> hotkeys = new List<Hotkey>();
            hotkeys.Add(new Hotkey(HotkeyFunction.ToggleFullscreen, ModifierKeys.Control, Key.F));
            hotkeys.Add(new Hotkey(HotkeyFunction.TogglePause, ModifierKeys.None, Key.NumPad0));
            hotkeys.Add(new Hotkey(HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad3) { SeekSeconds = 10, SeekDirection = Direction.Forward });
            hotkeys.Add(new Hotkey(HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad2) { SeekSeconds = 3, SeekDirection = Direction.Back });
            hotkeys.Add(new Hotkey(HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad2) { SeekSeconds = 10, SeekDirection = Direction.Back });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteBold, ModifierKeys.Control, Key.B));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteItalic, ModifierKeys.Control, Key.I));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad5) { Colour = Color.FromArgb(255, 200, 50, 50) });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad6) { Colour = Color.FromArgb(255, 50, 50, 200) });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad4) { Colour = Color.FromArgb(255, 150, 150, 150) });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteSetStartTime, ModifierKeys.None, Key.Divide));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteEditBegin, ModifierKeys.None, Key.NumPad7));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteEditCommit, ModifierKeys.None, Key.NumPad8));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteEditCancel, ModifierKeys.None, Key.NumPad9));
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D1) { Rating = 1 });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D2) { Rating = 2 });
            hotkeys.Add(new Hotkey(HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D3) { Rating = 3 });

            foreach (Hotkey hotkey in hotkeys)
            {
                _repo.SaveHotkey(hotkey);
            }
        }

        #endregion
    }
}
