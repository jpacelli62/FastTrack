using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Mapping;
using Faaast.Orm.Reader;
using System;
using System.Collections.Concurrent;

namespace Faaast.Orm
{
    public static class Meta
    {
        public static readonly Metadata<IDatabase, DatabaseMapping> Mapping = new Metadata<IDatabase, DatabaseMapping>(nameof(Mapping));
     
        public static readonly Metadata<IDatabase, ConcurrentDictionary<Type, ObjectReader>> Readers = new Metadata<IDatabase, ConcurrentDictionary<Type, ObjectReader>>(nameof(Readers));
    }
}
