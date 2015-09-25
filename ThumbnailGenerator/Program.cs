using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NamedPipeWrapper;

namespace ThumbnailGeneratorHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
            ThumbnailGeneratorHost host = new ThumbnailGeneratorHost();
            Process.GetCurrentProcess().WaitForExit();
        }

    }
}
