using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Risen.Client
{
    // Implements the connection logic for the socket client.
    internal sealed class SocketClient : IDisposable
    {
        // Constants for socket operations.
        private const Int32 ReceiveOperation = 1, SendOperation = 0;

        // The socket used to send/receive messages.
        private readonly Socket _clientSocket;

        // Flag for connected socket.
        private Boolean _connected;

        // Listener endpoint.
        private readonly IPEndPoint _hostEndPoint;

        // Signals a connection.
        private static readonly AutoResetEvent AutoConnectEvent = new AutoResetEvent(false);

        // Signals the send/receive operation.
        private static readonly AutoResetEvent[] AutoSendReceiveEvents = new[]
                                                                             {
                                                                                 new AutoResetEvent(false),
                                                                                 new AutoResetEvent(false)
                                                                             };

        // Create an uninitialized client instance.
        // To start the send/receive processing call the
        // Connect method followed by SendReceive method.
        internal SocketClient(String hostName, Int32 port)
        {
            // Get host related information.
            IPHostEntry host = Dns.GetHostEntry(hostName);

            // Address of the host.
            IPAddress[] addressList = host.AddressList;

            // Instantiates the endpoint and socket.
            _hostEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);
            _clientSocket = new Socket(_hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        // Connect to the host.
        internal void Connect()
        {
            var connectArgs = new SocketAsyncEventArgs {UserToken = _clientSocket, RemoteEndPoint = _hostEndPoint};
            connectArgs.Completed += OnConnect;
            _clientSocket.ConnectAsync(connectArgs);
            AutoConnectEvent.WaitOne();

            SocketError errorCode = connectArgs.SocketError;

            if (errorCode != SocketError.Success)
                throw new SocketException((Int32) errorCode);
        }

        /// Disconnect from the host.
        internal void Disconnect()
        {
            _clientSocket.Disconnect(false);
        }

        // Calback for connect operation
        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of connection.
            AutoConnectEvent.Set();

            // Set the flag for socket connected.
            _connected = (e.SocketError == SocketError.Success);
        }

        // Calback for receive operation
        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of receive.
            AutoSendReceiveEvents[SendOperation].Set();
        }

        // Calback for send operation
        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of send.
            AutoSendReceiveEvents[ReceiveOperation].Set();

            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    // Prepare receiving.
                    var socket = e.UserToken as Socket;

                    var receiveBuffer = new byte[255];
                    e.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                    e.Completed += OnReceive;
                    if (socket != null) socket.ReceiveAsync(e);
                }
            }
            else
                ProcessError(e);
        }

        // Close socket in case of failure and throws
        // a SockeException according to the SocketError.
        private void ProcessError(SocketAsyncEventArgs e)
        {
            var socket = e.UserToken as Socket;
            
            if (socket != null && socket.Connected)
            {
                // close the socket associated with the client
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // throws if client process has already closed
                }
                finally
                {
                    if (socket.Connected)
                    {
                        socket.Close();
                    }
                }
            }

            // Throw the SocketException
            throw new SocketException((Int32)e.SocketError);
        }

        // Exchange a message with the host.
        internal String SendReceive(String message)
        {
            if (_connected)
            {
                // Create a buffer to send.
                Byte[] sendBuffer = Encoding.ASCII.GetBytes(message);

                // Prepare arguments for send/receive operation.
                var completeArgs = new SocketAsyncEventArgs();
                completeArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                completeArgs.UserToken = _clientSocket;
                completeArgs.RemoteEndPoint = _hostEndPoint;
                completeArgs.Completed += OnSend;

                // Start sending asynchronously.
                _clientSocket.SendAsync(completeArgs);

                // Wait for the send/receive completed.
                AutoResetEvent.WaitAll(AutoSendReceiveEvents);

                // Return data from SocketAsyncEventArgs buffer.
                return Encoding.ASCII.GetString(completeArgs.Buffer, 
                                                completeArgs.Offset, completeArgs.BytesTransferred);
            }
            
            throw new SocketException((Int32)SocketError.NotConnected);
        }

        // Disposes the instance of SocketClient.
        public void Dispose()
        {
            AutoConnectEvent.Close();
            AutoSendReceiveEvents[SendOperation].Close();
            AutoSendReceiveEvents[ReceiveOperation].Close();

            if (_clientSocket.Connected)
            {
                _clientSocket.Close();
            }
        }
    }
}