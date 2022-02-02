using Faaast.Orm;
using Xunit;
using Faaast.Orm.Reader;
using System.Data;
using System;

namespace Faaast.Tests.Orm
{
    public class ExtensionsTests 
    {
        [Fact]
        public void Check_ToDbType_conversion()
        {
            Assert.Equal(DbType.Int32, Extensions.ToDbType(typeof(int)));
            Assert.Equal(DbType.Int32, Extensions.ToDbType(typeof(Nullable<Int32>)));
            Assert.Throws<ArgumentException>(() => Extensions.ToDbType(typeof(ExtensionsTests)));
        }

        [Fact]
        public void Check_Sanitize()
        {
            Assert.Null(Extensions.Sanitize(null));
            Assert.Equal(string.Empty, Extensions.Sanitize(string.Empty));
            Assert.Equal("test", Extensions.Sanitize("test"));
            Assert.Equal("test", Extensions.Sanitize("@test"));
            Assert.Equal("test", Extensions.Sanitize(":test"));
            Assert.Equal("test", Extensions.Sanitize("?test"));
        }
    }
}
