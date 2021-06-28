using Faaast.Metadata;
using System;
using System.Collections.Generic;

namespace Faaast.DatabaseModel
{
    public class DatabaseStore : IDatabaseStore
    {
        private readonly ReadWriteSync _sync;
        private Dictionary<string, IDatabase> Databases { get; set; }

        public IDatabase this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException(nameof(name));

                using (_sync.ReadAccess(10000))
                {
                    Databases.TryGetValue(name, out var db);
                    return db;
                }
            }
            set
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException(nameof(name));

                using (_sync.WriteAccess(10000))
                {
                    Databases[name] = value;
                }
            }
        }

        public DatabaseStore()
        {
            Databases = new Dictionary<string, IDatabase>(StringComparer.OrdinalIgnoreCase);
            _sync = new ReadWriteSync();
        }
    }
}
