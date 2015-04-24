using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catalog.Web.XamlPdf.Objects;

namespace Catalog.Web.XamlPdf.Writer
{
    public class PDFPageDefaultWriter : PDFPageWriter<object, object>
    {
        public override void WriteVisual(PDFPage page, object context, object visual)
        {
            Trace.WriteLine("Warning !! Writer not found for " + visual.GetType().FullName);
        }
    }
}
