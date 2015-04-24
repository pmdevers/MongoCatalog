using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Xml.Linq;

using Catalog.Web.XamlPdf.Xaml;

namespace Catalog.Web.Framework
{
    public class FlowDocumentView : IView, IViewEngine
    {
        private readonly ViewEngineResult result;

        public FlowDocumentView(ViewEngineResult result)
        {
            this.result = result;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var sb = new StringBuilder();
            var tw = new StringWriter(sb);

            result.View.Render(viewContext, tw);
            
            var resultCache = sb.ToString();

            var flowDocument = (FlowDocument)XamlReader.Parse(resultCache);
            string results;

            XamlPdfWriter writer1 = new XamlPdfWriter();

            var file = System.IO.File.Create("c:\\test.pdf");

            using (var stream = file)
            {
                writer1.Write(resultCache, stream);
            }   
            
            viewContext.HttpContext.Response.ContentType = "application/pdf";
            viewContext.HttpContext.Response.AddHeader("Content-Disposition", "attachment; filename=Form.pdf");

            return;
            //var stream = FlowDocumentToXPS(flowDocument, (int)flowDocument.PageWidth, (int)flowDocument.PageHeight);
            //stream.CopyTo(viewContext.HttpContext.Response.OutputStream);
            //viewContext.HttpContext.Response.ContentType = "Application/octet-stream";
            //viewContext.HttpContext.Response.AppendHeader("content-disposition", string.Format("inline;FileName=\"{0}\"", "document.xps"));
        }

        public MemoryStream FlowDocumentToXPS(FlowDocument flowDocument, int width, int height)
        {
            var stream = new MemoryStream();
            using (var package = Package.Open(stream, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var xpsDoc = new XpsDocument(package, CompressionOption.Maximum))
                {
                    var rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    paginator.PageSize = new System.Windows.Size(width, height);
                    rsm.SaveAsXaml(paginator);
                    rsm.Commit();
                }
            }
            stream.Position = 0;
            return stream;
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            throw new NotImplementedException();
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            throw new NotImplementedException();
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            result.ViewEngine.ReleaseView(controllerContext, result.View);
        }
    }
}
