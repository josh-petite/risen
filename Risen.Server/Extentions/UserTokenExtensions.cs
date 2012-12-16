using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Extentions
{
    public static class UserTokenExtensions
    {
        public static DataHoldingUserToken AsDataHoldingUserToken(this DataHoldingUserToken userToken)
        {
            return (DataHoldingUserToken) userToken;
        }
    }
}
