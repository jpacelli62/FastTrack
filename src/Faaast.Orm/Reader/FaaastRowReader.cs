using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Faaast.Orm.Reader
{
    public struct FaaastRowReader : IDisposable, IAsyncDisposable
    {
        internal FaaastCommand Source;

        internal DbCommand Command;

        internal DbDataReader Reader;

        internal int FieldsCount;

        internal bool HasRows;

        internal string[] Columns;

        internal object[] Buffer;

        internal LinkedList<DataReader> ColumnsReaders;

        public FaaastRowReader(FaaastCommand source, DbCommand command)
        {
            this.Source = source;
            this.Command = command;
            this.Reader = null;
            this.FieldsCount = 0;
            this.HasRows = false;
            this.Columns = null;
            this.Buffer = null;
            this.ColumnsReaders = new();
        }

        public void Dispose() => this.Source.PostCall(this.Command);

        public ValueTask DisposeAsync() => new(this.Source.PostCallAsync(this.Command));

        internal async Task PrepareAsync()
        {
            this.Reader = await this.Command.ExecuteReaderAsync(this.Source.CommandBehavior, this.Source.CancellationToken);
            this.InitFields();
        }

        internal void Prepare()
        {
            this.Reader = this.Command.ExecuteReader(this.Source.CommandBehavior);
            this.InitFields();
        }

        private void InitFields()
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

        public async Task<bool> ReadAsync() => this.FillBuffer(await this.Reader.ReadAsync(this.Source.CancellationToken));

        public bool Read() => this.FillBuffer(this.Reader.Read());

        private readonly bool FillBuffer(bool hasRead)
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
