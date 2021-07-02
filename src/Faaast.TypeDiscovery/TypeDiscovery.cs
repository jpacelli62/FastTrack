using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Faaast.TypeDiscovery
{
    public class TypeDiscovery
    {

        private class Discover
        {
            public Func<Assembly, Type, bool> Predicate { get; set; }
            public Action<IEnumerable<Type>> Todo { get; set; }
            public bool OneByAssembly { get; set; }
            public List<Type> Result { get; set; } = new List<Type>();
        }

        public static readonly string DefaultUsage = "Default";

        public bool OnlyTagged { get; set; } = true;

        private List<Discover> Predicates { get; set; } = new List<Discover>();

        public Func<Assembly, bool> AssemblyMatch { get; set; }


        public void When(Func<Assembly, Type, bool> predicate, Action<IEnumerable<Type>> action, bool oneByAssembly)
        {
            Predicates.Add(new Discover
            {
                Predicate = predicate,
                Todo = action,
                OneByAssembly = oneByAssembly
            });
        }

        public Task ScanAsync()
        {
            List<Task> scanTasks = new List<Task>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                DiscoveryAttribute attribute = assembly.GetCustomAttribute(typeof(DiscoveryAttribute)) as DiscoveryAttribute;
                if ((AssemblyMatch?.Invoke(assembly) ?? true) && (!OnlyTagged || attribute != null))
                {
                    scanTasks.Add(Task.Run(() => ScanAssemblyAsync(assembly)));
                }
            }

            return Task.WhenAll(scanTasks.ToArray()).ContinueWith(x =>
            {
                foreach (var action in Predicates)
                {
                    if (action.Result.Any())
                    {
                        action.Todo(action.Result);
                    }
                }
            });
        }

        private Task ScanAssemblyAsync(Assembly assembly)
        {
            foreach (var action in Predicates)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (action.Predicate(assembly, type))
                    {
                        action.Result.Add(type);
                        if (action.OneByAssembly)
                            break;
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
