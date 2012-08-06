using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using JayDev.Notemaker.Common;

namespace JayDev.Notemaker.View.Controls
{
    public class ComboBoxItemWithTooltip : ComboBoxItem
    {
        public ComboBoxItemWithTooltip()
            : base()
        {
            this.Loaded += new System.Windows.RoutedEventHandler(ComboBoxItem_Loaded);
            this.IsHitTestVisible = false;
        }

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
