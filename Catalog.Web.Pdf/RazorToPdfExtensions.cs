using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Catalog.Web.Pdf
{
    public static class MvcRazorToPdfExtensions
    {
        public static byte[] GeneratePdf(this ControllerContext context, object model = null, string viewName = null,
            Action<PdfWriter, Document> configureSettings = null)
        {
            return new RazorToPdf().GeneratePdfOutput(context, model, viewName, configureSettings);
        }
    }
}
