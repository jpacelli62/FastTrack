using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Mapping;
using Faaast.Orm.Reader;
using Faaast.Orm.Resolver;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace Faaast.Orm
{
    public class FaaastOrm
    {
        public IObjectMapper Mapper { get; set; }

        public IDatabaseStore DbStore { get; set; }

        public IDbConnection DbConnection { get; set; }

        internal MappedDatabase Database { get; set; }

        

        private ConcurrentDictionary<Type, ObjectReader> Parsers { get; set; }

        public FaaastOrm(IServiceProvider services)
        {
            this.Mapper = services.GetRequiredService<IObjectMapper>();
            this.DbStore = services.GetRequiredService<IDatabaseStore>();
            this.Parsers = new ConcurrentDictionary<Type, ObjectReader>();
        }

        public ObjectReader GetRowParser(Type type)
        {
            return Parsers.GetOrAdd(type, x =>
            {
                var mapping = Database.TypeToMapping[type];
                return new ObjectReader(mapping.Table.Columns, mapping);
            });
        }

        public IEnumerable<T> Query<T>(FaaastCommand commandDefinition)
        {
            using (IDbCommand command = commandDefinition.SetupCommand())
            {
                var type = typeof(T);
                command.Prepare();
                ObjectReader rowParsers = GetRowParser(type);
                var dbReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                int[] indices = ResolveColumnOrder(dbReader, rowParsers);
                while (dbReader.Read())
                {
                    yield return (T)rowParsers.Read(dbReader, indices, 0);
                }
            }
        }

        public IEnumerable<object[]> Query(FaaastCommand command, params Type[] types)
        {
            using (IDbCommand dbCommand = command.SetupCommand())
            {
                dbCommand.Prepare();
                ObjectReader[] rowParsers = new ObjectReader[types.Length];
                for (int i = 0; i < types.Length; i++)
                {
                    rowParsers[i] = GetRowParser(types[i]);
                }
                var dbReader = dbCommand.ExecuteReader(CommandBehavior.SequentialAccess);
                int[] indexes = ResolveColumnOrder(dbReader, rowParsers);

                while (dbReader.Read())
                {
                    object[] results = new object[types.Length];
                    int start = 0;
                    for (int i = 0; i < types.Length; i++)
                    {
                        results[i] = rowParsers[i].Read(dbReader, indexes, start);
                        start += rowParsers[i].ColumnsReaders.Length;
                    }

                    yield return results;
                }
            }
        }

        internal static int[] ResolveColumnOrder(IDataReader reader, params ObjectReader[] objects)
        {
            int startingindex = 0;
            int fieldCount = reader.FieldCount;
            int[] order = new int[fieldCount];
            for (int i = 0; i < fieldCount; i++)//3
            {
                order[i] = -1;
            }

            foreach (var columns in objects)
            {
                int columnsCount = columns.ColumnsNames.Length;
                int nextIndex = startingindex + columnsCount;
                for (int i = startingindex; i < nextIndex; i++)
                {
                    string name = reader.GetName(i);
                    for (int j = 0; j < columnsCount; j++)
                    {
                        if (columns.ColumnsNames[j].Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            order[j + startingindex] = i;
                            break;
                        }
                    }
                }

                startingindex += columnsCount;
            }

            if (startingindex != fieldCount)
                throw new InvalidOperationException("columns count different than expected");

            return order;
        }
    }
}
