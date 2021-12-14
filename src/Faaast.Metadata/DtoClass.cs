using System;
using System.Collections;
using System.Collections.Generic;

namespace Faaast.Metadata
{
    public class DtoClass : MetaModel<DtoClass>, IEnumerable<DtoProperty>
    {
        public string Name { get; set; }

        public Type Type { get; set; }

        private Dictionary<string, DtoProperty> Properties { get; set; }

        public Func<object> Activator { get; set; }

        public DtoProperty this[string propertyName]
        {
            get => this.Properties.TryGetValue(propertyName, out var property) ? property : null;
            set => this.Properties[propertyName] = value;
        }

        public DtoClass(Type type)
        {
            this.Type = type;
            this.Name = type.Name;
            this.Properties = new Dictionary<string, DtoProperty>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerator<DtoProperty> GetEnumerator() => this.Properties.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Properties.Values.GetEnumerator();
    }
}
