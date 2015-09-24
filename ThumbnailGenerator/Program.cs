using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumbnailGeneratorHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 4) {
                Console.WriteLine("Expected 4 args: thumbnail width, second step, thumbnail dir, and path to video file");
                return;
            }
            int thumbnailWidth = Int32.Parse(args[0]);
            int secondStep = Int32.Parse(args[1]);
            string thumbnailDir = args[2];
            string filepath = args[3];

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;


            var gen = new ThumbnailGenerator();
            gen.Generate(thumbnailWidth, secondStep, thumbnailDir, filepath);
        }
    }
}
