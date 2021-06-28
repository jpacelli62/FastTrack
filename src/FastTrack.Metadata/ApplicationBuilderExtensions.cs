using FastTrack.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddMetadata(this IServiceCollection services)
        {
            services.TryAddSingleton<IObjectMapper, DefaultObjectMapper>();
            return services;
        }
    }
}
