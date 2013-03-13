using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.Core
{
    public class Thumbnail
    {
        public string Filename { get; set; }
        public TimeSpan Time { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Thumbnail() { }
    }

    public class ThumbnailComparer : IComparer<Thumbnail>
    {
        public int Compare(Thumbnail x, Thumbnail y)
        {
            return x.Time.TotalSeconds.CompareTo(y.Time.TotalSeconds);
        }
    }

    public class ThumbnailGenerator : IDisposable
    {
        public int SecondStep { get; private set; }
        public string ImageDirectory { get; private set; }
        public List<Thumbnail> Thumbnails { get; private set; }
        private string videoFile;
        private int lengthSeconds;
        private Process mplayer = null;
        public bool IsTrackVideo { get; set; }
            
        public ThumbnailGenerator(int secondStep = 8)
        {
            SecondStep = secondStep;
            Thumbnails = new List<Thumbnail>();
            
            //ensure that the temporary directory for image generation exists
            ImageDirectory = Constants.ApplicationFolderPath + @"\ThumbnailGeneration";
            if (false == Directory.Exists(ImageDirectory))
            {
                if (false == Directory.Exists(Constants.ApplicationFolderPath))
                {
                    Directory.CreateDirectory(Constants.ApplicationFolderPath);
                }
                Directory.CreateDirectory(ImageDirectory);
            }
        }

        private void KillAnyCurrentGeneration()
        {
            //if we were part way through generating images of another video file, stop.
            if (null != mplayer)
            {
                if (false == mplayer.HasExited)
                    mplayer.Kill();
            }
        }

        public void Dispose()
        {
            //when the application ends, we want to make sure there is no thumbnail generation continuing.
            //JDW NOTE: killed as part of controller's dispose.
        }

        public void Generate(Track track)
        {
            this.videoFile = track.FilePath;
            IsTrackVideo = track.IsVideo;
            if (track.IsVideo == false)
            {
                //if the file is only audio, take this as a nice opportunity to clear out the image directory to save time next video.
                ThreadHelper.ExecuteBackground(delegate
                {
                    //ensure the thumbnail directory is empty.
                    var files = Directory.GetFiles(ImageDirectory);
                    files.ToList().ForEach(x => File.Delete(x));
                });
                return;
            }

            this.lengthSeconds = Convert.ToInt32(track.Length.TotalSeconds);

            //if we were part way through generating images of another video file, stop.
            KillAnyCurrentGeneration();
            
            ThreadHelper.ExecuteBackground(delegate {
                //ensure the thumbnail directory is empty.
                var files = Directory.GetFiles(ImageDirectory);
                files.ToList().ForEach(x => File.Delete(x));


                mplayer = new Process();
                mplayer.StartInfo.WorkingDirectory = ImageDirectory;
                mplayer.StartInfo.FileName = Constants.MPlayerExecutablePath;
                int width = Common.Constants.TRACKBAR_THUMBNAIL_WIDTH;
                int height = (int)((float)width / track.AspectRatio);
                //we instruct mplayer to stop 2 seconds before the end of the video, since if you don't specify an end position or it can't reach the specified position, it will just idle.
                //mplayer.StartInfo.Arguments = string.Format("-vo jpeg:quality=80 -sstep 6 -vf scale=200:112 \"{0}\" -nosound -endpos {1}", videoFile, lengthSeconds-8);
                //JDW NOTE: set max frames to 1000 as a catch, since if MediaScribe is stopped then mplayer will continue generating images forever.
                mplayer.StartInfo.Arguments = string.Format("-vo jpeg:quality={3} -sstep 6 -vf scale={1}:{2} \"{0}\" -nosound -frames 1000", videoFile, width, height, Common.Constants.TRACKBAR_THUMBNAIL_QUALITY);
                mplayer.StartInfo.UseShellExecute = false;
                mplayer.StartInfo.CreateNoWindow = true;
                mplayer.StartInfo.RedirectStandardOutput = true;
                mplayer.Start();

                //we want the thumbnail generation to not interfere with other operations
                mplayer.PriorityClass = ProcessPriorityClass.BelowNormal;

                //regex to get the time value, before the decimal.
                Regex frameTimeRegex = new Regex(@"V:\s*(?<frametime>[0-9]+?)\.");
                int count = 1;
                TimeSpan lastTime = TimeSpan.MinValue;
                while (!mplayer.StandardOutput.EndOfStream)
                {
                    string line = mplayer.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                
                    //mplayer spits out a lot of metainformation about the video. we want the lines of relevant video processing, which begin with "V:"
                    if (line.StartsWith("V:"))
                    {
                        string secondString = frameTimeRegex.Match(line).Groups["frametime"].Value;
                        TimeSpan time = new TimeSpan(0, 0, Int32.Parse(secondString));
                        //the default image filename cannot be configured.
                        Thumbnails.Add(new Thumbnail() { 
                            Time = time,
                            Filename = count.ToString("00000000'.jpg'"),
                            Width = width,
                            Height = height});
                        count++;

                        if (time > track.Length || time == lastTime)
                        {
                            mplayer.Kill();
                            break;
                        }

                        lastTime = time;
                    }
                }

                mplayer.Close();
            });
        }

        public Thumbnail GetThumbnailForTime(TimeSpan forTime)
        {
            //if there are no thumbnails, there is no match.
            if (Thumbnails.Count == 0)
                return null;

            int binSearchResult = Thumbnails.BinarySearch(new Thumbnail() { Time = forTime }, new ThumbnailComparer());

            int index = binSearchResult > 0 ? binSearchResult : ~binSearchResult - 1;

            //if the closest previous thumbnail is the LAST generated one, and its time gap is bigger than 3 * the minimum gap, then we probably haven't generated the thumbnail yet. simply return null to show we have no appropriate thumbnail.
            if (index == Thumbnails.Count - 1 && forTime.TotalSeconds - Thumbnails[index].Time.TotalSeconds > 3 * SecondStep)
                return null;

            return Thumbnails[index];
        }
    }
}
