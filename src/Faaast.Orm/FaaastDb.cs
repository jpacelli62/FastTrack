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

        internal virtual DatabaseMapping Mappings { get; set; }

        public abstract ConnectionSettings Connection { get; }

        public virtual DbConnection CreateConnection()
        {
            var connection =  Connection.Engine.Create();
            connection.ConnectionString = Connection.ConnectionString(Connection);
            return connection;
        }

        protected FaaastDb()
        {
        }

        protected abstract IEnumerable<SimpleTypeMapping> GetMappings();

        public FaaastDb(IServiceProvider services) : this()
        {
            this.Mapper = services.GetRequiredService<IObjectMapper>();
            this.DbStore = services.GetRequiredService<IDatabaseStore>();
            if(this.Mappings == null)
            {
                this.Mappings = this.DbStore[Connection.Name].Get(Meta.Mapping);
                if(this.Mappings == null)
                {
                    Database db = new Database(Connection);
                    foreach (var mapping in Initialize(db, Mapper, GetMappings()))
                    {
                        db.Tables.Add(mapping.Table.Table);
                    }
                    DbStore[Connection.Name] = db;
                }
            }
        }

        internal static IEnumerable<SimpleTypeMapping> Initialize(IDatabase database, IObjectMapper mapper, IEnumerable<SimpleTypeMapping> mappings)
        {
            List<TableMapping> tableMaps = new List<TableMapping>();
            foreach (var mapping in mappings)
            {
                var dto = mapper.Get(mapping.Type);
                mapping.Table.ObjectClass = dto;
                foreach (var columnMap in mapping.Table.ColumnMappings)
                {
                    columnMap.Property = dto[columnMap.Member.Name];
                }

                yield return mapping;
                tableMaps.Add(mapping.Table);
            }

            DatabaseMapping dbMap = new DatabaseMapping
            {
                Source = database,
                Mappings = tableMaps
            };
            database.Set(Meta.Mapping, dbMap);
        }
    }
}
