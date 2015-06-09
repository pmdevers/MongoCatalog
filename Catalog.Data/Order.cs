using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Catalog.Data
{
    public class Order
    {
        private readonly Mongo<Order> mongo;
        private List<Transaction> transactions;
        private List<OrderItem> orderItems;
        private List<OrderComment> orderComments;
        
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        private ObjectId id;

        private Order()
        {
            this.mongo = new Mongo<Order>();
            this.transactions = new List<Transaction>();
            this.orderItems = new List<OrderItem>();
            this.orderComments = new List<OrderComment>();
            
        }
        public string OrderNumber { get; private set; }
        public Address InvoiceAddress { get; set; }
        public Address DeliveryAddress { get; set; }
        public OrderItem[] Items { get { return orderItems.ToArray(); } private set { orderItems = new List<OrderItem>(value);} }
        public Transaction[] Transactions { get { return transactions.ToArray(); } private set { transactions = new List<Transaction>(value);} }
        public OrderComment[] Comments { get { return orderComments.ToArray(); } private set { orderComments = new List<OrderComment>(value); } }

        public decimal TotalPrice
        {
            get { return TotalPriceWithoutVat + TotalVat; }
        }
        public int TotalQuantity
        {
            get { return this.Items.Sum(x => x.Quantity); }
        }
        public decimal TotalPriceWithoutVat
        {
            get { return this.Items.Sum(x => x.TotalPriceWithoutVat); }
        }
        public decimal TotalVat
        {
            get { return Items.Sum(x => x.Vat); }
        }

        public decimal OpenAmount
        {
            get { return TotalPrice + Transactions.Where(x => x.Type == Transaction.TransactionType.Credit).Sum(x => x.Amount); }
        }

        public decimal PaidAmount
        {
            get { return Transactions.Where(x => x.Type == Transaction.TransactionType.Debit).Sum(x => x.Amount); }
        }

        public bool IsPaid
        {
            get { return OpenAmount - PaidAmount <= 0; }
        }

        public Dictionary<decimal, decimal> Taxes
        {
            get { return Items.GroupBy(x => x.VatPercentage).ToDictionary(x => x.Key, y => y.Sum(z => z.Vat)); }
        }

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
                VatPercentage = cartItem.VatPercentage,
                Price = cartItem.Price
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

        public void Add(OrderComment comment)
        {
            orderComments.Add(comment);
            Save();
        }
        
        public void Save()
        {
            mongo.Save(this);
        }
    }
}
