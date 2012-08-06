using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;

namespace JayDev.Notemaker.View.Controls
{
    public class UntoggleableToggleButton : ToggleButton
    {
        protected override void OnClick()
        {
            if (false == this.IsChecked)
                base.OnClick();
        }
    }
}
