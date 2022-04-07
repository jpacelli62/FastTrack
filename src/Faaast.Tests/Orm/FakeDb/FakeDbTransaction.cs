using System.Data;
using System.Data.Common;

namespace Faaast.Tests.Orm.FakeDb
{
    public class FakeDbTransaction : DbTransaction
    {
        private IsolationLevel _level;
        private DbConnection _dbConnection;
        public bool Commited { get; set; }
        public bool Rollbacked { get; set; }

        public override IsolationLevel IsolationLevel => _level;

        protected override DbConnection DbConnection => this._dbConnection;

        public void SetIsolationLevel(IsolationLevel level) => this._level = level;

        public void SetDbConnection(DbConnection dbConnection) => this._dbConnection = dbConnection;

        public FakeDbTransaction(DbConnection dbConnection, IsolationLevel level)
        {
            this._level = level;
            this._dbConnection = dbConnection;
        }

        public override void Commit() => this.Commited = true;
        public override void Rollback() => this.Rollbacked = true;
    }
}
