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
using System.Windows.Controls.Primitives;
using JayDev.MediaScribe.ViewModel;
using System.Windows.Threading;
using JayDev.MediaScribe.Common;
using System.Timers;
using MediaScribe.Common;

namespace JayDev.MediaScribe.View
{
    /// <summary>
    /// Interaction logic for CourseUseView.xaml
    /// </summary>
    public partial class CourseUseView : UserControl
    {
        private CourseUseViewModel _viewModel;

        private readonly Dispatcher _currentDispatcher;



        public CourseUseView(CourseUseViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            this.DataContext = _viewModel;
            _currentDispatcher = Dispatcher.CurrentDispatcher;
            notesGrid.Loaded += new RoutedEventHandler(notesGrid_Loaded);
        }


        void notesGrid_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.NotesLoadedCommand.Execute(null);
        }

        #region UI Event Handlers

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Adding 1 to make the row count start at 1 instead of 0
            // as pointed out by daub815
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// the vertical splitter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //_viewModel.LastEmbeddedVideoWidth = mainGrid.ColumnDefinitions[0].Width.Value;
        }

        /// <summary>
        /// the horizontal splitter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSplitter_DragCompleted_1(object sender, DragCompletedEventArgs e)
        {
            //_viewModel.LastEmbeddedVideoHeight = leftGridColumn.RowDefinitions[0].Height.Value;
        }

        #endregion

        public void HandleWindowKeypress(object sender, KeyEventArgs e)
        {
            var matches = HotkeyManager.CheckHotkey(e);

            if (null != matches && matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    switch (match.Function)
                    {
                        case HotkeyFunction.NoteEditBegin:
                            ThreadHelper.ExecuteAsyncUI(_currentDispatcher, delegate
                            {
                                if (notesGrid.IsEditing)
                                {
                                    notesGrid.CommitEdit();
                                }

                                notesGrid.BeginEditNewNote();
                            });
                            e.Handled = true;
                            break;
                        case HotkeyFunction.NoteEditCommit:
                            //commit the current edit, and hide the controls
                            if (notesGrid.IsEditing)
                            {
                                notesGrid.CommitEdit();
                            }
                            e.Handled = true;
                            break;
                        case HotkeyFunction.NoteEditCancel:
                            //cancel the current edit, and hide the controls
                            if (notesGrid.IsEditing)
                            {
                                notesGrid.CancelEdit();
                            }
                            e.Handled = true;
                            break;
                        case HotkeyFunction.NoteSetStartTime:
                            if (this.IsVisible)
                            {
                                Note currentNote = notesGrid.CurrentNote;
                                _viewModel.SetNoteStartTimeCommand.Execute(currentNote);
                                if (false == notesGrid.IsEditing)
                                {
                                    notesGrid.CommitEdit();
                                }
                                e.Handled = true;
                            }
                            break;
                        case HotkeyFunction.NoteDelete:
                            ThreadHelper.ExecuteAsyncUI(_currentDispatcher, delegate
                            {
                                notesGrid.DeleteSelectedNote.Execute(null);
                            });
                            e.Handled = true;
                            break;
                    }
                }
            }
        }
    }
}
