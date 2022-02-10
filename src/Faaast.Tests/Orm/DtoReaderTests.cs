using System;
using System.Collections.Generic;
using Faaast.Orm;
using Faaast.Orm.Reader;
using Faaast.Tests.Orm.FakeDb;
using Faaast.Tests.Orm.Fixture;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class DtoReaderTests : IClassFixture<FaaastOrmFixture>
    {
        private class NotMappedSampleClass
        {
            public int Id { get; set; }
            public string Label { get; set; }
            public string Description { get; set; }
        }

        public FaaastOrmFixture Fixture { get; set; }

        public DtoReaderTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        public FaaastRowReader BuildReader(Dictionary<string, object> data, int rows = 100)
        {
            var conn = new FakeDbConnection(data, rows);
            conn.Open();
            var command = new FaaastCommand(this.Fixture.Db, conn, string.Empty);
            return command.ExecuteReader();
        }

        private TException CaptureException<TException>(Action call) where TException: Exception
        {
            try
            {
                call();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch(Exception ex)
            {
                Assert.True(false, $"Expected exception of type \"{typeof(TException).Name}\", actual: \"{ex.GetType().Name}\"");
            }

            Assert.True(false, $"Expected exception of type \"{typeof(TException).Name}\" but nothing");
            return default;
        }

        [Fact]
        public void AddObjectReader_Dto_UnexpectedColumn()
        {
            var reader = this.BuildReader(new Dictionary<string, object>()
            {
                { "id", 123 }
            });

            var ex = CaptureException<FaaastOrmException>(() => reader.AddReader<SimpleModel>());
            Assert.Contains("Unexpected result column", ex.Message);
        }

        [Fact]
        public void AddObjectReader_Dto_UnexpectedEndOfColumns()
        {
            var reader = this.BuildReader(new Dictionary<string, object>()
            {
                { "V1", 123 }
            });

            var ex = CaptureException<FaaastOrmException>(() => reader.AddReader<SimpleModel>());
            Assert.Contains("Unexpected end of columns", ex.Message);
        }

        [Fact]
        public void AddObjectReader_Dto_AllColumns()
        {
            var data = new Dictionary<string, object>()
            {
                { "V1", 123 },
                { "V7", double.MaxValue },
                { "V3", DateTime.Now },
                { "V4", Guid.NewGuid() },
                { "V6", long.MaxValue },
                { "V2", "Lorem ipsum" },
                { "V5", float.MaxValue },
                { "V8", true }
            };
            var reader = this.BuildReader(data);
            var dtoReader = reader.AddReader<SimpleModel>();
            Assert.Equal(data.Count, dtoReader.End);
            Assert.True(reader.Read());
            Assert.NotNull(dtoReader.Value);
            var dto = this.Fixture.Db.Mapper.Get(typeof(SimpleModel));
            foreach (var property in dto)
            {
                Assert.Equal(data[property.Name], property.Read(dtoReader.Value));
            }
        }


        [Fact]
        public void AddObjectReader_NotMapped_UnexpectedColumn()
        {
            var reader = this.BuildReader(new Dictionary<string, object>()
            {
                { "mycolumn", 123 }
            });

            var ex = CaptureException<FaaastOrmException>(() => reader.AddReader<NotMappedSampleClass>());
            Assert.Contains("Unexpected result column", ex.Message);
        }

        [Fact]
        public void AddObjectReader_NotMapped_UnexpectedEndOfColumns()
        {
            var reader = this.BuildReader(new Dictionary<string, object>()
            {
                { "Id", 123 }
            });

            var ex = CaptureException<FaaastOrmException>(() => reader.AddReader<NotMappedSampleClass>());
            Assert.Contains("Unexpected end of columns", ex.Message);
        }

        [Fact]
        public void AddObjectReader_NotMapped_AllColumns()
        {
            var data = new Dictionary<string, object>()
            {
                { nameof(NotMappedSampleClass.Id), 123 },
                { nameof(NotMappedSampleClass.Label), "Lorem ipsum" },
                { nameof(NotMappedSampleClass.Description), DBNull.Value }
            };
            var reader = this.BuildReader(data);
            var dtoReader = reader.AddReader<NotMappedSampleClass>();
            Assert.Equal(data.Count, dtoReader.End);
            Assert.True(reader.Read());
            Assert.NotNull(dtoReader.Value);
            Assert.Equal(123, dtoReader.Value.Id);
            Assert.Equal("Lorem ipsum", dtoReader.Value.Label);
            Assert.Null(dtoReader.Value.Description);
        }
    }
}
