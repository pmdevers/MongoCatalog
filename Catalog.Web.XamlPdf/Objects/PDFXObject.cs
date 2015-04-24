namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFXObject : PDFObject
    {
        public string SubType
        {
            get { return GetValue<string>("SubType"); }
            set { SetValue("SubType", value); }
        }
    }

    public class PDFXObjects : PDFDictionary<PDFObject>
    {
        
    }
}
