using System;

namespace jamgev.Core
{
    public readonly struct SubscriptionToken : IEquatable<SubscriptionToken>
    {
        public readonly long Id { get; }

        public bool IsValid
        {
            get => Id != 0;
        }

        public static SubscriptionToken Invalid
        {
            get => default(SubscriptionToken);
        }

        internal SubscriptionToken(long id)
        {
            Id = id;
        }

        public bool Equals(SubscriptionToken other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is SubscriptionToken other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(SubscriptionToken left, SubscriptionToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SubscriptionToken left, SubscriptionToken right)
        {
            return !left.Equals(right);
        }
    }
}