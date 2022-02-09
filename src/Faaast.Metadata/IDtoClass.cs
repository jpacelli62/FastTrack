using System;
using System.Collections.Generic;

namespace Faaast.Metadata
{
    public interface IDtoClass : IEnumerable<IDtoProperty>, IMetaModel<IDtoClass>
    {
        IDtoProperty this[string propertyName] { get; set; }

        int PropertiesCount { get; }

        string Name { get; }

        Type Type { get; }

        object CreateInstance();
    }
}