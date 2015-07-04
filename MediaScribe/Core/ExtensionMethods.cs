using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.MediaScribe.Core
{
    public static class ExtensionMethods
    {
        public static IEnumerable<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");

            if (string.IsNullOrEmpty(str))
                yield break;

            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    break;
                yield return index;
            }
        }

        public static void AddRange<TSource>(this IList<TSource> mainCollection, IEnumerable<TSource> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                mainCollection.Add(item);
            }
        }
    }
}