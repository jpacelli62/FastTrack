using System;
using System.Data;
using System.Data.Common;

namespace Faaast.Tests.Orm.FakeDb
{
    public class FakeDbCommand : DbCommand
    {
        public Func<int> OnExecuteNonQuery { get; set; } = () => 18;
        public Func<object> OnExecuteScalar { get; set; } = () => new();
        public Func<CommandBehavior, DbDataReader> OnExecuteDbDataReader { get; set; }

        public FakeDbCommand()
        {
            OnExecuteDbDataReader = DefaultExecuteDbDataReader;
        }

        private DbDataReader DefaultExecuteDbDataReader(CommandBehavior behavior) =>
            this.DbConnection.State != ConnectionState.Open ? throw new Exception("Connection is not open") : this.Reader;

        public bool Prepared { get; set; }
        public FakeDbDataReader Reader { get; set; }
        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        protected override DbConnection DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameters();

        protected override DbTransaction DbTransaction { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }

        public override int ExecuteNonQuery() => OnExecuteNonQuery();

        public override object ExecuteScalar() => OnExecuteScalar();

        public override void Prepare() => this.Prepared = true;

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behaviour) => OnExecuteDbDataReader(behaviour);
        public override void Cancel() => throw new NotImplementedException();
        protected override DbParameter CreateDbParameter() => new FakeDbParameter();
    }
}
