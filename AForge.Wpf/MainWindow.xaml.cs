using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Video;
using AForge.Video.DirectShow;

namespace AForge.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }
        private FilterInfo _currentDevice;

        
        private IVideoSource _videoSource;

        BitmapImage bi;
        const string ip = "127.0.0.1";
        const int port = 9897;
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);
        private bool isTrue=true;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            GetVideoDevices();
        }


        private async void VideoSender()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp))
            {
                byte[] buffer = new byte[10000];
                socket.Connect(endPoint);

                //while(this.isTrue)
                //{
                    //if(videoPlayer.Source!=null)
                    //{
                    //MemoryStream stream = new MemoryStream(buffer);
                    buffer  = ConvertBitmapSourceToByteArray(bi);
                    socket.Send(buffer);
                    Thread.Sleep(70);
                socket.Disconnect(true);
                    //}
                //}
            }
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCamera();
            videoPlayer.Source = null;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(StartCamera);
            t.Start();
            Thread tt = new Thread(VideoSender);
            tt.Start();
            Reciever reciever = new Reciever();
            reciever.Show();


        }

        private void video_NewFrame(object sender, Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = bitmap.ToBitmapImage();
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
            videoPlayer.Source = null;
        }

        private void GetVideoDevices()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            }
        }

       

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }










































        static void ExecuteClient()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);
            // Creation TCP/IP Socket using  
            // Socket Class Costructor 
            Socket sender = new Socket(ipAddr.AddressFamily,
                       SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(localEndPoint);
            byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");
            int byteSent = sender.Send(messageSent);

            byte[] messageReceived = new byte[1024];
            int byteRecv = sender.Receive(messageReceived);
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }



        public static void ExecuteServer()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);
            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);
            while (true)
            {
                Socket clientSocket = listener.Accept();
                byte[] bytes = new Byte[1024];
                string data = null;

                while (true)
                {
                    int numByte = clientSocket.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, numByte);
                    if (data.IndexOf("<EOF>") > -1) break;
                }

                Console.WriteLine("Text received -> {0} ", data);
                byte[] message = Encoding.ASCII.GetBytes("Test Server");
                clientSocket.Send(message);
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        public static byte[] ConvertBitmapSourceToByteArray(ImageSource imageSource)
        {
            byte[] info;
            var image = imageSource as BitmapImage;
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                info = ms.ToArray();
                return info;
            }
        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            ImageSource imgSrc = biImg as ImageSource;
            return imgSrc;
        }
    }
}