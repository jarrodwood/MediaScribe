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
using GalaSoft.MvvmLight.Command;
using JayDev.Notemaker.Common;

namespace JayDev.Notemaker.View.Controls
{
    /// <summary>
    /// Interaction logic for MediaControlsControl.xaml
    /// </summary>
    public partial class MediaControlsControl : UserControl
    {
        #region CurrentPlayTime

        public TimeSpan CurrentPlayTime
        {
            get { return (TimeSpan)GetValue(CurrentPlayTimeProperty); }
            set { SetValue(CurrentPlayTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPlayTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPlayTimeProperty =
            DependencyProperty.Register("CurrentPlayTime", typeof(TimeSpan), typeof(MediaControlsControl), new UIPropertyMetadata(new TimeSpan()));

        #endregion

        #region MaxPlayTime

        public TimeSpan MaxPlayTime
        {
            get { return (TimeSpan)GetValue(MaxPlayTimeProperty); }
            set { SetValue(MaxPlayTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxPlayTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxPlayTimeProperty =
            DependencyProperty.Register("MaxPlayTime", typeof(TimeSpan), typeof(MediaControlsControl), new UIPropertyMetadata(new TimeSpan()));

        #endregion

        #region PlayPauseCommand

        public static readonly DependencyProperty PlayPauseCommandProperty =
            DependencyProperty.Register("PlayPauseCommand", typeof(ICommand), typeof(MediaControlsControl));

        /// <summary>
        /// Gets the PlayCommand.
        /// </summary>
        public ICommand PlayPauseCommand
        {
            get { return (ICommand)GetValue(PlayPauseCommandProperty); }
            set { SetValue(PlayPauseCommandProperty, value); }
        }

        #endregion

        #region StopCommand

        public static readonly DependencyProperty StopCommandProperty =
            DependencyProperty.Register("StopCommand", typeof(ICommand), typeof(MediaControlsControl));

        /// <summary>
        /// Gets the PlayCommand.
        /// </summary>
        public ICommand StopCommand
        {
            get { return (ICommand)GetValue(StopCommandProperty); }
            set { SetValue(StopCommandProperty, value); }
        }

        #endregion



        #region SeekToCommand

        public static readonly DependencyProperty SeekToCommandProperty =
            DependencyProperty.Register("SeekToCommand", typeof(ICommand), typeof(MediaControlsControl));

        /// <summary>
        /// Gets the SeekToCommand.
        /// </summary>
        public ICommand SeekToCommand
        {
            get { return (ICommand)GetValue(SeekToCommandProperty); }
            set { SetValue(SeekToCommandProperty, value); }
        }

        #endregion

        
        

        public MediaControlsControl()
        {
            InitializeComponent();
        }
        TimeSpanToSecondsConverter converter = new TimeSpanToSecondsConverter();

        private void timeSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var seekTo = ((Slider)sender).Value;
        
            TimeSpan result = (TimeSpan)converter.ConvertBack(seekTo, typeof(TimeSpan), null, null);
            SeekToCommand.Execute(result);
        }


    }
}
