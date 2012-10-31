using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace JayDev.MediaScribe.View.Controls
{
    public class MediaScribeMainTabControl : TabControl
    {
        /// <summary>
        /// We want to be able to suppress the handling of the SelectedIndexChange event on the application's main tab control, so that when
        /// the application itself triggers the tab change (as opposed to the user selecting the tab manually. Also note that when the
        /// application triggers the tab change, this occurs AFTER the Navigation logic has executed), it doesn't trigger the navigation
        /// logic again.
        /// </summary>
        public bool SuppressNextSelectedIndexChangeEvent { get; set; }
    }
}
