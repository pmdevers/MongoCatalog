using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Data
{
    public class Product
    {
        #region Public Properties
        
        public string SystemName { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string ItemCode { get; set; }
        public List<string> CategoryPaths { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, string> Attributes { get; set; }
        public Dictionary<string, List<string>> Options
        {
            get
            {
                return GetOptions();
            }
        }

        private Dictionary<string, List<string>> GetOptions()
        {
            var options = new Dictionary<string, List<string>>();
            var keys = Articles.Where(x=>x.Visible).SelectMany(x => x.Options.Select(y => y.Key)).Distinct();

            foreach (var key in keys)
            {
                var values = Articles.Where(x => x.Visible).Select(x => x.Options[key]).Distinct().ToList();
                options.Add(key, values);
            }

            return options;
        }
        public List<Article> Articles { get; set; }

        #endregion Public Properties

        #region Public Constructors

        private Product()
        {
            this.mongo = new Mongo<Product>();
            
        }

        #endregion Public Constructors

        #region Public Methods

        public static Product Create(string systemName)
        {
            return new Product() { SystemName = systemName, Attributes = new Dictionary<string, string>(), Articles = new List<Article>(), CategoryPaths = new List<string>() };
        }

        public static Product Get(Expression<Func<Product, bool>> predicate)
        {
            var mongo = new Mongo<Product>();

            return mongo.Get(predicate);
        }

        public static List<Product> Get(IMongoQuery query)
        {
            var mongo = new Mongo<Product>();

            return mongo.Get(query);
        }

        public static PagedResults<Product> GetProducts(IMongoQuery query, int page, int pageSize)
        {
            var mongo = new Mongo<Product>();

            return mongo.GetPagedResult(query, page, pageSize);
        }

        public static PagedResults<Product> GetProducts(Expression<Func<Product, bool>> predicate, int page, int pageSize)
        {
            var mongo = new Mongo<Product>();

            return mongo.GetPagedResult(predicate, page, pageSize);
        }

        public static bool Exists(Expression<Func<Product, bool>> predicate)
        {
            var mongo = new Mongo<Product>();

            return mongo.Exists(predicate);
        }

        public void AddArticle(string ean, decimal price, Dictionary<string, string> options)
        {
            var article = Article.Create();

            article.Ean = ean;
            article.Price = price;
            article.Options = options;
            article.Visible = true;
                       
            Articles.Add(article);
        }

        public void Save()
        {
            mongo.Save(this);
        }

        #endregion Public Methods

        #region Private Fields

        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        private ObjectId id;

        private Mongo<Product> mongo;

        #endregion Private Fields
    }
}
