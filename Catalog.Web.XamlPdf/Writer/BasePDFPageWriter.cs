using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catalog.Web.XamlPdf.Objects;

namespace Catalog.Web.XamlPdf.Writer
{
    public class BasePDFPageWriter
    {
        public virtual string GetValue(PDFPage page, Object context, Object val)
        {
            return null;
        }

        public virtual void Write(PDFPage page, Object context, Object val)
        {
        }

        public virtual Type VisualType
        {
            get
            {
                return null;
            }
        }

        public virtual void EndWrite(PDFPage page, object context, object fp)
        {

        }
    }

}
