using System;
using System.Collections.Concurrent;
using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Mapping;
using Faaast.Orm.Reader;

namespace Faaast.Orm
{
    public static class Meta
    {
        public static readonly Metadata<IDatabase, DatabaseMapping> Mapping = new(nameof(Mapping));

        public static readonly Metadata<IDatabase, ConcurrentDictionary<Type, ObjectReader>> Readers = new(nameof(Readers));
    }
}
