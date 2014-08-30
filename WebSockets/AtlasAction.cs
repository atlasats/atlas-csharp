using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public class AtlasAction : OutMessage
    {
        private Dictionary<string, object> props;
        public AtlasAction()
        {
            Channel = Channels.ACTIONS;
            MessageType = BayeuxMessageType.DATA;
            props = new Dictionary<string, object>();
        }

        public void Set(string name, object value)
        {
            props[name] = value;
        }

        public override string ToJSON()
        {
            Data = JToken.FromObject(props).ToString();
            return base.ToJSON();
        }
    }
}
