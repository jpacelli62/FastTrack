using System;

namespace FastTrack.Metadata
{
    public interface IObjectMapper
	{
        DtoClass Get(Type type);
    }
}
