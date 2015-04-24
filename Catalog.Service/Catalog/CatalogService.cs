using Catalog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Service
{
    public class CatalogService
    {
        public CatalogViewModel GetOverview(string catalog, string categories, int pageId)
        {
            var catalogue = Catalogue.Get(catalog);
            var category = catalogue.GetCategory(categories);
            var results = category != null ? category.GetProducts(pageId, 25) : catalogue.GetProducts(pageId, 25);
            
            
            
            var model = new CatalogViewModel
            {
                Catalog = catalogue,
                CurrentCategory = category,
                Products = results.Items,
                TotalItems = results.TotalItems,
                TotalPages = results.TotalPages,
                CurrentPage = results.CurrentPage,
                ItemsPerPage = results.ItemsPerPage
            };

            return model;
        }
    }
}
