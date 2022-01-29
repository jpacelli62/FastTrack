using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faaast.Metadata;
using Faaast.Orm.Model;

namespace Faaast.Orm.Reader
{
    public struct FaaastCommand
    {
        public string CommandText { get; set; }
        public object Parameters { get; set; }
        public DbTransaction Transaction { get; set; }
        public int? CommandTimeout { get; set; }
        public CommandType? CommandType { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public DbConnection Connection { get; set; }
        public IObjectMapper Mapper { get; set; }
        public IDatabase Database { get; set; }
        public CommandBehavior CommandBehavior { get; set; }
        public bool HandleConnection { get; set; }

        public FaaastCommand(
            IDatabase database,
            IObjectMapper mapper,
            string commandText,
            object parameters = null
            )
        {
            this.Database = database;
            this.Mapper = mapper;
            this.Connection = null;
            this.CommandText = commandText;
            this.Parameters = parameters;
            this.Transaction = null;
            this.CommandTimeout = null;
            this.CommandType = System.Data.CommandType.Text;
            this.CancellationToken = default;
            this.CommandBehavior = CommandBehavior.SequentialAccess;
            this.HandleConnection = false;
        }

        internal DbCommand SetupCommand()
        {
            if (this.Connection == null)
            {
                this.HandleConnection = true;
                this.Connection = this.Database.Connexion.Engine.Create();
                this.Connection.ConnectionString = this.Database.Connexion.ConnectionString(this.Database.Connexion);
            }

            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = this.CommandText;

            if (this.Transaction != null)
            {
                cmd.Transaction = this.Transaction;
            }

            if (this.CommandTimeout.HasValue)
            {
                cmd.CommandTimeout = this.CommandTimeout.Value;
            }

            if (this.CommandType.HasValue)
            {
                cmd.CommandType = this.CommandType.Value;
            }

            if (this.Parameters != null)
            {
                if (this.Parameters is IDictionary dictionary)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        var value = dictionary[key];
                        AddParameter(cmd, key.ToString(), value, value?.GetType(), ParameterDirection.Input);
                    }
                }
                else
                {
                    var map = this.Mapper.Get(this.Parameters.GetType());
                    foreach (var property in map)
                    {
                        AddParameter(cmd, property.Name, property.Read(this.Parameters), property.Type, ParameterDirection.Input);
                    }
                }
            }

            return cmd;
        }

        internal static void AddParameter(DbCommand command, string name, object value, Type valueType, ParameterDirection direction)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name.Sanitize();
            parameter.Value = value;
            parameter.Direction = direction;

            if (valueType != null)
            {
                parameter.DbType = valueType.ToDbType();
            }

            if (parameter.DbType == DbType.String)
            {
                parameter.Size = Encoding.Unicode.GetByteCount((string)value);
            }

            command.Parameters.Add(parameter);
        }

        internal static Task TryPrepareAsync(DbCommand dbCommand, CancellationToken cancellationToken)
        {
#if NET_5
            return dbCommand.PrepareAsync(cancellationToken);
#endif
            if (!cancellationToken.IsCancellationRequested)
            {
                dbCommand.Prepare();
            }

            return Task.CompletedTask;
        }

        internal Task TryCloseAsync(CancellationToken cancellationToken)
        {
            if (this.HandleConnection)
            {
#if NET_5
            return this.Connection.CloseAsync(cancellationToken);
#else
                if (!cancellationToken.IsCancellationRequested)
                {
                    this.Connection.Close();
                }
#endif
            }
            return Task.CompletedTask;
        }

        internal Task TryDisposeAsync(CancellationToken cancellationToken)
        {
            if (this.HandleConnection)
            {
#if NET_5
            return this.Connection.DisposeAsync(cancellationToken);
#else
                if (!cancellationToken.IsCancellationRequested)
                {
                    this.Connection.Dispose();
                }
#endif
            }
            return Task.CompletedTask;

        }
    }
}
