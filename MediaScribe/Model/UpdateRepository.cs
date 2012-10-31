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
        static UpdateRepository()
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

        private string GetDatabaseVersionNumber() {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {
                    string versionNumber = null;
                    try {
                    mycommand.CommandText = "SELECT DatabaseVersion FROM Version";
                    string stringResult = Convert.ToString(mycommand.ExecuteScalar());
                        versionNumber = stringResult;
                    }
                    catch(SQLiteException exception) {
                        if(exception.Message.Contains("no such table")) {
                            versionNumber = "0.9.0.0";
                        }
                        else {
                            throw;
                        }
                    }
                    return versionNumber;
                }
            }
        }
    }
}
