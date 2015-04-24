using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Catalog.Web.Pdf.PdfForms
{
    public class PdfFormActionResult : ActionResult
    {
        readonly string templatePath;
        readonly IEnumerable<IPdfMergeData> mergeDataItems;
        public PdfFormActionResult(string templatePath, IEnumerable<IPdfMergeData> mergeDataItems)
        {
            this.templatePath = templatePath;
            this.mergeDataItems = mergeDataItems;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            var streamer = new PdfMergeStreamer();
            byte[] results;
            using (var stream = new MemoryStream())
            {

                streamer.FillPDF(templatePath, mergeDataItems, stream);
                results = stream.ToArray();
            }

            if (!String.IsNullOrEmpty(FileDownloadName))
            {
                context.HttpContext.Response.AddHeader("content-disposition",
                    "attachment; filename=" + FileDownloadName);
            }

            new FileContentResult(results, "application/pdf")
                .ExecuteResult(context);

        }

        public string FileDownloadName { get; set; }
    }
}
