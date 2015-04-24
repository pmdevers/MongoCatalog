using Catalog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Service
{
    public class CartViewModel
    {
        public Cart Cart { get; set; }
        public bool IsEditable { get; set; }
    }
}
