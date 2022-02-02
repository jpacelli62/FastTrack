using System.Collections.Generic;
using System.Data;
using Faaast.Orm.Reader;
using Faaast.Tests.Orm.FakeDb;
using Faaast.Tests.Orm.Fixtures;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class FaaastCommandTests: IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public FaaastCommandTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        [Fact]
        public void Check_Setup_CommandText()
        {
            string sql = "Faaast is awsome";
            
            var com = Fixture.Db.CreateCommand(sql);
            var dbCom = new FakeDbCommand();
            com.Setup(dbCom);
            Assert.Equal(sql, dbCom.CommandText);
        }

        [Fact]
        public void Check_Setup_Transaction()
        {
            var com = new FaaastCommand(Fixture.Db, null, null);
            com.Transaction = new FakeDbTransaction(null, System.Data.IsolationLevel.ReadCommitted);
            var dbCom = new FakeDbCommand();
            com.Setup(dbCom);
            Assert.Equal(dbCom.Transaction, com.Transaction);
        }

        [Fact]
        public void Check_Setup_CommandTimeout()
        {
            var com = new FaaastCommand(Fixture.Db, null, null) { CommandTimeout = 200 };
            var dbCom = new FakeDbCommand();
            com.Setup(dbCom);
            Assert.Equal(200, dbCom.CommandTimeout);
        }

        [Fact]
        public void Check_Setup_CommandType()
        {
            var com = new FaaastCommand(Fixture.Db, null, null) { CommandType = CommandType.StoredProcedure };
            var dbCom = new FakeDbCommand();
            com.Setup(dbCom);
            Assert.Equal(CommandType.StoredProcedure, dbCom.CommandType);
        }

        [Fact]
        public void Check_Setup_ObjectParameter()
        {
            var com = new FaaastCommand(Fixture.Db, null, new { id = "lorem ipsum" });
            CheckParameter(com);
        }

        private static void CheckParameter(FaaastCommand com)
        {
            var dbCom = new FakeDbCommand();
            com.Setup(dbCom);
            Assert.Single(dbCom.Parameters);
            var param = dbCom.Parameters[0] as FakeDbParameter;
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
            var com = new FaaastCommand(Fixture.Db, null, param);
            CheckParameter(com);
        }

        [Fact]
        public void Check_Setup_nullParameter()
        {
            var dico = new Dictionary<string, object>() { { "id" , null } };
            var com = new FaaastCommand(Fixture.Db, null, dico);
            var dbCom = new FakeDbCommand();
            com.Setup(dbCom);
            Assert.Single(dbCom.Parameters);
            var param = dbCom.Parameters[0] as FakeDbParameter;
            Assert.Equal("id", param.ParameterName);
            Assert.Equal(DbType.AnsiString, param.DbType);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.True(param.IsNullable);
            Assert.Null(param.Value);
        }
    }
}
