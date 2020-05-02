namespace DataBridge.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    /// <summary>
    /// DictionaryExtensions
    /// </summary>
    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> addDictionary)
        {
            if (addDictionary == null)
            {
                return dictionary;
            }

            foreach (var item in addDictionary)
            {
                dictionary.Add(item.Key, item.Value);
            }

            return dictionary;
        }

        public static IDictionary<TKey, TValue> AddOrUpdateRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> addDictionary)
        {
            if (addDictionary == null)
            {
                return dictionary;
            }

            foreach (var item in addDictionary)
            {
                dictionary.AddOrUpdate(item.Key, item.Value);
            }

            return dictionary;
        }

        public static IDictionary<TKey, TValue> AddRange<TKey, TValue, TA>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TA> collection, Func<TA, TKey> getKey, Func<TA, TValue> getValue)
        {
            if (collection == null)
            {
                return dictionary;
            }

            foreach (var item in collection)
            {
                dictionary.Add(getKey(item), getValue(item));
            }

            return dictionary;
        }

        public static IDictionary<TKey, TValue> CloneDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            IDictionary<TKey, TValue> result = dictionary.GetType().GetConstructor(new Type[0]).Invoke(null) as IDictionary<TKey, TValue>;

            foreach (var key in dictionary.Keys)
            {
                result.Add(key, dictionary[key]);
            }

            return result;
        }

        public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        public static IDictionary<string, object> AddOrAppend(this IDictionary<string, object> dictionary, string key, string value, string separator = ",")
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = !string.IsNullOrEmpty(dictionary[key].ToStringOrEmpty())
                                                ? dictionary[key] + separator + value
                                                : value;
            }
            else
            {
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return default(TValue);
        }

        public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            dictionary.Add(key, default(TValue));

            return default(TValue);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value
                 : defaultValueProvider();
        }

        public static IDictionary<TKey, TValue> RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> condition)
        {
            var temp = new List<TKey>();

            foreach (var item in dictionary)
            {
                if (!condition(item))
                {
                    temp.Add(item.Key);
                }
            }

            foreach (var itemKey in temp)
            {
                dictionary.Remove(itemKey);
            }

            return dictionary;
        }

        public static IDictionary<TKey, TValue> RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            if (values != null)
            {
                foreach (var x in values)
                {
                    dictionary.Remove(x);
                }
            }

            return dictionary;
        }

        public static void SetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                value = default(TValue);
                return false;
            }
            else
            {
                value = dictionary[key];
                dictionary.Remove(key);
                return true;
            }
        }

        public static DataTable ToDataTable<T>(this IDictionary<string, T> dictionary)
        {
            var list = new List<IDictionary<string, T>>();
            list.Add(dictionary);
            return ToDataTable<T>(list);
        }

        public static DataTable ToDataTable<T>(this IList<IDictionary<string, T>> dictionary)
        {
            DataTable dataTable = new DataTable();

            if (dictionary == null || !dictionary.Any())
            {
                return dataTable;
            }

            foreach (var column in dictionary.First().Select(c => new DataColumn(c.Key, c.Value != null ? c.Value.GetType()
                                                                                                        : typeof(T))))
            {
                dataTable.Columns.Add(column);
            }

            foreach (var row in dictionary.Select(
                r =>
                {
                    var dataRow = dataTable.NewRow();
                    r.ToList().ForEach(c => dataRow.SetField(c.Key, c.Value));
                    return dataRow;
                }))
            {
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var keyValuePair in keyValuePairs)
            {
                result.AddOrUpdate(keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public static TReturn GetValue<TReturn>(this IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null)
            {
                return default(TReturn);
            }

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key].ConvertTo<TReturn>();
            }

            return default(TReturn);
        }

        public static TReturn GetValueOrAdd<TReturn>(this IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null)
            {
                return default(TReturn);
            }

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key].ConvertTo<TReturn>();
            }
            dictionary.Add(key, default(TReturn));

            return default(TReturn);
        }

        public static TReturn GetValueOrDefault<TReturn>(this IDictionary<string, object> dictionary, string key, TReturn defaultValue)
        {
            TReturn value;
            if (dictionary.ContainsKey(key))
            {
                return dictionary.GetValue<TReturn>(key);
            }

            return default;
        }
    }

    public static class KeyValuePairExtensions
    {
        public static bool IsEmpty<T, TU>(this KeyValuePair<T, TU> pair)
        {
            return pair.Equals(new KeyValuePair<T, TU>());
        }
    }
}