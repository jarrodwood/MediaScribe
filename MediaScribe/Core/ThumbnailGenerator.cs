//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;
//using System.Text.RegularExpressions;
//using System.IO;
//using JayDev.MediaScribe.Common;
//using System.ComponentModel;
//using System.Threading;

//namespace JayDev.MediaScribe.Core
//{
//    //public class Thumbnail
//    //{
//    //    public string Filename { get; set; }
//    //    public string FileDirectory { get; set; }
//    //    public TimeSpan Time { get; set; }
//    //    public int Width { get; set; }
//    //    public int Height { get; set; }

//    //    public Thumbnail() { }
//    //}

//    //public class ThumbnailComparer : IComparer<Thumbnail>
//    //{
//    //    public int Compare(Thumbnail x, Thumbnail y)
//    //    {
//    //        return x.Time.TotalSeconds.CompareTo(y.Time.TotalSeconds);
//    //    }
//    //}

//    public class ThumbnailGenerator : IDisposable
//    {
//        public int SecondStep { get; private set; }
//        public string RootImageDirectory { get; private set; }
//        public List<Thumbnail> Thumbnails { get; private set; }
//        private string videoFile;
//        private int lengthSeconds;
//        private Process mplayer = null;
//        public bool IsTrackVideo { get; set; }

//        private AsyncWorker worker;
//        private Random random = new Random();

//        public Tuple<TimeSpan, Action<Thumbnail>> NotifyWhenThumbnailReady { get; set; }
            
//        public ThumbnailGenerator(int secondStep = 8)
//        {
//            worker = new AsyncWorker();
//            SecondStep = secondStep;
//            Thumbnails = new List<Thumbnail>();
            
//            //ensure that the temporary directory for image generation exists
//            RootImageDirectory = Constants.ApplicationFolderPath + @"\ThumbnailGeneration";
//            if (false == Directory.Exists(RootImageDirectory))
//            {
//                if (false == Directory.Exists(Constants.ApplicationFolderPath))
//                {
//                    Directory.CreateDirectory(Constants.ApplicationFolderPath);
//                }
//                Directory.CreateDirectory(RootImageDirectory);
//            }
//        }


//        public void Dispose()
//        {
//            //when the application ends, we want to make sure there is no thumbnail generation continuing.
//            //JDW NOTE: killed as part of controller's dispose.
//        }


//        static void DeleteDir(string dir)
//        {
//            if (String.IsNullOrEmpty(dir))
//                throw new ArgumentException(
//                    "Starting directory is a null reference or an empty string",
//                    "dir");

//            try
//            {
//                foreach (var f in Directory.EnumerateFiles(dir))
//                {
//                    try
//                    {
//                        File.Delete(f);
//                    }
//                    catch (IOException)
//                    {
//                    }
//                }
//                foreach (var d in Directory.EnumerateDirectories(dir))
//                {
//                    DeleteDir(d);
//                }

//                var entries = Directory.EnumerateFileSystemEntries(dir);

//                if (!entries.Any())
//                {
//                    try
//                    {
//                        Directory.Delete(dir);
//                    }
//                    catch (UnauthorizedAccessException) { }
//                    catch (IOException) { }
//                }
//            }
//            catch (UnauthorizedAccessException) { }
//        }

//        public void Generate(Track track)
//        {
//            //if we've just generated the thumbnails for this track, there's nothing left to do.
//            if (track.FilePath == videoFile)
//                return;

//            //if we were part way through doing anything, stop it now.
//            worker.AbortAllWork();

//            NotifyWhenThumbnailReady = null;

//            //before we do /anything/, since we've been instructed to generate the thumbnails for a new track, clear out any existing thumbnails.
//            Thumbnails.Clear();

//            //and clear out the physical thumbnails from the generation folder.
//            foreach (var d in Directory.EnumerateDirectories(RootImageDirectory))
//            {
//                DeleteDir(d);
//            }

//            this.videoFile = track.FilePath;
//            IsTrackVideo = track.IsVideo;
//            if (track.IsVideo)
//            {
//                this.lengthSeconds = Convert.ToInt32(track.Length.TotalSeconds);

//                worker.QueueWorkItem((DoWorkEventArgs args) =>
//                {
//                    try
//                    {
//                        //before we do /anything/, since we've been instructed to generate the thumbnails for a new track, clear out any existing thumbnails.
//                        Thumbnails.Clear();

//                        mplayer = new Process();
//                        //JDW NOTE: we use subdirs w/ timestamp + random number because of issues deleting thumbnails
//                        //          when changing tracks -- there's a lock kept on them temporarily.
//                        string thumbnailDir = string.Format("{0}\\{1}_{2}", RootImageDirectory, DateTime.Now.ToString("HHmmss"), random.Next(10000));
//                        if (false == Directory.Exists(thumbnailDir))
//                            Directory.CreateDirectory(thumbnailDir);
//                        mplayer.StartInfo.WorkingDirectory = thumbnailDir;
//                        mplayer.StartInfo.FileName = Constants.MPlayerExecutablePath;
//                        int width = Common.Constants.TRACKBAR_THUMBNAIL_WIDTH;
//                        int height = (int)((float)width / track.AspectRatio);

