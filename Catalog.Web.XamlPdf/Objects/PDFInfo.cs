using System;
using System.IO;


namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFInfo : PDFObject
    {
        public override string Type
        {
            get { return ""; }
        }
        public string Title
        {
            get { return GetValue<string>("Title"); }
            set { SetValue("Title", value); }
        }
        public string Author
        {
            get { return GetValue<string>("Author"); }
            set { SetValue("Autor", value); }
        }
        public string Subject
        {
            get { return GetValue<string>("Subject"); }
            set { SetValue("Subject", value); }
        }
        public string Keywords
        {
            get { return GetValue<string>("Keywords"); }
            set { SetValue("Keywords", value); }
        }
        public string Producer
        {
            get { return GetValue<string>("Producer"); }
            set { SetValue("Producer", value); }
        }
        public string Creator
        {
            get { return GetValue<string>("Creator"); }
            set { SetValue("Creator", value); }
        }
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>("CreationDate"); }
            set { SetValue("CreationDate", value); }
        }
        public DateTime ModDate
        {
            get { return GetValue<DateTime>("ModDate"); }
            set { SetValue("ModDate", value); }
        }
        protected override void WriteProperty(TextWriter writer, object value, string key)
        {
            var val = value as string;
            if (val == null)
            {
                base.WriteProperty(writer, value, key);
            }

            writer.WriteLine("/{1} {0}", "(" + Encode(val) + ")", key);
        }

        private string Encode(string p)
        {
            p = p.Replace("\\", "\\\\");
            p = p.Replace("(", "\\(");
            p = p.Replace(")", "\\)");
            p = p.Replace("\n", "\\n");
            p = p.Replace("\r", "\\r");
            p = p.Replace("\t", "\\t");
            p = p.Replace("\b", "\\b");
            p = p.Replace("\f", "\\f");
            return p;
        }
    }
}
