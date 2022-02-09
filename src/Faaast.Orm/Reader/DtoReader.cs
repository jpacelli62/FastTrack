using System;
using System.Collections.Generic;
using Faaast.Metadata;

namespace Faaast.Orm.Reader
{
    public class DtoReader<T> : DataReader<T>
    {
        internal class ColumnMatch
        {
            public IDtoProperty Property { get; set; }
            public int Index { get; set; }
        }

        internal List<ColumnMatch> ColumnsToRead { get; set; }

        public DtoReader(BaseRowReader source, int start)
        {
            var type = typeof(T);
            this.ColumnsToRead = new();
            this.RowReader = source;
            this.Start = this.End = start;
            var mapping = source.Source.Db.Mapping(type);
            if(mapping != null)
            {
                this.PrepareDtoMapping(mapping);
            }
            else
            {
                var dto = source.Source.Db.Mapper.Get(type);
                this.PrepareDirectMapping(dto);
            }
        }

        internal void PrepareDirectMapping(IDtoClass dto)
        {
            var i = this.Start;
            while (this.ColumnsToRead.Count != dto.PropertiesCount && i < this.RowReader.Columns.Length)
            {
                var columnName = this.RowReader.Columns[i];
                var found = false;
#pragma warning disable S3267 // loop should be simplified with Linq
                foreach (var property in dto)
                {
                    if (string.Equals(property.Name, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.ColumnsToRead.Add(new ColumnMatch
                        {
                            Index = i,
                            Property = property
                        });
                        found = true;
                        this.End = i+1;
                        break;
                    }
                }
#pragma warning restore S3267// loop should be simplified with Linq

                if (!found)
                {
                    throw new FaaastOrmException($"Unexpected result column \"{columnName}\" while reading object \"{typeof(T).FullName}\", please check you query");
                }

                i++;
            }

            if (this.ColumnsToRead.Count != dto.PropertiesCount)
            {
                throw new FaaastOrmException($"Unexpected end of columns while reading object \"{typeof(T).FullName}\", please check you query to make sure all columns are returned");
            }
        }

        internal void PrepareDtoMapping(Mapping.TableMapping mapping)
        {
            var i = this.Start;
            while(this.ColumnsToRead.Count != mapping.ColumnMappings.Count && i < this.RowReader.Columns.Length)
            {
                var columnName = this.RowReader.Columns[i];
                var found = false;
#pragma warning disable S3267 // loop should be simplified with Linq
                foreach (var columnMapping in mapping.ColumnMappings)
                {
                    if(string.Equals(columnMapping.Column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.ColumnsToRead.Add(new ColumnMatch
                        {
                            Index = i,
                            Property = columnMapping.Property
                        });
                        found = true;
                        this.End = i + 1;
                        break;
                    }
                }
#pragma warning restore S3267// loop should be simplified with Linq

                if (!found)
                {
                    throw new FaaastOrmException($"Unexpected result column \"{columnName}\" while reading object \"{typeof(T).FullName}\", please update your mapping");
                }

                i++;
            }

            if(this.ColumnsToRead.Count != mapping.ColumnMappings.Count)
            {
                throw new FaaastOrmException($"Unexpected end of columns while reading object \"{typeof(T).FullName}\", please check you query to make sure all columns are returned");
            }
        }

        internal int? FirstIndexOf(string columnName)
        {
            for (var i = this.Start; i < this.RowReader.Columns.Length; i++)
            {
                if(string.Equals(this.RowReader.Columns[i], columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return null;
        }

        public override void Read()
        {

        }
    }
}
