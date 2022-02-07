using System.Collections.Generic;
using System.Data.Common;

namespace Faaast.Orm.Reader
{
    public class BaseRowReader
    {
        internal BaseCommand Source;
        internal DbDataReader Reader;
        internal int FieldsCount;
        internal bool HasRows;
        internal string[] Columns;
        internal object[] Buffer;
        internal LinkedList<DataReader> ColumnsReaders;

        public BaseRowReader(BaseCommand source)
        {
            this.Source = source;
            this.Reader = null;
            this.FieldsCount = 0;
            this.HasRows = false;
            this.Columns = null;
            this.Buffer = null;
            this.ColumnsReaders = new();
        }

        protected void InitFields()
        {
            this.FieldsCount = this.Reader.FieldCount;
            this.HasRows = this.Reader.HasRows;
            this.Buffer = new object[this.FieldsCount];
            this.Columns = new string[this.FieldsCount];
            for (var i = 0; i < this.FieldsCount; i++)
            {
                this.Columns[i] = this.Reader.GetName(i);
            }
        }

        public DataReader<T> AddReader<T>() where T : struct
        {
            var last = this.ColumnsReaders.Last?.Value?.End ?? 0;
            var reader = new SingleValueReader<T>
            {
                RowReader = this,
                Start = last,
                End = last + 1
            };
            this.ColumnsReaders.AddLast(reader);
            return reader;
        }

        public DataReader<Dictionary<string, object>> AddDictionaryReader(int? columnsCount = null)
        {
            var last = this.ColumnsReaders.Last?.Value?.End ?? 0;
            var reader = new DictionaryReader
            {
                RowReader = this,
                Start = last,
                End = columnsCount.HasValue ? last + columnsCount.Value : this.FieldsCount
            };
            this.ColumnsReaders.AddLast(reader);
            return reader;
        }

        protected bool FillBuffer(bool hasRead)
        {
            if (hasRead)
            {
                for (var i = 0; i < this.FieldsCount; i++)
                {
                    this.Buffer[i] = this.Reader[i];
                }

                foreach (var reader in this.ColumnsReaders)
                {
                    reader.Read();
                }

                return true;
            }

            return false;
        }
    }
}
