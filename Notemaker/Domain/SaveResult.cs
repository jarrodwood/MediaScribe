using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.Notemaker
{
    public class SaveResult
    {
        public bool IsSaveSuccessful { get { return errors.Count == 0; } }
        private List<string> errors = new List<string>();
        public List<string> Errors { get { return errors; } }

        public string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (string error in errors)
            {
                builder.AppendLine(error);
                builder.AppendLine("\r\n");
            }
            return builder.ToString();
        }
    }
}
