namespace MoralisDotNet.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Parse;

    public class LocalDatastore<T> : ILocalDatastore<T> where T : ParseObject
    {
        readonly LocalDatastoreController<T> _localDatastoreController;
        readonly ParseClient _client;
        public LocalDatastore(ParseClient client, LocalDatastoreController<T>? localDatastoreController = null)
        {
            _localDatastoreController ??= localDatastoreController;
            _client = client;
        }
        public bool IsEnabled { get; set; }
        public bool IsSyncing { get; set; }

        public R FromPinWithName<R>(string name) where R : T => _localDatastoreController.FromPinWithName<R>(name);
        public void PinWithName(string name, T value) => _localDatastoreController.PinWithName(name, value);
        public void UnpinWithName(string name) => _localDatastoreController.UnpinWithName(name);
        public void Clear() => _localDatastoreController.Clear();

        public IEnumerable<T> GetAllContents() => _localDatastoreController.GetAllContents();

        public async Task UpdateFromServer()
        {
            if (!IsEnabled || IsSyncing)
                return;
            var localDatastore = GetAllContents();
            var keys = new List<ParseObject>();
            foreach (var key in localDatastore)
            {
                // TODO: Check so that we really should compare with objectId
                if (key.ObjectId.StartsWith(LocalDatastoreUtils.OBJECT_PREFIX))
                {

                    keys.Add(key);
                }
            }
            if (keys.Count == 0)
            {
                return;
            }
            IsSyncing = true;
            var pointerHash = new Dictionary<string, List<string>>();
            foreach (ParseObject key in keys)
            {
                if (key is ParseUser)
                {

                }

                if (key.ObjectId.StartsWith("local"))
                {
                    continue;
                }

                if (!(pointerHash.ContainsKey(key.ClassName)))
                {
                    pointerHash[key.ClassName] = new List<string>();
                }
                pointerHash[key.ClassName].Add(key.ObjectId);
            }

            var queryTasks = new List<Task<IEnumerable<ParseObject>>>();
            foreach (var classname in pointerHash.Select(p => p.Key))
            {
                var objectIds = pointerHash[classname];
                var query = new ParseQuery<ParseObject>(_client, classname);
                query.Limit(objectIds.Count);
                if (objectIds.Count == 1)
                {
                    query.WhereEqualTo("objectId", objectIds.First());
                }
                else
                {
                    query.WhereContainedIn("objectId", objectIds);
                }

                queryTasks.Add(query.FindAsync());
            }

            try
            {
                foreach (var queryResult in await Task.WhenAll(queryTasks))
                {
                    foreach (var updatedObject in queryResult)
                    {
                        // TODO Should we really cast to T here?
                        PinWithName(updatedObject.ObjectId, updatedObject as T);
                    }
                }

                IsSyncing = false;
            }
            catch
            {
                IsSyncing = false;
                throw;
            }
        }
        public static string GetKeyForObject(T value)
        {
            var objectId = value.ObjectId ?? value.Get<string>("Id");
            return $"{LocalDatastoreUtils.OBJECT_PREFIX}{value.ClassName}_{objectId}";
        }

        public static string GetPinName(string pinName)
        {
            if (string.IsNullOrWhiteSpace(pinName) || pinName == LocalDatastoreUtils.DEFAULT_PIN)
            {
                return LocalDatastoreUtils.DEFAULT_PIN;
            }
            return LocalDatastoreUtils.PIN_PREFIX + pinName;
        }

        public bool CheckIfEnabled()
        {
            if (!IsEnabled)
            {
                Console.WriteLine("EnableLocalDatastore() must be called first");
            }
            return IsEnabled;

        }
    }
}