using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Faaast.Orm.Reader
{
    public class BaseRowReader: IDisposable
    {
        public FaaastCommand Source { get; private set; }

        internal DbDataReader Reader;
        internal int FieldsCount;
        internal bool HasRows;
        internal string[] Columns;
        internal object[] Buffer;
        internal LinkedList<DataReader> ColumnsReaders;

        public BaseRowReader(FaaastCommand source)
        {
            this.Source = source;
            this.Reader = null;
            this.FieldsCount = 0;
            this.HasRows = false;
            this.Columns = null;
            this.Buffer = null;
            this.ColumnsReaders = new();
        }

        public void Dispose() => this.Reader.Dispose();

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

        public DataReader<T> AddReader<T>()
        {
            var last = this.ColumnsReaders.Last?.Value.End ?? 0;
            var isValueType = typeof(T).IsValueType;
            var reader = isValueType
                ? new SingleValueReader<T>()
                {
                    RowReader = this,
                    Start = last,
                    End = last + 1
                }
                : (DataReader<T>)new DtoReader<T>(this, last);
            this.ColumnsReaders.AddLast(reader);
            return reader;
        }

        public DataReader AddReader(Type type)
        {
            var last = this.ColumnsReaders.Last?.Value.End ?? 0;
            var isValueType = type.IsValueType;
            DataReader reader;
            if (isValueType)
            {
                reader = (DataReader)Activator.CreateInstance(typeof(SingleValueReader<>).MakeGenericType(type));
                reader.RowReader = this;
                reader.Start = last;
                reader.End = last + 1;
            } 
            else
            {
                reader = (DataReader)Activator.CreateInstance(typeof(DtoReader<>).MakeGenericType(type), this, last);
            }

            this.ColumnsReaders.AddLast(reader);
            return reader;
        }

        public DataReader<T> AddValueReader<T>()
        {
            var last = this.ColumnsReaders.Last?.Value.End ?? 0;
            var reader = new SingleValueReader<T>()
            {
                RowReader = this,
                Start = last,
                End = last + 1
            };
            this.ColumnsReaders.AddLast(reader);
            return reader;
        }


        //public DataReader<T> AddValue<T>() where T : struct
        //{
        //    var last = this.ColumnsReaders.Last?.Value.End ?? 0;
        //    var reader = new SingleValueReader<T>()
        //    {
        //        RowReader = this,
        //        Start = last,
        //        End = last + 1
        //    };

        //    this.ColumnsReaders.AddLast(reader);
        //    return reader;
        //}

        //public DataReader<T> AddReader<T>() where T : new()
        //{
        //    var last = this.ColumnsReaders.Last?.Value.End ?? 0;
        //    var reader = (DataReader<T>)new DtoReader<T>(this, last);
        //    this.ColumnsReaders.AddLast(reader);
        //    return reader;
        //}

        public DataReader<Dictionary<string, object>> AddDictionaryReader(int? columnsCount = null)
        {
            var last = this.ColumnsReaders.Last?.Value.End ?? 0;
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
