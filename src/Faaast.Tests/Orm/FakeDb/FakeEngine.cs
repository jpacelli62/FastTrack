using System;
using System.Data.Common;
using Faaast.Orm.Model;

namespace Faaast.Tests.Orm.FakeDb
{
    public class FakeEngine : SqlEngine
    {
        public override string FriendlyName => "FakeDb";

        public FakeDbConnection FakeConnection { get; set; }

        public override Func<DbConnection> Create => ()=> this.FakeConnection;
    }
}
