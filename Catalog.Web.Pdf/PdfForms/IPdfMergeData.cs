using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Web.Pdf.PdfForms
{
    public interface IPdfMergeData
    {   IDictionary<string, string> MergeFieldValues { get; }
    }
}
