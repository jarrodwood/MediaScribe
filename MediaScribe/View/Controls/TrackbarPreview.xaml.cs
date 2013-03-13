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
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.View.Controls
{
    /// <summary>
    /// Interaction logic for TrackbarPreview.xaml
    /// </summary>
    public partial class TrackbarPreview : UserControl
    {

        #region CurrentPlayTime

        public TimeSpan CurrentPlayTime
        {
            get { return (TimeSpan)GetValue(CurrentPlayTimeProperty); }
            set { SetValue(CurrentPlayTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPlayTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPlayTimeProperty =
            DependencyProperty.Register("CurrentPlayTime", typeof(TimeSpan), typeof(TrackbarPreview), new UIPropertyMetadata(new TimeSpan()));

        #endregion




        public TrackbarPreview()
        {
            InitializeComponent();
        }
    }
}
