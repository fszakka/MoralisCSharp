namespace MoralisDotNet.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Parse.Abstractions.Infrastructure;

    public class MoralisCacheController : ICacheController
    {
        public MoralisCacheController()
        {
            storage = new LocalDataCache();
        }

        private LocalDataCache storage;

        public void Clear()
        {
            storage.Clear();
        }

        public FileInfo GetRelativeFile(string path)
        {
            throw new NotImplementedException();
        }

        public async Task TransferAsync(string originFilePath, string targetFilePath)
        {
            throw new NotImplementedException();
        }

        public async Task<IDataCache<string, object>> LoadAsync()
        {
            return storage;
        }

        public async Task<IDataCache<string, object>> SaveAsync(IDictionary<string, object> contents)
        {
            foreach (var content in contents)
            {
                storage.Add(content.Key, content.Value);
            }

            return storage;
        }
    }
}