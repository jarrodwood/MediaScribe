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
using JayDev.Notemaker.ViewModel;
using System.Windows.Threading;
using JayDev.Notemaker.Common;

namespace JayDev.Notemaker.View
{
    /// <summary>
    /// Interaction logic for CourseUseView.xaml
    /// </summary>
    public partial class CourseUseView : UserControl
    {
        private CourseUseViewModel _viewModel;



        //public ICommand TrackDataGridDoubleClick
        //{
        //    get { return (ICommand)GetValue(TrackDataGridDoubleClickProperty); }
        //    set { SetValue(TrackDataGridDoubleClickProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for TrackDataGridDoubleClick.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty TrackDataGridDoubleClickProperty =
        //    DependencyProperty.RegisterAttached("DataGridDoubleClickCommand", typeof(ICommand), typeof(CourseUseView), new PropertyMetadata(new PropertyChangedCallback(AttachOrRemoveDataGridDoubleClickEvent));

        

        public CourseUseView(CourseUseViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            this.DataContext = _viewModel;

            var message = new ReusableControlMessage(videoControl, ReusableControlType.VideoControl);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(message, MessageType.RegisterReusableControl);

            message = new ReusableControlMessage(notesGrid, ReusableControlType.NotesGridControl);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(message, MessageType.RegisterReusableControl);

            message = new ReusableControlMessage(mediaControls, ReusableControlType.NotesGridControl);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(message, MessageType.RegisterReusableControl);
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
            _viewModel.LastEmbeddedVideoWidth = mainGrid.ColumnDefinitions[0].Width.Value;
        }

        /// <summary>
        /// the horizontal splitter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSplitter_DragCompleted_1(object sender, DragCompletedEventArgs e)
        {
            _viewModel.LastEmbeddedVideoHeight = leftGridColumn.RowDefinitions[0].Height.Value;
        }

        #endregion

        
        //public static void AttachOrRemoveDataGridDoubleClickEvent(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        //{
        //    DataGrid dataGrid = obj as DataGrid;
        //    if ( dataGrid != null )
        //    {
        //        ICommand cmd = (ICommand) args.NewValue;

        //        if ( args.OldValue == null && args.NewValue != null )
        //        {
        //        dataGrid.MouseDoubleClick += ExecuteDataGridDoubleClick;
        //        }
        //        else if ( args.OldValue != null && args.NewValue == null )
        //        {
        //        dataGrid.MouseDoubleClick -= ExecuteDataGridDoubleClick;
        //        }
        //    }
        //}


        //private void trackGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    DependencyObject obj = sender as DependencyObject;
        //    ICommand cmd = (ICommand)obj.GetValue(TrackDataGridDoubleClickProperty);
        //    if (cmd != null)
        //    {
        //        if (cmd.CanExecute(obj))
        //        {
        //            cmd.Execute(obj);
        //        }
        //    }
        //}

    }
}
