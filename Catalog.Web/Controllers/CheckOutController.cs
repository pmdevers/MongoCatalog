using Catalog.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catalog.Web.Controllers
{
    public class CheckOutController : Controller
    {
        private CartService cartService;

        public CheckOutController()
        {
            cartService = new CartService();
        }

        // GET: CheckOut
        public ActionResult Basket()
        {
            var model = cartService.GetCart("basket");

            return View(model);
        }
    }
}