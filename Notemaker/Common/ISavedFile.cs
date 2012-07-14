using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace JayDev.Notemaker
{
    interface ISavedFile
    {
        /// <summary>
        /// When an object is saved as a file on the drive, the object's name is used as the file name.
        /// However, the object's name may change during the application's runtime. This is used to
        /// record the original name for the object's saved file, so that we can detect if the name
        /// has changed, and tidy up files on the drive accordingly.
        /// </summary>
        string LoadedFromFileName { get; set; }
        DateTime SavedDateTime { get; set; }
    }
}
