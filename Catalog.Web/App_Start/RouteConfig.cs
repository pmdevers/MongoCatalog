using Catalog.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Catalog.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var constraints = new RouteValueDictionary();
            constraints.Add("catalog", new CatalogueExistsConstraint());
            //constraints.Add("productUrl", new ProductExistsConstraint());
            
            routes.Add(new GreedyRoute("{catalog}/{*categories}/page/{pageid}", new RouteValueDictionary(new { controller = "Home", action = "Index" }), constraints, new MvcRouteHandler()));
            routes.Add(new GreedyRoute("{catalog}/page/{pageid}", new RouteValueDictionary(new { controller = "Home", action = "Index", categories = "", pageId = 0 }), constraints, new MvcRouteHandler()));
            routes.Add(new GreedyRoute("{catalog}/{*categories}", new RouteValueDictionary(new { controller = "Home", action = "Index", pageId = 0 }), constraints, new MvcRouteHandler()));
            routes.Add(new GreedyRoute("{catalog}", new RouteValueDictionary(new { controller = "Home", action = "Index", categories = "", pageId = 0 }), constraints, new MvcRouteHandler()));

            var productConstraints = new RouteValueDictionary();
            productConstraints.Add("productUrl", new ProductExistsConstraint());

            routes.MapRoute(
                name: "productRoute",
                url: "product/{productUrl}",
                defaults: new { controller = "Product", action = "Index" },
                constraints: productConstraints);

            routes.MapRoute(
                name: "default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index2", id = UrlParameter.Optional });
            
        }
    }
}
