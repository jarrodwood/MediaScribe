using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Configuration;

namespace JayDev.MediaScribe.Model
{
    public class CourseRepository : RepositorySqliteBase
    {

        public List<Course> GetCourseList()
        {
            List<Course> courses = null;

            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                courses = SqliteHelper.ReadAll<Course>(connection);

                    if (null != courses && courses.Count > 0)
                    {
                        Dictionary<int, Course> coursesByID = courses.ToDictionary(x => x.ID.Value);

                        List<Note> notes = SqliteHelper.ReadAll<Note>(connection);

                        List<TrackTime> trackTimes = SqliteHelper.ReadAll<TrackTime>(connection);
                        Dictionary<int, TrackTime> trackTimesByID = trackTimes.ToDictionary(x => x.ID.Value);

                        List<Track> tracks = SqliteHelper.ReadAll<Track>(connection);
                        Dictionary<int, Track> tracksByID = tracks.ToDictionary(x => x.ID.Value);


                        foreach (TrackTime trackTime in trackTimes)
                        {
                            trackTime.Track = tracksByID[trackTime.TrackID.Value];
                        }

                        foreach (var note in notes)
                        {
                            if (null != note.StartTrackTimeID)
                            {
                                note.Start = trackTimesByID[note.StartTrackTimeID.Value];
                            }
                            if (null != note.EndTrackTimeID)
                            {
                                note.End = trackTimesByID[note.EndTrackTimeID.Value];
                            }

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
                SqliteHelper.Save<Course>(course, connection);

                mytransaction.Commit();
            }
        }

        public void SaveCourseAndTracks(Course course)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                SqliteHelper.Save<Course>(course, connection);

                course.Tracks.ForEach(x => x.ParentCourseID = course.ID.Value);
                SqliteHelper.Save<Track>(course.Tracks, connection);

                mytransaction.Commit();
            }
        }

        public void SaveNote(Course parentCourse, Note note)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                if (null != note.Start)
                {
                    SqliteHelper.Save<TrackTime>(note.Start, connection);
                    note.StartTrackTimeID = note.Start.ID;
                }

                if (null != note.End)
                {
                    SqliteHelper.Save<TrackTime>(note.End, connection);
                    note.EndTrackTimeID = note.End.ID;
                }

                note.ParentCourseID = parentCourse.ID.Value;
                SqliteHelper.Save<Note>(note, connection);

                mytransaction.Commit();
            }
        }


        public void DeleteNote(Note note)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                SqliteHelper.Delete<Note>(note, connection);

                if (null != note.Start)
                {
                    SqliteHelper.Delete<TrackTime>(note.Start, connection);
                }

                if (null != note.End)
                {
                    SqliteHelper.Delete<TrackTime>(note.End, connection);
                }

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
                    SqliteHelper.Delete<Note>(note, connection);

                    if (null != note.StartTrackTimeID)
                    {
                        SqliteHelper.Delete<TrackTime>(new TrackTime() { ID = note.StartTrackTimeID }, connection);
                    }
                    if (null != note.EndTrackTimeID)
                    {
                        SqliteHelper.Delete<TrackTime>(new TrackTime() { ID = note.EndTrackTimeID }, connection);
                    }
                }

                foreach (Track track in course.Tracks)
                {
                    SqliteHelper.Delete<Track>(track, connection);
                }

                SqliteHelper.Delete<Course>(course, connection);
            }
        }
    }
}
