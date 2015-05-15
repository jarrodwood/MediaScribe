using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Configuration;

namespace JayDev.MediaScribe.Model
{
    public class SettingRepository : RepositoryBase
    {
        private object _destructiveOperationLockToken = new object();

        public SettingRepository()
            : base(DefaultDatabaseFilePath)
        {
        }

        public List<Hotkey> GetHotkeys()
        {
            PrepareConnection();
            List<Hotkey> hotkeys;
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                hotkeys = ReadAll<Hotkey>(connection);

                mytransaction.Commit();
            }

            return hotkeys;
        }

        public void PersistHotkeys(List<Hotkey> hotkeys)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {

                List<Hotkey> savedhotkeys = GetHotkeys();

                List<Hotkey> toDelete = new List<Hotkey>();
                List<Hotkey> toCreate = new List<Hotkey>();
                List<Hotkey> unchanged = new List<Hotkey>();
                foreach (Hotkey hotkey in hotkeys)
                {
                    Hotkey existingMatch = savedhotkeys.FirstOrDefault(x => x.Equals(hotkey));
                    if (null == existingMatch)
                    {
                        toCreate.Add(hotkey);
                    }
                    else
                    {
                        unchanged.Add(hotkey);
                    }
                }
                foreach (Hotkey hotkey in savedhotkeys)
                {
                    if (false == unchanged.Contains(hotkey))
                    {
                        toDelete.Add(hotkey);
                    }
                }

                foreach (Hotkey hotkey in toDelete)
                {
                    Delete<Hotkey>(hotkey, connection);
                }
                foreach (Hotkey hotkey in toCreate)
                {
                    //In case we're updating an existing hotkey... the existing copy will be removed, and now we need to save a new copy. So we clear out the ID.
                    hotkey.ID = null;
                    Save<Hotkey>(hotkey, connection);
                }

                mytransaction.Commit();
            }
        }

        public ApplicationSettings GetApplicationSettings()
        {
            PrepareConnection();
            ApplicationSettings settings;
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                var allSettings = ReadAll<ApplicationSettings>(connection);
                if (allSettings.Count > 1)
                    throw new Exception("error - expecting only one 'Settings' database row.");

                if (allSettings.Count == 0)
                    return ApplicationSettings.GetDefaultSettings();

                using (MemoryStream stream = new MemoryStream())
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(allSettings[0].SerializedData);
                    writer.Flush();
                    stream.Position = 0;
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ApplicationSettings));
                    settings = (ApplicationSettings)serializer.ReadObject(stream);
                    //ensure we record the SettingID of the object read from the database... we're going to need it to make sure we don't add more than one row to the database.
                    settings.ID = allSettings[0].ID;
                }

                mytransaction.Commit();
            }

            return settings;
        }

        public void SaveApplicationSettings(ApplicationSettings settings)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (MemoryStream stream1 = new MemoryStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ApplicationSettings));

                    serializer.WriteObject(stream1, settings);

                    stream1.Position = 0;
                    StreamReader sr = new StreamReader(stream1);
                    settings.SerializedData = sr.ReadToEnd();
                }

                Save<ApplicationSettings>(settings, connection);

                mytransaction.Commit();
            }
        }

    }
}
