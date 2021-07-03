//using Faaast.DatabaseModel;
//using Faaast.Orm.Resolver;
//using Faaast.Tests.Metadata;
//using Faaast.Tests.Orm.Fixtures;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Linq;
//using System.Collections.Generic;
//using Xunit;
//using Faaast.Orm;

//namespace Faaast.Tests.Orm
//{
//    public class MappingTests
//    {
//        public IServiceProvider Services { get; set; }

//        public Table SampleTable { get; set; }

//        public IDatabase SampleDb { get; set; }

//        public MappingTests()
//        {
//            ServiceCollection services = new ServiceCollection();

//            services.AddFaaastOrm(convention => convention
//                .Match(name => !name.Contains("sys."))
//                .RemoveTablePrefixes("tbl_", "joi_")
//                .AddSuffixToName("Dto"));

//            var provider = services.BuildServiceProvider();

//            SampleTable = new Table
//            {
//                Name = "tbl_Sample-Model",
//                Schema = "dbo",
//                Columns = new Column[]
//                {
//                     new Column(nameof(SampleModelDto.IntMember)).IsIdentity().IsPrimaryKey(),
//                     new Column(nameof(SampleModelDto.ReadWriteProperty)).Length(50).References("dbo", "pktable", "pkcolumn"),
//                     new Column(nameof(SampleModelDto.NullableBoolProperty)).IsNullable(),
//                     new Column(nameof(SampleModelDto.PrivateSetProperty))
//                }
//            };

//            SampleDb = new Database(new ConnectionSettings("site", SqlEngine.SQLServer, "sampleConnexionString"));
//            SampleDb.Tables.Add(SampleTable);
//            provider.UseDatabase(() => SampleDb);
//        }

//        [Fact]
//        public void Mapping_has_been_done()
//        {
//            var definition = SampleTable.Get(Meta.PocoObject);
//            Assert.NotNull(definition);

//            Assert.True(SampleTable.Columns.Where(x => x.Key != nameof(SampleModelDto.IntMember)).All(x => x.Value.Identity == false && x.Value.PrimaryKey == false));
//            Assert.True(SampleTable.Columns[nameof(SampleModelDto.IntMember)].Identity && SampleTable.Columns[nameof(SampleModelDto.IntMember)].PrimaryKey);

//            Assert.Equal("dbo", SampleTable.Columns[nameof(SampleModelDto.ReadWriteProperty)].Get(DbMeta.ReferenceSchema));
//            Assert.Equal("pktable", SampleTable.Columns[nameof(SampleModelDto.ReadWriteProperty)].Get(DbMeta.ReferenceTable));
//            Assert.Equal("pkcolumn", SampleTable.Columns[nameof(SampleModelDto.ReadWriteProperty)].Get(DbMeta.ReferenceColumn));

//            Assert.Equal(50, SampleTable.Columns[nameof(SampleModelDto.ReadWriteProperty)].Get(DbMeta.Length));

//            //var pk = SampleTable.PrimaryKeyColumns();
//            //Assert.True(pk.Length == 1);
//            //Assert.Equal(nameof(SampleModelDto.IntMember), pk[0].Name);
//            Assert.True(false);
//        }
//    }
//}
