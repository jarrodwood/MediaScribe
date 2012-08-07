using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace JayDev.MediaScribe.Common
{

    public static class Utility
    {

        public static bool AreAllValidNumericChars(string str)
        {
            foreach (char c in str)
            {
                if (!Char.IsNumber(c)) return false;
            }

            return true;
        }

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
