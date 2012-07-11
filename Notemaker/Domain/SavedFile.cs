using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace JayDev.Notemaker
{
    [DataContract]
    public class SavedFile : ISavedFile
    {
        string ISavedFile.LoadedFromFileName { get; set; }
        [DataMember(Name = "SavedDateTime")]
        DateTime ISavedFile.SavedDateTime { get; set; }
    }
}
