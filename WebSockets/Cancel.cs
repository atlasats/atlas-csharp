using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public class Cancel : AtlasAction
    {
        private Cancel(string orderId)
        {
            Set("action", "order:cancel");
            Set("oid", orderId);
        }

        public static Cancel Create(string orderId)
        {
            var cancel = new Cancel(orderId);
            return cancel;
        }
    }
}
