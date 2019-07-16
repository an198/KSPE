using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AForge.Wpf
{
    class SendAndRecieve
    {
        public static void Send(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;  // how many bytes is already sent
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (sent < size);
        }
        //Sending info
        //Socket socket = tcpClient.Client;
        //string str = "Hello world!";
        //MyClass.Send(socket, Encoding.UTF8.GetBytes(str), 0, str.Length, 10000);
        //sending info


        public static void Receive(Socket socket, byte[] buffer)
        {
            while (true)
            {
                socket.Receive(buffer);
                Thread.Sleep(30);
            }
        }
        //Calling recieve method
        //Socket socket = tcpClient.Client;
        //byte[] buffer = new byte[12];
        //MyClass.Receive(socket, buffer, 0, buffer.Length, 10000);
        //string str = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        //Calling recieve method
    }
}
