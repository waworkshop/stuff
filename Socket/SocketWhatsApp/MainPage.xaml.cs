namespace SocketWhatsApp
{
    using System;
    using System.Threading;
    using Windows.Foundation;
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Security.Cryptography;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml; 
    using Windows.UI.Xaml.Controls;
    using System.Runtime.InteropServices.WindowsRuntime;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private StreamSocket socket;
        private StreamSocketControl controller;

        private Timer pingTimer;
        private IBuffer helloBuffer;

        public MainPage()
        {
            this.InitializeComponent();
            helloBuffer = CryptographicBuffer.ConvertStringToBinary("hello", BinaryStringEncoding.Utf8);
        }

        public static async void TimerMethod(object state)
        {
            var THIS = state as MainPage;
            if (THIS.socket != null)
            {
                // Send a ping
                await THIS.socket.OutputStream.WriteAsync(THIS.helloBuffer);
                await THIS.socket.OutputStream.FlushAsync();
                THIS.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,() => 
                {
                    THIS.State.Text = "ping " + DateTime.Now.ToString();
                });
            }
        }

        private async void ConnectDisconnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectDisconnect.IsEnabled = false;
            try
            {
                if (socket != null)
                {
                    pingTimer.Dispose();
                    pingTimer = null;
                    // Close method is available for StreamSocket, but only callable from the WinRT implementation - NOT from C#
                    socket.Dispose();
                    socket = null;
                    State.Text = "Disconnected";
                    ConnectDisconnect.Content = "Connect";
                }
                else
                if (!string.IsNullOrEmpty(ServerURI.Text))
                {
                    socket = new StreamSocket();
                    controller = socket.Control;
                    controller.KeepAlive = true;

                    var netendpoint = new HostName(ServerURI.Text);
                    await socket.ConnectAsync(netendpoint, Port.Text);
                    State.Text = "Connected";
                    ConnectDisconnect.Content = "Disconnect";
                    pingTimer = new Timer(TimerMethod, this, 5000, 5000);


                    IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                        async (workItem) =>
                        {
                            byte[] bob = new byte[200];

                            while (workItem.Status != AsyncStatus.Canceled)
                            {
                                await socket.InputStream.ReadAsync(bob.AsBuffer(), 200, InputStreamOptions.Partial);
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                    () =>
                                    {
                                        State.Text = "Read " + bob[0];
                                    });
                            }
                        });
                }
            }
            catch (Exception exception)
            {
                State.Text = exception.Message;
            }
            finally
            {
                ConnectDisconnect.IsEnabled = true;
            }
        }
    }
}
