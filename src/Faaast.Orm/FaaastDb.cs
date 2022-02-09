using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Faaast.Metadata;
using Faaast.Orm.Mapping;
using Faaast.Orm.Model;
using Faaast.Orm.Reader;
using Microsoft.Extensions.DependencyInjection;

namespace Faaast.Orm
{
    public abstract class FaaastDb
    {
        internal virtual IObjectMapper Mapper { get; set; }

        internal virtual IDatabaseStore DbStore { get; set; }

        internal virtual IDatabase Database{ get; set; }

        public virtual Lazy<DatabaseMapping> Mappings { get; }

        public abstract ConnectionSettings Connection { get; }

        private FaaastDb()
        {
        }

        protected abstract IEnumerable<SimpleTypeMapping> LoadMappings();

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
                this.Database = this.DbStore[this.Connection.Name];
                if (this.Database == null)
                {
                    this.Database = Initialize(this.Connection, this.Mapper, this.LoadMappings());
                    this.DbStore[this.Connection.Name] = this.Database;
                }

                return this.Database.Get(Meta.Mapping);
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
            return db;
        }

        public virtual FaaastCommand CreateCommand(string sql, object parameters = null, DbConnection dbConnection = null)
        {
            var connection = dbConnection;
            var handleConnection = dbConnection == null;
            if (handleConnection)
            {
                connection = this.Connection.Engine.Create();
                connection.ConnectionString = this.Connection.ConnectionString(this.Connection);
                connection.Open();
            }

            var command = new FaaastCommand(this, connection, sql, parameters)
            {
                AutoClose = true
            };

            return command;
        }

        public virtual async Task<AsyncFaaastCommand> CreateCommandAsync(string sql, object parameters = null, DbConnection dbConnection = null)
        {
            var connection = dbConnection;
            var handleConnection = dbConnection == null;
            if (handleConnection)
            {
                connection = this.Connection.Engine.Create();
                connection.ConnectionString = this.Connection.ConnectionString(this.Connection);
                await connection.OpenAsync();
            }

            var command = new AsyncFaaastCommand(this, connection, sql, parameters)
            {
                AutoClose = true
            };

            return command;
        }

        public TableMapping Mapping<TClass>() => this.Mapping(typeof(TClass));

        public TableMapping Mapping(Type type)
        {
            var mapping = this.Mappings.Value;
            return mapping.TypeToMapping.TryGetValue(type, out var found) ? found : null;
        }
    }
}
