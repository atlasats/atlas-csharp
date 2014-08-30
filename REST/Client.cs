using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.REST
{
    public class Client
    {
        private RestClient client;
        private string token;

        public Client(string url, string token)
        {
            client = new RestClient(url);
            this.token = token;
        }

        private string get(string path, Dictionary<string, object> parameters, bool authenticated=false)
        {
            var request = new RestRequest(path, Method.GET);
            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    request.AddParameter(key, parameters[key]);
                }
            }

            if (authenticated)
            {
                request.AddHeader("Authorization", string.Format("Token token=\"{0}\"", token));
            }

            var response = client.Execute(request);

            return response.Content;
        }

        public string Book(string item, string currency)
        {
            return get("api/v1/market/book", new Dictionary<string, object> { { "item", item }, { "currency", currency } });
        }

        public string RecentTrades(string item, string currency)
        {
            return get("api/v1/market/trades/recent", new Dictionary<string, object> { { "item", item }, { "currency", currency } });
        }

        public string RecentOrders()
        {
            return get("api/v1/orders", null, true);
        }

        public string OrderInfo(string orderid)
        {
            return get(string.Format("api/v1/orders/{0}", orderid), null, true);
        }

        public string AccountInfo()
        {
            return get("api/v1/account", null, true);
        }
    }
}
