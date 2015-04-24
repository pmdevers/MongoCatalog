using Catalog.Data;
using Catalog.Web.Framework;
using Catalog.Service;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catalog.Web.Controllers
{
    public class HomeController : Controller
    {
        public CatalogService catalogService;
        public CartService cartService;

        public HomeController()
        {
            catalogService = new CatalogService();
            cartService = new CartService();
        }

        // GET: Home
        public ActionResult Index()
        {
            var catalogName = RouteData.Values["catalog"].ToString();
            var categories = RouteData.Values["categories"].ToString();
            var pageId = int.Parse(RouteData.Values["pageId"].ToString());

            var model = catalogService.GetOverview(catalogName, categories, pageId);

            return View(model);
        }

        
    }
}