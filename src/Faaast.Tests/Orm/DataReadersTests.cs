using System;
using System.Collections.Generic;
using System.Linq;
using Faaast.Orm.Reader;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class DataReadersTests
    {

        public FaaastRowReader BuildReader(Dictionary<string, object> data)
        {
            return new FaaastRowReader()
            {
                FieldsCount = data.Count,
                Columns = data.Keys.ToArray(),
                Buffer = data.Values.ToArray()
            };
        }

        [Fact]
        public void Test_SingleValueReader_normal()
        {
            SingleValueReader<int> reader = new SingleValueReader<int>()
            {
                Start = 0,
                End = 1,
                RowReader = BuildReader(new Dictionary<string, object>() { { "id", 123 } })
            };
            reader.Read();
            Assert.Equal(123, reader.Value);
        }

        [Fact]
        public void Test_SingleValueReader_null()
        {
            var reader = new SingleValueReader<int>()
            {
                Start = 0,
                End = 1,
                RowReader = BuildReader(new Dictionary<string, object>() { { "id", DBNull.Value } })
            };

            reader.Read();
            Assert.Equal(0, reader.Value);
        }

        [Fact]
        public void Test_DictionaryReader()
        {
            var reader = new DictionaryReader()
            {
                Start = 0,
                End = 3,
                RowReader = BuildReader(new Dictionary<string, object>() { 
                    { "id", 123 },
                    { "label", "Lorem ipsum"},
                    { "description", DBNull.Value}
                })
            };

            reader.Read();
            Assert.Equal(123, reader.Value["id"]);
            Assert.Equal("Lorem ipsum", reader.Value["label"]);
            Assert.Null(reader.Value["description"]);
        }
    }
}
