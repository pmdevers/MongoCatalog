using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Data
{
    public class CartItem
    {
        public string Ean { get; set; }

        public int Quantity { get; set; }

        public decimal Price
        {
            get
            {
                return 20;
            }
        }

        public decimal TotalPrice
        {
            get
            {
                return Quantity * 20;
            }
        }

        public decimal VatPercentage
        {
            get { return Tax.GetVatPercentage(Ean); }
        }

        [BsonIgnore]
        public Product Product
        {
            get
            {
                return Product.Get(x => x.Articles.Any(a => a.Ean == Ean));
            }
        }

        [BsonIgnore]
        public Article Article
        {
            get
            {
                return Product.Articles.FirstOrDefault(x => x.Ean == Ean);
            }
        }
    }
}
