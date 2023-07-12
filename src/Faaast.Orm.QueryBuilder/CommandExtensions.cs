using System;
using System.Collections.Generic;
using System.Data.Common;
using Faaast.Orm.Reader;
using SqlKata;

namespace Faaast.Orm
{
    public static class CommandExtensions
    {
        public static FaaastCommand WithOptions(this FaaastCommand command, Action<FaaastCommand> options)
        {
            options(command);
            return command;
        }

        public static FaaastCommand CreateCommand(this Query query, DbConnection dbConnection = null)
        {
            var q = (FaaastQuery)query;
            var compiledQuery = q.Db.Compile(q);
            return q.Db.CreateCommand(compiledQuery.Sql, compiledQuery.Parameters, dbConnection);
        }

        public static void ExecuteReader(this Query query, Action<FaaastRowReader> stuff, DbConnection dbConnection = null)
        {
            using var command = CreateCommand(query, dbConnection);
            command.ExecuteReader(stuff);
        }

        public static int ExecuteNonQuery(this Query query, DbConnection dbConnection = null)
        {
            using var command = CreateCommand(query, dbConnection);
            return command.ExecuteNonQuery();
        }

        public static ICollection<T> ToList<T>(this Query query, DbConnection dbConnection = null)
        {
            using var command = query.CreateCommand(dbConnection);
            return command.ToList<T>();
        }

        public static T FirstOrDefault<T>(this Query query, DbConnection dbConnection = null)
        {
            using var command = query.CreateCommand(dbConnection);
            return command.FirstOrDefault<T>();
        }
    }
}
