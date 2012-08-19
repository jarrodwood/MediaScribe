using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Configuration;

namespace JayDev.MediaScribe.Model
{
    public class RepositorySqliteBase
    {
        protected SQLiteConnection connection = null;

        protected void PrepareConnection()
        {
            if (null == connection)
            {
                SQLiteFactory factory = new SQLiteFactory();
                connection = factory.CreateConnection() as SQLiteConnection;
                var connString = ConfigurationManager.ConnectionStrings["MediaScribeDB"].ConnectionString;
                connection.ConnectionString = connString;
                connection.Open();
            }
        }

    }
}
