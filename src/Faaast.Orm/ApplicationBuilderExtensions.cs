using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Reader;
using Faaast.Orm.Resolver;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddFaaastOrm(this IServiceCollection services, Action<NamingConvention> convention)
        {
            services.AddMetadata();
            services.AddDatabaseModel();
            services.AddConventionMapping(convention);
            return services;
        }

        public static IDatabase UseDatabase(this IServiceProvider services, Func<IDatabase> builder)
        {
            var database = builder();
            services.GetRequiredService<IDatabaseStore>()[database.Connexion.Name] = database;
            var resolver = services.GetRequiredService<ITypeResolver>();
            var mapper = services.GetRequiredService<IObjectMapper>();
            return database.Automap(resolver, mapper);
        }

        public static IDatabase UseDatabase<TResolver, TObjectMapper>(this IServiceProvider services, Func<IDatabase> builder) where TResolver : ITypeResolver where TObjectMapper : IObjectMapper
        {
            var database = builder();
            services.GetRequiredService<IDatabaseStore>()[database.Connexion.Name] = database;
            var resolver = services.GetRequiredService<TResolver>();
            var mapper = services.GetRequiredService<TObjectMapper>();
            return database.Automap(resolver, mapper);
        }
    }
}
