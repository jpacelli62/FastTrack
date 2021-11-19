using System.Collections.Generic;

namespace Faaast.Metadata
{
    public class MetaModel<TModel> : IMetaModel<TModel> where TModel : class
    {
        private Dictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

        public T Get<T>(Metadata<TModel, T> metadata)
        {
            T result = default;
            if (this.Metadata.TryGetValue(metadata.Name, out var value))
            {
                result = (T)value;
            }

            return result;
        }

        public bool Has<T>(Metadata<TModel, T> metadata) => this.Metadata.ContainsKey(metadata.Name);

        public void Set<T>(Metadata<TModel, T> metadata, T value) => this.Metadata[metadata.Name] = value;
    }
}
