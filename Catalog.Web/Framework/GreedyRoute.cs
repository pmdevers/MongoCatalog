using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

namespace Catalog.Web.Framework
{
    /// <summary>
    ///     This route is used for cases where we want greedy route segments anywhere in the route URL definition
    /// </summary>
    public class GreedyRoute : Route
    {
        #region Properties

        private readonly bool hasGreedySegment;
        private readonly LinkedList<GreedyRouteSegment> urlSegments = new LinkedList<GreedyRouteSegment>();

        /// <summary>Gets the URL pattern for the route.</summary>
        public new string Url { get; private set; }

        /// <summary>Gets minimum number of segments that this route requires.</summary>
        public int MinRequiredSegments { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GreedyRoute" /> class, using the specified URL pattern and handler
        ///     class.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GreedyRoute(string url, IRouteHandler routeHandler)
            : this(url, null, null, null, routeHandler)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GreedyRoute" /> class, using the specified URL pattern, handler class,
        ///     and default parameter values.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GreedyRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : this(url, defaults, null, null, routeHandler)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GreedyRoute" /> class, using the specified URL pattern, handler class,
        ///     default parameter values, and constraints.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="constraints">A regular expression that specifies valid values for a URL parameter.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GreedyRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
            : this(url, defaults, constraints, null, routeHandler)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GreedyRoute" /> class, using the specified URL pattern, handler class,
        ///     default parameter values, constraints, and custom values.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="constraints">A regular expression that specifies valid values for a URL parameter.</param>
        /// <param name="dataTokens">
        ///     Custom values that are passed to the route handler, but which are not used to determine
        ///     whether the route matches a specific URL pattern. The route handler might need these values to process the request.
        /// </param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GreedyRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints,
            RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url.Replace("*", ""), defaults, constraints, dataTokens, routeHandler)
        {
            Defaults = defaults ?? new RouteValueDictionary();
            Constraints = constraints;
            DataTokens = dataTokens;
            RouteHandler = routeHandler;
            Url = url;
            MinRequiredSegments = 0;

            // URL must be defined
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Route URL must be defined.", "url");
            }

            // correct URL definition can have AT MOST ONE greedy segment
            if (url.Split('*').Length > 2)
            {
                throw new ArgumentException("Route URL can have at most one greedy segment, but not more.", "url");
            }

            var rx = new Regex(@"^(?<isToken>{)?(?(isToken)(?<isGreedy>\*?))(?<name>[a-zA-Z0-9-_]+)(?(isToken)})$",
                RegexOptions.Compiled | RegexOptions.Singleline);
            foreach (string segment in url.Split('/'))
            {
                // segment must not be empty
                if (string.IsNullOrEmpty(segment))
                {
                    throw new ArgumentException("Route URL is invalid. Sequence \"//\" is not allowed.", "url");
                }

                if (rx.IsMatch(segment))
                {
                    Match m = rx.Match(segment);
                    var s = new GreedyRouteSegment
                    {
                        IsToken = m.Groups["isToken"].Value.Length.Equals(1),
                        IsGreedy = m.Groups["isGreedy"].Value.Length.Equals(1),
                        Name = m.Groups["name"].Value
                    };
                    urlSegments.AddLast(s);
                    hasGreedySegment |= s.IsGreedy;

                    continue;
                }
                throw new ArgumentException("Route URL is invalid.", "url");
            }

            // get minimum required segments for this route
            LinkedListNode<GreedyRouteSegment> seg = urlSegments.Last;
            int sIndex = urlSegments.Count;
            while (seg != null && MinRequiredSegments.Equals(0))
            {
                if (!seg.Value.IsToken || !Defaults.ContainsKey(seg.Value.Name))
                {
                    MinRequiredSegments = Math.Max(MinRequiredSegments, sIndex);
                }
                sIndex--;
                seg = seg.Previous;
            }

