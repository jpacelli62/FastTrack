using System;
using System.Data.Common;
using Faaast.Orm.Model;
using Faaast.Tests.Orm.FakeConnection;

namespace Faaast.Tests.Orm.Fake
{
    public class FakeEngine : SqlEngine
    {
        public override string FriendlyName => "FakeDb";

        public FakeDbConnection FakeConnection = new();

        public override Func<DbConnection> Create => ()=> FakeConnection;
    }
}
