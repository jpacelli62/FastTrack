using System;
using System.Diagnostics;

namespace Faaast.Metadata
{
    [DebuggerDisplay("{Name} ({Type.FullName})")]
    public class DtoProperty : MetaModel<DtoProperty>
    {
        public string Name { get; internal set; }

        public Type Type { get; internal set; }

        public bool CanRead { get; internal set; }

        public bool CanWrite { get; internal set; }

        public Func<object, object> Read { get; internal set; }

        public Action<object, object> Write { get; internal  set; }

        public DtoProperty(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
