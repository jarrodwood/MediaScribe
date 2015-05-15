using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace JayDev.MediaScribe.Model
{
    public class ExportFileRepository : RepositoryBase
    {
        protected SQLiteConnection exportConnection = null;

        private string _filePath;
        const string ConnectionStringFormat = "Data Source={0};Version=3";

        public ExportFileRepository(string databaseFilePath)
            : base(databaseFilePath)
        {
        }

        public void ExportCourse(Course course)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                Save<Course>(course, connection);

                course.Tracks.ForEach(x => x.ParentCourseID = course.ID.Value);
                Save<Track>(course.Tracks, connection);

                course.Notes.ForEach(x => x.ParentCourseID = course.ID.Value);
                Save<Note>(course.Notes, connection);

                mytransaction.Commit();
            }
        }
    }
}
