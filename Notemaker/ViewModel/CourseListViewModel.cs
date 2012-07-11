using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using JayDev.Notemaker.Model;

namespace JayDev.Notemaker.ViewModel
{
    class CourseListViewModel : ViewModelBase
    {
        private readonly Dispatcher currentDispatcher;
        public ObservableCollection<Course> Courses { get; set; }

        public CourseListViewModel()
        {
            //Note the dispatcher of the UI thread, used to create this view model. We'll need this to update collections used by the UI
            this.currentDispatcher = Dispatcher.CurrentDispatcher;

            CourseRepository service = new CourseRepository();
            List<Course> list = service.GetCourseList();
            Courses = new ObservableCollection<Course>(list);
        }
    }
}
