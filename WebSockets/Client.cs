using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using WebSocketSharp;
using Newtonsoft.Json.Linq;

namespace Atlas.WebSockets
{
    public enum ConnectionState
    {
        CONNECTED,
        CONNECTING,
        DISCONNECTED
    }

    public class Client
    {
        private ILog log;
        private string url;
        private WebSocket wsClient;
        private ConnectionState state;
        private ICollection<AccountListener> accountListeners;
        private ICollection<OrderListener> orderListeners;
        private ICollection<StatefulOrderListener> statefulOrderListeners;
        private ICollection<MarketDataListener> marketListeners;
        private ICollection<BayeuxExtension> extensions;
        private Queue<Subscription> pendingSubscriptions;
        private List<Subscription> subscriptions;

        public String ApiKey { get; private set; }
        public String ApiSecret { get; private set; }
        public int AccountNo { get; private set; }

        public String ClientId { get; private set; }

        private Client(string wsUrl)
        {
            // initialize stuff
            ClientId = "";
            url = wsUrl;
            log = LogManager.GetLogger(typeof(Client));
            wsClient = new WebSocket(url);
            state = ConnectionState.DISCONNECTED;
            extensions = new LinkedList<BayeuxExtension>();
            accountListeners = new LinkedList<AccountListener>();
            orderListeners = new LinkedList<OrderListener>();
            statefulOrderListeners = new LinkedList<StatefulOrderListener>();
            marketListeners = new LinkedList<MarketDataListener>();
            pendingSubscriptions = new Queue<Subscription>();
            subscriptions = new List<Subscription>();

            // define websocket event handlers
            wsClient.OnOpen += onOpen;
            wsClient.OnClose += onClose;
            wsClient.OnMessage += onMessage;
        }

        public void Connect()
        {
            log.Info("connecting to " + url);
            wsClient.Connect();
        }

        public bool Subscribe(MarketDataListener listener, ICollection<Subscription> subscriptions)
        {
            bool success = true;
            foreach (var sub in subscriptions)
            {
                if (!subscribe(sub)) success = false;
            }
            if (success)
            {
                lock(marketListeners)
                {
                    if (!marketListeners.Contains(listener))
                    {
                        marketListeners.Add(listener);
                    }
                }
            }
            return success;
        }

        public bool Subscribe(AccountListener listener)
        {
            if (subscribe(new AccountSubscription(AccountNo)))
            {
                lock (accountListeners)
                {
                    if (!accountListeners.Contains(listener))
                    {
                        accountListeners.Add(listener);
                    }
                }
                return true;
            }
            return false;
        }

        public bool Subscribe(OrderListener listener)
        {
            if (subscribe(new OrderSubscription(AccountNo)))
            {
                lock(orderListeners)
                {
                    if (!orderListeners.Contains(listener))
                    {
                        orderListeners.Add(listener);
                    }
                }
                return true;
            }
            return false;
        }

        public bool Subscribe(StatefulOrderListener listener)
        {
            if (subscribe(new StatefulOrderSubscription(AccountNo)))
            {
                lock(statefulOrderListeners)
                {
                    if (!statefulOrderListeners.Contains(listener))
                    {
                        statefulOrderListeners.Add(listener);
                    }
                }
                return true;
            }
            return false;
        }

        private bool addExtension(BayeuxExtension ext)
        {
            lock(extensions)
            {
                if(!extensions.Contains(ext))
                {
                    extensions.Add(ext);
                    return true;
                }
            }
            return false;
        }

        void onOpen(object sender, EventArgs e)
        {
            log.Info(string.Format("connected ({0})", url));
            handshake();
        }

        void onMessage(object sender, MessageEventArgs e)
        {
            ICollection<BayeuxMessage> messages = BayeuxMessageFactory.Create(e.Data);
            foreach (var message in messages)
            {
                if (!preprocess(message)) continue;
                log.Debug("processing in-message: " + message.Raw);
                switch (message.MessageType)
                {
                    case BayeuxMessageType.HANDSHAKE:
                        ClientId = message.ClientId;
                        state = ConnectionState.CONNECTED;
                        log.Info("handshake completed");
                        connect();
                        break;
                    case BayeuxMessageType.CONNECT:
                        connect();
                        break;
                    case BayeuxMessageType.ERROR:
                        log.Info("error: " + message.Error);
                        break;
                    case BayeuxMessageType.SUBSCRIPTION:
                        log.Info("subscribed: " + message.Data);
                        break;
                    default:
                        process(message);
                        break;
                }
            }
        }

        void onClose(object sender, CloseEventArgs e)
        {
            log.Info("connection closed: " + e.Reason);

            // never by us
            // todo: Reconnect
        }

