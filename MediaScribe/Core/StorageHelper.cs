using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.Core
{
    public class StorageHelper
    {
        private static readonly string DatabaseFileName = "MediaScribe.db";
        private static readonly string EmbeddedEmptyDatabaseFileName = "JayDev.MediaScribe.Resources.MediaScribeEmpty.db";

        public void VerifyAppFolderExists()
        {
            if (false == System.IO.Directory.Exists(Constants.ApplicationFolderPath))
            {
                System.IO.Directory.CreateDirectory(Constants.ApplicationFolderPath);
            }
        }

        /// <summary>
        /// TODO: make parameterized, so we can pass the database file path from the connection string.
        /// </summary>
        public void CreateDBIfNotExist()
        {
            VerifyAppFolderExists();
            //if the database doesn't exist, create it using our embedded empty database (containing only hotkeys)
            if (false == FileExistsInStorageFolder(DatabaseFileName))
            {
                Assembly _assembly;
                _assembly = Assembly.GetExecutingAssembly();
                using (var stream = _assembly.GetManifestResourceStream(EmbeddedEmptyDatabaseFileName))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    // TODO: use the buffer that was read

                    WriteFileToStorageFolder(DatabaseFileName, buffer);
                }
            }
        }

        public void AppendFileToStorageFolder(string fileName, string contents)
        {
            VerifyAppFolderExists();
            File.AppendAllText(string.Format(Constants.ApplicationGenericFilePath, fileName), contents);
        }

        public void WriteFileToStorageFolder(string fileName, string contents)
        {
            VerifyAppFolderExists();
            File.WriteAllText(string.Format(Constants.ApplicationGenericFilePath, fileName), contents);
        }

        public void WriteFileToStorageFolder(string fileName, byte[] contents)
        {
            VerifyAppFolderExists();
            File.WriteAllBytes(string.Format(Constants.ApplicationGenericFilePath, fileName), contents);
        }

        public bool FileExistsInStorageFolder(string fileName)
        {
            VerifyAppFolderExists();
            return File.Exists(string.Format(Constants.ApplicationGenericFilePath, fileName));
        }

        public string GetFullPathOfFileInStorageFolder(string fileName)
        {
            VerifyAppFolderExists();
            return string.Format(Constants.ApplicationGenericFilePath, fileName);
        }
    }
}
