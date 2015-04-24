using Catalog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catalog.Data
{
    public class ShippingMethod : IShippingMethod
    {
        public string Name { get { return "default"; } }

        public OrderItem GetOrderItem(Cart cart)
        {
            var orderItem = new OrderItem
            {
                Ean = "00000001",
                Description = "Shipping via Air baloon.",
                Price = 0,
                Quantity = 1
            };

            return orderItem;
        }


        public decimal GetPrice(Cart cart)
        {
            return 0;
        }
    }
}