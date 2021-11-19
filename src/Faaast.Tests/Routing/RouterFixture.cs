using System;
using Faaast.SeoRouter;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Faaast.Tests.Routing
{
    public class RouterFixture
    {
        public IConfiguration Configuration { get; set; }

        internal class FakeContext : IHttpContextAccessor
        {
            public HttpContext HttpContext { get; set; }
        }

        public RouterFixture() => this.Configuration = new ConfigurationBuilder()
                //.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                //.AddJsonFile("appsettings.Tests.json")
                .Build();

        public static IServiceProvider BuildProvider(Action<ServiceCollection> config)
        {
            var services = new ServiceCollection();
            config(services);

            var context = new FakeContext();
            services.AddSingleton<IHttpContextAccessor>(context);
            services.AddSeoRouter();
            var provider = services.BuildServiceProvider();

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(x => x.RequestServices).Returns(provider);
            context.HttpContext = httpContext.Object;
            return provider;
        }

        public static Router BuildRouterWith(IRouteProvider provider, out IServiceProvider services)
        {
            services = BuildProvider(config => config.AddSingleton(provider));

            var router = services.GetRequiredService<Router>();
            return router;
        }
    }
}
