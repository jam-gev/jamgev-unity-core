using System;

namespace jamgev.Core
{
    public interface IEventBus
    {
        SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct;

        void Unsubscribe(SubscriptionToken token);

        void Publish<TEvent>(TEvent @event) where TEvent : struct;
    }
}