using System;
using System.Collections;
using System.Collections.Generic;

namespace Faaast.Metadata
{
    public abstract class DtoClass : MetaModel<IDtoClass>, IDtoClass
    {
        public virtual string Name { get; internal set; }

        public virtual Type Type { get; internal set; }

        public Dictionary<string, IDtoProperty> Properties { get; set; }

        public abstract object CreateInstance();

        public virtual IDtoProperty this[string propertyName]
        {
            get => this.Properties.TryGetValue(propertyName, out var property) ? property : null;
            set => this.Properties[propertyName] = value;
        }

        protected DtoClass(Type type)
        {
            this.Type = type;
            this.Name = type.Name;
            this.Properties = new Dictionary<string, IDtoProperty>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerator<IDtoProperty> GetEnumerator() => this.Properties.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Properties.Values.GetEnumerator();
    }
}
