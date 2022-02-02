using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Faaast.Tests.Orm.FakeDb
{
    public class FakeDbDataReader : DbDataReader
    {
        public Dictionary<string, object> Data { get; set; }

        public int CurrentRow { get; set; } = 0;

        public int RowsCount { get; set; }
        
        private bool _isClosed;

        public FakeDbDataReader(Dictionary<string, object> data, int rowsCount)
        {
            this.Data = data;
            this.RowsCount = rowsCount;
            this._isClosed = false;
        }

        public override object this[int i] => this.GetValue(i);

        public override object this[string name] => this.GetValue(this.GetOrdinal(name));

        public override int Depth => throw new NotImplementedException();

        public override bool IsClosed => this._isClosed;

        public override int RecordsAffected => -1;

        public override int FieldCount => this.Data.Count;

        public override bool HasRows => this.RowsCount >0;

        public override void Close() => this._isClosed = true;

        public override bool GetBoolean(int ordinal) => (bool)this.GetValue(ordinal);

        public override byte GetByte(int ordinal) => (byte)this.GetValue(ordinal);

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => (long)this.GetValue(ordinal);

        public override char GetChar(int ordinal) => (char)this.GetValue(ordinal);

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => (long)this.GetValue(ordinal);

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

        public override string GetName(int ordinal)
        {
            var i = 0;
            foreach (var item in this.Data)
            {
                if (i == ordinal)
                {
                    return item.Key;
                }

                i++;
            }

            return null;
        }

        public override int GetOrdinal(string name)
        {
            var i = 0;
            foreach (var item in this.Data)
            {
                if(string.Equals(item.Key, name))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public override DataTable GetSchemaTable() => throw new NotImplementedException();

        public override string GetString(int ordinal) => (string)this.GetValue(ordinal);

        public override object GetValue(int ordinal)
        {
            var i = 0;
            foreach (var item in this.Data)
            {
                if (i == ordinal)
                {
                    return item.Value;
                }

                i++;
            }

            return null;
        }

        public override int GetValues(object[] values) => throw new NotImplementedException();

        public override bool IsDBNull(int ordinal) => this.GetValue(ordinal) == null;

        public override bool NextResult() => this.CurrentRow++ < this.RowsCount;

        public override bool Read() => this.NextResult();

        public override IEnumerator GetEnumerator() => throw new NotImplementedException();
    }
}
