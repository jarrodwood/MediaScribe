using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Windows.Controls.Primitives;

namespace JayDev.Notemaker.View.Controls
{
    /// <summary>
    /// Interaction logic for NotesGridControl.xaml
    /// </summary>
    public partial class NotesGridControl : UserControl
    {
        Dispatcher _uiDispatcher;

        public ObservableCollection<Note> Notes
        {
            get { return (ObservableCollection<Note>)GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Notes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(ObservableCollection<Note>), typeof(NotesGridControl), new UIPropertyMetadata(null));




        //public Track CurrentTrack
        //{
        //    get { return (Track)GetValue(CurrentTrackProperty); }
        //    set { SetValue(CurrentTrackProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for CurrentTrack.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty CurrentTrackProperty =
        //    DependencyProperty.Register("CurrentTrack", typeof(Track), typeof(NotesGridControl), new UIPropertyMetadata(null));




        //public TimeSpan CurrentPosition
        //{
        //    get { return (TimeSpan)GetValue(CurrentPositionProperty); }
        //    set { SetValue(CurrentPositionProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for CurrentPosition.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty CurrentPositionProperty =
        //    DependencyProperty.Register("CurrentPosition", typeof(TimeSpan), typeof(NotesGridControl), new UIPropertyMetadata(new TimeSpan()));






        #region PrepareNoteForEditCommand

        public static readonly DependencyProperty PrepareNoteForEditCommandProperty =
            DependencyProperty.Register("PrepareNoteForEditCommand", typeof(ICommand), typeof(NotesGridControl));

        /// <summary>
        /// Gets the PrepareNewNoteCommand.
        /// </summary>
        public ICommand PrepareNoteForEditCommand
        {
            get { return (ICommand)GetValue(PrepareNoteForEditCommandProperty); }
            set { SetValue(PrepareNoteForEditCommandProperty, value); }
        }

        #endregion

        #region NoteSavedCommand

        public static readonly DependencyProperty NoteSavedCommandProperty =
            DependencyProperty.Register("NoteSavedCommand", typeof(ICommand), typeof(NotesGridControl));

        /// <summary>
        /// Gets the NoteSavedCommand.
        /// </summary>
        public ICommand NoteSavedCommand
        {
            get { return (ICommand)GetValue(NoteSavedCommandProperty); }
            set { SetValue(NoteSavedCommandProperty, value); }
        }

        #endregion

        #region SetNoteStartTimeCommand

        public static readonly DependencyProperty SetNoteStartTimeCommandProperty =
            DependencyProperty.Register("SetNoteStartTimeCommand", typeof(ICommand), typeof(NotesGridControl));

        /// <summary>
        /// Gets the SetNoteStartTimeCommand.
        /// </summary>
        public ICommand SetNoteStartTimeCommand
        {
            get { return (ICommand)GetValue(SetNoteStartTimeCommandProperty); }
            set { SetValue(SetNoteStartTimeCommandProperty, value); }
        }

        #endregion

        #region PlayNoteCommand

        public static readonly DependencyProperty PlayNoteCommandProperty =
            DependencyProperty.Register("PlayNoteCommand", typeof(ICommand), typeof(NotesGridControl));

        /// <summary>
        /// Gets the PlayNoteCommand.
        /// </summary>
        public ICommand PlayNoteCommand
        {
            get { return (ICommand)GetValue(PlayNoteCommandProperty); }
            set { SetValue(PlayNoteCommandProperty, value); }
        }

        #endregion
        

        public NotesGridControl()
        {
            InitializeComponent();
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            Messenger.Default.Register<KeyEventArgs>(this, 999, (message) => HandleKeyPress(message));
        }

        private void HandleKeyPress(KeyEventArgs e)
        {
            //only fuck around with the UI if the window's visible
            if (this.Visibility == Visibility.Visible)
            {
                switch (e.Key)
                {
                    case Key.NumPad7:
                        Messenger.Default.Send<string>("show", 12345);
                        BeginEditNewNote();
                        e.Handled = true;
                        break;
                    case Key.NumPad8:
                        noteDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                        Messenger.Default.Send<string>("hide", 12345);
                        e.Handled = true;
                        break;
                    case Key.NumPad9:
                        noteDataGrid.CancelEdit(DataGridEditingUnit.Row);
                        break;
                }
            }
        }

        private void BeginEditNewNote()
        {
            DataGridCell cell = null;
            for (int i = 0; i < noteDataGrid.Columns.Count; i++)
            {
                if (String.Equals(noteDataGrid.Columns[i].Header, "Note"))
                {
                    cell = GetCell(noteDataGrid, noteDataGrid.Items.Count - 1, i);
                    break;
                }
            }
            if (cell != null)
            {
                ////operation to perform on row edit ended
                _uiDispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
                {
                    noteDataGrid.ScrollIntoView(noteDataGrid.Items[noteDataGrid.Items.Count-1]);
                    cell.Focus();
                    noteDataGrid.CurrentCell = new DataGridCellInfo(cell);
                    noteDataGrid.BeginEdit();
                    return null;
                }), DispatcherPriority.Send, new object[] { null });
            }
        }
        //public DataGridCell GetCell(int row, int column)
        //{
        //    DataGridRow rowContainer = GetRow(row);

