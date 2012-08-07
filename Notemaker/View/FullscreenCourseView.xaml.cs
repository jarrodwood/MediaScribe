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
using JayDev.MediaScribe.ViewModel;
using System.Timers;
using JayDev.MediaScribe.View.Controls;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using JayDev.MediaScribe.Common;
using JayDev.MediaScribe.Core;
using MediaScribe.Common;

namespace JayDev.MediaScribe.View
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
        }

        public void HandleWindowKeypress(object sender, KeyEventArgs e)
        {
            CourseUseView.HandleWindowKeypressForBothViews(sender, e, _currentDispatcher, notesGrid, _viewModel, this.IsVisible, HandleMessage);
        }

        public void HideControls()
        {
        }

        public void ShowControls()
        {
        }

        void FullscreenCourseView_KeyDown(object sender, KeyEventArgs e)
        {
            Messenger.Default.Send(new NavigateArgs(NavigateMessage.ToggleFullscreen), MessageType.Navigate);
        }

        private void HandleMessage(string message)
        {
            Debug.WriteLine("Handling message: " + message);
            if (message == "show")
            {
                if (this.notesGrid.Visibility != Visibility.Visible)
                {
                    ThreadHelper.ExecuteSyncUI(_currentDispatcher, delegate
                    {
                        this.mediaControls.Visibility = System.Windows.Visibility.Visible;
                        this.notesGrid.Visibility = System.Windows.Visibility.Visible;
                        Mouse.OverrideCursor = null;
                        Debug.WriteLine(DateTime.Now.ToLongTimeString() + " override - show");
                    });
                }
            }
            else
            {
                if (this.notesGrid.Visibility != System.Windows.Visibility.Collapsed)
                {
                    ThreadHelper.ExecuteSyncUI(_currentDispatcher, delegate
                    {
                        this.mediaControls.Visibility = System.Windows.Visibility.Collapsed;
                        this.notesGrid.Visibility = System.Windows.Visibility.Collapsed;
                        Mouse.OverrideCursor = Cursors.None;
                        Debug.WriteLine(DateTime.Now.ToLongTimeString() + " override - none");
                    });
                }
            }
        }

    }
}
