using System.Collections.Generic;
using System.IO;

namespace Catalog.Web.XamlPdf.Objects
{
    public interface IPDFDictionary
    {
        bool HasValues { get; }

        void Write(TextWriter writer);
    }

    public class PDFDictionary<T> : IPDFDictionary 
        where T : PDFObject
    {
        private readonly Dictionary<string, PDFObject> references = new Dictionary<string, PDFObject>();

        public T this[string name]
        {
            get
            {
                PDFObject obj = null;
                references.TryGetValue(name, out obj);
                return (T)obj;
            }
            set
            {
                references[name] = value;
                value.Name = name;
            }
        }

        public bool HasValues
        {
            get
            {
                return references.Count > 0;
            }
        }

        public void Write(TextWriter writer)
        {
            writer.WriteLine("<<");
            foreach (var item in references)
            {
                writer.WriteLine("/{0} {1}", item.Key, item.Value.Ref);
            }
            writer.WriteLine(">>");
        }
    }
}
