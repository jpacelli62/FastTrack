using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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
            Assert.NotNull(this.Db.Mapper);
            Assert.NotNull(this.Db.Mappings.Value);
        }
    }
}
