using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using Faaast.Metadata;
using Faaast.Orm.Mapping;

namespace Faaast.Orm.Reader
{
    public struct ObjectReader
    {
        public IDtoClass Model { get; private set; }

        public ColumnReader[] Columns { get; private set; }

        public ObjectReader(TableMapping model)
        {
            this.Model = model.ObjectClass;
            this.Columns = new ColumnReader[model.ColumnMappings.Count];
            var index = 0;
            foreach (var column in model.ColumnMappings)
            {
                this.Columns[index++] = new ColumnReader(model.ColumnToProperty[column.Column], column);
            }
        }

        public ObjectReader(IDtoClass dto, params ColumnReader[] Columns)
        {
            this.Model = dto;
            this.Columns = Columns;
        }

        public object NewInstance() => this.Model.CreateInstance();

        public void Read(IDataReader reader, int dataReaderIndex, int mappingIndex, ref object instance)
        {
            if (mappingIndex != -1)
            {
                this.Columns[mappingIndex].Read(reader, dataReaderIndex, instance);
            }
        }

        //public static ObjectReader ForDynamic(IDataReader reader)
        //{
        //    var dto = new Metadata.DtoClass(typeof(ExpandoObject))
        //    {
        //        Activator = () => new ExpandoObject(),
        //        Name = nameof(ExpandoObject),
        //        Type = typeof(ExpandoObject),
        //    };

        //    var cols = new ColumnReader[reader.FieldCount];
        //    for (var fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
        //    {
        //        var dataName = reader.GetName(fieldIndex);
        //        var property = new Metadata.DtoProperty(dataName, typeof(object))
        //        {
        //            CanRead = true,
        //            CanWrite = true,
        //            Read = x => ((IDictionary<string, object>)x)[dataName],
        //            Write = (x, value) => ((IDictionary<string, object>)x)[dataName] = value
        //        };
        //        cols[fieldIndex] = new ColumnReader(property, dataName, true);
        //    }

        //    return new ObjectReader(dto, cols);
        //}
    }
}
