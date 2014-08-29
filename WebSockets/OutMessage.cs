using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public class OutMessage : BayeuxMessage
    {
        private JObject json;
        public OutMessage()
        {
            json = new JObject();
            Data = "";
        }

        public void AddRoot(String key, Object obj)
        {
            json[key] = JToken.FromObject(obj);
        }

        public void AddExtension(String key, JObject obj)
        {
            if (json[JSONKeys.EXT] == null)
            {
                json[JSONKeys.EXT] = new JObject();
            }
            JObject ext = (JObject)json[JSONKeys.EXT];
            ext[key] = obj;
        }

        public virtual String ToJSON()
        {
            json[JSONKeys.CHANNEL] = JToken.FromObject(Channel);
            if (!string.IsNullOrEmpty(ClientId))
            {
                json[JSONKeys.CLIENTID] = JToken.FromObject(ClientId);
            }
            json[JSONKeys.DATA] = JToken.FromObject(Data);
            return json.ToString();
        }
    }
}
