using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public class BayeuxMessageFactory
    {
        public static ICollection<BayeuxMessage> Create(String jsonString)
        {
            ICollection<BayeuxMessage> messages = new LinkedList<BayeuxMessage>();
            if (jsonString != null && jsonString.Length > 0)
            {
                var obj = JToken.Parse(jsonString);
                if (obj.Type == JTokenType.Array)
                {
                    JArray array = (JArray)obj;
                    foreach (var item in array)
                    {
                        var message = parse((JObject)item);
                        messages.Add(message);
                    }
                }
                else
                {
                    var message = parse((JObject)obj);
                    messages.Add(message);
                }
            }
            return messages;
        }

        static BayeuxMessage parse(JObject json)
        {
            return new BayeuxMessage(json);
        }
    }
}
