using Faaast.Orm.Resolver;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddConventionMapping(this IServiceCollection services, NamingConvention convention)
        {
            DefaultTypeResolver resolver = new DefaultTypeResolver(convention);
            services.TryAddSingleton<ITypeResolver>(resolver);
            return services;
        }

        public static IServiceCollection AddConventionMapping(this IServiceCollection services, Action<NamingConvention> config)
        {
            NamingConvention convention = new NamingConvention();
            config(convention);
            return services.AddConventionMapping(convention);
        }
    }
}
