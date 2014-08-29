using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public enum BayeuxMessageType
    {
        HANDSHAKE,
        CONNECT,
        ERROR,
        SUBSCRIPTION,
        DATA
    }

    public class BayeuxMessage
    {
        public BayeuxMessageType MessageType { get; protected set; }
        public String Raw { get; protected set; }
        public String Data { get; protected set; }
        public String Channel { get; protected set; }
        public String ClientId { get; internal set; }
        public String Error { get; protected set; }

        public override string ToString()
        {
            return "bayeux:" + MessageType.ToString();
        }

        public BayeuxMessage()
        {
            MessageType = BayeuxMessageType.ERROR;
            Raw = "";
            Data = "";
            Channel = "";
            ClientId = "";
            Error = "";
        }

        public BayeuxMessage(JObject json)
        {
            Raw = json.ToString();
            Channel = (string)json[JSONKeys.CHANNEL];
            if (Channel == Channels.HANDSHAKE)
            {
                MessageType = BayeuxMessageType.HANDSHAKE;
                ClientId = (string)json[JSONKeys.CLIENTID];
            }
            else if (Channel == Channels.CONNECT)
            {
                MessageType = BayeuxMessageType.CONNECT;
            }
            else if (Channel == Channels.SUBSCRIBE)
            {
                MessageType = BayeuxMessageType.SUBSCRIPTION;
                Data = (string)json[JSONKeys.SUBSCRIPTION];
            }
            else
            {
                MessageType = BayeuxMessageType.DATA;
                Data = (string)json[JSONKeys.DATA];
            }
        }
    }
}
