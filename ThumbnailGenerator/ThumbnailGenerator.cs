using LibVLC.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NamedPipeWrapper;

namespace ThumbnailGeneratorHelper
{
    class ThumbnailGenerator : IDisposable
    {

        private SemaphoreSlim signalPlaying = null;

        private bool endReached = false;
        private bool stopped = false;

        private bool errorOccured = false;

        private void EventManager_Event(ref LibVLCLibrary.libvlc_event_t e, IntPtr userData)
        {
            Debug.WriteLine(String.Format("EventManager_Event({0})", e.type), "MediaPlayer");

            if (e.type == LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerPlaying)
            {
                signalPlaying.Release();
            }
            if (e.type == LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerEndReached)
            {
                endReached = true;
            }
            if (e.type == LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerStopped)
            {
                stopped = true;
            }
            if (e.type == LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerEncounteredError)
            {
                errorOccured = true;
                signalPlaying.Release();
            }
            if (e.type == LibVLCLibrary.libvlc_event_e.libvlc_VlmMediaInstanceStatusError)
            {
                throw new Exception("libvlc_VlmMediaInstanceStatusError occurred. this shouldn't happen.");
            }
        }



        private static readonly LibVLCLibrary.libvlc_event_e[] m_Events = 
        { 
            //LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerBackward,
            /*LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerBuffering,*/
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerEncounteredError,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerEndReached,
            //LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerForward,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerLengthChanged,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerMediaChanged,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerNothingSpecial,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerOpening,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerPausableChanged,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerPaused,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerPlaying,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerPositionChanged,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerSeekableChanged,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerSnapshotTaken,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerStopped,
            LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerTimeChanged,
            //LibVLCLibrary.libvlc_event_e.libvlc_MediaPlayerTitleChanged                     
        };


        public enum GeneratorState { Running, Cancelling, Stopped }
        public GeneratorState State = GeneratorState.Stopped;

        LibVLCLibrary library = null;
        IntPtr mp = IntPtr.Zero;
        IntPtr inst = IntPtr.Zero;


        public void Generate(int thumbnailWidth, int secondStep, string thumbnailDir, string filepath, NamedPipeClient<string> mediaScribeServer)
        {
            if (State != GeneratorState.Stopped)
                throw new Exception("ThumbnailGenerator must be in a 'stopped' state before we can start generating thumbnails. Currently, it is in state: " + State.ToString());

            State = GeneratorState.Running;
            GenerateImpl(thumbnailWidth, secondStep, thumbnailDir, filepath, mediaScribeServer);
            State = GeneratorState.Stopped;
        }

