using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace lageant.client.Models
{
    [ProtoContract(SkipConstructor = true)]
    public class Keystore
    {
        [ProtoMember(1)]
        public List<Key> Keys { get; set; }

        public Key GetKeyById(byte[] id)
        {
            return Keys.FirstOrDefault(k => k.KeyId.SequenceEqual(id));
        }

        public Key GetKeyByPublicKey(byte[] publicKey)
        {
            return Keys.FirstOrDefault(k => k.PublicKey.SequenceEqual(publicKey));
        }
    }
}