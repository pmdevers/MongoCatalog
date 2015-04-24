using Catalog.Framework;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Data
{
    public class Cart
    {
        #region Public Properties

        public string CartNumber { get { return id.ToString(); } }

        public Address DeliveryAddres { get; set; }

        public Address InvoiceAddress { get; set; }

        public CartItem[] Items { get { return this.items.ToArray(); } private set { this.items = new List<CartItem>(value); } }

        public string Name { get; set; }

        public string ShippingMethod { get; set; }

        public decimal TotalPrice
        {
            get
            {
                var totalOrderItemPrice = items.Sum(x=>x.TotalPrice);
                var shippingCosts = shippingMethod != null ? shippingMethod.GetPrice(this) : 0;
                return totalOrderItemPrice + shippingCosts;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static Cart Create(string name)
        {
            var cart = new Cart { Name = name, items = new List<CartItem>() };
            cart.Save();
            return cart;
        }

        public static Cart Current(string name = "basket")
        {
            var cartNumber = Cookie.Get(name);
            Cart cart;
            if (string.IsNullOrEmpty(cartNumber))
            {
                cart = Cart.Create(name);
                Cookie.Set(name, cart.CartNumber);
            }
            else
            {
                cart = Cart.Get(cartNumber);
            }

            return cart;
        }

        public static Cart Get(string cartNumber)
        {
            var id = ObjectId.Parse(cartNumber);
            var mongo = new Mongo<Cart>();
            var cart = mongo.Get(x => x.id == id);
            if (cart == null)
                throw new Exception(string.Format("Cart with id: {0} is not found!", cartNumber));

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(x => x.GetTypes());
            var methods = types.Where(x => typeof(IShippingMethod).IsAssignableFrom(x) && !x.IsInterface);
            var methodsObjects = methods.Select(x => (IShippingMethod)Activator.CreateInstance(x));
            var method = methodsObjects.FirstOrDefault(x => x.Name == cart.ShippingMethod);

            if (method != null)
                cart.SetShippingMethod(method);

            return cart;
        }

        public void AddArticle(string ean, int quantity)
        {
            var item = Items.FirstOrDefault(x => x.Ean == ean);
            if (item == null)
                items.Add(new CartItem { Ean = ean, Quantity = quantity });
            else
                item.Quantity += quantity;

            Save();
        }

        public Order CreateOrder()
        {
            var order = Order.Create();

            order.DeliveryAddress = DeliveryAddres;
            order.InvoiceAddress = InvoiceAddress;
            items.ForEach(x => order.Add(x));

            if (shippingMethod != null)
            {
                var item = shippingMethod.GetOrderItem(this);
                order.Add(item);
            }

            order.Add(new Transaction(Transaction.TransactionType.Credit, 200, "OpenAmount"));

            order.Save();
            return order;
        }

        public void RemoveArticle(string ean)
        {
            items.RemoveAll(x => x.Ean == ean);
            Save();
        }

        public void Save()
        {
            mongo.Save(this);
        }

        public void SetShippingMethod(IShippingMethod shippingMethod)
        {
            this.shippingMethod = shippingMethod;
            ShippingMethod = shippingMethod.Name;
            Save();
        }

        public IShippingMethod CurrentShippingMethod()
        {
            return shippingMethod;
        }

        public List<IShippingMethod> AvailableShippingMethods()
        {
            return new List<IShippingMethod>{
                new ShippingMethod(),
                new ShippingMethod1()
            };
        }

        #endregion Public Methods

        #region Private Fields

        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        private ObjectId id;

        private List<CartItem> items;
        private Mongo<Cart> mongo;
        private IShippingMethod shippingMethod;

        #endregion Private Fields

        #region Private Constructors

        private Cart()
        {
            this.mongo = new Mongo<Cart>();
            this.items = new List<CartItem>();
            this.shippingMethod = new ShippingMethod();
        }

        #endregion Private Constructors

        public void UpdateArticle(string ean, int quantity)
        {
            var item = Items.FirstOrDefault(x => x.Ean == ean);
            if (item == null)
            {
                return;
            }

            item.Quantity = quantity;

            Save();
        }
    }
}
