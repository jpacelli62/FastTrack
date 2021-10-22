using Faaast.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Faaast.DatabaseModel
{
    public class DatabaseStore : IDatabaseStore
    {
        private ConcurrentDictionary<string, IDatabase> Databases { get; set; } = new ConcurrentDictionary<string, IDatabase>(StringComparer.OrdinalIgnoreCase);

        public IDatabase this[string name]
        {
            get
            {
                Databases.TryGetValue(name, out var db);
                return db;
            }
            set
            {
                Databases.AddOrUpdate(name, value, (a, b) => value);
            }
        }
    }
}
