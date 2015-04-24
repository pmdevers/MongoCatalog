using Catalog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catalog.Service
{
    public class CatalogViewModel
    {
        public Catalogue Catalog { get; set; }
        public Category CurrentCategory { get; set; }
        public List<Product> Products { get; set; }
        public long TotalPages { get; set; }
        public long TotalItems { get; set; }

        public int CurrentPage { get; set; }

        public int ItemsPerPage { get; set; }
    }
}