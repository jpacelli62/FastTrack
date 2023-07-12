using System;
using System.Collections.Concurrent;

namespace Faaast.Orm.Model
{
    public class DatabaseStore
    {
        private ConcurrentDictionary<string, Database> Databases { get; } = new ConcurrentDictionary<string, Database>(StringComparer.OrdinalIgnoreCase);

        public Database this[string name]
        {
            get
            {
                this.Databases.TryGetValue(name, out var db);
                return db;
            }
            set => this.Databases.TryAdd(name, value);
        }
    }
}
