using Faaast.Orm.Model;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddFaaastOrm(this IServiceCollection services)
        {
            services.AddMetadata();
            services.TryAddSingleton<IDatabaseStore, DatabaseStore>();
            return services;
        }
    }
}
