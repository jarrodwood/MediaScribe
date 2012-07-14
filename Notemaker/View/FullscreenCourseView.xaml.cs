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
        }

        private void HandleMessage(string message)
        {
            if (message == "show")
            {
                Action dispatchAction = () =>
                {
                    this.mediaControls.Visibility = System.Windows.Visibility.Visible;
                    this.notesGrid.Visibility = System.Windows.Visibility.Visible;
                    Mouse.OverrideCursor = null;
                };
                _currentDispatcher.BeginInvoke(dispatchAction);
            }
            else
            {
                Action dispatchAction = () =>
                {
                    this.mediaControls.Visibility = System.Windows.Visibility.Collapsed;
                    this.notesGrid.Visibility = System.Windows.Visibility.Collapsed;
                    Mouse.OverrideCursor = Cursors.None;
                };
                _currentDispatcher.BeginInvoke(dispatchAction);
            }
        }

    }
}
