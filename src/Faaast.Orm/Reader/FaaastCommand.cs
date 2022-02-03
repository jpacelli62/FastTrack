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
            DbConnection connection,
            string commandText,
            object parameters = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            bool handleConnection = true,
            CancellationToken cancellationToken = default)
        {
            this.Database = database;
            this.Mapper = mapper;
            this.Connection = connection;
            this.CommandText = commandText;
            this.Parameters = parameters;
            this.Transaction = transaction;
            this.CommandTimeout = commandTimeout;
            this.CommandType = commandType;
            this.CancellationToken = cancellationToken;
            this.CommandBehavior = CommandBehavior.SequentialAccess;
            this.HandleConnection = handleConnection;
        }

        internal DbCommand Setup()
        {
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

        public async Task<DbCommand> PreprareAsync()
        {
            var cmd = this.Setup();
#if NET_5
            await cmd.PrepareAsync(this.CancellationToken ?? CancellationToken.None).ConfigureAwait(false);
#else
            cmd.Prepare();
            await Task.CompletedTask;
#endif
            return cmd;
        }

        public DbCommand Preprare()
        {
            var cmd = this.Setup();
            cmd.Prepare();
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
                parameter.Size = value == null ? 0 : Encoding.Unicode.GetByteCount((string)value);
            }

            command.Parameters.Add(parameter);
        }
    }
}
