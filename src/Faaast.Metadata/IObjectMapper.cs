using System;

namespace Faaast.Metadata
{
    public interface IObjectMapper
    {
        IDtoClass Get(Type type);
    }
}
