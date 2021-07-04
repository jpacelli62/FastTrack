using Faaast.Orm.Resolver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Faaast
{
    public static partial class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddConventionMapping(this IServiceCollection services, Action<NamingConvention> config)
        {
            NamingConvention convention = new NamingConvention();
            config(convention);
            services.TryAddSingleton(convention);
            return services;
        }
    }
}
