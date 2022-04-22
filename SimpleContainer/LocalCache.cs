using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleContainer
{
    public class LocalCache
    {
        private static readonly ConcurrentDictionary<string, ValueItem> dicValues = new ConcurrentDictionary<string, ValueItem>();

        private static readonly Lazy<LocalCache> _lazy = new Lazy<LocalCache>(() => new LocalCache());
        public static LocalCache Instance => _lazy.Value;


        public void TryAdd(string key, ValueItem valueItem)
        {
            string dicKey = string.IsNullOrEmpty(key) ? valueItem.ToString() : key;
            if (dicValues.ContainsKey(dicKey))
            {
                return;
            }

            dicValues.TryAdd(dicKey, valueItem);
        }

        public void AddOrUpdate(string key, ValueItem valueItem)
        {
            string dicKey = string.IsNullOrEmpty(key) ? valueItem.ToString() : key;
            if (dicValues.TryGetValue(dicKey.GetType().Name, out var val))
            {
                dicValues.TryUpdate(dicKey, valueItem, val);
                return;
            }
            dicValues.TryAdd(dicKey, valueItem);
        }

        public bool ContainsKey(string key)
        {
            return dicValues.ContainsKey(key);
        }

        public string GetKeyByInterface(Type interfaceType)
        {
            return dicValues.Values.FirstOrDefault(item => item.InterfaceType == interfaceType).ToString();
        }

        public ValueItem TryGetValue(string key)
        {
            if (dicValues.TryGetValue(key, out var valueItem))
            {
                return valueItem;
            }

            return null;
        }

    }
}
