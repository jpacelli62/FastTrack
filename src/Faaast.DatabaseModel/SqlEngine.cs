using System;
using System.Data.Common;

namespace Faaast.DatabaseModel
{
    public abstract class SqlEngine
    {
        public abstract string FriendlyName { get; }

        public abstract Func<DbConnection> Create { get; }
    }
}