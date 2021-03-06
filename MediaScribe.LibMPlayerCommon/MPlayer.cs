﻿/*

Copyright 2010 (C) Peter Gill <peter@majorsilence.com>

This file is part of LibMPlayerCommon.

LibMPlayerCommon is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 3 of the License, or
(at your option) any later version.

LibMPlayerCommon is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Text;


using System.Diagnostics;


namespace LibMPlayerCommon
{

    /// <summary>
    /// The seek type that is used when seeking a new position in the video stream.
    /// </summary>
    public enum Seek
    {
        Relative = 0,
        Percentage = 1,
        Absolute=2
    }

    /// <summary>
    /// Status of the mplayer.exe instance.
    /// </summary>
    public enum MediaStatus
    {
        Paused,
        Playing,
        Stopped
    }

    /// <summary>
    /// The video output backend that mplayer is using.
    /// </summary>
    public enum MplayerBackends
    {
        OpenGL,
		GL, // Simple Version
		GL2, // Simple Version.  Variant of the OpenGL  video  output  driver.   
			// Supports  videos larger  than  the maximum texture size but lacks 
			// many of the ad‐vanced features and optimizations of the gl driver  
			// and  is  un‐likely to be extended further.
        Direct3D, // Windows
		DirectX, // Windows
		X11, // Linux
		VESA,
		Quartz, // Mac OS X
		CoreVideo, // Mac OS X
        SDL, // Cross Platform
		Vdpau, // Linux
		ASCII, // ASCII art video output driver that works on a text console.
		ColorASCII, // Color  ASCII  art  video output driver that works on a text console.
    	Directfb, // Linux.  Play video using the DirectFB library.
		Wii, // Linux.  Nintendo Wii/GameCube specific video output driver.
		V4l2, // Linux.   requires Linux 2.6.22+ kernel,  Video output driver for 
			// V4L2 compliant cards with built-in hardware MPEG decoder.
	
	}

    public class MPlayer : IDisposable
    {
        private int _wid;
        private bool _fullscreen;
        private int mplayerProcessID=-1;
        private MplayerBackends _mplayerBackend;
        private int _currentPosition = 0; // Current position in seconds in stream.
        private int _totalTime = 0; // The total length that the video is in seconds.
        private float _aspectRatio = 1;
        private string currentFilePath;


        public event MplayerEventHandler VideoExited;

        public float AspectRatio { get { return _aspectRatio; } }


        private MPlayer(){}
        public MPlayer(int wid, MplayerBackends backend)
        {
            this._wid = wid;
            this._fullscreen = false;
            this.MplayerRunning = false;
            this._mplayerBackend = backend;
            MediaPlayer = new System.Diagnostics.Process();
        }

        /// <summary>
        /// Cleanup resources.  Currently this means that mplayer is closed if it is still running.
        /// </summary>
        ~MPlayer()
        {
            // Cleanup

            Dispose();
        }

        private object lockToken = new object();
        public void Dispose() {
            lock(lockToken) {
            if (this.mplayerProcessID != -1)
            {
                try
                {
                    System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(this.mplayerProcessID);
                    if (p.HasExited == false)
                    {
                        p.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Instance.WriteLine(ex);
                }
            }
            }
        }


        /// <summary>
        /// Is mplayer alreadying running?  True or False.
        /// </summary>
        public bool MplayerRunning{get; set; }

        /// <summary>
        /// The current status of the player.
        /// </summary>
        public MediaStatus CurrentStatus { get; set; }

        public bool HardwareAccelerated { get; set; }


        /// <summary>
        /// The process that is running mplayer.  Generally you should not need to interact with it directly.  But it is left open so
        /// it is possible to send commands that are not covered by this library and read output etc....
        /// </summary>
        public System.Diagnostics.Process MediaPlayer { get; set; }

		private string MplayerBackend()
		{
			string backend;
            if (this._mplayerBackend == MplayerBackends.Direct3D)
            {
                backend = "direct3d";
            }
            if (this._mplayerBackend == MplayerBackends.DirectX)
            {
                backend = "directx";
            }
			else if (this._mplayerBackend == MplayerBackends.X11)
            {
                backend = "x11";
            }
			else if (this._mplayerBackend == MplayerBackends.Quartz)
            {
                backend = "quartz";
            }
			else if (this._mplayerBackend == MplayerBackends.CoreVideo)
            {
                backend = "corevideo";
            }
            else if (this._mplayerBackend == MplayerBackends.SDL)
            {
                backend = "sdl";
            }
			else if (this._mplayerBackend == MplayerBackends.GL)
            {
                backend = "gl";
            }
			else if (this._mplayerBackend == MplayerBackends.GL2)
            {
                backend = "gl2";
            }
			else if (this._mplayerBackend == MplayerBackends.ASCII)
            {
                backend = "aa";
            }
			else if (this._mplayerBackend == MplayerBackends.ColorASCII)
            {
                backend = "caca";
            }
			else if (this._mplayerBackend == MplayerBackends.Directfb)
            {
                backend = "directfb";
            }
			else if (this._mplayerBackend == MplayerBackends.Wii)
            {
                backend = "wii";
            }
			else if (this._mplayerBackend == MplayerBackends.V4l2)
            {
                backend = "v4l2";
            }
			else if (this._mplayerBackend == MplayerBackends.VESA)
            {
                backend = "vesa";
            }
            else
            {
                backend = "opengl";
            }	
			
			return backend;
		}


        /// <summary>
        /// Initializes MPlayer, which CAN (but doesn't have to be) performed before trying to play a file.
        /// </summary>
        public void Init()
        {
            MediaPlayer.StartInfo.CreateNoWindow = true;
            MediaPlayer.StartInfo.UseShellExecute = false;
            MediaPlayer.StartInfo.ErrorDialog = false;
            MediaPlayer.StartInfo.RedirectStandardOutput = true;
            MediaPlayer.StartInfo.RedirectStandardInput = true;
            MediaPlayer.StartInfo.RedirectStandardError = true;


            string backend = MplayerBackend();
            //noautosub disables subtiles... which were causing mplayer to go into an infinite loop. not cool.
            //NOTE: the "-af scaletempo" makes it so that when we change playback speed, the pitch stays the same.
            MediaPlayer.StartInfo.Arguments = string.Format("-slave -noautosub -af scaletempo -nofontconfig -quiet -idle -v -vo {0} -wid {1}", backend, this._wid);
            MediaPlayer.StartInfo.FileName = BackendPrograms.MPlayer;

            MediaPlayer.Start();

            this.CurrentStatus = MediaStatus.Stopped;

            this.MplayerRunning = true;
            this.mplayerProcessID = MediaPlayer.Id;

            //System.IO.StreamWriter mw = MediaPlayer.StandardInput;
            //mw.AutoFlush = true;

            MediaPlayer.OutputDataReceived += HandleMediaPlayerOutputDataReceived;
            MediaPlayer.ErrorDataReceived += HandleMediaPlayerErrorDataReceived;
            MediaPlayer.BeginErrorReadLine();
            MediaPlayer.BeginOutputReadLine();
        }

        /// <summary>
        /// Load and start playing a video.
        /// </summary>
        /// <param name="filePath"></param>
        public void Play(string filePath)
        {
            this.currentFilePath = filePath;

            if (this.MplayerRunning)
            {
                LoadFile(filePath);
                this.CurrentStatus = MediaStatus.Playing;
                return;
            }


            MediaPlayer.StartInfo.CreateNoWindow = true;
            MediaPlayer.StartInfo.UseShellExecute = false;
            MediaPlayer.StartInfo.ErrorDialog = false;
            MediaPlayer.StartInfo.RedirectStandardOutput = true;
            MediaPlayer.StartInfo.RedirectStandardInput = true;
            MediaPlayer.StartInfo.RedirectStandardError = true;

            //
            //slave
            //    mandatory; tells MPlayer to start in slave mode
            //quiet
            //    optional; reduces the amount of messages that MPlayer will output
            //idle
            //    optional; it doesn’t close MPlayer after a file finished playing; 
            //    this is quite useful as you don’t want to start a new process everytime 
            //    you want to play a file, but rather loading the file into the existing 
            //    already started process (for performance reasons)
			//
			// -ss HH:MM:SS  seek the position. Works with webm.
			//
			// -bandwidth <bytes>    Specify the maximum bandwidth for network streaming (for servers
            //  that are able to send content in different bitrates).
			//
			// -cache
			//
			// -prefer-ipv4   Use  IPv4  on network connections.  Falls back on IPv6 automatically.
			//
			// -user <username>   Specify username for HTTP authentication.
			// -passwd <password> Specify password for HTTP authentication.
			//
			// -user-agent <string>
            //  Use <string> as user agent for HTTP streaming.
			//
			// -wid <window ID> (also see -guiwid) (X11, OpenGL and DirectX only)
			//
			// -vc <[-|+]codec1,[-|+]codec2,...[,]>
            //  Specify a priority list of video codecs to be used, according to
            //  their  codec  name  in  codecs.conf. 

			
            string backend = MplayerBackend();



            //noautosub disables subtiles... which were causing mplayer to go into an infinite loop. not cool.
            MediaPlayer.StartInfo.Arguments = string.Format("-slave -noautosub -nofontconfig -quiet -idle -v -vo {0} -wid {1} \"{2}\"", backend, this._wid, filePath);
            MediaPlayer.StartInfo.FileName = BackendPrograms.MPlayer;

            MediaPlayer.Start();

            this.CurrentStatus = MediaStatus.Playing;

            this.MplayerRunning = true;
            this.mplayerProcessID = MediaPlayer.Id;

            //System.IO.StreamWriter mw = MediaPlayer.StandardInput;
            //mw.AutoFlush = true;

            MediaPlayer.OutputDataReceived += HandleMediaPlayerOutputDataReceived;
            MediaPlayer.ErrorDataReceived += HandleMediaPlayerErrorDataReceived;
            MediaPlayer.BeginErrorReadLine();
            MediaPlayer.BeginOutputReadLine();

            this.LoadCurrentPlayingFileLength();

            //ForceAspectRatio();
        }

        /// <summary>
        /// Starts a new video/audio file immediatly.  Requires that Play has been called.
        /// </summary>
        /// <param name="filePath">string</param>
        public void LoadFile(string filePath)
        {
            string LoadCommand = @"" + string.Format("loadfile \"{0}\"", PrepareFilePath(filePath));
            MediaPlayer.StandardInput.WriteLine(LoadCommand);
            //MediaPlayer.StandardInput.WriteLine("play");
            MediaPlayer.StandardInput.Flush();
            this.LoadCurrentPlayingFileLength();
            this.CurrentStatus = MediaStatus.Playing;
        }

        public void FrameStep()
        {
            string LoadCommand = @"frame_step";
            MediaPlayer.StandardInput.WriteLine(LoadCommand);
            MediaPlayer.StandardInput.Flush();
            this.LoadCurrentPlayingFileLength();
        }

        /// <summary>
        /// Prepare filepaths to be used witht the loadfile command.  
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks>
        /// For some reason it strips the DirectorySeperatorChar so we double it up here.
        /// </remarks>
        private string PrepareFilePath(string filePath)
        {

            string preparedPath = filePath.Replace("" + System.IO.Path.DirectorySeparatorChar, "" + System.IO.Path.DirectorySeparatorChar + System.IO.Path.DirectorySeparatorChar);

            return preparedPath;
        }


        public void ForceAspectRatio(int width, int height)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("switch_ratio {0}", _aspectRatio));
            MediaPlayer.StandardInput.Flush();
            //int correctedWidth = -1;
            //int correctedHeight = -1;
            //float panelAspectRatio = (float)width / (float)height;
            //if (panelAspectRatio >= _aspectRatio)
            //{
            //    correctedHeight = height;
            //    correctedWidth = Convert.ToInt32((float)height * _aspectRatio);
            //}
            //else
            //{
            //    correctedWidth = width;
            //    correctedHeight = Convert.ToInt32((float)width / _aspectRatio);
            //}
            //MediaPlayer.StandardInput.WriteLine(string.Format("set_property height {0}", correctedHeight));
            //MediaPlayer.StandardInput.Flush();
            //MediaPlayer.StandardInput.WriteLine(string.Format("set_property width {0}", correctedWidth));
            //MediaPlayer.StandardInput.Flush();
        }

        /// <summary>
        /// Move to a new position in the video.
        /// </summary>
        /// <param name="timePosition">Seconds.  The position to seek move to.</param>
        public void MovePosition(int timePosition)
        {
            Debug.WriteLine(DateTime.Now.ToLongTimeString() + " MPLAYER move position, {0} seconds");
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property time_pos {0}", timePosition));
            MediaPlayer.StandardInput.Flush();
        }



        /// <summary>
        /// Seek a new postion.
        /// Seek to some place in the movie.
        /// Seek.Relative is a relative seek of +/- value seconds (default).
        /// Seek.Percentage is a seek to value % in the movie.
        /// Seek.Absolute is a seek to an absolute position of value seconds.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public void Seek(int value, Seek type)
        {
            Debug.WriteLine(DateTime.Now.ToLongTimeString() + " MPLAYER Seek. {1} type, {0} seconds", value, type.ToString());
            MediaPlayer.StandardInput.WriteLine(string.Format("pausing_keep_force seek {0} {1}", value, (int)type));
            MediaPlayer.StandardInput.Flush();
        }

        /// <summary>
        /// Pause the current video.  If paused it will unpause.
        /// </summary>
        public void Pause()
        {
            if (this.CurrentStatus != MediaStatus.Playing && this.CurrentStatus != MediaStatus.Paused)
            {
                return;
            }

            try
            {
                MediaPlayer.StandardInput.WriteLine("pause");
                MediaPlayer.StandardInput.Flush();
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(ex);
                return;
            }

            if (this.CurrentStatus == MediaStatus.Paused)
            {
                this.CurrentStatus = MediaStatus.Playing;
            }
            else
            {
                this.CurrentStatus = MediaStatus.Paused;
            }
        }

        /// <summary>
        /// Stop the current video.
        /// </summary>
        public void Stop()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("pausing_keep stop");
                MediaPlayer.StandardInput.Flush();
                this.CurrentStatus = MediaStatus.Stopped;
            }
        }

        /// <summary>
        /// Close MPlayer instance.
        /// </summary>
        public void Quit()
        {
            try
            {
                MediaPlayer.StandardInput.WriteLine("quit");
                MediaPlayer.StandardInput.Flush();
                MediaPlayer.Kill();
            }
            catch (ObjectDisposedException ex)
            {
                Logging.Instance.WriteLine(ex);
            }
        }


        /// <summary>
        /// Retrieves the number of seconds of the current playing video.
        /// </summary>
        public int CurrentPlayingFileLength()
        {
            return this._totalTime;
        }
        // Sets in motions events to set this._totalTime.  Is called as soon as the video starts.
        private void LoadCurrentPlayingFileLength()
        {
            // This works even with streaming.
            Discover file = new Discover(this.currentFilePath);
            this._totalTime = file.Length;
            this._aspectRatio = file.AspectRatio;
        }


        public int CurrentPosition()
        {

            MediaPlayer.StandardInput.WriteLine("pausing_keep get_time_pos");
            MediaPlayer.StandardInput.Flush();

            // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
            System.Threading.Thread.Sleep(150);
            return this._currentPosition;
        }

        /// <summary>
        /// Get if the video is full is screen or not.  Set video to play in fullscreen.
        /// </summary>
        public bool FullScreen
        {
            get { return _fullscreen; }
            set 
            {
                _fullscreen = value;
                MediaPlayer.StandardInput.WriteLine(string.Format("pausing_keep set_property fullscreen {0}", Convert.ToInt32(_fullscreen)));
                MediaPlayer.StandardInput.Flush();
            }
        }

        /// <summary>
        /// Toggle Fullscreen.
        /// </summary>
        public void ToggleFullScreen()
        {
            if (this.MplayerRunning)
            {
                MediaPlayer.StandardInput.WriteLine("pausing_keep vo_fullscreen");
                MediaPlayer.StandardInput.Flush();
            }
        }

        /// <summary>
        /// Toggle Mute.  
        /// </summary>
        public void Mute()
        {
            MediaPlayer.StandardInput.WriteLine("pausing_keep mute");
            MediaPlayer.StandardInput.Flush();
        }


        /// <summary>
        /// Accepts a volume value of 0 - 100.
        /// </summary>
        /// <param name="volume"></param>
        public void Volume(int volume, bool isAbsolute)
        {
            Debug.Assert(volume >= 0 && volume <= 100);
            int absoluteAsInt = isAbsolute ? 1 : 0;
            MediaPlayer.StandardInput.WriteLine(string.Format("pausing_keep volume {0} {1}", volume, absoluteAsInt));
            MediaPlayer.StandardInput.Flush();

        }

        public void Speed(double speed)
        {
            Debug.Assert(speed >= 0 && speed <= 10);
            MediaPlayer.StandardInput.WriteLine(string.Format("speed_set {0}", speed));
            MediaPlayer.StandardInput.Flush();
        }


        /// <summary>
        /// All mplayer standard output is read through this function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMediaPlayerOutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string line = e.Data.ToString();


                if (line.StartsWith("ANS_TIME_POSITION="))
                {
                    this._currentPosition =(int) float.Parse(line.Substring("ANS_TIME_POSITION=".Length));
                }
                else if (line.StartsWith("ANS_length="))
                {
                    this._totalTime = (int)float.Parse(line.Substring("ANS_length=".Length));
                }
                else if (line.StartsWith("Exiting") || line.ToLower().StartsWith("eof code"))
                {
                    if (this.VideoExited != null)
                    {
                        this.VideoExited(this, new MplayerEvent("Exiting File"));
                    }
                }

                System.Console.WriteLine(line);
            }
        }

        /// <summary>
        /// All mplayer error output is read through this function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMediaPlayerErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                System.Console.WriteLine(e.Data.ToString());
            }
        }

    }
}
