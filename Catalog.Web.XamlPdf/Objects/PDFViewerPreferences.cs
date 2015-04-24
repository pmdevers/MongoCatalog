namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFViewerPreferences : PDFObject
    {
        #region Public Properties

        public bool CenterWindow
        {
            get { return GetValue<bool>("CenterWindow"); }
            set { SetValue("CenterWindow", value); }
        }

        public PDFDirection Direction
        {
            get { return GetValue("Direction", PDFDirection.L2R); }
            set { SetValue("Direction", value); }
        }

        public bool DisplayDocTitle
        {
            get { return GetValue<bool>("DisplayDocTitle"); }
            set { SetValue("DisplayDocTitle", value); }
        }

        public PDFDuplex Duplex
        {
            get { return GetValue("Duplex", PDFDuplex.Simplex); }
            set { SetValue("Duplex", value); }
        }

        public bool FitWindow
        {
            get { return GetValue<bool>("FitWindow"); }
            set { SetValue("FitWindow", value); }
        }

        public bool HideMenubar
        {
            get { return GetValue<bool>("HideMenubar"); }
            set { SetValue("HideMenubar", value); }
        }

        public bool HideToolbar
        {
            get { return GetValue<bool>("HideToolbar"); }
            set { SetValue("HideToolbar", value); }
        }

        public bool HideWindowUI
        {
            get { return GetValue<bool>("HideWindowUI"); }
            set { SetValue("HideWindowUI", value); }
        }

        public PDFNonFullScreenPageMode NonFullScreenPageMode
        {
            get { return GetValue("NonFullScreenPageMode", PDFNonFullScreenPageMode.UseNone); }
            set { SetValue("NonFullScreenPageMode", value); }
        }

        public PDFPrintScaling PrintScaling
        {
            get { return GetValue("PrintScaling", PDFPrintScaling.None); }
            set { SetValue("PrintScaling", value); }
        }

        public override string Type
        {
            get
            {
                return "";
            }
        }

        #endregion Public Properties

        #region Public Enums

        public enum PDFDirection
        {
            L2R,
            R2L
        }

        public enum PDFDuplex
        {
            None,
            Simplex,
            DuplexFlipShortEdge,
            DuplexFlipLongEdge
        }

        public enum PDFNonFullScreenPageMode
        {
            UseNone,
            UseOutlines,
            UseThumbs,
            UseOC
        }

        public enum PDFPrintScaling
        {
            None,
            AppDefault
        }

        #endregion Public Enums
    }
}
