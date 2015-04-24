using Catalog.Data;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Catalog.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateCatalog();            
            
            var catalog = Catalogue.Get("Producten");
            var category = catalog.GetCategory("/dames");
            //var products = category.GetProducts(0, 25);
            //var product = products.Items.First();


            category.ChangeName("Dame");

            

            //category.Add("test");

            var test = category.TotalProducts();

            Cart cart;
            try
            {
                cart = Cart.Get("550c3fda7d71931ad4b423b3");
            } catch (Exception)
            {
                cart = Cart.Create("basket");
            }

            if(string.IsNullOrEmpty(cart.ShippingMethod))
                cart.SetShippingMethod(new ShippingMethod());


            var order = cart.CreateOrder();

            order.Save();

            //cart.AddArticle(product.Articles.First().Ean, 1);

            //product.Articles.ForEach(x => { x.Visible = false; });




            //Query.Matches("CategoryPaths", new BsonRegularExpression(new Regex("^" + Path, RegexOptions.IgnoreCase)));

            /*
                var test = Query.Matches("Attributes.Description", new BsonRegularExpression(new Regex("Test101", RegexOptions.IgnoreCase)));

                var product1 = Product.GetProducts(test, 0, 25);
                var product3 = Product.GetProducts(test, 2, 25);
             
             */
        }

        private static void CreateCatalog()
        {
            var catalog = Catalogue.Get("Producten");

            var cata = catalog.Add("Heren");
            
            var catb = cata.Add("Schoenen");
            
            var cat1 = catb.Add("Slippers");
            var cat2 = catb.Add("Sport");
            PopulateDatabase(cat1, 1000);
            PopulateDatabase(cat2, 2000);

            var cate = cata.Add("Kleren");

            var cat5 = cate.Add("Polo");
            var cat6 = cate.Add("Blouse");
            PopulateDatabase(cat5, 3000);
            PopulateDatabase(cat6, 4000);
            
            
            var catc = catalog.Add("Dames");
            
            var catd = catc.Add("Schoenen");
            
            var cat3 = catd.Add("Hakken");
            var cat4 = catd.Add("Slippers");
            PopulateDatabase(cat3, 5000);
            PopulateDatabase(cat4, 6000);

            catalog.Save();
        }

        private static void PopulateDatabase(Category category, int counter)
        {
            for (int i = counter; i < counter + 1000; i++)
            {
                Product test = Product.Create(string.Format("tst00{0}", i));

                test.Name = string.Format("Test{0} Product", i);
                test.ItemCode = string.Format("tst00{0}", i);
                test.Url = string.Format("tst00{0}", i);
                
                category.AddProduct(test);

                test.Attributes.Add("Description", string.Format("A Test{0} Product", i));
                test.Attributes.Add("Image1", "Image1");

                int ai = 1;

                foreach (var value1 in new List<string> { "xl", "l", "m", "s", "xs" })
                {
                    foreach (var value2 in new List<string> { "red", "green", "yellow", "blue", "pink" })
                    {
                        var options = new Dictionary<string, string>();

                        options.Add("size", value1);
                        options.Add("color", value2);

                        test.AddArticle(i + "-" + ai, 0, options);

                        ai++;
                    }
                }

                test.Save();
            }
        }
    }
}
