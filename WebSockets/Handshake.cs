using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    class Handshake : OutMessage
    {
        public Handshake()
        {
            AddRoot("version", "1.0");
            AddRoot("supportedConnectionTypes", new string[] { "websocket" });
            Channel = Channels.HANDSHAKE;
            MessageType = BayeuxMessageType.HANDSHAKE;
        }

        public override string ToString()
        {
            return "out:handshake";
        }
    }
}
