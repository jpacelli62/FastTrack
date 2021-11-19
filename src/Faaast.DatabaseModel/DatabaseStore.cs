using System;
using System.Collections.Concurrent;

namespace Faaast.DatabaseModel
{
    public class DatabaseStore : IDatabaseStore
    {
        private ConcurrentDictionary<string, IDatabase> Databases { get; } = new ConcurrentDictionary<string, IDatabase>(StringComparer.OrdinalIgnoreCase);

        public IDatabase this[string name]
        {
            get
            {
                this.Databases.TryGetValue(name, out var db);
                return db;
            }
            set => this.Databases.AddOrUpdate(name, value, (a, b) => value);
        }
    }
}
