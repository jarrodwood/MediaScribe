using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Configuration;

namespace JayDev.MediaScribe.Model
{
    public class CourseRepository : RepositoryBase
    {

        public List<Course> GetCourseList()
        {
            List<Course> courses = null;

            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                courses = ReadAll<Course>(connection);

                    if (null != courses && courses.Count > 0)
                    {
                        Dictionary<int, Course> coursesByID = courses.ToDictionary(x => x.ID.Value);

                        List<Note> notes = ReadAll<Note>(connection);

                        List<Track> tracks = ReadAll<Track>(connection);
                        Dictionary<int, Track> tracksByNumber = tracks.ToDictionary(x => x.TrackNumber.Value);

                        foreach (var note in notes)
                        {
                            Course parentCourse = coursesByID[note.ParentCourseID];
                            note.ParentCourse = parentCourse;
                            parentCourse.Notes.Add(note);
                        }

                        foreach (var track in tracks)
                        {
                            Course parentCourse = coursesByID[track.ParentCourseID];
                            track.ParentCourse = parentCourse;
                            parentCourse.Tracks.Add(track);
                        }
                    }
                mytransaction.Commit();
            }

            return courses;
        }



        public Course GetCourse(int courseID)
        {
            return GetCourseList().First(x => x.ID == courseID);
        }

        public void SaveCourseOnly(Course course)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                Save<Course>(course, connection);

                mytransaction.Commit();
            }
        }

        public void SaveCourseAndTracks(Course course)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                Save<Course>(course, connection);

                course.Tracks.ForEach(x => x.ParentCourseID = course.ID.Value);
                Save<Track>(course.Tracks, connection);

                mytransaction.Commit();
            }
        }

        public void SaveNote(Course parentCourse, Note note)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                note.ParentCourseID = parentCourse.ID.Value;
                Save<Note>(note, connection);

                mytransaction.Commit();
            }
        }


        public void DeleteNote(Note note)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                Delete<Note>(note, connection);

                mytransaction.Commit();
            }
        }

        public void DeleteCourse(Course course)
        {
            course = GetCourse(course.ID.Value);
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                foreach (Note note in course.Notes)
                {
                    Delete<Note>(note, connection);
                }

                foreach (Track track in course.Tracks)
                {
                    Delete<Track>(track, connection);
                }

                Delete<Course>(course, connection);

                mytransaction.Commit();
            }
        }
    }
}