//                        //JDW NOTE: set max frames as a catch, since if MediaScribe is stopped then mplayer will continue generating images forever.
//                        int maxFrames = (int)(track.Length.TotalSeconds / (double)SecondStep) + 1;


//                        //we instruct mplayer to stop 2 seconds before the end of the video, since if you don't specify an end position or it can't reach the specified position, it will just idle.
//                        //mplayer.StartInfo.Arguments = string.Format("-vo jpeg:quality=80 -sstep 6 -vf scale=200:112 \"{0}\" -nosound -endpos {1}", videoFile, lengthSeconds-8);
//                        mplayer.StartInfo.Arguments = string.Format("-vo jpeg:quality={3} -noautosub -nofontconfig -nofontconfig -sstep 6 -vf scale={1}:{2} \"{0}\" -nosound -frames {4}", videoFile, width, height, Common.Constants.TRACKBAR_THUMBNAIL_QUALITY, maxFrames);
//                        mplayer.StartInfo.UseShellExecute = false;
//                        mplayer.StartInfo.CreateNoWindow = true;
//                        mplayer.StartInfo.RedirectStandardOutput = true;
//                        mplayer.Start();

//                        //we want the thumbnail generation to not interfere with other operations
//                        mplayer.PriorityClass = ProcessPriorityClass.BelowNormal;

//                        //regex to get the time value, before the decimal.
//                        Regex frameTimeRegex = new Regex(@"V:\s*(?<frametime>[0-9]+?)\.");
//                        int count = 1;
//                        TimeSpan lastTime = TimeSpan.MinValue;
//                        while (!mplayer.StandardOutput.EndOfStream)
//                        {
//                            //if we've got notice that we have to cancel the operation, do so.
//                            if (true == args.Cancel)
//                            {
//                                mplayer.Kill();
//                                break;
//                            }

//                            string line = mplayer.StandardOutput.ReadLine();
//                            Console.WriteLine(line);

//                            //mplayer spits out a lot of metainformation about the video. we want the lines of relevant video processing, which begin with "V:"
//                            if (line.StartsWith("V:"))
//                            {
//                                string secondString = frameTimeRegex.Match(line).Groups["frametime"].Value;
//                                TimeSpan time = new TimeSpan(0, 0, Int32.Parse(secondString));
//                                //the default image filename cannot be configured.
//                                Thumbnails.Add(new Thumbnail()
//                                {
//                                    Time = time,
//                                    Filename = count.ToString("00000000'.jpg'"),
//                                    FileDirectory = thumbnailDir,
//                                    Width = width,
//                                    Height = height
//                                });
//                                count++;

//                                //if we're waiting for notification that the thumbnail's been generated,
//                                //and we've generated the thumbnail, perform the appropriate action.
//                                if (null != NotifyWhenThumbnailReady
//                                    && time > NotifyWhenThumbnailReady.Item1)
//                                {
//                                    Thumbnail thumbnail = GetThumbnailForTime(time);
//                                    NotifyWhenThumbnailReady.Item2(thumbnail);
//                                    //we've fulfilled our obligation, clear the notification requirement.
//                                    NotifyWhenThumbnailReady = null;
//                                }

//                                if (time > track.Length || time == lastTime)
//                                {
//                                    mplayer.Kill();
//                                    break;
//                                }

//                                lastTime = time;
//                            }
//                        }

//                        mplayer.Close();
//                    }
//                    catch (Exception e)
//                    {
//                        Logging.Log(LoggingSource.ThumbnailGeneration, "*** mplayer error ***");
//                        Logging.Log(LoggingSource.ThumbnailGeneration, e.ToString());
//                    }
//                });
//            }
//        }



//        public Thumbnail GetThumbnailForTime(TimeSpan forTime)
//        {
//            //if there are no thumbnails, there is no match.
//            if (Thumbnails.Count == 0)
//                return null;

//            int binSearchResult = Thumbnails.BinarySearch(new Thumbnail() { Time = forTime }, new ThumbnailComparer());

//            int index = binSearchResult > 0 ? binSearchResult : ~binSearchResult;
//            //if the thumbnail is after the first one, take the previous thumbnails. this is because
//            //the binary search will take the NEXT thumbnail from the given time; we want the previous.
//            if (index > 0)
//                index -= 1;

//            //if the closest previous thumbnail is the LAST generated one, and its time gap is bigger than 3 * the minimum gap, then we probably haven't generated the thumbnail yet. simply return null to show we have no appropriate thumbnail.
//            if (index == Thumbnails.Count - 1 && forTime.TotalSeconds - Thumbnails[index].Time.TotalSeconds > 3 * SecondStep)
//                return null;

//            var result = Thumbnails[index];
//            string fullPath = string.Format(@"{0}\{1}", result.FileDirectory, result.Filename);
//            if (false == File.Exists(fullPath))
//            {
//                Logging.Log(LoggingSource.ThumbnailGeneration, "Error: thumbnail missing - {0}", result.Filename);
//                result = null;
//            }
//            return result;
//        }
//    }
//}
