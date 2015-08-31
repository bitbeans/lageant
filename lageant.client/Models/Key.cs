using ProtoBuf;

namespace lageant.client.Models
{
    [ProtoContract]
    public class Key
    {
        [ProtoMember(1)]
        public KeyType KeyType { get; set; }

        [ProtoMember(2)]
        public byte[] KeyId { get; set; }

        [ProtoMember(3)]
        public byte[] PublicKey { get; set; }

        [ProtoMember(4)]
        public byte[] PrivateKey { get; set; }

        [ProtoMember(5)]
        public byte[] KeySalt { get; set; }

        [ProtoMember(6)]
        public byte[] KeyNonce { get; set; }
    }
}