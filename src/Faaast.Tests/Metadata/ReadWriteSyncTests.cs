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
        public void Test_ReadAccess_MultipleRead()
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
        public void Test_WriteAccess()
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
        public void Test_WriteAccess_Timeout()
        {
            ReadWriteSync sync = new();
            using var read = sync.ReadAccess(10000);
            Assert.ThrowsAny<ApplicationException>(() => sync.WriteAccess(1000));
        }

        [Fact]
        public void Test_UpgradeToWriteAccess()
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
