using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public interface MarketDataListener
    {
        void Book(String json);
        void Level1(String json);
        void Quote(String json);
        void Trade(String json);
    }
}
