using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public class Order : AtlasAction
    {
        public Order(String clientId, string item, string currency, string side, decimal quantity)
        {
            Set("action", "action:create");
            Set("clid", clientId);
            Set("item", item);
            Set("currency", currency);
            Set("side", side);
            Set("quantity", quantity);
        }

        public static Order CreateLimit(string clientId, string item, string currency, string side, decimal quantity, decimal price)
        {
            var order = new Order(clientId, item, currency, side, quantity);
            order.Set("type", "limit");
            order.Set("price", price);
            return order;
        }

        public static Order CreateMarket(string clientId, string item, string currency, string side, decimal quantity)
        {
            var order = new Order(clientId, item, currency, side, quantity);
            order.Set("type", "market");
            return order;
        }
    }
}
