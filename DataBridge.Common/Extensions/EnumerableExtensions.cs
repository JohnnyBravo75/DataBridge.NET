using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;

namespace DataBridge.Extensions
{
    public static class EnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> obj, T value)
        {
            return obj
                .Select((a, i) => (a.Equals(value)) ? i : -1)
                .Max();
        }

        public static int IndexOf<T>(this IEnumerable<T> obj, T value, IEqualityComparer<T> comparer)
        {
            return obj
                .Select((a, i) => (comparer.Equals(a, value)) ? i : -1)
                .Max();
        }

        public static IList<T> AddRange<T>(this IList<T> collection, IEnumerable<T> range)
        {
            foreach (var x in range)
            {
                collection.Add(x);
            }

            return collection;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T element in collection)
            {
                action(element);
            }

            return collection;
        }

        public static string ToString<T>(this IEnumerable<T> collection, Func<T, string> toString, string separator)
        {
            var sb = new StringBuilder();
            foreach (var item in collection)
            {
                sb.Append(toString(item));
                sb.Append(separator);
            }
            return sb.ToString(0, Math.Max(0, sb.Length - separator.Length));  // Remove at the end is faster
        }

        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> functionRecurse)
        {
            foreach (T item in source)
            {
                yield return item;

                IEnumerable<T> seqRecurse = functionRecurse(item);

                if (seqRecurse != null)
                {
                    foreach (T itemRecurse in Traverse(seqRecurse, functionRecurse))
                    {
                        yield return itemRecurse;
                    }
                }
            }
        }

        public static void Traverse<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> functionRecurse, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
                IEnumerable<T> seqRecurse = functionRecurse(item);

                if (seqRecurse != null)
                {
                    foreach (T itemRecurse in Traverse(seqRecurse, functionRecurse))
                    {
                        action(itemRecurse);
                    }
                }
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerableList)
        {
            if (enumerableList != null)
            {
                ////create an emtpy observable collection object
                var observableCollection = new ObservableCollection<T>();

                ////loop through all the records and add to observable collection object
                foreach (var item in enumerableList)
                {
                    observableCollection.Add(item);
                }
                ////return the populated observable collection
                return observableCollection;
            }

            return null;
        }

        /// <summary>
        /// Determines whether the collection is null or contains no elements.
        /// </summary>
        /// <typeparam name="T">The IEnumerable type.</typeparam>
        /// <param name="enumerable">The enumerable, which may be null or empty.</param>
        /// <returns>
        ///     <c>true</c> if the IEnumerable is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }

            var collection = enumerable as ICollection<T>;
            if (collection != null)
            {
                return collection.Count < 1;
            }
            return !enumerable.Any();
        }

        public static IDictionary<string, object> ToDictionary(this NameValueCollection col)
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            foreach (var k in col.AllKeys)
            {
                dict.Add(k, col[k]);
            }
            return dict;
        }
    }
}