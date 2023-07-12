using System;
using System.Collections.Generic;
using System.Data;
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
        internal virtual ObjectMapper Mapper { get; set; }

        internal virtual DatabaseStore DbStore { get; set; }

        internal virtual Database Database{ get; set; }

        internal virtual IServiceProvider Services { get; set; }

        public virtual Lazy<DatabaseMapping> Mappings { get; }

        public abstract ConnectionSettings Connection { get; }

        private FaaastDb()
        {
        }

        protected abstract IEnumerable<SimpleTypeMapping> LoadMappings();

        protected FaaastDb(IServiceProvider services) : this()
        {
            this.Mapper = services.GetRequiredService<ObjectMapper>();
            this.DbStore = services.GetRequiredService<DatabaseStore>();
            this.Mappings = new Lazy<DatabaseMapping>(this.InitMapping, true);
            this.Services = services;
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

                return this.Database.Mapping;
            }

            throw new ArgumentException(nameof(this.Connection));
        }

        internal static Database Initialize(ConnectionSettings connection, ObjectMapper mapper, IEnumerable<SimpleTypeMapping> mappings)
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

            db.Mapping = dbMap;
            return db;
        }

        public virtual FaaastCommand CreateCommand(
            string sql, 
            object parameters = null, 
            DbConnection dbConnection = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            DbTransaction transaction = null
            )
        {
            var connection = dbConnection;
            var handleConnection = dbConnection == null;
            if (handleConnection)
            {
                connection = this.Connection.Engine.Create();
                connection.ConnectionString = this.Connection.ConnectionString(this.Connection);
                connection.Open();
            }

            return this.CreateCommand(connection, handleConnection, sql, parameters, commandType, commandTimeout, transaction);
        }

        public virtual async Task<FaaastCommand> CreateCommandAsync(
            string sql, 
            object parameters = null, 
            DbConnection dbConnection = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            DbTransaction transaction = null,
            CancellationToken? cancellationToken = null
            )
        {
            var connection = dbConnection;
            var handleConnection = dbConnection == null;
            if (handleConnection)
            {
                connection = this.Connection.Engine.Create();
                connection.ConnectionString = this.Connection.ConnectionString(this.Connection);
                await connection.OpenAsync();
            }

            return this.CreateCommand(connection, handleConnection, sql, parameters, commandType, commandTimeout, transaction, cancellationToken);
        }

        internal FaaastCommand CreateCommand(
            DbConnection openConnection,
            bool handleConnexion,
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            int? commandTimeout = null,
            DbTransaction transaction = null,
            CancellationToken? cancellationToken = null)
        {
            var dbCommand = openConnection.CreateCommand();
            dbCommand.CommandText = sql ?? throw new ArgumentNullException(nameof(sql));
            dbCommand.Transaction = transaction;
            dbCommand.CommandType = commandType;

            if (commandTimeout.HasValue)
            {
                dbCommand.CommandTimeout = commandTimeout.Value;
            }

            if (parameters != null)
            {
                dbCommand.AddParameters(parameters, this.Mapper);
            }

            var command = new FaaastCommand(this, dbCommand)
            {
                AutoClose = handleConnexion,
                CancellationToken = cancellationToken ?? CancellationToken.None
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
