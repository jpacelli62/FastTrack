using System;
using System.Data;
using System.Diagnostics;
using Faaast.Metadata;
using Faaast.Orm.Mapping;
using Faaast.Orm.Model;

namespace Faaast.Orm.Reader
{
    [DebuggerDisplay("{ColumnName}")]
    public struct ColumnReader
    {
        public IDtoProperty Property { get; private set; }

        public string ColumnName { get; set; }

        public Action<IDataReader, int, object> Call { get; set; }

        public ColumnReader(IDtoProperty property, string columnName)
        {
            this.Property = property;
            this.ColumnName = columnName;
            this.Call = null;
            this.Call = this.Property?.CanWrite is false or null ? this.DoNothing : this.ReadNullable;
        }

        public ColumnReader(IDtoProperty property, ColumnMapping column) : this(property, column.Column.Name)
        {

        }

        private void DoNothing(IDataReader reader, int index, object instance)
        {
            // Really nothing
        }

        public void Read(IDataReader reader, int index, object instance) => this.Call(reader, index, instance);

        public void ReadNullable(IDataReader reader, int index, object instance)
        {
            var value = reader.GetValue(index);
            if (value != DBNull.Value)
            {
                this.Property.Write(instance, value);
            }
        }
    }
}
