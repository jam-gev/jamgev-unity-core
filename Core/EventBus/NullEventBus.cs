using System;

namespace jamgev.Core
{
    public sealed class NullEventBus : IEventBus
    {
        public static readonly NullEventBus Instance = new NullEventBus();

        private NullEventBus()
        {
        }

        public SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct => SubscriptionToken.Invalid;

        public void Unsubscribe(SubscriptionToken token)
        {
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : struct
        {
        }
    }
}