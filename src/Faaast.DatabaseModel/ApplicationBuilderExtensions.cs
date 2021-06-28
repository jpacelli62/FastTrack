using Faaast.DatabaseModel;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddDatabaseModel(this IServiceCollection services)
        {
            services.TryAddSingleton<IDatabaseStore, DatabaseStore>();
            return services;
        }
    }
}
