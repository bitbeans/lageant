using Caliburn.Micro;
using ProtoBuf;

namespace lageant.core.Models
{
    [ProtoContract]
    public class Key : PropertyChangedBase
    {
        private byte[] _keyId;
        private byte[] _publicKey;
        private byte[] _privateKey;

        /// <summary>
        ///     Constructor for XAML.
        /// </summary>
        public Key()
        {
        }

        [ProtoMember(1)]
        public KeyType KeyType { get; set; }

        [ProtoMember(2)]
        public byte[] KeyId
        {
            get { return _keyId; }
            set
            {
                if (value.Equals(_keyId)) return;
                _keyId = value;
                NotifyOfPropertyChange(() => KeyId);
            }
        }

        [ProtoMember(3)]
        public byte[] PublicKey
        {
            get { return _publicKey; }
            set
            {
                if (value.Equals(_publicKey)) return;
                _publicKey = value;
                NotifyOfPropertyChange(() => PublicKey);
            }
        }

        [ProtoMember(4)]
        public byte[] PrivateKey
        {
            get { return _privateKey; }
            set
            {
                if (value.Equals(_privateKey)) return;
                _privateKey = value;
                NotifyOfPropertyChange(() => PrivateKey);
            }
        }

        [ProtoMember(5)]
        public byte[] KeySalt { get; set; }
    }
}