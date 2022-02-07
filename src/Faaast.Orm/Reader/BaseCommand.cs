using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace Faaast.Orm.Reader
{
    public class BaseCommand
    {
        public FaaastDb Db { get; set; }
        public DbConnection Connection { get; set; }
        public DbCommand Command { get; set; }

        public string CommandText { get; set; }
        public object Parameters { get; set; }
        public DbTransaction Transaction { get; set; }
        public int? CommandTimeout { get; set; }
        public CommandType? CommandType { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public CommandBehavior CommandBehavior { get; set; }

        public bool AutoClose { get; set; }

        public BaseCommand(FaaastDb db, DbConnection dbConnection, string commandText, object parameters = null)
        {
            this.Db = db ?? throw new ArgumentNullException(nameof(db));
            this.Connection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            this.CommandText = commandText;
            this.Parameters = parameters;
            this.CommandType = System.Data.CommandType.Text;
            this.CancellationToken = CancellationToken.None;
            this.CommandBehavior = CommandBehavior.SequentialAccess;
            this.AutoClose = false;
        }

        internal DbCommand CreateInternalCommand()
        {
            this.Command = this.Connection.CreateCommand();
            this.Command.CommandText = this.CommandText;

            if (this.Transaction != null)
            {
                this.Command.Transaction = this.Transaction;
            }

            if (this.CommandTimeout.HasValue)
            {
                this.Command.CommandTimeout = this.CommandTimeout.Value;
            }

            if (this.CommandType.HasValue)
            {
                this.Command.CommandType = this.CommandType.Value;
            }

            if (this.Parameters != null)
            {
                if (this.Parameters is IDictionary dictionary)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        var value = dictionary[key];
                        AddParameter(this.Command, key.ToString(), value, value?.GetType(), ParameterDirection.Input);
                    }
                }
                else
                {
                    var map = this.Db.Mapper.Get(this.Parameters.GetType());
                    foreach (var property in map)
                    {
                        AddParameter(this.Command, property.Name, property.Read(this.Parameters), property.Type, ParameterDirection.Input);
                    }
                }
            }

            return this.Command;
        }

        internal static void AddParameter(DbCommand command, string name, object value, Type valueType, ParameterDirection direction)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name.Sanitize();
            parameter.Value = value;
            parameter.Direction = direction;
            parameter.Size = 0;
            if(value == null || value == DBNull.Value)
            {
                parameter.Value = DBNull.Value;
                parameter.IsNullable = true;
            }
            else
            {
                if (valueType != null)
                {
                    parameter.DbType = valueType.ToDbType();
                }

                if (parameter.DbType == DbType.String)
                {
                    parameter.Size = Encoding.Unicode.GetByteCount((string)value);
                }
            }

            command.Parameters.Add(parameter);
        }
    }
}
