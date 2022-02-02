using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Faaast.Tests.Orm.FakeConnection
{
    public class FakeCommand : DbCommand
    {
        private class FakeDbParameter : DbParameter
        {
            public override DbType DbType { get; set; }
            public override ParameterDirection Direction { get; set; }
            public override bool IsNullable { get; set; }
            public override string ParameterName { get; set; }
            public override int Size { get; set; }
            public override string SourceColumn { get; set; }
            public override bool SourceColumnNullMapping { get; set; }
            public override object Value { get; set; }

            public override void ResetDbType() => this.DbType = DbType.AnsiString;
        }
        private class DataParameters : DbParameterCollection
        {
            private readonly List<DbParameter> Parameters = new();


            public override int Count => this.Parameters.Count;

            public override object SyncRoot => throw new NotImplementedException();

            public override int Add(object value) 
            { 
                Parameters.Add((DbParameter)value); 
                return Parameters.Count; 
            }

            public override void AddRange(Array values) => throw new NotImplementedException();
            public override void Clear() => throw new NotImplementedException();
            public override bool Contains(string value) => this.Parameters.Any(x => x.ParameterName == value);
            public override bool Contains(object value) => throw new NotImplementedException();
            public override void CopyTo(Array array, int index) => throw new NotImplementedException();
            public override IEnumerator GetEnumerator() => Parameters.GetEnumerator();
            public override int IndexOf(string parameterName) => this.IndexOf(this.Parameters.FirstOrDefault(x => x.ParameterName == parameterName));
            public override int IndexOf(object value) => throw new NotImplementedException();
            public override void Insert(int index, object value) => throw new NotImplementedException();
            public override void Remove(object value) => throw new NotImplementedException();
            public override void RemoveAt(string parameterName) => this.Remove(this.Parameters.FirstOrDefault(x => x.ParameterName == parameterName));
            public override void RemoveAt(int index) => throw new NotImplementedException();
            protected override DbParameter GetParameter(int index) => Parameters[index];
            protected override DbParameter GetParameter(string parameterName) => throw new NotImplementedException();
            protected override void SetParameter(int index, DbParameter value) => throw new NotImplementedException();
            protected override void SetParameter(string parameterName, DbParameter value) => throw new NotImplementedException();
        }
        public bool Prepared { get; set; }

        public FakeDataReader Reader { get; set; }
        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        protected override DbConnection DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection { get; } = new DataParameters();

        protected override DbTransaction DbTransaction { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }

        public override int ExecuteNonQuery() => 18;

        public override object ExecuteScalar() => new();

        public override void Prepare() => this.Prepared = true;

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => this.Reader ?? new FakeDataReader();
        public override void Cancel() => throw new NotImplementedException();
        protected override DbParameter CreateDbParameter() => new FakeDbParameter();
    }
}
