using Faaast.DatabaseModel;
using System.Collections.Generic;
using System.Data;

namespace Faaast.Orm.Reader
{
    public struct ObjectReader
    {
        public TableMapping Model;
        public ColumnReader[] Columns;

        public ObjectReader(ICollection<Column> columns, TableMapping model)
        {
            this.Model = model;
            this.Columns = null;
            if (columns != null)
            {
                this.Columns = new ColumnReader[columns.Count];
                int index = 0;
                foreach (var column in columns)
                {
                    this.Columns[index] = new ColumnReader(model.ColumnToProperty[column], column);
                    index++;
                }
            }
        }

        public object NewInstance()
        {
            return this.Model.ObjectClass.Activator();
        }

        public void Read(IDataReader reader, int dataReaderIndex, int mappingIndex, ref object instance)
        {
            if (mappingIndex != -1)
                Columns[mappingIndex].Read(reader, dataReaderIndex, instance);
        }
    }
}
