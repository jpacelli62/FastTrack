using System;
using System.Collections.Generic;
using System.Data.Common;
using Faaast.Metadata;
using Faaast.Orm.Mapping;
using Faaast.Orm.Model;
using Microsoft.Extensions.DependencyInjection;

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
            var connection = this.Connection.Engine.Create();
            connection.ConnectionString = this.Connection.ConnectionString(this.Connection);
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
            this.Mappings = new Lazy<DatabaseMapping>(this.InitMapping, true);
        }

        protected DatabaseMapping InitMapping()
        {
            if (this.Connection != null)
            {
                var database = this.DbStore[this.Connection.Name];
                if (database == null)
                {
                    database = Initialize(this.Connection, this.Mapper, this.GetMappings());
                    this.DbStore[this.Connection.Name] = database;
                }

                return database.Get(Meta.Mapping);
            }

            throw new ArgumentException(nameof(this.Connection));
        }

        internal static Database Initialize(ConnectionSettings connection, IObjectMapper mapper, IEnumerable<SimpleTypeMapping> mappings)
        {
            var db = new Database(connection);
            var tableMaps = new List<TableMapping>();
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

            var dbMap = new DatabaseMapping
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
