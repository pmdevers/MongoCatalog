using System;
using System.Collections.Generic;
using System.IO;

namespace Catalog.Web.XamlPdf.Objects
{
    public interface IPDFInlineObject { }

    public class PDFAnnotationAction : PDFObject, IPDFInlineObject
    {
        public override string Type
        {
            get { return "/Action"; }
        }

        public string Action
        {
            get { return GetValue<string>("S"); }
            set { SetValue("S", value); }
        }
    }

    public class PDFAnnotationURIAction : PDFAnnotationAction
    {
        public PDFAnnotationURIAction()
        {
            Action = "URI";
        }

        public Uri Uri
        {
            get { return GetValue<Uri>("URI"); }
            set { SetValue("URI", value); }
        }
    }

    public class PDFAnnotation : PDFXObject
    {
        public override string Type
        {
            get { return "/Annots"; }
        }
    }

    public class PDFLinkAnnotation : PDFAnnotation
    {
        private PDFAnnotationURIAction action;

        protected internal override void Initialize()
        {
            base.Initialize();

            SubType = "Link";

            action = new PDFAnnotationURIAction();

            SetValue("A", action);

            SetValue("Border", new PDFRect {});
        }

        public PDFRect Rect
        {
            get { return GetValue<PDFRect>("Rect"); }
            set { SetValue("Rect", value); }
        }

        public Uri Uri
        {
            get { return action.Uri; }
            set { action.Uri = value; }
        }
    }

    public class PDFAnnotations : PDFObject
    {
        private List<PDFAnnotation> annotations = new List<PDFAnnotation>();

        public void AddLink(Uri link, PDFRect rect)
        {
            var l = Document.CreateObject<PDFLinkAnnotation>();
            l.Uri = link;
            l.Rect = rect;
            annotations.Add(l);
        }

        protected override void WriteHeader(TextWriter writer)
        {
            // Do not write header for this ellement;
        }

        protected override void WriteContents(TextWriter writer)
        {
            writer.WriteLine("[");
            annotations.ForEach(x => writer.WriteLine("{0} 0 R", x.Id));
            writer.WriteLine("]");
        }
    }
}
