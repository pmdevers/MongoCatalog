using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catalog.Web.XamlPdf.Objects;

namespace Catalog.Web.XamlPdf
{
    public class PDFObject : IDisposable
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public virtual string Type
        {
            get
            {
                string name = GetType().Name;
                if (name.ToLower().StartsWith("pdf"))
                {
                    name = name.Substring(3);
                }

                return "/" + name;
            }
        }

        public string Name
        {
            get { return GetValue<string>("Name"); }
            set { SetValue("Name", value); }
        }

        public int Id { get; internal set; }
        public int Offset { get; internal set; }

        public virtual string Ref
        {
            get { return string.Format("{0} 0 R", Id); }
        }

        public PDFDocument Document { get; internal set; }

        internal protected virtual void Initialize()
        {
            
        }

        public virtual void Write(TextWriter writer)
        {
            var s = ((StreamWriter)writer).BaseStream;
            Offset = (int)s.Length;

            writer.WriteLine("{0} 0 obj", Id);
            WriteHeader(writer);
            WriteContents(writer);
            writer.WriteLine("endobj");

        }

        protected virtual void WriteHeader(TextWriter writer)
        {
            writer.Write("<<");
            
            if (!string.IsNullOrEmpty(Type))
            {
                writer.WriteLine("/Type {0}", Type);    
            }
            WriteProperties(writer);
            writer.WriteLine(">>");
        }

        protected virtual void WriteProperties(TextWriter writer)
        {
            foreach (var item in values)
            {
                var value = item.Value;
                var key = item.Key;
                WriteProperty(writer, value, key);
            }
        }

        protected virtual void WriteProperty(TextWriter writer, object value, string key)
        {
            if (value is string[])
            {
                writer.WriteLine("/{0} [/{1}]", key, string.Join(" /", ((string[])value)));
                return;
            }
            if (value is IPDFDictionary)
            {
                var d = value as IPDFDictionary;
                if (d.HasValues)
                {
                    writer.Write("/{0} ", key);
                    d.Write(writer);
                }
                return;
            }
            if (value is IPDFInlineObject)
            {
                writer.Write("/{0} ", key);
                var pdfobj = value as PDFObject;
                pdfobj.WriteHeader(writer);
                return;
            }
            if (value is PDFObject)
            {
                writer.WriteLine("/{0} {1}", key, ((PDFObject)value).Ref);
                return;
            }
            if (value is DateTime)
            {
                writer.WriteLine("/{0} (D:{1:yyyyMMddHHmmss})", key, value);
                return;
            }
            if (value.GetType().IsEnum)
            {
                var t = value.GetType();
                if (t.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0)
                {
                    var val = (int)value;
                    writer.WriteLine("/{0} {1}", key, val);
                    return;
                }
                writer.WriteLine("/{0} /{1}", key, value.ToString());
                return;
            }
            if (value is string)
            {
                writer.WriteLine("/{0} /{1}", key, value);
                return;
            }

            if (value is Uri)
            {
                writer.WriteLine("/{0} ({1})", key, value);
                return;
            }

            writer.WriteLine("/{0} {1}", key, value);
        }

        protected virtual void WriteContents(TextWriter writer)
        {

        }

        public T GetValue<T>(string name, T val = default(T))
        {
            object v = null;
            if (!values.TryGetValue(name, out v))
            {
                values[name] = val;
            }
            else
            {
                val = (T)v;
            }
            return val;
        }

        public void SetValue<T>(string name, T val)
        {
            values[name] = val;
        }

        public TC GetCollection<TC>(string name)
            where TC : PDFObject
        {
            TC val = null;
            object obj = null;
            if (!values.TryGetValue(name, out obj))
            {
                val = Document.CreateObject<TC>();
                values[name] = val;
            }
            else
            {
                val = (TC)obj;
            }
            return val;
        }


        public virtual void Dispose()
        {

        }
    }
}
