using System.Web.Routing;

namespace Catalog.Web.Framework
{
    /// <summary>
    /// Represents a generated route URL with route data.
    /// </summary>
    public class RouteUrl
    {
        /// <summary>
        /// Gets or sets the route URL.
        /// </summary>
        /// <value>Route URL.</value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets route values.
        /// </summary>
        /// <value>Route values.</value>
        public RouteValueDictionary Values { get; set; }
    }
}
