using Faaast.Orm;
using Faaast.Orm.Mapping;
using Faaast.Tests.Orm.Fixture;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class TableMappingTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public TableMappingTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        [Fact]
        public void Init()
        {
            var test = new SimpleTypeMapping<SimpleModel>();
            var dbname = "mydb";
            test.ToDatabase(dbname);
            Assert.Equal(dbname, test.Table.Database);
        }
    }
}
