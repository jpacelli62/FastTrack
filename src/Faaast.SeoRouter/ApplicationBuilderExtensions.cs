using Faaast.SeoRouter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddSeoRouter(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });  

            services.AddRouting();
            services.TryAddTransient<IInlineConstraintResolver, DefaultInlineConstraintResolver>();
            services.AddHttpContextAccessor();

#if NETSTANDARD2_0
            services.TryAddTransient<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddSingleton<ObjectPool<UriBuildingContext>>(s =>
            {
                var provider = s.GetRequiredService<ObjectPoolProvider>();
                return provider.Create<UriBuildingContext>(new UriBuilderContextPooledObjectPolicy());
            });
#endif

            services.TryAddSingleton<IMvcHandler, DefaultMvcHandler>();
            services.TryAddSingleton<SimpleTemplateBinderFactory>();
            services.TryAddSingleton<Router>();

            return services;
        }

        public static IApplicationBuilder UseSeoRouter(this IApplicationBuilder app)
        {
            app.UseRouter(app.ApplicationServices.GetRequiredService<Router>());
            return app;
        }
    }
}
