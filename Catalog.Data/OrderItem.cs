using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Catalog.Data
{
    public class OrderItem
    {
        public string Ean { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal VatPercentage { get; set; }
        public decimal TotalPriceWithoutVat {get { return (Quantity * Price); } }
        public decimal Vat { get { return (TotalPriceWithoutVat / 100) * VatPercentage; } }
        public decimal TotalPrice { get { return TotalPriceWithoutVat + Vat; } }

    }
}
