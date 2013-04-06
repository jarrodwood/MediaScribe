using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.Shell;
using System.Reflection;
using System.IO;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "MediaScribe";

        [STAThread]
        public static void Main()
        {
            #if (DEBUG==false)
            try
            {
            #endif
                if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
                {
                    var application = new App();

                    application.InitializeComponent();

                    application.Run();

                    // Allow single instance code to perform cleanup operations
                    SingleInstance<App>.Cleanup();
                }
            #if (DEBUG==false)
            }
            catch (Exception e)
            {
                Logging.Log(LoggingSource.Errors, e.ToString() + "\r\n\r\n");
                MessageBox.Show("An error has occured -- please go to http://mediascribe.jarrod.co.nz/contact/ and get in touch.");
            }
#endif
        }

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // ...

            return true;
        }

        #endregion
    }
}
