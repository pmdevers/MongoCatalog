using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Catalog.Web.XamlPdf.Xaml
{
    public class InlineDisposable : IDisposable
    {
        private readonly Action d;

        public InlineDisposable(Action disposeAction)
        {
            d = disposeAction;
        }

        public void Dispose()
        {
            d();
        }
    }

    [ContentProperty("Base64Source")]
    public class InlineImage : Image
    {
        /// <summary>
        /// Gets or sets the base64 source.
        /// </summary>
        public string Base64Source
        {
            get { return (string)GetValue(Base64SourceProperty); }
            set { SetValue(Base64SourceProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property to get or set the base64 source
        /// </summary>
        public static readonly DependencyProperty Base64SourceProperty = DependencyProperty.Register("Base64Source", typeof(string), typeof(InlineImage), new FrameworkPropertyMetadata(null, OnBase64SourceChanged));
        
        private static void OnBase64SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var inlineImage = (InlineImage)sender;

            string val = inlineImage.Base64Source;
            if (string.IsNullOrWhiteSpace(val))
                return;

            var stream = new MemoryStream(Convert.FromBase64String(inlineImage.Base64Source));


            var bitmapImage = new BitmapImage();
            try
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            catch
            {
                stream = new MemoryStream(XResources.EmptyImage1);
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            inlineImage.Source = bitmapImage;

        }
    }
}
