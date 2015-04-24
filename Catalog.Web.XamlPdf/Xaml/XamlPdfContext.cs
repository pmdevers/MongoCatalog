using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps.Packaging;

using Catalog.Web.XamlPdf.Objects;

namespace Catalog.Web.XamlPdf.Xaml
{
    public class XamlPdfContext
    {
        public PDFDocument PDFDocument { get; set; }
        public FlowDocument FlowDocument { get; set; }
        public XpsDocument XpsDocument { get; set; }
        public FixedDocument FixedDocument { get; set; }
        public FixedPage FixedPage { get; set; }
        public PDFPage PDFPage { get; set; }
        public Package Package { get; set; }

        public byte[] GetResource(Uri partUri)
        {
            var p = Package.GetPart(partUri);
            var ms = new MemoryStream();
            using (var s = p.GetStream())
            {
                s.CopyTo(ms);
            }
            return ms.ToArray();
        }

        public FontFamily GetFont(Uri partUri)
        {
            var data = GetResource(partUri);

            XpsFont xf = null;

            foreach (var item in XpsDocument.FixedDocumentSequenceReader.FixedDocuments)
            {
                foreach (var fp in item.FixedPages)
                {
                    foreach (var f in fp.Fonts)
                    {
                        if (f.Uri == partUri)
                            xf = f;
                    }
                }
            }



            var reader = new MemoryStream(data);
            var writer = new MemoryStream();

            if (xf.IsObfuscated)
            {
                ObfuscationSwitcher(partUri.ToString(), reader, writer);
            }
            else
            {
                reader.CopyTo(writer);
            }

            var tmp = Path.GetTempFileName();

            File.WriteAllBytes(tmp, writer.ToArray());

            return new FontFamily("file:///" + tmp);
        }

        public static void ObfuscationSwitcher(string uri, MemoryStream font, Stream streamOut)
        {
            if (font == null || streamOut == null)
                throw new ArgumentNullException();
            
            const int count1 = 4096;
            const int length1 = 16;
            const int count2 = 32;

            var originalString = uri;
            var startIndex = originalString.LastIndexOf('/') + 1;
            var length2 = originalString.LastIndexOf('.') - startIndex;
            var str = new Guid(originalString.Substring(startIndex, length2)).ToString("N");
            var numArray = new byte[length1];

            for (var index = 0; index < numArray.Length; ++index)
            {
                numArray[index] = Convert.ToByte(str.Substring(index * 2, 2), 16);
            }

            var buffer1 = new byte[count2];

            font.Read(buffer1, 0, count2);

            for (var index1 = 0; index1 < count2; ++index1)
            {
                var index2 = numArray.Length - index1 % numArray.Length - 1;
                buffer1[index1] ^= numArray[index2];
            }

            streamOut.Write(buffer1, 0, count2);
            var buffer2 = new byte[count1];
            int count3;

            while ((count3 = font.Read(buffer2, 0, count1)) > 0)
            {
                streamOut.Write(buffer2, 0, count3);
            }

            streamOut.Position = 0L;
        }

        private Visual lastVisual = null;

        internal Point TransformPoint(Visual visual, Point p)
        {
            lastVisual = visual;
            p = Transform(visual, p);
            p.Y = PDFPage.MediaBox.Height - p.Y;
            return p;
        }

        private Point Transform(DependencyObject visual, Point p)
        {
            if (visual == FixedPage)
                return p;

            var r = p;

            var e = visual as UIElement;
            if (e != null)
            {
                var t = e.RenderTransform;
                var m = t.Value;

                if (m != null && !m.IsIdentity)
                {
                    r = t.Transform(p);
                }
            }
            return Transform((Visual)VisualTreeHelper.GetParent(visual), r);
        }
       
        internal Point TransformPoint(Point p)
        {
            return TransformPoint(lastVisual, p);
        }

        readonly Dictionary<GlyphTypeface, string> fonts = new Dictionary<GlyphTypeface, string>();

        internal string GetFont(PDFResources resources, System.Windows.Media.GlyphTypeface typeFace)
        {
            var familyName = typeFace.FamilyNames.Values.FirstOrDefault();

            string key = null;
            if (!fonts.TryGetValue(typeFace, out key))
            {
                key = "R" + resources.Id + "F" + fonts.Count;
                PDFFont pf = PDFDocument.CreateObject<PDFFont>();
                pf.BaseFont = familyName;
                pf.SubType = "Type1";
                pf.Encoding = "MacRomanEncoding";
                resources.Font[key] = pf;

                var pd = PDFDocument.CreateObject<PDFFontDescriptor>();

                pf.FontDescriptor = pd;

                pd.FontName = familyName;
                pd.FontFamily = familyName;
                pd.FontWeight = typeFace.Weight.ToOpenTypeWeight();


                pd.XHeight = typeFace.XHeight;
                pd.CapHeight = typeFace.CapsHeight;
                pd.StemV = typeFace.StrikethroughThickness;

                pd.Flags = PDFFontFlags.None;

                if (typeFace.Weight == FontWeights.Bold)
                {
                    pd.Flags |= PDFFontFlags.ForceBold;
                }

                if (typeFace.Symbol)
                {
                    pd.Flags |= PDFFontFlags.Symbolic;
                }
                else
                {
                    pd.Flags |= PDFFontFlags.Nonsymbolic;
                }
                pd.Ascent = typeFace.AdvanceHeights.Select(x => x.Value).Max() - typeFace.Baseline;
                pd.Descent = -(typeFace.DistancesFromHorizontalBaselineToBlackBoxBottom.Select(x => x.Value).Max());

                pd.FontBBox = new PDFRect
                {
                    Width = (int)typeFace.AdvanceWidths.Select(x => x.Value).Sum(),
                    Height = (int)typeFace.AdvanceHeights.Select(x => x.Value).Sum()
                };

                fonts[typeFace] = key;
            }
            return key;
        }
    }
}
