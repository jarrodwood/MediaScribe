using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.View.Controls
{
    public class DoubleClickableDataGrid : DataGrid
    {
        #region RowDoubleClickCommand

        public ICommand RowDoubleClickCommand
        {
            get { return (ICommand)GetValue(RowDoubleClickCommandProperty); }
            set { SetValue(RowDoubleClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowDoubleClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowDoubleClickCommandProperty =
            DependencyProperty.Register("RowDoubleClickCommand", typeof(ICommand), typeof(DoubleClickableDataGrid), new UIPropertyMetadata(null));

        #endregion

        #region RowDoubleClickCommandParameter

        public object RowDoubleClickCommandParameter
        {
            get { return (object)GetValue(RowDoubleClickCommandParameterProperty); }
            set { SetValue(RowDoubleClickCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowDoubleClickCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowDoubleClickCommandParameterProperty =
            DependencyProperty.Register("RowDoubleClickCommandParameter", typeof(object), typeof(DoubleClickableDataGrid), new UIPropertyMetadata(null));

        #endregion

        #region HightlightItem

        public object HighlightItem
        {
            get { return (object)GetValue(HighlightItemProperty); }
            set { SetValue(HighlightItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightItemProperty =
            DependencyProperty.Register("HighlightItem", typeof(object), typeof(DoubleClickableDataGrid), new UIPropertyMetadata(null));

        #endregion


        public DoubleClickableDataGrid()
            : base()
        {
            this.MouseDoubleClick += new MouseButtonEventHandler(DoubleClickableDataGrid_MouseDoubleClick);
        }

        #region Event Handlers

        void DoubleClickableDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ensure that the double-click happened on a ROW, not the header or something.
            DataGridRow row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)this, e.GetPosition(this));
            if (row != null)
            {
                RowDoubleClickCommand.Execute(RowDoubleClickCommandParameter);
            }
        }

        #endregion
    }
}
