namespace MoralisDotNet.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Parse.Abstractions.Infrastructure;

    public class LocalDataCache : IDataCache<string, object>
    {
        public LocalDataCache()
        {
            Storage = new Dictionary<string, object>();
        }
        public Dictionary<string,object> Storage { get; set; }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Storage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if (Storage.ContainsKey(item.Key))
            {
                Storage[item.Key] = item.Value;
                return;
            }
            Storage.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Storage.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Storage.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Storage.Remove(item.Key);
        }

        public int Count => Storage.Count;
        public bool IsReadOnly => false;
        public void Add(string key, object value)
        {
            if (Storage.ContainsKey(key))
            {
                Storage[key] = value;
                return;
            }
            Storage.Add(key,value);
        }

        public bool ContainsKey(string key)
        {
            return Storage.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Storage.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return Storage.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get => Storage[key];
            set => Storage[key] = value;
        }

        public ICollection<string> Keys => Storage.Keys;
        public ICollection<object> Values => Storage.Values;
        public async Task AddAsync(string key, object value)
        {
            if (Storage.ContainsKey(key))
            {
                Storage[key] = value;
                return;
            }
            
            Storage.Add(key,value);
        }

        public async Task RemoveAsync(string key)
        {
            Storage.Remove(key);
        }
    }
}
