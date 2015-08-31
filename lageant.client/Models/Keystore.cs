using System;
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

        /// <summary>
        ///     Try to retrieve a key by it`s key id.
        /// </summary>
        /// <param name="id">A 8 byte key id.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>A key.</returns>
        public Key GetKeyById(byte[] id)
        {
            //validate the length of the key id
            if (id == null || id.Length != 8)
                throw new ArgumentOutOfRangeException("id", (id == null) ? 0 : id.Length,
                    string.Format("id must be {0} bytes in length.", 8));

            return Keys.FirstOrDefault(k => k.KeyId.SequenceEqual(id));
        }

        /// <summary>
        ///     Try to retrieve a key by it`s key id.
        /// </summary>
        /// <param name="id">A 8 byte key id (as hex).</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>A key.</returns>
        public Key GetKeyById(string id)
        {
            return GetKeyById(ConvertToByteArray(id));
        }

        /// <summary>
        ///     Try to retrieve a key by it`s public key.
        /// </summary>
        /// <param name="publicKey">A 32 byte public key.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>A key.</returns>
        public Key GetKeyByPublicKey(byte[] publicKey)
        {
            //validate the length of the public key
            if (publicKey == null || publicKey.Length != 32)
                throw new ArgumentOutOfRangeException("publicKey", (publicKey == null) ? 0 : publicKey.Length,
                    string.Format("publicKey must be {0} bytes in length.", 32));

            return Keys.FirstOrDefault(k => k.PublicKey.SequenceEqual(publicKey));
        }

        /// <summary>
        ///     Try to retrieve a key by it`s public key.
        /// </summary>
        /// <param name="publicKey">A 32 byte public key (as hex).</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>A key.</returns>
        public Key GetKeyByPublicKey(string publicKey)
        {
            return GetKeyByPublicKey(ConvertToByteArray(publicKey));
        }

        /// <summary>
        ///     Try to retrieve a key by it`s private key.
        /// </summary>
        /// <param name="privateKey">A 32 byte private key.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>A key.</returns>
        public Key GetKeyByPrivateKey(byte[] privateKey)
        {
            //validate the length of the private key
            if (privateKey == null || privateKey.Length != 32)
                throw new ArgumentOutOfRangeException("privateKey", (privateKey == null) ? 0 : privateKey.Length,
                    string.Format("privateKey must be {0} bytes in length.", 32));

            return Keys.FirstOrDefault(k => k.PrivateKey.SequenceEqual(privateKey));
        }

        /// <summary>
        ///     Try to retrieve a key by it`s private key.
        /// </summary>
        /// <param name="privateKey">A 32 byte private key (as hex).</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>A key.</returns>
        public Key GetKeyByPrivateKey(string privateKey)
        {
            return GetKeyByPrivateKey(ConvertToByteArray(privateKey));
        }

        /// <summary>
        ///     Convert hex into byte array.
        /// </summary>
        /// <param name="hex">A valid hex string.</param>
        /// <returns>A byte array.</returns>
        private static byte[] ConvertToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x%2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}