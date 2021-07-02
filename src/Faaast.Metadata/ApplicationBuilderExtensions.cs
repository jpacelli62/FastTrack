using Faaast.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Faaast
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
