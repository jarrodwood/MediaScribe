using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

namespace JayDev.MediaScribe.View
{
    public interface IController
    {
        List<Course> AllCourses { get; }

        void Initialize(MainWindow mainWindow, JayDev.MediaScribe.View.Controls.MediaScribeMainTabControl tabControl, UnityContainer unityContainer);

        void UpdateCourseInMemory(Course course);

        void RemoveCourseFromMemory(Course course);
        bool IsFullscreen { get; }
    }
}
