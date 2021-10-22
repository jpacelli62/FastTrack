using Faaast;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddFaaastOrm(this IServiceCollection services)
        {
            services.AddMetadata();
            services.AddDatabaseModel();
            return services;
        }


    }
}
