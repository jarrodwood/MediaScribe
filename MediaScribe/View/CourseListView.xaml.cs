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
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.View
{
    /// <summary>
    /// Interaction logic for CourseMaintenanceView.xaml
    /// </summary>
    public partial class CourseListView : UserControl
    {
        private CourseListViewModel _viewModel;

        public CourseListView(CourseListViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            this._viewModel = viewModel;
        }

    }
}
