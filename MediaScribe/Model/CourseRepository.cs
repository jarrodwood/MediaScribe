﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Configuration;

namespace JayDev.MediaScribe.Model
{
    public class CourseRepository : RepositoryBase
    {
        /// <summary>
        /// Create a CourseRepository that connects to the default MediaScribe dataabase
        /// </summary>
        public CourseRepository()
            : base(DefaultDatabaseFilePath)
        {
        }

        /// <summary>
        /// Create a CourseRepository that connects to a specific MediaScribe database, intended for when importing notes from old MediaScribe versions
        /// </summary>
        /// <param name="connectionString"></param>
        public CourseRepository(string databaseFilePath)
            : base(databaseFilePath)
        {
        }

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

                        //sort the courses' track collection by track number
                        foreach (Course course in courses)
                        {
                            course.Tracks.Sort((x, y) => x.TrackNumber.Value.CompareTo(y.TrackNumber.Value));
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="course"></param>
        /// <param name="saveTracks"></param>
        /// <param name="saveNotes"></param>
        /// <param name="saveAsNewCourse">If set to true, all IDs will be wiped meaning the course will be appended as a new one to the list of courses, rather than any existing version updated</param>
        public void SaveCourse(Course course, bool saveTracks = false, bool saveNotes = false, bool saveAsNewCourse = false)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                if (saveAsNewCourse)
                {
                    course.ID = null;
                    course.Tracks.ForEach(x => x.ID = null);
                    course.Notes.ForEach(x => x.ID = null);
                    ////ensure the notes and tracks are ordered sensibly, which may be useful...
                    //course.Tracks = course.Tracks.OrderBy(x => x.TrackNumber).ToList();
                    //course.Notes = course.Notes.OrderBy(x => x.StartTrackNumber).ThenBy(x => x.StartTime).ToList();
                }

                Save<Course>(course, connection);

                if (saveTracks)
                {
                    course.Tracks.ForEach(x => x.ParentCourseID = course.ID.Value);
                    Save<Track>(course.Tracks, connection);
                }

                if (saveNotes)
                {
                    course.Notes.ForEach(x => x.ParentCourseID = course.ID.Value);
                    Save<Note>(course.Notes, connection);
                }

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
