using Catalog.Data;
using Catalog.Service;
using Catalog.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using Catalog.Web.Framework;

using Catalog.Web.Pdf;

namespace Catalog.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService cartService;

        public CartController()
        {
            cartService = new CartService();
        }

        // GET: Cart
        public ActionResult Index(string cart)
        {
            return View("Index-" + cart, cartService.GetCart(cart));
        }
        public ActionResult Summary(string cart)
        {
            return View("Summary-" + cart, cartService.GetCart(cart));
        }
        public ActionResult Mini(string cart)
        {
            return View("Mini-" + cart, cartService.GetCart(cart));
        }
        public ActionResult Small(string cart)
        {
            return View("Small-" + cart, cartService.GetCart(cart));
        }
        public ActionResult Delete(string ean, string returnUrl, string cart)
        {
            Cart.Current(cart).RemoveArticle(ean);
            return Redirect(returnUrl);
        }

        public ActionResult Print(string cart)
        {
            return new PdfActionResult("print-" + cart, Cart.Current(cart)){FileDownloadName = cart + ".pdf"};
        }

        public ActionResult Print1(string cart)
        {
            return new FlowDocumentResult(Cart.Current(cart), "print1-" + cart);
        }

        


        

       

        [HttpPost]
        public ActionResult Update(string ean, int quantity, string cart, string returnUrl)
        {
            Cart.Current(cart).UpdateArticle(ean, quantity);
            return Redirect(returnUrl);
        }
        [HttpPost]
        public ActionResult SetShippingMethod(string methodName, string cart, string returnUrl)
        {
            var myCart = Cart.Current(cart);
            var method = myCart.AvailableShippingMethods().FirstOrDefault(x => x.Name == methodName);

            myCart.SetShippingMethod(method);

            return Redirect(returnUrl);
        }
        [HttpPost]
        public ActionResult AddProduct(FormCollection collection)
        {
            var productUrl = collection["productUrl"];
            var cart = collection["cart"];
            var keys = collection["selectedKey"].Split(new[] { ',' });
            var values = collection["selectedValue"].Split(new[] { ',' });
            var returnUrl = collection["returnUrl"];

            cartService.AddProduct(productUrl, keys, values, cart);

            return Redirect(returnUrl);
        }
        [HttpPost]
        public ActionResult AddArticle(string ean, int quantity, string returnUrl, string cart)
        {
            cartService.AddArticle(ean, quantity, cart);
            return Redirect(returnUrl);
        }
    }
}