using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;
using System.Xaml;
using System.Xml.Linq;

namespace Catalog.Web.XamlPdf.Xaml
{
    public class DownloadItem
    {
        #region Public Properties

        public string FilePath { get; set; }

        public List<XElement> Nodes { get; private set; }

        public string PackgeUri { get; set; }

        public string Url { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public DownloadItem()
        {
            Nodes = new List<XElement>();
        }

        #endregion Public Constructors
    }

    public class FlowDocumentPackage : IDisposable
    {

        #region Public Fields

        public List<IDisposable> toDispose;

        #endregion Public Fields

        #region Public Properties

        public FixedDocument Document { get; private set; }

        #endregion Public Properties

        #region Public Constructors

        public FlowDocumentPackage(string xamlFlowDocument)
        {
            toDispose = new List<IDisposable>();

            TempFolder = new DirectoryInfo(Path.GetTempPath() + "\\XamlToPDF\\" + DateTime.Now.Ticks.ToString());
            if (!TempFolder.Exists)
                TempFolder.Create();

            XpsDoc = new XpsDocument(TempFolder.FullName + "\\a.xps", FileAccess.ReadWrite, CompressionOption.NotCompressed);
            toDispose.Add(new InlineDisposable(() => XpsDoc.Close()));

            System.Windows.Xps.XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(XpsDoc);

            xamlFlowDocument = ReplaceImages(writer, xamlFlowDocument);

            xamlFlowDocument = xamlFlowDocument.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");

            //var flowDoc = XamlServices.Parse(xamlFlowDocument) as FlowDocument;
            flowDoc = (FlowDocument)System.Windows.Markup.XamlReader.Parse(xamlFlowDocument);

            IDocumentPaginatorSource src = (IDocumentPaginatorSource)flowDoc;

            if (double.IsNaN(flowDoc.PageHeight))
            {
                flowDoc.PageHeight = 792;
            }
            if (double.IsNaN(flowDoc.PageWidth))
            {
                flowDoc.PageWidth = 612;
            }
            var pgn = src.DocumentPaginator;

            writer.Write(pgn);

            var seq = XpsDoc.GetFixedDocumentSequence();
            var reff = seq.References.First();

            Document = reff.GetDocument(true);
        }

        public FlowDocumentPackage(FlowDocument flowDocument)
        {
            toDispose = new List<IDisposable>();

            TempFolder = new DirectoryInfo(Path.GetTempPath() + "\\XamlToPDF\\" + DateTime.Now.Ticks.ToString());
            if (!TempFolder.Exists)
                TempFolder.Create();

            XpsDoc = new XpsDocument(TempFolder.FullName + "\\a.xps", FileAccess.ReadWrite, CompressionOption.NotCompressed);
            toDispose.Add(new InlineDisposable(() => XpsDoc.Close()));

            System.Windows.Xps.XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(XpsDoc);

            //var flowDoc = XamlServices.Parse(xamlFlowDocument) as FlowDocument;
            flowDoc = flowDocument;

            IDocumentPaginatorSource src = (IDocumentPaginatorSource)flowDoc;

            if (double.IsNaN(flowDoc.PageHeight))
            {
                flowDoc.PageHeight = 792;
            }
            if (double.IsNaN(flowDoc.PageWidth))
            {
                flowDoc.PageWidth = 612;
            }
            var pgn = src.DocumentPaginator;

            writer.Write(pgn);

            var seq = XpsDoc.GetFixedDocumentSequence();
            var reff = seq.References.First();

            Document = reff.GetDocument(true);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {

            if (toDispose.Count > 0)
            {
                foreach (var item in toDispose.ToArray().Reverse())
                {
                    item.Dispose();
                }

                try
                {

                    TempFolder.Delete(true);
                }
                catch { }
            }

        }

        #endregion Public Methods

        #region Private Fields

        private readonly Dictionary<string, DownloadItem> cache = new Dictionary<string, DownloadItem>();
        private readonly FlowDocument flowDoc;
        private readonly DirectoryInfo TempFolder;
        private readonly XpsDocument XpsDoc;
        private FlowDocument flowDocument;

        #endregion Private Fields

        #region Private Methods

        private void DownloadFile(DownloadItem item)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(item.Url, item.FilePath);
                }
            }
            catch
            {

            }

            // check if zero sized file...
            FileInfo info = new FileInfo(item.FilePath);
            if (!info.Exists || info.Length == 0)
            {
                // write empty file....
                File.WriteAllBytes(item.FilePath, XResources.EmptyImage1);
            }

        }

        private string ReplaceImages(System.Windows.Xps.XpsDocumentWriter writer, string xamlFlowDocument)
        {
            var doc = XDocument.Parse(xamlFlowDocument);

            foreach (var img in doc.Descendants().Where(x => x.Name.LocalName == "Image" && x.Attributes().Any(a => a.Name.LocalName == "Source")))
            {
                var at = img.Attributes().FirstOrDefault(x => x.Name.LocalName == "Source");
                if (at == null)
                    continue;
                var url = at.Value;

                if (url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    var urlKey = at.Value.ToLower();
                    DownloadItem file = null;

                    if (!cache.TryGetValue(urlKey, out file))
                    {
                        file = new DownloadItem
                        {
                            FilePath = TempFolder + "\\" + cache.Count + ".dat",
                            Url = url
                        };
                        cache[urlKey] = file;
                    }
                    file.Nodes.Add(img);
                }
            }

            Parallel.ForEach(cache.Values, DownloadFile);

            foreach (var item in cache.Values)
            {
                foreach (var node in item.Nodes)
                {
                    node.Name = XName.Get("InlineImage", node.Name.NamespaceName);
                    var d = new XCData(Convert.ToBase64String(File.ReadAllBytes(item.FilePath)));
                    node.Add(d);

                    var a = node.Attributes().FirstOrDefault(x => x.Name.LocalName == "Source");
                    a.Remove();
                }

            }

            using (var sw = new StringWriter())
            {
                doc.Save(sw, SaveOptions.OmitDuplicateNamespaces);
                xamlFlowDocument = sw.ToString();
            };
            return xamlFlowDocument;
        }

        #endregion Private Methods
    }
}
