using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Catalog.Web.XamlPdf.Objects;
using System.Windows.Shapes;

namespace Catalog.Web.XamlPdf.Xaml
{
    public class XamlPdfWriter
    {
        #region Public Properties

        public FixedPage FixedPage { get; set; }

        public PDFPage Page { get; private set; }

        public PDFDocument PDFDocument { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Write(string flowDocument, System.IO.Stream writer)
        {
            Exception error = null;

            var t = new Thread(x =>
            {
                try
                {
                    using (PDFDocument = new PDFDocument())
                    {
                        using (fdp = new FlowDocumentPackage(flowDocument))
                        {
                            var fd = fdp.Document;

                            foreach (var page in fd.Pages)
                            {
                                page.UpdateLayout();
                                var fp = page.GetPageRoot(true);
                                fp.UpdateLayout();
                                CreatePage(fp);
                            }

                            PDFDocument.Write(writer);

                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            if (error != null)
                throw new InvalidOperationException("Exection Error", error);
        }

        #endregion Public Methods

        #region Internal Methods

        internal string GetFont(PDFResources resources, GlyphTypeface typeFace)
        {
            var familyName = typeFace.FamilyNames.Values.FirstOrDefault();
            var faceName = familyName;
            var fn = typeFace.FaceNames.Where(x => x.Key.LCID == System.Globalization.CultureInfo.CurrentCulture.LCID).Select(x => x.Value).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(fn))
            {
                if (fn != "Regular")
                    faceName += "," + fn;
            }

            string key;

            if (!fonts.TryGetValue(faceName, out key))
            {
                key = "R" + resources.Id + "F" + fonts.Count;
                var pf = PDFDocument.CreateObject<PDFFont>();
                pf.BaseFont = faceName;
                pf.SubType = "Type1";
                pf.Encoding = "WinAnsiEncoding";
                resources.Font[key] = pf;
                fonts[faceName] = key;
            }
            return key;
        }

        internal Point TransformPoint(Visual visual, Point p)
        {
            p = Transform(visual, p);
            p.Y = Page.MediaBox.Height - p.Y;
            return p;
        }

        internal Point TransformPoint(Point p)
        {
            return TransformPoint(lastVisual, p);
        }

        #endregion Internal Methods

        #region Private Fields

        private FlowDocumentPackage fdp;
        private Dictionary<string, string> fonts = new Dictionary<string, string>();
        private Visual lastVisual = null;

        #endregion Private Fields

        #region Private Methods

        private void CreatePage(FixedPage fp)
        {
            FixedPage = fp;
            var page = PDFDocument.Catalog.Pages.Create<PDFPage>();
            page.MediaBox.Width = (int)fp.Width;
            page.MediaBox.Height = (int)fp.Height;
            CreateVisual(page, fp);
        }
        private void CreateVisual(PDFPage page, Visual fp)
        {
            Page = page;
            WriteVisual(fp);

            var count = VisualTreeHelper.GetChildrenCount(fp);

            for (var i = 0; i < count; i++)
            {
                var v = VisualTreeHelper.GetChild(fp, i) as Visual;
                if (v == null)
                {
                    continue;
                }

                CreateVisual(page, v);
            }

            FinishVisual(fp);
        }
        private string Encode(string p)
        {
            p = p.Replace("\\", "\\\\");
            p = p.Replace("(", "\\(");
            p = p.Replace(")", "\\)");
            p = p.Replace("\n", "\\n");
            p = p.Replace("\r", "\\r");
            p = p.Replace("\t", "\\t");
            p = p.Replace("\b", "\\b");
            p = p.Replace("\f", "\\f");
            return p;
        }

        private void FinishVisual(Visual fp)
        {
            var c = fp as Canvas;
            if (c != null)
            {
                if (c.Clip != null)
                {
                    Page.ContentStream.WriteLine("Q");
                }
            }
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
        private void Write(Glyphs visual)
        {
            var gr = visual.ToGlyphRun();

            Point p = TransformPoint(visual, new Point(visual.OriginX, visual.OriginY));

            Page.Resources.HasText = true;
            Page.ContentStream.WriteLine("BT");
            Page.ContentStream.WriteLine("/{0} {1} Tf", GetFont(Page.Resources, gr.GlyphTypeface), gr.FontRenderingEmSize);
            Page.ContentStream.WriteLine("{0} {1} Td", p.X, p.Y);

            if (visual.Fill != null)
            {
                WriteFill(visual.Fill);
            }

            Page.ContentStream.WriteLine("({0}) Tj", Encode(visual.UnicodeString));
            Page.ContentStream.WriteLine("ET");
        }

        private void Write(Path visual)
        {

            var pg = PathGeometry.CreateFromGeometry(visual.Data);
            var url = FixedPage.GetNavigateUri(visual);

            if (url != null)
            {

                var pa = Page.Annotations;
                var bounds = pg.Bounds;
                var topLeft = TransformPoint(visual, bounds.TopLeft);
                var bottomRight = TransformPoint(visual, bounds.BottomRight);

                pa.AddLink(url, new PDFRect { Left = (int)topLeft.X, Top = (int)topLeft.Y, Width = (int)bottomRight.X, Height = (int)bottomRight.Y });
                return;
            }


            if (visual.Fill is ImageBrush)
            {
                WriteImage(visual, visual.Fill as ImageBrush);
                return;
            }

            if (visual.Stroke != null)
            {
                WriteStroke(visual.Stroke);
            }

            if (visual.Fill != null)
            {
                WriteFill(visual.Fill);
            }


            foreach (var item in pg.Figures)
            {
                var start = TransformPoint(visual, item.StartPoint);

                Page.ContentStream.WriteLine("{0} {1} m", start.X, start.Y);

                foreach (var s in item.Segments)
                {
                    WriteSegment(s);
                }

                if (item.IsClosed)
                {
                    Page.ContentStream.WriteLine("{0} {1} l", start.X, start.Y);
                }

                var op = "S";
                if (visual.Stroke != null && visual.Fill != null)
                {
                    op = "B";
                }
                else
                {
                    if (visual.Stroke != null)
                        op = "S";
                    if (visual.Fill != null)
                        op = "f";
                }

                Page.ContentStream.WriteLine(op);
            }
        }

        private void Write(PolyLineSegment visual)
        {
            foreach (var item in visual.Points)
            {
                var p = TransformPoint(item);
                Page.ContentStream.WriteLine("{0} {1} l", p.X, p.Y);
            }
        }

        private void Write(LineSegment visual)
        {
            var p = TransformPoint(visual.Point);
            Page.ContentStream.WriteLine("{0} {1} l", p.X, p.Y);
        }

        private void WriteFill(Brush brush)
        {
            SolidColorBrush sb = brush as SolidColorBrush;
            if (sb != null)
            {
                WriteFillBrush(sb);
                return;
            }

            ImageBrush ib = brush as ImageBrush;
            if (ib != null)
            {
                WriteFillBrush(ib);
                return;
            }

            throw new NotImplementedException(brush.GetType() + " not supported.");
        }

        private void WriteFillBrush(ImageBrush ib)
        {
            Trace.WriteLine("Image Brush..");
        }

        private void WriteFillBrush(SolidColorBrush value)
        {
            Page.ContentStream.WriteLine("{0} {1} {2} rg", ((double)value.Color.R / 255.0), ((double)value.Color.G / 255.0), ((double)value.Color.B / 255.0));
        }

        private void WriteImage(Path visual, ImageBrush brush)
        {
            var frame = brush.ImageSource as BitmapFrame;
            var img = PDFDocument.CreateObject<PDFImage>();

            var key = "R" + Page.Resources.Id + "I" + img.Id;

            Page.Resources.XObject[key] = img;

            img.ColorSpace = "DeviceRGB";
            img.BitsPerComponent = 8;

            var enc = new JpegBitmapEncoder();


            /*TransformedBitmap tb = new TransformedBitmap(frame, new ScaleTransform { 
                ScaleX = brush.Viewport.Width / frame.Width ,
                ScaleY = brush.Viewport.Height / frame.Height
            });*/

            var tb = frame;

            img.Width = tb.PixelWidth;
            img.Height = tb.PixelHeight;

            enc.Frames.Add(BitmapFrame.Create(tb));
            enc.QualityLevel = 100;
            enc.Save(img.Stream);

            Page.ContentStream.WriteLine("q");

            var pg = PathGeometry.CreateFromGeometry(visual.Data);
            var p = pg.Figures.First().StartPoint;
            p = TransformPoint(visual, p);

            Page.ContentStream.WriteLine("{0} 0 0 {1} {2} {3} cm", brush.Viewport.Width, brush.Viewport.Height, p.X, p.Y - brush.Viewport.Height);
            Page.ContentStream.WriteLine("/" + key + " Do");
            Page.ContentStream.WriteLine("Q");
        }

        private void WriteSegment(PathSegment s)
        {
            LineSegment ls = s as LineSegment;
            if (ls != null)
            {
                Write(ls);
                return;
            }

            PolyLineSegment ps = s as PolyLineSegment;
            if (ps != null)
            {
                Write(ps);
                return;
            }

            throw new NotImplementedException(s.GetType() + " not supported.");
        }

        private void WriteStroke(Brush brush)
        {
            SolidColorBrush sb = brush as SolidColorBrush;
            if (sb != null)
            {
                WriteStrokeBrush(sb);
                return;
            }
            throw new NotImplementedException(brush.GetType() + " not supported.");
        }

        private void WriteStrokeBrush(SolidColorBrush value)
        {
            Page.ContentStream.WriteLine("{0} {1} {2} RG", ((double)value.Color.R / 255.0), ((double)value.Color.G / 255.0), ((double)value.Color.B / 255.0));
        }

        private void WriteVisual(Visual fp)
        {
            lastVisual = fp;

            var f = fp as FixedPage;
            if (f != null)
                return;

            var canvas = fp as Canvas;
            if (canvas != null)
            {
                // clip...
                var g = canvas.Clip;
                if (g != null)
                {

                    Page.ContentStream.WriteLine("q");
                    var pg = PathGeometry.CreateFromGeometry(g);
                    foreach (var item in pg.Figures)
                    {
                        Point p = TransformPoint(item.StartPoint);
                        Page.ContentStream.WriteLine("{0} {1} m", p.X, p.Y);
                        foreach (var segment in item.Segments)
                        {
                            WriteSegment(segment);
                        }
                        Page.ContentStream.WriteLine("h");
                        Page.ContentStream.WriteLine("W");
                        Page.ContentStream.WriteLine("n");
                    }
                }
                return;
            }

            var path = fp as Path;
            if (path != null)
            {
                Write(path);
                return;
            }

            var glyphs = fp as Glyphs;
            if (glyphs != null)
            {
                Write(glyphs);
                return;
            }

            throw new NotImplementedException(fp.GetType() + " not supported.");
        }

        #endregion Private Methods
    }
}
