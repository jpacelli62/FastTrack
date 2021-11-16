using System;
using System.Collections.Generic;
using System.Data;

namespace Faaast.Tests.Orm.FakeConnection
{
    public class FakeDataReader : IDataReader
    {
        public List<string> columns = new List<string>();

        public object[] Values { get; set; }

        public int CurrentIndex { get; set; } = 0;

        public int Count { get; set; } = 10000000;

        public object this[int i] => GetValue(i);

        public object this[string name] => GetValue(GetOrdinal(name));

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => false;

        public int RecordsAffected => -1;

        public int FieldCount => columns.Count;

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return (long)GetValue(i);
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return (long)GetValue(i);
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            return GetValue(i)?.GetType();
        }

        public float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            return columns[i];
        }

        public int GetOrdinal(string name)
        {
            return columns.IndexOf(name);
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            return Values[i];
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return Values[i] == null;
        }

        public bool NextResult()
        {

            return CurrentIndex++ < Count;
        }

        public bool Read()
        {
            return CurrentIndex++ < Count;
        }
    }
}
