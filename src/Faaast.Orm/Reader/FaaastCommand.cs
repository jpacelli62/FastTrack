using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faaast.Metadata;

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
        public CommandBehavior CommandBehavior { get; set; }

        public IObjectMapper Mapper { get; set; }
        public FaaastDb Db { get; set; }
        public bool HandleConnection { get; set; }

        public FaaastCommand(
            FaaastDb db,
            string commandText,
            object parameters = null
            )
        {
            this.Db = db;
            this.Mapper = db.Mapper;
            this.Connection = null;
            this.CommandText = commandText;
            this.Parameters = parameters;
            this.Transaction = null;
            this.CommandTimeout = null;
            this.CommandType = System.Data.CommandType.Text;
            this.CancellationToken = CancellationToken.None;
            this.CommandBehavior = CommandBehavior.SequentialAccess;
            this.HandleConnection = false;
        }

        public void Setup(DbCommand cmd)
        {
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
        }

        internal static void AddParameter(DbCommand command, string name, object value, Type valueType, ParameterDirection direction)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name.Sanitize();
            parameter.Value = value;
            parameter.Direction = direction;
            parameter.IsNullable = value == null;

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

        public DbCommand PreCall()
        {
            if (this.Connection == null)
            {
                this.HandleConnection = true;
                this.Connection = this.Db.Connection.Engine.Create();
                this.Connection.ConnectionString = this.Db.Connection.ConnectionString(this.Db.Connection);
                this.Connection.Open();
            }

            var cmd = this.Connection.CreateCommand();
            this.Setup(cmd);
            cmd.Prepare();
            return cmd;
        }

        public async Task<DbCommand> PreCallAsync()
        {
            if (this.Connection == null)
            {
                this.HandleConnection = true;
                this.Connection = this.Db.Connection.Engine.Create();
                this.Connection.ConnectionString = this.Db.Connection.ConnectionString(this.Db.Connection);
                await this.Connection.OpenAsync(this.CancellationToken).ConfigureAwait(false);
            }

            var cmd = this.Connection.CreateCommand();
            this.Setup(cmd);
#if NET_5
            cmd.PrepareAsync(this.CancellationToken).ConfigureAwait(false);
#else
            cmd.Prepare();
#endif            
            return cmd;
        }
        public void PostCall(DbCommand cmd)
        {
            cmd.Dispose();
            if (this.HandleConnection)
            {
                this.Connection.Close();
                this.Connection.Dispose();
            }
        }

        public async Task PostCallAsync(DbCommand cmd)
        {
#if NET_5
            await cmd.DisposeAsync(this.CancellationToken).ConfigureAwait(false);
            if (this.HandleConnection)
            {
                await this.Connection.CloseAsync().ConfigureAwait(false);
                await this.Connection.DisposeAsync().ConfigureAwait(false);
            }
#else
            this.PostCall(cmd);
            await Task.CompletedTask;
#endif
        }

        public async Task<int> ExecuteNonQueryAsync()
        {
            var dbCommand = await this.PreCallAsync().ConfigureAwait(false);
            var result = await dbCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            await this.PostCallAsync(dbCommand).ConfigureAwait(false);

            return result;
        }

        public int ExecuteNonQuery()
        {
            var dbCommand = this.PreCall();
            var result = dbCommand.ExecuteNonQuery();
            this.PostCall(dbCommand);

            return result;
        }

        public async Task<FaaastRowReader> ExecuteReaderAsync()
        {
            var dbCommand = await this.PreCallAsync().ConfigureAwait(false);
            var reader = new FaaastRowReader(this, dbCommand);
            await reader.PrepareAsync().ConfigureAwait(false);
            return reader;
        }

        public FaaastRowReader ExecuteReader()
        {
            var dbCommand = this.PreCall();
            var reader = new FaaastRowReader(this, dbCommand);
            reader.Prepare();
            return reader;
        }
    }
}
