using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// WPF ComboBoxItems do not support tooltips out of the box - when you hover the cursor over the ComboBoxItem, it will attempt to
    /// display the tooltip of the the ComboBoxItem's ancestral 'Border' control. Therefore to make tooltips work, this subclass of the
    /// ComboBoxItem will automatically replicate any tooltip given to it, to its ancestral 'Border' control too.
    /// </summary>
    public class ComboBoxItemWithTooltip : ComboBoxItem
    {
        public ComboBoxItemWithTooltip()
            : base()
        {
            this.Loaded += new System.Windows.RoutedEventHandler(ComboBoxItem_Loaded);
            this.IsHitTestVisible = false;
        }

        /// <summary>
        /// When this visual element is loaded, it will replicate its specified tooltip to the ancestral 'Border' control, too.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ComboBoxItem_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            if (null != item)
            {
                var parentBorder = UIHelpers.TryFindParent<Border>((ComboBoxItem)sender);
                parentBorder.ToolTip = item.ToolTip;
            }
        }
    }
}
