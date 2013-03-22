using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.MediaScribe.Common;

namespace AvalonTextBox
{
    class AvalonHelper
    {
        public static List<Section> GetInitialSectionsForBlankString()
        {
            return new List<Section>(new[] { 
                    new Section() {
                        Colour = ColorHelper.ApplicationDefaultTextColour,
                        Text = string.Empty }});
        }
    }
}
