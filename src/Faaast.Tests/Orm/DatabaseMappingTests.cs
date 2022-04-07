using Faaast.Tests.Orm.Fixture;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class DatabaseMappingTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public DatabaseMappingTests(FaaastOrmFixture fixture) => this.Fixture = fixture;

        [Fact]
        public void Mappings()
        {
            var mapping = this.Fixture.Db.Mappings.Value;
            Assert.NotNull(mapping);
            Assert.Single(mapping.Mappings);
        }

        [Fact]
        public void Source()
        {
            var mapping = this.Fixture.Db.Mappings.Value;
            Assert.NotNull(mapping.Source);
            Assert.Equal(this.Fixture.Db.Database, mapping.Source);
            Assert.Equal(this.Fixture.Db.Connection, mapping.Source.Connexion);
        }
    }
}