            // check that segments after greedy segment don't define a default
            if (hasGreedySegment)
            {
                LinkedListNode<GreedyRouteSegment> s = urlSegments.Last;
                while (s != null && !s.Value.IsGreedy)
                {
                    if (s.Value.IsToken && Defaults.ContainsKey(s.Value.Name))
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Defaults for route segment \"{0}\" is not allowed, because it's specified after greedy catch-all segment.",
                                s.Value.Name), "defaults");
                    }
                    s = s.Previous;
                }
            }
        }

        #endregion

        #region GetRouteData

        /// <summary>
        ///     Returns information about the requested route.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <returns>
        ///     An object that contains the values from the route definition.
        /// </returns>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            string virtualPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) +
                                 (httpContext.Request.PathInfo ?? string.Empty);

            RouteValueDictionary values = ParseRoute(virtualPath);
            if (values == null)
            {
                return null;
            }

            var result = new RouteData(this, RouteHandler);
            if (!ProcessConstraints(httpContext, values, RouteDirection.IncomingRequest))
            {
                return null;
            }

            // everything's fine, fill route data
            foreach (var value in values)
            {
                result.Values.Add(value.Key, value.Value);
            }
            if (DataTokens != null)
            {
                foreach (var token in DataTokens)
                {
                    result.DataTokens.Add(token.Key, token.Value);
                }
            }
            return result;
        }

        #endregion

        #region GetVirtualPath

        /// <summary>
        ///     Returns information about the URL that is associated with the route.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the requested route.</param>
        /// <param name="values">An object that contains the parameters for a route.</param>
        /// <returns>
        ///     An object that contains information about the URL that is associated with the route.
        /// </returns>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            RouteUrl url = Bind(requestContext.RouteData.Values, values);
            if (url == null)
            {
                return null;
            }
            if (!ProcessConstraints(requestContext.HttpContext, url.Values, RouteDirection.UrlGeneration))
            {
                return null;
            }

            var data = new VirtualPathData(this, url.Url);
            if (DataTokens != null)
            {
                foreach (var pair in DataTokens)
                {
                    data.DataTokens[pair.Key] = pair.Value;
                }
            }
            return data;
        }

        #endregion

        #region Private methods

        #region ProcessConstraints

        /// <summary>
        ///     Processes constraints.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="values">Route values.</param>
        /// <param name="direction">Route direction.</param>
        /// <returns><c>true</c> if constraints are satisfied; otherwise, <c>false</c>.</returns>
        private bool ProcessConstraints(HttpContextBase httpContext, RouteValueDictionary values, RouteDirection direction)
        {
            if (Constraints != null)
            {
                foreach (var constraint in Constraints)
                {
                    if (!ProcessConstraint(httpContext, constraint.Value, constraint.Key, values, direction))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region ParseRoute

        /// <summary>
        ///     Parses the route into segment data as defined by this route.
        /// </summary>
        /// <param name="virtualPath">Virtual path.</param>
        /// <returns>Returns <see cref="System.Web.Routing.RouteValueDictionary" /> dictionary of route values.</returns>
        private RouteValueDictionary ParseRoute(string virtualPath)
        {
            var parts = new Stack<string>(
                virtualPath
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Reverse() // we have to reverse it because parsing starts at the beginning not the end.
                );

            // number of request route parts must match route URL definition
            if (parts.Count < MinRequiredSegments)
            {
                return null;
            }

            var result = new RouteValueDictionary();

            // start parsing from the beginning
            bool finished = false;
            LinkedListNode<GreedyRouteSegment> currentSegment = urlSegments.First;
            while (!finished && !currentSegment.Value.IsGreedy)
            {
                object p = parts.Count > 0 ? parts.Pop() : null;
                if (currentSegment.Value.IsToken)
                {
                    p = p ?? Defaults[currentSegment.Value.Name];
                    result.Add(currentSegment.Value.Name, p);
                }
                else
                {
                    if (!currentSegment.Value.Name.Equals(p))
                    {
                        return null;
                    }
                }
                currentSegment = currentSegment.Next;
                finished = currentSegment == null;
            }

            // continue from the end if needed
            parts = new Stack<string>(parts); // this will reverse stack elements
            currentSegment = urlSegments.Last;
            while (!finished && !currentSegment.Value.IsGreedy)
            {
                object p = parts.Count > 0 ? parts.Pop() : null;
                if (currentSegment.Value.IsToken)
                {
                    p = p ?? Defaults[currentSegment.Value.Name];
                    result.Add(currentSegment.Value.Name, p);
                }
                else
                {
                    if (!currentSegment.Value.Name.Equals(p))
                    {
                        return null;
                    }
                }
                currentSegment = currentSegment.Previous;
                finished = currentSegment == null;
            }

            // fill in the greedy catch-all segment
            if (!finished)
            {
                var remaining = string.Join("/", parts.Reverse().ToArray()) ?? Defaults[currentSegment.Value.Name];
                result.Add(currentSegment.Value.Name, remaining);
            }

            // add remaining default values
            foreach (var def in Defaults)
            {
                if (!result.ContainsKey(def.Key))
                {
                    result.Add(def.Key, def.Value);
                }
            }

            return result;
        }

        #endregion

        #region Bind

        /// <summary>
        ///     Binds the specified current values and values into a URL.
        /// </summary>
        /// <param name="currentValues">Current route data values.</param>
        /// <param name="values">Additional route values that can be used to generate the URL.</param>
        /// <returns>Returns a URL route string.</returns>
        private RouteUrl Bind(RouteValueDictionary currentValues, RouteValueDictionary values)
        {
            currentValues = currentValues ?? new RouteValueDictionary();
            values = values ?? new RouteValueDictionary();

            var required = new HashSet<string>(urlSegments.Where(seg => seg.IsToken).ToList().ConvertAll(seg => seg.Name),
                StringComparer.OrdinalIgnoreCase);
            var routeValues = new RouteValueDictionary();

            foreach (string token in new List<string>(required))
            {
                object dataValue = values[token] ?? currentValues[token] ?? Defaults[token];
                if (IsUsable(dataValue))
                {
                    var val = dataValue as string;
                    if (val != null)
                    {
                        val = val.StartsWith("/") ? val.Substring(1) : val;
                        val = val.EndsWith("/") ? val.Substring(0, val.Length - 1) : val;
                    }
                    routeValues.Add(token, val ?? dataValue);
                    required.Remove(token);
                }
            }

            // this route data is not related to this route
            if (required.Count > 0)
            {
                return null;
            }

            // add all remaining values
            foreach (var pair1 in values)
            {
                if (IsUsable(pair1.Value) && !routeValues.ContainsKey(pair1.Key))
                {
                    routeValues.Add(pair1.Key, pair1.Value);
                }
            }

            // add remaining defaults
            foreach (var pair2 in Defaults)
            {
                if (IsUsable(pair2.Value) && !routeValues.ContainsKey(pair2.Key))
                {
                    routeValues.Add(pair2.Key, pair2.Value);
                }
            }

            // check that non-segment defaults are the same as those provided
            var nonRouteDefaults = new RouteValueDictionary(Defaults);
            foreach (GreedyRouteSegment seg in urlSegments.Where(ss => ss.IsToken))
            {
                nonRouteDefaults.Remove(seg.Name);
            }
            foreach (var pair3 in nonRouteDefaults)
            {
                if (!routeValues.ContainsKey(pair3.Key) || !RoutePartsEqual(pair3.Value, routeValues[pair3.Key]))
                {
                    // route data is not related to this route
                    return null;
                }
            }

            var sb = new StringBuilder();
            var valuesToUse = new RouteValueDictionary(routeValues);
            bool mustAdd = hasGreedySegment;

            // build URL string
            LinkedListNode<GreedyRouteSegment> s = urlSegments.Last;
            while (s != null)
            {
                object segmentValue = null;
                if (s.Value.IsToken)
                {
                    segmentValue = valuesToUse[s.Value.Name];
                    mustAdd = mustAdd || !RoutePartsEqual(segmentValue, Defaults[s.Value.Name]);
                    valuesToUse.Remove(s.Value.Name);
                }
                else
                {
                    segmentValue = s.Value.Name;
                    mustAdd = true;
                }

                if (mustAdd)
                {
                    sb.Insert(0, sb.Length > 0 ? "/" : string.Empty);
                    sb.Insert(0, Uri.EscapeUriString(Convert.ToString(segmentValue, CultureInfo.InvariantCulture)));
                }

                s = s.Previous;
            }

            // add remaining values
            if (valuesToUse.Count > 0)
            {
                bool first = true;
                foreach (var pair3 in valuesToUse)
                {
                    // only add when different from defaults
                    if (!RoutePartsEqual(pair3.Value, Defaults[pair3.Key]))
                    {
                        sb.Append(first ? "?" : "&");
                        sb.Append(Uri.EscapeDataString(pair3.Key));
                        sb.Append("=");
                        sb.Append(Uri.EscapeDataString(Convert.ToString(pair3.Value, CultureInfo.InvariantCulture)));
                        first = false;
                    }
                }
            }

            return new RouteUrl
            {
                Url = sb.ToString(),
                Values = routeValues
            };
        }

        #endregion

        #region IsUsable

        /// <summary>
        ///     Determines whether an object actually is instantiated or has a value.
        /// </summary>
        /// <param name="value">Object value to check.</param>
        /// <returns>
        ///     <c>true</c> if an object is instantiated or has a value; otherwise, <c>false</c>.
        /// </returns>
        private bool IsUsable(object value)
        {
            var val = value as string;
            if (val != null)
            {
                return val.Length > 0;
            }
            return value != null;
        }

        #endregion

        #region RoutePartsEqual

        /// <summary>
        ///     Checks if two route parts are equal
        /// </summary>
        /// <param name="firstValue">The first value.</param>
        /// <param name="secondValue">The second value.</param>
        /// <returns><c>true</c> if both values are equal; otherwise, <c>false</c>.</returns>
        private bool RoutePartsEqual(object firstValue, object secondValue)
        {
            var sFirst = firstValue as string;
            var sSecond = secondValue as string;
            if ((sFirst != null) && (sSecond != null))
            {
                return string.Equals(sFirst, sSecond, StringComparison.OrdinalIgnoreCase);
            }
            if ((firstValue != null) && (secondValue != null))
            {
                return firstValue.Equals(secondValue);
            }
            return (firstValue == secondValue);
        }

        #endregion

        #endregion
    }
}