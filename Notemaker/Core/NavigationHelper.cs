using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.Notemaker.Common;

namespace JayDev.Notemaker.Core
{
    class NavigationHelper
    {
    }


    public class NavigateArgs
    {
        public Course Course { get; set; }
        public NavigateMessage Message { get; set; }
        public NavigateArgs(NavigateMessage message)
        {
            this.Message = message;
        }
        public NavigateArgs(NavigateMessage message, Course course)
        {
            this.Message = message;
            this.Course = course;
        }
    }

}
