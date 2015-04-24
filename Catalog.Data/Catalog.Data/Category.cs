using Catalog.Data.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Catalog.Data
{
    public class Category
    {
        private List<Category> categories;
        private Category()
        {
        }
        internal Category(Catalogue catalog, Category parent, string name)
        {
            this.Name = name;
            Parent = parent;
            categories = new List<Category>();
            Catalog = catalog;
        }
        public string Name { get; private set; }
        public string Url { get { return UrlSlugger.ToUrlSlug(Name); } }

        public Category[] Categories { 
            get { return categories.ToArray(); } 
            set { categories = new List<Category>(value); } 
        }

        [BsonIgnore]
        public Category Parent { get; internal set; }

        [BsonIgnore]
        public Catalogue Catalog { get; internal set; }
        
        public Category Add(string name)
        {
            if (categories.Any(x => x.Name == name))
                throw new Exception(string.Format("Category {0} already exists.", name));

            var test = new Category(Catalog, this, name);
            categories.Add(test);
            return test;
        }

        public string Path
        {
            get
            {
                return Parent == null ? "/" + Catalog.Url + "/" + Url : Parent.Path + "/" + Url;
            }
        }

        public void ChangeName(string newName)
        {
            var mongo = new Mongo<Product>();
            var oldPath = this.Path + "/";
            Name = newName;
            var newPath = this.Path + "/";

            var query = Query.Matches("CategoryPaths", new BsonRegularExpression(new Regex("^" + oldPath, RegexOptions.IgnoreCase)));
            var result = mongo.Collection.Find(query);

            foreach(var product in result)
            {

                var newResult = product.CategoryPaths.Where(x=>!x.StartsWith(oldPath)).ToList();
                foreach(var catPath in product.CategoryPaths.Where(x=>x.StartsWith(oldPath)))
                {
                    newResult.Add(catPath.Replace(oldPath, newPath));
                }

                product.CategoryPaths = newResult;
                product.Save();
            }

            Catalog.Save();
        }

        public void AddProduct(Product product, bool drillup = false)
        {
            if (drillup && this.Parent != null)
                this.Parent.AddProduct(product, drillup);

            if (product.CategoryPaths.Any(x => x == this.Path))
                return;
            
            product.CategoryPaths.Add(this.Path);

            product.Save();
        }

        public long TotalProducts()
        {
            var query = Query.Matches("CategoryPaths", new BsonRegularExpression(new Regex("^" + Path, RegexOptions.IgnoreCase)));
            var prod = new Mongo<Product>().Collection.Count(query);
            return prod;
        }

        public PagedResults<Product> GetProducts(int page, int pageSize)
        {
            var mongo = new Mongo<Product>();

            return mongo.GetPagedResult(x => x.CategoryPaths.Contains(this.Path), page, pageSize);
        }

        internal Category Get(string[] segments, int depth)
        {
            depth++;

            if (segments.Length -1 < depth)
                return this;

            Category category = this;
            foreach(var cat in Categories)
            {
                if (cat.Url == segments[depth])
                    category = cat.Get(segments, depth);
            }

            return category;
        }
    }
}
