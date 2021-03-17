namespace MoralisDotNet.Core
{
    using System.Collections.Generic;
    using System.Linq;
    public class StorageController<T> : IStorage<T>
    {
        public StorageController()
        {
            MemMap = new Dictionary<string, T>();
            
        }
        private Dictionary<string, T> MemMap { get; set; }

        public T GetItem(string key) => MemMap.ContainsKey(key) ? MemMap[key] : default(T);
        public void SetItem(string key, T value) => MemMap[key] = value;
        public void RemoveItem(string key) => MemMap[key] = default(T);
       
        public IEnumerable<string> GetAllKeys() => MemMap.Select(s=> s.Key);

        public void Clear() => MemMap.Clear();
    }
}