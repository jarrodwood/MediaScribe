using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.Notemaker.Model;

namespace JayDev.Notemaker.ViewModel
{
    class CourseMaintenanceViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private CourseRepository _repo;

        public CourseMaintenanceViewModel(CourseRepository repo)
        {
            _repo = repo;
        }
    }
}
