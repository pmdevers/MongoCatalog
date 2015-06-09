using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalog.Data
{
    public class Transaction
    {
        public enum TransactionType {
            Debit = 0,
            Credit = 1
        }
        
        public Transaction(TransactionType type, decimal amount, string description, Dictionary<string, string> attributes = null)
        {
            Type = type;
            Amount = amount;
            Description = description;
            Attributes = attributes ?? new Dictionary<string, string>();
        }

        public TransactionType Type { get; private set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; }

        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, string> Attributes { get; private set; }
        

    }
}
