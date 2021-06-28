using System.Collections.Generic;

namespace Faaast.Metadata
{
    public class MetaModel
    {
        private Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public T Get<T>(Metadata<T> metadata)
        {
            T result = default;
            if (this.Metadata.TryGetValue(metadata.Name, out object value))
            {
                result = (T)value;
            }

            return result;
        }

        public void Set<T>(Metadata<T> metadata, T value)
        {
            Metadata[metadata.Name] = value;
        }
    }
}
