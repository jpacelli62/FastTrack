using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Faaast.Orm.Reader;
using Faaast.Tests.Orm.FakeDb;
using Faaast.Tests.Orm.Fixture;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class FaaastCommandTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public FaaastCommandTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        private static void CheckParameter(FaaastCommand com)
        {
            com.CreateInternalCommand();
            Assert.Single(com.Command.Parameters);
            var param = com.Command.Parameters[0] as FakeDbParameter;
            Assert.Equal("id", param.ParameterName);
            Assert.Equal(DbType.String, param.DbType);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.False(param.IsNullable);
            Assert.Equal("lorem ipsum".Length * 2, param.Size);
            Assert.Equal("lorem ipsum", param.Value);
        }

        private static void CheckReadValues(BaseRowReader reader)
        {
            Assert.Equal(reader.Buffer.Length, reader.Columns.Length);

            var data = ((FakeDbDataReader)reader.Reader).Data;
            Assert.Equal(data.Count, reader.Columns.Length);
            var values = data.Values.ToArray();
            for (var i = 0; i < reader.Buffer.Length; i++)
            {
                Assert.Equal(values[i], reader.Buffer[i]);
            }
        }

        [Fact]
        public void ExecuteNonQuery()
        {
            var sql = "Faaast is awsome";
            var called = false;
            using var command = this.Fixture.Db.CreateCommand(sql);
            Assert.Equal(ConnectionState.Open, command.Connection.State);
            var con = (FakeDbConnection)command.Connection;
            con.Command.OnExecuteNonQuery = () =>
            {
                called = true;
                Assert.Equal(sql, command.CommandText);
                Assert.NotNull(command.Connection);
                Assert.Equal(ConnectionState.Open, command.Connection.State);
                return 18;
            };

            var result = command.ExecuteNonQuery();
            Assert.True(called);
            Assert.Equal(18, result);
            Assert.Equal(ConnectionState.Closed, command.Connection.State);
        }

        [Fact]
        public async Task ExecuteNonQueryAsync()
        {
            var sql = "Faaast is awsome";
            var called = false;
            await using var command = await this.Fixture.Db.CreateCommandAsync(sql);
            Assert.Equal(ConnectionState.Open, command.Connection.State);
            var con = (FakeDbConnection)command.Connection;
            con.Command.OnExecuteNonQuery = () =>
            {
                called = true;
                Assert.Equal(sql, command.CommandText);
                Assert.NotNull(command.Connection);
                Assert.Equal(ConnectionState.Open, command.Connection.State);
                return 18;
            };

            var result = await command.ExecuteNonQueryAsync();
            Assert.True(called);
            Assert.Equal(18, result);
            Assert.Equal(ConnectionState.Closed, command.Connection.State);
        }

        [Fact]
        public void ExecuteNonQuery_WithException()
        {
            var sql = "Faaast is awsome";
            using var command = this.Fixture.Db.CreateCommand(sql);
            Assert.Equal(ConnectionState.Open, command.Connection.State);
            var con = (FakeDbConnection)command.Connection;
            con.Command.OnExecuteNonQuery = () =>
            {
                Assert.Equal(ConnectionState.Open, command.Connection.State);
                throw new Exception("test");
            };

            Assert.Throws<Exception>(() => command.ExecuteNonQuery());
            Assert.Equal(ConnectionState.Closed, command.Connection.State);
        }

        [Fact]
        public async Task ExecuteNonQueryAsync_WithException()
        {
            var sql = "Faaast is awsome";
            await using var command = await this.Fixture.Db.CreateCommandAsync(sql);
            Assert.Equal(ConnectionState.Open, command.Connection.State);
            var con = (FakeDbConnection)command.Connection;
            con.Command.OnExecuteNonQuery = () =>
            {
                Assert.Equal(ConnectionState.Open, command.Connection.State);
                throw new Exception("test");
            };

            await Assert.ThrowsAsync<Exception>(async () => await command.ExecuteNonQueryAsync());
            Assert.Equal(ConnectionState.Closed, command.Connection.State);
        }

        [Fact]
        public void ExecuteReader()
        {
            var sql = "Faaast is awsome";
            using var command = this.Fixture.Db.CreateCommand(sql);
            using var reader = command.ExecuteReader();
            Assert.Equal(reader.Source, command);
            Assert.True(reader.Read());
            CheckReadValues(reader);
        }

        [Fact]
        public async Task ExecuteReaderAsync()
        {
            var sql = "Faaast is awsome";
            await using var command = await this.Fixture.Db.CreateCommandAsync(sql);
            await using var reader = await command.ExecuteReaderAsync();
            Assert.Equal(reader.Source, command);
            Assert.True(await reader.ReadAsync());
            CheckReadValues(reader);
        }

        [Fact]
        public void CreateInternalCommand_Transaction()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null)
            {
                Transaction = new FakeDbTransaction(null, System.Data.IsolationLevel.ReadCommitted)
            };
            com.CreateInternalCommand();
            Assert.Equal(com.Transaction, com.Command.Transaction);
        }

        [Fact]
        public void CreateInternalCommand_CommandTimeout()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null) { CommandTimeout = 200 };
            com.CreateInternalCommand();
            Assert.Equal(200, com.Command.CommandTimeout);
        }

        [Fact]
        public void CreateInternalCommand_CommandType()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null) { CommandType = CommandType.StoredProcedure };
            com.CreateInternalCommand();
            Assert.Equal(CommandType.StoredProcedure, com.Command.CommandType);
        }

        [Fact]
        public void CreateInternalCommand_ObjectParameter()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null, new { id = "lorem ipsum" });
            CheckParameter(com);
        }

        [Fact]
        public void CreateInternalCommand_WithDictionaryParameter()
        {
            var param = new Dictionary<string, object>()
            {
                { "id" , "lorem ipsum" }
            };
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null, param);
            CheckParameter(com);
        }

        [Fact]
        public void CreateInternalCommand_WithNullParameter()
        {
            var dico = new Dictionary<string, object>() { { "id", null } };
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null, dico);
            com.CreateInternalCommand();
            Assert.Single(com.Command.Parameters);
            var param = com.Command.Parameters[0] as FakeDbParameter;
            Assert.Equal("id", param.ParameterName);
            Assert.Equal(DbType.AnsiString, param.DbType);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.True(param.IsNullable);
            Assert.Equal(DBNull.Value, param.Value);
        }

        [Fact]
        public void Constructor_NullParameters()
        {
            Assert.Throws<ArgumentNullException>(() => new FaaastCommand(null, new FakeDbConnection(), null, new { }));
            Assert.Throws<ArgumentNullException>(() => new FaaastCommand(this.Fixture.Db, null, null, new { }));
        }
    }
}
