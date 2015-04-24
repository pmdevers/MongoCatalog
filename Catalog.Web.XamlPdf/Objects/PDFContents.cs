using System.IO;
using System.Text;

namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFContents : PDFObject
    {
        StringWriter contentWriter = new StringWriter();

        public StringWriter ContentWriter
        {
            get { return this.contentWriter; }
        }

        public string Content { get; set; }

        public override string Type
        {
            get { return null; }
        }

        public void Compress()
        {
            var content = contentWriter.ToString();
            contentWriter = new StringWriter();
            var ms = new MemoryStream();
            var d = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress);

            var data = Encoding.Default.GetBytes(content);

            d.Write(data, 0, data.Length);
            d.Flush();
            d.Close();

            data = ms.ToArray();

            contentWriter.Write(Encoding.Default.GetString(data));

            SetValue("Filter", new [] { "FlateDecode" });
        }

        public override void Write(TextWriter writer)
        {
            SetValue("Length", contentWriter.ToString().Length);
            base.Write(writer);
        }

        protected override void WriteContents(TextWriter writer)
        {
            writer.WriteLine("stream");
            writer.WriteLine(contentWriter.ToString());
            writer.WriteLine("endstream");
        }

        public override void Dispose()
        {
            base.Dispose();

            if (contentWriter == null)
            {
                return;
            }

            contentWriter.Dispose();
            contentWriter = null;
        }

    }
}
