using System;

namespace Faaast.Metadata
{
    public interface IObjectMapper
    {
        DtoClass Get(Type type);
    }
}
