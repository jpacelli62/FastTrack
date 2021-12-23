using System;
using System.Data.Common;

namespace Faaast.Orm.Model
{
    public abstract class SqlEngine
    {
        public abstract string FriendlyName { get; }

        public abstract Func<DbConnection> Create { get; }
    }
}