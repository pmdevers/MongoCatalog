using Catalog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Catalog.Web.Framework
{
    public class ProductExistsConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if(values[parameterName] == null || !Product.Exists(x=>x.Url == values[parameterName].ToString()))
            {
                return false;
            }

            return true;
        }
    }
}