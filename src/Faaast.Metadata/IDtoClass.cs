using System;
using System.Collections.Generic;

namespace Faaast.Metadata
{
    public interface IDtoClass : IEnumerable<IDtoProperty>, IMetaModel<IDtoClass>
    {
        IDtoProperty this[string propertyName] { get; set; }

        string Name { get; }

        Type Type { get; }

        object CreateInstance();
    }
}