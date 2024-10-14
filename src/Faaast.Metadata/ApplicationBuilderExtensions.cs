using System.Collections.Generic;
using System;
using Faaast.Metadata;
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

        public static IServiceCollection AddMetadata(this IServiceCollection services, Dictionary<Type, IDtoClass> definitions)
        {
            var mapper = new DefaultObjectMapper(definitions);
            services.TryAddSingleton<IObjectMapper>(mapper);
            return services;
        }
    }
}
