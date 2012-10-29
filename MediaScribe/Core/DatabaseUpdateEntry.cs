using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.MediaScribe.Core
{
    public struct DatabaseUpdateEntry
    {
        public string VersionNumber { get; set; }
        public string UpdateScript { get; set; }
    }
}
