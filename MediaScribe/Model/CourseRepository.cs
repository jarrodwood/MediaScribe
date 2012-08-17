//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using JayDev.MediaScribe.Core;
//using NHibernate;
//using NHibernate.Criterion;

//namespace JayDev.MediaScribe.Model
//{
//    public class CourseRepository
//    {
//        public CourseRepository() { }

//        private object _destructiveOperationLockToken = new object();

//        public List<Course> GetCourseList()
//        {
//            IList<Course> result;
//            using (ISession session = NHibernateHelper.OpenSession())
//            {
//                result = session.CreateCriteria<Course>().List<Course>();
//            }

//            foreach (Course course in result)
//            {
//                foreach (Note note in course.Notes)
//                {
//                    if (null != note.Start)
//                        note.Start.ParentCourse = course;
//                    if (null != note.End)
//                        note.End.ParentCourse = course;
//                }
//            }

//            return result as List<Course>;
//        }

//        //public Course GetCourse(int courseID)
//        //{
//        //    Course result = null;
//        //    using (ISession session = NHibernateHelper.OpenSession())
//        //    using (ITransaction transaction = session.BeginTransaction())
//        //    {
//        //        var results = session.CreateCriteria<Course>().Add(Restrictions.Eq("CourseID", courseID)).List<Course>();
//        //        if (null != results && results.Count == 1)
//        //            result = results[0];

//        //        transaction.Commit();
//        //    }

//        //    foreach (Note note in result.Notes)
//        //    {
//        //        if (null != note.Start)
//        //            note.Start.ParentCourse = result;
//        //        if (null != note.End)
//        //            note.End.ParentCourse = result;
//        //    }

//        //    return result;
//        //}

//        public Course GetCourse(int courseID)
//        {
//            Course result;
//            using (ISession session = NHibernateHelper.OpenSession())
//            using (ITransaction transaction = session.BeginTransaction())
//            {
//                result = session.Get<Course>(courseID);

//                transaction.Commit();
//            }

//            foreach (Note note in result.Notes)
//            {
//                if (null != note.Start)
//                    note.Start.ParentCourse = result;
//                if (null != note.End)
//                    note.End.ParentCourse = result;
//            }

//            return result;
//        }

//        public void SaveCourse(Course course)
//        {
//            lock (_destructiveOperationLockToken)
//            {
//                using (ISession session = NHibernateHelper.OpenSession())
//                using (ITransaction transaction = session.BeginTransaction())
//                {
//                    session.SaveOrUpdate(course);

//                    transaction.Commit();
//                }
//            }
//        }

//        public void SaveCourseAndTracks(Course course)
//        {
//            lock (_destructiveOperationLockToken)
//            {
//                using (ISession session = NHibernateHelper.OpenSession())
//                using (ITransaction transaction = session.BeginTransaction())
//                {
//                    //save changes to the course
//                    if (null == course.ID)
//                    {
//                        course.DateCreated = DateTime.Now;
//                        course.DateViewed = DateTime.Now;
//                        session.Save(course);
//                    }
//                    else
//                    {
//                        session.Update(course);
//                    }

//                    IList<Track> savedTracks = session.CreateCriteria<Track>().Add(Expression.Where<Track>(x => x.ParentCourse.ID == course.ID)).List<Track>();
//                    savedTracks = savedTracks ?? new List<Track>();


//                    IList<Track> potentiallyUpdatedTracks = course.Tracks;

//                    List<Track> mergedTracks = new List<Track>();
//                    List<Track> normalTracksToSave = new List<Track>();

//                    //time to use some intelligence. if the FILE NAME of a track that was removed from the collection... is the same as the
//                    //name of a track that's been /added/ to the collection... it's probably the same file, but's been moved or accidentally
//                    //removed and re-added. we'll be smart, and persist the new values across to the old tracks (so that we can keep all
//                    //the note associations for it)
//                    foreach(Track track in potentiallyUpdatedTracks) 
//                    {
//                        Track savedTrackWithSameName = savedTracks.FirstOrDefault(x => x.FileName == track.FileName && x.ID != track.ID);
//                        if (null != savedTrackWithSameName)
//                        {
//                            track.CopyTo(savedTrackWithSameName);
//                            //update, don't merge. because the object is persistent (within scope of session)
//                            session.Update(savedTrackWithSameName);
//                            //store the object in the merged collection, which will be returned.
//                            mergedTracks.Add(savedTrackWithSameName);
//                        }
//                        else
//                        {
//                            normalTracksToSave.Add(track);
//                        }
//                    }

