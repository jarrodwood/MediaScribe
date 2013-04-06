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

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            this.MouseLeftButtonDown+=new MouseButtonEventHandler(About_MouseLeftButtonDown);
            this.DataContext = new AboutViewModel();
        }


        private void About_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


        private void UndecoratedButton_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
    }
}
