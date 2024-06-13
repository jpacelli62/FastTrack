using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.ObjectPool;

namespace Faaast.SeoRouter
{
    internal sealed class SimpleTemplateBinderFactory
    {
#if NETSTANDARD2_0 || NET461 || NET7_0 || NET8_0

        private readonly ObjectPool<UriBuildingContext> _pool;

        public SimpleTemplateBinderFactory(ObjectPool<UriBuildingContext> pool) => _pool = pool;

        public TemplateBinder Create(RouteTemplate template, RouteValueDictionary defaults) => new(UrlEncoder.Default, _pool, template, defaults);

#elif NET5_0 || NET6_0 

        private readonly TemplateBinderFactory _factory;

        public SimpleTemplateBinderFactory(TemplateBinderFactory factory) => _factory = factory;

        public TemplateBinder Create(RouteTemplate template, RouteValueDictionary defaults) => _factory.Create(template, defaults);

#endif
    }
}
