using System;
using System.Diagnostics;

namespace Faaast.Metadata
{
    [DebuggerDisplay("{Name} ({Type.FullName})")]
    public class DtoProperty : MetaModel<IDtoProperty>, IDtoProperty
    {
        public virtual string Name { get; set; }

        public virtual Type Type { get; set; }

        public virtual bool CanRead { get; set; }

        public virtual bool CanWrite { get; set; }

        public virtual Func<object, object> ReadFunc { get; set; }

        public virtual Action<object, object> WriteFunc { get; set; }

        public virtual object Read(object instance) => this.ReadFunc(instance);

        public virtual void Write(object instance, object value) => this.WriteFunc(instance, value);

        public DtoProperty(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
