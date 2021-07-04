//using Faaast.DatabaseModel;
//using Faaast.Metadata;
//using Faaast.Orm.Reader;
//using Faaast.Tests.Orm.Fixtures;
//using Microsoft.Extensions.DependencyInjection;
//using System.Diagnostics;
//using System.Linq;
//using Xunit;

//namespace Faaast.Tests.Orm
//{
//    public class FaaastOrmTests : IClassFixture<FaaastOrmFixture>
//    {
//        public FaaastOrmFixture Fixture { get; set; }

//        public FaaastOrmTests(FaaastOrmFixture fixture)
//        {
//            Fixture = fixture;
//        }

//        [Fact]
//        public void Check_Dommel_mapping()
//        {
//            var dbSet = Fixture.GetOrm(out var provider);
//            var dbStore = provider.GetService<IDatabaseStore>();
//            Assert.NotNull(dbStore);
//            var site = dbStore["site"];
//            Assert.NotNull(site);
//            Assert.Equal(1, site.Tables.Count);

//            var table = site.Tables.First();
//            Assert.Equal("SimpleModel", table.Name);
//            Assert.Equal(7, table.Columns.Count);




//            //var mapper = provider.GetService<IObjectMapper>();

//            //Stopwatch watch1 = new Stopwatch();
//            //watch1.Start();
//            //var co1 = Fixture.CreateFakeConnection();
//            //var fastTrackResult = dbSet.Query<SimpleModel>(new FaaastCommand(mapper, co1, "select * from SimpleModel")).ToList();
//            //watch1.Stop();
//            //Debugger.Log(1, "fastTrackResult", watch1.ElapsedMilliseconds.ToString());

//            //Stopwatch watch2 = new Stopwatch();
//            //watch2.Start();
//            //var co2 = Fixture.CreateFakeConnection();
//            //var dapperResult = Dapper.SqlMapper.Query(co2, "select * from SimpleModel").ToList();
//            //watch2.Stop();
//            //Debugger.Log(1, "dapperResult", watch1.ElapsedMilliseconds.ToString());
//        }
//    }
//}
