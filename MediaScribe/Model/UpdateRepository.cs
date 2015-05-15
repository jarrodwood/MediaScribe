using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Configuration;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.Model
{
    public class UpdateRepository : RepositoryBase
    {
        /// <summary>
        /// Create an UpdateRepository that connects to the default MediaScribe dataabase
        /// </summary>
        public UpdateRepository()
            : base(DefaultDatabaseFilePath)
        {
        }

        /// <summary>
        /// Create an UpdateRepository that connects to a specific MediaScribe database, intended for when importing notes from old MediaScribe versions
        /// </summary>
        /// <param name="connectionString"></param>
        public UpdateRepository(string databaseFilePath)
            : base(databaseFilePath)
        {
        }

        public void EnsureDatabaseIsUpdated()
        {
            string currentDatabaseVersionNumber = GetDatabaseVersionNumber();

            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {
                }
            }
        }
    }
}
