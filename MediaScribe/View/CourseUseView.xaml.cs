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
using JayDev.MediaScribe.View.Controls;

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
            HandleWindowKeypressForBothViews(sender, e, _currentDispatcher, notesGrid, _viewModel, IsVisible, (x) => { });
        }

        public static void HandleWindowKeypressForBothViews(object sender, KeyEventArgs e, Dispatcher _currentDispatcher, NotesGridControl notesGrid, CourseUseViewModel _viewModel, bool IsVisible, Action<ShowMessage> SendShowMessage)
        {
            var matches = HotkeyManager.CheckHotkey(e);

            if (null != matches && matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    switch (match.Function)
                    {
                        case HotkeyFunction.NoteEditBegin:
                            if (notesGrid.IsEditing)
                            {
                                notesGrid.CommitEdit();
                            }

                            SendShowMessage(new ShowMessage() { Show = true, Source = ShowSource.Hotkey });

                            notesGrid.BeginEditNewNote();
                            e.Handled = true;
                            break;
                        case HotkeyFunction.NoteEditCommit:
                            //commit the current edit, and hide the controls
                            if (notesGrid.IsEditing)
                            {
                                notesGrid.CommitEdit();
                            }
                            SendShowMessage(new ShowMessage() { Show = false, Source = ShowSource.Hotkey });
                            e.Handled = true;
                            break;
                        case HotkeyFunction.NoteEditCancel:
                            //cancel the current edit, and hide the controls
                            if (notesGrid.IsEditing)
                            {
                                notesGrid.CancelEdit();
                            }
                            SendShowMessage(new ShowMessage() { Show = false, Source = ShowSource.Hotkey });
                            e.Handled = true;
                            break;
                        case HotkeyFunction.NoteSetStartTime:
                            if (IsVisible)
                            {
                                Note currentNote = notesGrid.CurrentNote;
                                bool isDirty = currentNote.IsDirty;
                                if (false == isDirty)
                                {
                                    currentNote.BeginEdit();
                                }
                                _viewModel.SetNoteStartTimeCommand.Execute(currentNote);
                                if (false == isDirty)
                                {
                                    currentNote.EndEdit();
                                }

                                e.Handled = true;
                            }
                            break;
                        case HotkeyFunction.NoteRating:
                            if (IsVisible)
                            {
                                Note currentNote = notesGrid.CurrentNote;
                                bool isDirty = currentNote.IsDirty;
                                if (false == isDirty)
                                {
                                    currentNote.BeginEdit();
                                }
                                if (currentNote.Rating == match.Rating)
                                {
                                    currentNote.Rating = null;
                                }
                                else
                                {
                                    currentNote.Rating = match.Rating;
                                }
                                if (false == isDirty)
                                {
                                    currentNote.EndEdit();
                                }

                                e.Handled = true;
                            }
                            break;
                        case HotkeyFunction.NoteDelete:
                            if (IsVisible)
                            {
                                Note currentNote = notesGrid.CurrentNote;
                                notesGrid.DeleteSelectedNote.Execute(currentNote);
                                e.Handled = true;
                            }
                            break;
                    }
                }
            }
        }
    }
}
