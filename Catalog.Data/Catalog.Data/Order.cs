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
    public class Order
    {
        private Mongo<Order> mongo;
        private List<Transaction> transactions;
        private List<OrderItem> orderItems;

        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        private ObjectId id;
        private Order()
        {
            this.mongo = new Mongo<Order>();
            this.transactions = new List<Transaction>();
            this.orderItems = new List<OrderItem>();
        }
        public string OrderNumber { get; private set; }
        public Address InvoiceAddress { get; set; }
        public Address DeliveryAddress { get; set; }
        public OrderItem[] Items { get { return orderItems.ToArray(); } private set { orderItems = new List<OrderItem>(value);} }
        public Transaction[] Transactions { get { return transactions.ToArray(); } private set { transactions = new List<Transaction>(value);} }

        public static Order Create()
        {
            var order = new Order();
            order.OrderNumber = DateTime.Now.Ticks.ToString();

            return order;
        }

        public void Add(CartItem cartItem)
        {
            var orderItem = new OrderItem()
            {
                Ean = cartItem.Ean,
                Description = string.Format("{0}: {1}", cartItem.Product.Name, cartItem.Article.Ean),
                Quantity = cartItem.Quantity,
                Price = 0
            };

            this.orderItems.Add(orderItem);
            Save();
        }
        public void Add(OrderItem orderItem)
        {
            this.orderItems.Add(orderItem);
            Save();
        }
        public void Add(Transaction transaction)
        {
            transactions.Add(transaction);
            Save();
        }
        public void Save()
        {
            mongo.Save(this);
        }
    }
}
