using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MediaScribeScrubber
{
    class Program
    {
        public static readonly string ApplicationFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\MediaScribe";

        static void Main(string[] args)
        {
            if (false == Directory.Exists(ApplicationFolderPath))
            {
                Console.WriteLine("MediaScribe data doesn't actually exist. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            DeleteDir(ApplicationFolderPath);
            Console.WriteLine();
            if (Directory.Exists(ApplicationFolderPath))
            {
                Console.WriteLine("Scrubbing unsuccessful... what went wrong?");
            }
            else
            {
                Console.WriteLine("Scrubbing successful!");
            }
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
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
                        Console.WriteLine("Deleting file {0}...", f);
                        File.Delete(f);
                    }
                    catch (IOException)
                    {
                    }
                }
                foreach (var d in Directory.EnumerateDirectories(dir))
                {
                    Console.WriteLine("Deleting directory {0}...", d);
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

    }
}
