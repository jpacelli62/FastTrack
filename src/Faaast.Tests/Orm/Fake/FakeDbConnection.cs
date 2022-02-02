using System;
using System.Data;
using System.Data.Common;

namespace Faaast.Tests.Orm.FakeConnection
{
    public class FakeDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; }

        public override int ConnectionTimeout { get;  }

        public override  string Database { get; }

        public FakeCommand Command { get; set; }

        public override ConnectionState State { get => PrivateState; }

        public override string DataSource => throw new NotImplementedException();

        public override string ServerVersion => throw new NotImplementedException();

        private ConnectionState PrivateState;

        public override void ChangeDatabase(string databaseName)
        {
            //Do nothing
        }

        public override void Close() => PrivateState = ConnectionState.Closed;



        public override void Open() => PrivateState = ConnectionState.Open;
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotImplementedException();
        protected override DbCommand CreateDbCommand() => this.Command ?? new FakeCommand();
    }
}
