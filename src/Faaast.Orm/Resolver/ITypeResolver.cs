using Faaast.DatabaseModel;
using System;
using System.Reflection;

namespace Faaast.Orm.Resolver
{
    public interface ITypeResolver
    {
        TableMapping GetMapping(Type type);
    }
}
