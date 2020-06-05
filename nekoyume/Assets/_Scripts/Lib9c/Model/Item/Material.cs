using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Bencodex.Types;
using Libplanet;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Nekoyume.Model.Item
{
    [Serializable]
    public class Material : ItemBase
    {
        public HashDigest<SHA256> ItemId { get; }

        public Material(MaterialItemSheet.Row data) : base(data)
        {
            ItemId = data.ItemId;
        }

        public Material(Dictionary serialized) : base(serialized)
        {
            if (serialized.TryGetValue((Text) "item_id", out var itemId))
            {
                ItemId = itemId.ToItemId();
            }
        }

        protected bool Equals(Material other)
        {
            return base.Equals(other) && ItemId.Equals(other.ItemId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Material) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ ItemId.GetHashCode();
            }
        }

        public override IValue Serialize() =>
            new Dictionary(new Dictionary<IKey, IValue>
            {
                [(Text) "item_id"] = ItemId.Serialize()
            }.Union((Dictionary) base.Serialize()));
    }
}
