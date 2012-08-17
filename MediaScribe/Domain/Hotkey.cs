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
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe
{
    public class Hotkey : JayDev.MediaScribe.Common.HotkeyBase, INotifyPropertyChanged
    {

        public virtual int? ID { get; set; }

        public virtual event PropertyChangedEventHandler PropertyChanged;


        private Key _key;
        public override Key Key
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
        public override ModifierKeys ModifierKey { get; set; }

        private HotkeyFunction _function;

        public override HotkeyFunction Function
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

                    SetOrderWeight();
                }
            }
        }

        public override int SeekSeconds { get; set; }
        public override Direction SeekDirection { get; set; }

        public override Color Colour { get; set; }

        public override int Rating { get; set; }

        private int _orderWeight = -1;
        public virtual int OrderWeight
        {
            get
            {
                if (_orderWeight == -1)
                {
                    SetOrderWeight();
                }
                return _orderWeight;
            }
        }

        public Hotkey()
        {
            SetDefaultParameterValues();
        }

        public Hotkey(HotkeyFunction function, ModifierKeys modifier, Key key)
        {
            this.Function = function;
            this.Key = key;
            this.ModifierKey = modifier;

            SetDefaultParameterValues();
        }

        private void SetOrderWeight()
        {
            _orderWeight = Function.GetOrderWeight();
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
            this.Colour = ColorHelper.ApplicationDefaultTextColour;
            this.SeekDirection = Direction.Back;
            this.SeekSeconds = 3;
        }

        public override bool Equals(object obj)
        {
            Hotkey other = obj as Hotkey;
            if (null == other)
            {
                return false;
            }

            return (this.Function == other.Function
                && this.Colour == other.Colour
                && this.ModifierKey == other.ModifierKey
                && this.Rating == other.Rating
                && this.SeekDirection == other.SeekDirection
                && this.SeekSeconds == other.SeekSeconds
                && this.Key == other.Key);
        }
    }
}
