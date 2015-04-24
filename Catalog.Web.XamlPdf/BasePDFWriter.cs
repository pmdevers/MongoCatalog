using System.IO;

namespace Catalog.Web.XamlPdf
{
    public abstract class BasePDFWriter
    {
        readonly StringWriter stringWriter = new StringWriter();
        readonly TextWriter textWriter;

        protected BasePDFWriter(TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }

        protected BasePDFWriter()
        {
            
        }

        public TextWriter Writer
        {
            get { return this.textWriter; }
        }

        public override string ToString()
        {
            return stringWriter.GetStringBuilder().ToString();
        }

        public void Write(string text)
        {
            Writer.Write(text);
        }

        public void WriteLine(string line, params object[] args)
        {
            Writer.WriteLine(line, args);
        }
    }
}
