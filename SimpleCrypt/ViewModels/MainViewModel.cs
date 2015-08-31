using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using lageant.client;
using lageant.client.Models;
using ProtoBuf;
using SimpleCrypt.Utils;
using Sodium;
using StreamCryptor;
using StreamCryptor.Model;

namespace SimpleCrypt.ViewModels
{
    [Export]
    public sealed class MainViewModel : Screen
    {
        private readonly BindableCollection<Key> _keys = new BindableCollection<Key>();
        private readonly ILageantClient _lageantClient;
        private readonly IWindowManager _windowManager;

        private List<Key> _availableKeys;
        private string _mainViewError;
        private Key _selectedKey;
        private bool _tryAutoDecrypt;


        /// <summary>
        ///     MainViewModel construcor on start up.
        /// </summary>
        /// <param name="windowManager">The current window manager.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        [ImportingConstructor]
        public MainViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
            : this(
                windowManager, eventAggregator, new LageantClient())
        {
        }

        /// <summary>
        ///     MainViewModel construcor for XAML.
        /// </summary>
        public MainViewModel()
        {
        }

        /// <summary>
        ///     MainViewModel construcor.
        /// </summary>
        /// <param name="windowManager">The current window manager.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="lageantClient">The lageant client.</param>
        private MainViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            ILageantClient lageantClient)
        {
            _windowManager = windowManager;
            DisplayName = string.Format("SimpleCrypt {0}", VersionUtilities.PublishVersion);
            TryAutoDecrypt = false;
            MainViewError = string.Empty;

            _lageantClient = lageantClient;
            if (!_lageantClient.Connect()) return;
            RefreshKeys();
        }

        public List<Key> AvailableKeys
        {
            get { return _availableKeys; }
            set
            {
                _availableKeys = value;
                NotifyOfPropertyChange(() => AvailableKeys);
            }
        }

        public Key SelectedKey
        {
            get { return _selectedKey; }
            set
            {
                _selectedKey = value;
                NotifyOfPropertyChange(() => SelectedKey);
            }
        }

        public bool TryAutoDecrypt
        {
            get { return _tryAutoDecrypt; }
            set
            {
                _tryAutoDecrypt = value;
                NotifyOfPropertyChange(() => TryAutoDecrypt);
            }
        }

        public string MainViewError
        {
            get { return _mainViewError; }
            set
            {
                _mainViewError = value;
                NotifyOfPropertyChange(() => MainViewError);
            }
        }

        public void RefreshKeys()
        {
            MainViewError = string.Empty;
            AvailableKeys = _lageantClient.Keystore.Keys;
        }


        /// <summary>
        ///     Encrypt files per Drag and Drop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void DropFileForEncryption(object sender, DragEventArgs e)
        {
            MainViewError = string.Empty;
            if (SelectedKey != null)
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        if (!File.Exists(file)) continue;
                        await EncryptFile(SelectedKey.PublicKey, file);
                    }
                }
            }
            else
            {
                MainViewError = "You need to select a key first";
            }
        }

        public async void DropFileForDecryption(object sender, DragEventArgs e)
        {
            MainViewError = string.Empty;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (SelectedKey != null)
            {
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        if (!File.Exists(file)) continue;
                        var decrypt = false;
                        try
                        {
                            //simple check if the file is a StreamCryptor encrypted file
                            var header = GetSenderPublicKeyFromFileHeader(file);
                            if (header != null)
                            {
                                decrypt = true;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            //not a StreamCryptor encrypted file
                            decrypt = false;
                        }
                        catch (BadFileHeaderException)
                        {
                            //not a StreamCryptor encrypted file
                            decrypt = false;
                        }
                        if (!decrypt) continue;
                        var success = await DecryptFile(SelectedKey, file);
                        if (!success)
                        {
                            MainViewError = string.Format("Could not decrypt {0}", file);
                        } 
                    }
                }
            }
            else
            {
                //could be optimized
                if (TryAutoDecrypt)
                {
                    if (AvailableKeys.Any())
                    {
                        if (files.Any())
                        {
                            foreach (var file in files)
                            {
                                if (!File.Exists(file)) continue;
                                var keystore = _lageantClient.Keystore;
                                if (keystore != null)
                                {
                                    foreach (var k in keystore.Keys)
                                    {
                                        var success = await DecryptFile(k, file);
                                        if (success)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        MainViewError = "You need to select a key first (no keys in list)";
                    }
                }
                else
                {
                    MainViewError = "You need to select a key first";
                }
            }
        }

        private static async Task<bool> DecryptFile(Key key, string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    //use GenerateKeyPair to convert the key into a libsodium-net keypair
                    var keypair = PublicKeyBox.GenerateKeyPair(key.PrivateKey);
                    await Cryptor.DecryptFileWithStreamAsync(keypair, file, Path.GetDirectoryName(file), null, true);
                    keypair.Dispose();
                    return true;
                }
            }
            catch (CryptographicException)
            {
                return false;
            }
            return false;
        }

        private static async Task EncryptFile(byte[] recipientPublicKey, string file)
        {
            if (File.Exists(file))
            {
                var ephemeralKeyPair = PublicKeyBox.GenerateKeyPair();
                await
                    Cryptor.EncryptFileWithStreamAsync(ephemeralKeyPair, recipientPublicKey, file, null,
                        Path.GetDirectoryName(file));
                ephemeralKeyPair.Dispose();
            }
        }

        public void PreviewDragOverForEncryption(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.Move;
        }

        public void PreviewDragOverForDecryption(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.Move;
        }

        public void DragEnterEncryption(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        public void DragEnterDecryption(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private static byte[] GetSenderPublicKeyFromFileHeader(string file)
        {
            using (var fileStreamEncrypted = File.OpenRead(file))
            {
                var encryptedFileHeader =
                    Serializer.DeserializeWithLengthPrefix<EncryptedFileHeader>(fileStreamEncrypted,
                        PrefixStyle.Base128, 1);
                if (encryptedFileHeader == null)
                {
                    throw new BadFileHeaderException("Missing file header: maybe not a StreamCryptor encrypted file");
                }
                return encryptedFileHeader.SenderPublicKey;
            }
        }
    }
}