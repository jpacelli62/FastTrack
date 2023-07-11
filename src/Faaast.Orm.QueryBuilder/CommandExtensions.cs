using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
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
            ExecuteReader(command, stuff);
        }

        public static void ExecuteReader(this FaaastCommand command, Action<FaaastRowReader> stuff)
        {
            using var c = command;
            using var reader = command.ExecuteReader();
            stuff(reader);
        }

        public static int ExecuteNonQuery(this Query query, DbConnection dbConnection = null)
        {
            using var command = CreateCommand(query, dbConnection);
            return command.ExecuteNonQuery();
        }

        public static ICollection<T> ToList<T>(this Query query, DbConnection dbConnection = null)
        {
            using var command = query.CreateCommand(dbConnection);
            return ToList<T>(command);
        }

        public static ICollection<T> ToList<T>(this FaaastCommand command)
        {
            var result = new List<T>();
            ExecuteReader(command, reader =>
            {
                var tReader = reader.AddReader<T>();
                while (reader.Read())
                {
                    result.Add(tReader.Value);
                }
            });

            return result;
        }

        public static T FirstOrDefault<T>(this Query query, DbConnection dbConnection = null)
        {
            using var command = query.CreateCommand(dbConnection);
            return FirstOrDefault<T>(command);
        }

        public static T FirstOrDefault<T>(this FaaastCommand command)
        {
            T result = default;
            ExecuteReader(command, reader =>
            {
                var tReader = reader.AddReader<T>();
                if (reader.Read())
                {
                    result = tReader.Value;
                }
            });

            return result;
        }
    }
}
