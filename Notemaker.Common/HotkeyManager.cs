using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.MediaScribe.Common;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;

namespace MediaScribe.Common
{
    public class HotkeyManager
    {
        private static HotkeyManager _instance;
        public static HotkeyManager Singleton
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new HotkeyManager();
                }
                return _instance;
            }
        }

        struct keyMatch
        {
            public ModifierKeys Modifier { get; set; }
            public Key Key { get; set; }
        }
        static Dictionary<keyMatch, List<HotkeyBase>> hotkeysByKey = new Dictionary<keyMatch, List<HotkeyBase>>();
        static object lockToken = new object();
        public static List<HotkeyBase> CheckHotkey(KeyEventArgs e)
        {
            lock (lockToken)
            {
                var modifiers = (((System.Windows.Input.KeyboardEventArgs)(e)).KeyboardDevice).Modifiers;
                //if there are more than one flags set (bit operation), return
                if ((modifiers & (modifiers - 1)) != 0)
                {
                    return null;
                }

                keyMatch keyMatch = new keyMatch() { Key = e.Key, Modifier = modifiers };
                List<HotkeyBase> matches;
                if (hotkeysByKey.TryGetValue(keyMatch, out matches))
                {
                    return matches;
                }

                return null;
            }
        }

        public static void HandleHotkeyRegistration(List<HotkeyBase> hotkeys)
        {
            lock (lockToken)
            {
                hotkeysByKey = hotkeys.GroupBy(x => new keyMatch() { Key = x.Key, Modifier = x.ModifierKey }).ToDictionary(x => x.Key, x => x.ToList());
            }
        }
    }
}
