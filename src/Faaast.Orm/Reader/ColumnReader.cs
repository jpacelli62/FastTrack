using Faaast.DatabaseModel;
using Faaast.Metadata;
using System;
using System.Data;

namespace Faaast.Orm.Reader
{
    public struct ColumnReader
    {
        public DtoProperty Property;
        public Column Column;
        private Action<IDataReader, int, object> Call;

        public ColumnReader(DtoProperty property, Column column)
        {
            this.Property = property;
            this.Column = column;
            this.Call = null;
            if (!Property?.CanWrite ?? true)
            {
                Call = DoNothing;
            }
            else
            {
                Call = this.Property.Get(DbMeta.Nullable) ? ReadNullable : ReadNonNullable;
            }
        }

        private void DoNothing(IDataReader reader, int index, object instance)
        {
        }

        public void Read(IDataReader reader, int index, object instance)
        {
            Call(reader, index, instance);
        }

        public void ReadNonNullable(IDataReader reader, int index, object instance)
        {
            var value = reader.GetValue(index);
            Property.Write(instance, value);
        }

        public void ReadNullable(IDataReader reader, int index, object instance)
        {
            var value = reader.GetValue(index);
            if (value != DBNull.Value)
                Property.Write(instance, value);
        }
    }
}
