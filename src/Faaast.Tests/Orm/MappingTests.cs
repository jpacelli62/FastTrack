using Faaast.DatabaseModel;
using Faaast.Orm;
using Faaast.Orm.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
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
            List<string> letters = new List<string>();
            foreach(int i in Enumerable.Range(0, 15))
            {
                int charIndex = ((int)'A') + i;
                char letter = (char)charIndex;
                letters.Add(letter.ToString());

            }

            Assert.True(false);
            //var definition = SampleTable.Get(Meta.PocoObject);
            //Assert.NotNull(definition);
        }

        public static IEnumerable<FaaastTuple<TA,TB>> Fetch<TA,TB>(FaaastCommand command)
        {
            foreach (var row in command.Read(typeof(TA), typeof(TB)))
            {
                yield return new FaaastTuple<TA, TB>(
                    (TA)row[0],
                    (TB)row[1]
                    );
            }
        }

        public class FaaastTuple<TA, TB>
        {
            public TA A { get; set; }
            public TB B { get; set; }

            public FaaastTuple(TA A, TB B)
            {
                this.A = A;
                this.B = B;
            }

            public FaaastTuple(object[] values) : this((TA)values[0], (TB)values[1])
            {
            }
        }
    }
}
