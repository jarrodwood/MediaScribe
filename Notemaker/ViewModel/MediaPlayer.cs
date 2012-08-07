using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMPlayerCommon;
using JayDev.Notemaker.Common;
using System.Windows;
using GalaSoft.MvvmLight;
using System.Timers;
using System.Diagnostics;

namespace JayDev.Notemaker.ViewModel
{
    public class MediaPlayer : ViewModelBase
    {
        /// <summary>
        /// The maximum number of seconds difference between the time WE know we should be at, and the time that mplayer returns. If the
        /// difference is bigger than this number, we will stop updating the currentPlayPosition until it gets within this zone.
        /// </summary>
        private const int MAX_TIME_DIFFERENCE_SECONDS = 10;
        private const int PLAY_POSITION_UPDATE_INTERVAL_MILLISECONDS = 500;
        private static MPlayer _play;
        private string _filePath;
        private Timer _playPositionTimer = new Timer();
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

        /// <summary>
        /// Creates the wrapper for the mplayer library.
        /// </summary>
        /// <param name="handle">pointer to the panel where the video needs to be displayed</param>
        public MediaPlayer(IntPtr handle)
        {
            //we only initialize mplayer once.
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
            }

            //configure the timer that will check the current position twice every second, until the end of time.
            _playPositionTimer = new Timer();
            _playPositionTimer.Interval = PLAY_POSITION_UPDATE_INTERVAL_MILLISECONDS;
            _playPositionTimer.Elapsed += new ElapsedEventHandler(_playPositionTimer_Elapsed);
        }

        /// <summary>
        /// The <see cref="PlayStatus" /> property's name.
        /// </summary>
        public const string PlayStatusPropertyName = "PlayStatus";

        private PlayStatus _playStatus = PlayStatus.Stopped;

        /// <summary>
        /// Sets and gets the PlayStatus property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PlayStatus PlayStatus
        {
            get
            {
                return _playStatus;
            }

            set
            {
                if (_playStatus == value)
                {
                    return;
                }

                _playStatus = value;
                RaisePropertyChanged(PlayStatusPropertyName);
            }
        }

        void SetPlayStatus()
        {
            if (_play == null || _play.CurrentStatus == MediaStatus.Stopped)
                PlayStatus = PlayStatus.Stopped;
            if (_play.CurrentStatus == MediaStatus.Playing)
                PlayStatus = PlayStatus.Playing;
            if (_play.CurrentStatus == MediaStatus.Paused)
                PlayStatus = PlayStatus.Paused;
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
                if (PlayStatus == Common.PlayStatus.Stopped)
                {
                    PlayFile2(currentFilePath, CurrentPlayPosition, PlayAction.Play);
                }
                else
                {
                    _play.Pause();
                    SetPlayStatus();
                }
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
                SetPlayStatus();
                this.CurrentPlayPosition = TimeSpan.Zero;
                _playPositionTimer.Stop();
            }
        }

        string loadedFilePath = null;
        string currentFilePath = null;
        public enum PlayAction { DontPlay, Play, MaintainStatus }
        public void PlayFile2(string filePath, TimeSpan time, PlayAction action)
        {
            this.currentFilePath = filePath;
            this.CurrentPlayPosition = time;

            bool isChangingFile = loadedFilePath != currentFilePath;
            PlayStatus playStatusAtMethodCall = PlayStatus;
            if (filePath != loadedFilePath)
            {
                Stop();
            }

            if (action == PlayAction.Play || (action == PlayAction.MaintainStatus && playStatusAtMethodCall == Common.PlayStatus.Playing))
            {
                //'loadfile' is a bad method name -- it actually PLAYS the file, too.
                if (filePath != loadedFilePath
                    || playStatusAtMethodCall == Common.PlayStatus.Stopped)
                {
                    _play.LoadFile(filePath);
                    loadedFilePath = filePath;
                }


                //NOTE: if we're on the SAME FILE, and we want to seek to the start... we should be able to. if we're changing file... it
                //will start off at the beginning, so seeking will just make it stutter.
                if ((isChangingFile && time != TimeSpan.Zero)
                    || (false == isChangingFile))
                {
                    _play.Seek(Convert.ToInt32(time.TotalSeconds), LibMPlayerCommon.Seek.Absolute);
                }

                _playPositionTimer.Start();
            }


            SetPlayStatus();
            //set the play position, to instantly update the trackbar... instead of waiting for the timer to tick.
            //TODO: the timer still makes it jump back for some reason - mplayer's returning the old time for a second or two ?
            this.CurrentPlayPosition = time;
        }


        void _playPositionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_play.CurrentStatus == MediaStatus.Playing)
            {
                TimeSpan newTime = new TimeSpan(0, 0, _play.CurrentPosition());
                Debug.WriteLine(DateTime.Now.ToLongTimeString() + " Ticker. recorded position: {0}, mplayer position: {1}. Within scope: {2}", CurrentPlayPosition.ToString(), newTime.ToString(), Math.Abs(newTime.TotalSeconds - CurrentPlayPosition.TotalSeconds) <= MAX_TIME_DIFFERENCE_SECONDS);
                if (Math.Abs(newTime.TotalSeconds - CurrentPlayPosition.TotalSeconds) <= MAX_TIME_DIFFERENCE_SECONDS)
                {
                    CurrentPlayPosition = newTime;
                }
            }
        }
    }
}
