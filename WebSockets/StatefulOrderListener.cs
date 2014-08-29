using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public class StatefulOrderUpdateType
    {
        public const string Ack = "ack";
        public const string Fill = "fill";
        public const string UROut = "urout";
        public const string Reject = "reject";
    }

    public interface StatefulOrderListener
    {
        void Ack(String json);
        void Reject(String json);
        void Fill(String json);
        void UROut(String json);
    }
}
