using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using lageant.core.Models;
using ProtoBuf;

namespace lageant.core
{
    public class LageantCore : ILageantCore, IDisposable
    {
        private const string LageantLock = "LAGEANT_LOCK";

        private readonly string _smKeyStoreName;
        private readonly int _smKeyStoreSize;
        private bool _isLocked;
        private Mutex _keystoreLock;
        private MemoryMappedFile _memoryMappedFile;
        private MemoryMappedViewAccessor _reader;
        private MemoryMappedViewStream _writer;

        /// <summary>
        ///     Initialize a new Keystore in memory.
        /// </summary>
        /// <param name="keyStoreName">The name for the MemoryMappedFile.</param>
        /// <param name="keyStoreSize">The size of the MemoryMappedFile.</param>
        public LageantCore(string keyStoreName, int keyStoreSize)
        {
            _smKeyStoreName = keyStoreName;
            _smKeyStoreSize = keyStoreSize;
        }

        public LageantCore()
        {
            _smKeyStoreName = "lageant";
            _smKeyStoreSize = 1024*1024;
            OpenClient();
        }

        public void AddKey(Key key)
        {
            if (key == null) return;
            var currentKeyStore = Keystore;
            if (currentKeyStore.Keys == null)
            {
                currentKeyStore.Keys = new List<Key>();
            }
            currentKeyStore.Keys.Add(key);
            Keystore = currentKeyStore;
        }

        public void RemoveKey(Key key)
        {
            var currentKeyStore = Keystore;
            if (currentKeyStore.Keys != null)
            {
                for (var i = 0; i < currentKeyStore.Keys.Count; i++)
                {
                    if (currentKeyStore.Keys[i].PublicKey.SequenceEqual(key.PublicKey))
                    {
                        currentKeyStore.Keys.RemoveAt(0);
                    }
                }
                Keystore = currentKeyStore;
            }
        }

        /// <summary>
        ///     Read and write from/to the Keystore.
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
            set
            {
                if (value == null) return;
                _keystoreLock.WaitOne();
                _writer.Position = 0;
                Serializer.SerializeWithLengthPrefix(_writer, value, PrefixStyle.Base128, 0);
                _writer.Flush();
                _keystoreLock.ReleaseMutex();
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _memoryMappedFile?.Dispose();
            _keystoreLock?.Close();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool CreateServer()
        {
            try
            {
                _memoryMappedFile = MemoryMappedFile.CreateOrOpen(_smKeyStoreName, _smKeyStoreSize);
                _reader = _memoryMappedFile.CreateViewAccessor(0, _smKeyStoreSize, MemoryMappedFileAccess.ReadWrite);
                _writer = _memoryMappedFile.CreateViewStream(0, _smKeyStoreSize, MemoryMappedFileAccess.ReadWrite);
                _keystoreLock = new Mutex(true, LageantLock, out _isLocked);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool OpenClient()
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

        ~LageantCore()
        {
            Dispose();
        }
    }
}