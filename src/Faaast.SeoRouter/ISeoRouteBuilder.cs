namespace Faaast.SeoRouter
{
    public interface ISeoRouteBuilder
    {
        ISeoRouteBuilder MapRoute(string name, string template, object defaults = null);

        RoutingRules Build();
    }
}
