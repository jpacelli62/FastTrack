using System;
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
        public static IServiceCollection AddFaaastRouter(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

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

        public static IApplicationBuilder UseFaaastRouter(this IApplicationBuilder app, Action<ISeoRouteBuilder> routes = null)
        {
            var router = app.ApplicationServices.GetRequiredService<Router>();
            app.ApplicationServices.GetRequiredService<IRouteProvider>();
            if (routes != null)
            {
                ISeoRouteBuilder builder = new SeoRouteBuilder(app.ApplicationServices);
                routes(builder);
                router.StaticRoutes = builder.Build();
            }
            else
            {
                router.StaticRoutes = new RoutingRules();
            }

            app.UseRouter(router);
            return app;
        }
    }
}
