using System;
using System.Linq;
using Faaast.Metadata;
using Faaast.Orm;
using Faaast.Tests.Orm.Fixture;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class SimpleTypeMappingTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public SimpleTypeMappingTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        [Fact]
        public void Map_NotMemberExpression()
        {
            var test = new SimpleTypeMapping<SimpleModel>();
            Assert.Throws<ArgumentException>(() => test.Map(x => x.V2.ToUpper(), "Column"));
        }

        [Fact]
        public void ToDatabase()
        {
            var test = new SimpleTypeMapping<SimpleModel>();
            var dbname = "mydb";
            test.ToDatabase(dbname);
            Assert.Equal(dbname, test.Table.Database);
        }

        [Fact]
        public void ToTable()
        {
            var test = new SimpleTypeMapping<SimpleModel>();
            var tblName = "tbl";
            var schemaName = "dbo";
            test.ToTable(tblName, schemaName);
            Assert.Equal(tblName, test.Table.Table.Name);
            Assert.Equal(schemaName, test.Table.Table.Schema);
        }

        [Fact]
        public void Map()
        {
            var mapper = this.Fixture.Services.GetRequiredService<ObjectMapper>();
            var test = new SimpleTypeMapping<SimpleModel>();
            test.Map(x => x.V1, "id");
            test.Table.ObjectClass = mapper.Get(typeof(SimpleModel));
            test.Table.ColumnMappings.First().Property = test.Table.ObjectClass[nameof(SimpleModel.V1)];
            test.Table.Init();
            Assert.Single(test.Table.PropertyToColumn);
            Assert.Single(test.Table.ColumnToProperty);
        }
    }
}
