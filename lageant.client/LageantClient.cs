using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using lageant.client.Models;
using ProtoBuf;

namespace lageant.client
{
    public sealed class LageantClient : ILageantClient
    {
        private const string LageantLock = "LAGEANT_LOCK";

        private readonly string _smKeyStoreName;
        private readonly int _smKeyStoreSize;
        private bool _isLocked;
        private Mutex _keystoreLock;
        private MemoryMappedFile _memoryMappedFile;
        private MemoryMappedViewAccessor _reader;

        /// <summary>
        ///     Initialize a new client and connect to the lageant keystore.
        /// </summary>
        public LageantClient()
        {
            _smKeyStoreName = "lageant";
            _smKeyStoreSize = 1024*1024;
            //Connect();
        }

        /// <summary>
        ///     Read the Keystore.
        /// </summary>
        public Keystore Keystore
        {
            get
            {
                var buffer = new byte[_reader.Capacity];
                _reader.ReadArray(0, buffer, 0, buffer.Length);
                using (var memoryStream = new MemoryStream(buffer))
                {
                    try
                    {
                        var keystore = Serializer.DeserializeWithLengthPrefix<Keystore>(memoryStream,
                            PrefixStyle.Base128, 0);
                        return keystore;
                    }
                    catch (Exception)
                    {
                        return new Keystore();
                    }
                }
            }
        }

        /// <summary>
        ///     Dispose the client.
        /// </summary>
        public void Dispose()
        {
            _reader?.Dispose();
            _memoryMappedFile?.Dispose();
            _keystoreLock?.Close();
        }

        /// <summary>
        ///     Connect to the lageant keystore.
        /// </summary>
        /// <returns><c>true</c> on success, otherwise <c>false</c></returns>
        public bool Connect()
        {
            try
            {
                _memoryMappedFile = MemoryMappedFile.OpenExisting(_smKeyStoreName);
                _reader = _memoryMappedFile.CreateViewAccessor(0, _smKeyStoreSize, MemoryMappedFileAccess.Read);
                _keystoreLock = new Mutex(true, LageantLock, out _isLocked);
            }
            catch
            {
                return false;
            }

            return true;
        }

        ~LageantClient()
        {
            Dispose();
        }
    }
}