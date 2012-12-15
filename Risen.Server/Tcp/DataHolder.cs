﻿using System.Net;

namespace Risen.Server.Tcp
{
    public class DataHolder : IDataHolder
    {
        //Remember, if a socket uses a byte array for its buffer, that byte array is
        //unmanaged in .NET and can cause memory fragmentation. So, first write to the
        //buffer block used by the SAEA object. Then, you can copy that data to another
        //byte array, if you need to keep it or work on it, and want to be able to put
        //the SAEA object back in the pool quickly, or continue with the data
        //transmission quickly.
        //DataHolder has this byte array to which you can copy the data.
        public byte[] DataMessageReceived { get; set; }
        public int ReceivedTransmissionId { get; set; }
        public long SessionId { get; set; }
        public EndPoint RemoteEndpoint { get; set; } //for testing. With a packet analyzer this can help you see specific connections.
    }
}