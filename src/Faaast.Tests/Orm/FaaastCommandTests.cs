using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Faaast.Orm.Reader;
using Faaast.Tests.Orm.FakeDb;
using Faaast.Tests.Orm.Fixtures;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class FaaastCommandTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public FaaastCommandTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        [Fact]
        public void Test_ExecuteNonQuery()
        {
            var sql = "Faaast is awsome";
            bool called = false;
            using var command = this.Fixture.Db.CreateCommand(sql);
            {
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
            }

            var result = command.ExecuteNonQuery();
            Assert.True(called);
            Assert.Equal(18, result);
            Assert.Equal(ConnectionState.Closed, command.Connection.State);
        }

        [Fact]
        public async Task Test_ExecuteNonQueryAsync()
        {
            var sql = "Faaast is awsome";
            bool called = false;
            await using var command = await this.Fixture.Db.CreateCommandAsync(sql);
            {
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
        }

        [Fact]
        public void Test_ExecuteNonQuery_WithException()
        {
            var sql = "Faaast is awsome";
            using var command = this.Fixture.Db.CreateCommand(sql);
            {
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
        }

        [Fact]
        public async Task Test_ExecuteNonQueryAsync_WithException()
        {
            var sql = "Faaast is awsome";
            await using var command = await this.Fixture.Db.CreateCommandAsync(sql);
            {
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
        }
        [Fact]
        public void Test_ExecuteReader()
        {
            var sql = "Faaast is awsome";
            using var command = this.Fixture.Db.CreateCommand(sql);
            {
                var con = (FakeDbConnection)command.Connection;
                using var reader = command.ExecuteReader();
                Assert.NotNull(reader.Reader);
                Assert.Equal(reader.Source, command);
                Assert.Equal(reader.Buffer.Length, reader.Columns.Length);
                var data = ((FakeDbDataReader)reader.Reader).Data;
                Assert.Equal(data.Count, reader.Columns.Length);
                Assert.True(reader.Read());
                var values = data.Values.ToArray();
                for (var i = 0; i < reader.Buffer.Length; i++)
                {
                    Assert.Equal(values[i], reader.Buffer[i]);
                }
            }
        }

        [Fact]
        public async Task Test_ExecuteReaderAsync()
        {
            var sql = "Faaast is awsome";
            await using var command = await this.Fixture.Db.CreateCommandAsync(sql);
            {
                var con = (FakeDbConnection)command.Connection;
                await using var reader = await command.ExecuteReaderAsync();
                Assert.NotNull(reader.Reader);
                Assert.Equal(reader.Source, command);
                Assert.Equal(reader.Buffer.Length, reader.Columns.Length);
                var data = ((FakeDbDataReader)reader.Reader).Data;
                Assert.Equal(data.Count, reader.Columns.Length);
                Assert.True(await reader.ReadAsync());
                var values = data.Values.ToArray();
                for (var i = 0; i < reader.Buffer.Length; i++)
                {
                    Assert.Equal(values[i], reader.Buffer[i]);
                }
            }
        }


        [Fact]
        public void Test_CreateInternalCommand_Transaction()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null)
            {
                Transaction = new FakeDbTransaction(null, System.Data.IsolationLevel.ReadCommitted)
            };
            com.CreateInternalCommand();
            Assert.Equal(com.Transaction, com.Command.Transaction);
        }

        [Fact]
        public void Test_CreateInternalCommand_CommandTimeout()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null) { CommandTimeout = 200 };
            com.CreateInternalCommand();
            Assert.Equal(200, com.Command.CommandTimeout);
        }

        [Fact]
        public void Test_CreateInternalCommand_CommandType()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null) { CommandType = CommandType.StoredProcedure };
            com.CreateInternalCommand();
            Assert.Equal(CommandType.StoredProcedure, com.Command.CommandType);
        }

        [Fact]
        public void Test_CreateInternalCommand_ObjectParameter()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null, new { id = "lorem ipsum" });
            CheckParameter(com);
        }

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

        [Fact]
        public void Test_CreateInternalCommand_WithDictionaryParameter()
        {
            var param = new Dictionary<string, object>()
            {
                { "id" , "lorem ipsum" }
            };
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null, param);
            CheckParameter(com);
        }

        [Fact]
        public void Test_CreateInternalCommand_WithNullParameter()
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
    }
}
