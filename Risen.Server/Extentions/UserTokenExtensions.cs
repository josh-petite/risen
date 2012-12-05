using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risen.Server.Tcp.Tokens;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Server.Extentions
{
    public static class UserTokenExtensions
    {
        public static DataHoldingUserToken AsDataHoldingUserToken(this IUserToken userToken)
        {
            return (DataHoldingUserToken) userToken;
        }
    }
}
