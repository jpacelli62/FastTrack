using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Faaast.Tests.Orm.FakeConnection
{
    public class FakeDataReader : DbDataReader
    {
        public List<string> columns = new();

        public object[] Values { get; set; }

        public int CurrentIndex { get; set; } = 0;

        public int Count { get; set; } = 10000000;

        public override object this[int i] => this.GetValue(i);

        public override object this[string name] => this.GetValue(this.GetOrdinal(name));

        public override int Depth => throw new NotImplementedException();

        public override bool IsClosed => false;

        public override int RecordsAffected => -1;

        public override int FieldCount => columns.Count;

        public override bool HasRows => true;

        public override void Close()
        {
            // Do nothing
        }

        public override bool GetBoolean(int ordinal) => (bool)this.GetValue(ordinal);

        public override byte GetByte(int ordinal) => (byte)this.GetValue(ordinal);

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => (long)this.GetValue(ordinal);

        public override char GetChar(int ordinal) => (char)this.GetValue(ordinal);

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => (long)this.GetValue(ordinal);

        //public override IDataReader GetData(int i) => throw new NotImplementedException();

        public override string GetDataTypeName(int ordinal) => throw new NotImplementedException();

        public override DateTime GetDateTime(int ordinal) => (DateTime)this.GetValue(ordinal);

        public override decimal GetDecimal(int ordinal) => (decimal)this.GetValue(ordinal);

        public override double GetDouble(int ordinal) => (double)this.GetValue(ordinal);

        public override Type GetFieldType(int ordinal) => this.GetValue(ordinal)?.GetType();

        public override float GetFloat(int ordinal) => (float)this.GetValue(ordinal);

        public override Guid GetGuid(int ordinal) => (Guid)this.GetValue(ordinal);

        public override short GetInt16(int ordinal) => (short)this.GetValue(ordinal);

        public override int GetInt32(int ordinal) => (int)this.GetValue(ordinal);

        public override long GetInt64(int ordinal) => (long)this.GetValue(ordinal);

        public override string GetName(int ordinal) => columns[ordinal];

        public override int GetOrdinal(string name) => columns.IndexOf(name);

        public override DataTable GetSchemaTable() => throw new NotImplementedException();

        public override string GetString(int ordinal) => (string)this.GetValue(ordinal);

        public override object GetValue(int ordinal) => this.Values[ordinal];

        public override int GetValues(object[] values) => throw new NotImplementedException();

        public override bool IsDBNull(int ordinal) => this.Values[ordinal] == null;

        public override bool NextResult() => this.CurrentIndex++ < this.Count;

        public override bool Read() => this.CurrentIndex++ < this.Count;
        public override IEnumerator GetEnumerator() => throw new NotImplementedException();
    }
}
