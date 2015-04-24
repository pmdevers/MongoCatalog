using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catalog.Web.Framework
{
    public class FlowDocumentResult : ViewResult
    {
        public FlowDocumentResult(object model, string name)
        {
            ViewData = new ViewDataDictionary(model);
            ViewName = name;
        }

        public FlowDocumentResult() : this(new ViewDataDictionary(), "FlowDocument") { }
        public FlowDocumentResult(object model) : this(model, "FlowDocument") { }

        protected override ViewEngineResult FindView(ControllerContext context)
        {
            var result = base.FindView(context);
            if (result.View == null)
                return result;

            var flowDocumentView = new FlowDocumentView(result);

            return new ViewEngineResult(flowDocumentView, flowDocumentView);
        }
    }
}