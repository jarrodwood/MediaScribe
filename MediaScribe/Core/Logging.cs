using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace JayDev.MediaScribe.Core
{
    enum LoggingSource { ThumbnailGeneration, MPlayerWindow, AsyncWorker, Errors, DragEnabledDataGrid, FullscreenToggle }
    internal class Logging
    {
        static List<LoggingSource> showSources = new List<LoggingSource>();
        static Logging()
        {
            showSources = new List<LoggingSource>()
            {
                LoggingSource.ThumbnailGeneration,
                LoggingSource.Errors,
            };
        }

        public static void Log(LoggingSource source, string text)
        {
            if (showSources.Contains(source))
            {
                Debug.WriteLine(string.Format("{0} [{1}] - {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), source, text));
            }
        }

        public static void Log(LoggingSource source, string text, params string[] replacement)
        {
            if (showSources.Contains(source))
            {
                string replacementExpanded = string.Format(text, replacement);
                Debug.WriteLine(string.Format("{0} [{1}] - {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), source, replacementExpanded));
            }
        }
    }
}
