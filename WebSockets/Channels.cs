using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    class Channels
    {
        public static readonly String HANDSHAKE = "/meta/handshake";
        public static readonly String CONNECT = "/meta/connect";
        public static readonly String SUBSCRIBE = "/meta/subscribe";
        public static readonly String MARKET = "/market";
        public static readonly String LEVEL1 = "/level1";
        public static readonly String TRADES = "/trades";
        public static readonly String ACTIONS = "/actions";
        public static string ACCOUNT(int accountno)
        {
            return string.Format("/account/{0}/info", accountno);
        }
        public static string ORDERS(int accountno)
        {
            return string.Format("/account/{0}/orders", accountno);
        }
        public static string STATEFUL(int accountno)
        {
            return string.Format("/account/{0}/orders/stateful", accountno);
        }
    }
}
