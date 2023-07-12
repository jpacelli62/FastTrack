using Faaast.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faaast.Tests.Metadata
{
    public class ApplicationBuilderExtensionsTest
    {
        [Fact]
        public void AddMetadata()
        {
            var services = new ServiceCollection();
            services.AddMetadata();
            Assert.NotNull(services.BuildServiceProvider().GetService<ObjectMapper>());
        }
    }
}
