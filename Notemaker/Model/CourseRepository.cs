using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.Notemaker.Model
{
    public class CourseRepository
    {
        public CourseRepository() { }

        public List<Course> GetCourseList()
        {
            CourseList list = DataAccess.GetCourseList();
            return list.Courses;
        }

        public SaveResult SaveCourseList(List<Course> list)
        {
            var oldList = DataAccess.GetCourseList();
            oldList.Courses = list;
            return DataAccess.SaveCourseList(oldList);
        }
        public Course GetCourse(string courseName)
        {
            Course course = DataAccess.GetCourse(courseName);
            return course;
        }

        public SaveResult SaveCourse(Course courseToSave) {

            var allCourses = GetCourseList();
            for(int i = 0; i < allCourses.Count; i++)
            {
                if (allCourses[i].Name == courseToSave.Name)
                {
                    allCourses[i] = courseToSave;
                    SaveCourseList(allCourses);
                    break;
                }
            }
            return new SaveResult();
            //#region Validation

            //if (string.IsNullOrEmpty(course.Name))
            //{
            //    throw new ValidationException<Course>("Please enter a name for the course", course);
            //}

            //////If the course is new, or if the course has changed names... ensure that we don't already have a file with that name.
            ////string courseLoadedFromFile = (course as ISavedFile).LoadedFromFileName;
            ////if ((string.IsNullOrEmpty(courseLoadedFromFile) || courseLoadedFromFile != sanitisedName)
            ////    && File.Exists(string.Format(GenericFilePath, sanitisedName)))
            ////{
            ////    throw new ValidationException<Course>("There is already a course with this name.", course);
            ////}

            //#endregion

            //SaveResult result = DataAccess.SaveCourse(course);
            //return result;
        }
    }
}
