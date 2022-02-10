using System;
using System.Collections.Generic;
using Faaast.Metadata;
using Faaast.Orm.Converters;
using Faaast.Orm.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Faaast.Orm.Reader
{
    public class DtoReader<T> : DataReader<T>
    {
        internal struct ColumnMatch
        {
            public IDtoProperty Property;
            public int Index;
            public bool Nullable;
            public IValueConverter Converter;
        }

        internal List<ColumnMatch> ColumnsToRead { get; set; }

        internal IDtoClass Dto { get; set; }


        public DtoReader(BaseRowReader source, int start)
        {
            var type = typeof(T);
            this.ColumnsToRead = new();
            this.RowReader = source;
            this.Start = this.End = start;
            var mapping = source.Source.Db.Mapping(type);
            if(mapping != null)
            {
                this.Dto = mapping.ObjectClass;
                this.PrepareDtoMapping(mapping);
            }
            else
            {
                this.Dto = source.Source.Db.Mapper.Get(type);
                this.PrepareDirectMapping();
            }
        }

        internal void PrepareDirectMapping()
        {
            var i = this.Start;
            while (this.ColumnsToRead.Count != this.Dto.PropertiesCount && i < this.RowReader.Columns.Length)
            {
                var columnName = this.RowReader.Columns[i];
                var found = false;
#pragma warning disable S3267 // loop should be simplified with Linq
                foreach (var property in this.Dto)
                {
                    if (string.Equals(property.Name, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.ColumnsToRead.Add(new ColumnMatch
                        {
                            Index = i,
                            Property = property,
                            Nullable = property.Nullable
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

            if (this.ColumnsToRead.Count != this.Dto.PropertiesCount)
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
                        var converterType = columnMapping.Column.Get(DbMeta.Converter);
                        IValueConverter converter = null;
                        if(converterType != null)
                        {
                            converter = (IValueConverter)this.RowReader.Source.Db.Services.GetRequiredService(converterType);
                        }
                        this.ColumnsToRead.Add(new ColumnMatch
                        {
                            Index = i,
                            Property = columnMapping.Property,
                            Nullable = columnMapping.Column.Get(DbMeta.Nullable) ?? columnMapping.Property.Nullable,
                            Converter = converter
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

        public override void Read()
        {
            this.Value = (T)this.Dto.CreateInstance();
            foreach (var property in this.ColumnsToRead)
            {
                if(property.Property.CanWrite)
                {
                    var colValue = this.RowReader.Buffer[property.Index];
                    if(colValue == DBNull.Value && property.Nullable)
                    {
                        colValue = default;
                    }

                    if(property.Converter != null)
                    {
                        colValue = property.Converter.FromDb(colValue, property.Property.Type);
                    }

                    property.Property.Write(this.Value, colValue);
                }
            }
        }
    }
}
