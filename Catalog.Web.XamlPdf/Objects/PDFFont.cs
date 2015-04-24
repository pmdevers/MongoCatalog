namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFFont : PDFObject
    {
        public string SubType
        {
            get { return GetValue<string>("SubType"); }
            set { SetValue("SubType", value); }
        }

        public string BaseFont
        {
            get { return GetValue<string>("BaseFont"); }
            set { SetValue("BaseFont", value); }
        }

        public string Encoding
        {
            get { return GetValue<string>("Encoding"); }
            set { SetValue("Encoding", value); }
        }

        public PDFFontDescriptor FontDescriptor
        {
            get { return GetValue<PDFFontDescriptor>("FontDescriptor"); }
            set { SetValue("FontDesciptor", value); }
        }
    }

    public class PDFFonts : PDFDictionary<PDFFont>
    {
    }
}
