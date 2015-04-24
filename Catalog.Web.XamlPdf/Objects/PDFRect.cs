namespace Catalog.Web.XamlPdf.Objects
{
    public class PDFRect
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} {1} {2} {3}]", Left, Top, Width, Height);
        }
    }
}
