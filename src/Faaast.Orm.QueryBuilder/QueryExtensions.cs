namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        public static TableAlias<T> Alias<T>(this FaaastQueryDb db, string alias = null)
        {
            var mapping = db.Mapping<T>();
            return mapping != null ? new TableAlias<T>(mapping, alias) : null;
        }

        public static SqlKata.Query Query<T>(this SqlKata.Query query, TableAlias<T> alias) => query.Select(alias.AllColumns).From(alias);

        public static string AsSqlLike(this string value) => string.Concat("%", value ?? string.Empty, "%");
    }
}
