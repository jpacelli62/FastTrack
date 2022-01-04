using System;
using System.Net;
using System.Threading.Tasks;
using Faaast.Authentication.OAuth2;
using Faaast.Orm;
using Faaast.Orm.Model;
using Faaast.Tests.Orm.Fixtures;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class OrmTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public OrmTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        [Fact]
        public void Mapping_is_populated()
        {
            var db = this.Fixture.Db;
            Assert.NotNull(db);
            Assert.NotNull(db.Mapper);
            var dbMapping = db.Mappings.Value;
            Assert.NotNull(dbMapping);
        }

        [Fact]
        public void Check_mapping()
        {
            var db = this.Fixture.Db;
            db.DbStore[db.Connection.Name] = db.Mappings.Value.Source;

            Assert.Equal(1, db.Mappings.Value.Mappings.Count);
            Assert.Equal(db.Connection.Name, db.Mappings.Value.Source.Connexion.Name);
            Assert.Equal(db.Connection.Engine, db.Mappings.Value.Source.Connexion.Engine);
            Assert.Equal("sampleConnectionString", db.Connection.ConnectionString(null));   
            
            var mapping = db.Mapping<SimpleModel>();
            Assert.NotNull(mapping);
            Assert.Equal(typeof(SimpleModel), mapping.ObjectClass.Type);
            Assert.Equal(8, mapping.PropertyToColumn.Count);
            Assert.Equal(8, mapping.ColumnToProperty.Count);
            Assert.Equal("MyDb", mapping.Database);
            Assert.Equal("sampleTable", mapping.Table.Name);
            Assert.Equal("dbo", mapping.Table.Schema);

            Assert.True(mapping.PropertyToColumn[nameof(SimpleModel.V1)].Identity);
            Assert.True(mapping.PropertyToColumn[nameof(SimpleModel.V1)].PrimaryKey);
            Assert.Equal("v1", mapping.PropertyToColumn[nameof(SimpleModel.V1)].Name);
        }
    }
}
