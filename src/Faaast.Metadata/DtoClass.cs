using System;
using System.Collections;
using System.Collections.Generic;

namespace Faaast.Metadata
{
    public abstract class DtoClass : MetaModel<DtoClass>, IEnumerable<DtoProperty>
    {
        public virtual string Name { get; internal set; }

        public virtual Type Type { get; internal set; }

        public Dictionary<string, DtoProperty> Properties { get; set; }

        public int PropertiesCount => this.Properties.Count;

        public abstract object CreateInstance();

        public virtual DtoProperty this[string propertyName]
        {
            get => this.Properties.TryGetValue(propertyName, out var property) ? property : null;
            set => this.Properties[propertyName] = value;
        }

        protected DtoClass(Type type)
        {
            this.Type = type;
            this.Name = type.Name;
            this.Properties = new Dictionary<string, DtoProperty>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerator<DtoProperty> GetEnumerator() => this.Properties.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Properties.Values.GetEnumerator();
    }
}
