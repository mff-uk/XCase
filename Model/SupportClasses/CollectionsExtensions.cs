using System;
using System.Collections.Generic;
using System.Linq;

namespace XCase.Model
{
    public static class CollectionsExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return collection.Count() == 0;
        }

        public static void AddIfNotContained<T>(this ICollection<T> collection, T item)
        {
            if (!collection.Contains(item))
                collection.Add(item);
        }

        public static string ConcatWithSeparator<T>(this IEnumerable<T> xpathExpressions, string op)
        {
            string result = string.Empty;
            foreach (T expr in xpathExpressions)
            {
                result += expr + op;
            }
            if (result.Length > 0)
                result = result.Remove(result.Length - op.Length, op.Length);
            return result;
        }

        public static string ConcatWithSeparator<T>(this IEnumerable<T> xpathExpressions, Func<T, string> stringConverter, string op)
        {
            string result = string.Empty;
            foreach (T expr in xpathExpressions)
            {
                result += stringConverter(expr) + op;
            }
            if (result.Length > 0)
                result = result.Remove(result.Length - op.Length, op.Length);
            return result;
        }
    }
}