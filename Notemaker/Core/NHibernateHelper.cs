using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using JayDev.MediaScribe;
using System.Reflection;

namespace JayDev.MediaScribe.Core
{
    public class NHibernateHelper
    {
        private static readonly string ApplicationFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\MediaScribe";
        private static readonly string GenericFilePath = ApplicationFolderPath + @"\{0}";
        private static readonly string DatabaseFileName = "MediaScribe.db";
        private static readonly string DatabaseFilePath = string.Format(GenericFilePath, DatabaseFileName);
        private static readonly string EmbeddedEmptyDatabaseFileName = "JayDev.MediaScribe.Resources.MediaScribeEmpty.db";

        private static ISessionFactory _sessionFactory;
 
        private static ISessionFactory SessionFactory
        {
            get
            {
                if (false == System.IO.Directory.Exists(ApplicationFolderPath))
                {
                    System.IO.Directory.CreateDirectory(ApplicationFolderPath);
                }
                //if the database doesn't exist, create it using our embedded empty database (containing only hotkeys)
                if (false == System.IO.File.Exists(DatabaseFilePath))
                {
                    Assembly _assembly;
                    _assembly = Assembly.GetExecutingAssembly();
                    using (var stream = _assembly.GetManifestResourceStream(EmbeddedEmptyDatabaseFileName))
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        // TODO: use the buffer that was read

                        System.IO.File.WriteAllBytes(DatabaseFilePath, buffer);
                    }
                }

                if(_sessionFactory == null)
                {
                    var configuration = new Configuration();
                    configuration.Configure();
                    configuration.AddAssembly("MediaScribe");
                    string newConnectionString = string.Format(@"Data Source={0};Version=3", DatabaseFilePath);
                    configuration.SetProperty("connection.connection_string", newConnectionString);
                    _sessionFactory = configuration.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }
 
        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}