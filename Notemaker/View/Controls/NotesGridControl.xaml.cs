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
using System.ComponentModel;
using System.Diagnostics;

namespace JayDev.Notemaker.View.Controls
{
    /// <summary>
    /// Interaction logic for NotesGridControl.xaml
    /// </summary>
    public partial class NotesGridControl : UserControl
    {
        Dispatcher _uiDispatcher;


        public Note CurrentNote { get { return (Note)noteDataGrid.SelectedItem; } }

        public ObservableCollection<Note> Notes
        {
            get { return (ObservableCollection<Note>)GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
        }


        #region Dependency Properties

        // Using a DependencyProperty as the backing store for Notes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(ObservableCollection<Note>), typeof(NotesGridControl), new UIPropertyMetadata(null));




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

        #region NoteEditCompletedCommand

        public static readonly DependencyProperty NoteEditCompletedCommandProperty =
            DependencyProperty.Register("NoteEditCompletedCommand", typeof(ICommand), typeof(NotesGridControl));

        /// <summary>
        /// Gets the NoteEditCompletedCommand.
        /// </summary>
        public ICommand NoteEditCompletedCommand
        {
            get { return (ICommand)GetValue(NoteEditCompletedCommandProperty); }
            set { SetValue(NoteEditCompletedCommandProperty, value); }
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

        #endregion

        public NotesGridControl()
        {
            InitializeComponent();
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            Messenger.Default.Register<KeyEventArgs>(this, 999, (message) => HandleKeyPress(message));
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(NotesGridControl_IsVisibleChanged);
        }


        private static NotesGridControl _lastNotesGridControlVisible = null;

        void NotesGridControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (true == (bool)e.NewValue)
            {
                _lastNotesGridControlVisible = this;
            }
        }



        private void HandleKeyPress(KeyEventArgs e)
        {
                //only fuck around with the UI if the window's visible
                //TODO: this is kind of hacky. figure out a better way!
                if (_lastNotesGridControlVisible == this)
                {
                    switch (e.Key)
                    {
                        case Key.NumPad7:
                            //noteDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
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
                            e.Handled = true;
                            break;
                    }
                }
        }

        public void BeginEditNewNote()
        {
            //for (int i = 100; i >= 0; i--)
            //{
            //    //JDW: commented out, since the reason the container wasn't ready was because the instance of the grid wasn't visible.
            //    if (i == 0)
            //        throw new Exception("why the hell?!");
            //    if (noteDataGrid.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            //    {
            //        break;
            //    }
            //    System.Threading.Thread.Sleep(20);
            //}
            DataGridCell cell = null;
            for (int i = 0; i < noteDataGrid.Columns.Count; i++)
            {
                if (String.Equals(noteDataGrid.Columns[i].Header, "Note"))
                {
                    cell = GetCell(noteDataGrid, Notes.Count, i);
                    break;
                }
            }
            if (cell != null)
            {
                noteDataGrid.ScrollIntoView(noteDataGrid.Items[noteDataGrid.Items.Count - 1]);
                cell.Focus();
                noteDataGrid.CurrentCell = new DataGridCellInfo(cell);
                noteDataGrid.BeginEdit();
            }
        }


        DataGridCell GetCell(DataGrid dg, int rowIndex, int columnIndex)
        {
            var dr = dg.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            var dc = dg.Columns[columnIndex].GetCellContent(dr);
            return dc.Parent as DataGridCell;
        }


        private void noteDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Note context = e.Row.DataContext as Note;
                //operation to perform on row edit ended
                this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
                {
                    NoteEditCompletedCommand.Execute(context);
                    return null;
                }), DispatcherPriority.Normal, new object[] { null });
            }
        }

        private bool _isEditing = false;
        public bool IsEditing { get { return _isEditing; } }

        private void noteDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Debug.WriteLine("BEGIN EDIT");
            _isEditing = true;
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
            Debug.WriteLine("LEAVING EDIT");
            _isEditing = false;
        }


        public void CommitEdit()
        {
            noteDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }
        public void CancelEdit()
        {
            noteDataGrid.CancelEdit(DataGridEditingUnit.Row);
        }

    }
}
