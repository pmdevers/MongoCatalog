using Catalog.Data.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
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
    public class Mongo<T>  where T : class
    {
        private MongoClient client;
        private MongoServer server;
        private MongoDatabase database;
        private MongoCollection<T> collection;

        internal MongoCollection<T> Collection { get { return this.collection; } }

        public Mongo()
        {
            this.client = new MongoClient(string.Format("mongodb://{0}", Settings.Server));
            this.server = client.GetServer();
            this.database = server.GetDatabase(Settings.Database);
            this.collection = database.GetCollection<T>(typeof(T).Name);
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return collection.AsQueryable<T>().Any(predicate);
        }

        public T Get(Expression<Func<T, bool>> query)
        {
            return collection.AsQueryable().FirstOrDefault(query);
        }

        public List<T> Get(IMongoQuery query)
        {
            return collection.Find(query).ToList();
        }

        public PagedResults<T> GetPagedResult(Expression<Func<T, bool>> predicate, int page, int pageSize)
        {
            if (page < 1)
                page = 1;

            var results = new PagedResults<T>();
            
            results.Items = collection.AsQueryable().Where(predicate).Skip((page-1) * pageSize).Take(pageSize).ToList();
            results.ItemsPerPage = pageSize;
            results.TotalItems = TotalItems(predicate);
            results.TotalPages = results.TotalItems / pageSize;
            results.CurrentPage = page;

            return results;
        }
        public PagedResults<T> GetPagedResult(IMongoQuery query, int page, int pageSize)
        {
            var results = new PagedResults<T>();

            results.Items = collection.Find(query).Skip(page * pageSize).Take(pageSize).ToList();
            results.ItemsPerPage = pageSize;
            results.TotalItems = TotalItems(query);
            results.TotalPages = results.TotalItems / pageSize;
            results.CurrentPage = page;

            return results;
        }
        private long TotalItems(IMongoQuery query)
        {
            return collection.Count(query);
        }
        private long TotalItems(Expression<Func<T, bool>> predicate)
        {
            return collection.AsQueryable().Count(predicate);
        }

        public void Save(T item)
        {
            collection.Save(item);
        }
    }
}
