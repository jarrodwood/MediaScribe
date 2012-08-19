using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace JayDev.MediaScribe.Model
{
    public class ExportImportRepository : RepositorySqliteBase
    {
        protected SQLiteConnection exportConnection = null;

        private string _filePath;
        const string ConnectionStringFormat = "Data Source={0};Version=3";

        public ExportImportRepository(string filePath)
        {
            this._filePath = filePath;
        }

        protected void PrepareExportConnection()
        {
            if (null == exportConnection)
            {
                SQLiteFactory factory = new SQLiteFactory();
                exportConnection = factory.CreateConnection() as SQLiteConnection;
                var connString = string.Format(ConnectionStringFormat, _filePath);
                connection.ConnectionString = connString;
                connection.Open();
            }
        }

        public void ExportCourse(Course course)
        {
            PrepareExportConnection();
        }
    }
}
