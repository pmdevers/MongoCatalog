using Catalog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Service
{
    public class ProductService
    {
        public ProductViewModel GetProduct(string  productUrl)
        {
            var model = new ProductViewModel
            {

                Product = Product.Get(x => x.Url == productUrl)
            };

            return model;
        }
    }
}
