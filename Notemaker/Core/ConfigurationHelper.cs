using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JayDev.MediaScribe.Core
{
    public class StorageHelper
    {
        private static readonly string ApplicationFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\MediaScribe";
        private static readonly string GenericFilePath = ApplicationFolderPath + @"\{0}";

        public void VerifyAppFolderExists()
        {
            if (false == System.IO.Directory.Exists(ApplicationFolderPath))
            {
                System.IO.Directory.CreateDirectory(ApplicationFolderPath);
            }
        }


        public void AppendFileToStorageFolder(string fileName, string contents)
        {
            VerifyAppFolderExists();
            File.AppendAllText(string.Format(GenericFilePath, fileName), contents);
        }

        public void WriteFileToStorageFolder(string fileName, string contents)
        {
            VerifyAppFolderExists();
            File.WriteAllText(string.Format(GenericFilePath, fileName), contents);
        }

        public void WriteFileToStorageFolder(string fileName, byte[] contents)
        {
            VerifyAppFolderExists();
            File.WriteAllBytes(string.Format(GenericFilePath, fileName), contents);
        }

        public bool FileExistsInStorageFolder(string fileName)
        {
            VerifyAppFolderExists();
            return File.Exists(string.Format(GenericFilePath, fileName));
        }

        public string GetFullPathOfFileInStorageFolder(string fileName)
        {
            VerifyAppFolderExists();
            return string.Format(GenericFilePath, fileName);
        }
    }
}
