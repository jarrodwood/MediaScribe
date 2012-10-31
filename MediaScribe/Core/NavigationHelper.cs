using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.Core
{
    class NavigationHelper
    {
    }


    public class NavigateArgs
    {
        public Course Course { get; set; }
        public NavigateMessage Message { get; set; }
        public TabChangeSource Source { get; set; }
        public NavigateArgs(NavigateMessage message)
        {
            this.Message = message;
        }
        public NavigateArgs(NavigateMessage message, TabChangeSource source)
        {
            this.Message = message;
            this.Source = source;
        }
        public NavigateArgs(NavigateMessage message, Course course, TabChangeSource source)
        {
            this.Message = message;
            this.Course = course;
            this.Source = source;
        }
    }

}
