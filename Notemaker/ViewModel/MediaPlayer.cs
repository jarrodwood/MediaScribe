using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMPlayerCommon;
using JayDev.Notemaker.Common;
using System.Windows;
using GalaSoft.MvvmLight;
using System.Timers;

namespace JayDev.Notemaker.ViewModel
{
    public class MediaPlayer : ViewModelBase
    {
        private Discover _videoSettings;
        private MPlayer _play;
        private string _filePath;
        private bool disposed = false; // to detect redundant calls
        private Timer _playPositionTimer;

        #region CurrentPlayPosition

        /// <summary>
        /// The <see cref="CurrentPlayPosition" /> property's name.
        /// </summary>
        public const string CurrentPlayPositionPropertyName = "CurrentPlayPosition";

        private TimeSpan _currentPlayPosition;

        /// <summary>
        /// Sets and gets the CurrentPlayPosition property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public TimeSpan CurrentPlayPosition
        {
            get
            {
                return _currentPlayPosition;
            }

            set
            {
                if (_currentPlayPosition == value)
                {
                    return;
                }

                _currentPlayPosition = value;
                RaisePropertyChanged(CurrentPlayPositionPropertyName);
            }
        }

        #endregion CurrentPlayPosition
		

        public PlayStatus PlayingStatus
        {
            get
            {
                if (this._play == null || this._play.CurrentStatus == MediaStatus.Stopped)
                    return PlayStatus.Stopped;
                if (this._play.CurrentStatus == MediaStatus.Playing)
                    return PlayStatus.Playing;
                if (this._play.CurrentStatus == MediaStatus.Paused)
                    return PlayStatus.Paused;
                throw new Exception("error: unexpected playing status?");
            }
        }

        public void TogglePause()
        {
            if (null != this._play)
            {
                //note the current playing status, because it takes a little while to update after we tell mplayer to pause...
                PlayStatus now = PlayingStatus;
                if (now == PlayStatus.Playing)
                {
                    _playPositionTimer.Stop();
                }

                this._play.Pause();

                if (now == PlayStatus.Paused || now == PlayStatus.Stopped)
                {
                    _playPositionTimer.Start();
                }
            }
        }
        public void Seek(int time)
        {
            this._play.Seek(time, LibMPlayerCommon.Seek.Absolute);
            //set the play position, to instantly update the trackbar... instead of waiting for the timer to tick.
            //TODO: the timer still makes it jump back for some reason - mplayer's returning the old time for a second or two ?
            this.CurrentPlayPosition = new TimeSpan(0, 0, time);
        }
        public void SeekRelative(int relativeTime)
        {
            this._play.Seek(relativeTime, LibMPlayerCommon.Seek.Relative);
        }

        public void Volume(double volumePercent)
        {
            if (null != this._play)
            {
                this._play.Volume((int)volumePercent, true);
            }
        }

        public void Stop()
        {
            if (null != this._play)
            {
                _playPositionTimer.Stop();
                this._play.Stop();

            }
        }

        public void Play(string filePath, IntPtr handle)
        {
            this._filePath = filePath;
            if (this._filePath == String.Empty || this._filePath == null)
            {
                MessageBox.Show("You must select a video file.", "Select a file", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _videoSettings = new Discover(this._filePath);


            MplayerBackends backend;
            System.PlatformID runningPlatform = System.Environment.OSVersion.Platform;
            if (runningPlatform == System.PlatformID.Unix)
            {
                backend = MplayerBackends.GL2;
            }
            else if (runningPlatform == PlatformID.MacOSX)
            {
                backend = MplayerBackends.OpenGL;
            }
            else
            {
                backend = MplayerBackends.Direct3D;
            }

            this._play = new MPlayer((int)handle, backend);
            this._play.Play(this._filePath);
            //TODO: make the following work in its new home
            //SetPanelSizeForAspectRatio();

            _playPositionTimer = new Timer();
            _playPositionTimer.Interval = 500;
            _playPositionTimer.Elapsed += new ElapsedEventHandler(_playPositionTimer_Elapsed);
            _playPositionTimer.Start();
        }

        void _playPositionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentPlayPosition = new TimeSpan(0,0,_play.CurrentPosition());
        }
    }
}
