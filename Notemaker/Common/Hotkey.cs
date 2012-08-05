using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;

namespace JayDev.Notemaker.Common
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the DescriptionAttribute valud of an enum value, if none are found uses the string version of the specified value
        /// </summary>
        public static InformationAttribute GetInformation(this Enum value)
        {
            Type type = value.GetType();

            return GetEnumInformation(value.ToString(), type);
        }

        public static InformationAttribute GetInformationForEnum(this Type type, object value)
        {
            return GetEnumInformation(value.ToString(), type);
        }

        private static InformationAttribute GetEnumInformation(string value, Type type)
        {
            MemberInfo[] memberInfo = type.GetMember(value);

            if (memberInfo != null && memberInfo.Length > 0)
            {
                // default to the first member info, it's for the specific enum value
                var info = memberInfo.First().GetCustomAttributes(typeof(InformationAttribute), false).FirstOrDefault();

                if (info != null)
                    return ((InformationAttribute)info);
            }

            // no description - return the string value of the enum
            return null;
        }
    }
    public class Hotkey : INotifyPropertyChanged
    {
        public static List<Hotkey> GetDefaultHotkeys()
        {
            List<Hotkey> hotkeys = new List<Hotkey>();
            hotkeys.Add(new Hotkey("Toggle fullscreen", HotkeyFunction.ToggleFullscreen, ModifierKeys.Control, Key.F));
            hotkeys.Add(new Hotkey("Toggle Pause", HotkeyFunction.TogglePause, ModifierKeys.None, Key.NumPad0));
            hotkeys.Add(new Hotkey("Forward 10", HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad3) { SeekSeconds = 10, SeekDirection = Direction.Forward });
            hotkeys.Add(new Hotkey("Back 3", HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad2) { SeekSeconds = 3, SeekDirection = Direction.Back });
            hotkeys.Add(new Hotkey("Back 10", HotkeyFunction.Seek, ModifierKeys.None, Key.NumPad2) { SeekSeconds = 10, SeekDirection = Direction.Back });
            hotkeys.Add(new Hotkey("Bold", HotkeyFunction.NoteBold, ModifierKeys.Control, Key.B));
            hotkeys.Add(new Hotkey("Italics", HotkeyFunction.NoteItalic, ModifierKeys.Control, Key.I));
            hotkeys.Add(new Hotkey("Colour red", HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad5) { Colour = Color.FromArgb(255, 200, 50, 50) });
            hotkeys.Add(new Hotkey("Colour blue", HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad6) { Colour = Color.FromArgb(255, 50, 50, 200) });
            hotkeys.Add(new Hotkey("Colour gray", HotkeyFunction.NoteColour, ModifierKeys.None, Key.NumPad4) { Colour = Color.FromArgb(255, 150, 150, 150) });
            hotkeys.Add(new Hotkey("Set start time", HotkeyFunction.NoteSetStartTime, ModifierKeys.None, Key.Divide));
            hotkeys.Add(new Hotkey("Edit begin", HotkeyFunction.NoteEditBegin, ModifierKeys.None, Key.NumPad7));
            hotkeys.Add(new Hotkey("Edit commit", HotkeyFunction.NoteEditCommit, ModifierKeys.None, Key.NumPad8));
            hotkeys.Add(new Hotkey("Edit cancel", HotkeyFunction.NoteEditCancel, ModifierKeys.None, Key.NumPad9));
            hotkeys.Add(new Hotkey("Rating 1", HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D1) { Rating = 1 });
            hotkeys.Add(new Hotkey("Rating 2", HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D2) { Rating = 2 });
            hotkeys.Add(new Hotkey("Rating 3", HotkeyFunction.NoteRating, ModifierKeys.Control, Key.D3) { Rating = 3 });
            return hotkeys;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public string UserLabel { get; set; }
        private Key _key;
        public Key Key
        {
            get
            {
                return _key;
            }
            set
            {
                bool isChanged = _key != value;
                _key = value;
                if (isChanged)
                {
                    OnPropertyChanged("Key");
                }
            }
        }
        public ModifierKeys ModifierKey { get; set; }

        private HotkeyFunction _function;

        public HotkeyFunction Function
        {
            get { return _function; }
            set
            {
                bool isChanged = _function != value;
                _function = value;
                if (isChanged)
                {
                    OnPropertyChanged("Function");

                    //on changing the function, clear all of the parameter fields
                    SetDefaultParameterValues();
                }
            }
        }

        public int SeekSeconds { get; set; }
        public Direction SeekDirection { get; set; }

        public Color Colour { get; set; }

        public int Rating { get; set; }

        public Hotkey()
        {
            SetDefaultParameterValues();
        }

        public Hotkey(string userLabel, HotkeyFunction function, ModifierKeys modifier, Key key)
        {
            this.Function = function;
            this.UserLabel = userLabel;
            this.Key = key;
            this.ModifierKey = modifier;

            SetDefaultParameterValues();
        }

        // Create the OnPropertyChanged method to raise the event
        public virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void SetDefaultParameterValues()
        {
            //Value: #FF3F3F3F
            this.Colour = ColorExtensions.ApplicationDefaultTextColour;
            this.SeekDirection = Direction.Back;
            this.SeekSeconds = 3;
        }
    }
}
