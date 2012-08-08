using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.Shell;
using System.Reflection;
using System.IO;

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
            try
            {
                if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
                {
                    var application = new App();

                    application.InitializeComponent();

                    application.Run();

                    // Allow single instance code to perform cleanup operations
                    SingleInstance<App>.Cleanup();
                }
            }
            catch (Exception e)
            {
                string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                System.IO.File.AppendAllText(currentAssemblyDirectoryName + @"\exception.txt", DateTime.Now.ToLongTimeString() + "\r\n" + e.ToString() + "\r\n\r\n");
                MessageBox.Show("An error has occured -- please go to www.jarrod.co.nz/MediaScribe/ and get in touch.");
            }
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
