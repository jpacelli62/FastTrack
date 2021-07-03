using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Faaast.Tests.TypeScan
{
    [Faaast.TypeDiscovery.Discovery]
    public class TypeDiscoveryTests
    {

        [Fact]
        public void DiscoverThis()
        {
            var discover = new Faaast.TypeDiscovery.TypeDiscovery();
            bool scanned = false;
            discover.When(
                (a, t) => t == typeof(TypeDiscoveryTests), 
                types => {
                    Assert.Single(types);
                    scanned = true;
                }, true);
            discover.ScanAsync().Wait();
            Assert.True(scanned);
        }
    }
}
