//using Faaast.DatabaseModel;
//using Faaast.Orm;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Linq;

//namespace Faaast.Tests.Orm.Fixtures
//{
//    public class FaaastOrmFixture
//    {
//        public FaaastOrmFixture()
//        {

//        }

//        public FaaastOrm GetOrm(out ServiceProvider provider)
//        {
//            ServiceCollection services = new ServiceCollection();

//            services.AddFaaastOrm(convention => convention
//                .Match(name => !name.Contains("sys."))
//                .RemoveTablePrefixes("tbl_", "joi_")
//                .AddSuffixToName("Dto"));

//            provider = services.BuildServiceProvider();
//            provider.WithDommelFluentMap(
//                new ConnectionSettings("site", SqlEngine.SQLServer, "sampleConnexionString"),
//                config => config.AddMap(new SimpleModelDommelMap())
//            );

//            var dbset = new FaaastOrm(provider);
//            return dbset;
//        }

//        public FakeDbConnection CreateFakeConnection()
//        {
//            var connection = new FakeDbConnection();
//            connection.Command = new FakeCommand()
//            {
//                Reader = new FakeDataReader()
//                {
//                    Count = 10000000,
//                    columns = Enumerable.Range(1, 7).Select(i => "v" + i).ToList(),
//                    Values = new object[] { 123, "lorem ipsum", DateTime.Now, Guid.NewGuid(), 3.14f, (long)89765464, 0.001d },
//                }
//            };
//            return connection;
//        }
//    }
//}
