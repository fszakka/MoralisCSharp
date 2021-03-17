namespace MoralisDotNet.Core
{
    using System.Collections.Generic;

    public class Storage<T> : IStorage<T>
    {
        readonly StorageController<T> _storageController;
        public Storage(StorageController<T>? controller = null) => _storageController ??= new StorageController<T>();

        public T GetItem(string key) => _storageController.GetItem(key);

        public void SetItem(string key, T value) => _storageController.SetItem(key,value);

        public void RemoveItem(string key) => _storageController.RemoveItem(key);

        public IEnumerable<string> GetAllKeys() => _storageController.GetAllKeys();

        public void Clear() => _storageController.Clear();
    }
}