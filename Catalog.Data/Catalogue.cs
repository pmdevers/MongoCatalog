using Catalog.Data.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Data
{
    public class Catalogue
    {
        private List<Category> categories;

        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        private ObjectId id;
        private readonly Mongo<Catalogue> mongo;

        public string Name { get; private set; }
        public string Url { get { return UrlSlugger.ToUrlSlug(Name); } }

        public Category[] Categories 
        {
            get { return this.categories.ToArray(); }
            set { categories = new List<Category>(value); } 
        }

        public Catalogue()
        {
            this.mongo = new Mongo<Catalogue>();
            categories = new List<Category>();
        }

        public Catalogue(string catalogName)
            :this()
        {
            Name = catalogName;
            Save();
        }

        public Category Add(string name)
        {
            var test = new Category(this, null, name);
            categories.Add(test);
            return test;
        }

        public Category GetCategory(string path)
        {
            path = this.Url + "/" + path;
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            Category category = null;
            var depth = 1;
            if (segments.Length <= 1)
                return null;

            foreach(var cat in Categories)
            {
                if(cat.Url == segments[depth])
                    category = cat.Get(segments, depth);
            }
            if (category == null)
                throw new Exception(string.Format("Category '{0}' not found!", path));

            return category;
        }
        public static Catalogue Get(string catalogName)
        {
            var mongo = new Mongo<Catalogue>();

            var catalog = mongo.Get(x => x.Name.ToLower() == catalogName.ToLower()) ?? new Catalogue(catalogName);

            PopulateParents(catalog, catalog.Categories);

            return catalog;
        }
        private static void PopulateParents(Catalogue catalog, Category[] categories, Category parent = null)
        {
            foreach(var category in categories)
            {
                category.Catalog = catalog;
                category.Parent = parent;
                PopulateParents(catalog, category.Categories, category);
            }
        }

        public long TotalProducts()
        {
            var prod = new Mongo<Product>().Collection.Count(Query<Product>.Where(x => x.CategoryPaths.Contains("/" + this.Url)));
            return prod;
        }

        public PagedResults<Product> GetProducts(int page, int pageSize)
        {
            var productCollection = new Mongo<Product>();
            return productCollection.GetPagedResult(x => x.CategoryPaths.Contains("/" + Url), page, pageSize);
        }

        public void Save()
        {
            mongo.Save(this);
        }

        public static bool Exists(string catalogName)
        {
            return new Mongo<Catalogue>().Exists(x => x.Name.ToLower() == catalogName.ToLower());
        }
    }

}
