using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Client.Tcp.Tokens
{
    public class ConnectOperationUserToken
    {
        private readonly int _tokenId;

        public ConnectOperationUserToken(int id)
        {
            _tokenId = id;
        }

        public OutgoingMessageHolder OutgoingMessageHolder { get; set; }

        public int TokenId { get { return _tokenId; } }
    }
}
