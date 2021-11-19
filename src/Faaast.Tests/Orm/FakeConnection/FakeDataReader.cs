using System;
using System.Collections.Generic;
using System.Data;

namespace Faaast.Tests.Orm.FakeConnection
{
    public class FakeDataReader : IDataReader
    {
        public List<string> columns = new();

        public object[] Values { get; set; }

        public int CurrentIndex { get; set; } = 0;

        public int Count { get; set; } = 10000000;

        public object this[int i] => this.GetValue(i);

        public object this[string name] => this.GetValue(this.GetOrdinal(name));

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => false;

        public int RecordsAffected => -1;

        public int FieldCount => columns.Count;

        public void Close()
        {
            // Do nothing
        }

        public void Dispose() => GC.SuppressFinalize(this);

        public bool GetBoolean(int i) => (bool)this.GetValue(i);

        public byte GetByte(int i) => (byte)this.GetValue(i);

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => (long)this.GetValue(i);

        public char GetChar(int i) => (char)this.GetValue(i);

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => (long)this.GetValue(i);

        public IDataReader GetData(int i) => throw new NotImplementedException();

        public string GetDataTypeName(int i) => throw new NotImplementedException();

        public DateTime GetDateTime(int i) => (DateTime)this.GetValue(i);

        public decimal GetDecimal(int i) => (decimal)this.GetValue(i);

        public double GetDouble(int i) => (double)this.GetValue(i);

        public Type GetFieldType(int i) => this.GetValue(i)?.GetType();

        public float GetFloat(int i) => (float)this.GetValue(i);

        public Guid GetGuid(int i) => (Guid)this.GetValue(i);

        public short GetInt16(int i) => (short)this.GetValue(i);

        public int GetInt32(int i) => (int)this.GetValue(i);

        public long GetInt64(int i) => (long)this.GetValue(i);

        public string GetName(int i) => columns[i];

        public int GetOrdinal(string name) => columns.IndexOf(name);

        public DataTable GetSchemaTable() => throw new NotImplementedException();

        public string GetString(int i) => (string)this.GetValue(i);

        public object GetValue(int i) => this.Values[i];

        public int GetValues(object[] values) => throw new NotImplementedException();

        public bool IsDBNull(int i) => this.Values[i] == null;

        public bool NextResult() => this.CurrentIndex++ < this.Count;

        public bool Read() => this.CurrentIndex++ < this.Count;
    }
}
