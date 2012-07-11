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
using LibMPlayerCommon;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Messaging;
using JayDev.Notemaker.ViewModel;

namespace JayDev.Notemaker.View
{
    /// <summary>
    /// Interaction logic for CreateCourseWindow.xaml
    /// </summary>
    public partial class CreateCourseWindow : Window
    {
        Course course = new Course();
        CreateCourseViewModel vm;

        public CreateCourseWindow()
        {
            InitializeComponent();
            vm = new CreateCourseViewModel();
            this.DataContext = vm;

            

        }



        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Adding 1 to make the row count start at 1 instead of 0
            // as pointed out by daub815
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            vm.AddTracks();
        }

        private void saveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
