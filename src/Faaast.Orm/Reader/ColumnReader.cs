using System;
using System.Data;
using System.Diagnostics;
using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Mapping;

namespace Faaast.Orm.Reader
{
    [DebuggerDisplay("{ColumnName}")]
    public struct ColumnReader
    {
        public DtoProperty Property { get; private set; }

        public string ColumnName { get; set; }


        public Action<IDataReader, int, object> Call { get; set; }

        public ColumnReader(DtoProperty property, string columnName)
        {
            this.Property = property;
            this.ColumnName = columnName;
            this.Call = null;
        }

        public ColumnReader(DtoProperty property, string columnName, bool nullable) : this(property, columnName)
        {
            if (!Property?.CanWrite ?? true)
            {
                this.Call = DoNothing;
            }
            else
            {
                this.Call = nullable ? ReadNullable : ReadNonNullable;
            }
        }

        public ColumnReader(DtoProperty property, ColumnMapping column) : this(property, column.Column.Name, column.Column.Get(DbMeta.Nullable) == true)
        {
            
        }

        private void DoNothing(IDataReader reader, int index, object instance)
        {
            // Really nothing
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
