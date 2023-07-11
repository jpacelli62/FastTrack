using System;

namespace Faaast.Metadata
{
    public interface IDtoProperty : IMetaModel<IDtoProperty>
    {
        bool CanRead { get; }
        bool CanWrite { get; }
        string Name { get; }
        Type Type { get; }
        Type NullableUnderlyingType { get; set; }
        bool Nullable { get; }
        object Read(object instance);
        void Write(object instance, object value);
    }
}