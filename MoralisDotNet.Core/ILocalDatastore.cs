namespace MoralisDotNet.Core
{
    using System.Collections.Generic;

    public interface ILocalDatastore<T>
    {
        public R FromPinWithName<R>(string name) where R : T;
        public void PinWithName(string name, T value);
        public void UnpinWithName(string name);
        public void Clear();

        public IEnumerable<T> GetAllContents();
    }
}