using Faaast.DatabaseModel;
using Faaast.Orm.Resolver;
using Faaast.Tests.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class MappingTests
    {
        public IServiceProvider Services { get; set; }

        public Table SampleTable { get; set; }

        public IDatabase SampleDb { get; set; }

        public MappingTests()
        {
            //ServiceCollection services = new ServiceCollection();

            //services.AddFaaastOrm(convention => convention
            //    .Match(name => !name.Contains("sys."))
            //    .RemoveTablePrefixes("tbl_", "joi_")
            //    .AddSuffixToName("Dto"));

            //var provider = services.BuildServiceProvider();

            //SampleTable = new Table
            //{
            //    Name = "tbl_Sample-Model",
            //    Schema = "dbo",
            //    Columns = new Dictionary<string, Column>
            //    {
            //        { nameof(SampleModelDto.IntMember), new Column(nameof(SampleModelDto.IntMember)).IsIdentity().IsPrimaryKey() },
            //        { nameof(SampleModelDto.ReadWriteProperty), new Column(nameof(SampleModelDto.ReadWriteProperty)) },
            //        { nameof(SampleModelDto.NullableBoolProperty), new Column(nameof(SampleModelDto.NullableBoolProperty)).IsNullable()},
            //        { nameof(SampleModelDto.PrivateSetProperty), new Column(nameof(SampleModelDto.PrivateSetProperty)) }
            //    }
            //};

            //SampleDb = new Database(new ConnectionSettings("site", SqlEngine.SQLServer, "sampleConnexionString"));
            //SampleDb.Tables[SampleTable.Name] = SampleTable;
            //provider.UseDatabase(() => SampleDb);
        }

        [Fact]
        public void Mapping_has_been_done()
        {
            Assert.True(false);
            //var definition = SampleTable.Get(Meta.PocoObject);
            //Assert.NotNull(definition);
        }
    }
}
