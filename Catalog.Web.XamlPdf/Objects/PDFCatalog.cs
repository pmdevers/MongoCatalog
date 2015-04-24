namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFCatalog : PDFObject
    {
        protected internal override void Initialize()
        {
            SetValue("ViewerPreferences", Document.CreateObject<PDFViewerPreferences>());

            Outlines.Clear();
            Pages.Clear();

            ViewerPreferences.Duplex = PDFViewerPreferences.PDFDuplex.Simplex;
            ViewerPreferences.NonFullScreenPageMode = PDFViewerPreferences.PDFNonFullScreenPageMode.UseOutlines;
        }

        public PDFResources Resources
        {
            get
            {
                return Document.Resources;
            }
        }

        public PDFPages Pages
        {
            get
            {
                return GetCollection<PDFPages>("Pages");
            }
        }

        public PDFOutlines Outlines
        {
            get
            {
                return GetCollection<PDFOutlines>("Outlines");
            }
        }

        public PDFViewerPreferences ViewerPreferences
        {
            get
            {
                return GetValue<PDFViewerPreferences>("ViewerPreferences");
            }
        }

        public PDFPageMode PageMode
        {
            get
            {
                return GetValue<PDFPageMode>("PageMode", PDFPageMode.UseNone);
            }
            set
            {
                SetValue("PageMode", value);
            }
        }

        public enum PDFPageMode
        {
            UseNone,
            UseOutlines,
            UseThumbs,
            FullScreen,
            UseOC,
            UseAttachments
        }
    }
}
