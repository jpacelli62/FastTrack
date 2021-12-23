using System;

namespace Faaast.Orm.Model
{
    public class ConnectionSettings
    {
        public string Name { get; set; }

        public SqlEngine Engine { get; set; }

        public Func<ConnectionSettings, string> ConnectionString { get; set; }

        public ConnectionSettings(string name, SqlEngine engine, string connectionString) : this(name, engine, x => connectionString)
        {
        }

        public ConnectionSettings(string name, SqlEngine engine, Func<ConnectionSettings, string> connectionString)
        {
            this.Name = name;
            this.Engine = engine;
            this.ConnectionString = connectionString;
        }
    }
}