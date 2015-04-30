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
using JayDev.MediaScribe.Common;
using GalaSoft.MvvmLight.Command;

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// Interaction logic for NotesGridControl.xaml
    /// </summary>
    public partial class NotesGridControl : UserControl
    {
        Dispatcher _uiDispatcher;


        public Note CurrentSelectedNote { get { return (Note)noteDataGrid.SelectedItem; } }


        public bool IsLoaded { get { return noteDataGrid.IsLoaded; } }
        //TODO: get rid of this!
        public DataGrid NoteGrid { get { return noteDataGrid; } }

        #region Dependency Properties


        

        public ObservableCollection<Note> Notes
        {
            get { return (ObservableCollection<Note>)GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Notes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(ObservableCollection<Note>), typeof(NotesGridControl), new UIPropertyMetadata(null));

        #endregion

        #region Commands

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

        private RelayCommand _doNothing;

        /// <summary>
        /// We need a blank command to bind the 'delete' key to, in the WPF datagrid... since it automatically hooks up the delete key, to
        /// delete the selected items. we want to control all hotkeys ourselves...
        /// </summary>
        public RelayCommand DoNothing
        {
            get
            {
                return _doNothing
                    ?? (_doNothing = new RelayCommand(
                                          () =>
                                          {
                                          }));
            }
        }

        private RelayCommand _deleteSelectedNote;

        /// <summary>
        /// Gets the DeleteSelectedNote.
        /// </summary>
        public RelayCommand DeleteSelectedNote
        {
            get
            {
                return _deleteSelectedNote
                    ?? (_deleteSelectedNote = new RelayCommand(
                                          () =>
                                          {
                                              if (null != noteDataGrid.SelectedItem && noteDataGrid.SelectedItem is Note)
                                              {
                                                  Note note = noteDataGrid.SelectedItem as Note;
                                                  Notes.Remove(note);
                                              }
                                          }));
            }
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

        public void BeginEditNewNote()
        {
            //NOTE: we need to scroll down to the bottom row, to make sure the placeholder row is in view. if it isn't in view, the cells
            //      aren't rendered, and we can't get a reference to the cells to start editing.
            //NOTE: always update layout before scrolling
            noteDataGrid.UpdateLayout();
            noteDataGrid.ScrollIntoView(noteDataGrid.Items[noteDataGrid.Items.Count - 1]);
            
            DataGridCell cell = null;
            DataGridColumn noteColumn = null;
            for (int i = 0; i < noteDataGrid.Columns.Count; i++)
            {
                if (String.Equals(noteDataGrid.Columns[i].Header, "Note"))
                {
                    noteColumn = noteDataGrid.Columns[i];
                    cell = GetCell(noteDataGrid, Notes.Count, i);
                    break;
                }
            }
            if (cell != null)
            {
                cell.Focus();
                noteDataGrid.CurrentCell = new DataGridCellInfo(cell);
                //NOTE: if we don't update layout here, we sometimes get argumentoutofrangeexceptions being thrown when beginning edit
                noteDataGrid.UpdateLayout();
                noteDataGrid.BeginEdit();

                //the code above successfully brings focus to the cell (even if there are scrollsbars), and begins editing... however,
                //sometimes the scrollbars jump UP, so the row can't be seen. the reason /why/ is unclear, but it's related to the datagrid
                //code. without digging through datagrid code (ergh) the simplest hack is to scroll /back/ into view, at 'input' priority
                //(sending any higher than this will scroll into view BEFORE it jumps up)
                ThreadHelper.ExecuteSyncUI(Dispatcher.CurrentDispatcher, delegate
                {
                    noteDataGrid.ScrollIntoView(noteDataGrid.Items[noteDataGrid.Items.Count - 1], noteColumn);
                }, DispatcherPriority.Input);
            }
        }


        /// <summary>
        /// Ensures that whatever the user types into the 'rating' textbox, is only numeric, and not of an excessively large length.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            bool areAllNumbersNumericChars = Utility.AreAllValidNumericChars(e.Text);
            bool isLengthOK = textbox.Text.Length + e.Text.Length <= 4;
            bool stopInput = (false == areAllNumbersNumericChars || false == isLengthOK);
            if (stopInput)
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }


        DataGridCell GetCell(DataGrid dg, int rowIndex, int columnIndex)
        {
            var dr = dg.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            //NOTE: must always update layout before scrolling.
            dg.UpdateLayout();
            //NOTE: if we don't scroll the specific cell into view here, we /sometimes/ have the problem that the cell cannot be found. WPF...
            dg.ScrollIntoView(dr, dg.Columns[columnIndex]);
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
                    noteDataGrid.ScrollIntoView(noteDataGrid.Items[noteDataGrid.Items.Count - 1]);
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
                //TODO: check 2013-04-04
                if (null == context.StartTime)
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

        /// <summary>
        /// Code to execute when we begin editing a cell. NOTE: this is called for /any/ cell, so may contain different logic depending on
        /// which column the cell belongs to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void noteDataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            //Logic for Notes column cells
            var textbox = UIHelpers.GetVisualChild<AvalonTextBox.AvalonTextBox>(e.Row);
            if (textbox != null)
            {
                //set focus on the textbox, so the user can just start typing.
                Keyboard.Focus(textbox);

                //add a resize event handler, so that if the size of the textbox increases, we can scroll the grid to see the new line/s.
                textbox.TextArea.TextView.SizeChanged -= TextView_SizeChanged;
                textbox.TextArea.TextView.SizeChanged += new SizeChangedEventHandler(TextView_SizeChanged);
            }
        }

        /// <summary>
        /// When the size of the textbox for a currently-edited note changes, ensure that the datagrid scrolls to show the entire textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //TODO: change logic to execute on textbox content change (i.e. when they hit a key), rather than size change. This will be
            //      a lot more sensible, but more expensive to implement and i'll have to figure out just /how much/ more later.
            noteDataGrid.UpdateLayout();
            
            //year 2015, was: noteDataGrid.ScrollIntoView(noteDataGrid.Items[noteDataGrid.Items.Count - 1]);
            //but the problem was, it didn't work for editing cells in high pages, only the bottom one.
            noteDataGrid.ScrollIntoView(noteDataGrid.CurrentCell.Item);
        }
    }
}
