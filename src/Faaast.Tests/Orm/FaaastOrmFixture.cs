using System;
using System.Linq;
using Faaast.Tests.Orm.Fake;
using Faaast.Tests.Orm.FakeConnection;
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

        public static FakeDbConnection CreateFakeConnection()
        {
            FakeDbConnection connection = new()
            {
                Command = new FakeCommand()
                {
                    Reader = new FakeDataReader()
                    {
                        Count = 10000000,
                        columns = Enumerable.Range(1, 7).Select(i => "v" + i).ToList(),
                        Values = new object[] { 123, "lorem ipsum", DateTime.Now, Guid.NewGuid(), 3.14f, (long)89765464, 0.001d },
                    }
                }
            };
            return connection;
        }
    }
}
