using System.Collections.Generic;

namespace Faaast.Orm
{
    public class CompiledQuery
    {
        public string Sql { get; set; }

        public Dictionary<string, object> Parameters { get; set; }

        public CompiledQuery(string sql, Dictionary<string, object> parameters)
        {
            this.Sql = sql;
            this.Parameters = parameters;
        }
    }
}
