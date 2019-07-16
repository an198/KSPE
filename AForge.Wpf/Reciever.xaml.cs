using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace AForge.Wpf
{
    
    public partial class Reciever : Window
    {
        const string ip = "127.0.0.1";
        const int port = 9897;
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);
        private bool isTrue = true;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RecieveImages();
        }

        private void RecieveImages()
        {
            using(Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                byte[] buffer = new byte[10000];
                socket.Receive(buffer);
                RecieverImageFrame.Source = ByteToImage(buffer);
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