﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using JayDev.MediaScribe.Common;
using System.Data.SQLite;

namespace JayDev.MediaScribe
{
    public static class XmlDataAccess
    {
        #region Constants

        private static readonly string ApplicationFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MediaScribe";
        private static readonly string GenericFilePath = ApplicationFolderPath + @"\{0}";
        private static readonly string CourseListFileName = "CourseList.xml";
        private static readonly string CourseListFilePath = string.Format(GenericFilePath, CourseListFileName);

        #endregion

        private static StreamWriter openCourseListStream = null;
        private static Dictionary<string, StreamWriter> openCourseStreams = new Dictionary<string, StreamWriter>();

        #region Public Static Methods

        public static CourseList GetCourseList()
        {
            try
            {
                if (isListChangedSinceLastSave)
                {
                    isListChangedSinceLastSave = false;
                }
                else
                {
                    return lastSavedCourseList;
                }
                #region Preparation
                //If this is the first time running the application, we won't have a data folder or a course list file... so we'll need to create some.

                //If data folder doesn't exist, create it.
                if (false == Directory.Exists(ApplicationFolderPath))
                {
                    Directory.CreateDirectory(ApplicationFolderPath);
                }
                string fileName = string.Format(GenericFilePath, CourseListFileName);

                //If we don't have a course list storage file, create a new, blank one
                if (false == File.Exists(fileName))
                {
                    string applicationVersionNumber = Utility.ApplicationVersionNumber;
                    CourseList emptyList = new CourseList()
                    {
                        VersionNumber = applicationVersionNumber
                    };
                    string emptyListXml = Serialize(emptyList);
                    File.WriteAllText(fileName, emptyListXml);
                }
                #endregion

                CourseList result = new CourseList();
                string xml = System.IO.File.ReadAllText(fileName);
                result = Deserialize<CourseList>(xml);

                if (result.VersionNumber != Utility.ApplicationVersionNumber)
                {
                    throw new ApplicationException("Todo: course data is outdated");
                }

                foreach (var course in result.Courses)
                {
                    foreach (Track track in course.Tracks)
                    {
                        track.ParentCourse = course;
                    }

                    foreach (Note note in course.Notes)
                    {
                        note.ParentCourse = course;

                        if (null != note.Start)
                        {
                            note.Start.Track = course.Tracks.First(x => x.FilePath == note.Start.Track.FilePath);
                        }
                        if (null != note.End)
                        {
                            note.End.Track = course.Tracks.First(x => x.FilePath == note.End.Track.FilePath);
                        }
                    }
                }

                lastSavedCourseList = Deserialize<CourseList>(xml);
                return result;
            }
            catch (IOException e)
            {
                throw new Exception("An error has occured attempting to load the course list.", e);
            }
        }

        static CourseList lastSavedCourseList = null;
        static bool isListChangedSinceLastSave = true;

        public static SaveResult SaveCourseList(CourseList list)
        {
            SaveResult result = new SaveResult();
            try
            {
                isListChangedSinceLastSave = true;

                if (File.Exists(string.Format(GenericFilePath, "bkup.xml")))
                {
                    File.Delete(string.Format(GenericFilePath, "bkup.xml"));
                }
                File.Copy(string.Format(GenericFilePath, CourseListFileName), string.Format(GenericFilePath, "bkup.xml"));
                string xmlData = XmlDataAccess.Serialize(list);
                string filePath = string.Format(GenericFilePath, CourseListFileName);
                System.IO.File.WriteAllText(filePath, xmlData, Encoding.UTF8);

                //Set the saved-file metadata, and get outta here
                //ISavedFile file = list as ISavedFile;
                //file.SavedDateTime = System.IO.File.GetLastWriteTime(filePath);
            }
            catch (Exception e)
            {
                result.Errors.Add("Error saving course list: " + e.ToString());
            }

            return result;
        }

        public static SaveResult SaveCourse(Course course)
        {
            SaveResult result = new SaveResult();

            string sanitisedName = MakeValidFileName(course.Name);
            string xmlData = XmlDataAccess.Serialize(course);
            string filePath = string.Format(GenericFilePath, sanitisedName);
            System.IO.File.WriteAllText(filePath, xmlData, Encoding.UTF8);

            CourseList list = GetCourseList();
            list.Courses.Add(course);
            SaveResult courseListSaveResult = SaveCourseList(list);
            if (false == result.IsSaveSuccessful)
            {
                result.Errors.Add("There was an error saving the course list: " + courseListSaveResult.ToString());
                return result;
            }

            //if (null == conn)
            //{
            //    string dbFilePath = string.Format(GenericFilePath, "MediaScribeTest.db");
            //    conn = new SQLiteConnection(@"Data Source=" + dbFilePath + ";Version=3;");
            //}



            return result;
        }

        public static Course GetCourse(string courseName)
        {
            Course result = new Course();

            string sanitisedName = MakeValidFileName(courseName);
            System.IO.File.ReadAllText(string.Format(GenericFilePath, sanitisedName));

            //TODO
            //foreach (var note in result.Notes)
            //{
            //    note.Start.ParentCourse = result;
            //    note.End.ParentCourse = result;
            //}
            return result;
        }

        #endregion

        #region Private Static Methods

        private static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}]+", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }

        private static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        private static T Deserialize<T>(string xml)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(memoryStream, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null);
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(reader);
            }
        }

        #endregion
    }
}
