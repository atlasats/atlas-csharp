using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public class BayeuxExtensionClientId : BayeuxExtension
    {
        private Client client;

        public BayeuxExtensionClientId(Client client)
        {
            this.client = client;
        }

        public bool incoming(BayeuxMessage message)
        {
            return true;
        }

        public bool outgoing(OutMessage message)
        {
            if (!string.IsNullOrEmpty(client.ClientId))
            {
                message.ClientId = client.ClientId;
            }
            return true;
        }
    }
}