        bool send(OutMessage message, bool async = true)
        {
            if (wsClient.ReadyState != WebSocketState.Open)
            {
                log.Error(string.Format("connection not open (currently: {0})", wsClient.ReadyState.ToString()));
            }
            else
            {
                lock(extensions)
                {
                    foreach (var ext in extensions)
                    {
                        if (!ext.outgoing(message))
                        {
                            log.Info(ext + " discarded " + message);
                            return false;
                        }
                    }
                }

                var jsonMsg = message.ToJSON();
                log.Debug("sending: " + jsonMsg);

                // send message async
                if (async)
                {
                    wsClient.SendAsync(jsonMsg, (x) => { });
                }
                else
                {
                    wsClient.Send(jsonMsg);
                }
                return true;
            }
            return false;
        }

        bool handshake()
        {
            log.Info("sending handshake");
            if (state == ConnectionState.DISCONNECTED && send(new Handshake()))
            {
                state = ConnectionState.CONNECTING;
                return true;
            }
            log.Warn("invalid state for handshake: " + state);
            return false;
        }

        bool connect()
        {
            log.Info("sending connect");
            if (send(new Connect(), async: false))
            {
                lock(pendingSubscriptions)
                {
                    while(pendingSubscriptions.Count > 0)
                    {
                        var sub = pendingSubscriptions.Dequeue();
                        log.Info("subscripting to pending " + sub);
                        subscribe(sub);
                    }
                }
                return true;
            }
            return false;
        }

        bool subscribe(Subscription subscription)
        {
            if (state == ConnectionState.CONNECTED)
            {
                if (addSubscription(subscription))
                {
                    return send(subscription);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return addPendingSubscription(subscription);
            }
        }

        bool preprocess(BayeuxMessage message)
        {
            lock(extensions)
            {
                foreach (var ext in extensions)
                {
                    if (!ext.incoming(message))
                    {
                        log.Info(ext + " discarding in-message" + message);
                        return false;
                    }
                }
            }
            return true;
        }

        void process(BayeuxMessage message)
        {
            if (message.Channel == Channels.MARKET)
            {
                lock (marketListeners)
                {
                    foreach (var l in marketListeners)
                    {
                        l.Book(message.Data);
                    }
                }
            }
            else if (message.Channel == Channels.LEVEL1)
            {
                lock (marketListeners)
                {
                    foreach (var l in marketListeners)
                    {
                        l.Level1(message.Data);
                    }
                }
            }
            else if (message.Channel == Channels.TRADES)
            {
                lock (marketListeners)
                {
                    foreach (var l in marketListeners)
                    {
                        l.Trade(message.Data);
                    }
                }
            }
            else if (message.Channel == Channels.ACCOUNT(AccountNo))
            {
                lock (accountListeners)
                {
                    foreach (var l in accountListeners)
                    {
                        l.Update(message.Data);
                    }
                }
            }
            else if (message.Channel == Channels.ORDERS(AccountNo))
            {
                lock (orderListeners)
                {
                    foreach (var l in orderListeners)
                    {
                        l.Update(message.Data);
                    }
                }
            }
            else if (message.Channel == Channels.STATEFUL(AccountNo))
            {
                lock (statefulOrderListeners)
                {
                    foreach (var l in statefulOrderListeners)
                    {
                        var msg = (JObject)JToken.Parse(message.Data);
                        var type = (string)msg["type"];

                        switch (type)
                        {
                            case StatefulOrderUpdateType.Ack:
                                l.Ack(message.Data);
                                break;
                            case StatefulOrderUpdateType.Fill:
                                l.Fill(message.Data);
                                break;
                            case StatefulOrderUpdateType.UROut:
                                l.UROut(message.Data);
                                break;
                            case StatefulOrderUpdateType.Reject:
                            default:
                                l.Reject(message.Data);
                                break;
                        }
                    }
                }
            }
        }

        bool addSubscription(Subscription subscription)
        {
            lock(subscriptions)
            {
                if (!subscriptions.Contains(subscription))
                {
                    subscriptions.Add(subscription);
                    return true;
                }
            }
            return false;
        }

        bool addPendingSubscription(Subscription subscription)
        {
            lock(pendingSubscriptions)
            {
                if (!pendingSubscriptions.Contains(subscription))
                {
                    pendingSubscriptions.Enqueue(subscription);
                    return true;
                }
            }
            return false;
        }

        public static Client CreateMarketDataOnly(string url)
        {
            var client = createDefaultClient(url);
            return client;
        }

        public static Client Create(string url, string key, string secret, int account)
        {
            var client = createDefaultClient(url);
            client.ApiKey = key;
            client.ApiSecret = secret;
            client.AccountNo = account;

            client.addExtension(new BayeuxExtensionHMACAuthenticate(client.ApiKey, client.ApiSecret));

            return client;
        }

        private static Client createDefaultClient(string url)
        {
            var client = new Client(url);
            client.addExtension(new BayeuxExtensionClientId(client));

            return client;
        }
    }
}
