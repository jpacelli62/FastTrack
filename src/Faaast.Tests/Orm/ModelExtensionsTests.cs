using Faaast.Orm;
using Faaast.Orm.Model;
using Faaast.Tests.Orm.Fixture;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class ModelExtensionsTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public ModelExtensionsTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        [Fact]
        public void Name()
        {
            var columnName = "MyColumn";
            var test = new Column(columnName);
            Assert.Equal(columnName, test.Name);
        }

        [Fact]
        public void IsIdentity()
        {
            var test = new Column(string.Empty);
            test.IsIdentity();
            Assert.True(test.Identity);
        }

        [Fact]
        public void IsComputed()
        {
            var test = new Column(string.Empty);
            test.IsComputed();
            Assert.True(test.Computed);
        }

        [Fact]
        public void IsPrimaryKey()
        {
            var test = new Column(string.Empty);
            test.IsPrimaryKey();
            Assert.True(test.PrimaryKey);
        }

        [Fact]
        public void PrimaryKeyColumn()
        {
            var test = new SimpleTypeMapping<SimpleModel>();
            test.ToTable("tbl");
            test.Map(x => x.V1, "id").IsPrimaryKey();
            var columns = test.Table.Table.PrimaryKeyColumns();
            Assert.Single(columns);
            Assert.Single(test.Table.Table.Get(DbMeta.PrimaryKeyColumns));
        }
    }
}
