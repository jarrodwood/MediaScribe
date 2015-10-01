using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Windows.Controls;

namespace JayDev.MediaScribe.View
{
    public interface IController
    {
        List<Course> AllCourses { get; }

        Course LastCourse { get; }

        void Initialize(MainWindow mainWindow, TabControl tabControl, UnityContainer unityContainer);

        void UpdateCourseInMemory(Course course);

        void RemoveCourseFromMemory(Course course);
        bool IsFullscreen { get; }
    }
}
