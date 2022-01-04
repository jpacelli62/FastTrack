using System;
using System.Collections;
using System.Collections.Generic;

namespace Faaast.Metadata
{
    public class DtoClass : MetaModel<IDtoClass>, IDtoClass
    {
        public virtual string Name { get; internal set; }

        public virtual Type Type { get; internal set; }

        protected Dictionary<string, IDtoProperty> Properties { get; set; }

        public virtual Func<object> Activator { get; internal set; }

        public virtual object CreateInstance() => this.Activator();

        public virtual IDtoProperty this[string propertyName]
        {
            get => this.Properties.TryGetValue(propertyName, out var property) ? property : null;
            set => this.Properties[propertyName] = value;
        }

        public DtoClass(Type type)
        {
            this.Type = type;
            this.Name = type.Name;
            this.Properties = new Dictionary<string, IDtoProperty>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerator<IDtoProperty> GetEnumerator() => this.Properties.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Properties.Values.GetEnumerator();
    }
}
