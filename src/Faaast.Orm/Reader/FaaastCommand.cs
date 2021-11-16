using Faaast.DatabaseModel;
using Faaast.Metadata;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace Faaast.Orm.Reader
{
    public struct FaaastCommand
    {
        public string CommandText { get; }
        public object Parameters { get; }
        public DbTransaction Transaction { get; }
        public int? CommandTimeout { get; }
        public CommandType? CommandType { get; }
        public CancellationToken CancellationToken { get; }
        public DbConnection Connection { get; }
        public IObjectMapper Mapper { get; }
        public IDatabase Database { get; }
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
            CancellationToken cancellationToken = default,
            bool handleConnection = true)
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

        internal DbCommand SetupCommand()
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = CommandText;

            if (Transaction != null)
                cmd.Transaction = Transaction;

            if (CommandTimeout.HasValue)
                cmd.CommandTimeout = CommandTimeout.Value;

            if (CommandType.HasValue)
                cmd.CommandType = CommandType.Value;

            if (Parameters != null)
            {
                if(Parameters is IDictionary dictionary)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        var value = dictionary[key];
                        AddParameter(cmd, key.ToString(), value, value?.GetType(), ParameterDirection.Input);
                    }
                }
                else
                {
                    DtoClass map = Mapper.Get(Parameters.GetType());
                    foreach (var property in map)
                    {
                        AddParameter(cmd, property.Name, property.Read(Parameters), property.Type, ParameterDirection.Input);
                    }
                }
            }

            return cmd;
        }

        internal void AddParameter(DbCommand command, string name, object value, Type valueType, ParameterDirection direction)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name.Sanitize();
            parameter.Value = value;
            parameter.Direction = direction;

            if(valueType != null)
                parameter.DbType = valueType.ToDbType();

            if (parameter.DbType == DbType.String)
                parameter.Size = Encoding.Unicode.GetByteCount((string)value);

            command.Parameters.Add(parameter);
        }
    }
}
