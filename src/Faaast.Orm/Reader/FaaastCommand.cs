using Faaast.DatabaseModel;
using Faaast.Metadata;
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
                DtoClass map = Mapper.Get(Parameters.GetType());
                foreach (var property in map)
                {
                    var parameter = cmd.CreateParameter();
                    parameter.Value = property.Read(Parameters);
                    parameter.ParameterName = property.Name.Sanitize();
                    parameter.DbType = property.Type.ToDbType();
                    parameter.Direction = ParameterDirection.Input;
                }
            }

            return cmd;
        }
    }
}
