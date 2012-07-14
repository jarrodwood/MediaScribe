using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace JayDev.Notemaker.Common
{
    public static class Utility
    {
        private static string applicationVersionNumber = null;
        public static string ApplicationVersionNumber
        {
            get
            {
                if (applicationVersionNumber == null)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    applicationVersionNumber = fvi.ProductVersion;
                }
                return applicationVersionNumber;
            }
        }

        public static string GetTimeSpanAsShortString(TimeSpan input)
        {
            StringBuilder resultBuilder = new StringBuilder();
            if (input.TotalHours > 1)
            {
                //cast as int first, because we don't care about partial hours
                resultBuilder.Append(((int)input.TotalHours).ToString("00"));
                resultBuilder.Append(":");
            }
            resultBuilder.Append(input.Minutes.ToString("00"));
            resultBuilder.Append(":");
            resultBuilder.Append(input.Seconds.ToString("00"));
            return resultBuilder.ToString();
        }
        public static string GetTimeSpanAsLongString(TimeSpan input)
        {
            StringBuilder resultBuilder = new StringBuilder();
            //cast as int first, because we don't care about partial hours
            resultBuilder.Append(((int)input.TotalHours).ToString("00"));
            resultBuilder.Append(":");
            resultBuilder.Append(input.Minutes.ToString("00"));
            resultBuilder.Append(":");
            resultBuilder.Append(input.Seconds.ToString("00"));
            return resultBuilder.ToString();
        }
    }
}
