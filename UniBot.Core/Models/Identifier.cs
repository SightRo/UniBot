using System;
using System.Collections.Generic;

namespace UniBot.Core.Models
{
    public class Identifier<TId> : IEquatable<Identifier<TId>>
    {
        public Identifier(TId id, string messenger)
        {
            Id = id;
            Messenger = messenger;
        }

        public TId Id { get; }
        public string Messenger { get; }

        public bool Equals(Identifier<TId>? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            
            return EqualityComparer<TId>.Default.Equals(Id, other.Id) &&
                   string.Equals(Messenger, other.Messenger, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            
            return Equals((Identifier<TId>) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add(Messenger, StringComparer.OrdinalIgnoreCase);
            return hashCode.ToHashCode();
        }
    }
}