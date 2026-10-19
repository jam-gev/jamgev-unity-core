using System;
using System.Collections.Generic;

namespace jamgev.Core
{
    public sealed class EventBus : IEventBus
    {
        private sealed class EventBucket
        {
            public readonly List<Subscription> Subscriptions = new List<Subscription>(4);

            public Subscription[] Snapshot = Array.Empty<Subscription>();

            public void RebuildSnapshot()
            {
                int count = Subscriptions.Count;
                Subscription[] snapshot = new Subscription[count];

                for (int i = 0; i < count; i++)
                {
                    snapshot[i] = Subscriptions[i];
                }

                Snapshot = snapshot;
            }
        }

        private sealed class Subscription
        {
            public readonly long Id;
            public readonly Delegate Handler;
            public bool Active = true;

            public Subscription(long id, Delegate handler)
            {
                Id = id;
                Handler = handler;
            }
        }

        private readonly struct SubscriptionLocation
        {
            public readonly Type EventType;

            public SubscriptionLocation(Type eventType)
            {
                EventType = eventType;
            }
        }

        private static long s_nextTokenId;

        public Action<Type, Exception> OnHandlerException;

        private readonly Dictionary<Type, EventBucket> _handlers = new Dictionary<Type, EventBucket>(32);

        private readonly Dictionary<long, SubscriptionLocation> _tokenMap = new Dictionary<long, SubscriptionLocation>(64);

        public int SubscriptionCount
        {
            get => this._tokenMap.Count;
        }

        public SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            if (handler == null)
            {
                return SubscriptionToken.Invalid;
            }

            Type eventType = typeof(TEvent);

            if (!this._handlers.TryGetValue(eventType, out EventBucket bucket))
            {
                bucket = new EventBucket();
                this._handlers.Add(eventType, bucket);
            }

            long id = NextTokenId();

            bucket.Subscriptions.Add(new Subscription(id, handler));
            this._tokenMap.Add(id, new SubscriptionLocation(eventType));

            bucket.RebuildSnapshot();

            return new SubscriptionToken(id);
        }

        public void Unsubscribe(SubscriptionToken token)
        {
            if (!token.IsValid || !this._tokenMap.TryGetValue(token.Id, out SubscriptionLocation location))
            {
                return;
            }

            if (!this._handlers.TryGetValue(location.EventType, out EventBucket bucket))
            {
                this._tokenMap.Remove(token.Id);
                return;
            }

            List<Subscription> subscriptions = bucket.Subscriptions;

            for (int i = 0; i < subscriptions.Count; i++)
            {
                if (subscriptions[i].Id != token.Id)
                {
                    continue;
                }

                subscriptions[i].Active = false;
                subscriptions.RemoveAt(i);
                break;
            }

            this._tokenMap.Remove(token.Id);

            if (subscriptions.Count == 0)
            {
                this._handlers.Remove(location.EventType);
            }
            else
            {
                bucket.RebuildSnapshot();
            }
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : struct
        {
            if (!this._handlers.TryGetValue(typeof(TEvent), out EventBucket bucket))
            {
                return;
            }

            Subscription[] snapshot = bucket.Snapshot;
            int count = snapshot.Length;

            for (int i = 0; i < count; i++)
            {
                Subscription sub = snapshot[i];

                if (!sub.Active)
                {
                    continue;
                }

                try
                {
                    ((Action<TEvent>)sub.Handler).Invoke(@event);
                }
                catch (Exception ex)
                {
                    if (OnHandlerException == null)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                    else
                    {
                        OnHandlerException?.Invoke(typeof(TEvent), ex);
                    }
                }
            }
        }

        private static long NextTokenId()
        {
            return System.Threading.Interlocked.Increment(ref s_nextTokenId);
        }
    }
}