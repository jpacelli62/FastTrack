using Faaast.Orm.Converters;
using Faaast.Tests.Orm.Fixture;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class EnumValueConverterTests
    {
        [Fact]
        public void FromDb()
        {
            var converter = new EnumToIntValueConverter<TestState>();
            Assert.Equal(TestState.ItWorks, converter.FromDb(1, typeof(TestState)));
        }

        [Fact]
        public void ToDb()
        {
            var converter = new EnumToIntValueConverter<TestState>();
            Assert.Equal(1, converter.ToDb(TestState.ItWorks, typeof(TestState)));
        }
    }
}
