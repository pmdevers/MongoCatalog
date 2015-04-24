using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Data
{
    public interface IShippingMethod
    {
        OrderItem GetOrderItem(Cart cart);
        string Name { get; }
        decimal GetPrice(Cart cart);
    }
}
