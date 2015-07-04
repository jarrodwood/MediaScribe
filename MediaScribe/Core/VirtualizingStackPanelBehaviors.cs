using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace JayDev.MediaScribe.Core
{
    /// <summary>
    /// Hack to expose the 'ScrollUnit' property of WPF data grid. This is made a public property .Net 4.5,
    /// but is internal in .Net 4.0. Hack from http://stackoverflow.com/questions/13383613/how-can-i-get-a-datagrid-to-scroll-smoothly-when-the-row-is-taller-than-the-view
    /// </summary>
    public static class VirtualizingStackPanelBehaviors
    {
        public static bool GetIsPixelBasedScrolling(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPixelBasedScrollingProperty);
        }

        public static void SetIsPixelBasedScrolling(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPixelBasedScrollingProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsPixelBasedScrolling.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPixelBasedScrollingProperty =
            DependencyProperty.RegisterAttached("IsPixelBasedScrolling", typeof(bool), typeof(VirtualizingStackPanelBehaviors), new UIPropertyMetadata(false, OnIsPixelBasedScrollingChanged));

        private static void OnIsPixelBasedScrollingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var virtualizingStackPanel = o as VirtualizingStackPanel;
            if (virtualizingStackPanel == null)
                throw new InvalidOperationException();

            var isPixelBasedPropertyInfo = typeof(VirtualizingStackPanel).GetProperty("IsPixelBased", BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);
            if (isPixelBasedPropertyInfo == null)
                throw new InvalidOperationException();

            isPixelBasedPropertyInfo.SetValue(virtualizingStackPanel, (bool)(e.NewValue), null);
        }
    }
}
