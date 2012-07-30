using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace JayDev.Notemaker
{
    [DataContract]
    public class CourseList
    {
        [DataMember]
        public List<Course> Courses = new List<Course>();
        [DataMember]
        public string VersionNumber { get; set; }
    }
}
