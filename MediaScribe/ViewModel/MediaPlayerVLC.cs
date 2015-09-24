using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.MediaScribe.Common;
using System.Windows;
using GalaSoft.MvvmLight;
using System.Timers;
using System.Diagnostics;
using LibVLC.NET;
using MediaPlayer = LibVLC.NET.MediaPlayer;
using LibVLC.NET.Presentation;

namespace JayDev.MediaScribe.ViewModel
{
    public class MediaPlayerVLC : ViewModelBase
    {
        private double _currentVolume = Constants.DEFAULT_VOLUME;

        public delegate void TrackFinishedEventHandler(object sender, EventArgs e);

        public event TrackFinishedEventHandler TrackFinished;

        #region CurrentTrack

        /// <summary>
        /// The <see cref="CurrentTrack" /> property's name.
        /// </summary>
        public const string CurrentTrackPropertyName = "CurrentTrack";

        private Track _currentTrack = null;

        /// <summary>
        /// Sets and gets the CurrentTrack property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Track CurrentTrack
        {
            get
            {
                return _currentTrack;
            }

            set
            {
                if (_currentTrack == value)
                {
                    return;
                }

                _currentTrack = value;
                RaisePropertyChanged(CurrentTrackPropertyName);
            }
        }

        #endregion

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
        public MediaPlayerVLC()
            : base(null)
        {
            //we only initialize mplayer once.
            if (null == MediaElement.SingletonPlayer)
            {
                var m_Library = LibVLCLibrary.Load(null);
                MediaElement.SingletonPlayer = new LibVLC.NET.MediaPlayer(m_Library, null);
                MediaElement.SingletonPlayer.Event += SingletonPlayer_Event;
            }
        }

