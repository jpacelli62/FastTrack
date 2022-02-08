using System.Collections.Generic;
using System.Threading.Tasks;
using Faaast.Orm.Reader;

namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        public static FaaastCommand CreateCommand(this FaaastQuery query)
        {
            var compiledQuery = query.Compile();
            return query.Db.CreateCommand(compiledQuery.Sql, compiledQuery.Parameters);
        }

        public static Task<AsyncFaaastCommand> CreateCommandAsync(this FaaastQuery query)
        {
            var compiledQuery = query.Compile();
            return query.Db.CreateCommandAsync(compiledQuery.Sql, compiledQuery.Parameters);
        } 
        
        public static TClass FirstOrDefault<TClass>(this FaaastQuery query)
        {
            using var command = query.CreateCommand();
            using var reader = command.ExecuteReader();
            var objReader = reader.AddReader<TClass>();
            return reader.Read() ? objReader.Value : default;
        }

        public static async Task<TClass> FirstOrDefaultAsync<TClass>(this FaaastQuery<TClass> query)
        {
            await using var command = await query.CreateCommandAsync();
            await using var reader = await command.ExecuteReaderAsync();
            var objReader = reader.AddReader<TClass>();
            return await reader.ReadAsync() ? objReader.Value : default;
        }

        public static ICollection<TClass> ToList<TClass>(this FaaastQuery query)
        {
            using var command = query.CreateCommand();
            using var reader = command.ExecuteReader();
            var objReader = reader.AddReader<TClass>();
            var result = new List<TClass>();
            while(reader.Read())
            {
                result.Add(objReader.Value);
            }

            return result;
        }

        public static async Task<ICollection<TClass>> ToListAsync<TClass>(this FaaastQuery<TClass> query)
        {
            await using var command = await query.CreateCommandAsync();
            await using var reader = await command.ExecuteReaderAsync();
            var objReader = reader.AddReader<TClass>();
            var result = new List<TClass>();
            while (await reader.ReadAsync())
            {
                result.Add(objReader.Value);
            }

            return result;
        }
    }
}
