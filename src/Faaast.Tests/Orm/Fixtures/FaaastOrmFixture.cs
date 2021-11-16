using Faaast.DatabaseModel;
using Faaast.Orm;
using Faaast.Tests.Orm.FakeConnection;
using Microsoft.Extensions.DependencyInjection;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Faaast.Tests.Orm.Fixtures
{
    public class FaaastOrmFixture
    {
        public class FakeDB : FaaastQueryDb
        {
            Compiler _compiler;
            public FakeDB(IServiceProvider provider) : base(provider)
            {
                _compiler = new SqlServerCompiler();
            }

            protected override Compiler Compiler => _compiler;

            public void SetCompiler (Compiler instance)
            {
                this._compiler = instance;
            }

            public override ConnectionSettings Connection => new ConnectionSettings("connectionName", null, "sampleConnectionString");

            protected override IEnumerable<SimpleTypeMapping> GetMappings()
            {
                var mapping = new SimpleTypeMapping<SimpleModel>();
                mapping.ToTable("sampleTable");
                mapping.Map(x => x.V1, "v1").IsPrimaryKey().IsIdentity();
                mapping.Map(x => x.V2, "V2");
                mapping.Map(x => x.V3, "V3");
                mapping.Map(x => x.V4, "V4");
                mapping.Map(x => x.V5, "V5");
                mapping.Map(x => x.V6, "V6");
                mapping.Map(x => x.V7, "V7");
                mapping.Map(x => x.V8, "V8");
                yield return mapping;
            }
        }

        public FaaastOrmFixture()
        {
        }

        public FakeDB GetDb(out ServiceProvider provider)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddFaaastOrm();
            services.AddSingleton<FakeDB>();
            provider = services.BuildServiceProvider();
            return provider.GetRequiredService<FakeDB>();
        }

        public FakeDbConnection CreateFakeConnection()
        {
            var connection = new FakeDbConnection();
            connection.Command = new FakeCommand()
            {
                Reader = new FakeDataReader()
                {
                    Count = 10000000,
                    columns = Enumerable.Range(1, 7).Select(i => "v" + i).ToList(),
                    Values = new object[] { 123, "lorem ipsum", DateTime.Now, Guid.NewGuid(), 3.14f, (long)89765464, 0.001d },
                }
            };
            return connection;
        }
    }
}
