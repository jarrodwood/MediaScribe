using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace JayDev.Notemaker.Common
{
    public class DoubleClickableDataGrid : DataGrid
    {
        public ICommand RowDoubleClickCommand
        {
            get { return (ICommand)GetValue(RowDoubleClickCommandProperty); }
            set { SetValue(RowDoubleClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowDoubleClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowDoubleClickCommandProperty =
            DependencyProperty.Register("RowDoubleClickCommand", typeof(ICommand), typeof(DoubleClickableDataGrid), new UIPropertyMetadata(null));

        public DoubleClickableDataGrid()
            : base()
        {
            this.MouseDoubleClick += new MouseButtonEventHandler(DoubleClickableDataGrid_MouseDoubleClick);
        }

        void DoubleClickableDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)this, e.GetPosition(this));

            if (row != null)
            {
                RowDoubleClickCommand.Execute(null);
            }
        }
    }
}
