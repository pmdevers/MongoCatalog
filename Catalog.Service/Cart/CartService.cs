using Catalog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Service
{
    public class CartService
    {
        public CartViewModel GetCart(string cart, bool isEditable = true)
        {
            return new CartViewModel
            {
                Cart = Cart.Current(cart),
                IsEditable = isEditable
            };
        }

        

        public void AddProduct(string productUrl, string[] keys, string[] values, string cart)
        {
            var product = Product.Get(x => x.Url == productUrl);
            var article = product.Articles.Where(x => x.Options[keys[0]] == values[0] && x.Options[keys[1]] == values[1]).FirstOrDefault();

            Cart.Current(cart).AddArticle(article.Ean, 1);
        }

        public void AddArticle(string ean, int quantity, string cart)
        {
            Cart.Current(cart).AddArticle(ean, quantity);
        }
    }
}
