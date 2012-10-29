using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Configuration;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.Model
{
    public class UpdateRepository : RepositorySqliteBase
    {
        private static List<DatabaseUpdateEntry> _UpdateEntries = new List<DatabaseUpdateEntry>();
        static UpdateRepository()
        {
            _UpdateEntries.Add(new DatabaseUpdateEntry()
            {
                VersionNumber = "0.9",
                UpdateScript = null
            });

            _UpdateEntries.Add(new DatabaseUpdateEntry()
            {
                VersionNumber = "0.95",
                UpdateScript = null
            });
        }

        public void EnsureDatabaseIsUpdated()
        {
            string currentDatabaseVersionNumber = GetDatabaseVersionNumber();

            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
            }
        }

        private string GetDatabaseVersionNumber()
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                
            }
            return null;
        }
    }
}
