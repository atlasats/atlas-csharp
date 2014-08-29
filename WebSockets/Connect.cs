using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public class Connect : OutMessage
    {
        public Connect()
        {
            Channel = Channels.CONNECT;
            AddRoot("connectionType", "websocket");
        }
    }
}
