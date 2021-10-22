//using Faaast.DatabaseModel;
//using System;
//using System.Linq.Expressions;

//namespace Faaast.Orm
//{
//    public class TableQuery<TA>
//    {
//        internal FaaastDb Db { get; set; }

//        internal SqlKata.Query Query { get; set; }

//        internal TableMapping Mapping { get; set; }

//        public TableQuery(FaaastDb db)
//        {
//            var type = typeof(TA);
//            this.Db = db;
//            this.Mapping = this.Db.Mappings.TypeToMapping[type];
//            if (this.Mapping == null)
//                throw new Exception(string.Concat("No mapping for type ", type.Name));

//            this.Query = new SqlKata.Query(Mapping.Table.Name);
//        }
//    }

//    public static partial class QueryTableExtensions
//    {
//        public static TableQuery<TA> From<TA>(this FaaastDb db)
//        {
//            return new TableQuery<TA>(db);
//        }

//        public static TableQuery<TA> Where<TA, TB>(this TableQuery<TA> query, Expression<Func<TA, TB>> property, TB value)
//        {
//            var member = property.Body as MemberExpression;
//            if (member == null)
//                throw new NotImplementedException();
            
//            query.Query.Where(member.Member.Name, value);
//            return query;
//        }
//    }
//}
