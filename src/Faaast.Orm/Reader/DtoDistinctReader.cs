using System;
using System.Collections.Generic;

namespace Faaast.Orm.Reader
{
    public class DtoDistinctReader<T> : DtoReader<T>
    {
        private Dictionary<int, T> Cache { get; set; }

        public virtual DtoReader<T> SourceReader { get; set; }

        public DtoDistinctReader(BaseRowReader source, int start) : base(source, start)
        {
            this.Cache = new Dictionary<int, T>();
            if (this.HasKey)
            {
                LinkedList<DtoReader<T>.ColumnMatch> newList = new();
                foreach (var item in this.ColumnsToRead)
                {
                    if (item.IsKey)
                    {
                        newList.AddLast(item);
                    }
                }
                
                this.ColumnsToRead = newList;
            }
        }

        public override void Read()
        {
            var hashCode = new System.HashCode();
            foreach (var property in this.ColumnsToRead)
            {
                hashCode.Add(property.Property.Name);
                var colValue = this.RowReader.Buffer[property.Index];
                if (property.Converter != null)
                {
                    colValue = property.Converter.FromDb(colValue, property.Property.Type);
                }

                if (colValue != DBNull.Value)
                {
                    hashCode.Add(colValue);
                }
            }

            var hash = hashCode.ToHashCode();
            if(this.Cache.TryGetValue(hash, out var item))
            {
                this.Value = item;
            }
            else
            {
                var instance = this.SourceReader.Value;
                this.Cache.Add(hash, instance);
                this.Value = instance;
            }
        }
    }
}
