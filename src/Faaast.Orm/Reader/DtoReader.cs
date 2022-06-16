using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Faaast.Metadata;
using Faaast.Orm.Converters;
using Faaast.Orm.Model;

namespace Faaast.Orm.Reader
{
    public class DtoReader<T> : DataReader<T>
    {
        internal struct ColumnMatch
        {
            public IDtoProperty Property;
            public int Index;
            public bool Nullable;
            public bool IsKey;
            public IValueConverter Converter;
        }

        internal List<ColumnMatch> ColumnsToRead { get; set; }

        public IDtoClass MembersDto { get; set; }

        public IDtoClass InstanceDto { get; set; }

        protected bool HasKey { get; set; }

        public DtoReader(BaseRowReader source, int start)
        {
            var type = typeof(T);
            this.ColumnsToRead = new();
            this.RowReader = source;
            this.Start = this.End = start;
            var mapping = source.Source.Db.Mapping(type);
            if (mapping != null)
            {
                this.MembersDto = mapping.ObjectClass;
                this.PrepareDtoMapping(mapping);
            }
            else
            {
                this.MembersDto = source.Source.Db.Mapper.Get(type);
                this.PrepareDirectMapping();
            }

            this.InstanceDto = this.MembersDto;
        }

        internal void PrepareDirectMapping()
        {
            var i = this.Start;
            while (this.ColumnsToRead.Count != this.MembersDto.PropertiesCount && i < this.RowReader.Columns.Length)
            {
                var columnName = this.RowReader.Columns[i];
                var found = false;
#pragma warning disable S3267 // loop should be simplified with Linq
                foreach (var property in this.MembersDto)
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
                        this.End = i + 1;
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

            if (this.ColumnsToRead.Count != this.MembersDto.PropertiesCount)
            {
                throw new FaaastOrmException($"Unexpected end of columns while reading object \"{typeof(T).FullName}\", please check you query to make sure all columns are returned");
            }
        }

        internal void PrepareDtoMapping(Mapping.TableMapping mapping)
        {
            var i = this.Start;
            while (this.ColumnsToRead.Count != mapping.ColumnMappings.Count && i < this.RowReader.Columns.Length)
            {
                var columnName = this.RowReader.Columns[i];
                var found = false;
#pragma warning disable S3267 // loop should be simplified with Linq
                foreach (var columnMapping in mapping.ColumnMappings)
                {
                    if (string.Equals(columnMapping.Column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        var converterType = columnMapping.Column.Get(DbMeta.Converter);
                        var converterInstance = columnMapping.Column.Get(DbMeta.ConverterInstance);

                        if (converterInstance is null && converterType != null)
                        {
                            converterInstance = (IValueConverter)Activator.CreateInstance(converterType);
                            columnMapping.Column.Set(DbMeta.ConverterInstance, converterInstance);
                        }

                        this.ColumnsToRead.Add(new ColumnMatch
                        {
                            Index = i,
                            Property = columnMapping.Property,
                            Nullable = columnMapping.Column.Get(DbMeta.Nullable) ?? columnMapping.Property.Nullable,
                            Converter = converterInstance,
                            IsKey = columnMapping.Column.PrimaryKey
                        });
                        found = true;
                        this.HasKey |= columnMapping.Column.PrimaryKey;
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

            if (this.ColumnsToRead.Count != mapping.ColumnMappings.Count)
            {
                throw new FaaastOrmException($"Unexpected end of columns while reading object \"{typeof(T).FullName}\", please check you query to make sure all columns are returned");
            }
        }

        protected virtual void CreateInstance() => this.Value = (T)this.InstanceDto.CreateInstance();

        public override void Read()
        {
            this.CreateInstance();
            var readSomething = false;
            foreach (var property in this.ColumnsToRead)
            {
                if (property.Property.CanWrite)
                {
                    var colValue = this.RowReader.Buffer[property.Index];
                    if (property.Converter != null)
                    {
                        colValue = property.Converter.FromDb(colValue, property.Property.Type);
                    }

                    if (colValue != DBNull.Value)
                    {
                        readSomething = true;

                        var baseType = Nullable.GetUnderlyingType(property.Property.Type);
                        colValue = baseType != null ? Convert.ChangeType(colValue, baseType) : Convert.ChangeType(colValue, property.Property.Type);
                        property.Property.Write(this.Value, colValue);
                    }
                }
            }

            if(!readSomething)
            {
                this.Value = default;
            }
        }

        public override DataReader<TChild> ExtendedBy<TChild>()
        {
            var reader = new DtoExtendsReader<T, TChild>(this.RowReader, this.End)
            {
                ParentReader = this
            };

            this.InstanceDto = reader.InstanceDto;
            this.RowReader.ColumnsReaders.AddLast(reader);

            return reader;
        }

        public override DataReader<T> Distinct()
        {
            var reader = new DtoDistinctReader<T>(this.RowReader, this.Start)
            {
                SourceReader = this
            };

            this.RowReader.ColumnsReaders.AddLast(reader);

            return reader;
        }
    }
}
