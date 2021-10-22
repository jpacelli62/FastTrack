using Faaast.DatabaseModel;
using Faaast.Metadata;
using System.Collections;
using System.Data;
using System.Data.Common;
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
        public FaaastCommand(
            IDatabase database,
            IObjectMapper mapper,
            DbConnection connection,
            string commandText,
            object parameters = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
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
            this.CommandBehavior = CommandBehavior.Default;
        }

        internal DbCommand SetupCommand()
        {
            var cmd = (DbCommand)Connection.CreateCommand();
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
                        var parameter = cmd.CreateParameter();
                        parameter.Value = value;
                        parameter.ParameterName = key.ToString().Sanitize();
                        if(value != null)
                        {
                            parameter.DbType = value.GetType().ToDbType();
                        }
                        parameter.Direction = ParameterDirection.Input;
                        cmd.Parameters.Add(parameter);
                    }
                }
                else
                {
                    DtoClass map = Mapper.Get(Parameters.GetType());
                    foreach (var property in map)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.Value = property.Read(Parameters);
                        parameter.ParameterName = property.Name.Sanitize();
                        parameter.DbType = property.Type.ToDbType();
                        parameter.Direction = ParameterDirection.Input;
                        cmd.Parameters.Add(parameter);
                    }
                }

            }

            return cmd;
        }
    }
}
