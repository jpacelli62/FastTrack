using Dapper.FluentMap.Dommel.Mapping;

namespace Faaast.Tests.Orm.Fixtures
{
    public class SimpleModelDommelMap : DommelEntityMap<SimpleModel>
    {
        public SimpleModelDommelMap()
        {
            ToTable("SimpleModel");
            Map(x => x.V1).ToColumn("v1").IsKey().IsIdentity();
            Map(x => x.V2).ToColumn("V2");
            Map(x => x.V3).ToColumn("V3");
            Map(x => x.V4).ToColumn("V4");
            Map(x => x.V5).ToColumn("V5");
            Map(x => x.V6).ToColumn("V6");
            Map(x => x.V7).ToColumn("V7");
            Map(x => x.V8).ToColumn("V8");
        }
    }
}