//                    //now, go through and delete from the database, all the tracks that were removed from the collection but weren't
//                    //'copies', like we dealt with just above.
//                    List<Track> removedTracks = savedTracks.Except(mergedTracks, x => x.ID).Except(course.Tracks, x => x.ID).ToList();
//                    foreach (Track removedTrack in removedTracks)
//                    {
//                        session.Delete(removedTrack);
//                    }

//                    //finally, go through the REST of the tracks (new tracks, and updated tracks) and merge them.
//                    foreach (Track track in normalTracksToSave)
//                    {
//                        if (null == track.ID)
//                        {
//                            track.ParentCourse = course;
//                            track.ParentCourseID = course.ID.Value;
//                            session.Save(track);
//                            mergedTracks.Add(track);
//                        }
//                        else
//                        {
//                            Track mergedTrack = session.Merge<Track>(track);
//                            mergedTracks.Add(mergedTrack);
//                        }
//                    }

//                    //now we can store the merged collection against the course, and get outta here!
//                    course.Tracks.Clear();
//                    course.Tracks.AddRange(mergedTracks);

//                    transaction.Commit();
//                }
//            }
//        }

//        public void SaveNote(Course parentCourse, Note note)
//        {
//            lock (_destructiveOperationLockToken)
//            {
//                using (ISession session = NHibernateHelper.OpenSession())
//                using (ITransaction transaction = session.BeginTransaction())
//                {
//                    note.ParentCourse = parentCourse;
//                    note.ParentCourseID = parentCourse.ID.Value;

//                    if (null == note.ID)
//                    {
//                        session.Save(note);
//                    }
//                    else
//                    {
//                        Note oldNote = session.Get<Note>(note.ID);
//                        if (null != oldNote.Start && note.Start.ID != oldNote.Start.ID)
//                        {
//                            session.Delete(oldNote.Start);
//                        }
//                        if (null != oldNote.End && note.End.ID != oldNote.End.ID)
//                        {
//                            session.Delete(oldNote.End);
//                        }

//                        note = session.Merge<Note>(note);
//                    }

//                    ////TODO: detect if start or end have been removed, and delete them from DB...
//                    //if (null != note.Start)
//                    //{
//                    //    session.Save(note.Start);
//                    //}
//                    //if (null != note.End)
//                    //{
//                    //    session.Save(note.End);
//                    //}

//                    transaction.Commit();
//                }
//            }
//        }


//        public void DeleteNote(Note note)
//        {
//            if (null == note.ID)
//                return;

//            using (ISession session = NHibernateHelper.OpenSession())
//            using (ITransaction transaction = session.BeginTransaction())
//            {
//                note = session.Get<Note>(note.ID.Value);
//            }
//            lock (_destructiveOperationLockToken)
//            {
//                using (ISession session = NHibernateHelper.OpenSession())
//                using (ITransaction transaction = session.BeginTransaction())
//                {
//                    session.Delete(note);

//                    transaction.Commit();
//                }
//            }
//        }

//        public void DeleteCourse(Course course)
//        {
//            //make sure we have all the notes and stuff.
//            course = GetCourse(course.ID.Value);
//            lock (_destructiveOperationLockToken)
//            {
//                using (ISession session = NHibernateHelper.OpenSession())
//                using (ITransaction transaction = session.BeginTransaction())
//                {
//                    foreach (Note note in course.Notes)
//                    {
//                        session.Delete(note);
//                        if (null != note.Start)
//                        {
//                            session.Delete(note.Start);
//                        }
//                        if (null != note.End)
//                        {
//                            session.Delete(note.End);
//                        }
//                    }

//                    foreach (Track track in course.Tracks)
//                    {
//                        session.Delete(track);
//                    }

//                    session.Delete(course);


//                    transaction.Commit();
//                }
//            }
//        }
//    }
//}
