using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Faaast.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Metadata
{
    class Program
    {
        public class Customer
        {
            public int Id { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        public static readonly Metadata<DtoClass, bool?> IsAwesome = new(nameof(IsAwesome));

        private static IObjectMapper ObjectMapper { get; set; }

        static void Main(string[] args)
        {
            // Register service in dependency injection
            var servicesCollection = new ServiceCollection();
            servicesCollection.AddMetadata();
            var services = servicesCollection.BuildServiceProvider();

            // Retrieve service from dependaency injection
            ObjectMapper = services.GetRequiredService<IObjectMapper>();

            // Load and caches the metadata for the type
            var typeMetadata = ObjectMapper.Get(typeof(Customer));

            // Now we can get/set properties/members and create new instances without any reflexion
            Customer objectInstance = (Customer)typeMetadata.Activator();

            typeMetadata[nameof(Customer.Id)].Write(objectInstance, 123);
            typeMetadata[nameof(Customer.FirstName)].Write(objectInstance, "John");
            typeMetadata[nameof(Customer.LastName)].Write(objectInstance, "Doe");
            foreach (var property in typeMetadata)
            {
                Console.WriteLine($"Property \"{property.Name}\": {property.Read(objectInstance)}");
            }

            // We can also read/write arbitrary metadata on class
            typeMetadata.Set(IsAwesome, true);
            if(typeMetadata.Has(IsAwesome))
            {
                Console.WriteLine($"Meta \"{IsAwesome.Name}\": {typeMetadata.Get(IsAwesome)}");
            }

            PerfComparison();
            Console.ReadLine();

            /* Produces:
                Property "Id": 123
                Property "FirstName": John
                Property "LastName": Doe
                Meta "IsAwesome": True
                Performance Comparison:
                Create 1000000 instances using Activator.CreateInstance(typeof(Customer)) : 34 ms
                Create 1000000 instances using IObjectMapper : 4 ms
                Create 1000000 instances using Reflexion : 66 ms
                Create 1000000 instances using Direct call : 0 ms
                Set 1000000 times a property using IObjectMapper : 3 ms
                Set 1000000 times a property using Reflexion : 101 ms
                Set 1000000 times a property using Direct call : 1 ms
            */
        }

        static void PerfComparison()
        {
            Console.WriteLine($"Performance Comparison:");

            CreateInstanceComparison();
            SetPropertyComparison();
        }

        static void CreateInstanceComparison()
        {
            Stopwatch watch = new Stopwatch();
            var iterations = 1000000;
            watch.Start();
            for (int i = 0; i < iterations; i++)
            {
                var instance = (Customer)Activator.CreateInstance(typeof(Customer));
            }
            watch.Stop();
            Console.WriteLine($"Create {iterations} instances using Activator.CreateInstance(typeof(Customer)) : {watch.ElapsedMilliseconds} ms");

            var typeMetadata = ObjectMapper.Get(typeof(Customer));
            watch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var instance = (Customer)typeMetadata.Activator();
            }
            watch.Stop();
            Console.WriteLine($"Create {iterations} instances using IObjectMapper : {watch.ElapsedMilliseconds} ms");

            var ctor = typeof(Customer).GetConstructor(Type.EmptyTypes);
            watch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var instance = (Customer)ctor.Invoke(null);
            }
            watch.Stop();
            Console.WriteLine($"Create {iterations} instances using Reflexion : {watch.ElapsedMilliseconds} ms");

            watch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var instance = new Customer();
            }
            watch.Stop();
            Console.WriteLine($"Create {iterations} instances using Direct call : {watch.ElapsedMilliseconds} ms");
        }

        static void SetPropertyComparison()
        {
            Stopwatch watch = new Stopwatch();
            var iterations = 1000000;
            Customer instance = new();
            var typeMetadata = ObjectMapper.Get(typeof(Customer));
            var property = typeMetadata[nameof(Customer.FirstName)];
            watch.Start();
            for (int i = 0; i < iterations; i++)
            {
                property.Write(instance, "test");
            }
            watch.Stop();
            Console.WriteLine($"Set {iterations} times a property using IObjectMapper : {watch.ElapsedMilliseconds} ms");

            var ctor = typeof(Customer).GetProperty(nameof(Customer.FirstName)).GetSetMethod();
            watch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                ctor.Invoke(instance, new object[] { "test" });
            }
            watch.Stop();
            Console.WriteLine($"Set {iterations} times a property using Reflexion : {watch.ElapsedMilliseconds} ms");

            watch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                instance.FirstName = "test";
            }
            watch.Stop();
            Console.WriteLine($"Set {iterations} times a property using Direct call : {watch.ElapsedMilliseconds} ms");
        }
    }
}
