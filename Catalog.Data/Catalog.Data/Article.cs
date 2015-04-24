using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Data
{
    public class Article
    {
        private Article()
        {

        }

        public string Ean { get; set; }
        public decimal Price { get; set; }
        
        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, string> Options { get; set; }

        public static Article Create()
        {
            return new Article { Options = new Dictionary<string, string>() };
        }

        public bool Visible { get; set; }
    }
}
