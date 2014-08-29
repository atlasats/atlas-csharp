using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace Atlas.WebSockets
{
    class BayeuxExtensionHMACAuthenticate : BayeuxExtension
    {
        private string key;
        private string secret;

        public BayeuxExtensionHMACAuthenticate(string key, string secret)
        {
            this.key = key;
            this.secret = secret;
        }

        public bool incoming(BayeuxMessage message)
        {
            return true;
        }

        public bool outgoing(OutMessage message)
        {
            if (message.MessageType == BayeuxMessageType.HANDSHAKE || message.MessageType == BayeuxMessageType.CONNECT) return true;

            long nonce = DateTime.Now.Ticks;

            var rawSig = genRawSignature(message, nonce);

            var sig = sign(rawSig);

            var ident = createIdent(nonce, sig);

            message.AddExtension("ident", ident);

            return true;
        }

        private string genRawSignature(OutMessage message, long nonce)
        {
            var raw = new StringBuilder();
            raw.Append(this.key);
            raw.Append(":");
            raw.Append(nonce);
            raw.Append(":");
            raw.Append(message.Channel);
            raw.Append(":");
            raw.Append(message.Data);
            return raw.ToString();
        }

        private string sign(string rawdata)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(this.secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawdata));

            return BitConverter.ToString(hash).Replace("-", "");
        }

        private JObject createIdent(long nonce, string signature)
        {
            var obj = new JObject();
            obj["key"] = JToken.FromObject(this.key);
            obj["nounce"] = JToken.FromObject(nonce);
            obj["signature"] = JToken.FromObject(signature);

            return obj;
        }
    }
}
