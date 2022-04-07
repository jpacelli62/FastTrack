using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Faaast.Tests.Orm.FakeDb
{
    public class FakeDbParameters : DbParameterCollection
    {
        private readonly List<DbParameter> Parameters = new();

        public override int Count => this.Parameters.Count;

        public override object SyncRoot => Parameters;

        public override int Add(object value)
        {
            Parameters.Add((DbParameter)value);
            return Parameters.Count;
        }

        public override void AddRange(Array values) => this.Parameters.AddRange(values?.Cast<DbParameter>() ?? Array.Empty<DbParameter>());
        public override void Clear() => this.Parameters.Clear();
        public override bool Contains(string value) => this.Parameters.Any(x => x.ParameterName == value);
        public override bool Contains(object value) => this.Parameters.Contains(value);
        public override void CopyTo(Array array, int index)
        {
            for (var i = 0; i < this.Parameters.Count; i++)
            {
                array.SetValue(this.Parameters[i], i + index);
            }
        }

        public override IEnumerator GetEnumerator() => Parameters.GetEnumerator();
        public override int IndexOf(string parameterName) => this.IndexOf(this.Parameters.FirstOrDefault(x => x.ParameterName == parameterName));
        public override int IndexOf(object value) => this.Parameters.IndexOf(value as DbParameter);
        public override void Insert(int index, object value) => this.Parameters.Insert(index, value as DbParameter);
        public override void Remove(object value) => this.Parameters.Remove(value as DbParameter);
        public override void RemoveAt(string parameterName) => this.Remove(this.Parameters.FirstOrDefault(x => x.ParameterName == parameterName));
        public override void RemoveAt(int index) => this.Parameters.RemoveAt(index);
        protected override DbParameter GetParameter(int index) => Parameters[index];
        protected override DbParameter GetParameter(string parameterName) => this.Parameters.FirstOrDefault(x => x.ParameterName == parameterName);
        protected override void SetParameter(int index, DbParameter value) => this.Parameters[index] = value;
        protected override void SetParameter(string parameterName, DbParameter value) {
            var param = this.Parameters.FirstOrDefault(x => x.ParameterName == parameterName);
            if(param == null)
            {
                value.ParameterName = parameterName;
                this.Parameters.Add(value);
            }
            else
            {
                this.SetParameter(this.Parameters.IndexOf(param), value);
            }
        }
    }
}
