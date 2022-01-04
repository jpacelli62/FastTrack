using System;
using System.Collections;
using Faaast.Metadata;
using Faaast.Tests.Orm.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faaast.Tests.Metadata
{
    public class ReadWriteSyncTests
    {
        [Fact]
        public void Multiple_read()
        {
            ReadWriteSync sync = new();
            var success = false;
            using(var read1 = sync.ReadAccess())
            {
                using var read2 = sync.ReadAccess();
                success = true;
            }

            Assert.True(success);
        }
        [Fact]
        public void Single_write()
        {
            ReadWriteSync sync = new(1000);
            var success = false;
            using (var read1 = sync.WriteAccess())
            {
                success = true;
            }

            Assert.True(success);
        }

        [Fact]
        public void Write_wait_read()
        {
            ReadWriteSync sync = new();
            using var read = sync.ReadAccess(10000);
            Assert.ThrowsAny<ApplicationException>(() => sync.WriteAccess(1000));
        }

        [Fact]
        public void Upgrade()
        {
            ReadWriteSync sync = new();
            var success = false;
            using (var read = sync.ReadAccess())
            {
                using var write = sync.UpgradeToWriteAccess();
                success = true;
            }

            Assert.True(success);
        }
    }
}
