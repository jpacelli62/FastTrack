using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Faaast.Tests.Orm.FakeDb
{
    public class FakeDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; }

        public override int ConnectionTimeout { get;  }

        public override  string Database { get; }

        public FakeDbCommand Command { get; set; }

        public FakeDbTransaction Transaction { get; set; }

        public override ConnectionState State => PrivateState;

        public override string DataSource => throw new NotImplementedException();

        public override string ServerVersion => throw new NotImplementedException();

        private ConnectionState PrivateState;

        public FakeDbConnection(Dictionary<string, object> data, int rowsCount) => this.Command = new FakeDbCommand()
        {
            Connection = this,
            Reader = new FakeDbDataReader(data, rowsCount)
        };

        public FakeDbConnection() => this.Command = new FakeDbCommand()
        {
            Connection = this,
            Reader = new FakeDbDataReader(new Dictionary<string, object>() { { "Foo", "bar" } }, 100)
        };

        public override void ChangeDatabase(string databaseName)
        {
            //Do nothing
        }

        public override void Close() => PrivateState = ConnectionState.Closed;

        public override void Open() => PrivateState = ConnectionState.Open;
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            if (this.State != ConnectionState.Open)
            {
                throw new Exception("Connection is not open");
            }

            this.Transaction = new FakeDbTransaction(this, isolationLevel);
            return this.Transaction;
        }

        protected override DbCommand CreateDbCommand() => this.Command;
    }
}
