using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catalog.Web.XamlPdf.Objects;

namespace Catalog.Web.XamlPdf
{
    public class PDFDocument : IDisposable
    {
        private readonly List<PDFObject> objects = new List<PDFObject>();
        public PDFDocument()
        {
            Catalog = CreateObject<PDFCatalog>();
            //objects = new List<PDFObject>();
            Info = CreateObject<PDFInfo>();
            Info.Creator = "XamlToPDF Converter";
            Info.Producer = "NeuroSpeech XAML to PDF Converter";
            Resources = CreateObject<PDFResources>();
        }

        public PDFInfo Info { get; private set; }

        public PDFResources Resources { get; private set; }

        public PDFCatalog Catalog { get; private set; }

        public IEnumerable<PDFObject> Objects { get { return objects; } }

        public T CreateObject<T>() where T : PDFObject
        {
            var obj = Activator.CreateInstance<T>();
            obj.Id = objects.Count() + 1;
            obj.Document = this;
            objects.Add(obj);
            obj.Initialize();
            return obj;
        }

        public void Write(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.AutoFlush = true;

                writer.WriteLine("%PDF-1.6");
                foreach (var item in objects.OrderBy(x => x.Id))
                {
                    item.Write(writer);
                }

                int start = (int)stream.Length;

                writer.WriteLine("xref");
                writer.WriteLine("0 {0}", objects.Count + 1);
                writer.WriteLine("0000000000 65535 f");

                foreach (var item in objects.OrderBy(x => x.Id))
                {
                    writer.WriteLine("{0:D10} 00000 n", item.Offset);
                }

                writer.WriteLine();

                writer.WriteLine("trailer");
                writer.WriteLine("<<");
                writer.WriteLine("/Size {0}", objects.Count + 1);
                writer.WriteLine("/Root 1 0 R");
                writer.WriteLine("/Info {0} 0 R", Info.Id);
                writer.WriteLine(">>");
                writer.WriteLine("startxref");
                writer.WriteLine("{0}", start);
                writer.WriteLine("%%EOF");
            }

        }


        #region public void  Dispose()
        public void Dispose()
        {
            foreach (var item in Objects)
            {
                item.Dispose();
            }
        }
        #endregion

    }
}
