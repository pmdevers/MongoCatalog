using System.Collections.Generic;

using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Catalog.Web.Pdf.PdfForms
{
    public class PdfMergeStreamer
    {
        public void FillPDF(string templatePath, IEnumerable<IPdfMergeData> mergeDataItems, System.IO.MemoryStream outputStream)
        {
            // Agggregate successive pages here:
            var pagesAll = new List<byte[]>();

            // Hold individual pages Here:

            foreach (var mergeItem in mergeDataItems)
            {
                // Read the form template for each item to be output:
                var templateReader = new PdfReader(templatePath);
                using (var tempStream = new System.IO.MemoryStream())
                {
                    var stamper = new PdfStamper(templateReader, tempStream) { FormFlattening = true };
                    var fields = stamper.AcroFields;
                    stamper.Writer.CloseStream = false;

                    // Grab a reference to the Dictionary in the current merge item:
                    var fieldVals = mergeItem.MergeFieldValues;

                    // Walk the Dictionary keys, fnid teh matching AcroField, 
                    // and set the value:
                    foreach (var name in fieldVals.Keys)
                    {
                        fields.SetField(name, fieldVals[name]);
                    }

                    // If we had not set the CloseStream property to false, 
                    // this line would also kill our memory stream:
                    stamper.Close();

                    // Reset the stream position to the beginning before reading:
                    tempStream.Position = 0;

                    // Grab the byte array from the temp stream . . .
                    var pageBytes = tempStream.ToArray();

                    // And add it to our array of all the pages:
                    pagesAll.Add(pageBytes);
                }
            }

            // Create a document container to assemble our pieces in:
            var mainDocument = new Document(PageSize.A4);

            // Copy the contents of our document to our output stream:
            var pdfCopier = new PdfSmartCopy(mainDocument, outputStream) { CloseStream = false };

            // Once again, don't close the stream when we close the document:

            mainDocument.Open();
            foreach (var pageByteArray in pagesAll)
            {
                // Copy each page into the document:
                mainDocument.NewPage();
                pdfCopier.AddPage(pdfCopier.GetImportedPage(new PdfReader(pageByteArray), 1));
            }
            pdfCopier.Close();

            // Set stream position to the beginning before returning:
            outputStream.Position = 0;
        }
    }
}