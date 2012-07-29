using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.Notemaker.Model
{
    public class CourseRepository
    {
        public CourseRepository() { }

        //public List<Course> GetCourseList()
        //{
        //    CourseList list = DataAccess.GetCourseList();
        //    return list.Courses;
        //}

        //public SaveResult SaveCourseList(List<Course> list)
        //{
        //    var oldList = DataAccess.GetCourseList();
        //    oldList.Courses = list;
        //    return DataAccess.SaveCourseList(oldList);
        //}
        //public SaveResult SaveCourse(Course courseToSave) {

        //    var allCourses = GetCourseList();
        //    for(int i = 0; i < allCourses.Count; i++)
        //    {
        //        if (allCourses[i].Name == courseToSave.Name)
        //        {
        //            allCourses[i] = courseToSave;
        //            SaveCourseList(allCourses);
        //            break;
        //        }
        //    }
        //    return new SaveResult();
        //}

        public List<Course> GetCourseList()
        {
            return new List<Course>(Course.FindAll());
        }

        public SaveResult SaveCourseList(List<Course> list)
        {
            foreach (Course course in list)
            {
                course.SaveAndFlush();
            }
            return new SaveResult();
        }

        public SaveResult SaveCourse(Course courseToSave)
        {
            courseToSave.SaveAndFlush();
            return new SaveResult();
        }

        public void SaveNote(Course parentCourse, Note note)
        {
            note.ParentCourse = parentCourse;
            note.SaveAndFlush();
        }
    }
}
