using System;
using System.Collections.Generic;
using System.Data;
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
        public void Check_CreateCommand()
        {
            var sql = "Faaast is awsome";
            var command = this.Fixture.Db.CreateCommand(sql);
            command.CreateInternalCommand();
            Assert.Equal(sql, command.CommandText);
            Assert.NotNull(command.Connection);
            Assert.Equal(ConnectionState.Open, command.Connection.State);
        }

        [Fact]
        public async Task Check_CreateCommandAsync()
        {
            var sql = "Faaast is awsome";
            var command = await this.Fixture.Db.CreateCommandAsync(sql);
            command.CreateInternalCommand();
            Assert.Equal(sql, command.CommandText);
            Assert.NotNull(command.Connection);
            Assert.Equal(ConnectionState.Open, command.Connection.State);
            var result = await command.ExecuteNonQueryAsync();
            Assert.Equal(18, result);
        }

        [Fact]
        public void Check_Setup_Transaction()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null)
            {
                Transaction = new FakeDbTransaction(null, System.Data.IsolationLevel.ReadCommitted)
            };
            com.CreateInternalCommand();
            Assert.Equal(com.Transaction, com.Command.Transaction);
        }

        [Fact]
        public void Check_Setup_CommandTimeout()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null) { CommandTimeout = 200 };
            com.CreateInternalCommand();
            Assert.Equal(200, com.Command.CommandTimeout);
        }

        [Fact]
        public void Check_Setup_CommandType()
        {
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null) { CommandType = CommandType.StoredProcedure };
            com.CreateInternalCommand();
            Assert.Equal(CommandType.StoredProcedure, com.Command.CommandType);
        }

        [Fact]
        public void Check_Setup_ObjectParameter()
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
        public void Check_Setup_DicoParameter()
        {
            var param = new Dictionary<string, object>()
            {
                { "id" , "lorem ipsum" }
            };
            var com = new FaaastCommand(this.Fixture.Db, new FakeDbConnection(), null, param);
            CheckParameter(com);
        }

        [Fact]
        public void Check_Setup_nullParameter()
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

        //[Fact]
        //public void Check_Handle_exception()
        //{
        //    FakeDbConnection conn = default;

        //    Assert.Throws<Exception>(() => this.Fixture.Db.Call(x =>
        //          {
        //              conn = (FakeDbConnection) x.Connection;
        //              Assert.NotNull(conn);
        //              Assert.Equal(ConnectionState.Open, conn.State);
        //              throw new Exception();
        //          }, "Faaast is awsome"));

        //    Assert.NotNull(conn);
        //    Assert.Equal(ConnectionState.Closed, conn.State);
        //}

        //[Fact]
        //public void Check_Handle_exception_with_result()
        //{
        //    FakeDbConnection conn = default;
        //    Assert.Throws<Exception>(() => this.Fixture.Db.Call<int>(x =>
        //    {
        //        conn = (FakeDbConnection)x.Connection;
        //        Assert.NotNull(conn);
        //        Assert.Equal(ConnectionState.Open, conn.State);
        //        throw new Exception();
        //    }, "Faaast is awsome"));

        //    Assert.NotNull(conn);
        //    Assert.Equal(ConnectionState.Closed, conn.State);
        //}
    }
}
