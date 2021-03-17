namespace MoralisDotNet.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public class LocalDatastoreController<T> : ILocalDatastore<T>
    {
        readonly Storage<T> _storage;
        public LocalDatastoreController(Storage<T>? storage = null)
        {
            _storage ??= storage;
        }

        public R FromPinWithName<R>(string name) where R : T =>  (R)_storage.GetItem(name);
        public void PinWithName(string name, T value) => _storage.SetItem(name, value);
        public void UnpinWithName(string name) => _storage.RemoveItem(name);
        public void Clear() => _storage.Clear();

        public IEnumerable<T> GetAllContents() => _storage.GetAllKeys().Select(key => _storage.GetItem(key)).ToList();
    }
}