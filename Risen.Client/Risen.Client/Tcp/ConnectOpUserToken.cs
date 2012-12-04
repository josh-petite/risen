using System;

namespace Risen.Client.Tcp
{
    public class ConnectOpUserToken
    {
        internal OutgoingMessageHolder OutgoingMessageHolder;

        private Int32 id; //for testing only


        public ConnectOpUserToken(Int32 identifier)
        {
            id = identifier;
        }

        public Int32 TokenId
        {
            get
            {
                return id;
            }
        }
    }
}