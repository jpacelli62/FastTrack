using System.Linq;
using SqlKata;

namespace Faaast.Orm
{
    public class FaaastQuery : SqlKata.Query
    {
        public FaaastQueryDb Db { get; set; }

        public FaaastQuery(FaaastQueryDb db) => this.Db = db;

        public FaaastQuery(FaaastQueryDb db, string table) : base(table) => this.Db = db;

        public CompiledQuery Compile() => this.Db.Compile(this);

        public override Query Clone()
        {

            var query = new FaaastQuery(this.Db)
            {
                Clauses = this.Clauses.Select(x => x.Clone()).ToList(),
                QueryAlias = this.QueryAlias,
                IsDistinct = this.IsDistinct,
                Method = this.Method,
                Includes = Includes,
                Variables = Variables
            };

            query.SetEngineScope(EngineScope);

            return query;
        }

        public override Query NewQuery() => new FaaastQuery(this.Db);
    }
}
