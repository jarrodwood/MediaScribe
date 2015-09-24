using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using JayDev.MediaScribe.Common;
using System.ComponentModel;
using System.Threading;
using System.Drawing;
using LibVLC.NET;

namespace JayDev.MediaScribe.Core
{
    public class Thumbnail
    {
        public string Filename { get; set; }
        public string FileDirectory { get; set; }
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
        public string RootImageDirectory { get; private set; }
        public List<Thumbnail> Thumbnails { get; private set; }
        private string videoFile;
        private int lengthSeconds;
        private Process mplayer = null;
        public bool IsTrackVideo { get; set; }

        private AsyncWorker worker;
        private Random random = new Random();

        public Tuple<TimeSpan, Action<Thumbnail>> NotifyWhenThumbnailReady { get; set; }


        public ThumbnailGenerator(int secondStep = 10)
        {
            worker = new AsyncWorker();
            SecondStep = secondStep;
            Thumbnails = new List<Thumbnail>();

            //ensure that the temporary directory for image generation exists
            RootImageDirectory = Constants.ApplicationFolderPath + @"\ThumbnailGeneration";
            if (false == Directory.Exists(RootImageDirectory))
            {
                if (false == Directory.Exists(Constants.ApplicationFolderPath))
                {
                    Directory.CreateDirectory(Constants.ApplicationFolderPath);
                }
                Directory.CreateDirectory(RootImageDirectory);
            }
        }


        public void Dispose()
        {
            //when the application ends, we want to make sure there is no thumbnail generation continuing.
            //JDW NOTE: killed as part of controller's dispose.
        }


        static void DeleteDir(string dir)
        {
            if (String.IsNullOrEmpty(dir))
                throw new ArgumentException(
                    "Starting directory is a null reference or an empty string",
                    "dir");

            try
            {
                foreach (var f in Directory.EnumerateFiles(dir))
                {
                    try
                    {
                        File.Delete(f);
                    }
                    catch (IOException)
                    {
                    }
                }
                foreach (var d in Directory.EnumerateDirectories(dir))
                {
                    DeleteDir(d);
                }

                var entries = Directory.EnumerateFileSystemEntries(dir);

                if (!entries.Any())
                {
                    try
                    {
                        Directory.Delete(dir);
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (IOException) { }
                }
            }
            catch (UnauthorizedAccessException) { }
        }


        public void Generate(Track track)
        {
            //if we've just generated the thumbnails for this track, there's nothing left to do.
            if (track.FilePath == videoFile)
                return;

            //if we were part way through doing anything, stop it now.
            worker.AbortAllWork();

            NotifyWhenThumbnailReady = null;

            //before we do /anything/, since we've been instructed to generate the thumbnails for a new track, clear out any existing thumbnails.
            Thumbnails.Clear();

            //and clear out the physical thumbnails from the generation folder.
            foreach (var d in Directory.EnumerateDirectories(RootImageDirectory))
            {
                DeleteDir(d);
            }

            this.videoFile = track.FilePath;
            IsTrackVideo = track.IsVideo;
            if (track.IsVideo)
            {
                this.lengthSeconds = Convert.ToInt32(track.Length.TotalSeconds);

                worker.QueueWorkItem((DoWorkEventArgs args) =>
                {
                    try
                    {
                        //before we do /anything/, since we've been instructed to generate the thumbnails for a new track, clear out any existing thumbnails.
                        Thumbnails.Clear();

                        //and clear out the physical thumbnails from the generation folder.
                        foreach (var d in Directory.EnumerateDirectories(RootImageDirectory))
                        {
                            DeleteDir(d);
                        }
                        //JDW NOTE: we use subdirs w/ timestamp + random number because of issues deleting thumbnails
                        //          when changing tracks -- there's a lock kept on them temporarily.
                        string thumbnailDir = string.Format("{0}\\{1}_{2}", RootImageDirectory, DateTime.Now.ToString("HHmmss"), random.Next(10000));
                        if (false == Directory.Exists(thumbnailDir))
                            Directory.CreateDirectory(thumbnailDir);

                        int thumbnailWidth = Common.Constants.TRACKBAR_THUMBNAIL_WIDTH;

                        int thumbnailHeight = -1;
                        string thumbnailGenArgs = String.Join(" ", new string[] { thumbnailWidth.ToString(), SecondStep.ToString(), "\"" + thumbnailDir + "\"", "\"" + track.FilePath + "\"" });
                        ProcessStartInfo psi = new ProcessStartInfo()
                        {
                            FileName = "ThumbnailGeneratorHelper.exe",
                            WorkingDirectory = Directory.GetCurrentDirectory(),
                            Arguments = thumbnailGenArgs,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                        };
                        var proc = Process.Start(psi);
                        proc.PriorityClass = ProcessPriorityClass.BelowNormal;
                        while (!proc.StandardOutput.EndOfStream)
                        {
                            string line = proc.StandardOutput.ReadLine();
                            int seconds = Int32.Parse(line.Substring(0, 6));
                            TimeSpan time = new TimeSpan(0,0,seconds);
                            string thumbnailFilename = line.Substring(7);
                            if (args.Cancel)
                            {
                                proc.Kill();
                                break;
                            }

                            //determine the thumbnail height... since for each track, this could be different
                            if (thumbnailHeight == -1)
                            {
                                string thumbnailFullPath = Path.Combine(thumbnailDir, thumbnailFilename);
                                if (File.Exists(thumbnailFullPath))
                                {
                                    Bitmap img = new Bitmap(thumbnailFullPath);
                                    thumbnailHeight = img.Height;
                                }
                            }

                            Thumbnails.Add(new Thumbnail()
                            {
                                Time = time,
                                Filename = thumbnailFilename,
                                FileDirectory = thumbnailDir,
                                Width = thumbnailWidth,
                                Height = thumbnailHeight
                            });
                        }

                    }
                    catch (Exception e)
                    {
                        Logging.Log(LoggingSource.ThumbnailGeneration, "*** libvlc error ***");
                        Logging.Log(LoggingSource.ThumbnailGeneration, e.ToString());
                    }
                });
            }
        }



        public Thumbnail GetThumbnailForTime(TimeSpan forTime)
        {
            //if there are no thumbnails, there is no match.
            if (Thumbnails.Count == 0)
                return null;

            int binSearchResult = Thumbnails.BinarySearch(new Thumbnail() { Time = forTime }, new ThumbnailComparer());

            int index = binSearchResult > 0 ? binSearchResult : ~binSearchResult;
            //if the thumbnail is after the first one, take the previous thumbnails. this is because
            //the binary search will take the NEXT thumbnail from the given time; we want the previous.
            if (index > 0)
                index -= 1;

            //if the closest previous thumbnail is the LAST generated one, and its time gap is bigger than 3 * the minimum gap, then we probably haven't generated the thumbnail yet. simply return null to show we have no appropriate thumbnail.
            if (index == Thumbnails.Count - 1 && forTime.TotalSeconds - Thumbnails[index].Time.TotalSeconds > 3 * SecondStep)
                return null;

            var result = Thumbnails[index];
            string fullPath = string.Format(@"{0}\{1}", result.FileDirectory, result.Filename);
            if (false == File.Exists(fullPath))
            {
                Logging.Log(LoggingSource.ThumbnailGeneration, "Error: thumbnail missing - {0}", result.Filename);
                result = null;
            }
            return result;
        }
    }
}
