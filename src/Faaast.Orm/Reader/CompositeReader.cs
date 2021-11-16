using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm;
using Faaast.Orm.Reader;
using System;
using System.Collections.Generic;
using System.Data;

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
            var parsers = Command.Database.Get(Meta.Readers);
            return parsers.GetOrAdd(type, x =>
            {
                var db = ((MetaModel<IDatabase>)Command.Database).Get(Meta.Mapping);
                if (!db.TypeToMapping.ContainsKey(type))
                    throw new ArgumentException($"No mapping found for type \"{type.FullName}\"");

                var mapping = db.TypeToMapping[type];
                return new ObjectReader(mapping);
            });
        }

        public CompositeReader(FaaastCommand command)
        {
            this.Command = command;
        }

        public CompositeReader(FaaastCommand command, IDataReader reader, params Type[] types) : this(command)
        {
            this.Matches = new List<Match>();

            Readers = new ObjectReader[types.Length];
            for (int typeIndex = 0; typeIndex < types.Length; typeIndex++)
            {
                Readers[typeIndex] = GetReader(types[typeIndex]);
            }

            for (int fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
            {
                string dataName = reader.GetName(fieldIndex);
                Match m = new Match() { DataName = dataName, DataIndex = fieldIndex };

                for (int typeIndex = 0; typeIndex < types.Length; typeIndex++)
                {
                    bool skip = false;
                    foreach (var found in this.Matches)
                    {
                        if (found.DataName == dataName && found.TypeIndex == typeIndex)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip)
                        continue;

                    var columns = Readers[typeIndex].Columns;
                    for (int colIndex = 0; colIndex < columns.Length; colIndex++)
                    {
                        if (columns[colIndex].ColumnName.Equals(dataName, StringComparison.OrdinalIgnoreCase))
                        {


                            m.TargetReader = Readers[typeIndex];
                            m.TargetColumn = Readers[typeIndex].Columns[colIndex];
                            m.TargetIndex = colIndex;
                            m.TypeIndex = typeIndex;
                            m.HasMatch = true;
                            this.Matches.Add(m);
                            break;
                        }
                    }
                    if (m.HasMatch)
                        break;
                }

            }
        }
        public object[] Read(IDataReader reader)
        {
            object[] results = new object[Readers.Length];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = Readers[i].NewInstance();
            }

            foreach (Match item in this.Matches)
            {
                if (item.HasMatch)
                {
                    item.TargetReader.Read(reader, item.DataIndex, item.TargetIndex, ref results[item.TypeIndex]);
                }
            }

            return results;
        }

        public static CompositeReader DynamicReader(FaaastCommand command, IDataReader reader)
        {
            CompositeReader result = new CompositeReader(command);
            var dynamicReader = ObjectReader.ForDynamic(reader);
            result.Readers = new ObjectReader[1] { dynamicReader };
            result.Matches = new List<Match>();

            int i = 0;
            foreach (var item in dynamicReader.Columns)
            {
                result.Matches.Add(new Match
                {
                    DataIndex = i,
                    DataName = item.ColumnName,
                    HasMatch = true,
                    TargetReader = dynamicReader,
                    TargetColumn = item,
                    TargetIndex = i,
                    TypeIndex = 0
                });
            }
            return result;
        }
    }
}