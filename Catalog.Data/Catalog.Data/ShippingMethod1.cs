using Catalog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catalog.Data
{
    public class ShippingMethod1 : IShippingMethod
    {
        public string Name { get { return "default1"; } }

        public OrderItem GetOrderItem(Cart cart)
        {
            var orderItem = new OrderItem
            {
                Ean = "00000002",
                Description = "Shipping via Car.",
                Price = 0,
                Quantity = 1
            };

            return orderItem;
        }


        public decimal GetPrice(Cart cart)
        {
            return 20;
        }
    }
}