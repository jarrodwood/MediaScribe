using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.MediaScribe.Core
{
    public static class ExtensionMethods
    {

        public static void AddRange<TSource>(this IList<TSource> mainCollection, IEnumerable<TSource> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                mainCollection.Add(item);
            }
        }
    }
}