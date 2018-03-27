using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EmojiTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        bool image1 = true;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            string curr = null;
            rb.Document.GetText(Windows.UI.Text.TextGetOptions.None, out curr);
            //rb.Document.SetText(Windows.UI.Text.TextSetOptions.None, curr + "Test");
            //rb.Document.Selection.SetText(Windows.UI.Text.TextSetOptions.None, "Test");

            var charSize = rb.Document.Selection.CharacterFormat.Size;
            var tb = new TextBlock { Text = "Ig", FontSize = charSize };
            tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile storelogo = await InstallationFolder.GetFileAsync(@"Assets\1F993_64x64.png");
            using (IRandomAccessStream imagestram = await storelogo.OpenReadAsync())
            {
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(imagestram);
                rb.Document.Selection.InsertImage((int) tb.DesiredSize.Height , (int)tb.DesiredSize.Height, 0, Windows.UI.Text.VerticalCharacterAlignment.Baseline, "Image", imagestram);
            }
        }

        //public static void RenderEmoji(ref InlineCollection inlines, string s, WaRichText.Formats format, RenderArgs args)
        //{
        //    var emojiChar = Emoji.GetEmojiChar(s);
        //    if (emojiChar == null)
        //    {
        //        RenderPlainText(ref inlines, s, args);
        //    }
        //    else
        //    {
        //        inlines.Add(new InlineUIContainer()
        //        {
        //            Child = CreateEmojiBlock(emojiChar, format, args)
        //        });
        //    }
        //}

        private async void TestDisplay_Click(object sender, RoutedEventArgs e)
        {
            var charSize = rb.Document.Selection.CharacterFormat.Size;
            var tb = new TextBlock { Text = "Ig", FontSize = charSize };
            tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var imageSource = (image1) ? "1F993_64x64.png" : "1F937-1F3FC-200D-2640-FE0F_64x64.png";
            image1 = !image1;
            StorageFile storelogo = await InstallationFolder.GetFileAsync(@"Assets\" + imageSource);

            using (IRandomAccessStream imagestram = await storelogo.OpenReadAsync())
            {
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(imagestram);
                
                emojiImage.Source = image;
                emojiImage.Width = (int)tb.DesiredSize.Height;
                emojiImage.Height = (int)tb.DesiredSize.Height;

            }
        }
    }
}
