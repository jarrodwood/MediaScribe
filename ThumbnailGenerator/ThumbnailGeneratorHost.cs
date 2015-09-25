using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NamedPipeWrapper;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace ThumbnailGeneratorHelper
{
    public class ThumbnailGeneratorHost
    {
        NamedPipeClient<string> mediaScribeServer;
        ThumbnailGenerator generator = new ThumbnailGenerator();

        public ThumbnailGeneratorHost()
        {

            mediaScribeServer = new NamedPipeClient<string>("MediaScribe-ThumbnailGenerator");
            mediaScribeServer.Start();
            mediaScribeServer.ServerMessage += mediaScribeServer_ServerMessage;
        }

        void mediaScribeServer_ServerMessage(NamedPipeConnection<string, string> connection, string message)
        {
            Task.Factory.StartNew(() =>
            {
                if (message == "ABORT")
                {
                    if(generator.State == ThumbnailGenerator.GeneratorState.Running)
                        generator.State = ThumbnailGenerator.GeneratorState.Cancelling;
                }
                else
                {
                    string[] args = message.Split(new[] { '|' });
                    if (args.Length != 4)
                    {
                        Console.WriteLine("Expected 4 args: thumbnail width, second step, thumbnail dir, and path to video file");
                        return;
                    }
                    int thumbnailWidth = Int32.Parse(args[0]);
                    int secondStep = Int32.Parse(args[1]);
                    string thumbnailDir = args[2];
                    string filepath = args[3];

                    bool successfulGeneration = false;
                    do
                    {
                        try
                        {
                            int sleepDurationMs = 30;
                            int totalSleepTimeMs = 0;
                            while (generator.State != ThumbnailGenerator.GeneratorState.Stopped)
                            {
                                Thread.Sleep(sleepDurationMs);
                                totalSleepTimeMs += sleepDurationMs;
                                if (sleepDurationMs > 1000)
                                    throw new Exception("ThumbnailGenerator error, taking over a second to cancel last generation?");
                            }
                            
                            successfulGeneration = false;
                            generator.Generate(thumbnailWidth, secondStep, thumbnailDir, filepath, mediaScribeServer);
                            successfulGeneration = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Thumbnail generator crashed: " + ex.ToString());
                            try
                            {
                                generator.Dispose();
                            }
                            catch { }
                            generator = new ThumbnailGenerator();
                        }
                    }
                    while (false == successfulGeneration);
                }
            });
        }
    }
}
