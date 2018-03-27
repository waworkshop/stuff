using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WindowsRuntimeComponent1;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WinRTTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private WAByteBuffer bob = new WAByteBuffer();
        private byte[] MyBuffer;

        public MainPage()
        {
            this.InitializeComponent();
            MyBuffer = new byte[120];
            CryptographicBuffer.CopyToByteArray(CryptographicBuffer.ConvertStringToBinary("This Is My String", BinaryStringEncoding.Utf8), out MyBuffer);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // PutWithCopy
            bob.PutWithCopy(CryptographicBuffer.ConvertStringToBinary(this.TextIn.Text, BinaryStringEncoding.Utf8));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Put
            bob.Put(CryptographicBuffer.CreateFromByteArray(MyBuffer));
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            IBuffer returnBuffer;
            // Get
            bob.Get(out returnBuffer);

            string message = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, returnBuffer);

            MessageDialog md = new MessageDialog(message);
            await md.ShowAsync();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (bob.ChangeTheBufferConent())
            {
                // Now get the content
                Button_Click_2(sender, e);
            }
        }
    }
}
