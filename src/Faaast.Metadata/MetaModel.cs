using System.Collections.Generic;

namespace Faaast.Metadata
{
    public class MetaModel<TModel> : IMetaModel<TModel> where TModel : class
    {
        private Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public T Get<T>(Metadata<TModel, T> metadata)
        {
            T result = default;
            if (this.Metadata.TryGetValue(metadata.Name, out object value))
            {
                result = (T)value;
            }

            return result;
        }

        public void Set<T>(Metadata<TModel, T> metadata, T value)
        {
            Metadata[metadata.Name] = value;
        }
    }
}
