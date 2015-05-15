using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaScribe.Monitor
{
    class Program
    {
        static Process mediaScribeProcess = null;
        static Process mplayerProcess = null;
        private static ManualResetEvent waitHandle = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            if (null == args || args.Length != 2)
            {
                throw new Exception("Must be run with two arguments - the first, the Process ID of MediaScribe, the second the Process ID of the mplayer instance.");
            }

            int mediaScribePID = Int32.Parse(args[0]);
            int mplayerPID = Int32.Parse(args[1]);
            mediaScribeProcess = Process.GetProcessById(mediaScribePID);
            mplayerProcess = Process.GetProcessById(mplayerPID);

            //ensure the mediascribe process raises events, so we'll be notified when it exits.
            mediaScribeProcess.EnableRaisingEvents = true;

            //hook the event up, so when mediascribe exits we can kill the potentially lingering mplayer.
            mediaScribeProcess.Exited += mediaScribeProcess_Exited;

            Console.WriteLine("Monitoring MediaScribe (PID {0}) and MPlayer (PID {1}).", mediaScribePID, mplayerPID);

            //blocking wait until mediascribe has exited, before we exit this application.
            waitHandle.WaitOne();
            Console.WriteLine("Monitor existing. Goodbye!");
        }

        static void mediaScribeProcess_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("MediaScribe exited. checking mplayer existance...");
            if (false == mplayerProcess.HasExited)
            {
                Console.WriteLine("mplayer instance found. Killing now...");
                mplayerProcess.Kill();
            }

            //allow the main() method to continue and exit the application.
            Console.WriteLine("Triggering monitor exit...");
            waitHandle.Set();   
        }
    }
}
