using System;
using System.Collections;
using System.Collections.Generic;

namespace Faaast.Metadata
{
    public class DtoClass : MetaModel<DtoClass>, IEnumerable<DtoProperty>
    {
        public string Name { get; internal set; }

        public Type Type { get; internal set; }

        private Dictionary<string, DtoProperty> Properties { get; set; }

        public Func<object> Activator { get; internal set; }


        public DtoProperty this[string propertyName]
        {
            get
            {
                if (Properties.TryGetValue(propertyName, out var property))
                    return property;

                return null;
            }
            set
            {
                Properties[propertyName] = value;
            }
        }

        public DtoClass(Type type)
        {
            Type = type;
            Name = type.Name;
            Properties = new Dictionary<string, DtoProperty>();
        }

        public IEnumerator<DtoProperty> GetEnumerator()
        {
            return Properties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Properties.Values.GetEnumerator();
        }
    }
}
