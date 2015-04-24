using Catalog.Data;
using Catalog.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catalog.Web.Controllers
{
    public class ProductController : Controller
    {

        private ProductService productService;

        public ProductController()
        {
            productService = new ProductService();
        }

        // GET: Product
        public ActionResult Index(string productUrl)
        {
            var model = productService.GetProduct(productUrl);

            return View(model);
        }
    }
}