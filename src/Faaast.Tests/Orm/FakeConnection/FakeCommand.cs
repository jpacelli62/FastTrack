using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Faaast.Tests.Orm.FakeConnection
{
    public class FakeCommand : IDbCommand
    {
        private class DataParameters : List<IDbDataParameter>, IDataParameterCollection
        {
            object IDataParameterCollection.this[string parameterName] { get => ((List<IDbDataParameter>)this)[this.IndexOf(parameterName)]; set => ((List<IDbDataParameter>)this)[this.IndexOf(parameterName)] = (IDbDataParameter)value; }

            public bool Contains(string parameterName) => this.Any(x => x.ParameterName == parameterName);

            public int IndexOf(string parameterName) => this.IndexOf(this.FirstOrDefault(x => x.ParameterName == parameterName));

            public void RemoveAt(string parameterName) => this.Remove(this.FirstOrDefault(x => x.ParameterName == parameterName));
        }
        public bool Prepared { get; set; }

        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDbConnection Connection { get; set; }

        public IDataParameterCollection Parameters { get; set; } = new DataParameters();

        public IDbTransaction Transaction { get; set; }

        public UpdateRowSource UpdatedRowSource { get; set; }

        public FakeDataReader Reader { get; set; }

        public void Cancel()
        {
            //Do nothing
        }

        public IDbDataParameter CreateParameter() => throw new NotImplementedException();

        public void Dispose() => GC.SuppressFinalize(this);

        public int ExecuteNonQuery() => throw new NotImplementedException();

        public IDataReader ExecuteReader() => this.Reader;

        public IDataReader ExecuteReader(CommandBehavior behavior) => this.Reader;

        public object ExecuteScalar() => throw new NotImplementedException();

        public void Prepare() => this.Prepared = true;
    }
}
