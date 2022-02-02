using System;
using Microsoft.Extensions.DependencyInjection;

namespace Faaast.Tests.Orm.Fixtures
{
    public class FaaastOrmFixture
    {
        public IServiceProvider Services { get; set; }

        public FakeDB Db { get; set; }

        public FaaastOrmFixture()
        {
            var services = new ServiceCollection();
            services.AddFaaastOrm();
            services.AddSingleton<FakeDB>();
            this.Services = services.BuildServiceProvider();
            this.Db = this.Services.GetRequiredService<FakeDB>();
        }
    }
}
