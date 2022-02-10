using System;
using Faaast.Orm;
using Faaast.Orm.Model;
using Faaast.Tests.Orm.Fixture;
using Moq;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class FaaastDbTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public FaaastDbTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        [Fact]
        public void InitMapping_NullConnection()
        {
            var mock = new Mock<FaaastDb>(this.Fixture.Services)
            {
                CallBase = true
            };
            mock.SetupGet(x => x.Connection).Returns((ConnectionSettings)null);
            Assert.Throws<ArgumentException>(() => mock.Object.Mappings.Value);
        }

        [Fact]
        public void Mapping()
        {
            var map = this.Fixture.Db.Mapping<SimpleModel>();
            Assert.NotNull(map);
            Assert.Equal(typeof(SimpleModel), map.ObjectClass.Type);
        }

        [Fact]
        public void Mapping_Unknown()
        {
            var map = this.Fixture.Db.Mapping<FaaastDbTests>();
            Assert.Null(map);
        }
    }
}
