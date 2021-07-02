using Faaast.Metadata;
using System;
using System.Collections.Generic;

namespace Faaast.DatabaseModel
{
    public class DatabaseStore : IDatabaseStore
    {
        private Dictionary<string, IDatabase> Databases { get; set; } = new Dictionary<string, IDatabase>(StringComparer.OrdinalIgnoreCase);

        public IDatabase this[string name]
        {
            get
            {
                Databases.TryGetValue(name, out var db);
                return db;
            }
            set
            {
                Databases[name] = value;
            }
        }
    }
}
