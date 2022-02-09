using System;
using System.Collections.Generic;
using Faaast.Orm.Reader;
using Faaast.Tests.Orm.FakeDb;
using Faaast.Tests.Orm.Fixture;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class DataReadersTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public DataReadersTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        public FaaastRowReader BuildReader(Dictionary<string, object> data, int rows = 100)
        {
            var conn = new FakeDbConnection(data, rows);
            conn.Open();
            var command = new FaaastCommand(this.Fixture.Db, conn, string.Empty);
            return command.ExecuteReader();
        }

        [Fact]
        public void AddReaderInt()
        {
            var reader = this.BuildReader(new Dictionary<string, object>()
            {
                { "id", 123 },
                { "size", 10 }
            });
            var valueReader1 = reader.AddReader<int>();
            var valueReader2 = reader.AddReader<int>();
            reader.Read();
            Assert.Equal(123, valueReader1.Value);
            Assert.Equal(10, valueReader2.Value);
        }

        [Fact]
        public void AddReaderInt_WithNullValue()
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
        public void AddDictionaryReader()
        {
            var reader = this.BuildReader(new Dictionary<string, object>() {
                    { "id", 123 },
                    { "label", "Lorem ipsum"},
                    { "description", DBNull.Value}
                });
            var valueReader1 = reader.AddDictionaryReader(2);
            var valueReader2 = reader.AddDictionaryReader();
            reader.Read();
            Assert.Equal(123, valueReader1.Value["id"]);
            Assert.Equal("Lorem ipsum", valueReader1.Value["label"]);
            Assert.Null(valueReader2.Value["description"]);
        }

        [Fact]
        public void FillBuffer_Empty()
        {
            var reader = this.BuildReader(new Dictionary<string, object>()
            {
                { "id", 123 }
            }, 0);
            reader.AddReader<int>();
            Assert.False(reader.Read());
        }
    }
}
