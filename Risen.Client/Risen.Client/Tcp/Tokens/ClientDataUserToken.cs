using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Client.Tcp.Tokens
{
    public class ClientDataUserToken : IDataHoldingUserToken
    {
        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        public int TokenId { get; set; }
        public int ReceivedMessageBytesDoneCount { get; private set; }
        public int ReceivedPrefixBytesDoneCount { get; set; }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void CreateNewDataHolder()
        {
            throw new NotImplementedException();
        }
    }
}
