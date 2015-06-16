using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using JayDev.MediaScribe.Common;
using System.IO;

namespace JayDev.MediaScribe.Core
{
    enum LoggingSource
    {
        ThumbnailGeneration,
        MPlayerWindow,
        AsyncWorker,
        Errors,
        DragEnabledDataGrid,
        FullscreenToggle,
        CourseUseViewModel,
        Temp
    }
    internal class Logging
    {
        static List<LoggingSource> sourcesToWriteToDebug = new List<LoggingSource>();
        static List<LoggingSource> sourcesToWriteToFile = new List<LoggingSource>();
        static Logging()
        {
            sourcesToWriteToDebug = new List<LoggingSource>()
            {
                LoggingSource.ThumbnailGeneration,
                LoggingSource.Errors,
                LoggingSource.Temp
            };
            sourcesToWriteToFile = new List<LoggingSource>()
            {
                LoggingSource.Errors,
            };
        }

        public static void VerifyAppFolderExists()
        {
            if (false == System.IO.Directory.Exists(Constants.ApplicationFolderPath))
            {
                System.IO.Directory.CreateDirectory(Constants.ApplicationFolderPath);
            }
        }

        public static void Log(LoggingSource source, string text)
        {
            string prefixedText = string.Format("{0} [{1}] - {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), source, text);

            if (sourcesToWriteToDebug.Contains(source))
            {
                Debug.WriteLine(prefixedText);
                
            }

            if (sourcesToWriteToFile.Contains(source))
            {
                VerifyAppFolderExists();
                File.AppendAllText(string.Format(Constants.ApplicationGenericFilePath, "errorlog.txt"), prefixedText);
            }
        }

        public static void Log(LoggingSource source, string text, params string[] replacement)
        {
            string replacementExpanded = string.Format(text, replacement);
            Log(source, replacementExpanded);
        }
    }
}
