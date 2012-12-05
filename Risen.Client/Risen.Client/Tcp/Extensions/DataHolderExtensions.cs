using Risen.Client.Tcp.Tokens;
using Risen.Shared.Tcp;

namespace Risen.Client.Tcp.Extensions
{
    public static class DataHolderExtensions
    {
        public static ClientDataHolder AsClientDataHolder(this IDataHolder dataHolder)
        {
            return (ClientDataHolder) dataHolder;
        }
    }
}
