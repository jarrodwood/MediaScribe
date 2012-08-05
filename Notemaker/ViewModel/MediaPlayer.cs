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
        private static MPlayer _play;
        private string _filePath;
        private bool disposed = false; // to detect redundant calls
        private static Timer _playPositionTimer = new Timer();
        private IntPtr _handle;

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


        public MediaPlayer(IntPtr handle)
        {
            if (null == _play)
            {
                this._handle = handle;

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

                _play = new MPlayer((int)handle, backend);
                _play.Init();

                _playPositionTimer = new Timer();
                _playPositionTimer.Interval = 500;
                _playPositionTimer.Elapsed += new ElapsedEventHandler(_playPositionTimer_Elapsed);
                _playPositionTimer.Start();
            }
        }

        public PlayStatus PlayingStatus
        {
            get
            {
                if (_play == null || _play.CurrentStatus == MediaStatus.Stopped)
                    return PlayStatus.Stopped;
                if (_play.CurrentStatus == MediaStatus.Playing)
                    return PlayStatus.Playing;
                if (_play.CurrentStatus == MediaStatus.Paused)
                    return PlayStatus.Paused;
                throw new Exception("error: unexpected playing status?");
            }
        }

        public void ToggleMute()
        {
            if (null != _play)
            {
                _play.Mute();
            }
        }

        public void TogglePause()
        {
            if (null != _play)
            {

                _play.Pause();
            }
        }

        public void Seek(int time)
        {
            _play.Seek(time, LibMPlayerCommon.Seek.Absolute);
            //set the play position, to instantly update the trackbar... instead of waiting for the timer to tick.
            //TODO: the timer still makes it jump back for some reason - mplayer's returning the old time for a second or two ?
            this.CurrentPlayPosition = new TimeSpan(0, 0, time);
        }
        public void SeekRelative(int relativeTime)
        {
            _play.Seek(relativeTime, LibMPlayerCommon.Seek.Relative);
            if (relativeTime >= 0)
            {
                this.CurrentPlayPosition = this.CurrentPlayPosition.Add(new TimeSpan(0, 0, relativeTime));
            }
            else
            {
                this.CurrentPlayPosition = this.CurrentPlayPosition.Subtract(new TimeSpan(0, 0, Math.Abs(relativeTime)));
            }
        }

        public void Volume(double volumePercent)
        {
            if (null != _play)
            {
                _play.Volume((int)volumePercent, true);
            }
        }

        public void Stop()
        {
            if (null != _play)
            {
                _play.Stop();

            }
        }

        string currentFilePath = null;
        public void PlayFile2(string filePath, TimeSpan time, bool instantPause)
        {
            this.CurrentPlayPosition = time;
            if (instantPause)
                return;

            if (filePath != currentFilePath)
            {
                _play.Stop();
                _play.LoadFile(filePath);
                currentFilePath = filePath;
            }

            if (time != TimeSpan.Zero)
            {
                _play.Seek(Convert.ToInt32(time.TotalSeconds), LibMPlayerCommon.Seek.Absolute);
                //set the play position, to instantly update the trackbar... instead of waiting for the timer to tick.
                //TODO: the timer still makes it jump back for some reason - mplayer's returning the old time for a second or two ?
                this.CurrentPlayPosition = time;
            }

        }


        void _playPositionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_play.CurrentStatus == MediaStatus.Playing)
            {
                TimeSpan newTime = new TimeSpan(0, 0, _play.CurrentPosition());
                if (Math.Abs(newTime.TotalSeconds - CurrentPlayPosition.TotalSeconds) <= 5)
                {
                    CurrentPlayPosition = newTime;
                }
            }

            System.Diagnostics.Debug.WriteLine("ELAPZED " + DateTime.Now.ToLongTimeString());
        }
    }
}
