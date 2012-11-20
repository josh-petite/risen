using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Risen.Logic.Tcp
{
    public class SocketServer
    {
        private Socket _socket;
        
        public void Execute()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(0, 1234));
            _socket.Listen(0);
            Socket accepted = null;

            try
            {
                new Thread(() =>
                    {
                        accepted = _socket.Accept();
                        _socket.Close();

                        while (true)
                        {
                            try
                            {
                                var buffer = new byte[255];
                                var rec = accepted.Receive(buffer, 0, buffer.Length, 0);

                                if (rec <= 0)
                                    throw new SocketException();

                                Array.Resize(ref buffer, rec);

                                Console.WriteLine(Encoding.Default.GetString(buffer));
                            }
                            catch
                            {
                                Console.WriteLine("Disconnection");
                            }
                        }
                    }).Start();
                    
                //_buffer = Encoding.Default.GetBytes("Hello Client!!");
                //accepted.Send(_buffer, 0, _buffer.Length, 0);
                //_buffer = new byte[256];
                //int bytesRead = accepted.Receive(_buffer, 0, _buffer.Length, 0);
                //Array.Resize(ref _buffer, bytesRead);
            }
            finally
            {
                if (_socket.Connected)
                    _socket.Close();

                if (accepted != null) 
                    accepted.Close();
            }
        }
    }
}
