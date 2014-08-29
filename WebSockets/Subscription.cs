using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public abstract class Subscription : OutMessage
    {
        public virtual string SubscriptionChannel { get { return ""; } }

        public Subscription()
        {
            Channel = Channels.SUBSCRIBE;
            MessageType = BayeuxMessageType.SUBSCRIPTION;
        }

        public virtual bool IsPublic()
        {
            return true;
        }

        public override string ToJSON()
        {
            AddRoot(JSONKeys.SUBSCRIPTION, this.SubscriptionChannel);
            return base.ToJSON();
        }
    }

    public class Level1Subscription : Subscription
    {
        public override string SubscriptionChannel
        {
            get
            {
                return Channels.LEVEL1;
            }
        }
    }

    public class TradesSubscription : Subscription
    {
        public override string SubscriptionChannel
        {
            get
            {
                return Channels.TRADES;
            }
        }
    }

    public class BookSubscription : Subscription
    {
        public override string SubscriptionChannel
        {
            get
            {
                return Channels.MARKET;
            }
        }
    }

    public abstract class PrivateSubscription : Subscription
    {
        protected int account;

        public PrivateSubscription (int account)
        {
            this.account = account;
        }

        public override bool IsPublic()
        {
            return false;
        }
    }

    public class AccountSubscription : PrivateSubscription
    {
        public AccountSubscription(int account) : base(account)
        {

        }

        public override string SubscriptionChannel
        {
            get
            {
                return Channels.ACCOUNT(account);
            }
        }
    }

    public class OrderSubscription : PrivateSubscription
    {
        public OrderSubscription(int account) : base(account)
        {

        }

        public override string SubscriptionChannel
        {
            get
            {
                return Channels.ORDERS(account);
            }
        }
    }

    public class StatefulOrderSubscription : PrivateSubscription
    {
        public StatefulOrderSubscription(int account) : base(account)
        {

        }

        public override string SubscriptionChannel
        {
            get
            {
                return Channels.STATEFUL(account);
            }
        }
    }
}