        private void GenerateImpl(int thumbnailWidth, int secondStep, string thumbnailDir, string filepath, NamedPipeClient<string> mediaScribeServer)
        {
            //TODO - ensure that these don't get wrapped in quotes, so we don't have to strip them out.
            filepath = filepath.Substring(1, filepath.Length - 2);
            thumbnailDir = thumbnailDir.Substring(1, thumbnailDir.Length - 2);

            if (false == Directory.Exists(thumbnailDir))
                Directory.CreateDirectory(thumbnailDir);

            if (null == library)
            {
                #region start VLC
                string[] vlcargs = {
                   "--intf", "dummy",                  /* no interface                   */
                   "--vout", "dummy",                  /* we don't want video (output)   */
                   "--no-audio",                       /* we don't want audio (decoding) */
                   "--no-video-title-show",            /* nor the filename displayed     */
                   "--no-stats",                       /* no stats                       */
                   "--no-sub-autodetect-file",         /* we don't want subtitles        */
                   "--no-spu",
                   "--no-osd",

             //      "--no-inhibit",                     /* we don't want interfaces       */
                   "--no-disable-screensaver",         /* we don't want interfaces       */
                   "--no-snapshot-preview",            /* no blending in dummy vout      */
                                      };
                library = LibVLCLibrary.Load(null);
                IntPtr m;

                /* Load the VLC engine */
                inst = library.libvlc_new(vlcargs);


                /* Create a new item */
                m = library.libvlc_media_new_path(inst, filepath);


                /* Create a media player playing environement */
                mp = library.libvlc_media_player_new_from_media(m);


                //hook up the libvlc event listening
                int event_index = 0;
                IntPtr media_player_event_manager = library.libvlc_media_player_event_manager(mp);
                LibVLCLibrary.libvlc_callback_t m_EventManagerEventCallback = EventManager_Event;
                while (event_index < m_Events.Length)
                    if (library.libvlc_event_attach(media_player_event_manager, m_Events[event_index++], m_EventManagerEventCallback, IntPtr.Zero) != 0)
                        throw new LibVLCException(library);

                #endregion
            }
            else
            {
                library.libvlc_media_player_release(mp);
                IntPtr m = library.libvlc_media_new_path(inst, filepath);
                mp = library.libvlc_media_player_new_from_media(m);
                //hook up the libvlc event listening
                int event_index = 0;
                IntPtr media_player_event_manager = library.libvlc_media_player_event_manager(mp);
                LibVLCLibrary.libvlc_callback_t m_EventManagerEventCallback = EventManager_Event;
                while (event_index < m_Events.Length)
                    if (library.libvlc_event_attach(media_player_event_manager, m_Events[event_index++], m_EventManagerEventCallback, IntPtr.Zero) != 0)
                        throw new LibVLCException(library);
            }
            //configure time tracking
            TimeSpan time = new TimeSpan(0, 0, 0);
            TimeSpan step = new TimeSpan(0, 0, secondStep);

            //wait until VLC is playing
            signalPlaying = new SemaphoreSlim(0, 1);
            /* play the media_player */
            int successCode = library.libvlc_media_player_play(mp);
            signalPlaying.Wait();
            if (errorOccured)
            {
                string errormsg = library.libvlc_errmsg();
                throw new Exception("libvlc_MediaPlayerEncounteredError error: " + errormsg ?? "unknown error... ah crap.");
            }

            endReached = false;
            stopped = false;

            int thumbnailHeight = -1;

            long trackLength = library.libvlc_media_player_get_length(mp);

            //while there's still video left, collect screenshots
            while (endReached != true && stopped != true)
            {
                //if we're flagged to cancel, exit out.
                if (State == GeneratorState.Cancelling)
                    return;

                //take screenshot
                string fileName = time.TotalSeconds + ".jpg";
                string thumbnailFullPath = Path.Combine(thumbnailDir, fileName);

                int result = -1;
                do
                {
                    if (endReached)
                        break;
                    //if we're flagged to cancel, exit out.
                    if (State == GeneratorState.Cancelling)
                        return;

                    int tryCount = 3;
                    bool successfulSnapshot = false;
                    Exception error = null;
                    do
                    {
                        try
                        {
                            //if we're flagged to cancel, exit out.
                            if (State == GeneratorState.Cancelling)
                                return;
                            successfulSnapshot = false;
                            result = library.libvlc_video_take_snapshot(mp, 0, thumbnailFullPath, (uint)thumbnailWidth, 0);
                            successfulSnapshot = true;
                        }
                        catch (Exception ex)
                        {
                            tryCount--;
                            error = ex;
                        }
                    }
                    while (!successfulSnapshot && tryCount > 0);
                    if (null != error && tryCount == 0)
                    {
                        throw error;
                    }
                }
                while (false == File.Exists(thumbnailFullPath));

                if (result == 0)
                {
                    mediaScribeServer.PushMessage(time.TotalSeconds.ToString().PadLeft(6, '0') + " " + fileName);
                    //Thread.Sleep(70);
                }

                //increment the time
                time += step;
                //signalPosChanged.Reset();
                library.libvlc_media_player_set_time(mp, (long)time.TotalMilliseconds);
                if (library.libvlc_media_player_get_state(mp) == LibVLCLibrary.libvlc_state_t.libvlc_Playing)
                    library.libvlc_media_player_pause(mp);

                if (library.libvlc_media_player_get_time(mp) > trackLength)
                    return;

                if (endReached)
                    return;
            }
        }

        public void Dispose()
        {
            /* Stop playing */
            library.libvlc_media_player_stop(mp);

            /* Free the media_player */
            library.libvlc_media_player_release(mp);

            library.libvlc_release(inst);
        }
    }
}
