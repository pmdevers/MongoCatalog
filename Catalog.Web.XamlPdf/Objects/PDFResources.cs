namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFResources : PDFObject
    {
        public bool HasText { get; set; }
        public bool HasImage { get; set; }

        protected override void WriteProperties(System.IO.TextWriter writer)
        {
            var ps = "/PDF ";
            if (HasText)
            {
                ps += "/Text";
            }
            if (HasImage)
            {
                ps += "/Image";
            }

            writer.WriteLine("/ProcSet [{0}]", ps);
            base.WriteProperties(writer);
        }


        #region public override string  Type
        public override string Type
        {
            get { return null; }
        }
        #endregion

        public PDFFonts Font
        {
            get { return GetValue("Font", new PDFFonts()); }
        }

        public PDFXObjects XObject
        {
            get { return GetValue("XObject", new PDFXObjects()); }
        }

    }
}
