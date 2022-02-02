using System;
using System.Data;
using Faaast.Orm;
using Faaast.Orm.Model;
using Faaast.Tests.Orm.Fake;
using Faaast.Tests.Orm.Fixtures;
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

            Assert.True(mapping.PropertyToColumn[nameof(SimpleModel.V4)].Computed);
            Assert.True(mapping.PropertyToColumn[nameof(SimpleModel.V5)].Get(DbMeta.Nullable).Value);
            Assert.Equal(100, mapping.PropertyToColumn[nameof(SimpleModel.V2)].Get(DbMeta.Length).Value);
            Assert.Single(mapping.Table.PrimaryKeyColumns());
            Assert.Equal("otherschema", mapping.PropertyToColumn[nameof(SimpleModel.V2)].Get(DbMeta.ReferenceSchema));
            Assert.Equal("othertable", mapping.PropertyToColumn[nameof(SimpleModel.V2)].Get(DbMeta.ReferenceTable));
            Assert.Equal("othercolumn", mapping.PropertyToColumn[nameof(SimpleModel.V2)].Get(DbMeta.ReferenceColumn));
        }

        [Fact]
        public void Throws_argumentException()
        {
            Assert.Throws<ArgumentException>(() => new SimpleTypeMapping<SampleDto>().Map(x => x.RefProperty.ToLower(), "test"));

            var services = new ServiceCollection();
            services.AddFaaastOrm();
            services.AddSingleton<FakeDB>();
            var provider = services.BuildServiceProvider();
            var db = provider.GetRequiredService<FakeDB>();
            db.ConnectionOverride = null;
            Assert.Throws<ArgumentException>(() => db.Mappings.Value);
        }

        [Fact]
        public void Check_Reader()
        {
            var db = this.Fixture.Db;
            var sql = "select * from sample";
            var parameters = new { @id = 123 };

            var command = db.CreateCommand(sql, parameters);
            var reader = command.ExecuteReader();

            Assert.Equal(sql, command.CommandText);
            Assert.Equal(parameters, command.Parameters);

            Assert.False(command.CancellationToken.IsCancellationRequested);
            Assert.Equal(System.Data.CommandBehavior.SequentialAccess, command.CommandBehavior);
            Assert.Equal(sql, reader.Command.CommandText);
            Assert.Equal(System.Data.CommandType.Text, reader.Command.CommandType);
            Assert.Null(reader.Command.Transaction);
            Assert.Single(reader.Command.Parameters);
            //Assert.Equal(ConnectionState.Open, reader.Command.Connection.State);

            var param = reader.Command.Parameters[0];
            Assert.Equal("id", param.ParameterName);
            Assert.Equal(123, param.Value);
            Assert.Equal(System.Data.DbType.Int32, param.DbType);
            Assert.Equal(ParameterDirection.Input, param.Direction);

            var v1Read = reader.AddReader<int>();
            var dicoRead = reader.AddDictionaryReader();
            while (reader.Read())
            {
                var v1 = v1Read.Value;
                var dico1 = dicoRead.Value;

            }
        }
    }
}
