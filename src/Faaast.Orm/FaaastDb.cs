using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Faaast.Orm
{
    public abstract class FaaastDb
    {
        internal virtual IObjectMapper Mapper { get; set; }

        internal virtual IDatabaseStore DbStore { get; set; }

        public virtual Lazy<DatabaseMapping> Mappings { get; }

        public abstract ConnectionSettings Connection { get; }

        public virtual DbConnection CreateConnection()
        {
            var connection =  Connection.Engine.Create();
            connection.ConnectionString = Connection.ConnectionString(Connection);
            return connection;
        }

        private FaaastDb()
        {
        }

        protected abstract IEnumerable<SimpleTypeMapping> GetMappings();

        protected FaaastDb(IServiceProvider services) : this()
        {
            this.Mapper = services.GetRequiredService<IObjectMapper>();
            this.DbStore = services.GetRequiredService<IDatabaseStore>();
            this.Mappings = new Lazy<DatabaseMapping>(InitMapping, true);
        }

        protected DatabaseMapping InitMapping()
        {
            if (Connection != null)
            {
                var database = this.DbStore[Connection.Name];
                if (database == null)
                {
                    database = Initialize(Connection, Mapper, GetMappings());
                    DbStore[Connection.Name] = database;
                }
                return database.Get(Meta.Mapping);
            }

            throw new ArgumentException(nameof(Connection));
        }

        internal static Database Initialize(ConnectionSettings connection, IObjectMapper mapper, IEnumerable<SimpleTypeMapping> mappings)
        {
            Database db = new Database(connection);
            List<TableMapping> tableMaps = new List<TableMapping>();
            foreach (var mapping in mappings)
            {
                var dto = mapper.Get(mapping.Type);
                mapping.Table.ObjectClass = dto;
                foreach (var columnMap in mapping.Table.ColumnMappings)
                {
                    columnMap.Property = dto[columnMap.Member.Name];
                }
                mapping.Table.Init();

                db.Tables.Add(mapping.Table.Table);
                tableMaps.Add(mapping.Table);
            }

            DatabaseMapping dbMap = new DatabaseMapping
            {
                Source = db,
                Mappings = tableMaps
            };

            db.Set(Meta.Mapping, dbMap);
            db.Set(Meta.Readers, new System.Collections.Concurrent.ConcurrentDictionary<Type, Reader.ObjectReader>());
            return db;
        }
    }
}
