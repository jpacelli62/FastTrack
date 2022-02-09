using System;
using System.Collections.Generic;
using System.Linq;
using Faaast.Orm.Reader;
using Faaast.Tests.Orm.FakeDb;
using Faaast.Tests.Orm.Fixtures;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class DataReadersTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public DataReadersTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        public FaaastRowReader BuildReader(Dictionary<string, object> data)
        {
            var conn = new FakeDbConnection(data, 100);
            conn.Open();
            var command = new FaaastCommand(this.Fixture.Db, conn, string.Empty);
            return command.ExecuteReader();
        }

        [Fact]
        public void Test_AddReaderInt()
        {
            var reader = this.BuildReader(new Dictionary<string, object>()
            {
                { "id", 123 }
            });
            var valueReader = reader.AddReader<int>();

            reader.Read();
            Assert.Equal(123, valueReader.Value);
        }

        [Fact]
        public void Test_AddReaderInt_WithNullValue()
        {
            var reader = this.BuildReader(new Dictionary<string, object>()
            {
                { "id", DBNull.Value }
            });
            var valueReader = reader.AddReader<int>();
            reader.Read();
            Assert.Equal(0, valueReader.Value);
        }

        [Fact]
        public void Test_AddDictionaryReader()
        {
            var reader = this.BuildReader(new Dictionary<string, object>() {
                    { "id", 123 },
                    { "label", "Lorem ipsum"},
                    { "description", DBNull.Value}
                });
            var valueReader = reader.AddDictionaryReader(3);

            reader.Read();
            Assert.Equal(123, valueReader.Value["id"]);
            Assert.Equal("Lorem ipsum", valueReader.Value["label"]);
            Assert.Null(valueReader.Value["description"]);
        }
    }
}
