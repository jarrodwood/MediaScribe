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

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// Interaction logic for NavigationControl.xaml
    /// </summary>
    public partial class NavigationControl : UserControl
    {

        public ICommand ExportCommand
        {
            get { return (ICommand)GetValue(ExportCommandProperty); }
            set { SetValue(ExportCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExportCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExportCommandProperty =
            DependencyProperty.Register("ExportCommand", typeof(ICommand), typeof(NavigationControl));



        public ICommand NavigateCommand
        {
            get { return (ICommand)GetValue(NavigateCommandProperty); }
            set { SetValue(NavigateCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NavigateCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NavigateCommandProperty =
            DependencyProperty.Register("NavigateCommand", typeof(ICommand), typeof(NavigationControl));

        
        public NavigationControl()
        {
            InitializeComponent();
        }
    }
}
