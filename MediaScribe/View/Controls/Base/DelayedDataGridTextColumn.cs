﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// This modification of the typical DataGridTextColumn will make the loading of its containing text a lower priority - all other
    /// controls in the window will be rendered first, /then/ the text in this control. This will make loading feel 'snappier' since the
    /// whole screen won't halt while every chunk of text is rendered.
    /// </summary>
    public class DelayedDataGridTextColumn : DataGridTextColumn
    {
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var textBlock = new TextBlock();
            textBlock.SetValue(FrameworkElement.StyleProperty, ElementStyle);

            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action<TextBlock>(x => x.SetBinding(TextBlock.TextProperty, Binding)),
            textBlock);
            return textBlock;
        }
    }
}
