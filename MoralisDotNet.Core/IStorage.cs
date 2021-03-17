namespace MoralisDotNet.Core
{
    using System.Collections.Generic;

    public interface IStorage<T>
    {
        public T GetItem(string key);
        public void SetItem(string key, T value) ;
        public void RemoveItem(string key);

        public IEnumerable<string> GetAllKeys();

        public void Clear();
    }
}