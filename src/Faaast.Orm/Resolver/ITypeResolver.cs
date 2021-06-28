using Faaast.DatabaseModel;
using System;
using System.Reflection;

namespace Faaast.Orm.Resolver
{
    public interface ITypeResolver
    {
        Type GetModel(Table table);

        MemberInfo GetMember(Table table, Column column);
    }
}
