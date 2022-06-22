using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace andywiecko.ECS
{
    public static class SystemExtensions
    {
        public static string ToNonPascal(this string s) => Regex.Replace(s, "[A-Z]", " $0").Trim();

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static T[] GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>().ToArray();
    }
}