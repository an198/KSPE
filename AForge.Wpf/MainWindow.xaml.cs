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
        const string Myip = "172.20.10.4";
        const string Parthnerip = "172.20.10.3";
        const int port = 9897;
        byte[] buffer = new byte[10000];
        IPEndPoint MyendPoint = new IPEndPoint(IPAddress.Parse(Myip), port);
        IPEndPoint ParthnerendPoint = new IPEndPoint(IPAddress.Parse(Parthnerip), port);
        private bool isTrue=true;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            GetVideoDevices();
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCamera();
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //Thread t = new Thread(StartCamera);
            //t.Start();
            Thread tt = new Thread(VideoSender);
            tt.Start();
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            const int port = 9897;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(MyendPoint);
                socket.Listen(10);
                socket.Accept();
                while (isTrue)
                {
                    socket.Receive(buffer);
                    VideoReciever.Source = ByteToImage(buffer);
                }
            }
        }



        private void VideoSender()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(ParthnerendPoint);

                while (this.isTrue)
                {
                        buffer = ConvertBitmapSourceToByteArray(bi);
                        socket.Send(buffer);
                        Thread.Sleep(70);
                }
            }
        }



        //private void video_NewFrame(object sender, Video.NewFrameEventArgs eventArgs)
        //{
        //    try
        //    {

        //        using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
        //        {
        //            bi = bitmap.ToBitmapImage();
        //        }
        //        bi.Freeze(); // avoid cross thread operations and prevents leaks
        //        Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; }));
        //    }
        //    catch (Exception exc)
        //    {
        //        MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        StopCamera();
        //    }
        //}

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
        







        private void video_NewFrame(object sender, Video.NewFrameEventArgs eventArgs)
        {
            try
            {

                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = bitmap.ToBitmapImage();
                    buffer = ConvertBitmapSourceToByteArray(bi);
                }
                //bi.Freeze(); // avoid cross thread operations and prevents leaks
                //Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; VideoSender(); }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
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
    }
}