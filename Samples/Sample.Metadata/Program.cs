using System;
using Faaast.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Metadata
{
    public class Program
    {
        public static readonly Metadata<IDtoClass, bool?> IsAwesome = new(nameof(IsAwesome));

        private static IObjectMapper ObjectMapper { get; set; }

        static void Main()
        {
            // Register service in dependency injection
            var servicesCollection = new ServiceCollection();
            servicesCollection.AddMetadata();
            var services = servicesCollection.BuildServiceProvider();

            // Retrieve service from dependency injection
            ObjectMapper = services.GetRequiredService<IObjectMapper>();

            // Load and caches the metadata for the type
            var typeMetadata = ObjectMapper.Get(typeof(Customer));

            // Now we can get/set properties/members and create new instances without any reflexion
            var objectInstance = (Customer)typeMetadata.CreateInstance();

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

            Console.ReadLine();

            /* Produces:
                Property "Id": 123
                Property "FirstName": John
                Property "LastName": Doe
                Meta "IsAwesome": True
            */
        }
    }
}
