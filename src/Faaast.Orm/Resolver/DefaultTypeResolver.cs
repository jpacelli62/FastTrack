//using Faaast.DatabaseModel;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Reflection;

//namespace Faaast.Orm.Resolver
//{
//    public class DefaultTypeResolver : ITypeResolver
//    {
//        public IEnumerable<Assembly> ModelAssemblies { get; set; }

//        private readonly ConcurrentDictionary<Table, Type> _cache;

//        public NamingConvention NamingConvention { get; set; }

//        public DefaultTypeResolver(NamingConvention convention)
//        {
//            _cache = new ConcurrentDictionary<Table, Type>();
//            this.NamingConvention = convention;
//        }

//        public MemberInfo GetMember(Table table, Column column)
//        {
//            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
//            Type model = GetModel(table);
//            string memberName = NamingConvention.Format(column.Name);

//            return (MemberInfo)model.GetProperty(memberName, flags) ?? model.GetField(memberName, flags);
//        }

        //public Type GetModel(Table table)
        //{
        //    if (!_cache.ContainsKey(table))
        //    {
        //        var candidateName = NamingConvention.Format(table.Name);
        //        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        //        {
        //            if (new string[] { "System", "Microsoft", "netstandard","Newtonsoft.Json","visualstudio", "xunit" }.Any(x => assembly.FullName.IndexOf(x, StringComparison.OrdinalIgnoreCase) > -1))
        //                continue;

        //            if (!assembly.IsDynamic)
        //            {
        //                Type[] exportedTypes = null;
        //                try
        //                {
        //                    exportedTypes = assembly.GetExportedTypes();
        //                }
        //                catch (ReflectionTypeLoadException e)
        //                {
        //                    exportedTypes = e.Types;
        //                }

        //                if (exportedTypes != null)
        //                {
        //                    foreach (var type in exportedTypes)
        //                    {
        //                        if (type.Name.Equals(candidateName, StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            _cache[table] = type;
        //                            return type;
        //                        }
        //                    }
        //                }
        //            }
        //            //string assemblyName = assembly.FullName;
        //            //Type target = assembly.GetType(candidateName, false, true);
        //            //if (target != null)
        //            //{
        //            //    _cache[table] = target;
        //            //    return target;
        //            //}
        //        }

//                _cache[table] = null;
//            }

//            return _cache[table];
//        }
//    }
//}
