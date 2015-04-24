using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catalog.Web.XamlPdf.Objects;

namespace Catalog.Web.XamlPdf.Writer
{
    public abstract class PDFPageWriter<T, TX> : BasePDFPageWriter
    {
        public sealed override Type VisualType
        {
            get { return typeof(T); }
        }

        public sealed override void Write(PDFPage page, object context, object val)
        {
            WriteVisual(page, (TX)context, (T)val);
        }

        public sealed override void EndWrite(PDFPage page, object context, object fp)
        {
            EndWriteVisual(page, (TX)context, (T)fp);
        }

        public sealed override string GetValue(PDFPage page, object context, object val)
        {
            return GetPDFValue(page, (TX)context, (T)val);
        }
        protected virtual string GetPDFValue(PDFPage page, TX context, T value)
        {
            return "";
        }

        public abstract void WriteVisual(PDFPage page, TX context, T visual);
        public virtual void EndWriteVisual(PDFPage page, TX context, T visual)
        {
        }
    }
}
