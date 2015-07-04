using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.MediaScribe.Common
{
    public class HighlightMatch
    {
        public int MatchStartIndex { get; set; }
        public int MatchLength { get; set; }
        public bool CurrentMatch { get; set; }

        public HighlightMatch(int matchStartIndex, int matchLength, bool currentMatch = false)
        {
            this.MatchStartIndex = matchStartIndex;
            this.MatchLength = matchLength;
            this.CurrentMatch = currentMatch;
        }
    }
    class HighlightHelper
    {
    }
}
