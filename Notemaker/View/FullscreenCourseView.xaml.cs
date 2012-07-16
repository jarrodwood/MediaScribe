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
using JayDev.Notemaker.ViewModel;
using System.Timers;
using JayDev.Notemaker.View.Controls;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using JayDev.Notemaker.Common;

namespace JayDev.Notemaker.View
{
    /// <summary>
    /// Interaction logic for FullscreenCourseView.xaml
    /// </summary>
    public partial class FullscreenCourseView : UserControl
    {
        private CourseUseViewModel _viewModel;
        private readonly Dispatcher _currentDispatcher;

        public FullscreenCourseView(CourseUseViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            this.DataContext = _viewModel;
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            Messenger.Default.Register<string>(this, 12345, (message) => HandleMessage(message));

            this.KeyDown += new KeyEventHandler(FullscreenCourseView_KeyDown);
        }

        public void HandleKeypress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.NumPad7:
                    ThreadHelper.ExecuteAsyncUI(_currentDispatcher, delegate
                    {
                        if (notesGrid.IsEditing)
                        {
                            notesGrid.CommitEdit();
                        }

                        HandleMessage("show");

                        notesGrid.BeginEditNewNote();
                    });
                    e.Handled = true;
                    break;
                case Key.NumPad8:
                    //commit the current edit, and hide the controls
                    if (notesGrid.IsEditing)
                    {
                        notesGrid.CommitEdit();
                    }
                    HandleMessage("hide");
                    e.Handled = true;
                    break;
                case Key.NumPad9:
                    //cancel the current edit, and hide the controls
                    if (notesGrid.IsEditing)
                    {
                        notesGrid.CancelEdit();
                    }
                    HandleMessage("hide");
                    e.Handled = true;
                    break;
                case Key.Divide:
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
                case Key.Multiply:
                    if (this.IsVisible)
                    {
                        Note currentNote = notesGrid.CurrentNote;
                        _viewModel.SetNoteEndTimeCommand.Execute(currentNote);
                        if (false == notesGrid.IsEditing)
                        {
                            notesGrid.CommitEdit();
                        }
                        e.Handled = true;
                    }
                    break;
            }
        }

        public void HideControls()
        {
        }

        public void ShowControls()
        {
        }

        void FullscreenCourseView_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void HandleMessage(string message)
        {
            if (message == "show")
            {
                ThreadHelper.ExecuteSyncUI(_currentDispatcher, delegate
                {
                    this.mediaControls.Visibility = System.Windows.Visibility.Visible;
                    this.notesGrid.Visibility = System.Windows.Visibility.Visible;
                    Mouse.OverrideCursor = null;
                });
            }
            else
            {
                ThreadHelper.ExecuteSyncUI(_currentDispatcher, delegate
                {
                    this.mediaControls.Visibility = System.Windows.Visibility.Collapsed;
                    this.notesGrid.Visibility = System.Windows.Visibility.Collapsed;
                    Mouse.OverrideCursor = Cursors.None;
                });
            }
        }

    }
}
