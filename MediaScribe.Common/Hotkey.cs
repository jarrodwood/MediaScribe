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

namespace JayDev.MediaScribe.Common
{
    public class HotkeyBase
    {

        public virtual Key Key { get; set; }
        public virtual ModifierKeys ModifierKey { get; set; }

        public virtual HotkeyFunction Function { get; set; }

        public virtual int SeekSeconds { get; set; }

        public virtual Direction SeekDirection { get; set; }

        public virtual Color Colour { get; set; }

        public virtual int Rating { get; set; }

        public virtual int SpeedModifierPercent { get; set; }
    }
}
