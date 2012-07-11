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
using System.Windows.Shapes;
using JayDev.Notemaker.Model;
using JayDev.Notemaker.ViewModel;

namespace JayDev.Notemaker.View
{
    /// <summary>
    /// Interaction logic for CourseListWindow.xaml
    /// </summary>
    public partial class CourseListWindow : Window
    {
        private CourseRepository _courseRepo = null;
        public CourseListWindow()
        {
            InitializeComponent();

            var courseList = DataAccess.GetCourseList();
            courseListGrid.ItemsSource = courseList.Courses;
            _courseRepo = new CourseRepository();
        }

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            CreateCourseWindow window = new CreateCourseWindow();
            CreateCourseViewModel viewModel = new CreateCourseViewModel();
            
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            window.Owner = this.Owner;
            window.ShowDialog();
            this.IsEnabled = true;
        }
    }
}