        //    if (rowContainer != null)
        //    {
        //        DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

        //        // try to get the cell but it may possibly be virtualized
        //        DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
        //        if (cell == null)
        //        {
        //            // now try to bring into view and retreive the cell
        //            DataGrid_Standard.ScrollIntoView(rowContainer, DataGrid_Standard.Columns[column]);
        //            cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
        //        }
        //        return cell;
        //    }
        //    return null;
        //}


        //public DataGridRow GetRow(int index)
        //{
        //    DataGridRow row = (DataGridRow)DataGrid_Standard.ItemContainerGenerator.ContainerFromIndex(index);
        //    if (row == null)
        //    {
        //        // may be virtualized, bring into view and try again
        //        DataGrid_Standard.ScrollIntoView(DataGrid_Standard.Items[index]);
        //        row = (DataGridRow)DataGrid_Standard.ItemContainerGenerator.ContainerFromIndex(index);
        //    }
        //    return row;
        //}

        DataGridCell GetCell(DataGrid dg, int rowIndex, int columnIndex)
        {
            var dr = dg.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            var dc = dg.Columns[columnIndex].GetCellContent(dr);
            return dc.Parent as DataGridCell;
        }
        //public static DataGridCell GetDataGridCell(DataGrid grid, int rowIndex, int colIndex)
        //{
        //    DataGridCell result = null;
        //    DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
        //    if (row != null)
        //    {

        //        DataGridCellsPresenter presenter = GetFirstVisualChild<DataGridCellsPresenter>(row);
        //        result = presenter.ItemContainerGenerator.ContainerFromIndex(colIndex) as DataGridCell;

        //    }

        //    return result;
        //}

        //public static T GetFirstVisualChild<T>(DependencyObject depObj)
        //{
        //    if (depObj != null)
        //    {
        //        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        //        {
        //            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
        //            if (child != null && child is T)
        //            {
        //                return (T)child;
        //            }

        //            T childItem = GetFirstVisualChild(child);
        //            if (childItem != null) return childItem;
        //        }
        //    }

        //    return null;
        //}

        private void noteDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Note context = e.Row.DataContext as Note;
            if (null != context)
            {
                if (string.IsNullOrEmpty(context.Body))
                {
                    context.Start = null;
                    e.Cancel = true;
                    //For some reason, this doesn't seem to cancel properly when we programmatically begin editing. To ensure
                    //that blank notes don't get added to the collection, once this operation is performed we'll manually remove
                    //them. set at 'send' priority, because we really want this to happen.
                    this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
                    {
                        if (Notes.Count > 0)
                        {
                            for (int i = Notes.Count-1; i >= 0; i--)
                            {
                                if (string.IsNullOrEmpty(Notes[i].Body))
                                {
                                    Notes.RemoveAt(i);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            Note blankNote = new Note();
                            Notes.Add(blankNote);
                            noteDataGrid.UpdateLayout();
                            noteDataGrid.ScrollIntoView(noteDataGrid.Items[noteDataGrid.Items.Count-1]);
                        }
                        return null;
                    }), DispatcherPriority.Send, new object[] { null });
                }
                else
                {
                    //operation to perform on row edit ended
                    this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
                    {
                        NoteSavedCommand.Execute(context);
                        if (false == string.IsNullOrEmpty(Notes.Last().Body))
                        {
                            Notes.Add(new Note());
                        }
                        noteDataGrid.UpdateLayout();
                        noteDataGrid.ScrollIntoView(noteDataGrid.Items[noteDataGrid.Items.Count - 1]);
                        return null;
                    }), DispatcherPriority.Background, new object[] { null });
                }
            }
        }

        private void noteDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Note context = e.Row.DataContext as Note;
            if (null != context)
            {
                if (null == context.Start)
                {
                    PrepareNoteForEditCommand.Execute(context);
                }
            }
        }



        private void setNoteStartTime_Click(object sender, RoutedEventArgs e)
        {
            Note note = ((FrameworkElement)sender).DataContext as Note;
            SetNoteStartTimeCommand.Execute(note);
            noteDataGrid.CommitEdit();
        }

        private void playNoteButton_Click(object sender, RoutedEventArgs e)
        {
            Note note = ((FrameworkElement)sender).DataContext as Note;
            PlayNoteCommand.Execute(note);
        }

        private void noteDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

    }
}
