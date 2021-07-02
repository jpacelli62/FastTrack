using System.Data;
using System.Threading;
using Faaast.Metadata;

namespace Faaast.Orm.Reader
{
    public struct FaaastCommand
    {
        public string CommandText { get; }
        public object Parameters { get; }
        public IDbTransaction Transaction { get; }
        public int? CommandTimeout { get; }
        public CommandType? CommandType { get; }
        public CancellationToken CancellationToken { get; }
        public IDbConnection Connection { get; }
        public IObjectMapper Mapper { get; }

        public FaaastCommand(
            IObjectMapper mapper,
            IDbConnection connection,
            string commandText,
            object parameters = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            this.Mapper = mapper;
            Connection = connection;
            CommandText = commandText;
            Parameters = parameters;
            Transaction = transaction;
            CommandTimeout = commandTimeout;
            CommandType = commandType;
            CancellationToken = cancellationToken;
        }

        internal IDbCommand SetupCommand()
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
