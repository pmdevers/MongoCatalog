using System.IO;


namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFImage : PDFXObject
    {
        public MemoryStream Stream { get; private set; }
        public override string Type
        {
            get { return "/XObject"; }
        }
        public int Width
        {
            get { return GetValue<int>("Width"); }
            set { SetValue("Width", value); }
        }
        public int Height
        {
            get { return GetValue<int>("Height"); }
            set { SetValue("Height", value); }
        }
        public string ColorSpace
        {
            get { return GetValue<string>("ColorSpace"); }
            set { SetValue("ColorSpace", value); }
        }
        public int BitsPerComponent
        {
            get { return GetValue<int>("BitsPerComponent"); }
            set { SetValue("BitsPerComponent", value); }
        }

        protected override void WriteContents(TextWriter writer)
        {
            writer.WriteLine("stream");
            writer.Flush();

            var i = writer as StreamWriter;
            var a = Stream.ToArray();
            i.BaseStream.Write(a, 0, a.Length);
            i.BaseStream.Flush();
            writer.WriteLine("endstream");
        }
        public override void Dispose()
        {
            base.Dispose();
            if (Stream == null)
            {
                return;
            }
            Stream.Dispose();
            Stream = null;
        }
        public override void Write(TextWriter writer)
        {
            //Compress();

            //Encode();

            SetValue("Filter", "DCTDecode");
            SetValue("Length", Stream.ToArray().Length);
            base.Write(writer);
        }
        protected internal override void Initialize()
        {
            base.Initialize();

            SubType = "Image";

            Stream = new MemoryStream();
        }

    }

    public class PDFImages : PDFDictionary<PDFImage>
    {
    }
}
