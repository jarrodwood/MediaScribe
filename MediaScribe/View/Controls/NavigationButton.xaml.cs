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
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// Interaction logic for ImageButton.xaml
    /// </summary>
    public partial class NavigationButton : UserControl
    {

        public bool IsContextMenuButton
        {
            get { return (bool)GetValue(IsContextMenuButtonProperty); }
            set { SetValue(IsContextMenuButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsContextMenuButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsContextMenuButtonProperty =
            DependencyProperty.Register("IsContextMenuButton", typeof(bool), typeof(NavigationButton), new UIPropertyMetadata());

        

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(NavigationButton), new UIPropertyMetadata(null));




        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(NavigationButton), new UIPropertyMetadata(null));

        
        

        public ImageSource MySource
        {
            get { return (ImageSource)GetValue(MySourceProperty); }
            set { SetValue(MySourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MySource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MySourceProperty =
            DependencyProperty.Register("MySource", typeof(ImageSource), typeof(NavigationButton), new UIPropertyMetadata(null));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NavigationButton), new UIPropertyMetadata(null));

        
        
        //public ImageSource MySource { get; set; }

        public NavigationButton()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(NavigationButton_Loaded);
        }

        void NavigationButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (true == IsContextMenuButton)
            {
                var navButton = (NavigationButton)sender;
                navButton.myButton.Click += new RoutedEventHandler((sender2, e2) =>
                {
                    navButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    navButton.ContextMenu.PlacementTarget = navButton;
                    navButton.ContextMenu.IsOpen = true;
                });
            }
        }
    }
}
