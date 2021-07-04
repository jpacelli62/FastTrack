using System;
using System.Data;
using System.Data.Common;

namespace Faaast.DatabaseModel
{
    public abstract class SqlEngine
    {
        public string FriendlyName { get; set; }

        public Func<DbConnection> Create { get; }
    }
}