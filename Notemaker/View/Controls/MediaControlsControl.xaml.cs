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
using System.Timers;
using System.Windows.Threading;
using JayDev.Notemaker.Converters;

namespace JayDev.Notemaker.View.Controls
{
    /// <summary>
    /// Interaction logic for MediaControlsControl.xaml
    /// </summary>
    public partial class MediaControlsControl : UserControl
    {
        Dispatcher _uiDispatcher;

        #region PlayStatus

        public PlayStatus PlayStatus
        {
            get { return (PlayStatus)GetValue(PlayStatusProperty); }
            set { SetValue(PlayStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlayStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayStatusProperty =
            DependencyProperty.Register("PlayStatus", typeof(PlayStatus), typeof(MediaControlsControl), new UIPropertyMetadata(null));

        #endregion

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

        
        #region TrackTitle

        public string TrackTitle
        {
            get { return (string)GetValue(TrackTitleProperty); }
            set { SetValue(TrackTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TrackTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrackTitleProperty =
            DependencyProperty.Register("TrackTitle", typeof(string), typeof(MediaControlsControl), new UIPropertyMetadata(null));

        #endregion


        #region Volume

        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Volume.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(MediaControlsControl), new UIPropertyMetadata(0d));

        #endregion



        public bool IsMuted
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMuted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMutedProperty =
            DependencyProperty.Register("IsMuted", typeof(bool), typeof(MediaControlsControl), new UIPropertyMetadata(null));





        public ICommand ToggleMuteCommand
        {
            get { return (ICommand)GetValue(ToggleMuteCommandProperty); }
            set { SetValue(ToggleMuteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToggleMuteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToggleMuteCommandProperty =
            DependencyProperty.Register("ToggleMuteCommand", typeof(ICommand), typeof(MediaControlsControl), new UIPropertyMetadata(null));




        public ICommand NextTrackCommand
        {
            get { return (ICommand)GetValue(NextTrackCommandProperty); }
            set { SetValue(NextTrackCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NextTrackCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NextTrackCommandProperty =
            DependencyProperty.Register("NextTrackCommand", typeof(ICommand), typeof(MediaControlsControl), new UIPropertyMetadata(null));


        public ICommand PrevTrackCommand
        {
            get { return (ICommand)GetValue(PrevTrackCommandProperty); }
            set { SetValue(PrevTrackCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrevTrackCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrevTrackCommandProperty =
            DependencyProperty.Register("PrevTrackCommand", typeof(ICommand), typeof(MediaControlsControl), new UIPropertyMetadata(null));

        


        
        

        public MediaControlsControl()
        {
            InitializeComponent();
            _uiDispatcher = Dispatcher.CurrentDispatcher;
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