        void SingletonPlayer_Event(object sender, MediaPlayerEventArgs e)
        {
            LibVLC.NET.MediaPlayer player = sender as LibVLC.NET.MediaPlayer;
         
            Action action = null;

            switch (e.Event)
            {
                case MediaPlayerEvent.TimeChanged:
                    //action = media_element.MediaPlayer_TimeChanged;
                    this.CurrentPlayPosition = player.Time;
                    break;

                case MediaPlayerEvent.MediaChanged:
                    //action = media_element.MediaPlayer_MediaChanged;
                    break;

                case MediaPlayerEvent.Opening:
                    this.PlayStatus = Common.PlayStatus.Playing;
                    break;

                case MediaPlayerEvent.Playing:
                    this.PlayStatus = Common.PlayStatus.Playing;
                    break;

                case MediaPlayerEvent.Paused:
                    this.PlayStatus = Common.PlayStatus.Paused;
                    break;

                case MediaPlayerEvent.Stopped:
                    this.PlayStatus = Common.PlayStatus.Stopped;
                    break;

                case MediaPlayerEvent.EndReached:
                    this.PlayStatus = Common.PlayStatus.Stopped;
                    ThreadHelper.ExecuteAsyncUI(_uiDispatcher, delegate
                    {
                        TrackFinished.Invoke(null, null);
                    });
                    break;

                case MediaPlayerEvent.EncounteredError:
                    //action = media_element.MediaPlayer_EncounteredError;
                    break;
            }

            if (action != null)
                action.Invoke();
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

        public void ToggleMute()
        {
            if (null != MediaElement.SingletonPlayer)
            {
                //TODO - do we actually use this?
                //MediaElement.SingletonPlayer.Mute();
            }
        }

        public void TogglePause()
        {
            if (null != MediaElement.SingletonPlayer)
            {
                if (PlayStatus == Common.PlayStatus.Stopped)
                {
                    PlayFile(_allTracks, _currentTrackIndex, CurrentPlayPosition, PlayAction.Play);
                }
                else
                {
                    MediaElement.SingletonPlayer.Pause();
                }
            }
        }

        public void Seek(int time)
        {
            var timespan = new TimeSpan(0, 0, time);
            MediaElement.SingletonPlayer.Time = timespan;
            //set the play position, to instantly update the trackbar... instead of waiting for the timer to tick.
            //TODO: the timer still makes it jump back for some reason - mplayer's returning the old time for a second or two ?
            this.CurrentPlayPosition = timespan;
        }

        public void SeekRelative(int relativeTime)
        {
            TimeSpan timespan;
            if (relativeTime >= 0)
            {
                timespan = this.CurrentPlayPosition.Add(new TimeSpan(0, 0, relativeTime));
            }
            else
            {
                timespan = this.CurrentPlayPosition.Subtract(new TimeSpan(0, 0, Math.Abs(relativeTime)));
            }
            
            MediaElement.SingletonPlayer.Time = timespan;
            Debug.WriteLine(DateTime.Now.ToLongTimeString() + " Seek relative, {1} seconds. recorded position: {0}", CurrentPlayPosition.ToString(), relativeTime);

            //set the play position, to instantly update the trackbar... instead of waiting for the timer to tick.
            //TODO: the timer still makes it jump back for some reason - mplayer's returning the old time for a second or two ?
            this.CurrentPlayPosition = timespan;
        }

        public void SetSpeed(double speedPercent)
        {
            if (null != MediaElement.SingletonPlayer)
            {
                MediaElement.SingletonPlayer.Rate = (float)speedPercent;
            }
        }

        public void Volume(double volumePercent)
        {
            _currentVolume = volumePercent;
            if (null != MediaElement.SingletonPlayer)
            {
                MediaElement.SingletonPlayer.Volume = (int)volumePercent;
            }
        }

        public void Stop()
        {
            if (null != MediaElement.SingletonPlayer)
            {
                MediaElement.SingletonPlayer.Stop();
                MediaElement.SingletonPlayer.Volume = (int)_currentVolume;
                this.CurrentPlayPosition = TimeSpan.Zero;
            }
        }

        Track _loadedTrack = null;
        int _currentTrackIndex;
        List<Track> _allTracks = null;
        public enum PlayAction { DontPlay, Play, MaintainStatus }

        public void PlayFile(List<Track> tracks, int trackIndex, TimeSpan time, PlayAction action)
        {
            _allTracks = tracks;
            Track track = tracks[trackIndex];
            this.CurrentTrack = track;
            this._currentTrackIndex = trackIndex;
            this.CurrentPlayPosition = time;

            bool isChangingFile = _loadedTrack != CurrentTrack;
            PlayStatus playStatusAtMethodCall = PlayStatus;
            //if (track != _loadedTrack)
            //{
            //    Stop();
            //}

            if (action == PlayAction.Play || (action == PlayAction.MaintainStatus && playStatusAtMethodCall == Common.PlayStatus.Playing))
            {
                if (track != _loadedTrack
                    || playStatusAtMethodCall == Common.PlayStatus.Stopped)
                {
                    //libvlc.net quirk - need to "stop" first before it'll actually play.
                    MediaElement.SingletonPlayer.Stop();
                    MediaElement.SingletonPlayer.Location = new Uri(track.FilePath);
                    _loadedTrack = track;
                    MediaElement.SingletonPlayer.Play();
                }

                //When we load a file, mplayer resets the volume. If we have the non-default volume, ensure it's set. This isn't perfect
                //since it'll start the file at default volume for a splitsecond.
                //TODO: switch volume control from mplayer's native controls to Windows's volume settings for the application.
                if (_currentVolume != Constants.DEFAULT_VOLUME)
                {
                    Volume(_currentVolume);
                }

                //NOTE: if we're on the SAME FILE, and we want to seek to the start... we should be able to. if we're changing file... it
                //will start off at the beginning, so seeking will just make it stutter.
                if ((isChangingFile && time != TimeSpan.Zero)
                    || (false == isChangingFile))
                {
                    Seek((int)time.TotalSeconds);
                }
            }


            //set the play position, to instantly update the trackbar... instead of waiting for the timer to tick.
            //TODO: the timer still makes it jump back for some reason - mplayer's returning the old time for a second or two ?
            this.CurrentPlayPosition = time;
        }
    }
}
