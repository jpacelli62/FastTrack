using System;
using System.Collections.Generic;
using System.Data;
using Faaast.Metadata;
using Faaast.Orm.Model;

namespace Faaast.Orm.Reader
{
    public class CompositeReader
    {
        private struct Match
        {
            public string DataName;
            public int DataIndex;

            public ObjectReader TargetReader;
            public ColumnReader TargetColumn;
            public int TargetIndex;
            public int TypeIndex;
            public bool HasMatch;
        }
        private List<Match> Matches { get; set; }
        public FaaastCommand Command { get; set; }

        private ObjectReader[] Readers { get; set; }

        public ObjectReader GetReader(Type type)
        {
            var parsers = this.Command.Database.Get(Meta.Readers);
            return parsers.GetOrAdd(type, x =>
            {
                var db = ((MetaModel<IDatabase>)this.Command.Database).Get(Meta.Mapping);
                if (!db.TypeToMapping.ContainsKey(type))
                {
                    throw new ArgumentException($"No mapping found for type \"{type.FullName}\"");
                }

                var mapping = db.TypeToMapping[type];
                return new ObjectReader(mapping);
            });
        }

        public CompositeReader(FaaastCommand command) => this.Command = command;

        public CompositeReader(FaaastCommand command, IDataReader reader, params Type[] types) : this(command)
        {
            this.Matches = new List<Match>();

            this.Readers = new ObjectReader[types.Length];
            for (var typeIndex = 0; typeIndex < types.Length; typeIndex++)
            {
                this.Readers[typeIndex] = this.GetReader(types[typeIndex]);
            }

            for (var fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
            {
                var dataName = reader.GetName(fieldIndex);
                var m = new Match() { DataName = dataName, DataIndex = fieldIndex };

                for (var typeIndex = 0; typeIndex < types.Length; typeIndex++)
                {
                    var skip = false;
                    foreach (var found in this.Matches)
                    {
                        if (found.DataName == dataName && found.TypeIndex == typeIndex)
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (skip)
                    {
                        continue;
                    }

                    var columns = this.Readers[typeIndex].Columns;
                    for (var colIndex = 0; colIndex < columns.Length; colIndex++)
                    {
                        if (columns[colIndex].ColumnName.Equals(dataName, StringComparison.OrdinalIgnoreCase))
                        {

                            m.TargetReader = this.Readers[typeIndex];
                            m.TargetColumn = this.Readers[typeIndex].Columns[colIndex];
                            m.TargetIndex = colIndex;
                            m.TypeIndex = typeIndex;
                            m.HasMatch = true;
                            this.Matches.Add(m);
                            break;
                        }
                    }

                    if (m.HasMatch)
                    {
                        break;
                    }
                }
            }
        }

        public object[] Read(IDataReader reader)
        {
            var results = new object[this.Readers.Length];
            for (var i = 0; i < results.Length; i++)
            {
                results[i] = this.Readers[i].NewInstance();
            }

            foreach (var item in this.Matches)
            {
                if (item.HasMatch)
                {
                    item.TargetReader.Read(reader, item.DataIndex, item.TargetIndex, ref results[item.TypeIndex]);
                }
            }

            return results;
        }

        //public static CompositeReader DynamicReader(FaaastCommand command, IDataReader reader)
        //{
        //    var result = new CompositeReader(command);
        //    var dynamicReader = ObjectReader.ForDynamic(reader);
        //    result.Readers = new ObjectReader[1] { dynamicReader };
        //    result.Matches = new List<Match>();

        //    var i = 0;
        //    foreach (var item in dynamicReader.Columns)
        //    {
        //        result.Matches.Add(new Match
        //        {
        //            DataIndex = i,
        //            DataName = item.ColumnName,
        //            HasMatch = true,
        //            TargetReader = dynamicReader,
        //            TargetColumn = item,
        //            TargetIndex = i,
        //            TypeIndex = 0
        //        });
        //    }

        //    return result;
        //}
    }
}